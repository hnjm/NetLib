using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class Client : Identified, IDisposable {
        TcpClient tcp;
        public event Action<bool> connected;
        public event Action<StopReason, string> disconnected;
        public event Action<Directive> received;

        public bool Connected { get; private set; }
        public readonly string hostname;
        public readonly int port;
        Link link;

        public Client(string hostname, int port, Guid? customID = null) : base() {
            tcp = new TcpClient();
            this.hostname = hostname;
            this.port = port;
            ID = customID ?? Guid.NewGuid();
        }
        public void Dispose() {
            if (Connected)
                disconnect();
            tcp = null;
            link = null;
            debugInstances.Remove(this);
        }

        public void connect() {
            if (!Connected) {
                Task.Run(() => {
                    bool res;
                    try {
                        redirect = link = new Link(tcp, ID);
                        link.received += read;
                        link.disconnect += (a) => endDisconnect(a, "");
                        res = tcp.ConnectAsync(hostname, port).Wait(3000);
                    } catch (Exception) {
                        res = false;
                        connected(false);
                    }
                    Connected = res;
                    if (res) {
                        link.start();
                        link.write(true, EnumString(InternalCommands.connect), ID.ToString());
                        debugInstances.Add(this);
                        linkTables(link);
                    }
                });
            }
        }
        public void disconnect() {
            write(true, EnumString(InternalCommands.disconnect), EnumString(StopReason.user), "");
            endDisconnect(StopReason.user, "");
        }
        public void write(params Directive[] msgs) {
            write(false, msgs.AllStrings());
        }

        void read(Data data) {
            if (data.intern) {
                InternalCommands cmd = StringEnum<InternalCommands>(data.cmds[0]);
                link.debugCommand(false, link, cmd);
                
                if (cmd == InternalCommands.ok && link.state == LinkStates.creating) {
                    link.state = LinkStates.none;
                    connected(true);
                    debugNotice(this);
                }
                if (cmd == InternalCommands.disconnect && link.state == LinkStates.none)
                    endDisconnect(StringEnum<StopReason>(data.cmds[1]), data.cmds[2]);
            } else {
                write(true, EnumString(InternalCommands.ok));
                foreach (var cmd in data.cmds)
                    received(Directive.Parse(cmd));
            }
        }
        void write(bool intern, params string[] cmds) {
            link.write(intern, cmds);
            if (intern)
                link.debugCommand(true, link, cmds);
        }
        void endDisconnect(StopReason reason, string additional) {
            Connected = false;
            tcp.Close();
            link.stop();
            disconnected(reason, additional);
            debugNotice(this);
        }
    }
}
