#pragma warning disable CS0660, CS0661
using System.Text.RegularExpressions;
using static Micro.NetLib.Core;

namespace Micro.NetLib.Information {
    public class User : Identified, ISerializable, IParsable<User> {
        //sguid,nick
        public static Regex regx = new Regex(@"^[(\[{]?(" + PatternSGUID + @")[)\]}]?,(.+)$");
        public readonly string Nickname;

        public User(SGuid id, string nick) : base(true) {
            ID = id;
            Nickname = nick;
            debugNotice(this);
        }
        public string Serialize()
            => string.Join(",", ID, Nickname);
        public static User Parse(string txt) {
            var grps = regx.Match(txt).Groups;
            string nick = grps[2].Value;
            return new User(SGuid.Parse(grps[1].Value), string.IsNullOrWhiteSpace(nick) ? "<blank_nickname>" : nick);
        }
        public override string ToString()
            => Nickname;
        public static implicit operator string(User u)
            => u.Serialize();
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
        User IParsable<User>.Parse(string str)
            => Parse(str);
    }
}