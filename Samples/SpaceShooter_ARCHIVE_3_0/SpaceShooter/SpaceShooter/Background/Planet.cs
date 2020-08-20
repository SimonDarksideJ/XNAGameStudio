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
    class Planet : DrawableGameComponent
    {
        SphereMesh planet;

        Vector3 position;

        Texture2D planetDiffuse;
        Texture2D planetBump;
        Texture2D planetSpec;
        Texture2D planetNight;

        Vector4 hazeColor;

        Effect planetEffect;
        EffectParameter planetWorldMatrix;
        EffectParameter planetViewMatrix;
        EffectParameter planetProjectionMatrix;
        EffectParameter planetDiffuseTexture;
        EffectParameter planetLightPosition;
        EffectParameter planetLightColor;
        EffectParameter planetCameraPos;
        EffectParameter planetSpecularPow;
        EffectParameter planetSpecularIntensity;
        EffectParameter planetBumpTexture;
        EffectParameter planetSpecTexture;
        EffectParameter planetNightTexture;
        EffectParameter planetHazeColor;

        Vector3 rotation;
        float radius;

        BoundingSphere bs;

        ContentManager localContent;

        public Planet(Game game)
            : base(game)
        {
            localContent = new ContentManager(game.Services, "Content");
        }

        public void Initialize(float planetRadius, Vector4 haze, float polarRadius, Vector3 pos)
        {
            planet = new SphereMesh(new Vector3(planetRadius, polarRadius, planetRadius), 100, 100);
            radius = planetRadius;

            hazeColor = haze;

            position = pos;

            base.Initialize();
        }

        public void LoadContent(Game game, string diffuseTexture, string bumpMap, string specMap, string nightMap)
        {
            planet.LoadContent(game);

            bs = new BoundingSphere(position, radius);

            planetDiffuse = localContent.Load<Texture2D>(@"textures\" + diffuseTexture);

            if (!string.IsNullOrEmpty(bumpMap))
                planetBump = localContent.Load<Texture2D>(@"textures\" + bumpMap);

            if (!string.IsNullOrEmpty(specMap))
                planetSpec = localContent.Load<Texture2D>(@"textures\" + specMap);

            if (!string.IsNullOrEmpty(nightMap))
                planetNight = localContent.Load<Texture2D>(@"textures\" + nightMap);

            planetEffect = game.Content.Load<Effect>(@"shaders\planetEffect");

            planetWorldMatrix = planetEffect.Parameters["World"];
            planetViewMatrix = planetEffect.Parameters["View"];
            planetProjectionMatrix = planetEffect.Parameters["Projection"];

            planetDiffuseTexture = planetEffect.Parameters["DiffuseTexture"];
            planetLightPosition = planetEffect.Parameters["DirectionalLight"];
            planetLightColor = planetEffect.Parameters["DirectionalLightColor"];
            planetHazeColor = planetEffect.Parameters["hazeColor"];

            planetCameraPos = planetEffect.Parameters["cameraPosition"];

            planetSpecularPow = planetEffect.Parameters["specularPower"];
            planetSpecularIntensity = planetEffect.Parameters["specularIntensity"];

            planetBumpTexture = planetEffect.Parameters["NormalMapTexture"];

            planetSpecTexture = planetEffect.Parameters["SpecularTexture"];

            planetNightTexture = planetEffect.Parameters["NightTexture"];
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
                float angularSize = (float)Math.Tan(radius / distance);
                float sizeInPixels = angularSize * GraphicsDevice.Viewport.Height / camera.FieldOfView;

                if (sizeInPixels > 6.0f)
                {
                    Matrix worldMatrix = Matrix.CreateFromYawPitchRoll(rotation.X, 0, 0);
                    worldMatrix = worldMatrix * Matrix.CreateFromYawPitchRoll(0, 0, rotation.Y);
                    worldMatrix.Translation = position;

                    Vector3 lightDir = Vector3.Forward;

                    planetEffect.Begin();
                    planetEffect.Techniques[0].Passes[0].Begin();

                    planetWorldMatrix.SetValue(worldMatrix);
                    planetViewMatrix.SetValue(camera.View);
                    planetProjectionMatrix.SetValue(camera.Projection);
                    planetDiffuseTexture.SetValue(planetDiffuse);
                    planetBumpTexture.SetValue(planetBump);
                    planetSpecTexture.SetValue(planetSpec);
                    planetNightTexture.SetValue(planetNight);

                    planetLightPosition.SetValue(lightDir);
                    planetLightColor.SetValue(new Vector4(1, 1, 1, 1));
                    planetSpecularPow.SetValue(20.0f);
                    planetSpecularIntensity.SetValue(1.0f);
                    planetCameraPos.SetValue(camera.CameraPosition);
                    planetHazeColor.SetValue(hazeColor);

                    planetEffect.CommitChanges();

                    GraphicsDevice.RenderState.CullMode = CullMode.CullClockwiseFace;
                    GraphicsDevice.RenderState.DepthBufferEnable = true;
                    GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

                    GraphicsDevice.RenderState.AlphaBlendEnable = false;

                    planet.Render(GraphicsDevice);

                    GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

                    planetEffect.Techniques[0].Passes[0].End();
                    planetEffect.End();
                }
            }
        }
    }
}