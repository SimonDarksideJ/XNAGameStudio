#region File Description
//-----------------------------------------------------------------------------
// Maze.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

#endregion

namespace MarbleMazeGame
{
    class Maze : DrawableComponent3D
    {
        #region Initializations
        public Maze(Game game)
            : base(game, "maze1")
        {
            preferPerPixelLighting = false;
        }
        #endregion        

        #region Render
        /// <summary>
        /// Draw the maze
        /// </summary>
        /// <param name="gameTime">The game time</param>
        public override void Draw(GameTime gameTime)
        {
            var originalSamplerState = GraphicsDevice.SamplerStates[0];

            // Cause the maze's textures to linearly wrap            
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            foreach (var mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    // Set the effect for drawing the maze
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = preferPerPixelLighting;

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
