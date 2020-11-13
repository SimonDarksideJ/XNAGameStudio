#region File Description
//-----------------------------------------------------------------------------
// CarSelection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.GameLogic;
using RacingGame.Graphics;
using RacingGame.Sounds;
using Texture = RacingGame.Graphics.Texture;
using RacingGame.Shaders;
using RacingGame.Properties;
#endregion

namespace RacingGame.GameScreens
{
    /// <summary>
    /// Car selection
    /// </summary>
    /// <returns>IGame screen</returns>
    class CarSelection : IGameScreen
    {
        #region Car type variables (max speed, acceleration, etc.)
        /// <summary>
        /// Max speed for each car type
        /// </summary>
        private static float[] CarTypeMaxSpeed = new float[]
            {
                // Car 1 (orange stripes on top)
                CarPhysics.DefaultMaxSpeed * 1.05f, // 288 mph
                // Car 2 (blue stripes on side)
                CarPhysics.DefaultMaxSpeed, // 275 mph
                // Car 3 (Just white)
                CarPhysics.DefaultMaxSpeed * 0.88f, // 240 mph
            };

        /// <summary>
        /// Car mass for each car type
        /// </summary>
        private static float[] CarTypeMass = new float[]
            {
                // Car 1 (orange stripes on top)
                CarPhysics.DefaultCarMass * 1.015f, // 1015 kg
                // Car 2 (blue stripes on side)
                CarPhysics.DefaultCarMass * 1.175f, // 1175 kg
                // Car 3 (Just white)
                CarPhysics.DefaultCarMass * 0.875f, // 875 kg
            };

        /// <summary>
        /// Max acceleration for each car type
        /// </summary>
        private static float[] CarTypeMaxAcceleration = new float[]
            {
                // Car 1 (orange stripes on top)
                CarPhysics.DefaultMaxAccelerationPerSec * 0.85f, // 4 m/s^2
                // Car 2 (blue stripes on side)
                CarPhysics.DefaultMaxAccelerationPerSec * 1.2f, // 6 m/s^2
                // Car 3 (Just white)
                CarPhysics.DefaultMaxAccelerationPerSec, // 5 m/s^2
            };
        // Rest of car variables is automatically calculated below!
        #endregion

		#region Update
		/// <summary>
		/// Unimplemented
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update(GameTime gameTime)
		{

		}
		#endregion

