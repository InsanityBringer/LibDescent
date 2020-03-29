using LibDescent.Data;
using NUnit.Framework;

namespace LibDescent.Tests
{
    class WallTests
    {
        private IBlock block;

        [SetUp]
        public void Setup()
        {
            var stream = GetType().Assembly.GetManifestResourceStream(GetType(), "test.blx");
            block = ExtendedBlock.CreateFromStream(stream);
        }

        [Test]
        public void TestWallProperties()
        {
            var wall = block.Segments[1].GetSide(SegSide.Right).Wall;
            Assert.IsNotNull(wall);
            Assert.AreEqual(WallType.Open, wall.Type);

            wall = block.Segments[3].GetSide(SegSide.Left).Wall;
            Assert.IsNotNull(wall);
            Assert.AreEqual(WallType.Door, wall.Type);
            Assert.AreEqual((Fix)0, wall.HitPoints);
            Assert.AreEqual(WallFlags.DoorLocked | WallFlags.DoorAuto, wall.Flags);
            Assert.AreEqual((WallState)0, wall.State);
            Assert.AreEqual(18, wall.DoorClipNumber);
            Assert.AreEqual(WallKeyFlags.None, wall.Keys);
            Assert.AreEqual(0, wall.CloakOpacity);
        }

        [Test]
        public void TestWallSideLinks()
        {
            // Open wall (trigger) on right side of segment 1
            var wall = block.Segments[1].GetSide(SegSide.Right).Wall;
            Assert.IsNotNull(wall);
            Assert.AreSame(block.Segments[1].GetSide(SegSide.Right), wall.Side);

            // Door on left side of segment 3
            wall = block.Segments[3].GetSide(SegSide.Left).Wall;
            Assert.IsNotNull(wall);
            Assert.AreSame(block.Segments[3].GetSide(SegSide.Left), wall.Side);
        }

        [Test]
        public void TestOppositeWall()
        {
            // Open wall (trigger) on right side of segment 1
            var wall = block.Segments[1].GetSide(SegSide.Right).Wall;
            Assert.IsNotNull(wall);
            Assert.IsNull(wall.OppositeWall);

            // Door on left side of segment 3 - opposite side is in segment 7
            wall = block.Segments[3].GetSide(SegSide.Left).Wall;
            Assert.IsNotNull(wall);
            Assert.IsNotNull(wall.OppositeWall);
            Assert.AreSame(block.Segments[7].GetSide(SegSide.Right).Wall, wall.OppositeWall);
        }
    }
}
