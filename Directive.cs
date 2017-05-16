using System;
using System.Text.RegularExpressions;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class Directive {
        public const string textSep_s = "_";
        public const char   textSep_c = '_';
        public static Regex regx = new Regex(
            string.Format(@"^{1}{0}{2}{0}{3}{0}{4}$",
                textSep_s,
                @"(\d+)",
                @"[(\[{]?([-0-9a-f]{36})[)\]}]?",
                @"[(\[{]?([-0-9a-f]{36})[)\]}]?",
                @"([\w\W]*)"));
        public string[] values;
        public Guid from, to;
        public ManagedCommands type;
        
        public Directive(ManagedCommands type, Guid from, params string[] values) {
            this.type = type;
            this.from = from;
            this.to = Guid.Empty;
            this.values = values;
        }
        public Directive(ManagedCommands type, Guid from, Guid to, params string[] values) {
            this.type = type;
            this.from = from;
            this.to = to;
            this.values = values;
        }
        internal static Directive Parse(string msg) {
            var grps = regx.Match(msg).Groups;
            return new Directive(
                StringEnum<ManagedCommands>(grps[1].Value),
                Guid.Parse(grps[2].Value),
                Guid.Parse(grps[3].Value),
                Data.pullStrings(grps[4].Value, textSep_c));
        }

        public override string ToString() => string.Join(textSep_s, EnumString(type), from, to, Data.pushStrings(values, textSep_s));
        public static implicit operator string(Directive m) => m.ToString();
    }
}
