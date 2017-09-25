using System.Threading.Tasks;
using System.ComponentModel;
#pragma warning disable CS0168
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Micro.Utils;
using Micro.NetLib.Information;
using static Micro.NetLib.Core;

namespace Micro.NetLib.Abstraction {
    public abstract class TiedValue : ISerializable {
        public delegate void ChangedValueHandler(TiedValue sender);
        public event ChangedValueHandler ChangedValue;
        internal event ChangedValueHandler SyncValue;
        public string Value
            => Serialize();
        public string FullValue {
            get {
                fullRequest = true;
                var r = Value;
                fullRequest = false;
                return r;
            }
        }
        public IReadOnlyList<SGuid> Registered => registered.ToList().AsReadOnly();
        public readonly Connection connection;
        public readonly string ID;
        protected int enumerating = 0;
        protected bool fullRequest = false,
                       editRequest = false;
        internal readonly HashSet<SGuid> registered = new HashSet<SGuid>();
        public TiedValue(Connection c, string id) {
            connection = c;
            ID = id;
            ChangedValue += SyncValue;
        }
        public abstract string Serialize();
        internal void ApplyUser(SGuid id, string data) {
            editRequest = true;
            waitEnumerations();
            lock (registered) {
                if (!registered.Contains(id))
                    registered.Add(id);
            }
            trimUsers();
            applyUser(id, data);
            editRequest = false;
        }
        internal void DeleteUser(SGuid id) {
            editRequest = true;
            waitEnumerations();
            lock (registered)
                registered.Remove(id);
            trimUsers();
            editRequest = false;
        }
        internal void ClearUsers() {
            editRequest = true;
            waitEnumerations();
            lock (registered)
                registered.Clear();
            trimUsers();
            editRequest = false;
        }
        internal virtual void SentChanges() { }
        protected void TriggerChange()
            => ChangedValue?.Invoke(this);
        protected void TriggerSync()
            => SyncValue?.Invoke(this);
        protected abstract void applyUser(SGuid id, string data);
        protected abstract void trimUsers();
        void waitEnumerations()
            => Task.Factory.StartNew(() => {
                   while (enumerating > 0)
                       Task.Delay(linkTick).Wait();
               }).Wait();
    }
    
    public abstract class TiedGeneric<T> : TiedValue, IEnumerable<T>, IEnumerable<Tuple<SGuid, T>> {
        public new T Value { get; protected set; }
        protected readonly Dictionary<SGuid, T> otherValues = new Dictionary<SGuid, T>();
        public T this[SGuid u]
            => u == connection.ID ? Value : otherValues[u];
        public TiedGeneric(Connection c, string id) : base(c, id) { }
        protected override void trimUsers() {
            lock (otherValues) {
                var keys = otherValues.Keys.ToArray();
                foreach (var key in keys) {
                    if (!registered.Contains(key))
                        otherValues.Remove(key);
                }
            }
        }
        public IEnumerator<T> GetEnumerator() {
            yield return Value;
            if (editRequest)
                yield break;
            lock (this)
                enumerating++;
            lock (otherValues) {
                foreach (var u in otherValues)
                    yield return u.Value;
            }
            lock (this)
                enumerating--;
        }
        IEnumerator<Tuple<SGuid, T>> IEnumerable<Tuple<SGuid, T>>.GetEnumerator() {
            yield return new Tuple<SGuid, T>(connection.Myself, Value);
            if (editRequest)
                yield break;
            lock (this)
                enumerating++;
            lock (otherValues) {
                foreach (var u in otherValues)
                    yield return new Tuple<SGuid, T>(u.Key, u.Value);
            }
            lock (this)
                enumerating--;
        }
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public static implicit operator T(TiedGeneric<T> obj)
            => obj.Value;
    }
    
    public class TiedObject<T> : TiedGeneric<T> where T : IRenewable, IChangesCheck, new() {
        public TiedObject(Connection c, string id) : base(c, id)
            => Value = new T();
        public override string Serialize()
            => Value.Serialize();
        public void ManualSync() {
            if (Value.SomethingChanged()) {
                Value.MarkAsUnchanged();
                TriggerChange();
            }
        }
        protected override void applyUser(SGuid id, string data) {
            if (!registered.Contains(id))
                return;
            if (!otherValues.ContainsKey(id)) {
                lock (otherValues)
                    otherValues[id] = new T();
            }
            otherValues[id].Renew(data);
        }
    }
    
