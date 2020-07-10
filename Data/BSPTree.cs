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
using System.Numerics;

//Horrible BSP compiler made from some online article. Ugh. Blech.
//Its not even complete...
namespace LibDescent.Data
{
    public enum BSPClassification
    {
        Front,
        Back,
        OnPlane,
        Spanning,
    }
    public enum BSPNodeType
    {
        Node,
        Leaf,
    }
    public class BSPVertex
    {
        public Vector3 Point;
        public Vector3 UVs;
        public BSPClassification Classification;
    }

    public class BSPNode
    {
        public BSPFace Splitter;
        public Vector3 Point;
        public Vector3 Normal;
        public BSPNode Front;
        public BSPNode Back;
        public List<BSPFace> faces = new List<BSPFace>();
        public BSPNodeType type;
    }

    public class BSPFace
    {
        public Vector3 Point;
        public Vector3 Normal;
        public int textureID;
        public int color;
        public List<BSPVertex> Points = new List<BSPVertex>();
        public BSPClassification Classification;
    }

    public class BSPTree
    {
        public bool PointOnPlane(Vector3 point, Vector3 planePoint, Vector3 planeNorm)
        {
            Vector3 localPoint = point - planePoint;
            return Math.Abs(Vector3.Dot(localPoint, planeNorm)) < .0001;
        }

        public bool PointInFront(Vector3 point, Vector3 planePoint, Vector3 planeNorm)
        {
            Vector3 localPoint = point - planePoint;
            return Vector3.Dot(localPoint, planeNorm) > 0;
        }

        public void BuildTree(BSPNode node, List<BSPFace> faces)
        {
            List<BSPFace> frontList = new List<BSPFace>();
            List<BSPFace> backList = new List<BSPFace>();
            node.Splitter = FindSplitter(faces, ref node.Point, ref node.Normal);
            if (node.Splitter == null) //If a splitter wasn't found, this set of faces is convex
            {
                node.faces = faces;
                node.type = BSPNodeType.Leaf;
            }
            else //A splitter is known, so do any needed splits and recurse
            {
                foreach (BSPFace face in faces)
                {
                    if (face != node.Splitter)
                    {
                        ClassifyFace(face, node.Point, node.Normal);
                        switch (face.Classification)
                        {
                            case BSPClassification.Front:
                                frontList.Add(face);
                                break;
                            case BSPClassification.Back:
                                backList.Add(face);
                                break;
                            case BSPClassification.Spanning:
                                BSPFace frontFace = new BSPFace();
                                BSPFace backFace = new BSPFace();
                                SplitPolygon(face, node.Point, node.Normal, ref frontFace, ref backFace);
                                frontList.Add(frontFace);
                                backList.Add(backFace);
                                break;
                        }
                    }
                }

                if (frontList.Count > 0)
                {
                    BSPNode newNode = new BSPNode();
                    newNode.type = BSPNodeType.Node;
                    BuildTree(newNode, frontList);
                    node.Front = newNode;
                }

                if (backList.Count > 0)
                {
                    BSPNode newNode = new BSPNode();
                    newNode.type = BSPNodeType.Node;
                    BuildTree(newNode, faces);
                    node.Back = newNode;
                }
            }
        }

        public void ClassifyPoint(BSPVertex vert, Vector3 planePoint, Vector3 planeNorm)
        {
            if (PointOnPlane(vert.Point, planePoint, planeNorm)) vert.Classification = BSPClassification.OnPlane;
            else if (PointInFront(vert.Point, planePoint, planeNorm)) vert.Classification = BSPClassification.Front;
            else vert.Classification = BSPClassification.Back;
        }

        public void ClassifyFace(BSPFace face, Vector3 planePoint, Vector3 planeNorm)
        {
            ClassifyPoint(face.Points[0], planePoint, planeNorm);
            foreach (BSPVertex point in face.Points)
            {
                ClassifyPoint(point, planePoint, planeNorm);
                if (point.Classification != face.Points[0].Classification)
                {
                    face.Classification = BSPClassification.Spanning;
                    return;
                }
            }
            if (face.Points[0].Classification == BSPClassification.OnPlane) //Place coplanar faces on the front side of the plane if it is facing the same direction
            {
                if (Vector3.Dot(face.Normal, planeNorm) >= 0)
                    face.Classification = BSPClassification.Front;
                else
                    face.Classification = BSPClassification.Back;
            }
            else
            {
                face.Classification = face.Points[0].Classification;
            }
        }

