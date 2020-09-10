#region File Description
//-----------------------------------------------------------------------------
//NetworkPlayer.cs
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


#endregion


namespace Yacht
{
    /// <summary>
    /// A remote player for the Yacht game.
    /// </summary>
    class NetworkPlayer : YachtPlayer
    {
        #region Fields


        const string text = "Waiting for network player";
        Vector2 position; 


        #endregion

        #region Initialization
        

        /// <summary>
        /// Initialize a new network player.
        /// </summary>
        /// <param name="name">The name of the player.</param>
        /// <param name="screenBounds">The screen's bounds.</param>
        public NetworkPlayer(string name, Rectangle screenBounds)
            : base(name, null)
        {
            Vector2 measure = YachtGame.Font.MeasureString(text);
            position = new Vector2(screenBounds.Center.X - measure.X / 2, screenBounds.Bottom - 70);
        }


        #endregion

        #region Render


        /// <summary>
        /// Draw a prompt stating a wait for the remote player's move.
        /// </summary>
        /// <param name="spriteBatch">The <see cref="SpriteBatch"/> to use when drawing.</param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(YachtGame.Font, text, position, Color.White);

            base.Draw(spriteBatch);
        } 


        #endregion
    }
}
