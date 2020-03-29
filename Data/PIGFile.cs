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
using System.IO;

namespace LibDescent.Data
{
    public class PIGFile
    {
        public List<PIGImage> Bitmaps { get; private set; }
        private long startptr = 0L;
        private int header, version;
        public string filename;

        public PIGFile()
        {
            Bitmaps = new List<PIGImage>(2620);
            //Init a bogus texture for all piggyfiles
            PIGImage bogusTexture = new PIGImage(64, 64, 0, 0, 0, 0, "bogus", 0);
            bogusTexture.data = new byte[64 * 64];
            //Create an X using descent 1 palette indicies. For accuracy. Heh
            for (int i = 0; i < 4096; i++)
            {
                bogusTexture.data[i] = 85;
            }
            for (int i = 0; i < 64; i++)
            {
                bogusTexture.data[i * 64 + i] = 193;
                bogusTexture.data[i * 64 + (63 - i)] = 193;
            }
            Bitmaps.Add(bogusTexture);
        }

        public void Read(string name)
        {
            BinaryReader br = new BinaryReader(File.Open(name, FileMode.Open));

            header = br.ReadInt32();
            version = br.ReadInt32();
            int textureCount = br.ReadInt32();

            for (int x = 0; x < textureCount; x++)
            {
                bool hashitnull = false;
                char[] localname = new char[8];
                for (int i = 0; i < 8; i++)
                {
                    char c = (char)br.ReadByte();
                    if (c == 0)
                        hashitnull = true;
                    if (!hashitnull)
                        localname[i] = c;
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
                Bitmaps.Add(image);
            }
            startptr = br.BaseStream.Position;

            for (int i = 1; i < Bitmaps.Count; i++)
            {
                br.BaseStream.Seek(startptr + Bitmaps[i].offset, SeekOrigin.Begin);
                if ((Bitmaps[i].flags & PIGImage.BM_FLAG_RLE) != 0)
                {
                    int compressedSize = br.ReadInt32();
                    Bitmaps[i].data = br.ReadBytes(compressedSize - 4);
                }
                else
                {
                    Bitmaps[i].data = br.ReadBytes(Bitmaps[i].width * Bitmaps[i].height);
                }
                //images[i].LoadData(br);
            }
            
            br.Close();
            br.Dispose();
            filename = name;
        }

        public void Write(string name)
        {
            BinaryWriter bw = new BinaryWriter(File.Open(name, FileMode.Create));
            int offset = 0;
            bw.Write(header);
            bw.Write(version);
            bw.Write(Bitmaps.Count-1); //Start from 1 to avoid writing the bogus image
            for (int i = 1; i < Bitmaps.Count; i++)
            {
                Bitmaps[i].offset = offset;
                offset += Bitmaps[i].GetSize();
                Bitmaps[i].WriteImageHeader(bw);
            }
            for (int i = 1; i < Bitmaps.Count; i++)
            {
                Bitmaps[i].WriteImage(bw);
            }
            bw.Flush();
            bw.Close();
            bw.Dispose();
            filename = name;
        }

        public PIGImage GetImage(int id)
        {
            if (id >= Bitmaps.Count || id < 0) return Bitmaps[0];
            return Bitmaps[id];
        }

        public PIGImage GetImage(string name)
        {
            for (int x = 0; x < Bitmaps.Count; x++)
            {
                //todo: Dictionary
                if (Bitmaps[x].name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return Bitmaps[x];
                }
            }
            return Bitmaps[0];
        }

        public byte[] GetBitmap(int id)
        {
            if (id >= Bitmaps.Count) return Bitmaps[0].GetData();
            PIGImage image = Bitmaps[id];
            return image.GetData();
        }

        public byte[] GetBitmap(string name)
        {
            for (int x = 0; x < Bitmaps.Count; x++)
            {
                //todo: Dictionary
                if (Bitmaps[x].name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return GetBitmap(x);
                }
            }
            return GetBitmap(0);
        }

        public int GetBitmapIDFromName(string name)
        {
            for (int x = 0; x < Bitmaps.Count; x++)
            {
                if (Bitmaps[x].name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return x;
                }
            }
            return 0;
        }
    }
}