    public class TiedNative<T> : TiedGeneric<T> where T : struct {
        public new T Value {
            get => base.Value;
            set {
                if (!value.Equals(base.Value)) {
                    base.Value = value;
                    TriggerChange();
                }
            }
        }
        readonly TypeCode type;
        public TiedNative(Connection c, string id) : base(c, id) {
            type = Convert.GetTypeCode(default(T));
            if (type == TypeCode.Object || type == TypeCode.Empty || type == TypeCode.DBNull)
                throw new NotSupportedException(@"Generic type T must be in System.TypeCode except for Object, Empty and DBNull.");
        }
        public override string Serialize()
            => Convert.ToString(Value);
        protected override void applyUser(SGuid id, string data) {
            if (registered.Contains(id))
                otherValues[id] = (T)Convert.ChangeType(data, type);
        }
    }

    public class TiedString : TiedGeneric<string> {
        public new string Value {
            get => base.Value;
            set {
                value = value ?? "";
                if (value != base.Value) {
                    base.Value = value;
                    TriggerChange();
                }
            }
        }
        public TiedString(Connection c, string id) : base(c, id) { }
        public override string Serialize()
            => Value;
        protected override void applyUser(SGuid id, string data) {
            if (registered.Contains(id))
                otherValues[id] = data;
        }
    }

    public class TiedList<T> : TiedGeneric<List<T>> where T : struct {
        const string separator_s = ";";
        const char separator_c = ';';
        public new EventList<T> Value => (EventList<T>)base.Value;
        readonly TypeCode type;
        Queue<string> changes = new Queue<string>();
        
        public TiedList(Connection c, string id) : base(c, id) {
            base.Value = new EventList<T>();
            type = Convert.GetTypeCode(default(T));
            if (type == TypeCode.Object || type == TypeCode.Empty || type == TypeCode.DBNull)
                throw new NotSupportedException(@"Generic type T must be in System.TypeCode except for Object, Empty and DBNull.");
            Value.ItemAdd += _IS;
            Value.ItemSet += _IS;
            Value.ItemRemove += (a, b) => makeMessage(Actions.Remove, new[] { a }, new[] { b });
            Value.CollectionAdd += (a, b) => makeMessage(Actions.Set, a, b);
            Value.CollectionRemove += (a, b) => makeMessage(Actions.Remove, a, b);
            void _IS(T a, int b) => makeMessage(Actions.Set, new[] { a }, new[] { b });
        }
        string makeMessage(Actions a, IEnumerable<T> items, IEnumerable<int> indexes = null)
            => string.Join(separator_s,
                EnumString(Actions.Entire),
                Data.PushStrings(items.Select(v => stringFromT(v)).ToArray()),
                indexes == null ? null :
                Data.PushStrings(indexes.Select(v => v + "").ToArray()));
        void sendMessage(string msg) {
            lock (changes)
                changes.Enqueue(msg);
            SentChanges();
        }
        public override string Serialize() {
            if (!fullRequest && changes.Count > 0) {
                lock (changes)
                    return changes.Dequeue();
            } else {
                lock (Value)
                    return makeMessage(Actions.Entire, Value);
            }
        }
        internal override void SentChanges() {
            if (!fullRequest && changes.Count > 0)
                TriggerSync();
        }
        protected override void applyUser(SGuid id, string data) {
            var parts = data.Split(separator_c);
            var action = StringEnum<Actions>(parts[0]);
            switch (action) {
                case Actions.Entire:
                    var l = otherValues[id] = otherValues[id] ?? new List<T>();
                    l.Clear();
                    l.AddRange(Data.PullStrings(parts[1]).Select(v => stringToT(v)));
                    break;
                case Actions.Set:
                    break;
                case Actions.Remove:
                    break;
            }
        }
        string stringFromT(T value)
            => Convert.ToString(value);
        T stringToT(string value)
            => (T)Convert.ChangeType(value, type);

        enum Actions {
            Entire,
            Set,
            Remove,
        }
    }


