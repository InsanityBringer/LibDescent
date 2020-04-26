using LibDescent.Data;
using NUnit.Framework;
using System.Collections;

namespace LibDescent.Tests
{
    [TestFixtureSource("TestData")]
    class Rl2XLTests
    {
        private readonly D2Level level;

        public static IEnumerable TestData
        {
            get
            {
                // First case - test level (saved by DLE)
                var hogFile = new HOGFile(TestUtils.GetResourceStream("d2x-xl.hog"));
                var level = D2Level.CreateFromStream(hogFile.GetLumpAsStream("level3.rl2"));
                yield return new TestFixtureData(level);
            }
        }

        public Rl2XLTests(D2Level level)
        {
            this.level = level;
        }

        [Test]
        public void TestLoadLevelSucceeds()
        {
            // Level already loaded by Setup, just check it's there
            Assert.NotNull(level);
        }
    }
}
