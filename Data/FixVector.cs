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
using System;
using System.Collections.Generic;
using System.Numerics;

namespace LibDescent.Data
{
    public struct FixAngles
    {
        public short p, b, h;

        public FixAngles(short p, short b, short h)
        {
            this.p = p;
            this.b = b;
            this.h = h;
        }

        public static FixAngles FromRawValues(short p, short b, short h)
        {
            return new FixAngles(p, b, h);
        }
    }

    public struct FixVector
    {
        public Fix x;
        public Fix y;
        public Fix z;

        public FixVector(Fix x, Fix y, Fix z)
        {
            this.x = x; this.y = y; this.z = z;
        }

        public static FixVector FromRawValues(int x, int y, int z)
        {
            return new FixVector(new Fix(x), new Fix(y), new Fix(z));
        }

        public static explicit operator Vector3(FixVector v)
            => new Vector3(v.x, v.y, v.z);
        public static implicit operator FixVector(Vector3 v)
            => new FixVector(v.X, v.Y, v.Z);

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", x, y, z);
        }

        public static FixVector operator +(FixVector a)
            => a.Scale(1);
        public static FixVector operator -(FixVector a)
            => a.Scale(-1);
        public static FixVector operator +(FixVector a, FixVector b)
            => a.Add(b);
        public static FixVector operator -(FixVector a, FixVector b)
            => a.Sub(b);
        public static FixVector operator *(double d, FixVector v)
            => v.Scale(d);
        public static FixVector operator *(FixVector v, double d)
            => v.Scale(d);
        public static bool operator ==(FixVector a, FixVector b)
            => a.Equals(b);
        public static bool operator !=(FixVector a, FixVector b)
            => !(a == b);

        public FixVector Add(FixVector other)
        {
            return new FixVector(this.x + other.x, this.y + other.y, this.z + other.z);
        }

        public FixVector Sub(FixVector other)
        {
            return new FixVector(this.x - other.x, this.y - other.y, this.z - other.z);
        }

        public FixVector Scale(double scale)
        {
            return new FixVector(this.x * scale, this.y * scale, this.z * scale);
        }

        public double Dot(FixVector other)
        {
            // optimized to avoid intermediate fix conversions
            // must divide by 2^16 at the end since we were dealing with fixes
            return ((((long)this.x.value * other.x.value) >> 16)
                  + (((long)this.y.value * other.y.value) >> 16)
                  + (((long)this.z.value * other.z.value) >> 16)) * (1.0 / (1 << 16));
        }

        public FixVector Cross(FixVector other)
        {
            return new FixVector(
                this.y * other.z - this.z * other.y,
                this.z * other.x - this.x * other.z,
                this.x * other.y - this.y * other.x
            );
        }

        public double Mag()
        {
            return Math.Sqrt((double)this.x * (double)this.x + (double)this.y * (double)this.y + (double)this.z * (double)this.z);
        }

        public FixVector Normalize()
        {
            return Scale(1.0/Mag());
        }

        public override bool Equals(object obj)
        {
            return obj is FixVector vector &&
                (x == vector.x) && (y == vector.y) && (z == vector.z);
        }

