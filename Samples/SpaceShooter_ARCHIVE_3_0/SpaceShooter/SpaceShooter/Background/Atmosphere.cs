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
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace SpaceShooter
{
    class Atmosphere : DrawableGameComponent
    {
        SphereMesh atmosphereMesh;

        Texture2D atmosphereDiffuse;

        Effect atmosphereEffect;
        EffectParameter atmosWorldMatrix;
        EffectParameter atmosViewMatrix;
        EffectParameter atmosProjectionMatrix;
        EffectParameter atmosDiffuseTexture;
        EffectParameter atmosLightPosition;
        EffectParameter atmosLightColor;

        float planetRadius;
        float atmosRadius;

        Vector3 rotation;
        Vector3 position;

        BoundingSphere bs;
        ContentManager localContent;

        public Atmosphere(Game game)
            : base(game)
        {
            localContent = new ContentManager(game.Services, "Content");
        }

        public void Initialize(float planetRadius, float atmosRadius, Vector4 haloColor, float polarRadius, Vector3 pos)
        {
            float atRadius = planetRadius + atmosRadius;
            float prRadius = polarRadius + atmosRadius;
            atmosphereMesh = new SphereMesh(new Vector3(atRadius, prRadius, atRadius), 100, 100);

            this.planetRadius = planetRadius;
            this.atmosRadius = atmosRadius;

            position = pos;

            base.Initialize();
        }

        public void LoadContent(Game game, string atmosphereTexture)
        {
            if (!string.IsNullOrEmpty(atmosphereTexture))
                atmosphereDiffuse = localContent.Load<Texture2D>(@"textures\" + atmosphereTexture);

            atmosphereEffect = game.Content.Load<Effect>(@"shaders\atmosphereEffect");

            atmosWorldMatrix = atmosphereEffect.Parameters["World"];
            atmosViewMatrix = atmosphereEffect.Parameters["View"];
            atmosProjectionMatrix = atmosphereEffect.Parameters["Projection"];

            atmosDiffuseTexture = atmosphereEffect.Parameters["DiffuseTexture"];
            atmosLightPosition = atmosphereEffect.Parameters["DirectionalLight"];
            atmosLightColor = atmosphereEffect.Parameters["DirectionalLightColor"];

            atmosphereMesh.LoadContent(game);

            bs = new BoundingSphere(position, planetRadius + atmosRadius);
        }

        public override void Update(GameTime gameTime)
        {
            rotation.Y = MathHelper.ToRadians(23.45f);
            float currentTime = (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            rotation.X += MathHelper.ToRadians(1.30f) * currentTime;

            base.Update(gameTime);
        }

        public bool IsVisible(Camera camera)
        {
            ContainmentType contains = camera.BF.Contains(bs);
            return (contains == ContainmentType.Contains || contains == ContainmentType.Intersects);
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            if (IsVisible(camera))
            {
                float distance = (position - camera.CameraPosition).Length();
                float angularSize = (float)Math.Tan((planetRadius + atmosRadius) / distance);
                float sizeInPixels = angularSize * GraphicsDevice.Viewport.Height / MathHelper.ToRadians(45.0f);

                if (sizeInPixels > 6.0f)
                {
                    Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(rotation.X, 0, 0);
                    worldMatrix = worldMatrix * Matrix.CreateFromYawPitchRoll(0, 0, rotation.Y);
                    worldMatrix.Translation = position;

                    Vector3 lightDir = Vector3.Forward;

                    if (atmosphereDiffuse != null)
                    {
                        // Draw the cloud layer.
                        atmosphereEffect.Begin();
                        atmosphereEffect.Techniques[0].Passes[0].Begin();

                        atmosWorldMatrix.SetValue(worldMatrix);
                        atmosViewMatrix.SetValue(camera.View);
                        atmosProjectionMatrix.SetValue(camera.Projection);
                        atmosDiffuseTexture.SetValue(atmosphereDiffuse);
                        atmosLightPosition.SetValue(lightDir);
                        atmosLightColor.SetValue(new Vector4(1, 1, 1, 1));

                        atmosphereEffect.CommitChanges();

                        GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                        GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                        GraphicsDevice.RenderState.DepthBufferEnable = false;
                        GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

                        atmosphereMesh.Render(GraphicsDevice);

                        GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
                        GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        GraphicsDevice.RenderState.DepthBufferEnable = true;
                        GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

                        atmosphereEffect.Techniques[0].Passes[0].End();
                        atmosphereEffect.End();
                    }
                }
            }
        }
    }
}