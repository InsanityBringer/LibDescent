using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LibDescent.Data;

namespace LibDescent.Edit
{
    public class EditorHXMFile : IDataFile
    {
        public HXMFile BaseFile { get; private set; }
        public EditorHAMFile BaseHAM { get; private set; }

        public EditorVHAMFile augmentFile;

        public List<Robot> replacedRobots { get; private set; }
        public List<JointPos> replacedJoints { get; private set; }
        public List<Polymodel> replacedModels { get; private set; }
        public List<ReplacedBitmapElement> replacedObjBitmaps { get; private set; }
        public List<ReplacedBitmapElement> replacedObjBitmapPtrs { get; private set; }

        public List<string> RobotNames { get; private set; }
        public List<string> ModelNames { get; private set; }

        public EditorHXMFile(HXMFile baseFile, EditorHAMFile baseHAM)
        {
            this.BaseFile = baseFile;
            this.BaseHAM = baseHAM;
            replacedRobots = new List<Robot>();
            replacedJoints = new List<JointPos>();
            replacedModels = new List<Polymodel>();
            replacedObjBitmaps = new List<ReplacedBitmapElement>();
            replacedObjBitmapPtrs = new List<ReplacedBitmapElement>();

            RobotNames = new List<string>();
            ModelNames = new List<string>();
        }

        public EditorHXMFile(EditorHAMFile baseHAM) : this(new HXMFile(), baseHAM)
        {
        }

        //---------------------------------------------------------------------
        // LOADING
        //---------------------------------------------------------------------

        public void Read(Stream stream)
        {
            BaseFile.Read(stream);

            CreateLocalLists();
            GenerateNameTable();
            BuildModelTextureTables(); //fuck hxm files
            foreach (Robot robot in replacedRobots)
            {
                BuildModelAnimation(robot);
            }
        }

        private void CreateLocalLists()
        {
            foreach (Robot robot in BaseFile.replacedRobots)
                replacedRobots.Add(robot);
            foreach (JointPos joint in BaseFile.replacedJoints)
                replacedJoints.Add(joint);
            foreach (Polymodel model in BaseFile.replacedModels)
                replacedModels.Add(model);
            foreach (ReplacedBitmapElement bm in BaseFile.replacedObjBitmaps)
                replacedObjBitmaps.Add(bm);
            foreach (ReplacedBitmapElement bm in BaseFile.replacedObjBitmapPtrs)
                replacedObjBitmapPtrs.Add(bm);
        }

        //I dunno why i'm being as masochistic as I am with this but okay. 
        /// <summary>
        /// Creates the texture tables for all polygon models in this HXM file.
        /// </summary>
        private void BuildModelTextureTables()
        {
            //Write down unanimated texture names
            Dictionary<int, string> TextureNames = new Dictionary<int, string>();
            //Write down EClip IDs for tracking animated texture names
            Dictionary<int, string> EClipNames = new Dictionary<int, string>();
            EClip clip;
            for (int i = 0; i < BaseHAM.EClips.Count; i++)
            {
                clip = BaseHAM.EClips[i];
                if (clip.ChangingObjectTexture != -1)
                {
                    EClipNames.Add(clip.ChangingObjectTexture, BaseHAM.EClipNames[i]);
                }
            }
            ushort bitmap; string name;
            for (int i = 0; i < BaseHAM.BaseFile.ObjBitmaps.Count; i++)
            {
                bitmap = GetObjBitmap(i);
                //if (bitmap == 0) continue;
                PIGImage image = BaseHAM.piggyFile.Bitmaps[bitmap];
                name = image.Name.ToLower();
                if (!image.IsAnimated)
                {
                    TextureNames.Add(i, name);
                }
            }
            foreach (Polymodel model in replacedModels)
            {
                model.UseTextureList = true;
                int textureID, pointer;
                for (int i = model.FirstTexture; i < (model.FirstTexture + model.NumTextures); i++)
                {
                    pointer = GetObjBitmapPointer(i);
                    textureID = GetObjBitmap(pointer);
                    if (EClipNames.ContainsKey(pointer))
                    {
                        model.TextureList.Add(EClipNames[pointer]);
                    }
                    else if (TextureNames.ContainsKey(pointer))
                    {
                        model.TextureList.Add(TextureNames[pointer]);
                    }
                    else
                    {
                        model.TextureList.Add("bogus");
                    }
                }
                Console.Write("Model texture list: [");
                foreach (string texture in model.TextureList)
                {
                    Console.Write("{0} ", texture);
                }
                Console.WriteLine("]");
                model.BaseTexture = FindFirstObjBitmap(model);
            }
        }

