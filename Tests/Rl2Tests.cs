using LibDescent.Data;
using NUnit.Framework;
using System.Collections;
using System.IO;

namespace LibDescent.Tests
{
    [TestFixtureSource("TestData")]
    class Rl2Tests
    {
        private readonly D2Level level;

        public static IEnumerable TestData
        {
            get
            {
                // First case - test level (saved by DLE)
                D2Level level;
                using (var stream = TestUtils.GetResourceStream("test.rl2"))
                {
                    level = D2Level.CreateFromStream(stream);
                }
                yield return new TestFixtureData(level);

                // Second case - output of D2Level.WriteToStream
                using (var stream = new MemoryStream())
                {
                    level.WriteToStream(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    level = D2Level.CreateFromStream(stream);
                }
                yield return new TestFixtureData(level);
            }
        }

        public Rl2Tests(D2Level level)
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
            Assert.AreEqual("d2test", level.LevelName);
        }

        [Test]
        public void TestVertices()
        {
            // Most of this is no different from D1, so just look at basic stuff
            Assert.AreEqual(278, level.Vertices.Count);
        }

        [Test]
        public void TestSegments()
        {
            Assert.AreEqual(75, level.Segments.Count);

            Assert.IsTrue(level.Segments[49].Sides[1].Exit);
            Assert.AreSame(level.Vertices[0], level.Segments[0].Vertices[0]);
            Assert.AreEqual(SegFunction.None, level.Segments[0].Function);
            Assert.AreEqual(0, level.Segments[0].Flags);

            // Segment 2 - blue goal
            Assert.AreEqual(SegFunction.BlueGoal, level.Segments[2].Function);

            // Segment 37 - red goal
            Assert.AreEqual(SegFunction.RedGoal, level.Segments[37].Function);

            // Segment 55 - energy center
            Assert.AreEqual(SegFunction.FuelCenter, level.Segments[55].Function);

            // Segment 74 - reactor
            Assert.AreEqual(SegFunction.Reactor, level.Segments[74].Function);
        }

