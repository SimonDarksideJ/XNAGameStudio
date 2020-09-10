#region File Description
//-----------------------------------------------------------------------------
// WaypointList.cs
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
using Microsoft.Xna.Framework.Content;
#endregion

namespace Pathfinding
{
    /// <summary>
    /// WaypointList is a drawable List of screen locations
    /// </summary>
    public class WaypointList : Queue<Vector2>
    {
        #region Constants
        /// <summary>
        /// Scales the draw size of the search nodes
        /// </summary>
        const float waypointNodeDrawScale = .75f;
        #endregion

        #region Fields

        private float scale = 1f;
        public float Scale
        {
            get { return scale; }
            set { scale = value * waypointNodeDrawScale; }
        }

        // Draw data
        private Texture2D waypointTexture;
        private Vector2 waypointCenter;
        #endregion

        #region Initialization

        /// <summary>
        /// Load the WaypointLists' texture resources
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            waypointTexture = content.Load<Texture2D>("dot");
            waypointCenter =
                new Vector2(waypointTexture.Width / 2, waypointTexture.Height / 2);
        }

        #endregion

        #region Draw
        /// <summary>
        /// Draw the waypoint list, fading from red for the first to 
        /// blue for the last
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            int numberPoints = this.Count - 1;
            // This catches a special case where we have only one waypoint in the
            // list, in this case the waypoint won’t draw correctly because we divide
            // 0 by 0 later on and get NaN for our result, fortunately for us this 
            // doesn’t cause the code to crash, but we end up getting a bad color 
            // later on, so we catch this special case early and fix it
            if (numberPoints == 0)
            {
                numberPoints = 1;
            }

            float lerpAmt;
            float i = 0f;
            Color drawColor;

            spriteBatch.Begin();
            foreach (Vector2 location in this)
            {
                // This creates a gradient between 0 for the first waypoint on the 
                // list and 1 for the last, 0 creates a color that's completely red 
                // and 1 creates a color that's completely blue
                lerpAmt = i / numberPoints;
                drawColor = new Color(Vector4.Lerp(
                    Color.Red.ToVector4(), Color.Blue.ToVector4(), lerpAmt));

                spriteBatch.Draw(waypointTexture, location, null, drawColor, 0f,
                    waypointCenter, scale, SpriteEffects.None, 0f);

                i += 1f;
            }
            spriteBatch.End();
        }
        #endregion
    }
}