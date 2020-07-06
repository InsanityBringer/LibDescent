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
using System.Linq;

namespace LibDescent.Data
{
    public class Descent1PIGFile
    {
        private int DataPointer;
        private bool big;
        public bool LoadData { get; private set; }
        public List<PIGImage> Bitmaps { get; private set; }
        public List<SoundData> PIGSounds { get; private set; }

        /// <summary>
        /// Amount of textures considered used by this PIG file.
        /// </summary>
        public int numTextures;
        /// <summary>
        /// List of piggy IDs of all the textures available for levels.
        /// </summary>
        public ushort[] Textures { get; private set; }
        /// <summary>
        /// List of information for mapping textures into levels.
        /// </summary>
        public TMAPInfo[] TMapInfo { get; private set; }
        /// <summary>
        /// List of sound IDs.
        /// </summary>
        public byte[] Sounds { get; private set; }
        /// <summary>
        /// List to remap given sounds into other sounds when Descent is run in low memory mode.
        /// </summary>
        public byte[] AltSounds { get; private set; }
        /// <summary>
        /// Number of VClips considered used by this PIG file. Not used in vanilla descent 1.
        /// </summary>
        public int numVClips;
        /// <summary>
        /// List of all VClip animations.
        /// </summary>
        public VClip[] VClips { get; private set; }
        /// <summary>
        /// Number of EClips considered used by this PIG file.
        /// </summary>
        public int numEClips;
        /// <summary>
        /// List of all Effect animations.
        /// </summary>
        public EClip[] EClips { get; private set; }
        /// <summary>
        /// Number of WClips considered used by this PIG file.
        /// </summary>
        public int numWClips;
        /// <summary>
        /// List of all Wall (door) animations.
        /// </summary>
        public WClip[] WClips { get; private set; }
        /// <summary>
        /// Number of Robots considered used by this PIG file.
        /// </summary>
        public int numRobots;
        /// <summary>
        /// List of all robots.
        /// </summary>
        public Robot[] Robots { get; private set; }
        /// <summary>
        /// Number of Joints considered used by this PIG file.
        /// </summary>
        public int numJoints;
        /// <summary>
        /// List of all robot joints used for animation.
        /// </summary>
        public JointPos[] Joints { get; private set; }
        /// <summary>
        /// Number of Weapons considered used by this PIG file.
        /// </summary>
        public int numWeapons;
        /// <summary>
        /// List of all weapons.
        /// </summary>
        public Weapon[] Weapons { get; private set; }
        /// <summary>
        /// Number of Models considered used by this PIG file.
        /// </summary>
        public int numModels;
        /// <summary>
        /// List of all polymodels.
        /// </summary>
        public Polymodel[] Models { get; private set; }
        /// <summary>
        /// List of gauge piggy IDs.
        /// </summary>
        public ushort[] Gauges { get; private set; }
        public int NumObjBitmaps = 0; //This is important to track the unique number of obj bitmaps, to know where to inject new ones. 
        public int NumObjBitmapPointers = 0; //Also important to tell how many obj bitmap pointer slots the user have left. 
        /// <summary>
        /// List of piggy IDs available for polymodels.
        /// </summary>
        public ushort[] ObjBitmaps { get; private set; }
        /// <summary>
        /// List of pointers into the ObjBitmaps table for polymodels.
        /// </summary>
        public ushort[] ObjBitmapPointers { get; private set; }
        /// <summary>
        /// The player ship.
        /// </summary>
        public Ship PlayerShip;
        /// <summary>
        /// Number of Cockpits considered used by this PIG file.
        /// </summary>
        int numCockpits;
        /// <summary>
        /// List of piggy IDs for all heads-up display modes.
        /// </summary>
        public ushort[] Cockpits { get; private set; }
        /// <summary>
        /// Number of editor object definitions considered used by this PIG file.
        /// </summary>
        public int numObjects;
        /// <summary>
        /// Editor object defintions. Not generally useful, but contains reactor model number.  
        /// </summary>
        public EditorObjectDefinition[] ObjectTypes { get; private set; }
        /// <summary>
        /// The singular reactor.
        /// </summary>
        public Reactor reactor;
        /// <summary>
        /// Number of Powerups considered used by this PIG file.
        /// </summary>
        public int numPowerups;
        /// <summary>
        /// List of all powerups.
        /// </summary>
        public Powerup[] Powerups { get; private set; }
        /// <summary>
        /// The index in the ObjBitmapPointers table of the first multiplayer color texture.
        /// </summary>
        public int FirstMultiBitmapNum;
        /// <summary>
        /// Table to remap piggy IDs to other IDs for low memory mode.
        /// </summary>
        public ushort[] BitmapXLATData { get; private set; }

