using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDescent.Data
{
    public class BBMImage
    {
        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public short Width { get; private set; }
        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public short Height { get; private set; }
        /// <summary>
        /// The type of this BBM image.
        /// </summary>
        public BBMType Type { get; private set; }
        /// <summary>
        /// Number of bitplanes. Always 8 for Descent images.
        /// </summary>
        public byte NumPlanes { get; private set; }
        /// <summary>
        /// Mask information. 0 for no mask, 1 for mask plane or 2 for transparent color (mask color):
        /// </summary>
        public byte Mask { get; set; }
        /// <summary>
        /// The compression used. 0 for no compression and 1 for RLE.
        /// </summary>
        public byte Compression { get; private set; }
        /// <summary>
        /// The transparent color. Applies only if Mask = 2.
        /// </summary>
        public short TransparentColor { get; set; }
        /// <summary>
        /// The palette used in ths image.
        /// </summary>
        public Color[] Palette { get; }
        /// <summary>
        /// The decoded pixel data in this image, with one byte per pixel.
        /// </summary>
        public byte[] Data { get; private set; }

        public BBMImage() : this(0, 0) { }

        public BBMImage(short width, short height)
        {
            Width = width;
            Height = height;
            Data = new byte[width * height];
            Type = BBMType.PBM;
            NumPlanes = 8;
            Mask = 2;
            Compression = 0;
            TransparentColor = 255;
            Palette = new Color[1 << NumPlanes];
            for (int i = 0; i < Palette.Length; ++i)
                Palette[i] = new Color(i == TransparentColor ? 0 : 255, i, i, i);
        }

        private void ReadBMHD(BinaryReaderBE br)
        {
            Width = br.ReadInt16();
            Height = br.ReadInt16();
            int originData = br.ReadInt32();
            NumPlanes = br.ReadByte();
            Mask = br.ReadByte();
            Compression = br.ReadByte();
            /* pad = */ br.ReadByte();
            TransparentColor = br.ReadInt16();
            short aspectRatio = br.ReadInt16();
            short pageWidth = br.ReadInt16();
            short pageHeight = br.ReadInt16();

            if (NumPlanes != 8)
                throw new ArgumentException("only supported NumPlanes value is 8");
            if (Mask != 0 && Mask != 2)
                throw new ArgumentException("only supported Mask values are 0 and 2");
            if (Compression != 0 && Compression != 1)
                throw new ArgumentException("only supported Compression values are 0 and 1");

            for (int i = 0; i < Palette.Length; ++i)
                Palette[i] = new Color(Mask == 2 && i == TransparentColor ? 0 : 255, Palette[i].R, Palette[i].G, Palette[i].B);
        }

        private void ReadCMAP(BinaryReaderBE br, uint length)
        {
            int nColors = (int)(length / 3);
            for (int i = 0; i < nColors; ++i)
                Palette[i] = new Color(Palette[i].A, br.ReadByte(), br.ReadByte(), br.ReadByte());
        }

        /// <summary>
        /// Gets the image data as RGB (B8G8R8).
        /// </summary>
        /// <returns>The image data as 24-bit RGB.</returns>
        public byte[] GetRGBData()
        {
            byte[] result = new byte[Data.Length * 3];
            Color clr;
            int p = 0;
            for (int i = 0; i < Data.Length; ++i)
            {
                clr = Palette[Data[i]];
                result[p++] = (byte)clr.B;
                result[p++] = (byte)clr.G;
                result[p++] = (byte)clr.R;
            }
            return result;
        }

        private void ConvertILBMToPBM()
        {
            byte[] newData = new byte[Width * Height];
            int dst = 0;
            int rowSz = ((Width + 15) >> 3) & ~1;
            int row, rowOff;
            byte mask, planar;

            for (int y = 0; y < Height; ++y)
            {
                row = y * rowSz * NumPlanes;
                mask = 0x80;
                for (int x = 0; x < Width; ++x)
                {
                    rowOff = x >> 3;
                    planar = 0;

                    for (int p = 0; p < NumPlanes; ++p)
                    {
                        planar >>= 1;
                        if ((Data[row + rowSz * p + rowOff] & mask) != 0)
                            planar |= 0x80;
                    }

                    newData[dst++] = planar;
                    if ((mask >>= 1) == 0)
                        mask = 0x80;
                }
            }

            Data = newData;
        }

        private void ReadBODY(BinaryReaderBE br, uint length)
        {
            int stride, depth;
            switch (Type)
            {
                case BBMType.PBM:
                    stride = Width;
                    depth = 1;
                    break;
                case BBMType.ILBM:
                    stride = (Width + 7) / 8;
                    depth = NumPlanes;
                    break;
                default:
                    return;
            }

            byte[] inData = br.ReadBytes((int)length);
            if (inData.Length < length)
                throw new EndOfStreamException();
            Data = new byte[Width * Height];

            // read offset, write offset
            int i = 0, j = 0;

            switch (Compression)
            {
                case 0: // none
                    {
                        for (int y = 0; y < Height; ++y)
                        {
                            for (int n = 0; n < stride * depth; ++n)
                                Data[j++] = inData[i++];
                            if ((Mask & 1) != 0)
                                i += stride;
                            i += (stride & 1); // pad
                        }
                    }
                    break;
                case 1: // RLE
                    {
                        int rowPixelsEnd = -(stride & 1), runLength;
                        byte rleByte, runByte;
                        for (int rowPixels = stride, plane = 0; i < inData.Length && j < Data.Length; )
                        {
                            if (rowPixels == rowPixelsEnd)
                            {
                                rowPixels = stride;
                                ++plane;
                                if (((Mask & 1) != 0 && plane == depth + 1)
                                        || ((Mask & 1) == 0 && plane == depth))
                                    plane = 0;
                            }

                            rleByte = inData[i++];
                            if (rleByte < 128)      // N+1 (1-128) uncompressed bytes
                            {
                                runLength = rleByte + 1;
                                rowPixels -= runLength;
                                if (rowPixels < 0)
                                    --runLength;

                                if (plane != depth)
                                    for (int k = 0; k < runLength; ++k)
                                        Data[j++] = inData[i++];
                                else
                                    i += runLength;

                                if (rowPixels < 0)
                                    ++i; // pad
                            }
                            else if (rleByte == 128)
                                break;
                            else                    // 257-N (2-128) repeating bytes (run)
                            {
                                runLength = 257 - rleByte;
                                rowPixels -= runLength;
                                if (rowPixels < 0)
                                    --runLength;

                                runByte = inData[i++];

                                if (plane != depth)
                                    for (int k = 0; k < runLength; ++k)
                                        Data[j++] = runByte;
                            }
                        }
                    }
                    break;
            }

            if (Type == BBMType.ILBM)
                ConvertILBMToPBM();
        }

        /// <summary>
        /// Loads a BBM image from a stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns></returns>
        public void Read(Stream stream)
        {
            using (BinaryReaderBE br = new BinaryReaderBE(stream))
            {
                stream.Seek(0, SeekOrigin.Begin);
                if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "FORM")
                    throw new ArgumentException("Not a valid .BBM");
                int dataSize = br.ReadInt32();

                string formatID = Encoding.ASCII.GetString(br.ReadBytes(4));
                if (formatID == "PBM ")
                    Type = BBMType.PBM;
                else if (formatID == "ILBM")
                    Type = BBMType.ILBM;
                else
                    throw new ArgumentException("Unsupported .BBM format");

                if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "BMHD")
                    throw new ArgumentException("Not a valid .BBM");
                int headerSize = br.ReadInt32();
                ReadBMHD(br);

                string chunkID;
                uint lenChunk;
                while (true)
                {
                    chunkID = Encoding.ASCII.GetString(br.ReadBytes(4));
                    if (chunkID.Length < 4) break;
                    lenChunk = br.ReadUInt32();

                    switch (chunkID)
                    {
                        case "CMAP": // palette
                            ReadCMAP(br, lenChunk);
                            break;
                        case "BODY": // image data
                            try
                            {
                                ReadBODY(br, lenChunk);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                throw new EndOfStreamException();
                            }
                            break;
                        case "TINY": // thumbnail
                        case "GRAB": // cursor info
                        case "CRNG": // color range
                        default:
                            br.BaseStream.Seek(lenChunk, SeekOrigin.Current);
                            break;
                    }

                    if ((lenChunk & 1) != 0)
                        br.ReadByte(); // skip one pad byte
                }
            }
        }

        /// <summary>
        /// Loads a BBM image from a file.
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
        /// Loads a BBM image from an array.
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

        /// <summary>
        /// Writes this BBM image into a stream.
        /// </summary>
        /// <param name="stream">The stream to write into.</param>
        public void Write(Stream stream)
        {
            BinaryWriterBE bw = new BinaryWriterBE(stream);
            bw.Write(Encoding.ASCII.GetBytes("FORM"));
            long sizePos = bw.BaseStream.Position;
            bw.Write((int)0);
            bw.Write(Encoding.ASCII.GetBytes("PBM "));

            bw.Write(Encoding.ASCII.GetBytes("BMHD"));
            bw.Write(20); // BMHD length
            bw.Write(Width);
            bw.Write(Height);
            bw.Write(0); // origin (0, 0)
            bw.Write(NumPlanes);
            bw.Write(Mask);
            bw.Write(Compression);
            bw.Write((byte)0); // pad
            bw.Write(TransparentColor);
            bw.Write((short)0x0506); // aspect ratio
            bw.Write((short)320); // page width
            bw.Write((short)200); // page height

            bw.Write(Encoding.ASCII.GetBytes("CMAP"));
            bw.Write(768);
            for (int i = 0; i < Palette.Length; ++i)
            {
                bw.Write((byte)Palette[i].R);
                bw.Write((byte)Palette[i].G);
                bw.Write((byte)Palette[i].B);
            }

            bw.Write(Encoding.ASCII.GetBytes("BODY"));
            bw.Write(((Width + 1) & ~1) * Height);
            int p = 0;
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                    bw.Write(Data[p++]);
                if ((Width & 1) != 0)
                    bw.Write((byte)0);
            }

            long size = bw.BaseStream.Position;
            bw.BaseStream.Position = sizePos;
            bw.Write((int)size - 8);
        }

        /// <summary>
        /// Writes this BBM image into a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        public void Write(string filePath)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                Write(fs);
            }
        }

        /// <summary>
        /// Writes this BBM image into a byte array.
        /// </summary>
        public byte[] Write()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Write(ms);
                return ms.ToArray();
            }
        }
    }

    public enum BBMType
    {
        /// <summary>
        /// Planar Bitmap. Pixels are stored in sequential bitplanes.
        /// </summary>
        PBM,
        /// <summary>
        /// Interleaved Bitmap. Pixels are stored in interleaved bitplanes.
        /// </summary>
        ILBM
    }
}
