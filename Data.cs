using System;
using System.Text;
using System.Text.RegularExpressions;
using static Micro.NetLib.Core;

/*  PACKET SYNTAX
 *  Skeleton:       [header start][message bytes length]|                                               [trasmission end]
 *  Message:                                             [internal?]|[text start][base64 text][text end]
 */

namespace Micro.NetLib {
    public class Data {
        public const string startHeader = "\x1",
                            endTransmission = "\x4",
                            startText = "\x2",
                            endText = "\x3";
        private static readonly Regex regx = new Regex(
            string.Format(@"^(.*?){0}(\d+)\|(?:(0|1)\|)?{1}({2}){3}{4}(.*)$",
                          startHeader, startText,
                          @"(?:(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?\|?)+",
                          endText, endTransmission));
        string encCMD;
        public int length { get; private set; }
        public bool? intern { get; private set; }
        public string[] cmds {
            get { return pullStrings(encCMD); }
            set { encCMD = pushStrings(value); }
        }
        public string capsule { get { return getCapsule(intern ?? false, encCMD); } }
        
        public bool isComplete { get { return capsule.Length == length; } }
        public bool isNew { get { return length == 0; } }

        public Data() { }
        public Data(bool intern, params string[] cmds) {
            if (cmds.Length == 0)
                throw new ArgumentException("Empty cmds.");

            this.cmds = cmds;
            this.intern = intern;
            length = getCapsule(intern, encCMD).Length;
        }
        public bool Is(commands cmd) {
            return (intern ?? false) && cmds[0] == EnumString(cmd);
        }

        public static byte[] encode(string text) {
            return Encoding.UTF8.GetBytes(text);
        }
        public static string decode(byte[] data) {
            return Encoding.UTF8.GetString(data);
        }
        public static string toBase64(string text) {
            return Convert.ToBase64String(encode(text));
        }
        public static string fromBase64(string base64) {
            return decode(Convert.FromBase64String(base64));
        }
        public static string pushStrings(params string[] strs) {
            if (strs.Length == 0)
                return "";
            string ret = "";
            for (int i = 0; i < strs.Length; i++) {
                ret += toBase64(strs[i]) + (i < strs.Length - 1 ? "|" : "");
            }
            return ret;
        }
        public static string[] pullStrings(string pushed) {
            if (string.IsNullOrWhiteSpace(pushed))
                return new string[0];
            string[] ret = pushed.Split(new char[] { '|' });
            for (int i = 0; i < ret.Length; i++) {
                ret[i] = fromBase64(ret[i]);
            }
            return ret;
        }
        public static string getCapsule(bool intern, string cmd) {
            return string.Format("{0}|\x2{1}\x3", (intern ? "1" : "0"), cmd);
        }
        public byte[] getPacket() {
            var str = string.Format("\x1{0}|{1}\x4", length, capsule);
            return encode(str);
        }

        //public void addRaw(string data, out string exceed) {
        //    if (_length > 0) {
        //        addData(data, out exceed);
        //    } else {
        //        if (regLen.IsMatch(data)) {
        //            var args = data.Split('|');
        //            _length = int.Parse(args[0]);
        //            _cmd = "";
        //            if (args[1].Length > 0)
        //                addData(args[1], out exceed);
        //            else
        //                exceed = "";
        //        } else
        //            exceed = data;
        //    }
        //}
        //public void addData(string data, out string exceed) {
        //    var diff = _length - _cmd.Length;
        //    if (diff > 0) {
        //        var add = data.Substring(0, diff);
        //        exceed = data.Remove(0, diff);
        //        _cmd += add;
        //    } else
        //        exceed = data;
        //}
        //void addData(string data, out string exceed) {
        //    if ((isEmpty || !isComplete) && data != "") {
        //        var b = encode(data);
        //        var len = b.Length;
        //        var diff = length - bytesCount - len;
        //        if (diff >= 0) {
        //            checkIntern(ref data);
        //            cmd += data;
        //            exceed = "";
        //        } else {
        //            var sub = b.Where((by, i) => i < len + diff).ToArray();
        //            exceed = decode(b.Where((by, i) => i >= len + diff).ToArray());
        //            var dd = decode(sub);
        //            checkIntern(ref dd);
        //            cmd += dd;
        //        }
        //    } else
        //        exceed = data;
        //}
        //void checkIntern(ref string cmd) {
        //    if (intern == null) {
        //        if (cmd.StartsWith(regx.Match(cmd).Value)) {
        //            var arr = cmd.Split('|');
        //            intern = arr[0] == "1" ? true : false;
        //            cmd = cmd.Remove(0, 2);
        //        }
        //    }
        //}
        public void addRaw(string data, out string exceed) {
            //Console.WriteLine(
            //    "BEFORE  " + 
            //    "length: " + capsule.Length + ", " + 
            //    (isNew ? "isNew, " : "") + 
            //    (isComplete ? "isComplete, " : "") +
            //    "cmd: " + cmd + ", " + 
            //    "Data: " + data);
            if (isNew) {
                var match = regx.Match(data);
                if (match.Success) {
                    var grps = match.Groups;
                    length = int.Parse(grps[2].Value);
                    var inte = grps[3].Value;
                    if (inte != "")
                        intern = inte == "1";
                    encCMD = grps[4].Value;
                    exceed = grps[1].Value + grps[5].Value;
                } else
                    exceed = data;
            } else
                exceed = data;
            //if (isNew) {
            //    if (data.StartsWith(regx.Match(data).Value)) {
            //        var arr = data.Split('|');
            //        length = int.Parse(arr[0]);
            //        cmd = "";
            //        addData(data.Replace(arr[0] + "|", ""), out exceed);
            //    } else
            //        exceed = data;
            //} else if (!isComplete)
            //    addData(data, out exceed);
            //else
            //    exceed = data;
            //Console.WriteLine(
            //    "AFTER   " +
            //    "length: " + capsule.Length + ", " +
            //    (isNew ? "isNew, " : "") +
            //    (isComplete ? "isComplete, " : "") +
            //    "cmd: " + cmd + ", " +
            //    "Data: " + data);
        }

        public Data Clone() => new Data() {
            encCMD = encCMD,
            length = length,
            intern = intern
        };
    }
}
