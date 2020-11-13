#region File Description
//-----------------------------------------------------------------------------
// LoadingScreen.cs
//
// Display a screen to inform the player that their game is still responding,
// while loading content in the background.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using RacingGame.Graphics;
#endregion

namespace RacingGame.GameScreens
{
    /// <summary>
    /// Loading screen
    /// </summary>
    class LoadingScreen : IGameScreen
	{
		#region Variables
		private const string loadingText = "Loading...";
		private int loadingTextWidth = TextureFont.GetTextWidth(loadingText);
		private string loadingStatus = "";
		#endregion

		#region Constructor
		public LoadingScreen()
		{
			//Setup the handler before we start the thread
			RacingGameManager.LoadEvent += new EventHandler<EventArgs>(LoadEvent);
		}
		#endregion

		#region Update LoadingScreen
		/// <summary>
		/// Gather input on the loading screen and update it if progress has
		/// changed in loading the game.
		/// </summary>
		public void Update(GameTime gameTime)
		{
			if (RacingGameManager.LoadingThread.ThreadState == ThreadState.Unstarted)
				RacingGameManager.LoadingThread.Start();
		}

		public void LoadEvent(object sender, EventArgs e)
		{
			loadingStatus = (string)sender;
		}
		#endregion

		#region RenderLoadingScreen
		/// <summary>
        /// Render loading screen
        /// </summary>
        public bool Render()
        {
			SpriteBatch textBatch = new SpriteBatch(BaseGame.Device);
			Vector2 position = new Vector2((BaseGame.Width / 2) - 50, (BaseGame.Height / 2) - 20);

			for (int i = 0; i < loadingText.Length; i++)
			{
				string charStr = new string(loadingText[i], 1);
				int charHeight = (int)(position.Y + 7 * Math.Abs(Math.Sin((i / 4f) + (-BaseGame.TotalTime * 3))));
				TextureFont.WriteText((int)position.X, charHeight, charStr, Color.Red);

				position.X += TextureFont.GetTextWidth(charStr);
			}

			TextureFont.WriteTextCentered(BaseGame.Width / 2, (int)position.Y + 40, loadingStatus);

			return RacingGameManager.ContentLoaded;
        }
        #endregion
	}
}
