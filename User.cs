#pragma warning disable CS0660, CS0661
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    public class User : Identified {
        public static Regex regx = new Regex(@"^[(\[{]?([-0-9a-f]{36})[)\]}]?,(.+)$");
        public readonly string nickname;

        public User(Guid id, string nick) : base(true) {
            ID = id;
            nickname = nick;
            debugNotice(this);
        }
        public static User Parse(string txt) {
            var grps = regx.Match(txt).Groups;
            var nick = grps[2].Value;
            return new User(Guid.Parse(grps[1].Value), string.IsNullOrWhiteSpace(nick) ? "Anonymous" : nick);
        }
        public override string ToString() {
            return string.Join(",", ID, nickname);
        }

        public static bool operator ==(User a, User b) {
            return a?.ID == b?.ID;
        }
        public static bool operator ==(Guid a, User b) {
            return a == b?.ID;
        }
        public static bool operator ==(User a, Guid b) {
            return a?.ID == b;
        }
        public static bool operator !=(User a, User b) {
            return a?.ID != b?.ID;
        }
        public static bool operator !=(Guid a, User b) {
            return a != b?.ID;
        }
        public static bool operator !=(User a, Guid b) {
            return a?.ID != b;
        }
    }
}
