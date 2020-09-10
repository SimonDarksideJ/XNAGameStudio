#region File Description
//-----------------------------------------------------------------------------
// Spaceship.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Graphics3DSample;
using System;
#endregion

namespace Graphics3DSample
{
    /// <summary>
    /// Helper class for drawing a spaceship model with animated wheels and turret.
    /// </summary>
    public class Spaceship
    {
        #region Fields
        // The XNA framework Model object that we are going to display.
        Model spaceshipModel;

        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it
        // is more efficient to reuse a single array, as this avoids creating
        // unnecessary garbage.
        Matrix[] boneTransforms;

        // Spaceship drawing parameters
        private Matrix projection;
        private Matrix rotation;
        private Matrix view;
        private bool[] lights;
        bool isTextureEnabled;
        bool isPerPixelLightingEnabled;
        #endregion

        #region Initialization
        /// <summary>
        /// Loads the spaceship model.
        /// </summary>
        public void Load(ContentManager content)
        {
            // Load the spaceship model from the ContentManager.
            spaceshipModel = content.Load<Model>("Models/spaceship");

            // Allocate the transform matrix array.
            boneTransforms = new Matrix[spaceshipModel.Bones.Count];
        }
        #endregion

        #region Public accessors
        /// <summary>
        /// Gets or sets the projection matrix value.
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }

        /// <summary>
        /// Gets or sets the rotation matrix value.
        /// </summary>
        public Matrix Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// Gets or sets the rotation matrix value.
        /// </summary>
        public bool IsTextureEnabled
        {
            get { return isTextureEnabled; }
            set { isTextureEnabled = value; }
        }

        /// <summary>
        /// Gets or sets the view matrix value.
        /// </summary>
        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }

        /// <summary>
        /// Gets or sets the lights states.
        /// </summary>
        public bool[] Lights
        {
            get { return lights; }
            set { lights = value; }
        }

        /// <summary>
        /// Gets or sets the per pixel lighting preferences
        /// </summary>
        public bool IsPerPixelLightingEnabled
        {
            get { return isPerPixelLightingEnabled; }
            set { isPerPixelLightingEnabled = value; }
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draws the spaceship model, using the current drawing parameters.
        /// </summary>
        public void Draw()
        {
            // Set the world matrix as the root transform of the model.
            spaceshipModel.Root.Transform = Rotation;

            // Look up combined bone matrices for the entire model.
            spaceshipModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Draw the model.
            foreach (ModelMesh mesh in spaceshipModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    effect.View = View;
                    effect.Projection = Projection;

                    SetEffectLights(effect, Lights);
                    SetEffectPerPixelLightingEnabled(effect);

                    effect.TextureEnabled = IsTextureEnabled;
                }

                mesh.Draw();
            }
        }

        /// <summary>
        /// Sets effect's per pixel lighting preference
        /// </summary>
        /// <param name="effect"></param>
        private void SetEffectPerPixelLightingEnabled(BasicEffect effect)
        {
            effect.PreferPerPixelLighting = isPerPixelLightingEnabled;
        }

        /// <summary>
        /// Sets effects lighting properties
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="lights"></param>
        private void SetEffectLights(BasicEffect effect, bool[] lights)
        {

            effect.Alpha = 1.0f;
            effect.DiffuseColor = new Vector3(0.75f, 0.75f, 0.75f);
            effect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            effect.SpecularPower = 5.0f;
            effect.AmbientLightColor = new Vector3(0.75f, 0.75f, 0.75f);

            effect.DirectionalLight0.Enabled = lights[0];
            effect.DirectionalLight0.DiffuseColor = Vector3.One;
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(1, -1, 0));
            effect.DirectionalLight0.SpecularColor = Vector3.One;

            effect.DirectionalLight1.Enabled = lights[1];
            effect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
            effect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(-1, -1, 0));
            effect.DirectionalLight1.SpecularColor = new Vector3(1f, 1f, 1f);

            effect.DirectionalLight2.Enabled = lights[2];
            effect.DirectionalLight2.DiffuseColor = new Vector3(0.3f, 0.3f, 0.3f);
            effect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(-1, -1, -1));
            effect.DirectionalLight2.SpecularColor = new Vector3(0.3f, 0.3f, 0.3f);

            effect.LightingEnabled = true;
        }
        #endregion

    }
}
