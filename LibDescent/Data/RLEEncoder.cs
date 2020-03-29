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
    public class RLEEncoder
    {
        /// <summary>
        /// Decodes an RLE scanline.
        /// </summary>
        /// <param name="input">Array of the conpressed data.</param>
        /// <param name="output">Array to store the decompressed pixels in.</param>
        /// <param name="offset">Offset into the input for the scanline's data.</param>
        /// <param name="width">Width of the scanline.</param>
        public static void DecodeScanline(byte[] input, byte[] output, int offset, int width)
        {
            byte curdata = 0;
            int position = offset;
            byte color = 0;
            int count; 
            int linelocation = 0;
            while (linelocation < width)
            {
                curdata = input[position++];
                if (curdata == 0xE0)
                    continue;

                if (curdata > 0xE0)
                {
                    count = (byte)(curdata & 0x1F);
                    color = input[position++];
                    for (int temp = 0; temp < count; temp++)
                    {
                        output[linelocation++] = color;
                        if (linelocation >= width) break; //Looks like we're done early?
                    }
                }
                else
                {
                    output[linelocation++] = curdata;
                }
            }
        }
    }
}
