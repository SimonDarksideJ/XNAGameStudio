#region File Description
//-----------------------------------------------------------------------------
// ParticleReader.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace RobotGameData.ParticleSystem
{
    /// <summary>
    /// Content Pipeline class for loading ParticleSequenceInfo data from XNB format.
    /// </summary>
    public class ParticleReader : ContentTypeReader<ParticleSequenceInfo>
    {
        ContentReader input = null;

        protected override ParticleSequenceInfo Read(ContentReader input,
                                                ParticleSequenceInfo existingInstance)
        {
            this.input = input;

            return ReadParticeSequenceInfo();
        }

        private ParticleSequenceInfo ReadParticeSequenceInfo()
        {
            ParticleSequenceInfo newParticleInfo = new ParticleSequenceInfo();

            newParticleInfo.Name = input.ReadString();

            int count = input.ReadInt32();

            // read TimeSequenceInfo structures.
            for (int i = 0; i < count; i++)
            {
                newParticleInfo.TimeSequencesInfo.Add(ReadTimeSequenceInfo());
            }

            return newParticleInfo;
        }

        private TimeSequenceInfo ReadTimeSequenceInfo()
        {
            TimeSequenceInfo timeSequenceInfo = new TimeSequenceInfo();

            timeSequenceInfo.StartTime = input.ReadSingle();
            timeSequenceInfo.Duration = input.ReadSingle();
            timeSequenceInfo.Style = (SequenceStyle)input.ReadInt32();

            // read a ParticleInfo structure.
            if( input.ReadBoolean() )
                timeSequenceInfo.ParticleInfo = ReadParticleInfo();

            // read a TextureSequence structure.
            if (input.ReadBoolean() )
                timeSequenceInfo.TextureSequence = ReadTextureSequence();

            return timeSequenceInfo;
        }

        private ParticleInfo ReadParticleInfo()
        {
            ParticleInfo particleInfo = new ParticleInfo();

            particleInfo.Name = input.ReadString();
            particleInfo.ParticleType = 
                (ParticleInfo.ParticleObjectType)input.ReadInt32();

            // read a MeshData structure.
            if (input.ReadBoolean() )
            {
                particleInfo.MeshData = ReadMeshData();
            }

            particleInfo.TextureFileName = input.ReadString();
            particleInfo.AlphaBlendEnable = input.ReadBoolean();
            particleInfo.DepthBufferEnable = input.ReadBoolean();
            particleInfo.SourceBlend = (Blend)input.ReadInt32();
            particleInfo.DestinationBlend = (Blend)input.ReadInt32();
            particleInfo.BlendFunction = (BlendFunction)input.ReadInt32();
            particleInfo.LifeTime = input.ReadSingle();
            particleInfo.ObjectLifeTime = input.ReadSingle();
            particleInfo.MassMin = input.ReadSingle();
            particleInfo.MassMax = input.ReadSingle();
            particleInfo.InitialObjectCount = input.ReadInt32();
            particleInfo.MaxObjectCount = input.ReadInt32();
            particleInfo.EmitCount = input.ReadInt32();
            particleInfo.Volatile = input.ReadBoolean();

            particleInfo.EmitType = (ParticleInfo.EmitObjectType)input.ReadInt32();
            particleInfo.EmitPosition = input.ReadVector3();
            particleInfo.EmitDirection = input.ReadVector3();
            particleInfo.EmitAngle = input.ReadSingle();
            particleInfo.EmitInterval = input.ReadSingle();
            particleInfo.UpVector = input.ReadVector3();

            /////////////////////////////////////////// Position
            particleInfo.PositionStyle = input.ReadUInt32();
            particleInfo.PositionUpdateInterval = input.ReadSingle();

            // read PositionFunc structures.
            int positionFuncCount = input.ReadInt32();
            if (positionFuncCount > 0)
            {
                particleInfo.PositionFunc = new List<ParticleInfo.FuncType>();

                for (int i = 0; i < positionFuncCount; i++)
                {
                    particleInfo.PositionFunc.Add(
                        (ParticleInfo.FuncType)input.ReadInt32());
                }
            }

            // read PositionInit structures.
            int positionInitCount = input.ReadInt32();
            if (positionInitCount > 0)
            {
                particleInfo.PositionInit = new List<float>();

                for (int i = 0; i < positionInitCount; i++)
                    particleInfo.PositionInit.Add(input.ReadSingle());
            }

            // read PositionFactor structures.
            int positionFactorCount = input.ReadInt32();
            if (positionFactorCount > 0)
            {
                particleInfo.PositionFactor = new List<float>();

                for (int i = 0; i < positionFactorCount; i++)
                    particleInfo.PositionFactor.Add(input.ReadSingle());
            }

            // read PositionTable structures.
            int positionTableCount = input.ReadInt32();
            if (positionTableCount > 0)
            {
                particleInfo.PositionTable = new List<KeyFrameTable>();

                for (int i = 0; i < positionTableCount; i++)
                    particleInfo.PositionTable.Add(ReadKeyFrameTable());
            }

            particleInfo.PositionMin = input.ReadSingle();
            particleInfo.PositionMax = input.ReadSingle();
            particleInfo.PositionInitialRandomFactor = input.ReadSingle();
            particleInfo.PositionRandomFactor = input.ReadVector3();
            particleInfo.PositionRandomInterval = input.ReadSingle();

            /////////////////////////////////////////// Scale
            particleInfo.ScaleStyle = input.ReadUInt32();
            particleInfo.ScaleUpdateInterval = input.ReadSingle();

            // read ScaleFunc structures.
            int scaleFuncCount = input.ReadInt32();
            if (scaleFuncCount > 0)
            {
                particleInfo.ScaleFunc = new List<ParticleInfo.FuncType>();

                for (int i = 0; i < scaleFuncCount; i++)
                    particleInfo.ScaleFunc.Add((ParticleInfo.FuncType)input.ReadInt32());
            }

            // read ScaleInit structures.
            int scaleInitCount = input.ReadInt32();
            if (scaleInitCount > 0)
            {
                particleInfo.ScaleInit = new List<float>();

                for (int i = 0; i < scaleInitCount; i++)
                    particleInfo.ScaleInit.Add(input.ReadSingle());
            }

            // read ScaleFactor structures.
            int scaleFactorCount = input.ReadInt32();
            if (scaleFactorCount > 0)
            {
                particleInfo.ScaleFactor = new List<float>();

                for (int i = 0; i < scaleFactorCount; i++)
                    particleInfo.ScaleFactor.Add(input.ReadSingle());
            }

            // read ScaleTable structures.
            int scaleTableCount = input.ReadInt32();
            if (scaleTableCount > 0)
            {
                particleInfo.ScaleTable = new List<KeyFrameTable>();

                for (int i = 0; i < scaleTableCount; i++)
                    particleInfo.ScaleTable.Add(ReadKeyFrameTable());
            }

            particleInfo.ScaleInitialRandomFactor = input.ReadSingle();
            particleInfo.ScaleMin = input.ReadSingle();
            particleInfo.ScaleMax = input.ReadSingle();
            particleInfo.ScaleMask = input.ReadVector3();
            particleInfo.ScaleBillboardFactor = input.ReadSingle();

            /////////////////////////////////////////// Rotate
            particleInfo.RotateStyle = input.ReadUInt32();
            particleInfo.RotateUpdateInterval = input.ReadSingle();
            particleInfo.RotateRandomFactor = input.ReadSingle();
            particleInfo.RotateFunc = (ParticleInfo.FuncType)input.ReadInt32();
            particleInfo.RotateInit = input.ReadSingle();
            particleInfo.RotateFactor = input.ReadSingle();

            // read RotateTable structures.
            if (input.ReadInt32() > 0)
            {
                particleInfo.RotateTable = ReadKeyFrameTable();
            }

            /////////////////////////////////////////// Color
            particleInfo.ColorStyle = input.ReadUInt32();
            particleInfo.ColorUpdateInterval = input.ReadSingle();
            particleInfo.RgbFunc = (ParticleInfo.FuncType)input.ReadInt32();
            particleInfo.RgbInit = input.ReadString();

            // read Rtable structures.
            if (input.ReadInt32() > 0)
            {
                particleInfo.Rtable = ReadKeyFrameTable();
            }

            // read Gtable structures.
            if (input.ReadInt32() > 0)
            {
                particleInfo.Gtable = ReadKeyFrameTable();
            }

            // read Btable structures.
            if (input.ReadInt32() > 0)
            {
                particleInfo.Btable = ReadKeyFrameTable();
            }

            // read Atable structures.
            if (input.ReadInt32() > 0)
            {
                particleInfo.Atable = ReadKeyFrameTable();
            }

            particleInfo.AlphaFunc = (ParticleInfo.FuncType)input.ReadInt32();
            particleInfo.AlphaInit = input.ReadUInt32();

            return particleInfo;
        }

        private TextureSequence ReadTextureSequence()
        {
            TextureSequence textureSequence = new TextureSequence();

            textureSequence.TextureFileName = input.ReadString();
            textureSequence.IsUseStaticTime = input.ReadBoolean();
            textureSequence.IsRepeat = input.ReadBoolean();
            textureSequence.IsRandomMode = input.ReadBoolean();
            textureSequence.IsFixedFrameMode = input.ReadBoolean();
            textureSequence.FrameWidth = input.ReadSingle();
            textureSequence.FrameHeight = input.ReadSingle();
            textureSequence.StaticInterval = input.ReadSingle();
            textureSequence.Count = input.ReadUInt32();
            textureSequence.StartIndex = input.ReadUInt32();

            // read TimeTable structures.
            int timeTableCount = input.ReadInt32();
            if (timeTableCount > 0)
            {
                textureSequence.TimeTable = new List<float>();

                for (int i = 0; i < timeTableCount; i++)
                    textureSequence.TimeTable.Add(input.ReadSingle());
            }

            return textureSequence;
        }


        private KeyFrameTable ReadKeyFrameTable()
        {
            KeyFrameTable table = new KeyFrameTable();

            table.Count = input.ReadInt32();
            table.IsFixedInterval = input.ReadBoolean();

            int tableCount = input.ReadInt32();
            if (tableCount > 0)
            {
                table.Table = new List<float>();

                for (int i = 0; i < tableCount; i++)
                    table.Table.Add(input.ReadSingle());
            }

            int timeCount = input.ReadInt32();
            if (timeCount > 0)
            {
                table.Time = new List<float>();

                for (int i = 0; i < timeCount; i++)
                    table.Time.Add(input.ReadSingle());
            }

            return table;
        }

        private VertexData ReadMeshData()
        {
            VertexData data = new VertexData();

            data.HasPosition = input.ReadBoolean();
            data.HasNormal = input.ReadBoolean();
            data.HasColor = input.ReadBoolean();
            data.HasTextureCoord = input.ReadBoolean();
            data.HasIndex = input.ReadBoolean();

            int positionCount = input.ReadInt32();
            if (positionCount > 0)
            {
                data.Position = new List<Vector3>();

                // read positions.
                for (int i = 0; i < positionCount; i++)
                {
                    data.Position.Add(input.ReadVector3());
                }
            }

            int normalCount = input.ReadInt32();
            if (normalCount > 0)
            {
                data.Normal = new List<Vector3>();

                // read normals.
                for (int i = 0; i < normalCount; i++)
                {
                    data.Normal.Add(input.ReadVector3());
                }
            }

            int colorCount = input.ReadInt32();
            if (colorCount > 0)
            {
                data.Color = new List<Color>();

                // read colors.
                for (int i = 0; i < colorCount; i++)
                {
                    Color color = new Color();
                    color.PackedValue = input.ReadUInt32();

                    data.Color.Add(color);
                }
            }

            int textureCoordCount = input.ReadInt32();
            if (textureCoordCount > 0)
            {
                data.TextureCoord = new List<Vector2>();

                // read texture coordinates.
                for (int i = 0; i < textureCoordCount; i++)
                {
                    data.TextureCoord.Add(input.ReadVector2());
                }
            }

            int indexCount = input.ReadInt32();
            if (indexCount > 0)
            {
                data.Index = new List<short>();

                // read indices.
                for (int i = 0; i < indexCount; i++)
                {
                    data.Index.Add(input.ReadInt16());
                }
            }

            return data;
        }
    }
}
