using LibDescent.Data;
using NUnit.Framework;
using System.Linq;
using System.Xml.Linq;

namespace LibDescent.Tests
{
    class MatcenTests
    {
        private IBlock block;

        [SetUp]
        public void Setup()
        {
            var stream = GetType().Assembly.GetManifestResourceStream(GetType(), "test.blx");
            block = ExtendedBlock.CreateFromStream(stream);
        }

        [Test]
        public void TestMatcenLinking()
        {
            var matcen = block.MatCenters[0];
            Assert.AreSame(matcen, block.Segments[8].MatCenter);
            Assert.AreSame(block.Segments[8], matcen.Segment);
            Assert.AreEqual(SegFunction.MatCenter, block.Segments[8].Function);
        }

        [Test]
        public void TestMatcenProperties()
        {
            var matcen = block.MatCenters[0];
            Assert.AreEqual(5.0, (double)matcen.Interval);
            Assert.AreEqual(500, (double)matcen.HitPoints);
        }

        [Test]
        public void TestMatcenRobotListDefaults()
        {
            var matcen = block.MatCenters[0];
            Assert.IsEmpty(matcen.SpawnedRobotIds);
        }

        [Test]
        public void TestMatcenInitializeSpawnedRobots()
        {
            var xmlStream = GetType().Assembly.GetManifestResourceStream(GetType(), "DefaultSegment.xml");
            var segment = Segment.FromXML(XDocument.Load(xmlStream).Root);
            var matcen = new MatCenter(segment);

            // Empty list
            uint[] robotListFlags = new uint[1];
            Assert.DoesNotThrow(() => matcen.InitializeSpawnedRobots(robotListFlags));
            Assert.IsEmpty(matcen.SpawnedRobotIds);

            // D1 list - one robot
            robotListFlags[0] = 0x00000001;
            Assert.DoesNotThrow(() => matcen.InitializeSpawnedRobots(robotListFlags));
            Assert.AreEqual(1, matcen.SpawnedRobotIds.Count);
            Assert.Contains(0, matcen.SpawnedRobotIds);

            // D1 list - different robot
            robotListFlags[0] = 0x80000000;
            Assert.DoesNotThrow(() => matcen.InitializeSpawnedRobots(robotListFlags));
            // Should have been reset
            Assert.AreEqual(1, matcen.SpawnedRobotIds.Count);
            Assert.Contains(31, matcen.SpawnedRobotIds);

            // D1 list - multiple robots
            robotListFlags[0] = 0x000000F0;
            Assert.DoesNotThrow(() => matcen.InitializeSpawnedRobots(robotListFlags));
            Assert.AreEqual(4, matcen.SpawnedRobotIds.Count);
            var expectedRobotIds = new uint[] { 4, 5, 6, 7 };
            Assert.That(Enumerable.SequenceEqual(expectedRobotIds, matcen.SpawnedRobotIds));

            // D2 list - one robot, first byte
            robotListFlags = new uint[2];
            robotListFlags[0] = 0x80000000;
            Assert.DoesNotThrow(() => matcen.InitializeSpawnedRobots(robotListFlags));
            Assert.AreEqual(1, matcen.SpawnedRobotIds.Count);
            Assert.Contains(31, matcen.SpawnedRobotIds);

            // D2 list - one robot, second uint
            robotListFlags[0] = 0x00000000;
            robotListFlags[1] = 0x80000000;
            Assert.DoesNotThrow(() => matcen.InitializeSpawnedRobots(robotListFlags));
            Assert.AreEqual(1, matcen.SpawnedRobotIds.Count);
            Assert.Contains(63, matcen.SpawnedRobotIds);

            // D2 list - multiple robots
            robotListFlags[0] = 0x000C0000;
            robotListFlags[1] = 0x00010804;
            Assert.DoesNotThrow(() => matcen.InitializeSpawnedRobots(robotListFlags));
            Assert.AreEqual(5, matcen.SpawnedRobotIds.Count);
            expectedRobotIds = new uint[] { 18, 19, 34, 43, 48 };
            Assert.That(Enumerable.SequenceEqual(expectedRobotIds, matcen.SpawnedRobotIds));
        }
    }
}
