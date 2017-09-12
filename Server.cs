using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Micro.ThreadTimer;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class Server : Identified {
        public event Action<SGuid> joinClient;
        public event Action<SGuid, StopReason, string> leaveClient;
        public event Action<Directive> received;
        public event Action<bool> listening;
        public event Action stopped;
        public bool Listening { get; private set; }
        public readonly ushort port;
        public IReadOnlyList<Link> clients => _clientsCopy.AsReadOnly();
        List<Link> _clientsCopy => _clients.ToList();
        readonly List<Link> _clients;
        Clock clock;
        LinkStates state;
        TcpListener tcp;

        public Server(ushort port, SGuid? customID = null) {
            tcp = new TcpListener(IPAddress.Any, port);
            this.port = port;
            state = LinkStates.ready;
            ID = customID ?? SGuid.NewSGuid();
            _clients = new List<Link>();
            clock = new Clock(serverTick, tick);
        }
        public void Start() {
            if (state != LinkStates.ready)
                return;
            try {
                if (!Listening)
                    tcp.Start();
                state = LinkStates.listening;
                clock.Start();
                Listening = true;
                listening?.Invoke(true);
                debugInstances.Add(this);
            }
            catch (Exception) {
                state = LinkStates.ready;
                clock.Stop();
                Listening = false;
                listening?.Invoke(false);
            }
        }
        public void Stop() {
            state = LinkStates.disconnecting;
            broadcast(true, EnumString(InternalCommands.disconnect), EnumString(StopReason.serverStop), "");
            Listening = false;
            state = LinkStates.ready;
            clock.Stop();
            tcp.Stop();
            stopped?.Invoke();
            foreach (var c in _clients)
                c.Stop();
            _clients.Clear();
            tcp = null;
            clock = null;
            debugInstances.Remove(this);
        }
        public void Write(SGuid id, params Directive[] msgs) {
            Link link = _clients.Find(a => a.ID == id);
            if (link != null)
                write(link, false, msgs.AllStrings());
        }
        public void Broadcast(Directive msg, SGuid[] skip = null) {
            broadcast(false, skip, msg);
        }
        public void Kick(SGuid id, string reason) {
            Link link;
            lock (_clients)
                link = _clients.Find(a => a.ID == id);
            if (link != null) {
                var rsn = StopReason.kicked;
                write(link, true, EnumString(InternalCommands.disconnect), EnumString(rsn), reason);
                disconnect(link, rsn, reason);
            }
        }
        void tick() {
            lock (tcp) {
                if (state == LinkStates.listening && tcp.Pending()) {
                    TcpClient client = tcp.AcceptTcpClient();
                    var link = new Link(client);
                    link.received += a => read(link, a);
                    link.disconnect += a => disconnect(link, a, "");
                    lock (_clients)
                        _clients.Add(link);
                    link.Start();
                    linkTables(link);
                }
            }
        }
        void read(Link link, Data data) {
            if (data.Intern) {
                var cmd = StringEnum<InternalCommands>(data.Cmds[0]);
                link.debugCommand(false, link, cmd);
                if (cmd == InternalCommands.connect) {
                    write(link, true, EnumString(InternalCommands.ok));
                    lock (link) {
                        link.ID = SGuid.Parse(data.Cmds[1]);
                        link.state = LinkStates.ready;
                        joinClient?.Invoke(link.ID);
                    }
                    debugNotice(this);
                }
                if (cmd == InternalCommands.disconnect && link.state == LinkStates.ready)
                    disconnect(link, StringEnum<StopReason>(data.Cmds[1]), data.Cmds[2]);
            }
            else {
                write(link, true, EnumString(InternalCommands.ok));
                foreach (string cmd in data.Cmds)
                    received(Directive.Parse(cmd));
            }
        }
        void write(Link link, bool intern, params string[] cmds) {
            var success = true;
            try {
                link.Write(intern, cmds);
            } catch (Exception) {
                success = false;
                disconnect(link, StopReason.dropped, "");
            }
            if (success && intern)
                link.debugCommand(true, link, cmds);
        }
        void broadcast(bool intern, params string[] cmds)
            => broadcast(intern, null, cmds);
        void broadcast(bool intern, SGuid[] skip = null, params string[] cmds) {
            foreach (Link l in _clientsCopy)
                if (!(skip?.Contains(l.ID) ?? false))
                    write(l, intern, cmds);
        }
        void disconnect(Link link, StopReason reason, string additional) {
            lock (link)
                link.Stop();
            lock (_clients)
                _clients.Remove(link);
            leaveClient?.Invoke(link.ID, reason, additional);
        }
    }
}