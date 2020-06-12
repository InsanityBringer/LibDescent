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
    public class POFWriter
    {
        public static void SerializePolymodel(BinaryWriter bw, Polymodel model, short version)
        {
            bw.Write(0x4F505350);
            bw.Write(version);
            if (model.NumTextures > 0)
                SerializeTextures(bw, model, version);
            SerializeObject(bw, model, version);
            for (int i = 0; i < model.NumSubmodels; i++)
                SerializeSubobject(bw, i, model.Submodels[i], version);
            if (model.numGuns > 0)
                SerializeGuns(bw, model, version);
            if (model.isAnimated)
                SerializeAnim(bw, model, version);
            SerializeIDTA(bw, model, version);
        }

        private static void SerializeTextures(BinaryWriter bw, Polymodel model, short version)
        {
            int size = 2;
            int padBytes = 0;
            foreach (string texture in model.TextureList)
            {
                size += texture.Length + 1;
            }
            if (version >= 8)
            {
                padBytes = 4 - (((int)bw.BaseStream.Position + size + 8) % 4);
                if (padBytes == 4) padBytes = 0;
                size += padBytes;
            }
            bw.Write(0x52545854);
            bw.Write(size);
            bw.Write((short)model.TextureList.Count);
            foreach (string texture in model.TextureList)
            {
                size += texture.Length + 1;
                for (int i = 0; i < texture.Length; i++)
                {
                    bw.Write((byte)texture[i]);
                }
                bw.Write((byte)0);
            }
            for (int i = 0; i < padBytes; i++)
                bw.Write((byte)0);
        }

        private static void SerializeObject(BinaryWriter bw, Polymodel model, short version)
        {
            int size = 32;
            int padBytes = 0;
            if (version >= 8)
            {
                padBytes = 4 - (((int)bw.BaseStream.Position + size + 8) % 4);
                if (padBytes == 4) padBytes = 0;
                size += padBytes;
            }
            bw.Write(0x5244484F);
            bw.Write(size);
            bw.Write(model.NumSubmodels);
            bw.Write(model.Radius.value);
            bw.Write(model.Mins.x.value);
            bw.Write(model.Mins.y.value);
            bw.Write(model.Mins.z.value);
            bw.Write(model.Maxs.x.value);
            bw.Write(model.Maxs.y.value);
            bw.Write(model.Maxs.z.value);
            for (int i = 0; i < padBytes; i++)
                bw.Write((byte)0);
        }

        private static void SerializeSubobject(BinaryWriter bw, int id, Submodel model, short version)
        {
            bw.Write(0x4A424F53);
            bw.Write(48);
            bw.Write((short)id);
            if (model.Parent == 255)
                bw.Write((short)-1);
            else
                bw.Write((short)model.Parent);
            bw.Write(model.Normal.x.value);
            bw.Write(model.Normal.y.value);
            bw.Write(model.Normal.z.value);
            bw.Write(model.Point.x.value);
            bw.Write(model.Point.y.value);
            bw.Write(model.Point.z.value);
            bw.Write(model.Offset.x.value);
            bw.Write(model.Offset.y.value);
            bw.Write(model.Offset.z.value);
            bw.Write(model.Radius.value);
            bw.Write(model.Pointer);
        }

        private static void SerializeGuns(BinaryWriter bw, Polymodel model, short version)
        {
            int size;
            if (version >= 7)
                size = (model.numGuns * 28) + 4;
            else
                size = (model.numGuns * 16) + 4;
            bw.Write(0x534E5547);
            bw.Write(size);
            bw.Write(model.numGuns);
            for (int i = 0; i < model.numGuns; i++)
            {
                bw.Write((short)i);
                bw.Write((short)model.gunSubmodels[i]);
                bw.Write(model.gunPoints[i].x.value);
                bw.Write(model.gunPoints[i].y.value);
                bw.Write(model.gunPoints[i].z.value);
                if (version >= 7)
                {
                    bw.Write(model.gunDirs[i].x.value);
                    bw.Write(model.gunDirs[i].y.value);
                    bw.Write(model.gunDirs[i].z.value);
                }
            }
        }

        private static void SerializeAnim(BinaryWriter bw, Polymodel model, short version)
        {
            int size = 2 + 6 * model.NumSubmodels * Robot.NumAnimationStates;
            int padBytes = 0;
            if (version >= 8)
            {
                padBytes = 4 - (((int)bw.BaseStream.Position + size + 8) % 4);
                if (padBytes == 4) padBytes = 0;
                size += padBytes;
            }
            bw.Write(0x4D494E41);
            bw.Write(size);
            bw.Write((short)Robot.NumAnimationStates);
            for (int i = 0; i < model.NumSubmodels; i++)
            {
                for (int f = 0; f < Robot.NumAnimationStates; f++)
                {
                    bw.Write(model.animationMatrix[i, f].p);
                    bw.Write(model.animationMatrix[i, f].b);
                    bw.Write(model.animationMatrix[i, f].h);
                }
            }

            for (int i = 0; i < padBytes; i++)
                bw.Write((byte)0);
        }

        private static void SerializeIDTA(BinaryWriter bw, Polymodel model, short version)
        {
            int size = model.ModelIDTASize;
            int padBytes = 0;
            if (version >= 8)
            {
                padBytes = 4 - (((int)bw.BaseStream.Position + size + 8) % 4);
                if (padBytes == 4) padBytes = 0;
                size += padBytes;
            }
            bw.Write(0x41544449);
            bw.Write(size);
            bw.Write(model.InterpreterData);

            for (int i = 0; i < padBytes; i++)
                bw.Write((byte)0);
        }
    }
}
