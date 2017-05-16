using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Micro.ThreadTimer;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class Server : Identified, IDisposable {
        TcpListener tcp;
        public event Action<Guid> joinClient;
        public event Action<Guid, StopReason, string> leaveClient;
        public event Action<Directive> received;
        public event Action<bool> listening;
        public event Action stopped;

        public IReadOnlyList<Link> clients => _clientsCopy;
        public bool Listening { get; private set; }
        public readonly int port;
        List<Link> _clients;
        List<Link> _clientsCopy => _clients.ToList();
        Timer clock;
        LinkStates state;

        public Server(int port, Guid? customID = null) : base() {
            tcp = new TcpListener(IPAddress.Any, port);
            this.port = port;
            state = LinkStates.none;
            ID = customID ?? Guid.NewGuid();
            _clients = new List<Link>();
            clock = new Timer(serverTick);
            clock.Tick += tick;
        }
        public void Dispose() {
            if (Listening)
                stop();
            tcp = null;
            clock = null;
            debugInstances.Remove(this);
        }

        public void start() {
            if (state == LinkStates.none) {
                try {
                    if (!Listening)
                        tcp.Start();
                    state = LinkStates.listening;
                    clock.start();
                    Listening = true;
                    listening(true);
                    debugInstances.Add(this);
                } catch (Exception) {
                    state = LinkStates.none;
                    clock.stop();
                    Listening = false;
                    listening(false);
                }
            }
        }
        public void stop() {
            state = LinkStates.disconnecting;
            broadcast(true, EnumString(InternalCommands.disconnect), EnumString(StopReason.serverStop), "");
            Listening = false;
            state = LinkStates.none;
            clock.stop();
            tcp.Stop();
            stopped();
            _clients.Clear();
        }
        public void write(Guid id, params Directive[] msgs) {
            var link = _clients.Find(a => a.ID == id);
            if (link != null)
                write(link, false, msgs.AllStrings());
        }
        public void broadcast(Directive msg, Guid[] skip = null) {
            broadcast(false, skip, msg);
        }
        public void kick(Guid id, string reason) {
            lock (this) {
                var link = _clients.Find(a => a.ID == id);
                if (link != null) {
                    var rsn = StopReason.kicked;
                    write(link, true, EnumString(InternalCommands.disconnect), EnumString(rsn), reason);
                    disconnect(link, rsn, reason);
                }
            }
        }

        void tick() {
            if (state == LinkStates.listening) {
                if (tcp.Pending()) {
                    var client = tcp.AcceptTcpClient();
                    var link = new Link(client);
                    link.received += (a) => read(link, a);
                    link.disconnect += (a) => disconnect(link, a, "");
                    lock (_clients)
                        _clients.Add(link);
                    link.start();
                    linkTables(link);
                }
            }
        }
        void read(Link link, Data data) {
            if (data.intern) {
                InternalCommands cmd = StringEnum<InternalCommands>(data.cmds[0]);
                link.debugCommand(false, link, cmd);

                if (cmd == InternalCommands.connect) {
                    write(link, true, EnumString(InternalCommands.ok));
                    link.ID = Guid.Parse(data.cmds[1]);
                    link.state = LinkStates.none;
                    joinClient(link.ID);
                    debugNotice(this);
                }
                if (cmd == InternalCommands.disconnect && link.state == LinkStates.none)
                    disconnect(link, StringEnum<StopReason>(data.cmds[1]), data.cmds[2]);
            } else {
                write(link, true, EnumString(InternalCommands.ok));
                foreach (var cmd in data.cmds)
                    received(Directive.Parse(cmd));
            }
        }
        void write(Link link, bool intern, params string[] cmds) {
            bool success = true;
            try {
                link.write(intern, cmds);
            } catch (Exception) {
                success = false;
                disconnect(link, StopReason.dropped, "");
            }
            if (success && intern)
                link.debugCommand(true, link, cmds);
        }
        void broadcast(bool intern, params string[] cmds) {
            broadcast(intern, null, cmds);
        }
        void broadcast(bool intern, Guid[] skip = null, params string[] cmds) {
            foreach (var l in _clientsCopy) {
                if (!(skip?.Contains(l.ID) ?? false))
                    write(l, intern, cmds);
            }
        }
        void disconnect(Link link, StopReason reason, string additional) {
            lock (this) {
                _clients.Remove(link);
                link.stop();
                leaveClient(link.ID, reason, additional);
            }
        }
    }
}
