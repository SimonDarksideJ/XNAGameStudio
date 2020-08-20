#region File Description
//-----------------------------------------------------------------------------
// RetroStarfield.cs
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

namespace Spacewar
{
    /// <summary>
    /// The stars for the background of retro mode
    /// </summary>
    public class RetroStarfield : Shape
    {        
        private const int numberOfTriangles = 800;
        private const int numberOfPoints = numberOfTriangles * 3; // Each point is actually a triangle
        private const int percentBigStars = 20;
        private Effect effect;
        private EffectParameter worldViewProjectionParam;

        public RetroStarfield(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// Creates vertex buffer full of points
        /// </summary>
        public override void Create()
        {
            OnCreateDevice();
        }

        /// <summary>
        /// Renders the starfield
        /// </summary>
        public override void Render()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));

            GraphicsDevice device = graphicsService.GraphicsDevice;            
            device.SetVertexBuffer(buffer);

            worldViewProjectionParam.SetValue(World * SpacewarGame.Camera.View * SpacewarGame.Camera.Projection);
            effect.Techniques[0].Passes[0].Apply();

            device.DrawPrimitives(PrimitiveType.TriangleList, 0, numberOfTriangles);
        }

        public override void OnCreateDevice()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));

            effect = SpacewarGame.ContentManager.Load<Effect>(SpacewarGame.Settings.MediaPath + @"shaders\simple");

            worldViewProjectionParam = effect.Parameters["worldViewProjection"];            

            buffer = new VertexBuffer(graphicsService.GraphicsDevice, typeof(VertexPositionColor), numberOfPoints, BufferUsage.WriteOnly);

            VertexPositionColor[] data = new VertexPositionColor[numberOfPoints];            

            int pointCount = 0;
            int triangleCount = 0;
            Random random = new Random();
            while (triangleCount < numberOfTriangles)
            {
                byte greyValue = (byte)(random.Next(200) + 56); // 56-255
                Color color = new Color(greyValue, greyValue, greyValue);                
                Vector2 position = new Vector2(random.Next(560) - 280, random.Next(420) - 210);

                //Add a big star if the time is right and there is room in the buffer
                if (random.Next(100) < percentBigStars && (triangleCount + 2) < numberOfTriangles)
                {
                    //Big stars are just 4 points
                    data[pointCount++] = new VertexPositionColor(new Vector3(position.X, position.Y, 0), color);
                    data[pointCount++] = new VertexPositionColor(new Vector3(position.X - 1f, position.Y, 0), color);
                    data[pointCount++] = new VertexPositionColor(new Vector3(position.X, position.Y + 1f, 0), color);

                    data[pointCount++] = new VertexPositionColor(new Vector3(position.X - 1f, position.Y, 0), color);
                    data[pointCount++] = new VertexPositionColor(new Vector3(position.X - 1f, position.Y + 1f, 0), color);
                    data[pointCount++] = new VertexPositionColor(new Vector3(position.X, position.Y + 1f, 0), color);

                    triangleCount += 2;
                }
                else
                {
                    data[pointCount++] = new VertexPositionColor(new Vector3(position.X, position.Y, 0), color);
                    data[pointCount++] = new VertexPositionColor(new Vector3(position.X - 0.5f, position.Y, 0), color);
                    data[pointCount++] = new VertexPositionColor(new Vector3(position.X, position.Y + 0.5f, 0), color);                    

                    triangleCount++;
                }
            }

            buffer.SetData<VertexPositionColor>(data);
        }
    }
}
