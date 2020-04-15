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

using System;

namespace LibDescent.Data
{
    public enum RobotCloakType
    {
        None,
        Always,
        OnlyWhenFiring
    }
    public enum RobotAttackType
    {
        Ranged,
        Melee
    }
    public enum RobotBossType
    {
        NoBoss,
        Descent1Level7,
        Descent1Level27,

        RedFatty = 20,
        WaterBoss,
        FireBoss,
        IceBoss,
        Alien1Boss,
        Alien2Boss,
        VertigoBoss1,
        VertigoBoss2
        
    }
    public enum RobotAIType
    {
        Invalid,

        Still = 0x80,
        Normal,
        Sneak,
        Flee,
        Station,
        Sniper,
        Follow
    }
    public class Robot
    {
        public const int NUM_ANIMATION_STATES = 5;
        public const int NUM_DIFFICULTY_LEVELS = 5;
        //important crap
        public struct JointList
        {
            public short n_joints;
            public short offset;
        }

        /// <summary>
        /// which polygon model?
        /// </summary>
        public int ModelNum { get; set; }
        /// <summary>
        /// where each gun model is
        /// </summary>
        public FixVector[] GunPoints { get; } = new FixVector[Polymodel.MAX_GUNS];
        /// <summary>
        /// which submodel is each gun in?
        /// </summary>
        public byte[] GunSubmodels { get; } = new byte[Polymodel.MAX_GUNS];
        /// <summary>
        /// VClip shown when robot is hit.
        /// </summary>
        public short HitVClipNum { get; set; }
        /// <summary>
        /// Sound played when robot is hit.
        /// </summary>
        public short HitSoundNum { get; set; }
        /// <summary>
        /// VClip shown when robot is killed.
        /// </summary>
        public short DeathVClipNum { get; set; }
        /// <summary>
        /// Sound played when robot is killed.
        /// </summary>
        public short DeathSoundNum { get; set; }
        /// <summary>
        /// ID of weapon type fired.
        /// </summary>
        public sbyte WeaponType { get; set; }
        /// <summary>
        /// Secondary weapon number, -1 means none, otherwise gun #0 fires this weapon.
        /// </summary>
        public sbyte WeaponTypeSecondary { get; set; }
        /// <summary>
        /// how many different gun positions
        /// </summary>
        public sbyte NumGuns { get; set; }
        /// <summary>
        /// ID of powerup this robot can contain.
        /// </summary>
        public sbyte ContainsID { get; set; }
        /// <summary>
        /// Max number of things this instance can contain.
        /// </summary>
        public sbyte ContainsCount { get; set; }
        /// <summary>
        /// Probability that this instance will contain something in N/16
        /// </summary>
        public sbyte ContainsProbability { get; set; }
        /// <summary>
        /// Type of thing contained, robot or powerup, in bitmaps.tbl, 2=robot, 7=powerup. 
        /// </summary>
        public sbyte ContainsType { get; set; } //[ISB] Should be instance of ObjectType tbh
        /// <summary>
        /// !0 means commits suicide when hits you, strength thereof. 0 means no.
        /// </summary>
        public sbyte Kamikaze { get; set; }
        /// <summary>
        /// Score from this robot.
        /// </summary>
        public short ScoreValue { get; set; }
        /// <summary>
        /// Dies with badass explosion, and strength thereof, 0 means NO.
        /// </summary>
        public byte DeathExplosionRadius { get; set; }
        /// <summary>
        /// Points of energy drained at each collision.
        /// </summary>
        public byte EnergyDrain { get; set; }
        /// <summary>
        /// (Unused) should this be here or with polygon model?
        /// </summary>
        public Fix Lighting { get; set; }
        /// <summary>
        /// Initial shields of robot
        /// </summary>
        public Fix Strength { get; set; }
        /// <summary>
        /// how heavy is this thing?
        /// </summary>
        public Fix Mass { get; set; }
        /// <summary>
        /// how much drag does it have?
        /// </summary>
        public Fix Drag { get; set; }
        /// <summary>
        /// compare this value with forward_vector.dot.vector_to_player, if field_of_view <, then robot can see player
        /// </summary>
        public Fix[] FieldOfView { get; } = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// time in seconds between shots
        /// </summary>
        public Fix[] FiringWait { get; } = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// time in seconds between shots. For gun 2
        /// </summary>
        public Fix[] FiringWaitSecondary { get; } = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// time in seconds to rotate 360 degrees in a dimension
        /// </summary>
        public Fix[] TurnTime { get; } = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// (Unused) Descent 1 serialization data.
        /// </summary>
        public Fix[] FirePower { get; } = new Fix[NUM_DIFFICULTY_LEVELS]; //Descent 1 waste data
        /// <summary>
        /// (Unused) Descent 1 serialization data.
        /// </summary>
        public Fix[] Shield { get; } = new Fix[NUM_DIFFICULTY_LEVELS]; //Retained for serialization reasons. 
        /// <summary>
        /// maximum speed attainable by this robot
        /// </summary>
        public Fix[] MaxSpeed { get; } = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// distance at which robot circles player
        /// </summary>
        public Fix[] CircleDistance { get; } = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// number of shots fired rapidly
        /// </summary>
        public sbyte[] RapidfireCount { get; } = new sbyte[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// rate at which robot can evade shots, 0=none, 4=very fast
        /// </summary>
        public sbyte[] EvadeSpeed { get; } = new sbyte[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// 0=never, 1=always, 2=except-when-firing
        /// </summary>
        public RobotCloakType CloakType { get; set; } //was sbyte
        /// <summary>
        /// 0=firing, 1=charge (like green guy)
        /// </summary>
        public RobotAttackType AttackType { get; set; } //was sbyte
        /// <summary>
        /// sound robot makes when it first sees the player
        /// </summary>
        public byte SeeSound { get; set; }
        /// <summary>
        /// sound robot makes when it attacks the player
        /// </summary>
        public byte AttackSound { get; set; }
        /// <summary>
        /// sound robot makes as it claws you (attack_type should be 1)
        /// </summary>
        public byte ClawSound { get; set; }
        /// <summary>
        /// sound robot makes after you die if this code was ever actually enabled and used.
        /// </summary>
        public byte TauntSound { get; set; }
        /// <summary>
        /// 0 = not boss, 1 = boss.  Is that surprising?
        /// </summary>
        public RobotBossType BossFlag; //was sbyte
        /// <summary>
        /// Companion robot, leads you to things.
        /// </summary>
        public bool Companion { get; set; } //was sbyte
        /// <summary>
        /// how many smart blobs are emitted when this guy dies! if this code was ever actually enabled and used.
        /// </summary>
        public sbyte SmartBlobsOnDeath { get; set; }
        /// <summary>
        /// how many smart blobs are emitted when this guy gets hit by energy weapon!
        /// </summary>
        public sbyte SmartBlobsOnHit { get; set; }
        /// <summary>
        /// !0 means this guy can steal when he collides with you!
        /// </summary>
        public bool Thief { get; set; } //was sbyte
        /// <summary>
        /// !0 means pursues player after he goes around a corner.  4 = 4/2 pursue up to 4/2 seconds after becoming invisible if up to 4 segments away
        /// </summary>
        public sbyte Pursuit { get; set; }
        /// <summary>
        /// Amount of light cast. 1 is default.  10 is very large.
        /// </summary>
        public sbyte LightCast { get; set; }
        /// <summary>
        /// 0 = dies without death roll. !0 means does death roll, larger = faster and louder
        /// </summary>
        public sbyte DeathRollTime { get; set; }
        /// <summary>
        /// misc properties, and by that a single flag that is literally not used in the final game. heeeh
        /// </summary>
        public byte	Flags { get; set; }
        //three bytes pad
        /// <summary>
        /// if has deathroll, what sound?
        /// </summary>
        public byte DeathRollSound { get; set; }
        /// <summary>
        /// apply this light to robot itself. stored as 4:4 fixed-point
        /// </summary>
        public byte Glow { get; set; }
        /// <summary>
        /// Default behavior.
        /// </summary>
        public RobotAIType Behavior { get; set; }
        /// <summary>
        /// 255 = perfect, less = more likely to miss.  0 != random, would look stupid.  0=45 degree spread. 
        /// </summary>
        public byte Aim { get; set; }

        /// <summary>
        /// animation info
        /// </summary>
        public JointList[,] AnimStates { get; } = new JointList[Polymodel.MAX_GUNS + 1, NUM_ANIMATION_STATES];
        public int baseJoint = 0; //for HXM files aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa

        /// <summary>
        /// debugging. Retained for serializaiton purposes. 
        /// </summary>
        public int Always0xABCD { get; set; }

        public int replacementID;
        public int ID;

        public Robot()
        {
            Always0xABCD = 0xABCD;
            ContainsType = 7;
            WeaponTypeSecondary = -1;
        }

        public Robot(Robot other)
        {
            ModelNum = other.ModelNum;
            HitVClipNum = other.HitVClipNum;
            HitSoundNum = other.HitSoundNum;
            DeathVClipNum = other.DeathVClipNum;
            DeathSoundNum = other.DeathSoundNum;

            WeaponType = other.WeaponType;
            WeaponTypeSecondary = other.WeaponTypeSecondary;

            ContainsCount = other.ContainsCount;
            ContainsID = other.ContainsID;
            ContainsProbability = other.ContainsProbability;
            ContainsType = other.ContainsType;

            Kamikaze = other.Kamikaze;
            ScoreValue = other.ScoreValue;
            DeathExplosionRadius = other.DeathExplosionRadius;
            EnergyDrain = other.EnergyDrain;

            Lighting = other.Lighting;
            Strength = other.Strength;

            Mass = other.Mass;
            Drag = other.Drag;

            for (int i = 0; i < NUM_DIFFICULTY_LEVELS; i++)
            {
                FieldOfView[i] = other.FieldOfView[i];
                FiringWait[i] = other.FiringWait[i];
                FiringWaitSecondary[i] = other.FiringWaitSecondary[i];
                TurnTime[i] = other.TurnTime[i];
                MaxSpeed[i] = other.MaxSpeed[i];
                CircleDistance[i] = other.CircleDistance[i];

                RapidfireCount[i] = other.RapidfireCount[i];
                EvadeSpeed[i] = other.EvadeSpeed[i];
            }

            CloakType = other.CloakType;
            AttackType = other.AttackType;

            SeeSound = other.SeeSound;
            AttackSound = other.AttackSound;
            ClawSound = other.ClawSound;
            TauntSound = other.TauntSound;

            BossFlag = other.BossFlag;
            Companion = other.Companion;
            SmartBlobsOnDeath = other.SmartBlobsOnDeath;
            SmartBlobsOnHit = other.SmartBlobsOnHit;

            Thief = other.Thief;
            Pursuit = other.Pursuit;
            LightCast = other.LightCast;
            DeathRollTime = other.DeathRollTime;

            Flags = other.Flags;

            DeathRollSound = other.DeathRollSound;
            Glow = other.Glow;
            Behavior = other.Behavior;
            Aim = other.Aim;

            Always0xABCD = 0xabcd;
        }

        public bool UpdateRobot(int tag, ref int value, int curAI, int curGun)
        {
            bool clamped = false;
            switch (tag)
            {
                case 1:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    ModelNum = value;
                    break;
                case 2:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    HitVClipNum = (short)value;
                    break;
                case 3:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    HitSoundNum = (short)value;
                    break;
                case 4:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    DeathVClipNum = (short)value;
                    break;
                case 5:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    WeaponType = (sbyte)value;
                    break;
                case 6:
                    WeaponTypeSecondary = (sbyte)(value - 1);
                    break;
                case 7:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    NumGuns = (sbyte)value;
                    break;
                case 8:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    ContainsID = (sbyte)value;
                    break;
                case 9:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    ContainsCount = (sbyte)value;
                    break;
                case 10:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    ContainsProbability = (sbyte)value;
                    break;
                /*case 11:
                    value = Utilites.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    contains_type = (sbyte)value;
                    break;*/ //don't use doesn't update rferences
                case 12:
                    if (value == 0)
                        ClawSound = 255;
                    else
                        ClawSound = (byte)(value);
                    break;
                case 13:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    ScoreValue = (short)value;
                    break;
                case 14:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    DeathExplosionRadius = (byte)value;
                    break;
                case 15:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    EnergyDrain = (byte)value;
                    break;
                case 16:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    Lighting = new Fix(value);
                    break;
                case 17:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    Strength = new Fix(value);
                    break;
                case 18:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    Mass = new Fix(value);
                    break;
                case 19:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    Drag = new Fix(value);
                    break;
                case 20:
                    value = (int)(Math.Cos(value * Math.PI / 180.0D) * 65536.0);
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    FieldOfView[curAI] = new Fix(value);
                    break;
                case 21:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    FiringWait[curAI] = new Fix(value);
                    break;
                case 22:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    FiringWaitSecondary[curAI] = new Fix(value);
                    break;
                case 23:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    TurnTime[curAI] = new Fix(value);
                    break;
                case 24:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    MaxSpeed[curAI] = new Fix(value);
                    break;
                case 25:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    CircleDistance[curAI] = new Fix(value);
                    break;
                case 26:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    RapidfireCount[curAI] = (sbyte)value;
                    break;
                case 27:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    EvadeSpeed[curAI] = (sbyte)value;
                    break;
                case 30:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        SeeSound = 255;
                    else
                        SeeSound = (byte)(value);
                    break;
                case 31:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        AttackSound = 255;
                    else
                        AttackSound = (byte)(value);
                    break;
                case 33:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        TauntSound = 255;
                    else
                        TauntSound = (byte)(value);
                    break;
                case 34:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        DeathRollSound = 255;
                    else
                        DeathRollSound = (byte)(value);
                    break;
                case 36:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    SmartBlobsOnDeath = (sbyte)value;
                    break;
                case 37:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    SmartBlobsOnHit = (sbyte)value;
                    break;
                case 38:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    Pursuit = (sbyte)value;
                    break;
                case 39:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    LightCast = (sbyte)value;
                    break;
                case 40:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        HitSoundNum = 255;
                    else
                        HitSoundNum = (byte)(value);
                    break;
                case 41:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        DeathSoundNum = 255;
                    else
                        DeathSoundNum = (byte)(value);
                    break;
                case 43:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    Glow = (byte)value;
                    break;
                case 45:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    Aim = (byte)value;
                    break;
            }
            return clamped;
        }

        public void ClearAndUpdateDropReference(int v)
        {
            //[ISB] this doesn't really need to exist but may as well..
            ContainsType = (sbyte)v;
            ContainsID = 0;
        }
    }
}
