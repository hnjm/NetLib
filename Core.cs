#pragma warning disable CS0162
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Micro.NetLib {
    public static class Core {
        public const bool dataDebug = false,
                          commandDebug = false,
                          highLevelDebug = false,
                          consoleDebug = false;
        public const int linkTick = 25,
                         serverTick = 250,
                         tennis = 10000,
                         tennisPause = 1000,
                         tennisWait = 30000,
                         tennisSec = tennisWait / tennisPause;
        public const char lineGroup = '|';
        static bool console = false;

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(uint nStdHandle);
        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(uint nStdHandle, IntPtr handle);

        public static void ShowConsole() {
            if (consoleDebug && !console) {
                console = true;
                IntPtr handle = (IntPtr)random(0, int.MaxValue);
                AllocConsole();
                IntPtr defaultStdout = new IntPtr(7);
                IntPtr currentStdout = GetStdHandle((uint)handle);

                if (currentStdout != defaultStdout)
                    SetStdHandle((uint)handle, defaultStdout);
                
                TextWriter writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                Console.SetOut(writer);
            }
        }

        public static string EnumString(dynamic val){
            return (int)val + "";
        }
        public static type StringEnum<type>(string val) {
            return (type)Enum.Parse(typeof(type), val);
        }
        public static void AddTuple<T1, T2>(this IList<Tuple<T1, T2>> list, T1 item1, T2 item2) {
            list.Add(Tuple.Create(item1, item2));
        }
        public static string[] toString(this object[] val) {
            var ret = new string[val.Length];
            for (int i = 0; i < val.Length; i++) {
                ret[i] = val[i].ToString();
            }
            return ret;
        }
        public static int random(int min, int max, int seed = 0) {
            return new Random(seed != 0 ? seed : (int)DateTime.Now.Ticks).Next(min, max);
        }
        
        public static void writeData(string action, Data data) {
            if (dataDebug) {
                string commands = "";
                for (int i = 0; i < data.cmds.Length; i++) {
                    var s = data.cmds[i];
                    commands += i == 0 ? $"\t[{s}]" : $"\n\t\t\t[{s}]";
                }
                if (string.IsNullOrWhiteSpace(commands))
                    commands = "\tempty";

                var strings = new string[] {
                    action,
                    $"Length:\t{data.length}",
                    $"Internal:\t{data.intern ?? false}",
                    $"Commands:{commands}"
                };
                writeSection(strings);
            }
        }
        public static void writeCommand(string action, commands cmd, Link link, int number = 0) {
            if (commandDebug) {
                var numSt = number > 0 ? $" [{number}]" : "";
                var strings = new string[] {
                    $"{action}{numSt}",
                    $"Command:\t{cmd}",
                    $"Link ID:\t{link.ID}",
                    $"Link st:\t{link.state}"
                };
                writeSection(strings);
            }
        }
        public static void writeHigh(Connection c, string action, Directive msg = null, int number = 0) {
            if (highLevelDebug) {
                if (msg != null) {
                    string values = "";
                    for (int i = 0; i < msg.values.Length; i++) {
                        var s = msg.values[i];
                        values += i == 0 ? $"\t[{s}]" : $"\n\t\t\t[{s}]";
                    }
                    if (string.IsNullOrWhiteSpace(values))
                        values = "\tempty";

                    var numSt = number > 0 ? $" [{number}]" : "";
                    var strings = new string[] {
                        $"{action}{numSt}",
                        $"Type:\t{(Commands)msg.type}",
                        $"From:\t{c.getUser(msg.from)?.nickname ?? msg.from + ""}",
                        $"To:\t\t{c.getUser(msg.to)?.nickname ?? (msg.to != Guid.Empty ? msg.to + "" : "<everyone>")}",
                        $"Values:{values}"
                    };
                    writeSection(strings);
                } else
                    writeSection(action);
            }
        }
        static void writeSection(params string[] strings) {
            var now = DateTime.Now;
            var time = $"{(now.Minute + "").PadLeft(2, '0')}:{(now.Second + "").PadLeft(2, '0')}.{(now.Millisecond + "").PadLeft(3, '0')}";
            for (int i = 0; i < strings.Length; i++) {
                var nl = i == 0 ? "\n" : "";
                Console.WriteLine($"{nl}{time}  {lineGroup}{strings[i]}");
            }
        }
    }

    public enum commands {
        ok,
        ping,
        pong,
        received,
        connect,
        disconnect
    }
    public enum states {
        none,
        creating,
        listening,
        disconnecting
    }
    public enum stopReason {
        user,
        timeout,
        dropped,
        serverStop,
        kicked
    }
}
