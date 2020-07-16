using LibDescent.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDescent.Tests
{
    [TestFixtureSource("TestData")]
    class SongListTests
    {
        private SongList sng;

        [SetUp]
        public void Setup()
        {
            sng = new SongList();
        }

        [Test]
        public void TestRead()
        {
            sng.Read(TestUtils.GetResourceStream("ulterior.sng"));
            Assert.AreEqual(28, sng.Songs.Count);
            Assert.AreEqual("descent.hmp", sng.Songs[0].Name);
            Assert.AreEqual("hammelo.bnk", sng.Songs[0].MelodicBank);
            Assert.AreEqual("hamdrum.bnk", sng.Songs[0].PercussionBank);
        }
    }
}
