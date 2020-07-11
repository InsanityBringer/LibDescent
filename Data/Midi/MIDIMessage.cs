using System;
using System.Collections.Generic;
using System.Text;

namespace LibDescent.Data.Midi
{
    /// <summary>
    /// Represents a MIDI event.
    /// </summary>
    public class MIDIMessage
    {
        /// <summary>
        /// The type of this event.
        /// </summary>
        public MIDIMessageType Type { get; }
        /// <summary>
        /// The channel of this event (0-15), or -1 if not applicable.
        /// </summary>
        public int Channel { get; }

        protected MIDIMessage(MIDIMessageType type, int channel)
        {
            Type = type;
            Channel = channel;
        }
    }

    /// <summary>
    /// Represents a MIDI NoteOn, NoteOff or NoteAftertouch event.
    /// </summary>
    public class MIDINoteMessage : MIDIMessage
    {
        /// <summary>
        /// The key or note in question. Middle C (C-3) is 60, and each octave is separated by 12.
        /// </summary>
        public int Key;
        /// <summary>
        /// The velocity of the note (0-127), if NoteOn/NoteOff, or the pressure value (0-127) if NoteAfterttouch.
        /// </summary>
        public int Velocity;

        public MIDINoteMessage(MIDIMessageType type, int channel, int key, int velocity) : base(type, channel)
        {
            if (type != MIDIMessageType.NoteOn && type != MIDIMessageType.NoteOff && type != MIDIMessageType.NoteAftertouch)
                throw new ArgumentException("Invalid event type for MIDINoteEvent");
            Key = key;
            Velocity = velocity;
        }
    }

    /// <summary>
    /// Represents a MIDI ControlChange message.
    /// </summary>
    public class MIDIControlChangeMessage : MIDIMessage
    {
        /// <summary>
        /// The controller to be adjusted.
        /// </summary>
        public MIDIControl Controller;
        /// <summary>
        /// The new raw value for the controller.
        /// </summary>
        public int Value;

        public MIDIControlChangeMessage(int channel, MIDIControl controller, int value) : base(MIDIMessageType.ControlChange, channel)
        {
            Controller = controller;
            Value = value;
        }

        /// <summary>
        /// Whether the value is on or off. Used for some events, like those controlling pedals.
        /// </summary>
        public bool On => Value >= 64;
    }

    /// <summary>
    /// Represents a MIDI ProgramChange message.
    /// </summary>
    public class MIDIProgramChangeMessage : MIDIMessage
    {
        /// <summary>
        /// The program to change to.
        /// </summary>
        public byte Program;

        public MIDIProgramChangeMessage(int channel, byte program) : base(MIDIMessageType.ProgramChange, channel)
        {
            Program = program;
        }
    }

    /// <summary>
    /// Represents a MIDI ChannelAftertouch message.
    /// </summary>
    public class MIDIChannelAftertouchMessage : MIDIMessage
    {
        /// <summary>
        /// The new pressure value.
        /// </summary>
        public byte Value;

        public MIDIChannelAftertouchMessage(int channel, byte value) : base(MIDIMessageType.ChannelAftertouch, channel)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Represents a MIDI PitchBend message.
    /// </summary>
    public class MIDIPitchBendMessage : MIDIMessage
    {
        /// <summary>
        /// The new pitch value. 0x200 (512) represents standard pitch, and the value ranges from 0 to 0x400 (1024).
        /// </summary>
        public short Pitch;

        public MIDIPitchBendMessage(int channel, short pitch) : base(MIDIMessageType.PitchBend, channel)
        {
            Pitch = pitch;
        }
    }

    /// <summary>
    /// Represents a MIDI SysEx message.
    /// </summary>
    public class MIDISysExMessage : MIDIMessage
    {
        /// <summary>
        /// Whether this message continues an earlier SysEx message.
        /// </summary>
        public bool Continue;
        /// <summary>
        /// The raw message data.
        /// </summary>
        public byte[] Message;

        public MIDISysExMessage(int channel, bool cont, byte[] message) : base(MIDIMessageType.SysEx, channel)
        {
            Continue = cont;
            Message = message;
        }
    }

    /// <summary>
    /// Represents a MIDI meta text or data message.
    /// </summary>
    public class MIDIMetaMessage : MIDIMessage
    {
        /// <summary>
        /// The length of the text associated with this message.
        /// </summary>
        public int Length;
        /// <summary>
        /// The raw text data of this message.
        /// </summary>
        public byte[] Text;

        public MIDIMetaMessage(int channel, MIDIMessageType type, int length, byte[] text) : base(type, channel)
        {
            Length = length;
            Text = text;
            if (text.Length != Length)
                throw new ArgumentException("Length does not match length of text data");
        }
    }

    /// <summary>
    /// Represents a MIDI meta tempo message.
    /// </summary>
    public class MIDITempoMessage : MIDIMessage
    {
        /// <summary>
        /// Tempo in microseconds per MIDI quarter note (usually 480 ticks).
        /// </summary>
        public int Tempo;

        public MIDITempoMessage(int channel, int tempo) : base(MIDIMessageType.SetTempo, channel)
        {
            Tempo = tempo;
        }

