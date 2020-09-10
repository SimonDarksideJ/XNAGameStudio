//-----------------------------------------------------------------------------
// ModelViewerCamera.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace RimLighting
{
    /// <summary>
    /// Simple ModelViewerCamera which uses two arcballs, one for rotating objects in world space, another one for rotating camera
    /// </summary>
    public class ModelViewerCamera
    {
        // Mode selector. Is currently rotating in world space or rotating camera?
        // True when rotating world, false when rotating camera
        public bool IsRotatingWorld
        {
            get { return isRotatingWorldInt; }
            set
            {
                if (isRotatingWorldInt == false && value == true)
                {
                    // Absorb the difference from last view rotation to current view rotation into world rotation
                    worldArcball.SetCurrentRotation(Matrix.Invert(viewArcball.GetCurrentRotationMatrix()) * lastViewRotation *
                                                    worldArcball.GetCurrentRotationMatrix() *
                                                    Matrix.Invert(lastViewRotation) * viewArcball.GetCurrentRotationMatrix());

                    // So that when lastViewRotation is updated here, GetWorldMatrix() still returns the same world rotation
                    // This is necessary since we don't want the object to jump when switching between world/camera rotation modes
                    lastViewRotation = viewArcball.GetCurrentRotationMatrix();
                }

                isRotatingWorldInt = value;
            }
        }
        protected bool isRotatingWorldInt = true;

        // Acrballs for rotating world and camera
        Arcball worldArcball;
        Arcball viewArcball;

        // The initial camera position and up direction
        Vector3 cameraPosition;
        Vector3 cameraUpDir;

        /// <summary>
        /// Constructor, camera's position and its up direction, as well as the bounding box of the arcball are needed
        /// </summary>
        public ModelViewerCamera(Vector3 CameraPosition, Vector3 CameraUpDir, int x, int y, int width, int height)
        {
            cameraPosition = CameraPosition;
            cameraUpDir = CameraUpDir;

            worldArcball = new Arcball(x, y, width, height);
            viewArcball = new Arcball(x, y, width, height);
        }

        /// <summary>
        /// Process the touch input, rotates the world or camera according to current mode selector
        /// </summary>
        public void HandleTouch(TouchLocation loc)
        {
            if (IsRotatingWorld)
                worldArcball.HandleTouch(loc);
            else
                viewArcball.HandleTouch(loc);
        }

        Matrix lastViewRotation = Matrix.Identity;

        /// <summary>
        /// Get current world matrix
        /// </summary>
        public Matrix GetWorldMatrix()
        {
            // V * W * V^-1 is needed here becase we want the object to rotate natually 
            // no matter whether the rotation of view matrix is identity or not
            return (lastViewRotation) * worldArcball.GetCurrentRotationMatrix() * Matrix.Invert(lastViewRotation);
        }

        /// <summary>
        /// Get current view matrix
        /// </summary>
        public Matrix GetViewMatrix()
        {
            // Rotate the camera 
            Matrix rot = Matrix.Invert(viewArcball.GetCurrentRotationMatrix());
            return Matrix.CreateLookAt(Vector3.Transform(cameraPosition, rot),
                                       Vector3.Zero,
                                       Vector3.Transform(cameraUpDir, rot));
        }
    }
}
