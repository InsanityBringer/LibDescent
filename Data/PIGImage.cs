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
        private const byte animMask = 1 | 2 | 4 | 8 | 16;

        public const int BM_FLAG_TRANSPARENT = 1;
        public const int BM_FLAG_SUPER_TRANSPARENT = 2;
        public const int BM_FLAG_NO_LIGHTING = 4;
        public const int BM_FLAG_RLE = 8;
        public const int BM_FLAG_PAGED_OUT = 16; //This is unneeded in definitions, and isn't exposed in a property. 
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
        /// <summary>
        /// Used for POG files, which base PIG bitmap this replaces.
        /// </summary>
        public ushort ReplacementNum { get; set; }

        //Flag properties
        /// <summary>
        /// Gets or sets whether or not the bitmap should be drawn with palette index 255 transparent.
        /// </summary>
        public bool Transparent
        {
            get
            {
                return (flags & BM_FLAG_TRANSPARENT) != 0;
            }
            set
            {
                if (value)
                    flags |= BM_FLAG_TRANSPARENT;
                else
                    flags = (byte)(flags & ~BM_FLAG_TRANSPARENT);
            }
        }
        /// <summary>
        /// Gets or sets whether or not the bitmap should show through a base texture with pixels with palette index 254 when used as a secondary texture in a level.
        /// </summary>
        public bool SuperTransparent
        {
            get
            {
                return (flags & BM_FLAG_SUPER_TRANSPARENT) != 0;
            }
            set
            {
                if (value)
                    flags |= BM_FLAG_SUPER_TRANSPARENT;
                else
                    flags = (byte)(flags & ~BM_FLAG_SUPER_TRANSPARENT);
            }
        }

        /// <summary>
        /// Gets or sets whether or not the bitmap should be drawn without lighting from the world.
        /// </summary>
        public bool NoLighting
        {
            get
            {
                return (flags & BM_FLAG_NO_LIGHTING) != 0;
            }
            set
            {
                if (value)
                    flags |= BM_FLAG_NO_LIGHTING;
                else
                    flags = (byte)(flags & ~BM_FLAG_NO_LIGHTING);
            }
        }
        /// <summary>
        /// Gets or sets whether or not the data is compressed.
        /// </summary>
        public bool RLECompressed
        {
            get
            {
                return (flags & BM_FLAG_RLE) != 0;
            }
            set
            {
                if (value) //TODO: This should either strip or remove compression as set. 
                    flags |= BM_FLAG_RLE;
                else
                    flags = (byte)(flags & ~BM_FLAG_RLE);
            }
        }
        /// <summary>
        /// Gets whether or not the data is compressed and the image is wider than 255 pixels.
        /// </summary>
        public bool RLECompressedBig 
        {
            get
            {
                return (flags & BM_FLAG_RLE_BIG) != 0;
            }
            private set //Not exposed as it should only be managed by the internal compression code to avoid issues. 
            {
                if (value) 
                    flags |= BM_FLAG_RLE_BIG;
                else
                    flags = (byte)(flags & ~BM_FLAG_RLE_BIG);
            }
        }

        public byte flags;
        public byte averageIndex;
        public int offset;
        public int frame;
        public Palette paletteData;
        public byte extension;
        public bool isAnimated;

        /// <summary>
        /// Creates a new PIG image that can be up to 1024x1024 in size. Used by Descent 2 PIG and POG files.
        /// </summary>
        /// <param name="baseWidth">Base width of the image in the range 0-255</param>
        /// <param name="baseHeight">Base height of the image in the range 0-255</param>
        /// <param name="dFlags">Animation and extra data for the image. Bit 6 specifies an animated image, bits 0-4 are used as the frame number.</param>
        /// <param name="flags">Flags for the image.</param>
        /// <param name="averageIndex">Index of the image's average color in the palette.</param>
        /// <param name="dataOffset">Offset to the data in the source file.</param>
        /// <param name="name">Filename of the image.</param>
        /// <param name="sizeExtra">Extra data to append to the base width and height. First four bits are appended to the width, last four are appended to the height.</param>
        public PIGImage(int baseWidth, int baseHeight, byte dFlags, byte flags, byte averageIndex, int dataOffset, string name, byte sizeExtra)
        {
            this.baseWidth = baseWidth; this.baseHeight = baseHeight; this.flags = flags; this.averageIndex = averageIndex; frameData = dFlags; offset = dataOffset; this.extension = sizeExtra;
            width = this.baseWidth + (((int)sizeExtra & 0x0f) << 8); height = this.baseHeight + (((int)sizeExtra & 0xf0) << 4);
            this.name = name;
            frame = ((int)frameData & (int)animMask);
            isAnimated = ((frameData & 64) != 0);
        }

        //Descent 1 version
        //This code seriously needs a cleanup
        /// <summary>
        /// Creates a new PIG image that can be up to 511x255 in size. Used by Descent 1 PIG files.
        /// </summary>
        /// <param name="baseWidth">Base width of the image in the range 0-255</param>
        /// <param name="baseHeight">Base height of the image in the range 0-255</param>
        /// <param name="dFlags">Animation and extra data for the image. Bit 6 specifies an animated image, bits 0-4 are used as the frame number. Bit 7 adds 256 to the image's width.</param>
        /// <param name="flags">Flags for the image.</param>
        /// <param name="averageIndex">Index of the image's average color in the palette.</param>
        /// <param name="dataOffset">Offset to the data in the source file.</param>
        /// <param name="name">Filename of the image.</param>
        public PIGImage(int baseWidth, int baseHeight, byte dFlags, byte flags, byte averageIndex, int dataOffset, string name)
        {
            this.baseWidth = baseWidth; this.baseHeight = baseHeight; this.flags = flags; this.averageIndex = averageIndex; frameData = dFlags; offset = dataOffset; this.extension = 0;
            width = this.baseWidth; height = this.baseHeight;
            if ((frameData & 128) != 0)
                width += 256;
            this.name = name;
            frame = ((int)frameData & (int)animMask);
            isAnimated = ((frameData & 64) != 0);
        }

        /// <summary>
        /// Gets the size of the data stored on disk.
        /// </summary>
        /// <returns>The size of the data stored on disk, in bytes.</returns>
        public int GetSize()
        {
            if ((flags & BM_FLAG_RLE) != 0)
            {
                return data.Length + 4;
            }
            return width * height;
        }

        /// <summary>
        /// Decompresses the image if needed and returns the raw bitmap data.
        /// </summary>
        /// <returns>A byte array containing the raw bitmap data.</returns>
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
            //Return a copy rather than the original data, like with compressed images. 
            byte[] buffer = new byte[data.Length];
            Array.Copy(data, buffer, data.Length);
            return buffer;
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
