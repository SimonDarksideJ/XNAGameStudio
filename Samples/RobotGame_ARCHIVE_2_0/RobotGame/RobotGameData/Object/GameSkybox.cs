#region File Description
//-----------------------------------------------------------------------------
// GameSkybox.cs
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
using RobotGameData.Resource;
#endregion

namespace RobotGameData.GameObject
{
    /// <summary>
    /// this model class processes Skybox.
    /// When the basis position gets specified, it gets drawn on the specified position.
    /// </summary>
    public class GameSkybox : GameModel
    {
        #region Fields

        bool followOwner = false;

        #endregion

        #region Properties

        public bool FollowOwner
        {
            get { return followOwner; }
            set { followOwner = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resource">model resource</param>
        public GameSkybox(GameResourceModel resource)
            : base(resource) {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">model file name</param>
        public GameSkybox(string fileName)
            : base(fileName) {}

        public override void Initialize()
        {                        
            base.Initialize();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
        }

        protected override void OnDraw(RenderTracer renderTracer)
        {
            base.OnDraw(renderTracer);
        }

        /// <summary>
        /// gets moved to the specified position.
        /// </summary>
        /// <param name="position">follow position</param>
        public void SetBasisPosition(Vector3 position)
        {
            //  The sky follows this position
            this.Position = position;
        }
    }
}
