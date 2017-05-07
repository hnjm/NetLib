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
    public class Link {
        public event Action<Data> read;
        public event Action<stopReason> disconnect;
        public readonly TcpClient client;
        public Guid ID { get; private set; }
        public states state { get; set; }
        bool sending;
        string buffer, buffOld;
        Timer clock;
        ConcurrentQueue<Data> sendQueue;
        Data tempData;
        PingPong pingPong;

        public Link(TcpClient c) {
            client = c;
            clock = new Timer(linkTick);
            clock.Tick += _read;
            sending = false;
            buffer = "";
            sendQueue = new ConcurrentQueue<Data>();
            ID = Guid.Empty;
            state = states.creating;
            pingPong = new PingPong(this);
            pingPong.timeout += timeout;
        }
        public void setID(Guid id) {
            ID = id;
        }

        public void start() {
            clock.start();
            pingPong.start();
        }
        public void stop() {
            clock.stop();
            pingPong.stop();
        }
        void _read() {
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
                            parsePacket(tempData.Clone());
                    } while (buffer != buffOld);
                }
            }
        }
        void parsePacket(Data p) {
            Task.Run(() => {
                if (!p.isNew) {
                    writeData("Read", p);
                    if (p.Is(commands.received))
                        nextPacket();
                    else if (p.Is(commands.ping))
                        write(true, EnumString(commands.pong));
                    else if (p.Is(commands.pong))
                        pingPong.pong();
                    else {
                        if (!(p.intern ?? false))
                            write(true, EnumString(commands.received));
                        read(p);
                    }
                }
            });
        }
        void timeout() {
            disconnect(stopReason.timeout);
        }

        public void write(bool intern, params string[] cmds) {
            var d = new Data(intern, cmds);
            if (intern || !sending)
                write(d);
            else
                sendQueue.Enqueue(d);
        }
        void nextPacket() {
            if (sendQueue.Count > 0) {
                Data deq;
                if (sendQueue.TryDequeue(out deq))
                    write(deq);
            } else
                sending = false;
        }
        async Task write(Data data) {
            if (client.Connected) {
                try {
                    writeData("Write", data);
                    var bytes = data.getPacket();
                    await client.GetStream().WriteAsync(bytes, 0, bytes.Length);
                } catch (Exception) {
                    disconnect(stopReason.dropped);
                }
            }
        }
    }
}
