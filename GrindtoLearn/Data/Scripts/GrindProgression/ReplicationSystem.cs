using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRage.Game;
using VRage.ObjectBuilders;
using VRageMath;

namespace Phoera
{
    public class ReplicationSystem
    {
        Dictionary<Type, Replicator> _replicators = new Dictionary<Type, Replicator>();
        Dictionary<string, Replicator> _stringReplicators = new Dictionary<string, Replicator>();

        public Replicator<T> GetReplicator<T>()
        {
            Replicator repl;
            if (_replicators.TryGetValue(typeof(T), out repl))
            {
                return (Replicator<T>)repl;
            }
            if (typeof(T).ToString().EndsWith("]"))
            {
                var r = new ReplicatorWrapper<T>(new ArrayReplicator(this, typeof(T).ToString()));
                RegisterReplicator(r);
                return r;
            }
            return new XmlReplicator<T>();
        }
        void RegisterReplicator<T>(Replicator<T> repl)
        {
            _replicators[typeof(T)] = repl;
            _stringReplicators[typeof(T).ToString()] = repl;
        }
        Replicator GetReplicator(string type)
        {
            Replicator repl;
            if (_stringReplicators.TryGetValue(type, out repl))
            {
                return repl;
            }
            if (type.EndsWith("]"))
            {
                return new ArrayReplicator(this, type);
            }
            return null;
        }
        public ReplicationSystem()
        {
            RegisterReplicator(new IntReplicator());
            RegisterReplicator(new UIntReplicator());
            RegisterReplicator(new ULongReplicator());
            RegisterReplicator(new LongReplicator());
            RegisterReplicator(new ShortReplicator());
            RegisterReplicator(new UShortReplicator());
            RegisterReplicator(new ByteReplicator());
            RegisterReplicator(new SByteReplicator());
            RegisterReplicator(new CharReplicator());
            RegisterReplicator(new StringReplicator());
            RegisterReplicator(new DateTimeReplicator());
            RegisterReplicator(new TimeSpanReplicator());
            RegisterReplicator(new FloatReplicator());
            RegisterReplicator(new DoubleReplicator());
            RegisterReplicator(new DecimalReplicator());
            RegisterReplicator(new MyDefinitionIdReplicator());
            RegisterReplicator(new ByteArrayReplicator());
            RegisterReplicator(new RndReplicator());
        }
        public abstract class Replicator
        {
            public Replicator(int? length, Type type)
            {
                HaveConstantLength = length != null;
                ConstantLength = length ?? 0;
                Type = type;
            }
            public Type Type { get; protected set; }
            public virtual int RequestLength(object data)
            {
                return ConstantLength;
            }
            public bool HaveConstantLength { get; }
            public int ConstantLength { get; }
            public abstract void WriteObject(object data, byte[] target, ref int offset);
            public abstract object GetVirtObject(byte[] data, ref int offset);
        }
        public abstract class Replicator<T> : Replicator
        {
            public Replicator(int? length) : base(length, typeof(T)) { }
            public virtual int RequestLength(T data)
            {
                return ConstantLength;
            }
            public override int RequestLength(object data)
            {
                if (data is T)
                {
                    return RequestLength((T)data);
                }
                return 0;
            }
            public abstract void WriteObject(T data, byte[] target, ref int offset);
            public abstract T GetObject(byte[] data, ref int offset);
            public override object GetVirtObject(byte[] data, ref int offset)
            {
                return GetObject(data, ref offset);
            }
            public override void WriteObject(object data, byte[] target, ref int offset)
            {
                if (data is T)
                {
                    WriteObject((T)data, target, ref offset);
                }
            }
        }
        #region Registable Replicators
        public class IntReplicator : Replicator<int>
        {
            public IntReplicator() : base(4)
            {
            }

            public override int GetObject(byte[] data, ref int offset)
            {
                int res = data[offset];
                res |= (data[offset + 1] << 8);
                res |= (data[offset + 2] << 16);
                res |= (data[offset + 3] << 24);
                offset += 4;
                return res;
            }

            public override void WriteObject(int data, byte[] target, ref int offset)
            {
                target[offset] = (byte)(data & 0x000000FF);
                target[offset + 1] = (byte)((data & 0x0000FF00) >> 8);
                target[offset + 2] = (byte)((data & 0x00FF0000) >> 16);
                target[offset + 3] = (byte)((data & 0xFF000000) >> 24);
                offset += 4;
            }
        }
        public class UIntReplicator : Replicator<uint>
        {
            public UIntReplicator() : base(4)
            {
            }


