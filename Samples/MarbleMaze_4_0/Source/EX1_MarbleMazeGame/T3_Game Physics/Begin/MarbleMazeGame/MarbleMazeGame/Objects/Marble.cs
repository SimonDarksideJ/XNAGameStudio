#region File Description
//-----------------------------------------------------------------------------
// Marble.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


#endregion

namespace MarbleMazeGame
{
    class Marble : DrawableComponent3D
    {
        #region Fields/Properties
        public Maze Maze { get; set; }
        Texture2D m_marbleTexture;
        #endregion

        #region Initializtions
        public Marble(Game game)
            : base(game, "marble")
        {
            preferPerPixelLighting = true;
        }
        #endregion Initializtions

        #region Loading
        /// <summary>
        /// Load the marble content
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Load the texture of the marble
            m_marbleTexture = Game.Content.Load<Texture2D>(@"Textures\Marble");
        }
        #endregion

        #region Render
        /// <summary>
        /// Draw the marble
        /// </summary>
        /// <param name="gameTime">The game time</param>
        public override void Draw(GameTime gameTime)
        {
            var originalSamplerState = GraphicsDevice.SamplerStates[0];

            // Cause the marble's textures to linearly clamp            
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            foreach (var mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    // Set the effect for drawing the marble
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = preferPerPixelLighting;
                    effect.TextureEnabled = true;
                    effect.Texture = m_marbleTexture;

                    // Apply camera settings
                    effect.Projection = Camera.Projection;
                    effect.View = Camera.View;

                    // Apply necessary transformations
                    effect.World = AbsoluteBoneTransforms[mesh.ParentBone.Index] *
                        FinalWorldTransforms;
                }

                mesh.Draw();
            }

            // Return to the original state
            GraphicsDevice.SamplerStates[0] = originalSamplerState;
        }
        #endregion
    }
}
