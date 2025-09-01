/*
    Copyright (c) 2025 The LibDescent Team

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

namespace LibDescent.Data
{
    public enum ControlTypeID
    {
        None,
        AI,
        Explosion,
        Unknown3,
        Flying,
        Slew,
        Flythrough,
        Unknown7,
        Unknown8,
        Weapon,
        RepairCen,
        Morph,
        Debris,
        Powerup,
        Light,
        Remote,
        ControlCenter,
        Waypoint, // D2X-XL
    }

    public static class ControlTypeFactory
    {
        public static ControlType NewControlType(ControlTypeID id)
        {
            switch (id)
            {
                case ControlTypeID.AI:
                    return new AIControl();
                case ControlTypeID.Morph:
                    return new MorphControl();
                case ControlTypeID.Explosion:
                    return new ExplosionControl();
                case ControlTypeID.Flying:
                    return new FlyingControl();
                case ControlTypeID.Slew:
                    return new SlewControl();
                case ControlTypeID.Flythrough:
                    return new FlythroughControl();
                case ControlTypeID.Weapon:
                    return new WeaponControl();
                case ControlTypeID.RepairCen:
                    return new RepairCenterControl();
                case ControlTypeID.Debris:
                    return new DebrisControl();
                case ControlTypeID.Powerup:
                    return new PowerupControl();
                case ControlTypeID.Light:
                    return new LightControl();
                case ControlTypeID.ControlCenter:
                    return new ControlCenterControl();
                case ControlTypeID.Remote:
                    return new RemoteControl();

                case ControlTypeID.None:
                    return new NullControl();

            }
            throw new ArgumentException("ControlTypeFactory::NewControlType: bad controltype");
        }
    }

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

    //Hack for completeness. This can be used in level files but raises an Int3.
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
        public int Count { get; set; }
    }

    public class LightControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.Light;

        public Fix Intensity { get; set; }
    }

    public class WaypointControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.Waypoint;

        public int WaypointId { get; set; }
        public int NextWaypointId { get; set; }
        public int Speed { get; set; }
    }

    public class WeaponControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.Weapon;

        public short ParentType { get; set; }
        public short ParentNum { get; set; }
        public int ParentSig { get; set; }
    }

    //Empty control types
    public class NullControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.None;
    }

    public class FlyingControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.Flying;
    }

    public class SlewControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.Slew;
    }

    /// <summary>
    /// This control type is dummied and only included for completeness.
    /// </summary>
    public class FlythroughControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.Flythrough;
    }

    /// <summary>
    /// This control type is dummied and only included for completeness.
    /// </summary>
    public class RepairCenterControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.RepairCen;
    }

    /// <summary>
    /// This control type is dummied and only included for completeness.
    /// </summary>
    public class DebrisControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.Debris;
    }

    /// <summary>
    /// This control type is not useful in level definitions and only included for completeness.
    /// </summary>
    public class RemoteControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.Remote;
    }

    public class ControlCenterControl : ControlType
    {
        public override ControlTypeID Identifier => ControlTypeID.ControlCenter;
    }
}
