#region File Description
//-----------------------------------------------------------------------------
// TextManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RobotGameData.Text
{
    /// <summary>
    /// It displays text on screen.  Each text is stored as and managed 
    /// as TextItem class.  The texts, displayed via TextManager, do not 
    /// get affected by the game screen (i.e., screen fade or sprite) but 
    /// gets displayed independently on an overlay screen.
    /// In order to display the texts, which do get affected by the game screen, 
    /// (i.e. Hud information) GameText class, which inherited GameSceneNode, 
    /// has to be created and registered to scene2DLayer node as a child.
    /// </summary>
    public class TextManager : DrawableGameComponent
    {
        #region Fields

        protected SpriteBatch textBatch = null;
        protected List<TextItem> textList = new List<TextItem>();

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public TextManager(Game game)
            : base(game) {}

        /// <summary>
        /// Allows the game component to perform any initialization it needs 
        /// to before starting to run.  This is where it can query for any 
        /// required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            textBatch = new SpriteBatch(FrameworkCore.Game.GraphicsDevice);

            base.Initialize();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }
        
        /// <summary>
        /// displays the message of every text time that has been registered to 
        /// the list on 2D screen.
        /// </summary>
        public override void  Draw(GameTime gameTime)
        {
            textBatch.Begin(SpriteBlendMode.AlphaBlend);

            //  Draw each text on screen
            foreach( TextItem item in textList)
            {
                if (item.Visible )
                {
                    textBatch.DrawString(item.Font,
                                            item.Text,
                                            item.Position,
                                            item.Color,
                                            item.Rotation,
                                            Vector2.Zero,
                                            item.Scale,
                                            SpriteEffects.None,
                                            1.0f);
                }
            }

            textBatch.End();

            base.Draw(gameTime);            
        }

        protected override void Dispose(bool disposing)
        {
            textBatch.Dispose();
            textBatch = null;

            ClearTextAll();

            base.Dispose(disposing);
        }
      
        /// <summary>
        /// adds a text item.
        /// </summary>
        /// <param name="item">a text item</param>
        public void AddText(TextItem item)
        {
            textList.Add(item);
        }

        /// <summary>
        /// adds a text.
        /// </summary>
        /// <param name="font">sprite font</param>
        /// <param name="text">message</param>
        /// <param name="x">screen x-position</param>
        /// <param name="y">screen y-position</param>
        /// <param name="color">text color</param>
        /// <returns>text item</returns>
        public TextItem AddText(SpriteFont font, string text, int x, int y, Color color)
        {
            TextItem item = new TextItem(font, text, x, y, color);

            AddText(item);

            return item;
        }

        /// <summary>
        /// removes the text item.
        /// </summary>
        /// <param name="item">text item</param>
        public bool RemoveText(TextItem item)
        {
            return textList.Remove(item);
        }

        public void ClearTextAll()
        {
            textList.Clear();
        }

        /// <summary>
        /// gets a text item by id.
        /// </summary>
        /// <param name="id">text item id</param>
        /// <returns>text item</returns>
        public TextItem GetText(int id)
        {
            foreach (TextItem item in textList)
            {
                if (item.Id == id)
                    return item;
            }

            return null;
        }
    }
}


