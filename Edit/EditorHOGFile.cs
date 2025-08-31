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

using LibDescent.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibDescent.Edit
{
    /// <summary>
    /// Represents a HOG file, a composite data file containing one or more lumps. 
    /// </summary>
    public class EditorHOGFile : HOGFile
    {
        /// <summary>
        /// A map that allows look-up of HOG lumps based on filename.
        /// </summary>
        /// <remarks>Acquire _lumpLock before accessing this field.</remarks>
        private Dictionary<string, int> _lumpNameMap = null;

        /// <summary>
        /// The current filename that the HOG file is read from and written to. 
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The format that the HOG file is encoded with.
        /// </summary>
        public new HOGFormat Format
        {
            get => base.Format;

            set
            {
                // We need to rewrite and reopen the file when switching formats so that
                // we don't corrupt the HOG
                Write(Filename, value);
            }
        }

        public EditorHOGFile() { }

        /// <summary>
        /// Creates a HOGFile instance, reading data from the specified file.
        /// </summary>
        /// <param name="filename">The filename to read the HOG file from.</param>
        public EditorHOGFile(string filename)
        {
            Read(filename);
        }

        /// <summary>
        /// Creates a HOGFile instance, reading data from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to read the HOG file from.</param>
        public EditorHOGFile(Stream stream)
        {
            Read(stream);
        }

        /// <summary>
        /// Reads the data of a HOG file specified by filename.
        /// </summary>
        /// <param name="filename">The filename to read the HOG file from.</param>
        public void Read(string filename)
        {
            Filename = filename;
            Read(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        private void RebuildLumpNameMap()
        {
            lock (_lumpLock)
            {
                _lumpNameMap = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
                for (int i = 0; i < NumLumps; i++)
                {
                    // In case of duplicates, first entry takes precedence
                    if (!_lumpNameMap.ContainsKey(Lumps[i].Name))
                    {
                        _lumpNameMap[Lumps[i].Name] = i;
                    }
                }
            }
        }

        /// <summary>
        /// Writes the current contents of the HOG file to its associated file on disk.
        /// </summary>
        public void Write()
        {
            if (Filename == null)
            {
                throw new InvalidOperationException("This HOG file has no associated filename.");
            }

            Write(Filename);
        }

        /// <summary>
        /// Writes the current contents of the HOG file to a file with the given filename, then
        /// uses that file as the new source.
        /// </summary>
        /// <param name="filename">The filename to write the HOG file to.</param>
        public override void Write(string filename)
        {
            Write(filename, Format);
        }

        /// <summary>
        /// Writes the current contents of the HOG file to a file with the given filename, then
        /// uses that file as the new source.
        /// </summary>
        /// <param name="filename">The filename to write the HOG file to.</param>
        /// <param name="format">The format to write the HOG file in.</param>
        public override void Write(string filename, HOGFormat format)
        {
            lock (_hogFileLock)
            {
                string tempFilename = Path.ChangeExtension(filename, ".newtmp");
                using (var stream = File.Open(tempFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    Write(stream, format);
                }

                //Dispose of the old stream, and open up the new file as the read stream
                Close();

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

                // Simpler to reopen the file than make a duplicate Write() method that updates
                // offsets etc
                Read(filename);
            }
        }

        /// <summary>
        /// Gets the number of a lump of a given filename.
        /// </summary>
        /// <param name="filename">The filename to find.</param>
        /// <returns>The number of the lump if it exists, or -1 if it does not exist.</returns>
        public new int GetLumpNum(string filename)
        {
            lock (_lumpLock)
            {
                if (_lumpNameMap == null)
                {
                    RebuildLumpNameMap();
                }
                if (!_lumpNameMap.TryGetValue(filename, out int index))
                {
                    return -1;
                }
                return index;
            }
        }

        /// <summary>
        /// Gets the header information for a given lump.
        /// </summary>
        /// <param name="id">The number of the lump to get the header of.</param>
        /// <returns>The lump header.</returns>
        public HOGLump GetLumpHeader(int id)
        {
            lock (_lumpLock)
            {
                if (id < 0 || id >= Lumps.Count) return null;
                return Lumps[id];
            }
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
        /// If a HOG file is open on disk, closes it.
        /// </summary>
        public void Close()
        {
            lock (_hogFileLock)
            {
                if (_fileStream != null)
                {
                    _fileStream.Close();
                    _fileStream.Dispose();
                    _fileStream = null;
                }
            }
        }

        protected override void CheckFileStreamNotNull()
        {
            lock (_hogFileLock)
            {
                if (_fileStream == null)
                {
                    if (Filename != null)
                    {
                        _fileStream = new BinaryReader(File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.Read));
                    }
                    else
                    {
                        throw new InvalidOperationException("HOG file stream is not available.");
                    }
                }
            }
        }

        /// <summary>
        /// Adds a lump to the HOG file.
        /// </summary>
        /// <param name="lump">The lump to add.</param>
        public void AddLump(HOGLump lump)
        {
            lock (_lumpLock)
            {
                Lumps.Add(lump);
                if (_lumpNameMap != null && !_lumpNameMap.ContainsKey(lump.Name))
                {
                    _lumpNameMap[lump.Name] = Lumps.Count - 1;
                }
            }
        }

        /// <summary>
        /// Replaces the lump in a HOG file. The replacement applies
        /// if another lump with the same file name is already present,
        /// and otherwise the lump is simply added.
        /// </summary>
        /// <param name="lump">The lump to add.</param>
        public void ReplaceLump(HOGLump lump)
        {
            lock (_lumpLock)
            {
                int lumpNum;
                do
                {
                    lumpNum = GetLumpNum(lump.Name);
                    if (lumpNum >= 0)
                    {
                        Lumps.RemoveAt(lumpNum);
                        _lumpNameMap = null;
                    }
                } while (lumpNum >= 0);
                AddLump(lump);
            }
        }

        /// <summary>
        /// Adds a lump to the HOG file at a given index.
        /// </summary>
        /// <param name="lump">The lump to add.</param>
        /// <param name="index">The index to add at.</param>
        public void AddLumpAt(HOGLump lump, int index)
        {
            lock (_lumpLock)
            {
                Lumps.Insert(index, lump);
                //For now, invalidate the lump map when adding a lump at a given location
                //since otherwise the old indicies would be messed up.
                _lumpNameMap = null;
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
            lock (_lumpLock)
            {
                Lumps.RemoveAt(id);

                // We need to rebuild _lumpNameMap because the indices may have all changed
                // Do this on-demand (makes multiple deletions faster, especially if the index
                // isn't being used)
                _lumpNameMap = null;
            }
        }

        /// <summary>
        /// Returns whether the given file name is used in the .HOG.
        /// </summary>
        /// <param name="name">The file name to look for.</param>
        /// <returns>Whether the file exists.</returns>
        public bool ContainsFile(string name)
        {
            return GetLumpNum(name) >= 0;
        }

        /// <summary>
        /// Finds all lumps that match the specified type.
        /// </summary>
        /// <param name="type">The lump type to search for.</param>
        /// <returns>All lumps in the HOG file that match the type requested.</returns>
        public List<HOGLump> GetLumpsByType(LumpType type)
        {
            return Lumps.Where(lump => lump.Type == type).ToList();
        }
    }
}
