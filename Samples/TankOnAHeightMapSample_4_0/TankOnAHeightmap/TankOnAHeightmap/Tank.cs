#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
#endregion

namespace TanksOnAHeightmap
{
    class Tank
    {
        #region Constants


        // This constant controls how quickly the tank can move forward and backward
        const float TankVelocity = 2;

        // The radius of the tank's wheels. This is used when we calculate how fast they
        // should be rotating as the tank moves.
        const float TankWheelRadius = 18;

        // controls how quickly the tank can turn from side to side.
        const float TankTurnSpeed = .025f;


        #endregion


        #region Properties

        /// <summary>
        /// The position of the tank. The camera will use this value to position itself.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }            
        }
        private Vector3 position;        

        /// <summary>
        /// The direction that the tank is facing, in radians. This value will be used
        /// to position and and aim the camera.
        /// </summary>
        public float FacingDirection
        {
            get { return facingDirection; }            
        }
        private float facingDirection;


        #endregion


        #region Fields

        // The tank's model - a fearsome sight.
        Model model;

        // how is the tank oriented? We'll calculate this based on the user's input and
        // the heightmap's normals, and then use it when drawing.
        Matrix orientation = Matrix.Identity;

        // we'll use this value when making the wheels roll. It's calculated based on 
        // the distance moved.
        Matrix wheelRollMatrix = Matrix.Identity;

        // The Simple Animation Sample at creators.xna.com explains the technique that 
        // we will be using in order to roll the tanks wheels. In this technique, we
        // will keep track of the ModelBones that control the wheels, and will manually
        // set their transforms. These next eight fields will be used for this
        // technique.
        ModelBone leftBackWheelBone;
        ModelBone rightBackWheelBone;
        ModelBone leftFrontWheelBone;
        ModelBone rightFrontWheelBone;

        Matrix leftBackWheelTransform;
        Matrix rightBackWheelTransform;
        Matrix leftFrontWheelTransform;
        Matrix rightFrontWheelTransform;

        #endregion


        #region Initialization

        /// <summary>
        /// Called when the Game is loading its content. Pass in a ContentManager so the
        /// tank can load its model.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Tank");

            // as discussed in the Simple Animation Sample, we'll look up the bones
            // that control the wheels.
            leftBackWheelBone = model.Bones["l_back_wheel_geo"];
            rightBackWheelBone = model.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = model.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = model.Bones["r_front_wheel_geo"];

            // Also, we'll store the original transform matrix for each animating bone.
            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;

        }

        #endregion


        #region Update and Draw

        /// <summary>
        /// This function is called when the game is Updating in response to user input.
        /// It'll move the tank around the heightmap, and update all of the tank's 
        /// necessary state.
        /// </summary>
        public void HandleInput(GamePadState currentGamePadState,
            KeyboardState currentKeyboardState, HeightMapInfo heightMapInfo)
        {
            // First, we want to check to see if the tank should turn. turnAmount will 
            // be an accumulation of all the different possible inputs.
            float turnAmount = -currentGamePadState.ThumbSticks.Left.X;
            if (currentKeyboardState.IsKeyDown(Keys.A) ||
                currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                turnAmount += 1;
            }

            if (currentKeyboardState.IsKeyDown(Keys.D) ||
                currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                turnAmount -= 1;
            }

            // clamp the turn amount between -1 and 1, and then use the finished
            // value to turn the tank.
            turnAmount = MathHelper.Clamp(turnAmount, -1, 1);
            facingDirection += turnAmount * TankTurnSpeed;


            // Next, we want to move the tank forward or back. to do this, 
            // we'll create a Vector3 and modify use the user's input to modify the Z
            // component, which corresponds to the forward direction.
            Vector3 movement = Vector3.Zero;
            movement.Z = -currentGamePadState.ThumbSticks.Left.Y;

            if (currentKeyboardState.IsKeyDown(Keys.W) ||
                currentKeyboardState.IsKeyDown(Keys.Up) ||
                currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                movement.Z = -1;
            }
            if (currentKeyboardState.IsKeyDown(Keys.S) ||
                currentKeyboardState.IsKeyDown(Keys.Down) ||
                currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                movement.Z = 1;
            }

            // next, we'll create a rotation matrix from the direction the tank is 
            // facing, and use it to transform the vector.
            orientation = Matrix.CreateRotationY(FacingDirection);
            Vector3 velocity = Vector3.Transform(movement, orientation);
            velocity *= TankVelocity;

            // Now we know how much the user wants to move. We'll construct a temporary
            // vector, newPosition, which will represent where the user wants to go. If
            // that value is on the heightmap, we'll allow the move.
            Vector3 newPosition = Position + velocity;
            if (heightMapInfo.IsOnHeightmap(newPosition))
            {
                // now that we know we're on the heightmap, we need to know the correct
                // height and normal at this position.
                Vector3 normal;
                heightMapInfo.GetHeightAndNormal(newPosition,
                    out newPosition.Y, out normal);


                // As discussed in the doc, we'll use the normal of the heightmap
                // and our desired forward direction to recalculate our orientation
                // matrix. It's important to normalize, as well.
                orientation.Up = normal;

                orientation.Right = Vector3.Cross(orientation.Forward, orientation.Up);
                orientation.Right = Vector3.Normalize(orientation.Right);

                orientation.Forward = Vector3.Cross(orientation.Up, orientation.Right);
                orientation.Forward = Vector3.Normalize(orientation.Forward);

                // now we need to roll the tank's wheels "forward." to do this, we'll
                // calculate how far they have rolled, and from there calculate how much
                // they must have rotated.
                float distanceMoved = Vector3.Distance(Position, newPosition);
                float theta = distanceMoved / TankWheelRadius;
                int rollDirection = movement.Z > 0 ? 1 : -1;

                wheelRollMatrix *= Matrix.CreateRotationX(theta * rollDirection);

                // once we've finished all computations, we can set our position to the
                // new position that we calculated.
                position = newPosition;
            }
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            // Apply matrices to the relevant bones, as discussed in the Simple 
            // Animation Sample.
            leftBackWheelBone.Transform = wheelRollMatrix * leftBackWheelTransform;
            rightBackWheelBone.Transform = wheelRollMatrix * rightBackWheelTransform;
            leftFrontWheelBone.Transform = wheelRollMatrix * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = wheelRollMatrix * rightFrontWheelTransform;

            // now that we've updated the wheels' transforms, we can create an array
            // of absolute transforms for all of the bones, and then use it to draw.
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            
            // calculate the tank's world matrix, which will be a combination of our
            // orientation and a translation matrix that will put us at at the correct
            // position.
            Matrix worldMatrix = orientation * Matrix.CreateTranslation(Position);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    // Set the fog to match the black background color
                    effect.FogEnabled = true;
                    effect.FogColor = Vector3.Zero;
                    effect.FogStart = 1000;
                    effect.FogEnd = 3200;
                }
                mesh.Draw();
            }
        }

        #endregion
    }
}
