#pragma warning disable CS0162
#pragma warning disable CS0660
#pragma warning disable CS0661
using Micro.NetLib.Abstraction;
using Micro.NetLib.Information;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Micro.Utils;

namespace Micro.NetLib {
    using Table = EventList<object[]>;

    public static class Core {
        public delegate void StartHandler(bool success);
        public delegate void StopHandler(LeaveReason reason, string additional);

        public const string PatternGUID = "[-0-9a-f]{36}";
        public const string PatternSGUID = "[0-9A-Za-z+/]{22}";
        public const int
            linkTick = 16,
            serverTick = 200,
            tennis = 10000,
            connectTimeout = 5000;
        public static readonly TimeSpan LocalUtcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        public static DateTime Now => DateTime.UtcNow + LocalUtcOffset;
        public static string EnumString(dynamic val)
            => (int)val + "";
        public static T StringEnum<T>(string val)
            => (T)Enum.Parse(typeof(T), val);
        public static void AddTuple<T1, T2>(this IList<Tuple<T1, T2>> list, T1 item1, T2 item2)
            => list.Add(Tuple.Create(item1, item2));
        public static string[] AllStrings(this object[] val) {
            var ret = new string[val.Length];
            for (var i = 0; i < val.Length; i++) ret[i] = val[i].ToString();
            return ret;
        }
        public static string[] AllSerialize<T>(this T[] val) where T : ISerializable {
            var ret = new string[val.Length];
            for (var i = 0; i < val.Length; i++) ret[i] = val[i].Serialize();
            return ret;
        }
        public static void WriteLine(string txt)
            => Debug.WriteLine(txt);
        public static Func<string, T> GetParsable<T>(Type t) where T : IParsable<T>
            => (Func<string, T>)GetInterfaceStatic<T, Func<string, T>>(t, "Parse");
        public static Delegate GetInterfaceStatic<T, D>(Type t, string m) {
            var method = t.GetMethod(m);
            if (method == null || !method.IsPublic || !method.IsStatic || method.ReturnType != t)
                return null;
            var @params = method.GetParameters();
            if (@params.Length == 1 && @params[0].ParameterType == typeof(string))
                return Delegate.CreateDelegate(typeof(D), method);
            else
                return null;
        }

        #region Debug
        internal static EventList<Identified> debugInstances = new EventList<Identified>();
        internal static event Action<Identified> dbgNotice;
        internal static volatile FormDebug formDebug;
#if DEBUG
        internal static volatile bool
            trackRaw = false,
            trackCommands = false,
            trackHigh = false;
#else
        internal static volatile bool
            trackRaw = false,
            trackCommands = false,
            trackHigh = false;
#endif

        public static void debugStart() {
            if (formDebug == null)
                new Thread(() => Application.Run(formDebug = new FormDebug())).Start();
        }
        internal static void debugNotice(Identified obj)
            => dbgNotice?.Invoke(obj);

        public class Identified {
            const string
                msgSent = "Sent",
                msgRec = "Received";
            public SGuid ID { get; internal set; }
            internal Table tRaw
                => redirect == null ? _tRaw : redirect._tRaw;
            internal Table tCommand
                => redirect == null ? _tCommand : redirect._tCommand;
            internal Table tHigh
                => redirect == null ? _tHigh : redirect._tHigh;
            protected Identified redirect;
            readonly Table _tCommand;
            readonly Table _tHigh;
            readonly Table _tRaw;