            public override uint GetObject(byte[] data, ref int offset)
            {
                uint res = data[offset];
                res |= (((uint)data[offset + 1]) << 8);
                res |= (((uint)data[offset + 2]) << 16);
                res |= (((uint)data[offset + 3]) << 24);
                offset += 4;
                return res;
            }

            public override void WriteObject(uint data, byte[] target, ref int offset)
            {
                target[offset] = (byte)(data & 0x000000FF);
                target[offset + 1] = (byte)((data & 0x0000FF00) >> 8);
                target[offset + 2] = (byte)((data & 0x00FF0000) >> 16);
                target[offset + 3] = (byte)((data & 0xFF000000) >> 24);
                offset += 4;
            }
        }
        public class ULongReplicator : Replicator<ulong>
        {
            public ULongReplicator() : base(8)
            {
            }


            public override ulong GetObject(byte[] data, ref int offset)
            {
                ulong res = data[offset];
                res |= (((ulong)data[offset + 1]) << 8);
                res |= (((ulong)data[offset + 2]) << 16);
                res |= (((ulong)data[offset + 3]) << 24);
                res |= (((ulong)data[offset + 4]) << 32);
                res |= (((ulong)data[offset + 5]) << 40);
                res |= (((ulong)data[offset + 6]) << 48);
                res |= (((ulong)data[offset + 7]) << 56);
                offset += 8;
                return res;
            }

            public override void WriteObject(ulong data, byte[] target, ref int offset)
            {
                target[offset] = (byte)(data & 0xFF);
                target[offset + 1] = (byte)((data & 0xFF00) >> 8);
                target[offset + 2] = (byte)((data & 0xFF0000) >> 16);
                target[offset + 3] = (byte)((data & 0xFF000000) >> 24);
                target[offset + 4] = (byte)((data & 0xFF00000000) >> 32);
                target[offset + 5] = (byte)((data & 0xFF0000000000) >> 40);
                target[offset + 6] = (byte)((data & 0xFF000000000000) >> 48);
                target[offset + 7] = (byte)((data & 0xFF00000000000000) >> 56);
                offset += 8;
            }
        }
        public class LongReplicator : Replicator<long>
        {
            public LongReplicator() : base(8)
            {
            }


            public override long GetObject(byte[] data, ref int offset)
            {
                long res = data[offset];
                res |= (((long)data[offset + 1]) << 8);
                res |= (((long)data[offset + 2]) << 16);
                res |= (((long)data[offset + 3]) << 24);
                res |= (((long)data[offset + 4]) << 32);
                res |= (((long)data[offset + 5]) << 40);
                res |= (((long)data[offset + 6]) << 48);
                res |= (((long)data[offset + 7]) << 56);
                offset += 8;
                return res;
            }

            public override void WriteObject(long data, byte[] target, ref int offset)
            {
                target[offset] = (byte)(data & 0xFF);
                target[offset + 1] = (byte)((data & 0xFF00) >> 8);
                target[offset + 2] = (byte)((data & 0xFF0000) >> 16);
                target[offset + 3] = (byte)((data & 0xFF000000) >> 24);
                target[offset + 4] = (byte)((data & 0xFF00000000) >> 32);
                target[offset + 5] = (byte)((data & 0xFF0000000000) >> 40);
                target[offset + 6] = (byte)((data & 0xFF000000000000) >> 48);
                target[offset + 7] = (byte)((data >> 56) & 0xFF);
                offset += 8;
            }
        }
        public class ShortReplicator : Replicator<short>
        {
            public ShortReplicator() : base(2)
            {
            }
            public override short GetObject(byte[] data, ref int offset)
            {
                short res = data[offset];
                res |= (short)((data[offset + 1]) << 8);
                offset += 2;
                return res;
            }

            public override void WriteObject(short data, byte[] target, ref int offset)
            {
                target[offset] = (byte)(data & 0xFF);
                target[offset + 1] = (byte)((data >> 8) & 0xFF);
                offset += 2;
            }
        }
        public class UShortReplicator : Replicator<ushort>
        {
            public UShortReplicator() : base(2)
            {
            }


            public override ushort GetObject(byte[] data, ref int offset)
            {
                ushort res = data[offset];
                res |= (ushort)((data[offset + 1]) << 8);
                offset += 2;
                return res;
            }

