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
    public class EClip
    {
        public int ID; //Needed for convenience
        public VClip vc = new VClip();				//imbedded vclip
        public int time_left;		//for sequencing
        public int frame_count;	//for sequencing
        public short changing_wall_texture;			//Which element of Textures array to replace.
        public short changing_object_texture;		//Which element of ObjBitmapPtrs array to replace.
        public int flags;			//see above
        public int crit_clip;		//use this clip instead of above one when mine critical
        public int dest_bm_num;	//use this bitmap when monitor destroyed
        public int dest_vclip;		//what vclip to play when exploding
        public int dest_eclip;		//what eclip to play when exploding
        public Fix dest_size;		//3d size of explosion
        public int sound_num;		//what sound this makes
        public int segnum, sidenum;	//what seg & side, for one-shot clips

        public EClip()
        {
            dest_vclip = -1;
            dest_eclip = -1;
            crit_clip = -1;
            dest_bm_num = -1;
            sound_num = -1;
        }

        public int GetCurrentTMap()
        {
            return changing_wall_texture;
        }
    }
}
