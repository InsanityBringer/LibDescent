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
    public class Robot
    {
        public const int NUM_ANIMATION_STATES = 5;
        public const int NUM_DIFFICULTY_LEVELS = 5;
        //important crap
        public struct jointlist
        {
            public short n_joints;
            public short offset;
        }

        /// <summary>
        /// which polygon model?
        /// </summary>
        public int model_num;
        /// <summary>
        /// where each gun model is
        /// </summary>
        public FixVector[] gun_points = new FixVector[Polymodel.MAX_GUNS];
        /// <summary>
        /// which submodel is each gun in?
        /// </summary>
        public byte[] gun_submodels = new byte[Polymodel.MAX_GUNS];
        /// <summary>
        /// VClip shown when robot is hit.
        /// </summary>
        public short exp1_vclip_num;
        /// <summary>
        /// Sound played when robot is hit.
        /// </summary>
        public short exp1_sound_num;
        /// <summary>
        /// VClip shown when robot is killed.
        /// </summary>
        public short exp2_vclip_num;
        /// <summary>
        /// Sound played when robot is killed.
        /// </summary>
        public short exp2_sound_num;
        /// <summary>
        /// ID of weapon type fired.
        /// </summary>
        public sbyte weapon_type;
        /// <summary>
        /// Secondary weapon number, -1 means none, otherwise gun #0 fires this weapon.
        /// </summary>
        public sbyte weapon_type2;
        /// <summary>
        /// how many different gun positions
        /// </summary>
        public sbyte n_guns;
        /// <summary>
        /// ID of powerup this robot can contain.
        /// </summary>
        public sbyte contains_id;
        /// <summary>
        /// Max number of things this instance can contain.
        /// </summary>
        public sbyte contains_count;
        /// <summary>
        /// Probability that this instance will contain something in N/16
        /// </summary>
        public sbyte contains_prob;
        /// <summary>
        /// Type of thing contained, robot or powerup, in bitmaps.tbl, 2=robot, 7=powerup. 
        /// </summary>
        public sbyte contains_type; //[ISB] Should be instance of ObjectType tbh
        /// <summary>
        /// !0 means commits suicide when hits you, strength thereof. 0 means no.
        /// </summary>
        public sbyte kamikaze;
        /// <summary>
        /// Score from this robot.
        /// </summary>
        public short score_value;
        /// <summary>
        /// Dies with badass explosion, and strength thereof, 0 means NO.
        /// </summary>
        public byte badass;
        /// <summary>
        /// Points of energy drained at each collision.
        /// </summary>
        public byte energy_drain;
        /// <summary>
        /// should this be here or with polygon model?
        /// </summary>
        public Fix lighting;
        /// <summary>
        /// Initial shields of robot
        /// </summary>
        public Fix strength;
        /// <summary>
        /// how heavy is this thing?
        /// </summary>
        public Fix mass;
        /// <summary>
        /// how much drag does it have?
        /// </summary>
        public Fix drag;
        /// <summary>
        /// compare this value with forward_vector.dot.vector_to_player, if field_of_view <, then robot can see player
        /// </summary>
        public Fix[] field_of_view = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// time in seconds between shots
        /// </summary>
        public Fix[] firing_wait = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// time in seconds between shots. For gun 2
        /// </summary>
        public Fix[] firing_wait2 = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// time in seconds to rotate 360 degrees in a dimension
        /// </summary>
        public Fix[] turn_time = new Fix[NUM_DIFFICULTY_LEVELS];
        public Fix[] fire_power = new Fix[NUM_DIFFICULTY_LEVELS]; //Descent 1 waste data
        public Fix[] shield = new Fix[NUM_DIFFICULTY_LEVELS]; //Retained for serialization reasons. 
        /// <summary>
        /// maximum speed attainable by this robot
        /// </summary>
        public Fix[] max_speed = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// distance at which robot circles player
        /// </summary>
        public Fix[] circle_distance = new Fix[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// number of shots fired rapidly
        /// </summary>
        public sbyte[] rapidfire_count = new sbyte[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// rate at which robot can evade shots, 0=none, 4=very fast
        /// </summary>
        public sbyte[] evade_speed = new sbyte[NUM_DIFFICULTY_LEVELS];
        /// <summary>
        /// 0=never, 1=always, 2=except-when-firing
        /// </summary>
        public sbyte cloak_type;
        /// <summary>
        /// 0=firing, 1=charge (like green guy)
        /// </summary>
        public sbyte attack_type;
        /// <summary>
        /// sound robot makes when it first sees the player
        /// </summary>
        public byte see_sound;
        /// <summary>
        /// sound robot makes when it attacks the player
        /// </summary>
        public byte attack_sound;
        /// <summary>
        /// sound robot makes as it claws you (attack_type should be 1)
        /// </summary>
        public byte claw_sound;
        /// <summary>
        /// sound robot makes after you die if this code was ever actually enabled and used.
        /// </summary>
        public byte taunt_sound;
        /// <summary>
        /// 0 = not boss, 1 = boss.  Is that surprising?
        /// </summary>
        public sbyte boss_flag;
        /// <summary>
        /// Companion robot, leads you to things.
        /// </summary>
        public sbyte companion;
        /// <summary>
        /// how many smart blobs are emitted when this guy dies! if this code was ever actually enabled and used.
        /// </summary>
        public sbyte smart_blobs;
        /// <summary>
        /// how many smart blobs are emitted when this guy gets hit by energy weapon!
        /// </summary>
        public sbyte energy_blobs;
        /// <summary>
        /// !0 means this guy can steal when he collides with you!
        /// </summary>
        public sbyte thief;
        /// <summary>
        /// !0 means pursues player after he goes around a corner.  4 = 4/2 pursue up to 4/2 seconds after becoming invisible if up to 4 segments away
        /// </summary>
        public sbyte pursuit;
        /// <summary>
        /// Amount of light cast. 1 is default.  10 is very large.
        /// </summary>
        public sbyte lightcast;
        /// <summary>
        /// 0 = dies without death roll. !0 means does death roll, larger = faster and louder
        /// </summary>
        public sbyte death_roll;
        /// <summary>
        /// misc properties, and by that a single flag that is literally not used in the final game. heeeh
        /// </summary>
        public byte	flags;
        //three bytes pad
        /// <summary>
        /// if has deathroll, what sound?
        /// </summary>
        public byte deathroll_sound;
        /// <summary>
        /// apply this light to robot itself. stored as 4:4 fixed-point
        /// </summary>
        public byte glow;
        /// <summary>
        /// Default behavior.
        /// </summary>
        public byte behavior;
        /// <summary>
        /// 255 = perfect, less = more likely to miss.  0 != random, would look stupid.  0=45 degree spread. 
        /// </summary>
        public byte aim;

        /// <summary>
        /// animation info
        /// </summary>
        public jointlist[,] anim_states = new jointlist[Polymodel.MAX_GUNS+1, NUM_ANIMATION_STATES];
        public int baseJoint = 0; //for HXM files aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa

        /// <summary>
        /// debugging. Retained for serializaiton purposes. 
        /// </summary>
        public int always_0xabcd;

        public int replacementID;
        public int ID;

        public Robot()
        {
            always_0xabcd = 0xabcd;
            contains_type = 7;
            weapon_type2 = -1;
        }

        public Robot(Robot other)
        {
            model_num = other.model_num;
            exp1_vclip_num = other.exp1_vclip_num;
            exp1_sound_num = other.exp1_sound_num;
            exp2_vclip_num = other.exp2_vclip_num;
            exp2_sound_num = other.exp2_sound_num;

            weapon_type = other.weapon_type;
            weapon_type2 = other.weapon_type2;

            contains_count = other.contains_count;
            contains_id = other.contains_id;
            contains_prob = other.contains_prob;
            contains_type = other.contains_type;

            kamikaze = other.kamikaze;
            score_value = other.score_value;
            badass = other.badass;
            energy_drain = other.energy_drain;

            lighting = other.lighting;
            strength = other.strength;

            mass = other.mass;
            drag = other.drag;

            for (int i = 0; i < NUM_DIFFICULTY_LEVELS; i++)
            {
                field_of_view[i] = other.field_of_view[i];
                firing_wait[i] = other.firing_wait[i];
                firing_wait2[i] = other.firing_wait2[i];
                turn_time[i] = other.turn_time[i];
                max_speed[i] = other.max_speed[i];
                circle_distance[i] = other.circle_distance[i];

                rapidfire_count[i] = other.rapidfire_count[i];
                evade_speed[i] = other.evade_speed[i];
            }

            cloak_type = other.cloak_type;
            attack_type = other.attack_type;

            see_sound = other.see_sound;
            attack_sound = other.attack_sound;
            claw_sound = other.claw_sound;
            taunt_sound = other.taunt_sound;

            boss_flag = other.boss_flag;
            companion = other.companion;
            smart_blobs = other.smart_blobs;
            energy_blobs = other.energy_blobs;

            thief = other.thief;
            pursuit = other.pursuit;
            lightcast = other.lightcast;
            death_roll = other.death_roll;

            flags = other.flags;

            deathroll_sound = other.deathroll_sound;
            glow = other.glow;
            behavior = other.behavior;
            aim = other.aim;

            always_0xabcd = 0xabcd;
        }

        public bool UpdateRobot(int tag, ref int value, int curAI, int curGun)
        {
            bool clamped = false;
            switch (tag)
            {
                case 1:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    model_num = value;
                    break;
                case 2:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    exp1_vclip_num = (short)value;
                    break;
                case 3:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    exp1_sound_num = (short)value;
                    break;
                case 4:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    exp2_vclip_num = (short)value;
                    break;
                case 5:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    weapon_type = (sbyte)value;
                    break;
                case 6:
                    weapon_type2 = (sbyte)(value - 1);
                    break;
                case 7:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    n_guns = (sbyte)value;
                    break;
                case 8:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    contains_id = (sbyte)value;
                    break;
                case 9:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    contains_count = (sbyte)value;
                    break;
                case 10:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    contains_prob = (sbyte)value;
                    break;
                /*case 11:
                    value = Utilites.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    contains_type = (sbyte)value;
                    break;*/ //don't use doesn't update rferences
                case 12:
                    if (value == 0)
                        claw_sound = 255;
                    else
                        claw_sound = (byte)(value);
                    break;
                case 13:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    score_value = (short)value;
                    break;
                case 14:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    badass = (byte)value;
                    break;
                case 15:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    energy_drain = (byte)value;
                    break;
                case 16:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    lighting = new Fix(value);
                    break;
                case 17:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    strength = new Fix(value);
                    break;
                case 18:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    mass = new Fix(value);
                    break;
                case 19:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    drag = new Fix(value);
                    break;
                case 20:
                    value = (int)(Math.Cos(value * Math.PI / 180.0D) * 65536.0);
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    field_of_view[curAI] = new Fix(value);
                    break;
                case 21:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    firing_wait[curAI] = new Fix(value);
                    break;
                case 22:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    firing_wait2[curAI] = new Fix(value);
                    break;
                case 23:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    turn_time[curAI] = new Fix(value);
                    break;
                case 24:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    max_speed[curAI] = new Fix(value);
                    break;
                case 25:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    circle_distance[curAI] = new Fix(value);
                    break;
                case 26:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    rapidfire_count[curAI] = (sbyte)value;
                    break;
                case 27:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    evade_speed[curAI] = (sbyte)value;
                    break;
                case 30:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        see_sound = 255;
                    else
                        see_sound = (byte)(value);
                    break;
                case 31:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        attack_sound = 255;
                    else
                        attack_sound = (byte)(value);
                    break;
                case 33:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        taunt_sound = 255;
                    else
                        taunt_sound = (byte)(value);
                    break;
                case 34:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        deathroll_sound = 255;
                    else
                        deathroll_sound = (byte)(value);
                    break;
                case 36:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    smart_blobs = (sbyte)value;
                    break;
                case 37:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    energy_blobs = (sbyte)value;
                    break;
                case 38:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    pursuit = (sbyte)value;
                    break;
                case 39:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    lightcast = (sbyte)value;
                    break;
                case 40:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        exp1_sound_num = 255;
                    else
                        exp1_sound_num = (byte)(value);
                    break;
                case 41:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        exp2_sound_num = 255;
                    else
                        exp2_sound_num = (byte)(value);
                    break;
                case 43:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    glow = (byte)value;
                    break;
                case 45:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    aim = (byte)value;
                    break;
            }
            return clamped;
        }

        public void ClearAndUpdateDropReference(int v)
        {
            //[ISB] this doesn't really need to exist but may as well..
            contains_type = (sbyte)v;
            contains_id = 0;
        }
    }
}
