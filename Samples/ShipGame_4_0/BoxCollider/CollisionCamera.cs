#region File Description
//-----------------------------------------------------------------------------
// CollisionCamera.cs
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
    // base camera class 
    public abstract class CollisionCamera : CollisionTreeElemDynamic
    {
        private float nearPlane = 1.0f;
        public float NearPlane
        {
            get { return nearPlane; }
            set
            {
                nearPlane = value;
                projection = Matrix.CreatePerspectiveFieldOfView(Angle, Aspect,
                    NearPlane, FarPlane);
                frustum = new BoundingFrustum(view * projection);
            }
        }

        private float farPlane = 10000.0f;
        public float FarPlane
        {
            get { return farPlane; }
            set
            {
                farPlane = value;
                projection = Matrix.CreatePerspectiveFieldOfView(Angle, Aspect,
                    NearPlane, FarPlane);
                frustum = new BoundingFrustum(view * projection);
            }
        }

        private float angle = MathHelper.ToRadians(60);
        public float Angle
        {
            get { return angle; }
            set
            {
                angle = value;
                projection = Matrix.CreatePerspectiveFieldOfView(Angle, Aspect, 
                    NearPlane, FarPlane);
                frustum = new BoundingFrustum(view * projection);
            }
        }

        private float aspect = 1.0f;
        public float Aspect
        {
            get { return aspect; }
            set
            {
                aspect = value;
                projection = Matrix.CreatePerspectiveFieldOfView(Angle, Aspect, 
                    NearPlane, FarPlane);
                frustum = new BoundingFrustum(view * projection);
            }
        }

        public Matrix world;        // camera position and rotation
        public Matrix view;         // view = inverse( world )
        public Matrix projection;   // projection matrix
        
        public BoundingFrustum frustum; // camera frustum

        protected CollisionCamera(
            Vector3 position, 
            Vector3 lookPosition, 
            float angle, 
            float aspect)
        {
            this.Angle = angle;
            this.Aspect = aspect;

            projection = Matrix.CreatePerspectiveFieldOfView(
                                    angle, aspect, NearPlane, FarPlane);

            view = Matrix.CreateLookAt(position, lookPosition, Vector3.Up);

            world = Matrix.Invert(view);

            frustum = new BoundingFrustum(view * projection);
        }

        // get world matrix axis or its tranlation component 
        // (0 for X, 1 for Y, 2 for Z and 3 for translation)
        public Vector3 GetWorldVector(int axis)
        {
            switch (axis)
            {
                case 0: return new Vector3(world.M11, world.M12, world.M13);
                case 1: return new Vector3(world.M21, world.M22, world.M23);
                case 2: return new Vector3(world.M31, world.M32, world.M33);
                case 3: return new Vector3(world.M41, world.M42, world.M43);
            }

            return Vector3.Zero;
        }

        // get view matrix axis or its tranlation component 
        // (0 for X, 1 for Y, 2 for Z and 3 for translation)
        public Vector3 GetViewVector(int axis)
        {
            switch (axis)
            {
                case 0: return new Vector3(view.M11, view.M12, view.M13);
                case 1: return new Vector3(view.M21, view.M22, view.M23);
                case 2: return new Vector3(view.M31, view.M32, view.M33);
                case 3: return new Vector3(view.M41, view.M42, view.M43);
            }

            return Vector3.Zero;
        }

        // get tranlation and rotation from input devices
        static public void GetInputVectors(
            GamePadState gamepadState, KeyboardState keyboardState,
            out Vector3 translate, out Vector3 rotate)
        {
            translate = Vector3.Zero;
            rotate = Vector3.Zero;

            translate.X = gamepadState.ThumbSticks.Left.X;
            if (keyboardState.IsKeyDown(Keys.Q))
                translate.X -= 1.0f;
            if (keyboardState.IsKeyDown(Keys.E))
                translate.X += 1.0f;

            translate.Y = 0;

            translate.Z = gamepadState.ThumbSticks.Left.Y;
            if (keyboardState.IsKeyDown(Keys.W))
                translate.Z += 1.0f;
            if (keyboardState.IsKeyDown(Keys.S))
                translate.Z -= 1.0f;

            rotate.X = gamepadState.ThumbSticks.Right.Y;
            if (keyboardState.IsKeyDown(Keys.Down))
                rotate.X -= 0.7f;
            if (keyboardState.IsKeyDown(Keys.Up))
                rotate.X += 0.7f;

            rotate.Y = gamepadState.ThumbSticks.Right.X;
            if (keyboardState.IsKeyDown(Keys.Left))
                rotate.Y -= 0.7f;
            if (keyboardState.IsKeyDown(Keys.Right))
                rotate.Y += 0.7f;
            
            rotate.Z = 0;
            if (gamepadState.Buttons.LeftShoulder == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.A))
                rotate.Z += 0.7f;
            if (gamepadState.Buttons.RightShoulder == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.D))
                rotate.Z -= 0.7f;

            if (rotate.X >= 0.00001f && rotate.X < 0.00001f)
                rotate.X = 0;
            if (rotate.Y >= 0.00001f && rotate.Y < 0.00001f)
                rotate.Y = 0;
            if (rotate.Z >= 0.00001f && rotate.Z < 0.00001f)
                rotate.Z = 0;
        }

        // make sure matrix axis are perpendicular and unit size
        static public void Orthonormalize(ref Matrix m)
        {
            Vector3 axisX = new Vector3(m.M11, m.M12, m.M13);
            Vector3 axisY = new Vector3(m.M21, m.M22, m.M23);
            Vector3 axisZ = new Vector3(m.M31, m.M32, m.M33);
            axisZ = Vector3.Normalize(Vector3.Cross(axisX, axisY));
            axisY = Vector3.Normalize(Vector3.Cross(axisZ, axisX));
            axisX = Vector3.Normalize(Vector3.Cross(axisY, axisZ));
            m.M11 = axisX.X; m.M12 = axisX.Y; m.M13 = axisX.Z;
            m.M21 = axisY.X; m.M22 = axisY.Y; m.M23 = axisY.Z;
            m.M31 = axisZ.X; m.M32 = axisZ.Y; m.M33 = axisZ.Z;
        }

        public abstract void Draw(GraphicsDevice gd);
        public abstract void Reset(Matrix m);
        public abstract void Update(
            TimeSpan elapsedTime, CollisionMesh collisionMesh,
            GamePadState gamepadState, KeyboardState keyboardState);
    }
}
