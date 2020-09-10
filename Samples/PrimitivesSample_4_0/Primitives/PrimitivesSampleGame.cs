#region File Description
//-----------------------------------------------------------------------------
// PrimitivesSampleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
#endregion

namespace PrimitivesSample
{
    // This sample illustrates the use of PrimitiveBatch to draw lines and points
    // on the screen. Lines and points are used to recreate the Spacewars starter kit's
    // retro mode.
    public class PrimitivesSampleGame : Microsoft.Xna.Framework.Game
    {
        #region Constants

        // this constant controls the number of stars that will be created when the game
        // starts up.
        const int NumStars = 500;

        // what percentage of those stars will be "big" stars? the default is 20%.
        const float PercentBigStars = .2f;

        // how bright will stars be?  somewhere between these two values.
        const byte MinimumStarBrightness = 56;
        const byte MaximumStarBrightness = 255;

        // how big is the ship?
        const float ShipSizeX = 10f;
        const float ShipSizeY = 15f;
        const float ShipCutoutSize = 5f;

        // the radius of the sun.
        const float SunSize = 30f;

        #endregion

        #region Fields

        GraphicsDeviceManager graphics;

        // PrimitiveBatch is the new class introduced in this sample. We'll use it to
        // draw everything in this sample, including the stars, ships, and sun.
        PrimitiveBatch primitiveBatch;

        // these two lists, stars, and starColors, keep track of the positions and
        // colors of all the stars that we will randomly generate during the initialize
        // phase.
        List<Vector2> stars = new List<Vector2>();
        List<Color> starColors = new List<Color>();

        #endregion

        #region Initialization
        public PrimitivesSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;
            graphics.IsFullScreen = true;
#else
            // set the backbuffer size to something that will work well on both xbox
            // and windows.
            graphics.PreferredBackBufferWidth = 853;
            graphics.PreferredBackBufferHeight = 480;
#endif
        }

        protected override void Initialize()
        {
            base.Initialize();

            // CreateStars needs to know how big the GraphicsDevice's viewport is, so 
            // once base.Initialize has been called, we can call this.
            CreateStars();
        }

        private void CreateStars()
        {
            // since every star will be put in a random place and have a random color, 
            // a random number generator might come in handy.
            Random random = new Random();

            // where can we put the stars?
            int screenWidth = graphics.GraphicsDevice.Viewport.Width;
            int screenHeight = graphics.GraphicsDevice.Viewport.Height;


            for (int i = 0; i < NumStars; i++)
            {
                // pick a random spot...
                Vector2 where = new Vector2(
                    random.Next(0, screenWidth),
                    random.Next(0, screenHeight));

                // ...and a random color. it's safe to cast random.Next to a byte,
                // because MinimumStarBrightness and MaximumStarBrightness are both
                // bytes.
                byte greyValue =
                    (byte)random.Next(MinimumStarBrightness, MaximumStarBrightness);
                Color color = new Color(greyValue, greyValue, greyValue);

                // if the random number was greater than the percentage chance for a big
                // star, this is just a normal star.
                if ((float)random.NextDouble() > PercentBigStars)
                {
                    starColors.Add(color);
                    stars.Add(where);
                }
                else
                {
                    // if this star is randomly selected to be a "big" star, we actually
                    // add four points and colors to stars and starColors. big stars are
                    // a block of four points, instead of just one point.
                    for (int j = 0; j < 4; j++)
                    {
                        starColors.Add(color);
                    }

                    stars.Add(where);
                    stars.Add(where + Vector2.UnitX);
                    stars.Add(where + Vector2.UnitY);
                    stars.Add(where + Vector2.One);
                }
            }
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            primitiveBatch = new PrimitiveBatch(graphics.GraphicsDevice);
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // how big is the screen? we'll use that information to center the sun
            // and place the ships.
            int screenWidth = graphics.GraphicsDevice.Viewport.Width;
            int screenHeight = graphics.GraphicsDevice.Viewport.Height;

            // draw the sun in the center
            DrawSun(new Vector2(screenWidth / 2, screenHeight / 2));

            // draw the left hand ship
            DrawShip(new Vector2(100, screenHeight / 2));

            // and the right hand ship
            DrawShip(new Vector2(screenWidth - 100, screenHeight / 2));

            DrawStars();

            base.Draw(gameTime);
        }