		#region Render
		/// <summary>
        /// Render
        /// </summary>
        /// <returns>Bool</returns>
        public bool Render()
        {
            if (BaseGame.AllowShadowMapping)
            {
                // Let camera point directly at the center, around 10 units away.
                BaseGame.ViewMatrix = Matrix.CreateLookAt(
                    new Vector3(0, 10.45f, 2.75f),
                    new Vector3(0, 0, -1),
                    new Vector3(0, 0, 1));

                // Let the light come from the front!
                Vector3 lightDir = -LensFlare.DefaultLightPos;
                lightDir = new Vector3(lightDir.X, lightDir.Y, -lightDir.Z);
                // LightDirection will normalize
                BaseGame.LightDirection = lightDir;

                // Show 3d cars
                // Rotate all 3 cars depending on the current selection
                float perCarRot = MathHelper.Pi * 2.0f / 3.0f;
                float newCarSelectionRotationZ =
                    RacingGameManager.currentCarNumber * perCarRot;
                carSelectionRotationZ = InterpolateRotation(
                    carSelectionRotationZ, newCarSelectionRotationZ,
                    BaseGame.MoveFactorPerSecond * 5.0f);
                // Prebuild all render matrices, we will use them for several times
                // here.
                Matrix[] renderMatrices = new Matrix[3];
                for (int carNum = 0; carNum < 3; carNum++)
                    renderMatrices[carNum] =
                        Matrix.CreateRotationZ(BaseGame.TotalTime / 3.9f) *
                        Matrix.CreateTranslation(new Vector3(0, 5.0f, 0)) *
                        Matrix.CreateRotationZ(-carSelectionRotationZ + carNum * perCarRot) *
                        Matrix.CreateTranslation(new Vector3(1.5f, 0.0f, 1.0f));
                // Last translation translates the position of the cars in the UI;

                // For shadows make sure the car position is the origin
                RacingGameManager.Player.SetCarPosition(Vector3.Zero,
                    new Vector3(0, 1, 0), new Vector3(0, 0, 1));

                // Generate shadows
                ShaderEffect.shadowMapping.GenerateShadows(
                    delegate
                    {
                        for (int carNum = 0; carNum < 3; carNum++)
                            // Only the car throws shadows
                            RacingGameManager.CarModel.GenerateShadow(
                                renderMatrices[carNum]);
                    });

                // Render shadows
                ShaderEffect.shadowMapping.RenderShadows(
                    delegate
                    {
                        for (int carNum = 0; carNum < 3; carNum++)
                        {
                            // Both the car and the selection plate receive shadows
                            RacingGameManager.CarSelectionPlate.UseShadow(
                                renderMatrices[carNum]);
                            RacingGameManager.CarModel.UseShadow(renderMatrices[carNum]);
                        }
                    });
            }

            // This starts both menu and in game post screen shader!
            // It will render into the sceneMap texture which we will use
            // later then.
            BaseGame.UI.PostScreenMenuShader.Start();

            // Render background and black bar
            BaseGame.UI.RenderMenuBackground();
            BaseGame.UI.RenderBlackBar(170, 390);

            // Immediately paint here, else post screen UI will
            // be drawn over!
            Texture.additiveSprite.End();
            Texture.alphaSprite.End();
            //SpriteHelper.DrawAllSprites();

            // Restart the sprites after the paint
            Texture.additiveSprite.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            Texture.alphaSprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Cars header
            int posX = 10;
            int posY = 18;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                posX += 36;
                posY += 26;
            }

            BaseGame.UI.Headers.RenderOnScreenRelative1600(
                posX, posY, UIRenderer.HeaderChooseCarGfxRect);

            // Allow selecting the car color
            TextureFont.WriteText(BaseGame.XToRes(85), BaseGame.YToRes(512),
                "Car Color: ");
            for (int num = 0; num < RacingGameManager.CarColors.Count; num++)
            {
                Rectangle rect =
                    RacingGameManager.currentCarColor == num ?
                    BaseGame.CalcRectangle(250 + num * 50 - 6, 500 - 6, 46 + 12, 46 + 12) :
                    BaseGame.CalcRectangle(250 + num * 50, 500, 46, 46);
                RacingGameManager.colorSelectionTexture.RenderOnScreen(
                    rect, RacingGameManager.colorSelectionTexture.GfxRectangle,
                    RacingGameManager.CarColors[num]);

                if (Input.MouseInBox(rect) &&
                    Input.MouseLeftButtonPressed)
                {
                    if (RacingGameManager.currentCarColor != num)
                        Sound.Play(Sound.Sounds.Highlight);
                    RacingGameManager.currentCarColor = num;
                }
            }

            // Show car maxSpeed, Acceleration and Mass values.
            // Also show braking, friction and engine values based on that.
            CarPhysics.SetCarVariablesForCarType(
                CarTypeMaxSpeed[RacingGameManager.currentCarNumber],
                CarTypeMass[RacingGameManager.currentCarNumber],
                CarTypeMaxAcceleration[RacingGameManager.currentCarNumber]);

            // Show info and helper texts
            //TextureFont.WriteText(30, BaseGame.YToRes(280),
            //    "Car: Left/Right");
            //TextureFont.WriteText(30, BaseGame.YToRes(370),
            //    "Color: Up/Down");

