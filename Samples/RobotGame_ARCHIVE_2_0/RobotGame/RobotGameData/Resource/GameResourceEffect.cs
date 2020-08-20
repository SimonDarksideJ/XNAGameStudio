#region File Description
//-----------------------------------------------------------------------------
// GameResourceEffect.cs
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
    /// an resource element structure with Effect class.
    /// When a shader(.fx) file is loaded from the resource manager, it gets stored here.
    /// </summary>
    public class GameResourceEffect : GameResourceBase
    {
        #region Fields

        Effect effect = null;

        #endregion

        #region Properties

        public Effect Effect
        {
            get { return effect; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="assetName">asset name</param>
        /// <param name="resource">effect resource</param>
        public GameResourceEffect(string key, string assetName, Effect resource)
            : base(key, assetName)
        {
            this.effect = resource;

            this.resource = (object)this.effect;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (effect != null)
                {
                    effect.Dispose();
                    effect = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
