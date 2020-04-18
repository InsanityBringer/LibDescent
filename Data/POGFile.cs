/*
    Copyright (c) 2019 The LibDescent team

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
using System.Text;
using System.IO;

namespace LibDescent.Data
{
    public class POGFile : IDataFile
    {
        private int header;
        private int version;
        private int startptr;

        public List<PIGImage> Bitmaps { get; } = new List<PIGImage>();
        public void Read(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            header = br.ReadInt32();
            version = br.ReadInt32();

            if (header != 1196380228)
            {
                br.Dispose();
                throw new InvalidDataException("POGFile::Read: POG file has bad header.");
            }
            if (version != 1)
            {
                br.Dispose();
                throw new InvalidDataException(string.Format("POGFile::Read: POG file has bad version. Got {0}, but expected 1", version));
            }

            int textureCount = br.ReadInt32();
            ushort[] replacements = new ushort[textureCount];
            for (int i = 0; i < textureCount; i++)
            {
                replacements[i] = br.ReadUInt16();
            }

            for (int i = 0; i < textureCount; i++)
            {
                bool hashitnull = false;
                char[] localname = new char[8];
                for (int j = 0; j < 8; j++)
                {
                    char c = (char)br.ReadByte();
                    if (c == 0)
                        hashitnull = true;
                    if (!hashitnull)
                        localname[j] = c;
                }
                string imagename = new String(localname);
                imagename = imagename.Trim(' ', '\0');
                byte framedata = br.ReadByte();
                byte lx = br.ReadByte();
                byte ly = br.ReadByte();
                byte extension = br.ReadByte();
                byte flag = br.ReadByte();
                byte average = br.ReadByte();
                int offset = br.ReadInt32();

                PIGImage image = new PIGImage(lx, ly, framedata, flag, average, offset, imagename, extension);
                image.ReplacementNum = replacements[i];
                Bitmaps.Add(image);
            }
            startptr = (int)br.BaseStream.Position;

            for (int i = 1; i < Bitmaps.Count; i++)
            {
                br.BaseStream.Seek(startptr + Bitmaps[i].Offset, SeekOrigin.Begin);
                if ((Bitmaps[i].Flags & PIGImage.BM_FLAG_RLE) != 0)
                {
                    int compressedSize = br.ReadInt32();
                    Bitmaps[i].Data = br.ReadBytes(compressedSize - 4);
                }
                else
                {
                    Bitmaps[i].Data = br.ReadBytes(Bitmaps[i].Width * Bitmaps[i].Height);
                }
            }
        }

        public void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
