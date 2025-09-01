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
    public enum MovementTypeID
    {
        None = 0,
        Physics,
        Spinning = 3,
    }

    public static class MovementTypeFactory
    {
        public static MovementType NewMovementType(MovementTypeID id)
        {
            switch (id)
            {
                case MovementTypeID.Physics:
                    return new PhysicsMoveType();
                case MovementTypeID.Spinning:
                    return new SpinningMoveType();

                case MovementTypeID.None:
                    return new NullMovementType();
            }
            throw new ArgumentException("MovementTypeFactory::NewMovementType: bad movement type");
        }
    }
    public abstract class MovementType
    {
        public abstract MovementTypeID Identifier { get; }
    }

    [Flags]
    public enum PhysicsFlags
    {
        /// <summary>
        /// roll when turning
        /// </summary>
        Turnroll = 0x01,
        /// <summary>
        /// level object with closest side
        /// </summary>
        Levelling = 0x02,
        /// <summary>
        /// bounce (not slide) when hit wall
        /// </summary>
        Bounce = 0x04,
        /// <summary>
        /// wiggle while flying (players only)
        /// </summary>
        Wiggle = 0x08,
        /// <summary>
        /// object sticks (stops moving) when hits wall
        /// </summary>
        Stick = 0x10,
        /// <summary>
        /// object keeps going even after it hits another object (eg, fusion cannon)
        /// </summary>
        Persistent = 0x20,
        /// <summary>
        /// this object uses its thrust
        /// </summary>
        UsesThrust = 0x40
    }

    public class PhysicsMoveType : MovementType
    {
        public override MovementTypeID Identifier => MovementTypeID.Physics; 

        public FixVector Velocity { get; set; }
        public FixVector Thrust { get; set; }
        public Fix Mass { get; set; }
        public Fix Drag { get; set; }
        public Fix Brakes { get; set; }
        public FixVector AngularVel { get; set; }
        public FixVector RotationalThrust { get; set; }
        public short Turnroll { get; set; } //fixang
        public PhysicsFlags Flags { get; set; }
    }

    public class SpinningMoveType : MovementType
    {
        public override MovementTypeID Identifier => MovementTypeID.Spinning;

        public FixVector SpinRate { get; set; }
    }

    public class NullMovementType : MovementType
    {
        public override MovementTypeID Identifier => MovementTypeID.None;
    }
}