            // Calculate values
            float maxSpeed =
                -1.5f + 2.45f *
                (CarTypeMaxSpeed[RacingGameManager.currentCarNumber] /
                CarPhysics.DefaultMaxSpeed);
            float acceleration =
                -1.25f + 1.85f *
                (CarTypeMaxAcceleration[RacingGameManager.currentCarNumber] /
                CarPhysics.DefaultMaxAccelerationPerSec);
            float mass =
                -0.65f + 1.5f *
                (CarTypeMass[RacingGameManager.currentCarNumber] /
                CarPhysics.DefaultCarMass);
            float braking =
                -0.2f + acceleration - mass + maxSpeed;
            float friction =
                -1 + (1 / mass + maxSpeed / 5);
            float engine =
                -0.2f + 0.5f * (maxSpeed / mass + acceleration - maxSpeed * 5 + 5);
            if (engine > 0.95f)
                engine = 0.95f;

            ShowCarPropertyBar(
                BaseGame.XToRes(1024 - 258), BaseGame.YToRes(190),
                "Max Speed: " +
                (int)(CarTypeMaxSpeed[RacingGameManager.currentCarNumber] /
                CarPhysics.MphToMeterPerSec) + "mph",
                maxSpeed);
            ShowCarPropertyBar(
                BaseGame.XToRes(1024 - 258), BaseGame.YToRes(235),
                "Acceleration:", acceleration);
            ShowCarPropertyBar(
                BaseGame.XToRes(1024 - 258), BaseGame.YToRes(280),
                "Car Mass:",
                mass);
            ShowCarPropertyBar(
                BaseGame.XToRes(1024 - 258), BaseGame.YToRes(335),
                "Braking:", braking);
            ShowCarPropertyBar(
                BaseGame.XToRes(1024 - 258), BaseGame.YToRes(390),
                "Friction:", friction);
            ShowCarPropertyBar(
                BaseGame.XToRes(1024 - 258), BaseGame.YToRes(445),
                "Engine:", engine);

            // Also show bouncing arrow on top of car
            float arrowWave =
                (float)Math.Sin(BaseGame.TotalTime / 0.46f) *
                (float)Math.Cos(BaseGame.TotalTime / 0.285f);
            float arrowScale = 0.75f - 0.065f * arrowWave;
            Rectangle arrowRect = BaseGame.CalcRectangle(512, 120,
                (int)Math.Round(UIRenderer.BigArrowGfxRect.Width * arrowScale),
                (int)Math.Round(UIRenderer.BigArrowGfxRect.Width * arrowScale));
            arrowRect.X -= arrowRect.Width / 2;
            // Not displayed anymore ..

            // Show left/right arrows
            Rectangle selArrowGfxRect = UIRenderer.SelectionArrowGfxRect;
            Rectangle leftRect = BaseGame.CalcRectangle(35, 250,
                selArrowGfxRect.Width, selArrowGfxRect.Height);
            leftRect.Y = BaseGame.YToRes(300 + 60) + arrowRect.Y / 3;
            leftRect.X += (int)Math.Round(BaseGame.XToRes(12) * arrowWave);
            BaseGame.UI.Buttons.RenderOnScreen(
                leftRect, new Rectangle(selArrowGfxRect.X + selArrowGfxRect.Width,
                selArrowGfxRect.Y, -selArrowGfxRect.Width, selArrowGfxRect.Height));

            Rectangle rightRect = BaseGame.CalcRectangle(
                1024 - 335 - selArrowGfxRect.Width, 250,
                selArrowGfxRect.Width, selArrowGfxRect.Height);
            rightRect.Y = BaseGame.YToRes(300 + 60) + arrowRect.Y / 3;
            rightRect.X -= (int)Math.Round(BaseGame.XToRes(12) * arrowWave);
            BaseGame.UI.Buttons.RenderOnScreen(
                rightRect, UIRenderer.SelectionArrowGfxRect);

