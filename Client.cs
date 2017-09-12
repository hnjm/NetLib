using System;
using System.Net.Sockets;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class Client : Identified {
        public event Action<bool> connected;
        public event Action<StopReason, string> disconnected;
        public event Action<Directive> received;
        public bool Connected { get; private set; }
        public readonly string hostname;
        public readonly ushort port;
        Link link;
        TcpClient tcp;

        public Client(string hostname, ushort port, SGuid? customID = null) {
            tcp = new TcpClient();
            this.hostname = hostname;
            this.port = port;
            ID = customID ?? SGuid.NewSGuid();
        }
        public void Connect() {
            if (Connected)
                return;
            bool res = false;
            try {
                redirect = link = new Link(tcp, ID);
                link.received += read;
                link.disconnect += a => _disconnect(a, "");
                tcp.ConnectAsync(hostname, port).Wait(connectTimeout);
                res = true;
            } catch (Exception) {
                connected?.Invoke(false);
            }
            Connected = res;
            if (res) {
                link.Start();
                link.Write(true, EnumString(InternalCommands.connect), ID.ToString());
                debugInstances.Add(this);
                linkTables(link);
            }
        }
        public void Disconnect() {
            write(true, EnumString(InternalCommands.disconnect), EnumString(StopReason.user), "");
            lock (this) {
                _disconnect(StopReason.user, "");
                tcp = null;
                link = null;
            }
            debugInstances.Remove(this);
        }
        public void Write(params Directive[] msgs) {
            write(false, msgs.AllStrings());
        }
        void read(Data data) {
            if (data.Intern) {
                var cmd = StringEnum<InternalCommands>(data.Cmds[0]);
                link.debugCommand(false, link, cmd);
                if (cmd == InternalCommands.ok && link.state == LinkStates.creating) {
                    lock (link)
                        link.state = LinkStates.ready;
                    connected?.Invoke(true);
                    debugNotice(this);
                }
                if (cmd == InternalCommands.disconnect && link.state == LinkStates.ready)
                    _disconnect(StringEnum<StopReason>(data.Cmds[1]), data.Cmds[2]);
            }
            else {
                write(true, EnumString(InternalCommands.ok));
                foreach (string cmd in data.Cmds)
                    received(Directive.Parse(cmd));
            }
        }
        void write(bool intern, params string[] cmds) {
            link.Write(intern, cmds);
            if (intern)
                link.debugCommand(true, link, cmds);
        }
        void _disconnect(StopReason reason, string additional) {
            Connected = false;
            lock (tcp)
                tcp.Close();
            lock (link)
                link.Stop();
            disconnected?.Invoke(reason, additional);
            debugNotice(this);
        }
    }
}