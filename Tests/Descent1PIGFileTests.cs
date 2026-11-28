using LibDescent.Data;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace LibDescent.Tests
{
    public class Descent1PIGFileTests
    {
        [Test]
        public void LoadD15PigFile()
        {
            Descent1PIGFile piggie = new();
            using FileStream file = File.OpenRead(TestUtils.RequireD1Pig());
            piggie.Read(file);
        }

        [Test]
        public void SaveD15PigFile()
        {
            Descent1PIGFile piggie = new();

            string path = TestUtils.RequireD1Pig();
            using (FileStream file = File.OpenRead(path))
            {
                piggie.Read(file);
            }

            byte[] newFileBytes = null;
            using (MemoryStream ms = new())
            {
                piggie.Write(ms);

                // Now compare the bytes
                ms.Position = 0;
                newFileBytes = ms.ToArray();
            }

            byte[] realBytes = File.ReadAllBytes(path);
            Assert.AreEqual(realBytes.Length, newFileBytes.Length);
            Assert.AreEqual(realBytes, newFileBytes);
        }

        [Test]
        [Explicit("This just swaps a model")]
        public void SwapModelsTest()
        {
            Descent1PIGFile piggie = new();

            string path = TestUtils.RequireD1Pig();
            string outputPath = TestUtils.RequireD1OutputPig();

            using (FileStream file = File.OpenRead(path))
            {
                piggie.Read(file);
            }

            piggie.Models[5] = piggie.Models[2];

            using (FileStream newFile = File.OpenWrite(outputPath))
            {
                piggie.Write(newFile);
            }
        }

        /// <summary>
        /// Similar to <see cref="BSPTreeTests.GenerateBSPTest"/> except it writes the updated model to a PIG file
        /// then verifies the write was correct.
        /// </summary>
        [Test]
        public void RebuildModelRoundTrip()
        {
            string pigPath = TestUtils.RequireD1Pig();
            Descent1PIGFile pig = new();
            using (FileStream stream = new(pigPath, FileMode.Open, FileAccess.Read))
            {
                pig.Read(stream);
            }
            // Separate unmodified copy to compare models against
            Descent1PIGFile original = new();
            using (FileStream stream = new(pigPath, FileMode.Open, FileAccess.Read))
            {
                original.Read(stream);
            }
            int originalModelCount = original.numModels;

            // Rebuild a model from a POF file
            Polymodel model = POFReader.ReadPOFFile(TestUtils.GetResourceStream("NewConcussion.pof"));
            model.ExpandSubmodels();
            PolymodelBuilder polymodelBuilder = new();
            polymodelBuilder.RebuildModel(model);

            // Replace, write pig, reload pig
            const int replacedIndex = 5;
            pig.Models[replacedIndex] = model;
            Descent1PIGFile reloaded = new();
            using (MemoryStream ms = new())
            {
                pig.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                reloaded.Read(ms);
            }

            Assert.AreEqual(originalModelCount, reloaded.numModels);
            // Make sure the replaced model was written correctly
            AssertPigModelsEqual(model, reloaded.Models[replacedIndex], $"Replaced model {replacedIndex}");

            // Make sure neighboring models were not corrupted
            foreach (int i in new[] { replacedIndex - 1, replacedIndex + 1 })
            {
                AssertPigModelsEqual(original.Models[i], reloaded.Models[i], $"Neighboring model {i}");
            }
        }

        /// <summary>
        /// Helper function that compares two polymodels and checks that they are equal.
        /// (Some submodel fields are skipped for brevity.)
        /// </summary>
        private static void AssertPigModelsEqual(Polymodel expected, Polymodel actual, string label)
        {
            void AssertFixVectorsEqual(FixVector expected, FixVector actual, string label)
            {
                Assert.AreEqual(expected.X.value, actual.X.value, $"{label}.X");
                Assert.AreEqual(expected.Y.value, actual.Y.value, $"{label}.Y");
                Assert.AreEqual(expected.Z.value, actual.Z.value, $"{label}.Z");
            }

            Assert.AreEqual(expected.NumSubmodels, actual.NumSubmodels, $"{label}: NumSubmodels");
            Assert.AreEqual(expected.ModelIDTASize, actual.ModelIDTASize, $"{label}: ModelIDTASize");
            Assert.AreEqual(expected.ModelIDTAPointer, actual.ModelIDTAPointer, $"{label}: ModelIDTAPointer");
            for (int s = 0; s < Polymodel.MaxSubmodels; s++)
            {
                Submodel e = expected.Submodels[s];
                Submodel a = actual.Submodels[s];
                string sub = $"{label}: Submodels[{s}]";
                Assert.AreEqual(e.Pointer, a.Pointer, $"{sub}.Pointer");
                AssertFixVectorsEqual(e.Offset, a.Offset, $"{sub}.Offset");
                AssertFixVectorsEqual(e.Normal, a.Normal, $"{sub}.Normal");
                AssertFixVectorsEqual(e.Point, a.Point, $"{sub}.Point");
                Assert.AreEqual(e.Radius.value, a.Radius.value, $"{sub}.Radius");
                Assert.AreEqual(e.Parent, a.Parent, $"{sub}.Parent");
                AssertFixVectorsEqual(e.Mins, a.Mins, $"{sub}.Mins");
                AssertFixVectorsEqual(e.Maxs, a.Maxs, $"{sub}.Maxs");
            }
            AssertFixVectorsEqual(expected.Mins, actual.Mins, $"{label}: Mins");
            AssertFixVectorsEqual(expected.Maxs, actual.Maxs, $"{label}: Maxs");
            Assert.AreEqual(expected.Radius.value, actual.Radius.value, $"{label}: Radius");
            Assert.AreEqual(expected.NumTextures, actual.NumTextures, $"{label}: NumTextures");
            Assert.AreEqual(expected.FirstTexture, actual.FirstTexture, $"{label}: FirstTexture");
            Assert.AreEqual(expected.SimplerModels, actual.SimplerModels, $"{label}: SimplerModels");
            Assert.AreEqual(expected.InterpreterData, actual.InterpreterData, $"{label}: InterpreterData");
            Assert.AreEqual(expected.DyingModelnum, actual.DyingModelnum, $"{label}: DyingModelnum");
            Assert.AreEqual(expected.DeadModelnum, actual.DeadModelnum, $"{label}: DeadModelnum");
        }
    }
}
