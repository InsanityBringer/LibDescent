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
    // implements 16.16 fixed-point numbers as used by Descent -- Parabolicus
    public struct Fix
    {
        private int value;

        public static explicit operator int(Fix f)
            => f.value / 65536;
        public static implicit operator Fix(int i)
            => FromRawValue(checked(i * 65536));
        public static implicit operator float(Fix f)
            => f.value / 65536.0f;
        public static implicit operator Fix(float d)
            => FromRawValue(checked((int)(d * 65536.0f)));
        public static implicit operator double(Fix f)
            => f.value / 65536.0;
        public static implicit operator Fix(double d)
            => FromRawValue(checked((int)(d * 65536.0)));

        public static Fix operator +(Fix a)
            => FromRawValue(a.value);
        public static Fix operator -(Fix a)
            => FromRawValue(-a.value);
        public static Fix operator +(Fix a, Fix b)
            => FromRawValue(checked(a.value + b.value));
        public static Fix operator -(Fix a, Fix b)
            => FromRawValue(checked(a.value - b.value));

        public static Fix operator *(Fix a, Fix b)
        {
            long product = (long)a.value * (long)b.value;
            return FromRawValue(checked((int)(product >> 16)));
        }
        public static Fix operator /(Fix a, Fix b)
        {
            long quotient = ((long)a.value << 16) / (long)b.value;
            return FromRawValue((int)quotient);
        }

        public static Fix operator <<(Fix a, int shift) => FromRawValue(checked(a.value << shift));
        public static Fix operator >>(Fix a, int shift) => FromRawValue(a.value >> shift);
        public static bool operator ==(Fix a, Fix b) => a.value == b.value;
        public static bool operator !=(Fix a, Fix b) => a.value != b.value;

        public override string ToString()
        {
            return ((double)this).ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
        }

        public int GetRawValue()
        {
            return value;
        }

        public static Fix FromRawValue(int value)
        {
            return new Fix
            {
                value = value
            };
        }

        public override bool Equals(object obj)
        {
            return obj is Fix fix &&
                   value == fix.value;
        }

        public override int GetHashCode()
        {
            return -1584136870 + value.GetHashCode();
        }
    }
}
