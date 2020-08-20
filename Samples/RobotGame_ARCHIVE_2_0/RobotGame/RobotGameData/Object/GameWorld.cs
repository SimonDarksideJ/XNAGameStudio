#region File Description
//-----------------------------------------------------------------------------
// GameWorld.cs
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
    /// this model class processes the 3D world model.
    /// It contains cube map texture.
    /// </summary>
    public class GameWorld : GameModel
    {
        #region Fields

        //  Cube map texture.
        public TextureCube textureCubeMap = null;

        #endregion

        #region Properties

        public TextureCube TextureCubeMap
        {
            get { return textureCubeMap; }
            set { textureCubeMap = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resource">model resource</param>
        public GameWorld(GameResourceModel resource)
            : base(resource) {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">model file name</param>
        public GameWorld(string fileName)
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
        /// load cube map texture.
        /// </summary>
        /// <param name="file">cube map texture file name</param>
        public void LoadTextureCubeMap(string file)
        {
            this.TextureCubeMap = FrameworkCore.ContentManager.Load<TextureCube>(
                                        file);
            
        }

        protected override void Dispose(bool disposing)
        {
            if (textureCubeMap != null)
            {
                textureCubeMap.Dispose();
                textureCubeMap = null;
            }

            base.Dispose(disposing);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }
    }
}