            public Identified(bool noData = false) {
                if (!noData) {
                    _tRaw = new Table();
                    _tCommand = new Table();
                    _tHigh = new Table();
                }
            }
            protected void linkTables(Identified id2) {
                id2._tRaw.ItemAdd += (o, i) => _tRaw.Add(o);
                id2._tCommand.ItemAdd += (o, i) => _tCommand.Add(o);
                id2._tHigh.ItemAdd += (o, i) => _tHigh.Add(o);
            }
            internal void clearEvents() {
                tRaw.ClearEvents();
                tCommand.ClearEvents();
                tHigh.ClearEvents();
            }
            /*     ALL: Date/Time, Sent/Received?
                *     RAW: Internal?, [MessageLength, Message]{1..*}
                *  INTERN: Command, User?, Link.ID, Link.state
                * COMPLEX: Description, User?, Directive.type, Directive.from, Directive.to (0 -> everyone), Directive.values
                */
            internal void debugRaw(bool write, Data data) {
                if (trackRaw)
                    tRaw.Add(new object[] { Now, write ? msgSent : msgRec, ID, data.Length, data.Intern, data.Cmds });
            }
            internal void debugCommand(bool write, Link link, params object[] cmds) {
                if (trackCommands) {
                    var cmds2 = new object[cmds.Length];
                    cmds.CopyTo(cmds2, 0);
                    if (cmds2[0] is string)
                        cmds2[0] = StringEnum<InternalCommands>((string)cmds2[0]);
                    if (cmds2.Length > 1 && (InternalCommands)cmds2[0] == InternalCommands.disconnect)
                        cmds2[1] = StringEnum<LeaveReason>((string)cmds2[1]);
                    tCommand.Add(new object[] { Now, write ? msgSent : msgRec, ID.ToString(), cmds2, link.state });
                }
            }
            internal void debugHigh(Connection c, ManagedEvents action, SGuid id, string reason = "") {
                if (trackHigh)
                    tHigh.Add(new object[] { Now, action, id, reason });
            }
            internal void debugHigh(Connection c, ManagedEvents action, Directive msg = null) {
                if (trackHigh)
                    tHigh.Add(new object[] { Now, action, msg });
            }
            public static implicit operator SGuid(Identified i)
                => i.ID;
        }
        #endregion Debug
    }

    public interface ISerializable {
        string Serialize();
    }
    /// <summary>
    /// This interface requires "public static [type] Parse(string str)"
    /// </summary>
    /// <example>
    /// How to achieve static Parse.
    /// <code>
    /// class YourClass : IParsable<YourClass> {
    ///     public static YourClass Parse(string str) { ... }
    ///     YourClass IParsable<YourClass>.Parse(string str)
    ///         => Parse(str);
    /// }
    /// </code>
    /// </example>
    public interface IParsable<T> : ISerializable {
        T Parse(string str);
    }
    public interface IRenewable : ISerializable {
        void Renew(string str);
    }
    public interface IChangesCheck {
        bool SomethingChanged();
        void MarkAsUnchanged();
    }

    public struct SGuid {
        public static readonly SGuid Empty = new SGuid(Guid.Empty);
        string data;
        public Guid From {
            get => new Guid(Convert.FromBase64String(data + "=="));
            set => data = Convert.ToBase64String(value.ToByteArray()).Trim('=');
        }
        public SGuid(Guid id) {
            data = "";
            From = id;
        }
        public static SGuid NewSGuid()
            => new SGuid(Guid.NewGuid());
        public static SGuid Parse(string s)
            => new SGuid { data = s };
        public override string ToString()
            => data;
        public static implicit operator string(SGuid a)
            => a.data;
        public static bool operator ==(SGuid a, SGuid b)
            => a.data == b.data;
        public static bool operator !=(SGuid a, SGuid b)
            => a.data != b.data;
    }

    public enum LinkStates {
        ready,
        creating,
        listening,
        disconnecting
    }
    public enum InternalCommands {
        ok,
        ping,
        pong,
        received,
        connect,
        disconnect
    }
    public enum ManagedCommands {
        message,    //Messaggio non gestito
        tieReg,     //Partecipa ad un valore condiviso
        tieApply,   //Aggiorna un valore condiviso
        tieUnreg,   //Abbandona un valore condiviso
        serverUser, //Utente server
        app,        //Identità utente + assembly applicazione
        users,      //Lista utenti (solo alla connessione)
        join,       //Entrata utente
        leave       //Uscita utente
    }
    public enum LeaveReason {
        user,
        timeout,
        dropped,
        serverStop,
        kicked
    }
    public enum ConnectionModes {
        idle,
        client,
        server
    }
    internal enum ManagedEvents {
        clientConnected,
        serverStarted,
        serverJoin,
        serverLeft,
        stop,
        read,
        interpret,
        send,
        broadcast
    }
}