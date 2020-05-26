/*
    Copyright (c) 2019 The LibDescent Team

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
        Marker,
        Cambot, // D2X-XL
        Monsterball, // D2X-XL
        Smoke, // D2X-XL
        Explosion, // D2X-XL
        Effect, // D2X-XL
    }
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

    public enum MovementTypeID
    {
        None = 0,
        Physics,
        Spinning = 3,
    }

    public enum RenderTypeID
    {
        None,
        Polyobj,
        Fireball,
        Laser,
        Hostage,
        Powerup,
        Morph,
        WeaponVClip,
        Thruster, // D2X-XL: like afterburner, but doesn't cast light
        ExplosionBlast, // D2X-XL: white explosion light blast
        Shrapnel, // D2X-XL
        Particle, // D2X-XL
        Lightning, // D2X-XL
        Sound, // D2X-XL
    }

    //Move info
    public struct PhysicsInfo
    {
        public FixVector velocity;
        public FixVector thrust;
        public Fix mass;
        public Fix drag;
        public Fix brakes;
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
        public Fix SpawnTime;
        public Fix DeleteTime;
        public short DeleteObject;
    }

    public struct PowerupInfo
    {
        public int count;
    }

    // D2X-XL
    public struct WaypointInfo
    {
        public int waypointId;
        public int nextWaypointId;
        public int speed;
    }

    // D2X-XL
    public struct ParticleInfo
    {
        public int nLife;
        // nSize is a 2-element array in DLE, but the second element isn't used...
        // I guess that means we don't need it?
        public int nSize;
        public int nParts;
        public int nSpeed;
        public int nDrift;
        public int nBrightness;
        public Color color;
        public byte nSide;
        public byte nType;
        public bool enabled;
    }

    // D2X-XL
    public struct LightningInfo
    {
        public int nLife;
        public int nDelay;
        public int nLength;
        public int nAmplitude;
        public int nOffset;
        public int nWayPoint;
        public short nBolts;
        public short nId;
        public short nTarget;
        public short nNodes;
        public short nChildren;
        public short nFrames;
        public byte nWidth;
        public byte nAngle;
        public byte nStyle;
        public byte nSmoothe;
        public byte bClamp;
        public byte bPlasma;
        public byte bSound;
        public byte bRandom;
        public byte bInPlane;
        public Color color;
        public bool enabled;
    }

    // D2X-XL
    public struct SoundInfo
    {
        public string filename;
        // Expected range 0-10, indicates how loud the sound should be
        public int volume;
        public bool enabled;
    }

    //Render info
    public class PolymodelInfo
    {
        public int modelNum;
        public FixAngles[] animAngles = new FixAngles[Polymodel.MAX_SUBMODELS];
        public int flags;
        public int textureOverride;
    }

    public struct SpriteInfo
    {
        public int vclipNum;
        public Fix frameTime;
        public byte frameNumber;
    }

    public class LevelObject
    {
        public ObjectType type;
        public byte id;

        public ControlTypeID controlType;
        public MovementTypeID moveType;
        //public RenderTypeID renderType;
        public RenderTypeID RenderTypeID
        {
            get
            {
                if (RenderType == null)
                    return RenderTypeID.None;
                return RenderType.Identifier;
            }
        }

        public byte flags;

        /// <summary>
        /// Indicates if this object is only present in multiplayer modes. D2X-XL only.
        /// </summary>
        public bool MultiplayerOnly { get; set; }
        public short segnum;
        public short attachedObject;

        public FixVector position;
        public FixMatrix orientation;
        public Fix size;
        public Fix shields;
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
        public WaypointInfo waypointInfo = new WaypointInfo();
        //Render info
        public RenderType RenderType { get; set; }

        public int sig;

        /// <summary>
        /// The object trigger assigned to this object. D2X-XL only.
        /// </summary>
        public D2XXLTrigger Trigger { get; set; }
    }
}
