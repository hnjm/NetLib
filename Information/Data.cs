using System;
using System.Text;
using System.Text.RegularExpressions;
using static Micro.NetLib.Core;

/*  PACKET SYNTAX
 *  Skeleton:      [packet start][message bytes length][internal]                                   [packet end]
 *  Message:                                                     [text start][base64 text][text end]
 */

namespace Micro.NetLib.Information {
    public class Data {
        public const string packetStart = "\x1",
            packetEnd = "\x4",
            textStart = "\x2",
            textEnd = "\x3",
            regexBase64 = @"(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?",
            textSep_s = "|";
        public const char textSep_c = '|';
        public string[] Cmds {
            get => PullStrings(rawData);
            set => rawData = PushStrings(value);
        }
        public int Length { get; private set; }
        public bool Intern { get; private set; }
        public string Capsule => textStart + rawData + textEnd;
        public string Packet => packetStart + Length + (Intern ? "1" : "0") + Capsule + packetEnd;
        public bool IsComplete => Capsule.Length == Length;
        public bool IsNew => Length == 0;
        static readonly Regex regx = new Regex(
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

        public Data() {
        }
        public Data(bool intern, params string[] cmds) {
            if (cmds.Length == 0)
                throw new ArgumentException("Empty cmds.");

            Cmds = cmds;
            Intern = intern;
            Length = Capsule.Length;
        }
        public void AddRaw(string data, out string excess) {
            if (IsNew) {
                var match = regx.Match(data);
                if (match.Success) {
                    var grps = match.Groups;
                    Length = int.Parse(grps[2].Value);
                    Intern = grps[3].Value == "1";
                    rawData = grps[4].Value;
                    excess = grps[1].Value + grps[5].Value;
                } else
                    excess = data;
            } else
                excess = data;
        }
        public byte[] GetBytes()
            => Encode(Packet);
        public bool IsCommand(InternalCommands cmd)
            => Intern && Cmds[0] == EnumString(cmd);
        public Data Clone()
            => new Data {
                rawData = rawData,
                Length = Length,
                Intern = Intern
            };

        public static string PushStrings(string[] strs, string sep = textSep_s) {
            if (strs.Length == 0)
                return "";

            var ret = "";
            for (var i = 0; i < strs.Length; i++)
                ret += ToBase64(strs[i]) + (i < strs.Length - 1 ? sep : "");

            return ret;
        }
        public static string[] PullStrings(string pushed, char sep = textSep_c) {
            if (string.IsNullOrWhiteSpace(pushed))
                return new string[0];

            string[] ret = pushed.Split(sep);
            for (var i = 0; i < ret.Length; i++)
                ret[i] = FromBase64(ret[i]);

            return ret;
        }
        public static byte[] Encode(string text)
            => Encoding.ASCII.GetBytes(text);
        public static string Decode(byte[] data)
            => Encoding.ASCII.GetString(data);
        public static string ToBase64(string text)
            => Convert.ToBase64String(Encode(text));
        public static string FromBase64(string base64)
            => Regex.IsMatch(base64, $"^{regexBase64}$") ? Decode(Convert.FromBase64String(base64)) : base64;
    }
}