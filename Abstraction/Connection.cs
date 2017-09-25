using Micro.NetLib.Information;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Micro.NetLib.Core;

namespace Micro.NetLib.Abstraction {
    public class Connection {
        public delegate void IncomingUserHandler(User user);
        public delegate void IncomingMessagesHandler(User from, string[] messages);
        public delegate void LeavingUserHandler(User user, LeaveReason reason, string additional);

        public event Action UpdateUserList, UpdateForm, StoppedServer;
        public event StartHandler Started;
        public event IncomingMessagesHandler IncomingMessages;
        public event IncomingUserHandler IncomingUser;
        public event StopHandler Disconnected;
        public event LeavingUserHandler LeavingUser;

        public bool AllowAppMismatch { get; private set; }
        public ConnectionModes Mode { get; private set; }
        public User Myself { get; private set; }
        public User ServerUser {
            get => IsServer ? Myself : _serverUser;
            set => _serverUser = value;
        }
        public List<User> Users
            => ServerUser != null ? new[] { ServerUser }.Concat(users).ToList() : users.ToList();
        public Identified Basis
            => Mode == ConnectionModes.client ? client :
               Mode == ConnectionModes.server ? server :
               (Identified)null;
        public SGuid ID
            => Basis?.ID ?? SGuid.Empty;
        public bool IsIdle
            => Mode == ConnectionModes.idle;
        public bool IsClient
            => Mode == ConnectionModes.client;
        public bool IsServer
            => Mode == ConnectionModes.server;
        public bool IsInternalConnected
            => IsClient ? client.Connected : IsServer && server.Listening;
        public bool IsStopping
            => stopping;

        readonly string appID = Assembly.GetEntryAssembly().FullName;
        readonly List<User> users = new List<User>();
        readonly Dictionary<string, TiedValue> ties = new Dictionary<string, TiedValue>();
        readonly TiedValue.ChangedValueHandler tieChanged;
        User _serverUser;
        Client client;
        Server server;
        bool needSendJoin, stopping;
        string nickname;

