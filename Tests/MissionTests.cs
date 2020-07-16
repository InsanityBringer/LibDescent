using LibDescent.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDescent.Tests
{
    [TestFixtureSource("TestData")]
    class MissionTests
    {
        private MissionFile msn;

        [SetUp]
        public void Setup()
        {
            msn = new MissionFile();
        }

        [Test]
        public void TestRead()
        {
            msn.Read(TestUtils.GetResourceStream("d2exit.mn2"));
            Assert.AreEqual("D2 exit test", msn.Name);
            Assert.AreEqual(MissionType.Normal, msn.Type);
            Assert.AreEqual(1, msn.Levels.Count);
            Assert.AreEqual("d2exit.rl2", msn.Levels[0]);
            Assert.AreEqual("DLE", msn.Metadata.Editor);
            Assert.AreEqual("this is not a comment", msn.Metadata.Comment.Trim());
        }

        [Test]
        public void TestWrite()
        {
            byte[] origData = TestUtils.GetArrayFromResourceStream("d2exit.mn2");
            msn.Read(origData);
            byte[] data = msn.Write();
            string origText = Encoding.ASCII.GetString(origData).Replace("\r", "");
            string text = Encoding.ASCII.GetString(data).Replace("\r", "");
            Assert.AreEqual(origText, text);
        }
    }
}
