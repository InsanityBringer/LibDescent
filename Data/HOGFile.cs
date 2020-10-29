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
using System.Linq;
using System.Text;

namespace LibDescent.Data
{
    /// <summary>
    /// Represents a HOG file, a composite data file containing one or more lumps. 
    /// </summary>
    public class HOGFile : IDataFile
    {
        /// <summary>
        /// Persistent stream to the HOG file, to allow loading lump data on demand.
        /// </summary>
        private BinaryReader fileStream;
        /// <summary>
        /// A list of all the HOG lumps.
        /// </summary>
        public List<HOGLump> Lumps { get; } = new List<HOGLump>();

        /// <summary>
        /// The amount of lumps in the current HOG file.
        /// </summary>
        public int NumLumps { get { return Lumps.Count; } }

        /// <summary>
        /// The format that the HOG file is encoded with.
        /// </summary>
        public HOGFormat Format { get; set; }

        // mutexes/locks. acquiring order: hogFileLock, lumpLock, lumpNameLock.
        private readonly object hogFileLock = new object();      // used when accessing/modifying file stream

        public HOGFile() { }

        /// <summary>
        /// Creates a HOGFile instance, reading data from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read the HOG file from.</param>
        public HOGFile(Stream stream)
        {
            Read(stream);
        }

        /// <summary>
        /// Reads the data of a HOG file from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the HOG file from. The stream must be seekable</param>
        public void Read(Stream stream)
        {
            lock (hogFileLock)
            {
                if (!stream.CanSeek)
                    throw new ArgumentException("HOGFIle:Read: Passed stream must be seekable.");

                BinaryReader br = new BinaryReader(stream);
                fileStream = br;
                Lumps.Clear();

                char[] header = new char[3];
                header[0] = (char)br.ReadByte();
                header[1] = (char)br.ReadByte();
                header[2] = (char)br.ReadByte();

                var headerString = new string(header);
                switch (headerString)
                {
                    case "DHF":
                        Format = HOGFormat.Standard;
                        break;
                    case "D2X":
                        Format = HOGFormat.D2X_XL;
                        break;
                    default:
                        throw new InvalidDataException($"Unrecognized HOG header \"{headerString}\"");
                }

                try
                {
                    while (true)
                    {
                        char[] filenamedata = new char[13];
                        bool hashitnull = false;
                        for (int x = 0; x < 13; x++)
                        {
                            char c = (char)br.ReadByte();
                            if (c == 0)
                            {
                                hashitnull = true;
                            }
                            if (!hashitnull)
                            {
                                filenamedata[x] = c;
                            }
                        }
                        string filename = new string(filenamedata);
                        filename = filename.Trim(' ', '\0');
                        int filesize = br.ReadInt32();
                        if (Format == HOGFormat.D2X_XL && filesize < 0)
                        {
                            // D2X-XL format encodes "extended" lump headers with negative file sizes
                            filesize = -filesize;

                            string longFilename = Encoding.ASCII.GetString(br.ReadBytes(256));
                            if (longFilename.Contains("\0"))
                            {
                                longFilename = longFilename.Remove(longFilename.IndexOf('\0'));
                            }
                            longFilename = longFilename.Trim(' ');
                            // No real reason to use short filename in this instance; just replace it
                            filename = longFilename;
                        }
                        int offset = (int)br.BaseStream.Position;
                        br.BaseStream.Seek(filesize, SeekOrigin.Current); //I hate hog files. Wads are cooler..

                        HOGLump lump = new HOGLump(filename, filesize, offset);
                        Lumps.Add(lump);
                    }
                }
                catch (EndOfStreamException)
                {
                    //we got all the files
                    //heh
                    //i love hog
                    //classification now lives in EditorHOGFile since only editors care about it
                }
            }
        }

        /// <summary>
        /// Writes the current contents of the HOG file to the given stream.
        /// </summary>
        /// <param name="stream">The stream to write the HOG file to.</param>
        public void Write(Stream stream)
        {
            lock (hogFileLock)
            {
                BinaryWriter bw = new BinaryWriter(stream, Encoding.Default, true);

                string headerString = (Format == HOGFormat.Standard) ? "DHF" : "D2X";
                foreach (char character in headerString)
                {
                    bw.Write((byte)character);
                }

                HOGLump lump;
                for (int i = 0; i < Lumps.Count; i++)
                {
                    lump = Lumps[i];
                    for (int c = 0; c < 13; c++)
                    {
                        if (c < lump.Name.Length)
                            bw.Write((byte)lump.Name[c]);
                        else
                            bw.Write((byte)0);
                    }
                    if (Format == HOGFormat.Standard || lump.Name.Length <= 13)
                    {
                        bw.Write(lump.Size);
                    }
                    else // D2X-XL with long filename
                    {
                        bw.Write(-lump.Size);
                        var longFilenameBuffer = new byte[256]; // automatically zero-initialized
                        // Cut off the last character if needed to ensure null-termination
                        Encoding.ASCII.GetBytes(lump.Name.Substring(0, Math.Min(lump.Name.Length, 255)))
                            .CopyTo(longFilenameBuffer, 0);
                        bw.Write(longFilenameBuffer);
                    }
                    if (lump.Offset == -1) //This lump has cached data
                        bw.Write(lump.Data);
                    else //This lump doesn't have cached data, and instead needs to be read from the old stream
                    {
                        byte[] data = GetLumpData(i);
                        bw.Write(data);
                    }
                    lump.Offset = (int)bw.BaseStream.Position - lump.Size; //Update the offset for the new file
                }
                bw.Flush();
            }
        }

        /// <summary>
        /// Gets the raw data of a given lump.
        /// </summary>
        /// <param name="id">The number of the lump to get the data of.</param>
        /// <returns>A byte[] array of the lump's data.</returns>
        public byte[] GetLumpData(int id)
        {
            lock (hogFileLock)
            {
                if (id < 0 || id >= Lumps.Count) return null;
                if (Lumps[id].Data != null)
                    return Lumps[id].Data;

                //CheckFileStreamNotNull(); //TODO: Need a way to check stream validity
                fileStream.BaseStream.Seek(Lumps[id].Offset, SeekOrigin.Begin);
                return fileStream.ReadBytes(Lumps[id].Size);
            }
        }

        /// <summary>
        /// Opens a lump in a stream for reading.
        /// </summary>
        /// <param name="id">The number of the lump to open.</param>
        /// <returns>A stream containing the lump's data.</returns>
        public Stream GetLumpAsStream(int id)
        {
            if (id < 0 || id >= Lumps.Count) return null;
            byte[] data = GetLumpData(id);
            return new MemoryStream(data);
        }
    }

    public enum HOGFormat
    {
        /// <summary>
        /// HOG format used by Descent and Descent 2, including most source ports.
        /// Supports up to 250 files with a maximum size of 2 GB, and filenames in 8.3 format.
        /// </summary>
        Standard,
        /// <summary>
        /// Extended HOG format used by D2X-XL.
        /// Adds 255-character filename support and has no maximum file count.
        /// </summary>
        D2X_XL
    }
}
