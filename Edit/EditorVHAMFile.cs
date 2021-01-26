using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LibDescent.Data;

namespace LibDescent.Edit
{
    public class EditorVHAMFile : IDataFile
    {
        public VHAMFile BaseFile { get; private set; }
        public EditorHAMFile BaseHAM { get; private set; }

        public List<Robot> Robots { get; private set; }
        public List<Weapon> Weapons { get; private set; }
        public List<Polymodel> Models { get; private set; }
        public List<JointPos> Joints { get; private set; }
        public List<ushort> ObjBitmaps { get; private set; }
        public List<ushort> ObjBitmapPointers { get; private set; }

        //Namelists
        public List<string> RobotNames = new List<string>();
        public List<string> WeaponNames = new List<string>();
        public List<string> ModelNames = new List<string>();

        public EditorVHAMFile(VHAMFile baseFile, EditorHAMFile baseHAM)
        {
            BaseFile = baseFile;
            BaseHAM = baseHAM;

            Robots = new List<Robot>();
            Weapons = new List<Weapon>();
            Models = new List<Polymodel>();
            Joints = new List<JointPos>();
            ObjBitmaps = new List<ushort>();
            ObjBitmapPointers = new List<ushort>();
        }

        public EditorVHAMFile(EditorHAMFile baseHAM) : this(new VHAMFile(), baseHAM)
        {
        }

        public void Read(Stream stream)
        {
            //If a namefile isn't present, automatically generate namelists for our convenience. 
            bool generateNameLists = true;
            BaseFile.Read(stream);
            CreateLocalLists();

            foreach (Robot robot in Robots)
            {
                BuildModelAnimation(robot);
            }
            BuildModelTextureTables();

            if (generateNameLists)
            {
                for (int i = 0; i < Weapons.Count; i++)
                {
                    WeaponNames.Add(String.Format("New Weapon {0}", i + 1));
                }
                for (int i = 0; i < Robots.Count; i++)
                {
                    RobotNames.Add(String.Format("New Robot {0}", i + 1));
                }
                for (int i = 0; i < Models.Count; i++)
                {
                    ModelNames.Add(String.Format("New Model {0}", i + 1));
                }
            }
        }

        private void CreateLocalLists()
        {
            foreach (Robot robot in BaseFile.Robots)
                Robots.Add(robot);
            foreach (Weapon weapon in BaseFile.Weapons)
                Weapons.Add(weapon);
            foreach (Polymodel model in BaseFile.Models)
                Models.Add(model);
            foreach (JointPos joint in BaseFile.Joints)
                Joints.Add(joint);
            foreach (ushort bm in BaseFile.ObjBitmaps)
                ObjBitmaps.Add(bm);
            foreach (ushort bm in BaseFile.ObjBitmapPointers)
                ObjBitmapPointers.Add(bm);
        }

        private void BuildModelAnimation(Robot robot)
        {
            //this shouldn't happen?
            if (robot.ModelNum == -1) return;
            //If the robot is referring to a base HAM file model, reject it
            if (robot.ModelNum < VHAMFile.NumDescent2Polymodels) return;
            Polymodel model = Models[robot.ModelNum - VHAMFile.NumDescent2Polymodels];
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
            for (int m = 0; m < Polymodel.MaxGuns + 1; m++)
            {
                for (int f = 0; f < Robot.NumAnimationStates; f++)
                {
                    Robot.JointList robotjointlist = robot.AnimStates[m, f];
                    basejoint = robotjointlist.Offset;
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
        }

        //Variation of the HAM one, only applies to new models
        public void BuildModelTextureTables()
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
            for (int i = 0; i < VHAMFile.NumDescent2ObjBitmaps + ObjBitmaps.Count; i++)
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
            foreach (Polymodel model in Models)
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
                }
                Console.Write("Addon model texture list: [");
                foreach (string texture in model.TextureList)
                {
                    Console.Write("{0} ", texture);
                }
                Console.WriteLine("]");
            }
        }

