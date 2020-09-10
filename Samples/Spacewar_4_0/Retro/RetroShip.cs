#region File Description
//-----------------------------------------------------------------------------
// RetroShip.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace Spacewar
{
    /// <summary>
    /// The shape that is drawn for the retro ship
    /// </summary>
    public class RetroShip : VectorShape
    {
        public RetroShip(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// Load up the data for the retro ship
        /// </summary>
        /// <param name="data">The vertex buffer</param>
        protected override void FillBuffer(VertexPositionColor[] data)
        {
            data[0] = new VertexPositionColor(new Vector3(0f, 1.5f, 0f), Color.White);
            data[1] = new VertexPositionColor(new Vector3(-1f, -1.5f, 0f), Color.White);
            data[2] = data[1];
            data[3] = new VertexPositionColor(new Vector3(0f, -1f, 0f), Color.White);
            data[4] = data[3];
            data[5] = new VertexPositionColor(new Vector3(1f, -1.5f, 0f), Color.White);
            data[6] = data[5];
            data[7] = data[0];
        }

        /// <summary>
        /// Number of vectors in retro ship
        /// </summary>
        protected override int NumberOfVectors
        {
            get
            {
                return 4;
            }
        }

        public override void Render()
        {
            base.Render();
        }
    }
}
