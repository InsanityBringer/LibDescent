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
    public class SoundData
    {
        public string Name;
        public int Length;
        public int Offset;

        public byte[] Data;
        public byte[] LocalName;
    }
    public class SNDFile
    {
        //public List<string> sounds = new List<string>();
        public List<SoundData> Sounds = new List<SoundData>();
        public Dictionary<string, int> SoundIDs = new Dictionary<string, int>();
        private long startptr = 0L;
        private BinaryReader stream;
        private long soundptr = 0;
        public string FileName; 

        public void LoadDataFile(string name)
        {
            BinaryReader br = new BinaryReader(File.Open(name, FileMode.Open, FileAccess.Read, FileShare.Read));

            int header = br.ReadInt32();
            //48 41 4D 21
            if (header == 0x214D4148) //secret demo sound extractor
            {
                int version = br.ReadInt32();
                if (version != 2)
                {
                    br.Close();
                    throw new Exception("HAM header is not version 2");
                }
                soundptr = br.ReadInt32();
                br.BaseStream.Seek(soundptr, SeekOrigin.Begin);
            }

            else if (header != 0x444E5344)
            {
                br.Close();
                throw new Exception("Sound header lacks DSND header");
            }
            else
            {
                int version = br.ReadInt32();
                if (version != 1)
                {
                    br.Close();
                    throw new Exception("Sound header is not version 1");
                }
                soundptr = 0;
            }
            int soundCount = br.ReadInt32();

            bool hashitnull = false;

            for (int x = 0; x < soundCount; x++)
            {
                hashitnull = false;
                char[] localname = new char[8];
                for (int i = 0; i < 8; i++)
                {
                    char c = (char)br.ReadByte();
                    if (c == 0)
                    {
                        hashitnull = true;
                    }
                    if (!hashitnull)
                    {
                        localname[i] = c;
                    }
                }
                string soundname = new string(localname);
                soundname = soundname.Trim(' ', '\0');
                int num1 = br.ReadInt32();
                int num2 = br.ReadInt32();
                int offset = br.ReadInt32();

                SoundData sound = new SoundData { Data = null };
                sound.Name = soundname;
                sound.Offset = offset;
                sound.Length = num1;
                Sounds.Add(sound);

                //sounds.Add(soundname);
                SoundIDs.Add(soundname, x);
            }
            startptr = br.BaseStream.Position;

            stream = br;
            //br.Close();
            FileName = name;
        }

        public byte[] LoadSound(int id)
        {
            int offset = Sounds[id].Offset;
            int len = Sounds[id].Length;

            byte[] data = new byte[len];
            long loc = stream.BaseStream.Position;
            stream.BaseStream.Seek(offset, SeekOrigin.Current);
            data = stream.ReadBytes(len);

            stream.BaseStream.Seek(loc, SeekOrigin.Begin);

            return data;
        }

        public void CloseDataFile()
        {
            stream.Close();
        }
    }
}
