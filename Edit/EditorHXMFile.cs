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

        public EditorVHAMFile AugmentFile;

        public List<Robot> ReplacedRobots { get; private set; }
        public List<JointPos> ReplacedJoints { get; private set; }
        public List<Polymodel> ReplacedModels { get; private set; }
        public List<ReplacedBitmapElement> ReplacedObjBitmaps { get; private set; }
        public List<ReplacedBitmapElement> ReplacedObjBitmapPtrs { get; private set; }

        public EditorHXMFile(HXMFile baseFile, EditorHAMFile baseHAM)
        {
            this.BaseFile = baseFile;
            this.BaseHAM = baseHAM;
            ReplacedRobots = new List<Robot>();
            ReplacedJoints = new List<JointPos>();
            ReplacedModels = new List<Polymodel>();
            ReplacedObjBitmaps = new List<ReplacedBitmapElement>();
            ReplacedObjBitmapPtrs = new List<ReplacedBitmapElement>();
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
            foreach (Robot robot in ReplacedRobots)
            {
                BuildModelAnimation(robot);
            }
        }

        private void CreateLocalLists()
        {
            foreach (Robot robot in BaseFile.ReplacedRobots)
                ReplacedRobots.Add(robot);
            foreach (JointPos joint in BaseFile.ReplacedJoints)
                ReplacedJoints.Add(joint);
            foreach (Polymodel model in BaseFile.ReplacedModels)
                ReplacedModels.Add(model);
            foreach (ReplacedBitmapElement bm in BaseFile.ReplacedObjBitmaps)
                ReplacedObjBitmaps.Add(bm);
            foreach (ReplacedBitmapElement bm in BaseFile.ReplacedObjBitmapPtrs)
                ReplacedObjBitmapPtrs.Add(bm);
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
                    EClipNames.Add(clip.ChangingObjectTexture, clip.Name.ToLower());
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
            foreach (Polymodel model in ReplacedModels)
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
            if (model.ReplacementID == -1) return;
            List<FixAngles> jointlist = new List<FixAngles>();
            model.NumGuns = robot.NumGuns;
            for (int i = 0; i < Polymodel.MaxGuns; i++)
            {
                model.GunPoints[i] = robot.GunPoints[i];
                model.GunDirs[i] = FixVector.FromRawValues(65536, 0, 0);
                model.GunSubmodels[i] = robot.GunSubmodels[i];
            }
            int[,] jointmapping = new int[10, 5];
            for (int m = 0; m < Polymodel.MaxSubmodels; m++)
            {
                for (int f = 0; f < Robot.NumAnimationStates; f++)
                {
                    jointmapping[m, f] = -1;
                }
            }
            int basejoint = 0;
            for (int m = 0; m < robot.NumGuns + 1; m++)
            {
                for (int f = 0; f < Robot.NumAnimationStates; f++)
                {
                    Robot.JointList robotjointlist = robot.AnimStates[m, f];
                    basejoint = robotjointlist.Offset;
                    if (basejoint < lowestJoint)
                        lowestJoint = basejoint;
                    for (int j = 0; j < robotjointlist.NumJoints; j++)
                    {
                        JointPos joint = GetJoint(basejoint);
                        jointmapping[joint.JointNum, f] = basejoint;
                        model.IsAnimated = true;
                        basejoint++;
                    }
                }
            }

            for (int m = 1; m < Polymodel.MaxSubmodels; m++)
            {
                for (int f = 0; f < Robot.NumAnimationStates; f++)
                {
                    int jointnum = jointmapping[m, f];
                    if (jointnum != -1)
                    {
                        JointPos joint = GetJoint(jointnum);
                        model.AnimationMatrix[m, f].P = joint.Angles.P;
                        model.AnimationMatrix[m, f].B = joint.Angles.B;
                        model.AnimationMatrix[m, f].H = joint.Angles.H;
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
            for (int i = 0; i < ReplacedRobots.Count; i++)
            {
                ReplacedRobots[i].Name = string.Format("Replaced Robot {0}", i);
            }
            for (int i = 0; i < ReplacedModels.Count; i++)
            {
                ReplacedModels[i].Name = string.Format("Replaced Model {0}", i);
            }
        }

        //---------------------------------------------------------------------
        // UTILITY FUNCTIONS
        //---------------------------------------------------------------------

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
            ReplacedJoints.Clear();
            foreach (Robot robot in ReplacedRobots)
            {
                LoadAnimations(robot, GetModel(robot.ModelNum));
            }
            LoadModelTextures();
            CreateDataLists();
            BaseFile.Write(stream);
        }

        private void CreateDataLists()
        {
            BaseFile.ReplacedRobots.Clear();
            BaseFile.ReplacedJoints.Clear();
            BaseFile.ReplacedModels.Clear();
            BaseFile.ReplacedObjBitmaps.Clear();
            BaseFile.ReplacedObjBitmapPtrs.Clear();
            foreach (Robot robot in ReplacedRobots)
                BaseFile.ReplacedRobots.Add(robot);
            foreach (JointPos joint in ReplacedJoints)
                BaseFile.ReplacedJoints.Add(joint);
            foreach (Polymodel model in ReplacedModels)
                BaseFile.ReplacedModels.Add(model);
            foreach (ReplacedBitmapElement bm in ReplacedObjBitmaps)
                BaseFile.ReplacedObjBitmaps.Add(bm);
            foreach (ReplacedBitmapElement bm in ReplacedObjBitmapPtrs)
                BaseFile.ReplacedObjBitmapPtrs.Add(bm);
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
                    textureMapping.Add(img.Name.ToLower(), i);
            }
            //Add EClip names
            for (int i = 0; i < BaseHAM.EClips.Count; i++)
            {
                clip = BaseHAM.EClips[i];
                if (clip.ChangingObjectTexture != -1)
                    textureMapping.Add(clip.Name.ToLower(), clip.ChangingObjectTexture);
            }
            //If augment file, add augment obj bitmaps
            if (AugmentFile != null)
            {
                for (int i = 0; i < AugmentFile.ObjBitmaps.Count; i++)
                {
                    img = BaseHAM.piggyFile.GetImage(AugmentFile.ObjBitmaps[i]);
                    if (!textureMapping.ContainsKey(img.Name.ToLower()))
                        textureMapping.Add(img.Name.ToLower(), i + VHAMFile.NumDescent2ObjBitmaps);
                }
            }

            //Nuke the old replaced ObjBitmaps and ObjBitmapPointers because they aren't needed anymore
            ReplacedObjBitmaps.Clear();
            ReplacedObjBitmapPtrs.Clear();

            //Generate the new elements
            Polymodel model;
            int replacedNum;
            List<int> newTextures = new List<int>();
            string texName;
            for (int i = 0; i < ReplacedModels.Count; i++)
            {
                model = ReplacedModels[i];
                replacedNum = model.BaseTexture;

                //Find the unique textures in this model
                for (int j = 0; j < model.TextureList.Count; j++)
                {
                    texName = model.TextureList[j].ToLower();
                    if (!textureMapping.ContainsKey(texName))
                        newTextures.Add(BaseHAM.piggyFile.GetBitmapIDFromName(texName));
                }
                //Generate the new ObjBitmaps
                foreach (int newID in newTextures)
                {
                    ReplacedBitmapElement elem;
                    elem.Data = (ushort)newID;
                    elem.ReplacementID = replacedNum;
                    ReplacedObjBitmaps.Add(elem);
                    replacedNum++;
                }

                newTextures.Clear();
            }

            //Finally augment things with our own images
            for (int i = 0; i < ReplacedObjBitmaps.Count; i++)
            {
                bm = ReplacedObjBitmaps[i];
                img = BaseHAM.piggyFile.GetImage(bm.Data);
                if (!textureMapping.ContainsKey(img.Name.ToLower()))
                    textureMapping.Add(img.Name.ToLower(), bm.ReplacementID);
            }

            //Final stage: generate new ObjBitmapPointers
            for (int i = 0; i < ReplacedModels.Count; i++)
            {
                model = ReplacedModels[i];
                replacedNum = model.FirstTexture;

                foreach (string texture in model.TextureList)
                {
                    string ltexture = texture.ToLower();
                    ReplacedBitmapElement elem;
                    if (textureMapping.ContainsKey(ltexture))
                        elem.Data = (ushort)textureMapping[ltexture];
                    else
                        elem.Data = 0;
                    elem.ReplacementID = replacedNum;
                    ReplacedObjBitmapPtrs.Add(elem);
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
            robot.NumGuns = (sbyte)model.NumGuns;
            for (int i = 0; i < 8; i++)
            {
                robot.GunPoints[i] = model.GunPoints[i];
                robot.GunSubmodels[i] = (byte)model.GunSubmodels[i];
            }
            for (int m = 0; m < 9; m++)
            {
                for (int f = 0; f < 5; f++)
                {
                    robot.AnimStates[m, f].NumJoints = 0;
                    robot.AnimStates[m, f].Offset = 0;
                }
            }
            if (!model.IsAnimated) return;
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
                    robot.AnimStates[g, state].NumJoints = 0;
                    robot.AnimStates[g, state].Offset = (short)NumRobotJoints;

                    for (int m = 0; m < model.NumSubmodels; m++)
                    {
                        if (gunNums[m] == g)
                        {
                            JointPos joint = new JointPos();
                            joint.JointNum = (short)m;
                            joint.Angles = model.AnimationMatrix[m, state];
                            joint.ReplacementID = NumRobotJoints;
                            ReplacedJoints.Add(joint);
                            robot.AnimStates[g, state].NumJoints++;
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
            if (AugmentFile != null)
                numRobots += AugmentFile.Robots.Count;
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
                for (int i = 0; i < ReplacedRobots.Count; i++)
                {
                    if (ReplacedRobots[i].replacementID == id) return $"New robot {id}"; //return ReplacedRobots[i].Name;
                }
            }
            if (AugmentFile != null && id >= VHAMFile.NumDescent2RobotTypes)
            {
                if (id - VHAMFile.NumDescent2RobotTypes >= AugmentFile.Robots.Count)
                    return string.Format("Unallocated #{0}", id);
                return AugmentFile.Robots[id - VHAMFile.NumDescent2RobotTypes].Name;
            }
            if (id >= BaseHAM.Robots.Count)
                return string.Format("Unallocated #{0}", id);
            return BaseHAM.Robots[id].Name;
        }

        /// <summary>
        /// Gets a robot definition, passing through to the HAM or VHAM files if not replaced.
        /// </summary>
        /// <param name="id">ID of the robot.</param>
        /// <returns>The robot.</returns>
        public Robot GetRobot(int id)
        {
            foreach (Robot robot in ReplacedRobots)
            {
                if (robot.replacementID == id) return robot;
            }
            if (AugmentFile != null)
                return AugmentFile.GetRobot(id); //passes through
            return BaseHAM.GetRobot(id);
        }

        public int GetNumWeapons()
        {
            int numWeapons = BaseHAM.Weapons.Count;
            if (AugmentFile != null)
                numWeapons += AugmentFile.Weapons.Count;
            return numWeapons;
        }

        public string GetWeaponName(int id)
        {
            if (AugmentFile != null && id >= VHAMFile.NumDescent2WeaponTypes)
                return AugmentFile.Weapons[id - VHAMFile.NumDescent2WeaponTypes].Name;
            return BaseHAM.Weapons[id].Name;
        }

        public Weapon GetWeapon(int id)
        {
            if (AugmentFile != null)
                return AugmentFile.GetWeapon(id); //passes through
            return BaseHAM.GetWeapon(id);
        }

        public int GetNumModels()
        {
            int numWeapons = BaseHAM.Models.Count;
            if (AugmentFile != null)
                numWeapons += AugmentFile.Models.Count;
            return numWeapons;
        }

        public string GetModelName(int id, bool baseOnly = false)
        {
            //This is a horrible hack
            if (!baseOnly)
            {
                for (int i = 0; i < ReplacedModels.Count; i++)
                {
                    if (ReplacedModels[i].ReplacementID == id) return $"New model {id}"; //return ReplacedModels[i].Name;
                }
            }
            if (AugmentFile != null && id >= VHAMFile.NumDescent2Polymodels)
            {
                if (id - VHAMFile.NumDescent2Polymodels >= AugmentFile.Models.Count)
                    return string.Format("Unallocated #{0}", id);
                return AugmentFile.Models[id - VHAMFile.NumDescent2Polymodels].Name;
            }
            if (id >= BaseHAM.Models.Count)
                return string.Format("Unallocated #{0}", id);
            return BaseHAM.Models[id].Name;
        }

        public Polymodel GetModel(int id)
        {
            foreach (Polymodel model in ReplacedModels)
            {
                if (model.ReplacementID == id) return model;
            }
            if (AugmentFile != null)
                return AugmentFile.GetModel(id);
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
            foreach (JointPos joint in ReplacedJoints)
            {
                if (joint.ReplacementID == id) return joint;
            }
            if (AugmentFile != null)
                return AugmentFile.GetJoint(id);
            return BaseHAM.Joints[id];
        }

        public ushort GetObjBitmap(int id)
        {
            foreach (ReplacedBitmapElement bitmap in ReplacedObjBitmaps)
                if (bitmap.ReplacementID == id) return bitmap.Data;
            if (AugmentFile != null)
                return AugmentFile.GetObjBitmap(id);
            return BaseHAM.BaseFile.ObjBitmaps[id];
        }

        public ushort GetObjBitmapPointer(int id)
        {
            foreach (ReplacedBitmapElement bitmap in ReplacedObjBitmapPtrs)
                if (bitmap.ReplacementID == id) return bitmap.Data;
            if (AugmentFile != null)
                return AugmentFile.GetObjBitmapPointer(id);
            return BaseHAM.BaseFile.ObjBitmapPointers[id];
        }
    }
}
