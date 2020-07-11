using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibDescent.Data.Midi
{
    /// <summary>
    /// Represents a (Standard) MIDI sequence.
    /// </summary>
    public class MIDISequence
    {
        /// <summary>
        /// The MIDI format used by this sequence.
        /// </summary>
        public MIDIFormat Format;
        /// <summary>
        /// The tracks contained in this sequence.
        /// </summary>
        public List<MIDITrack> Tracks;

        private bool tickRateSMPTE;
        private int tickRateQuarterTicks;
        private int tickRateFrameTicks;
        private int tickRateFrames;

        /// <summary>
        /// Initializes a new instance of the MIDISequence class, representing a MIDI sequence with one track without any events.
        /// </summary>
        public MIDISequence()
        {
            Format = MIDIFormat.Type1;
            Tracks = new List<MIDITrack>();
            Tracks.Add(new MIDITrack());

            tickRateSMPTE = false;
            tickRateQuarterTicks = 480;
        }

        /// <summary>
        /// Gets the total number of tracks in this sequence.
        /// </summary>
        public int TrackCount => Tracks.Count;

        /// <summary>
        /// Loads a MIDI music piece from a stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns></returns>
        public void Read(Stream stream)
        {
            using (BinaryReaderMIDI br = new BinaryReaderMIDI(stream))
            {
                stream.Seek(0, SeekOrigin.Begin);
                if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "MThd")
                    throw new ArgumentException("Not a valid MIDI");
                if (br.ReadInt32() != 6)
                    throw new ArgumentException("Not a valid MIDI");
                short format = br.ReadInt16();
                if (format >= 3)
                    throw new ArgumentException("Not a valid MIDI");
                Format = (MIDIFormat)format;
                ushort nTracks = br.ReadUInt16();
                short division = br.ReadInt16();
                if (tickRateSMPTE = (division < 0))
                {
                    tickRateFrameTicks = division & 0xFF;
                    tickRateFrames = -(division >> 8);
                }
                else
                    tickRateQuarterTicks = division & 0x7FFF;

                Tracks.Clear();
                for (int i = 0; i < nTracks; ++i)
                {
                    MIDITrack trk = new MIDITrack();
                    if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "MTrk")
                        throw new ArgumentException("Not a valid MIDI");
                    int trackLength = br.ReadInt32();
                    byte[] trackData = br.ReadBytes(trackLength);
                    if (trackData.Length < trackLength)
                        throw new EndOfStreamException();

                    ReadMIDITrack(trk, trackData);
                    Tracks.Add(trk);
                }
            }
        }

        private void ReadMIDITrack(MIDITrack trk, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReaderMIDI br = new BinaryReaderMIDI(ms))
            {
                byte status = 0;
                ulong position = 0;
                ulong delta;
                MIDIMessage evt;
                int metaChannel = -1;
                while (ReadMIDIEvent(br, ref status, ref metaChannel, out delta, out evt))
                {
                    position += delta;
                    if (evt != null)
                        trk.AddEvent(new MIDIEvent(position, evt));
                }
            }
        }

        private static readonly MIDIMessageType[] midiMetaTypes = new MIDIMessageType[]
        {
            0,
            MIDIMessageType.MetaText,
            MIDIMessageType.MetaCopyright,
            MIDIMessageType.MetaTrackName,
            MIDIMessageType.MetaInstrumentName,
            MIDIMessageType.MetaLyric,
            MIDIMessageType.MetaMarker,
            MIDIMessageType.MetaCuePoint
        };

        private bool ReadMIDIEvent(BinaryReaderMIDI br, ref byte status, ref int metaChannel, out ulong delta, out MIDIMessage evt)
        {
            byte tmp;
            delta = (ulong)br.ReadVLQ();
            tmp = br.ReadByte();
            if ((tmp & 0x80) == 0x80)
                status = tmp;

            int hinib = (status >> 4) & 7;
            int lonib = status & 15;
            switch (hinib)
            {
                case 0:             // NoteOff
                    evt = new MIDINoteMessage(MIDIMessageType.NoteOff, lonib, br.ReadByte(), br.ReadByte());
                    metaChannel = lonib;
                    return true;
                case 1:             // NoteOn
                    evt = new MIDINoteMessage(MIDIMessageType.NoteOn, lonib, br.ReadByte(), br.ReadByte());
                    metaChannel = lonib;
                    return true;
                case 2:             // NoteAftertouch
                    evt = new MIDINoteMessage(MIDIMessageType.NoteAftertouch, lonib, br.ReadByte(), br.ReadByte());
                    metaChannel = lonib;
                    return true;
                case 3:             // ControlChange
                    evt = new MIDIControlChangeMessage(lonib, (MIDIControl)br.ReadByte(), br.ReadByte());
                    metaChannel = lonib;
                    return true;
                case 4:             // ProgramChange
                    evt = new MIDIProgramChangeMessage(lonib, br.ReadByte());
                    metaChannel = lonib;
                    return true;
                case 5:             // ChannelAftertouch
                    evt = new MIDIChannelAftertouchMessage(lonib, br.ReadByte());
                    metaChannel = lonib;
                    return true;
                case 6:             // PitchBend
                    evt = new MIDIPitchBendMessage(lonib, br.ReadMidi14());
                    metaChannel = lonib;
                    return true;
                case 7:             // ...below...
                    break;
            }

            int seqlen;
            switch (status)
            {
                case 0xF0:          // SysEx
                    seqlen = br.ReadVLQ();
                    evt = new MIDISysExMessage(metaChannel, false, br.ReadBytes(seqlen));
                    return true;
                case 0xF7:          // SysEx Continue
                    seqlen = br.ReadVLQ();
                    evt = new MIDISysExMessage(metaChannel, true, br.ReadBytes(seqlen));
                    return true;
                case 0xFF:          // meta
                    byte metaType = br.ReadByte();
                    switch (metaType)
                    {
                        case 0:     // ignore
                            br.ReadByte();
                            evt = null;
                            return true;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            seqlen = br.ReadVLQ();
                            evt = new MIDIMetaMessage(midiMetaTypes[metaType], metaChannel, br.ReadBytes(seqlen));
                            return true;
                        case 0x20:
                            if (br.ReadByte() == 1)
                            {
                                int trackNum = br.ReadByte();
                                metaChannel = trackNum & 15;
                                evt = null;
                                return true;
                            }
                            break;
                        case 0x2F:
                            if (br.ReadByte() == 0)
                            {
                                // end of track
                                evt = null;
                                return false;
                            }
                            break;
                        case 0x51:
                            if (br.ReadByte() == 3)
                            {
                                int tempo = 0;
                                tempo |= (br.ReadByte() << 16);
                                tempo |= (br.ReadByte() << 8);
                                tempo |= br.ReadByte();
                                evt = new MIDITempoMessage(metaChannel, tempo);
                                return true;
                            }
                            break;
                        case 0x54:
                            br.ReadBytes(br.ReadByte());
                            evt = null;
                            return true;
                        case 0x58:
                            if (br.ReadByte() == 4)
                            {
                                byte n = br.ReadByte();
                                byte d = br.ReadByte();
                                byte c = br.ReadByte();
                                byte b = br.ReadByte();
                                evt = new MIDITimeSignatureMessage(metaChannel, n, d, c, b);
                                return true;
                            }
                            break;
                        case 0x59:
                            if (br.ReadByte() == 2)
                            {
                                byte sf = br.ReadByte();
                                byte mi = br.ReadByte();
                                evt = new MIDIKeySignatureMessage(metaChannel, sf, mi > 0);
                                return true;
                            }
                            break;
                        case 0x7F:
                            // ignore this data
                            seqlen = br.ReadVLQ();
                            br.ReadBytes(seqlen);
                            evt = null;
                            return true;
                        default:
                            break;
                    }
                    break;
            }

            evt = null;
            return false;
        }

        /// <summary>
        /// Loads a MIDI music piece from a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns></returns>
        public void Read(string filePath)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open))
            {
                Read(fs);
            }
        }

        /// <summary>
        /// Loads a MIDI music piece from an array.
        /// </summary>
        /// <param name="contents">The array to load from.</param>
        /// <returns></returns>
        public void Read(byte[] contents)
        {
            using (MemoryStream ms = new MemoryStream(contents))
            {
                Read(ms);
            }
        }
    }

    /// <summary>
    /// Represents a MIDI track.
    /// </summary>
    public class MIDITrack : IEnumerable<MIDIEvent>
    {
        internal SortedSet<MIDIInstant> tree;
        internal Dictionary<UInt64, MIDIInstant> idict;

        public MIDITrack()
        {
            tree = new SortedSet<MIDIInstant>(new MIDIPointComparer());
            idict = new Dictionary<ulong, MIDIInstant>();
        }

        IEnumerator<MIDIEvent> IEnumerable<MIDIEvent>.GetEnumerator()
        {
            return new MIDITrackEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MIDITrackEnumerator(this);
        }

        /// <summary>
        /// Returns an enumerator that iterates over relative events (instead of the time
        /// since the beginning of the track, the time will be from the last event).
        /// </summary>
        /// <returns>An enumerator that can be used to iterate over relative-time events.</returns>
        public IEnumerator<MIDIEventRelative> GetRelativeEnumerator()
        {
            return new MIDITrackDeltaEnumerator(this);
        }

        /// <summary>
        /// Gets all MIDI events that happen at a given point in time.
        /// </summary>
        /// <param name="time">The point in time, in ticks from the beginning of the track.</param>
        /// <returns>The set of MIDI events occurring at that time.</returns>
        public ICollection<MIDIEvent> GetEventsAt(ulong time)
        {
            List<MIDIEvent> events = new List<MIDIEvent>();
            if (idict.ContainsKey(time))
            {
                foreach (MIDIMessage msg in idict[time].Messages)
                    events.Add(new MIDIEvent(time, msg));
            }
            return events;
        }

        /// <summary>
        /// Gets all MIDI events that occur between two given points in time.
        /// </summary>
        /// <param name="start">The earliest time for which events should be returned.</param>
        /// <param name="end">The latest time for which events should be returned.</param>
        /// <returns>The set of MIDI events occurring between the two given points in time.</returns>
        public ICollection<MIDIEvent> GetEventsBetween(ulong start, ulong end)
        {
            MIDIInstant ghostStart = new MIDIInstant(start);
            MIDIInstant ghostEnd = new MIDIInstant(end);
            List<MIDIEvent> events = new List<MIDIEvent>();
            foreach (MIDIInstant point in tree.GetViewBetween(ghostStart, ghostEnd))
                foreach (MIDIMessage msg in point.Messages)
                    events.Add(new MIDIEvent(point.Time, msg));
            return events;
        }

        /// <summary>
        /// Gets all events in this MIDI track, in order.
        /// </summary>
        /// <returns>The set of all MIDI events on this track.</returns>
        public ICollection<MIDIEvent> GetAllEvents()
        { 
            List<MIDIEvent> events = new List<MIDIEvent>();
            foreach (MIDIInstant point in tree)
                foreach (MIDIMessage msg in point.Messages)
                    events.Add(new MIDIEvent(point.Time, msg));
            return events;
        }

        /// <summary>
        /// Gets the total number of events on this track.
        /// </summary>
        public int EventCount => tree.Select(p => p.Messages.Count).DefaultIfEmpty(0).Sum();

        /// <summary>
        /// Adds a new event onto this MIDI track.
        /// </summary>
        /// <param name="evt">The MIDI event to add.</param>
        public void AddEvent(MIDIEvent evt)
        {
            if (!idict.ContainsKey(evt.Time))
            {
                MIDIInstant point = new MIDIInstant(evt.Time);
                point.Messages = new List<MIDIMessage>();
                idict[evt.Time] = point;
                tree.Add(point);
            }
            idict[evt.Time].Messages.Add(evt.Data);
        }
        
        /// <summary>
        /// Removes an event from this MIDI track.
        /// </summary>
        /// <param name="evt">The event to remove.</param>
        /// <returns>Whether the event was removed from this track.</returns>
        public bool RemoveEvent(MIDIEvent evt)
        {
            ulong time = evt.Time;
            if (!idict.ContainsKey(time))
                return false;
            return idict[time].Messages.Remove(evt.Data);
        }

        /// <summary>
        /// An enumerator that is used to iterate over all of the events in a track in order,
        /// with the events containing the associated message and the point in time measured in
        /// MIDI ticks from the beginning of the track.
        /// </summary>
        public class MIDITrackEnumerator : IEnumerator<MIDIEvent>
        {
            private MIDITrack track;
            private IEnumerator<MIDIInstant> ienum;
            private int listIndex = 0;
            private int listLength = 0;
            private MIDIEvent ed;

            public MIDITrackEnumerator(MIDITrack track)
            {
                this.track = track;
                this.ienum = track.tree.GetEnumerator();
                Reset();
            }

            public MIDIEvent Current => ed;
            object IEnumerator.Current => ed;

            public bool MoveNext()
            {
                while (listIndex >= listLength)
                {
                    if (!ienum.MoveNext())
                        return false;
                    listIndex = 0;
                    listLength = ienum.Current.Messages.Count;
                }
                ed = new MIDIEvent(ienum.Current.Time, ienum.Current.Messages[listIndex++]);
                return true;
            }

            public void Dispose()
            {
                ienum.Dispose();
            }

            public void Reset()
            {
                ienum.Reset();
                listIndex = 0;
                listLength = 0;
            }
        }

        /// <summary>
        /// An enumerator that is used to iterate over all of the events in a track in order,
        /// with the events containing the associated message and the point in time measured in
        /// MIDI ticks from the last event.
        /// </summary>
        public class MIDITrackDeltaEnumerator : IEnumerator<MIDIEventRelative>
        {
            private MIDITrack track;
            private IEnumerator<MIDIInstant> ienum;
            private int listIndex = 0;
            private int listLength = 0;
            private MIDIEventRelative ed;

            public MIDITrackDeltaEnumerator(MIDITrack track)
            {
                this.track = track;
                this.ienum = track.tree.GetEnumerator();
                Reset();
            }

            public MIDIEventRelative Current => ed;
            object IEnumerator.Current => ed;

            public bool MoveNext()
            {
                ulong start = ienum.Current.Time;
                ulong delta;
                while (listIndex >= listLength)
                {
                    if (!ienum.MoveNext())
                        return false;
                    listIndex = 0;
                    listLength = ienum.Current.Messages.Count;
                }
                delta = ienum.Current.Time - start;
                ed = new MIDIEventRelative(delta, ienum.Current.Messages[listIndex++]);
                return true;
            }

            public void Dispose()
            {
                ienum.Dispose();
            }

            public void Reset()
            {
                ienum.Reset();
                listIndex = 0;
                listLength = 0;
            }
        }

        internal class MIDIPointComparer : Comparer<MIDIInstant>
        {
            public override int Compare(MIDIInstant a, MIDIInstant b)
            {
                return a.Time.CompareTo(b.Time);
            }
        }
    }

    /// <summary>
    /// Represents a MIDI event and its position on the track.
    /// </summary>
    public struct MIDIEvent
    {
        /// <summary>
        /// The number of ticks since the beginning of the track.
        /// </summary>
        public ulong Time;
        /// <summary>
        /// The event itself.
        /// </summary>
        public MIDIMessage Data;

        public MIDIEvent(ulong position, MIDIMessage evt)
        {
            Time = position;
            Data = evt;
        }
    }

    /// <summary>
    /// Represents a MIDI event and the number of ticks that has elapsed since the previous event.
    /// </summary>
    public struct MIDIEventRelative
    {
        /// <summary>
        /// The number of ticks since last event.
        /// </summary>
        public ulong Interval;
        /// <summary>
        /// The event itself.
        /// </summary>
        public MIDIMessage Event;

        public MIDIEventRelative(ulong interval, MIDIMessage evt)
        {
            Interval = interval;
            Event = evt;
        }
    }

    internal class MIDIInstant
    {
        internal ulong Time;
        internal List<MIDIMessage> Messages;

        internal MIDIInstant() : this(0) { }

        internal MIDIInstant(ulong position)
        {
            Time = position;
        }

        internal MIDIInstant(ulong position, IEnumerable<MIDIMessage> messages) : this(position)
        {
            Messages = new List<MIDIMessage>();
            Messages.AddRange(messages);
        }
    }

    /// <summary>
    /// Represents the internal MIDI format.
    /// </summary>
    public enum MIDIFormat
    {
        /// <summary>
        /// Type 0. One track.
        /// </summary>
        Type0 = 0,
        /// <summary>
        /// Type 1. Multiple synchronous tracks.
        /// </summary>
        Type1 = 1,
        /// <summary>
        /// Type 2. Multiple consecutive tracks.
        /// </summary>
        Type2 = 2,
    }

    /// <summary>
    /// A BinaryReader intended for use with MIDI files. Reads big-endian data and has a method for reading MIDI VLQs (variable-length quantity).
    /// </summary>
    public class BinaryReaderMIDI : BinaryReaderBE
    {
        public BinaryReaderMIDI(Stream stream) : base(stream) { }

        /// <summary>
        /// Reads a MIDI variable length quantity (VLQ) to the current stream and advances the stream position accordingly.
        /// </summary>
        /// <returns>The value that was read from the stream.</returns>
        public int ReadVLQ()
        {
            byte b;
            int r = 0;
            do
            {
                r <<= 7;
                b = ReadByte();
                r |= (b & 0x7F);
            } while (b >= 0x80);
            return r;
        }

        /// <summary>
        /// Reads a 14-bit quantity to the current stream and advances the stream position by two bytes.
        /// </summary>
        /// <returns>The 14-bit quantity read from the stream.</returns>
        public short ReadMidi14()
        {
            return (short)(((ReadByte() & 0x7F) << 7) | (ReadByte() & 0x7F));
        }
    }

    /// <summary>
    /// A BinaryWriter intended for use with MIDI files. Writes big-endian data and has a method for writing MIDI VLQs (variable-length quantity).
    /// </summary>
    public class BinaryWriterMIDI : BinaryWriterBE
    {
        public BinaryWriterMIDI(Stream stream) : base(stream) { }

        private byte[] vlq_buf = new byte[4];

        /// <summary>
        /// Writes a MIDI variable length quantity (VLQ) to the current stream and advances the stream position accordingly.
        /// </summary>
        /// <param name="v">The value to write.</param>
        public void WriteVLQ(int v)
        {
            if (v >= 0xFFFFFFF || v < 0)
                throw new ArgumentOutOfRangeException("n is over maximum allowed VLQ value");
            int q = 0;
            vlq_buf[q] = 0;
            while (v > 0)
            {
                vlq_buf[q++] = (byte)(v & 0x7F);
                v >>= 7;
            }
            while (--q > 0)
                Write((byte)(vlq_buf[q] | 0x80));
            Write(vlq_buf[0]);
        }

        /// <summary>
        /// Writes a 14-bit quantity to the current stream and advances the stream position by two bytes.
        /// </summary>
        /// <param name="v">The value to write.</param>
        public void WriteMidi14(short v)
        {
            Write((byte)((v >> 7) & 0x7F));
            Write((byte)(v & 0x7F));
        }
    }
}