        public int EvalulateSplitter(List<BSPFace> faces, Vector3 planePoint, Vector3 planeNorm, BSPFace splitter)
        {
            int numFront = 0, numBack = 0, numSplits = 0;
            foreach (BSPFace face in faces)
            {
                if (face == splitter) continue;
                ClassifyFace(face, planePoint, planeNorm);
                switch (face.Classification)
                {
                    case BSPClassification.Front:
                        numFront++;
                        break;
                    case BSPClassification.Back:
                        numBack++;
                        break;
                    case BSPClassification.Spanning:
                        numSplits++;
                        break;
                }
            }

            if (numSplits == 0 && (numFront == 0 || numBack == 0)) //If everything is on one side of this splitter, it has no value. 
            {
                return int.MaxValue;
            }
            return Math.Abs(numFront - numBack) + (numSplits * 8);
        }

        public BSPFace FindSplitter(List<BSPFace> faces, ref Vector3 planePoint, ref Vector3 planeNorm)
        {
            int bestScore = int.MaxValue;
            BSPFace bestFace = null;
            int score;
            foreach (BSPFace potential in faces)
            {
                score = EvalulateSplitter(faces, planePoint, planeNorm, potential);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestFace = potential;
                }
            }
            /*if (bestFace != null) //Splitter can fail to be found if volume is convex
            {
                planePoint = bestFace.Point;
                planeNorm = bestFace.Normal;
                return bestFace;
            }*/
            return bestFace;
        }

        public bool SplitEdge(Vector3 point1, Vector3 point2, Vector3 planePoint, Vector3 planeNorm, ref float percentage, ref Vector3 intersect)
        {
            Vector3 direction = point2 - point1;
            float lineLength = Vector3.Dot(direction, planeNorm);

            if (Math.Abs(lineLength) < 0.0001)
                return false;

            Vector3 L1 = planePoint - point1;
            float distFromPlane = Vector3.Dot(L1, planeNorm);
            percentage = distFromPlane / lineLength;

            if (percentage < 0) return false;
            if (percentage > 1) return false;
            intersect = point1 + (direction * percentage);
            return true;
        }

        public void SplitPolygon(BSPFace face, Vector3 planePoint, Vector3 planeNorm, ref BSPFace front, ref BSPFace back)
        {
            BSPVertex firstPoint = face.Points[0];
            if (firstPoint.Classification == BSPClassification.OnPlane)
            {
                front.Points.Add(firstPoint);
                back.Points.Add(firstPoint);
            }
            else if (firstPoint.Classification == BSPClassification.Front)
                front.Points.Add(firstPoint);
            else
                back.Points.Add(firstPoint);
            int current = 0;
            BSPVertex vert1, vert2;
            for (int i = 1; i < face.Points.Count + 1; i++)
            {
                if (i == face.Points.Count) current = 0;
                else current = i;

                vert1 = face.Points[i - 1];
                vert2 = face.Points[current];

                ClassifyPoint(vert2, planePoint, planeNorm);
                if (vert2.Classification == BSPClassification.OnPlane)
                {
                    front.Points.Add(vert2);
                    back.Points.Add(vert2);
                }
                else
                {
                    Vector3 intersect = new Vector3();
                    float percentage = 0.0f;
                    bool split = SplitEdge(vert1.Point, vert2.Point, planePoint, planeNorm, ref percentage, ref intersect);
                    if (split)
                    {
                        Vector3 texDelta = vert1.UVs - vert2.UVs;
                        BSPVertex newVert = new BSPVertex
                        {
                            Classification = BSPClassification.OnPlane,
                            Point = intersect,
                            UVs = texDelta * percentage + vert1.UVs
                        };

                        if (vert2.Classification == BSPClassification.Front)
                        {
                            back.Points.Add(newVert);
                            front.Points.Add(newVert);
                            front.Points.Add(vert2);
                        }
                        else if (vert2.Classification == BSPClassification.Back)
                        {
                            front.Points.Add(newVert);
                            back.Points.Add(newVert);
                            back.Points.Add(vert2);
                        }
                    }
                    else
                    {
                        if (vert2.Classification == BSPClassification.Front)
                        {
                            front.Points.Add(vert2);
                        }
                        else if (vert2.Classification == BSPClassification.Back)
                        {
                            back.Points.Add(vert2);
                        }
                    }
                }
            }
            //TODO: This isn't always accurate at extreme splits. 
            front.Normal = face.Normal;
            back.Normal = face.Normal;
        }
    }
}
