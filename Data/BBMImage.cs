using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDescent.Data
{
    public class BBMImage
    {
        public short Width { get; private set; }
        public short Height { get; private set; }
        public BBMType Type { get; private set; }
        public byte NumPlanes { get; private set; }
        public byte Mask { get; set; }
        public byte Compression { get; private set; }
        public short TransparentColor { get; set; }
        public Color[] Palette { get; }
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
                throw new ArgumentException("only supported Mask value is 0 or 2");
            if (Compression != 0 && Compression != 1)
                throw new ArgumentException("only supported Compression value is 0 or 1");

            for (int i = 0; i < Palette.Length; ++i)
                Palette[i] = new Color(Mask != 0 && i == TransparentColor ? 0 : 255, Palette[i].R, Palette[i].G, Palette[i].B);
        }

        private void ReadCMAP(BinaryReaderBE br, uint length)
        {
            int nColors = (int)(length / 3);
            for (int i = 0; i < nColors; ++i)
                Palette[i] = new Color(Palette[i].A, br.ReadByte(), br.ReadByte(), br.ReadByte());
        }

        private void ConvertILBMToPBM()
        {
            byte[] newData = new byte[Width * Height];
            int dst = 0;
            int rowSz = ((Width + 15) >> 3) & ~1;
            int row, rowoff;
            byte checkmask, newbyte, setbit;

            for (int y = 0; y < Height; ++y)
            {
                row = y * rowSz * NumPlanes;
                checkmask = 0x80;
                for (int x = 0; x < Width; ++x)
                {
                    rowoff = x >> 3;
                    newbyte = 0;
                    setbit = 1;

                    for (int p = 0; p < NumPlanes; ++p)
                    {
                        if ((Data[row + rowSz * p + rowoff] & checkmask) != 0)
                            newbyte |= setbit;
                        setbit <<= 1;
                    }

                    newData[dst++] = newbyte;
                    if ((checkmask >>= 1) == 0)
                        checkmask = 0x80;
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

            byte[] rawData = br.ReadBytes((int)length);
            if (rawData.Length < length)
                throw new EndOfStreamException();

            // read offset, write offset
            int i = 0, j = 0;

            switch (Compression)
            {
                case 0: // none
                    {
                        for (int y = 0; y < Height; ++y)
                        {
                            for (int n = 0; n < stride * depth; ++n)
                                Data[j++] = rawData[i++];
                            if ((Mask & 1) != 0)
                                i += stride;
                            i += (stride & 1); // pad
                        }
                    }
                    break;
                case 1: // RLE
                    {
                        int n, nn, end_cnt = -(stride & 1);
                        byte tmp;
                        for (int wid_cnt = stride, plane = 0;
                            i < rawData.Length && j < Data.Length; )
                        {
                            if (wid_cnt == end_cnt)
                            {
                                wid_cnt = stride;
                                ++plane;
                                if (((Mask & 1) != 0 && plane == depth + 1)
                                        || ((Mask & 1) == 0 && plane == depth))
                                    plane = 0;
                            }
                            n = rawData[i++];
                            if (n < 128)
                            {
                                nn = n + 1;
                                wid_cnt -= nn;
                                if (wid_cnt < 0)
                                    --nn;

                                if (plane != depth)
                                {
                                    for (int k = 0; k < nn; ++k)
                                        Data[j++] = rawData[i++];
                                }
                                else
                                    j += nn;

                                if (wid_cnt < 0)
                                    ++i; // pad
                            }
                            else
                            {
                                tmp = rawData[i++];
                                nn = 257 - n;
                                wid_cnt -= nn;
                                if (wid_cnt < 0)
                                    --nn;

                                if (plane != depth)
                                {
                                    for (int k = 0; k < nn; ++k)
                                        Data[j++] = tmp;
                                }
                            }
                        }
                    }
                    break;
            }

            if (Type == BBMType.ILBM)
                ConvertILBMToPBM();
        }

        /// <summary>
        /// Loads a PCX image from a stream.
        /// </summary>
        /// <param name="fs">The stream to load from.</param>
        /// <returns></returns>
        public void Read(Stream fs)
        {
            using (BinaryReaderBE br = new BinaryReaderBE(fs))
            {
                fs.Seek(0, SeekOrigin.Begin);
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
                            catch (IndexOutOfRangeException ex)
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
    }

    public enum BBMType
    {
        PBM, ILBM
    }
}
