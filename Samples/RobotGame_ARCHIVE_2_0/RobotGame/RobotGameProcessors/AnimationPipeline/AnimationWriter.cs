#region File Description
//-----------------------------------------------------------------------------
// AnimationWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using RobotGameData.GameObject;
#endregion

namespace AnimationPipeline
{
    public class WriteContentData
    {
        AnimationSequence data = null;

        public AnimationSequence Data
        {
            get { return data; }
        }

        public WriteContentData()
        {
            this.data = new AnimationSequence();
        }

        public WriteContentData(AnimationSequence data)
        {
            this.data = data;
        }
    }

    /// <summary>
    /// Content Pipeline class for saving AnimationSequence data into XNB format.
    /// </summary>
    [ContentTypeWriter]
    public class AnimationWriter : ContentTypeWriter<WriteContentData>
    {
        ContentWriter output = null;

        protected override void Write(ContentWriter output, WriteContentData value)
        {
            this.output = output;

            WriteAnimationSequence(value.Data);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(AnimationSequence).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "RobotGameData.GameObject.AnimationReader, " +
                    "RobotGameData, Version=1.0.0.0, Culture=neutral";
        }

        private void WriteAnimationSequence(AnimationSequence value)
        {
            output.Write(value.KeyFrameSequenceCount);
            output.Write(value.Duration);

            for (int i = 0; i < value.KeyFrameSequences.Count; i++)
            {
                WriteKeyFrameSequence(value.KeyFrameSequences[i]);
            }
        }

        private void WriteKeyFrameSequence(KeyFrameSequence value)
        {
            output.Write(value.BoneName);
            output.Write(value.KeyCount);
            output.Write(value.Duration);
            output.Write(value.KeyInterval);

            output.Write(value.HasTranslation);
            output.Write(value.HasRotation);
            output.Write(value.HasScale);
            output.Write(value.HasTime);

            output.Write(value.FixedTranslation);
            output.Write(value.FixedRotation);
            output.Write(value.FixedScale);

            // write position values.
            output.Write(value.Translation.Count);

            for (int i = 0; i < value.Translation.Count; i++)
                output.Write(value.Translation[i]);

            // write rotation values.
            output.Write(value.Rotation.Count);

            for (int i = 0; i < value.Rotation.Count; i++)
                output.Write(value.Rotation[i]);

            // write scale values.
            output.Write(value.Scale.Count);

            for (int i = 0; i < value.Scale.Count; i++)
                output.Write(value.Scale[i]);

            // write time values.
            output.Write(value.Time.Count);

            for (int i = 0; i < value.Time.Count; i++)
                output.Write(value.Time[i]);
        }
    }
}