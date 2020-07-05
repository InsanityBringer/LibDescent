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
using System.IO;

namespace LibDescent.Data
{
    class HAMDataWriter
    {
        public void WriteTMAPInfo(TMAPInfo tmapinfo, BinaryWriter bw)
        {
            bw.Write(tmapinfo.Flags);
            bw.Write(new byte[3]);
            bw.Write(tmapinfo.Lighting.value);
            bw.Write(tmapinfo.Damage.value);
            bw.Write(tmapinfo.EClipNum);
            bw.Write(tmapinfo.DestroyedID);
            bw.Write(tmapinfo.SlideU);
            bw.Write(tmapinfo.SlideV);
        }

        public void WriteVClip(VClip clip, BinaryWriter bw)
        {
            bw.Write(clip.PlayTime.value);
            bw.Write(clip.NumFrames);
            bw.Write(clip.FrameTime.value);
            bw.Write(clip.Flags);
            bw.Write(clip.SoundNum);
            for (int x = 0; x < 30; x++)
            {
                bw.Write(clip.Frames[x]);
            }
            bw.Write(clip.LightValue.value);
        }

        public void WriteEClip(EClip clip, BinaryWriter bw)
        {
            WriteVClip(clip.Clip, bw);
            bw.Write(clip.TimeLeft);
            bw.Write(clip.FrameCount);
            bw.Write(clip.ChangingWallTexture);
            bw.Write(clip.ChangingObjectTexture);
            bw.Write(clip.Flags);
            bw.Write(clip.CriticalClip);
            bw.Write(clip.DestroyedBitmapNum);
            bw.Write(clip.ExplosionVClip);
            bw.Write(clip.ExplosionEClip);
            bw.Write(clip.ExplosionSize.value);
            bw.Write(clip.SoundNum);
            bw.Write(clip.SegNum);
            bw.Write(clip.SideNum);
        }

        public void WriteWClip(WClip clip, BinaryWriter bw)
        {
            bw.Write(clip.PlayTime.value);
            bw.Write(clip.NumFrames);
            for (int x = 0; x < 50; x++)
            {
                bw.Write(clip.Frames[x]);
            }
            bw.Write(clip.OpenSound);
            bw.Write(clip.CloseSound);
            bw.Write(clip.Flags);
            for (int x = 0; x < 13; x++)
            {
                bw.Write((byte)clip.Filename[x]);
            }
            bw.Write(clip.Pad);
        }

