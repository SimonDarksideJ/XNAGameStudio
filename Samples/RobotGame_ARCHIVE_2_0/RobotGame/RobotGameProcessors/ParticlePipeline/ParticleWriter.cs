#region File Description
//-----------------------------------------------------------------------------
// ParticleWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RobotGameData.ParticleSystem;
#endregion

namespace ParticlePipeline
{
    public class WriteContentData
    {
        ParticleSequenceInfo data = null;

        public ParticleSequenceInfo Data
        {
            get { return data; }
        }

        public WriteContentData()
        {
            this.data = new ParticleSequenceInfo();
        }

        public WriteContentData(ParticleSequenceInfo data)
        {
            this.data = data;
        }
    }

    /// <summary>
    /// Content Pipeline class for saving ParticleSequenceInfo data into XNB format.
    /// </summary>
    [ContentTypeWriter]
    public class ParticleWriter : ContentTypeWriter<WriteContentData>
    {
        ContentWriter output = null;

        protected override void Write(ContentWriter output, WriteContentData value)
        {
            this.output = output;

            WriteParticleSequenceInfo(value);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(ParticleSequenceInfo).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "RobotGameData.ParticleSystem.ParticleReader, " +
                    "RobotGameData, Version=1.0.0.0, Culture=neutral";
        }

        private void WriteParticleSequenceInfo(WriteContentData value)
        {
            output.Write(value.Data.Name);
            output.Write(value.Data.TimeSequencesInfo.Count);

            // write TimeSequenceInfo structure.
            foreach (TimeSequenceInfo timeSequenceInfo in value.Data.TimeSequencesInfo)
            {                
                WriteTimeSequenceInfo(timeSequenceInfo);
            }
        }

        private void WriteTimeSequenceInfo(TimeSequenceInfo timeSequenceInfo)
        {
            output.Write(timeSequenceInfo.StartTime);
            output.Write(timeSequenceInfo.Duration);
            output.Write((int)timeSequenceInfo.Style);

            // writes a ParticleInfo structure.
            if (timeSequenceInfo.ParticleInfo != null)
            {
                output.Write(true);

                WriteParticleInfo(timeSequenceInfo.ParticleInfo);
            }
            else
            {
                output.Write(false);
            }

            // writes a TextureSequence structure.
            if (timeSequenceInfo.TextureSequence != null)
            {
                output.Write(true);

                WriteTextureSequence(timeSequenceInfo.TextureSequence);
            }
            else
            {
                output.Write(false);
            }
        }

