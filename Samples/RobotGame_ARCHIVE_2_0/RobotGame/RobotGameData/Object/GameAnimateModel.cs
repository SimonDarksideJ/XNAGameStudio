#region File Description
//-----------------------------------------------------------------------------
// GameAnimateModel.cs
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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.Render;
using RobotGameData.GameObject;
using RobotGameData.Resource;
using RobotGameData.Collision;
#endregion

namespace RobotGameData.GameObject
{
    /// <summary>
    /// It has the Model’s animation data and processes the animate function
    /// of the Model.
    /// </summary>
    public class GameAnimateModel : GameModel
    {
        #region Fields
        
        protected List<AnimationBlender> animationBlenderList = 
            new List<AnimationBlender>();
        protected List<AnimationSequence> animationList = 
            new List<AnimationSequence>();
        
        protected bool traceAnimation = false;

        #endregion

        #region Properties

        public int AnimationCount
        {
            get { return animationList.Count; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resource">model resource</param>         
        public GameAnimateModel(GameResourceModel resource)
            : base(resource) {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">model file name</param>
        public GameAnimateModel(string fileName)
            : base( fileName) {}

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        /// <summary>
        /// animates the skeleton of bones using stored key frame 
        /// in the AnimationBlender. 
        /// </summary>
        protected override void OnUpdate(GameTime gameTime)
        {
            //  Reset the bone's transform by source transform
            this.ModelData.model.CopyBoneTransformsFrom(this.ModelData.boneTransforms);
                       
            if (animationList.Count > 0)
            {
                for(int i=0; i<this.ModelData.model.Bones.Count; i++)
                {
                    ModelBone bone = this.ModelData.model.Bones[i];

                    AnimationBlender Blender = animationBlenderList[i];

                    if (Blender.IsEmpty == false)
                    {
                        //  if exist KeyFrameSequence in the AnimationBinder, 
                        //  calculates the animation key frame matrix
                        if (Blender.AnimationBinder != null)
                        {
                            //  gets calculated animation key frame on this time
                            bone.Transform = 
                                    Blender.GetKeyFrameMatrix(
                                    (float)gameTime.ElapsedGameTime.TotalSeconds);
                        }
                    }
                }                
            }
        
            base.OnUpdate(gameTime);
        }

        protected override void OnReset()
        {
            base.OnReset();
        }

        protected override void OnDraw(RenderTracer renderTracer)
        {
            base.OnDraw(renderTracer);
        }

        /// <summary>
        /// processes the necessary steps for binding a model.
        /// It creates AnimationBlender to the number of the model's bone.
        /// </summary>
        public override void BindModel(ModelData modelData)
        {
            base.BindModel(modelData);

            if (animationBlenderList.Count == 0)
            {
                //  Insert all bones in the AnimationBinder
                for (int i = 0; i < modelData.model.Bones.Count; i++)
                {
                    AnimationBlender animationBlender = new AnimationBlender();
                    animationBlender.Name = modelData.model.Bones[i].Name;

                    animationBlenderList.Add(animationBlender);
                }
            }
        }

        /// <summary>
        /// adds an animation to the list.
        /// </summary>
        /// <param name="animation">animation structure</param>
        /// <returns>index of the added animation</returns>
        public int AddAnimation(AnimationSequence animation)
        {
            if (animation == null)
            {
                throw new ArgumentNullException("animation");
            }

             animationList.Add(animation);

            return animationList.IndexOf(animation);
        }

        /// <summary>
        /// adds an animation to the list.
        /// </summary>
        /// <param name="fileName">animation file (.Animation)</param>
        /// <returns>index of the added animation</returns>
        public int AddAnimation(string fileName)
        {
            //  Load and find an animation resource
            GameResourceAnimation resource = 
                                FrameworkCore.ResourceManager.LoadAnimation(fileName);
            
            return AddAnimation(resource.Animation);
        }

        /// <summary>
        /// removes the animation by index.
        /// </summary>
        public bool RemoveAnimation(int index)
        {
            animationList.RemoveAt(index);

            return false;
        }

        /// <summary>
        /// remove all stored animations.
        /// </summary>
        public void ClearAnimationAll()
        {
            animationList.Clear();
        }

        /// <summary>
        /// gets the animation structure by index.
        /// </summary>
        /// <param name="index">stored animation index</param>
        /// <returns>animation structure</returns>
        public AnimationSequence GetAnimation(int index)
        {
            return animationList[index];
        }

        public AnimationBlender FindAnimationBlenderByBoneName(string boneName)
        {
            for (int i = 0; i < animationBlenderList.Count; i++)
            {
                if (animationBlenderList[i].Name == boneName)
                    return animationBlenderList[i];
            }

            return null;
        }

        /// <summary>
        /// plays the bone animation by index.
        /// </summary>
        /// <param name="index">stored animation index</param>
        /// <param name="playMode">animation play mode</param>
        /// <returns></returns>
        public bool PlayAnimation(int index, AnimPlayMode playMode)
        {
            return PlayAnimation(index, 0.0f, 0.0f, 1.0f, playMode);
        }

        /// <summary>
        /// plays the bone animation by index.
        /// </summary>
        /// <param name="index">stored animation index</param>
        /// <param name="startTime">begin time of the animation</param>
        /// <param name="blendTime">blending time of the animation</param>
        /// <param name="timeScaleFactor">
        /// time scale of the animation (default is 1.0)
        /// </param>
        /// <param name="playMode">animation play mode</param>
        /// <returns></returns>
        public bool PlayAnimation(int index, float startTime, float blendTime, 
                                  float timeScaleFactor, AnimPlayMode playMode)
        {
            AnimationSequence animation = GetAnimation(index);
            if (animation != null)
            {                
                //  Binding the playable AnimationSequence to AnimationBinder
                for( int i=0; i<animation.KeyFrameSequences.Count; i++)
                {
                    KeyFrameSequence sequence = animation.KeyFrameSequences[i];

                    AnimationBlender blender = 
                        FindAnimationBlenderByBoneName(sequence.BoneName);
                    if (blender == null)
                    {
                        throw new InvalidOperationException(
                            "The animation specified a bone (\"" + sequence.BoneName + 
                            "\") that the model (\"" + this.Name + "\") does not have.");
                    }

                    //  Initialize KeyFrameSequence infomation
                    blender.AddKeyFrameSequence(sequence, startTime, blendTime,
                                                timeScaleFactor, playMode);
                }

                if (traceAnimation)
                {
                    System.Diagnostics.Debug.WriteLine(
                        string.Format("Play Animtion : {0} ({1})", Name, index));
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// gets animation playing time of the bone
        /// </summary>
        /// <param name="boneName">bone's name</param>
        /// <returns>current playing time</returns>
        public float GetBonePlayTime(string boneName)
        {
            for (int i = 0; i < animationBlenderList.Count; i++)
            {
                if (animationBlenderList[i].Name == boneName)
                {
                    return animationBlenderList[i].AnimationBinder.LocalTime;
                }
            }

            return 0.0f;
        }
    }
}
