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
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LibDescent.Data
{
    public class HAMFile : IElementManager
    {
        /// <summary>
        /// Version of the archive, needed for writing back. Version 2 has sound information, version 3 is latest supported, used by the release game.
        /// </summary>
        public int version = 0;

        private int NumRobotJoints = 0; //needed to track the amount of robot joints constructed.

        /// <summary>
        /// Validity check, I guess. Probably needed but should be done better.
        /// </summary>
        public bool hasRead = false;

        //HAM data tables. Now properly encaspulated!
        /// <summary>
        /// List of piggy IDs of all the textures available for levels.
        /// </summary>
        public List<ushort> Textures { get; private set; }
        /// <summary>
        /// List of information for mapping textures into levels.
        /// </summary>
        public List<TMAPInfo> TMapInfo { get; private set; }
        /// <summary>
        /// List of sound IDs.
        /// </summary>
        public List<byte> Sounds { get; private set; }
        /// <summary>
        /// List to remap given sounds into other sounds when Descent is run in low memory mode.
        /// </summary>
        public List<byte> AltSounds { get; private set; }
        /// <summary>
        /// List of all VClip animations.
        /// </summary>
        public List<VClip> VClips { get; private set; }
        /// <summary>
        /// List of all Effect animations.
        /// </summary>
        public List<EClip> EClips { get; private set; }
        /// <summary>
        /// List of all Wall (door) animations.
        /// </summary>
        public List<WClip> WClips { get; private set; }
        /// <summary>
        /// List of all robots.
        /// </summary>
        public List<Robot> Robots { get; private set; }
        /// <summary>
        /// List of all robot joints used for animation.
        /// </summary>
        public List<JointPos> Joints { get; private set; }
        /// <summary>
        /// List of all weapons.
        /// </summary>
        public List<Weapon> Weapons { get; private set; }
        /// <summary>
        /// List of all polymodels.
        /// </summary>
        public List<Polymodel> Models { get; private set; }
        /// <summary>
        /// List of gauge piggy IDs.
        /// </summary>
        public List<ushort> Gauges { get; private set; }
        /// <summary>
        /// List of gague piggy IDs used for the highres cockpit.
        /// </summary>
        public List<ushort> GaugesHires { get; private set; }
        public int NumObjBitmaps = 0; //This is important to track the unique number of obj bitmaps, to know where to inject new ones. 
        public int NumObjBitmapPointers = 0; //Also important to tell how many obj bitmap pointer slots the user have left. 
        /// <summary>
        /// List of piggy IDs available for polymodels.
        /// </summary>
        public List<ushort> ObjBitmaps { get; private set; }
        /// <summary>
        /// List of pointers into the ObjBitmaps table for polymodels.
        /// </summary>
        public List<ushort> ObjBitmapPointers { get; private set; }
        /// <summary>
        /// The player ship.
        /// </summary>
        public Ship PlayerShip;
        /// <summary>
        /// List of piggy IDs for all heads-up display modes.
        /// </summary>
        public List<ushort> Cockpits { get; private set; }
        /// <summary>
        /// List of all reactors.
        /// </summary>
        public List<Reactor> Reactors { get; private set; }
        /// <summary>
        /// List of all powerups.
        /// </summary>
        public List<Powerup> Powerups { get; private set; }
        /// <summary>
        /// The index in the ObjBitmapPointers table of the first multiplayer color texture.
        /// </summary>
        public int FirstMultiBitmapNum;
        /// <summary>
        /// Table to remap piggy IDs to other IDs for low memory mode.
        /// </summary>
        public ushort[] BitmapXLATData { get; private set; }
        //Demo specific data
        public int ExitModelnum, DestroyedExitModelnum;

        public byte[] sounddata;

        public HAMFile()
        {
            Textures = new List<ushort>();
            TMapInfo = new List<TMAPInfo>();
            Sounds = new List<byte>();
            AltSounds = new List<byte>();
            VClips = new List<VClip>();
            EClips = new List<EClip>();
            WClips = new List<WClip>();
            Robots = new List<Robot>();
            Joints = new List<JointPos>();
            Weapons = new List<Weapon>();
            Models = new List<Polymodel>();
            Gauges = new List<ushort>();
            GaugesHires = new List<ushort>();
            ObjBitmaps = new List<ushort>();
            ObjBitmapPointers = new List<ushort>();
            Cockpits = new List<ushort>();
            Reactors = new List<Reactor>();
            Powerups = new List<Powerup>();
            BitmapXLATData = new ushort[2620];
        }

        public int Read(Stream stream)
        {
            BinaryReader br;
            br = new BinaryReader(stream);
            HAMDataReader bm = new HAMDataReader();

            int sig = br.ReadInt32();
            if (sig != 0x214D4148)
            {
                br.Dispose();
                return -1;
            }
            version = br.ReadInt32();
            if (version < 2 || version > 3)
            {
                br.Dispose();
                return -2;
            }
            int sndptr = 0;
            if (version == 2)
            {
                sndptr = br.ReadInt32();
            }

            int NumBitmaps = br.ReadInt32();
            for (int x = 0; x < NumBitmaps; x++)
            {
                Textures.Add(br.ReadUInt16());
            }
            for (int x = 0; x < NumBitmaps; x++)
            {
                TMapInfo.Add(bm.ReadTMAPInfo(br));
                TMapInfo[x].ID = x;
            }
            
            int NumSounds = br.ReadInt32();
            for (int x = 0; x < NumSounds; x++)
            {
                Sounds.Add(br.ReadByte());
            }
            for (int x = 0; x < NumSounds; x++)
            {
                AltSounds.Add(br.ReadByte());
            }
            
            int NumVClips = br.ReadInt32();
            for (int x = 0; x < NumVClips; x++)
            {
                VClips.Add(bm.ReadVClip(br));
                VClips[x].ID = x;
            }
            
            int NumEClips = br.ReadInt32();
            for (int x = 0; x < NumEClips; x++)
            {
                EClips.Add(bm.ReadEClip(br));
                EClips[x].ID = x;
            }
            
            int NumWallAnims = br.ReadInt32();
            for (int x = 0; x < NumWallAnims; x++)
            {
                WClips.Add(bm.ReadWClip(br));
            }
            
            int NumRobots = br.ReadInt32();
            for (int x = 0; x < NumRobots; x++)
            {
                Robots.Add(bm.ReadRobot(br));
                Robots[x].ID = x;
            }
            
            int NumLoadJoints = br.ReadInt32();
            for (int x = 0; x < NumLoadJoints; x++)
            {
                JointPos joint = new JointPos();
                joint.jointnum = br.ReadInt16();
                joint.angles.p = br.ReadInt16();
                joint.angles.b = br.ReadInt16();
                joint.angles.h = br.ReadInt16();
                Joints.Add(joint);
            }

            int NumWeaponTypes = br.ReadInt32();
            for (int x = 0; x < NumWeaponTypes; x++)
            {
                if (version >= 3)
                {
                    Weapons.Add(bm.ReadWeapon(br));
                }
                else
                {
                    Weapons.Add(bm.ReadWeaponInfoVersion2(br));
                }
                Weapons[x].ID = x;
            }

            int NumPowerups = br.ReadInt32();
            for (int x = 0; x < NumPowerups; x++)
            {
                Powerup powerup = new Powerup();
                powerup.VClipNum = br.ReadInt32();
                powerup.HitSound = br.ReadInt32();
                powerup.Size = new Fix(br.ReadInt32());
                powerup.Light = new Fix(br.ReadInt32());
                powerup.ID = x;
                Powerups.Add(powerup);
            }
            
            int NumPolygonModels = br.ReadInt32();
            for (int x = 0; x < NumPolygonModels; x++)
            {
                Models.Add(bm.ReadPolymodelInfo(br));
                Models[x].ID = x;
            }

            for (int x = 0; x < NumPolygonModels; x++)
            {
                PolymodelData modeldata = new PolymodelData(Models[x].ModelIDTASize);
                for (int y = 0; y < Models[x].ModelIDTASize; y++)
                {
                    modeldata.InterpreterData[y] = br.ReadByte();
                }
                Models[x].Data = modeldata;
                //PolymodelData.Add(modeldata);
            }
            for (int x = 0; x < NumPolygonModels; x++)
            {
                Models[x].DyingModelnum = br.ReadInt32();
            }
            for (int x = 0; x < NumPolygonModels; x++)
            {
                Models[x].DeadModelnum = br.ReadInt32();
            }
            int gagueCount = br.ReadInt32();
            for (int x = 0; x < gagueCount; x++)
            {
                Gauges.Add(br.ReadUInt16());
            }
            for (int x = 0; x < gagueCount; x++)
            {
                GaugesHires.Add(br.ReadUInt16());
            }
            
            int bitmapCount = br.ReadInt32();
            for (int x = 0; x < bitmapCount; x++)
            {
                ObjBitmaps.Add(br.ReadUInt16());
            }
            ushort value;
            for (int x = 0; x < bitmapCount; x++)
            {
                value = br.ReadUInt16();
                if ((value+1) > NumObjBitmaps)
                    NumObjBitmaps = (value+1);
                ObjBitmapPointers.Add(value);
            }
            
            PlayerShip = new Ship();
            PlayerShip.ModelNum = br.ReadInt32();
            PlayerShip.DeathVClipNum = br.ReadInt32();
            PlayerShip.Mass = new Fix(br.ReadInt32());
            PlayerShip.Drag = new Fix(br.ReadInt32());
            PlayerShip.MaxThrust = new Fix(br.ReadInt32());
            PlayerShip.ReverseThrust = new Fix(br.ReadInt32());
            PlayerShip.Brakes = new Fix(br.ReadInt32());
            PlayerShip.Wiggle = new Fix(br.ReadInt32());
            PlayerShip.MaxRotationThrust = new Fix(br.ReadInt32());
            for (int x = 0; x < 8; x++)
            {
                PlayerShip.GunPoints[x] = FixVector.FromRawValues(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
            }
            
            int NumCockpits = br.ReadInt32();
            for (int x = 0; x < NumCockpits; x++)
            {
                Cockpits.Add(br.ReadUInt16());
            }
            //Build a table of all multiplayer bitmaps, to inject into the object bitmap table
            FirstMultiBitmapNum = br.ReadInt32();

            int NumReactors = br.ReadInt32();
            for (int x = 0; x < NumReactors; x++)
            {
                Reactor reactor = new Reactor();
                reactor.ModelNum = br.ReadInt32();
                reactor.NumGuns = br.ReadInt32();
                for (int y = 0; y < 8; y++)
                {
                    reactor.GunPoints[y] = FixVector.FromRawValues(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                }
                for (int y = 0; y < 8; y++)
                {
                    reactor.GunDirs[y] = FixVector.FromRawValues(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                }
                Reactors.Add(reactor);
            }
            PlayerShip.MarkerModel = br.ReadInt32();
            //2620
            if (version < 3)
            {
                ExitModelnum = br.ReadInt32();
                DestroyedExitModelnum = br.ReadInt32();
            }
            for (int x = 0; x < 2600; x++)
            {
                BitmapXLATData[x] = br.ReadUInt16();
            }

            if (version < 3)
            {
                br.BaseStream.Seek(sndptr, SeekOrigin.Begin);
                int dataToRead = (int)(br.BaseStream.Length - br.BaseStream.Position);
                sounddata = br.ReadBytes(dataToRead);
            }

            hasRead = true;
            //br.Dispose();

            return 0;
        }

        public void Write(Stream stream, bool compatObjBitmaps)
        {
            HAMDataWriter writer = new HAMDataWriter();
            BinaryWriter bw = new BinaryWriter(stream);
            bw.Write(558711112);
            bw.Write(version);
            int returnPoint = (int)bw.BaseStream.Position;
            if (version < 3)
            {
                bw.Write(0);
            }
            bw.Write(Textures.Count);
            for (int x = 0; x < Textures.Count; x++)
            {
                ushort texture = Textures[x];
                bw.Write(texture);
            }
            for (int x = 0; x < TMapInfo.Count; x++)
            {
                TMAPInfo texture = TMapInfo[x];
                writer.WriteTMAPInfo(texture, bw);
            }

            bw.Write(Sounds.Count);
            for (int x = 0; x < Sounds.Count; x++)
            {
                byte sound = Sounds[x];
                bw.Write(sound);
            }
            for (int x = 0; x < Sounds.Count; x++)
            {
                byte sound = AltSounds[x];
                bw.Write(sound);
            }
            bw.Write(VClips.Count);
            for (int x = 0; x < VClips.Count; x++)
            {
                writer.WriteVClip(VClips[x], bw);
            }
            bw.Write(EClips.Count);
            for (int x = 0; x < EClips.Count; x++)
            {
                writer.WriteEClip(EClips[x], bw);
            }
            bw.Write(WClips.Count);
            for (int x = 0; x < WClips.Count; x++)
            {
                writer.WriteWClip(WClips[x], bw);
            }
            bw.Write(Robots.Count);
            for (int x = 0; x < Robots.Count; x++)
            {
                writer.WriteRobot(Robots[x], bw);
            }
            bw.Write(Joints.Count);
            for (int x = 0; x < Joints.Count; x++)
            {
                JointPos joint = Joints[x];
                bw.Write(joint.jointnum);
                bw.Write(joint.angles.p);
                bw.Write(joint.angles.b);
                bw.Write(joint.angles.h);
            }
            bw.Write(Weapons.Count);
            if (version < 3)
            {
                for (int x = 0; x < Weapons.Count; x++)
                {
                    writer.WriteWeaponV2(Weapons[x], bw);
                }
            }
            else
            {
                for (int x = 0; x < Weapons.Count; x++)
                {
                    writer.WriteWeapon(Weapons[x], bw);
                }
            }
            bw.Write(Powerups.Count);
            for (int x = 0; x < Powerups.Count; x++)
            {
                Powerup powerup = Powerups[x];
                bw.Write(powerup.VClipNum);
                bw.Write(powerup.HitSound);
                bw.Write(powerup.Size.value);
                bw.Write(powerup.Light.value);
            }
            bw.Write(Models.Count);
            for (int x = 0; x < Models.Count; x++)
            {
                writer.WritePolymodel(Models[x], bw);
            }
            for (int x = 0; x < Models.Count; x++)
            {
                PolymodelData pmd = Models[x].Data;
                bw.Write(pmd.InterpreterData);
            }
            for (int x = 0; x < Models.Count; x++)
            {
                int modelnum = Models[x].DyingModelnum;
                bw.Write(modelnum);
            }
            for (int x = 0; x < Models.Count; x++)
            {
                int modelnum = Models[x].DeadModelnum;
                bw.Write(modelnum);
            }
            bw.Write(Gauges.Count);
            for (int x = 0; x < Gauges.Count; x++)
            {
                ushort gague = Gauges[x];
                bw.Write(gague);
            }
            for (int x = 0; x < Gauges.Count; x++)
            {
                ushort gague = GaugesHires[x];
                bw.Write(gague);
            }
            //Always write exactly 600 ObjBitmaps, the limit in Descent 2, to conform to the original data files.
            //Can be optimized if you need to save a kb of data I guess
            bw.Write(600);
            for (int x = 0; x < 600; x++)
            {
                if (x < ObjBitmaps.Count)
                {
                    bw.Write(ObjBitmaps[x]);
                }
                else
                {
                    bw.Write((ushort)0);
                }
            }
            for (int x = 0; x < 600; x++)
            {
                if (x < ObjBitmapPointers.Count)
                {
                    bw.Write(ObjBitmapPointers[x]);
                }
                else
                {
                    bw.Write((ushort)0);
                }
            }
            writer.WritePlayerShip(PlayerShip, bw);
            bw.Write(Cockpits.Count);
            for (int x = 0; x < Cockpits.Count; x++)
            {
                ushort cockpit = Cockpits[x];
                bw.Write(cockpit);
            }
            bw.Write(FirstMultiBitmapNum);
            bw.Write(Reactors.Count);
            for (int x = 0; x < Reactors.Count; x++)
            {
                Reactor reactor = Reactors[x];
                bw.Write(reactor.ModelNum);
                bw.Write(reactor.NumGuns);
                for (int y = 0; y < 8; y++)
                {
                    bw.Write(reactor.GunPoints[y].x.value);
                    bw.Write(reactor.GunPoints[y].y.value);
                    bw.Write(reactor.GunPoints[y].z.value);
                }
                for (int y = 0; y < 8; y++)
                {
                    bw.Write(reactor.GunDirs[y].x.value);
                    bw.Write(reactor.GunDirs[y].y.value);
                    bw.Write(reactor.GunDirs[y].z.value);
                }
            }
            bw.Write(PlayerShip.MarkerModel);
            if (version < 3)
            {
                bw.Write(ExitModelnum);
                bw.Write(DestroyedExitModelnum);
            }
            for (int x = 0; x < 2600; x++)
            {
                bw.Write(BitmapXLATData[x]);
            }
            int ptr = (int)bw.BaseStream.Position;
            if (version < 3)
            {
                bw.BaseStream.Seek(returnPoint, SeekOrigin.Begin);
                bw.Write(ptr);
                bw.BaseStream.Seek(ptr, SeekOrigin.Begin);
                bw.Write(sounddata);
            }
            bw.Dispose();
        }

        public TMAPInfo GetTMAPInfo(int id)
        {
            if (id == -1) return null;
            return TMapInfo[id];
        }

        public VClip GetVClip(int id)
        {
            if (id == -1 || id == 255) return null;
            return VClips[id];
        }

        public EClip GetEClip(int id)
        {
            if (id == -1) return null;
            return EClips[id];
        }

        public WClip GetWClip(int id)
        {
            if (id == -1) return null;
            return WClips[id];
        }

        public Robot GetRobot(int id)
        {
            if (id == -1) return null;
            return Robots[id];
        }

        public Weapon GetWeapon(int id)
        {
            if (id == -1) return null;
            return Weapons[id];
        }

        public Polymodel GetModel(int id)
        {
            if (id == -1) return null;
            return Models[id];
        }

        public Powerup GetPowerup(int id)
        {
            if (id == -1) return null;
            return Powerups[id];
        }

        public Reactor GetReactor(int id)
        {
            if (id == -1) return null;
            return Reactors[id];
        }
    }
}