        public Connection() {
            tieChanged = t => tieSync(t, t.Value, null);
        }
        public void StartClient(string nick, string host, ushort port, SGuid? customID = null) {
            if (Mode != 0)
                Stop();
            if (client == null) {
                Mode = ConnectionModes.client;
                nickname = nick;
                client = new Client(host, port, customID);
                client.connected += started;
                client.disconnected += stoppedClient;
                client.received += read;
                client.Connect();
            }
        }
        public void StartServer(string nick, ushort port, SGuid? customID = null, bool allowAppMismatch = false) {
            if (Mode != 0)
                Stop();
            if (server == null) {
                Mode = ConnectionModes.server;
                AllowAppMismatch = allowAppMismatch;
                nickname = nick;
                server = new Server(port, customID);
                server.listening += started;
                server.stopped += stoppedServer;
                server.joinClient += serverJoinClient;
                server.leaveClient += serverLeaveClient;
                server.received += read;
                server.Start();
            }
        }
        public void Stop() {
            if (stopping)
                return;
            stopping = true;
            lock (ties) {
                foreach (var tied in ties.Values.ToList())
                    TieUnregister(tied);
            }
            Basis?.debugHigh(this, ManagedEvents.stop);
            if (IsClient)
                client.Disconnect();
            else if (IsServer)
                server.Stop();
        }
        public User GetUser(SGuid id) {
            if (id == ID)
                return Myself;

            List<User> clone = Users.ToList();
            int i = clone.FindIndex(u => u.ID == id);
            return i >= 0 ? clone[i] : null;
        }
        public bool ExistUser(User u) {
            if (u.ID == ID)
                return true;

            return Users.Exists(us => us.ID == u.ID);
        }
        public string GetNick(SGuid id) {
            User us = GetUser(id);
            return us != null ? us.Nickname : "<unknown>";
        }
        public void Send(params string[] txts)
            => Send(null, txts);
        public void Send(User to, params string[] txts) {
            SGuid toID = to?.ID ?? SGuid.Empty;
            if (IsServer && to == null) {
                Broadcast(txts);
                return;
            }
            send(new Directive((int)ManagedCommands.message, ID, toID, txts), toID);
        }
        public void Broadcast(params string[] txts)
            => Broadcast(null, txts);
        public void Broadcast(User[] skip, params string[] txts) {
            if (IsClient && skip == null) {
                Send(txts);
                return;
            }
            var skips = skip?.Select(u => u.ID)?.ToArray() ?? new SGuid[0];
            broadcast(new Directive((int)ManagedCommands.message, ID, SGuid.Empty, txts), skips);
        }
        public void Kick(SGuid link, string reason) {
            if (IsServer)
                server.Kick(link, reason);
        }
        public string GetLeaveReason(LeaveReason reason, string additional, User user = null) {
            string msg = "disconnected",
                rson = "";
            switch (reason) {
                case LeaveReason.user:
                    rson = additional;
                    break;
                case LeaveReason.timeout:
                    rson = "timeout";
                    break;
                case LeaveReason.dropped:
                    rson = "dropped connection";
                    break;
                case LeaveReason.serverStop:
                    rson = "server closed";
                    break;
                case LeaveReason.kicked:
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
        public void TieRegister(TiedValue t) {
            if (ties.ContainsKey(t.ID))
                throw new InvalidOperationException("There's already a registered TiedValue. It must be unregistered first.");
            ties[t.ID] = t;
            t.ChangedValue += tieChanged;
            var msg = new Directive(ManagedCommands.tieReg, ID, t.ID, t.Value);
            send(msg, SGuid.Empty);
            t.SentChanges();
        }
        public void TieUnregister(TiedValue t) {
            if (!ties.ContainsKey(t.ID))
                return;
            ties.Remove(t.ID);
            t.ChangedValue -= tieChanged;
            t.ClearUsers();
            var msg = new Directive(ManagedCommands.tieUnreg, ID, t.ID);
            send(msg, SGuid.Empty);
        }

        void checkBasilarEvents() {
            if (Started == null)
                throw new MissingMethodException(@"There's no method attached to the event ""startResult"".");
            if (UpdateUserList == null)
                throw new MissingMethodException(@"There's no method attached to the event ""updateUserList"".");
            if (Disconnected == null)
                throw new MissingMethodException(@"There's no method attached to the event ""stoppedClient"".");
            if (StoppedServer == null)
                throw new MissingMethodException(@"There's no method attached to the event ""stoppedServer"".");
        }
        void started(bool success) {
            checkBasilarEvents();
            if (success) {
                Myself = new User(ID, nickname);
                debugNotice(Myself);
                if (IsClient) {
                    Basis?.debugHigh(this, ManagedEvents.clientConnected);
                    needSendJoin = true;
                } else {
                    Basis?.debugHigh(this, ManagedEvents.serverStarted);
                    Started?.Invoke(true);
                }
            } else {
                clear();
                Started?.Invoke(false);
            }
            UpdateForm?.Invoke();
        }
        void serverJoinClient(SGuid link) {
            Basis?.debugHigh(this, ManagedEvents.serverJoin, link);
            send(new Directive(ManagedCommands.serverUser, Myself.ID, link, Myself), link);
            send(new Directive(ManagedCommands.users, SGuid.Empty, users.Select(u => u.Serialize()).ToArray()), link);
        }
        void serverLeaveClient(SGuid link, LeaveReason reason, string additional) {
            broadcast(new Directive(ManagedCommands.leave, link, EnumString(reason), additional), new[] { link });
            User us = GetUser(link);
            Basis?.debugHigh(this, ManagedEvents.serverLeft, link, GetLeaveReason(reason, additional, us));
            lock (users)
                if (users.Contains(us))
                    users.Remove(us);
            UpdateUserList?.Invoke();
            LeavingUser?.Invoke(us ?? new User(link, "Connecting user"), reason, additional);
        }
        void stoppedClient(LeaveReason reason, string additional) {
            Basis?.debugHigh(this, ManagedEvents.stop);
            checkBasilarEvents();
            clear();
            Disconnected?.Invoke(reason, additional);
        }
        void stoppedServer() {
            checkBasilarEvents();
            clear();
            StoppedServer?.Invoke();
        }
        void clear() {
            lock (this) {
                Mode = 0;
                needSendJoin = stopping = false;
                client = null;
                server = null;
                _serverUser = null;
                lock (users)
                    users.Clear();
                lock (ties)
                    ties.Clear();
                UpdateUserList?.Invoke();
                UpdateForm?.Invoke();
            }
        }
        void read(Directive msg) {
            if (IsIdle)
                return;
            Basis?.debugHigh(this, ManagedEvents.read, msg);
            bool forSomeone = msg.to != SGuid.Empty,
                  forMeOnly = msg.to == ID;
            if (!forMeOnly) {
                if (IsServer) {
                    if (forSomeone)
                        send(msg, msg.to);
                    else
                        broadcast(msg, msg.from);
                }
                if (forSomeone)
                    return;
            }
            interpret(msg);
        }
        void interpret(Directive msg) {
            Basis?.debugHigh(this, ManagedEvents.interpret, msg);
            string tieID;
            User u;

            //1. serverJoinClient()
            //2. (<-) serverUser
            //3. (->) app
            //4. (<-) app
            //5. (->) join
            switch (msg.type) {
                case ManagedCommands.message:
                    IncomingMessages?.Invoke(GetUser(msg.from), msg.values);
                    break;

                case ManagedCommands.tieReg:
                    tieID = msg.values[0];
                    if (ties.ContainsKey(tieID)) {
                        var tt = ties[tieID];
                        tt.ApplyUser(msg.from, msg.values[1]);
                        tieSync(tt, tt.FullValue, msg.from);
                    }
                    break;

                case ManagedCommands.tieApply:
                    tieID = msg.values[0];
                    if (ties.ContainsKey(tieID))
                        ties[tieID].ApplyUser(msg.from, msg.values[1]);
                    break;

                case ManagedCommands.tieUnreg:
                    tieID = msg.values[0];
                    if (ties.ContainsKey(tieID))
                        ties[tieID].DeleteUser(msg.from);
                    break;

                case ManagedCommands.serverUser:
                    ServerUser = User.Parse(msg.values[0]);
                    if (needSendJoin && IsClient) {
                        needSendJoin = false;
                        send(new Directive(ManagedCommands.app, ID, ServerUser, appID), ServerUser);
                    }
                    break;

                case ManagedCommands.app:
                    if (IsServer) {
                        if (!AllowAppMismatch && msg.values[0] != appID)
                            Kick(msg.from, "application mismatch");
                        else
                            send(new Directive(ManagedCommands.app, ID, msg.from), msg.from);
                    } else if (IsClient && msg.from == ServerUser) {
                        users.Add(Myself);
                        send(new Directive(ManagedCommands.join, ID, nickname), SGuid.Empty);
                        Started?.Invoke(true);
                        UpdateUserList?.Invoke();
                    }
                    break;

                case ManagedCommands.users:
                    foreach (string s in msg.values)
                        users.Add(User.Parse(s));
                    UpdateUserList?.Invoke();
                    break;

                case ManagedCommands.join:
                    UpdateUserList?.Invoke();
                    u = new User(msg.from, msg.values[0]);
                    users.Add(u);
                    UpdateUserList?.Invoke();
                    IncomingUser?.Invoke(u);
                    break;

                case ManagedCommands.leave:
                    u = GetUser(msg.from);
                    if (u != null) {
                        users.Remove(u);
                        UpdateUserList?.Invoke();
                        LeavingUser?.Invoke(u, StringEnum<LeaveReason>(msg.values[0]), msg.values[1]);
                    }
                    break;
            }
        }
        void send(Directive msg, SGuid fromServerTo) {
            if (IsClient) {
                client.Write(msg);
                Basis?.debugHigh(this, ManagedEvents.send, msg);
            } else if (IsServer) {
                if (fromServerTo == Myself)
                    read(msg);
                else {
                    server.Write(fromServerTo, msg);
                    Basis?.debugHigh(this, ManagedEvents.send, msg);
                }
            }
        }
        void broadcast(Directive msg, params SGuid[] skip) {
            //Se client, il server inoltrerà automaticamente (vedi read), ma non terrà conto di skip
            if (IsServer) {
                Basis?.debugHigh(this, ManagedEvents.broadcast, msg);
                server.Broadcast(msg, skip);
            } else if (IsClient) {
                if (skip.Length == 0) {
                    send(msg, msg.to);
                    return;
                }
                foreach (User u in users) {
                    if (u == Myself)
                        continue;
                    msg.to = u;
                    if (!(skip?.Contains(u.ID) ?? false))
                        send(msg, u.ID);
                }
            }
        }
        void tieSync(TiedValue tie, string newValue, SGuid? specific = null) {
            if (specific != null) {
                send(new Directive(ManagedCommands.tieApply, ID, tie.ID, newValue), specific.Value);
                return;
            }
            foreach (var u in tie.registered)
                send(new Directive(ManagedCommands.tieApply, ID, tie.ID, newValue), u);
            tie.SentChanges();
        }
    }
}