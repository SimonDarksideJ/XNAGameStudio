//-----------------------------------------------------------------------------
// LevelSelectScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UserInterfaceSample.Controls;


namespace UserInterfaceSample
{
    public class LevelInfo
    {
        public string Name;
        public string Description;
        public string Image;
    }

    // This class demonstrates the PageFlipControl, by letting the player choose from a
    // set of game levels. Each level is shown with an 
    public class LevelSelectScreen : SingleControlScreen
    {
        // Descriptions of the different levels.
        LevelInfo[] LevelInfos = new LevelInfo[] {
            new LevelInfo
            {
                Name="House",
                Description="Find a way out of your house--if you dare!",
                Image="Levels\\House"
            },
            new LevelInfo
            {
                Name="Pasture",
                Description="Locate your magical cow",
                Image="Levels\\Pasture"
            },
            new LevelInfo
            {
                Name="Hills",
                Description="Graze across the hills",
                Image="Levels\\Hills"
            },
            new LevelInfo
            {
                Name="Castle",
                Description="Explore the old ruined castle",
                Image="Levels\\Castle"
            },
            new LevelInfo
            {
                Name="Dungeon",
                Description="Conquer the dreaded Dungeon Critter",
                Image="Levels\\Dungeon"
            },
        };

        public override void LoadContent()
        {
            EnabledGestures = PageFlipTracker.GesturesNeeded;
            ContentManager content = ScreenManager.Game.Content;

            RootControl = new PageFlipControl();

            foreach (LevelInfo info in LevelInfos)
            {
                RootControl.AddChild(new LevelDescriptionPanel(content, info));
            }
        }
    }

    public class LevelDescriptionPanel : PanelControl
    {
        const float MarginLeft = 20;
        const float MarginTop = 20;
        const float DescriptionTop = 440;

        public LevelDescriptionPanel(ContentManager content, LevelInfo info)
        {
            Texture2D backgroundTexture = content.Load<Texture2D>(info.Image);
            ImageControl background = new ImageControl(backgroundTexture, Vector2.Zero);
            AddChild(background);

            SpriteFont titleFont = content.Load<SpriteFont>("Font\\MenuTitle");
            TextControl title = new TextControl(info.Name, titleFont, Color.Black, new Vector2(MarginLeft, MarginTop));
            AddChild(title);

            SpriteFont descriptionFont = content.Load<SpriteFont>("Font\\MenuDetail");
            TextControl description = new TextControl(info.Description, descriptionFont, Color.Black, new Vector2(MarginLeft, DescriptionTop));
            AddChild(description);
        }
    }
}
