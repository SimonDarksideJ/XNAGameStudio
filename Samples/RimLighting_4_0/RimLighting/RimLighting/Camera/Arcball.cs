//-----------------------------------------------------------------------------
// Arcball.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace RimLighting
{
    /// <summary>
    /// Simple Arcball class for handling rotations from user's drag operations on screen 
    /// </summary>
    public class Arcball
    {
        // Current rotation quaternion
        Quaternion qNow = Quaternion.Identity;

        // The rotation quaternion when the user click down on the screen and begins dragging
        Quaternion qDown = Quaternion.Identity;

        /// <summary>
        /// Returns current rotation as quaternion 
        /// </summary>
        public Quaternion GetCurrentRotationQuaternion()
        {
            return qNow;
        }

        /// <summary>
        /// Returns current rotation as matrix 
        /// </summary>
        public Matrix GetCurrentRotationMatrix()
        {
            return Matrix.CreateFromQuaternion(qNow);
        }

        /// <summary>
        /// Set current rotation using quaternion         
        /// </summary>
        public void SetCurrentRotation(Quaternion rotation)
        {
            qNow = rotation;
        }

        /// <summary>
        /// Set current rotation using matrix 
        /// </summary>
        public void SetCurrentRotation(Matrix rotation)
        {
            qNow = Quaternion.CreateFromRotationMatrix(rotation);
        }

        // The bounding box in which drag operations is processed by this arcball
        Vector2 vec2Offset = new Vector2();
        Vector2 vec2Size = new Vector2();

        // Coordinates on the sphere when the user begins dragging
        Vector3 vec3DownPt = new Vector3();

        // Coordinates on the sphere during dragging
        Vector3 vec3CurrentPt = new Vector3();

        // Sphere radius (proportional to the bounding box)
        float fRadius = 0.9f;

        // Is the user currently dragging on this arcball?
        public bool IsDragging
        {
            get { return isDraggingInt; }
            private set { isDraggingInt = value; }
        }
        protected bool isDraggingInt = false;

        /// <summary>
        /// Constructor, the bounding box of the arcball is needed 
        /// </summary>
        public Arcball(int x, int y, int width, int height)
        {
            vec2Offset.X = x;
            vec2Offset.Y = y;
            vec2Size.X = width;
            vec2Size.Y = height;
        }

        /// <summary>
        /// Process touch input. This should be called within Update() of the game
        /// </summary>
        public void HandleTouch(TouchLocation loc)
        {
            if (loc.State == TouchLocationState.Pressed && !IsDragging)
            {
                if (loc.Position.X >= vec2Offset.X && loc.Position.X < (vec2Offset.X + vec2Size.X) &&
                    loc.Position.Y >= vec2Offset.Y && loc.Position.Y < (vec2Offset.Y + vec2Size.Y))
                {
                    IsDragging = true;
                    qDown = qNow;
                    vec3DownPt = screenToVector(loc.Position.X, loc.Position.Y);
                }
            }
            else
            {
                if (loc.State == TouchLocationState.Released)
                {
                    IsDragging = false;
                }
            }

            if (IsDragging)
            {
                vec3CurrentPt = screenToVector(loc.Position.X, loc.Position.Y);
                qNow = quatFromBallPoints(vec3DownPt, vec3CurrentPt) * qDown;
            }
        }

        /// <summary>
        /// Converts screen coordinates to coordinates on the sphere
        /// </summary>        
        protected Vector3 screenToVector(float screenX, float screenY)
        {
            // Scale to screen
            float x = (screenX - vec2Offset.X - vec2Size.X / 2.0f) / (fRadius * vec2Size.X / 2.0f);
            float y = (screenY - vec2Offset.Y - vec2Size.Y / 2.0f) / (fRadius * vec2Size.Y / 2.0f);

            float z = 0.0f;
            float mag = x * x + y * y;

            if (mag > 1.0f)
            {
                float scale = (float)(1.0f / Math.Sqrt(mag));
                x *= scale;
                y *= scale;
            }
            else
                z = (float)Math.Sqrt(1.0f - mag);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Compute rotation quaternion from two points on the sphere      
        /// </summary>        
        protected Quaternion quatFromBallPoints(Vector3 from, Vector3 to)
        {
            float dot = Vector3.Dot(from, to);
            Vector3 qPart = Vector3.Cross(from, to);
            return new Quaternion(qPart.X, qPart.Y, qPart.Z, dot);
        }
    }
}
