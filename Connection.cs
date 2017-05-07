#pragma warning disable CS0660, CS0661
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    using waitDirs = Dictionary<Guid, Queue<Directive>>;
    public class Connection {
        string nickname,
               appID = Assembly.GetEntryAssembly().FullName;
        bool needSendJoin = false,
             canProcessMessages = false;
        Client client;
        Server server;
        User _serverUser;
        List<User> users = new List<User>();
        Queue<Directive> toProcess = new Queue<Directive>();    //Lista d'attesa per i messaggi in entrata pre-preparazione
        waitDirs waitList = new waitDirs();                     //Lista d'attesa per i messaggi in uscita non interni pre-identità

        public bool allowAppMismatch { get; private set; }
        public Mode mode { get; private set; }
        public User myself { get; private set; }
        public User serverUser {
            get => isServer ? myself : _serverUser;
            set => _serverUser = value;
        }
        public List<User> Users =>
            serverUser != null ?
                new User[] { serverUser }.Concat(users).ToList() :
                users.ToList();
        public Guid id =>
            mode == Mode.client ? client.id :
            mode == Mode.server ? server.id :
            Guid.Empty;
        public bool isNone => mode == Mode.none;
        public bool isClient => mode == Mode.client;
        public bool isServer => mode == Mode.server;
        public bool isInternalConnected =>
            isClient ? client.Connected :
            isServer ? server.Listening :
            false;

        public event Action<bool> startResult;
        public event Action<stopReason, string> stoppedClient;
        public event Action stoppedServer, updateUserList, updateForm;
        public event Action<User, string[]> incomingMessage;
        public event Action<User> incomingUser;
        public event Action<User, stopReason, string> leavingUser;
        
        public void startClient(string nick, string host, int port, Guid? customID = null) {
            mode = Mode.client;
            nickname = nick;
            if (client == null) {
                client = new Client(host, port, customID);
                client.connected += startConclusion;
                client.disconnected += stopClient;
                client.received += received;
                client.connect();
            }
        }
        public void startServer(string nick, int port, Guid? customID = null, bool allowAppMismatch = false) {
            mode = Mode.server;
            this.allowAppMismatch = allowAppMismatch;
            nickname = nick;
            if (server == null) {
                server = new Server(port, customID);
                server.listening += startConclusion;
                server.stopped += stopServer;
                server.joinClient += serverJoinClient;
                server.leaveClient += serverLeaveClient;
                server.received += received;
                server.start();
            }
        }
        void startConclusion(bool success) {
            if (success) {
                myself = new User(id, nickname);
                if (isClient) {
                    writeHigh(this, "Client connected");
                    needSendJoin = true;
                } else {
                    writeHigh(this, "Server started");
                    startResult(true);
                }
            } else {
                mode = 0;
                client = null;
                server = null;
                startResult(false);
            }
            updateForm();
        }

        public void stop() {
            if (isClient)
                client.disconnect();
            else if (isServer)
                server.stop();
            writeHigh(this, "Stop");
        }
        void stopClient(stopReason reason, string additional) {
            clear();
            stoppedClient(reason, additional);
        }
        void stopServer() {
            clear();
            stoppedServer();
        }
        void clear() {
            mode = 0;
            canProcessMessages = false;
            client = null;
            server = null;
            _serverUser = null;
            users.Clear();
            toProcess.Clear();
            waitList.Clear();
            updateUserList();
            updateForm();
        }

        void serverJoinClient(Guid link) {
            writeHigh(this, $"New client: {link}");
            send(link, new Directive((int)Commands.serverUser, myself.id, link, myself + ""));
            lock (waitList) {
                if (waitList.ContainsKey(link)) {
                    waitList[link].Clear();
                    waitList[link] = new Queue<Directive>();
                } else
                    waitList.Add(link, new Queue<Directive>());
            }
            send(link, new Directive((int)Commands.users, Guid.Empty, (from u in users select u.ToString()).ToArray()));
        }
        void serverLeaveClient(Guid link, stopReason reason, string additional) {
            writeHigh(this, $"End client: {link}, {reason}, {additional}");
            broadcast(new Guid[] { link }, new Directive((int)Commands.leave, link, EnumString(reason), additional));
            var us = getUser(link);
            if (users.Contains(us))
                users.Remove(us);
            if (waitList.ContainsKey(link))
                waitList.Remove(link);
            updateUserList();
            leavingUser(us ?? new User(link, "Connecting user"), reason, additional);
        }

        public void startProcessing() {
            canProcessMessages = true;
            foreach (var msg in toProcess) {
                received(msg);
            }
            toProcess.Clear();
        }
        void received(Directive msg) {
            writeHigh(this, "Received", msg, Users.FindIndex(u => u.id == msg.from) + 1);
            var type = (Commands)msg.type;
            if (type == Commands.msg && !canProcessMessages) {
                toProcess.Enqueue(msg);
                return;
            }
            bool target = msg.to != Guid.Empty,
                 me = msg.to == id;
            if (isServer) {
                if (!me) {
                    if (target)
                        send(msg.to, msg);
                    else
                        broadcast(new Guid[] { msg.from }, msg);
                }
            }

            if (!target || me) {
                switch (type) {
                    case Commands.msg:
                        incomingMessage(getUser(msg.from), msg.values);
                        break;
                  //case Commands.join:
                  //    if (isServer)
                  //        if (!allowAppMismatch && msg.values[0] != appID)
                  //            kick(msg.from, "application mismatch");
                  //        else {
                  //            var ju = addUser(msg.from, msg.values[1]);
                  //            sendWaiting(msg);
                  //            incomingUser(ju);
                  //        }
                  //    else if (isClient)
                  //        incomingUser(addUser(Guid.Parse(msg.values[0]), msg.values[2]));
                  //    break;
                    case Commands.serverUser:
                        serverUser = User.Parse(msg.values[0]);
                        if (needSendJoin && isClient) {
                            needSendJoin = false;
                            send(serverUser.id, new Directive((int)Commands.app, id, serverUser.id, appID));
                        }
                        break;
                    case Commands.app:
                        if (isServer) {
                            if (!allowAppMismatch && msg.values[0] != appID)
                                kick(msg.from, "application mismatch");
                            else {
                                unlockWaiting(msg.from);
                                send(msg.from, new Directive((int)Commands.app, id, msg.from));
                            }
                        } else if (isClient && msg.from == serverUser) {
                            users.Add(this.myself);
                            send(Guid.Empty, new Directive((int)Commands.join, id, nickname));
                            startResult(true);
                            updateUserList();
                        }
                        break;
                    case Commands.users:
                        foreach (var u in msg.values)
                            addUser(User.Parse(u));
                        break;
                    case Commands.join:
                        updateUserList();
                        incomingUser(addUser(msg.from, msg.values[0]));
                        break;
                    case Commands.leave:
                        var us = getUser(msg.from);
                        if (us != null) {
                            users.Remove(us);
                            updateUserList();
                            leavingUser(us, StringEnum<stopReason>(msg.values[0]), msg.values[1]);
                        }
                        break;
                }
            }
        }
        public void sendAll(params string[] txts) {
            if (isClient)
                send(null, txts);
            else if (isServer)
                broadcast(null, txts);
        }
        public void send(User to, params string[] txts) {
            var toID = to?.id ?? Guid.Empty;
            send(toID, new Directive((int)Commands.msg, id, toID, txts));
        }
        public void broadcast(User[] skip, params string[] txts) {
            Guid[] skips = new Guid[0];
            if (skip != null)
                skips = (from s in skip select s.id).ToArray();
            broadcast(skips, new Directive((int)Commands.msg, id, Guid.Empty, txts));
        }
        void send(Guid to, Directive msg) {
            int i = Users.FindIndex(u => u.id == to) + 1;
            if (isClient) {
                writeHigh(this, "Send", msg, i);
                client.write(msg);
            } else if (isServer && to != null) {
                if (to == myself)
                    received(msg);
                else {
                    if (waitList.ContainsKey(to)) {
                        writeHigh(this, "Send (wait)", msg, i);
                        waitList[to].Enqueue(msg);
                    } else {
                        writeHigh(this, "Send", msg, i);
                        server.write(to, msg);
                    }
                }
            }
        }
        void broadcast(Guid[] skip, Directive msg) {
            writeHigh(this, "Broadcast", msg);
            if (isServer) {
                List<Guid> skips = new List<Guid>();
                if (skip != null)
                    skips.AddRange(skip ?? new Guid[0]);
                skips.AddRange(from u in waitList select u.Key);
                foreach (var u in waitList.Keys) {
                    waitList[u].Enqueue(msg);
                }
                server.broadcast(msg, skips.ToArray());
            }
        }
        void unlockWaiting(Guid id) {
            if (waitList.ContainsKey(id)) {
                var jwd = waitList[id];
                waitList.Remove(id);
                foreach (var dir in jwd)
                    send(id, dir);
            }
        }
        public void kick(Guid link, string reason) {
            if (isServer)
                server.kick(link, reason);
        }

        public string getLeaveReason(stopReason reason, string additional, User user = null) {
            string msg = "disconnected",
                   rson = "";
            switch (reason) {
                case stopReason.user:
                    rson = additional;
                    break;
                case stopReason.timeout:
                    rson = "timeout";
                    break;
                case stopReason.dropped:
                    rson = "dropped connection";
                    break;
                case stopReason.serverStop:
                    rson = "server closed";
                    break;
                case stopReason.kicked:
                    msg = "kicked";
                    if (!string.IsNullOrEmpty(additional))
                        rson = additional;
                    break;
            }
            if (!string.IsNullOrEmpty(rson))
                msg += string.Format(" ({0})", rson);
            if (user != null)
                msg = user.nickname + " " + msg;
            else
                msg = char.ToUpper(msg[0]) + msg.Substring(1);
            return msg;
        }
        User addUser(Guid id, string nick) {
            var u = new User(id, nick);
            addUser(u);
            return u;
        }
        void addUser(User u) {
            users.Add(u);
            updateUserList();
        }

        public User getUser(Guid id) {
            if (id == this.id)
                return myself;
            var clone = Users.ToList();
            int i = clone.FindIndex(u => u.id == id);
            return i >= 0 ? clone[i] : null;
        }
        public bool existUser(User u) {
            if (u.id == id)
                return true;
            return Users.Exists(us => us.id == u.id);
        }
        public string getNick(Guid id) {
            var us = getUser(id);
            return us != null ? us.nickname : "<unknown>";
        }
    }

    public enum Mode {
        none,
        client,
        server
    }
    public enum Commands {
        msg,            //Messaggio esterno
        serverUser,     //Utente server
        app,            //Identità utente + assembly applicazione
        users,          //Lista utenti (solo alla connessione)
        join,           //Nuovo utente
        leave           //Fine utente
    }
    public class User {
        public static Regex regx = new Regex(@"^[(\[{]?([-0-9a-f]{36})[)\]}]?,(.+)$");
        public readonly Guid id;
        public readonly string nickname;

        public User(Guid id, string nick) {
            this.id = id;
            nickname = nick;
        }
        public static User Parse(string txt) {
            var grps = regx.Match(txt).Groups;
            var nick = grps[2].Value;
            return new User(Guid.Parse(grps[1].Value), string.IsNullOrWhiteSpace(nick) ? "Anonymous" : nick);
        }
        public override string ToString() {
            return string.Join(",", id, nickname);
        }

        public static bool operator ==(User a, User b) {
            return a?.id == b?.id;
        }
        public static bool operator !=(User a, User b) {
            return a?.id != b?.id;
        }
        public static bool operator ==(Guid a, User b) {
            return a == b?.id;
        }
        public static bool operator !=(Guid a, User b) {
            return a != b?.id;
        }
        public static bool operator ==(User a, Guid b) {
            return a?.id == b;
        }
        public static bool operator !=(User a, Guid b) {
            return a?.id != b;
        }
    }
}
