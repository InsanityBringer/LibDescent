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
using System.IO;

namespace LibDescent.Data
{
    public class PIGImage
    {
        private const byte animdata = 1 | 2 | 4 | 8 | 16;

        public const int BM_FLAG_TRANSPARENT = 1;
        public const int BM_FLAG_SUPER_TRANSPARENT = 2;
        public const int BM_FLAG_NO_LIGHTING = 4;
        public const int BM_FLAG_RLE = 8;
        public const int BM_FLAG_PAGED_OUT = 16;
        public const int BM_FLAG_RLE_BIG = 32;

        //Metaflags, not for use in the game data, but needed for managing data
        public const int BM_META_LOADED = 1;
        /// <summary>
        /// Name of the image in the Piggy archive
        /// </summary>
        public string name;
        /// <summary>
        /// Animation flags for the image
        /// </summary>
        public byte frameData;
        /// <summary>
        /// Base width of the image, before the extra bits are added
        /// </summary>
        public int baseWidth;
        /// <summary>
        /// Base height of the image, before the extra bits are added
        /// </summary>
        public int baseHeight;
        /// <summary>
        /// Final width of the image.
        /// </summary>
        public int width;
        /// <summary>
        /// Final height of the image.
        /// </summary>
        public int height; 
        /// <summary>
        /// Raw image data.
        /// </summary>
        public byte[] data;

        public byte flags;
        public byte averageIndex;
        public int offset;
        public int frame;
        public Palette paletteData;
        public byte extension;
        public bool isAnimated;
        public PIGImage(int mx, int my, byte framed, byte flag, byte average, int dataOffset, string imagename, byte extension)
        {
            baseWidth = mx; baseHeight = my; flags = flag; averageIndex = average; frameData = framed; offset = dataOffset; this.extension = extension;
            width = baseWidth + (((int)extension & 0x0f) << 8); height = baseHeight + (((int)extension & 0xf0) << 4);
            name = imagename;
            frame = ((int)frameData & (int)animdata);
            isAnimated = ((frameData & 64) != 0);
        }

        //Descent 1 version
        //This code seriously needs a cleanup
        public PIGImage(int mx, int my, byte framed, byte flag, byte average, int dataOffset, string imagename)
        {
            baseWidth = mx; baseHeight = my; flags = flag; averageIndex = average; frameData = framed; offset = dataOffset; this.extension = 0;
            width = baseWidth; height = baseHeight;
            if ((frameData & 128) != 0)
                width += 256;
            name = imagename;
            frame = ((int)frameData & (int)animdata);
            isAnimated = ((frameData & 64) != 0);
        }

        public int GetSize()
        {
            if ((flags & BM_FLAG_RLE) != 0)
            {
                return data.Length + 4;
            }
            return width * height;
        }

        public byte[] GetData()
        {
            if ((flags & BM_FLAG_RLE) != 0)
            {
                byte[] expand = new byte[width * height];
                byte[] scanline = new byte[width];

                for (int cury = 0; cury < height; cury++)
                {
                    if ((flags & BM_FLAG_RLE_BIG) != 0)
                    {
                        offset = height * 2;
                        for (int i = 0; i < cury; i++)
                        {
                            offset += data[i * 2] + (data[i * 2 + 1] << 8);
                        }
                    }
                    else
                    {
                        offset = height;
                        for (int i = 0; i < cury; i++)
                        {
                            offset += data[i];
                        }
                    }
                    RLEEncoder.DecodeScanline(data, scanline, offset, width);
                    Array.Copy(scanline, 0, expand, cury * width, width);
                }

                return expand;
            }
            return data;
        }

        public void WriteImage(BinaryWriter bw)
        {
            if ((flags & BM_FLAG_RLE) != 0)
            {
                bw.Write(data.Length+4); //okay maybe this was a bad idea...
                bw.Write(data);
            }
            else
            {
                bw.Write(data);
            }
        }

        public void WriteImageHeader(BinaryWriter bw)
        {
            for (int sx = 0; sx < 8; sx++)
            {
                if (sx < name.Length)
                {
                    bw.Write((byte)name[sx]);
                }
                else
                {
                    bw.Write((byte)0);
                }
            }
            bw.Write(frameData);
            bw.Write((byte)baseWidth);
            bw.Write((byte)baseHeight);
            bw.Write(extension);
            bw.Write(flags);
            bw.Write(averageIndex);
            bw.Write(offset);
        }
    }
}
