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
    public class Weapon
    {
        /// <summary>
        /// How to draw 0=crash, 1=blob, 2=object, 3=vclip, 255=invis
        /// </summary>
        public byte render_type;
        /// <summary>
        /// 0 = dies when it hits something, 1 = continues (eg, fusion cannon)
        /// </summary>
        public byte persistent;
        /// <summary>
        /// Model num if rendertype==2.
        /// </summary>
        public short model_num;
        /// <summary>
        /// Model num of inner part if rendertype==2.
        /// </summary>
        public short model_num_inner;
        /// <summary>
        /// What vclip to use for muzzle flash
        /// </summary>
        public sbyte flash_vclip;
        /// <summary>
        /// What vclip for impact with robot
        /// </summary>
        public sbyte robot_hit_vclip;
        /// <summary>
        /// What sound to play when fired
        /// </summary>
        public short flash_sound;
        /// <summary>
        /// What vclip for impact with wall
        /// </summary>
        public sbyte wall_hit_vclip;
        /// <summary>
        /// Number of bursts fired from EACH GUN per firing.  For weapons which fire from both sides, 3*fire_count shots will be fired.
        /// </summary>
        public byte fire_count;
        /// <summary>
        /// What sound for impact with robot
        /// </summary>
        public short robot_hit_sound;
        /// <summary>
        /// How many units of ammunition it uses.
        /// </summary>
        public byte ammo_usage;
        /// <summary>
        /// Vclip to render for the weapon, itself.
        /// </summary>
        public sbyte weapon_vclip;
        /// <summary>
        /// What sound for impact with wall
        /// </summary>
        public short wall_hit_sound;
        /// <summary>
        /// If !0, this weapon can be destroyed by another weapon.
        /// </summary>
        public byte destroyable;
        /// <summary>
        /// Flag: set if this object is matter (as opposed to energy)
        /// </summary>
        public byte matter;
        /// <summary>
        /// 1==always bounces, 2=bounces twice 
        /// </summary>
        public byte bounce;
        /// <summary>
        /// Set if this weapon can home in on a target.
        /// </summary>
        public byte homing_flag;
        /// <summary>
        /// allowed variance in speed below average, /128: 64 = 50% meaning if speed = 100, can be 50..100
        /// </summary>
        public byte speedvar; 
        /// <summary>
        /// In practice this is actually "placeable"
        /// </summary>
        public byte flags;
        /// <summary>
        /// Flash effect
        /// </summary>
        public sbyte flash;
        /// <summary>
        /// Size of blobs in F1_0/16 units.  Player afterburner size = 2.5.
        /// </summary>
        public sbyte afterburner_size;
        /// <summary>
        /// ID of weapon to drop if this contains children.  -1 means no children.
        /// </summary>
        public sbyte children;
        /// <summary>
        /// How much fuel is consumed to fire this weapon.
        /// </summary>
        public Fix energy_usage;
        /// <summary>
        /// Time until this weapon can be fired again.
        /// </summary>
        public Fix fire_wait;
        /// <summary>
        /// Scale damage by this amount when applying to player in multiplayer.  F1_0 means no change.
        /// </summary>
        public Fix multi_damage_scale;
        /// <summary>
        /// Pointer to bitmap if rendertype==0 or 1.
        /// </summary>
        public ushort bitmap;
        /// <summary>
        /// Size of blob if blob type
        /// </summary>
        public Fix blob_size;
        /// <summary>
        /// How big to draw the flash
        /// </summary>
        public Fix flash_size;
        /// <summary>
        /// How big of an impact
        /// </summary>
        public Fix impact_size;
        /// <summary>
        /// How much damage it can inflict
        /// </summary>
        public Fix[] strength = new Fix[5];
        /// <summary>
        /// How fast it can move, difficulty level based.
        /// </summary>
        public Fix[] speed = new Fix[5];
        /// <summary>
        /// How much mass it has
        /// </summary>
        public Fix mass;
        /// <summary>
        /// How much drag it has
        /// </summary>
        public Fix drag;
        /// <summary>
        /// How much thrust it has
        /// </summary>
        public Fix thrust;
        /// <summary>
        /// For polyobjects, the ratio of len/width. (10 maybe?)
        /// </summary>
        public Fix po_len_to_width_ratio;
        /// <summary>
        /// Amount of light this weapon casts.
        /// </summary>
        public Fix light;
        /// <summary>
        /// Lifetime in seconds of this weapon.
        /// </summary>
        public Fix lifetime;
        /// <summary>
        /// Radius of damage caused by weapon, used for missiles (not lasers) to apply to damage to things it did not hit
        /// </summary>
        public Fix damage_radius;
        /// <summary>
        /// a picture of the weapon for the cockpit
        /// </summary>
        public ushort picture;
        /// <summary>
        /// a hires picture of the above
        /// </summary>
        public ushort hires_picture;

        public int ID;

        public Weapon()
        {
            model_num = -1;
            model_num_inner = -1;
            render_type = 3;
            po_len_to_width_ratio = 10;
            speedvar = 128;
            fire_count = 1;
            children = -1;
        }

        public void CopyDataFrom(Weapon other, IElementManager manager)
        {
            render_type = other.render_type;
            model_num = other.model_num;
            model_num_inner = other.model_num_inner;
            flash_vclip = other.flash_vclip;
            robot_hit_vclip = other.robot_hit_vclip;
            flash_sound = other.flash_sound;
            wall_hit_vclip = other.wall_hit_vclip;
            fire_count = other.fire_count;
            robot_hit_sound = other.robot_hit_sound;
            ammo_usage = other.ammo_usage;
            weapon_vclip = other.weapon_vclip;
            wall_hit_sound = other.wall_hit_sound;
            bounce = other.bounce;
            speedvar = other.speedvar;
            flash = other.flash;
            afterburner_size = other.afterburner_size;
            children = other.children;
            energy_usage = other.energy_usage;
            fire_wait = other.fire_wait;
            multi_damage_scale = other.multi_damage_scale;
            destroyable = other.destroyable;
            matter = other.matter;
            homing_flag = other.homing_flag;
            flags = other.flags;
            bitmap = other.bitmap;
            blob_size = other.blob_size;
            flash_size = other.flash_size;
            impact_size = other.impact_size;
            mass = other.mass;
            drag = other.drag;
            thrust = other.thrust;
            po_len_to_width_ratio = other.po_len_to_width_ratio;
            light = other.light;
            lifetime = other.lifetime;
            damage_radius = other.damage_radius;
            picture = other.picture;
            hires_picture = other.hires_picture;
            for (int i = 0; i < 5; i++)
            {
                strength[i] = other.strength[i];
                speed[i] = other.speed[i];
            }
        }

        public void UpdateWeapon(int tag, int value, int index)
        {
            switch (tag)
            {
                case 2:
                    model_num = (short)(value-1);
                    break;
                case 3:
                    model_num_inner = (short)(value-1);
                    break;
                case 4:
                    flash_vclip = (sbyte)(value - 1);
                    break;
                case 5:
                    robot_hit_vclip = (sbyte)(value - 1);
                    break;
                case 6:
                    if (value == 0) flash_sound = -1;
                    else flash_sound = (short)(value - 1);
                    break;
                case 7:
                    wall_hit_vclip = (sbyte)(value - 1);
                    break;
                case 8:
                    fire_count = (byte)value;
                    break;
                case 9:
                    if (value == 0) robot_hit_sound = -1;
                    else robot_hit_sound = (short)(value - 1);
                    break;
                case 10:
                    ammo_usage = (byte)value;
                    break;
                case 11:
                    weapon_vclip = (sbyte)(value - 1);
                    break;
                case 12:
                    if (value == 0) wall_hit_sound = -1;
                    else wall_hit_sound = (short)(value - 1);
                    break;
                case 15:
                    bounce = (byte)value;
                    break;
                case 17:
                    speedvar = (byte)value;
                    break;
                case 19:
                    flash = (sbyte)value;
                    break;
                case 20:
                    afterburner_size = (sbyte)value;
                    break;
                case 21:
                    children = (sbyte)(value - 1);
                    break;
                case 22:
                    energy_usage = new Fix(value);
                    break;
                case 23:
                    fire_wait = new Fix(value);
                    break;
                case 24:
                    multi_damage_scale = value;
                    break;
                case 25:
                    bitmap = (ushort)value;
                    break;
                case 26:
                    blob_size = new Fix(value);
                    break;
                case 27:
                    flash_size = new Fix(value);
                    break;
                case 28:
                    impact_size = new Fix(value);
                    break;
                case 29:
                    strength[index] = new Fix(value);
                    break;
                case 30:
                    speed[index] = new Fix(value);
                    break;
                case 31:
                    mass = new Fix(value);
                    break;
                case 32:
                    drag = new Fix(value);
                    break;
                case 33:
                    thrust = new Fix(value);
                    break;
                case 34:
                    po_len_to_width_ratio = new Fix(value);
                    break;
                case 35:
                    light = new Fix(value);
                    break;
                case 36:
                    lifetime = new Fix(value);
                    break;
                case 37:
                    picture = (ushort)value;
                    break;
                case 38:
                    hires_picture = (ushort)value;
                    break;
                case 39:
                    damage_radius = new Fix(value);
                    break;
            }
        }
    }
}
