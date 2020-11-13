#region File Description
//-----------------------------------------------------------------------------
// LensFlare.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using RacingGame.Helpers;
using RacingGame.Graphics;
using RacingGame.GameLogic;
using Material = RacingGame.Graphics.Material;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RacingGame.Graphics
{
    /// <summary>
    /// Lens flare class for simple 2d lens flare rendering.
    /// 
    /// If you want to read more about lens flares I suggest the following
    /// articles:
    /// http://www.gamedev.net/reference/articles/article813.asp
    /// http://www.gamedev.net/reference/articles/article874.asp
    /// Game Programming Gems, Chapter 5.1
    /// Game Programming Gems 2, Chapter 5.5
    /// 
    /// Note: This class is not as powerful as the lensflares in
    /// Rocket Commander because occlusion querying is not possible in XNA yet!
    /// </summary>
    public class LensFlare : IDisposable
    {
        #region Variables
        /// <summary>
        /// Default sun position!
        /// </summary>
        public static Vector3 DefaultSunPos =
            new Vector3(+2500, -22500, +15000);

        /// <summary>
        /// Put light at a little different location because we want
        /// to see the sun, but still have the light come more from the top.
        /// </summary>
        public static Vector3 DefaultLightPos =
            //new Vector3(+5500, -9250, +15000);
            new Vector3(+8500, -7250, +15000);

        /// <summary>
        /// Returns rotated sun position for the game.
        /// </summary>
        /// <param name="rotation">Rotation</param>
        /// <returns>Vector 3</returns>
        public static Vector3 RotateSun(float rotation)
        {
            Vector3 sunPos = DefaultSunPos;
            Vector2 right = new Vector2(
                +(float)Math.Cos(rotation),
                +(float)Math.Sin(rotation));
            Vector2 up = new Vector2(
                +(float)Math.Sin(rotation),
                -(float)Math.Cos(rotation));

            // Only rotate x and z
            return new Vector3(
                -right.X * sunPos.X - up.X * sunPos.Z,
                sunPos.Y,
                -right.Y * sunPos.X - up.Y * sunPos.Z);
        }

        /// <summary>
        /// Lens flare origin in 3D.
        /// </summary>
        private static Vector3 lensOrigin3D;

        /// <summary>
        /// Screen flare size (resolution dependant, 250 is for 1024*768).
        /// </summary>
        private static int ScreenFlareSize = 225;

        /// <summary>
        /// Flare texture types.
        /// </summary>
        protected const int
            SunFlareType = 0,
            GlowFlareType = 1,
            LensFlareType = 2,
            StreaksType = 3,
            RingType = 4,
            HaloType = 5,
            CircleType = 6,
            NumberOfFlareTypes = 7;

        /// <summary>
        /// Flare textures
        /// </summary>
        protected Texture[] flareTextures =
            new Texture[NumberOfFlareTypes];

        /// <summary>
        /// Flare texture names
        /// </summary>
        string[] flareTextureNames = new string[]
            {
                "Sun",
                "Glow",
                "Lens",
                "Streaks",
                "Ring",
                "Halo",
                "Circle",
            };

        /// <summary>
        /// Load textures, should be called in constructor.
        /// </summary>
        private void LoadTextures()
        {
            for (int num = 0; num < NumberOfFlareTypes; num++)
                flareTextures[num] = new Texture(flareTextureNames[num]);
        }
        #endregion

        #region Flare data struct
        /// <summary>
        /// Flare data struct for the quick and easy flare type list below.
        /// </summary>
        protected struct FlareData
        {
            /// <summary>
            /// Type of flare, see above.
            /// </summary>
            public int type;
            /// <summary>
            /// Position of flare (1=origin, 0=center of screen, -1=other side)
            /// </summary>
            public float position;
            /// <summary>
            /// Scale of flare in relation to MaxFlareSize.
            /// </summary>
            public float scale;
            /// <summary>
            /// Color of this flare.
            /// </summary>
            public Color color;

            /// <summary>
            /// Constructor to set all values.
            /// </summary>
            /// <param name="setType">Set type</param>
            /// <param name="setPosition">Set position</param>
            /// <param name="setScale">Set scale</param>
            /// <param name="setColor">Set Color</param>
            public FlareData(int setType, float setPosition,
                float setScale, Color setColor)
            {
                type = setType;
                position = setPosition;
                scale = setScale;
                color = setColor;
            }
        }

        /// <summary>
        /// Flare types for the lens flares.
        /// </summary>
        protected FlareData[] flareTypes = new FlareData[]
        {
            // Small red/yellow/gray halo behind sun
            new FlareData(
            CircleType, 1.2f, 0.55f, new Color(175, 175, 255, 20)),

            // The sun, sun+streaks+glow+red ring
            //small sun:
            //new FlareData(
            //SunFlareType, 1.0f, 0.6f, new Color(255, 255, 255, 255)),
            //new FlareData(
            //StreaksType, 1.0f, 1.5f, new Color(255, 255, 255, 128)),
            //new FlareData(
            //GlowFlareType, 1.0f, 1.7f, new Color(255, 255, 200, 100)),
            //bigger sun and much bigger glow effect:
            new FlareData(
            SunFlareType, 1.0f, 0.9f, new Color(255, 255, 255, 255)),
            new FlareData(
            StreaksType, 1.0f, 1.8f, new Color(255, 255, 255, 128)),
            new FlareData(
            GlowFlareType, 1.0f, 2.6f, new Color(255, 255, 200, 100)),
            //new FlareData(
            //RingNum, 1.0f, 0.9f, new Color(255, 120, 120, 150)),

            // 3 blue circles at 0.5 distance
            new FlareData(
            CircleType, 0.5f, 0.12f, new Color(60, 60, 180, 35)),
            new FlareData(
            CircleType, 0.45f, 0.46f, new Color(100, 100, 200, 60)),
            new FlareData(
            CircleType, 0.4f, 0.17f, new Color(120, 120, 220, 40)),

            new FlareData(
            RingType, 0.15f, 0.2f, new Color(60, 60, 255, 100)),
            new FlareData(
            LensFlareType, -0.5f, 0.2f, new Color(255, 60, 60, 130)),
            new FlareData(
            LensFlareType, -0.15f, 0.15f, new Color(255, 60, 60, 90)),
            new FlareData(
            HaloType, -0.3f, 0.6f, new Color(60, 60, 255, 180)),
            
            // 3 red halos and circles on the opposite side of the blue halos
            new FlareData(
            HaloType, -0.4f, 0.2f, new Color(220, 80, 80, 98)),
            //new FlareData(
            //HaloNum, -0.45f, 0.6f, new Color(220, 80, 80, 95)),
            new FlareData(
            CircleType, -0.5f, 0.1f, new Color(220, 80, 80, 85)),

            new FlareData(
            HaloType, -0.6f, 0.5f, new Color(60, 60, 255, 80)),
            new FlareData(
            RingType, -0.8f, 0.3f, new Color(90, 60, 255, 110)),

            new FlareData(
            HaloType, -0.95f, 0.5f, new Color(60, 60, 255, 120)),
            new FlareData(
            CircleType, -1.0f, 0.15f, new Color(60, 60, 255, 85)),
        }; // flareTypes[]
        #endregion

        #region Properties
        /// <summary>
        /// Origin 3D
        /// </summary>
        /// <returns>Vector3</returns>
        public static Vector3 Origin3D
        {
            set
            {
                lensOrigin3D = value;
            }
            get
            {
                return lensOrigin3D;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create lens flare base
        /// </summary>
        public LensFlare(Vector3 setLensOrigin3D)
        {
            lensOrigin3D = setLensOrigin3D;
            LoadTextures();
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int num = 0; num < NumberOfFlareTypes; num++)
                    if (flareTextures[num] != null)
                        flareTextures[num].Dispose();
            }
        }
        #endregion

        #region Render
        /// <summary>
        /// Current sun intensity used for our lens flare effect.
        /// </summary>
        float sunIntensity = 0.0f;

        /// <summary>
        /// Render lens flare.
        /// Checks the zbuffer if objects are occluding the lensflare sun.
        /// </summary>
        public void Render(Color sunColor)
        {
            ScreenFlareSize = 250 * BaseGame.Width / 1024;
            Vector3 relativeLensPos = lensOrigin3D + BaseGame.CameraPos;

            // Only show lens flare if facing in the right direction
            if (BaseGame.IsInFrontOfCamera(relativeLensPos) == false)
                return;

            // Convert 3D point to 2D!
            Point lensOrigin =
                BaseGame.Convert3DPointTo2D(relativeLensPos);

            // Check sun occlusion itensity and fade it in and out!
            float thisSunIntensity = 0.75f;
            /*unsupported in XNA: BaseGame.OcclusionIntensity(
                 flareMaterials[SunFlareType].diffuseTexture,
                 lensOrigin, ScreenFlareSize / 5);
             */
            sunIntensity = thisSunIntensity * 0.1f + sunIntensity * 0.9f;

            // We can skip rendering the sun if the itensity is to low
            if (sunIntensity < 0.01f)
                return;

            int resWidth = BaseGame.Width,
                resHeight = BaseGame.Height;
            Point center = new Point(resWidth / 2, resHeight / 2);
            Point relOrigin = new Point(
                center.X - lensOrigin.X, center.Y - lensOrigin.Y);

            // Check if origin is on screen, fade out at borders
            float alpha = 1.0f;
            float distance = Math.Abs(Math.Max(relOrigin.X, relOrigin.Y));
            if (distance > resHeight / 1.75f)
            {
                distance -= resHeight / 1.75f;
                // If distance is more than half the resolution, don't show anything!
                if (distance > resHeight / 1.75f)
                    return;
                alpha = 1.0f - (distance / ((float)resHeight / 1.75f));
                if (alpha > 1.0f)
                    alpha = 1.0f;
            }

            // Use square of sunIntensity for lens flares because we want
            // them to get very weak if sun is not fully visible.
            alpha *= sunIntensity * sunIntensity;

            foreach (FlareData data in flareTypes)
            {
                int size = (int)(ScreenFlareSize * data.scale);
                flareTextures[data.type].RenderOnScreen(
                    new Rectangle(
                    (int)(center.X - relOrigin.X * data.position - size / 2),
                    (int)(center.Y - relOrigin.Y * data.position - size / 2),
                    size, size),
                    flareTextures[data.type].GfxRectangle,
                    ColorHelper.ApplyAlphaToColor(//MixAlphaToColor(
                    ColorHelper.MultiplyColors(sunColor, data.color),
                    ((float)data.color.A / 255.0f) *
                    // For the sun and glow flares try always to use max. intensity
                    (data.type == SunFlareType || data.type == GlowFlareType ?
                    sunIntensity : alpha)),
                    BlendState.Additive);
            }
        }
        #endregion
    }
}