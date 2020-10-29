using LibDescent.Data;
using NUnit.Framework;

namespace LibDescent.Tests
{
    class MiscLevelLoadTests
    {
        [Test]
        public void TestAutoD1LevelLoad()
        {
            ILevel level = null;
            using (var stream = TestUtils.GetResourceStream("test.rdl"))
            {
                level = LevelFactory.CreateFromStream(stream);
            }
            Assert.NotNull(level);
            Assert.IsInstanceOf<D1Level>(level);
        }

        [Test]
        public void TestAutoD2LevelLoad()
        {
            ILevel level = null;
            using (var stream = TestUtils.GetResourceStream("test.rl2"))
            {
                level = LevelFactory.CreateFromStream(stream);
            }
            Assert.NotNull(level);
            Assert.IsInstanceOf<D2Level>(level);
            Assert.IsNotInstanceOf<D2XXLLevel>(level);
        }

        // Levels saved by DEVIL (and apparently beta versions of DMB) have some differences;
        // make sure we can load them properly
        [Test]
        public void TestLoadDevilLevel()
        {
            D1Level level;
            using (var stream = TestUtils.GetResourceStream("fusfrens.rdl"))
            {
                level = D1Level.CreateFromStream(stream);
            }
            Assert.NotNull(level);
        }

        [Test]
        public void TestAutoD2XXLLevelLoad()
        {
            var hogFile = new HOGFile(TestUtils.GetResourceStream("d2x-xl.hog"));
            //TODO: Magic number
            ILevel level = LevelFactory.CreateFromStream(hogFile.GetLumpAsStream(6));
            Assert.NotNull(level);
            Assert.IsInstanceOf<D2XXLLevel>(level);
        }
    }
}
