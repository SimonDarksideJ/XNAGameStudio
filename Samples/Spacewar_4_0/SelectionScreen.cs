#region File Description
//-----------------------------------------------------------------------------
// SelectionScreen.cs
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
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Spacewar
{
    /// <summary>
    /// Represents the ship selection screen
    /// </summary>
    public class SelectionScreen : Screen
    {
        private static string selectionTexture = @"textures\ship_select_FINAL";
        private Vector4 white = new Vector4(1f, 1f, 1f, 1f);

        private SceneItem[] ships = new SceneItem[2];
        private int[] selectedShip = new int[] { 0, 0 };
        private int[] selectedSkin = new int[] { 0, 0 };

        private bool player1Ready = false;
        private bool player2Ready = false;

        private Cue menuMusic;

        /// <summary>
        /// Creates a new selection screen. Plays the music and initializes the models
        /// </summary>
        public SelectionScreen(Game game)
            : base(game)
        {
            //Start menu music
            menuMusic = Sound.Play(Sounds.MenuMusic);

            ships[0] = new SceneItem(game, new EvolvedShape(game, EvolvedShapes.Ship, PlayerIndex.One, selectedShip[0], selectedSkin[0], LightingType.Menu), new Vector3(-120, 0, 0));
            ships[0].Scale = new Vector3(.05f, .05f, .05f);
            scene.Add(ships[0]);

            ships[1] = new SceneItem(game, new EvolvedShape(game, EvolvedShapes.Ship, PlayerIndex.Two, selectedShip[1], selectedSkin[1], LightingType.Menu), new Vector3(120, 0, 0));
            ships[1].Scale = new Vector3(.05f, .05f, .05f);
            scene.Add(ships[1]);
        }

        /// <summary>
        /// Updates the scene, handles the input
        /// </summary>
        /// <param name="time">Current game time</param>
        /// <param name="elapsedTime">Elapsed time since last update</param>
        /// <returns>New gamestate if required or GameState.None</returns>
        public override GameState Update(TimeSpan time, TimeSpan elapsedTime)
        {
            //A button makes a player ready. B button makes a player not ready
            if ((XInputHelper.GamePads[PlayerIndex.One].APressed) || (!XInputHelper.GamePads[PlayerIndex.One].State.IsConnected && SpacewarGame.CurrentPlatform != PlatformID.Win32NT))
                player1Ready = true;

            if ((XInputHelper.GamePads[PlayerIndex.Two].APressed) || (!XInputHelper.GamePads[PlayerIndex.Two].State.IsConnected && SpacewarGame.CurrentPlatform != PlatformID.Win32NT))
                player2Ready = true;

            if (XInputHelper.GamePads[PlayerIndex.One].BPressed)
                player1Ready = false;

            if (XInputHelper.GamePads[PlayerIndex.Two].BPressed)
                player2Ready = false;

            for (int player = 0; player < 2; player++)
            {
                if ((!player1Ready && player == 0) || (!player2Ready && player == 1))
                {
                    int ship = selectedShip[player];
                    int skin = selectedSkin[player];
                    if (XInputHelper.GamePads[(PlayerIndex)player].UpPressed)
                        selectedShip[player] += 5; //Wrap around

                    if (XInputHelper.GamePads[(PlayerIndex)player].DownPressed)
                        selectedShip[player]++;

                    if (XInputHelper.GamePads[(PlayerIndex)player].LeftPressed)
                        selectedSkin[player] += 5; //This will wraparound

                    if (XInputHelper.GamePads[(PlayerIndex)player].RightPressed)
                        selectedSkin[player]++;

                    //Make selections wrap around
                    selectedShip[player] = selectedShip[player] % 3;
                    selectedSkin[player] = selectedSkin[player] % 3;

                    //If anything's change then load the new ship/skin
                    if ((ship != selectedShip[player]) || (skin != selectedSkin[player]))
                    {
                        Sound.PlayCue(Sounds.MenuScroll);
                        ships[player].ShapeItem = new EvolvedShape(GameInstance, EvolvedShapes.Ship, (PlayerIndex)player, selectedShip[player], selectedSkin[player], LightingType.Menu);
                    }
                }
            }

            //Spin the Ships
            for (int i = 0; i < 2; i++)
            {
                //(i * 2 -1) makes player 2 spin the other way
                ships[i].Rotation = new Vector3(-.3f, (float)time.TotalSeconds * (i * 2 - 1), 0);
            }

            //Update the Scene
            base.Update(time, elapsedTime);

            //Play the next level when both players are ready
            if (player1Ready && player2Ready)
            {
                //Set global ship and skins
                for (int i = 0; i < 2; i++)
                {
                    SpacewarGame.Players[i].ShipClass = (ShipClass)selectedShip[i];
                    SpacewarGame.Players[i].Skin = selectedSkin[i];
                }

                Shutdown();

                return GameState.PlayEvolved;
            }
            else
            {
                return GameState.None;
            }
        }

        /// <summary>
        /// Tidy up anything that needs tidying
        /// </summary>
        public override void Shutdown()
        {
            //Stop menu music
            Sound.Stop(menuMusic);

            base.Shutdown();
        }

        /// <summary>
        /// Renders the screen
        /// </summary>
        public override void Render()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));

            GraphicsDevice device = graphicsService.GraphicsDevice;
            Texture2D mainTexture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + selectionTexture);

            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque);

            //Sprites will always be at the back.
            device.DepthStencilState = DepthStencilState.DepthRead;

            //Main background
            SpriteBatch.Draw(mainTexture, Vector2.Zero, new Rectangle(0, 0, 1280, 720), Color.White);
            SpriteBatch.End();

            //New sprite to ensure they are drawn on top
            SpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);

            //Ready buttons
            if (player1Ready)
            {
                SpriteBatch.Draw(mainTexture, new Vector2(50, 610), new Rectangle(960, 1095, 190, 80), Color.White);

                //grey out ships & skins
                SpriteBatch.Draw(mainTexture, new Vector2(10, 127), new Rectangle(594, 911, 290, 112), Color.White);
                SpriteBatch.Draw(mainTexture, new Vector2(10, 239), new Rectangle(594, 1050, 290, 180), Color.White);
                SpriteBatch.Draw(mainTexture, new Vector2(10, 419), new Rectangle(10, 1200, 290, 140), Color.White);

                SpriteBatch.Draw(mainTexture, new Vector2(331, 573), new Rectangle(960, 911, 68, 70), Color.White);
                SpriteBatch.Draw(mainTexture, new Vector2(399, 573), new Rectangle(1040, 911, 68, 70), Color.White);
                SpriteBatch.Draw(mainTexture, new Vector2(467, 573), new Rectangle(1120, 911, 68, 70), Color.White);
            }
            else
            {
                //P1 ship & skins
                switch (selectedShip[0])
                {
                    case 0:
                        SpriteBatch.Draw(mainTexture, new Vector2(10, 127), new Rectangle(10, 730, 290, 112), Color.White);
                        break;

                    case 1:
                        SpriteBatch.Draw(mainTexture, new Vector2(10, 239), new Rectangle(10, 860, 290, 180), Color.White);
                        break;

                    case 2:
                        SpriteBatch.Draw(mainTexture, new Vector2(10, 419), new Rectangle(10, 1050, 290, 140), Color.White);
                        break;
                }

                switch (selectedSkin[0])
                {
                    case 0:
                        SpriteBatch.Draw(mainTexture, new Vector2(331, 573), new Rectangle(960, 730, 68, 70), Color.White);
                        break;

                    case 1:
                        SpriteBatch.Draw(mainTexture, new Vector2(399, 573), new Rectangle(1040, 730, 68, 70), Color.White);
                        break;

                    case 2:
                        SpriteBatch.Draw(mainTexture, new Vector2(467, 573), new Rectangle(1120, 730, 68, 70), Color.White);
                        break;
                }
            }

            if (player2Ready)
            {
                SpriteBatch.Draw(mainTexture, new Vector2(1040, 610), new Rectangle(960, 1200, 190, 80), Color.White);

                //grey out ships & skins
                SpriteBatch.Draw(mainTexture, new Vector2(960, 127), new Rectangle(594, 1290, 290, 112), Color.White);
                SpriteBatch.Draw(mainTexture, new Vector2(1040, 239), new Rectangle(332, 1050, 225, 180), Color.White);
                SpriteBatch.Draw(mainTexture, new Vector2(1040, 419), new Rectangle(331, 1290, 225, 140), Color.White);

                SpriteBatch.Draw(mainTexture, new Vector2(745, 573), new Rectangle(960, 1000, 68, 70), Color.White);
                SpriteBatch.Draw(mainTexture, new Vector2(813, 573), new Rectangle(1040, 1000, 68, 70), Color.White);
                SpriteBatch.Draw(mainTexture, new Vector2(881, 573), new Rectangle(1120, 1000, 68, 70), Color.White);
            }
            else
            {
                //p2 ships & skins
                switch (selectedShip[1])
                {
                    case 0:
                        SpriteBatch.Draw(mainTexture, new Vector2(960, 127), new Rectangle(331, 730, 290, 112), Color.White);
                        break;

                    case 1:
                        SpriteBatch.Draw(mainTexture, new Vector2(1040, 239), new Rectangle(331, 860, 225, 180), Color.White);
                        break;

                    case 2:
                        SpriteBatch.Draw(mainTexture, new Vector2(1040, 419), new Rectangle(700, 730, 225, 140), Color.White);
                        break;
                }

                switch (selectedSkin[1])
                {
                    case 0:
                        SpriteBatch.Draw(mainTexture, new Vector2(745, 573), new Rectangle(960, 820, 68, 70), Color.White);
                        break;

                    case 1:
                        SpriteBatch.Draw(mainTexture, new Vector2(813, 573), new Rectangle(1040, 820, 68, 70), Color.White);
                        break;

                    case 2:
                        SpriteBatch.Draw(mainTexture, new Vector2(881, 573), new Rectangle(1120, 820, 68, 70), Color.White);
                        break;
                }
            }

            SpriteBatch.End();

            //Ship names
            Font.Begin();
            Font.Draw(FontStyle.ShipNames, 331, 500, selectedShip[0].ToString(), new Vector4(.2f, .89f, 1, 1));
            Font.Draw(FontStyle.ShipNames, 745, 500, selectedShip[1].ToString(), new Vector4(1, .733f, .392f, 1));
            Font.End();

            base.Render();
        }

        public override void OnCreateDevice()
        {
            base.OnCreateDevice();

            ships[0].ShapeItem.OnCreateDevice();
            ships[1].ShapeItem.OnCreateDevice();
        }
    }
}
