#region File Description
//-----------------------------------------------------------------------------
// Screen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion



namespace ShipGame
{
    public enum ScreenType
    {
        ScreenIntro = 0,
        ScreenHelp,
        ScreenPlayer,
        ScreenLevel,
        ScreenGame,
        ScreenEnd
    };

    public abstract class Screen
    {
        // called when screen gets or looses focus
        public abstract void SetFocus(ContentManager content, bool focus);

        // called to update input
        public abstract void ProcessInput(float elapsedTime, InputManager input);

        // called to update state
        public abstract void Update(float elapsedTime);

        // called to draw the 3D world
        public abstract void Draw3D(GraphicsDevice gd);

        // called to draw the 2D info text and hud
        public abstract void Draw2D(GraphicsDevice gd, FontManager font);
    }
}