            // Also handle xbox controller input
            if (Input.GamePadLeftJustPressed ||
                Input.KeyboardLeftJustPressed ||
                Input.MouseLeftButtonJustPressed &&
                Input.MouseInBoxRelative(new Rectangle(512 + 50, 170, 512 - 150, 135)))
            {
                Sound.Play(Sound.Sounds.Highlight);
                RacingGameManager.currentCarNumber =
                    (RacingGameManager.currentCarNumber + 1) % 3;
            }
            else if (Input.GamePadRightJustPressed ||
                Input.KeyboardRightJustPressed ||
                Input.MouseLeftButtonJustPressed &&
                Input.MouseInBoxRelative(new Rectangle(100, 170, 512 - 200, 135)))
            {
                Sound.Play(Sound.Sounds.Highlight);
                RacingGameManager.currentCarNumber =
                    (RacingGameManager.currentCarNumber + 2) % 3;
            }

            // Mouse input is handled in RacingGameManager.cs
            if (Input.GamePadUpJustPressed ||
                Input.KeyboardUpJustPressed)
            {
                Sound.Play(Sound.Sounds.Highlight);
                RacingGameManager.currentCarColor = (RacingGameManager.currentCarColor +
                    RacingGameManager.NumberOfCarColors - 1) %
                    RacingGameManager.NumberOfCarColors;
            }
            else if (Input.GamePadDownJustPressed ||
                Input.KeyboardDownJustPressed)
            {
                Sound.Play(Sound.Sounds.Highlight);
                RacingGameManager.currentCarColor =
                    (RacingGameManager.currentCarColor + 1) %
                    RacingGameManager.NumberOfCarColors;
            }

            bool aButtonPressed = BaseGame.UI.RenderBottomButtons(false);
            if (Input.GamePadAJustPressed ||
                Input.KeyboardSpaceJustPressed ||
                aButtonPressed)
            {
                RacingGameManager.AddGameScreen(new TrackSelection());
                return false;
            }

            if (Input.KeyboardEscapeJustPressed ||
                Input.GamePadBJustPressed ||
                Input.GamePadBackJustPressed ||
                BaseGame.UI.backButtonPressed)
                return true;

            return false;
        }
        #endregion

        #region PostUIRender
        #region Helpers
        /// <summary>
        /// Helper for rotating the 3 cars in the car selection screen.
        /// </summary>
        float carSelectionRotationZ = 0.0f;

        /// <summary>
        /// Helper function for RotateSlowly, max. distance between
        /// sourceRot and desiredRot is PI, this allows very easy checks.
        /// </summary>
        public static void AdjustRotRange(ref float desiredRot, float sourceRot)
        {
            if (desiredRot >= sourceRot + (float)Math.PI)
                desiredRot -= (float)Math.PI * 2.0f;
            if (desiredRot < sourceRot - (float)Math.PI)
                desiredRot += (float)Math.PI * 2.0f;
        }

        /// <summary>
        /// Adjust rotation to -PI - PI range
        /// </summary>
        public static void AdjustRotToPIRange(ref float desiredRot)
        {
            if (desiredRot <= -(float)Math.PI)
                desiredRot += (float)Math.PI * 2.0f;
            if (desiredRot > (float)Math.PI)
                desiredRot -= (float)Math.PI * 2.0f;
        }

        /// <summary>
        /// Interpolate rotation
        /// </summary>
        /// <param name="rot">Rot</param>
        /// <param name="targetRot">Target rot</param>
        /// <param name="nearlyEqualRot">Nearly equal rot</param>
        /// <returns>Float</returns>
        public static float InterpolateRotation(
            float rot, float targetRot, float nearlyEqualRot)
        {
            AdjustRotRange(ref targetRot, rot);

            if (rot > targetRot)
            {
                if (Math.Abs(rot - targetRot) < nearlyEqualRot)
                    rot = targetRot;
                else
                    rot -= nearlyEqualRot;
            }
            else if (rot < targetRot)
            {
                if (Math.Abs(rot - targetRot) < nearlyEqualRot)
                    rot = targetRot;
                else
                    rot += nearlyEqualRot;
            }

            // Check if rot is in -PI-PI range (for easier calculations!)
            AdjustRotToPIRange(ref rot);

            return rot;
        }
        #endregion

