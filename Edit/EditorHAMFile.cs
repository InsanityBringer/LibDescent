using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LibDescent.Data;

namespace LibDescent.Edit
{
    public class EditorHAMFile : IDataFile
    {
        public HAMFile BaseFile { get; private set; }

        /// <summary>
        /// Reference to a pig file, for looking up image names.
        /// </summary>
        public PIGFile piggyFile;

        //Data tables.
        //TODO: These need to be rewritten to use new "editor friendly" classes.
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

        //Remapping information
        //Multiplayer color variants, injected into the object bitmap table
        public ushort[] multiplayerBitmaps = new ushort[14];
        //Map EClip names to their IDs, since this is important for model textures
        public Dictionary<string, EClip> EClipNameMapping { get; private set; }
        //Map texture names to the first found ObjBitmap, for ease of finding textures
        public Dictionary<string, int> ObjBitmapMapping { get; private set; }

        //Nametables.
        public List<string> VClipNames { get; private set; }
        public List<string> EClipNames { get; private set; }
        public List<string> RobotNames { get; private set; }
        public List<string> WeaponNames { get; private set; }
        public List<string> ModelNames { get; private set; }
        public List<string> SoundNames { get; private set; }
        public List<string> ReactorNames { get; private set; }
        public List<string> PowerupNames { get; private set; }

        private int NumRobotJoints;

        public bool ExportExtraData { get; set; } = true;
        public bool CompatObjBitmaps { get; set; } = false;

        public EditorHAMFile(HAMFile baseFile, PIGFile piggyFile)
        {
            BaseFile = baseFile;
            this.piggyFile = piggyFile;
            EClipNameMapping = new Dictionary<string, EClip>();
            ObjBitmapMapping = new Dictionary<string, int>();

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

            VClipNames = new List<string>();
            EClipNames = new List<string>();
            RobotNames = new List<string>();
            WeaponNames = new List<string>();
            ModelNames = new List<string>();
            SoundNames = new List<string>();
            ReactorNames = new List<string>();
            PowerupNames = new List<string>();
        }

        public EditorHAMFile(PIGFile piggyFile) : this(new HAMFile(), piggyFile)
        {
        }

        //---------------------------------------------------------------------
        // EDITING
        //---------------------------------------------------------------------

        public int AddElement(HAMType type)
        {
            switch (type)
            {
                case HAMType.EClip:
                    EClip eclip = new EClip();
                    eclip.ID = EClips.Count;
                    EClips.Add(eclip);
                    EClipNames.Add(string.Format("NewEClip{0}", eclip.ID));
                    return eclip.ID;
                case HAMType.Robot:
                    Robot robot = new Robot();
                    robot.ID = Robots.Count;
                    Robots.Add(robot);
                    RobotNames.Add(string.Format("New Robot #{0}", robot.ID));
                    return robot.ID;
                case HAMType.Weapon:
                    Weapon weapon = new Weapon();
                    weapon.ID = Weapons.Count;
                    Weapons.Add(weapon);
                    WeaponNames.Add(string.Format("New Weapon #{0}", weapon.ID));
                    return weapon.ID;
                case HAMType.Model:
                    Polymodel model = new Polymodel();
                    model.ExpandSubmodels();
                    model.ID = Models.Count;
                    Models.Add(model);
                    ModelNames.Add(string.Format("New Polymodel #{0}", model.ID));
                    return model.ID;
            }
            return -1;
        }

        //If you call this function, you're a masochist or don't know what you're doing tbh.
        public int DeleteElement(HAMType type, int slot)
        {
            Console.WriteLine("DeleteElement: STUB");
            /*
            switch (type)
            {
                case HAMType.EClip:
                    {
                        EClip eclip = EClips[slot];
                        int refCount = eclip.References.Count;
                        if (refCount > 0)
                            return -1;
                        EClips.RemoveAt(slot);
                        EClipNameMapping.Remove(EClipNames[slot]);
                        EClipNames.RemoveAt(slot);
                        //eclip.ClearReferences(this);
                        RenumberElements(type);
                        return EClips.Count;
                    }
                case HAMType.Robot:
                    {
                        Robot robot = Robots[slot];
                        int refCount = robot.References.Count;
                        if (refCount > 0)
                            return -1;
                        Robots.RemoveAt(slot);
                        RobotNames.RemoveAt(slot);
                        //robot.ClearReferences(this);
                        RenumberElements(type);
                        return Robots.Count;
                    }
                case HAMType.Weapon:
                    {
                        Weapon weapon = Weapons[slot];
                        int refCount = weapon.References.Count;
                        if (refCount > 0)
                            return -1;
                        Weapons.RemoveAt(slot);
                        WeaponNames.RemoveAt(slot);
                        //weapon.ClearReferences(this);
                        RenumberElements(type);
                        return Weapons.Count;
                    }
                case HAMType.Model:
                    {
                        Polymodel model = PolygonModels[slot];
                        int refCount = model.References.Count;
                        if (refCount > 0)
                            return -1;
                        PolygonModels.RemoveAt(slot);
                        ModelNames.RemoveAt(slot);
                        //model.ClearReferences(this);
                        RenumberElements(type);
                        return PolygonModels.Count;
                    }
            }
            */
            return -1;
        }