        /// <summary>
        /// Creates the animation matricies for all robot's animations.
        /// </summary>
        /// <param name="robot">The robot to read the joints from.</param>
        private void BuildModelAnimation(Robot robot)
        {
            int lowestJoint = int.MaxValue;
            if (robot.ModelNum == -1) return;
            Polymodel model = GetModel(robot.ModelNum);
            if (model.replacementID == -1) return;
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
            for (int m = 0; m < robot.NumGuns + 1; m++)
            {
                for (int f = 0; f < Robot.NUM_ANIMATION_STATES; f++)
                {
                    Robot.JointList robotjointlist = robot.AnimStates[m, f];
                    basejoint = robotjointlist.offset;
                    if (basejoint < lowestJoint)
                        lowestJoint = basejoint;
                    for (int j = 0; j < robotjointlist.n_joints; j++)
                    {
                        JointPos joint = GetJoint(basejoint);
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
                        JointPos joint = GetJoint(jointnum);
                        model.animationMatrix[m, f].p = joint.angles.p;
                        model.animationMatrix[m, f].b = joint.angles.b;
                        model.animationMatrix[m, f].h = joint.angles.h;
                    }
                }
            }

            if (lowestJoint != int.MaxValue)
                robot.baseJoint = lowestJoint;
        }

        /// <summary>
        /// Generates default new robot names.
        /// </summary>
        public void GenerateNameTable()
        {
            for (int i = 0; i < replacedRobots.Count; i++)
            {
                RobotNames.Add(string.Format("Replaced Robot {0}", i));
            }
            for (int i = 0; i < replacedModels.Count; i++)
            {
                ModelNames.Add(string.Format("Replaced Model {0}", i));
            }
        }

        //---------------------------------------------------------------------
        // UTILITY FUNCTIONS
        //---------------------------------------------------------------------

        public int AddRobot()
        {
            replacedRobots.Add(new Robot());
            RobotNames.Add("New Robot");
            return replacedRobots.Count - 1;
        }

        public int AddModel()
        {
            Polymodel newModel = new Polymodel();
            replacedModels.Add(newModel);
            ModelNames.Add("New Model");
            return replacedModels.Count - 1;
        }

        /// <summary>
        /// Counts the amount of textures present in a model that aren't present in the parent file.
        /// </summary>
        /// <param name="model">The model to count the textures of.</param>
        /// <returns>The number of unique textures not found in the parent file.</returns>
        public int CountUniqueObjBitmaps(Polymodel model)
        {
            int num = 0;
            foreach (string tex in model.TextureList)
            {
                if (!BaseHAM.ObjBitmapMapping.ContainsKey(tex))
                {
                    num++;
                }
            }
            return num;
        }

        /// <summary>
        /// Looks through a model's textures, and finds the first ObjBitmap not present in the parent file.
        /// </summary>
        /// <param name="model">The model to check the textures of.</param>
        /// <returns>The index of the first new texture, or 0 if there are no new textures.</returns>
        public int FindFirstObjBitmap(Polymodel model)
        {
            int num = int.MaxValue;
            string tex;
            for (int i = 0; i < model.NumTextures; i++)
            {
                tex = model.TextureList[i];
                if (!BaseHAM.ObjBitmapMapping.ContainsKey(tex))
                {
                    //This texture isn't present in the base file, so it's new. Figure out where it is
                    num = GetObjBitmapPointer(model.FirstTexture + i);
                }
            }
            if (num == int.MaxValue) return 0;
            return num;
        }

