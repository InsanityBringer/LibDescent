using System;
using System.IO;

namespace LibDescent.Data
{
    /// <summary>
    /// Represents a PCX image.
    /// </summary>
    public class PCXImage
    {
        /// <summary>
        /// The Manufacturer number. Only supported value is 0x0A
        /// </summary>
        public byte Manufacturer;
        /// <summary>
        /// The version of PC Paintbrush; only supported version is 5.
        /// </summary>
        public byte Version;
        /// <summary>
        /// The ID of the encoding used. The only supported encoding is 1 (RLE).
        /// </summary>
        public byte Encoding;
        /// <summary>
        /// Number of bits per pixel. Only supported is 8 (256 colors).
        /// </summary>
        public byte BitsPerPixel;
        /// <summary>
        /// The left-most X coordinate of this image.
        /// </summary>
        public short Xmin;
        /// <summary>
        /// The top-most Y coordinate of this image.
        /// </summary>
        public short Ymin;
        /// <summary>
        /// The right-most X coordinate of this image.
        /// </summary>
        public short Xmax;
        /// <summary>
        /// The bottom-most Y coordinate of this image.
        /// </summary>
        public short Ymax;
        /// <summary>
        /// The dots per inch value on the horizontal axis.
        /// </summary>
        public short Hdpi;
        /// <summary>
        /// The dots per inch value on the vertical axis.
        /// </summary>
        public short Vdpi;
        /// <summary>
        /// The first 16 colors.
        /// </summary>
        public Color[] ColorMap;
        /// <summary>
        /// The 256-color palette.
        /// </summary>
        public Color[] Palette;
        /// <summary>
        /// Number of bit-planes. Only 1 supported.
        /// </summary>
        public byte NPlanes;
        /// <summary>
        /// The original image data, compressed with RLE.
        /// </summary>
        public byte[] RawData;
        /// <summary>
        /// The decoded image data, in 24bpp (R8G8B8) format.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Gets the default 256-color palette used for PCX images.
        /// </summary>
        /// <returns>The default palette.</returns>
        public Color[] GetDefaultPalette()
        {
            Color[] palette = new Color[256];
            palette[0] = new Color(255, 0x00, 0x00, 0x00);
            palette[1] = new Color(255, 0x00, 0x00, 0xAA);
            palette[2] = new Color(255, 0x00, 0xAA, 0x00);
            palette[3] = new Color(255, 0x00, 0xAA, 0xAA);
            palette[4] = new Color(255, 0xAA, 0x00, 0x00);
            palette[5] = new Color(255, 0xAA, 0x00, 0xAA);
            palette[6] = new Color(255, 0xAA, 0x55, 0x00);
            palette[7] = new Color(255, 0xAA, 0xAA, 0xAA);
            palette[8] = new Color(255, 0x55, 0x55, 0x55);
            palette[9] = new Color(255, 0x55, 0x55, 0xFF);
            palette[10] = new Color(255, 0x55, 0xFF, 0x55);
            palette[11] = new Color(255, 0x55, 0xFF, 0xFF);
            palette[12] = new Color(255, 0xFF, 0x55, 0x55);
            palette[13] = new Color(255, 0xFF, 0x55, 0xFF);
            palette[14] = new Color(255, 0xFF, 0xFF, 0x55);
            palette[15] = new Color(255, 0xFF, 0xFF, 0xFF);
            for (int i = 16; i < 256; ++i)
                palette[i] = palette[0];
            return palette;
        }

        public static Color ClosestColor(Color[] palette, Color color)
        {
            int minDist = Int32.MaxValue;
            Color closestColor = new Color();

            foreach (Color c in palette)
            {
                int dist = (c.R - color.R) * (c.R - color.R) + (c.G - color.G) * (c.G - color.G) + (c.B - color.B) * (c.B - color.B);
                if (minDist > dist)
                {
                    minDist = dist;
                    closestColor = c;
                }
            }

            return closestColor;
        }

        /// <summary>
        /// The width of this image in pixels.
        /// </summary>
        public int Width => Xmax - Xmin + 1;

        /// <summary>
        /// The height of this image in pixels.
        /// </summary>
        public int Height => Ymax - Ymin + 1;

