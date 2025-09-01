/*
    Copyright (c) 2019 The LibDescent Team

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
    public interface ILevelObject
    {
        /// <summary>
        /// The object's basic type.
        /// </summary>
        IObjectType Type { get; set; }
        /// <summary>
        /// The ID of the object's subtype. Meaning is based on the value of Type.
        /// </summary>
        byte SubtypeID { get; set; }
        /// <summary>
        /// Misc object flags. Not generally useful in level files.
        /// </summary>
        byte Flags { get; set; }
        /// <summary>
        /// The segment containing this object.
        /// </summary>
        Segment Segment { get; set; }
        /// <summary>
        /// Number of the object this is attached to. Only useful for Fireball objects.
        /// </summary>
        short AttachedObject { get; set; }
        /// <summary>
        /// The current position of the object.
        /// </summary>
        FixVector Position { get; set; }
        /// <summary>
        /// The current orientation of the object.
        /// </summary>
        FixMatrix Orientation { get; set; }
        /// <summary>
        /// Radius of the object's collision sphere, in map units.
        /// </summary>
        Fix Size { get; set; }
        /// <summary>
        /// Number of hit points the object has.
        /// </summary>
        Fix Shields { get; set; }
        /// <summary>
        /// The position of the object in the previous frame.
        /// </summary>
        FixVector LastPos { get; set; }
        /// <summary>
        /// The type of the object this object contains. Should either be Powerup (2) or Robot (7) if ContainsCount > 0.
        /// </summary>
        IObjectType ContainsType { get; set; }
        /// <summary>
        /// The ID of the subtype of the object this object contains.
        /// </summary>
        byte ContainsSubtypeID { get; set; }
        /// <summary>
        /// The number of objects contained in this object. Set to 0 to use default drops.
        /// </summary>
        byte ContainsCount { get; set; }
        /// <summary>
        /// The current movement type for this object.
        /// </summary>
        MovementType MoveType { get; set; }
        /// <summary>
        /// The current control type for this object.
        /// </summary>
        ControlType ControlType { get; set; }
        /// <summary>
        /// The current render type for this object.
        /// </summary>
        RenderType RenderType { get; set; }
    }

    internal class LevelObjectCommon
    {
        public byte SubtypeID { get; set; }
        public byte Flags { get; set; }
        public Segment Segment { get; set; }
        public short AttachedObject { get; set; }
        public FixVector Position { get; set; }
        public FixMatrix Orientation { get; set; }
        public Fix Size { get; set; }
        public Fix Shields { get; set; }
        public FixVector LastPos { get; set; }
        public byte ContainsSubtypeID { get; set; }
        public byte ContainsCount { get; set; }
        public MovementType MoveType { get; set; }
        public ControlType ControlType { get; set; }
        public RenderType RenderType { get; set; }
    }

    public class D1LevelObject : ILevelObject
    {
        private readonly LevelObjectCommon _commonData = new LevelObjectCommon();
        private ID1ObjectType _type;
        private ID1ObjectType _containsType;

        #region ILevelObject implementation
        public IObjectType Type
        {
            get => _type;
            set
            {
                if (!(value is ID1ObjectType))
                    throw new ArgumentException("D1LevelObject.Type: value must be a valid Descent object type");
                _type = (ID1ObjectType)value;
            }
        }

        public byte SubtypeID { get => _commonData.SubtypeID; set => _commonData.SubtypeID = value; }
        public byte Flags { get => _commonData.Flags; set => _commonData.Flags = value; }
        public Segment Segment { get => _commonData.Segment; set => _commonData.Segment = value; }
        public short AttachedObject { get => _commonData.AttachedObject; set => _commonData.AttachedObject = value; }
        public FixVector Position { get => _commonData.Position; set => _commonData.Position = value; }
        public FixMatrix Orientation { get => _commonData.Orientation; set => _commonData.Orientation = value; }
        public Fix Size { get => _commonData.Size; set => _commonData.Size = value; }
        public Fix Shields { get => _commonData.Shields; set => _commonData.Shields = value; }
        public FixVector LastPos { get => _commonData.LastPos; set => _commonData.LastPos = value; }

        public IObjectType ContainsType
        {
            get => _containsType;
            set
            {
                if (value is null)
                {
                    _containsType = null;
                    return;
                }
                if (value is ID1ObjectType d1Type)
                {
                    _containsType = d1Type;
                    return;
                }
                throw new ArgumentException("D1LevelObject.ContainsType: value must be a valid Descent object type");
            }
        }

        public byte ContainsSubtypeID { get => _commonData.ContainsSubtypeID; set => _commonData.ContainsSubtypeID = value; }
        public byte ContainsCount { get => _commonData.ContainsCount; set => _commonData.ContainsCount = value; }
        public MovementType MoveType { get => _commonData.MoveType; set => _commonData.MoveType = value; }
        public ControlType ControlType { get => _commonData.ControlType; set => _commonData.ControlType = value; }
        public RenderType RenderType { get => _commonData.RenderType; set => _commonData.RenderType = value; }
        #endregion

        /// <summary>
        /// Gets the numeric ID of the current ControlType.
        /// </summary>
        public ControlTypeID ControlTypeID => ControlType?.Identifier ?? ControlTypeID.None;

        /// <summary>
        /// Gets the numeric ID of the current MoveType.
        /// </summary>
        public MovementTypeID MoveTypeID => MoveType?.Identifier ?? MovementTypeID.None;

        /// <summary>
        /// Gets the numeric ID of the current RenderType.
        /// </summary>
        public RenderTypeID RenderTypeID => RenderType?.Identifier ?? RenderTypeID.None;
    }

    public class D2LevelObject : ILevelObject
    {
        private readonly LevelObjectCommon _commonData = new LevelObjectCommon();
        private ID2ObjectType _type;
        private ID2ObjectType _containsType;

        #region ILevelObject implementation
        public IObjectType Type
        {
            get => _type;
            set
            {
                if (!(value is ID2ObjectType))
                    throw new ArgumentException("D2LevelObject.Type: value must be a valid Descent 2 object type");
                _type = (ID2ObjectType)value;
            }
        }

        public byte SubtypeID { get => _commonData.SubtypeID; set => _commonData.SubtypeID = value; }
        public byte Flags { get => _commonData.Flags; set => _commonData.Flags = value; }
        public Segment Segment { get => _commonData.Segment; set => _commonData.Segment = value; }
        public short AttachedObject { get => _commonData.AttachedObject; set => _commonData.AttachedObject = value; }
        public FixVector Position { get => _commonData.Position; set => _commonData.Position = value; }
        public FixMatrix Orientation { get => _commonData.Orientation; set => _commonData.Orientation = value; }
        public Fix Size { get => _commonData.Size; set => _commonData.Size = value; }
        public Fix Shields { get => _commonData.Shields; set => _commonData.Shields = value; }
        public FixVector LastPos { get => _commonData.LastPos; set => _commonData.LastPos = value; }

        public IObjectType ContainsType
        {
            get => _containsType;
            set
            {
                if (value is null)
                {
                    _containsType = null;
                    return;
                }
                if (value is ID2ObjectType d2Type)
                {
                    _containsType = d2Type;
                    return;
                }
                throw new ArgumentException("D2LevelObject.ContainsType: value must be a valid Descent 2 object type");
            }
        }

        public byte ContainsSubtypeID { get => _commonData.ContainsSubtypeID; set => _commonData.ContainsSubtypeID = value; }
        public byte ContainsCount { get => _commonData.ContainsCount; set => _commonData.ContainsCount = value; }
        public MovementType MoveType { get => _commonData.MoveType; set => _commonData.MoveType = value; }
        public ControlType ControlType { get => _commonData.ControlType; set => _commonData.ControlType = value; }
        public RenderType RenderType { get => _commonData.RenderType; set => _commonData.RenderType = value; }
        #endregion

        /// <summary>
        /// Gets the numeric ID of the current ControlType.
        /// </summary>
        public ControlTypeID ControlTypeID => ControlType?.Identifier ?? ControlTypeID.None;

        /// <summary>
        /// Gets the numeric ID of the current MoveType.
        /// </summary>
        public MovementTypeID MoveTypeID => MoveType?.Identifier ?? MovementTypeID.None;

        /// <summary>
        /// Gets the numeric ID of the current RenderType.
        /// </summary>
        public RenderTypeID RenderTypeID => RenderType?.Identifier ?? RenderTypeID.None;
    }

    public class D2XXLLevelObject : ILevelObject
    {
        private readonly LevelObjectCommon _commonData = new LevelObjectCommon();
        private ID2XXLObjectType _type;
        private ID2XXLObjectType _containsType;

        #region ILevelObject implementation
        public IObjectType Type
        {
            get => _type;
            set
            {
                if (!(value is ID2XXLObjectType))
                    throw new ArgumentException("D2XXLLevelObject.Type: value must be a valid D2X-XL object type");
                _type = (ID2XXLObjectType)value;
            }
        }

        public byte SubtypeID { get => _commonData.SubtypeID; set => _commonData.SubtypeID = value; }
        public byte Flags { get => _commonData.Flags; set => _commonData.Flags = value; }
        public Segment Segment { get => _commonData.Segment; set => _commonData.Segment = value; }
        public short AttachedObject { get => _commonData.AttachedObject; set => _commonData.AttachedObject = value; }
        public FixVector Position { get => _commonData.Position; set => _commonData.Position = value; }
        public FixMatrix Orientation { get => _commonData.Orientation; set => _commonData.Orientation = value; }
        public Fix Size { get => _commonData.Size; set => _commonData.Size = value; }
        public Fix Shields { get => _commonData.Shields; set => _commonData.Shields = value; }
        public FixVector LastPos { get => _commonData.LastPos; set => _commonData.LastPos = value; }

        public IObjectType ContainsType
        {
            get => _containsType;
            set
            {
                if (value is null)
                {
                    _containsType = null;
                    return;
                }
                if (value is ID2XXLObjectType xlType)
                {
                    _containsType = xlType;
                    return;
                }
                throw new ArgumentException("D2XXLLevelObject.ContainsType: value must be a valid D2X-XL object type");
            }
        }

        public byte ContainsSubtypeID { get => _commonData.ContainsSubtypeID; set => _commonData.ContainsSubtypeID = value; }
        public byte ContainsCount { get => _commonData.ContainsCount; set => _commonData.ContainsCount = value; }
        public MovementType MoveType { get => _commonData.MoveType; set => _commonData.MoveType = value; }
        public ControlType ControlType { get => _commonData.ControlType; set => _commonData.ControlType = value; }
        public RenderType RenderType { get => _commonData.RenderType; set => _commonData.RenderType = value; }
        #endregion

        /// <summary>
        /// Gets the numeric ID of the current ControlType.
        /// </summary>
        public ControlTypeID ControlTypeID => ControlType?.Identifier ?? ControlTypeID.None;

        /// <summary>
        /// Gets the numeric ID of the current MoveType.
        /// </summary>
        public MovementTypeID MoveTypeID => MoveType?.Identifier ?? MovementTypeID.None;

        /// <summary>
        /// Gets the numeric ID of the current RenderType.
        /// </summary>
        public RenderTypeID RenderTypeID => RenderType?.Identifier ?? RenderTypeID.None;

        /// <summary>
        /// Indicates if this object is only present in multiplayer modes. D2X-XL only.
        /// </summary>
        public bool MultiplayerOnly { get; set; } = false;

        /// <summary>
        /// The object trigger assigned to this object. D2X-XL only.
        /// </summary>
        public D2XXLTrigger Trigger { get; set; }
    }
}
