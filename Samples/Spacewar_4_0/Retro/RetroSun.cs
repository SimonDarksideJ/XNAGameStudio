#region File Description
//-----------------------------------------------------------------------------
// RetroSun.cs
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

namespace Spacewar
{
    /// <summary>
    /// The shape drawn for the sun in retro mode
    /// </summary>
    public class RetroSun : VectorShape
    {
        public RetroSun(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// Loads up the data for a sun shape
        /// </summary>
        /// <param name="data"></param>
        protected override void FillBuffer(VertexPositionColor[] data)
        {
            float r2 = (float)Math.Sqrt(2);
            data[0] = new VertexPositionColor(new Vector3(0, 2, 0), Color.White);
            data[1] = new VertexPositionColor(new Vector3(0, -2, 0), Color.White);
            data[2] = new VertexPositionColor(new Vector3(2, 0, 0), Color.White);
            data[3] = new VertexPositionColor(new Vector3(-2, 0, 0), Color.White);
            data[4] = new VertexPositionColor(new Vector3(-r2, r2, 0f), Color.Gray);
            data[5] = new VertexPositionColor(new Vector3(r2, -r2, 0f), Color.Gray);
            data[6] = new VertexPositionColor(new Vector3(r2, r2, 0f), Color.Gray);
            data[7] = new VertexPositionColor(new Vector3(-r2, -r2, 0f), Color.Gray);
        }

        /// <summary>
        /// Sun consists of 4 lines
        /// </summary>
        protected override int NumberOfVectors
        {
            get
            {
                return 4;
            }
        }
    }
}