        //Convenience members to access elements by their absolute ID, when needed
        public Robot GetRobot(int id)
        {
            if (id >= 0 && id < BaseHAM.Robots.Count && id < VHAMFile.NumDescent2RobotTypes)
                return BaseHAM.Robots[id];
            else if (id >= VHAMFile.NumDescent2RobotTypes)
                return Robots[id - VHAMFile.NumDescent2RobotTypes];
            //sorry, you get null and you better like it
            return null;
        }

        public Weapon GetWeapon(int id)
        {
            if (id >= 0 && id < BaseHAM.Weapons.Count && id < VHAMFile.NumDescent2WeaponTypes)
                return BaseHAM.Weapons[id];
            else if (id >= VHAMFile.NumDescent2WeaponTypes)
                return Weapons[id - VHAMFile.NumDescent2WeaponTypes];
            return null;
        }

        public Polymodel GetModel(int id)
        {
            if (id >= 0 && id < BaseHAM.Models.Count && id < VHAMFile.NumDescent2Polymodels)
                return BaseHAM.Models[id];
            else if (id >= VHAMFile.NumDescent2Polymodels)
                return Models[id - VHAMFile.NumDescent2Polymodels];
            return null;
        }

        public JointPos GetJoint(int id)
        {
            if (id >= 0 && id < BaseHAM.Joints.Count && id < VHAMFile.NumDescent2Joints)
                return BaseHAM.Joints[id];
            else if (id >= VHAMFile.NumDescent2Joints)
                return Joints[id - VHAMFile.NumDescent2Joints];
            return new JointPos(); //shouldn't happen
        }

        public ushort GetObjBitmap(int id)
        {
            if (id >= 0 && id < BaseHAM.ObjBitmaps.Count && (id < VHAMFile.NumDescent2ObjBitmaps || id >= ObjBitmaps.Count + VHAMFile.NumDescent2ObjBitmaps))
                return BaseHAM.ObjBitmaps[id];
            else if (id >= VHAMFile.NumDescent2ObjBitmaps)
                return ObjBitmaps[id - VHAMFile.NumDescent2ObjBitmaps];
            return 0;
        }

        public ushort GetObjBitmapPointer(int id)
        {
            if (id >= 0 && id < BaseHAM.ObjBitmaps.Count && (id < VHAMFile.NumDescent2ObjBitmapPointers || id >= ObjBitmapPointers.Count + VHAMFile.NumDescent2ObjBitmapPointers))
                return BaseHAM.ObjBitmapPointers[id];
            else if (id >= VHAMFile.NumDescent2ObjBitmapPointers)
                return ObjBitmapPointers[id - VHAMFile.NumDescent2ObjBitmapPointers];
            return 0;
        }

        public string GetRobotName(int id)
        {
            if (id >= 0 && id < BaseHAM.Robots.Count && id < VHAMFile.NumDescent2RobotTypes)
                return BaseHAM.RobotNames[id];
            else if (id >= VHAMFile.NumDescent2RobotTypes)
                return RobotNames[id - VHAMFile.NumDescent2RobotTypes];
            return "<undefined>";
        }

        public string GetWeaponName(int id)
        {
            if (id >= 0 && id < BaseHAM.Weapons.Count && id < VHAMFile.NumDescent2WeaponTypes)
                return BaseHAM.WeaponNames[id];
            else if (id >= VHAMFile.NumDescent2WeaponTypes)
                return WeaponNames[id - VHAMFile.NumDescent2WeaponTypes];
            return "<undefined>";
        }

        public string GetModelName(int id)
        {
            if (id >= 0 && id < BaseHAM.Models.Count && id < VHAMFile.NumDescent2Polymodels)
                return BaseHAM.ModelNames[id];
            else if (id >= VHAMFile.NumDescent2Polymodels)
                return ModelNames[id - VHAMFile.NumDescent2Polymodels];
            return "<undefined>";
        }

