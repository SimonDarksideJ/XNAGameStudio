#region File Description
//-----------------------------------------------------------------------------
// GameResourceFont.cs
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
#endregion

namespace RobotGameData.Resource
{
    /// <summary>
    /// a resource element structure with SpriteFont class.
    /// When a font(.spritefont) file is loaded from the resource manager, 
    /// it gets stored here.
    /// </summary>
    public class GameResourceFont : GameResourceBase
    {
        #region Fields

        SpriteFont spriteFont = null;

        #endregion

        #region Properties

        public SpriteFont SpriteFont
        {
            get { return spriteFont; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="assetName">asset name</param>
        /// <param name="resource">sprite font resource</param>
        public GameResourceFont(string key, string assetName, SpriteFont resource)
                                : base(key, assetName)
        {
            this.spriteFont = resource;

            this.resource = (object)this.spriteFont;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (spriteFont != null)
                {
                    spriteFont = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
