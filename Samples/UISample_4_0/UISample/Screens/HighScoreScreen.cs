//-----------------------------------------------------------------------------
// HighScoreScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using UserInterfaceSample.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;


namespace UserInterfaceSample
{
    /// <summary>
    /// LeaderboardScreen is a GameScreen that creates a single PageFlipControl containing
    /// a collection of LeaderboardPanel controls, which display a game's leaderboards.
    /// 
    /// You will need to customize the LoadContent() method of this class to create the
    /// appropriate list of leaderboards to match your game configuration.
    /// </summary>
    public class HighScoreScreen : SingleControlScreen
    {
        public override void LoadContent()
        {
            EnabledGestures = ScrollTracker.GesturesNeeded;
            ContentManager content = ScreenManager.Game.Content;

            RootControl = new HighScorePanel(content);
            base.LoadContent();
        }
    }
}
