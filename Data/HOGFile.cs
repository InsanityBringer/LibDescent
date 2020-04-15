﻿/*
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
    public class HOGFile
    {
        /// <summary>
        /// Persistent stream to the HOG file, to allow loading lump data on demand.
        /// </summary>
        private BinaryReader fileStream;
        /// <summary>
        /// A list of all the HOG lumps.
        /// </summary>
        private readonly List<HOGLump> lumps = new List<HOGLump>();
        /// <summary>
        /// A map that allows look-up of HOG lumps based on filename.
        /// </summary>
        private readonly Dictionary<string, int> lumpNameDirectory = new Dictionary<string, int>();

        /// <summary>
        /// The amount of lumps in the current HOG file.
        /// </summary>
        public int NumLumps { get { return lumps.Count; } }
        /// <summary>
        /// The current filename that the HOG file is read from and written to. 
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// The format that the HOG file is encoded with.
        /// </summary>
        public HOGFormat Format { get; set; }

        public HOGFile() { }

        /// <summary>
        /// Creates a HOGFile instance, reading data from the specified file.
        /// </summary>
        /// <param name="filename">The filename to read the HOG file from.</param>
        public HOGFile(string filename)
        {
            Read(filename);
        }

        /// <summary>
        /// Creates a HOGFile instance, reading data from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read the HOG file from.</param>
        public HOGFile(Stream stream)
        {
            Read(stream);
        }

        /// <summary>
        /// Reads the data of a HOG file specified by filename.
        /// </summary>
        /// <param name="filename">The filename to read the HOG file from.</param>
        public void Read(string filename)
        {
            Read(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
            Filename = filename;
        }

        /// <summary>
        /// Reads the data of a HOG file from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the HOG file from.</param>
        public void Read(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            fileStream = br;
            lumps.Clear();

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
                    lumps.Add(lump);
                }
            }
            catch (EndOfStreamException)
            {
                //we got all the files
                //heh
                //i love hog
                byte[] data;
                for (int i = 0; i < NumLumps; i++)
                {
                    data = GetLumpData(i);
                    lumps[i].type = HOGLump.IdentifyLump(lumps[i].name, data);

                    // In case of duplicates, first entry takes precedence
                    if (!lumpNameDirectory.ContainsKey(lumps[i].name))
                    {
                        lumpNameDirectory[lumps[i].name] = i;
                    }
                }
            }
        }

        /// <summary>
        /// Writes the current contents of the HOG file to a file with the given filename.
        /// </summary>
        /// <param name="filename">The filename to write the HOG file to.</param>
        public void Write(string filename)
        {
            string tempFilename = Path.ChangeExtension(filename, ".newtmp");
            using (var stream = File.Open(tempFilename, FileMode.Create))
            {
                Write(stream);
            }

            //Dispose of the old stream, and open up the new file as the read stream
            fileStream.Close();
            fileStream.Dispose();

            if (File.Exists(filename))
            {
                try
                {
                    File.Delete(filename);
                }
                catch (Exception exc) //Can't delete the old file for whatever reason...
                {
                    File.Delete(tempFilename); //Delete the temp file then...
                    throw exc;
                }
            }
            File.Move(tempFilename, filename);

            fileStream = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
            Filename = filename;
        }

        /// <summary>
        /// Writes the current contents of the HOG file to the given stream.
        /// </summary>
        /// <param name="stream">The stream to write the HOG file to.</param>
        public void Write(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream, Encoding.Default, true);

            string headerString = (Format == HOGFormat.Standard) ? "DHF" : "D2X";
            foreach (char character in headerString)
            {
                bw.Write((byte)character);
            }

            HOGLump lump;
            for (int i = 0; i < lumps.Count; i++)
            {
                lump = lumps[i];
                for (int c = 0; c < 13; c++)
                {
                    if (c < lump.name.Length)
                        bw.Write((byte)lump.name[c]);
                    else
                        bw.Write((byte)0);
                }
                if (Format == HOGFormat.Standard || lump.name.Length <= 13)
                {
                    bw.Write(lump.size);
                }
                else // D2X-XL with long filename
                {
                    bw.Write(-lump.size);
                    var longFilenameBuffer = new byte[256]; // automatically zero-initialized
                    // Cut off the last character if needed to ensure null-termination
                    Encoding.ASCII.GetBytes(lump.name.Substring(0, Math.Min(lump.name.Length, 255)))
                        .CopyTo(longFilenameBuffer, 0);
                    bw.Write(longFilenameBuffer);
                }
                if (lump.offset == -1) //This lump has cached data
                    bw.Write(lump.data);
                else //This lump doesn't have cached data, and instead needs to be read from the old stream
                {
                    byte[] data = GetLumpData(i);
                    bw.Write(data);
                }
                lump.offset = (int)bw.BaseStream.Position - lump.size; //Update the offset for the new file
            }
            bw.Flush();
        }

        /// <summary>
        /// Gets the number of a lump of a given filename.
        /// </summary>
        /// <param name="filename">The filename to find.</param>
        /// <returns>The number of the lump if it exists, or -1 if it does not exist.</returns>
        public int GetLumpNum(string filename)
        {
            if (!lumpNameDirectory.TryGetValue(filename.ToLower(), out int index))
            {
                return -1;
            }
            return index;
        }

        /// <summary>
        /// Gets the header information for a given lump.
        /// </summary>
        /// <param name="id">The number of the lump to get the header of.</param>
        /// <returns>The lump header.</returns>
        public HOGLump GetLumpHeader(int id)
        {
            if (id < 0 || id >= lumps.Count) return null;
            return lumps[id];
        }

        /// <summary>
        /// Gets the header information for a given lump.
        /// </summary>
        /// <param name="filename">The filename of the lump to get the header of.</param>
        /// <returns>The lump header.</returns>
        public HOGLump GetLumpHeader(string filename)
        {
            return GetLumpHeader(GetLumpNum(filename));
        }

        /// <summary>
        /// Gets the raw data of a given lump.
        /// </summary>
        /// <param name="id">The number of the lump to get the data of.</param>
        /// <returns>A byte[] array of the lump's data.</returns>
        public byte[] GetLumpData(int id)
        {
            if (id < 0 || id >= lumps.Count) return null;
            if (lumps[id].data != null)
                return lumps[id].data;

            fileStream.BaseStream.Seek(lumps[id].offset, SeekOrigin.Begin);
            return fileStream.ReadBytes(lumps[id].size);
        }

        /// <summary>
        /// Gets the raw data of a given lump.
        /// </summary>
        /// <param name="filename">The filename of the lump to get the data of.</param>
        /// <returns>A byte[] array of the lump's data.</returns>
        public byte[] GetLumpData(string filename)
        {
            return GetLumpData(GetLumpNum(filename));
        }

        /// <summary>
        /// Opens a lump in a stream for reading.
        /// </summary>
        /// <param name="id">The number of the lump to open.</param>
        /// <returns>A stream containing the lump's data.</returns>
        public Stream GetLumpAsStream(int id)
        {
            if (id < 0 || id >= lumps.Count) return null;
            byte[] data = GetLumpData(id);
            return new MemoryStream(data);
        }

        /// <summary>
        /// Opens a lump in a stream for reading.
        /// </summary>
        /// <param name="filename">The filename of the lump to open.</param>
        /// <returns>A stream containing the lump's data.</returns>
        public Stream GetLumpAsStream(string filename)
        {
            return GetLumpAsStream(GetLumpNum(filename));
        }

        /// <summary>
        /// Adds a lump to the HOG file.
        /// </summary>
        /// <param name="lump">The lump to add.</param>
        public void AddLump(HOGLump lump)
        {
            lumps.Add(lump);
            if (!lumpNameDirectory.ContainsKey(lump.name))
            {
                lumpNameDirectory[lump.name] = lumps.Count - 1;
            }
        }

        /// <summary>
        /// Adds a file from disk to the HOG file.
        /// </summary>
        /// <param name="filename">The path to the file to add.</param>
        public void AddFile(string filename)
        {
            AddFile(Path.GetFileName(filename), File.OpenRead(filename));
        }

        /// <summary>
        /// Adds a lump to the HOG file, copying its contents from the specified stream.
        /// </summary>
        /// <param name="filename">The filename to use for the new lump.</param>
        /// <param name="fileData">A stream holding the contents of the file to add.</param>
        public void AddFile(string filename, Stream fileData)
        {
            // Make a copy of the file contents and put that in the HOGLump
            var dataBuffer = new MemoryStream();
            fileData.CopyTo(dataBuffer);
            AddLump(new HOGLump(filename, dataBuffer.ToArray()));
        }

        /// <summary>
        /// Deletes a lump from the HOG file by number.
        /// </summary>
        /// <param name="id">The number of the lump to delete.</param>
        public void DeleteLump(int id)
        {
            lumps.RemoveAt(id);

            // We need to rebuild lumpNameDirectory because the indices may have all changed
            lumpNameDirectory.Clear();
            for (int i = 0; i < NumLumps; i++)
            {
                if (!lumpNameDirectory.ContainsKey(lumps[i].name))
                {
                    lumpNameDirectory[lumps[i].name] = i;
                }
            }
        }

        /// <summary>
        /// Finds all lumps that match the specified type.
        /// </summary>
        /// <param name="type">The lump type to search for.</param>
        /// <returns>All lumps in the HOG file that match the type requested.</returns>
        public List<HOGLump> GetLumpsByType(LumpType type)
        {
            return lumps.Where(lump => lump.type == type).ToList();
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
