using LibDescent.Data.Midi;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDescent.Tests
{
    [TestFixtureSource("TestData")]
    class HMPTests
    {
        private MIDISequence midi;

        [SetUp]
        public void Setup()
        {
            midi = new MIDISequence();
        }

        [Test]
        public void TestRead()
        {
            midi.ReadHMP(TestUtils.GetResourceStream("vgame20.hmp"));
            Assert.AreEqual(13, midi.TrackCount);
            Assert.AreEqual(HMPValidDevice.Default, midi.Tracks[0].HMPDevices);
            for (int i = 1; i < midi.TrackCount; ++i)
                Assert.AreEqual(HMPValidDevice.All, midi.Tracks[i].HMPDevices);
            foreach (MIDITrack track in midi.Tracks)
                Assert.AreEqual(0, track.HMPBranchPoints.Count);

            List<MIDIEvent> events = new List<MIDIEvent>();
            MIDITrack track1 = midi.Tracks[1];
            Assert.AreEqual(2952, track1.EventCount);
            events.Clear();
            events.AddRange(track1.GetAllEvents());
            Assert.AreEqual(0, events[2].Time);
            Assert.AreEqual(5743, events[120].Time);
            Assert.AreEqual(5977, events[300].Time);
        }

        [Test]
        public void TestWrite()
        {
            midi.ReadHMP(TestUtils.GetResourceStream("vgame20.hmp"));
            Assert.AreEqual(110712, midi.Write().Length);
        }
    }
}
