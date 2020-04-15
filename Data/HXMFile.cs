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
    /// <summary>
    /// Wraps a replaced bitmap index.
    /// </summary>
    public struct ReplacedBitmapElement
    {
        public int replacementID;
        public ushort data; 
    }
    public class HXMFile
    {
        public int sig, ver;
        public List<Robot> replacedRobots { get; private set; }
        public List<JointPos> replacedJoints { get; private set; }
        public List<Polymodel> replacedModels { get; private set; }
        public List<ReplacedBitmapElement> replacedObjBitmaps { get; private set; }
        public List<ReplacedBitmapElement> replacedObjBitmapPtrs { get; private set; }

        /// <summary>
        /// Creates a new HXM File with a parent HAM file.
        /// </summary>
        /// <param name="baseFile">The HAM file this HXM file will replace elements of.</param>
        public HXMFile()
        {
            replacedRobots = new List<Robot>();
            replacedJoints = new List<JointPos>();
            replacedModels = new List<Polymodel>();
            replacedObjBitmaps = new List<ReplacedBitmapElement>();
            replacedObjBitmapPtrs = new List<ReplacedBitmapElement>();
        }

        /// <summary>
        /// Loads an HXM file from a given stream.
        /// </summary>
        /// <param name="stream">The stream to load the HXM data from.</param>
        /// <returns></returns>
        public int Read(Stream stream)
        {
            BinaryReader br;
            br = new BinaryReader(stream);

            HAMDataReader data = new HAMDataReader();

            sig = br.ReadInt32();
            ver = br.ReadInt32();

            int replacedRobotCount = br.ReadInt32();
            for (int x = 0; x < replacedRobotCount; x++)
            {
                int replacementID = br.ReadInt32();
                Robot robot = data.ReadRobot(br);
                robot.replacementID = replacementID;
                replacedRobots.Add(robot);
            }
            int replacedJointCount = br.ReadInt32();
            for (int x = 0; x < replacedJointCount; x++)
            {
                int replacementID = br.ReadInt32();
                JointPos joint = new JointPos();
                joint.jointnum = br.ReadInt16();
                joint.angles.p = br.ReadInt16();
                joint.angles.b = br.ReadInt16();
                joint.angles.h = br.ReadInt16();
                joint.replacementID = replacementID;
                replacedJoints.Add(joint);
            }
            int modelsToReplace = br.ReadInt32();
            for (int x = 0; x < modelsToReplace; x++)
            {
                int replacementID = br.ReadInt32();
                Polymodel model = data.ReadPolymodelInfo(br);
                model.replacementID = replacementID;
                model.InterpreterData = br.ReadBytes(model.ModelIDTASize);
                replacedModels.Add(model);
                model.DyingModelnum = br.ReadInt32();
                model.DeadModelnum = br.ReadInt32();
            }
            int objBitmapsToReplace = br.ReadInt32();
            for (int x = 0; x < objBitmapsToReplace; x++)
            {
                ReplacedBitmapElement objBitmap = new ReplacedBitmapElement();
                objBitmap.replacementID = br.ReadInt32();
                objBitmap.data = br.ReadUInt16();
                replacedObjBitmaps.Add(objBitmap);
                //Console.WriteLine("Loading replacement obj bitmap, replacing slot {0} with {1} ({2})", objBitmap.replacementID, objBitmap.data, baseFile.piggyFile.images[objBitmap.data].name);
            }
            int objBitmapPtrsToReplace = br.ReadInt32();
            for (int x = 0; x < objBitmapPtrsToReplace; x++)
            {
                ReplacedBitmapElement objBitmap = new ReplacedBitmapElement();
                objBitmap.replacementID = br.ReadInt32();
                objBitmap.data = br.ReadUInt16();
                replacedObjBitmapPtrs.Add(objBitmap);
            }
            return 0;
        }

        /// <summary>
        /// Saves the HXM file to a given stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public void Write(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
            HAMDataWriter datawriter = new HAMDataWriter();

            bw.Write(sig);
            bw.Write(ver);

            bw.Write(replacedRobots.Count);
            for (int x = 0; x < replacedRobots.Count; x++)
            {
                bw.Write(replacedRobots[x].replacementID);
                datawriter.WriteRobot(replacedRobots[x], bw);
            }
            bw.Write(replacedJoints.Count);
            for (int x = 0; x < replacedJoints.Count; x++)
            {
                bw.Write(replacedJoints[x].replacementID);
                bw.Write(replacedJoints[x].jointnum);
                bw.Write(replacedJoints[x].angles.p);
                bw.Write(replacedJoints[x].angles.b);
                bw.Write(replacedJoints[x].angles.h);
            }
            bw.Write(replacedModels.Count);
            for (int x = 0; x < replacedModels.Count; x++)
            {
                bw.Write(replacedModels[x].replacementID);
                datawriter.WritePolymodel(replacedModels[x], bw);
                bw.Write(replacedModels[x].InterpreterData);
                bw.Write(replacedModels[x].DyingModelnum);
                bw.Write(replacedModels[x].DeadModelnum);
            }
            bw.Write(replacedObjBitmaps.Count);
            for (int x = 0; x < replacedObjBitmaps.Count; x++)
            {
                bw.Write(replacedObjBitmaps[x].replacementID);
                bw.Write(replacedObjBitmaps[x].data);
            }
            bw.Write(replacedObjBitmapPtrs.Count);
            for (int x = 0; x < replacedObjBitmapPtrs.Count; x++)
            {
                bw.Write(replacedObjBitmapPtrs[x].replacementID);
                bw.Write(replacedObjBitmapPtrs[x].data);
            }

            bw.Dispose();
        }
    }
}
