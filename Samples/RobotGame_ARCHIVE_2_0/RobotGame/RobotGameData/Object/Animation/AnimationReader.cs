#region File Description
//-----------------------------------------------------------------------------
// AnimationReader.cs
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

namespace RobotGameData.GameObject
{
    /// <summary>
    /// Content Pipeline class for loading AnimationSequence data from XNB format.
    /// </summary>  
    public class AnimationReader : ContentTypeReader<AnimationSequence>
    {
        ContentReader input = null;

        protected override AnimationSequence Read(ContentReader input,
                                                    AnimationSequence existingInstance)
        {
            this.input = input;

            return ReadAnimationSequence();
        }

        private AnimationSequence ReadAnimationSequence()
        {
            AnimationSequence animationSequence = new AnimationSequence();

            animationSequence.KeyFrameSequenceCount = input.ReadInt32();
            animationSequence.Duration = input.ReadSingle();

            if (animationSequence.KeyFrameSequenceCount > 0)
            {
                animationSequence.KeyFrameSequences = new List<KeyFrameSequence>();

                for (int i = 0; i < animationSequence.KeyFrameSequenceCount; i++)
                    animationSequence.KeyFrameSequences.Add(ReadKeyFrameSequence());
            }

            return animationSequence;
        }

        private KeyFrameSequence ReadKeyFrameSequence()
        {
            KeyFrameSequence keyFrameSequence = new KeyFrameSequence();

            keyFrameSequence.BoneName = input.ReadString();
            keyFrameSequence.KeyCount = input.ReadInt32();
            keyFrameSequence.Duration = input.ReadSingle();
            keyFrameSequence.KeyInterval = input.ReadSingle();

            keyFrameSequence.HasTranslation = input.ReadBoolean();
            keyFrameSequence.HasRotation = input.ReadBoolean();
            keyFrameSequence.HasScale = input.ReadBoolean();
            keyFrameSequence.HasTime = input.ReadBoolean();

            keyFrameSequence.FixedTranslation = input.ReadBoolean();
            keyFrameSequence.FixedRotation = input.ReadBoolean();
            keyFrameSequence.FixedScale = input.ReadBoolean();

            // read position values.
            int translationCount = input.ReadInt32();
            if (translationCount > 0)
            {
                keyFrameSequence.Translation = new List<Vector3>();

                for (int i = 0; i < translationCount; i++)
                    keyFrameSequence.Translation.Add(input.ReadVector3());
            }

            // read rotation values.
            int rotationCount = input.ReadInt32();
            if (rotationCount > 0)
            {
                keyFrameSequence.Rotation = new List<Quaternion>();

                for (int i = 0; i < rotationCount; i++)
                    keyFrameSequence.Rotation.Add(input.ReadQuaternion());
            }

            // read scale values.
            int scaleCount = input.ReadInt32();
            if (scaleCount > 0)
            {
                keyFrameSequence.Scale = new List<Vector3>();

                for (int i = 0; i < scaleCount; i++)
                    keyFrameSequence.Scale.Add(input.ReadVector3());
            }

            // read time values.
            int timeCount = input.ReadInt32();
            if (timeCount > 0)
            {
                keyFrameSequence.Time = new List<float>();

                for (int i = 0; i < timeCount; i++)
                    keyFrameSequence.Time.Add(input.ReadSingle());
            }

            return keyFrameSequence;
        }
    }
}