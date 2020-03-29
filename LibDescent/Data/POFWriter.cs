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
            if (model.n_textures > 0)
                SerializeTextures(bw, model, version);
            SerializeObject(bw, model, version);
            for (int i = 0; i < model.n_models; i++)
                SerializeSubobject(bw, i, model.submodels[i], version);
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
            foreach (string texture in model.textureList)
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
            bw.Write((short)model.textureList.Count);
            foreach (string texture in model.textureList)
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
            bw.Write(model.n_models);
            bw.Write(model.rad.GetRawValue());
            bw.Write(model.mins.x.GetRawValue());
            bw.Write(model.mins.y.GetRawValue());
            bw.Write(model.mins.z.GetRawValue());
            bw.Write(model.maxs.x.GetRawValue());
            bw.Write(model.maxs.y.GetRawValue());
            bw.Write(model.maxs.z.GetRawValue());
            for (int i = 0; i < padBytes; i++)
                bw.Write((byte)0);
        }

        private static void SerializeSubobject(BinaryWriter bw, int id, Submodel model, short version)
        {
            bw.Write(0x4A424F53);
            if (version >= 9)
                bw.Write(72);
            else bw.Write(48);
            bw.Write((short)id);
            if (model.Parent == 255)
                bw.Write((short)-1);
            else
                bw.Write((short)model.Parent);
            bw.Write(model.Normal.x.GetRawValue());
            bw.Write(model.Normal.y.GetRawValue());
            bw.Write(model.Normal.z.GetRawValue());
            bw.Write(model.Point.x.GetRawValue());
            bw.Write(model.Point.y.GetRawValue());
            bw.Write(model.Point.z.GetRawValue());
            bw.Write(model.Offset.x.GetRawValue());
            bw.Write(model.Offset.y.GetRawValue());
            bw.Write(model.Offset.z.GetRawValue());
            if (version >= 9)
            {
                bw.Write(model.Mins.x.GetRawValue());
                bw.Write(model.Mins.y.GetRawValue());
                bw.Write(model.Mins.z.GetRawValue());
                bw.Write(model.Maxs.x.GetRawValue());
                bw.Write(model.Maxs.y.GetRawValue());
                bw.Write(model.Maxs.z.GetRawValue());
            }
            bw.Write(model.Radius.GetRawValue());
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
                bw.Write(model.gunPoints[i].x.GetRawValue());
                bw.Write(model.gunPoints[i].y.GetRawValue());
                bw.Write(model.gunPoints[i].z.GetRawValue());
                if (version >= 7)
                {
                    bw.Write(model.gunDirs[i].x.GetRawValue());
                    bw.Write(model.gunDirs[i].y.GetRawValue());
                    bw.Write(model.gunDirs[i].z.GetRawValue());
                }
            }
        }

        private static void SerializeAnim(BinaryWriter bw, Polymodel model, short version)
        {
            int size = 2 + 6 * model.n_models * Robot.NUM_ANIMATION_STATES;
            int padBytes = 0;
            if (version >= 8)
            {
                padBytes = 4 - (((int)bw.BaseStream.Position + size + 8) % 4);
                if (padBytes == 4) padBytes = 0;
                size += padBytes;
            }
            bw.Write(0x4D494E41);
            bw.Write(size);
            bw.Write((short)Robot.NUM_ANIMATION_STATES);
            for (int i = 0; i < model.n_models; i++)
            {
                for (int f = 0; f < Robot.NUM_ANIMATION_STATES; f++)
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
            int size = model.model_data_size;
            int padBytes = 0;
            if (version >= 8)
            {
                padBytes = 4 - (((int)bw.BaseStream.Position + size + 8) % 4);
                if (padBytes == 4) padBytes = 0;
                size += padBytes;
            }
            bw.Write(0x41544449);
            bw.Write(size);
            bw.Write(model.data.InterpreterData);

            for (int i = 0; i < padBytes; i++)
                bw.Write((byte)0);
        }
    }
}
