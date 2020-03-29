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

namespace LibDescent.Data
{
    public class VClip
    {
        public Fix play_time;
        public int num_frames;
        public Fix frame_time;
        public int flags;
        public short sound_num;
        public ushort[] frames = new ushort[30];
        public Fix light_value;

        public int ID;

        public void RemapVClip(int firstFrame, PIGFile piggyFile)
        {
            int numFrames = 0;
            int nextFrame = 0;
            PIGImage img = piggyFile.Bitmaps[firstFrame];
            if (img.isAnimated)
            {
                //Clear the old animation
                for (int i = 0; i < 30; i++) frames[i] = 0;

                frames[numFrames] = (ushort)(firstFrame + numFrames);
                img = piggyFile.Bitmaps[firstFrame + numFrames + 1];
                numFrames++;
                while (img.frame == numFrames)
                {
                    if (firstFrame + numFrames + 1 >= piggyFile.Bitmaps.Count) break; 
                    frames[numFrames] = (ushort)(firstFrame + numFrames);
                    img = piggyFile.Bitmaps[firstFrame + numFrames + 1];
                    numFrames++;
                    nextFrame++;
                }
                this.num_frames = numFrames;
            }
            frame_time = play_time / num_frames;
        }
    }
}