        private void WriteParticleInfo(ParticleInfo info)
        {
            output.Write(info.Name);
            output.Write((int)info.ParticleType);

            if (info.MeshData != null)
            {
                output.Write(true);

                // writes a MeshData structure.
                WriteMeshData(info.MeshData);
            }
            else
            {
                output.Write(false);
            }

            output.Write(info.TextureFileName);
            output.Write(info.AlphaBlendEnable);
            output.Write(info.DepthBufferEnable);
            output.Write((int)info.SourceBlend);
            output.Write((int)info.DestinationBlend);
            output.Write((int)info.BlendFunction);
            output.Write(info.LifeTime);
            output.Write(info.ObjectLifeTime);
            output.Write(info.MassMin);
            output.Write(info.MassMax);
            output.Write(info.InitialObjectCount);
            output.Write(info.MaxObjectCount);
            output.Write(info.EmitCount);
            output.Write(info.Volatile);

            output.Write((int)info.EmitType);
            output.Write(info.EmitPosition);
            output.Write(info.EmitDirection);
            output.Write(info.EmitAngle);
            output.Write(info.EmitInterval);
            output.Write(info.UpVector);

            /////////////////////////////////////////// Position
            output.Write(info.PositionStyle);
            output.Write(info.PositionUpdateInterval);

            // write PositionFunc structures.
            if (info.PositionFunc != null)
            {
                output.Write(info.PositionFunc.Count);

                for (int i = 0; i < info.PositionFunc.Count; i++)
                    output.Write((int)info.PositionFunc[i]);
            }
            else
            {
                output.Write((int)0);
            }

            // write PositionInit structures.
            if (info.PositionInit != null)
            {
                output.Write(info.PositionInit.Count);

                for (int i = 0; i < info.PositionInit.Count; i++)
                    output.Write(info.PositionInit[i]);
            }
            else
            {
                output.Write((int)0);
            }

            // write PositionFactor structures.
            if (info.PositionFactor != null)
            {
                output.Write(info.PositionFactor.Count);

                for (int i = 0; i < info.PositionFactor.Count; i++)
                    output.Write(info.PositionFactor[i]);
            }
            else
            {
                output.Write((int)0);
            }

            // write PositionTable structures.
            if (info.PositionTable != null)
            {
                output.Write(info.PositionTable.Count);

                for (int i = 0; i < info.PositionTable.Count; i++)
                    WriteKeyFrameTable(info.PositionTable[i]);
            }
            else
            {
                output.Write((int)0);
            }

            output.Write(info.PositionMin);
            output.Write(info.PositionMax);
            output.Write(info.PositionInitialRandomFactor);
            output.Write(info.PositionRandomFactor);
            output.Write(info.PositionRandomInterval);

            /////////////////////////////////////////// Scale
            output.Write(info.ScaleStyle);
            output.Write(info.ScaleUpdateInterval);

            // write ScaleFunc structures.
            if (info.ScaleFunc != null)
            {
                output.Write(info.ScaleFunc.Count);

                for (int i = 0; i < info.ScaleFunc.Count; i++)
                    output.Write((int)info.ScaleFunc[i]);
            }
            else
            {
                output.Write((int)0);
            }

            // write ScaleInit structures.
            if (info.ScaleInit != null)
            {
                output.Write(info.ScaleInit.Count);

                for (int i = 0; i < info.ScaleInit.Count; i++)
                    output.Write(info.ScaleInit[i]);
            }
            else
            {
                output.Write((int)0);
            }

            // write ScaleFactor structures.
            if (info.ScaleFactor != null)
            {
                output.Write(info.ScaleFactor.Count);

                for (int i = 0; i < info.ScaleFactor.Count; i++)
                    output.Write(info.ScaleFactor[i]);
            }
            else
            {
                output.Write((int)0);
            }

            // write ScaleTable structures.
            if (info.ScaleTable != null)
            {
                output.Write(info.ScaleTable.Count);

                for (int i = 0; i < info.ScaleTable.Count; i++)
                    WriteKeyFrameTable(info.ScaleTable[i]);
            }
            else
            {
                output.Write((int)0);
            }

            output.Write(info.ScaleInitialRandomFactor);
            output.Write(info.ScaleMin);
            output.Write(info.ScaleMax);
            output.Write(info.ScaleMask);
            output.Write(info.ScaleBillboardFactor);

            /////////////////////////////////////////// Rotate
            output.Write(info.RotateStyle);
            output.Write(info.RotateUpdateInterval);
            output.Write(info.RotateRandomFactor);
            output.Write((int)info.RotateFunc);
            output.Write(info.RotateInit);
            output.Write(info.RotateFactor);

            // write RotateTable structures.
            if (info.RotateTable != null)
            {
                output.Write(info.RotateTable.Count);
                    
                WriteKeyFrameTable(info.RotateTable);
            }
            else
            {
                output.Write((int)0);
            }

            /////////////////////////////////////////// Color
            output.Write(info.ColorStyle);
            output.Write(info.ColorUpdateInterval);
            output.Write((int)info.RgbFunc);
            output.Write(info.RgbInit);

            // write Rtable structures.
            if (info.Rtable != null)
            {
                output.Write(info.Rtable.Count);

                WriteKeyFrameTable(info.Rtable);
            }
            else
            {
                output.Write((int)0);
            }

            // write Gtable structures.
            if (info.Gtable != null)
            {
                output.Write(info.Gtable.Count);

                WriteKeyFrameTable(info.Gtable);
            }
            else
            {
                output.Write((int)0);
            }

            // write Btable structures.
            if (info.Btable != null)
            {
                output.Write(info.Btable.Count);

                WriteKeyFrameTable(info.Btable);
            }
            else
            {
                output.Write((int)0);
            }

            // write Atable structures.
            if (info.Atable != null)
            {
                output.Write(info.Atable.Count);

                WriteKeyFrameTable(info.Atable);
            }
            else
            {
                output.Write((int)0);
            }

            output.Write((int)info.AlphaFunc);
            output.Write(info.AlphaInit);
        }

