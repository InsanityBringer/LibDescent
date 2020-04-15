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
using System.IO;

namespace LibDescent.Data
{
    public class VHAMFile
    {
        public HAMFile baseFile;

        public List<Robot> Robots { get; private set; }
        public List<Weapon> Weapons { get; private set; }
        public List<Polymodel> Models { get; private set; }
        public List<JointPos> Joints { get; private set; }
        public List<ushort> ObjBitmaps { get; private set; }
        public List<ushort> ObjBitmapPointers { get; private set; }

        //ARGH
        //VHAM elements are loaded at fixed locations
        public const int N_D2_ROBOT_TYPES = 66;
        public const int N_D2_ROBOT_JOINTS = 1145;
        public const int N_D2_POLYGON_MODELS = 166;
        public const int N_D2_OBJBITMAPS = 422;
        public const int N_D2_OBJBITMAPPTRS = 502;
        public const int N_D2_WEAPON_TYPES = 62;

        public int NumRobots { get { return Robots.Count + N_D2_ROBOT_TYPES; } }
        public int NumWeapons { get { return Weapons.Count + N_D2_WEAPON_TYPES; } }
        public int NumModels { get { return Models.Count + N_D2_POLYGON_MODELS; } }

        public VHAMFile(HAMFile baseFile)
        {
            this.baseFile = baseFile;
            Robots = new List<Robot>();
            Weapons = new List<Weapon>();
            Models = new List<Polymodel>();
            Joints = new List<JointPos>();
            ObjBitmaps = new List<ushort>();
            ObjBitmapPointers = new List<ushort>();
        }

        public int Read(Stream stream)
        {
            BinaryReader br;

            br = new BinaryReader(stream);

            HAMDataReader bm = new HAMDataReader();
            int sig = br.ReadInt32();
            if (sig != 0x5848414D)
            {
                br.Close();
                br.Dispose();
                return -1;
            }
            int version = br.ReadInt32();
            if (version != 1)
            {
                br.Close();
                br.Dispose();
                return -2;
            }

            int numWeapons = br.ReadInt32();
            for (int i = 0; i < numWeapons; i++)
            {
                Weapons.Add(bm.ReadWeapon(br));
                Weapons[i].ID = i + N_D2_WEAPON_TYPES;
            }
            int numRobots = br.ReadInt32();
            for (int i = 0; i < numRobots; i++)
            {
                Robots.Add(bm.ReadRobot(br));
                Robots[i].ID = i + N_D2_ROBOT_TYPES;
            }
            int numJoints = br.ReadInt32();
            for (int i = 0; i < numJoints; i++)
            {
                JointPos joint = new JointPos();
                joint.jointnum = br.ReadInt16();
                joint.angles.p = br.ReadInt16();
                joint.angles.b = br.ReadInt16();
                joint.angles.h = br.ReadInt16();
                Joints.Add(joint);
            }
            int numModels = br.ReadInt32();
            for (int i = 0; i < numModels; i++)
            {
                Models.Add(bm.ReadPolymodelInfo(br));
                Models[i].ID = i + N_D2_POLYGON_MODELS;
            }
            for (int x = 0; x < numModels; x++)
            {
                Models[x].InterpreterData = br.ReadBytes(Models[x].ModelIDTASize);
            }
            for (int i = 0; i < numModels; i++)
            {
                Models[i].DyingModelnum = br.ReadInt32();
            }
            for (int i = 0; i < numModels; i++)
            {
                Models[i].DeadModelnum = br.ReadInt32();
            }
            int numObjBitmaps = br.ReadInt32();
            for (int i = 0; i < numObjBitmaps; i++)
            {
                ObjBitmaps.Add(br.ReadUInt16());
            }
            int numObjBitmapPointers = br.ReadInt32();
            for (int i = 0; i < numObjBitmapPointers; i++)
            {
                ObjBitmapPointers.Add(br.ReadUInt16());
            }

            br.Dispose();

            return 0;
        }
    }
}
