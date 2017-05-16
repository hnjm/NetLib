using System;
using System.Text;
using System.Text.RegularExpressions;
using static Micro.NetLib.Core;

/*  PACKET SYNTAX
 *  Skeleton:      [packet start][message bytes length][internal]                                   [packet end]
 *  Message:                                                     [text start][base64 text][text end]
 */

namespace Micro.NetLib {
    public class Data {
        public const string packetStart = "\x1",
                            packetEnd   = "\x4",
                            textStart   = "\x2",
                            textEnd     = "\x3",
                            regexBase64 = @"(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?",
                            textSep_s   = "|";
        public const char   textSep_c   = '|';
        private static readonly Regex regx = new Regex(
            string.Format(@"^{0}{1}{2}{3}{4}{5}{6}{7}$",
                @"(.*?)",
                    packetStart,
                    @"(\d+?)(0|1)",
                    textStart,
                        $@"((?:{regexBase64}\{textSep_s}?)+)",
                    textEnd,
                    packetEnd,
                @"(.*)"));
        string rawData;
        public string[] cmds {
            get { return pullStrings(rawData); }
            set { rawData = pushStrings(value); }
        }
        public int length { get; private set; }
        public bool intern { get; private set; }
        public string capsule  =>
            new StringBuilder()
                .Append(textStart)
                .Append(rawData)
                .Append(textEnd)
            .ToString();
        public string packet =>
            new StringBuilder()
                .Append(packetStart)
                .Append(length)
                .Append(intern ? "1" : "0")
                .Append(capsule)
                .Append(packetEnd)
            .ToString();
        public bool isComplete => capsule.Length == length;
        public bool isNew      => length == 0;

        public Data() { }
        public Data(bool intern, params string[] cmds) {
            if (cmds.Length == 0)
                throw new ArgumentException("Empty cmds.");

            this.cmds = cmds;
            this.intern = intern;
            length = capsule.Length;
        }

        public void addRaw(string data, out string excess) {
            if (isNew) {
                var match = regx.Match(data);
                if (match.Success) {
                    var grps = match.Groups;
                    length = int.Parse(grps[2].Value);
                    intern = grps[3].Value == "1";
                    rawData = grps[4].Value;
                    excess = grps[1].Value + grps[5].Value;
                } else
                    excess = data;
            } else
                excess = data;
        }
        public byte[] getBytes() => encode(packet);
        public bool IsCommand(InternalCommands cmd) => intern && cmds[0] == EnumString(cmd);
        public Data Clone() => new Data() {
            rawData = rawData,
            length = length,
            intern = intern
        };

        public static string pushStrings(string[] strs, string sep = textSep_s) {
            if (strs.Length == 0)
                return "";
            string ret = "";
            for (int i = 0; i < strs.Length; i++)
                ret += toBase64(strs[i]) + (i < strs.Length - 1 ? sep : "");
            return ret;
        }
        public static string[] pullStrings(string pushed, char sep = textSep_c) {
            if (string.IsNullOrWhiteSpace(pushed))
                return new string[0];
            string[] ret = pushed.Split(sep);
            for (int i = 0; i < ret.Length; i++)
                ret[i] = fromBase64(ret[i]);
            return ret;
        }
        public static byte[] encode(string text)       => Encoding.ASCII.GetBytes(text);
        public static string decode(byte[] data)       => Encoding.ASCII.GetString(data);
        public static string toBase64(string text)     => Convert.ToBase64String(encode(text));
        public static string fromBase64(string base64) => Regex.IsMatch(base64, $"^{regexBase64}$") ?
                                                              decode(Convert.FromBase64String(base64)) : base64;
    }
}