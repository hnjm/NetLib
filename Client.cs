using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class Client {
        TcpClient tcp;
        public event Action<bool> connected;
        public event Action<stopReason, string> disconnected;
        public event Action<Directive> received;

        public bool Connected { get; private set; }
        public readonly string hostname;
        public readonly int port;
        public readonly Guid id;
        Link link;

        public Client(string hostname, int port, Guid? customID = null) {
            ShowConsole();
            tcp = new TcpClient();
            this.hostname = hostname;
            this.port = port;
            id = customID ?? Guid.NewGuid();
        }

        public void connect() {
            if (!Connected) {
                Task.Run(delegate {
                    bool res;
                    try {
                        link = new Link(tcp);
                        link.setID(id);
                        link.read += read;
                        link.disconnect += (a) => endDisconnect(a, "");
                        res = tcp.ConnectAsync(hostname, port).Wait(3000);
                    } catch (Exception) {
                        res = false;
                        connected(false);
                    }
                    Connected = res;
                    if (res) {
                        link.start();
                        link.write(true, EnumString(commands.connect), id.ToString());
                    }
                });
            }
        }
        public void disconnect() {
            write(true, EnumString(commands.disconnect), EnumString(stopReason.user), "");
            endDisconnect(stopReason.user, "");
        }
        public void write(params Directive[] msgs) {
            write(false, msgs.toString());
        }

        void read(Data data) {
            if (data.intern ?? false) {
                commands cmd = StringEnum<commands>(data.cmds[0]);
                writeCommand("Client internal", cmd, link);

                //if (ok && link.state == states.waiting)
                //    link.toggle(states.none);
                if (cmd == commands.ok && link.state == states.creating) {
                    link.state = states.none;
                    connected(true);
                }
                if (cmd == commands.disconnect && link.state == states.none) {
                    endDisconnect(StringEnum<stopReason>(data.cmds[1]), data.cmds[2]);
                }
            } else {
                write(true, EnumString(commands.ok));
                foreach (var cmd in data.cmds) {
                    received(Directive.Parse(cmd));
                }
            }
        }
        void write(bool intern, params string[] cmds) {
            link.write(intern, cmds);
        }
        void endDisconnect(stopReason reason, string additional) {
            Connected = false;
            tcp.Close();
            link.stop();
            disconnected(reason, additional);
        }
    }
}
