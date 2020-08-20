#region File Description
//-----------------------------------------------------------------------------
// TitleMenu.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace XnaGraphicsDemo
{
    /// <summary>
    /// The main menu screen allows users to choose between the various demo screens.
    /// </summary>
    class TitleMenu : MenuComponent
    {
        // Constants.
        const float XnaSpawnRate = 1.5f;
        const float XnaLifespan = 7;


        // Fields.
        int attractCycle;
        float time;

        Random random = new Random();


        // We display a set of floating "xna" text labels in the background of the menu.
        class FloatingXna
        {
            public Vector2 Position;
            public float Age;
            public float Size;
        }

        List<FloatingXna> floatingXnas = new List<FloatingXna>();


        /// <summary>
        /// Constructor.
        /// </summary>
        public TitleMenu(DemoGame game)
            : base(game)
        {
            Entries.Add(new MenuEntry { Text = "basic effect",           Clicked = delegate { Game.SetActiveMenu(1); } });
            Entries.Add(new MenuEntry { Text = "dual texture effect",    Clicked = delegate { Game.SetActiveMenu(2); } });
            Entries.Add(new MenuEntry { Text = "alpha test effect",      Clicked = delegate { Game.SetActiveMenu(3); } });
            Entries.Add(new MenuEntry { Text = "skinned effect",         Clicked = delegate { Game.SetActiveMenu(4); } });
            Entries.Add(new MenuEntry { Text = "environment map effect", Clicked = delegate { Game.SetActiveMenu(5); } });
            Entries.Add(new MenuEntry { Text = "particles",              Clicked = delegate { Game.SetActiveMenu(6); } });
            Entries.Add(new MenuEntry { Text = "quit",                   Clicked = delegate { game.Exit(); } });
        }


        /// <summary>
        /// Resets the menu state.
        /// </summary>
        public override void Reset()
        {
            floatingXnas.Clear();
            time = 0;

            base.Reset();
        }


        /// <summary>
        /// The main menu wants a shorter attract delay than the other screens.
        /// </summary>
        override protected TimeSpan AttractDelay { get { return TimeSpan.FromSeconds(3); } }


        /// <summary>
        /// When the attract mode timeout is reached, we cycle through each other screen in turn.
        /// </summary>
        override protected void OnAttract()
        {
            Entries[attractCycle].OnClicked();

            if (++attractCycle >= Entries.Count - 1)
                attractCycle = 0;
        }


        /// <summary>
        /// Updates the floating "xna" background labels.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Spawn a new label?
            if (time > XnaSpawnRate)
            {
                FloatingXna xna = new FloatingXna();

                xna.Size = (float)random.NextDouble() * 2 + 0.5f;

                xna.Position.X = (float)random.NextDouble() * 320 + 80;
                xna.Position.Y = (float)random.NextDouble() * 700 + 50;

                floatingXnas.Add(xna);

                time -= XnaSpawnRate;
            }

            // Animate the existing labels.
            int i = 0;

            while (i < floatingXnas.Count)
            {
                FloatingXna xna = floatingXnas[i];

                xna.Age += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Different size labels move at different speeds.
                float speed = 1.5f - xna.Size;

                if (Math.Abs(speed) > 0.01f)
                    xna.Position.Y -= xna.Age * xna.Age / speed / 10;

                // Remove old labels.
                if (xna.Age >= XnaLifespan)
                    floatingXnas.RemoveAt(i);
                else
                    i++;
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Draws the main menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            DrawTitle("xna demo", Color.CornflowerBlue, Color.Lerp(Color.Blue, Color.CornflowerBlue, 0.85f));

            // Draw the background "xna" labels.
            SpriteBatch.Begin();

            foreach (FloatingXna blob in floatingXnas)
            {
                float alpha = Math.Min(blob.Age, 1) * Math.Min((XnaLifespan - blob.Age) / (XnaLifespan - 2), 1);

                alpha *= alpha;
                alpha /= 8;

                SpriteBatch.DrawString(BigFont, "xna", blob.Position, Color.Blue * alpha, MathHelper.PiOver2, Vector2.Zero, blob.Size, 0, 0);
            }

            SpriteBatch.End();

            // This will draw the various menu items.
            base.Draw(gameTime);
        }
    }
}
