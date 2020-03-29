using LibDescent.Data;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace LibDescent.Tests
{
    class BlockTests
    {
        private IBlock blkBlock;
        private IBlock blxBlock;

        [SetUp]
        public void Setup()
        {
            var blkStream = GetType().Assembly.GetManifestResourceStream(GetType(), "test.blk");
            blkBlock = Block.CreateFromStream(blkStream);

            var blxStream = GetType().Assembly.GetManifestResourceStream(GetType(), "test.blx");
            blxBlock = ExtendedBlock.CreateFromStream(blxStream);
        }

        [Test]
        public void TestBlockSegments()
        {
            IBlock[] blockFormats = { blkBlock, blxBlock };

            foreach (var block in blockFormats)
            {
                Assert.AreEqual(9, block.Segments.Count);
                Assert.IsNull(block.Segments[0].Sides[0].ConnectedSegment);
                Assert.AreSame(block.Segments[1], block.Segments[0].Sides[4].ConnectedSegment);
            }
        }

        [Test]
        public void TestBlockVertexLocations()
        {
            IBlock[] blockFormats = { blkBlock, blxBlock };

            foreach (var block in blockFormats)
            {
                Assert.AreEqual(new FixVector(0, 20, 0), block.Segments[0].Vertices[0].Location);
                Assert.AreEqual(new FixVector(20, -20, -60), block.Segments[7].Vertices[3].Location);
            }
        }

        [Test]
        public void TestBlockVertexMerging()
        {
            IBlock[] blockFormats = { blkBlock, blxBlock };

            foreach (var block in blockFormats)
            {
                Assert.AreEqual(36, block.GetVertexCount());
                Assert.AreSame(block.Segments[0].Vertices[7], block.Segments[1].Vertices[3]);
                Assert.AreSame(block.Segments[0].Vertices[7], block.Segments[3].Vertices[0]);
                Assert.AreEqual(3, block.Segments[0].Vertices[7].ConnectedSegments.Count);
                Assert.AreEqual(9, block.Segments[0].Vertices[7].ConnectedSides.Count);
            }
        }

        [Test]
        public void TestBlockTextures()
        {
            IBlock[] blockFormats = { blkBlock, blxBlock };

            foreach (var block in blockFormats)
            {
                Assert.AreEqual(42, block.Segments[2].Sides[4].BaseTextureIndex); // rock160
                Assert.AreEqual(new Uvl(0, 0, 0.582275390625), block.Segments[2].Sides[4].Uvls[0]);
                Assert.AreEqual(new Uvl(0, 1, 0.59796142578125), block.Segments[2].Sides[4].Uvls[1]);
                Assert.AreEqual(new Uvl(-1, 1, 0.715576171875), block.Segments[2].Sides[4].Uvls[2]);
                Assert.AreEqual(new Uvl(-1, 0, 0.8023681640625), block.Segments[2].Sides[4].Uvls[3]);

                Assert.AreEqual(219, block.Segments[0].Sides[2].BaseTextureIndex); // metl046 (rotated)
                Assert.AreEqual(new Uvl(0, 0, 0.836212158203125), block.Segments[0].Sides[2].Uvls[0]);
                Assert.AreEqual(new Uvl(1, 0, 0.823486328125), block.Segments[0].Sides[2].Uvls[1]);
                Assert.AreEqual(new Uvl(1, 1, 0.326934814453125), block.Segments[0].Sides[2].Uvls[2]);
                Assert.AreEqual(new Uvl(0, 1, 0.32733154296875), block.Segments[0].Sides[2].Uvls[3]);

                Assert.AreEqual(0, block.Segments[1].Sides[0].BaseTextureIndex); // rock021 (no wall)
                Assert.AreEqual(new Uvl(0, 0, 0.245361328125), block.Segments[1].Sides[0].Uvls[0]);
                Assert.AreEqual(new Uvl(0, 0, 0.24090576171875), block.Segments[1].Sides[0].Uvls[1]);
                Assert.AreEqual(new Uvl(0, 0, 0.313140869140625), block.Segments[1].Sides[0].Uvls[2]);
                Assert.AreEqual(new Uvl(0, 0, 0.319091796875), block.Segments[1].Sides[0].Uvls[3]);
            }
        }

        [Test]
        public void TestBlockSecondaryTextures()
        {
            IBlock[] blockFormats = { blkBlock, blxBlock };

            foreach (var block in blockFormats)
            {
                Assert.AreEqual(341, block.Segments[2].Sides[4].OverlayTextureIndex); // misc086 (rotated 180 degrees)
                Assert.AreEqual(OverlayRotation.Rotate180, block.Segments[2].Sides[4].OverlayRotation);

                Assert.AreEqual(268, block.Segments[0].Sides[2].OverlayTextureIndex); // metl153
                Assert.AreEqual(OverlayRotation.Rotate0, block.Segments[0].Sides[2].OverlayRotation);

                Assert.AreEqual(0, block.Segments[1].Sides[0].OverlayTextureIndex); // rock021 (no wall)
                Assert.AreEqual(OverlayRotation.Rotate0, block.Segments[1].Sides[0].OverlayRotation);
            }
        }

        [Test]
        public void TestStandardBlockSegmentLightValues()
        {
            IBlock[] blockFormats = { blkBlock, blxBlock };

            foreach (var block in blockFormats)
            {
                Assert.AreEqual(7.186, block.Segments[0].Light, 0.001);
                Assert.AreEqual(3.243, block.Segments[1].Light, 0.001);
                Assert.AreEqual(0.000, block.Segments[7].Light, 0.001);
            }
        }

        [Test]
        public void TestStandardBlockWallsEmpty()
        {
            Assert.IsEmpty(blkBlock.Walls);
        }

        [Test]
        public void TestStandardBlockTriggersEmpty()
        {
            Assert.IsEmpty(blkBlock.Triggers);
        }

        [Test]
        public void TestStandardBlockAnimatedLightsEmpty()
        {
            Assert.IsEmpty(blkBlock.AnimatedLights);
        }

        [Test]
        public void TestStandardBlockWrite()
        {
            var stream = new MemoryStream();
            Assert.DoesNotThrow(() => blkBlock.WriteToStream(stream));

            var originalFileContents = TestUtils.GetArrayFromResourceStream("test.blk");
            var resultingFileContents = stream.ToArray();
            Assert.IsTrue(Enumerable.SequenceEqual(originalFileContents, resultingFileContents));
        }

        [Test]
        public void TestExtendedBlockWalls()
        {
            Assert.AreEqual(7, blxBlock.Walls.Count);

            // Open wall (trigger) on right side of segment 1
            Assert.IsNotNull(blxBlock.Segments[1].GetSide(SegSide.Right).Wall);
            var wall = blxBlock.Segments[1].GetSide(SegSide.Right).Wall;
            Assert.Contains(wall, blxBlock.Walls);

            // Opposite side (left side of segment 5) has no wall
            Assert.IsNull(blxBlock.Segments[5].GetSide(SegSide.Left).Wall);

            // Door on left side of segment 3 - opposite side is in segment 7
            Assert.IsNotNull(blxBlock.Segments[3].GetSide(SegSide.Left).Wall);
            wall = blxBlock.Segments[3].GetSide(SegSide.Left).Wall;
            Assert.Contains(wall, blxBlock.Walls);
        }

        [Test]
        public void TestExtendedBlockTriggers()
        {
            Assert.AreEqual(2, blxBlock.Triggers.Count);

            // Check trigger is present and attached to correct face
            var trigger = blxBlock.Triggers[0];
            Assert.IsNotNull(blxBlock.Segments[1].GetSide(SegSide.Right).Wall.Trigger);
            Assert.AreSame(trigger, blxBlock.Segments[1].GetSide(SegSide.Right).Wall.Trigger);
        }

        [Test]
        public void TestExtendedBlockAnimatedLights()
        {
            Assert.AreEqual(1, blxBlock.AnimatedLights.Count);

            var light = blxBlock.AnimatedLights[0];
            Assert.IsNotNull(blxBlock.Segments[0].GetSide(SegSide.Front).AnimatedLight);
            Assert.AreSame(light, blxBlock.Segments[0].GetSide(SegSide.Front).AnimatedLight);
            Assert.AreSame(blxBlock.Segments[0].GetSide(SegSide.Front), light.Side);
            Assert.AreEqual(0x7B23C0FF, light.Mask);
            Assert.AreEqual(0.1, light.TickLength, 0.001);
            Assert.AreEqual(0.25, light.TimeToNextTick, 0.001);
        }

        [Test]
        public void TestExtendedBlockMatcens()
        {
            Assert.AreEqual(1, blxBlock.MatCenters.Count);

            var matcen = blxBlock.MatCenters[0];
            Assert.AreSame(matcen, blxBlock.Segments[8].MatCenter);
        }

        [Test]
        public void TestExtendedBlockWriteNotImplemented()
        {
            var stream = new MemoryStream();
            Assert.Throws<NotImplementedException>(() => blxBlock.WriteToStream(stream));
        }
    }
}
