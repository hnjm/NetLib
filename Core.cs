#pragma warning disable CS0162
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static Micro.NetLib.Core;

namespace Micro.NetLib {
    using Table = EventList<object[]>;
    public static class Core {
        public const int linkTick = 25,
                         serverTick = 500,
                         tennis = 10000,
                         tennisPause = 1000,
                         tennisWait = 15000,
                         tennisSec = tennisWait / tennisPause;
        public static readonly TimeSpan LocalUtcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
        public static DateTime Now => DateTime.UtcNow + LocalUtcOffset;
        public static event Action<Identified> dbgNotice;
        public static EventList<Identified> debugInstances = new EventList<Identified>();
        private static volatile FormDebug formDebug;
        internal static volatile bool trackRaw      = true,
                                      trackCommands = true,
                                      trackHigh     = true;
        
        public static string EnumString(dynamic val) => (int)val + "";
        public static type StringEnum<type>(string val) => (type)Enum.Parse(typeof(type), val);
        public static void AddTuple<T1, T2>(this IList<Tuple<T1, T2>> list, T1 item1, T2 item2) => list.Add(Tuple.Create(item1, item2));
        public static string[] AllStrings(this object[] val) {
            var ret = new string[val.Length];
            for (int i = 0; i < val.Length; i++) {
                ret[i] = val[i].ToString();
            }
            return ret;
        }
        
        public static void debugStart() {
            if (formDebug == null)
                new Thread(new ThreadStart(() => Application.Run(formDebug = new FormDebug()))).Start();
        }
        public static void debugNotice(Identified obj) => dbgNotice?.Invoke(obj);
    }

    public abstract class Identified {
        private const string msgSent = "Sent",
                             msgRec  = "Received";
        public Guid ID { get; internal set; }
        public Table tRaw     => redirect == null ? _tRaw     : redirect._tRaw;
        public Table tCommand => redirect == null ? _tCommand : redirect._tCommand;
        public Table tHigh    => redirect == null ? _tHigh    : redirect._tHigh;
        protected Identified redirect;
        Table _tRaw, _tCommand, _tHigh;

        public Identified(bool noData = false) {
            if (!noData) {
                _tRaw = new Table();
                _tCommand = new Table();
                _tHigh = new Table();
            }
        }
        protected void linkTables(Identified id2) {
            id2._tRaw.added += _tRaw.Add;
            id2._tCommand.added += _tCommand.Add;
            id2._tHigh.added += _tHigh.Add;
        }
        public void clearEvents() {
            tRaw.clearEvents();
            tCommand.clearEvents();
            tHigh.clearEvents();
        }
        
        /*     ALL: Date/Time, Sent/Received?
         *     RAW: Internal?, [MessageLength, Message]{1..*}
         *  INTERN: Command, User?, Link.ID, Link.state
         * COMPLEX: Description, User?, Directive.type, Directive.from, Directive.to (0 -> everyone), Directive.values
         */
        public void debugRaw(bool write, Data data) {
            if (trackRaw)
                tRaw.Add(new object[] { Now, write ? msgSent : msgRec, ID, data.length, data.intern, data.cmds });
        }
        public void debugCommand(bool write, Link link, params object[] cmds) {
            if (trackCommands) {
                var cmds2 = new object[cmds.Length];
                cmds.CopyTo(cmds2, 0);
                if (cmds2?[0] is string)
                    cmds2[0] = StringEnum<InternalCommands>((string)cmds2[0]);
                if (cmds2.Length > 1 && ((InternalCommands)cmds2?[0]) == InternalCommands.disconnect)
                    cmds2[1] = StringEnum<StopReason>((string)cmds2[1]);
                tCommand.Add(new object[] { Now, write ? msgSent : msgRec, ID, cmds2, link.state });
            }
        }
        public void debugHigh(Connection c, ManagedActions action, Guid id, string reason = "") {
            if (trackHigh)
                tHigh.Add(new object[] { Now, action, id, reason });
        }
        public void debugHigh(Connection c, ManagedActions action, Directive msg = null) {
            if (trackHigh)
                tHigh.Add(new object[] { Now, action, msg });
        }
    }
    public class EventList<T> : List<T> {
        public event Action<T> added, removed;

        public new void Add(T element) {
            base.Add(element);
            added?.Invoke(element);
        }
        public new void Remove(T element) {
            if (base.Remove(element))
                removed?.Invoke(element);
        }
        public void clearEvents() {
            added = null;
            removed = null;
        }
    }

    public enum LinkStates {
        none,
        creating,
        listening,
        disconnecting
    }
    public enum StopReason {
        user,
        timeout,
        dropped,
        serverStop,
        kicked
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
        msg,            //Messaggio esterno
        serverUser,     //Utente server
        app,            //Identità utente + assembly applicazione
        users,          //Lista utenti (solo alla connessione)
        join,           //Nuovo utente
        leave           //Fine utente
    }
    public enum ManagedActions {
        clientConnected,
        serverStarted,
        serverJoin,
        serverLeft,
        stop,
        received,
        send,
        sendEnqueue,
        broadcast
    }
    public enum ConnectionModes {
        none,
        client,
        server
    }
}