        private void ParseHeader(byte[] block)
        {
            Manufacturer = block[0];
            Version = block[1];
            Encoding = block[2];
            BitsPerPixel = block[3];
            Xmin = BitConverter.ToInt16(block, 4);
            Ymin = BitConverter.ToInt16(block, 6);
            Xmax = BitConverter.ToInt16(block, 8);
            Ymax = BitConverter.ToInt16(block, 10);
            Hdpi = BitConverter.ToInt16(block, 12);
            Vdpi = BitConverter.ToInt16(block, 14);
            ColorMap = new Color[16];
            for (int i = 0; i < 16; ++i)
            {
                ColorMap[i] = PCXImage.ReadRGB(block, 16 + 3 * i);
            }
            NPlanes = block[65];
            //BytesPerLine = BitConverter.ToInt16(block, 66);
        }

        /// <summary>
        /// Generates Data from the current contents of RawData, decoding 8bpp into 24bpp (R8G8B8).
        /// </summary>
        public void Decode()
        {
            BinaryReader br = new BinaryReader(new MemoryStream(RawData));
            int pixelsRead = 0;
            int pixelsTotal = Width * Height;
            int stride = Width * 3;
            int bytes = stride * Height;
            byte[] rgbValues = new byte[bytes];

            int x, y, p;
            Color clr;
            while (pixelsRead < pixelsTotal)
            {
                int runLength = 1;
                int runByte = br.ReadByte();
                if ((runByte & 0xC0) == 0xC0)
                {
                    runLength = runByte & 0x3F;
                    runByte = br.ReadByte();
                }
                for (int i = 0; i < runLength && pixelsRead < pixelsTotal; ++i)
                {
                    y = Math.DivRem(pixelsRead, Width, out x);
                    p = y * stride + (x * 3);
                    clr = this.Palette[runByte];
                    rgbValues[p + 0] = (byte)clr.B;
                    rgbValues[p + 1] = (byte)clr.G;
                    rgbValues[p + 2] = (byte)clr.R;
                    ++pixelsRead;
                }
            }

            Data = rgbValues;
            br.Close();
        }

        private static Color ReadRGB(byte[] block, int v)
        {
            return new Color(255, block[v], block[v + 1], block[v + 2]);
        }

        /// <summary>
        /// Loads a PCX image from a stream.
        /// </summary>
        /// <param name="fs">The stream to load from.</param>
        /// <returns></returns>
        public void Read(Stream fs)
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
                Palette = GetDefaultPalette();
                ParseHeader(br.ReadBytes(128));
                if (Manufacturer != 0x0a)
                    throw new ArgumentException("file is not a valid PCX image");
                if (Encoding != 1)
                    throw new ArgumentException("only PCX encoding 1 is supported");
                if (NPlanes != 1)
                    throw new ArgumentException("only PCX with 1 plane is supported");
                if (BitsPerPixel != 8)
                    throw new ArgumentException("only PCX with 8bpp (256 colors) is supported");
                if (Version != 5)
                    throw new ArgumentException("only PCX version 5 is supported");

                // test extended palette
                fs.Seek(-769, SeekOrigin.End);
                long imageDataEnd = fs.Position;
                if (br.ReadByte() == 0x0C)
                {
                    // has ext palette
                    byte[] pal = br.ReadBytes(768);
                    for (int i = 0; i < 255; ++i)
                    {
                        Palette[i] = ReadRGB(pal, i * 3);
                    }
                }

                // read image data
                fs.Seek(128, SeekOrigin.Begin);
                int imageDataLength = (int)(imageDataEnd - 128);
                RawData = br.ReadBytes(imageDataLength);
                Decode();
            }
        }

        /// <summary>
        /// Loads a PCX image from a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="palette">The palette of the decoded image. Defined only if return value is zero.</param>
        /// <returns></returns>
        public void Read(string filePath)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open))
            {
                Read(fs);
            }
        }

        /// <summary>
        /// Loads a PCX image from an array.
        /// </summary>
        /// <param name="contents">The array to load from.</param>
        /// <param name="palette">The palette of the decoded image. Defined only if return value is zero.</param>
        /// <returns></returns>
        public void Read(byte[] contents)
        {
            using (MemoryStream ms = new MemoryStream(contents))
            {
                Read(ms);
            }
        }
    }
}