        [Test]
        public void TestObjects()
        {
            Assert.AreEqual(28, level.Objects.Count);

            // Object 0 - player
            Assert.AreEqual(ObjectType.Player, level.Objects[0].type);
            Assert.AreEqual(0, level.Objects[0].id);
            Assert.AreEqual(MovementType.Physics, level.Objects[0].moveType);
            Assert.AreEqual(ControlType.Slew, level.Objects[0].controlType);
            Assert.AreEqual(RenderType.Polyobj, level.Objects[0].renderType);
            Assert.AreEqual(new FixVector(0, 0, 0), level.Objects[0].position);
            var expectedOrientation = new FixMatrix(new FixVector(1, 0, 0), new FixVector(0, 1, 0), new FixVector(0, 0, 1));
            Assert.AreEqual(expectedOrientation, level.Objects[0].orientation);
            Assert.AreEqual(108, level.Objects[0].modelInfo.modelNum);
            Assert.AreEqual(0, level.Objects[0].segnum);

            // Object 1 - reactor
            Assert.AreEqual(ObjectType.ControlCenter, level.Objects[1].type);
            Assert.AreEqual(2, level.Objects[1].id);
            Assert.AreEqual(MovementType.None, level.Objects[1].moveType);
            Assert.AreEqual(ControlType.ControlCenter, level.Objects[1].controlType);
            Assert.AreEqual(RenderType.Polyobj, level.Objects[1].renderType);
            Assert.AreEqual(new FixVector(50, -20, -95), level.Objects[1].position);
            Assert.AreEqual(expectedOrientation, level.Objects[1].orientation);
            Assert.AreEqual(97, level.Objects[1].modelInfo.modelNum);
            Assert.AreEqual(74, level.Objects[1].segnum);

            // Object 3 - hostage
            Assert.AreEqual(ObjectType.Hostage, level.Objects[3].type);
            Assert.AreEqual(0, level.Objects[3].id);
            Assert.AreEqual(MovementType.None, level.Objects[3].moveType);
            Assert.AreEqual(ControlType.Powerup, level.Objects[3].controlType);
            Assert.AreEqual(RenderType.Hostage, level.Objects[3].renderType);
            Assert.AreEqual(new FixVector(45, -65, 30), level.Objects[3].position);
            Assert.AreEqual(expectedOrientation, level.Objects[3].orientation);
            Assert.AreEqual(33, level.Objects[3].spriteInfo.vclipNum);
            Assert.AreEqual(39, level.Objects[3].segnum);

            // Object 6 - co-op player
            Assert.AreEqual(ObjectType.Coop, level.Objects[6].type);
            Assert.AreEqual(8, level.Objects[6].id);
            Assert.AreEqual(MovementType.Physics, level.Objects[6].moveType);
            Assert.AreEqual(ControlType.None, level.Objects[6].controlType);
            Assert.AreEqual(RenderType.Polyobj, level.Objects[6].renderType);
            Assert.AreEqual(new FixVector(20, 0, 40), level.Objects[6].position);
            expectedOrientation = new FixMatrix(new FixVector(0, 0, 1), new FixVector(0, 1, 0), new FixVector(-1, 0, 0));
            Assert.AreEqual(expectedOrientation, level.Objects[6].orientation);
            Assert.AreEqual(108, level.Objects[6].modelInfo.modelNum);
            Assert.AreEqual(5, level.Objects[6].segnum);

            // Object 9 - Guide-bot
            Assert.AreEqual(ObjectType.Robot, level.Objects[9].type);
            Assert.AreEqual(33, level.Objects[9].id);
            Assert.AreEqual(MovementType.Physics, level.Objects[9].moveType);
            Assert.AreEqual(ControlType.AI, level.Objects[9].controlType);
            Assert.AreEqual(RenderType.Polyobj, level.Objects[9].renderType);
            Assert.AreEqual(new FixVector(0, 30, 120), level.Objects[9].position);
            expectedOrientation = new FixMatrix(new FixVector(1, 0, 0), new FixVector(0, 1, 0), new FixVector(0, 0, 1));
            Assert.AreEqual(expectedOrientation, level.Objects[9].orientation);
            Assert.AreEqual(51, level.Objects[9].modelInfo.modelNum);
            Assert.AreEqual(15, level.Objects[9].segnum);

            // Object 21 - robot (Sidearm) with contained robots
            Assert.AreEqual(ObjectType.Robot, level.Objects[21].type);
            Assert.AreEqual(30, level.Objects[21].id);
            Assert.AreEqual(MovementType.Physics, level.Objects[21].moveType);
            Assert.AreEqual(ControlType.AI, level.Objects[21].controlType);
            Assert.AreEqual(RenderType.Polyobj, level.Objects[21].renderType);
            Assert.AreEqual(new FixVector(120, -20, -105), level.Objects[21].position);
            Assert.AreEqual(expectedOrientation, level.Objects[21].orientation);
            Assert.AreEqual((Fix)120, Fix.FromRawValue(level.Objects[21].shields));
            Assert.AreEqual(2, level.Objects[21].containsType); // robot
            Assert.AreEqual(4, level.Objects[21].containsCount);
            Assert.AreEqual(50, level.Objects[21].containsId); // sidearm modula
            Assert.AreEqual(47, level.Objects[21].modelInfo.modelNum);
            Assert.AreEqual(61, level.Objects[21].segnum);

            // Object 25 - blue flag
            Assert.AreEqual(ObjectType.Powerup, level.Objects[25].type);
            Assert.AreEqual(46, level.Objects[25].id);
            Assert.AreEqual(MovementType.None, level.Objects[25].moveType);
            Assert.AreEqual(ControlType.Powerup, level.Objects[25].controlType);
            Assert.AreEqual(RenderType.Powerup, level.Objects[25].renderType);
            Assert.AreEqual(new FixVector(120, -20, 10), level.Objects[25].position);
            Assert.AreEqual(expectedOrientation, level.Objects[25].orientation);
            Assert.AreEqual(42, level.Objects[25].segnum);
        }

