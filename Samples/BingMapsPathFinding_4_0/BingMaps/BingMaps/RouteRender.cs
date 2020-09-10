#region File Description
//-----------------------------------------------------------------------------
// RouteRender.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

#endregion

namespace BingMaps
{
    /// <summary>
    /// Draws a route using a collection of lines.
    /// </summary>
    class RouteRender
    {
        #region Fields

        GraphicsDevice graphicsDevice;
        BasicEffect effect;
        List<VertexPositionColor> vertexPositions = new List<VertexPositionColor>();
        List<short> indices = new List<short>();

        #endregion

        #region Initialization

        /// <summary>
        /// Create new instance of RouteRender class.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public RouteRender(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

            effect = new BasicEffect(graphicsDevice);
            effect.VertexColorEnabled = true;
            effect.Alpha = 1f;
            effect.View = Matrix.Identity;
            effect.World = Matrix.Identity;
            effect.Projection = Matrix.CreateOrthographicOffCenter(0f, graphicsDevice.Viewport.Width,
                                    graphicsDevice.Viewport.Height, 0f, 0f, -1.0F);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new line to draw.
        /// </summary>
        /// <param name="startPoint">The start point of the line.</param>
        /// <param name="endPoint">The end point of the line.</param>
        /// <param name="color">The color of the line.</param>
        public void AddRouteLine(Vector2 startPoint, Vector2 endPoint, Color color)
        {
            VertexPositionColor startVertexPosition = new VertexPositionColor(new Vector3(startPoint, 1f), color);
            vertexPositions.Add(startVertexPosition);
            indices.Add((short)indices.Count);

            VertexPositionColor endVertexPosition = new VertexPositionColor(new Vector3(endPoint, 1f), color);
            vertexPositions.Add(endVertexPosition);
            indices.Add((short)indices.Count);
        }

        /// <summary>
        /// Draws the route and removes all lines contained in the instance.
        /// </summary>
        public void EndRoute()
        {
            if(vertexPositions.Count > 0)
            {
                foreach( EffectPass effectPass in effect.CurrentTechnique.Passes )
                {
                    effectPass.Apply();
                    graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList, 
                        vertexPositions.ToArray(), 0, vertexPositions.Count, 
                        indices.ToArray(), 0, vertexPositions.Count / 2 );
                }
            }

            vertexPositions.Clear();
            indices.Clear();
        }

        #endregion
    }
}