        //---------------------------------------------------------------------
        // SAVING
        //---------------------------------------------------------------------

        /// <summary>
        /// Saves the HXM file to a given stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public void Write(Stream stream)
        {
            replacedJoints.Clear();
            foreach (Robot robot in replacedRobots)
            {
                LoadAnimations(robot, GetModel(robot.ModelNum));
            }
            LoadModelTextures();
            CreateDataLists();
            BaseFile.Write(stream);
        }

        private void CreateDataLists()
        {
            BaseFile.replacedRobots.Clear();
            BaseFile.replacedJoints.Clear();
            BaseFile.replacedModels.Clear();
            BaseFile.replacedObjBitmaps.Clear();
            BaseFile.replacedObjBitmapPtrs.Clear();
            foreach (Robot robot in replacedRobots)
                BaseFile.replacedRobots.Add(robot);
            foreach (JointPos joint in replacedJoints)
                BaseFile.replacedJoints.Add(joint);
            foreach (Polymodel model in replacedModels)
                BaseFile.replacedModels.Add(model);
            foreach (ReplacedBitmapElement bm in replacedObjBitmaps)
                BaseFile.replacedObjBitmaps.Add(bm);
            foreach (ReplacedBitmapElement bm in replacedObjBitmapPtrs)
                BaseFile.replacedObjBitmapPtrs.Add(bm);
        }

        /// <summary>
        /// Generates all model's needed ObjBitmaps and ObjBitmapPointers
        /// </summary>
        private void LoadModelTextures()
        {
            Dictionary<string, int> textureMapping = new Dictionary<string, int>();
            PIGImage img;
            EClip clip;
            ReplacedBitmapElement bm;
            //Add base file ObjBitmaps to this mess
            for (int i = 0; i < BaseHAM.BaseFile.ObjBitmaps.Count; i++)
            {
                img = BaseHAM.piggyFile.GetImage(BaseHAM.BaseFile.ObjBitmaps[i]);
                if (!img.IsAnimated && !textureMapping.ContainsKey(img.Name))
                    textureMapping.Add(img.Name, i);
            }
            //Add EClip names
            for (int i = 0; i < BaseHAM.EClips.Count; i++)
            {
                clip = BaseHAM.EClips[i];
                if (clip.ChangingObjectTexture != -1)
                    textureMapping.Add(BaseHAM.EClipNames[i], clip.ChangingObjectTexture);
            }
            //If augment file, add augment obj bitmaps
            if (augmentFile != null)
            {
                for (int i = 0; i < augmentFile.ObjBitmaps.Count; i++)
                {
                    img = BaseHAM.piggyFile.GetImage(augmentFile.ObjBitmaps[i]);
                    if (!textureMapping.ContainsKey(img.Name))
                        textureMapping.Add(img.Name, i + VHAMFile.N_D2_OBJBITMAPS);
                }
            }

            //Nuke the old replaced ObjBitmaps and ObjBitmapPointers because they aren't needed anymore
            replacedObjBitmaps.Clear();
            replacedObjBitmapPtrs.Clear();

            //Generate the new elements
            Polymodel model;
            int replacedNum;
            List<int> newTextures = new List<int>();
            string texName;
            for (int i = 0; i < replacedModels.Count; i++)
            {
                model = replacedModels[i];
                replacedNum = model.BaseTexture;

                //Find the unique textures in this model
                for (int j = 0; j < model.TextureList.Count; j++)
                {
                    texName = model.TextureList[j];
                    if (!textureMapping.ContainsKey(texName))
                        newTextures.Add(BaseHAM.piggyFile.GetBitmapIDFromName(texName));
                }
                //Generate the new ObjBitmaps
                foreach (int newID in newTextures)
                {
                    ReplacedBitmapElement elem;
                    elem.data = (ushort)newID;
                    elem.replacementID = replacedNum;
                    replacedObjBitmaps.Add(elem);
                    replacedNum++;
                }

                newTextures.Clear();
            }

            //Finally augment things with our own images
            for (int i = 0; i < replacedObjBitmaps.Count; i++)
            {
                bm = replacedObjBitmaps[i];
                img = BaseHAM.piggyFile.GetImage(bm.data);
                if (!textureMapping.ContainsKey(img.Name))
                    textureMapping.Add(img.Name, bm.replacementID);
            }

            //Final stage: generate new ObjBitmapPointers
            for (int i = 0; i < replacedModels.Count; i++)
            {
                model = replacedModels[i];
                replacedNum = model.FirstTexture;

                foreach (string texture in model.TextureList)
                {
                    ReplacedBitmapElement elem;
                    if (textureMapping.ContainsKey(texture))
                        elem.data = (ushort)textureMapping[texture];
                    else
                        elem.data = 0;
                    elem.replacementID = replacedNum;
                    replacedObjBitmapPtrs.Add(elem);
                    replacedNum++;
                }
            }
        }

