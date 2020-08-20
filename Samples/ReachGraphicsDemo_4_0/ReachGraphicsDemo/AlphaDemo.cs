#region File Description
//-----------------------------------------------------------------------------
// AlphaDemo.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkinnedModel;
using SimpleAnimation;
using System.Diagnostics;
#endregion

namespace XnaGraphicsDemo
{
    /// <summary>
    /// Demo shows how to use AlphaTestEffect.
    /// </summary>
    class AlphaDemo : MenuComponent
    {
        // Fields.
        Tank tank = new Tank();
        Model grid;
        RenderTarget2D renderTarget;
        AlphaTestEffect alphaTestEffect;

        float cameraRotation = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public AlphaDemo(DemoGame game)
            : base(game)
        {
            Entries.Add(new MenuEntry { Text = "back", Clicked = delegate { Game.SetActiveMenu(0); } });
        }


        /// <summary>
        /// Resets the menu state.
        /// </summary>
        public override void Reset()
        {
            cameraRotation = 0.85f;

            base.Reset();
        }


        /// <summary>
        /// Loads content for this demo.
        /// </summary>
        protected override void LoadContent()
        {
            tank.Load(Game.Content);

            renderTarget = new RenderTarget2D(GraphicsDevice, 400, 400, false, SurfaceFormat.Color, DepthFormat.Depth24);

            alphaTestEffect = new AlphaTestEffect(GraphicsDevice);
            alphaTestEffect.AlphaFunction = CompareFunction.Greater;
            alphaTestEffect.ReferenceAlpha = 128;

            grid = Game.Content.Load<Model>("grid");
        }


        /// <summary>
        /// Animates the tank model.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            tank.Animate(gameTime);

            base.Update(gameTime);
        }


        /// <summary>
        /// Draws the AlphaTestEffect demo.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Compute camera matrices.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            Matrix tankRotation = Matrix.CreateRotationY(time * 0.15f);
            Matrix sceneRotation = Matrix.CreateRotationY(cameraRotation);

            Vector3 cameraPosition = new Vector3(1250, 250, 0);
            Vector3 cameraTarget = new Vector3(0, -100, 0);

            Matrix view = Matrix.CreateLookAt(cameraPosition,
                                              cameraTarget,
                                              Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    GraphicsDevice.Viewport.AspectRatio,
                                                                    10,
                                                                    10000);

            // Draw a single copy of the tank model into a rendertarget.
            DrawTankIntoRenderTarget(tankRotation, sceneRotation);

            // Draw the scene background.
            DrawTitle("alpha test effect", new Color(192, 192, 192), new Color(156, 156, 156));

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            grid.Draw(Matrix.CreateTranslation(0, -8, 0) * sceneRotation, view, projection);

            // Draw many copies of the imposter sprite, faking the illusion of a more complex 3D scene with many tanks.
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            DrawImposterSprites(tankRotation, sceneRotation, cameraPosition, cameraTarget, view, projection);

            base.Draw(gameTime);
        }


        /// <summary>
        /// Draws the 3D tank model into a rendertarget.
        /// </summary>
        void DrawTankIntoRenderTarget(Matrix tankRotation, Matrix sceneRotation)
        {
            Matrix view = Matrix.CreateLookAt(new Vector3(1250, 650, 0),
                                              new Vector3(0, 0, 0),
                                              Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    1,
                                                                    10,
                                                                    10000);

            RenderTargetBinding[] previousRenderTargets = GraphicsDevice.GetRenderTargets();

            GraphicsDevice.SetRenderTarget(renderTarget);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            GraphicsDevice.Clear(Color.Transparent);

            tank.Draw(tankRotation * sceneRotation * Matrix.CreateScale(0.9f), view, projection, LightingMode.OneVertexLight, true);

            GraphicsDevice.SetRenderTargets(previousRenderTargets);
        }


        /// <summary>
        /// Draws many copies of the rendertarget as 2D billboard sprites, positioned within the 3D scene.
        /// </summary>
        void DrawImposterSprites(Matrix tankRotation, Matrix sceneRotation, Vector3 cameraPosition, Vector3 cameraTarget, Matrix view, Matrix projection)
        {
            const int start = -2;
            const int end = 3;
            const int count = (end - start) * (end - start);

            const float size = 0.2f;
            const float spacing = 240;

            const float width = 120;
            const float height1 = 135;
            const float height2 = -135;

            // Create billboard vertices.
            VertexPositionTexture[] vertices = new VertexPositionTexture[count * 4];
            int i = 0;

            for (int x = start; x < end; x++)
            {
                for (int z = start; z < end; z++)
                {
                    Matrix scale = Matrix.CreateScale(size);
                    Matrix translation = Matrix.CreateTranslation(x * spacing, 0, z * spacing);
                    Matrix world = tankRotation * scale * translation * sceneRotation;

                    Matrix billboard = Matrix.CreateConstrainedBillboard(world.Translation, cameraPosition, Vector3.Up, cameraTarget - cameraPosition, null);

                    vertices[i].Position = Vector3.Transform(new Vector3(width, height1, 0), billboard);
                    vertices[i++].TextureCoordinate = new Vector2(0, 0);

                    vertices[i].Position = Vector3.Transform(new Vector3(-width, height1, 0), billboard);
                    vertices[i++].TextureCoordinate = new Vector2(1, 0);

                    vertices[i].Position = Vector3.Transform(new Vector3(-width, height2, 0), billboard);
                    vertices[i++].TextureCoordinate = new Vector2(1, 1);

                    vertices[i].Position = Vector3.Transform(new Vector3(width, height2, 0), billboard);
                    vertices[i++].TextureCoordinate = new Vector2(0, 1);
                }
            }

            // Create billboard indices.
            short[] indices = new short[count * 6];
            short currentVertex = 0;
            i = 0;

            while (i < indices.Length)
            {
                indices[i++] = currentVertex;
                indices[i++] = (short)(currentVertex + 1);
                indices[i++] = (short)(currentVertex + 2);

                indices[i++] = currentVertex;
                indices[i++] = (short)(currentVertex + 2);
                indices[i++] = (short)(currentVertex + 3);

                currentVertex += 4;
            }

            // Draw the billboard sprites.
            alphaTestEffect.World = Matrix.Identity;
            alphaTestEffect.View = view;
            alphaTestEffect.Projection = projection;
            alphaTestEffect.Texture = renderTarget;

            alphaTestEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, count * 4, indices, 0, count * 2);
        }


        /// <summary>
        /// Dragging on the menu background rotates the camera.
        /// </summary>
        protected override void OnDrag(Vector2 delta)
        {
            cameraRotation += delta.X / 400;
        }
    }
}
