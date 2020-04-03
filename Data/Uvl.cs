/*
    Copyright (c) 2019 SaladBadger

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

namespace LibDescent.Data
{
    public struct Uvl
    {
        public Fix u;
        public Fix v;
        public Fix l;

        public Uvl(Fix u, Fix v, Fix l)
        {
            this.u = u;
            this.v = v;
            this.l = l;
        }

        public static Uvl FromRawValues(short u, short v, ushort l)
        {
            // UVL elements are written to file as 16-bit values, but are converted to 16.16 fixed-point
            // when loaded, using bitshifts. We do the same conversion here.
            return new Uvl(Fix.FromRawValue(u << 5), Fix.FromRawValue(v << 5), Fix.FromRawValue(l << 1));
        }

        public (short u, short v, ushort l) ToRawValues()
        {
            return ((short)(u.value >> 5), (short)(v.value >> 5), (ushort)(l.value >> 1));
        }

        public (double u, double v, double l) ToDoubles()
        {
            return (u, v, l);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", u, v, l);
        }
    }
}