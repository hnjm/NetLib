using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Micro.ThreadTimer;
using static Micro.NetLib.Core;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Micro.NetLib {
    public class Link : Identified {
        public event Action<Data> received;
        public event Action<StopReason> disconnect;
        public readonly TcpClient client;
        public LinkStates state { get; set; }
        readonly Clock clock;
        readonly PingPong pingPong;
        readonly Queue<Data> sendQueue;
        string buffer;
        bool sending;
        Data tempData;

        public Link(TcpClient c, SGuid id = default(SGuid)) {
            client = c;
            clock = new Clock(linkTick, read);
            pingPong = new PingPong(this, timeout);
            sending = false;
            buffer = "";
            sendQueue = new Queue<Data>();
            ID = id;
            state = LinkStates.creating;
        }
        public void Start() {
            clock.Start();
            pingPong.Start();
        }
        public void Stop() {
            pingPong.Stop();
            clock.Stop();
            debugNotice(this);
        }
        public void Write(bool intern, params string[] cmds) {
            var d = new Data(intern, cmds);
            if (intern || !sending)
                write(d);
            else {
                lock (sendQueue)
                    sendQueue.Enqueue(d);
            }
        }
        void read() {
            lock (client) {
                if (!client.Connected)
                    return;
                if (client.Available > 0) {
                    var data = new byte[client.Available];
                    client.GetStream().Read(data, 0, data.Length);
                    buffer += Data.Decode(data);
                }
            }
            if (buffer.Length > 0) {
                string buffOld;
                do {
                    buffOld = buffer; //<-- important
                    tempData = new Data();
                    tempData.AddRaw(buffer, out buffer);
                    if (tempData.IsComplete)
                        interpret(tempData.Clone());
                } while (buffer != buffOld);
            }
        }
        void interpret(Data pkt) {
            if (pkt.IsNew)
                return;
            debugRaw(false, pkt);
            if (pkt.IsCommand(InternalCommands.received))
                nextPacket();
            else if (pkt.IsCommand(InternalCommands.ping))
                lock (pingPong)
                    pingPong.pingReply();
            else if (pkt.IsCommand(InternalCommands.pong))
                lock (pingPong)
                    pingPong.pong();
            else {
                if (!pkt.Intern)
                    Write(true, EnumString(InternalCommands.received));
                received?.Invoke(pkt);
            }
        }
        void timeout()
            => disconnect(StopReason.timeout);
        void nextPacket() {
            lock (sendQueue) {
                if (sendQueue.Count > 0)
                    write(sendQueue.Dequeue());
                else
                    sending = false;
            }
        }
        void write(Data data) {
            if (client.Connected) {
                try {
                    debugRaw(true, data);
                    byte[] bytes = data.GetBytes();
                    client.GetStream().Write(bytes, 0, bytes.Length);
                } catch (Exception) {
                    disconnect(StopReason.dropped);
                }
            }
        }
    }
}