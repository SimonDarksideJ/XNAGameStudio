#region File Description
//-----------------------------------------------------------------------------
// CompletedScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace Pickture
{
    /// <summary>
    /// Reward screen for completing a puzzle. The pictures used for the puzzle are put
    /// onto photographs. The photographs float down away from the camera and 
    /// fade to black.
    /// </summary>
    class CompletedScreen : GameScreen
    {
        Model photographModel;

        PictureSet pictureSet;
        List<Photograph> photographs;

        Matrix projectionMatrix;

        // Distance from camera where the black fog begins
        const float FadeOutStartZValue = -2000.0f;
        // Distance from the camera where the photograph is fully faded out, so it 
        // should be moved back behind the camera to float down again.
        public const float ResetZValue = -4000.0f;

        static readonly Matrix viewMatrix = Matrix.CreateLookAt(
            new Vector3(0, 0, 150.0f), Vector3.Zero, Vector3.UnitY);

        /// <summary>
        /// Constructs an instance of Completed screen with the set of pictures to put
        /// onto the photographs.
        /// </summary>
        public CompletedScreen(PictureSet pictureSet)
        {
            this.pictureSet = pictureSet;
            
            Audio.Play("Puzzle Completed");
            photographs = new List<Photograph>();

            int photographCount = 40;
            for (int i = 0; i < photographCount; i++)
            {
                Photograph newphotograph = new Photograph();
                ResetPhotograph(newphotograph);
                photographs.Add(newphotograph);
            }

            TransitionOnTime = Pickture.TransitionTime;
            TransitionOffTime = TimeSpan.FromSeconds(0.75f);
        }

        public override void LoadContent()
        {
            photographModel = Pickture.Instance.Content.Load<Model>(
                "Models/photograph");

            pictureSet.Load();

            // Recalculate aspect ratio
            Viewport viewport = Pickture.Instance.GraphicsDevice.Viewport;
            float aspectRatio = (float)viewport.Width / viewport.Height;
            // Recalculate projection matrix
            projectionMatrix =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f),
                aspectRatio, 1.0f, ResetZValue * -1.5f);

            base.LoadContent();
        }

        public override void Update(GameTime gameTime,
            bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Let the previous screen transition fully off before dropping photographs
            if (ScreenState == ScreenState.TransitionOn)
                return;

            // Animate all of the photographs
            foreach (Photograph photograph in photographs)
            {
                photograph.Update(gameTime);

                // If a photograph has floated out of view, move it back behind 
                // the camera to float down again.
                if (photograph.Position.Z < ResetZValue)
                    ResetPhotograph(photograph);
            }
        }

        public override void HandleInput(InputState input)
        {
            // If the user presses a button, return to the menu
            if (input.MenuCancel || input.MenuSelect)
            {
                ExitScreen();
                ScreenManager.AddScreen(new MainMenuScreen());
            }
        }

        /// <summary>
        ///  Resets a photograph for another fall. Also puts a random picture on it.
        /// </summary>
        /// <param name="photograph">The photograph to reset.</param>
        void ResetPhotograph(Photograph photograph)
        {
            photograph.Texture =
                pictureSet.GetTexture(RandomHelper.Random.Next(pictureSet.Count));
            photograph.Reset();
        }

        public override void UnloadContent()
        {
            pictureSet.Unload();

            base.UnloadContent();
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // Let the previous screen transition fully off before drawing anything
            if (ScreenState == ScreenState.TransitionOn)
                return;

            // When this screen transitions off, the fog rolls in closer to the camera.
            // This causes all of the photographs to fade to black behind the menu.
            float fogEnd = -ResetZValue * (1 - TransitionPosition);

            DrawHelper.SetState();

            foreach (Photograph photograph in photographs)
            {
                foreach (ModelMesh mesh in photographModel.Meshes)
                {
                    // Set effect parameters for the picture and its frame
                    foreach (BasicEffect basicEffect in mesh.Effects)
                    {
                        basicEffect.World = photograph.WorldTransform;
                        basicEffect.View = viewMatrix;
                        basicEffect.Projection = projectionMatrix;
                        basicEffect.Texture = photograph.Texture;
                        basicEffect.PreferPerPixelLighting = true;
                        basicEffect.EnableDefaultLighting();

                        // Fog is used to make distant photographs gradually 
                        // fade to black
                        basicEffect.FogEnabled = true;
                        basicEffect.FogEnd = fogEnd;
                    }

                    // Enable the texture on the picture part
                    ((BasicEffect)mesh.Effects[1]).TextureEnabled = true;

                    mesh.Draw();
                }
            }         
        }
    }
}
