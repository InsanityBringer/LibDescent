using LibDescent.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDescent.Tests
{
    [TestFixtureSource("TestData")]
    class PCXTests
    {
        private PCXImage pcx;

        [SetUp]
        public void Setup()
        {
            pcx = new PCXImage();
        }

        [Test]
        public void TestRead()
        {
            pcx.Read(TestUtils.GetResourceStream("carpet.pcx"));
            Assert.AreEqual(16, pcx.Width);
            Assert.AreEqual(16, pcx.Height);
            Assert.AreEqual(72, pcx.Hdpi);
            Assert.AreEqual(72, pcx.Vdpi);
            Assert.AreEqual(256, pcx.Data.Length);

            byte[] increasing = new byte[256];
            Color[] palette = new Color[256];

            for (int i = 0; i < 256; ++i)
            {
                increasing[i] = (byte)i;
                palette[i] = new Color(255, i, i, i);
            }

            Assert.AreEqual(increasing, pcx.Data);
            Assert.AreEqual(palette, pcx.Palette);
        }

        // TODO: more tests with more elaborate palettes?

        [Test]
        public void TestWrite()
        {
            pcx = new PCXImage(16, 16);
            for (int i = 0; i < pcx.Data.Length; ++i)
            {
                pcx.Data[i] = (byte)i;
                pcx.Palette[i] = new Color(255, i, i, i);
            }
            pcx.Hdpi = 72;
            pcx.Vdpi = 72;

            byte[] data;
            using (MemoryStream ms = new MemoryStream())
            {
                pcx.Write(ms, true);
                data = ms.ToArray();
            }

            Assert.AreEqual(TestUtils.GetArrayFromResourceStream("carpet.pcx"), data);
        }
    }
}
