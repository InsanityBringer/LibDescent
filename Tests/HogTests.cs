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
            var lumpHeader = hogFile.GetLumpHeader(0);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("test.hxm", lumpHeader.name);
            Assert.AreEqual(0x14, lumpHeader.offset);
            Assert.AreEqual(32530, lumpHeader.size);
            Assert.AreEqual(LumpType.HXMFile, lumpHeader.type);

            // Second lump - .pog
            lumpHeader = hogFile.GetLumpHeader(1);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("test.pog", lumpHeader.name);
            Assert.AreEqual(32567, lumpHeader.offset);
            Assert.AreEqual(4128, lumpHeader.size);
            Assert.AreEqual(LumpType.Unknown, lumpHeader.type);

            // Third lump - .rl2
            lumpHeader = hogFile.GetLumpHeader(2);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("test.rl2", lumpHeader.name);
            Assert.AreEqual(36712, lumpHeader.offset);
            Assert.AreEqual(7010, lumpHeader.size);
            Assert.AreEqual(LumpType.Level, lumpHeader.type);
        }

        [Test]
        public void TestLumpData()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));
            var lumpHeader = hogFile.GetLumpHeader(2);

            var lumpData = hogFile.GetLumpData(2);
            Assert.NotNull(lumpData);
            Assert.AreEqual(lumpHeader.size, lumpData.Length);

            var lumpStream = hogFile.GetLumpAsStream(2);
            Assert.NotNull(lumpStream);
            Assert.AreEqual(lumpHeader.size, lumpStream.Length);

            // One last thing... try reading the stream
            var level = LevelFactory.CreateFromStream(lumpStream);
            Assert.NotNull(level);
            Assert.AreEqual(20, level.Segments.Count);
        }

        [Test]
        public void TestGetLumpByName()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));

            var lumpNum = hogFile.GetLumpNum("test.pog");
            Assert.AreEqual(1, lumpNum);

            var lumpHeader = hogFile.GetLumpHeader("test.pog");
            Assert.NotNull(lumpHeader);
            Assert.AreSame(hogFile.GetLumpHeader(lumpNum), lumpHeader);

            var lumpData = hogFile.GetLumpData("test.pog");
            Assert.NotNull(lumpData);
            Assert.AreEqual(lumpHeader.size, lumpData.Length);

            var lumpStream = hogFile.GetLumpAsStream("test.pog");
            Assert.NotNull(lumpStream);
            Assert.AreEqual(lumpHeader.size, lumpStream.Length);
        }

        [Test]
        public void TestGetLumpsByType()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("d2x-xl.hog"));

            var lumps = hogFile.GetLumpsByType(LumpType.Level);
            Assert.NotNull(lumps);
            Assert.AreEqual(3, lumps.Count);

            foreach (var lump in lumps)
            {
                Assert.AreEqual(LumpType.Level, lump.type);
            }
        }

        [Test]
        public void TestAddLump()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));

            // Method 1 - set up lump manually
            var file1Data = TestUtils.GetArrayFromResourceStream("test.rdl");
            var hogLump = new HOGLump("test.rdl", file1Data.Length, 0);
            hogLump.data = file1Data;
            hogFile.AddLump(hogLump);
            Assert.AreEqual(4, hogFile.NumLumps);
            Assert.AreEqual(3, hogFile.GetLumpNum("test.rdl"));

            // Method 2 - use constructor
            var file2Data = TestUtils.GetArrayFromResourceStream("test.rl2");
            hogLump = new HOGLump("test2.rl2", file2Data);
            hogFile.AddLump(hogLump);
            Assert.AreEqual(5, hogFile.NumLumps);
            Assert.AreEqual(4, hogFile.GetLumpNum("test2.rl2"));

            // Make sure it writes correctly
            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            hogFile.Read(memoryStream);
            Assert.AreEqual(5, hogFile.NumLumps);
            Assert.AreEqual(3, hogFile.GetLumpNum("test.rdl"));
            Assert.That(hogFile.GetLumpData(3), Is.EqualTo(file1Data));
            Assert.AreEqual(4, hogFile.GetLumpNum("test2.rl2"));
            Assert.That(hogFile.GetLumpData(4), Is.EqualTo(file2Data));
        }

        [Test]
        public void TestAddFile()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));
            var filename = "test.rdl";
            hogFile.AddFile(filename, TestUtils.GetResourceStream(filename));

            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            hogFile.Read(memoryStream);
            Assert.AreEqual(4, hogFile.NumLumps);
            Assert.AreEqual(3, hogFile.GetLumpNum(filename));
            Assert.That(hogFile.GetLumpData(filename), Is.EqualTo(TestUtils.GetArrayFromResourceStream(filename)));
        }

        [Test]
        public void TestDeleteLump()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("standard.hog"));
            hogFile.DeleteLump(1);
            Assert.AreEqual(2, hogFile.NumLumps);

            // Other files should have been reassigned
            Assert.IsNotNull(hogFile.GetLumpHeader(0));
            Assert.AreEqual("test.hxm", hogFile.GetLumpHeader(0).name);
            Assert.IsNotNull(hogFile.GetLumpHeader(1));
            Assert.AreEqual("test.rl2", hogFile.GetLumpHeader(1).name);
            Assert.IsNull(hogFile.GetLumpHeader(2));
            Assert.AreEqual(-1, hogFile.GetLumpNum("test.pog"));

            // Check persistence
            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            hogFile.Read(memoryStream);
            Assert.AreEqual(2, hogFile.NumLumps);
            Assert.AreEqual("test.rl2", hogFile.GetLumpHeader(1).name);
            Assert.AreEqual(-1, hogFile.GetLumpNum("test.pog"));
        }

        [Test]
        public void TestCreateNewHog()
        {
            var hogFile = new HOGFile();
            Assert.AreEqual(0, hogFile.NumLumps);

            var filename = "test.rdl";
            hogFile.AddFile(filename, TestUtils.GetResourceStream(filename));
            Assert.AreEqual(1, hogFile.NumLumps);

            var memoryStream = new MemoryStream();
            hogFile.Write(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            hogFile.Read(memoryStream);
            Assert.AreEqual(1, hogFile.NumLumps);
            Assert.IsNotNull(hogFile.GetLumpHeader(filename));
            Assert.That(hogFile.GetLumpData(filename), Is.EqualTo(TestUtils.GetArrayFromResourceStream(filename)));
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

            var lumpHeader = hogFile.GetLumpHeader(0);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.msg", lumpHeader.name);
            Assert.AreEqual(0x14, lumpHeader.offset);
            Assert.AreEqual(246, lumpHeader.size);
            Assert.AreEqual(LumpType.Text, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(1);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level2.msg", lumpHeader.name);
            Assert.AreEqual(283, lumpHeader.offset);
            Assert.AreEqual(246, lumpHeader.size);
            Assert.AreEqual(LumpType.Text, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(2);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level3.msg", lumpHeader.name);
            Assert.AreEqual(546, lumpHeader.offset);
            Assert.AreEqual(246, lumpHeader.size);
            Assert.AreEqual(LumpType.Text, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(3);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.rl2", lumpHeader.name);
            Assert.AreEqual(1065, lumpHeader.offset);
            Assert.AreEqual(75266, lumpHeader.size);
            Assert.AreEqual(LumpType.Level, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(4);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.lgt", lumpHeader.name);
            Assert.AreEqual(76604, lumpHeader.offset);
            Assert.AreEqual(3640, lumpHeader.size);
            Assert.AreEqual(LumpType.Unknown, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(5);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.clr", lumpHeader.name);
            Assert.AreEqual(80517, lumpHeader.offset);
            Assert.AreEqual(11830, lumpHeader.size);
            Assert.AreEqual(LumpType.Unknown, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(6);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level3.rl2", lumpHeader.name);
            Assert.AreEqual(92620, lumpHeader.offset);
            Assert.AreEqual(57744, lumpHeader.size);
            Assert.AreEqual(LumpType.Level, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(7);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level3.lgt", lumpHeader.name);
            Assert.AreEqual(150637, lumpHeader.offset);
            Assert.AreEqual(3640, lumpHeader.size);
            Assert.AreEqual(LumpType.Unknown, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(8);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level3.clr", lumpHeader.name);
            Assert.AreEqual(154550, lumpHeader.offset);
            Assert.AreEqual(11830, lumpHeader.size);
            Assert.AreEqual(LumpType.Unknown, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(9);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level2.rl2", lumpHeader.name);
            Assert.AreEqual(166653, lumpHeader.offset);
            Assert.AreEqual(109608, lumpHeader.size);
            Assert.AreEqual(LumpType.Level, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(10);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level2.lgt", lumpHeader.name);
            Assert.AreEqual(276534, lumpHeader.offset);
            Assert.AreEqual(3640, lumpHeader.size);
            Assert.AreEqual(LumpType.Unknown, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader(11);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level2.clr", lumpHeader.name);
            Assert.AreEqual(280447, lumpHeader.offset);
            Assert.AreEqual(11830, lumpHeader.size);
            Assert.AreEqual(LumpType.Unknown, lumpHeader.type);
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

            var lumpHeader = hogFile.GetLumpHeader(0);
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.msg", lumpHeader.name);
            Assert.AreEqual(0x14, lumpHeader.offset);
            Assert.AreEqual(246, lumpHeader.size);
            Assert.AreEqual(LumpType.Text, lumpHeader.type);

            lumpHeader = hogFile.GetLumpHeader("level1.rl2");
            Assert.NotNull(lumpHeader);
            Assert.AreEqual("level1.rl2", lumpHeader.name);
            // Moves because the new header is standard - not using long filename
            Assert.AreEqual(809, lumpHeader.offset);
            Assert.AreEqual(75266, lumpHeader.size);
            Assert.AreEqual(LumpType.Level, lumpHeader.type);
        }

        [Test]
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
            Assert.AreEqual(filename, lumpHeader.name);
            Assert.AreEqual(data.Length, lumpHeader.size);
            Assert.That(hogFile.GetLumpData(lumpNum), Is.EqualTo(data));
        }
    }
}