        private void WriteTextureSequence(TextureSequence textureSequence)
        {
            output.Write(textureSequence.TextureFileName);
            output.Write(textureSequence.IsUseStaticTime);
            output.Write(textureSequence.IsRepeat);
            output.Write(textureSequence.IsRandomMode);
            output.Write(textureSequence.IsFixedFrameMode);
            output.Write(textureSequence.FrameWidth);
            output.Write(textureSequence.FrameHeight);
            output.Write(textureSequence.StaticInterval);
            output.Write(textureSequence.Count);
            output.Write(textureSequence.StartIndex);

            if (textureSequence.TimeTable != null)
            {
                output.Write(textureSequence.TimeTable.Count);

                for (int i = 0; i < textureSequence.TimeTable.Count; i++)
                    output.Write(textureSequence.TimeTable[i]);
            }
            else
            {
                output.Write((int)0);
            }
        }

        private void WriteKeyFrameTable(KeyFrameTable table)
        {
            output.Write(table.Count);
            output.Write(table.IsFixedInterval);

            if (table.Table != null)
            {
                output.Write(table.Table.Count);

                for (int i = 0; i < table.Table.Count; i++)
                    output.Write(table.Table[i]);
            }
            else
            {
                output.Write((int)0);
            }

            if (table.Time != null)
            {
                output.Write(table.Time.Count);

                for (int i = 0; i < table.Time.Count; i++)
                    output.Write(table.Time[i]);
            }
            else
            {
                output.Write((int)0);
            }
        }

        private void WriteMeshData(VertexData data)
        {
            output.Write(data.HasPosition);
            output.Write(data.HasNormal);
            output.Write(data.HasColor);
            output.Write(data.HasTextureCoord);
            output.Write(data.HasIndex);

            if (data.HasPosition)
            {
                output.Write(data.Position.Count);

                // write positions.
                for (int i = 0; i < data.Position.Count; i++)
                {
                    output.Write(data.Position[i]);
                }
            }
            else
            {
                output.Write((int)0);
            }

            if (data.HasNormal)
            {
                output.Write(data.Normal.Count);

                // write normals.
                for (int i = 0; i < data.Normal.Count; i++)
                {
                    output.Write(data.Normal[i]);
                }
            }
            else
            {
                output.Write((int)0);
            }

            if (data.HasColor)
            {
                output.Write(data.Color.Count);

                // write colors.
                for (int i = 0; i < data.Color.Count; i++)
                {
                    output.Write(data.Color[i].PackedValue);
                }
            }
            else
            {
                output.Write((int)0);
            }

            if (data.HasTextureCoord)
            {
                output.Write(data.TextureCoord.Count);

                // write texture coordinates.
                for (int i = 0; i < data.TextureCoord.Count; i++)
                {
                    output.Write(data.TextureCoord[i]);
                }
            }
            else
            {
                output.Write((int)0);
            }

            if (data.HasIndex)
            {
                output.Write(data.Index.Count);

                // write indices.
                for (int i = 0; i < data.Index.Count; i++)
                {
                    output.Write(data.Index[i]);
                }
            }
            else
            {
                output.Write((int)0);
            }
        }
    }
}