        /// <summary>
        /// Generates a robot's anim_states and creates the joint elements for the robot
        /// </summary>
        /// <param name="robot">The robot to generate joints for.</param>
        /// <param name="model">The model to use to generate joints.</param>
        private void LoadAnimations(Robot robot, Polymodel model)
        {
            int NumRobotJoints = robot.baseJoint;
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
                            joint.replacementID = NumRobotJoints;
                            replacedJoints.Add(joint);
                            robot.AnimStates[g, state].n_joints++;
                            NumRobotJoints++;
                        }
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        // DATA GETTERS
        //---------------------------------------------------------------------

        /// <summary>
        /// Reads a TMAPInfo from the original data file.
        /// </summary>
        /// <param name="id">ID of the TMAPInfo.</param>
        /// <returns>The TMAPInfo.</returns>
        public TMAPInfo GetTMAPInfo(int id)
        {
            return BaseHAM.GetTMAPInfo(id);
        }

        /// <summary>
        /// Reads a VClip from the original data file.
        /// </summary>
        /// <param name="id">ID of the VClip.</param>
        /// <returns>The VClip.</returns>
        public VClip GetVClip(int id)
        {
            return BaseHAM.GetVClip(id);
        }

        /// <summary>
        /// Reads a EClip from the original data file.
        /// </summary>
        /// <param name="id">ID of the EClip</param>
        /// <returns>The EClip.</returns>
        public EClip GetEClip(int id)
        {
            return BaseHAM.GetEClip(id);
        }

        /// <summary>
        /// Reads a WClip from the original data file.
        /// </summary>
        /// <param name="id">ID of the WClip</param>
        /// <returns>The WClip.</returns>
        public WClip GetWClip(int id)
        {
            return BaseHAM.GetWClip(id);
        }

        /// <summary>
        /// Counts the number of robots present in the parent HAM file and the augment V-HAM file.
        /// </summary>
        /// <returns>Count of all available robots.</returns>
        public int GetNumRobots()
        {
            int numRobots = BaseHAM.Robots.Count;
            if (augmentFile != null)
                numRobots += augmentFile.Robots.Count;
            return numRobots;
        }

        /// <summary>
        /// Gets a robot name, passing through to the HAM or VHAM files if not replaced.
        /// </summary>
        /// <param name="id">ID of the robot to get the name of.</param>
        /// <param name="baseOnly">Set to true to only get original names, no replaced names.</param>
        /// <returns>The robot name.</returns>
        public string GetRobotName(int id, bool baseOnly = false)
        {
            //This is a horrible hack
            if (!baseOnly)
            {
                for (int i = 0; i < replacedRobots.Count; i++)
                {
                    if (replacedRobots[i].replacementID == id) return RobotNames[i];
                }
            }
            if (augmentFile != null && id >= VHAMFile.N_D2_ROBOT_TYPES)
            {
                if (id - VHAMFile.N_D2_ROBOT_TYPES >= augmentFile.RobotNames.Count)
                    return string.Format("Unallocated #{0}", id);
                return augmentFile.RobotNames[id - VHAMFile.N_D2_ROBOT_TYPES];
            }
            if (id >= BaseHAM.RobotNames.Count)
                return string.Format("Unallocated #{0}", id);
            return BaseHAM.RobotNames[id];
        }

