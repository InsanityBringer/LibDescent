using System;
using System.Collections.Generic;
using System.Text;
using LibDescent.Data;

namespace LibDescent.Edit
{
    public class RobotData
    {
        public Robot robot;

        public RobotData(Robot robot)
        {
            this.robot = robot;
        }

        public bool UpdateRobot(int tag, ref int value, int curAI, int curGun)
        {
            bool clamped = false;
            switch (tag)
            {
                case 1:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.model_num = value;
                    break;
                case 2:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    robot.exp1_vclip_num = (short)value;
                    break;
                case 3:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    robot.exp1_sound_num = (short)value;
                    break;
                case 4:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    robot.exp2_vclip_num = (short)value;
                    break;
                case 5:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.weapon_type = (sbyte)value;
                    break;
                case 6:
                    robot.weapon_type2 = (sbyte)(value - 1);
                    break;
                case 7:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.n_guns = (sbyte)value;
                    break;
                case 8:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.contains_id = (sbyte)value;
                    break;
                case 9:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.contains_count = (sbyte)value;
                    break;
                case 10:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.contains_prob = (sbyte)value;
                    break;
                case 12:
                    if (value == 0)
                        robot.claw_sound = 255;
                    else
                        robot.claw_sound = (byte)(value);
                    break;
                case 13:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, ref clamped);
                    robot.score_value = (short)value;
                    break;
                case 14:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    robot.badass = (byte)value;
                    break;
                case 15:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    robot.energy_drain = (byte)value;
                    break;
                case 16:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.lighting = new Fix(value);
                    break;
                case 17:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.strength = new Fix(value);
                    break;
                case 18:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.mass = new Fix(value);
                    break;
                case 19:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.drag = new Fix(value);
                    break;
                case 20:
                    value = (int)(Math.Cos(value * Math.PI / 180.0D) * 65536.0);
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.field_of_view[curAI] = new Fix(value);
                    break;
                case 21:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.firing_wait[curAI] = new Fix(value);
                    break;
                case 22:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.firing_wait2[curAI] = new Fix(value);
                    break;
                case 23:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.turn_time[curAI] = new Fix(value);
                    break;
                case 24:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.max_speed[curAI] = new Fix(value);
                    break;
                case 25:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, ref clamped);
                    robot.circle_distance[curAI] = new Fix(value);
                    break;
                case 26:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.rapidfire_count[curAI] = (sbyte)value;
                    break;
                case 27:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.evade_speed[curAI] = (sbyte)value;
                    break;
                case 30:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        robot.see_sound = 255;
                    else
                        robot.see_sound = (byte)(value);
                    break;
                case 31:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        robot.attack_sound = 255;
                    else
                        robot.attack_sound = (byte)(value);
                    break;
                case 33:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        robot.taunt_sound = 255;
                    else
                        robot.taunt_sound = (byte)(value);
                    break;
                case 34:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        robot.deathroll_sound = 255;
                    else
                        robot.deathroll_sound = (byte)(value);
                    break;
                case 36:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.smart_blobs = (sbyte)value;
                    break;
                case 37:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.energy_blobs = (sbyte)value;
                    break;
                case 38:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.pursuit = (sbyte)value;
                    break;
                case 39:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, ref clamped);
                    robot.lightcast = (sbyte)value;
                    break;
                case 40:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        robot.exp1_sound_num = 255;
                    else
                        robot.exp1_sound_num = (byte)(value);
                    break;
                case 41:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    if (value == 0)
                        robot.exp2_sound_num = 255;
                    else
                        robot.exp2_sound_num = (byte)(value);
                    break;
                case 43:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    robot.glow = (byte)value;
                    break;
                case 45:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, ref clamped);
                    robot.aim = (byte)value;
                    break;
            }
            return clamped;
        }

        public void ClearAndUpdateDropReference(int v)
        {
            //[ISB] this doesn't really need to exist anymore but may as well..
            robot.contains_type = (sbyte)v;
            robot.contains_id = 0;
        }
    }
}
