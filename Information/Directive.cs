using System.Text.RegularExpressions;
using static Micro.NetLib.Core;

namespace Micro.NetLib.Information {
    internal class Directive : ISerializable, IParsable<Directive> {
        //type_sguid_sguid_values
        public const string textSep_s = "_";
        public const char textSep_c = '_';
        public static Regex regx = new Regex(
            string.Format(@"^{1}{0}{2}{0}{3}{0}{4}$",
                textSep_s,
                @"(\d+)",
                @"[(\[{]?(" + PatternSGUID + @")[)\]}]?",
                @"[(\[{]?(" + PatternSGUID + @")[)\]}]?",
                @"([\w\W]*)"));
        public SGuid from, to;
        public ManagedCommands type;
        public string[] values;
        //match problem?

        public Directive(ManagedCommands type, SGuid from, params string[] values) {
            this.type = type;
            this.from = from;
            to = SGuid.Empty;
            this.values = values;
        }
        public Directive(ManagedCommands type, SGuid from, SGuid to, params string[] values) {
            this.type = type;
            this.from = from;
            this.to = to;
            this.values = values;
        }
        public string Serialize()
            => string.Join(textSep_s, EnumString(type), from, to, Data.PushStrings(values, textSep_s));
        public static Directive Parse(string msg) {
            GroupCollection grps = regx.Match(msg).Groups;
            return new Directive(
                StringEnum<ManagedCommands>(grps[1].Value),
                SGuid.Parse(grps[2].Value),
                SGuid.Parse(grps[3].Value),
                Data.PullStrings(grps[4].Value, textSep_c));
        }
        public static implicit operator string(Directive m)
            => m.Serialize();
        Directive IParsable<Directive>.Parse(string str)
            => Parse(str);
    }
}