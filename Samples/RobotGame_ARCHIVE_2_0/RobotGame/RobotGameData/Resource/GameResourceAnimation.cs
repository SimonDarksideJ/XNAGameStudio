#region File Description
//-----------------------------------------------------------------------------
// GameResourceAnimation.cs
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
using RobotGameData.GameObject;
#endregion

namespace RobotGameData.Resource
{
    /// <summary>
    /// a resource element structure with AnimationSequence class.
    /// When an animation(.Animation) file is loaded from the resource manager, 
    /// it gets stored here.
    /// </summary>
    public class GameResourceAnimation : GameResourceBase
    {
        #region Fields

        AnimationSequence animationSequence = null;

        #endregion

        #region Properties

        public AnimationSequence Animation
        {
            get { return animationSequence; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="assetName">asset name</param>
        /// <param name="resource">animation resource</param>
        public GameResourceAnimation(string key, string assetName, 
                                    AnimationSequence resource) 
            : base(key, assetName)
        {
            this.animationSequence = resource;

            this.resource = (object)this.animationSequence;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (animationSequence != null)
                {
                    animationSequence = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
