using LibDescent.Data;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace LibDescent.Tests
{
    class HogTests
    {
        [Test]
        public void TestStandardHogRead()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));
            Assert.AreEqual(HOGFormat.Standard, hogFile.Format);
        }

        [Test]
        public void TestStandardHogWrite()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));
            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            FileAssert.AreEqual(TestUtils.GetResourceStream("standard.hog"), memoryStream);
        }

        [Test]
        public void TestStandardHogLumpHeaders()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));
            Assert.AreEqual(3, hogFile.NumLumps);

            // First lump - .hxm
            var lumpHeader = hogFile.Lumps[0];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("test.hxm", lumpHeader.Name);
            Assert.AreEqual(0x14, lumpHeader.Offset);
            Assert.AreEqual(32530, lumpHeader.Size);
            //[ISB]: Classification is killed for non-editor HOG files. 
            //Assert.AreEqual(LumpType.HXMFile, lumpHeader.Type);

            // Second lump - .pog
            lumpHeader = hogFile.Lumps[1];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("test.pog", lumpHeader.Name);
            Assert.AreEqual(32567, lumpHeader.Offset);
            Assert.AreEqual(4128, lumpHeader.Size);
            //Assert.AreEqual(LumpType.Unknown, lumpHeader.Type);

            // Third lump - .rl2
            lumpHeader = hogFile.Lumps[2];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("test.rl2", lumpHeader.Name);
            Assert.AreEqual(36712, lumpHeader.Offset);
            Assert.AreEqual(7010, lumpHeader.Size);
            //Assert.AreEqual(LumpType.Level, lumpHeader.Type);
        }

        [Test]
        public void TestLumpData()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));
            var lumpHeader = hogFile.Lumps[2];

            var lumpData = hogFile.GetLumpData(2);
            Assert.NotNull(lumpData);
            Assert.AreEqual(lumpHeader.Size, lumpData.Length);

            var lumpStream = hogFile.GetLumpAsStream(2);
            Assert.NotNull(lumpStream);
            Assert.AreEqual(lumpHeader.Size, lumpStream.Length);

            // One last thing... try reading the stream
            var level = LevelFactory.CreateFromStream(lumpStream);
            Assert.NotNull(level);
            Assert.AreEqual(20, level.Segments.Count);
        }

        //[ISB] TODO: Port to testing EditorHOGFile
        /*[Test]
        public void TestGetLumpByName()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));

            var lumpNum = hogFile.GetLumpNum("test.pog");
            Assert.AreEqual(1, lumpNum);
            Assert.AreEqual(lumpNum, hogFile.GetLumpNum("TEST.POG"));

            var lumpHeader = hogFile.GetLumpHeader("test.pog");
            Assert.NotNull(lumpHeader);
            Assert.AreSame(hogFile.GetLumpHeader(lumpNum), lumpHeader);

            var lumpData = hogFile.GetLumpData("test.pog");
            Assert.NotNull(lumpData);
            Assert.AreEqual(lumpHeader.Size, lumpData.Length);

            var lumpStream = hogFile.GetLumpAsStream("test.pog");
            Assert.NotNull(lumpStream);
            Assert.AreEqual(lumpHeader.Size, lumpStream.Length);
        }*/

        //[ISB] TODO: Port to testing EditorHOGFile
        /*[Test]
        public void TestGetLumpsByType()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("d2x-xl.hog"));

            var lumps = hogFile.GetLumpsByType(LumpType.Level);
            Assert.NotNull(lumps);
            Assert.AreEqual(3, lumps.Count);

            foreach (var lump in lumps)
            {
                Assert.AreEqual(LumpType.Level, lump.Type);
            }
        }*/


        [Test]
        public void TestAddLump()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));

            // Method 1 - set up lump manually
            var file1Data = TestUtils.GetArrayFromResourceStream("test.rdl");
            var hogLump = new HOGLump("test.rdl", file1Data.Length, 0)
            {
                Data = file1Data
            };
            hogFile.Lumps.Add(hogLump);
            Assert.AreEqual(4, hogFile.NumLumps);
            //Assert.AreEqual(3, hogFile.GetLumpNum("test.rdl")); //api change

            // Method 2 - use constructor
            var file2Data = TestUtils.GetArrayFromResourceStream("test.rl2");
            hogLump = new HOGLump("test2.rl2", file2Data);
            hogFile.Lumps.Add(hogLump);
            Assert.AreEqual(5, hogFile.NumLumps);
            //Assert.AreEqual(4, hogFile.GetLumpNum("test2.rl2"));

            // Make sure it writes correctly
            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            hogFile.Read(memoryStream);
            Assert.AreEqual(5, hogFile.NumLumps);
            //Assert.AreEqual(3, hogFile.GetLumpNum("test.rdl"));
            Assert.That(hogFile.GetLumpData(3), Is.EqualTo(file1Data));
            //Assert.AreEqual(4, hogFile.GetLumpNum("test2.rl2"));
            Assert.That(hogFile.GetLumpData(4), Is.EqualTo(file2Data));
        }

        //[ISB] TODO: Port to testing EditorHOGFile
        /*[Test]
        public void TestReplaceLump()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));

            // Method 1 - set up lump manually
            var file1Data = TestUtils.GetArrayFromResourceStream("test.rdl");
            var hogLump = new HOGLump("test.rdl", file1Data.Length, 0)
            {
                Data = file1Data
            };
            hogFile.AddLump(hogLump);
            //Assert.AreEqual(4, hogFile.NumLumps);

            int numLumps = hogFile.NumLumps;
            hogFile.ReplaceLump(hogLump);
            Assert.AreEqual(numLumps, hogFile.NumLumps);
        }*/

        //[ISB] TODO: Port to testing EditorHOGFile
        /*
        [Test]
        public void TestAddFile()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));
            var filename = "test.rdl";
            //[ISB] this API is gone from normal HOGFile
            hogFile.AddFile(filename, TestUtils.GetResourceStream(filename));

            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            hogFile.Read(memoryStream);
            Assert.AreEqual(4, hogFile.NumLumps);
            Assert.AreEqual(3, hogFile.GetLumpNum(filename));
            Assert.That(hogFile.GetLumpData(filename), Is.EqualTo(TestUtils.GetArrayFromResourceStream(filename)));
        }*/

        /*[Test]
        public void TestDeleteLump()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));
            //hogFile.DeleteLump(1);
            hogFile.Lumps.RemoveAt(1);
            Assert.AreEqual(2, hogFile.NumLumps);

            // Other files should have been reassigned
            Assert.IsNotNull(hogFile.GetLumpHeader(0));
            Assert.AreEqual("test.hxm", hogFile.GetLumpHeader(0).Name);
            Assert.IsNotNull(hogFile.GetLumpHeader(1));
            Assert.AreEqual("test.rl2", hogFile.GetLumpHeader(1).Name);
            Assert.IsNull(hogFile.GetLumpHeader(2));
            Assert.AreEqual(-1, hogFile.GetLumpNum("test.pog"));

            // Check persistence
            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            hogFile.Read(memoryStream);
            Assert.AreEqual(2, hogFile.NumLumps);
            Assert.AreEqual("test.rl2", hogFile.Lumps[1].Name);
            Assert.AreEqual(-1, hogFile.GetLumpNum("test.pog"));
        }*/

        [Test]
        public void TestCreateNewHog()
        {
            var hogFile = new HOGFile();
            Assert.AreEqual(0, hogFile.NumLumps);

            var filename = "test.rdl";
            //hogFile.AddFile(filename, TestUtils.GetResourceStream(filename));
            HOGLump lump = new HOGLump(filename, TestUtils.GetArrayFromResourceStream(filename));
            hogFile.Lumps.Add(lump);
            Assert.AreEqual(1, hogFile.NumLumps);

            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            hogFile.Read(memoryStream);
            Assert.AreEqual(1, hogFile.NumLumps);

            //TODO: api change
            //Assert.IsNotNull(hogFile.GetLumpHeader(filename));
            //Assert.That(hogFile.GetLumpData(filename), Is.EqualTo(TestUtils.GetArrayFromResourceStream(filename)));
        }

        [Test]
        public void TestXLHogRead()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("d2x-xl.hog"));
            Assert.AreEqual(HOGFormat.D2X_XL, hogFile.Format);
        }

        [Test]
        public void TestXLHogLumpHeaders()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("d2x-xl.hog"));
            Assert.AreEqual(12, hogFile.NumLumps);

            // The .msg files use standard format headers; subsequent files use extended headers

            var lumpHeader = hogFile.Lumps[0];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.msg", lumpHeader.Name);
            Assert.AreEqual(0x14, lumpHeader.Offset);
            Assert.AreEqual(246, lumpHeader.Size);
            //Assert.AreEqual(LumpType.Text, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[1];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level2.msg", lumpHeader.Name);
            Assert.AreEqual(283, lumpHeader.Offset);
            Assert.AreEqual(246, lumpHeader.Size);
            //Assert.AreEqual(LumpType.Text, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[2];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level3.msg", lumpHeader.Name);
            Assert.AreEqual(546, lumpHeader.Offset);
            Assert.AreEqual(246, lumpHeader.Size);
            //Assert.AreEqual(LumpType.Text, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[3];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.rl2", lumpHeader.Name);
            Assert.AreEqual(1065, lumpHeader.Offset);
            Assert.AreEqual(75266, lumpHeader.Size);
            //Assert.AreEqual(LumpType.Level, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[4];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.lgt", lumpHeader.Name);
            Assert.AreEqual(76604, lumpHeader.Offset);
            Assert.AreEqual(3640, lumpHeader.Size);
            //Assert.AreEqual(LumpType.LGTMap, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[5];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.clr", lumpHeader.Name);
            Assert.AreEqual(80517, lumpHeader.Offset);
            Assert.AreEqual(11830, lumpHeader.Size);
            //Assert.AreEqual(LumpType.CLRMap, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[6];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level3.rl2", lumpHeader.Name);
            Assert.AreEqual(92620, lumpHeader.Offset);
            Assert.AreEqual(57744, lumpHeader.Size);
            //Assert.AreEqual(LumpType.Level, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[7];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level3.lgt", lumpHeader.Name);
            Assert.AreEqual(150637, lumpHeader.Offset);
            Assert.AreEqual(3640, lumpHeader.Size);
            //Assert.AreEqual(LumpType.LGTMap, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[8];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level3.clr", lumpHeader.Name);
            Assert.AreEqual(154550, lumpHeader.Offset);
            Assert.AreEqual(11830, lumpHeader.Size);
            //Assert.AreEqual(LumpType.CLRMap, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[9];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level2.rl2", lumpHeader.Name);
            Assert.AreEqual(166653, lumpHeader.Offset);
            Assert.AreEqual(109608, lumpHeader.Size);
            //Assert.AreEqual(LumpType.Level, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[10];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level2.lgt", lumpHeader.Name);
            Assert.AreEqual(276534, lumpHeader.Offset);
            Assert.AreEqual(3640, lumpHeader.Size);
            //Assert.AreEqual(LumpType.LGTMap, lumpHeader.Type);

            lumpHeader = hogFile.Lumps[11];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level2.clr", lumpHeader.Name);
            Assert.AreEqual(280447, lumpHeader.Offset);
            Assert.AreEqual(11830, lumpHeader.Size);
            //Assert.AreEqual(LumpType.CLRMap, lumpHeader.Type);
        }

        [Test]
        public void TestXLHogWrite()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("d2x-xl.hog"));
            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Read it back in and make sure the right things are still there
            hogFile.Read(memoryStream);
            Assert.AreEqual(12, hogFile.NumLumps);

            var lumpHeader = hogFile.Lumps[0];
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.msg", lumpHeader.Name);
            Assert.AreEqual(0x14, lumpHeader.Offset);
            Assert.AreEqual(246, lumpHeader.Size);
            //Assert.AreEqual(LumpType.Text, lumpHeader.Type);

            /*lumpHeader = hogFile.GetLumpHeader("level1.rl2");
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.rl2", lumpHeader.Name);
            // Moves because the new header is standard - not using long filename
            Assert.AreEqual(809, lumpHeader.Offset);
            Assert.AreEqual(75266, lumpHeader.Size);
            Assert.AreEqual(LumpType.Level, lumpHeader.Type);*/
        }

        /*[Test]
        public void TestXLHogAddLongFilename()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("d2x-xl.hog"));

            var filename = "testlongfilename.txt";
            var data = Encoding.ASCII.GetBytes("test");
            hogFile.AddLump(new HOGLump(filename, data));

            // Write to file
            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Read back in and make sure the filename is preserved
            hogFile.Read(memoryStream);
            Assert.AreEqual(13, hogFile.NumLumps);

            var lumpNum = hogFile.NumLumps - 1;
            var lumpHeader = hogFile.GetLumpHeader(lumpNum);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual(filename, lumpHeader.Name);
            Assert.AreEqual(data.Length, lumpHeader.Size);
            Assert.That(hogFile.GetLumpData(lumpNum), Is.EqualTo(data));
        }*/
    }
}
