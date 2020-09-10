#region File Description
//-----------------------------------------------------------------------------
// VictoryScreen.cs
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
#endregion

namespace Spacewar
{
    /// <summary>
    /// The 'Player X won' screen
    /// </summary>
    public class VictoryScreen : FullScreenSplash
    {
        private static string victoryScreen = @"textures\victory";
        private int winningPlayerNumber;
        private SceneItem ship;

        /// <summary>
        /// Makes a new splash screen with the right texture, no timeout and will move to the logo screen
        /// </summary>
        public VictoryScreen(Game game)
            : base(game, victoryScreen, TimeSpan.Zero, GameState.LogoSplash)
        {
            Sound.PlayCue(Sounds.TitleMusic);

            //Whoever won we need to render their ship.
            winningPlayerNumber = (SpacewarGame.Players[0].Score > SpacewarGame.Players[1].Score) ? 0 : 1;

            Player winningPlayer = SpacewarGame.Players[winningPlayerNumber];

            ship = new SceneItem(game, new EvolvedShape(game, EvolvedShapes.Ship, (winningPlayerNumber == 0) ? PlayerIndex.One : PlayerIndex.Two, (int)winningPlayer.ShipClass, winningPlayer.Skin, LightingType.Menu), new Vector3(-90, -30, 0));
            ship.Scale = new Vector3(.07f, .07f, .07f);
            scene.Add(ship);
        }

        /// <summary>
        /// Updates the scene
        /// </summary>
        /// <param name="time">Current Game time</param>
        /// <param name="elapsedTime">Elapsed Game time since last update</param>
        /// <returns></returns>
        public override GameState Update(TimeSpan time, TimeSpan elapsedTime)
        {
            ship.Rotation = new Vector3(-.3f, (float)time.TotalSeconds, 0);

            return base.Update(time, elapsedTime);
        }

        /// <summary>
        /// Renders the victory screen
        /// </summary>
        public override void Render()
        {
            //Render everything else
            base.Render();

            //Change the 'player1' to player2 if player2 won
            if (winningPlayerNumber == 1) //player2 won
            {
                IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));

                GraphicsDevice device = graphicsService.GraphicsDevice;
                Texture2D mainTexture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + victoryScreen);

                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);
                device.DepthStencilState = DepthStencilState.DepthRead;

                SpriteBatch.Draw(mainTexture, new Vector2(320, 525), new Rectangle(50, 730, 320, 85), Color.White);
                SpriteBatch.End();
            }
        }

        public override void OnCreateDevice()
        {
            base.OnCreateDevice();

            ship.ShapeItem.OnCreateDevice();
        }
    }
}
