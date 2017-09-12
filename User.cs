#pragma warning disable CS0660, CS0661
using System.Text.RegularExpressions;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class User : Identified {
        public static Regex regx = new Regex(@"^[(\[{]?(" + PatternSGUID + @")[)\]}]?,(.+)$");
        public readonly string Nickname;

        public User(SGuid id, string nick) : base(true) {
            ID = id;
            Nickname = nick;
            debugNotice(this);
        }
        public static User Parse(string txt) {
            var grps = regx.Match(txt).Groups;
            string nick = grps[2].Value;
            return new User(SGuid.Parse(grps[1].Value), string.IsNullOrWhiteSpace(nick) ? "Anonymous" : nick);
        }
        public override string ToString()
            => string.Join(",", ID, Nickname);
        public static bool operator ==(User a, User b)
            => a?.ID == b?.ID;
        public static bool operator ==(SGuid a, User b)
            => a == b?.ID;
        public static bool operator ==(User a, SGuid b)
            => a?.ID == b;
        public static bool operator !=(User a, User b)
            => a?.ID != b?.ID;
        public static bool operator !=(SGuid a, User b)
            => a != b?.ID;
        public static bool operator !=(User a, SGuid b)
            => a?.ID != b;
    }
}