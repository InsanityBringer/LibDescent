using LibDescent.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDescent.Tests
{
    [TestFixtureSource("TestData")]
    class BBMTests
    {
        private BBMImage bbm;

        [SetUp]
        public void Setup()
        {
            bbm = new BBMImage();
        }

        [Test]
        public void TestReadSimple()
        {
            bbm.Read(TestUtils.GetResourceStream("carpet.bbm"));
            Assert.AreEqual(16, bbm.Width);
            Assert.AreEqual(16, bbm.Height);
            Assert.AreEqual(256, bbm.Data.Length);
            Assert.AreEqual(BBMType.PBM, bbm.Type);

            byte[] increasing = new byte[256];
            Color[] palette = new Color[256];

            for (int i = 0; i < 256; ++i)
            {
                increasing[i] = (byte)i;
                palette[i] = new Color(255, i, i, i);
            }

            Assert.AreEqual(increasing, bbm.Data);
            Assert.AreEqual(palette, bbm.Palette);
        }

        // TODO: do we want to test (reading) RLE compressed PBM files and ILBM files?

        [Test]
        public void TestWrite()
        {
            bbm = new BBMImage(16, 16);
            for (int i = 0; i < bbm.Data.Length; ++i)
            {
                bbm.Data[i] = (byte)i;
                bbm.Palette[i] = new Color(255, i, i, i);
            }
            bbm.Mask = 0;

            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                bbm.Write(ms);
                data = ms.ToArray();
            }

            Assert.AreEqual(TestUtils.GetArrayFromResourceStream("carpet.bbm"), data);
        }
    }
}
