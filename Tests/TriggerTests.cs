using LibDescent.Data;
using NUnit.Framework;

namespace LibDescent.Tests
{
    class TriggerTests
    {
        private IBlock block;

        [SetUp]
        public void Setup()
        {
            var stream = GetType().Assembly.GetManifestResourceStream(GetType(), "test.blx");
            block = ExtendedBlock.CreateFromStream(stream);
        }

        [Test]
        public void TestTriggerTargets()
        {
            var trigger = block.Triggers[0];
            Assert.IsNotNull(trigger.Targets);
            Assert.AreEqual(1, trigger.Targets.Count);
            var targetSide = block.Segments[3].GetSide(SegSide.Left);
            Assert.AreSame(targetSide, trigger.Targets[0]);
            Assert.AreEqual(1, targetSide.Wall.ControllingTriggers.Count);
            Assert.AreEqual(0, targetSide.Wall.ControllingTriggers[0].targetNum);
            Assert.AreSame(trigger, targetSide.Wall.ControllingTriggers[0].trigger);
        }

        [Test]
        public void TestTriggerProperties()
        {
            var trigger = block.Triggers[0];
            Assert.AreEqual(TriggerType.OpenDoor, trigger.Type);
            Assert.AreEqual(0, trigger.Flags);
            Assert.AreEqual((Fix)5.0, new Fix((int)trigger.Value));
            Assert.AreEqual(-1, trigger.Time);

            trigger = block.Triggers[1];
            Assert.AreEqual(TriggerType.Matcen, trigger.Type);
            Assert.AreEqual(D2TriggerFlags.OneShot, (D2TriggerFlags)trigger.Flags);
        }
    }
}
