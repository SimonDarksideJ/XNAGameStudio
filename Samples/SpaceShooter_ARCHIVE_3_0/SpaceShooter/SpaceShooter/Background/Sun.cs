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
    class Sun : DrawableGameComponent
    {
        SphereMesh star;
        Texture2D starDiffuse;
        Texture2D haloDiffuse;

        SpriteBatch halo;
        Color haloColor;
        Vector4 starColor;

        Effect starEffect;
        EffectParameter starWorldMatrix;
        EffectParameter starViewMatrix;
        EffectParameter starProjectionMatrix;
        EffectParameter starDiffuseTexture;
        EffectParameter starColorData;

        Vector3 rotation;
        Vector3 position;

        Vector2 spriteCenter;

        public Sun(Game game)
            : base(game)
        {
        }

        public void Initialize(float radius, Vector3 pos)
        {
            star = new SphereMesh(new Vector3(radius, radius, radius), 100, 100);

            position = pos;

            base.Initialize();
        }

        public void LoadContent(Game game, string diffuseTexture, Color starColor, Color haloColor)
        {
            starDiffuse = game.Content.Load<Texture2D>(@"textures\" + diffuseTexture);
            haloDiffuse = game.Content.Load<Texture2D>(@"textures\glow");

            spriteCenter = new Vector2(haloDiffuse.Width / 2, haloDiffuse.Height / 2);

            starEffect = game.Content.Load<Effect>(@"shaders\sunEffect");

            starWorldMatrix = starEffect.Parameters["World"];
            starViewMatrix = starEffect.Parameters["View"];
            starProjectionMatrix = starEffect.Parameters["Projection"];

            starDiffuseTexture = starEffect.Parameters["DiffuseTexture"];

            starColorData = starEffect.Parameters["StarColor"];

            halo = new SpriteBatch(game.GraphicsDevice);
            this.starColor = new Vector4((float)starColor.R / 255.0f, (float)starColor.G / 255.0f, (float)starColor.B / 255.0f, 1.0f);
            this.haloColor = haloColor;

            star.LoadContent(game);
        }

        public override void Update(GameTime gameTime)
        {
            rotation.Y = MathHelper.ToRadians(0);
            float currentTime = (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            rotation.X += MathHelper.ToRadians(0.30f) * currentTime;
        }

        public bool IsVisible(Vector2 screenPos)
        {
            // check if halo is anywhere on screen
            if (((screenPos.X + spriteCenter.X) < 0.0f) ||
               ((screenPos.X - spriteCenter.X) > (float)GraphicsDevice.Viewport.Width) ||
               ((screenPos.Y + spriteCenter.Y) < 0.0f) ||
               ((screenPos.Y - spriteCenter.Y) > (float)GraphicsDevice.Viewport.Height))
                return false;

            return true;
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            // Figure out where on screen to draw halo Effect sprite
            Matrix viewProj = camera.View * camera.Projection;
            Vector4 projResult = Vector4.Transform(position, viewProj);

            float halfScreenY = ((float)GraphicsDevice.Viewport.Height / 2.0f);
            float halfScreenX = ((float)GraphicsDevice.Viewport.Width / 2.0f);

            Vector2 screenPos = new Vector2(((projResult.X / projResult.W) * halfScreenX) + halfScreenX, halfScreenY - ((projResult.Y / projResult.W) * halfScreenY));

            // First check of projResult.W is to determine 
            // if camera is facing the sun or turned away from the sun
            // projResult.W is negative if camera is facing away
            if ((projResult.W > 0.0f) && IsVisible(screenPos))
            {
                Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(rotation.X, 0, 0);
                worldMatrix = worldMatrix * Matrix.CreateFromYawPitchRoll(0, 0, rotation.Y);

                worldMatrix.Translation = position;

                // Draw Halo effect around star
                halo.Begin();
                halo.Draw(haloDiffuse, screenPos,
                           null, haloColor, 0.0f, spriteCenter, 1.0f, SpriteEffects.None, 0.0f);
                halo.End();

                // Draw Star
                starEffect.Begin();
                starEffect.Techniques[0].Passes[0].Begin();

                starWorldMatrix.SetValue(worldMatrix);
                starViewMatrix.SetValue(camera.View);
                starProjectionMatrix.SetValue(camera.Projection);
                starDiffuseTexture.SetValue(starDiffuse);
                starColorData.SetValue(starColor);

                starEffect.CommitChanges();

                GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                GraphicsDevice.RenderState.DepthBufferEnable = true;
                GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                star.Render(GraphicsDevice);

                GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

                starEffect.Techniques[0].Passes[0].End();
                starEffect.End();
            }
        }
    }
}
