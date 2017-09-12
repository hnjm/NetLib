using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    using waitDirs = Dictionary<SGuid, Queue<Directive>>;

    public class Connection {
        public event Action<bool> startResult;
        public event Action updateUserList, updateForm, stoppedServer;
        public event Action<StopReason, string> stoppedClient;
        public event Action<User, string[]> incomingMessage;
        public event Action<User> incomingUser;
        public event Action<User, StopReason, string> leavingUser;

        public bool allowAppMismatch { get; private set; }
        public ConnectionModes mode { get; private set; }
        public User myself { get; private set; }
        public User serverUser {
            get => isServer ? myself : _serverUser;
            set => _serverUser = value;
        }
        public List<User> Users =>
            serverUser != null ? new[] {serverUser}.Concat(users).ToList() : users.ToList();
        public Identified basis =>
            mode == ConnectionModes.client ? client :
            mode == ConnectionModes.server ? server :
            (Identified) null;
        public SGuid id => basis?.ID ?? SGuid.Empty;
        public bool isNone => mode == ConnectionModes.none;
        public bool isClient => mode == ConnectionModes.client;
        public bool isServer => mode == ConnectionModes.server;
        public bool isInternalConnected =>
            isClient ? client.Connected : isServer && server.Listening;

        readonly string appID = Assembly.GetEntryAssembly().FullName;
        readonly Queue<Directive> toProcess = new Queue<Directive>(); //Lista d'attesa per i messaggi in entrata pre-preparazione
        readonly waitDirs waitList = new waitDirs();                  //Lista d'attesa per i messaggi in uscita non interni pre-identità
        readonly List<User> users = new List<User>();
        User _serverUser;
        Client client;
        Server server;
        bool needSendJoin, canProcessMessages;
        string nickname;

        public void StartClient(string nick, string host, ushort port, SGuid? customID = null) {
            if (mode != 0)
                Stop();
            if (client == null) {
                mode = ConnectionModes.client;
                nickname = nick;
                client = new Client(host, port, customID);
                client.connected += startConclusion;
                client.disconnected += stopClient;
                client.received += received;
                client.Connect();
            }
        }
        public void StartServer(string nick, ushort port, SGuid? customID = null, bool allowAppMismatch = false) {
            if (mode != 0)
                Stop();
            if (server == null) {
                mode = ConnectionModes.server;
                this.allowAppMismatch = allowAppMismatch;
                nickname = nick;
                server = new Server(port, customID);
                server.listening += startConclusion;
                server.stopped += stopServer;
                server.joinClient += serverJoinClient;
                server.leaveClient += serverLeaveClient;
                server.received += received;
                server.Start();
            }
        }
        public void StartProcessing() {
            canProcessMessages = true;
            foreach (Directive msg in toProcess)
                received(msg);

            toProcess.Clear();
        }
        public void Stop() {
            basis.debugHigh(this, ManagedActions.stop);
            if (isClient)
                client.Disconnect();
            else if (isServer)
                server.Stop();
        }
        public User GetUser(SGuid id) {
            if (id == this.id)
                return myself;

            List<User> clone = Users.ToList();
            int i = clone.FindIndex(u => u.ID == id);
            return i >= 0 ? clone[i] : null;
        }
        public bool ExistUser(User u) {
            if (u.ID == id)
                return true;

            return Users.Exists(us => us.ID == u.ID);
        }
        public string GetNick(SGuid id) {
            User us = GetUser(id);
            return us != null ? us.Nickname : "<unknown>";
        }
        public void SendAll(params string[] txts) {
            if (isClient)
                Send(null, txts);
            else if (isServer)
                Broadcast(null, txts);
        }
        public void Send(User to, params string[] txts) {
            SGuid toID = to?.ID ?? SGuid.Empty;
            send(toID, new Directive((int) ManagedCommands.msg, id, toID, txts));
        }
        public void Broadcast(User[] skip, params string[] txts) {
            var skips = new SGuid[0];
            if (skip != null)
                skips = (from s in skip select s.ID).ToArray();
            broadcast(skips, new Directive((int) ManagedCommands.msg, id, SGuid.Empty, txts));
        }
        public void Kick(SGuid link, string reason) {
            if (isServer)
                server.Kick(link, reason);
        }
        public string GetLeaveReason(StopReason reason, string additional, User user = null) {
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
            msg = user != null ? user.Nickname + " " + msg : char.ToUpper(msg[0]) + msg.Substring(1);
            return msg;
        }
        void checkBasilarEvents() {
            if (startResult == null)
                throw new MissingMethodException(@"There's no method attached to the event ""startResult"".");
            if (updateUserList == null)
                throw new MissingMethodException(@"There's no method attached to the event ""updateUserList"".");
            if (stoppedClient == null)
                throw new MissingMethodException(@"There's no method attached to the event ""stoppedClient"".");
            if (stoppedServer == null)
                throw new MissingMethodException(@"There's no method attached to the event ""stoppedServer"".");
        }
        void startConclusion(bool success) {
            checkBasilarEvents();
            if (success) {
                myself = new User(id, nickname);
                debugNotice(myself);
                if (isClient) {
                    basis.debugHigh(this, ManagedActions.clientConnected);
                    needSendJoin = true;
                }
                else {
                    basis.debugHigh(this, ManagedActions.serverStarted);
                    startResult?.Invoke(true);
                }
            }
            else {
                clear();
                startResult?.Invoke(false);
            }
            updateForm?.Invoke();
        }
        void serverJoinClient(SGuid link) {
            basis.debugHigh(this, ManagedActions.serverJoin, link);
            send(link, new Directive(ManagedCommands.serverUser, myself.ID, link, myself + ""));
            if (waitList.ContainsKey(link)) {
                waitList[link].Clear();
                waitList[link] = new Queue<Directive>();
            } else
                waitList.Add(link, new Queue<Directive>());
            send(link, new Directive(ManagedCommands.users, SGuid.Empty, users.Select(u => u.ToString()).ToArray()));
        }
        void serverLeaveClient(SGuid link, StopReason reason, string additional) {
            broadcast(new[] {link}, new Directive(ManagedCommands.leave, link, EnumString(reason), additional));
            User us = GetUser(link);
            basis.debugHigh(this, ManagedActions.serverLeft, link, GetLeaveReason(reason, additional, us));
            if (users.Contains(us))
                users.Remove(us);
            if (waitList.ContainsKey(link))
                waitList.Remove(link);
            updateUserList?.Invoke();
            leavingUser?.Invoke(us ?? new User(link, "Connecting user"), reason, additional);
        }
        void stopClient(StopReason reason, string additional) {
            checkBasilarEvents();
            clear();
            stoppedClient?.Invoke(reason, additional);
        }
        void stopServer() {
            checkBasilarEvents();
            clear();
            stoppedServer?.Invoke();
        }
        void clear() {
            mode = 0;
            needSendJoin = canProcessMessages = false;
            client = null;
            server = null;
            _serverUser = null;
            users.Clear();
            toProcess.Clear();
            waitList.Clear();
            updateUserList?.Invoke();
            updateForm?.Invoke();
        }
        void received(Directive msg) {
            basis.debugHigh(this, ManagedActions.received, msg);
            if (msg.type == ManagedCommands.msg && !canProcessMessages) {
                toProcess.Enqueue(msg);
                return;
            }

            bool hasTarget = msg.to != SGuid.Empty,
                      toMe = msg.to == id;
            if (isServer && !toMe) {
                if (hasTarget)
                    send(msg.to, msg);
                else
                    broadcast(new[] { msg.from }, msg);
            }

            //1. serverJoinClient()
            //2. (<-) serverUser
            //3. (->) app
            //4. (<-) app
            //5. (->) join
            if (toMe || !hasTarget)
                switch (msg.type) {
                    case ManagedCommands.msg:
                        incomingMessage?.Invoke(GetUser(msg.from), msg.values);
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
                                Kick(msg.from, "application mismatch");
                            else {
                                if (waitList.ContainsKey(msg.from)) {
                                    Queue<Directive> jwd = waitList[msg.from];
                                    waitList.Remove(msg.from);
                                    foreach (Directive dir in jwd)
                                        send(msg.from, dir);
                                }
                                send(msg.from, new Directive(ManagedCommands.app, id, msg.from));
                            }
                        } else if (isClient && msg.from == serverUser) {
                            users.Add(myself);
                            send(SGuid.Empty, new Directive(ManagedCommands.join, id, nickname));
                            startResult?.Invoke(true);
                            updateUserList?.Invoke();
                        }

                        break;
                    case ManagedCommands.users:
                        foreach (string u in msg.values)
                            users.Add(User.Parse(u));

                        updateUserList?.Invoke();
                        break;
                    case ManagedCommands.join:
                        updateUserList?.Invoke();
                        var nu = new User(msg.from, msg.values[0]);
                        users.Add(nu);
                        updateUserList?.Invoke();
                        incomingUser?.Invoke(nu);
                        break;
                    case ManagedCommands.leave:
                        User us = GetUser(msg.from);
                        if (us != null) {
                            users.Remove(us);
                            updateUserList?.Invoke();
                            leavingUser?.Invoke(us, StringEnum<StopReason>(msg.values[0]), msg.values[1]);
                        }
                        break;
                }
        }
        void send(SGuid to, Directive msg) {
            if (isClient) {
                client.Write(msg);
                basis.debugHigh(this, ManagedActions.send, msg);
            }
            else if (isServer) {
                if (to == myself)
                    received(msg);
                else {
                    if (waitList.ContainsKey(to)) {
                        waitList[to].Enqueue(msg);
                        basis.debugHigh(this, ManagedActions.sendEnqueue, msg);
                    }
                    else {
                        server.Write(to, msg);
                        basis.debugHigh(this, ManagedActions.send, msg);
                    }
                }
            }
        }
        void broadcast(SGuid[] skip, Directive msg) {
            basis.debugHigh(this, ManagedActions.broadcast, msg);
            if (isServer) {
                var skips = new List<SGuid>();
                if (skip != null)
                    skips.AddRange(skip);
                skips.AddRange(from u in waitList select u.Key);
                foreach (SGuid u in waitList.Keys)
                    waitList[u].Enqueue(msg);
                server.Broadcast(msg, skips.ToArray());
            }
        }
    }
}