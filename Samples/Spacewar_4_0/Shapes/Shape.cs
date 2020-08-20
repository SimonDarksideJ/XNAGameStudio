#region File Description
//-----------------------------------------------------------------------------
// Shape.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Spacewar
{
    /// <summary>
    /// Shape is the base class of any object that is renderable
    /// </summary>
    public abstract class Shape : IDisposable
    {
        /// <summary>
        /// The vertex buffer used by this shape
        /// </summary>
        protected VertexBuffer buffer;
        protected VertexDeclaration vertexDecl;

        /// <summary>
        /// The current world matrix used to render this shape
        /// </summary>
        protected Matrix world;
        protected Vector3 position;

        private Game game = null;

        #region Properties
        public Matrix World
        {
            get
            {
                return world;
            }
            set
            {
                world = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        protected Game GameInstance
        {
            get
            {
                return game;
            }
        }
        #endregion

        /// <summary>
        /// Creates a new shape. Calls the virtual Create method to generate any vertex buffers etc
        /// </summary>
        public Shape(Game game)
        {
            this.game = game;
            Create();
        }

        /// <summary>
        /// Creates the vertex buffers etc. This routine is called on object creation and on device reset etc
        /// </summary>
        abstract public void Create();

        /// <summary>
        /// Renders the shape. Base class does nothing
        /// </summary>
        public virtual void Render()
        {
        }

        /// <summary>
        /// Updates the shape. Base class does nothing
        /// </summary>
        /// <param name="time">Game Time</param>
        /// <param name="elapsedTime">Elapsed game time since last call</param>
        public virtual void Update(TimeSpan time, TimeSpan elapsedTime)
        {
            //Nothing for now
        }

        /// <summary>
        /// Creates a rectangle with a certain number of rows and columns. The buffer is PositionOnly and always 0.0-1.0
        /// </summary>
        /// <param name="Columns">Number of columns</param>
        /// <param name="Rows">Number of Rows</param>
        /// <returns>Populated Vertex buffer</returns>
        public VertexBuffer Plane(int Columns, int Rows)
        {
            //TODO: Would be better to have a PositionOnly vertex type here but that's not in this build
            //Its used for the backdrop of evolved which is a full screen quad with texture coordinates derived from
            //other shader variables
            //TODO: Fix up shader for evolved backdrop to ignore the color in the vertex format
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));

            VertexBuffer Buffer = new VertexBuffer(graphicsService.GraphicsDevice, typeof(VertexPositionColor), Columns * Rows * 6, BufferUsage.WriteOnly);
            //VertexPositionColor data = Buffer.Lock<PositionOnly>(0, 0, LockFlags.None);
            VertexPositionColor[] data = new VertexPositionColor[Columns * Rows * 6];
            //Buffer coordinates are 0.0-1.0 so we can use them as the base texture coordinates too
            int pointCount = 0;
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    data[pointCount + 0] = new VertexPositionColor(new Vector3((float)x / (float)Columns, (float)y / (float)Rows, 0), Color.White);
                    data[pointCount + 1] = new VertexPositionColor(new Vector3((float)(x + 1) / (float)Columns, (float)y / (float)Rows, 0), Color.White);
                    data[pointCount + 2] = new VertexPositionColor(new Vector3((float)(x + 1) / (float)Columns, (float)(y + 1) / (float)Rows, 0), Color.White);
                    data[pointCount + 3] = new VertexPositionColor(new Vector3((float)x / (float)Columns, (float)y / (float)Rows, 0), Color.White); //same as 0
                    data[pointCount + 4] = new VertexPositionColor(new Vector3((float)(x + 1) / (float)Columns, (float)(y + 1) / (float)Rows, 0), Color.White); //same as 2
                    data[pointCount + 5] = new VertexPositionColor(new Vector3((float)x / (float)Columns, (float)(y + 1) / (float)Rows, 0), Color.White);

                    pointCount += 6;
                }
            }
            Buffer.SetData<VertexPositionColor>(data);

            return Buffer;
        }

        /// <summary>
        /// Called when a device is created
        /// </summary>
        public virtual void OnCreateDevice()
        {
            Create();
        }

        protected virtual void Dispose(bool all)
        {
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            if (vertexDecl != null)
            {
                vertexDecl.Dispose();
                vertexDecl = null;
            }
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
