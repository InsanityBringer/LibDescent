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
using System.Linq;
using System.Text;

namespace LibDescent.Data
{
    /// <summary>
    /// Enumerates the types of lump that can be viewed or edited with the HOG editor
    /// </summary>
    public enum LumpType
    {
        Unknown,
        Text, //for DESCENT.SNG and mission files, identify by file containing only printable bytes? (and SUB...)
        EncodedText, //for *.TXB, and BITMAPS.BIN (shareware and registered descent 1.0). How to identify this...
        Font, //*.FNT lumps, Identified by PSFN header and sane version
        RawSound, //Identified by *.RAW. ugh. Only needed for digtest.raw
        Midi, //*.MID lumps, Identified by MThd header and sane length. These exist solely for the old native Windows version. 
        HMP, //*.HMP/*.HMQ lumps, Identified by HMIMIDIP header and sane header data. 
        OPLBank, //*.BNK lumps, Identified by 0x0 0x0 A * L I B perhaps? I dunno...
        Level, //*.SL2/*.RL2 lumps, Identified by LVLP header and sane version and pointers
        Palette, //*.256 lumps, Identified by being 9472 bytes long, and first 768 are all <64
        PCXImage, //*.PCX lumps, Identifed by 0x10 0x5 0x1 0x8?
        LBMImage, //*.BBM/*.LBM lumps, Identified by FORM and sane header values
        HAMFile, //*.HAM lumps, Identified by HAM! header and sane version. Embeddable for DXX-Rebirth
        HXMFile, //*.HXM lumps, Identified by HXM! header and sane version.
        VHAMFile, //*.VHAM lumps, Identified by MAHX header and sane version.
    }

    public class HOGLump
    {
        /// <summary>
        /// The file name associated with this lump.
        /// </summary>
        public string Name;
        /// <summary>
        /// The size of the data of this lump.
        /// </summary>
        public int Size;
        /// <summary>
        /// The offset of this lump within the .HOG that it is contained in.
        /// </summary>
        public int Offset;
        /// <summary>
        /// The raw data contained within this lump.
        /// </summary>
        public byte[] Data; //Needed for imported items
        /// <summary>
        /// The type of this lump, as detected from the data.
        /// </summary>
        public LumpType Type;

        public HOGLump(string name, int size, int offset)
        {
            this.Name = name;
            this.Size = size;
            this.Offset = offset;
        }

        public HOGLump(string name, byte[] data)
        {
            this.Name = name;
            Size = data.Length;
            Offset = -1;
            this.Data = data;
            Type = IdentifyLump(name, data);
        }

        public static LumpType IdentifyLump(string name, byte[] data)
        {
            if (IsLevel(data)) return LumpType.Level;
            if (IsHAM(data)) return LumpType.HAMFile;
            if (IsHXM(data)) return LumpType.HXMFile;
            if (IsVHAM(data)) return LumpType.VHAMFile;
            if (IsILBM(data)) return LumpType.LBMImage;
            if (IsPCX(data)) return LumpType.PCXImage;
            if (IsFont(data)) return LumpType.Font;
            // Midi - missing
            if (IsHMP(data)) return LumpType.HMP;
            if (IsOPLBank(data)) return LumpType.OPLBank;
            if (IsPalette(data)) return LumpType.Palette;
            string ext = (name.IndexOf('.') >= 0) ? name.Substring(name.IndexOf('.')) : "";
            if (ext.Equals(".raw", StringComparison.OrdinalIgnoreCase)) return LumpType.RawSound;
            if (IsText(data))
            {
                if (ext.Equals(".txb", StringComparison.OrdinalIgnoreCase) || ext.Equals(".bin", StringComparison.OrdinalIgnoreCase)) //stupid hacks
                    return LumpType.EncodedText;
                return LumpType.Text;
            }
            return LumpType.Unknown;
        }

        private static bool CheckSignature(string expectedSignature, byte[] data, int signatureOffset = 0)
        {
            var signature = new ArraySegment<byte>(data, signatureOffset, expectedSignature.Length).ToArray();
            return Encoding.ASCII.GetString(signature).CompareTo(expectedSignature) == 0;
        }

