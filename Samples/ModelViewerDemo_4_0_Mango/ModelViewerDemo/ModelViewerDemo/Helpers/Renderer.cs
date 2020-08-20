#region File Description
//-----------------------------------------------------------------------------
// Renderer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace ModelViewerDemo
{
    /// <summary>
    /// Renderer implements two interfaces for application lifetime. IApplicationService allows us to add it to the
    /// ApplicationLifetimeObjects collection and IApplicationLifetimeAware allows the class to be aware of the 
    /// application's life span, enabling us to load content once we know the app has started up.
    /// </summary>
	public class Renderer : IApplicationService, IApplicationLifetimeAware
	{
		public static Renderer Current { get; private set; }

		private Tank tank;
        private Sky sky;

        private VertexBuffer groundVertices;
        private BasicEffect groundEffect;

		private RendererState state;

        private float wheelAnimationTime, steerAnimationTime, turrentAnimationTime, cannonAnimationTime, hatchAnimationTime;

		public Tank Tank { get { return tank; } }
		public RendererState State { get { return state; } }
        
		public Renderer()
		{
			if (Current != null)
				throw new InvalidOperationException();

			Current = this;
		}

		public void Update(TimeSpan elapsedTime, TimeSpan totalTime)
		{
            float dt = (float)elapsedTime.TotalSeconds;

            if (state.TankWheelAnimationEnabled)
            {
                wheelAnimationTime += dt * 5;
                tank.WheelRotation = wheelAnimationTime;
            }

            if (state.TankSteeringAnimationEnabled)
            {
                steerAnimationTime += dt * 0.75f;
                tank.SteerRotation = (float)Math.Sin(steerAnimationTime) * 0.5f;
            }

            if (state.TankTurretAnimationEnabled)
            {
                turrentAnimationTime += dt * 0.333f;
                tank.TurretRotation = (float)Math.Sin(turrentAnimationTime) * 1.25f;
            }

            if (state.TankCannonAnimationEnabled)
            {
                cannonAnimationTime += dt * 0.25f;
                tank.CannonRotation = (float)Math.Sin(cannonAnimationTime) * 0.333f - 0.333f;
            }

            if (state.TankHatchAnimationEnabled)
            {
                hatchAnimationTime += dt * 2f;
                tank.HatchRotation = MathHelper.Clamp((float)Math.Sin(hatchAnimationTime) * 2, -1, 0);
            }
		}

		public void Draw()
		{
			GraphicsDevice device = SharedGraphicsDeviceManager.Current.GraphicsDevice;

            device.BlendState = BlendState.Opaque;
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;		

			// Store the old viewport and set the desired viewport
			Viewport oldViewport = device.Viewport;
			device.Viewport = new Viewport(state.ViewportRect);

			// Calculate the world-view-projection matrices for the tank and camera
			Matrix world = Matrix.CreateScale(.002f) * Matrix.CreateRotationY(MathHelper.ToRadians((float)state.TankRotationY));
			Matrix view = Matrix.CreateLookAt(state.CameraPosition, state.CameraTarget, Vector3.Up);
			Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, device.Viewport.AspectRatio, .01f, 100f);

            // Draw the sky
            sky.Draw(view, projection);

            // Draw the ground
            groundEffect.World = Matrix.Identity;
            groundEffect.View = view;
            groundEffect.Projection = projection;
            device.SetVertexBuffer(groundVertices);
            foreach (var pass in groundEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

			// Draw the tank
			tank.Draw(world, view, projection, state);

			// Reset the viewport
			device.Viewport = oldViewport;
		}

		#region IApplicationLifetimeAware Members

		void IApplicationLifetimeAware.Started()
		{
            // Create our state
            state = new RendererState();

			// Get the application's ContentManager and load the tank and sky
			ContentManager content = (Application.Current as App).Content;
			tank = new Tank();
			tank.Load(content, state);
            sky = content.Load<Sky>("sky");

            // Create the ground plane
            groundEffect = new BasicEffect(SharedGraphicsDeviceManager.Current.GraphicsDevice)
            {
                TextureEnabled = true,
                Texture = content.Load<Texture2D>("rocks"),
                LightingEnabled = false,
            };

            const float groundHalfSize = 50f;
            const float groundTiling = 15f;
            groundVertices = new VertexBuffer(SharedGraphicsDeviceManager.Current.GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            groundVertices.SetData(new[]
            {
                new VertexPositionTexture(new Vector3(-groundHalfSize, 0, -groundHalfSize), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(groundHalfSize, 0, -groundHalfSize), new Vector2(groundTiling, 0)),
                new VertexPositionTexture(new Vector3(-groundHalfSize, 0, groundHalfSize), new Vector2(0, groundTiling)),
                new VertexPositionTexture(new Vector3(groundHalfSize, 0, groundHalfSize), new Vector2(groundTiling, groundTiling)),
            });
		}

		void IApplicationLifetimeAware.Starting() { }
		void IApplicationLifetimeAware.Exited() { }
		void IApplicationLifetimeAware.Exiting() { }

		#endregion

		#region IApplicationService Members

		public void StartService(ApplicationServiceContext context) { }
		public void StopService() { }

		#endregion
	}
}