        public void UpdateName(HAMType type, int element, string newName)
        {
            switch (type)
            {
                case HAMType.VClip:
                    VClipNames[element] = newName;
                    return;
                case HAMType.EClip:
                    EClipNames[element] = newName;
                    return;
                case HAMType.Robot:
                    RobotNames[element] = newName;
                    return;
                case HAMType.Weapon:
                    WeaponNames[element] = newName;
                    return;
                case HAMType.Sound:
                    SoundNames[element] = newName;
                    return;
                case HAMType.Model:
                    ModelNames[element] = newName;
                    return;
                case HAMType.Powerup:
                    PowerupNames[element] = newName;
                    return;
            }
        }

        public int CopyElement(HAMType type, int source, int destination)
        {
            /*switch (type)
            {
                case HAMType.Robot:
                    if (source < Robots.Count && destination < Robots.Count)
                        Robots[destination].CopyDataFrom(Robots[source], this);
                    else
                        return -1;
                    break;
                case HAMType.Weapon:
                    if (source < Weapons.Count && destination < Weapons.Count)
                        Weapons[destination].CopyDataFrom(Weapons[source], this);
                    else
                        return -1;
                    break;
                default:
                    return 1;
            }*/
            return 0;
        }

        //---------------------------------------------------------------------
        // READING
        //---------------------------------------------------------------------

        /// <summary>
        /// Converts all the base file's data classes into the editor class for that type.
        /// </summary>
        private void CreateLocalLists()
        {
            //TODO: This is just passthrough for now, need "editor" classes
            foreach (ushort texture in BaseFile.Textures)
                Textures.Add(texture);
            foreach (TMAPInfo tmapInfo in BaseFile.TMapInfo)
                TMapInfo.Add(tmapInfo);
            foreach (byte sound in BaseFile.Sounds)
                Sounds.Add(sound);
            foreach (byte sound in BaseFile.AltSounds)
                AltSounds.Add(sound);
            foreach (VClip clip in BaseFile.VClips)
                VClips.Add(clip);
            foreach (EClip clip in BaseFile.EClips)
                EClips.Add(clip);
            foreach (WClip clip in BaseFile.WClips)
                WClips.Add(clip);
            foreach (Robot robot in BaseFile.Robots)
                Robots.Add(robot);
            foreach (JointPos joint in BaseFile.Joints)
                Joints.Add(joint);
            foreach (Weapon weapon in BaseFile.Weapons)
                Weapons.Add(weapon);
            foreach (Polymodel model in BaseFile.Models)
                Models.Add(model);
            foreach (ushort gauge in BaseFile.Gauges)
                Gauges.Add(gauge);
            foreach (ushort gauge in BaseFile.GaugesHires)
                GaugesHires.Add(gauge);
            PlayerShip = BaseFile.PlayerShip;
            foreach (ushort cockpit in BaseFile.Cockpits)
                Cockpits.Add(cockpit);
            foreach (Reactor reactor in BaseFile.Reactors)
                Reactors.Add(reactor);
            foreach (Powerup powerup in BaseFile.Powerups)
                Powerups.Add(powerup);
            FirstMultiBitmapNum = BaseFile.FirstMultiBitmapNum;
            for (int i = 0; i < 2620; i++)
                BitmapXLATData[i] = BaseFile.BitmapXLATData[i];
            foreach (ushort bm in BaseFile.ObjBitmaps)
                ObjBitmaps.Add(bm);
            foreach (ushort bm in BaseFile.ObjBitmapPointers)
                ObjBitmapPointers.Add(bm);
        }

        /// <summary>
        /// Reads the base HAM file from a given stream, and then the additional data used by the editor.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public void Read(Stream stream)
        {
            BaseFile.Read(stream);

            CreateLocalLists();

            BinaryReader br = new BinaryReader(stream);
            bool generateNameLists = true;
            int chunkName;
            while (br.BaseStream.Position <= br.BaseStream.Length - 4) //Still room for some chunks
            {
                chunkName = br.ReadInt32();
                if (chunkName == 0x4E4D4148 && ReadNamefile(br) != -1)
                {
                    generateNameLists = false;
                }
                else if (chunkName == 0x4E50524F)
                {
                    ReadOrphanedModels(br);
                }
            }

            if (generateNameLists)
                GenerateDefaultNamelists();

            UpdateEClipMapping();

            for (int i = 0; i < 14; i++)
            {
                multiplayerBitmaps[i] = (ushort)(BaseFile.ObjBitmaps[BaseFile.ObjBitmapPointers[FirstMultiBitmapNum + i]]);
            }

            foreach (Robot robot in Robots)
            {
                BuildModelAnimation(robot);
            }
            foreach (Reactor reactor in Reactors)
            {
                BuildModelGunsFromReactor(reactor);
            }
            BuildModelGunsFromShip(PlayerShip);
            BuildModelTextureTables();
        }