        public override int GetHashCode()
        {
            var hashCode = 373119288;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            return hashCode;
        }
    }

    public struct FixMatrix
    {
        // Field order matches Descent structure (and read/write) order

        public FixVector right;
        public FixVector up;
        public FixVector forward;

        public FixMatrix(FixVector right, FixVector up, FixVector forward)
        {
            this.right = right; this.up = up; this.forward = forward;
        }

        public FixMatrix(FixAngles angs)
        {
            Fix sinp = Math.Sin((angs.p / 16384.0) * (Math.PI * 2));
            Fix cosp = Math.Cos((angs.p / 16384.0) * (Math.PI * 2));
            Fix sinb = Math.Sin((angs.b / 16384.0) * (Math.PI * 2));
            Fix cosb = Math.Cos((angs.b / 16384.0) * (Math.PI * 2));
            Fix sinh = Math.Sin((angs.h / 16384.0) * (Math.PI * 2));
            Fix cosh = Math.Cos((angs.h / 16384.0) * (Math.PI * 2));

            Fix sbsh = sinb * sinh;
            Fix cbch = cosb * cosh;
            Fix cbsh = cosb * sinh;
            Fix sbch = sinb * cosh;

            right.x = cbch + (sinp * sbsh);
            up.z = sbsh + (sinp * cbch);

            up.x = (sinp * cbsh) - sbch;
            right.z = (sinp * sbch) - cbsh;

            forward.x = sinh * cosp;
            right.y = sinb * cosp;
            up.y = cosb * cosp;
            forward.z = cosh * cosp;

            forward.y = -sinp;
        }

        public static FixMatrix operator +(FixMatrix a)
            => a.Scale(1);
        public static FixMatrix operator -(FixMatrix a)
            => a.Scale(-1);
        public static FixMatrix operator +(FixMatrix a, FixMatrix b)
            => a.Add(b);
        public static FixMatrix operator -(FixMatrix a, FixMatrix b)
            => a.Sub(b);
        public static FixMatrix operator *(double d, FixMatrix v)
            => v.Scale(d);
        public static FixMatrix operator *(FixMatrix v, double d)
            => v.Scale(d);
        public static FixMatrix operator *(FixMatrix v, FixMatrix d)
            => v.Mul(d);
        public static bool operator ==(FixMatrix a, FixMatrix b)
            => a.Equals(b);
        public static bool operator !=(FixMatrix a, FixMatrix b)
            => !(a == b);

        public FixMatrix Add(FixMatrix other)
        {
            return new FixMatrix(this.right + other.right, this.up + other.up, this.forward + other.forward);
        }
        public FixMatrix Sub(FixMatrix other)
        {
            return new FixMatrix(this.right - other.right, this.up - other.up, this.forward - other.forward);
        }
        public FixMatrix Scale(double scale)
        {
            return new FixMatrix(this.right * scale, this.up * scale, this.forward * scale);
        }
        public FixMatrix Mul(FixMatrix other)
        {
            return new FixMatrix(
                right: new FixVector(
                    this.forward.z * other.forward.x + this.up.z * other.forward.y + this.right.z * other.forward.z,
                    this.forward.z * other.up.x + this.up.z * other.up.y + this.right.z * other.up.z,
                    this.forward.z * other.right.x + this.up.z * other.right.y + this.right.z * other.right.z),
                up: new FixVector(
                    this.forward.y * other.forward.x + this.up.y * other.forward.y + this.right.y * other.forward.z,
                    this.forward.y * other.up.x + this.up.y * other.up.y + this.right.y * other.up.z,
                    this.forward.y * other.right.x + this.up.y * other.right.y + this.right.y * other.right.z),
                forward: new FixVector(
                    this.forward.x * other.forward.x + this.up.x * other.forward.y + this.right.x * other.forward.z,
                    this.forward.x * other.up.x + this.up.x * other.up.y + this.right.x * other.up.z,
                    this.forward.x * other.right.x + this.up.x * other.right.y + this.right.x * other.right.z)
            );
        }
        public FixMatrix Transpose()
        {
            return new FixMatrix(
                right: new FixVector(
                    this.right.x,
                    this.up.x,
                    this.forward.x),
                up: new FixVector(
                    this.right.y,
                    this.up.y,
                    this.forward.y),
                forward: new FixVector(
                    this.right.z,
                    this.up.z,
                    this.forward.z)
                );

        }

        public override string ToString()
        {
            return string.Format("r:({0}), u:({1}), f:({2})", right, up, forward);
        }

        public override bool Equals(object obj)
        {
            return obj is FixMatrix matrix &&
                 (right == matrix.right) && (up == matrix.up) && (forward == matrix.forward);
        }

        public override int GetHashCode()
        {
            var hashCode = 1938967743;
            hashCode = hashCode * -1521134295 + right.GetHashCode();
            hashCode = hashCode * -1521134295 + up.GetHashCode();
            hashCode = hashCode * -1521134295 + forward.GetHashCode();
            return hashCode;
        }
    }
}