        #region Show car properties bar
        Rectangle gfxBarFromOptionsScreen = new Rectangle(
            372, 297, 472, 6);
        /// <summary>
        /// Show car property bar for car selection to differentiate
        /// the different cars.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        private void ShowCarPropertyBar(int x, int y,
            string propertyName, float value)
        {
            TextureFont.WriteText(x, y, propertyName);
            RacingGameManager.UI.OptionsScreen.RenderOnScreen(
                //try1: new Rectangle(x + BaseGame.XToRes(150), y + BaseGame.YToRes(5),
                //BaseGame.XToRes((int)(145 * value)), BaseGame.YToRes(6)),
                new Rectangle(x, y + BaseGame.YToRes(29),
                BaseGame.XToRes((int)(192 * value)), BaseGame.YToRes(6)),
                gfxBarFromOptionsScreen);
        }
        #endregion

        /// <summary>
        /// Post user interface render
        /// </summary>
        public void PostUIRender()
        {
            // Let camera point directly at the center, around 10 units away.
            Matrix remViewMatrix = BaseGame.ViewMatrix;
            BaseGame.ViewMatrix = Matrix.CreateLookAt(
                new Vector3(0, 10.45f, 2.75f),
                new Vector3(0, 0, -1),
                new Vector3(0, 0, 1));

            // Let the light come from the front!
            Vector3 lightDir = -LensFlare.DefaultLightPos;
            lightDir = new Vector3(lightDir.X, lightDir.Y, -lightDir.Z);
            // LightDirection will normalize
            BaseGame.LightDirection = lightDir;

            // Show 3d cars
            // Rotate all 3 cars depending on the current selection
            float perCarRot = MathHelper.Pi * 2.0f / 3.0f;
            float newCarSelectionRotationZ =
                RacingGameManager.currentCarNumber * perCarRot;
            carSelectionRotationZ = InterpolateRotation(
                carSelectionRotationZ, newCarSelectionRotationZ,
                BaseGame.MoveFactorPerSecond * 5.0f);
            // Prebuild all render matrices, we will use them for several times
            // here.
            Matrix[] renderMatrices = new Matrix[3];
            for (int carNum = 0; carNum < 3; carNum++)
                renderMatrices[carNum] =
                    Matrix.CreateRotationZ(BaseGame.TotalTime / 3.9f) *
                    Matrix.CreateTranslation(new Vector3(0, 5.0f, 0)) *
                    Matrix.CreateRotationZ(-carSelectionRotationZ + carNum * perCarRot) *
                    Matrix.CreateTranslation(new Vector3(1.5f, 0.0f, 1.0f));
            // Last translation translates the position of the cars in the UI

            // For shadows make sure the car position is the origin
            RacingGameManager.Player.SetCarPosition(Vector3.Zero,
                new Vector3(0, 1, 0), new Vector3(0, 0, 1));

            // Now do the real rendering:
            for (int carNum = 0; carNum < 3; carNum++)
            {
                RacingGameManager.CarSelectionPlate.Render(renderMatrices[carNum]);
                RacingGameManager.CarModel.RenderCar(
                    carNum,
                    RacingGameManager.CarColor,
                    false,
                    renderMatrices[carNum]);
            }

            // Render all models we remembered this frame (we are in PostUIRender,
            // and we changed our view matrix, render directly here).
            BaseGame.MeshRenderManager.Render();

            // And finally add shadows to the scene
            if (BaseGame.AllowShadowMapping)
            {
                ShaderEffect.shadowMapping.ShowShadows();
            }

            // Reset stuff
            BaseGame.WorldMatrix = Matrix.Identity;
            BaseGame.ViewMatrix = remViewMatrix;
        }
        #endregion
    }
}
