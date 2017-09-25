using System;
using Micro.Utils;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    internal class PingPong {
        public event Action timeout;
        readonly Link link;
        readonly Clock timer;
        bool waiting;

        public PingPong(Link l, Action timeout) {
            timer = new Clock(tennis, ping);
            this.timeout += timeout;
            link = l;
        }
        public void Start() => timer.Start();
        public void Stop()  => timer.Stop();
        public void pingReply() {
            lock (this)
                waiting = false;
            link.Write(true, EnumString(InternalCommands.pong));
        }
        public void pong() {
            lock (this)
                waiting = false;
        }
        void ping() {
            if (!waiting) {
                lock (this)
                    waiting = true;
                link.Write(true, EnumString(InternalCommands.ping));
            } else
                timeout?.Invoke();
        }
    }
}