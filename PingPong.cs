using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Micro.ThreadTimer;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class PingPong {
        public event Action timeout;
        Link link;
        Timer timer;
        bool waiting;
        int counter;

        public PingPong(Link l) {
            timer = new Timer(tennis);
            timer.Tick += ping;
            link = l;
        }

        public void start() {
            timer.start();
        }
        public void stop() {
            timer.stop();
        }
        public void pingReply() {
            link.write(true, EnumString(InternalCommands.pong));
        }
        public void pong() {
            counter = 0;
            waiting = false;
        }
        void ping() {
            if (!waiting) {
                link.write(true, EnumString(InternalCommands.ping));
                waiting = true;
            }
        }
        async Task waitResponse() {
            for (counter = 0; counter < tennisSec; counter++) {
                if (waiting)
                    await Task.Delay(tennisPause);
                else
                    return;
            }
            timeout();
        }
    }
}