        [Test]
        public void TestWalls()
        {
            Assert.AreEqual(36, level.Walls.Count);

            // Wall 0 - start door
            Assert.AreSame(level.Walls[0], level.Segments[0].GetSide(SegSide.Front).Wall);
            Assert.AreSame(level.Segments[0].GetSide(SegSide.Front), level.Walls[0].Side);
            Assert.AreSame(level.Walls[1], level.Walls[0].OppositeWall);
            Assert.AreSame(level.Walls[0], level.Walls[1].OppositeWall);
            Assert.AreEqual(WallType.Door, level.Walls[0].Type);
            Assert.AreEqual(WallFlags.DoorLocked, level.Walls[0].Flags);
            Assert.AreEqual(WallState.DoorClosed, level.Walls[0].State);
            Assert.AreEqual(WallKeyFlags.None, level.Walls[0].Keys);

            // Wall 6 - control panel
            Assert.IsNull(level.Walls[6].OppositeWall);
            Assert.AreEqual(WallType.Overlay, level.Walls[6].Type);
            Assert.AreEqual(WallFlags.WallSwitch, level.Walls[6].Flags);
            Assert.AreEqual((WallState)0, level.Walls[6].State);

            // Wall 7 - force field
            Assert.AreSame(level.Walls[8], level.Walls[7].OppositeWall);
            Assert.AreSame(level.Walls[7], level.Walls[8].OppositeWall);
            Assert.AreEqual(WallType.Closed, level.Walls[7].Type);
            Assert.AreEqual((WallFlags)0, level.Walls[7].Flags);
            Assert.AreEqual((WallState)0, level.Walls[7].State);

            // Wall 9 - secret exit
            Assert.AreEqual(WallType.Illusion, level.Walls[9].Type);
            Assert.AreEqual((WallFlags)0, level.Walls[9].Flags);
            Assert.AreEqual((WallState)0, level.Walls[9].State);

            // Wall 13 - guide bot door
            Assert.AreEqual(WallType.Blastable, level.Walls[13].Type);
            Assert.AreEqual((Fix)100, level.Walls[13].HitPoints);
            Assert.AreEqual((WallFlags)0, level.Walls[13].Flags);
            Assert.AreEqual((WallState)0, level.Walls[13].State);
            Assert.AreEqual((WallKeyFlags)0, level.Walls[13].Keys);

            // Wall 32 - open trigger
            Assert.IsNull(level.Walls[32].OppositeWall);
            Assert.AreEqual(WallType.Open, level.Walls[32].Type);
            Assert.AreEqual((WallFlags)0, level.Walls[32].Flags);
            Assert.AreEqual((WallState)0, level.Walls[32].State);
        }

        [Test]
        public void TestTriggers()
        {
            Assert.AreEqual(8, level.Triggers.Count);

            // Trigger 0 - control panel - open door
            Assert.AreSame(level.Triggers[0], level.Walls[6].Trigger);
            Assert.AreEqual(1, level.Triggers[0].Targets.Count);
            Assert.AreSame(level.Segments[6].Sides[0], level.Triggers[0].Targets[0]);
            Assert.AreEqual(D2TriggerFlags.NoMessage, level.Triggers[0].Flags);
            Assert.AreEqual(TriggerType.OpenDoor, level.Triggers[0].Type);
            Assert.AreEqual((Fix)5, level.Triggers[0].Value);
            Assert.AreEqual(-1, level.Triggers[0].Time);

            // Trigger 2 - control panel - open wall
            Assert.AreSame(level.Triggers[2], level.Walls[17].Trigger);
            Assert.AreEqual(TriggerType.OpenWall, level.Triggers[2].Type);

            // Trigger 3 - exit
            Assert.AreSame(level.Triggers[3], level.Walls[18].Trigger);
            Assert.IsEmpty(level.Triggers[3].Targets);
            Assert.AreEqual((D2TriggerFlags)0, level.Triggers[3].Flags);
            Assert.AreEqual(TriggerType.Exit, level.Triggers[3].Type);

            // Trigger 6 - one shot matcen
            Assert.AreSame(level.Triggers[6], level.Walls[31].Trigger);
            Assert.AreEqual(1, level.Triggers[6].Targets.Count);
            Assert.AreSame(level.Segments[29].Sides[1], level.Triggers[6].Targets[0]);
            Assert.AreEqual(D2TriggerFlags.OneShot, level.Triggers[6].Flags);
            Assert.AreEqual(TriggerType.Matcen, level.Triggers[6].Type);
        }

        [Test]
        public void TestReactorProperties()
        {
            Assert.AreEqual(30, level.BaseReactorCountdownTime);
            Assert.AreEqual(300, level.ReactorStrength);
            Assert.AreEqual(1, level.ReactorTriggerTargets.Count);
            Assert.AreSame(level.Segments[43].Sides[2], level.ReactorTriggerTargets[0]);
        }

