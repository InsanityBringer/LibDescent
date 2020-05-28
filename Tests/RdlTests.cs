using LibDescent.Data;
using NUnit.Framework;
using System.Collections;
using System.IO;

namespace LibDescent.Tests
{
    [TestFixtureSource("TestData")]
    class RdlTests
    {
        private readonly D1Level level;

        public static IEnumerable TestData
        {
            get
            {
                // First case - test level (saved by DLE)
                D1Level level;
                using (var stream = TestUtils.GetResourceStream("test.rdl"))
                {
                    level = D1Level.CreateFromStream(stream);
                }
                yield return new TestFixtureData(level);

                // Second case - output of D1Level.WriteToStream
                using (var stream = new MemoryStream())
                {
                    level.WriteToStream(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    level = D1Level.CreateFromStream(stream);
                }
                yield return new TestFixtureData(level);

                // This can be used to debug specific cases (e.g. copyrighted levels).
                // Level must be placed in the working directory of the test.
                //using (var stream = new FileStream("level01.rdl", FileMode.Open, FileAccess.Read))
                //{
                //    level = D1Level.CreateFromStream(stream);
                //}
                //yield return new TestFixtureData(level);
            }
        }

        public RdlTests(D1Level level)
        {
            this.level = level;
        }

        [Test]
        public void TestLoadLevelSucceeds()
        {
            // Level already loaded by Setup, just check it's there
            Assert.NotNull(level);
        }

        [Test]
        public void TestLevelName()
        {
            Assert.AreEqual("test", level.LevelName);
        }

        [Test]
        public void TestVertices()
        {
            Assert.AreEqual(68, level.Vertices.Count);

            // Vertex 0
            Assert.AreEqual(new FixVector(10d, 0d, -80d), level.Vertices[0].Location);
            Assert.AreEqual(1, level.Vertices[0].ConnectedSegments.Count);
            var connectedSegments = level.Vertices[0].ConnectedSegments;
            Assert.AreEqual(0, level.Segments.IndexOf(connectedSegments[0].segment));
            Assert.AreEqual(0, connectedSegments[0].vertexNum);
            Assert.AreEqual(3, level.Vertices[0].ConnectedSides.Count);

            // Vertex 30
            Assert.AreEqual(new FixVector(70d, 0d, 0d), level.Vertices[30].Location);
            Assert.AreEqual(3, level.Vertices[30].ConnectedSegments.Count);
            Assert.AreEqual(9, level.Vertices[30].ConnectedSides.Count);
            connectedSegments = level.Vertices[30].ConnectedSegments;
            Assert.AreEqual(7, level.Segments.IndexOf(connectedSegments[0].segment));
            Assert.AreEqual(8, level.Segments.IndexOf(connectedSegments[1].segment));
            Assert.AreEqual(15, level.Segments.IndexOf(connectedSegments[2].segment));

            // Vertex 57
            Assert.AreEqual(new FixVector(50d, -20d, 60d), level.Vertices[57].Location);
            Assert.AreEqual(1, level.Vertices[57].ConnectedSegments.Count);
            Assert.AreEqual(3, level.Vertices[57].ConnectedSides.Count);
            connectedSegments = level.Vertices[57].ConnectedSegments;
            Assert.AreEqual(14, level.Segments.IndexOf(connectedSegments[0].segment));

            // Vertex 67
            Assert.AreEqual(new FixVector(-50d, 20d, 60d), level.Vertices[67].Location);
            Assert.AreEqual(1, level.Vertices[67].ConnectedSegments.Count);
            connectedSegments = level.Vertices[67].ConnectedSegments;
            Assert.AreEqual(17, level.Segments.IndexOf(connectedSegments[0].segment));
            Assert.AreEqual(7, connectedSegments[0].vertexNum);
            Assert.AreEqual(3, level.Vertices[67].ConnectedSides.Count);
            var connectedSides = level.Vertices[67].ConnectedSides;
            Assert.AreEqual(level.Segments[17].GetSide(SegSide.Left), connectedSides[0].side);
            Assert.AreEqual(0, connectedSides[0].vertexNum);
            Assert.AreEqual(level.Segments[17].GetSide(SegSide.Up), connectedSides[1].side);
            Assert.AreEqual(2, connectedSides[1].vertexNum);
            Assert.AreEqual(level.Segments[17].GetSide(SegSide.Back), connectedSides[2].side);
            Assert.AreEqual(3, connectedSides[2].vertexNum);
        }

        [Test]
        public void TestSegments()
        {
            Assert.AreEqual(18, level.Segments.Count);

            Assert.AreSame(level.Vertices[0], level.Segments[0].Vertices[0]);
            Assert.AreEqual(SegFunction.None, level.Segments[0].Function);
            Assert.AreEqual(9.938d, level.Segments[0].Light, 0.001);

            Assert.AreEqual(SegFunction.FuelCenter, level.Segments[13].Function);
            Assert.AreEqual(12.0d, level.Segments[13].Light, 0.001);

            Assert.AreEqual(SegFunction.Reactor, level.Segments[14].Function);
            Assert.AreEqual(6.115d, level.Segments[14].Light, 0.001);
        }

        [Test]
        public void TestEndOfExitTunnel()
        {
            // Exit tunnel side - true
            Assert.IsTrue(level.Segments[17].GetSide(SegSide.Back).Exit);
            // Should not read texture data
            Assert.AreEqual(0, level.Segments[17].GetSide(SegSide.Back).BaseTextureIndex);

            // Joined side - false
            Assert.IsFalse(level.Segments[0].GetSide(SegSide.Back).Exit);

            // Unjoined side - false
            Assert.IsFalse(level.Segments[17].GetSide(SegSide.Left).Exit);
        }

        [Test]
        public void TestSideTextures()
        {
            // Segment 0 side 4 is joined, no textures
            Assert.AreEqual(0, level.Segments[0].Sides[4].BaseTextureIndex);
            Assert.AreEqual(0, level.Segments[0].Sides[4].OverlayTextureIndex);

            // Segment 11 side 0 has one texture
            Assert.AreEqual(19, level.Segments[11].Sides[0].BaseTextureIndex);
            Assert.AreEqual(0, level.Segments[11].Sides[0].OverlayTextureIndex);

            // Segment 3 side 4 has two textures
            Assert.AreEqual(109, level.Segments[3].Sides[4].BaseTextureIndex);
            Assert.AreEqual(255, level.Segments[3].Sides[4].OverlayTextureIndex);
            Assert.AreEqual(OverlayRotation.Rotate0, level.Segments[3].Sides[4].OverlayRotation);

            // Segment 10 side 0 has a flipped overlay texture
            Assert.AreEqual(100, level.Segments[10].Sides[0].BaseTextureIndex);
            Assert.AreEqual(250, level.Segments[10].Sides[0].OverlayTextureIndex);
            Assert.AreEqual(OverlayRotation.Rotate180, level.Segments[10].Sides[0].OverlayRotation);
        }

        [Test]
        public void TestSideUVLs()
        {
            Assert.That(level.Segments[3].Sides[4].Uvls[0].ToDoubles(), Is.EqualTo((0, 0, 1.0)).Within(0.001));
            Assert.That(level.Segments[3].Sides[4].Uvls[1].ToDoubles(), Is.EqualTo((0, 1, 0.7764)).Within(0.001));
            Assert.That(level.Segments[3].Sides[4].Uvls[2].ToDoubles(), Is.EqualTo((-1, 1, 1.0)).Within(0.001));
            Assert.That(level.Segments[3].Sides[4].Uvls[3].ToDoubles(), Is.EqualTo((-1, 0, 1.0)).Within(0.001));

            Assert.That(level.Segments[10].Sides[3].Uvls[0].ToDoubles(), Is.EqualTo((0, 0, 1.0)).Within(0.001));
            Assert.That(level.Segments[10].Sides[3].Uvls[1].ToDoubles(), Is.EqualTo((-1, 1, 1.0)).Within(0.001));
            Assert.That(level.Segments[10].Sides[3].Uvls[2].ToDoubles(), Is.EqualTo((-2, 1, 1.0)).Within(0.001));
            Assert.That(level.Segments[10].Sides[3].Uvls[3].ToDoubles(), Is.EqualTo((-3, 0, 1.0)).Within(0.001));
        }

        [Test]
        public void TestObjects()
        {
            Assert.AreEqual(5, level.Objects.Count);

            // Player
            Assert.AreEqual(ObjectType.Player, level.Objects[0].Type);
            Assert.AreEqual(0, level.Objects[0].SubtypeID);
            Assert.AreEqual(MovementTypeID.Physics, level.Objects[0].MoveTypeID);
            Assert.AreEqual(ControlTypeID.Slew, level.Objects[0].ControlTypeID);
            Assert.AreEqual(RenderTypeID.Polyobj, level.Objects[0].RenderTypeID);
            Assert.AreEqual(new FixVector(0, -10, -70), level.Objects[0].Position);
            // Point directly forward
            var expectedOrientation = new FixMatrix(new FixVector(1, 0, 0), new FixVector(0, 1, 0), new FixVector(0, 0, 1));
            Assert.AreEqual(expectedOrientation, level.Objects[0].Orientation);
            Assert.IsTrue(level.Objects[0].RenderType is PolymodelRenderType);
            Assert.AreEqual(43, ((PolymodelRenderType)level.Objects[0].RenderType).ModelNum);
            Assert.AreEqual(0, level.Objects[0].Segnum);

            // Powerup
            Assert.AreEqual(ObjectType.Powerup, level.Objects[1].Type);
            Assert.AreEqual(5, level.Objects[1].SubtypeID); // red key
            Assert.AreEqual(MovementTypeID.None, level.Objects[1].MoveTypeID);
            Assert.AreEqual(ControlTypeID.Powerup, level.Objects[1].ControlTypeID);
            Assert.AreEqual(RenderTypeID.Powerup, level.Objects[1].RenderTypeID);
            Assert.AreEqual(new FixVector(-80, -10, 10), level.Objects[1].Position);
            Assert.AreEqual(expectedOrientation, level.Objects[1].Orientation);
            Assert.AreEqual(11, level.Objects[1].Segnum);

            // Reactor
            Assert.AreEqual(ObjectType.ControlCenter, level.Objects[2].Type);
            Assert.AreEqual(0, level.Objects[2].SubtypeID);
            Assert.AreEqual(MovementTypeID.None, level.Objects[2].MoveTypeID);
            Assert.AreEqual(ControlTypeID.ControlCenter, level.Objects[2].ControlTypeID);
            Assert.AreEqual(RenderTypeID.Polyobj, level.Objects[2].RenderTypeID);
            Assert.AreEqual(new FixVector(40, -10, 50), level.Objects[2].Position);
            Assert.AreEqual(expectedOrientation, level.Objects[2].Orientation);
            Assert.IsTrue(level.Objects[2].RenderType is PolymodelRenderType);
            Assert.AreEqual(39, ((PolymodelRenderType)level.Objects[2].RenderType).ModelNum);
            Assert.AreEqual(14, level.Objects[2].Segnum);

            // Hostage
            Assert.AreEqual(ObjectType.Hostage, level.Objects[3].Type);
            Assert.AreEqual(0, level.Objects[3].SubtypeID);
            Assert.AreEqual(MovementTypeID.None, level.Objects[3].MoveTypeID);
            Assert.AreEqual(ControlTypeID.Powerup, level.Objects[3].ControlTypeID);
            Assert.AreEqual(RenderTypeID.Hostage, level.Objects[3].RenderTypeID);
            Assert.AreEqual(new FixVector(80, -15, 10), level.Objects[3].Position);
            Assert.AreEqual(expectedOrientation, level.Objects[3].Orientation);
            Assert.IsTrue(level.Objects[3].RenderType is HostageRenderType);
            Assert.AreEqual(33, ((HostageRenderType)level.Objects[3].RenderType).VClipNum);
            Assert.AreEqual(15, level.Objects[3].Segnum);

            // Robot
            Assert.AreEqual(ObjectType.Robot, level.Objects[4].Type);
            Assert.AreEqual(0, level.Objects[4].SubtypeID); // medium hulk
            Assert.AreEqual(MovementTypeID.Physics, level.Objects[4].MoveTypeID);
            Assert.AreEqual(ControlTypeID.AI, level.Objects[4].ControlTypeID);
            Assert.AreEqual(RenderTypeID.Polyobj, level.Objects[4].RenderTypeID);
            Assert.AreEqual(new FixVector(40, -10, -30), level.Objects[4].Position);
            Assert.AreEqual(expectedOrientation, level.Objects[4].Orientation);
            // Physics info seems to be blank, probably initialized from HAM/HXM data
            Assert.AreEqual((Fix)0, ((PhysicsMoveType)level.Objects[4].MoveType).Mass);
            Assert.AreEqual((Fix)0, ((PhysicsMoveType)level.Objects[4].MoveType).Drag);
            Assert.AreEqual((PhysicsFlags)0, ((PhysicsMoveType)level.Objects[4].MoveType).Flags);
            Assert.AreEqual((Fix)100, level.Objects[4].Shields);
            Assert.AreEqual((ObjectType)7, level.Objects[4].ContainsType); // powerup
            Assert.AreEqual(1, level.Objects[4].ContainsCount);
            Assert.AreEqual(11, level.Objects[4].ContainsId); // 4-pack conc
            Assert.AreEqual(0, ((PolymodelRenderType)level.Objects[4].RenderType).ModelNum);
            Assert.AreEqual(16, level.Objects[4].Segnum);
        }

        [Test]
        public void TestWalls()
        {
            Assert.AreEqual(11, level.Walls.Count);

            // Wall 0 is the exit door
            Assert.AreSame(level.Walls[0], level.Segments[10].GetSide(SegSide.Back).Wall);
            Assert.AreSame(level.Segments[10].GetSide(SegSide.Back), level.Walls[0].Side);
            Assert.AreSame(level.Walls[1], level.Walls[0].OppositeWall);
            Assert.AreSame(level.Walls[0], level.Walls[1].OppositeWall);
            Assert.AreEqual(WallType.Door, level.Walls[0].Type);
            Assert.AreEqual(WallFlags.DoorLocked, level.Walls[0].Flags);
            Assert.AreEqual(WallState.DoorClosed, level.Walls[0].State);
            Assert.AreEqual(WallKeyFlags.None, level.Walls[0].Keys);

            // Wall 2 is an energy center sparkle effect
            Assert.AreEqual(WallType.Illusion, level.Walls[2].Type);
            Assert.AreEqual((WallFlags)0, level.Walls[2].Flags);
            Assert.AreEqual((WallState)0, level.Walls[2].State);

            // Wall 8 is a red door
            Assert.AreEqual(WallType.Door, level.Walls[8].Type);
            Assert.AreEqual(WallFlags.DoorAuto, level.Walls[8].Flags);
            Assert.AreEqual(WallState.DoorClosed, level.Walls[8].State);
            Assert.AreEqual(WallKeyFlags.Red, level.Walls[8].Keys);

            // Wall 10 is a fly-through trigger
            Assert.IsNull(level.Walls[10].OppositeWall);
            Assert.AreEqual(WallType.Open, level.Walls[10].Type);
            Assert.AreEqual((WallFlags)0, level.Walls[10].Flags);
            Assert.AreEqual((WallState)0, level.Walls[10].State);
        }

        [Test]
        public void TestTriggers()
        {
            Assert.AreEqual(3, level.Triggers.Count);

            // Trigger 0 is the exit trigger
            Assert.AreSame(level.Triggers[0], level.Walls[0].Trigger);
            Assert.AreEqual(1, level.Triggers[0].ConnectedWalls.Count);
            Assert.AreSame(level.Walls[0], level.Triggers[0].ConnectedWalls[0]);
            Assert.IsEmpty(level.Triggers[0].ConnectedObjects);
            Assert.IsEmpty(level.Triggers[0].Targets);
            Assert.AreEqual(D1TriggerFlags.Exit, level.Triggers[0].Flags);
            // .rdl (D1) should not use trigger type
            Assert.AreEqual((TriggerType)0, level.Triggers[0].Type);
            // Value/time have no effect on this trigger but are set anyway
            Assert.AreEqual((Fix)5, level.Triggers[0].Value);
            Assert.AreEqual(-1, level.Triggers[0].Time);

            // Trigger 2 is a matcen trigger
            Assert.AreSame(level.Triggers[2], level.Walls[10].Trigger);
            Assert.AreEqual(1, level.Triggers[2].ConnectedWalls.Count);
            Assert.AreSame(level.Walls[10], level.Triggers[2].ConnectedWalls[0]);
            Assert.AreEqual(1, level.Triggers[2].Targets.Count);
            Assert.AreSame(level.Segments[16].Sides[5], level.Triggers[2].Targets[0]);
            Assert.AreEqual(D1TriggerFlags.MatCenter, level.Triggers[2].Flags);

            // Trigger 1 is a door trigger
            Assert.AreEqual(D1TriggerFlags.ControlDoors, level.Triggers[1].Flags);
            Assert.AreEqual(1, level.Triggers[1].Targets.Count);
            Assert.AreSame(level.Segments[9].Sides[4], level.Triggers[1].Targets[0]);
            Assert.AreEqual(1, level.Segments[9].Sides[4].Wall.ControllingTriggers.Count);
            Assert.AreSame(level.Triggers[1], level.Segments[9].Sides[4].Wall.ControllingTriggers[0].trigger);
            Assert.AreEqual(0, level.Segments[9].Sides[4].Wall.ControllingTriggers[0].targetNum);
        }

        [Test]
        public void TestReactorTriggers()
        {
            Assert.AreEqual(1, level.ReactorTriggerTargets.Count);
            Assert.AreSame(level.Segments[10].Sides[4], level.ReactorTriggerTargets[0]);
        }

        [Test]
        public void TestMatcens()
        {
            Assert.AreEqual(1, level.MatCenters.Count);

            Assert.AreSame(level.MatCenters[0], level.Segments[16].MatCenter);
            Assert.AreSame(level.Segments[16], level.MatCenters[0].Segment);
            Assert.AreEqual(SegFunction.MatCenter, level.Segments[16].Function);
            Assert.AreEqual(2, level.MatCenters[0].SpawnedRobotIds.Count);
            var expectedRobotIds = new uint[] { 4, 20 };
            Assert.That(level.MatCenters[0].SpawnedRobotIds, Is.EquivalentTo(expectedRobotIds));
            // Hit points/interval are ignored but are part of the file format
            Assert.AreEqual((Fix)0, level.MatCenters[0].HitPoints);
            Assert.AreEqual((Fix)0, level.MatCenters[0].Interval);
        }

        [Test]
        public void TestUnconnectedSegmentConnections()
        {
            // D1 level 1 has a case where a segment connection is written for a side
            // that is not connected (-1). This needs to be handled.
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0xff, 0xff }));
            var loader = new D1LevelReader(null);
            var segment = new Segment();
            for (uint sideNum = 0; sideNum < segment.Sides.Length; sideNum++)
            {
                segment.Sides[sideNum] = new Side(segment, sideNum);
            }

            Assert.DoesNotThrow(() => loader.ReadSegmentConnections(reader, segment, 0x01));
            Assert.IsNull(segment.Sides[0].ConnectedSegment);
        }
    }

    class RdlWriteTests
    {
        [Test]
        public void TestSaveLevel()
        {
            D1Level level;
            using (var stream = TestUtils.GetResourceStream("test.rdl"))
            {
                level = D1Level.CreateFromStream(stream);
            }

            // Write the level and then re-load it. We don't save the same way as DLE
            // so the output won't match.
            // So we need to compare against something we saved earlier.
            var memoryStream = new MemoryStream();
            level.WriteToStream(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            level = D1Level.CreateFromStream(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Now do the test
            var originalFileContents = memoryStream.ToArray();
            Assert.DoesNotThrow(() => level.WriteToStream(memoryStream));
            var resultingFileContents = memoryStream.ToArray();
            Assert.That(resultingFileContents, Is.EqualTo(originalFileContents));
        }
    }
}
