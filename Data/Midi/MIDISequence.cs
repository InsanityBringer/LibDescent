/*
    Copyright (c) 2020 The LibDescent Team.

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

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
        private MIDISMPTEFrameRate tickRateFrames;

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
        /// Loads a MIDI sequence from a stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
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
                    int fr = -(division >> 8);
                    switch (fr)
                    {
                        case 24:
                        default:
                            tickRateFrames = MIDISMPTEFrameRate.F24;
                            break;
                        case 25:
                            tickRateFrames = MIDISMPTEFrameRate.F25;
                            break;
                        case 29:
                            tickRateFrames = MIDISMPTEFrameRate.F30Drop;
                            break;
                        case 30:
                            tickRateFrames = MIDISMPTEFrameRate.F30;
                            break;
                    }
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

                    ReadMIDITrack(i, trk, trackData);
                    Tracks.Add(trk);
                }
            }
        }

        /// <summary>
        /// Loads a HMP sequence from a stream.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        public void ReadHMP(Stream stream)
        {
            using (BinaryReaderHMP br = new BinaryReaderHMP(stream))
            {
                stream.Seek(0, SeekOrigin.Begin);
                if (Encoding.ASCII.GetString(br.ReadBytes(8)) != "HMIMIDIP")
                    throw new ArgumentException("Not a valid HMP");

                Format = MIDIFormat.HMI;

                //Offsets directly derived from Chocolate Descent source
                br.BaseStream.Seek(32, SeekOrigin.Begin);
                int size = br.ReadInt32();

                tickRateSMPTE = false;
                br.BaseStream.Seek(48, SeekOrigin.Begin);
                int nTracks = br.ReadInt32();
                //tickRateQuarterTicks = br.ReadInt32();
                tickRateQuarterTicks = 60;
                int bpm = br.ReadInt32();
                int seconds = br.ReadInt32(); //for debugging, I suppose.

                Tracks.Clear();
                br.BaseStream.Seek(776, SeekOrigin.Begin); //The meaning of this block of bytes hasn't been documented ever.
                for (int i = 0; i < nTracks; ++i)
                {
                    MIDITrack trk = new MIDITrack();
                    int chunkNum = br.ReadInt32();
                    int trackLength = br.ReadInt32();
                    trackLength -= 12; //track length in HMP includes the header, for some reason.
                    int trackNum = br.ReadInt32(); //descent2.com docs imply this is needed for loops (must be on track 1), but it's unclear if it's actually true from observation.
                    byte[] trackData = br.ReadBytes(trackLength); 
                    if (trackData.Length < trackLength)
                        throw new EndOfStreamException();

                    if (i == 0) // add tempo event to first track
                        trk.AddEvent(new MIDIEvent(0, new MIDITempoMessage(0, (double)bpm)));

                    ReadHMPTrack(i, trk, trackData);
                    Tracks.Add(trk);
                }
            }
        }

        private void ReadMIDITrack(int trackNum, MIDITrack trk, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReaderMIDI br = new BinaryReaderMIDI(ms))
            {
                ReadMIDITrackInternal(trackNum, trk, br);
            }
        }

        private void ReadHMPTrack(int trackNum, MIDITrack trk, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReaderHMP br = new BinaryReaderHMP(ms))
            {
                ReadMIDITrackInternal(trackNum, trk, br);
            }
        }

        private void ReadMIDITrackInternal(int trackNum, MIDITrack trk, IMIDIReader br)
        {
            byte status = 0;
            ulong position = 0;
            ulong delta;
            MIDIMessage evt;
            int metaChannel = -1;
            while (ReadMIDIMessage(trackNum, br, ref status, ref metaChannel, out delta, out evt))
            {
                position += delta;
                if (evt != null)
                    trk.AddEvent(new MIDIEvent(position, evt));
            }
            if (evt != null)
                trk.AddEvent(new MIDIEvent(position, evt));
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

        private static readonly int[] smpteFrameCount = new int[] { 24, 25, 29, 30 };

        private bool ReadMIDIMessage(int trackNum, IMIDIReader br, ref byte status, ref int metaChannel, out ulong delta, out MIDIMessage evt)
        {
            byte tmp;
            delta = (ulong)br.ReadVLQ();
            tmp = br.ReadByte();
            if ((tmp & 0x80) == 0x80)
                status = tmp;
            else                            // rewind by one byte
                br.Rewind(1);

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
                        case 0:
                            tmp = br.ReadByte();
                            if (tmp == 0)
                            {
                                evt = new MIDISequenceNumberMessage(metaChannel, trackNum);
                                return true;
                            }
                            else if (tmp == 2)
                            {
                                evt = new MIDISequenceNumberMessage(metaChannel, br.ReadMidi14());
                                return true;
                            }
                            br.Rewind(1);
                            goto default;
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
                                int channelNum = br.ReadByte();
                                metaChannel = channelNum & 15;
                                evt = null;
                                return true;
                            }
                            br.Rewind(1);
                            goto default;
                        case 0x2F:
                            if (br.ReadByte() == 0)
                            {
                                evt = new MIDIEndOfTrackMessage(metaChannel);
                                return false;
                            }
                            br.Rewind(1);
                            goto default;
                        case 0x51:
                            if (br.ReadByte() == 3)
                            {
                                int tempo = 0;
                                tempo |= (br.ReadByte() << 16);
                                tempo |= (br.ReadByte() << 8);
                                tempo |= br.ReadByte();
                                if (tempo == 0)
                                    tempo = 500000;
                                evt = new MIDITempoMessage(metaChannel, tempo);
                                return true;
                            }
                            br.Rewind(1);
                            goto default;
                        case 0x54:
                            if (br.ReadByte() == 5)
                            {
                                int hours = br.ReadByte();
                                MIDISMPTEFrameRate rate = (MIDISMPTEFrameRate)((hours >> 5) & 3);
                                hours &= 31;
                                int minutes = br.ReadByte() & 127;
                                int seconds = br.ReadByte() & 127;
                                int frames = br.ReadByte() & 127;
                                int fracFrames = br.ReadByte() & 127;

                                evt = new MIDISMPTEOffsetMessage(metaChannel, rate, hours, minutes, seconds, frames, fracFrames);
                                return true;
                            }
                            br.Rewind(1);
                            goto default;
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
                            br.Rewind(1);
                            goto default;
                        case 0x59:
                            if (br.ReadByte() == 2)
                            {
                                byte sf = br.ReadByte();
                                byte mi = br.ReadByte();
                                evt = new MIDIKeySignatureMessage(metaChannel, sf, mi > 0);
                                return true;
                            }
                            br.Rewind(1);
                            goto default;
                        case 0x7F:
                            seqlen = br.ReadVLQ();
                            evt = new MIDISequencerProprietaryMessage(metaChannel, br.ReadBytes(seqlen));
                            return true;
                        default:
                            // try to skip unknown meta event
                            seqlen = br.ReadVLQ();
                            br.Skip(seqlen);
                            evt = null;
                            return true;
                    }
                    break;
            }

            evt = null;
            return false;
        }

        /// <summary>
        /// Loads a MIDI sequence from a file.
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
        /// Loads a MIDI sequence from an array.
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

        /// <summary>
        /// Loads a HMP sequence from a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns></returns>
        public void ReadHMP(string filePath)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open))
            {
                ReadHMP(fs);
            }
        }

        /// <summary>
        /// Loads a HMP sequence from an array.
        /// </summary>
        /// <param name="contents">The array to load from.</param>
        /// <returns></returns>
        public void ReadHMP(byte[] contents)
        {
            using (MemoryStream ms = new MemoryStream(contents))
            {
                ReadHMP(ms);
            }
        }

        private byte[] WriteMIDITrack(int trackNum, MIDITrack trk)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriterMIDI bw = new BinaryWriterMIDI(ms))
            {
                trk.TerminateTrack();
                WriteMIDITrackInternal(trackNum, trk, bw);
                return ms.ToArray();
            }
        }

        private void WriteMIDITrackInternal(int trackNum, MIDITrack trk, IMIDIWriter bw)
        {
            ulong position = 0;
            ulong delta;
            int metaChannel = -1;
            byte status = 0;
            foreach (MIDIEvent evt in trk)
            {
                delta = evt.Time - position;
                position = evt.Time;
                bw.WriteVLQ((int)delta);
                WriteMIDIMessage(trackNum, bw, evt.Data, ref status, ref metaChannel);
            }
        }

        /// <summary>
        /// Writes a MIDI sequence to a stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public void Write(Stream stream)
        {
            int trackNum = 0;
            if (Format == MIDIFormat.HMI)
            {
                // TODO
            }
            else
            {
                if (Format == MIDIFormat.Type0 && TrackCount > 1)
                {
                    throw new ArgumentException("Cannot save Type-0 MIDI with multiple tracks; merge tracks or save as Type-1");
                }

                using (BinaryWriterMIDI bw = new BinaryWriterMIDI(stream))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    bw.Write(Encoding.ASCII.GetBytes("MThd"));
                    bw.Write(6);
                    bw.Write((short)Format);
                    bw.Write((short)TrackCount);
                    if (tickRateSMPTE)
                        bw.Write((short)((((-smpteFrameCount[(int)tickRateFrames]) << 8)) | tickRateFrameTicks));
                    else
                        bw.Write((short)(tickRateQuarterTicks));

                    foreach (MIDITrack track in Tracks)
                    {
                        bw.Write(Encoding.ASCII.GetBytes("MTrk"));
                        byte[] trackData = WriteMIDITrack(trackNum++, track);
                        bw.Write(trackData.Length);
                        bw.Write(trackData);
                    }
                }
            }
        }

        /// <summary>
        /// Writes a MIDI sequence from a file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns></returns>
        public void Write(string filePath)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                Write(fs);
            }
        }

        /// <summary>
        /// Writes a MIDI sequence from an array.
        /// </summary>
        /// <param name="contents">The array to load from.</param>
        /// <returns></returns>
        public byte[] Write()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Write(ms);
                return ms.ToArray();
            }
        }

        private void WriteMIDIMessage(int trackNum, IMIDIWriter bw, MIDIMessage message, ref byte status, ref int metaChannel)
        {
            byte newStatus;
            switch (message.Type)
            {
                case MIDIMessageType.NoteOff:
                    metaChannel = message.Channel;
                    newStatus = (byte)(0x80 | message.Channel);
                    if (newStatus != status)
                        bw.Write(status = newStatus);
                    bw.Write((byte)((message as MIDINoteMessage).Key));
                    bw.Write((byte)((message as MIDINoteMessage).Velocity));
                    return;
                case MIDIMessageType.NoteOn:
                    metaChannel = message.Channel;
                    newStatus = (byte)(0x90 | message.Channel);
                    if (newStatus != status)
                        bw.Write(status = newStatus);
                    bw.Write((byte)((message as MIDINoteMessage).Key));
                    bw.Write((byte)((message as MIDINoteMessage).Velocity));
                    return;
                case MIDIMessageType.NoteAftertouch:
                    metaChannel = message.Channel;
                    newStatus = (byte)(0xA0 | message.Channel);
                    if (newStatus != status)
                        bw.Write(status = newStatus);
                    bw.Write((byte)((message as MIDINoteMessage).Key));
                    bw.Write((byte)((message as MIDINoteMessage).Velocity));
                    return;
                case MIDIMessageType.ControlChange:
                    metaChannel = message.Channel;
                    newStatus = (byte)(0xB0 | message.Channel);
                    if (newStatus != status)
                        bw.Write(status = newStatus);
                    bw.Write((byte)((message as MIDIControlChangeMessage).Controller));
                    bw.Write((byte)((message as MIDIControlChangeMessage).Value));
                    return;
                case MIDIMessageType.ProgramChange:
                    metaChannel = message.Channel;
                    newStatus = (byte)(0xC0 | message.Channel);
                    if (newStatus != status)
                        bw.Write(status = newStatus);
                    bw.Write((byte)((message as MIDIProgramChangeMessage).Program));
                    return;
                case MIDIMessageType.ChannelAftertouch:
                    metaChannel = message.Channel;
                    newStatus = (byte)(0xD0 | message.Channel);
                    if (newStatus != status)
                        bw.Write(status = newStatus);
                    bw.Write((byte)((message as MIDIChannelAftertouchMessage).Value));
                    return;
                case MIDIMessageType.PitchBend:
                    metaChannel = message.Channel;
                    newStatus = (byte)(0xE0 | message.Channel);
                    if (newStatus != status)
                        bw.Write(status = newStatus);
                    bw.WriteMidi14((message as MIDIPitchBendMessage).Pitch);
                    return;
                case MIDIMessageType.EndOfTrack:
                    bw.Write(status = (byte)0xFF);
                    bw.Write((byte)0x2F);
                    bw.WriteVLQ(0);
                    return;
            }

            // metadata event
            int msgChannel = message.Channel;
            if (msgChannel >= 0 && msgChannel != metaChannel)
            {
                bw.Write((byte)0xFF);
                bw.Write((byte)0x20);
                bw.WriteVLQ(1);
                bw.Write((byte)msgChannel);
                metaChannel = msgChannel;
            }

            switch (message.Type)
            {
                case MIDIMessageType.SysEx:
                    var sysex = message as MIDISysExMessage;
                    bw.Write(status = (byte)(sysex.Continue ? 0xF7 : 0xF0));
                    bw.WriteVLQ(sysex.Message.Length);
                    bw.Write(sysex.Message);
                    return;
                case MIDIMessageType.SequenceNumber:
                    var seqnex = message as MIDISequenceNumberMessage;
                    bw.Write(status = (byte)0xFF);
                    if (seqnex.Sequence == trackNum)
                        bw.WriteVLQ(0);
                    {
                        bw.WriteVLQ(2);
                        bw.WriteMidi14((short)seqnex.Sequence);
                    }
                    return;
                case MIDIMessageType.MetaText:
                case MIDIMessageType.MetaCopyright:
                case MIDIMessageType.MetaTrackName:
                case MIDIMessageType.MetaInstrumentName:
                case MIDIMessageType.MetaLyric:
                case MIDIMessageType.MetaMarker:
                case MIDIMessageType.MetaCuePoint:
                    var meta = message as MIDIMetaMessage;
                    bw.Write(status = (byte)0xFF);
                    bw.Write((byte)(midiMetaTypes.IndexOf(message.Type)));
                    bw.WriteVLQ(meta.Data.Length);
                    bw.Write(meta.Data);
                    return;
                case MIDIMessageType.SetTempo:
                    var tempo = message as MIDITempoMessage;
                    bw.Write(status = (byte)0xFF);
                    bw.Write((byte)0x51);
                    bw.WriteVLQ(3);
                    bw.Write((byte)((tempo.Tempo >> 16) & 0xFF));
                    bw.Write((byte)((tempo.Tempo >> 8) & 0xFF));
                    bw.Write((byte)(tempo.Tempo & 0xFF));
                    return;
                case MIDIMessageType.SMPTEOffset:
                    var smpte = message as MIDISMPTEOffsetMessage;
                    bw.Write(status = (byte)0xFF);
                    bw.Write((byte)0x54);
                    bw.WriteVLQ(5);
                    bw.Write((byte)((((int)smpte.FrameRate) << 5) | smpte.Hours));
                    bw.Write((byte)(smpte.Minutes));
                    bw.Write((byte)(smpte.Seconds));
                    bw.Write((byte)(smpte.Frames));
                    bw.Write((byte)(smpte.FractionalFrames));
                    return;
                case MIDIMessageType.TimeSignature:
                    var ts = message as MIDITimeSignatureMessage;
                    bw.Write(status = (byte)0xFF);
                    bw.Write((byte)0x58);
                    bw.WriteVLQ(4);
                    bw.Write((byte)(ts.Numerator));
                    bw.Write((byte)(ts.DenomLog2));
                    bw.Write((byte)(ts.MetronomeClocks));
                    bw.Write((byte)(ts.NotatedQuarterTicks));
                    return;
                case MIDIMessageType.KeySignature:
                    var ks = message as MIDIKeySignatureMessage;
                    bw.Write(status = (byte)0xFF);
                    bw.Write((byte)0x59);
                    bw.WriteVLQ(2);
                    bw.Write((byte)(ks.SharpsFlats));
                    bw.Write((byte)(ks.Minor ? 1 : 0));
                    return;
                case MIDIMessageType.SequencerProprietary:
                    var seqp = message as MIDISequencerProprietaryMessage;
                    bw.Write(status = (byte)0xFF);
                    bw.Write((byte)0x7F);
                    bw.WriteVLQ(seqp.Data.Length);
                    bw.Write(seqp.Data);
                    return;
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
        /// Ensures there is only one end-of-track event and that it is the final event on the track.
        /// </summary>
        public void TerminateTrack()
        {
            List<MIDIEvent> evts = new List<MIDIEvent>();
            evts.AddRange(GetAllEvents());
            for (int i = 0; i < evts.Count - 1; ++i)
            {
                if (evts[i].Data.Type == MIDIMessageType.EndOfTrack)
                {
                    // remove all end-of-track events
                    foreach (MIDIInstant instant in tree)
                    {
                        instant.Messages.RemoveAll(m => m.Type == MIDIMessageType.EndOfTrack);
                    }
                    break;
                }
            }
            if (evts.Count < 1 || evts.Last().Data.Type != MIDIMessageType.EndOfTrack)
                AddEvent(new MIDIEvent(tree.Max.Time, new MIDIEndOfTrackMessage(-1)));
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
    public enum MIDIFormat : short
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
        /// <summary>
        /// HMI format. Modified version of Type 1 MIDI.
        /// </summary>
        HMI = 3,
    }

    /// <summary>
    /// Represents an SMPTE frame rate.
    /// </summary>
    public enum MIDISMPTEFrameRate
    {
        /// <summary>
        /// 24 frames per second.
        /// </summary>
        F24 = 0,
        /// <summary>
        /// 25 frames per second.
        /// </summary>
        F25 = 1,
        /// <summary>
        /// 29.97 frames per second.
        /// </summary>
        F30Drop = 2,
        /// <summary>
        /// 30 frames per second.
        /// </summary>
        F30 = 3
    }

    /// <summary>
    /// Common interface for MIDI and HMP readers.
    /// </summary>
    public interface IMIDIReader
    {
        byte ReadByte();
        byte[] ReadBytes(int length);
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();
        int ReadVLQ();
        short ReadMidi14();
        /// <summary>
        /// Rewinds the stream of the reader back by a given number of bytes.
        /// </summary>
        /// <param name="bytes">The number of bytes to rewind.</param>
        void Rewind(int bytes);
        /// <summary>
        /// Skips bytes from the stream.
        /// </summary>
        /// <param name="bytes">The number of bytes to skips.</param>
        void Skip(int bytes);
    }

    /// <summary>
    /// Common interface for MIDI and HMP writers.
    /// </summary>
    public interface IMIDIWriter
    {
        void Write(byte n);
        void Write(byte[] data);
        void Write(short n);
        void Write(int n);
        void Write(long n);
        void WriteVLQ(int v);
        void WriteMidi14(short v);
    }

    /// <summary>
    /// A BinaryReader intended for use with MIDI files. Reads big-endian data and has a method for reading MIDI VLQs (variable-length quantity).
    /// </summary>
    public class BinaryReaderMIDI : BinaryReaderBE, IMIDIReader
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

        public void Rewind(int bytes)
        {
            BaseStream.Position -= bytes;
        }

        public void Skip(int bytes)
        {
            BaseStream.Position += bytes;
        }
    }

    /// <summary>
    /// A BinaryReader intended for use with HMP files. Reads little-endian data and has a method for reading HMP VLQs (variable-length quantity).
    /// </summary>
    public class BinaryReaderHMP : BinaryReader, IMIDIReader
    {
        public BinaryReaderHMP(Stream stream) : base(stream) { }

        /// <summary>
        /// Reads a HMP variable length quantity (VLQ) to the current stream and advances the stream position accordingly.
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
            } while ((b & 0x80) == 0); //HMI inverts the meaning of 0x80 in delta encodings.
            return r;
        }

        /// <summary>
        /// Reads a 14-bit quantity to the current stream and advances the stream position by two bytes.
        /// </summary>
        /// <returns>The 14-bit quantity read from the stream.</returns>
        public short ReadMidi14()
        {
            return (short)((ReadByte() & 0x7F) | ((ReadByte() & 0x7F) << 7));
        }

        public void Rewind(int bytes)
        {
            BaseStream.Position -= bytes;
        }

        public void Skip(int bytes)
        {
            BaseStream.Position += bytes;
        }
    }

    /// <summary>
    /// A BinaryWriter intended for use with MIDI files. Writes big-endian data and has a method for writing MIDI VLQs (variable-length quantity).
    /// </summary>
    public class BinaryWriterMIDI : BinaryWriterBE, IMIDIWriter
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
