using System;
using System.Collections.Generic;
using System.Text;

namespace LibDescent.Data
{
    public abstract class ControlType
    {
        public abstract ControlTypeID Identifier { get; }
    }

    public class AIControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.AI;

        public const int NumAIFlags = 11;
        
        public byte Behavior { get; set; }
        public byte Flags { get; set; }
        public byte[] AIFlags { get; } = new byte[NumAIFlags];
        public short HideSegment { get; set; }
        public short HideIndex { get; set; }
        public short PathLength { get; set; }
        public short CurPathIndex { get; set; }
    }

    //Hack for completeness
    public class MorphControl : AIControl
    {
        public override ControlTypeID Identifier => ControlTypeID.Morph;
    }

    public class ExplosionControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.Explosion;

        public Fix SpawnTime { get; set; }
        public Fix DeleteTime { get; set; }
        public short DeleteObject { get; set; }
    }

    public class PowerupControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.Powerup;
        public int count;
    }
}
