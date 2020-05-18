/*
    Copyright (c) 2019 SaladBadger

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

using LibDescent.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDescent.Data
{
    public enum ObjectType
    {
        None = -1,
        Wall = 0,
        Fireball,
        Robot,
        Hostage,
        Player,
        Weapon,
        Camera,
        Powerup,
        Debris,
        ControlCenter,
        Flare,
        Clutter,
        Ghost,
        Light,
        Coop,
        Marker
    }
    public enum ControlType
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
    }

    public enum MovementType
    {
        None = 0,
        Physics,
        Spinning = 3,
    }

    public enum RenderType
    {
        None,
        Polyobj,
        Fireball,
        Laser,
        Hostage,
        Powerup,
        Morph,
        WeaponVClip,
    }

    //Move info
    public struct PhysicsInfo
    {
        public FixVector velocity;
        public FixVector thrust;
        public int mass;
        public int drag;
        public int brakes;
        public FixVector angVel;
        public FixVector rotThrust;
        public short turnroll; //fixang
        public short flags;
    }

    //Control info
    //man wouldn't it be nice if you could have a small array in a struct
    //I guess it doesn't matter much since we're not making a million of these each frame
    public class AIInfo
    {
        public const int NumAIFlags = 11;
        public byte behavior;
        public byte flags;
        public byte[] aiFlags = new byte[NumAIFlags];
        public short hideSegment;
        public short hideIndex;
        public short pathLength;
        public short curPathIndex;
    }

    public struct ExplosionInfo
    {
        public int SpawnTime;
        public int DeleteTime;
        public short DeleteObject;
    }

    public struct PowerupInfo
    {
        public int count;
    }

    //Render info
    public class PolymodelInfo
    {
        public int modelNum;
        //*drools*
        public FixAngles[] animAngles = new FixAngles[Polymodel.MAX_SUBMODELS];
        public int flags;
        public int textureOverride;
    }

    public struct SpriteInfo
    {
        public int vclipNum;
        public int frameTime;
        public byte frameNumber;
    }

    public class LevelObject
    {
        public ObjectType type;
        public byte id;

        public ControlType controlType;
        public MovementType moveType;
        public RenderType renderType;
        public byte flags;

        public short segnum;
        public short attachedObject;

        public FixVector position;
        public FixMatrix orientation;
        public int size;
        public int shields;
        public FixVector lastPos;

        public byte containsType;
        public byte containsId;
        public byte containsCount;
        //Move info
        public PhysicsInfo physicsInfo;
        public FixVector spinRate;
        //Control info
        public AIInfo aiInfo = new AIInfo();
        public ExplosionInfo explosionInfo = new ExplosionInfo();
        public int powerupCount;
        //Render info
        public PolymodelInfo modelInfo = new PolymodelInfo();
        public SpriteInfo spriteInfo;

        public int sig;

        /// <summary>
        /// The object trigger assigned to this object. D2X-XL only.
        /// </summary>
        public D2XXLTrigger Trigger { get; set; }
    }
}