            public override void WriteObject(ushort data, byte[] target, ref int offset)
            {
                target[offset] = (byte)(data & 0xFF);
                target[offset + 1] = (byte)((data >> 8) & 0xFF);
                offset += 2;
            }
        }
        public class ByteReplicator : Replicator<byte>
        {
            public ByteReplicator() : base(1)
            {
            }

            public override byte GetObject(byte[] data, ref int offset)
            {
                int of = offset;
                offset++;
                return data[of];
            }

            public override void WriteObject(byte data, byte[] target, ref int offset)
            {
                target[offset] = data;
                offset++;
            }
        }
        public class SByteReplicator : Replicator<sbyte>
        {
            public SByteReplicator() : base(1)
            {
            }


            public override sbyte GetObject(byte[] data, ref int offset)
            {
                int of = offset;
                offset++;
                return (sbyte)data[of];
            }

            public override void WriteObject(sbyte data, byte[] target, ref int offset)
            {
                target[offset] = (byte)data;
                offset++;
            }
        }
        public class CharReplicator : Replicator<char>
        {
            static UShortReplicator r = new UShortReplicator();
            public CharReplicator() : base(2)
            {
            }


            public override char GetObject(byte[] data, ref int offset)
            {
                return (char)r.GetObject(data, ref offset);
            }

            public override void WriteObject(char data, byte[] target, ref int offset)
            {
                r.WriteObject(data, target, ref offset);
            }
        }
        public class StringReplicator : Replicator<string>
        {
            static UShortReplicator r = new UShortReplicator();
            public StringReplicator() : base(null)
            {
            }

            public override string GetObject(byte[] data, ref int offset)
            {
                int len = r.GetObject(data, ref offset);
                var buf = new char[len];
                for (int j = 0; j < len; j++)
                {
                    buf[j] = (char)r.GetObject(data, ref offset);
                }
                return new string(buf);
            }

            public override int RequestLength(string data)
            {
                if (data == null)
                    return 2;
                return (data.Length << 1) + 2;
            }

            public override void WriteObject(string data, byte[] target, ref int offset)
            {
                if (data == null)
                {
                    r.WriteObject((ushort)data.Length, target, ref offset);
                    return;
                }
                r.WriteObject((ushort)data.Length, target, ref offset);
                for (int j = 0; j < data.Length; j++)
                {
                    r.WriteObject(data[j], target, ref offset);
                }
            }
        }
        public class DateTimeReplicator : Replicator<DateTime>
        {
            static LongReplicator l = new LongReplicator();
            public DateTimeReplicator() : base(9)
            {
            }

            public override DateTime GetObject(byte[] data, ref int offset)
            {
                var res = new DateTime(l.GetObject(data, ref offset), (DateTimeKind)data[offset]);
                offset++;
                return res;
            }

            public override void WriteObject(DateTime data, byte[] target, ref int offset)
            {
                l.WriteObject(data.Ticks, target, ref offset);
                target[offset] = (byte)data.Kind;
                offset++;
            }
        }
        public class TimeSpanReplicator : Replicator<TimeSpan>
        {
            static LongReplicator l = new LongReplicator();
            public TimeSpanReplicator() : base(8)
            {
            }

            public override TimeSpan GetObject(byte[] data, ref int offset)
            {
                return new TimeSpan(l.GetObject(data, ref offset));
            }

            public override void WriteObject(TimeSpan data, byte[] target, ref int offset)
            {
                l.WriteObject(data.Ticks, target, ref offset);
            }
        }
        public class FloatReplicator : Replicator<float>
        {
            public FloatReplicator() : base(4)
            {
            }

            public override float GetObject(byte[] data, ref int offset)
            {
                int of = offset;
                offset += 4;
                return BitConverter.ToSingle(data, of);
            }

            public override void WriteObject(float data, byte[] target, ref int offset)
            {
                var buf = BitConverter.GetBytes(data);
                Array.Copy(buf, 0, target, offset, 4);
            }
        }
        public class DoubleReplicator : Replicator<double>
        {
            public DoubleReplicator() : base(8)
            {
            }

            public override double GetObject(byte[] data, ref int offset)
            {
                int of = offset;
                offset += 8;
                return BitConverter.ToDouble(data, of);
            }

            public override void WriteObject(double data, byte[] target, ref int offset)
            {
                var buf = BitConverter.GetBytes(data);
                Array.Copy(buf, 0, target, offset, 8);
                offset += 8;
            }
        }
        public class DecimalReplicator : Replicator<decimal>
        {
            static IntReplicator i = new IntReplicator();
            public DecimalReplicator() : base(8)
            {
            }

