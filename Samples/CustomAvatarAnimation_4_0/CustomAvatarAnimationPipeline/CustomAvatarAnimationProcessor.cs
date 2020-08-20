#region File Description
//-----------------------------------------------------------------------------
// CustomAvatarAnimationProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.GamerServices;
using CustomAvatarAnimation;
using System.IO;
#endregion

namespace CustomAvatarAnimationPipeline
{
    [ContentProcessor(DisplayName = "CustomAvatarAnimationProcessor")]
    public class CustomAvatarAnimationProcessor :
                                 ContentProcessor<NodeContent, CustomAvatarAnimationData>
    {
        /// <summary>
        /// Stores the model's bind pose
        /// </summary>
        List<Matrix> bindPose = new List<Matrix>();

        /// <summary>
        /// The facial expression file to use for the animation
        /// </summary>
        [DisplayName("Facial expression file")]
        [Description("File to use for facial animations")]
        public string ExpressionFile
        {   get; set;    }

        /// <summary>
        /// Processor to convert the model into our CustomAvatarAnimationData type
        /// </summary>
        public override CustomAvatarAnimationData Process(NodeContent input,
                                                         ContentProcessorContext context)
        {
            // Find the skeleton
            NodeContent skeleton = FindSkeleton(input); 

            // Check for errors
            if (skeleton == null)
            {
                throw new InvalidContentException("Avatar skeleton not found.");
            }
            else if (skeleton.Animations.Count < 1)
            {
                throw new InvalidContentException("No animation was found in the file.");
            }
            else if (skeleton.Animations.Count > 1)
            {
                throw new InvalidContentException("More than one animation was found.");
            }

            // The expression animation keyframes
            List<AvatarExpressionKeyFrame> expressionAnimationKeyFrames = null;

            // Process expression animation
            if (!string.IsNullOrEmpty(ExpressionFile))
            {
                expressionAnimationKeyFrames = ProcessExpressionAnimation();
            }

            // Remove the extra bones that we will not be using
            RemoveEndBonesAndFixBoneNames(skeleton);

            // Create a list of the bones from the skeleton hierarchy
            IList<NodeContent> bones = FlattenSkeleton(skeleton);


            // Check for errors
            if (bones.Count != AvatarRenderer.BoneCount)
            {
                throw new InvalidContentException("Invalid number of bones found.");
            }

            // Fill the bind pose array with the transforms from the bones
            foreach (NodeContent bone in bones)
            {
                bindPose.Add(bone.Transform);
            }

            // Build up a table mapping bone names to indices
            Dictionary<string, int> boneNameMap = new Dictionary<string, int>();
            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;
                if (!string.IsNullOrEmpty(boneName))
                {
                    boneNameMap.Add(boneName, i);
                }
            }


            // Create the custom animation data
            // -- From the error-checking above, we know there will only be one animation
            CustomAvatarAnimationData avatarCustomAnimationData = null;
            foreach (KeyValuePair<string, AnimationContent> animation
                                                                  in skeleton.Animations)
            {
                // Check for an invalid animation
                if (animation.Value.Duration <= TimeSpan.Zero)
                {
                    throw new InvalidContentException("Animation has a zero duration.");
                }

                // Build a list of the avatar keyframes in the animation
                List<AvatarKeyFrame> animationKeyFrames =
                                          ProcessAnimation(animation.Value, boneNameMap);

                // Check for an invalid keyframes list
                if (animationKeyFrames.Count <= 0)
                {
                    throw new InvalidContentException("Animation has no keyframes.");
                }

                // Create the custom-animation object
                avatarCustomAnimationData = new CustomAvatarAnimationData(animation.Key,
                    animation.Value.Duration, animationKeyFrames, expressionAnimationKeyFrames);
            }

            return avatarCustomAnimationData;
        }

