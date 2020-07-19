/*
    Copyright (c) 2020 The LibDescent Team

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
using System.Linq;
using System.Collections.Generic;
using System.Numerics;

namespace LibDescent.Data
{
    public class PolymodelBuilder
    {
        public void RebuildModel (Polymodel model)
        {
            List<ModelData> data = ExtractModelData(model);
            List<BSPNode> trees = BuildBSPTrees(data);

            RebuildModel(model, trees);
        }

        private static List<BSPNode> BuildBSPTrees(List<ModelData> data)
        {
            List<BSPNode> trees = new List<BSPNode>();

            foreach (ModelData modelData in data)
            {
                BSPNode rootNode = new BSPNode();
                rootNode.type = BSPNodeType.Node;

                BSPTree tree = new BSPTree();

                var triangle = modelData.Triangles.First();

                triangle.CalculateCenter();
                //triangle.CalculateNormal();

                rootNode.Point = triangle.Point;
                rootNode.Normal = triangle.Normal;

                var faces = modelData.Triangles.ToList();

                foreach (var face in faces)
                {
                    face.CalculateCenter();
                    //face.CalculateNormal();
                }

                tree.BuildTree(rootNode, faces);

                trees.Add(rootNode);
            }

            return trees;
        }

        private static List<ModelData> ExtractModelData(Polymodel model)
        {
            PolymodelExtractor me = new PolymodelExtractor();
            me.SetModel(model);

            var data = me.Extract();
            if (me.IsPartitioned)
                throw new Exception("Model is already partitioned. Further partitioning will bloat data.");
            return data;
        }

        private void RebuildModel(Polymodel newModel, List<BSPNode> trees)
        {
            int offset = 0;

            newModel.InterpreterData = new byte[1024 * 1024];

            var data = newModel.InterpreterData;

            MetaInstructionBase hierarchy = this.GetHierarchy(newModel.Submodels[0]);

            hierarchy.Write(newModel.InterpreterData, ref offset, trees);

            SetShort(newModel.InterpreterData, ref offset, 0);

            newModel.ModelIDTASize = offset;
            newModel.InterpreterData = newModel.InterpreterData.Take(offset).ToArray();

        }

        private MetaInstructionBase GetHierarchy(Submodel rootModel)
        {
            MetaInstructionBase rootInstruction = new MetaModelInstruction
            {
                Model = rootModel
            };

            var meshesToProcess = rootModel.Children.ToList();

            while (meshesToProcess.Any())
            {
                var closestMesh = this.GetClosestModel(meshesToProcess, rootModel);

                meshesToProcess.Remove(closestMesh);

                MetaSortInstruction sorting = new MetaSortInstruction();

                CalculatePositionAndNormal(rootModel, closestMesh, sorting);

                sorting.BackInstruction = new MetaSubModelInstruction
                {
                    SubModel = closestMesh,
                    Instruction = GetHierarchy(closestMesh)

                };

                sorting.FrontInstruction = rootInstruction;

                rootInstruction = sorting;
            }

            return rootInstruction;
        }

        private void CalculatePositionAndNormal(Submodel rootModel, Submodel furthesModel, MetaSortInstruction sortInstruction)
        {
            Vector3 far = MakeVector3(furthesModel.Point);

            Vector3 center = (MakeVector3(rootModel.Point) + far) / 2.0f;

            Vector3 normal = Vector3.Normalize(far - center);

            // Set the center
            sortInstruction.Point = MakeFixVector(center);
            sortInstruction.Normal = MakeFixVector(normal);
        }

        private static FixVector MakeFixVector(Vector3 center)
        {
            Fix x = center.X;
            Fix y = center.Y;
            Fix z = center.Z;

            return new FixVector(x, y, z);
        }

        private Vector3 MakeVector3(FixVector point)
        {
            return new Vector3(point.X, point.Y, point.Z);
        }

        private Submodel GetClosestModel(List<Submodel> modelsToPlace, Submodel rootModel)
        {
            float dist = float.MaxValue;
            Submodel furthest = null;

            foreach (var childModel in modelsToPlace)
            {
                var d = FixVector.Dist(rootModel.Point, childModel.Point);

                if (d < dist)
                {
                    dist = d;
                    furthest = childModel;
                }
            }

            return furthest;
        }

        public static void BuildModelPolygons(BSPNode tree, byte[] data, ref int modelDataOffset)
        {
            short pointCount = 0;

            SetShort(data, ref modelDataOffset, ModelOpCode.DefinePointStart);

            int vertexCountOffset = modelDataOffset;

            SetShort(data, ref modelDataOffset, 0); // Point count: update this later on
            SetShort(data, ref modelDataOffset, 0); // Start
            SetShort(data, ref modelDataOffset, 0); // not sure what this is

            // Get all points
            GetVertexes(tree, data, ref modelDataOffset, ref pointCount);

            // Update the point count
            int returnLocation = modelDataOffset;
            //modelDataOffset = 2;
            modelDataOffset = vertexCountOffset;
            SetShort(data, ref modelDataOffset, pointCount); // Point count: update this later on
            modelDataOffset = returnLocation;

            // Get faces
            int vertexOffset = 0;

            GetFaces(tree, data, ref modelDataOffset, ref vertexOffset);
        }

        public static void GetFaces(BSPNode node, byte[] data, ref int modelDataOffset, ref int vertexOffset, bool deep = false)
        {
            if (node == null)
            {
                return;
            }

            if (node.Front == null && node.Back != null)
            {
                throw new Exception("eh f");
            }
            else if (node.Front != null && node.Back == null)
            {
                throw new Exception("eh b");
            }

            if (node.Front != null && node.Back != null)
            {
                if (node.Point.X == node.Point.Y && node.Point.Y == node.Point.Z && node.Point.Z == 0.0f)
                {
                    throw new Exception("0!");
                }

                // Sort start
                int sortStatPosition = modelDataOffset;

                SetShort(data, ref modelDataOffset, 4); // SORTNORM opcode

                SetShort(data, ref modelDataOffset, 0); // int n_points

                FixVector normal = new FixVector(node.Normal.X, node.Normal.Y, node.Normal.Z);
                FixVector point = new FixVector(node.Point.X, node.Point.Y, node.Point.Z);

                SetFixVector(data, ref modelDataOffset, normal);
                SetFixVector(data, ref modelDataOffset, point);

                short backOffset = (short)modelDataOffset;
                SetShort(data, ref modelDataOffset, backOffset); // fix the back offset later

                short frontOffset = (short)modelDataOffset;
                SetShort(data, ref modelDataOffset, frontOffset); // fix the front offset later

                // Terminator opcode
                SetShort(data, ref modelDataOffset, ModelOpCode.End);

                // Process front and store offset
                int frontOffsetValue = modelDataOffset - sortStatPosition;
                GetFaces(node.Front, data, ref modelDataOffset, ref vertexOffset, true);

                // Process back and store offset
                int backOffsetValue = modelDataOffset - sortStatPosition;
                GetFaces(node.Back, data, ref modelDataOffset, ref vertexOffset, true);


                // Store the end position
                int endPosition = modelDataOffset;



                // Correct the back offset
                modelDataOffset = backOffset;
                SetShort(data, ref modelDataOffset, (short)frontOffsetValue); // fix the back offset later

                // Correct the front offset
                modelDataOffset = frontOffset;
                SetShort(data, ref modelDataOffset, (short)backOffsetValue); // fix the back offset later


                // Restore the offset to the end position
                modelDataOffset = endPosition;

                if (node.faces != null && node.faces.Any())
                {
                    throw new Exception("Missing faces!");
                }
            }
            else if (node.faces != null)
            {
                int facesStatPosition = modelDataOffset;
                foreach (var face in node.faces)
                {
                    if (face.TextureID == -1)
                    {
                        // Flat poly opcode
                        SetShort(data, ref modelDataOffset, ModelOpCode.FlatPoly);

                        short pointc = (short)face.Points.Count();
                        SetShort(data, ref modelDataOffset, pointc);

                        Fix x = face.Point.X;
                        Fix y = face.Point.Y;
                        Fix z = face.Point.Z;

                        var facePoint = new FixVector(x, y, z);

                        SetFixVector(data, ref modelDataOffset, facePoint);

                        x = face.Normal.X;
                        y = face.Normal.Y;
                        z = face.Normal.Z;

                        var normal = new FixVector(x, y, z);

                        SetFixVector(data, ref modelDataOffset, normal);
                        SetShort(data, ref modelDataOffset, (short)face.Color);

                        for (short i = 0; i < pointc; i++)
                        {
                            SetShort(data, ref modelDataOffset, (short)vertexOffset);
                            vertexOffset++;
                        }

                        if (pointc % 2 == 0)
                        {
                            SetShort(data, ref modelDataOffset, 0);
                        }
                    }
                    else
                    {
                        // tmapped poly opcode
                        SetShort(data, ref modelDataOffset, ModelOpCode.TexturedPoly);

                        short pointc = (short)face.Points.Count();
                        SetShort(data, ref modelDataOffset, pointc);

                        Fix x = face.Point.X;
                        Fix y = face.Point.Y;
                        Fix z = face.Point.Z;

                        var facePoint = new FixVector(x, y, z);

                        SetFixVector(data, ref modelDataOffset, facePoint);

                        x = face.Normal.X;
                        y = face.Normal.Y;
                        z = face.Normal.Z;

                        var normal = new FixVector(x, y, z);

                        SetFixVector(data, ref modelDataOffset, normal);

                        SetShort(data, ref modelDataOffset, (short)face.TextureID);

                        for (short i = 0; i < pointc; i++)
                        {
                            SetShort(data, ref modelDataOffset, (short)vertexOffset);
                            vertexOffset++;
                        }

                        if (pointc % 2 == 0)
                        {
                            SetShort(data, ref modelDataOffset, 0);
                        }


                        for (short i = 0; i < pointc; i++)
                        {
                            x = face.Points[i].UVs.X;
                            y = face.Points[i].UVs.Y;
                            z = face.Points[i].UVs.Z;

                            var uv = new FixVector(x, y, z);

                            SetFixVector(data, ref modelDataOffset, uv);
                        }

                    }

                }

                SetShort(data, ref modelDataOffset, ModelOpCode.End);



            }
        }

        private static void GetVertexes(BSPNode bSPNode, byte[] interpreterData, ref int modelDataOffset, ref short pointCount)
        {
            if (bSPNode == null)
                return;

            GetVertexes(bSPNode.Front, interpreterData, ref modelDataOffset, ref pointCount);
            GetVertexes(bSPNode.Back, interpreterData, ref modelDataOffset, ref pointCount);

            if (bSPNode.faces != null)
            {
                foreach (var face in bSPNode.faces)
                {
                    foreach (var point in face.Points)
                    {
                        Fix x = point.Point.X;
                        Fix y = point.Point.Y;
                        Fix z = point.Point.Z;

                        var vec = new FixVector(x, y, z);

                        SetFixVector(interpreterData, ref modelDataOffset, vec);
                        pointCount++;
                    }
                }

            }
        }

        public static void SetShort(byte[] data, ref int offset, Int16 value)
        {
            data[offset] = (byte)(value & 0xff);
            data[offset + 1] = (byte)((value >> 8) & 0xff);

            offset += 2;
        }

        public static void SetInt(byte[] data, ref int offset, Int32 value)
        {
            data[offset] = (byte)(value & 0xff);
            data[offset + 1] = (byte)((value >> 8) & 0xff);
            data[offset + 2] = (byte)((value >> 16) & 0xff);
            data[offset + 3] = (byte)((value >> 24) & 0xff);

            offset += 4;
        }

        public static void SetFixVector(byte[] data, ref int offset, FixVector value)
        {
            SetInt(data, ref offset, value.X.Value);
            SetInt(data, ref offset, value.Y.Value);
            SetInt(data, ref offset, value.Z.Value);
        }
    }


    public class InstructionBase
    {

    }

    public class EndInstruction : InstructionBase
    {
        public void Read(DescentReader reader)
        {

        }
    }

    public class TexturedPolygonInstruction : InstructionBase
    {
        public short PointCount { get; private set; }
        public FixVector Point { get; private set; }
        public FixVector Normal { get; private set; }
        public short Texture { get; private set; }
        public short[] Points { get; private set; }
        public FixVector[] Uvls { get; private set; }
        public short Padding { get; private set; }

        public void Read(DescentReader reader)
        {
            PointCount = reader.ReadInt16();
            Point = reader.ReadFixVector();
            Normal = reader.ReadFixVector();

            Texture = reader.ReadInt16();

            Points = new short[PointCount]; //TODO: seems wasteful to do all these allocations?

            Uvls = new FixVector[PointCount];

            for (int i = 0; i < PointCount; i++)
            {
                Points[i] = reader.ReadInt16();
            }

            if (PointCount % 2 == 0)
            {
                Padding = reader.ReadInt16();
            }

            for (int i = 0; i < PointCount; i++)
            {
                Uvls[i] = reader.ReadFixVector();
            }
        }
    }

    public class SortInstruction : InstructionBase
    {
        public short PointCount { get; private set; }
        public FixVector Normal { get; set; }
        public FixVector Point { get; set; }
        public short BackOffset { get; private set; }
        public short FrontOffset { get; private set; }

        //        public InstructionBase BackInstruction { get; set; }

        //        public InstructionBase FrontInstruction { get; set; }

        public void Read(DescentReader reader)
        {
            //int baseOffset = offset - 2;
            PointCount = reader.ReadInt16();
            Normal = reader.ReadFixVector();
            Point = reader.ReadFixVector();
            BackOffset = reader.ReadInt16();
            FrontOffset = reader.ReadInt16();
        }
    }

    public class SubModelInstruction : InstructionBase
    {
        public int BaseOffset { get; private set; }
        public short SubmodelNum { get; private set; }
        public FixVector SubmodelOffset { get; private set; }
        public short ModelOffset { get; private set; }
        public short Skip { get; private set; }

        public void Read(DescentReader reader)
        {
            BaseOffset = (int)reader.BaseStream.Position - 2;

            SubmodelNum = reader.ReadInt16();

            SubmodelOffset = reader.ReadFixVector();

            ModelOffset = reader.ReadInt16();

            Skip = reader.ReadInt16();
        }


        //        public InstructionBase NextInstruction { get; set; }
    }

    public class DefPointsInstruction : InstructionBase
    {
        public short PointCount { get; set; }
        public short FirstPoint { get; set; }
        public short Skip { get; private set; }
        public FixVector[] Points { get; set; }

        public void Read(DescentReader reader)
        {
            PointCount = reader.ReadInt16();

            FirstPoint = reader.ReadInt16();

            Skip = reader.ReadInt16();

            Points = new FixVector[PointCount];

            //GL.PointSize(4.0f);
            //GL.Begin(PrimitiveType.Points);
            //GL.Color3(1.0f, 1.0f, 1.0f);

            for (int i = 0; i < PointCount; i++)
            {
                Points[i] = reader.ReadFixVector();
            }
        }
    }



    public abstract class MetaInstructionBase
    {
        public abstract void Write(byte[] data, ref int offset, List<BSPNode> trees);
    }

    public class MetaSortInstruction : MetaInstructionBase
    {
        public FixVector Normal { get; set; }
        public FixVector Point { get; set; }


        public MetaInstructionBase BackInstruction { get; set; }

        public MetaInstructionBase FrontInstruction { get; set; }


        public override void Write(byte[] data, ref int offset, List<BSPNode> trees)
        {
            int sortStatPosition = offset;

            PolymodelBuilder.SetShort(data, ref offset, 4); // SORTNORM opcode

            PolymodelBuilder.SetShort(data, ref offset, 0); // int n_points

            FixVector normal = new FixVector(Normal.X, Normal.Y, Normal.Z);
            FixVector point = new FixVector(Point.X, Point.Y, Point.Z);


            PolymodelBuilder.SetFixVector(data, ref offset, normal);
            PolymodelBuilder.SetFixVector(data, ref offset, point);

            short backOffset = (short)offset;
            PolymodelBuilder.SetShort(data, ref offset, backOffset); // fix the back offset later

            short frontOffset = (short)offset;
            PolymodelBuilder.SetShort(data, ref offset, frontOffset); // fix the front offset later


            // End
            PolymodelBuilder.SetShort(data, ref offset, ModelOpCode.End); // END opcode


            // Back
            int backOffsetValue = offset - sortStatPosition;
            BackInstruction.Write(data, ref offset, trees);

            // Front
            int frontOffsetValue = offset - sortStatPosition;
            FrontInstruction.Write(data, ref offset, trees);


            // store current position
            int endPosition = offset;

            offset = backOffset;
            PolymodelBuilder.SetShort(data, ref offset, (short)backOffsetValue);


            offset = frontOffset;
            PolymodelBuilder.SetShort(data, ref offset, (short)frontOffsetValue);

            // Return
            offset = endPosition;


        }
    }

    public class MetaModelInstruction : MetaInstructionBase
    {
        public Submodel Model { get; set; }

        public bool IsTerminator { get; set; }

        public override void Write(byte[] data, ref int offset, List<BSPNode> trees)
        {
            Model.Pointer = offset;

            var tree = trees[Model.ID];
            PolymodelBuilder.BuildModelPolygons(tree, data, ref offset);

            if (this.IsTerminator)
            {
                PolymodelBuilder.SetShort(data, ref offset, ModelOpCode.End);
            }
        }
    }

    public class MetaSubModelInstruction : MetaInstructionBase
    {
        public Submodel SubModel { get; set; }

        public MetaInstructionBase Instruction { get; set; }

        public override void Write(byte[] data, ref int offset, List<BSPNode> trees)
        {
            int index = SubModel.ID;
            var tree = trees[index];


            int offsetBase = offset;

            PolymodelBuilder.SetShort(data, ref offset, ModelOpCode.SubCall); // SUBCALL

            short submodelNum = (short)(index);
            PolymodelBuilder.SetShort(data, ref offset, submodelNum);


            //FixVector submodelOffset = newModel.Submodels[index].Offset;
            FixVector submodelOffset = SubModel.Offset;

            PolymodelBuilder.SetFixVector(data, ref offset, submodelOffset);

            // The address where we write the new offset value
            int offsetAddress = offset;
            short offsetValue = 0;


            PolymodelBuilder.SetShort(data, ref offset, offsetValue);
            offset += 2;



            // Always, write the end op code
            PolymodelBuilder.SetShort(data, ref offset, ModelOpCode.End);



            // Calculate the new offset 
            offsetValue = (short)(offset - offsetBase);


            // If there is a sub instruction, follow it here, just don't draw the model yourself
            Instruction?.Write(data, ref offset, trees);


            // Store offset
            var endOffset = offset;

            offset = offsetAddress;
            PolymodelBuilder.SetShort(data, ref offset, (short)offsetValue);

            offset = endOffset;




            // 

        }
    }
}