        public int GetNumRobots()
        {
            //More robots in the base file than the augment file would add. This is a horrible situation
            if (BaseHAM.Robots.Count > (VHAMFile.NumDescent2RobotTypes + Robots.Count))
                return BaseHAM.Robots.Count;

            return VHAMFile.NumDescent2RobotTypes + Robots.Count;
        }

        public int GetNumWeapons()
        {
            if (BaseHAM.Weapons.Count > (VHAMFile.NumDescent2WeaponTypes + Weapons.Count))
                return BaseHAM.Weapons.Count;

            return VHAMFile.NumDescent2WeaponTypes + Weapons.Count;
        }

        public int GetNumModels()
        {
            if (BaseHAM.Models.Count > (VHAMFile.NumDescent2Polymodels + Models.Count))
                return BaseHAM.Models.Count;

            return VHAMFile.NumDescent2Polymodels + Models.Count;
        }

        private void LoadAnimations(Robot robot, Polymodel model)
        {
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
                    robot.AnimStates[g, state].Offset = (short)(Joints.Count + VHAMFile.NumDescent2Joints);

                    for (int m = 0; m < model.NumSubmodels; m++)
                    {
                        if (gunNums[m] == g)
                        {
                            JointPos joint = new JointPos();
                            joint.JointNum = (short)m;
                            joint.Angles = model.AnimationMatrix[m, state];
                            Joints.Add(joint);
                            robot.AnimStates[g, state].NumJoints++;
                        }
                    }
                }
            }
        }

        private void GenerateObjectBitmapTables()
        {
            ObjBitmaps.Clear();
            ObjBitmapPointers.Clear();
            int lastObjectBitmap = VHAMFile.NumDescent2ObjBitmaps;
            int lastObjectBitmapPointer = VHAMFile.NumDescent2ObjBitmapPointers;
            PIGImage img;
            EClip clip;
            Dictionary<string, int> objectBitmapMapping = new Dictionary<string, int>();

            //Add the HAM file's object bitmaps so they can be referenced. 
            for (int i = 0; i < BaseHAM.BaseFile.ObjBitmaps.Count; i++)
            {
                img = BaseHAM.piggyFile.GetImage(BaseHAM.BaseFile.ObjBitmaps[i]);
                if (!img.IsAnimated && !objectBitmapMapping.ContainsKey(img.Name))
                    objectBitmapMapping.Add(img.Name, i);
            }
            //Add EClip names
            for (int i = 0; i < BaseHAM.EClips.Count; i++)
            {
                clip = BaseHAM.EClips[i];
                if (clip.ChangingObjectTexture != -1)
                    objectBitmapMapping.Add(BaseHAM.EClipNames[i], clip.ChangingObjectTexture);
            }

            Polymodel model;
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
                        ObjBitmaps.Add((ushort)(BaseHAM.piggyFile.GetBitmapIDFromName(textureName)));
                        lastObjectBitmap++;
                    }
                    ObjBitmapPointers.Add((ushort)objectBitmapMapping[textureName.ToLower()]);
                    lastObjectBitmapPointer++;
                }
            }
        }

        private void WriteLocalLists()
        {
            BaseFile.Robots.Clear();
            BaseFile.Weapons.Clear();
            BaseFile.Models.Clear();
            BaseFile.Joints.Clear();
            BaseFile.ObjBitmaps.Clear();
            BaseFile.ObjBitmapPointers.Clear();
            foreach (Robot robot in Robots)
                BaseFile.Robots.Add(robot);
            foreach (Weapon weapon in Weapons)
                BaseFile.Weapons.Add(weapon);
            foreach (Polymodel model in Models)
                BaseFile.Models.Add(model);
            foreach (JointPos joint in Joints)
                BaseFile.Joints.Add(joint);
            foreach (ushort bm in ObjBitmaps)
                BaseFile.ObjBitmaps.Add(bm);
            foreach (ushort bm in ObjBitmapPointers)
                BaseFile.ObjBitmapPointers.Add(bm);
        }

        public void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