        /// <summary>
        /// Converts the input expression animation file into expression animation keyframes
        /// </summary>
        private List<AvatarExpressionKeyFrame> ProcessExpressionAnimation()
        {
            List<AvatarExpressionKeyFrame> expressionAnimationKeyFrames = new List<AvatarExpressionKeyFrame>();

            FileStream fs = File.OpenRead(ExpressionFile);
            StreamReader sr = new StreamReader(fs);
            while (!sr.EndOfStream)
            {
                string currentLine = sr.ReadLine();

                // Skip comment lines
                if (currentLine.StartsWith("#"))
                    continue;

                string[] Components = currentLine.Split(',');

                // Check for the correct number of components
                if (Components.Length != 6)
                    throw new InvalidContentException("Error processing facial expression file");

                try
                {
                    TimeSpan time = TimeSpan.FromMilliseconds(Convert.ToDouble(Components[0]));
                    AvatarExpression avatarExpression = new AvatarExpression();
                    avatarExpression.LeftEye = (AvatarEye)Convert.ToInt32(Components[1]);
                    avatarExpression.LeftEyebrow = (AvatarEyebrow)Convert.ToInt32(Components[2]);
                    avatarExpression.Mouth = (AvatarMouth)Convert.ToInt32(Components[3]);
                    avatarExpression.RightEye = (AvatarEye)Convert.ToInt32(Components[4]);
                    avatarExpression.RightEyebrow = (AvatarEyebrow)Convert.ToInt32(Components[5]);

                    AvatarExpressionKeyFrame expressionKeyframe = new AvatarExpressionKeyFrame(time, avatarExpression);
                    expressionAnimationKeyFrames.Add(expressionKeyframe);
                }
                catch (Exception)
                {
                    throw new InvalidContentException("Error processing facial expression file");
                }
            }

            // Sort the animation frames
            expressionAnimationKeyFrames.Sort((frame1, frame2) => frame1.Time.CompareTo(frame2.Time));

            return expressionAnimationKeyFrames;
        }

        private NodeContent FindSkeleton(NodeContent input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Search for the root node of the skeleton
            if (input.Name.Contains("BASE__Skeleton"))
            {
                return input;
            }
            
            // Search children nodes
            foreach (NodeContent child in input.Children)
            {
                // Try to find the skeleton in the child nodes
                NodeContent skeleton = FindSkeleton(child);

                // Return the found skeleton
                if (skeleton != null)
                    return skeleton;
            }

            // Could not find the skeleton
            return null;
        }


        #region Skeleton Cleaning


        /// <summary>
        /// Flattens the skeleton into a list. The order in the list is sorted by
        /// depth first and then by name
        /// </summary>
        static IList<NodeContent> FlattenSkeleton(NodeContent skeleton)
        {
            // safety check on the parameter
            if (skeleton == null)
            {
                throw new ArgumentNullException("skeleton");
            }

            // Create the destination list of bones
            List<NodeContent> bones = new List<NodeContent>();

            // Create a list to track current items in the level of tree
            List<NodeContent> currentLevel = new List<NodeContent>();

            // Add the root node of the skeleton to the list
            currentLevel.Add(skeleton);

            while (currentLevel.Count > 0)
            {
                // Create a list of bones to track the next level of the tree
                List<NodeContent> nextLevel = new List<NodeContent>();

                // Sort the bones in the current level 
                IEnumerable<NodeContent> sortedBones = from item in currentLevel
                                                       orderby item.Name
                                                       select item;

                // Add the newly sorted items to the output list
                foreach (NodeContent bone in sortedBones)
                {
                    bones.Add(bone);
                    // Add the bone's children to the next-level list
                    foreach (NodeContent child in bone.Children)
                    {
                        nextLevel.Add(child);
                    }
                }

                // the next level is now the current level
                currentLevel = nextLevel;
            }

            // return the flattened array of bones
            return bones;
        }