        //This should be very low priority
        public static bool IsText(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] < ' ' && data[i] != '\t' && data[i] != '\r' && data[i] != '\n' && data[i] != 0x1A ) //Check printable or formatting. Also check ASCII SUB, since it terminates many files
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsILBM(byte[] data)
        {
            if (data.Length > 8)
            {
                if (data[0] == 'F' && data[1] == 'O' && data[2] == 'R' && data[3] == 'M')
                {
                    int dataLen = data[7] + (data[6] << 8) + (data[5] << 16) + (data[4] << 24);
                    if (dataLen <= data.Length)
                        return true;
                }
            }
            return false;
        }

        public static bool IsPCX(byte[] data)
        {
            if (data.Length > 128)
            {
                if (data[0] == 0x0A && data[1] <= 0x05 && data[2] <= 0x01 && data[3] == 0x08)
                {
                    return true; //Should do more validation, but given that these bytes aren't printable its hard to make a text file that would throw it off. Though maybe TXB could? Needs further testing...
                }
            }
            return false;
        }

        public static bool IsFont(byte[] data)
        {
            if (data.Length > 8)
            {
                //50 53 46 4E
                if (data[0] == 0x50 && data[1] == 0x53 && data[2] == 0x46 && data[3] == 0x4E)
                {
                    int dataLen = data[4] + (data[5] << 8) + (data[6] << 16) + (data[7] << 24);
                    if (dataLen <= data.Length)
                        return true;
                }
            }
            return false;
        }

        public static bool IsHMP(byte[] data)
        {
            if (data.Length > 0x324)
            {
                if (data[0] == 0x48 && data[1] == 0x4D && data[2] == 0x49 && data[3] == 0x4D && data[4] == 0x49 && data[5] == 0x44 && data[6] == 0x49 && data[7] == 0x50)
                {
                    return true; //TODO add additional tests
                }
            }
            return false;
        }

        public static bool IsOPLBank(byte[] data)
        {
            if (data.Length > 8)
            {
                if (data[0] == 0 && data[1] == 0 && data[2] == 'A' && data[4] == 'L' && data[5] == 'I' && data[6] == 'B')
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsPalette(byte[] data)
        {
            if (data.Length == 9472)
            {
                //Some additional validation. Check if all values are 6 bit
                for (int i = 0; i < 768; i++)
                {
                    if (data[i] > 63) return false;
                }
                return true;
            }
            return false;
        }

        public static bool IsLevel(byte[] data)
        {
            if (data.Length < 4) return false;
            if (!CheckSignature("LVLP", data)) return false;

            // Some basic validation
            // (we could just load the level and see if it succeeds but that's expensive)
            try
            {
                var reader = new BinaryReader(new MemoryStream(data));
                reader.BaseStream.Position = 4;
                var levelVersion = reader.ReadInt32();
                var mineDataOffset = reader.ReadInt32();
                var gameDataOffset = reader.ReadInt32();
                if (levelVersion < 0 || levelVersion > D2LevelReader.MaximumSupportedLevelVersion) return false;
                if (mineDataOffset == 0) return false;
                if (gameDataOffset == 0) return false;
            }
            catch (Exception ex) when (ex is EndOfStreamException || ex is IOException)
            {
                return false;
            }
            return true;
        }

        public static bool IsHAM(byte[] data)
        {
            if (data.Length < 4) return false;
            return CheckSignature("HAM!", data) && !IsText(data);
            // Supposed to have version checks here, need to confirm valid versions though.
        }

        public static bool IsHXM(byte[] data)
        {
            if (data.Length < 4) return false;
            // No, this is not a typo. At least not mine. Sigh.
            return CheckSignature("HMX!", data) && !IsText(data);
            // TODO add version checks
        }

        public static bool IsVHAM(byte[] data)
        {
            if (data.Length < 4) return false;
            return CheckSignature("MAHX", data) && !IsText(data);
            // TODO add version checks
        }
    }
}