        /// <summary>
        /// Validates that all referenced elements are valid.
        /// </summary>
        public void ValidateReferences()
        {
            foreach (TMAPInfo info in TMapInfo)
            {
                if (info.EClipNum < -1 || info.EClipNum >= EClips.Count)
                    info.EClipNum = -1;
            }
            foreach (VClip clip in VClips)
            {
                if (clip.SoundNum < -1 || clip.SoundNum >= Sounds.Count)
                    clip.SoundNum = -1;
            }
            foreach (EClip clip in EClips)
            {
                if (clip.SoundNum < -1 || clip.SoundNum >= Sounds.Count)
                    clip.SoundNum = -1;
                if (clip.ExplosionEClip < -1 || clip.ExplosionEClip >= EClips.Count)
                    clip.ExplosionEClip = -1;
                if (clip.ExplosionVClip < -1 || clip.ExplosionVClip >= VClips.Count)
                    clip.ExplosionVClip = -1;
                if (clip.CriticalClip < -1 || clip.CriticalClip >= EClips.Count)
                    clip.CriticalClip = -1;
            }
            foreach (WClip clip in WClips)
            {
                if (clip.OpenSound < -1 || clip.OpenSound >= Sounds.Count)
                    clip.OpenSound = -1;
                if (clip.CloseSound < -1 || clip.CloseSound >= Sounds.Count)
                    clip.CloseSound = -1;
                for (int i = 0; i < 50; i++)
                {
                    if (clip.Frames[i] >= Textures.Count)
                        clip.Frames[i] = 0;
                }
            }
            foreach (Robot robot in Robots)
            {
                if (robot.HitSoundNum < -1 || robot.HitSoundNum >= Sounds.Count)
                    robot.HitSoundNum = -1;
                if (robot.HitVClipNum < -1 || robot.HitVClipNum >= VClips.Count)
                    robot.HitVClipNum = -1;
                if (robot.DeathSoundNum < -1 || robot.DeathSoundNum >= Sounds.Count)
                    robot.DeathSoundNum = -1;
                if (robot.DeathVClipNum < -1 || robot.DeathVClipNum >= VClips.Count)
                    robot.DeathVClipNum = -1;
                if (robot.WeaponType < 0 || robot.WeaponType >= Weapons.Count)
                    robot.WeaponType = 10; //10 is close enough to the "default weapon" tbh
                if (robot.WeaponTypeSecondary < -1 || robot.WeaponTypeSecondary >= Weapons.Count)
                    robot.WeaponTypeSecondary = -1;
                if (robot.ContainsType != 2 && robot.ContainsType != 7)
                {
                    robot.ContainsType = 7; //Makes them drop extra lives
                    robot.ContainsID = 0;
                }
                else if (robot.ContainsType == 2)
                {
                    if (robot.ContainsID < 0 || robot.ContainsID > Robots.Count)
                        robot.ContainsID = 0;
                }
                else
                {
                    if (robot.ContainsID < 0 || robot.ContainsID > Powerups.Count)
                        robot.ContainsID = 0;
                }
                if (robot.SeeSound < 0 || robot.SeeSound >= Sounds.Count)
                    robot.SeeSound = 0;
                if (robot.AttackSound < 0 || robot.AttackSound >= Sounds.Count)
                    robot.AttackSound = 0;
                if (robot.ClawSound < 0 || robot.ClawSound >= Sounds.Count)
                    robot.ClawSound = 0;
                if (robot.TauntSound < 0 || robot.TauntSound >= Sounds.Count)
                    robot.TauntSound = 0;
                if (robot.DeathRollSound < 0 || robot.DeathRollSound >= Sounds.Count)
                    robot.DeathRollSound = 0;
                if (robot.Behavior != 0 && (robot.Behavior < RobotAIType.Still || robot.Behavior > RobotAIType.Follow)) //0 required for demo data
                    robot.Behavior = RobotAIType.Still;
                if (robot.BossFlag < 0 || (robot.BossFlag >= RobotBossType.Descent1Level27 && robot.BossFlag < RobotBossType.RedFatty) || robot.BossFlag >= RobotBossType.VertigoBoss2)
                    robot.BossFlag = 0;
            }
            foreach (Weapon weapon in Weapons)
            {
                if (weapon.ModelNum < 0 || weapon.ModelNum >= Models.Count)
                    weapon.ModelNum = 0;
                if (weapon.ModelNumInner < -1 || weapon.ModelNumInner >= Models.Count)
                    weapon.ModelNumInner = -1;
                if (weapon.MuzzleFlashVClip < -1 || weapon.MuzzleFlashVClip >= VClips.Count)
                    weapon.MuzzleFlashVClip = -1;
                if (weapon.RobotHitVClip < -1 || weapon.RobotHitVClip >= VClips.Count)
                    weapon.RobotHitVClip = -1;
                if (weapon.WallHitVclip < -1 || weapon.WallHitVclip >= VClips.Count)
                    weapon.WallHitVclip = -1;
                if (weapon.WeaponVClip < -1 || weapon.WeaponVClip >= VClips.Count)
                    weapon.WeaponVClip = -1;

                if (weapon.FiringSound < -1 || weapon.FiringSound >= Sounds.Count)
                    weapon.FiringSound = -1;
                if (weapon.RobotHitSound < -1 || weapon.RobotHitSound >= Sounds.Count)
                    weapon.RobotHitSound = -1;
                if (weapon.WallHitSound < -1 || weapon.WallHitSound >= Sounds.Count)
                    weapon.WallHitSound = -1;

                if (weapon.Children < -1 || weapon.Children >= Weapons.Count)
                    weapon.Children = -1;
            }
        }
        /// <summary>
        /// Transfers the gun information from the player ship to a Polymodel instance.
        /// </summary>
        /// <param name="ship">The ship to read the guns from.</param>
        private void BuildModelGunsFromShip(Ship ship)
        {
            Polymodel model = Models[ship.ModelNum];
            model.numGuns = 8;
            for (int i = 0; i < 8; i++)
            {
                model.gunPoints[i] = ship.GunPoints[i];
                model.gunDirs[i] = FixVector.FromRawValues(65536, 0, 0);
                model.gunSubmodels[i] = 0;
            }
        }

