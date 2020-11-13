#region File Description
//-----------------------------------------------------------------------------
// SkinningData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace SkinnedModel
{
    /// <summary>
    /// Combines all the data needed to render and animate a skinned object.
    /// This is typically stored in the Tag property of the Model being animated.
    /// </summary>
    public class SkinningData
    {
        #region Fields

        IDictionary<string, AnimationClip> animationClipsValue;
        IList<Matrix> bindPoseValue;
        IList<Matrix> inverseBindPoseValue;
        IList<int> skeletonHierarchyValue;

        #endregion


        /// <summary>
        /// Constructs a new skinning data object.
        /// </summary>
        public SkinningData(IDictionary<string, AnimationClip> animationClips,
                            IList<Matrix> bindPose, IList<Matrix> inverseBindPose,
                            IList<int> skeletonHierarchy)
        {
            animationClipsValue = animationClips;
            bindPoseValue = bindPose;
            inverseBindPoseValue = inverseBindPose;
            skeletonHierarchyValue = skeletonHierarchy;
        }


        /// <summary>
        /// Gets a collection of animation clips. These are stored by name in a
        /// dictionary, so there could for instance be clips for "Walk", "Run",
        /// "JumpReallyHigh", etc.
        /// </summary>
        public IDictionary<string, AnimationClip> AnimationClips
        {
            get { return animationClipsValue; }
        }


        /// <summary>
        /// Bindpose matrices for each bone in the skeleton,
        /// relative to the parent bone.
        /// </summary>
        public IList<Matrix> BindPose
        {
            get { return bindPoseValue; }
        }


        /// <summary>
        /// Vertex to bonespace transforms for each bone in the skeleton.
        /// </summary>
        public IList<Matrix> InverseBindPose
        {
            get { return inverseBindPoseValue; }
        }


        /// <summary>
        /// For each bone in the skeleton, stores the index of the parent bone.
        /// </summary>
        public IList<int> SkeletonHierarchy
        {
            get { return skeletonHierarchyValue; }
        }
    }
}
