#region File Description
//-----------------------------------------------------------------------------
// AnimationSequence.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using RobotGameData.Resource;
#endregion

namespace RobotGameData.GameObject
{
    /// <summary>
    /// this is the basic unit of an animation that has key frame of several bones.
    /// It can load an XML file(.Animation).
    /// </summary>
    [Serializable]
    public class AnimationSequence
    {
        #region Fields

        public int KeyFrameSequenceCount = 0;

        /// <summary>
        /// KeyFrame duration time
        /// </summary>
        public float Duration = 0.0f;

        /// <summary>
        /// KeyFrames container
        /// </summary>
        public List<KeyFrameSequence> KeyFrameSequences = null;

        #endregion
      
        /// <summary>
        /// Gets the key frame sequence by index
        /// </summary>
        public KeyFrameSequence GetKeyFrameSequence(int index)
        {
            return KeyFrameSequences[index];
        }

        /// <summary>
        /// Gets the bone name of the key frame sequence
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetKeyFrameSequenceBoneName(int index)
        {
            return GetKeyFrameSequence(index).BoneName;
        }
    }
}
