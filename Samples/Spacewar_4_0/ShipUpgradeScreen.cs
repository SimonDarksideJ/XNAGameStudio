#region File Description
//-----------------------------------------------------------------------------
// ShipUpgradeScreen.cs
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
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Spacewar
{
    /// <summary>
    /// ShipUpgradeScreen handles the screen that allows either user to select weapon upgrades
    /// </summary>
    public class ShipUpgradeScreen : Screen
    {
        private static string upgradeTexture = @"textures\weapon_select_FINAL";
        private Vector4 white = new Vector4(1f, 1f, 1f, 1f);
        private Vector4 upgradeFontColor = new Vector4(.882f, .596f, .286f, 1f);
        private const int countSpeed = 3;

        private bool player1Ready = false;
        private bool player2Ready = false;

        private bool playingTallySound = false;
        private Cue tallySound;
        private Cue menuMusic;

        private static string[] scoreLookup = new string[] { "000", "001", "011", "111" };

        private int[] playerCashCount = new int[] { 0, 0 };
        private float[] flashPercent = new float[2]; //Amount of flash on a selected weapon
        private TimeSpan[] flashEndTime = new TimeSpan[2]; //How long left to flash
        private TimeSpan flashTime = new TimeSpan(0, 0, 3);

        private SceneItem[] weapons = new SceneItem[2];
        private ProjectileType[] purchasedWeapon = new ProjectileType[] { ProjectileType.Peashooter, ProjectileType.Peashooter };

        private Vector2[,] weaponPositions = new Vector2[,]
        {
            {
                new Vector2(248, 133),
                new Vector2(75, 314),
                new Vector2(444, 314),
                new Vector2(248, 501),
            },
            {
                new Vector2(864, 133),
                new Vector2(686, 314),
                new Vector2(1055, 314),
                new Vector2(864, 501),
            }
        };

        /// <summary>
        /// Creates the ShipUpgradeScreen
        /// </summary>
        public ShipUpgradeScreen(Game game)
            : base(game)
        {
            //Play the menu music
            menuMusic = Sound.Play(Sounds.MenuMusic);
            weapons[0] = new SceneItem(game, new EvolvedShape(game, EvolvedShapes.Weapon, PlayerIndex.One, (int)ProjectileType.Peashooter, LightingType.Menu), new Vector3(-170, -30, 0));
            weapons[0].Scale = new Vector3(.06f, .06f, .06f);
            scene.Add(weapons[0]);

            weapons[1] = new SceneItem(game, new EvolvedShape(game, EvolvedShapes.Weapon, PlayerIndex.Two, (int)ProjectileType.Peashooter, LightingType.Menu), new Vector3(170, -30, 0));
            weapons[1].Scale = new Vector3(.06f, .06f, .06f);
            scene.Add(weapons[1]);
        }

        /// <summary>
        /// ShipUpgrade update will change the gamestate and start the next level when either
        /// 1. The upgrade timer expires
        /// 2. Both users have pressed A for ready
        /// </summary>
        /// <param name="time">Total game time since it was started</param>
        /// <param name="elapsedTime">Elapsed game time since the last call to update</param>
        /// <returns></returns>
        public override GameState Update(TimeSpan time, TimeSpan elapsedTime)
        {
            if ((XInputHelper.GamePads[PlayerIndex.One].APressed) || (!XInputHelper.GamePads[PlayerIndex.One].State.IsConnected && SpacewarGame.CurrentPlatform != PlatformID.Win32NT))
                player1Ready = true;

            if ((XInputHelper.GamePads[PlayerIndex.Two].APressed) || (!XInputHelper.GamePads[PlayerIndex.Two].State.IsConnected && SpacewarGame.CurrentPlatform != PlatformID.Win32NT))
                player2Ready = true;

            if (XInputHelper.GamePads[PlayerIndex.One].BPressed)
                player1Ready = false;

            if (XInputHelper.GamePads[PlayerIndex.Two].BPressed)
                player2Ready = false;

            //DPAD purchases weapons you can afford
            for (int player = 0; player < 2; player++)
            {
                //Can only buy weapons if you are not ready
                if ((!player1Ready && player == 0) || (!player2Ready && player == 1))
                {
                    for (int weapon = 1; weapon < 5; weapon++)
                    {
                        //Associate weapon selection with dpad direction
                        bool dpadPressed = false;
                        switch (weapon)
                        {
                            case 1:
                                dpadPressed = XInputHelper.GamePads[(PlayerIndex)player].UpPressed;
                                break;
                            case 2:
                                dpadPressed = XInputHelper.GamePads[(PlayerIndex)player].LeftPressed;
                                break;
                            case 3:
                                dpadPressed = XInputHelper.GamePads[(PlayerIndex)player].RightPressed;
                                break;
                            case 4:
                                dpadPressed = XInputHelper.GamePads[(PlayerIndex)player].DownPressed;
                                break;
                        }

                        //If a dpad is selected and you have enough money then purchase that weapon
                        if (dpadPressed && SpacewarGame.Players[player].Cash >= SpacewarGame.Settings.Weapons[weapon].Cost)
                        {
                            Sound.PlayCue(Sounds.MenuAdvance);
                            purchasedWeapon[player] = (ProjectileType)weapon;
                            SpacewarGame.Players[player].Cash -= SpacewarGame.Settings.Weapons[weapon].Cost;
                            SpacewarGame.Players[player].ProjectileType = (ProjectileType)weapon;
                            weapons[player].ShapeItem = new EvolvedShape(GameInstance, EvolvedShapes.Weapon, (PlayerIndex)player, weapon, LightingType.Menu);
                            flashEndTime[player] = time + flashTime;
                        }
                    }
                }
            }

            //Make the totals 'count'
            bool AnyPlayersCounting = false;
            for (int player = 0; player < 2; player++)
            {
                //If the count doesn't match the total
                if (playerCashCount[player] != SpacewarGame.Players[player].Cash)
                {
                    AnyPlayersCounting = true;

                    if (!playingTallySound)
                    {
                        tallySound = Sound.Play(Sounds.PointsTally);
                        playingTallySound = true;
                    }

                    //Then move the count towards it
                    int countDirection = Math.Sign(SpacewarGame.Players[player].Cash - playerCashCount[player]);
                    playerCashCount[player] += (int)(elapsedTime.TotalMilliseconds * (double)countSpeed * (double)countDirection);

                    //Ensure we don't go past the actual count
                    if (countDirection == 1)
                    {
                        playerCashCount[player] = Math.Min(playerCashCount[player], SpacewarGame.Players[player].Cash);
                    }
                    else
                    {
                        playerCashCount[player] = Math.Max(playerCashCount[player], SpacewarGame.Players[player].Cash);
                    }
                }
            }

            if (!AnyPlayersCounting && playingTallySound)
            {
                Sound.Stop(tallySound);
                playingTallySound = false;
            }

            //Spin the weapon
            for (int i = 0; i < 2; i++)
            {
                //(i * 2 -1) makes player 2 spin the other way
                weapons[i].Rotation = new Vector3(-.3f, (float)time.TotalSeconds * (i * 2 - 1), 0);
            }

            //Flash the select icon
            for (int player = 0; player < 2; player++)
            {
                if (flashEndTime[player] > time)
                {
                    flashPercent[player] = (float)(Math.Sin((time - flashEndTime[player]).TotalSeconds * 40) * .5 + 1);
                }
                else
                {
                    //Fully on
                    flashPercent[player] = 1.0f;
                }
            }

            //Don't forget to call the base class
            base.Update(time, elapsedTime);

            //Play the next level when both players are ready
            if (player1Ready && player2Ready)
            {
                //Copy over the purchased weapons
                for (int i = 0; i < 2; i++)
                {
                    SpacewarGame.Players[i].ProjectileType = purchasedWeapon[i];
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
        /// Tidy up anything that needs to be tidied
        /// </summary>
        public override void Shutdown()
        {
            //Silence any looped sounds
            if (playingTallySound)
                Sound.Stop(tallySound);

            Sound.Stop(menuMusic);

            base.Shutdown();
        }

        /// <summary>
        /// Renders this The ship upgrade screen. Current money and settings come from the static SpacewarGame object
        /// </summary>
        public override void Render()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));

            GraphicsDevice device = graphicsService.GraphicsDevice;
            Texture2D mainTexture = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + upgradeTexture);

            SpriteBatch.Begin(SpriteSortMode.Texture, BlendState.Opaque);

            //Sprites will always be at the back.
            device.DepthStencilState = DepthStencilState.DepthRead;

            //Main background
            SpriteBatch.Draw(mainTexture, Vector2.Zero, new Rectangle(0, 0, 1280, 720), Color.White);

            SpriteBatch.End();

            //New sprite to ensure they are drawn on top
            SpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);

            for (int player = 0; player < 2; player++)
            {
                for (int weapon = 1; weapon < 5; weapon++)
                {
                    //Grey out weapons when you are ready
                    if ((player1Ready && player == 0) || (player2Ready && player == 1))
                    {
                        SpriteBatch.Draw(mainTexture, weaponPositions[player, weapon - 1], readySprite(player, weapon), Color.White);
                    }
                    //Show purchased weapon
                    else if ((int)purchasedWeapon[player] == weapon)
                    {
                        SpriteBatch.Draw(mainTexture, weaponPositions[player, weapon - 1], selectedSprite(player, weapon), new Color(new Vector4(1f, 1f, 1f, flashPercent[player])));
                    }
                    //Disable weapons you can't afford
                    else if (SpacewarGame.Players[player].Cash < SpacewarGame.Settings.Weapons[weapon].Cost)
                    {
                        SpriteBatch.Draw(mainTexture, weaponPositions[player, weapon - 1], disabledSprite(player, weapon), Color.White);
                    }
                }
            }

            //Ready buttons
            if (player1Ready)
                SpriteBatch.Draw(mainTexture, new Vector2(55, 620), new Rectangle(10, 1205, 190, 70), Color.White);

            if (player2Ready)
                SpriteBatch.Draw(mainTexture, new Vector2(1040, 620), new Rectangle(330, 1205, 190, 70), Color.White);

            SpriteBatch.End();

            Font.Begin(); //Could reuse the sprite above but things may be drawn in the wrong order

            //Current cash and weapon costs
            //Ensure we use US number formatting
            for (int i = 0; i < 621; i += 620)
            {
                if ((!player1Ready && i == 0) || (!player2Ready && i == 620))
                {
                    Font.Draw(FontStyle.WeaponSmall, 296 + i, 255, String.Format("{0:$##,##0}", SpacewarGame.Settings.Weapons[1].Cost), upgradeFontColor);
                    Font.Draw(FontStyle.WeaponSmall, 110 + i, 438, String.Format("{0:$##,##0}", SpacewarGame.Settings.Weapons[2].Cost), upgradeFontColor);
                    Font.Draw(FontStyle.WeaponSmall, 480 + i, 438, String.Format("{0:$##,##0}", SpacewarGame.Settings.Weapons[3].Cost), upgradeFontColor);
                    Font.Draw(FontStyle.WeaponSmall, 296 + i, 621, String.Format("{0:$##,##0}", SpacewarGame.Settings.Weapons[4].Cost), upgradeFontColor);
                }
            }

            Font.Draw(FontStyle.WeaponLarge, 322, 40, "$", upgradeFontColor);
            Font.Draw(FontStyle.WeaponLarge, 346, 40, String.Format("{0:##,##0}", playerCashCount[0]), upgradeFontColor);
            Font.Draw(FontStyle.WeaponLarge, 840, 40, "$", upgradeFontColor);
            Font.Draw(FontStyle.WeaponLarge, 866, 40, String.Format("{0:##,##0}", playerCashCount[1]), upgradeFontColor);

            //Score buttons
            Font.Draw(FontStyle.ScoreButtons, 60 + 349, 70 + 24, scoreLookup[SpacewarGame.Players[0].Score]);
            Font.Draw(FontStyle.ScoreButtons, 1140 - 351, 70 + 24, scoreLookup[SpacewarGame.Players[1].Score]);

            Font.End();

            base.Render();
        }

        private Rectangle selectedSprite(int player, int weapon)
        {
            return new Rectangle(10 + 320 * (weapon - 1), 730 + player * 160, 150, 150);
        }

        private Rectangle disabledSprite(int player, int weapon)
        {
            return new Rectangle(170 + 320 * (weapon - 1), 730 + player * 160, 150, 150);
        }

        private Rectangle readySprite(int player, int weapon)
        {
            return new Rectangle(10 + 160 * (weapon - 1) + 640 * player, 1050, 150, 150);
        }

        public override void OnCreateDevice()
        {
            base.OnCreateDevice();

            weapons[0].ShapeItem.OnCreateDevice();
            weapons[1].ShapeItem.OnCreateDevice();
        }
    }
}
