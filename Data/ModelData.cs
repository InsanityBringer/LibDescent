using System;
using System.Collections.Generic;
using System.Text;

namespace LibDescent.Data
{

    public class ModelData
    {
        public List<BSPFace> Triangles { get; private set; }

        public FixVector modelOffset = new FixVector();

        public ModelData()
        {
            Triangles = new List<BSPFace>();
        }
    }
}
