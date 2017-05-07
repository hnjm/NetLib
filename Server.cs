using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Micro.ThreadTimer;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class Server {
        TcpListener tcp;
        public event Action<Guid> joinClient;
        public event Action<Guid, stopReason, string> leaveClient;
        public event Action<Directive> received;
        public event Action<bool> listening;
        public event Action stopped;

        public bool Listening { get; private set; }
        public readonly int port;
        public readonly Guid id;
        List<Link> _clients;
        Timer clock;
        states state;
        public IReadOnlyList<Link> clients { get { return _clients; } }

        public Server(int port, Guid? customID = null) {
            ShowConsole();
            tcp = new TcpListener(IPAddress.Any, port);
            this.port = port;
            state = states.none;
            id = customID ?? Guid.NewGuid();
            _clients = new List<Link>();
            clock = new Timer(serverTick);
            clock.Tick += tick;
        }

        public void start() {
            if (state == states.none) {
                try {
                    if (!Listening)
                        tcp.Start();
                    state = states.listening;
                    clock.start();
                    Listening = true;
                    listening(true);
                } catch (Exception) {
                    state = states.none;
                    clock.stop();
                    Listening = false;
                    listening(false);
                }
            }
        }
        public void stop() {
            state = states.disconnecting;
            broadcast(true, EnumString(commands.disconnect), EnumString(stopReason.serverStop), "");
            state = states.none;
            endStop();
        }
        void endStop() {
            Listening = false;
            state = states.none;
            clock.stop();
            tcp.Stop();
            stopped();
        }
        void tick() {
            if (state == states.listening) {
                if (tcp.Pending()) {
                    var client = tcp.AcceptTcpClient();
                    var user = new Link(client);
                    user.read += (a) => read(user, a);
                    user.disconnect += (a) => disconnect(user, a, "");
                    _clients.Add(user);
                    user.start();
                }
            }
        }
        
        void read(Link link, Data data) {
            if (data.intern ?? false) {
                commands cmd = StringEnum<commands>(data.cmds[0]);
                writeCommand("Server internal", cmd, link, _clients.IndexOf(link) + 1);

                if (cmd == commands.connect) {
                    write(link, true, EnumString(commands.ok));
                    link.setID(Guid.Parse(data.cmds[1]));
                    link.state = states.none;
                    joinClient(link.ID);
                }
                if (cmd == commands.disconnect && link.state == states.none)
                    disconnect(link, StringEnum<stopReason>(data.cmds[1]), data.cmds[2]);
            } else {
                write(link, true, EnumString(commands.ok));
                foreach (var cmd in data.cmds) {
                    received(Directive.Parse(cmd));
                }
            }
        }
        void write(Link l, bool intern, params string[] cmds) {
            try {
                l.write(intern, cmds);
            } catch (Exception) {
                disconnect(l, stopReason.dropped, "");
            }
        }
        void broadcast(bool intern, params string[] cmds) {
            broadcast(intern, null, cmds);
        }
        void broadcast(bool intern, Guid[] skip = null, params string[] cmds) {
            foreach (var l in _clients.ToList()) {
                if (!(skip?.Contains(l.ID) ?? false))
                    write(l, intern, cmds);
            }
        }
        public void write(Guid id, params Directive[] msgs) {
            var l = _clients.Find(a => a.ID == id);
            if (l != null)
                write(l, false, msgs.toString());
        }
        public void broadcast(Directive msg, Guid[] skip = null) {
            broadcast(false, skip, msg);
        }
        public void kick(Guid id, string reason) {
            lock (this) {
                var lnk = _clients.FindIndex(a => a.ID == id);
                if (lnk >= 0) {
                    var rsn = stopReason.kicked;
                    write(_clients[lnk], true, EnumString(commands.disconnect), EnumString(rsn), reason);
                    disconnect(_clients[lnk], rsn, reason);
                }
            }
        }
        void disconnect(Link link, stopReason reason, string additional) {
            lock (this) {
                _clients.Remove(link);
                link.stop();
                leaveClient(link.ID, reason, additional);
            }
        }
    }
}
