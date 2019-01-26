using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phoera
{
    public class Rnd
    {
        static Random rnd = new Random();

        ulong _x;
        ulong _y;
        public ulong InitX { get; }
        public ulong InitY { get; }
        const ulong MAX = ulong.MaxValue;

        public Rnd()
        {
            _x = (ulong)rnd.Next();
            _x <<= 32;
            _x |= (uint)rnd.Next();
            InitX = _x;
            _y = (ulong)rnd.Next();
            _y <<= 32;
            _y |= (uint)rnd.Next();
            InitY = _y;
        }

        public Rnd(ulong x, ulong y)
        {
            InitY = _y = y;
            InitX = _x = x;
        }

        ulong Sample()
        {
            ulong x = _x;
            ulong y = _y;
            _x = y;
            x ^= x << 23;
            _y = x ^ y ^ (x >> 17) ^ (y >> 26);
            return _y + y;
        }

        public double NextDouble()
        {
            return Sample() * (1.0 / MAX);
        }

        public ulong Next()
        {
            return Sample();
        }

    }
}
