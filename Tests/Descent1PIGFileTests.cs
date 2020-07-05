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
        [Test]
        public void LoadD15PigFile()
        {
            Descent1PIGFile piggie = new Descent1PIGFile();

            using (var file = File.OpenRead(@"D:\GOG Games\Descent\DESCENT.PIG"))
            {
                piggie.Read(file);
            }
        }
    }
}
