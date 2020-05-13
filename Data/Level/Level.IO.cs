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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibDescent.Data
{
    internal struct FileInfo
    {
        public ushort signature;
        public ushort version;
        public int size;
        public string mineFilename;
        public int levelNumber;
        public int playerOffset;
        public int playerSize;
        public int objectsOffset;
        public int objectsCount;
        public int objectsSize;
        public int wallsOffset;
        public int wallsCount;
        public int wallsSize;
        public int doorsOffset;
        public int doorsCount;
        public int doorsSize;
        public int triggersOffset;
        public int triggersCount;
        public int triggersSize;
        public int linksOffset;
        public int linksCount;
        public int linksSize;
        public int reactorTriggersOffset;
        public int reactorTriggersCount;
        public int reactorTriggersSize;
        public int matcenOffset;
        public int matcenCount;
        public int matcenSize;
        public int deltaLightIndicesOffset;
        public int deltaLightIndicesCount;
        public int deltaLightIndicesSize;
        public int deltaLightsOffset;
        public int deltaLightsCount;
        public int deltaLightsSize;
    }

    internal abstract class DescentLevelReader
    {
        protected Stream _stream;
        /// <summary>
        /// Level version info copied from D2 gamesave.cpp:
        /// 1 -> 2  add palette name
        /// 2 -> 3  add control center explosion time
        /// 3 -> 4  add reactor strength
        /// 4 -> 5  killed hostage text stuff
        /// 5 -> 6  added Secret_return_segment and Secret_return_orient
        /// 6 -> 7  added flickering lights
        /// 7 -> 8  made version 8 to be not compatible with D2 1.0 & 1.1
        /// </summary>
        protected int _levelVersion;
        protected int _mineDataOffset;
        protected int _gameDataOffset;
        protected FileInfo _fileInfo;
        private Dictionary<Side, uint> _sideWallLinks = new Dictionary<Side, uint>();
        private Dictionary<Segment, uint> _segmentMatcenLinks = new Dictionary<Segment, uint>();
        private Dictionary<Wall, byte> _wallTriggerLinks = new Dictionary<Wall, byte>();

        protected abstract ILevel Level { get; }

        protected void LoadLevel()
        {
            // Don't dispose of the stream, let the caller do that
            using (var reader = new BinaryReader(_stream, Encoding.ASCII, true))
            {
                int signature = reader.ReadInt32();
                const int expectedSignature = 'P' * 0x1000000 + 'L' * 0x10000 + 'V' * 0x100 + 'L';
                if (signature != expectedSignature)
                {
                    throw new InvalidDataException("Level signature is invalid.");
                }
                _levelVersion = reader.ReadInt32();
                CheckLevelVersion();

                _mineDataOffset = reader.ReadInt32();
                _gameDataOffset = reader.ReadInt32();

                if (_levelVersion >= 8)
                {
                    // Dummy Vertigo-related data
                    _ = reader.ReadInt32();
                    _ = reader.ReadInt16();
                    _ = reader.ReadByte();
                }

                if (_levelVersion < 5)
                {
                    // Hostage text offset - not used
                    _ = reader.ReadInt32();
                }

                LoadVersionSpecificLevelInfo(reader);
                LoadMineData(reader);
                LoadVersionSpecificMineData(reader);
                LoadGameInfo(reader);
                LoadVersionSpecificGameInfo(reader);
            }
        }

        private void LoadMineData(BinaryReader reader)
        {
            reader.BaseStream.Seek(_mineDataOffset, SeekOrigin.Begin);

            // Header
            _ = reader.ReadByte(); // compiled mine version, not used
            short numVertices = reader.ReadInt16();
            short numSegments = reader.ReadInt16();

            // Vertices
            for (int i = 0; i < numVertices; i++)
            {
                var vector = ReadFixVector(reader);
                var vertex = new LevelVertex(vector);
                Level.Vertices.Add(vertex);
            }

            // Segments

            // Allocate segments/sides before reading data so we don't need a separate linking phase for them
            for (int i = 0; i < numSegments; i++)
            {
                var segment = new Segment();
                for (uint sideNum = 0; sideNum < Segment.MaxSegmentSides; sideNum++)
                {
                    segment.Sides[sideNum] = new Side(segment, sideNum);
                }
                Level.Segments.Add(segment);
            }
            // Now read segment data
            foreach (var segment in Level.Segments)
            {
                byte segmentBitMask = reader.ReadByte();
                if (_levelVersion == 5)
                {
                    if (SegmentHasSpecialData(segmentBitMask))
                    {
                        ReadSegmentSpecial(reader, segment);
                    }
                    ReadSegmentVertices(reader, segment);
                    ReadSegmentConnections(reader, segment, segmentBitMask);
                }
                else
                {
                    ReadSegmentConnections(reader, segment, segmentBitMask);
                    ReadSegmentVertices(reader, segment);
                    if (_levelVersion <= 1 && SegmentHasSpecialData(segmentBitMask))
                    {
                        ReadSegmentSpecial(reader, segment);
                    }
                }

                if (_levelVersion <= 5)
                {
                    segment.Light = new Fix(reader.ReadUInt16() << 4);
                }

                ReadSegmentWalls(reader, segment);
                ReadSegmentTextures(reader, segment);
            }

            // D2 retail location for segment special data
            if (_levelVersion > 5)
            {
                foreach (var segment in Level.Segments)
                {
                    ReadSegmentSpecial(reader, segment);
                }
            }
        }

        internal void ReadSegmentConnections(BinaryReader reader, Segment segment, byte segmentBitMask)
        {
            for (int sideNum = 0; sideNum < Segment.MaxSegmentSides; sideNum++)
            {
                if ((segmentBitMask & (1 << sideNum)) != 0)
                {
                    var childSegmentId = reader.ReadInt16();
                    if (childSegmentId == -2)
                    {
                        segment.Sides[sideNum].Exit = true;
                    }
                    else if (childSegmentId >= 0) // -1 = disconnected
                    {
                        segment.Sides[sideNum].ConnectedSegment = Level.Segments[childSegmentId];
                    }
                }
            }
        }

        private void ReadSegmentVertices(BinaryReader reader, Segment segment)
        {
            for (uint i = 0; i < Segment.MaxSegmentVerts; i++)
            {
                var vertexNum = reader.ReadInt16();
                segment.Vertices[i] = Level.Vertices[vertexNum];
                segment.Vertices[i].ConnectedSegments.Add((segment, i));
            }

            // Connect vertices to sides
            foreach (var side in segment.Sides)
            {
                for (int vertexNum = 0; vertexNum < side.GetNumVertices(); vertexNum++)
                {
                    side.GetVertex(vertexNum).ConnectedSides.Add((side, (uint)vertexNum));
                }
            }
        }

        private void ReadSegmentWalls(BinaryReader reader, Segment segment)
        {
            byte wallsBitMask = reader.ReadByte();
            for (uint sideNum = 0; sideNum < Segment.MaxSegmentSides; sideNum++)
            {
                if ((wallsBitMask & (1 << (int)sideNum)) != 0)
                {
                    byte wallNum = reader.ReadByte();
                    if (wallNum != 255)
                    {
                        // Walls haven't been read yet so we need to record the numbers and link later
                        _sideWallLinks[segment.Sides[sideNum]] = wallNum;
                    }
                }
            }
        }

        private static bool SegmentHasSpecialData(byte segmentBitMask)
        {
            return (segmentBitMask & (1 << Segment.MaxSegmentSides)) != 0;
        }

        private void ReadSegmentSpecial(BinaryReader reader, Segment segment)
        {
            segment.Function = (SegFunction)reader.ReadByte();
            var matcenNum = reader.ReadByte();
            // fuelcen number
            _ = _levelVersion > 5 ? reader.ReadByte() : reader.ReadInt16();

            if (matcenNum != 0xFF)
            {
                _segmentMatcenLinks[segment] = matcenNum;
            }

            if (_levelVersion > 5)
            {
                segment.Flags = reader.ReadByte();
                segment.Light = new Fix(reader.ReadInt32());
            }
        }

        private void ReadSegmentTextures(BinaryReader reader, Segment segment)
        {
            for (int sideNum = 0; sideNum < Segment.MaxSegmentSides; sideNum++)
            {
                var side = segment.Sides[sideNum];
                if ((side.ConnectedSegment == null && !side.Exit) || _sideWallLinks.ContainsKey(side))
                {
                    var rawTextureIndex = reader.ReadUInt16();
                    side.BaseTextureIndex = (ushort)(rawTextureIndex & 0x7fffu);
                    if ((rawTextureIndex & 0x8000) == 0)
                    {
                        side.OverlayTextureIndex = 0;
                    }
                    else
                    {
                        rawTextureIndex = reader.ReadUInt16();
                        side.OverlayTextureIndex = (ushort)(rawTextureIndex & 0x3fffu);
                        side.OverlayRotation = (OverlayRotation)((rawTextureIndex & 0xc000u) >> 14);
                    }

                    for (int uv = 0; uv < side.GetNumVertices(); uv++)
                    {
                        var uvl = Uvl.FromRawValues(reader.ReadInt16(), reader.ReadInt16(), reader.ReadUInt16());
                        side.Uvls[uv] = uvl;
                    }
                }
            }
        }

        private void LoadGameInfo(BinaryReader reader)
        {
            reader.BaseStream.Seek(_gameDataOffset, SeekOrigin.Begin);

            // "FileInfo" segment
            _fileInfo.signature = reader.ReadUInt16();
            if (_fileInfo.signature != 0x6705)
            {
                throw new InvalidDataException("Game info signature is invalid.");
            }

            const int MIN_GAMEINFO_VERSION = 22;
            _fileInfo.version = reader.ReadUInt16();
            if (_fileInfo.version < MIN_GAMEINFO_VERSION)
            {
                throw new InvalidDataException("Game info version is invalid.");
            }

            _fileInfo.size = reader.ReadInt32();
            // This is not actually used by the game, it's from an older (probably obsolete) format
            _fileInfo.mineFilename = ReadString(reader, 15, false);
            _fileInfo.levelNumber = reader.ReadInt32();
            _fileInfo.playerOffset = reader.ReadInt32();
            _fileInfo.playerSize = reader.ReadInt32();
            _fileInfo.objectsOffset = reader.ReadInt32();
            _fileInfo.objectsCount = reader.ReadInt32();
            _fileInfo.objectsSize = reader.ReadInt32();
            _fileInfo.wallsOffset = reader.ReadInt32();
            _fileInfo.wallsCount = reader.ReadInt32();
            _fileInfo.wallsSize = reader.ReadInt32();
            _fileInfo.doorsOffset = reader.ReadInt32();
            _fileInfo.doorsCount = reader.ReadInt32();
            _fileInfo.doorsSize = reader.ReadInt32();
            _fileInfo.triggersOffset = reader.ReadInt32();
            _fileInfo.triggersCount = reader.ReadInt32();
            _fileInfo.triggersSize = reader.ReadInt32();
            _fileInfo.linksOffset = reader.ReadInt32();
            _fileInfo.linksCount = reader.ReadInt32();
            _fileInfo.linksSize = reader.ReadInt32();
            _fileInfo.reactorTriggersOffset = reader.ReadInt32();
            _fileInfo.reactorTriggersCount = reader.ReadInt32();
            _fileInfo.reactorTriggersSize = reader.ReadInt32();
            _fileInfo.matcenOffset = reader.ReadInt32();
            _fileInfo.matcenCount = reader.ReadInt32();
            _fileInfo.matcenSize = reader.ReadInt32();

            if (_fileInfo.version >= 29)
            {
                _fileInfo.deltaLightIndicesOffset = reader.ReadInt32();
                _fileInfo.deltaLightIndicesCount = reader.ReadInt32();
                _fileInfo.deltaLightIndicesSize = reader.ReadInt32();
                _fileInfo.deltaLightsOffset = reader.ReadInt32();
                _fileInfo.deltaLightsCount = reader.ReadInt32();
                _fileInfo.deltaLightsSize = reader.ReadInt32();
            }

            // Level name (as seen in automap)
            if (_fileInfo.version >= 14)
            {
                Level.LevelName = ReadString(reader, 36, true);
            }

            // POF file names (we currently don't use this)
            var pofFileNames = new List<string>();
            if (_fileInfo.version >= 19)
            {
                int numPofNames = reader.ReadInt16();
                // Check for special values used by editors that don't support POF file names
                if (numPofNames != 0x614d && numPofNames != 0x5547)
                {
                    for (int i = 0; i < numPofNames; i++)
                    {
                        pofFileNames.Add(ReadString(reader, 13, false));
                    }
                }
            }

            // Player info (empty)

            // Objects
            reader.BaseStream.Seek(_fileInfo.objectsOffset, SeekOrigin.Begin);
            for (int i = 0; i < _fileInfo.objectsCount; i++)
            {
                var levelObject = ReadObject(reader);
                Level.Objects.Add(levelObject);
            }

            // Walls
            if (_fileInfo.wallsOffset != -1)
            {
                reader.BaseStream.Seek(_fileInfo.wallsOffset, SeekOrigin.Begin);
                for (int i = 0; i < _fileInfo.wallsCount; i++)
                {
                    if (_fileInfo.version >= 20)
                    {
                        var segmentNum = reader.ReadInt32();
                        var sideNum = reader.ReadInt32();
                        var side = Level.Segments[segmentNum].Sides[sideNum];

                        Wall wall = new Wall(side);
                        wall.HitPoints = new Fix(reader.ReadInt32());
                        _ = reader.ReadInt32(); // opposite wall - will recalculate
                        wall.Type = (WallType)reader.ReadByte();
                        wall.Flags = (WallFlags)reader.ReadByte();
                        wall.State = (WallState)reader.ReadByte();
                        var triggerNum = reader.ReadByte();
                        if (triggerNum != 0xFF)
                        {
                            _wallTriggerLinks[wall] = triggerNum;
                        }
                        wall.DoorClipNumber = reader.ReadByte();
                        wall.Keys = (WallKeyFlags)reader.ReadByte();
                        _ = reader.ReadByte(); // controlling trigger - will recalculate
                        wall.CloakOpacity = reader.ReadByte();
                        Level.Walls.Add(wall);
                    }
                }

                foreach (var sideWallLink in _sideWallLinks)
                {
                    sideWallLink.Key.Wall = Level.Walls[(int)sideWallLink.Value];
                }
            }

            // Triggers
            if (_fileInfo.triggersOffset != -1)
            {
                reader.BaseStream.Seek(_fileInfo.triggersOffset, SeekOrigin.Begin);
                for (int i = 0; i < _fileInfo.triggersCount; i++)
                {
                    ITrigger trigger = ReadTrigger(reader);
                    AddTrigger(trigger);
                    for (int targetNum = 0; targetNum < trigger.Targets.Count; targetNum++)
                    {
                        trigger.Targets[targetNum].Wall?.ControllingTriggers.Add((trigger, (uint)targetNum));
                    }
                }

                foreach (var wallTriggerLink in _wallTriggerLinks)
                {
                    wallTriggerLink.Key.Trigger = Level.Triggers[wallTriggerLink.Value];
                }
            }

            // Reactor triggers
            if (_fileInfo.reactorTriggersOffset != -1)
            {
                reader.BaseStream.Seek(_fileInfo.reactorTriggersOffset, SeekOrigin.Begin);
                for (int i = 0; i < _fileInfo.reactorTriggersCount; i++)
                {
                    var numReactorTriggerTargets = reader.ReadInt16();

                    // Not actually counted by the number of targets, which is interesting
                    var targets = ReadFixedLengthTargetList(reader, DescentLevelCommon.MaxReactorTriggerTargets);

                    for (int targetNum = 0; targetNum < numReactorTriggerTargets; targetNum++)
                    {
                        var side = Level.Segments[targets[targetNum].segmentNum].Sides[targets[targetNum].sideNum];
                        Level.ReactorTriggerTargets.Add(side);
                    }
                }
            }

            // Matcens
            if (_fileInfo.matcenOffset != -1)
            {
                reader.BaseStream.Seek(_fileInfo.matcenOffset, SeekOrigin.Begin);
                for (int i = 0; i < _fileInfo.matcenCount; i++)
                {
                    var robotFlags = new uint[2];
                    robotFlags[0] = reader.ReadUInt32();
                    if (_fileInfo.version > 25)
                    {
                        robotFlags[1] = reader.ReadUInt32();
                    }
                    var hitPoints = reader.ReadInt32();
                    var interval = reader.ReadInt32();
                    var segmentNum = reader.ReadInt16();
                    _ = reader.ReadInt16(); // fuelcen number - not needed

                    MatCenter matcen = new MatCenter(Level.Segments[segmentNum]);
                    matcen.InitializeSpawnedRobots(robotFlags);
                    matcen.HitPoints = hitPoints;
                    matcen.Interval = interval;
                    Level.MatCenters.Add(matcen);
                }

                foreach (var segmentMatcenLink in _segmentMatcenLinks)
                {
                    segmentMatcenLink.Key.MatCenter = Level.MatCenters[(int)segmentMatcenLink.Value];
                }
            }
        }

        private LevelObject ReadObject(BinaryReader reader)
        {
            var levelObject = new LevelObject();
            levelObject.type = (ObjectType)reader.ReadSByte();
            levelObject.id = reader.ReadByte();
            levelObject.controlType = (ControlType)reader.ReadByte();
            levelObject.moveType = (MovementType)reader.ReadByte();
            levelObject.renderType = (RenderType)reader.ReadByte();
            levelObject.flags = reader.ReadByte();
            levelObject.segnum = reader.ReadInt16();
            levelObject.attachedObject = -1;
            levelObject.position = ReadFixVector(reader);
            levelObject.orientation = ReadFixMatrix(reader);
            levelObject.size = new Fix(reader.ReadInt32());
            levelObject.shields = new Fix(reader.ReadInt32());
            levelObject.lastPos = ReadFixVector(reader);
            levelObject.containsType = reader.ReadByte();
            levelObject.containsId = reader.ReadByte();
            levelObject.containsCount = reader.ReadByte();

            switch (levelObject.moveType)
            {
                case MovementType.Physics:
                    levelObject.physicsInfo.velocity = ReadFixVector(reader);
                    levelObject.physicsInfo.thrust = ReadFixVector(reader);
                    levelObject.physicsInfo.mass = new Fix(reader.ReadInt32());
                    levelObject.physicsInfo.drag = new Fix(reader.ReadInt32());
                    levelObject.physicsInfo.brakes = new Fix(reader.ReadInt32());
                    levelObject.physicsInfo.angVel = ReadFixVector(reader);
                    levelObject.physicsInfo.rotThrust = ReadFixVector(reader);
                    levelObject.physicsInfo.turnroll = reader.ReadInt16();
                    levelObject.physicsInfo.flags = reader.ReadInt16();
                    break;
                case MovementType.Spinning:
                    levelObject.spinRate = ReadFixVector(reader);
                    break;
            }
            switch (levelObject.controlType)
            {
                case ControlType.AI:
                    levelObject.aiInfo.behavior = reader.ReadByte();
                    for (int i = 0; i < AIInfo.NumAIFlags; i++)
                        levelObject.aiInfo.aiFlags[i] = reader.ReadByte();

                    levelObject.aiInfo.hideSegment = reader.ReadInt16();
                    levelObject.aiInfo.hideIndex = reader.ReadInt16();
                    levelObject.aiInfo.pathLength = reader.ReadInt16();
                    levelObject.aiInfo.curPathIndex = reader.ReadInt16();

                    if (_fileInfo.version <= 25)
                    {
                        reader.ReadInt32();
                    }
                    break;
                case ControlType.Explosion:
                    levelObject.explosionInfo.SpawnTime = new Fix(reader.ReadInt32());
                    levelObject.explosionInfo.DeleteTime = new Fix(reader.ReadInt32());
                    levelObject.explosionInfo.DeleteObject = reader.ReadInt16();
                    break;
                case ControlType.Powerup:
                    if (_fileInfo.version >= 25)
                    {
                        levelObject.powerupCount = reader.ReadInt32();
                    }
                    break;
            }
            switch (levelObject.renderType)
            {
                case RenderType.Polyobj:
                    {
                        levelObject.modelInfo.modelNum = reader.ReadInt32();
                        for (int i = 0; i < Polymodel.MAX_SUBMODELS; i++)
                        {
                            levelObject.modelInfo.animAngles[i] = ReadFixAngles(reader);
                        }
                        levelObject.modelInfo.flags = reader.ReadInt32();
                        levelObject.modelInfo.textureOverride = reader.ReadInt32();
                    }
                    break;
                case RenderType.WeaponVClip:
                case RenderType.Hostage:
                case RenderType.Powerup:
                case RenderType.Fireball:
                    levelObject.spriteInfo.vclipNum = reader.ReadInt32();
                    levelObject.spriteInfo.frameTime = new Fix(reader.ReadInt32());
                    levelObject.spriteInfo.frameNumber = reader.ReadByte();
                    break;
            }

            return levelObject;
        }

        protected static string ReadString(BinaryReader reader, int maxStringLength, bool variableLength)
        {
            char[] stringBuffer = new char[maxStringLength];
            for (int i = 0; i < maxStringLength; i++)
            {
                stringBuffer[i] = (char)reader.ReadByte();
                if (stringBuffer[i] == '\n')
                {
                    stringBuffer[i] = '\0';
                }
                if (variableLength && stringBuffer[i] == '\0')
                {
                    break;
                }
            }
            return new string(stringBuffer).Trim('\0');
        }

        protected static (short segmentNum, short sideNum)[] ReadFixedLengthTargetList(BinaryReader reader, int targetListLength)
        {
            var targetList = new (short segmentNum, short sideNum)[targetListLength];
            for (int i = 0; i < targetListLength; i++)
            {
                targetList[i].segmentNum = reader.ReadInt16();
            }
            for (int i = 0; i < targetListLength; i++)
            {
                targetList[i].sideNum = reader.ReadInt16();
            }
            return targetList;
        }

        protected static FixVector ReadFixVector(BinaryReader reader)
        {
            return FixVector.FromRawValues(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }

        protected static FixAngles ReadFixAngles(BinaryReader reader)
        {
            return FixAngles.FromRawValues(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
        }

        protected FixMatrix ReadFixMatrix(BinaryReader reader)
        {
            return new FixMatrix(ReadFixVector(reader), ReadFixVector(reader), ReadFixVector(reader));
        }

        protected abstract void CheckLevelVersion();
        protected abstract void LoadVersionSpecificLevelInfo(BinaryReader reader);
        protected abstract void LoadVersionSpecificMineData(BinaryReader reader);
        protected abstract ITrigger ReadTrigger(BinaryReader reader);
        protected abstract void AddTrigger(ITrigger trigger);
        protected abstract void LoadVersionSpecificGameInfo(BinaryReader reader);
    }

    internal class D1LevelReader : DescentLevelReader
    {
        private readonly D1Level _level = new D1Level();

        protected override ILevel Level => _level;

        public D1LevelReader(Stream stream)
        {
            _stream = stream;
        }

        public D1Level Load()
        {
            LoadLevel();
            return _level;
        }

        protected override void CheckLevelVersion()
        {
            if (_levelVersion != 1)
            {
                throw new InvalidDataException($"Level version should be 1 but was {_levelVersion}.");
            }
        }

        protected override void LoadVersionSpecificLevelInfo(BinaryReader reader)
        {
        }

        protected override void LoadVersionSpecificMineData(BinaryReader reader)
        {
        }

        protected override ITrigger ReadTrigger(BinaryReader reader)
        {
            var trigger = new D1Trigger();
            trigger.Type = (TriggerType)reader.ReadByte();
            trigger.Flags = (D1TriggerFlags)reader.ReadUInt16();
            trigger.Value = new Fix(reader.ReadInt32());
            trigger.Time = reader.ReadInt32();
            _ = reader.ReadByte(); // link_num - does nothing
            var numLinks = reader.ReadInt16();

            var targets = ReadFixedLengthTargetList(reader, D1Trigger.MaxWallsPerLink);
            for (int i = 0; i < numLinks; i++)
            {
                var side = Level.Segments[targets[i].segmentNum].Sides[targets[i].sideNum];
                trigger.Targets.Add(side);
            }

            return trigger;
        }

        protected override void AddTrigger(ITrigger trigger)
        {
            (Level as D1Level).Triggers.Add(trigger as D1Trigger);
        }

        protected override void LoadVersionSpecificGameInfo(BinaryReader reader)
        {
        }
    }

    internal class D2LevelReader : DescentLevelReader
    {
        // Standard D2 = 7, Vertigo = 8, XL = up to 27
        public const int MaximumSupportedLevelVersion = 27;

        private readonly D2Level _level = new D2Level();
        private List<(short segmentNum, short sideNum, uint mask, Fix timer, Fix delay)> _flickeringLights =
            new List<(short, short, uint, Fix, Fix)>();
        private int _secretReturnSegmentNum = 0;
        private List<LightDelta> _lightDeltas = new List<LightDelta>();

        protected override ILevel Level => _level;

        public D2LevelReader(Stream stream)
        {
            _stream = stream;
        }

        public D2Level Load()
        {
            LoadLevel();
            return _level;
        }

        protected override void CheckLevelVersion()
        {
            if (_levelVersion < 2 || _levelVersion > 8)
            {
                throw new InvalidDataException($"Level version should be between 2 and 8 but was {_levelVersion}.");
            }
        }

        protected override void LoadVersionSpecificLevelInfo(BinaryReader reader)
        {
            if (_levelVersion >= 2)
            {
                _level.PaletteName = ReadString(reader, 13, true);
            }

            if (_levelVersion >= 3)
            {
                _level.BaseReactorCountdownTime = reader.ReadInt32();
            }
            else
            {
                _level.BaseReactorCountdownTime = D2Level.DefaultBaseReactorCountdownTime;
            }

            if (_levelVersion >= 4)
            {
                _level.ReactorStrength = reader.ReadInt32();
            }
            else
            {
                _level.ReactorStrength = null;
            }

            if (_levelVersion >= 7)
            {
                var numFlickeringLights = reader.ReadInt32();
                for (int i = 0; i < numFlickeringLights; i++)
                {
                    // Probably should really be using a struct, but this is surprisingly readable...
                    _flickeringLights.Add((
                        segmentNum: reader.ReadInt16(),
                        sideNum: reader.ReadInt16(),
                        mask: reader.ReadUInt32(),
                        timer: new Fix(reader.ReadInt32()),
                        delay: new Fix(reader.ReadInt32())
                        ));
                }
            }

            if (_levelVersion >= 6)
            {
                _secretReturnSegmentNum = reader.ReadInt32();
                // Secret return matrix is actually serialized in a different order from every other matrix
                // in the RDL/RL2 format... so use named parameters to avoid problems
                _level.SecretReturnOrientation = new FixMatrix(
                    right: FixVector.FromRawValues(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()),
                    forward: FixVector.FromRawValues(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()),
                    up: FixVector.FromRawValues(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32())
                    );
            }
            else
            {
                _level.SecretReturnOrientation = new FixMatrix(
                    new FixVector(1, 0, 0), new FixVector(0, 1, 0), new FixVector(0, 0, 1)
                    );
            }
        }

        protected override void LoadVersionSpecificMineData(BinaryReader reader)
        {
            // Nothing to actually read, but we do need to set up some links

            foreach (var light in _flickeringLights)
            {
                var side = Level.Segments[light.segmentNum].Sides[light.sideNum];
                var animatedLight = new AnimatedLight(side);
                animatedLight.Mask = light.mask;
                animatedLight.TimeToNextTick = light.timer;
                animatedLight.TickLength = light.delay;
                _level.AnimatedLights.Add(animatedLight);
                side.AnimatedLight = animatedLight;
            }

            _level.SecretReturnSegment = _level.Segments[_secretReturnSegmentNum];
        }

        protected override ITrigger ReadTrigger(BinaryReader reader)
        {
            var trigger = new D2Trigger();
            trigger.Type = (TriggerType)reader.ReadByte();
            trigger.Flags = (D2TriggerFlags)reader.ReadByte();
            var numLinks = reader.ReadSByte();
            reader.ReadByte(); //padding byte
            trigger.Value = new Fix(reader.ReadInt32());
            trigger.Time = reader.ReadInt32();

            var targets = ReadFixedLengthTargetList(reader, D2Trigger.MaxWallsPerLink);
            for (int i = 0; i < numLinks; i++)
            {
                var side = Level.Segments[targets[i].segmentNum].Sides[targets[i].sideNum];
                trigger.Targets.Add(side);
            }

            return trigger;
        }

        protected override void AddTrigger(ITrigger trigger)
        {
            (Level as D2Level).Triggers.Add(trigger as D2Trigger);
        }

        protected override void LoadVersionSpecificGameInfo(BinaryReader reader)
        {
            // Delta lights (D2)
            // Reading this first to make lights easier to link up
            if (_fileInfo.deltaLightsOffset != -1)
            {
                reader.BaseStream.Seek(_fileInfo.deltaLightsOffset, SeekOrigin.Begin);
                for (int i = 0; i < _fileInfo.deltaLightsCount; i++)
                {
                    var segmentNum = reader.ReadInt16();
                    var sideNum = reader.ReadByte();
                    var lightDelta = new LightDelta(Level.Segments[segmentNum].Sides[sideNum]);
                    _ = reader.ReadByte(); // dummy - probably used for dword alignment
                    // Vertex deltas scaled by 2048 - see DL_SCALE in segment.h
                    lightDelta.vertexDeltas[0] = new Fix(reader.ReadByte() * 2048);
                    lightDelta.vertexDeltas[1] = new Fix(reader.ReadByte() * 2048);
                    lightDelta.vertexDeltas[2] = new Fix(reader.ReadByte() * 2048);
                    lightDelta.vertexDeltas[3] = new Fix(reader.ReadByte() * 2048);
                    _lightDeltas.Add(lightDelta);
                }
            }

            // Delta light indices (D2)
            if (_fileInfo.deltaLightIndicesOffset != -1)
            {
                reader.BaseStream.Seek(_fileInfo.deltaLightIndicesOffset, SeekOrigin.Begin);
                for (int i = 0; i < _fileInfo.deltaLightIndicesCount; i++)
                {
                    var segmentNum = reader.ReadInt16();
                    var sideNum = reader.ReadByte();
                    var count = reader.ReadByte();
                    var index = reader.ReadInt16();

                    var side = Level.Segments[segmentNum].Sides[sideNum];
                    var dynamicLight = new DynamicLight(side);
                    dynamicLight.LightDeltas.AddRange(_lightDeltas.GetRange(index, count));
                    _level.DynamicLights.Add(dynamicLight);
                    side.DynamicLight = dynamicLight;
                }
            }
        }
    }

    public class LevelFactory
    {
        public static ILevel CreateFromStream(Stream stream)
        {
            int levelVersion;
            var streamStartPosition = stream.Position;

            // First figure out what kind of level this is
            using (var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true))
            {
                int signature = reader.ReadInt32();
                const int expectedSignature = 'P' * 0x1000000 + 'L' * 0x10000 + 'V' * 0x100 + 'L';
                if (signature != expectedSignature)
                {
                    throw new InvalidDataException("Level signature is invalid.");
                }
                levelVersion = reader.ReadInt32();
            }

            // Rewind
            stream.Position = streamStartPosition;

            // Now do the actual load
            if (levelVersion == 1)
            {
                return new D1LevelReader(stream).Load();
            }
            else if (levelVersion >= 2 && levelVersion <= 8)
            {
                return new D2LevelReader(stream).Load();
            }
            else
            {
                throw new InvalidDataException($"Unrecognized level version {levelVersion}.");
            }
        }
    }

    internal abstract class DescentLevelWriter
    {
        protected Stream _stream;
        protected List<Segment> _fuelcens = new List<Segment>();
        protected List<string> _pofFiles = new List<string>();
        protected abstract ILevel Level { get; }
        protected abstract int LevelVersion { get; }
        protected abstract ushort GameDataVersion { get; }
        private const int GameDataSize = 143;

        /// <summary>
        /// The .POF file names to write into the level, for consumers that need it.
        /// Referenced by (non-robot) objects with a PolyObj render type.
        /// </summary>
        public List<string> PofFiles => _pofFiles;

        public void Write()
        {
            // Don't dispose of the stream, let the caller do that
            using (var writer = new BinaryWriter(_stream, Encoding.ASCII, true))
            {
                writer.Write(0x504C564C); // signature, "PLVL"
                writer.Write(LevelVersion);
                long pointerTable = writer.BaseStream.Position;
                writer.Write(0); // mine data pointer
                writer.Write(0); // game data pointer
                if (LevelVersion >= 8)
                {
                    // Dummy Vertigo-related data
                    writer.Write(0);
                    writer.Write((short)0);
                    writer.Write((byte)0);
                }
                if (LevelVersion < 5)
                {
                    writer.Write(0); // hostage text pointer
                }

                WriteVersionSpecificLevelInfo(writer);

                int mineDataPointer = (int)writer.BaseStream.Position;
                WriteMineData(writer);

                int gameDataPointer = (int)writer.BaseStream.Position;
                WriteGameData(writer);

                int hostageTextPointer = (int)writer.BaseStream.Position;

                // Go back and write pointers
                writer.BaseStream.Seek(pointerTable, SeekOrigin.Begin);
                writer.Write(mineDataPointer);
                writer.Write(gameDataPointer);
                if (LevelVersion >= 8)
                {
                    // Skip Vertigo data (this will never actually be needed, but...)
                    writer.BaseStream.Seek(7, SeekOrigin.Current);
                }
                if (LevelVersion < 5)
                {
                    writer.Write(hostageTextPointer);
                }
            }
        }

        private void WriteMineData(BinaryWriter writer)
        {
            writer.Write((byte)0); // compiled mine version
            writer.Write((short)Level.Vertices.Count);
            writer.Write((short)Level.Segments.Count);

            foreach (var vertex in Level.Vertices)
            {
                WriteFixVector(writer, vertex.Location);
            }

            // Generate fuelcen list before writing segments
            foreach (var segment in Level.Segments)
            {
                if (SegmentIsFuelcen(segment))
                {
                    _fuelcens.Add(segment);
                }
            }

            foreach (var segment in Level.Segments)
            {
                writer.Write(GetSegmentBitMask(segment));
                if (LevelVersion == 5)
                {
                    if (SegmentHasSpecialData(segment))
                    {
                        WriteSegmentSpecialData(writer, segment);
                    }
                    WriteSegmentVertices(writer, segment);
                    WriteSegmentConnections(writer, segment);
                }
                else
                {
                    WriteSegmentConnections(writer, segment);
                    WriteSegmentVertices(writer, segment);
                    if (LevelVersion <= 1 && SegmentHasSpecialData(segment))
                    {
                        WriteSegmentSpecialData(writer, segment);
                    }
                }

                if (LevelVersion <= 5)
                {
                    writer.Write((ushort)(segment.Light.value >> 4));
                }

                WriteSegmentWalls(writer, segment);
                WriteSegmentTextures(writer, segment);
            }

            if (LevelVersion > 5)
            {
                foreach (var segment in Level.Segments)
                {
                    WriteSegmentSpecialData(writer, segment);
                }
            }
        }

        private byte GetSegmentBitMask(Segment segment)
        {
            byte segmentBitMask = 0;

            for (uint sideNum = 0; sideNum < Segment.MaxSegmentSides; sideNum++)
            {
                var side = segment.Sides[sideNum];
                if (side.ConnectedSegment != null || side.Exit)
                {
                    segmentBitMask |= (byte)(1 << (int)sideNum);
                }

                if (SegmentHasSpecialData(segment))
                {
                    segmentBitMask |= (1 << Segment.MaxSegmentSides);
                }
            }

            return segmentBitMask;
        }

        private bool SegmentHasSpecialData(Segment segment)
        {
            if (LevelVersion > 5)
            {
                // Static light is now in special data, it's always needed
                return true;
            }
            if (segment.Function != SegFunction.None)
            {
                return true;
            }
            return false;
        }

        private void WriteSegmentSpecialData(BinaryWriter writer, Segment segment)
        {
            writer.Write((byte)segment.Function);
            byte matcenIndex = (segment.MatCenter == null) ? (byte)0xFF :
                (byte)Level.MatCenters.IndexOf(segment.MatCenter);
            writer.Write(matcenIndex);
            var fuelcenIndex = SegmentIsFuelcen(segment) ?
                _fuelcens.IndexOf(segment) : -1;
            if (LevelVersion > 5)
            {
                writer.Write((byte)fuelcenIndex);
            }
            else
            {
                writer.Write((short)fuelcenIndex);
            }

            if (LevelVersion > 5)
            {
                writer.Write(segment.Flags);
                writer.Write(segment.Light.value);
            }
        }

        private bool SegmentIsFuelcen(Segment segment)
        {
            switch (segment.Function)
            {
                case SegFunction.FuelCenter:
                case SegFunction.RepairCenter:
                case SegFunction.Reactor:
                case SegFunction.MatCenter:
                    return true;

                default:
                    return false;
            }
        }

        private void WriteSegmentVertices(BinaryWriter writer, Segment segment)
        {
            foreach (var vertex in segment.Vertices)
            {
                writer.Write((ushort)Level.Vertices.IndexOf(vertex));
            }
        }

        private void WriteSegmentConnections(BinaryWriter writer, Segment segment)
        {
            foreach (var side in segment.Sides)
            {
                if (side.ConnectedSegment != null)
                {
                    writer.Write((short)Level.Segments.IndexOf(side.ConnectedSegment));
                }
                else if (side.Exit)
                {
                    writer.Write((short)-2);
                }
                // We don't write -1s since the bitmask shouldn't be set for those
            }
        }

        private void WriteSegmentWalls(BinaryWriter writer, Segment segment)
        {
            byte wallsBitMask = 0;
            for (int sideNum = 0; sideNum < Segment.MaxSegmentSides; sideNum++)
            {
                if (segment.Sides[sideNum].Wall != null)
                {
                    wallsBitMask |= (byte)(1 << sideNum);
                }
            }
            writer.Write(wallsBitMask);

            foreach (var side in segment.Sides)
            {
                if (side.Wall != null)
                {
                    writer.Write((byte)Level.Walls.IndexOf(side.Wall));
                }
            }
        }

        private void WriteSegmentTextures(BinaryWriter writer, Segment segment)
        {
            foreach (var side in segment.Sides)
            {
                if ((side.ConnectedSegment == null && !side.Exit) || side.Wall != null)
                {
                    ushort rawTextureIndex = side.BaseTextureIndex;
                    if (side.OverlayTextureIndex != 0)
                    {
                        rawTextureIndex |= 0x8000;
                    }
                    writer.Write(rawTextureIndex);

                    if (side.OverlayTextureIndex != 0)
                    {
                        rawTextureIndex = side.OverlayTextureIndex;
                        rawTextureIndex |= (ushort)(((ushort)side.OverlayRotation) << 14);
                        writer.Write(rawTextureIndex);
                    }

                    foreach (var uvl in side.Uvls)
                    {
                        var rawUvl = uvl.ToRawValues();
                        writer.Write(rawUvl.u);
                        writer.Write(rawUvl.v);
                        writer.Write(rawUvl.l);
                    }
                }
            }
        }

        private void WriteGameData(BinaryWriter writer)
        {
            long fileInfoOffset = writer.BaseStream.Position;
            FileInfo fileInfo = new FileInfo();
            fileInfo.signature = 0x6705;
            fileInfo.version = GameDataVersion;
            fileInfo.size = GameDataSize;
            fileInfo.mineFilename = ""; // Not used, leave blank
            fileInfo.levelNumber = 0; // Doesn't seem to be used by Descent

            // We'll have to rewrite FileInfo later, but write it now to make space
            WriteFileInfo(writer, ref fileInfo);

            if (GameDataVersion >= 14)
            {
                var encodedLevelName = EncodeString(Level.LevelName, 36, true);
                if (GameDataVersion >= 31)
                {
                    // Newline-terminated
                    encodedLevelName[encodedLevelName.Length - 1] = (byte)'\n';
                }
                writer.Write(encodedLevelName);
            }

            // POF file names
            if (GameDataVersion >= 19)
            {
                writer.Write((short)_pofFiles.Count);
                foreach (string pofName in _pofFiles)
                {
                    writer.Write(EncodeString(pofName, 13, false));
                }
            }

            // Player info (empty)
            fileInfo.playerOffset = (int)writer.BaseStream.Position;
            fileInfo.playerSize = 0;

            // Objects
            fileInfo.objectsOffset = (int)writer.BaseStream.Position;
            fileInfo.objectsCount = Level.Objects.Count;
            foreach (var levelObject in Level.Objects)
            {
                WriteObject(writer, levelObject);
            }
            fileInfo.objectsSize = (int)writer.BaseStream.Position - fileInfo.objectsOffset;

            // Walls
            fileInfo.wallsOffset = (Level.Walls.Count > 0) ?
                (int)writer.BaseStream.Position : -1;
            fileInfo.wallsCount = Level.Walls.Count;
            if (GameDataVersion >= 20)
            {
                foreach (var wall in Level.Walls)
                {
                    writer.Write(Level.Segments.IndexOf(wall.Side.Segment));
                    writer.Write(wall.Side.SideNum);
                    writer.Write(wall.HitPoints.value);
                    writer.Write(wall.OppositeWall != null ? Level.Walls.IndexOf(wall.OppositeWall) : -1);
                    writer.Write((byte)wall.Type);
                    writer.Write((byte)wall.Flags);
                    writer.Write((byte)wall.State);
                    writer.Write((byte)(wall.Trigger != null ? Level.Triggers.IndexOf(wall.Trigger) : 0xFF));
                    writer.Write(wall.DoorClipNumber);
                    writer.Write((byte)wall.Keys);
                    // We can only write one controlling trigger, so use the first one
                    var controllingTriggerIndex = (wall.ControllingTriggers.Count > 0) ?
                        Level.Triggers.IndexOf(wall.ControllingTriggers[0].trigger) : -1;
                    writer.Write((byte)controllingTriggerIndex);
                    writer.Write(wall.CloakOpacity);
                }
            }
            fileInfo.wallsSize = (Level.Walls.Count > 0) ?
                (int)writer.BaseStream.Position - fileInfo.wallsOffset : 0;

            fileInfo.doorsOffset = -1;
            fileInfo.doorsCount = 0;
            fileInfo.doorsSize = 0;

            // Triggers
            fileInfo.triggersOffset = (Level.Triggers.Count > 0) ?
                (int)writer.BaseStream.Position : -1;
            fileInfo.triggersCount = Level.Triggers.Count;
            foreach (var trigger in Level.Triggers)
            {
                WriteTrigger(writer, trigger);
            }
            fileInfo.triggersSize = (Level.Triggers.Count > 0) ?
                (int)writer.BaseStream.Position - fileInfo.triggersOffset : 0;

            fileInfo.linksOffset = -1;
            fileInfo.linksCount = 0;
            fileInfo.linksSize = 0;

            // Reactor triggers
            fileInfo.reactorTriggersOffset = (Level.ReactorTriggerTargets.Count > 0) ?
                (int)writer.BaseStream.Position : -1;
            fileInfo.reactorTriggersCount = (Level.ReactorTriggerTargets.Count > 0) ?
                1 : 0;
            if (Level.ReactorTriggerTargets.Count > 0)
            {
                writer.Write((short)Level.ReactorTriggerTargets.Count);
                for (int targetNum = 0; targetNum < DescentLevelCommon.MaxReactorTriggerTargets; targetNum++)
                {
                    if (targetNum < Level.ReactorTriggerTargets.Count)
                    {
                        var segmentNum = Level.Segments.IndexOf(Level.ReactorTriggerTargets[targetNum].Segment);
                        writer.Write((short)segmentNum);
                    }
                    else
                    {
                        writer.Write((short)0);
                    }
                }

                for (int targetNum = 0; targetNum < DescentLevelCommon.MaxReactorTriggerTargets; targetNum++)
                {
                    if (targetNum < Level.ReactorTriggerTargets.Count)
                    {
                        writer.Write((short)Level.ReactorTriggerTargets[targetNum].SideNum);
                    }
                    else
                    {
                        writer.Write((short)0);
                    }
                }
            }
            fileInfo.reactorTriggersSize = (Level.ReactorTriggerTargets.Count > 0) ?
                (int)writer.BaseStream.Position - fileInfo.reactorTriggersOffset : 0;

            // Matcens
            fileInfo.matcenOffset = (Level.MatCenters.Count > 0) ?
                (int)writer.BaseStream.Position : -1;
            fileInfo.matcenCount = Level.MatCenters.Count;
            foreach (var matcen in Level.MatCenters)
            {
                var robotFlags = new uint[2];
                foreach (uint robotId in matcen.SpawnedRobotIds)
                {
                    if (robotId < 32)
                    {
                        robotFlags[0] |= 1u << (int)robotId;
                    }
                    else if (robotId < 64)
                    {
                        robotFlags[1] |= 1u << (int)(robotId - 32);
                    }
                }

                writer.Write(robotFlags[0]);
                if (GameDataVersion > 25)
                {
                    writer.Write(robotFlags[1]);
                }
                writer.Write(matcen.HitPoints.value);
                writer.Write(matcen.Interval.value);
                writer.Write((short)Level.Segments.IndexOf(matcen.Segment));
                writer.Write((short)_fuelcens.IndexOf(matcen.Segment));
            }
            fileInfo.matcenSize = (Level.MatCenters.Count > 0) ?
                (int)writer.BaseStream.Position - fileInfo.matcenOffset : 0;

            if (GameDataVersion >= 29)
            {
                WriteDynamicLights(writer, ref fileInfo);
            }

            // Rewrite FileInfo with updated data
            writer.BaseStream.Seek(fileInfoOffset, SeekOrigin.Begin);
            WriteFileInfo(writer, ref fileInfo);
        }

        private void WriteFileInfo(BinaryWriter writer, ref FileInfo fileInfo)
        {
            writer.Write(fileInfo.signature);
            writer.Write(fileInfo.version);
            writer.Write(fileInfo.size);
            writer.Write(EncodeString(fileInfo.mineFilename, 15, false));
            writer.Write(fileInfo.levelNumber);
            writer.Write(fileInfo.playerOffset);
            writer.Write(fileInfo.playerSize);
            writer.Write(fileInfo.objectsOffset);
            writer.Write(fileInfo.objectsCount);
            writer.Write(fileInfo.objectsSize);
            writer.Write(fileInfo.wallsOffset);
            writer.Write(fileInfo.wallsCount);
            writer.Write(fileInfo.wallsSize);
            writer.Write(fileInfo.doorsOffset);
            writer.Write(fileInfo.doorsCount);
            writer.Write(fileInfo.doorsSize);
            writer.Write(fileInfo.triggersOffset);
            writer.Write(fileInfo.triggersCount);
            writer.Write(fileInfo.triggersSize);
            writer.Write(fileInfo.linksOffset);
            writer.Write(fileInfo.linksCount);
            writer.Write(fileInfo.linksSize);
            writer.Write(fileInfo.reactorTriggersOffset);
            writer.Write(fileInfo.reactorTriggersCount);
            writer.Write(fileInfo.reactorTriggersSize);
            writer.Write(fileInfo.matcenOffset);
            writer.Write(fileInfo.matcenCount);
            writer.Write(fileInfo.matcenSize);

            if (GameDataVersion >= 29)
            {
                writer.Write(fileInfo.deltaLightIndicesOffset);
                writer.Write(fileInfo.deltaLightIndicesCount);
                writer.Write(fileInfo.deltaLightIndicesSize);
                writer.Write(fileInfo.deltaLightsOffset);
                writer.Write(fileInfo.deltaLightsCount);
                writer.Write(fileInfo.deltaLightsSize);
            }
        }

        private void WriteObject(BinaryWriter writer, LevelObject levelObject)
        {
            writer.Write((byte)levelObject.type);
            writer.Write(levelObject.id);
            writer.Write((byte)levelObject.controlType);
            writer.Write((byte)levelObject.moveType);
            writer.Write((byte)levelObject.renderType);
            writer.Write(levelObject.flags);
            writer.Write(levelObject.segnum);
            WriteFixVector(writer, levelObject.position);
            WriteFixMatrix(writer, levelObject.orientation);
            writer.Write(levelObject.size.value);
            writer.Write(levelObject.shields.value);
            WriteFixVector(writer, levelObject.lastPos);
            writer.Write(levelObject.containsType);
            writer.Write(levelObject.containsId);
            writer.Write(levelObject.containsCount);

            switch (levelObject.moveType)
            {
                case MovementType.Physics:
                    WriteFixVector(writer, levelObject.physicsInfo.velocity);
                    WriteFixVector(writer, levelObject.physicsInfo.thrust);
                    writer.Write(levelObject.physicsInfo.mass.value);
                    writer.Write(levelObject.physicsInfo.drag.value);
                    writer.Write(levelObject.physicsInfo.brakes.value);
                    WriteFixVector(writer, levelObject.physicsInfo.angVel);
                    WriteFixVector(writer, levelObject.physicsInfo.rotThrust);
                    writer.Write(levelObject.physicsInfo.turnroll);
                    writer.Write(levelObject.physicsInfo.flags);
                    break;
                case MovementType.Spinning:
                    WriteFixVector(writer, levelObject.spinRate);
                    break;
            }
            switch (levelObject.controlType)
            {
                case ControlType.AI:
                    writer.Write(levelObject.aiInfo.behavior);
                    for (int i = 0; i < AIInfo.NumAIFlags; i++)
                        writer.Write(levelObject.aiInfo.aiFlags[i]);

                    writer.Write(levelObject.aiInfo.hideSegment);
                    writer.Write(levelObject.aiInfo.hideIndex);
                    writer.Write(levelObject.aiInfo.pathLength);
                    writer.Write(levelObject.aiInfo.curPathIndex);

                    if (GameDataVersion <= 25)
                    {
                        // Follow path start/end segment; not needed
                        writer.Write((short)0);
                        writer.Write((short)0);
                    }

                    break;
                case ControlType.Explosion:
                    writer.Write(levelObject.explosionInfo.SpawnTime.value);
                    writer.Write(levelObject.explosionInfo.DeleteTime.value);
                    writer.Write(levelObject.explosionInfo.DeleteObject);
                    break;
                case ControlType.Powerup:
                    if (GameDataVersion >= 25)
                    {
                        writer.Write(levelObject.powerupCount);
                    }
                    break;
            }
            switch (levelObject.renderType)
            {
                case RenderType.Polyobj:
                    {
                        writer.Write(levelObject.modelInfo.modelNum);
                        for (int i = 0; i < Polymodel.MAX_SUBMODELS; i++)
                        {
                            WriteFixAngles(writer, levelObject.modelInfo.animAngles[i]);
                        }
                        writer.Write(levelObject.modelInfo.flags);
                        writer.Write(levelObject.modelInfo.textureOverride);
                    }
                    break;
                case RenderType.WeaponVClip:
                case RenderType.Hostage:
                case RenderType.Powerup:
                case RenderType.Fireball:
                    writer.Write(levelObject.spriteInfo.vclipNum);
                    writer.Write(levelObject.spriteInfo.frameTime.value);
                    writer.Write(levelObject.spriteInfo.frameNumber);
                    break;
            }
        }

        protected static byte[] EncodeString(string input, int maxLength, bool variableLength)
        {
            // Variable-length strings are null-terminated, so have to leave space for that
            int stringLength = Math.Min(input.Length, variableLength ? maxLength - 1 : maxLength);
            if (variableLength)
            {
                return Encoding.ASCII.GetBytes(input.Substring(0, stringLength) + '\0');
            }
            else
            {
                byte[] stringBuffer = new byte[maxLength];
                Encoding.ASCII.GetBytes(input.Substring(0, stringLength)).CopyTo(stringBuffer, 0);
                return stringBuffer;
            }
        }

        protected static void WriteFixVector(BinaryWriter writer, FixVector vector)
        {
            writer.Write(vector.x.value);
            writer.Write(vector.y.value);
            writer.Write(vector.z.value);
        }

        protected static void WriteFixAngles(BinaryWriter writer, FixAngles angles)
        {
            writer.Write(angles.p);
            writer.Write(angles.b);
            writer.Write(angles.h);
        }

        protected static void WriteFixMatrix(BinaryWriter writer, FixMatrix matrix)
        {
            WriteFixVector(writer, matrix.right);
            WriteFixVector(writer, matrix.up);
            WriteFixVector(writer, matrix.forward);
        }

        protected abstract void WriteVersionSpecificLevelInfo(BinaryWriter writer);
        protected abstract void WriteTrigger(BinaryWriter writer, ITrigger trigger);
        protected abstract void WriteDynamicLights(BinaryWriter writer, ref FileInfo fileInfo);
    }

    internal class D1LevelWriter : DescentLevelWriter
    {
        private readonly D1Level _level;

        protected override ILevel Level => _level;
        protected override int LevelVersion => 1;
        protected override ushort GameDataVersion => 25;

        public D1LevelWriter(D1Level level, Stream stream)
        {
            _stream = stream;
            _level = level;
        }

        protected override void WriteDynamicLights(BinaryWriter writer, ref FileInfo fileInfo)
        {
            // Only needed for D2, shouldn't be called for D1
            throw new NotImplementedException();
        }

        protected override void WriteTrigger(BinaryWriter writer, ITrigger trigger)
        {
            var d1trigger = (D1Trigger)trigger;
            writer.Write((byte)d1trigger.Type);
            writer.Write((ushort)d1trigger.Flags);
            writer.Write(((Fix)d1trigger.Value).value);
            writer.Write(d1trigger.Time);
            writer.Write((byte)0); // link_num
            writer.Write((short)d1trigger.Targets.Count);

            for (int i = 0; i < D1Trigger.MaxWallsPerLink; i++)
            {
                if (i < d1trigger.Targets.Count)
                {
                    writer.Write((short)Level.Segments.IndexOf(d1trigger.Targets[i].Segment));
                }
                else
                {
                    writer.Write((short)0);
                }
            }
            for (int i = 0; i < D1Trigger.MaxWallsPerLink; i++)
            {
                if (i < d1trigger.Targets.Count)
                {
                    writer.Write((short)d1trigger.Targets[i].SideNum);
                }
                else
                {
                    writer.Write((short)0);
                }
            }
        }

        protected override void WriteVersionSpecificLevelInfo(BinaryWriter writer)
        {
        }
    }

    internal class D2LevelWriter : DescentLevelWriter
    {
        private readonly D2Level _level;

        protected override ILevel Level => _level;
        protected override int LevelVersion { get; }
        protected override ushort GameDataVersion => 32;

        public D2LevelWriter(D2Level level, Stream stream, bool vertigoCompatible)
        {
            _stream = stream;
            _level = level;
            LevelVersion = vertigoCompatible ? 8 : 7;
        }

        protected override void WriteVersionSpecificLevelInfo(BinaryWriter writer)
        {
            var encodedPaletteName = EncodeString(_level.PaletteName, 13, true);
            // Newline-terminated
            encodedPaletteName[encodedPaletteName.Length - 1] = (byte)'\n';
            writer.Write(encodedPaletteName);

            writer.Write(_level.BaseReactorCountdownTime);
            writer.Write(_level.ReactorStrength.HasValue ? _level.ReactorStrength.Value : -1);

            writer.Write(_level.AnimatedLights.Count);
            foreach (var light in _level.AnimatedLights)
            {
                writer.Write((short)Level.Segments.IndexOf(light.Side.Segment));
                writer.Write((short)light.Side.SideNum);
                writer.Write(light.Mask);
                writer.Write(light.TimeToNextTick.value);
                writer.Write(light.TickLength.value);
            }

            writer.Write(Level.Segments.IndexOf(_level.SecretReturnSegment));
            WriteFixVector(writer, _level.SecretReturnOrientation.right);
            WriteFixVector(writer, _level.SecretReturnOrientation.forward);
            WriteFixVector(writer, _level.SecretReturnOrientation.up);
        }

        protected override void WriteTrigger(BinaryWriter writer, ITrigger trigger)
        {
            var d2trigger = (D2Trigger)trigger;
            writer.Write((byte)d2trigger.Type);
            writer.Write((byte)d2trigger.Flags);
            writer.Write((sbyte)d2trigger.Targets.Count);
            writer.Write((byte)0); // padding byte
            writer.Write(((Fix)d2trigger.Value).value);
            writer.Write(d2trigger.Time);
            for (int i = 0; i < D2Trigger.MaxWallsPerLink; i++)
            {
                if (i < d2trigger.Targets.Count)
                {
                    writer.Write((short)Level.Segments.IndexOf(d2trigger.Targets[i].Segment));
                }
                else
                {
                    writer.Write((short)0);
                }
            }
            for (int i = 0; i < D2Trigger.MaxWallsPerLink; i++)
            {
                if (i < d2trigger.Targets.Count)
                {
                    writer.Write((short)d2trigger.Targets[i].SideNum);
                }
                else
                {
                    writer.Write((short)0);
                }
            }
        }

        protected override void WriteDynamicLights(BinaryWriter writer, ref FileInfo fileInfo)
        {
            // Need to concatenate all light deltas for all dynamic lights into a list
            var lightDeltas = new List<LightDelta>();

            fileInfo.deltaLightIndicesOffset = (_level.DynamicLights.Count > 0) ?
                (int)writer.BaseStream.Position : -1;
            fileInfo.deltaLightIndicesCount = _level.DynamicLights.Count;
            foreach (var light in _level.DynamicLights)
            {
                // If we run out of space for light deltas, stop writing more
                var numDeltasToAdd = Math.Min(light.LightDeltas.Count, sbyte.MaxValue);
                numDeltasToAdd = Math.Min(numDeltasToAdd, short.MaxValue - lightDeltas.Count);

                writer.Write((short)Level.Segments.IndexOf(light.Source.Segment));
                writer.Write((byte)light.Source.SideNum);
                writer.Write((byte)numDeltasToAdd);
                writer.Write((short)lightDeltas.Count);
                lightDeltas.AddRange(light.LightDeltas.Take(numDeltasToAdd));

                if (lightDeltas.Count == short.MaxValue)
                {
                    break;
                }
            }
            fileInfo.deltaLightIndicesSize = (_level.DynamicLights.Count > 0) ?
                (int)writer.BaseStream.Position - fileInfo.deltaLightIndicesOffset : 0;

            fileInfo.deltaLightsOffset = (lightDeltas.Count > 0) ?
                (int)writer.BaseStream.Position : -1;
            fileInfo.deltaLightsCount = lightDeltas.Count;
            foreach (var lightDelta in lightDeltas)
            {
                writer.Write((short)Level.Segments.IndexOf(lightDelta.targetSide.Segment));
                writer.Write((byte)lightDelta.targetSide.SideNum);
                writer.Write((byte)0);
                writer.Write((byte)(lightDelta.vertexDeltas[0].value / 2048));
                writer.Write((byte)(lightDelta.vertexDeltas[1].value / 2048));
                writer.Write((byte)(lightDelta.vertexDeltas[2].value / 2048));
                writer.Write((byte)(lightDelta.vertexDeltas[3].value / 2048));
            }
            fileInfo.deltaLightsSize = (lightDeltas.Count > 0) ?
                (int)writer.BaseStream.Position - fileInfo.deltaLightsOffset : 0;
        }
    }
}