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
            midi.ReadHMP(TestUtils.GetResourceStream("vgame12.hmp"));
            Assert.AreEqual(midi.TrackCount, 17);
            Console.WriteLine("here's where I stick a breakpoint because there's not a real test yet... heeh.");
        }
    }
}
