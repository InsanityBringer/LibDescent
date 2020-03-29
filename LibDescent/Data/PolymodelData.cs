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
using System.IO;
using System.Numerics;

namespace LibDescent.Data
{
    public class PolymodelPlane
    {
        public FixVector point;
        public FixVector norm;

        public PolymodelPlane(FixVector point, FixVector norm)
        {
            this.point = point;
            this.norm = norm;
        }
    }
    public class PolymodelData
    {
        //int size;
        public byte[] InterpreterData;
        public PolymodelData(int size)
        {
            InterpreterData = new byte[size];
        }

        public void GetSubmodelMinMaxs(int num, Polymodel host)
        {
            MemoryStream ms = new MemoryStream(InterpreterData);
            BinaryReader br = new BinaryReader(ms);
            br.BaseStream.Seek(host.submodels[num].Pointer, SeekOrigin.Begin);
            short opcode = br.ReadInt16();
            FixVector mins = FixVector.FromRawValues(int.MaxValue, int.MaxValue, int.MaxValue);
            FixVector maxs = FixVector.FromRawValues(int.MinValue, int.MinValue, int.MinValue);
            switch (opcode)
            {
                case 1:
                    {
                        short pointc = br.ReadInt16(); //+2
                                                       //data.points = new HAMFile.vms_vector[pointc];
                        for (int x = 0; x < pointc; x++)
                        {
                            FixVector point = new FixVector();
                            point.x = Fix.FromRawValue(br.ReadInt32());
                            point.y = Fix.FromRawValue(br.ReadInt32());
                            point.z = Fix.FromRawValue(br.ReadInt32());

                            mins.x = Math.Min(mins.x, point.x);
                            mins.y = Math.Min(mins.y, point.y);
                            mins.z = Math.Min(mins.z, point.z);
                            maxs.x = Math.Max(maxs.x, point.x);
                            maxs.y = Math.Max(maxs.y, point.y);
                            maxs.z = Math.Max(maxs.z, point.z);
                        }
                    }
                    break;
                case 7:
                    {
                        short pointc = br.ReadInt16();
                        br.ReadInt32();
                        for (int x = 0; x < pointc; x++)
                        {
                            FixVector point = new FixVector();
                            point.x = Fix.FromRawValue(br.ReadInt32());
                            point.y = Fix.FromRawValue(br.ReadInt32());
                            point.z = Fix.FromRawValue(br.ReadInt32());

                            mins.x = Math.Min(mins.x, point.x);
                            mins.y = Math.Min(mins.y, point.y);
                            mins.z = Math.Min(mins.z, point.z);
                            maxs.x = Math.Max(maxs.x, point.x);
                            maxs.y = Math.Max(maxs.y, point.y);
                            maxs.z = Math.Max(maxs.z, point.z);
                        }
                    }
                    break;
            }
            br.Close();
            br.Dispose();

            host.submodels[num].Mins = mins;
            host.submodels[num].Maxs = maxs;
        }

        public void BuildPolymodelData(int num, Polymodel host, string traceto)
        {
            Console.WriteLine("BuildPolymodelData: STUB");
        }

        private void WriteLevel(StreamWriter sw)
        {
            Console.WriteLine("WriteLevel: STUB");
        }

        public void ExecuteInstruction(BinaryReader br, FixVector positionOffset, StreamWriter sw, Polymodel host, int modelnum)
        {
            Console.WriteLine("ExecuteInstruction: STUB");
        }
    }
}
