using LibDescent.Data;
using NUnit.Framework;
using System.Numerics;
using System.IO;

namespace LibDescent.Tests
{
    public class BSPTreeTests
    {
        [Test]
        public void ClassifyFaceFrontTest()
        {
            // Arrange
            BSPFace face = new BSPFace();

            face.Points.Add(new BSPVertex { Point = new Vector3(-1.0f, 2.0f, 0.0f) });
            face.Points.Add(new BSPVertex { Point = new Vector3(0.0f, 2.0f, 1.0f) });
            face.Points.Add(new BSPVertex { Point = new Vector3(1.0f, 2.0f, 0.0f) });
            face.Normal = new Vector3(0.0f, 1.0f, 0.0f);


            Vector3 planePoint = new Vector3(0.0f, 1.0f, 0.0f); // Just a basic plane
            Vector3 planeNormal = new Vector3(0.0f, 1.0f, 0.0f);

            BSPTree tree = new BSPTree();

            // Act
            tree.ClassifyFace(face, planePoint, planeNormal);

            // Assert
            Assert.AreEqual(BSPClassification.Front, face.Classification);
        }

        [Test]
        public void ClassifyFaceBackTest()
        {
            // Arrange
            BSPFace face = new BSPFace();

            face.Points.Add(new BSPVertex { Point = new Vector3(-1.0f, -2.0f, 0.0f) });
            face.Points.Add(new BSPVertex { Point = new Vector3(0.0f, -2.0f, 1.0f) });
            face.Points.Add(new BSPVertex { Point = new Vector3(1.0f, -2.0f, 0.0f) });
            face.Normal = new Vector3(0.0f, 1.0f, 0.0f);


            Vector3 planePoint = new Vector3(0.0f, 1.0f, 0.0f); // Just a basic plane
            Vector3 planeNormal = new Vector3(0.0f, 1.0f, 0.0f);

            BSPTree tree = new BSPTree();

            // Act
            tree.ClassifyFace(face, planePoint, planeNormal);

            // Assert
            Assert.AreEqual(BSPClassification.Back, face.Classification);
        }

        [Test]
        public void ClassifyFaceSpanTest()
        {
            // Arrange
            BSPFace face = new BSPFace();

            face.Points.Add(new BSPVertex { Point = new Vector3(-1.0f, -2.0f, 0.0f) });
            face.Points.Add(new BSPVertex { Point = new Vector3(0.0f, 2.0f, 0.0f) });
            face.Points.Add(new BSPVertex { Point = new Vector3(1.0f, -2.0f, 0.0f) });
            face.Normal = new Vector3(0.0f, 0.0f, 1.0f);


            Vector3 planePoint = new Vector3(0.0f, 1.0f, 0.0f); // Just a basic plane
            Vector3 planeNormal = new Vector3(0.0f, 1.0f, 0.0f);

            BSPTree tree = new BSPTree();

            // Act
            tree.ClassifyFace(face, planePoint, planeNormal);

            // Assert
            Assert.AreEqual(BSPClassification.Spanning, face.Classification);
        }

        [Test]
        public void ClassifyFaceCoPlanarFrontTest()
        {
            // Arrange
            BSPFace face = new BSPFace();

            face.Points.Add(new BSPVertex { Point = new Vector3(-1.0f, 1.0f, 0.0f) });
            face.Points.Add(new BSPVertex { Point = new Vector3(0.0f, 1.0f, 1.0f) });
            face.Points.Add(new BSPVertex { Point = new Vector3(1.0f, 1.0f, 0.0f) });
            face.Normal = new Vector3(0.0f, 1.0f, 0.0f);


            Vector3 planePoint = new Vector3(0.0f, 1.0f, 0.0f); // Just a basic plane
            Vector3 planeNormal = new Vector3(0.0f, 1.0f, 0.0f);

            BSPTree tree = new BSPTree();

            // Act
            tree.ClassifyFace(face, planePoint, planeNormal);

            // Assert
            Assert.AreEqual(BSPClassification.Front, face.Classification);
        }

        [Test]
        public void ClassifyFaceCoPlanarBackTest()
        {
            // Arrange
            BSPFace face = new BSPFace();

            face.Points.Add(new BSPVertex { Point = new Vector3(-1.0f, 1.0f, 0.0f) });
            face.Points.Add(new BSPVertex { Point = new Vector3(0.0f, 1.0f, 1.0f) });
            face.Points.Add(new BSPVertex { Point = new Vector3(1.0f, 1.0f, 0.0f) });
            face.Normal = new Vector3(0.0f, 1.0f, 0.0f);


            Vector3 planePoint = new Vector3(0.0f, 1.0f, 0.0f); // Just a basic plane
            Vector3 planeNormal = new Vector3(0.0f, -1.0f, 0.0f);

            BSPTree tree = new BSPTree();

            // Act
            tree.ClassifyFace(face, planePoint, planeNormal);

            // Assert
            Assert.AreEqual(BSPClassification.Back, face.Classification);
        }

        [Test]
        public void GenerateBSPTest() //note: this mostly just tests ATM that the code executes without error. 
            //Proper results currently need to be visually validated with a tool like Descent 2 Workshop
        {
            Polymodel model = LibDescent.Data.POFReader.ReadPOFFile(TestUtils.GetResourceStream("NewConcussion.pof"));
            model.ExpandSubmodels();

            //Build the BSP tree
            PolymodelBuilder polymodelBuilder = new PolymodelBuilder();
            polymodelBuilder.RebuildModel(model);

            BinaryWriter bw = new BinaryWriter(File.Open("NewConcussionBSP.pof", FileMode.Create));
            //the POFWriter API needs some changes...
            POFWriter.SerializePolymodel(bw, model, 8);
            bw.Flush();
            bw.Close();
            bw.Dispose();
        }
    }
}