        /// <summary>
        /// Transfers the gun information from a reactor to a Polymodel instance.
        /// </summary>
        /// <param name="reactor">The reactor to read the guns from.</param>
        private void BuildModelGunsFromReactor(Reactor reactor)
        {
            if (reactor.ModelNum == -1) return;
            Polymodel model = Models[reactor.ModelNum];
            model.numGuns = reactor.NumGuns;
            for (int i = 0; i < 8; i++)
            {
                model.gunPoints[i] = reactor.GunPoints[i];
                model.gunDirs[i] = reactor.GunDirs[i];
                model.gunSubmodels[i] = 0;
            }
        }

        /// <summary>
        /// Transfers the gun information from a robot to a Polymodel instance.
        /// </summary>
        /// <param name="robot">The robot to read the guns from.</param>
        private void BuildModelAnimation(Robot robot)
        {
            if (robot.ModelNum == -1) return;
            Polymodel model = Models[robot.ModelNum];
            List<FixAngles> jointlist = new List<FixAngles>();
            model.numGuns = robot.NumGuns;
            for (int i = 0; i < Polymodel.MAX_GUNS; i++)
            {
                model.gunPoints[i] = robot.GunPoints[i];
                model.gunDirs[i] = FixVector.FromRawValues(65536, 0, 0);
                model.gunSubmodels[i] = robot.GunSubmodels[i];
            }
            int[,] jointmapping = new int[10, 5];
            for (int m = 0; m < Polymodel.MAX_SUBMODELS; m++)
            {
                for (int f = 0; f < Robot.NUM_ANIMATION_STATES; f++)
                {
                    jointmapping[m, f] = -1;
                }
            }
            int basejoint = 0;
            for (int m = 0; m < Polymodel.MAX_GUNS + 1; m++)
            {
                for (int f = 0; f < Robot.NUM_ANIMATION_STATES; f++)
                {
                    Robot.JointList robotjointlist = robot.AnimStates[m, f];
                    basejoint = robotjointlist.offset;
                    for (int j = 0; j < robotjointlist.n_joints; j++)
                    {
                        JointPos joint = Joints[basejoint];
                        jointmapping[joint.jointnum, f] = basejoint;
                        model.isAnimated = true;
                        basejoint++;
                    }
                }
            }

            for (int m = 1; m < Polymodel.MAX_SUBMODELS; m++)
            {
                for (int f = 0; f < Robot.NUM_ANIMATION_STATES; f++)
                {
                    int jointnum = jointmapping[m, f];
                    if (jointnum != -1)
                    {
                        JointPos joint = Joints[jointnum];
                        model.animationMatrix[m, f].p = joint.angles.p;
                        model.animationMatrix[m, f].b = joint.angles.b;
                        model.animationMatrix[m, f].h = joint.angles.h;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        //Ultimately, it is too much of a pain to stick to the object bitmap and object bitmap pointer tables
        //Instead, don't track them at all in the first place. Build a texture list for each model, and only
        //reconstruct the tables at the time of export
        public void BuildModelTextureTables()
        {
            //Write down unanimated texture names
            Dictionary<int, string> TextureNames = new Dictionary<int, string>();
            //Write down EClip IDs for tracking animated texture names
            Dictionary<int, string> EClipNames = new Dictionary<int, string>();
            EClip clip;
            for (int i = 0; i < EClips.Count; i++)
            {
                clip = EClips[i];
                if (clip.ChangingObjectTexture != -1)
                {
                    EClipNames.Add(clip.ChangingObjectTexture, this.EClipNames[i]);
                    ObjBitmapMapping.Add(this.EClipNames[i], clip.ChangingObjectTexture);
                }
            }
            ushort bitmap; string name;
            for (int i = 0; i < BaseFile.ObjBitmaps.Count; i++)
            {
                bitmap = BaseFile.ObjBitmaps[i];
                //if (bitmap == 0) continue; //UNDONE: it's entirely valid something could have referred to bogus
                PIGImage image = piggyFile.Bitmaps[bitmap];
                name = image.name.ToLower();
                if (!image.isAnimated)
                {
                    TextureNames.Add(i, name);
                    if (!ObjBitmapMapping.ContainsKey(name))
                        ObjBitmapMapping.Add(name, i);
                }
            }
            foreach (Polymodel model in Models)
            {
                model.UseTextureList = true;
                int textureID, pointer;
                for (int i = model.FirstTexture; i < (model.FirstTexture + model.NumTextures); i++)
                {
                    pointer = BaseFile.ObjBitmapPointers[i];
                    textureID = BaseFile.ObjBitmaps[pointer];
                    if (EClipNames.ContainsKey(pointer))
                    {
                        model.TextureList.Add(EClipNames[pointer]);
                    }
                    else if (TextureNames.ContainsKey(pointer))
                    {
                        model.TextureList.Add(TextureNames[pointer]);
                    }
                }
                Console.Write("Model texture list: [");
                foreach (string texture in model.TextureList)
                {
                    Console.Write("{0} ", texture);
                }
                Console.WriteLine("]");
            }
        }

        public void UpdateEClipMapping()
        {
            EClipNameMapping.Clear();
            EClip clip;
            for (int i = 0; i < EClips.Count; i++)
            {
                clip = EClips[i];
                EClipNameMapping.Add(ElementLists.GetEClipName(i).ToLower(), clip);
            }
        }

        public int ReadNamefile(BinaryReader br)
        {
            try
            {
                int ver = br.ReadInt32();
                if (/*sig != 0x4E4D4148 ||*/ver != 1)
                {
                    return -1;
                }
                int VClipsCount = br.ReadInt32();
                int EClipsCount = br.ReadInt32();
                int RobotsCount = br.ReadInt32();
                int WeaponsCount = br.ReadInt32();
                int SoundsCount = br.ReadInt32();
                int PolymodelCount = br.ReadInt32();
                int PowerupsCount = br.ReadInt32();
                if (VClipsCount != VClips.Count || EClipsCount != EClips.Count || RobotsCount != Robots.Count || WeaponsCount != Weapons.Count ||
                    SoundsCount != Sounds.Count || PolymodelCount != Models.Count || PowerupsCount != Powerups.Count)
                {
                    return -1; //okay something went really wrong
                }
                for (int i = 0; i < VClips.Count; i++)
                    VClipNames.Add(br.ReadString());
                for (int i = 0; i < EClips.Count; i++)
                    EClipNames.Add(br.ReadString());
                for (int i = 0; i < Robots.Count; i++)
                    RobotNames.Add(br.ReadString());
                for (int i = 0; i < Weapons.Count; i++)
                    WeaponNames.Add(br.ReadString());
                for (int i = 0; i < Sounds.Count; i++)
                    SoundNames.Add(br.ReadString());
                for (int i = 0; i < Models.Count; i++)
                    ModelNames.Add(br.ReadString());
                for (int i = 0; i < Powerups.Count; i++)
                    PowerupNames.Add(br.ReadString());
            }
            catch (Exception) //godawful error handling
            {
                return -1;
            }

            return 0;
        }

        private int ReadOrphanedModels(BinaryReader br)
        {
            //4F 52 50 4E

            try
            {
                int numOrphaned = 0;
                Polymodel model;
                int version = br.ReadInt32();
                if (version != 1) return -1; //how
                numOrphaned = br.ReadInt32();
                int orphanedID;
                for (int i = 0; i < numOrphaned; i++)
                {
                    orphanedID = br.ReadInt32();
                    model = Models[orphanedID];
                    model.numGuns = br.ReadInt32();
                    for (int j = 0; j < model.numGuns; j++)
                    {
                        model.gunSubmodels[j] = br.ReadInt32();
                        model.gunPoints[j].x = new Fix(br.ReadInt32());
                        model.gunPoints[j].y = new Fix(br.ReadInt32());
                        model.gunPoints[j].z = new Fix(br.ReadInt32());
                        model.gunDirs[j].x = new Fix(br.ReadInt32());
                        model.gunDirs[j].y = new Fix(br.ReadInt32());
                        model.gunDirs[j].z = new Fix(br.ReadInt32());
                    }
                    model.isAnimated = br.ReadBoolean();
                    if (model.isAnimated)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            for (int k = 0; k < 5; k++)
                            {
                                model.animationMatrix[j, k].p = br.ReadInt16();
                                model.animationMatrix[j, k].b = br.ReadInt16();
                                model.animationMatrix[j, k].h = br.ReadInt16();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return 0;
        }

        private void GenerateDefaultNamelists()
        {
            for (int i = 0; i < BaseFile.VClips.Count; i++)
                VClipNames.Add(ElementLists.GetVClipName(i));
            for (int i = 0; i < BaseFile.EClips.Count; i++)
                EClipNames.Add(ElementLists.GetEClipName(i));
            for (int i = 0; i < BaseFile.Robots.Count; i++)
                RobotNames.Add(ElementLists.GetRobotName(i));
            for (int i = 0; i < BaseFile.Weapons.Count; i++)
                WeaponNames.Add(ElementLists.GetWeaponName(i));
            for (int i = 0; i < BaseFile.Sounds.Count; i++)
                SoundNames.Add(ElementLists.GetSoundName(i));
            if (BaseFile.version >= 3)
            {
                for (int i = 0; i < BaseFile.Models.Count; i++)
                    ModelNames.Add(ElementLists.GetModelName(i));
            }
            else
            {
                for (int i = 0; i < BaseFile.Models.Count; i++)
                    ModelNames.Add(ElementLists.GetDemoModelName(i));
            }
            for (int i = 0; i < BaseFile.Powerups.Count; i++)
                PowerupNames.Add(ElementLists.GetPowerupName(i));
        }

        //---------------------------------------------------------------------
        //SAVING
        //---------------------------------------------------------------------

        public void Write(Stream stream)
        {
            //Brute force solution
            //RenumberElements(HAMType.EClip);
            //RenumberElements(HAMType.Weapon);
            //RenumberElements(HAMType.Robot);
            //RenumberElements(HAMType.Model);
            //Science experiment
            GenerateObjectBitmapTables(CompatObjBitmaps);
            NumRobotJoints = 0;
            Console.WriteLine("Loaded {0} joints", Joints.Count);
            Joints.Clear();
            foreach (Robot robot in Robots)
            {
                LoadAnimations(robot, Models[robot.ModelNum]);
            }
            Console.WriteLine("Constructed {0} joints", Joints.Count);
            foreach (Reactor reactor in Reactors)
            {
                LoadReactorGuns(reactor);
            }
            LoadShipGuns(PlayerShip);

            CreateDataLists();
            BaseFile.Write(stream);

            if (ExportExtraData)
            {
                BinaryWriter bw = new BinaryWriter(stream);
                SaveNamefile(bw);
                WriteOrphanedModels(bw);
                bw.Dispose();
            }
        }

        private void CreateDataLists()
        {
            //TODO: This is just passthrough for now, need "editor" classes
            BaseFile.Textures.Clear();
            foreach (ushort texture in Textures)
                BaseFile.Textures.Add(texture);
            BaseFile.TMapInfo.Clear();
            foreach (TMAPInfo tmapInfo in TMapInfo)
                BaseFile.TMapInfo.Add(tmapInfo);
            BaseFile.Sounds.Clear();
            foreach (byte sound in Sounds)
                BaseFile.Sounds.Add(sound);
            BaseFile.AltSounds.Clear();
            foreach (byte sound in AltSounds)
                BaseFile.AltSounds.Add(sound);
            BaseFile.VClips.Clear();
            foreach (VClip clip in VClips)
                BaseFile.VClips.Add(clip);
            BaseFile.EClips.Clear();
            foreach (EClip clip in EClips)
                BaseFile.EClips.Add(clip);
            BaseFile.WClips.Clear();
            foreach (WClip clip in WClips)
                BaseFile.WClips.Add(clip);
            BaseFile.Robots.Clear();
            foreach (Robot robot in Robots)
                BaseFile.Robots.Add(robot);
            BaseFile.Joints.Clear();
            foreach (JointPos joint in Joints)
                BaseFile.Joints.Add(joint);
            BaseFile.Weapons.Clear();
            foreach (Weapon weapon in Weapons)
                BaseFile.Weapons.Add(weapon);
            BaseFile.Models.Clear();
            foreach (Polymodel model in Models)
                BaseFile.Models.Add(model);
            BaseFile.Gauges.Clear();
            foreach (ushort gauge in Gauges)
                BaseFile.Gauges.Add(gauge);
            BaseFile.GaugesHires.Clear();
            foreach (ushort gauge in GaugesHires)
                BaseFile.GaugesHires.Add(gauge);
            BaseFile.PlayerShip = PlayerShip;
            BaseFile.Cockpits.Clear();
            foreach (ushort cockpit in Cockpits)
                BaseFile.Cockpits.Add(cockpit);
            BaseFile.Reactors.Clear();
            foreach (Reactor reactor in Reactors)
                BaseFile.Reactors.Add(reactor);
            BaseFile.Powerups.Clear();
            foreach (Powerup powerup in Powerups)
                BaseFile.Powerups.Add(powerup);
            BaseFile.FirstMultiBitmapNum = FirstMultiBitmapNum;
            for (int i = 0; i < 2620; i++)
                BaseFile.BitmapXLATData[i] = BitmapXLATData[i];
            BaseFile.ObjBitmaps.Clear();
            foreach (ushort bm in ObjBitmaps)
                BaseFile.ObjBitmaps.Add(bm);
            BaseFile.ObjBitmapPointers.Clear();
            foreach (ushort bm in ObjBitmapPointers)
                BaseFile.ObjBitmapPointers.Add(bm);
        }

        private void LoadReactorGuns(Reactor reactor)
        {
            Polymodel model = Models[reactor.ModelNum];
            reactor.NumGuns = (byte)model.numGuns;
            for (int i = 0; i < reactor.NumGuns; i++)
            {
                reactor.GunPoints[i] = model.gunPoints[i];
                reactor.GunDirs[i] = model.gunDirs[i];
            }
        }

        private void LoadShipGuns(Ship ship)
        {
            Polymodel models = Models[ship.ModelNum];
            for (int i = 0; i < 8; i++)
            {
                ship.GunPoints[i] = models.gunPoints[i];
            }
        }

        //I actually hate this game's animation system sometimes
        private void LoadAnimations(Robot robot, Polymodel model)
        {
            robot.NumGuns = (sbyte)model.numGuns;
            for (int i = 0; i < 8; i++)
            {
                robot.GunPoints[i] = model.gunPoints[i];
                robot.GunSubmodels[i] = (byte)model.gunSubmodels[i];
            }
            for (int m = 0; m < 9; m++)
            {
                for (int f = 0; f < 5; f++)
                {
                    robot.AnimStates[m, f].n_joints = 0;
                    robot.AnimStates[m, f].offset = 0;
                }
            }
            if (!model.isAnimated) return;
            int[] gunNums = new int[10];

            for (int i = 1; i < model.NumSubmodels; i++)
            {
                gunNums[i] = robot.NumGuns;
            }
            gunNums[0] = -1;

            for (int g = 0; g < robot.NumGuns; g++)
            {
                int m = robot.GunSubmodels[g];

                while (m != 0)
                {
                    gunNums[m] = g;
                    m = model.Submodels[m].Parent;
                }
            }

            for (int g = 0; g < robot.NumGuns + 1; g++)
            {
                for (int state = 0; state < 5; state++)
                {
                    robot.AnimStates[g, state].n_joints = 0;
                    robot.AnimStates[g, state].offset = (short)NumRobotJoints;

                    for (int m = 0; m < model.NumSubmodels; m++)
                    {
                        if (gunNums[m] == g)
                        {
                            JointPos joint = new JointPos();
                            joint.jointnum = (short)m;
                            joint.angles = model.animationMatrix[m, state];
                            Joints.Add(joint);
                            robot.AnimStates[g, state].n_joints++;
                            NumRobotJoints++;
                        }
                    }
                }
            }
        }

        private void GenerateObjectBitmapTables(bool compatObjBitmaps)
        {
            ObjBitmaps.Clear();
            ObjBitmapPointers.Clear();
            int lastObjectBitmap = 0;
            int lastObjectBitmapPointer = 0;
            Dictionary<string, int> objectBitmapMapping = new Dictionary<string, int>();

            Polymodel model;
            if (compatObjBitmaps)
            {
                int lastShipmodel = PlayerShip.ModelNum;
                if (Models[PlayerShip.ModelNum].DyingModelnum != -1)
                    lastShipmodel = Models[PlayerShip.ModelNum].DyingModelnum;
                for (int i = 0; i < Models.Count; i++)
                {
                    model = Models[i];
                    model.FirstTexture = (ushort)lastObjectBitmapPointer;
                    model.NumTextures = (byte)model.TextureList.Count;
                    if (i == lastShipmodel)
                    {
                        //Inject multiplayer bitmaps
                        FirstMultiBitmapNum = lastObjectBitmapPointer;
                        for (int j = 0; j < 14; j++)
                        {
                            ObjBitmaps.Add((ushort)(multiplayerBitmaps[j])); ObjBitmapPointers.Add((ushort)(ObjBitmaps.Count - 1));
                            lastObjectBitmap++; lastObjectBitmapPointer++;
                        }
                        //Don't load textures for the dying ship. Because reasons. 
                        model.FirstTexture = Models[PlayerShip.ModelNum].FirstTexture;
                        model.NumTextures = Models[PlayerShip.ModelNum].NumTextures;
                    }
                    else
                    {
                        foreach (string textureName in model.TextureList)
                        {
                            if (EClipNameMapping.ContainsKey(textureName.ToLower()) && !objectBitmapMapping.ContainsKey(textureName.ToLower()))
                            {
                                objectBitmapMapping.Add(textureName.ToLower(), ObjBitmaps.Count);
                                ObjBitmaps.Add(0); //temp, will be remapped later
                                lastObjectBitmap++;
                            }
                            //In the compatible mode, all textures are redundant except vclips. 
                            if (objectBitmapMapping.ContainsKey(textureName.ToLower()))
                            {
                                ObjBitmapPointers.Add((ushort)objectBitmapMapping[textureName.ToLower()]);
                                lastObjectBitmapPointer++;
                            }
                            else
                            {
                                ObjBitmapPointers.Add((ushort)lastObjectBitmap);
                                lastObjectBitmapPointer++;
                                ObjBitmaps.Add((ushort)(piggyFile.GetBitmapIDFromName(textureName)));
                                lastObjectBitmap++;
                            }
                        }
                        //I hate hacks, but parallax couldn't keep tabs on their bitmaps.tbl file so...
                        //Descent's smart missile children are defined with model textures despite not being models so for compatibility add them in
                        if (i == Weapons[18].ModelNum || i == Weapons[28].ModelNum) //player and robot mega missiles
                        {
                            ObjBitmaps.Add((ushort)(piggyFile.GetBitmapIDFromName("glow04"))); ObjBitmapPointers.Add((ushort)(ObjBitmaps.Count - 1));
                            ObjBitmaps.Add((ushort)(piggyFile.GetBitmapIDFromName("rbot046"))); ObjBitmapPointers.Add((ushort)(ObjBitmaps.Count - 1));
                            lastObjectBitmapPointer += 2; lastObjectBitmap += 2;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Models.Count; i++)
                {
                    model = Models[i];
                    model.FirstTexture = (ushort)lastObjectBitmapPointer;
                    model.NumTextures = (byte)model.TextureList.Count;
                    foreach (string textureName in model.TextureList)
                    {
                        if (!objectBitmapMapping.ContainsKey(textureName.ToLower()))
                        {
                            objectBitmapMapping.Add(textureName.ToLower(), lastObjectBitmap);
                            ObjBitmaps.Add((ushort)(piggyFile.GetBitmapIDFromName(textureName)));
                            lastObjectBitmap++;
                        }
                        ObjBitmapPointers.Add((ushort)objectBitmapMapping[textureName.ToLower()]);
                        lastObjectBitmapPointer++;
                    }
                }
                //Inject multiplayer bitmaps
                FirstMultiBitmapNum = lastObjectBitmapPointer;
                for (int i = 0; i < 14; i++)
                {
                    ObjBitmaps.Add((ushort)(multiplayerBitmaps[i])); ObjBitmapPointers.Add((ushort)(ObjBitmaps.Count - 1));
                }
            }

            //Update EClips
            EClip clip;
            for (int i = 0; i < EClips.Count; i++)
            {
                clip = EClips[i];
                if (objectBitmapMapping.ContainsKey(EClipNames[i].ToLower()))
                {
                    clip.ChangingObjectTexture = (short)objectBitmapMapping[EClipNames[i].ToLower()];
                    ObjBitmaps[clip.ChangingObjectTexture] = (ushort)(clip.vc.Frames[0]);
                }
            }
        }

        public int SaveNamefile(BinaryWriter bw)
        {
            //48 41 4D 4E
            bw.Write(0x4E4D4148); //HAMN
            bw.Write(1); //version. in case i fuck something up
            //Write out the counts for safety. 
            bw.Write(VClips.Count);
            bw.Write(EClips.Count);
            bw.Write(Robots.Count);
            bw.Write(Weapons.Count);
            bw.Write(Sounds.Count);
            bw.Write(Models.Count);
            bw.Write(Powerups.Count);
            foreach (string name in VClipNames)
                bw.Write(name);
            foreach (string name in EClipNames)
                bw.Write(name);
            foreach (string name in RobotNames)
                bw.Write(name);
            foreach (string name in WeaponNames)
                bw.Write(name);
            foreach (string name in SoundNames)
                bw.Write(name);
            foreach (string name in ModelNames)
                bw.Write(name);
            foreach (string name in PowerupNames)
                bw.Write(name);

            return 0;
        }

        private int WriteOrphanedModels(BinaryWriter bw)
        {
            //4F 52 50 4E

            Polymodel model;
            bw.Write(0x4E50524F); //ORPH
            bw.Write(1);
            bw.Write(Models.Count);
            for (int i = 0; i < Models.Count; i++)
            {
                model = Models[i];
                bw.Write(i);
                bw.Write(model.numGuns);
                for (int j = 0; j < model.numGuns; j++)
                {
                    bw.Write(model.gunSubmodels[j]);
                    bw.Write(model.gunPoints[j].x.Value);
                    bw.Write(model.gunPoints[j].y.Value);
                    bw.Write(model.gunPoints[j].z.Value);
                    bw.Write(model.gunDirs[j].x.Value);
                    bw.Write(model.gunDirs[j].y.Value);
                    bw.Write(model.gunDirs[j].z.Value);
                }
                bw.Write(model.isAnimated);
                if (model.isAnimated)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            bw.Write(model.animationMatrix[j, k].p);
                            bw.Write(model.animationMatrix[j, k].b);
                            bw.Write(model.animationMatrix[j, k].h);
                        }
                    }
                }
            }

            return 0;
        }

        //---------------------------------------------------------------------
        // ACCESSORS
        //---------------------------------------------------------------------
        public TMAPInfo GetTMAPInfo(int id)
        {
            if (id < 0 || id >= TMapInfo.Count) return null;
            return TMapInfo[id];
        }

        public VClip GetVClip(int id)
        {
            if (id < 0 || id >= VClips.Count) return null;
            return VClips[id];
        }

        public EClip GetEClip(int id)
        {
            if (id < 0 || id >= EClips.Count) return null;
            return EClips[id];
        }

        public WClip GetWClip(int id)
        {
            if (id < 0 || id >= WClips.Count) return null;
            return WClips[id];
        }

        public Robot GetRobot(int id)
        {
            if (id < 0 || id >= Robots.Count) return null;
            return Robots[id];
        }

        public Weapon GetWeapon(int id)
        {
            if (id < 0 || id >= Weapons.Count) return null;
            return Weapons[id];
        }

        public Polymodel GetModel(int id)
        {
            if (id < 0 || id >= Models.Count) return null;
            return Models[id];
        }

        public Powerup GetPowerup(int id)
        {
            if (id < 0 || id >= Powerups.Count) return null;
            return Powerups[id];
        }

        public Reactor GetReactor(int id)
        {
            if (id < 0 || id >= Reactors.Count) return null;
            return Reactors[id];
        }
    }
}
