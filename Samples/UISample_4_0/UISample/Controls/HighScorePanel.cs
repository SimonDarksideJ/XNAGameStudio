//-----------------------------------------------------------------------------
// HighScorePanel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace UserInterfaceSample.Controls
{
    /// <remarks>
    /// This class displays a list of high scores, to give an example of presenting
    /// a list of data that the player can scroll through.
    /// </remarks>
    public class HighScorePanel : ScrollingPanelControl
    {
        Control resultListControl = null;

        SpriteFont titleFont;
        SpriteFont headerFont;
        SpriteFont detailFont;

        public HighScorePanel(ContentManager content)
        {
            titleFont = content.Load<SpriteFont>("Font\\MenuTitle");
            headerFont = content.Load<SpriteFont>("Font\\MenuHeader");
            detailFont = content.Load<SpriteFont>("Font\\MenuDetail");

            AddChild(new TextControl("High score", titleFont));
            AddChild(CreateHeaderControl());
            PopulateWithFakeData();
        }


        private void PopulateWithFakeData()
        {
            PanelControl newList = new PanelControl();
            Random rng = new Random();
            for (int i = 0; i < 50; i++)
            {
                long score = 10000 - i * 10;
                TimeSpan time = TimeSpan.FromSeconds(rng.Next(60, 3600));
                newList.AddChild(CreateLeaderboardEntryControl("player" + i.ToString(), score, time));
            }
            newList.LayoutColumn(0, 0, 0);

            if (resultListControl != null)
            {
                RemoveChild(resultListControl);
            }
            resultListControl = newList;
            AddChild(resultListControl);
            LayoutColumn(0, 0, 0);
        }

        protected Control CreateHeaderControl()
        {
            PanelControl panel = new PanelControl();

            panel.AddChild(new TextControl("Player", headerFont, Color.Turquoise, new Vector2(0, 0)));
            panel.AddChild(new TextControl("Score", headerFont, Color.Turquoise, new Vector2(200, 0)));

            return panel;
        }

        // Create a Control to display one entry in a leaderboard. The content is broken out into a parameter
        // list so that we can easily create a control with fake data when running under the emulator.
        //
        // Note that for time leaderboards, this function interprets the time as a count in seconds. The
        // value posted is simply a long, so your leaderboard might actually measure time in ticks, milliseconds,
        // or microfortnights. If that is the case, adjust this function to display appropriately.
        protected Control CreateLeaderboardEntryControl(string player, long rating, TimeSpan time)
        {
            Color textColor = Color.White;

            var panel = new PanelControl();

            // Player name
            panel.AddChild(
                new TextControl
                {
                    Text = player,
                    Font = detailFont,
                    Color = textColor,
                    Position = new Vector2(0, 0)
                });

            // Score
            panel.AddChild(
                new TextControl
                {
                    Text = String.Format("{0}", rating),
                    Font = detailFont,
                    Color = textColor,
                    Position = new Vector2(200, 0)
                });

            // Time
            panel.AddChild(
                new TextControl
                    {
                        Text = String.Format("Completed in {0:g}", time),
                        Font = detailFont,
                        Color = textColor,
                        Position = new Vector2(400, 0)
                    });

            return panel;
        }
    }
}
