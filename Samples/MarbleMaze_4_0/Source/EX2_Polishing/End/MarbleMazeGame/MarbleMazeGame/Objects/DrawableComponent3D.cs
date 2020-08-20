#region File Description
//-----------------------------------------------------------------------------
// DrawableComponent3D.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statments
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace MarbleMazeGame
{
    [Flags]
    public enum Axis
    {
        X = 0x1,
        Y = 0x2,
        Z = 0x4
    }

    public abstract class DrawableComponent3D : DrawableGameComponent
    {
        #region Fields
        public const float gravity = 100 * 9.81f;
        public const float wallFriction = 100 * 0.8f;

        string modelName;
        protected IntersectDetails intersectDetails = new IntersectDetails();
        protected bool preferPerPixelLighting = false;
        protected float staticGroundFriction = 0.1f;
        public Model Model = null;
        public Camera Camera;

        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Velocity = Vector3.Zero;
        public Vector3 Acceleration = Vector3.Zero;

        public Matrix[] AbsoluteBoneTransforms;
        public Matrix FinalWorldTransforms;
        public Matrix OriginalWorldTransforms = Matrix.Identity;                

        #endregion

        #region Initializations
        public DrawableComponent3D(Game game, string modelName)
            : base(game)
        {
            this.modelName = modelName;
        }
        #endregion Initializations

        #region Loading
        /// <summary>
        /// Load the component content
        /// </summary>
        protected override void LoadContent()
        {
            // Load the model
            Model = Game.Content.Load<Model>(@"Models\" + modelName);

            // Copy the absolute transforms
            AbsoluteBoneTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(AbsoluteBoneTransforms);

            base.LoadContent();
        }
        #endregion

        #region Update
        /// <summary>
        /// Update the component
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Perform physics calculations
            CalcPhysics(gameTime);

            // Update the final transformation to properly place the component in the
            // game world.
            UpdateFinalWorldTransform();

            base.Update(gameTime);
        }

        /// <summary>
        /// Default final transformation update.
        /// </summary>
        protected virtual void UpdateFinalWorldTransform()
        {
            FinalWorldTransforms = Matrix.Identity *
                Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                    OriginalWorldTransforms *
                    Matrix.CreateTranslation(Position);
        }

        /// <summary>
        /// Perform physics calculations
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void CalcPhysics(GameTime gameTime)
        {
            CalculateCollisions();
            CalculateAcceleration();
            CalculateFriction();
            CalculateVelocityAndPosition(gameTime);
        }

        /// <summary>
        /// Calculate friction between components by 
        /// accelerating opposite to their velocity.
        /// </summary>
        protected abstract void CalculateFriction();        

        /// <summary>
        /// Calculate the acceleration of a component.
        /// </summary>
        protected abstract void CalculateAcceleration();

        /// <summary>
        /// Calculate the velocity and update the position
        /// </summary>
        /// <param name="gameTime">The game time</param>
        protected abstract void CalculateVelocityAndPosition(GameTime gameTime);

        /// <summary>
        /// Calculate which components collide with this component
        /// </summary>
        protected abstract void CalculateCollisions();                
        #endregion

        #region Render
        /// <summary>
        /// Draw the component
        /// </summary>
        /// <param name="gameTime">The game time</param>
        public override void Draw(GameTime gameTime)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    // Set the effect for drawing the component
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = preferPerPixelLighting;

                    // Apply camera settings
                    effect.Projection = Camera.Projection;
                    effect.View = Camera.View;
                    
                    // Apply necessary transformations
                    effect.World = FinalWorldTransforms;
                }

                // Draw the mesh by the effect that set
                mesh.Draw();
            }

            base.Draw(gameTime);
        }
        #endregion
    }
}
