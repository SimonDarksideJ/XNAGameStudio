#region File Description
//-----------------------------------------------------------------------------
// SpinningTriangleControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace WinFormsGraphicsDevice
{
    /// <summary>
    /// Example control inherits from GraphicsDeviceControl, which allows it to
    /// render using a GraphicsDevice. This control shows how to draw animating
    /// 3D graphics inside a WinForms application. It hooks the Application.Idle
    /// event, using this to invalidate the control, which will cause the animation
    /// to constantly redraw.
    /// </summary>
    class SpinningTriangleControl : GraphicsDeviceControl
    {
        BasicEffect effect;
        Stopwatch timer;


        // Vertex positions and colors used to display a spinning triangle.
        public readonly VertexPositionColor[] Vertices =
        {
            new VertexPositionColor(new Vector3(-1, -1, 0), Color.Black),
            new VertexPositionColor(new Vector3( 1, -1, 0), Color.Black),
            new VertexPositionColor(new Vector3( 0,  1, 0), Color.Black),
        };


        /// <summary>
        /// Initializes the control.
        /// </summary>
        protected override void Initialize()
        {
            // Create our effect.
            effect = new BasicEffect(GraphicsDevice);

            effect.VertexColorEnabled = true;

            // Start the animation timer.
            timer = Stopwatch.StartNew();

            // Hook the idle event to constantly redraw our animation.
            Application.Idle += delegate { Invalidate(); };
        }


        /// <summary>
        /// Draws the control.
        /// </summary>
        protected override void Draw()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Spin the triangle according to how much time has passed.
            float time = (float)timer.Elapsed.TotalSeconds;

            float yaw = time * 0.7f;
            float pitch = time * 0.8f;
            float roll = time * 0.9f;

            // Set transform matrices.
            float aspect = GraphicsDevice.Viewport.AspectRatio;

            effect.World = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            effect.View = Matrix.CreateLookAt(new Vector3(0, 0, -5),
                                              Vector3.Zero, Vector3.Up);

            effect.Projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);

            // Set renderstates.
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Draw the triangle.
            effect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList,
                                              Vertices, 0, 1);
        }
    }
}
