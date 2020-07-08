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
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.ModelNum = value;
                    break;
                case 2:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, out clamped);
                    robot.HitVClipNum = (short)value;
                    break;
                case 3:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, out clamped);
                    robot.HitSoundNum = (short)value;
                    break;
                case 4:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, out clamped);
                    robot.DeathVClipNum = (short)value;
                    break;
                case 5:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.WeaponType = (sbyte)value;
                    break;
                case 6:
                    robot.WeaponTypeSecondary = (sbyte)(value - 1);
                    break;
                case 7:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.NumGuns = (sbyte)value;
                    break;
                case 8:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.ContainsID = (sbyte)value;
                    break;
                case 9:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.ContainsCount = (sbyte)value;
                    break;
                case 10:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.ContainsProbability = (sbyte)value;
                    break;
                case 12:
                    if (value == 0)
                        robot.ClawSound = 255;
                    else
                        robot.ClawSound = (byte)(value);
                    break;
                case 13:
                    value = Util.Clamp(value, short.MinValue, short.MaxValue, out clamped);
                    robot.ScoreValue = (short)value;
                    break;
                case 14:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, out clamped);
                    robot.DeathExplosionRadius = (byte)value;
                    break;
                case 15:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, out clamped);
                    robot.EnergyDrain = (byte)value;
                    break;
                case 16:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.Lighting = new Fix(value);
                    break;
                case 17:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.Strength = new Fix(value);
                    break;
                case 18:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.Mass = new Fix(value);
                    break;
                case 19:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.Drag = new Fix(value);
                    break;
                case 20:
                    value = (int)(Math.Cos(value * Math.PI / 180.0D) * 65536.0);
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.FieldOfView[curAI] = new Fix(value);
                    break;
                case 21:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.FiringWait[curAI] = new Fix(value);
                    break;
                case 22:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.FiringWaitSecondary[curAI] = new Fix(value);
                    break;
                case 23:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.TurnTime[curAI] = new Fix(value);
                    break;
                case 24:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.MaxSpeed[curAI] = new Fix(value);
                    break;
                case 25:
                    value = Util.Clamp(value, int.MinValue, int.MaxValue, out clamped);
                    robot.CircleDistance[curAI] = new Fix(value);
                    break;
                case 26:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.RapidfireCount[curAI] = (sbyte)value;
                    break;
                case 27:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.EvadeSpeed[curAI] = (sbyte)value;
                    break;
                case 30:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, out clamped);
                    if (value == 0)
                        robot.SeeSound = 255;
                    else
                        robot.SeeSound = (byte)(value);
                    break;
                case 31:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, out clamped);
                    if (value == 0)
                        robot.AttackSound = 255;
                    else
                        robot.AttackSound = (byte)(value);
                    break;
                case 33:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, out clamped);
                    if (value == 0)
                        robot.TauntSound = 255;
                    else
                        robot.TauntSound = (byte)(value);
                    break;
                case 34:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, out clamped);
                    if (value == 0)
                        robot.DeathRollSound = 255;
                    else
                        robot.DeathRollSound = (byte)(value);
                    break;
                case 36:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.SmartBlobsOnDeath = (sbyte)value;
                    break;
                case 37:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.SmartBlobsOnHit = (sbyte)value;
                    break;
                case 38:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.Pursuit = (sbyte)value;
                    break;
                case 39:
                    value = Util.Clamp(value, sbyte.MinValue, sbyte.MaxValue, out clamped);
                    robot.LightCast = (sbyte)value;
                    break;
                case 40:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, out clamped);
                    if (value == 0)
                        robot.HitSoundNum = 255;
                    else
                        robot.HitSoundNum = (byte)(value);
                    break;
                case 41:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, out clamped);
                    if (value == 0)
                        robot.DeathSoundNum = 255;
                    else
                        robot.DeathSoundNum = (byte)(value);
                    break;
                case 43:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, out clamped);
                    robot.Glow = (byte)value;
                    break;
                case 45:
                    value = Util.Clamp(value, byte.MinValue, byte.MaxValue, out clamped);
                    robot.Aim = (byte)value;
                    break;
            }
            return clamped;
        }

        public void ClearAndUpdateDropReference(int v)
        {
            //[ISB] this doesn't really need to exist anymore but may as well..
            robot.ContainsType = (sbyte)v;
            robot.ContainsID = 0;
        }
    }
}
