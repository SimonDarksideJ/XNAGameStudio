#region File Description
//-----------------------------------------------------------------------------
// GameResourceTexture2D.cs
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
    /// A resource element structure with Texture2D class.
    /// When an image file is loaded from the resource manager, 
    /// it gets stored here.
    /// </summary>
    public class GameResourceTexture2D : GameResourceBase
    {
        #region Fields

        Texture2D texture2D = null;

        #endregion

        #region Properties

        public Texture2D Texture2D
        {
            get { return texture2D; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="assetName">asset name</param>
        /// <param name="resource">texture resource</param>
        public GameResourceTexture2D(string key, string assetName, Texture2D resource)
                                    : base(key, assetName)
        {
            this.texture2D = resource;

            this.resource = (object)this.texture2D;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (texture2D != null)
                {
                    texture2D.Dispose();
                    texture2D = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
