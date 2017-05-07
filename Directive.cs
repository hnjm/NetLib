using System;
using System.Text.RegularExpressions;

namespace Micro.NetLib {
    public class Directive {
        public static Regex regx = new Regex(string.Format(@"^{0},{1},{2},{3}$",
                                                           @"(\d+)",
                                                           @"[(\[{]?([-0-9a-f]{36})[)\]}]?",
                                                           @"[(\[{]?([-0-9a-f]{36})[)\]}]?",
                                                           @"([\w\W]*)"));
        public string[] values;
        public Guid from, to;
        public int type;
        
        public Directive(int type, Guid from, params string[] values) {
            this.type = type;
            this.from = from;
            this.to = Guid.Empty;
            this.values = values;
        }
        public Directive(int type, Guid from, Guid to, params string[] values) {
            this.type = type;
            this.from = from;
            this.to = to;
            this.values = values;
        }
        internal static Directive Parse(string msg) {
            var grps = regx.Match(msg).Groups;
            return new Directive(
                int.Parse(grps[1].Value),
                Guid.Parse(grps[2].Value),
                Guid.Parse(grps[3].Value),
                Data.pullStrings(grps[4].Value));
        }

        public override string ToString() {
            return string.Join(",", type, from, to, Data.pushStrings(values));
        }
        public static implicit operator string(Directive m) {
            return m.ToString();
        }
    }
}
