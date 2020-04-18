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
    public class Palette
    {
        private byte[] palette = new byte[768];

        public Palette()
        {
            for (int c = 0; c < 255; c++)
            {
                for (int x = 0; x < 3; x++)
                {
                    palette[c * 3] = (byte)c;
                    palette[c * 3 + 1] = (byte)c;
                    palette[c * 3 + 2] = (byte)c;
                }
            }
        }

        public Palette(byte[] data, bool rescale = true)
        {
            for (int c = 0; c < 768; c++)
            {
                if (rescale)
                    palette[c] = (byte)(data[c] * 255 / 63);
                else
                    palette[c] = data[c];
            }
        }

        public byte this[int index, int channel]
        {
            get
            {
                if (channel < 0 || channel >= 3) channel = 0; //simple validation
                if (index < 0 || index >= 768) index = 0;
                return palette[index * 3 + channel];
            }
        }

        public byte this[int index]
        {
            get
            {
                return palette[index];
            }
        }

        public byte[] GetLinear() //basically a copy function tbh
        {
            byte[] linearPal = new byte[768];

            int offset = 0;
            for (int x = 0; x < 768; x++)
            {
                linearPal[offset++] = palette[x];
            }

            return linearPal;
        }

        public int GetNearestColor(int r, int g, int b)
        {
            int bestcolor = 0;
            int bestdist = int.MaxValue;
            int dist;

            for (int i = 0; i < 255; i++)
            {
                dist = (r - palette[i * 3]) * (r - palette[i * 3]) + (g - palette[i * 3 + 1]) * (g - palette[i * 3 + 1]) + (b - palette[i * 3 + 2]) * (b - palette[i * 3 + 2]);
                if (dist == 0) return i;
                if (dist < bestdist)
                {
                    bestcolor = i;
                    bestdist = dist;
                }
            }
            return bestcolor;
        }

        public int GetRGBAValue(int id)
        {
            int a = id == 255 ? 0 : 255;
            return ((a << 24) + (palette[id * 3] << 16) + (palette[id * 3 + 1] << 8) + (palette[id * 3 + 2]));
        }

        public static Palette defaultPalette = new Palette();
    }
}
