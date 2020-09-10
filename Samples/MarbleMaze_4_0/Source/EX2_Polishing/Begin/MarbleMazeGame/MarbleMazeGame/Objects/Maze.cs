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
        #region Field
        public List<Vector3> Ground = new List<Vector3>();
        public List<Vector3> Walls = new List<Vector3>();
        public List<Vector3> FloorSides = new List<Vector3>();
        public LinkedList<Vector3> Checkpoints = new LinkedList<Vector3>();
        public Vector3 StartPoistion;
        public Vector3 End;
        #endregion

        #region Initializations
        public Maze(Game game)
            : base(game, "maze1")
        {
            preferPerPixelLighting = false;
        }

        /// <summary>
        /// Load the maze content
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            // Load the start & end positions of the maze from the bone
            StartPoistion = Model.Bones["Start"].Transform.Translation;
            End = Model.Bones["Finish"].Transform.Translation;            

            // Get the maze's triangles from its mesh
            Dictionary<string, List<Vector3>> tagData = 
                (Dictionary<string, List<Vector3>>)Model.Tag;

            Ground = tagData["Floor"];
            FloorSides = tagData["floorSides"];

            Walls = tagData["walls"];

            // Add checkpoints to the maze
            Checkpoints.AddFirst(StartPoistion);
            foreach (var bone in Model.Bones)
            {
                if (bone.Name.Contains("spawn"))
                {
                    Checkpoints.AddLast(bone.Transform.Translation);
                }
            }
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

        #region Helper function
        /// <summary>
        /// Calculate which part of the maze a bounding sphere collides with.
        /// </summary>
        /// <param name="BoundingSphere">The bounding sphere to use for the collision
        /// check.</param>
        /// <param name="intersectDetails">Will hold the result of the collision 
        /// check.</param>
        /// <param name="light">Use light or full detection.</param>
        public void GetCollisionDetails(BoundingSphere BoundingSphere, ref IntersectDetails intersectDetails, 
            bool light)
        {
            intersectDetails.IntersectWithGround =
                TriangleSphereCollisionDetection.IsSphereCollideWithTringles(Ground,
                BoundingSphere, out intersectDetails.IntersectedGroundTriangle, true);
            intersectDetails.IntersectWithWalls =
                TriangleSphereCollisionDetection.IsSphereCollideWithTringles(Walls,
                BoundingSphere, out intersectDetails.IntersectedWallTriangle, light);
            intersectDetails.IntersectWithFloorSides =
                TriangleSphereCollisionDetection.IsSphereCollideWithTringles(FloorSides,
                BoundingSphere, out intersectDetails.IntersectedFloorSidesTriangle, true);
        }
        #endregion

        #region Overriding physics calculations
        protected override void CalculateCollisions()
        {
            // Nothing to do - Maze doesn't collide with itself
        }

        protected override void CalculateVelocityAndPosition(GameTime gameTime)
        {
            // Nothing to do - Maze doesn't move
        }

        protected override void CalculateFriction()
        {
            // Nothing to do - Maze is not affected by friction
        }

        protected override void CalculateAcceleration()
        {
            // Nothing to do - Maze doesn't move
        }
        #endregion
    }
}