    //public class TiedObjects<T, U> : TiedValue, IEnumerable<Tuple<SGuid, T>> where T : IEnumerable<U> where U : ISerializable {
    //    public new delegate void ChangedValueHandler(TiedObjects<T, U> sender, T oldValue, T newValue);
    //    public new event ChangedValueHandler ChangedValue;
    //    public new T Value {
    //        get => _value;
    //        set {
    //            var old = _value;
    //            _value = value;
    //            changingHere = true;
    //            base.Value = Data.PushStrings(value.Select(u => u?.Serialize()).ToArray());
    //            changingHere = false;
    //            if (firstChange || !value.Equals(old)) {
    //                firstChange = false;
    //                ChangedValue?.Invoke(this, old, value);
    //            }
    //        }
    //    }
    //    readonly Func<string, U> Parse = GetParsable<U>(typeof(U)) ??
    //        throw new NotImplementedException($@"{typeof(U).Name} must contains ""public static {typeof(U).Name} Parse(string str)""");
    //    bool changingHere = false,
    //         firstChange = true,
    //         isArray = typeof(IEnumerable<int>).IsAssignableFrom(typeof(int[]));
    //    new T _value;

    //    public new T this[SGuid u]
    //        => u == connection.ID ? Value : stringToT(otherValues[u]);
    //    public TiedObjects(Connection c, ulong id) : base(c, id)
    //        => base.ChangedValue += (send, oldV, newV) => {
    //            if (!changingHere)
    //                _value = stringToT(base.Value);
    //        };
    //    public new IEnumerator<Tuple<SGuid, T>> GetEnumerator() {
    //        yield return new Tuple<SGuid, T>(connection.Myself, Value);
    //        lock (otherValues)
    //            foreach (var u in otherValues)
    //                yield return new Tuple<SGuid, T>(u.Key, stringToT(u.Value));
    //    }
    //    IEnumerator IEnumerable.GetEnumerator()
    //        => GetEnumerator();
    //    T stringToT(string str) {
    //        if (str == null)
    //            return default(T);
    //        var strings = Data.PullStrings(str);
    //        var objs = strings.Select(s => stringToU(s));
    //        return isArray ? (T)objs.ToArray().AsEnumerable() : (T)objs;
    //    }
    //    U stringToU(string str)
    //        => str == null ? default(U) : Parse(str);
    //}


    //public class TiedNatives<T, U> : TiedValue, IEnumerable<Tuple<SGuid, T>> where T : IEnumerable<U> where U : struct {
    //    public new delegate void ChangedValueHandler(TiedNatives<T, U> sender, T oldValue, T newValue);
    //    public new event ChangedValueHandler ChangedValue;
    //    public new T Value {
    //        get => _value;
    //        set {
    //            var old = _value;
    //            _value = value;
    //            changingHere = true;
    //            base.Value = Data.PushStrings(value.Select(u => Convert.ToString(u)).ToArray());
    //            changingHere = false;
    //            if (firstChange || !value.Equals(old)) {
    //                firstChange = false;
    //                ChangedValue?.Invoke(this, old, value);
    //            }
    //        }
    //    }
    //    bool changingHere = false,
    //         firstChange = true,
    //         isArray = typeof(IEnumerable<int>).IsAssignableFrom(typeof(int[]));
    //    TypeCode type;
    //    new T _value;

    //    public new T this[SGuid u]
    //        => u == connection.ID ? Value : stringToT(otherValues[u]);
    //    public TiedNatives(Connection c, ulong id) : base(c, id) {
    //        type = Convert.GetTypeCode(new U());
    //        if (type == TypeCode.Empty || type == TypeCode.Object)
    //            throw new NotImplementedException(@"Generic type U must be in System.TypeCode, but must not be Object or Null");
    //        base.ChangedValue += (send, oldV, newV) => {
    //            if (changingHere)
    //                return;
    //            try {
    //                _value = stringToT(base.Value);
    //            } catch (FormatException ex) {
    //                Value = _value;
    //            }
    //        };
    //    }
    //    public new IEnumerator<Tuple<SGuid, T>> GetEnumerator() {
    //        yield return new Tuple<SGuid, T>(connection.Myself, Value);
    //        lock (otherValues)
    //            foreach (var u in otherValues)
    //                yield return new Tuple<SGuid, T>(u.Key, stringToT(u.Value));
    //    }
    //    IEnumerator IEnumerable.GetEnumerator()
    //        => GetEnumerator();
    //    T stringToT(string str) {
    //        if (str == null)
    //            return default(T);
    //        var strings = Data.PullStrings(str);
    //        var nums = strings.Select(s => stringToU(s));
    //        return isArray ? (T)nums.ToArray().AsEnumerable() : (T)nums;
    //    }
    //    U stringToU(string str)
    //        => str == null ? default(U) : (U)Convert.ChangeType(str, type);
    //}
}