        public int exitModelnum, destroyedExitModelnum;

        public Descent1PIGFile(bool macPig = false, bool loadData = true)
        {
            Textures = new ushort[800];
            TMapInfo = new TMAPInfo[800];
            Sounds = new byte[250];
            AltSounds = new byte[250];
            VClips = new VClip[70];
            EClips = new EClip[60];
            WClips = new WClip[30];
            Robots = new Robot[30];
            Joints = new JointPos[600];
            Weapons = new Weapon[30];
            Models = new Polymodel[85];
            if (macPig)
                Gauges = new ushort[85];
            else
                Gauges = new ushort[80];
            ObjBitmaps = new ushort[210];
            ObjBitmapPointers = new ushort[210];
            Cockpits = new ushort[4];
            ObjectTypes = new EditorObjectDefinition[100];
            Powerups = new Powerup[29];
            BitmapXLATData = new ushort[1800];
            reactor = new Reactor();

            Bitmaps = new List<PIGImage>();
            PIGSounds = new List<SoundData>();

            this.big = macPig;
            this.LoadData = loadData;
        }

        public int Read(Stream stream, StringBuilder sb = null)
        {
            BinaryReader br = new BinaryReader(stream);
            HAMDataReader reader = new HAMDataReader();

            if (LoadData)
            {
                DataPointer = br.ReadInt32();
                //So there's no sig, so we're going to take a guess based on the pointer. If it's greater than max bitmaps, we'll assume it's a descent 1 1.4+ piggy file
                if (DataPointer <= 1800)
                {
                    br.Dispose();
                    return -1;
                }

                sb?.AppendLine($"Start {stream.Position}");

                numTextures = br.ReadInt32();
                for (int i = 0; i < 800; i++)
                {
                    Textures[i] = br.ReadUInt16();
                }

                sb?.AppendLine($"Post Textures {stream.Position}");

                for (int i = 0; i < 800; i++)
                {
                    TMapInfo[i] = reader.ReadTMAPInfoDescent1(br);
                }

                sb?.AppendLine($"Post tmap {stream.Position}");

                Sounds = br.ReadBytes(250);
                AltSounds = br.ReadBytes(250);

                numVClips = br.ReadInt32(); //this value is bogus. rip
                for (int i = 0; i < 70; i++)
                {
                    VClips[i] = reader.ReadVClip(br);
                }
                sb?.AppendLine($"Post VClips:\t{stream.Position}");

                numEClips = br.ReadInt32();
                for (int i = 0; i < 60; i++)
                {
                    EClips[i] = reader.ReadEClip(br);
                }
                sb?.AppendLine($"Post EClips:\t{stream.Position}");

                numWClips = br.ReadInt32();
                for (int i = 0; i < 30; i++)
                {
                    WClips[i] = reader.ReadWClipDescent1(br);
                }
                sb?.AppendLine($"Post WClips:\t{stream.Position}");

                numRobots = br.ReadInt32();
                for (int i = 0; i < 30; i++) 
                {
                    Robots[i] = reader.ReadRobotDescent1(br);
                }

                sb?.AppendLine($"Post robots:\t{stream.Position}");

                numJoints = br.ReadInt32();
                for (int i = 0; i < 600; i++)
                {
                    JointPos joint = new JointPos();
                    joint.jointnum = br.ReadInt16();
                    joint.angles.p = br.ReadInt16();
                    joint.angles.b = br.ReadInt16();
                    joint.angles.h = br.ReadInt16();
                    Joints[i] = joint;
                }

                sb?.AppendLine($"Post joints:\t{stream.Position}");

                numWeapons = br.ReadInt32();
                for (int i = 0; i < 30; i++)
                {
                    Weapons[i] = reader.ReadWeaponInfoDescent1(br);
                }

                sb?.AppendLine($"Post Weapons:\t{stream.Position}");

                numPowerups = br.ReadInt32();
                for (int i = 0; i < 29; i++)
                {
                    Powerup powerup = new Powerup();
                    powerup.VClipNum = br.ReadInt32();
                    powerup.HitSound = br.ReadInt32();
                    powerup.Size = new Fix(br.ReadInt32());
                    powerup.Light = new Fix(br.ReadInt32());
                    Powerups[i] = powerup;
                }

                sb?.AppendLine($"Post powerups:\t{stream.Position}");

                numModels = br.ReadInt32();
                for (int i = 0; i < numModels; i++)
                {
                    Models[i] = reader.ReadPolymodelInfo(br);
                }
                for (int i = 0; i < numModels; i++)
                {
                    Models[i].InterpreterData = br.ReadBytes(Models[i].ModelIDTASize);
                }
                for (int i = 0; i < Gauges.Length; i++)
                {
                    Gauges[i] = br.ReadUInt16();
                }
                for (int i = 0; i < 85; i++)
                {
                    int num = br.ReadInt32();
                    if (i < numModels)
                    {
                        Models[i].DyingModelnum = num;
                    }
                    else
                    {
                        int wtfIsThis = num;
                    }
                }
                for (int i = 0; i < 85; i++)
                {
                    int num = br.ReadInt32();

                    if (i < numModels)
                    {
                        Models[i].DeadModelnum = num;
                    }
                    else
                    {
                        int wtfIsThis = num;
                    }
                }

                for (int i = 0; i < 210; i++)
                {
                    ObjBitmaps[i] = br.ReadUInt16();
                }
                for (int i = 0; i < 210; i++)
                {
                    ObjBitmapPointers[i] = br.ReadUInt16();
                }

                sb?.AppendLine($"Pre ship:\t{stream.Position}");

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
                numCockpits = br.ReadInt32();
                for (int i = 0; i < 4; i++)
                {
                    Cockpits[i] = br.ReadUInt16();
                }

                sb?.AppendLine($"Post cockpits:\t{stream.Position}");

                //heh
                Sounds = br.ReadBytes(250);
                AltSounds = br.ReadBytes(250);

                numObjects = br.ReadInt32();
                for (int i = 0; i < 100; i++)
                {
                    ObjectTypes[i].type = (EditorObjectType)(br.ReadSByte());
                }
                for (int i = 0; i < 100; i++)
                {
                    ObjectTypes[i].id = br.ReadByte();
                }
                for (int i = 0; i < 100; i++)
                {
                    ObjectTypes[i].strength = new Fix(br.ReadInt32());
                    //Console.WriteLine("type: {0}({3})\nid: {1}\nstr: {2}", ObjectTypes[i].type, ObjectTypes[i].id, ObjectTypes[i].strength, (int)ObjectTypes[i].type);
                }
                FirstMultiBitmapNum = br.ReadInt32();
                reactor.NumGuns = br.ReadInt32();
                for (int y = 0; y < 4; y++)
                {
                    reactor.GunPoints[y] = FixVector.FromRawValues(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                }
                for (int y = 0; y < 4; y++)
                {
                    reactor.GunDirs[y] = FixVector.FromRawValues(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                }

                sb?.AppendLine($"Post reactor:\t{stream.Position}");

                exitModelnum = br.ReadInt32();
                destroyedExitModelnum = br.ReadInt32();

                for (int i = 0; i < 1800; i++)
                {
                    BitmapXLATData[i] = br.ReadUInt16();
                }

                sb?.AppendLine($"Post XLAT:\t{stream.Position}");
            }

            /*
            //Init a bogus texture for all piggyfiles
            PIGImage bogusTexture = new PIGImage(64, 64, 0, 0, 0, 0, "bogus", 0);
            bogusTexture.Data = new byte[64 * 64];
            //Create an X using descent 1 palette indicies. For accuracy. Heh
            for (int i = 0; i < 4096; i++)
            {
                bogusTexture.Data[i] = 85;
            }
            for (int i = 0; i < 64; i++)
            {
                bogusTexture.Data[i * 64 + i] = 193;
                bogusTexture.Data[i * 64 + (63 - i)] = 193;
            }
            Bitmaps.Add(bogusTexture);
            */

            if (LoadData)
                br.BaseStream.Seek(DataPointer, SeekOrigin.Begin);

            int numBitmaps = br.ReadInt32();
            int numSounds = br.ReadInt32();

            sb?.AppendLine($"Post PIGSounds:\t{stream.Position}");

            sb?.AppendLine($"Bitmap Count:\t{numBitmaps}");

            for (int i = 0; i < numBitmaps; i++)
            {
                bool hashitnull = false;

                byte[] localNameBytes = br.ReadBytes(8);
                char[] localname = new char[8];

                for (int j = 0; j < 8; j++)
                {
                    char c = (char)localNameBytes[j];

                    if (c == 0)
                        hashitnull = true;
                    if (!hashitnull)
                        localname[j] = c;
                }

                string imagename = new String(localname);
                imagename = imagename.Trim(' ', '\0');
                byte framedata = br.ReadByte();
                byte lx = br.ReadByte();
                byte ly = br.ReadByte();
                byte flags = br.ReadByte();
                byte average = br.ReadByte();
                int offset = br.ReadInt32();

                PIGImage image = new PIGImage(lx, ly, framedata, flags, average, offset, imagename, big);
                image.LocalName = localNameBytes;
                Bitmaps.Add(image);
            }

            sb?.AppendLine($"Post BMPS:\t{stream.Position}");


            for (int i = 0; i < numSounds; i++)
            {
                bool hashitnull = false;

                byte[] localNameBytes = br.ReadBytes(8);
                char[] localname = new char[8];

                for (int j = 0; j < 8; j++)
                {
                    char c = (char)localNameBytes[j];

                    if (c == 0)
                        hashitnull = true;
                    if (!hashitnull)
                        localname[j] = c;
                }

                string soundname = new string(localname);
                soundname = soundname.Trim(' ', '\0');
                int num1 = br.ReadInt32();
                int num2 = br.ReadInt32();
                int offset = br.ReadInt32();

                SoundData sound = new SoundData { data = null };
                sound.name = soundname;
                sound.localName = localNameBytes;
                sound.offset = offset;
                sound.len = num1;
                PIGSounds.Add(sound);
            }
            
            int basePointer = (int)br.BaseStream.Position;

            sb?.AppendLine($"Pre bitmaps:\t{stream.Position}");

            for (int i = 0; i < Bitmaps.Count; i++)
            {
                br.BaseStream.Seek(basePointer + Bitmaps[i].Offset, SeekOrigin.Begin);
                if ((Bitmaps[i].Flags & PIGImage.BM_FLAG_RLE) != 0)
                {
                    int compressedSize = br.ReadInt32();
                    Bitmaps[i].Data = br.ReadBytes(compressedSize - 4);
                }
                else
                {
                    Bitmaps[i].Data = br.ReadBytes(Bitmaps[i].Width * Bitmaps[i].Height);
                }
            }

            sb?.AppendLine($"Pre sounds:\t{stream.Position}");


            for (int i = 0; i < PIGSounds.Count; i++)
            {
                br.BaseStream.Seek(basePointer + PIGSounds[i].offset, SeekOrigin.Begin);

                var soundBytes = br.ReadBytes(PIGSounds[i].len);

                var ps = PIGSounds[i];

                ps.data = soundBytes;
            }

            sb?.AppendLine($"End:\t{stream.Position}");

            br.Dispose();

            return 0;
        }

        public int Write(Stream stream, StringBuilder sb = null)
        {
            //BinaryReader br = new BinaryReader(stream);

            BinaryWriter bw = new BinaryWriter(stream);
            //HAMDataReader reader = new HAMDataReader();
            HAMDataWriter writer = new HAMDataWriter();

            Int32 DataPointer = 0; // update this later on

            {
                bw.Write(DataPointer); // update this later on

                sb?.AppendLine($"Start {stream.Position}");

                bw.Write((Int32)numTextures);

                for (int i = 0; i < 800; i++)
                {
                    //Textures[i] = br.ReadUInt16();
                    bw.Write((UInt16)Textures[i]);
                }

                sb?.AppendLine($"Post Textures {stream.Position}");

                for (int i = 0; i < 800; i++)
                {
                    // TMapInfo[i] = reader.ReadTMAPInfoDescent1(br);
                    writer.WriteTMAPInfoDescent1(bw, TMapInfo[i]);
                }

                sb?.AppendLine($"Post tmap {stream.Position}");

                //Sounds = bw.ReadBytes(250);
                bw.Write(Sounds);

                //AltSounds = br.ReadBytes(250);
                bw.Write(AltSounds);

                //numVClips = br.ReadInt32(); //this value is bogus. rip
                bw.Write((Int32)numVClips); //this value is bogus. rip

                for (int i = 0; i < 70; i++)
                {
                    //VClips[i] = reader.ReadVClip(br);
                    writer.WriteVClip(VClips[i], bw);
                }
                sb?.AppendLine($"Post VClips:\t{stream.Position}");

                //numEClips = br.ReadInt32();
                bw.Write((Int32)numEClips);

                for (int i = 0; i < 60; i++)
                {
                    //EClips[i] = reader.ReadEClip(br);
                    writer.WriteEClip(EClips[i], bw);
                }
                sb?.AppendLine($"Post EClips:\t{stream.Position}");

                //numWClips = br.ReadInt32();
                bw.Write((Int32)numWClips);
                for (int i = 0; i < 30; i++)
                {
                    //WClips[i] = reader.ReadWClipDescent1(br);
                    writer.WriteWClipD1(WClips[i], bw);
                }
                sb?.AppendLine($"Post WClips:\t{stream.Position}");

                //numRobots = br.ReadInt32();
                bw.Write((Int32)numRobots);
                for (int i = 0; i < 30; i++)
                {
                    //Robots[i] = reader.ReadRobotDescent1(br);
                    writer.WriteRobotDescent1(Robots[i], bw);
                }
                sb?.AppendLine($"Post robots:\t{stream.Position}");

                //numJoints = br.ReadInt32();
                bw.Write((Int32)numJoints);
                for (int i = 0; i < 600; i++)
                {
                    //JointPos joint = new JointPos();
                    JointPos joint = Joints[i];

                    //joint.jointnum = br.ReadInt16();
                    bw.Write((Int16)joint.jointnum);
                    //joint.angles.p = br.ReadInt16();
                    bw.Write((Int16)joint.angles.p);
                    //joint.angles.b = br.ReadInt16();
                    bw.Write((Int16)joint.angles.b);
                    //joint.angles.h = br.ReadInt16();
                    bw.Write((Int16)joint.angles.h);
                }

                sb?.AppendLine($"Post joints:\t{stream.Position}");

                //numWeapons = br.ReadInt32();
                bw.Write((Int32)numWeapons);
                for (int i = 0; i < 30; i++)
                {
                    //Weapons[i] = reader.ReadWeaponInfoDescent1(br);
                    writer.WriteWeaponInfoDescent1(bw, Weapons[i]);
                }

                sb?.AppendLine($"Post Weapons:\t{stream.Position}");

                //numPowerups = br.ReadInt32();
                bw.Write((Int32)numPowerups);
                for (int i = 0; i < 29; i++)
                {
                    //Powerup powerup = new Powerup();

                    var powerup = this.Powerups[i];

                    //powerup.VClipNum = br.ReadInt32();
                    HAMDataWriter.WriteInt32(bw, powerup.VClipNum);
                    //powerup.HitSound = br.ReadInt32();
                    HAMDataWriter.WriteInt32(bw, powerup.HitSound);
                    //powerup.Size = new Fix(br.ReadInt32());
                    HAMDataWriter.WriteInt32(bw, powerup.Size.Value);
                    //powerup.Light = new Fix(br.ReadInt32());
                    HAMDataWriter.WriteInt32(bw, powerup.Light.Value);
                }

                sb?.AppendLine($"Post powerups:\t{stream.Position}");

                //numModels = br.ReadInt32();
                bw.Write((Int32)numModels);
                for (int i = 0; i < numModels; i++)
                {
                    //Models[i] = reader.ReadPolymodelInfo(br);
                    writer.WritePolymodel(Models[i], bw);
                }

                for (int i = 0; i < numModels; i++)
                {
                    //Models[i].InterpreterData = br.ReadBytes(Models[i].ModelIDTASize);
                    bw.Write(Models[i].InterpreterData, 0, Models[i].ModelIDTASize);
                }

                for (int i = 0; i < Gauges.Length; i++)
                {
                    //Gauges[i] = br.ReadUInt16();
                    writer.WriteUInt16(bw, Gauges[i]);
                }

                for (int i = 0; i < 85; i++)
                {
                    //int num = br.ReadInt32();
                    //if (i < numModels)
                    //    Models[i].DyingModelnum = num;

                    if (Models[i] == null)
                    {
                        HAMDataWriter.WriteInt32(bw, -1);
                    }
                    else
                    {
                        HAMDataWriter.WriteInt32(bw, Models[i].DyingModelnum);
                    }
                }

                for (int i = 0; i < 85; i++)
                {

                    //int num = br.ReadInt32();
                    //if (i < numModels)
                    //    Models[i].DeadModelnum = num;

                    if (Models[i] == null)
                    {
                        HAMDataWriter.WriteInt32(bw, -1);
                    }
                    else
                    {
                        HAMDataWriter.WriteInt32(bw, Models[i].DeadModelnum);
                    }
                }

                for (int i = 0; i < 210; i++)
                {
                    //ObjBitmaps[i] = br.ReadUInt16();
                    writer.WriteUInt16(bw, ObjBitmaps[i]);
                }

                for (int i = 0; i < 210; i++)
                {
                    //ObjBitmapPointers[i] = br.ReadUInt16();
                    writer.WriteUInt16(bw, ObjBitmapPointers[i]);
                }

                sb?.AppendLine($"Pre ship:\t{stream.Position}");

                //PlayerShip = new Ship();
                //PlayerShip.ModelNum = br.ReadInt32();
                HAMDataWriter.WriteInt32(bw, PlayerShip.ModelNum);
                //PlayerShip.DeathVClipNum = br.ReadInt32();
                HAMDataWriter.WriteInt32(bw, PlayerShip.DeathVClipNum);
                //PlayerShip.Mass = new Fix(br.ReadInt32());
                HAMDataWriter.WriteInt32(bw, PlayerShip.Mass.Value);
                //PlayerShip.Drag = new Fix(br.ReadInt32());
                HAMDataWriter.WriteInt32(bw, PlayerShip.Drag.Value);
                //PlayerShip.MaxThrust = new Fix(br.ReadInt32());
                HAMDataWriter.WriteInt32(bw, PlayerShip.MaxThrust.Value);
                //PlayerShip.ReverseThrust = new Fix(br.ReadInt32());
                HAMDataWriter.WriteInt32(bw, PlayerShip.ReverseThrust.Value);
                //PlayerShip.Brakes = new Fix(br.ReadInt32());
                HAMDataWriter.WriteInt32(bw, PlayerShip.Brakes.Value);
                //PlayerShip.Wiggle = new Fix(br.ReadInt32());
                HAMDataWriter.WriteInt32(bw, PlayerShip.Wiggle.Value);
                //PlayerShip.MaxRotationThrust = new Fix(br.ReadInt32());
                HAMDataWriter.WriteInt32(bw, PlayerShip.MaxRotationThrust.Value);

                for (int x = 0; x < 8; x++)
                {
                    //PlayerShip.GunPoints[x] = FixVector.FromRawValues(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                    HAMDataWriter.WriteFixVector(bw, PlayerShip.GunPoints[x]);
                }

                //numCockpits = br.ReadInt32();
                bw.Write((Int32)numCockpits); // update the data pointer
                for (int i = 0; i < 4; i++)
                {
                    //Cockpits[i] = br.ReadUInt16();
                    bw.Write((Int16)Cockpits[i]);
                }

                sb?.AppendLine($"Post cockpits:\t{stream.Position}");

                //heh
                //Sounds = br.ReadBytes(250);
                bw.Write(Sounds, 0, 250);
                //AltSounds = br.ReadBytes(250);
                bw.Write(AltSounds, 0, 250);

                //numObjects = br.ReadInt32();
                bw.Write((Int32)numObjects);
                for (int i = 0; i < 100; i++)
                {
                    //ObjectTypes[i].type = (EditorObjectType)(br.ReadSByte());
                    bw.Write((sbyte)ObjectTypes[i].type);
                }
                for (int i = 0; i < 100; i++)
                {
                    //ObjectTypes[i].id = br.ReadByte();
                    bw.Write((byte)ObjectTypes[i].id);
                }
                for (int i = 0; i < 100; i++)
                {
                    //ObjectTypes[i].strength = new Fix(br.ReadInt32());
                    bw.Write(ObjectTypes[i].strength.Value);
                    //Console.WriteLine("type: {0}({3})\nid: {1}\nstr: {2}", ObjectTypes[i].type, ObjectTypes[i].id, ObjectTypes[i].strength, (int)ObjectTypes[i].type);
                }

                //FirstMultiBitmapNum = br.ReadInt32();
                bw.Write((Int32)FirstMultiBitmapNum);
                //reactor.NumGuns = br.ReadInt32();
                bw.Write((Int32)reactor.NumGuns);

                for (int y = 0; y < 4; y++)
                {
                    //reactor.GunPoints[y] = FixVector.FromRawValues(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                    HAMDataWriter.WriteFixVector(bw, reactor.GunPoints[y]);
                }
                for (int y = 0; y < 4; y++)
                {
                    //reactor.GunDirs[y] = FixVector.FromRawValues(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
                    HAMDataWriter.WriteFixVector(bw, reactor.GunDirs[y]);
                }

                sb?.AppendLine($"Post reactor:\t{stream.Position}");

                //exitModelnum = br.ReadInt32();
                bw.Write((Int32)exitModelnum);
                //destroyedExitModelnum = br.ReadInt32();
                bw.Write((Int32)destroyedExitModelnum);

                for (int i = 0; i < 1800; i++)
                {
                    //BitmapXLATData[i] = br.ReadUInt16();
                    bw.Write((Int16)BitmapXLATData[i]);
                }

                sb?.AppendLine($"Post XLAT:\t{stream.Position}");
            }

            // Now update the DataPointer
            DataPointer = (int)bw.BaseStream.Position;

            bw.BaseStream.Seek(0, SeekOrigin.Begin);
            bw.Write((Int32)DataPointer); // update the data pointer

            // Return to where we were
            bw.BaseStream.Seek(DataPointer, SeekOrigin.Begin);

            //int numBitmaps = br.ReadInt32();
            bw.Write((Int32)Bitmaps.Count);
            //int numSounds = br.ReadInt32();
            bw.Write((Int32)PIGSounds.Count);

            sb?.AppendLine($"Post PIGSounds:\t{stream.Position}");

            sb?.AppendLine($"Bitmap Count:\t{Bitmaps.Count}");

            //for (int i = 0; i < numBitmaps; i++)
            for (int i = 0; i < Bitmaps.Count; i++)
            {
                var bitmap = Bitmaps[i];

                bw.Write(bitmap.LocalName,0 , 8);

                writer.WriteByte(bw, (byte)bitmap.DFlags);
                writer.WriteByte(bw, (byte)bitmap.Width);
                writer.WriteByte(bw, (byte)bitmap.Height);
                writer.WriteByte(bw, (byte)bitmap.Flags);
                writer.WriteByte(bw, (byte)bitmap.AverageIndex);

                HAMDataWriter.WriteInt32(bw, bitmap.Offset); // make this one correct later on
            }

            sb?.AppendLine($"Post BMPS:\t{stream.Position}");

            //for (int i = 0; i < numSounds; i++)
            for (int i = 0; i < PIGSounds.Count; i++)
            {
                var sound = PIGSounds[i];

                //var nameBytes = NameHelper.GetNameBytes(sound.name, 8);
                //bw.Write(nameBytes);
                bw.Write(sound.localName, 0, 8);

                HAMDataWriter.WriteInt32(bw, sound.len);
                HAMDataWriter.WriteInt32(bw, sound.len);
                HAMDataWriter.WriteInt32(bw, sound.offset); // make this one correct later on
            }

            //int basePointer = (int)br.BaseStream.Position;
            int basePointer = (int)bw.BaseStream.Position;

            sb?.AppendLine($"Pre bitmaps:\t{stream.Position}");

            for (int i = 0; i < Bitmaps.Count; i++)
            {
                //br.BaseStream.Seek(basePointer + Bitmaps[i].Offset, SeekOrigin.Begin);

                //if ((Bitmaps[i].Flags & PIGImage.BM_FLAG_RLE) != 0)
                //{
                //    int compressedSize = br.ReadInt32();
                //    Bitmaps[i].Data = br.ReadBytes(compressedSize - 4);
                //}
                //else
                //{
                //    Bitmaps[i].Data = br.ReadBytes(Bitmaps[i].Width * Bitmaps[i].Height);
                //}

                Bitmaps[i].WriteImage(bw);
            }
            sb?.AppendLine($"Pre sounds:\t{stream.Position}");

            for (int i = 0; i < PIGSounds.Count; i++)
            {
                //br.BaseStream.Seek(basePointer + PIGSounds[i].offset, SeekOrigin.Begin);

                bw.Write(PIGSounds[i].data);
            }

            sb?.AppendLine($"End:\t{stream.Position}");


            return 0;
        }

    }
}
