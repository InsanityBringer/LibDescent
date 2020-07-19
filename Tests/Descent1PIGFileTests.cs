using LibDescent.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibDescent.Tests
{
    public class Descent1PIGFileTests
    {
        const string PigFileLocation = @"D:\GOG Games\Descent\rDESCENT.PIG";

        [Test]
        public void Jaap()
        {
            // Setup
            var modelToLoad = @"D:\GOG Games\Descent\aaa.pof";
            var model = LibDescent.Data.POFReader.ReadPOFFile(modelToLoad);
            model.ExpandSubmodels();

            // Rebuild
            PolymodelBuilder polymodelBuilder = new PolymodelBuilder();
            polymodelBuilder.RebuildModel(model);

            // Store
            Descent1PIGFile piggy = new Descent1PIGFile();


            using (var readFile = File.OpenRead(@"D:\GOG Games\Descent\RDESCENT.PIG"))
            {

                piggy.Read(readFile);
            }

            piggy.Models[5] = model;
            piggy.Models[7] = model;

            using (var readFile = File.OpenWrite(@"D:\GOG Games\Descent\DESCENT.PIG"))
            {

                piggy.Write(readFile);
            }
        }

        [Test]
        [Ignore("Requires real Descent 1 (1.5) pig file")]
        public void LoadD15PigFile()
        {
            Descent1PIGFile piggie = new Descent1PIGFile();

            using (var file = File.OpenRead(PigFileLocation))
            {
                piggie.Read(file);
            }
        }

        [Test]
        [Ignore("Requires real Descent 1 (1.5) pig file")]
        public void SaveD15PigFile()
        {
            Descent1PIGFile piggie = new Descent1PIGFile();

            using (var file = File.OpenRead(PigFileLocation))
            {
                piggie.Read(file);
            }

            byte[] newFileBytes = null;

            //using (var newFile = File.OpenWrite(@"D:\GOG Games\Descent\DESCENT.PIG")) {  piggie.Write(newFile); }

            using (MemoryStream ms = new MemoryStream())
            {
                piggie.Write(ms);

                // Now compare the bytes
                ms.Position = 0;
                newFileBytes = ms.ToArray();
            }

            var realBytes = File.ReadAllBytes(PigFileLocation);

            Assert.AreEqual(realBytes.Length, newFileBytes.Length);

            Assert.AreEqual(realBytes, newFileBytes);

        }

        [Test]
        [Ignore("This just swaps a model")]
        public void SwapModelsTest()
        {
            Descent1PIGFile piggie = new Descent1PIGFile();

            using (var file = File.OpenRead(PigFileLocation))
            {

                piggie.Read(file);
            }

            piggie.Models[5] = piggie.Models[2];

            using (var newFile = File.OpenWrite(@"D:\GOG Games\Descent\DESCENT.PIG")) { piggie.Write(newFile); }

        }
    }
}