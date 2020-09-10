#region File Description
//-----------------------------------------------------------------------------
// RobotGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RobotGameData;
using RobotGameData.Render;
using RobotGameData.Camera;
using RobotGameData.GameObject;
using RobotGameData.Collision;
using RobotGameData.Input;
using RobotGameData.Text;
using RobotGameData.Screen;
#endregion

namespace RobotGame
{
    /// <summary>
    /// This is the main type for your game
    /// It configures the graphics information which is need for the 
    /// initial execution of the game and sets up the startup screen.
    /// Also, it contains the game's global members, such as 
    /// currentGameLevel, currentStage, etc, that are needed from external classes.
    /// </summary>
    public class RobotGameGame : FrameworkCore
    {
        #region Fields

        static GamePlayer singlePlayer = null;
        static GameLevel currentGameLevel = null;
        static BaseStageScreen currentStage = null;
        static VersusGameInfo versusGameInfo = null;

        #endregion

        #region Properties

        /// <summary>
        /// This is only player 1 in the game. never other player.
        /// </summary>
        public static GamePlayer SinglePlayer
        {
            get { return singlePlayer; }
            set { singlePlayer = value; }
        }

        /// <summary>
        /// This is current level in the game.
        /// </summary>
        public static GameLevel CurrentGameLevel
        {
            get { return currentGameLevel; }
            set { currentGameLevel = value; }
        }

        /// <summary>
        /// This is current stage in the game.
        /// </summary>
        public static BaseStageScreen CurrentStage
        {
            get { return currentStage; }
            set { currentStage = value; }
        }

        /// <summary>
        /// Information for versus play
        /// </summary>
        public static VersusGameInfo VersusGameInfo
        {
            get { return versusGameInfo; }
            set { versusGameInfo = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public RobotGameGame() : base()
        {
            //  Set to window title name
            Window.Title = "Robot Game";

            //  Initialize a graphics information
            GraphicsInfo graphicsInfo = new GraphicsInfo();

#if XBOX
            graphicsInfo.screenWidth = (int)ViewerWidth.Width1080;
            graphicsInfo.screenHeight = (int)ViewerHeight.Height1080;
#else
            graphicsInfo.screenWidth = (int)ViewerWidth.Width720;
            graphicsInfo.screenHeight = (int)ViewerHeight.Height720;
#endif
            //  Set to shader info
            graphicsInfo.pixelShaderProfile = ShaderProfile.PS_2_0;
            graphicsInfo.vertexShaderProfile = ShaderProfile.VS_2_0;

            //  Must be call in this constructor
            ChangeGraphics(graphicsInfo);  
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to 
        /// before starting to run.  This is where it can query for any required services
        /// and load any non-graphics related content.  Calling base.Initialize will 
        /// enumerate through any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        { 
            base.Initialize();
            
            //  Initialize sound engine
            GameSound.Initialize();

            //  Create and set a startup screen...
            //  If you want to change startup screen, 
            //  change to new FirstStageScreen, etc.
            ScreenManager.AddScreen(new MainMenuScreen(), true);
        }


        internal static void ExitAccepted(object sender, EventArgs e)
        {
            //  Exit this program
            FrameworkCore.Game.Exit();
        }


        #region Entry Point


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            using (RobotGameGame game = new RobotGameGame())
            {
                game.Run();
            }
        }


        #endregion
    }
}