        /// <summary>
        /// Gets a robot definition, passing through to the HAM or VHAM files if not replaced.
        /// </summary>
        /// <param name="id">ID of the robot.</param>
        /// <returns>The robot.</returns>
        public Robot GetRobot(int id)
        {
            foreach (Robot robot in replacedRobots)
            {
                if (robot.replacementID == id) return robot;
            }
            if (augmentFile != null)
                return augmentFile.GetRobot(id); //passes through
            return BaseHAM.GetRobot(id);
        }

        public int GetNumWeapons()
        {
            int numWeapons = BaseHAM.Weapons.Count;
            if (augmentFile != null)
                numWeapons += augmentFile.Weapons.Count;
            return numWeapons;
        }

        public string GetWeaponName(int id)
        {
            if (augmentFile != null && id >= VHAMFile.N_D2_WEAPON_TYPES)
                return augmentFile.WeaponNames[id - VHAMFile.N_D2_WEAPON_TYPES];
            return BaseHAM.WeaponNames[id];
        }

        public Weapon GetWeapon(int id)
        {
            if (augmentFile != null)
                return augmentFile.GetWeapon(id); //passes through
            return BaseHAM.GetWeapon(id);
        }

        public int GetNumModels()
        {
            int numWeapons = BaseHAM.Models.Count;
            if (augmentFile != null)
                numWeapons += augmentFile.Models.Count;
            return numWeapons;
        }

        public string GetModelName(int id, bool baseOnly = false)
        {
            //This is a horrible hack
            if (!baseOnly)
            {
                for (int i = 0; i < replacedModels.Count; i++)
                {
                    if (replacedModels[i].replacementID == id) return ModelNames[i];
                }
            }
            if (augmentFile != null && id >= VHAMFile.N_D2_POLYGON_MODELS)
            {
                if (id - VHAMFile.N_D2_POLYGON_MODELS >= augmentFile.ModelNames.Count)
                    return string.Format("Unallocated #{0}", id);
                return augmentFile.ModelNames[id - VHAMFile.N_D2_POLYGON_MODELS];
            }
            if (id >= BaseHAM.ModelNames.Count)
                return string.Format("Unallocated #{0}", id);
            return BaseHAM.ModelNames[id];
        }

        public Polymodel GetModel(int id)
        {
            foreach (Polymodel model in replacedModels)
            {
                if (model.replacementID == id) return model;
            }
            if (augmentFile != null)
                return augmentFile.GetModel(id);
            return BaseHAM.GetModel(id);
        }

        public Powerup GetPowerup(int id)
        {
            return BaseHAM.GetPowerup(id);
        }

        public Reactor GetReactor(int id)
        {
            return BaseHAM.GetReactor(id);
        }

        public JointPos GetJoint(int id)
        {
            foreach (JointPos joint in replacedJoints)
            {
                if (joint.replacementID == id) return joint;
            }
            if (augmentFile != null)
                return augmentFile.GetJoint(id);
            return BaseHAM.Joints[id];
        }

        public ushort GetObjBitmap(int id)
        {
            foreach (ReplacedBitmapElement bitmap in replacedObjBitmaps)
                if (bitmap.replacementID == id) return bitmap.data;
            if (augmentFile != null)
                return augmentFile.GetObjBitmap(id);
            return BaseHAM.BaseFile.ObjBitmaps[id];
        }

        public ushort GetObjBitmapPointer(int id)
        {
            foreach (ReplacedBitmapElement bitmap in replacedObjBitmapPtrs)
                if (bitmap.replacementID == id) return bitmap.data;
            if (augmentFile != null)
                return augmentFile.GetObjBitmapPointer(id);
            return BaseHAM.BaseFile.ObjBitmapPointers[id];
        }
    }
}
