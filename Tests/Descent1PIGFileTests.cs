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
            StringBuilder readLog = new StringBuilder();
            StringBuilder writeLog = new StringBuilder();

            using (var file = File.OpenRead(PigFileLocation))
            {

                piggie.Read(file, readLog);
            }

            byte[] newFileBytes = null;


            using (var newFile = File.OpenWrite(@"D:\GOG Games\Descent\DESCENT.PIG")) {  piggie.Write(newFile); }

            using (MemoryStream ms = new MemoryStream())
            {
                piggie.Write(ms, writeLog);

                // Now compare the bytes
                ms.Position = 0;
                newFileBytes = ms.ToArray();
            }

            // compare read log and write log
            string read = readLog.ToString();
            string write = writeLog.ToString();

            // they should be the same
            Assert.AreEqual(read, write);


            var realBytes = File.ReadAllBytes(@"D:\GOG Games\Descent\RDESCENT.PIG");

            Assert.AreEqual(realBytes.Length, newFileBytes.Length);

            Assert.AreEqual(realBytes, newFileBytes);

        }


        [Test]
        public void SwapModelsTest()
        {
            Descent1PIGFile piggie = new Descent1PIGFile();
            StringBuilder readLog = new StringBuilder();
            StringBuilder writeLog = new StringBuilder();

            using (var file = File.OpenRead(PigFileLocation))
            {

                piggie.Read(file, readLog);
            }

            piggie.Models[5] = piggie.Models[2];

            using (var newFile = File.OpenWrite(@"D:\GOG Games\Descent\DESCENT.PIG")) { piggie.Write(newFile); }

        }
    }
}