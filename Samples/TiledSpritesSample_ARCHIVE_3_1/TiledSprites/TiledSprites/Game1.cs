#region File Description
//-----------------------------------------------------------------------------
// Game1.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace TiledSprites
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";            
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            lastInput = 0;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        Texture2D xna;
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            xna = Content.Load<Texture2D>("XNA");
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        double lastInput;
        //Input is clamped to 30 frames per second
        double inputdelay = 1000 / 30;  
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == 
                ButtonState.Pressed)
                this.Exit();


            if (lastInput + inputdelay <= 
                gameTime.TotalGameTime.TotalMilliseconds)
            {
                // Use the GamePad buttons to increase or decrease 
                // the number of tiles
                if (GamePad.GetState(PlayerIndex.One).Buttons.B == 
                    ButtonState.Pressed)
                    TilesX += 1;
                if (GamePad.GetState(PlayerIndex.One).Buttons.X == 
                    ButtonState.Pressed)
                    TilesX -= 1;

                if (GamePad.GetState(PlayerIndex.One).Buttons.Y == 
                    ButtonState.Pressed)
                    TilesY -= 1;
                if (GamePad.GetState(PlayerIndex.One).Buttons.A == 
                    ButtonState.Pressed)
                    TilesY += 1;
                lastInput = gameTime.TotalGameTime.TotalMilliseconds;
            }


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        
        int TilesX = 2;
        int TilesY = 2;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Rectangle source = new Rectangle(0, 0, xna.Width * TilesX, 
                xna.Height * TilesY);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, 
                SpriteSortMode.Immediate, SaveStateMode.None);
            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            Vector2 pos = new Vector2(50);
            spriteBatch.Draw(xna, pos, source, Color.White, 0, 
                Vector2.Zero, 0.5f, SpriteEffects.None, 1.0f);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
