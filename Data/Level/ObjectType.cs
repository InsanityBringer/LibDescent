/*
    Copyright (c) 2025 The LibDescent Team

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

namespace LibDescent.Data
{
    public enum ObjectTypeID : sbyte
    {
        None = -1,
        Wall = 0,
        Fireball,
        Robot,
        Hostage,
        Player,
        Weapon,
        Camera,
        Powerup,
        Debris,
        ControlCenter,
        Flare,
        Clutter,
        Ghost,
        Light,
        Coop,
        Marker, // Descent 2
        Cambot, // D2X-XL
        Monsterball, // D2X-XL
        Smoke, // D2X-XL
        Explosion, // D2X-XL
        Effect, // D2X-XL
    }

    public interface IObjectType
    {
        ObjectTypeID Identifier { get; }
    }

    public interface ID1ObjectType : IObjectType { }
    public interface ID2ObjectType : IObjectType { }
    public interface ID2XXLObjectType : IObjectType { }

    public class WallObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Wall;
    }

    public class FireballObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Fireball;
    }

    public class RobotObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Robot;
    }

    public class HostageObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Hostage;
    }

    public class PlayerObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Player;
    }

    public class WeaponObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Weapon;
    }

    public class CameraObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Camera;
    }

    public class PowerupObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Powerup;
    }

    public class DebrisObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Debris;
    }

    public class ControlCenterObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.ControlCenter;
    }

    public class FlareObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Flare;
    }

    public class ClutterObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Clutter;
    }

    public class GhostObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Ghost;
    }

    public class LightObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Light;
    }

    public class CoopObjectType : ID1ObjectType, ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Coop;
    }

    public class MarkerObjectType : ID2ObjectType, ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Marker;
    }

    public class CambotObjectType : ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Cambot;
    }

    public class MonsterballObjectType : ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Monsterball;
    }

    public class SmokeObjectType : ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Smoke;
    }

    public class ExplosionObjectType : ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Explosion;
    }

    public class EffectObjectType : ID2XXLObjectType
    {
        public ObjectTypeID Identifier => ObjectTypeID.Effect;
    }

    public static class ObjectTypeFactory
    {
        public static IObjectType Create(ObjectTypeID id)
        {
            switch (id)
            {
                case ObjectTypeID.None:
                    return null;
                default:
                    var type = Type.GetType($"LibDescent.Data.{id}ObjectType");
                    if (type != null)
                        return (IObjectType)Activator.CreateInstance(type);
                    break;
            }
            throw new ArgumentException("ObjectTypeFactory::Create: bad object type");
        }
    }
}