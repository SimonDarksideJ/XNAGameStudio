#region File Description
//-----------------------------------------------------------------------------
// SingleStageScreen.cs
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
using RobotGameData;
using RobotGameData.Screen;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It takes care of the common processes that are needed 
    /// for all the single play stages.
    /// It checks the current stage’s mission progress.
    /// </summary>
    public class SingleStageScreen : BaseStageScreen
    {        
        #region Mission

        /// <summary>
        /// checks the mission objective during the game play.
        /// Moves to the next stage after the mission gets cleared, 
        /// and when the mission is failed, returns to the main menu.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void CheckMission(GameTime gameTime)
        {
            //  Checking mission
            if (IsMissionFailed == false)
            {
                IsMissionFailed = GameLevel.IsMissionFailed();
            }

            if (IsMissionClear == false)
            {
                IsMissionClear = gameLevel.IsMissionClear();
            }

            //  Mission complete!!
            if (IsMissionClear )
            {
                if (GameSound.IsPlaying(soundBGM))
                {
                    SetVisibleHud(false);

                    //  The volume for the sound effects besides the 
                    //  background music will be lowered.
                    FrameworkCore.SoundManager.SetVolume(
                                        SoundCategory.Default.ToString(), 0.4f);

                    FrameworkCore.SoundManager.SetVolume(
                                        SoundCategory.Effect.ToString(), 0.4f);

                    FrameworkCore.SoundManager.SetVolume(
                                        SoundCategory.Music.ToString(), 1.0f);

                    //  Stop background music
                    GameSound.Stop(soundBGM);

                    //  Play victory music
                    GameSound.Play(SoundTrack.MissionClear);

                    //  Player stop
                    for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
                    {
                        GamePlayer player = GameLevel.GetPlayerInLevel(i);

                        player.MissionEnd();
                    }

                    if (NextScreen == null)
                        throw new InvalidOperationException("Please set to next screen");

                    //  Go to next stage
                    ExitScreen();
                }

                //  display the clear image
                if (missionResultElapsedTime > MissionResultVisibleTime)
                {
                    this.spriteObjMissionClear.Visible = true;
                }
                else
                {
                    if (missionResultElapsedTime < MissionResultVisibleTime)
                    {
                        missionResultElapsedTime +=
                                    (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }

                    this.spriteObjMissionClear.Visible = false;
                }
            }
            //  Mission failed
            else if (IsMissionFailed )
            {
                if (GameSound.IsPlaying(soundBGM))
                {
                    SetVisibleHud(false);

                    //  The volume for the sound effects besides the 
                    //  background music will be lowered.
                    FrameworkCore.SoundManager.SetVolume(
                                        SoundCategory.Default.ToString(), 0.4f);

                    FrameworkCore.SoundManager.SetVolume(
                                        SoundCategory.Effect.ToString(), 0.4f);

                    FrameworkCore.SoundManager.SetVolume(
                                        SoundCategory.Music.ToString(), 1.0f);

                    //  Stop background music
                    GameSound.Stop(soundBGM);

                    //  Play fail music
                    GameSound.Play(SoundTrack.MissionFail);

                    //  Player sound and action stop
                    for (int i = 0; i < GameLevel.PlayerCountInLevel; i++)
                    {
                        GamePlayer player = GameLevel.GetPlayerInLevel(i);

                        player.MissionEnd();
                    }

                    //  Go to main menu
                    NextScreen = new MainMenuScreen();
                    TransitionOffTime = TimeSpan.FromSeconds(8.0f);
                    ExitScreen();
                }

                //  display the failed image
                if (missionResultElapsedTime > MissionResultVisibleTime)
                {
                    this.spriteObjMissionFailed.Visible = true;
                }
                else
                {
                    if (missionResultElapsedTime < MissionResultVisibleTime)
                    {
                        missionResultElapsedTime +=
                                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }

                    this.spriteObjMissionFailed.Visible = false;
                }
            }
        }

        #endregion
    }
}
