using LibDescent.Data;
using NUnit.Framework;
using System.Numerics;

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
    }
}
