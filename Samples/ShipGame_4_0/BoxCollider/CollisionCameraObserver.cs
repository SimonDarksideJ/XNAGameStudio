#region File Description
//-----------------------------------------------------------------------------
// CollisionCameraObserver.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion



namespace BoxCollider
{
    // observer camera (descent like camera)
    public class CollisionCameraObserver : CollisionCamera, IDisposable
    {
        public CollisionCameraObserver(
            Vector3 position, 
            Vector3 lookPosition, 
            float angle, 
            float aspect, 
            float radius) :
            base(position, lookPosition, angle, aspect)
        {
            box = new CollisionBox(-radius, radius);
        }

        public override void Draw(GraphicsDevice gd)
        {
            box.min += world.Translation;
            box.max += world.Translation;

            box.Draw(gd);

            box.min -= world.Translation;
            box.max -= world.Translation;
        }

        public override void Reset(Matrix m)
        {
            world = m;
            view = Matrix.Invert(world);
            frustum = new BoundingFrustum(view * projection);
        }

        public override void Update(TimeSpan elapsedTime, CollisionMesh collisionMesh,
            GamePadState gamepadState, KeyboardState keyboardState)
        {
            if (collisionMesh == null)
            {
                throw new ArgumentNullException("collisionMesh");
            }

            float timeSeconds = (float)elapsedTime.TotalSeconds;

            float speedBoost = 0.0f;
            if (gamepadState.Buttons.LeftStick == ButtonState.Pressed)
                speedBoost = 1.0f;
            if (keyboardState != null && keyboardState.IsKeyDown(Keys.LeftShift))
                speedBoost = 1.0f;

            float rotSpeed = 2.0f * timeSeconds;
            float moveSpeed = (400.0f + 600.0f * speedBoost) * timeSeconds;

            Vector3 position = world.Translation;

            Vector3 axisX = new Vector3(world.M11, world.M12, world.M13);
            Vector3 axisY = new Vector3(world.M21, world.M22, world.M23);
            Vector3 axisZ = new Vector3(world.M31, world.M32, world.M33);

            Vector3 translate, rotate;
            GetInputVectors(gamepadState, keyboardState, out translate, out rotate);
            if (gamepadState.Buttons.RightStick == ButtonState.Pressed)
                rotate.X = rotate.Y = 0;

            Vector3 newPosition = position;
            newPosition += axisX * (moveSpeed * translate.X);
            newPosition += axisY * (moveSpeed * translate.Y);
            newPosition -= axisZ * (moveSpeed * translate.Z);

            collisionMesh.BoxMove(box, position, newPosition, 
                                1, 0, 3, out newPosition);

            Matrix rotX = Matrix.CreateFromAxisAngle(axisX, -rotSpeed * rotate.X);
            Matrix rotY = Matrix.CreateFromAxisAngle(axisY, -rotSpeed * rotate.Y);
            Matrix rotZ = Matrix.CreateFromAxisAngle(axisZ, rotSpeed * rotate.Z);

            world.Translation = new Vector3(0, 0, 0);

            world = world * (rotX * rotY * rotZ);

            world.Translation = newPosition;

            Orthonormalize(ref world);

            view = Matrix.Invert(world);

            frustum = new BoundingFrustum(view * projection);
        }

        #region IDisposable Members

        bool isDisposed = false;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                if (box != null)
                {
                    box.Dispose();
                    box = null;
                }
            }
        }

        #endregion
    }
}