        public void WriteRobot(Robot robot, BinaryWriter bw)
        {
            bw.Write(robot.ModelNum);
            for (int x = 0; x < 8; x++)
            {
                bw.Write(robot.GunPoints[x].x.value);
                bw.Write(robot.GunPoints[x].y.value);
                bw.Write(robot.GunPoints[x].z.value);
            }
            for (int x = 0; x < 8; x++)
            {
                bw.Write(robot.GunSubmodels[x]);
            }
            bw.Write(robot.HitVClipNum);
            bw.Write(robot.HitSoundNum);
            
            bw.Write(robot.DeathVClipNum);
            bw.Write(robot.DeathSoundNum);
            
            bw.Write(robot.WeaponType);
            bw.Write(robot.WeaponTypeSecondary);
            bw.Write(robot.NumGuns);
            bw.Write(robot.ContainsID);

            bw.Write(robot.ContainsCount);
            bw.Write(robot.ContainsProbability);
            bw.Write(robot.ContainsType);
            bw.Write(robot.Kamikaze);
            
            bw.Write(robot.ScoreValue);
            bw.Write(robot.DeathExplosionRadius);
            bw.Write(robot.EnergyDrain);
            
            bw.Write(robot.Lighting.value);
            bw.Write(robot.Strength.value);
            
            bw.Write(robot.Mass.value);
            bw.Write(robot.Drag.value);
            
            for (int x = 0; x < 5; x++)
            {
                bw.Write(robot.FieldOfView[x].value);
            }
            for (int x = 0; x < 5; x++)
            {
                bw.Write(robot.FiringWait[x].value);
            }
            for (int x = 0; x < 5; x++)
            {
                bw.Write(robot.FiringWaitSecondary[x].value);
            }
            for (int x = 0; x < 5; x++)
            {
                bw.Write(robot.TurnTime[x].value);
            }
            for (int x = 0; x < 5; x++)
            {
                bw.Write(robot.MaxSpeed[x].value);
            }
            for (int x = 0; x < 5; x++)
            {
                bw.Write(robot.CircleDistance[x].value);
            }
            for (int x = 0; x < 5; x++)
            {
                bw.Write(robot.RapidfireCount[x]);
            }
            for (int x = 0; x < 5; x++)
            {
                bw.Write(robot.EvadeSpeed[x]);
            }
            bw.Write((sbyte)robot.CloakType);
            bw.Write((sbyte)robot.AttackType);
           
            bw.Write(robot.SeeSound);
            bw.Write(robot.AttackSound);
            bw.Write(robot.ClawSound);
            bw.Write(robot.TauntSound);

            bw.Write((sbyte)robot.BossFlag);
            bw.Write((sbyte)(robot.Companion ? 1 : 0));
            bw.Write(robot.SmartBlobsOnDeath);
            bw.Write(robot.SmartBlobsOnHit);

            bw.Write((sbyte)(robot.Thief ? 1 : 0));
            bw.Write(robot.Pursuit);
            bw.Write(robot.LightCast);
            bw.Write(robot.DeathRollTime);

            bw.Write(robot.Flags);
            bw.Write(new byte[3]);

            bw.Write(robot.DeathRollSound);
            bw.Write(robot.Glow);
            bw.Write((sbyte)robot.Behavior);
            bw.Write(robot.Aim);

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    bw.Write(robot.AnimStates[y, x].NumJoints);
                    bw.Write(robot.AnimStates[y, x].Offset);
                }
            }
            bw.Write(robot.Always0xABCD);
        }

        public void WriteWeapon(Weapon weapon, BinaryWriter bw)
        {
            bw.Write((byte)weapon.RenderType);
            bw.Write((byte)(weapon.Persistent ? 1 : 0));
            bw.Write(weapon.ModelNum);
            bw.Write(weapon.ModelNumInner);
            
            bw.Write(weapon.MuzzleFlashVClip);
            bw.Write(weapon.RobotHitVClip);
            bw.Write(weapon.FiringSound);

            bw.Write(weapon.WallHitVClip);
            bw.Write(weapon.FireCount);
            bw.Write(weapon.RobotHitSound);
            
            bw.Write(weapon.AmmoUsage);
            bw.Write(weapon.WeaponVClip);
            bw.Write(weapon.WallHitSound);

            bw.Write((byte)(weapon.Destroyable ? 1 : 0));
            bw.Write((byte)(weapon.Matter ? 1 : 0));
            bw.Write((byte)weapon.Bounce);
            bw.Write((byte)(weapon.HomingFlag ? 1 : 0));

            bw.Write(weapon.SpeedVariance);

            bw.Write(weapon.Flags);

            bw.Write(weapon.Flash);
            bw.Write(weapon.AfterburnerSize);

            bw.Write(weapon.Children);
            
            bw.Write(weapon.EnergyUsage.value);
            bw.Write(weapon.FireWait.value);

            bw.Write(weapon.MultiDamageScale.value);
            
            bw.Write(weapon.Bitmap);
            
            bw.Write(weapon.BlobSize.value);
            bw.Write(weapon.FlashSize.value);
            bw.Write(weapon.ImpactSize.value);
            for (int x = 0; x < 5; x++)
            {
                bw.Write(weapon.Strength[x].value);
            }
            for (int x = 0; x < 5; x++)
            {
                bw.Write(weapon.Speed[x].value);
            }
            bw.Write(weapon.Mass.value);
            bw.Write(weapon.Drag.value);
            bw.Write(weapon.Thrust.value);
            bw.Write(weapon.POLenToWidthRatio.value);
            bw.Write(weapon.Light.value);
            bw.Write(weapon.Lifetime.value);
            bw.Write(weapon.DamageRadius.value);
            
            bw.Write(weapon.CockpitPicture);
            bw.Write(weapon.HiresCockpitPicture);
        }

        public void WriteWeaponV2(Weapon weapon, BinaryWriter bw)
        {
            bw.Write((byte)weapon.RenderType);
            bw.Write((byte)(weapon.Persistent ? 1 : 0));
            bw.Write(weapon.ModelNum);
            bw.Write(weapon.ModelNumInner);

            bw.Write(weapon.MuzzleFlashVClip);
            bw.Write(weapon.RobotHitVClip);
            bw.Write(weapon.FiringSound);

            bw.Write(weapon.WallHitVClip);
            bw.Write(weapon.FireCount);
            bw.Write(weapon.RobotHitSound);

            bw.Write(weapon.AmmoUsage);
            bw.Write(weapon.WeaponVClip);
            bw.Write(weapon.WallHitSound);

            bw.Write((byte)(weapon.Destroyable ? 1 : 0));
            bw.Write((byte)(weapon.Matter ? 1 : 0));
            bw.Write((byte)weapon.Bounce);
            bw.Write((byte)(weapon.HomingFlag ? 1 : 0));

            bw.Write(weapon.SpeedVariance);

            bw.Write(weapon.Flags);

            bw.Write(weapon.Flash);
            bw.Write(weapon.AfterburnerSize);

            bw.Write(weapon.EnergyUsage.value);
            bw.Write(weapon.FireWait.value);

            bw.Write(weapon.Bitmap);

            bw.Write(weapon.BlobSize.value);
            bw.Write(weapon.FlashSize.value);
            bw.Write(weapon.ImpactSize.value);
            for (int x = 0; x < 5; x++)
            {
                bw.Write(weapon.Strength[x].value);
            }
            for (int x = 0; x < 5; x++)
            {
                bw.Write(weapon.Speed[x].value);
            }
            bw.Write(weapon.Mass.value);
            bw.Write(weapon.Drag.value);
            bw.Write(weapon.Thrust.value);
            bw.Write(weapon.POLenToWidthRatio.value);
            bw.Write(weapon.Light.value);
            bw.Write(weapon.Lifetime.value);
            bw.Write(weapon.DamageRadius.value);

            bw.Write(weapon.CockpitPicture);
        }

        public void WritePolymodel(Polymodel model, BinaryWriter bw)
        {
            bw.Write(model.NumSubmodels);
            bw.Write(model.ModelIDTASize);
            bw.Write(model.ModelIDTAPointer);
            for (int s = 0; s < 10; s++)
            {
                bw.Write(model.Submodels[s].Pointer);
            }
            for (int s = 0; s < 10; s++)
            {
                bw.Write(model.Submodels[s].Offset.x.value);
                bw.Write(model.Submodels[s].Offset.y.value);
                bw.Write(model.Submodels[s].Offset.z.value);
            }
            for (int s = 0; s < 10; s++)
            {
                bw.Write(model.Submodels[s].Normal.x.value);
                bw.Write(model.Submodels[s].Normal.y.value);
                bw.Write(model.Submodels[s].Normal.z.value);
            }
            for (int s = 0; s < 10; s++)
            {
                bw.Write(model.Submodels[s].Point.x.value);
                bw.Write(model.Submodels[s].Point.y.value);
                bw.Write(model.Submodels[s].Point.z.value);
            }
            for (int s = 0; s < 10; s++)
            {
                bw.Write(model.Submodels[s].Radius.value);
            }
            for (int s = 0; s < 10; s++)
            {
                bw.Write(model.Submodels[s].Parent);
            }
            for (int s = 0; s < 10; s++)
            {
                bw.Write(model.Submodels[s].Mins.x.value);
                bw.Write(model.Submodels[s].Mins.y.value);
                bw.Write(model.Submodels[s].Mins.z.value);
            }
            for (int s = 0; s < 10; s++)
            {
                bw.Write(model.Submodels[s].Maxs.x.value);
                bw.Write(model.Submodels[s].Maxs.y.value);
                bw.Write(model.Submodels[s].Maxs.z.value);
            }
            bw.Write(model.Mins.x.value);
            bw.Write(model.Mins.y.value);
            bw.Write(model.Mins.z.value);
            bw.Write(model.Maxs.x.value);
            bw.Write(model.Maxs.y.value);
            bw.Write(model.Maxs.z.value);
            bw.Write(model.Radius.value);
            bw.Write(model.NumTextures);
            bw.Write(model.FirstTexture);
            bw.Write(model.SimplerModels);
        }

        public void WritePlayerShip(Ship ship, BinaryWriter bw)
        {
            bw.Write(ship.ModelNum);
            bw.Write(ship.DeathVClipNum);
            bw.Write(ship.Mass.value);
            bw.Write(ship.Drag.value);
            bw.Write(ship.MaxThrust.value);
            bw.Write(ship.ReverseThrust.value);
            bw.Write(ship.Brakes.value);
            bw.Write(ship.Wiggle.value);
            bw.Write(ship.MaxRotationThrust.value);
            for (int x = 0; x < 8; x++)
            {
                bw.Write(ship.GunPoints[x].x.value);
                bw.Write(ship.GunPoints[x].y.value);
                bw.Write(ship.GunPoints[x].z.value);
            }
        }

        public void WriteTMAPInfoDescent1(BinaryWriter bw, TMAPInfo tMAPInfo)
        {
            /*
            TMAPInfo mapinfo = new TMAPInfo();
            byte[] temp = br.ReadBytes(13);
            Array.Copy(temp, mapinfo.filename, 13);
            mapinfo.Flags = br.ReadByte();
            mapinfo.Lighting = new Fix(br.ReadInt32());
            mapinfo.Damage = new Fix(br.ReadInt32());
            mapinfo.EClipNum = (short)br.ReadInt32();

            return mapinfo;
            */

            byte[] temp = new byte[13];

            Array.Copy(tMAPInfo.filename, temp, tMAPInfo.filename.Length);
            bw.Write(temp, 0, 13);

            bw.Write((byte)tMAPInfo.Flags);
            bw.Write((Int32)tMAPInfo.Lighting);
            bw.Write((Int32)tMAPInfo.Damage);
            bw.Write((Int32)tMAPInfo.EClipNum);
        }

        public void WriteByte(BinaryWriter bw, byte value)
        {
            bw.Write(value);
        }

        public void WriteSByte(BinaryWriter bw, sbyte value)
        {
            bw.Write(value);
        }

        public void WriteInt16(BinaryWriter bw, Int16 value)
        {
            bw.Write(value);
        }

        public void WriteUInt16(BinaryWriter bw, UInt16 value)
        {
            bw.Write(value);
        }

        public static void WriteInt32(BinaryWriter bw, Int32 value)
        {
            bw.Write(value);
        }
        public static void WriteFixVector(BinaryWriter bw, FixVector a)
        {
            int x = (int)a.x;
            int y = (int)a.y;
            int z = (int)a.z;

            WriteInt32(bw, x);
            WriteInt32(bw, y);
            WriteInt32(bw, y);
        }

        public void WriteMany<T>(BinaryWriter bw, int count, T[] items, Action<T> writeAction)
        {
            for (var i = 0; i < count; i++)
            {
                writeAction(items[i]);
            }
        }

        public void WriteRobotDescent1(Robot robot, BinaryWriter bw)
        {
            //robot.ModelNum = br.ReadInt32();
            WriteInt32(bw, robot.ModelNum);
            //robot.NumGuns = (sbyte)br.ReadInt32();
            WriteInt32(bw, robot.NumGuns);

            //for (int s = 0; s < Polymodel.MaxGuns; s++)
            //{
            //    robot.GunPoints[s] = FixVector.FromRawValues(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
            //}
            this.WriteMany(bw, Polymodel.MaxGuns, robot.GunPoints, (a) => WriteFixVector(bw, a));

            //for (int s = 0; s < 8; s++)
            //{
            //    robot.GunSubmodels[s] = br.ReadByte();
            //}
            this.WriteMany(bw, 8, robot.GunSubmodels, (a) => this.WriteByte(bw, a));

            //robot.HitVClipNum = br.ReadInt16();
            this.WriteInt16(bw, robot.HitVClipNum);
            //robot.HitSoundNum = br.ReadInt16();
            this.WriteInt16(bw, robot.HitSoundNum);

            //robot.DeathVClipNum = br.ReadInt16();
            this.WriteInt16(bw, robot.DeathVClipNum);
            //robot.DeathSoundNum = br.ReadInt16();
            this.WriteInt16(bw, robot.DeathSoundNum);

            //robot.WeaponType = (sbyte)br.ReadInt16();
            this.WriteInt16(bw, robot.WeaponType);
            //robot.ContainsID = br.ReadSByte();
            this.WriteSByte(bw, robot.ContainsID);
            //robot.ContainsCount = br.ReadSByte();
            this.WriteSByte(bw, robot.ContainsCount);

            //robot.ContainsProbability = br.ReadSByte();
            this.WriteSByte(bw, robot.ContainsProbability);
            //robot.ContainsType = br.ReadSByte();
            this.WriteSByte(bw, robot.ContainsType);

            //robot.ScoreValue = (short)br.ReadInt32();
            WriteInt32(bw, robot.ScoreValue);

            //robot.Lighting = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)robot.Lighting);
            //robot.Strength = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)robot.Strength);

            //robot.Mass = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)robot.Mass);
            //robot.Drag = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)robot.Drag);

            //for (int s = 0; s < Robot.NumDifficultyLevels; s++)
            //{
            //    robot.FieldOfView[s] = new Fix(br.ReadInt32());
            //}
            this.WriteMany(bw, Robot.NumDifficultyLevels, robot.FieldOfView, (a) => WriteInt32(bw, (int)a));

            //for (int s = 0; s < Robot.NumDifficultyLevels; s++)
            //{
            //    robot.FiringWait[s] = new Fix(br.ReadInt32());
            //}
            this.WriteMany(bw, Robot.NumDifficultyLevels, robot.FiringWait, (a) => WriteInt32(bw, (int)a));

            //for (int s = 0; s < Robot.NumDifficultyLevels; s++)
            //{
            //    robot.TurnTime[s] = new Fix(br.ReadInt32());
            //}
            this.WriteMany(bw, Robot.NumDifficultyLevels, robot.TurnTime, (a) => WriteInt32(bw, (int)a));

            //for (int s = 0; s < Robot.NumDifficultyLevels; s++)
            //{
            //    robot.FirePower[s] = new Fix(br.ReadInt32());
            //}
            this.WriteMany(bw, Robot.NumDifficultyLevels, robot.FirePower, (a) => WriteInt32(bw, (int)a));

            //for (int s = 0; s < Robot.NumDifficultyLevels; s++)
            //{
            //    robot.Shield[s] = new Fix(br.ReadInt32());
            //}
            this.WriteMany(bw, Robot.NumDifficultyLevels, robot.Shield, (a) => WriteInt32(bw, (int)a));

            //for (int s = 0; s < Robot.NumDifficultyLevels; s++)
            //{
            //    robot.MaxSpeed[s] = new Fix(br.ReadInt32());
            //}
            this.WriteMany(bw, Robot.NumDifficultyLevels, robot.MaxSpeed, (a) => WriteInt32(bw, (int)a));

            //for (int s = 0; s < Robot.NumDifficultyLevels; s++)
            //{
            //    robot.CircleDistance[s] = new Fix(br.ReadInt32());
            //}
            this.WriteMany(bw, Robot.NumDifficultyLevels, robot.CircleDistance, (a) => WriteInt32(bw, (int)a));

            //for (int s = 0; s < Robot.NumDifficultyLevels; s++)
            //{
            //    robot.RapidfireCount[s] = br.ReadSByte();
            //}
            this.WriteMany(bw, Robot.NumDifficultyLevels, robot.RapidfireCount, (a) => this.WriteSByte(bw, a));

            //for (int s = 0; s < Robot.NumDifficultyLevels; s++)
            //{
            //    robot.EvadeSpeed[s] = br.ReadSByte();
            //}
            this.WriteMany(bw, Robot.NumDifficultyLevels, robot.EvadeSpeed, (a) => this.WriteSByte(bw, a));

            //robot.CloakType = (RobotCloakType)br.ReadSByte();
            this.WriteSByte(bw, (sbyte)robot.CloakType);
            //robot.AttackType = (RobotAttackType)br.ReadSByte();
            this.WriteSByte(bw, (sbyte)robot.AttackType);
            //robot.BossFlag = (RobotBossType)br.ReadSByte();
            this.WriteSByte(bw, (sbyte)robot.BossFlag);
            //robot.SeeSound = br.ReadByte();
            this.WriteByte(bw, robot.SeeSound);
            //robot.AttackSound = br.ReadByte();
            this.WriteByte(bw, robot.AttackSound);
            //robot.ClawSound = br.ReadByte();
            this.WriteByte(bw, robot.ClawSound);

            for (int v = 0; v < 9; v++)
            {
                for (int u = 0; u < 5; u++)
                {
                    //robot.AnimStates[v, u].NumJoints = br.ReadInt16();
                    this.WriteInt16(bw, robot.AnimStates[v, u].NumJoints);
                    //robot.AnimStates[v, u].Offset = br.ReadInt16();
                    this.WriteInt16(bw, robot.AnimStates[v, u].Offset);
                }
            }

            //robot.Always0xABCD = br.ReadInt32();
            WriteInt32(bw, robot.Always0xABCD);

        }

        internal void WriteWeaponInfoDescent1(BinaryWriter bw, Weapon weapon)
        {
            //weapon.RenderType = (WeaponRenderType)br.ReadByte();
            this.WriteByte(bw, (byte)weapon.RenderType);
            //weapon.ModelNum = br.ReadByte();
            this.WriteByte(bw, (byte)weapon.ModelNum);
            //weapon.ModelNumInner = br.ReadByte();
            this.WriteByte(bw, (byte)weapon.ModelNumInner);
            //weapon.Persistent = br.ReadByte() != 0 ? true : false;
            this.WriteByte(bw, (byte)(weapon.Persistent ? 1 : 0));

            //weapon.MuzzleFlashVClip = br.ReadSByte();
            this.WriteSByte(bw, weapon.MuzzleFlashVClip);
            //weapon.FiringSound = br.ReadInt16();
            this.WriteInt16(bw, weapon.FiringSound);

            //weapon.RobotHitVClip = br.ReadSByte();
            this.WriteSByte(bw, weapon.RobotHitVClip);
            //weapon.RobotHitSound = br.ReadInt16();
            this.WriteInt16(bw, weapon.RobotHitSound);

            //weapon.WallHitVClip = br.ReadSByte();
            this.WriteSByte(bw, weapon.WallHitVClip);
            //weapon.WallHitSound = br.ReadInt16();
            this.WriteInt16(bw, weapon.WallHitSound);

            //weapon.FireCount = br.ReadByte();
            this.WriteByte(bw, weapon.FireCount);
            //weapon.AmmoUsage = br.ReadByte();
            this.WriteByte(bw, weapon.AmmoUsage);
            //weapon.WeaponVClip = br.ReadSByte();
            this.WriteSByte(bw, weapon.WeaponVClip);
            //weapon.Destroyable = br.ReadByte() != 0 ? true : false;
            this.WriteByte(bw, (byte)(weapon.Destroyable ? 1 : 0));

            //weapon.Matter = br.ReadByte() != 0 ? true : false;
            this.WriteByte(bw, (byte)(weapon.Matter ? 1 : 0));
            //weapon.Bounce = (WeaponBounceType)br.ReadByte();
            this.WriteByte(bw, (byte)weapon.Bounce);
            //weapon.HomingFlag = br.ReadByte() != 0 ? true : false;
            this.WriteByte(bw, (byte)(weapon.Destroyable ? 1 : 0));
            //br.ReadBytes(3);

            //weapon.EnergyUsage = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.EnergyUsage);
            //weapon.FireWait = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.FireWait);

            //weapon.MultiDamageScale = 1;

            //weapon.Bitmap = br.ReadUInt16();
            WriteUInt16(bw, weapon.Bitmap);

            //weapon.BlobSize = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.BlobSize);
            //weapon.FlashSize = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.FlashSize);
            //weapon.ImpactSize = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.ImpactSize);

            for (int s = 0; s < 5; s++)
            {
                //weapon.Strength[s] = new Fix(br.ReadInt32());
                WriteInt32(bw, (int)weapon.Strength[s]);
            }
            for (int s = 0; s < 5; s++)
            {
                //weapon.Speed[s] = new Fix(br.ReadInt32());
                WriteInt32(bw, (int)weapon.Speed[s]);
            }

            //weapon.Mass = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.Mass);
            //weapon.Drag = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.Drag);
            //weapon.Thrust = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.Thrust);
            //weapon.POLenToWidthRatio = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.POLenToWidthRatio);
            //weapon.Light = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.Light);
            //weapon.Lifetime = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.Lifetime);
            //weapon.DamageRadius = new Fix(br.ReadInt32());
            WriteInt32(bw, (int)weapon.DamageRadius);

            //weapon.CockpitPicture = br.ReadUInt16();
            WriteUInt16(bw, weapon.CockpitPicture);
            //weapon.HiresCockpitPicture = weapon.CockpitPicture;
        }
    }
}
