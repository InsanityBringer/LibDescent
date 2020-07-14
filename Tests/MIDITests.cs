using LibDescent.Data.Midi;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDescent.Tests
{
    [TestFixtureSource("TestData")]
    class MIDITests
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
            midi.Read(TestUtils.GetResourceStream("Bass_sample.mid"));
            Assert.AreEqual(2, midi.TrackCount);

            int i = 0;
            List<MIDIEvent> events = new List<MIDIEvent>();

            MIDITrack track0 = midi.Tracks[0];
            Assert.AreEqual(4, track0.EventCount);
            events.Clear();
            events.AddRange(track0.GetAllEvents());
            Assert.AreEqual(0, events[0].Time);
            Assert.AreEqual(MIDIMessageType.TimeSignature, events[0].Data.Type);
            Assert.AreEqual(4, (events[0].Data as MIDITimeSignatureMessage).Numerator);
            Assert.AreEqual(4, (events[0].Data as MIDITimeSignatureMessage).Denominator);
            Assert.AreEqual(24, (events[0].Data as MIDITimeSignatureMessage).MetronomeClocks);
            Assert.AreEqual(8, (events[0].Data as MIDITimeSignatureMessage).NotatedQuarterTicks);

            Assert.AreEqual(0, events[1].Time);
            Assert.AreEqual(MIDIMessageType.SetTempo, events[1].Data.Type);
            Assert.AreEqual(500000, (events[1].Data as MIDITempoMessage).Tempo);
            Assert.That((events[1].Data as MIDITempoMessage).BeatsPerMinute, Is.EqualTo(120).Within(0.001));

            MIDITrack track1 = midi.Tracks[1];
            Assert.AreEqual(31, track1.EventCount);
            events.Clear();
            events.AddRange(track1.GetAllEvents());
            Assert.AreEqual(0, events[0].Time);
            Assert.AreEqual(MIDIMessageType.ProgramChange, events[0].Data.Type);
            Assert.AreEqual(0x21, (events[0].Data as MIDIProgramChangeMessage).Program);
            Assert.AreEqual(0, events[1].Time);
            Assert.AreEqual(MIDIMessageType.ControlChange, events[1].Data.Type);
            Assert.AreEqual(MIDIControl.ChannelVolumeMSB, (events[1].Data as MIDIControlChangeMessage).Controller);

            Assert.AreEqual(0, events[2].Time);
            Assert.AreEqual(129, events[3].Time);
            Assert.AreEqual(360, events[4].Time);
            Assert.AreEqual(480, events[5].Time);
            Assert.AreEqual(483, events[6].Time);
            Assert.AreEqual(600, events[7].Time);
            Assert.AreEqual(3450, events[29].Time);

            Assert.AreEqual(MIDIMessageType.NoteOn, events[2].Data.Type);
            Assert.AreEqual(MIDIMessageType.NoteOff, events[3].Data.Type);
            Assert.AreEqual(MIDIMessageType.NoteOff, events[29].Data.Type);

            Assert.AreEqual(0, (events[2].Data as MIDINoteMessage).Channel);

            Assert.AreEqual(0x2D, (events[2].Data as MIDINoteMessage).Key);
            Assert.AreEqual(0x2D, (events[3].Data as MIDINoteMessage).Key);
            Assert.AreEqual(0x32, (events[22].Data as MIDINoteMessage).Key);
        }

        [Test]
        public void TestWrite()
        {
            byte[] data = TestUtils.GetArrayFromResourceStream("Bass_sample.mid");
            midi.Read(data);
            MemoryStream ms = new MemoryStream();
            midi.Write(ms, MIDIWriteOptions.ExplicitStatus);
            Assert.AreEqual(data, ms.ToArray());
        }
    }
}
