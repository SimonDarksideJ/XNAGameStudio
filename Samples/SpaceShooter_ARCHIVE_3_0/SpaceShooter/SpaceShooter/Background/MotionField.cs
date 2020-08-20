#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#region Using
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SpaceShooter
{
    public struct VertexPointSprite
    {
        public Vector3 Position;
        public float Size;
        public float Opacity;
        public float Rotation;

        public static int SizeInBytes = 24;

        public VertexPointSprite(Vector3 position, float size)
        {
            this.Position = position;
            this.Size = size;
            this.Opacity = 1.0f;
            this.Rotation = 0.0f;
        }

        public static VertexElement[] VertexElements =
        {
            new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
            new VertexElement(0, 12, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.PointSize, 0),
            new VertexElement(0, 16, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.Color, 0),
            new VertexElement(0, 20, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.Color, 1),
        };
    }

    class MotionField : DrawableGameComponent
    {
        VertexDeclaration vertexDeclaration;
        VertexBuffer vertexBuffer;
        Texture2D starTexture;
        Effect starEffect;
        int starCount = 200;

        VertexPointSprite[] data;

        Random random = new Random();

        bool motionReady;

        public MotionField(Game game)
            : base(game)
        {
            data = new VertexPointSprite[starCount];
        }

        void GenerateStars(Camera camera)
        {
            Matrix xform = Matrix.CreateFromQuaternion(camera.CameraRotation);
            Vector3 forward = xform.Forward;

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = new VertexPointSprite();
                Vector3 direction = new Vector3(-1.0f + 2.0f * (float)random.NextDouble(),
                                                -1.0f + 2.0f * (float)random.NextDouble(),
                                                -1.0f + 2.0f * (float)random.NextDouble());
                direction.Normalize();

                float distance = 32.0f + 204.80f * (float)random.NextDouble();
                data[i].Position = (forward * 260.0f) + direction * distance;
                data[i].Size = 1.75f;
                data[i].Opacity = 1.0f;
                data[i].Rotation = -1.0f + 2.0f * (float)random.NextDouble();
            }

            motionReady = true;
        }

        void MoveStars(Camera camera)
        {
            Matrix xform = Matrix.CreateFromQuaternion(camera.CameraRotation);
            Vector3 forward = xform.Forward;

            for (int i = 0; i < data.Length; ++i)
            {
                Vector3 pointNormal = data[i].Position - camera.CameraPosition;
                float particleDistance = pointNormal.Length();
                pointNormal.Normalize();

                // Is this particle still in front of the camera
                if (Vector3.Dot(pointNormal, forward) < 0.0f)
                {
                    // No, so we need to throw this point back out in front of camera.
                    Vector3 direction = new Vector3(-1.0f + 2.0f * (float)random.NextDouble(),
                                                    -1.0f + 2.0f * (float)random.NextDouble(),
                                                    -1.0f + 2.0f * (float)random.NextDouble());
                    direction.Normalize();

                    float randomDist = 32.0f + 204.80f * (float)random.NextDouble();
                    data[i].Position = (camera.CameraPosition + (forward * 260.0f)) + (direction * randomDist);
                }
                else
                {
                    if (particleDistance > 400.0f)
                    {
                        // Particle is too far in front, throw it behind us
                        // ship is moving backward
                        Vector3 direction = new Vector3(-1.0f + 2.0f * (float)random.NextDouble(),
                                                        -1.0f + 2.0f * (float)random.NextDouble(),
                                                        -1.0f + 2.0f * (float)random.NextDouble());
                        direction.Normalize();

                        float randomDist = 32.0f + 32.0f * (float)random.NextDouble();
                        data[i].Position = (camera.CameraPosition + (forward * -40.0f)) + (direction * randomDist);
                    }
                }
            }
        }

        protected override void LoadContent()
        {
            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPointSprite.VertexElements);

            starTexture = Game.Content.Load<Texture2D>(@"particles\SmokeTexture");
            starEffect = Game.Content.Load<Effect>(@"shaders\MotionEffect");

            vertexBuffer = new VertexBuffer(GraphicsDevice, data.Length * VertexPointSprite.SizeInBytes, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPointSprite>(data);

            base.LoadContent();
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            // First, we need to figure out which stars
            // are behind us and throw them out in front of us again
            // based on how fast the camera is moving
            if (!motionReady)
            {
                GenerateStars(camera);
                vertexBuffer.SetData<VertexPointSprite>(data);
            }
            else
            {
                MoveStars(camera);
                vertexBuffer.SetData<VertexPointSprite>(data);
            }

            {
                //
                // STARS
                //
                starEffect.Parameters["World"].SetValue(Matrix.Identity);
                starEffect.Parameters["View"].SetValue(camera.View);
                starEffect.Parameters["Projection"].SetValue(camera.Projection);
                starEffect.Parameters["Texture"].SetValue(starTexture);
                starEffect.Parameters["ViewportHeight"].SetValue(GraphicsDevice.Viewport.Height);

                GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPointSprite.SizeInBytes);
                GraphicsDevice.VertexDeclaration = vertexDeclaration;

                GraphicsDevice.RenderState.PointSpriteEnable = true;

                // Set the alpha blend mode.
                GraphicsDevice.RenderState.AlphaBlendEnable = true;
                GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
                GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

                // Set the alpha test mode.
                GraphicsDevice.RenderState.AlphaTestEnable = true;
                GraphicsDevice.RenderState.AlphaFunction = CompareFunction.Greater;
                GraphicsDevice.RenderState.ReferenceAlpha = 0;

                // Enable the depth buffer (so particles will not be visible through
                // solid objects like the space ship), but disable depth writes
                // (so particles will not obscure other particles).
                GraphicsDevice.RenderState.DepthBufferEnable = true;
                GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

                starEffect.Begin();

                for (int i = 0; i < starEffect.CurrentTechnique.Passes.Count; ++i)
                {
                    starEffect.CurrentTechnique.Passes[i].Begin();

                    GraphicsDevice.DrawPrimitives(PrimitiveType.PointList, 0, starCount);

                    starEffect.CurrentTechnique.Passes[i].End();
                }

                starEffect.End();

                GraphicsDevice.RenderState.PointSpriteEnable = false;

                GraphicsDevice.Vertices[0].SetSource(null, 0, 0);

                GraphicsDevice.RenderState.AlphaBlendEnable = false;
                GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            }

            base.Draw(gameTime);
        }
    }
}