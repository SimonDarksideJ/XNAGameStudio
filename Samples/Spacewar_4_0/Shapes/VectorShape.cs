#region File Description
//-----------------------------------------------------------------------------
// VectorShape.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
#endregion

namespace Spacewar
{
    /// <summary>
    /// A base class for drawing simple vector shapes like the retro mode graphics
    /// </summary>
    public abstract class VectorShape : Shape
    {
        private Effect effect;
        private EffectParameter worldViewProjectionParam;

        public VectorShape(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// Creates the vertex buffers and calls Fill buffer to get the inhertied classes to complete the task
        /// </summary>
        public override void Create()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
            buffer = new VertexBuffer(graphicsService.GraphicsDevice, typeof(VertexPositionColor), 2 * NumberOfVectors, BufferUsage.WriteOnly);

            VertexPositionColor[] data = new VertexPositionColor[2 * NumberOfVectors];
            FillBuffer(data);

            buffer.SetData<VertexPositionColor>(data);

            //Load the correct shader and set up the parameters
            if (effect == null || effect.IsDisposed)
            {
                OnCreateDevice();
            }
        }

        /// <summary>
        /// Override this method to fill in the vertex buffer with your shape data
        /// </summary>
        /// <param name="data">A blob of vertex PositionColored data</param>
        abstract protected void FillBuffer(VertexPositionColor[] data);

        /// <summary>
        /// Override this to indicate how many vectors in your vector shape
        /// </summary>
        abstract protected int NumberOfVectors
        {
            get;
        }

        /// <summary>
        /// Draws the vector shape
        /// </summary>
        public override void Render()
        {
            base.Render();

            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
            GraphicsDevice device = graphicsService.GraphicsDevice;

            device.SetVertexBuffer(buffer);

            worldViewProjectionParam.SetValue(World * SpacewarGame.Camera.View * SpacewarGame.Camera.Projection);
            effect.Techniques[0].Passes[0].Apply();

            device.DrawPrimitives(PrimitiveType.LineList, 0, NumberOfVectors);            
        }

        public override void OnCreateDevice()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));

            effect = SpacewarGame.ContentManager.Load<Effect>(SpacewarGame.Settings.MediaPath + @"shaders\simple");

            worldViewProjectionParam = effect.Parameters["worldViewProjection"];

            buffer = new VertexBuffer(graphicsService.GraphicsDevice, typeof(VertexPositionColor), 2 * NumberOfVectors, BufferUsage.WriteOnly);
            VertexPositionColor[] data = new VertexPositionColor[2 * NumberOfVectors];
            FillBuffer(data);
            buffer.SetData<VertexPositionColor>(data);
        }
    }
}