            public override decimal GetObject(byte[] data, ref int offset)
            {
                var ar = new int[] { i.GetObject(data, ref offset), i.GetObject(data, ref offset), i.GetObject(data, ref offset), i.GetObject(data, ref offset) };
                return new decimal(ar);
            }

            public override void WriteObject(decimal data, byte[] target, ref int offset)
            {
                var ar = decimal.GetBits(data);
                i.WriteObject(ar[0], target, ref offset);
                i.WriteObject(ar[1], target, ref offset);
                i.WriteObject(ar[2], target, ref offset);
                i.WriteObject(ar[3], target, ref offset);
            }
        }
        public class Vector3DReplicator : Replicator<Vector3D>
        {
            DoubleReplicator d = new DoubleReplicator();
            public Vector3DReplicator() : base(24)
            {
            }

            public override Vector3D GetObject(byte[] data, ref int offset)
            {
                return new Vector3D(d.GetObject(data, ref offset), d.GetObject(data, ref offset), d.GetObject(data, ref offset));
            }

            public override void WriteObject(Vector3D data, byte[] target, ref int offset)
            {
                d.WriteObject(data.X, target, ref offset);
                d.WriteObject(data.Y, target, ref offset);
                d.WriteObject(data.Z, target, ref offset);
            }
        }
        public class MyDefinitionIdReplicator : Replicator<MyDefinitionId>
        {
            StringReplicator r = new StringReplicator();

            public MyDefinitionIdReplicator() : base(null)
            {
            }

            public override MyDefinitionId GetObject(byte[] data, ref int offset)
            {
                return new MyDefinitionId(MyObjectBuilderType.Parse(r.GetObject(data, ref offset)), r.GetObject(data, ref offset));
            }

            public override int RequestLength(MyDefinitionId data)
            {
                return r.RequestLength(data.TypeId.ToString()) + r.RequestLength(data.SubtypeName);
            }

            public override void WriteObject(MyDefinitionId data, byte[] target, ref int offset)
            {
                r.WriteObject(data.TypeId.ToString(), target, ref offset);
                r.WriteObject(data.SubtypeName, target, ref offset);
            }
        }
        public class ByteArrayReplicator : Replicator<byte[]>
        {
            UShortReplicator u = new UShortReplicator();

            public ByteArrayReplicator() : base(null)
            {
            }

            public override byte[] GetObject(byte[] data, ref int offset)
            {
                int len = u.GetObject(data, ref offset);
                var res = new byte[len];
                Array.Copy(data, offset, res, 0, len);
                offset += len;
                return res;
            }

            public override int RequestLength(byte[] data)
            {
                return 2 + data.Length;
            }

            public override void WriteObject(byte[] data, byte[] target, ref int offset)
            {
                u.WriteObject((ushort)data.Length, target, ref offset);
                Array.Copy(data, 0, target, offset, data.Length);
                offset += data.Length;
            }
        }
        public class RndReplicator : Replicator<Rnd>
        {
            ULongReplicator u = new ULongReplicator();

            public RndReplicator() : base(16)
            {
            }

            public override Rnd GetObject(byte[] data, ref int offset)
            {
                return new Rnd(u.GetObject(data, ref offset), u.GetObject(data, ref offset));
            }

            public override void WriteObject(Rnd data, byte[] target, ref int offset)
            {
                u.WriteObject(data.InitX, target, ref offset);
                u.WriteObject(data.InitY, target, ref offset);
            }
        }
        #endregion
        public class ReplicatorWrapper<T> : Replicator<T>
        {
            public Replicator r;
            public ReplicatorWrapper(Replicator rr) : base(rr.HaveConstantLength ? rr.ConstantLength : (int?)null)
            {
                r = rr;
            }

            public override T GetObject(byte[] data, ref int offset)
            {
                return (T)r.GetVirtObject(data, ref offset);
            }

            public override void WriteObject(T data, byte[] target, ref int offset)
            {
                r.WriteObject(data, target, ref offset);
            }
            public override int RequestLength(object data)
            {
                return r.RequestLength(data);
            }
            public override int RequestLength(T data)
            {
                return r.RequestLength(data);
            }
        }
        public class ArrayReplicator : Replicator
        {
            public Replicator r;
            IntReplicator i = new IntReplicator();
            ByteReplicator b = new ByteReplicator();
            int _rank;

