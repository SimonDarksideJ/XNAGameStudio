#region File Description
//-----------------------------------------------------------------------------
// CollisionCameraPerson.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
#endregion



namespace BoxCollider
{
    // person camera (quake like camera)
    public class CollisionCameraPerson : CollisionCamera, IDisposable
    {
        Matrix transform;       // the person transform matrix (without up/down rot)
        Vector3 velocity;       // current velocity vector used only by gravity

        float headHeight;      // height from center of box to eye position
        float stepHeight;      // max height for step player can climb without jumping

        float gravity;          // gravity intensity
        bool onGround;         // is player on ground (false if in air)
        float jumpHeight;      // height player will reach when jumping

        float upDownRot;      // up/down view rotation
        float autoMoveY;      // distance to move in Y axis on next update
                                // in order to climb up/down a step

        public CollisionCameraPerson(
            Vector3 position, 
            Vector3 lookPosition, 
            float angle, 
            float aspect,
            float width, 
            float height, 
            float stepHeight, 
            float headHeight, 
            float upDownRot, 
            float gravity, 
            float jumpHeight) :
            base(position, lookPosition, angle, aspect)
        {
            width *= 0.5f;
            height *= 0.5f;

            this.stepHeight = stepHeight;
            this.headHeight = headHeight - height;
            this.upDownRot = upDownRot;
            this.gravity = gravity;
            this.jumpHeight = jumpHeight;

            transform = world;

            onGround = false;
            velocity = Vector3.Zero;

            box = new CollisionBox(
                        new Vector3(-width, -height + stepHeight, -width),
                        new Vector3(width, height, width));
        }

        public override void Draw(GraphicsDevice gd)
        {
            box.min += world.Translation;
            box.max += world.Translation;
            box.min.Y -= headHeight;
            box.max.Y -= headHeight;

            box.Draw(gd);

            box.min -= world.Translation;
            box.max -= world.Translation;
            box.min.Y += headHeight;
            box.max.Y += headHeight;
        }

        public override void Reset(Matrix m)
        {
            // make sure matrix Y axis is (0,1,0)
            transform = m;
            if (transform.M22 < 0.9999f)
            {
                // rotate Y to (0,1,0)
                Vector3 axisY = new Vector3(transform.M21,transform.M22,transform.M23);
                float ang = (float)Math.Acos(axisY.Y);
                Vector3 axis = Vector3.Normalize(Vector3.Cross(axisY, Vector3.UnitY));
                Vector3 pos = transform.Translation;
                transform.Translation = Vector3.Zero;
                transform = transform * Matrix.CreateFromAxisAngle(axis, ang);
                transform.Translation = pos;
            }
            upDownRot = 0.0f;
            world = transform;
            view = Matrix.Invert(world);
            frustum = new BoundingFrustum(view * projection);
        }

        public override void Update(
            TimeSpan elapsedTime, 
            CollisionMesh collisionMesh,
            GamePadState gamepadState, 
            KeyboardState keyboardState)
        {
            if (collisionMesh == null)
            {
                throw new ArgumentNullException("collisionMesh");
            }

            float timeSeconds = (float)elapsedTime.TotalSeconds;

            float speedBoost = gamepadState.Triggers.Left;
            if (keyboardState.IsKeyDown(Keys.LeftShift))
                speedBoost = 1.0f;

            float rotSpeed = 2.0f * timeSeconds;
            float moveSpeed = (300.0f + 400.0f * speedBoost) * timeSeconds;

            if (onGround == false)
                velocity.Y -= gravity * timeSeconds;
            else
            {
                if (gamepadState.Buttons.A == ButtonState.Pressed ||
                    keyboardState.IsKeyDown(Keys.Space))
                {
                    velocity.Y = (float)Math.Sqrt(gravity * 2.0f * jumpHeight);
                    onGround = false;
                }
                else
                    velocity.Y = 0.0f;
            }

            Vector3 position = transform.Translation;

            Vector3 axisX = new Vector3(transform.M11, transform.M12, transform.M13);
            Vector3 axisY = new Vector3(0, 1, 0);
            Vector3 axisZ = new Vector3(transform.M31, transform.M32, transform.M33);

            Vector3 translate, rotate;
            GetInputVectors(gamepadState, keyboardState, out translate, out rotate);

            Vector3 newPosition = position;
            newPosition += axisX * (moveSpeed * translate.X);
            newPosition -= axisZ * (moveSpeed * translate.Z);
            newPosition += velocity * timeSeconds;

            float moveY = 12.5f * stepHeight * timeSeconds;
            if (autoMoveY >= 0)
            {
                if (moveY > autoMoveY)
                    moveY = autoMoveY;
            }
            else
            {
                moveY = -moveY;
                if (moveY < autoMoveY)
                    moveY = autoMoveY;
            }
            newPosition.Y += moveY;
            autoMoveY = 0;

            collisionMesh.BoxMove(box, position, newPosition, 
                                1, 0, 3, out newPosition);

            if (Math.Abs(newPosition.Y - position.Y) < 0.0001f && velocity.Y > 0.0f)
                velocity.Y = 0.0f;

            float dist;
            Vector3 pos, norm;
            if (velocity.Y <= 0)
                if (true == collisionMesh.BoxIntersect(box, newPosition, 
                            newPosition + new Vector3(0, -2 * stepHeight, 0), 
                            out dist, out pos, out norm))
                {
                    if (norm.Y > 0.70710678f)
                    {
                        onGround = true;
                        autoMoveY = stepHeight - dist;
                    }
                    else
                        onGround = false;
                }
                else
                    onGround = false;

            upDownRot -= rotSpeed * rotate.X;
            if (upDownRot > 1)
                upDownRot = 1;
            else
                if (upDownRot < -1)
                    upDownRot = -1;

            Matrix rotX = Matrix.CreateFromAxisAngle(axisX, upDownRot);
            Matrix rotY = Matrix.CreateFromAxisAngle(axisY, -rotSpeed * rotate.Y);

            transform.Translation = Vector3.Zero;
            transform = transform * rotY;
            Orthonormalize(ref transform);

            world.Translation = Vector3.Zero;
            world = transform * rotX;

            transform.Translation = newPosition;
            newPosition.Y += headHeight;
            world.Translation = newPosition;

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
