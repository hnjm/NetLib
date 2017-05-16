#pragma warning disable CS4014
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using Micro.ThreadTimer;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class Link : Identified {
        public event Action<Data> received;
        public event Action<StopReason> disconnect;
        public readonly TcpClient client;
        public LinkStates state { get; set; }
        bool sending;
        string buffer, buffOld;
        Timer clock;
        ConcurrentQueue<Data> sendQueue;
        Data tempData;
        PingPong pingPong;

        public Link(TcpClient c, Guid id = default(Guid)) : base() {
            client = c;
            clock = new Timer(linkTick);
            clock.Tick += read;
            sending = false;
            buffer = "";
            sendQueue = new ConcurrentQueue<Data>();
            ID = id;
            state = LinkStates.creating;
            pingPong = new PingPong(this);
            pingPong.timeout += timeout;
        }

        public void start() {
            clock.start();
            pingPong.start();
        }
        public void stop() {
            pingPong.stop();
            clock.stop();
            debugNotice(this);
        }
        public void write(bool intern, params string[] cmds) {
            var d = new Data(intern, cmds);
            if (intern || !sending)
                write(d);
            else
                sendQueue.Enqueue(d);
        }
        void read() {
            if (client.Connected) {
                if (client.Available > 0) {
                    var data = new byte[client.Available];
                    client.GetStream().Read(data, 0, data.Length);
                    var str = Data.decode(data);
                    buffer += str;
                }
                
                if (buffer.Length > 0) {
                    do {
                        buffOld = buffer;
                        tempData = new Data();
                        tempData.addRaw(buffer, out buffer);
                        if (tempData.isComplete)
                            interpret(tempData.Clone());
                    } while (buffer != buffOld);
                }
            }
        }
        void interpret(Data pkt) {
            Task.Run(() => {
                if (!pkt.isNew) {
                    debugRaw(false, pkt);
                    if (pkt.IsCommand(InternalCommands.received))
                        nextPacket();
                    else if (pkt.IsCommand(InternalCommands.ping))
                        pingPong.pingReply();
                    else if (pkt.IsCommand(InternalCommands.pong))
                        pingPong.pong();
                    else {
                        if (!pkt.intern)
                            write(true, EnumString(InternalCommands.received));
                        received(pkt);
                    }
                }
            });
        }
        void timeout() {
            disconnect(StopReason.timeout);
        }

        void nextPacket() {
            if (sendQueue.Count > 0) {
                if (sendQueue.TryDequeue(out Data deq))
                    write(deq);
            } else
                sending = false;
        }
        async Task write(Data data) {
            if (client.Connected) {
                try {
                    debugRaw(true, data);
                    var bytes = data.getBytes();
                    await client.GetStream().WriteAsync(bytes, 0, bytes.Length);
                } catch (Exception) {
                    disconnect(StopReason.dropped);
                }
            }
        }
    }
}
