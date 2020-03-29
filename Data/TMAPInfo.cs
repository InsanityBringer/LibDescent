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
    public class TMAPInfo
    {
        public const int TMI_VOLATILE = 1;//this material blows up when hit
        public const int TMI_WATER = 2;	//this material is water
        public const int TMI_FORCE_FIELD = 4;		//this is force field - flares don't stick
        public const int TMI_GOAL_BLUE = 8;	//this is used to remap the blue goal
        public const int TMI_GOAL_RED = 16;	//this is used to remap the red goal
        public const int TMI_GOAL_HOARD = 32;		//this is used to remap the goals
        public byte flags;
        //three bytes padding
        public Fix lighting;
        public Fix damage;
        public short eclip_num;
        public short destroyed;
        public short slide_u, slide_v;
        public ushort texture;
        public int ID;

        //Descent 1 extra data
        public byte[] filename = new byte[13];

        public void updateFlags(int flag, bool set)
        {
            int flagvalue = 1 << flag;
            if (set)
            {
                if ((flags & flagvalue) == 0)
                {
                    flags |= (byte)flagvalue;
                }
            }
            else
            {
                if ((flags & flagvalue) != 0)
                {
                    flags -= (byte)flagvalue;
                }
            }
        }
    }
}
