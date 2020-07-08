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
            if (robot.ModelNum < VHAMFile.N_D2_POLYGON_MODELS) return;
            Polymodel model = Models[robot.ModelNum - VHAMFile.N_D2_POLYGON_MODELS];
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
            for (int i = 0; i < VHAMFile.N_D2_OBJBITMAPS + ObjBitmaps.Count; i++)
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
            if (id >= 0 && id < BaseHAM.Robots.Count && id < VHAMFile.N_D2_ROBOT_TYPES)
                return BaseHAM.Robots[id];
            else if (id >= VHAMFile.N_D2_ROBOT_TYPES)
                return Robots[id - VHAMFile.N_D2_ROBOT_TYPES];
            //sorry, you get null and you better like it
            return null;
        }

        public Weapon GetWeapon(int id)
        {
            if (id >= 0 && id < BaseHAM.Weapons.Count && id < VHAMFile.N_D2_WEAPON_TYPES)
                return BaseHAM.Weapons[id];
            else if (id >= VHAMFile.N_D2_WEAPON_TYPES)
                return Weapons[id - VHAMFile.N_D2_WEAPON_TYPES];
            return null;
        }

        public Polymodel GetModel(int id)
        {
            if (id >= 0 && id < BaseHAM.Models.Count && id < VHAMFile.N_D2_POLYGON_MODELS)
                return BaseHAM.Models[id];
            else if (id >= VHAMFile.N_D2_POLYGON_MODELS)
                return Models[id - VHAMFile.N_D2_POLYGON_MODELS];
            return null;
        }

        public JointPos GetJoint(int id)
        {
            if (id >= 0 && id < BaseHAM.Joints.Count && id < VHAMFile.N_D2_ROBOT_JOINTS)
                return BaseHAM.Joints[id];
            else if (id >= VHAMFile.N_D2_ROBOT_JOINTS)
                return Joints[id - VHAMFile.N_D2_ROBOT_JOINTS];
            return new JointPos(); //shouldn't happen
        }

        public ushort GetObjBitmap(int id)
        {
            if (id >= 0 && id < BaseHAM.ObjBitmaps.Count && (id < VHAMFile.N_D2_OBJBITMAPS || id >= ObjBitmaps.Count + VHAMFile.N_D2_OBJBITMAPS))
                return BaseHAM.ObjBitmaps[id];
            else if (id >= VHAMFile.N_D2_OBJBITMAPS)
                return ObjBitmaps[id - VHAMFile.N_D2_OBJBITMAPS];
            return 0;
        }

        public ushort GetObjBitmapPointer(int id)
        {
            if (id >= 0 && id < BaseHAM.ObjBitmaps.Count && (id < VHAMFile.N_D2_OBJBITMAPPTRS || id >= ObjBitmapPointers.Count + VHAMFile.N_D2_OBJBITMAPPTRS))
                return BaseHAM.ObjBitmapPointers[id];
            else if (id >= VHAMFile.N_D2_OBJBITMAPPTRS)
                return ObjBitmapPointers[id - VHAMFile.N_D2_OBJBITMAPPTRS];
            return 0;
        }

        public string GetRobotName(int id)
        {
            if (id >= 0 && id < BaseHAM.Robots.Count && id < VHAMFile.N_D2_ROBOT_TYPES)
                return BaseHAM.RobotNames[id];
            else if (id >= VHAMFile.N_D2_ROBOT_TYPES)
                return RobotNames[id - VHAMFile.N_D2_ROBOT_TYPES];
            return "<undefined>";
        }

        public string GetWeaponName(int id)
        {
            if (id >= 0 && id < BaseHAM.Weapons.Count && id < VHAMFile.N_D2_WEAPON_TYPES)
                return BaseHAM.WeaponNames[id];
            else if (id >= VHAMFile.N_D2_WEAPON_TYPES)
                return WeaponNames[id - VHAMFile.N_D2_WEAPON_TYPES];
            return "<undefined>";
        }

        public string GetModelName(int id)
        {
            if (id >= 0 && id < BaseHAM.Models.Count && id < VHAMFile.N_D2_POLYGON_MODELS)
                return BaseHAM.ModelNames[id];
            else if (id >= VHAMFile.N_D2_POLYGON_MODELS)
                return ModelNames[id - VHAMFile.N_D2_POLYGON_MODELS];
            return "<undefined>";
        }

        public int GetNumRobots()
        {
            //More robots in the base file than the augment file would add. This is a horrible situation
            if (BaseHAM.Robots.Count > (VHAMFile.N_D2_ROBOT_TYPES + Robots.Count))
                return BaseHAM.Robots.Count;

            return VHAMFile.N_D2_ROBOT_TYPES + Robots.Count;
        }

        public int GetNumWeapons()
        {
            //More robots in the base file than the augment file would add. This is a horrible situation
            if (BaseHAM.Weapons.Count > (VHAMFile.N_D2_WEAPON_TYPES + Weapons.Count))
                return BaseHAM.Weapons.Count;

            return VHAMFile.N_D2_WEAPON_TYPES + Weapons.Count;
        }

        public int GetNumModels()
        {
            //More robots in the base file than the augment file would add. This is a horrible situation
            if (BaseHAM.Models.Count > (VHAMFile.N_D2_POLYGON_MODELS + Models.Count))
                return BaseHAM.Models.Count;

            return VHAMFile.N_D2_POLYGON_MODELS + Models.Count;
        }

        public void Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