        // DrawStars is called to do exactly what its name says: draw the stars.
        private void DrawStars()
        {
            // stars are drawn as a list of points, so begin the primitiveBatch.
            primitiveBatch.Begin(PrimitiveType.TriangleList);

            // loop through all of the stars, and tell primitive batch to draw them.
            // each star is a very small triangle.
            for (int i = 0; i < stars.Count; i++)
            {
                primitiveBatch.AddVertex(stars[i], starColors[i]);
                primitiveBatch.AddVertex(stars[i] + Vector2.UnitX, starColors[i]);
                primitiveBatch.AddVertex(stars[i] + Vector2.UnitY, starColors[i]);
            }

            // and then tell it that we're done.
            primitiveBatch.End();
        }

        // called to draw the spacewars ship at a point on the screen.
        private void DrawShip(Vector2 where)
        {
            // tell the primitive batch to start drawing lines
            primitiveBatch.Begin(PrimitiveType.LineList);

            // from the nose, down the left hand side
            primitiveBatch.AddVertex(
                where + new Vector2(0f, -ShipSizeY), Color.White);
            primitiveBatch.AddVertex(
                where + new Vector2(-ShipSizeX, ShipSizeY), Color.White);

            // to the right and up, into the cutout
            primitiveBatch.AddVertex(
                where + new Vector2(-ShipSizeX, ShipSizeY), Color.White);
            primitiveBatch.AddVertex(
                where + new Vector2(0f, ShipSizeY - ShipCutoutSize), Color.White);

            // to the right and down, out of the cutout
            primitiveBatch.AddVertex(
                where + new Vector2(0f, ShipSizeY - ShipCutoutSize), Color.White);
            primitiveBatch.AddVertex(
                where + new Vector2(ShipSizeX, ShipSizeY), Color.White);

            // and back up to the nose, where we started.
            primitiveBatch.AddVertex(
                where + new Vector2(ShipSizeX, ShipSizeY), Color.White);
            primitiveBatch.AddVertex(
                where + new Vector2(0f, -ShipSizeY), Color.White);

            // and we're done.
            primitiveBatch.End();
        }

        // called to draw the spacewars sun.
        private void DrawSun(Vector2 where)
        {
            // the sun is made from 4 lines in a circle.
            primitiveBatch.Begin(PrimitiveType.LineList);

            // draw the vertical and horizontal lines
            primitiveBatch.AddVertex(where + new Vector2(0, SunSize), Color.White);
            primitiveBatch.AddVertex(where + new Vector2(0, -SunSize), Color.White);

            primitiveBatch.AddVertex(where + new Vector2(SunSize, 0), Color.White);
            primitiveBatch.AddVertex(where + new Vector2(-SunSize, 0), Color.White);

            // to know where to draw the diagonal lines, we need to use trig.
            // cosine of pi / 4 tells us what the x coordinate of a circle's radius is
            // at 45 degrees. the y coordinate normally would come from sin, but sin and
            // cos 45 are the same, so we can reuse cos for both x and y.
            float sunSizeDiagonal = (float)Math.Cos(MathHelper.PiOver4);

            // since that trig tells us the x and y for a unit circle, which has a
            // radius of 1, we need scale that result by the sun's radius.
            sunSizeDiagonal *= SunSize;

            primitiveBatch.AddVertex(
                where + new Vector2(-sunSizeDiagonal, sunSizeDiagonal), Color.Gray);
            primitiveBatch.AddVertex(
                where + new Vector2(sunSizeDiagonal, -sunSizeDiagonal), Color.Gray);

            primitiveBatch.AddVertex(
                where + new Vector2(sunSizeDiagonal, sunSizeDiagonal), Color.Gray);
            primitiveBatch.AddVertex(
                where + new Vector2(-sunSizeDiagonal, -sunSizeDiagonal), Color.Gray);

            primitiveBatch.End();
        }

        #endregion

        #region Entry point

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            using (PrimitivesSampleGame game = new PrimitivesSampleGame())
            {
                game.Run();
            }
        }

        #endregion

    }
}