        /// <summary>
        /// Removes each bone node that contains "_END" in the name/
        /// </summary>
        /// <remarks>
        /// These bones are not needed by the AvatarRenderer runtime but
        /// are part of the Avatar rig used in modeling programs
        /// </remarks>
        static void RemoveEndBonesAndFixBoneNames(NodeContent bone)
        {
            // safety-check the parameter
            if (bone == null)
            {
                throw new ArgumentNullException("bone");
            }

            // Remove unneeded text from the bone name
            bone.Name = CleanBoneName(bone.Name);

            // Remove each child bone that contains "_END" in the name
            for (int i = 0; i < bone.Children.Count; ++i)
            {
                NodeContent child = bone.Children[i];
                if (child.Name.Contains("_END"))
                {
                    bone.Children.Remove(child);
                    --i;
                }
                else
                {
                    // Recursively search through the remaining child bones
                    RemoveEndBonesAndFixBoneNames(child);
                }
            }
        }


        /// <summary>
        /// Removes extra text from the bone names
        /// </summary>
        static string CleanBoneName(string boneName)
        {
            boneName = boneName.Replace("__Skeleton", "");
            return boneName;
        }


        #endregion


        #region Animation Processing


        /// <summary>
        /// Converts an intermediate-format content pipeline AnimationContent object 
        /// to an avatar-specific AvatarKeyFrame list.
        /// </summary>
        List<AvatarKeyFrame> ProcessAnimation(AnimationContent animation,
                                                     Dictionary<string, int> boneMap)
        {
            // Create the output list of keyframes
            List<AvatarKeyFrame> keyframes = new List<AvatarKeyFrame>();

            // Process each channel in the animation
            foreach (KeyValuePair<string, AnimationChannel> channel
                                                                   in animation.Channels)
            {
                // Don't add animation nodes with "_END" in the name
                // -- These bones were removed from the skeleton already
                if (channel.Key.Contains("_END"))
                {
                    continue;
                }

                // Look up what bone this channel is controlling.
                int boneIndex;
                if (!boneMap.TryGetValue(CleanBoneName(channel.Key), out boneIndex))
                {
                    throw new InvalidContentException(string.Format(
                        "Found animation for bone '{0}', " +
                        "which is not part of the skeleton.", channel.Key));
                }

                // Convert the keyframe data.
                foreach (AnimationKeyframe keyframe in channel.Value)
                {
                    keyframes.Add(new AvatarKeyFrame(boneIndex, keyframe.Time,
                                             CreateKeyframeMatrix(keyframe, boneIndex)));
                }
            }

            // Sort the merged keyframes by time. 
            keyframes.Sort((frame1, frame2) => frame1.Time.CompareTo(frame2.Time));

            return keyframes;
        }


        /// <summary>
        /// Create an AvatarRenderer-friendly matrix from an animation keyframe.
        /// </summary>
        /// <param name="keyframe">The keyframe to be converted.</param>
        /// <param name="boneIndex">The index of the bone this keyframe is for.</param>
        /// <returns>The converted AvatarRenderer-friendly matrix for this bone 
        /// and keyframe.</returns>
        Matrix CreateKeyframeMatrix(AnimationKeyframe keyframe, int boneIndex)
        {
            // safety-check the parameter
            if (keyframe == null)
            {
                throw new ArgumentNullException("keyframe");
            }

            // Retrieve the transform for this keyframe
            Matrix keyframeMatrix;

            // The root node is transformed by the root of the bind pose
            // We need to make the keyframe relative to the root
            if (boneIndex == 0)
            {
                // When the animation is exported the bind pose can have the 
                // wrong translation of the root node so we hard code it here
                Vector3 bindPoseTranslation = new Vector3(0.000f, 75.5199f, -0.8664f);

                Matrix keyTransfrom = keyframe.Transform;

                Matrix inverseBindPose = bindPose[boneIndex];
                inverseBindPose.Translation -= bindPoseTranslation;
                inverseBindPose = Matrix.Invert(inverseBindPose);

                keyframeMatrix = (keyTransfrom * inverseBindPose);
                keyframeMatrix.Translation -= bindPoseTranslation;

                // Scale from cm to meters
                keyframeMatrix.Translation *= 0.01f;
            }
            else
            {
                keyframeMatrix = keyframe.Transform;
                // Only the root node can have translation
                keyframeMatrix.Translation = Vector3.Zero;
            }

            return keyframeMatrix;
        }


        #endregion
    }
}