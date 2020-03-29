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
        public byte[,] palette = new byte[256, 3];

        public Palette()
        {
            for (int x = 0; x < 3; x++)
            {
                for (int c = 0; c < 255; c++)
                {
                    palette[c, x] = (byte)c;
                }
            }
        }

        public Palette(byte[] data, bool rescale = true)
        {
            for (int c = 0; c < 255; c++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (rescale)
                        palette[c, x] = (byte)(data[c * 3 + x] * 255 / 63);
                    else
                        palette[c, x] = data[c * 3 + x];
                }
            }
        }

        public byte[] GetLinear()
        {
            byte[] linearPal = new byte[768];

            int offset = 0;
            for (int x = 0; x < 256; x++)
            {
                for (int c = 0; c < 3; c++)
                {
                    linearPal[offset++] = palette[x, c];
                }
            }

            return linearPal;
        }

        /*public void CreateDrawingPalette(ColorPalette colorPalette)
        {
            //.NET's System.Drawing library seems like one of the most poorly thought out libraries of all time tbh
            for (int i = 0; i < 256; i++)
            {
                Color color = Color.FromArgb(palette[i, 0], palette[i, 1], palette[i, 2]);
                colorPalette.Entries[i] = color;
            }
        }*/

        public int GetDrawingColorH(int id)
        {
            int a = id == 255 ? 0 : 255;
            return ((a << 24) + (palette[id, 0] << 16) + (palette[id, 1] << 8) + (palette[id, 2]));
        }

        public static Palette defaultPalette = new Palette();
    }
}
