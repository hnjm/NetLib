using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    using waitDirs = Dictionary<Guid, Queue<Directive>>;
    public class Connection {
        public event Action<bool> startResult;
        public event Action<StopReason, string> stoppedClient;
        public event Action stoppedServer, updateUserList, updateForm;
        public event Action<User, string[]> incomingMessage;
        public event Action<User> incomingUser;
        public event Action<User, StopReason, string> leavingUser;

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
        public ConnectionModes mode { get; private set; }
        public User myself { get; private set; }
        public User serverUser {
            get => isServer ? myself : _serverUser;
            set => _serverUser = value;
        }
        public List<User> Users =>
            serverUser != null ?
                new User[] { serverUser }.Concat(users).ToList() :
                users.ToList();
        public Identified basis =>
            mode == ConnectionModes.client ? client :
            mode == ConnectionModes.server ? server :
            (Identified)null;
        public Guid id => basis?.ID ?? Guid.Empty;
        public bool isNone   => mode == ConnectionModes.none;
        public bool isClient => mode == ConnectionModes.client;
        public bool isServer => mode == ConnectionModes.server;
        public bool isInternalConnected =>
            isClient ? client.Connected :
            isServer ? server.Listening :
            false;

        public void startClient(string nick, string host, int port, Guid? customID = null) {
            if (mode != 0)
                stop();
            mode = ConnectionModes.client;
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
            if (mode != 0)
                stop();
            mode = ConnectionModes.server;
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
        public void startProcessing() {
            canProcessMessages = true;
            foreach (var msg in toProcess)
                received(msg);
            toProcess.Clear();
        }
        public void stop() {
            basis.debugHigh(this, ManagedActions.stop);
            if (isClient)
                client.disconnect();
            else if (isServer)
                server.stop();
        }
        public User getUser(Guid id) {
            if (id == this.id)
                return myself;
            var clone = Users.ToList();
            int i = clone.FindIndex(u => u.ID == id);
            return i >= 0 ? clone[i] : null;
        }
        public bool existUser(User u) {
            if (u.ID == id)
                return true;
            return Users.Exists(us => us.ID == u.ID);
        }
        public string getNick(Guid id) {
            var us = getUser(id);
            return us != null ? us.nickname : "<unknown>";
        }
        public void sendAll(params string[] txts) {
            if (isClient)
                send(null, txts);
            else if (isServer)
                broadcast(null, txts);
        }
        public void send(User to, params string[] txts) {
            var toID = to?.ID ?? Guid.Empty;
            send(toID, new Directive((int)ManagedCommands.msg, id, toID, txts));
        }
        public void broadcast(User[] skip, params string[] txts) {
            Guid[] skips = new Guid[0];
            if (skip != null)
                skips = (from s in skip select s.ID).ToArray();
            broadcast(skips, new Directive((int)ManagedCommands.msg, id, Guid.Empty, txts));
        }
        public void kick(Guid link, string reason) {
            if (isServer)
                server.kick(link, reason);
        }
        public string getLeaveReason(StopReason reason, string additional, User user = null) {
            string msg = "disconnected",
                   rson = "";
            switch (reason) {
                case StopReason.user:
                    rson = additional;
                    break;
                case StopReason.timeout:
                    rson = "timeout";
                    break;
                case StopReason.dropped:
                    rson = "dropped connection";
                    break;
                case StopReason.serverStop:
                    rson = "server closed";
                    break;
                case StopReason.kicked:
                    msg = "kicked";
                    if (!string.IsNullOrEmpty(additional))
                        rson = additional;
                    break;
            }
            if (!string.IsNullOrEmpty(rson))
                msg += $" ({rson})";
            msg = user != null ? (user.nickname + " " + msg) : (char.ToUpper(msg[0]) + msg.Substring(1));
            return msg;
        }

        void startConclusion(bool success) {
            if (success) {
                myself = new User(id, nickname);
                debugNotice(myself);
                if (isClient) {
                    basis.debugHigh(this, ManagedActions.clientConnected);
                    needSendJoin = true;
                } else {
                    basis.debugHigh(this, ManagedActions.serverStarted);
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
        void serverJoinClient(Guid link) {
            basis.debugHigh(this, ManagedActions.serverJoin, link);
            send(link, new Directive(ManagedCommands.serverUser, myself.ID, link, myself + ""));
            lock (waitList) {
                if (waitList.ContainsKey(link)) {
                    waitList[link].Clear();
                    waitList[link] = new Queue<Directive>();
                } else
                    waitList.Add(link, new Queue<Directive>());
            }
            send(link, new Directive(ManagedCommands.users, Guid.Empty, (from u in users select u.ToString()).ToArray()));
        }
        void serverLeaveClient(Guid link, StopReason reason, string additional) {
            broadcast(new Guid[] { link }, new Directive(ManagedCommands.leave, link, EnumString(reason), additional));
            var us = getUser(link);
            basis.debugHigh(this, ManagedActions.serverLeft, link, getLeaveReason(reason, additional, us));
            if (users.Contains(us))
                users.Remove(us);
            if (waitList.ContainsKey(link))
                waitList.Remove(link);
            updateUserList();
            leavingUser(us ?? new User(link, "Connecting user"), reason, additional);
        }
        void stopClient(StopReason reason, string additional) {
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
            client?.Dispose();
            server?.Dispose();
            client = null;
            server = null;
            _serverUser = null;
            users.Clear();
            toProcess.Clear();
            waitList.Clear();
            updateUserList();
            updateForm();
        }
        void received(Directive msg) {
            basis.debugHigh(this, ManagedActions.received, msg);
            if (msg.type == ManagedCommands.msg && !canProcessMessages) {
                toProcess.Enqueue(msg);
                return;
            }
            bool target = msg.to != Guid.Empty,
                 isMe = msg.to == id;
            if (isServer) {
                if (!isMe) {
                    if (target)
                        send(msg.to, msg);
                    else
                        broadcast(new Guid[] { msg.from }, msg);
                }
            }

            if (!target || isMe) {
                switch (msg.type) {
                    case ManagedCommands.msg:
                        incomingMessage(getUser(msg.from), msg.values);
                        break;
                    case ManagedCommands.serverUser:
                        serverUser = User.Parse(msg.values[0]);
                        if (needSendJoin && isClient) {
                            needSendJoin = false;
                            send(serverUser.ID, new Directive(ManagedCommands.app, id, serverUser.ID, appID));
                        }
                        break;
                    case ManagedCommands.app:
                        if (isServer) {
                            if (!allowAppMismatch && msg.values[0] != appID)
                                kick(msg.from, "application mismatch");
                            else {
                                if (waitList.ContainsKey(msg.from)) {
                                    var jwd = waitList[msg.from];
                                    waitList.Remove(msg.from);
                                    foreach (var dir in jwd)
                                        send(msg.from, dir);
                                }
                                send(msg.from, new Directive(ManagedCommands.app, id, msg.from));
                            }
                        } else if (isClient && msg.from == serverUser) {
                            users.Add(myself);
                            send(Guid.Empty, new Directive(ManagedCommands.join, id, nickname));
                            startResult(true);
                            updateUserList();
                        }
                        break;
                    case ManagedCommands.users:
                        foreach (var u in msg.values)
                            users.Add(User.Parse(u));
                        updateUserList();
                        break;
                    case ManagedCommands.join:
                        updateUserList();
                        var nu = new User(msg.from, msg.values[0]);
                        users.Add(nu);
                        updateUserList();
                        incomingUser(nu);
                        break;
                    case ManagedCommands.leave:
                        var us = getUser(msg.from);
                        if (us != null) {
                            users.Remove(us);
                            updateUserList();
                            leavingUser(us, StringEnum<StopReason>(msg.values[0]), msg.values[1]);
                        }
                        break;
                }
            }
        }
        void send(Guid to, Directive msg) {
            if (isClient) {
                client.write(msg);
                basis.debugHigh(this, ManagedActions.send, msg);
            } else if (isServer && to != null) {
                if (to == myself)
                    received(msg);
                else {
                    if (waitList.ContainsKey(to)) {
                        waitList[to].Enqueue(msg);
                        basis.debugHigh(this, ManagedActions.sendEnqueue, msg);
                    } else {
                        server.write(to, msg);
                        basis.debugHigh(this, ManagedActions.send, msg);
                    }
                }
            }
        }
        void broadcast(Guid[] skip, Directive msg) {
            basis.debugHigh(this, ManagedActions.broadcast, msg);
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
    }
}