            public ArrayReplicator(ReplicationSystem rs, string typeStr) : base(null, typeof(Array))
            {
                r = rs.GetReplicator(typeStr.Substring(0, typeStr.LastIndexOf('[')));
                if (r == null)
                    throw new KeyNotFoundException();
                _rank = 1;
                for (int j = typeStr.LastIndexOf('['); j < typeStr.Length; j++)
                    if (typeStr[j] == ',')
                        _rank++;
                Type = Array.CreateInstance(r.Type, Enumerable.Repeat(1, _rank).ToArray()).GetType();
            }
            public override int RequestLength(object data)
            {
                var ar = (data as Array);
                int size = 1 + (ar.Rank * 4);
                if (r.HaveConstantLength)
                    size += r.ConstantLength * ar.Length;
                else
                    foreach (var obj in IterateArray(ar))
                    {
                        size += r.RequestLength(obj);
                    }
                return size;
            }

            public override void WriteObject(object data, byte[] target, ref int offset)
            {
                var ar = (data as Array);
                if (ar.Rank != _rank)
                    throw new Exception("wrong array rank");
                b.WriteObject((byte)ar.Rank, target, ref offset);
                for (int j = 0; j < ar.Rank; j++)
                {
                    i.WriteObject(ar.GetUpperBound(j) + 1, target, ref offset);
                }
                foreach (var obj in IterateArray(ar))
                    r.WriteObject(obj, target, ref offset);
            }

            public override object GetVirtObject(byte[] data, ref int offset)
            {
                var ranks = new int[b.GetObject(data, ref offset)];
                if (ranks.Length != _rank)
                {
                    throw new Exception("wrong array rank");
                }
                for (int j = 0; j < ranks.Length; j++)
                    ranks[j] = i.GetObject(data, ref offset);
                Array ar = Array.CreateInstance(r.Type, ranks);
                var wrap = new OffsetWrapper(data, offset, r);
                IterateSetArray(ar, wrap.Create);
                offset = wrap.GetFinalOffset();
                return ar;
            }

            class OffsetWrapper
            {
                byte[] _data;
                int _offset;
                Replicator _r;

                public OffsetWrapper(byte[] data, int offset, Replicator r)
                {
                    _data = data;
                    _offset = offset;
                    _r = r;
                }

                public object Create()
                {
                    return _r.GetVirtObject(_data, ref _offset);
                }

                public int GetFinalOffset()
                {
                    return _offset;
                }
            }
        }

        static IEnumerable<object> IterateArray(Array array)
        {
            var ranks = new int[array.Rank];
            for (int i = 0; i < ranks.Length; i++)
                ranks[i] = array.GetLowerBound(i);
            while (ranks[0] <= array.GetUpperBound(0))
            {
                yield return array.GetValue(ranks);
                for (int i = ranks.Length - 1; i > 0; i--)
                {
                    ranks[i]++;
                    if (ranks[i] > array.GetUpperBound(i))
                    {
                        ranks[i - 1]++;
                        ranks[i] = array.GetLowerBound(i);
                    }
                    else
                        break;
                }
                if (ranks.Length == 1)
                    ranks[0]++;
            }
        }
        static void IterateSetArray(Array array, Func<object> factory)
        {
            var ranks = new int[array.Rank];
            for (int i = 0; i < ranks.Length; i++)
                ranks[i] = array.GetLowerBound(i);
            while (ranks[0] <= array.GetUpperBound(0))
            {
                array.SetValue(factory(), ranks);
                for (int i = ranks.Length - 1; i > 0; i--)
                {
                    ranks[i]++;
                    if (ranks[i] > array.GetUpperBound(i))
                    {
                        ranks[i - 1]++;
                        ranks[i] = array.GetLowerBound(i);
                    }
                    else
                        break;
                }
                if (ranks.Length == 1)
                    ranks[0]++;
            }
        }

        public class XmlReplicator<T> : Replicator<T>
        {
            StringReplicator r = new StringReplicator();
            T cache = default(T);
            string cached = null;

            public XmlReplicator() : base(null)
            {
            }

            public override T GetObject(byte[] data, ref int offset)
            {
                return MyAPIGateway.Utilities.SerializeFromXML<T>(r.GetObject(data, ref offset));
            }

            public override int RequestLength(T data)
            {
                cache = data;
                cached = MyAPIGateway.Utilities.SerializeToXML(data);
                return r.RequestLength(cached);
            }

            public override void WriteObject(T data, byte[] target, ref int offset)
            {
                if (cache.Equals(data))
                {
                    r.WriteObject(cached, target, ref offset);
                }
                else
                {
                    r.WriteObject(MyAPIGateway.Utilities.SerializeToXML(data), target, ref offset);
                }
            }
        }
    }
}