        /// <summary>
        /// Tempo in beats per minute.
        /// </summary>
        public double BeatsPerMinute
        {
            get => 60 * (1000000.0 / Tempo);
            set => Tempo = (int)Math.Round(1000000.0 / (value / 60));
        }
    }

    /// <summary>
    /// Represents a MIDI meta time signature message.
    /// </summary>
    public class MIDITimeSignatureMessage : MIDIMessage
    {
        /// <summary>
        /// The base-two logarithm of the denominator of the time signature.
        /// </summary>
        public int DenomLog2;
        /// <summary>
        /// The numerator of the time signature.
        /// </summary>
        public int Numerator;
        /// <summary>
        /// The number of MIDI ticks in a metronome click. Usually 24.
        /// </summary>
        public int MetronomeClocks;
        /// <summary>
        /// The number of notated 32th notes in a MIDI quarter note. Usually 8.
        /// </summary>
        public int NotatedQuarterTicks;

        public MIDITimeSignatureMessage(int channel, byte n, byte d, byte c, byte b) : base(MIDIMessageType.TimeSignature, channel)
        {
            Numerator = n;
            DenomLog2 = d;
            MetronomeClocks = c;
            NotatedQuarterTicks = b;
        }

        /// <summary>
        /// The denominator of the time signature.
        /// </summary>
        public int Denominator
        {
            get => 1 << DenomLog2;
            set
            {
                int c = 0, v = value >> 1;
                while ((v >>= 1) > 0)
                    ++c;
                DenomLog2 = c;
            }
        }
    }

    /// <summary>
    /// Represents a MIDI meta key signature message.
    /// </summary>
    public class MIDIKeySignatureMessage : MIDIMessage
    {
        /// <summary>
        /// Number of sharps (if positive) or flats (if negative).
        /// </summary>
        public int SharpsFlats;
        /// <summary>
        /// Whether the key is a minor key, as opposed to a major key.
        /// </summary>
        public bool Minor;

        public MIDIKeySignatureMessage(int channel, int sf, bool mi) : base(MIDIMessageType.KeySignature, channel)
        {
            SharpsFlats = sf;
            Minor = mi;
        }
    }

    /// <summary>
    /// Represents the possible MIDI event types.
    /// </summary>
    public enum MIDIMessageType
    {
        NoteOff,
        NoteOn,
        NoteAftertouch,
        ControlChange,
        ProgramChange,
        ChannelAftertouch,
        PitchBend,
        SysEx,
        MetaText,
        MetaCopyright,
        MetaTrackName,
        MetaInstrumentName,
        MetaLyric,
        MetaMarker,
        MetaCuePoint,
        ChannelPrefix,
        EndOfTrack,
        SetTempo,
        SMPTEOffset,
        TimeSignature,
        KeySignature,
        SequencerProprietary
    }

    /// <summary>
    /// Types of MIDI control changes.
    /// </summary>
    public enum MIDIControl : byte
    {
        BankSelectMSB = 0x00,
        ModulationWheelMSB = 0x01,
        BreathControlMSB = 0x02,
        FootControlMSB = 0x04,
        PortamentoTimeMSB = 0x05,
        DataEntryMSB = 0x06,
        ChannelVolumeMSB = 0x07,
        BalanceMSB = 0x08,
        PanMSB = 0x0A,
        ExpressionControlMSB = 0x0B,
        EffectControl1MSB = 0x0C,
        EffectControl2MSB = 0x0D,
        GeneralPurposeControl1MSB = 0x10,
        GeneralPurposeControl2MSB = 0x11,
        GeneralPurposeControl3MSB = 0x12,
        GeneralPurposeControl4MSB = 0x13,

        BreathControlLSB = 0x22,
        FootControlLSB = 0x24,
        PortamentoTimeLSB = 0x25,
        DataEntryLSB = 0x26,
        ChannelVolumeLSB = 0x27,
        BalanceLSB = 0x28,
        PanLSB = 0x2A,
        ExpressionControlLSB = 0x2B,
        EffectControl1LSB = 0x2C,
        EffectControl2LSB = 0x2D,
        GeneralPurposeControl1LSB = 0x30,
        GeneralPurposeControl2LSB = 0x31,
        GeneralPurposeControl3LSB = 0x32,
        GeneralPurposeControl4LSB = 0x33,

        Sustain = 0x40,
        Portamento = 0x41,
        Sustenuto = 0x42,
        SoftPedal = 0x43,
        Legato = 0x44,
        Hold2 = 0x45,
        SoundControl1 = 0x46,
        SoundControl2 = 0x47,
        SoundControl3 = 0x48,
        SoundControl4 = 0x49,
        SoundControl5 = 0x4A,
        SoundControl6 = 0x4B,
        SoundControl7 = 0x4C,
        SoundControl8 = 0x4D,
        SoundControl9 = 0x4E,
        SoundControl10 = 0x4F,

        AllSoundOff = 0x78,
        ResetAllControl = 0x79,
        LocalControl = 0x7A,
        AllNotesOff = 0x7B,
        OmniModeOff = 0x7C,
        OmniModeOn = 0x7D,
        PolyModeOff = 0x7E,
        PolyModeOn = 0x7F
    }
}
