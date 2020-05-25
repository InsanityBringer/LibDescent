using LibDescent.Data;
using NUnit.Framework;
using System.Collections;
using System.IO;

namespace LibDescent.Tests
{
    [TestFixtureSource("TestData")]
    class Rl2XLTests
    {
        private readonly D2XXLLevel level;

        public static IEnumerable TestData
        {
            get
            {
                // First case - test level (saved by DLE)
                var hogFile = new HOGFile(TestUtils.GetResourceStream("d2x-xl.hog"));
                var level = D2XXLLevel.CreateFromStream(hogFile.GetLumpAsStream("level3.rl2"));
                yield return new TestFixtureData(level);

                // Second case - output of D2XXLLevel.WriteToStream
                using (var stream = new MemoryStream())
                {
                    level.WriteToStream(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    level = D2XXLLevel.CreateFromStream(stream);
                }
                yield return new TestFixtureData(level);
            }
        }

        public Rl2XLTests(D2XXLLevel level)
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