        [Test]
        public void TestMatcens()
        {
            Assert.AreEqual(1, level.MatCenters.Count);

            Assert.AreSame(level.MatCenters[0], level.Segments[29].MatCenter);
            Assert.AreSame(level.Segments[29], level.MatCenters[0].Segment);
            Assert.AreEqual(SegFunction.MatCenter, level.Segments[29].Function);
            Assert.AreEqual(2, level.MatCenters[0].SpawnedRobotIds.Count);
            var expectedRobotIds = new uint[] { 37, 40 }; // IT Droid, Diamond Claw
            Assert.That(level.MatCenters[0].SpawnedRobotIds, Is.EquivalentTo(expectedRobotIds));
        }

        [Test]
        public void TestPaletteName()
        {
            Assert.AreEqual("groupa.256", level.PaletteName);
        }

        [Test]
        public void TestSecretExit()
        {
            Assert.AreSame(level.Segments[6], level.SecretReturnSegment);
            var expectedOrientation = new FixMatrix(new FixVector(0, 0, -1), new FixVector(0, 1, 0), new FixVector(1, 0, 0));
            Assert.AreEqual(expectedOrientation, level.SecretReturnOrientation);
            Assert.AreEqual(TriggerType.SecretExit, level.Triggers[1].Type);
        }

        [Test]
        public void TestDynamicLights()
        {
            Assert.AreEqual(13, level.DynamicLights.Count);

            // DLE's dynamic light handling is pretty opaque, so we'll just sanity-check
            // some things about the dynamic light list.

            // All lights should have a source side
            Assert.That(level.DynamicLights.TrueForAll(light => light.Source != null));

            // All lights should affect at least one side
            Assert.That(level.DynamicLights.TrueForAll(light => light.LightDeltas.Count > 0));

            // All light deltas for a light should have a target side
            foreach (var light in level.DynamicLights)
            {
                foreach (var delta in light.LightDeltas)
                {
                    Assert.IsNotNull(delta.targetSide);
                }
            }

            // Check that some of the lights are reading values properly
            Assert.That(level.DynamicLights[0].LightDeltas[0].vertexDeltas, Is.EqualTo(new Fix[] {
                Fix.FromRawValue(38912),
                Fix.FromRawValue(12288),
                Fix.FromRawValue(12288),
                Fix.FromRawValue(38912)
                }));
            Assert.That(level.DynamicLights[3].LightDeltas[25].vertexDeltas, Is.EqualTo(new Fix[] {
                Fix.FromRawValue(38912),
                Fix.FromRawValue(38912),
                Fix.FromRawValue(38912),
                Fix.FromRawValue(38912)
                }));
        }

        [Test]
        public void TestAnimatedLights()
        {
            Assert.AreEqual(2, level.AnimatedLights.Count);

            Assert.That(level.AnimatedLights.TrueForAll(light => light.DynamicLight != null));

            Assert.AreEqual((Fix)0.25, level.AnimatedLights[0].TickLength);
            Assert.AreEqual(0b00110011001100110011001100110011, level.AnimatedLights[0].Mask);
            Assert.AreEqual((Fix)0.25, level.AnimatedLights[1].TickLength);
            Assert.AreEqual(0b11001100110011001100110011001100, level.AnimatedLights[1].Mask);
        }
    }

    class Rl2WriteTests
    {
        [Test]
        public void TestSaveLevel()
        {
            D2Level level;
            using (var stream = TestUtils.GetResourceStream("test.rl2"))
            {
                level = D2Level.CreateFromStream(stream);
            }

            // Write the level and then re-load it. We don't save the same way as DLE
            // so the output won't match.
            // So we need to compare against something we saved earlier.
            var memoryStream = new MemoryStream();
            level.WriteToStream(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            level = D2Level.CreateFromStream(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Now do the test
            var originalFileContents = memoryStream.ToArray();
            Assert.DoesNotThrow(() => level.WriteToStream(memoryStream));
            var resultingFileContents = memoryStream.ToArray();
            Assert.That(resultingFileContents, Is.EqualTo(originalFileContents));
        }
    }
}
