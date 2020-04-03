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

using System.IO;

namespace LibDescent.Data
{
    public class POFReader
    {
        public static Polymodel ReadPOFFile(string filename, string traceto)
        {
            BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open));
            Polymodel model = new Polymodel();

            int sig = br.ReadInt32();
            short ver = br.ReadInt16();

            int chunk = br.ReadInt32();
            int datasize = br.ReadInt32();

            long dest = br.BaseStream.Position + datasize;

            while (true)
            {
                //1096041545
                switch (chunk)
                {
                    //TXTR
                    case 1381259348:
                        {
                            short texcount = br.ReadInt16();
                            model.n_textures = (byte)texcount;
                            for (int x = 0; x < texcount; x++)
                            {
                                char[] texchars = new char[128];
                                texchars[0] = (char)br.ReadByte();
                                int i = 1;
                                while (texchars[i-1] != '\0')
                                {
                                    texchars[i] = (char)br.ReadByte();
                                    i++;
                                }
                                string name = new string(texchars);
                                name = name.Trim(' ', '\0');
                                model.textureList.Add(name);
                            }
                        }
                        break;
                    //OHDR
                    case 1380206671:
                        {
                            model.n_models = br.ReadInt32();
                            model.rad = new Fix(br.ReadInt32());
                            model.mins = ReadVector(br);
                            model.maxs = ReadVector(br);
                            for (int i = 0; i < model.n_models; i++)
                            {
                                model.submodels.Add(new Submodel());
                            }
                        }
                        break;
                    //SOBJ
                    case 1245859667:
                        {
                            short modelnum = br.ReadInt16();
                            Submodel submodel = model.submodels[modelnum];
                            submodel.ID = modelnum;
                            short parentTest = br.ReadInt16();
                            submodel.Parent = (byte)parentTest;
                            submodel.Normal = ReadVector(br);
                            submodel.Point = ReadVector(br);
                            submodel.Offset = ReadVector(br);
                            if (ver > 8)
                            {
                                submodel.Mins = ReadVector(br);
                                submodel.Maxs = ReadVector(br);
                            }
                            submodel.Radius = new Fix(br.ReadInt32());
                            submodel.Pointer = br.ReadInt32();
                            model.submodels.Add(submodel);
                            if (submodel.Parent != 255)
                            {
                                model.submodels[submodel.Parent].Children.Add(submodel);
                            }
                        }
                        break;
                    //GUNS
                    case 0x534E5547:
                        {
                            int numGuns = br.ReadInt32();
                            model.numGuns = numGuns;
                            for (int i = 0; i < numGuns; i++)
                            {
                                short id = br.ReadInt16();
                                model.gunSubmodels[id] = br.ReadInt16();
                                model.gunPoints[id] = ReadVector(br);
                                model.gunDirs[id] = ReadVector(br);
                            }
                        }
                        break;
                    //ANIM
                    case 1296649793:
                        {
                            model.isAnimated = true;
                            //br.ReadBytes(datasize);
                            int numFrames = br.ReadInt16();
                            for (int submodel = 0; submodel < model.n_models; submodel++)
                            {
                                for (int i = 0; i < numFrames; i++)
                                {
                                    model.animationMatrix[submodel, i].p = br.ReadInt16();
                                    model.animationMatrix[submodel, i].b = br.ReadInt16();
                                    model.animationMatrix[submodel, i].h = br.ReadInt16();
                                }
                            }
                        }
                        break;
                    //IDTA
                    case 1096041545:
                        {
                            model.model_data_size = datasize;
                            PolymodelData data = new PolymodelData(datasize);
                            data.InterpreterData = br.ReadBytes(datasize);
                            model.data = data;
                        }
                        break;
                    default:
                        br.ReadBytes(datasize);
                        break;
                }
                //Maintain 4-byte alignment
                if (ver >= 8)
                {
                    br.BaseStream.Seek(dest, SeekOrigin.Begin);
                }
                if (br.BaseStream.Position >= br.BaseStream.Length)
                    break;
                chunk = br.ReadInt32();
                datasize = br.ReadInt32();
                dest = br.BaseStream.Position + datasize;
            }
            if (ver < 9)
            {
                for (int i = 0; i < model.n_models; i++)
                {
                    model.data.GetSubmodelMinMaxs(i, model);
                }
            }

            br.Close();
            return model;
        }

        private static FixVector ReadVector(BinaryReader br)
        {
            FixVector vec = new FixVector();

            vec.x = new Fix(br.ReadInt32());
            vec.y = new Fix(br.ReadInt32());
            vec.z = new Fix(br.ReadInt32());

            return vec;
        }
    }
}
