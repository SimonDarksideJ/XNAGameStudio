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
    class SkyBox : DrawableGameComponent
    {
        SphereBox sphereMesh;
        Texture2D skyBoxTexture;

        Effect effect;
        EffectParameter world;
        EffectParameter view;
        EffectParameter projection;
        EffectParameter diffuseTexture;

        public SkyBox(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            sphereMesh = new SphereBox(2000000, 5, 5);
            sphereMesh.TileUVs(16, 8);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            effect = Game.Content.Load<Effect>(@"shaders\skyeffect");
            skyBoxTexture = Game.Content.Load<Texture2D>(@"textures\StarField");

            world = effect.Parameters["World"];
            view = effect.Parameters["View"];
            projection = effect.Parameters["Projection"];

            diffuseTexture = effect.Parameters["DiffuseTexture"];

            sphereMesh.LoadContent(Game);
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            Matrix worldMatrix = Matrix.Identity;

            effect.Begin();
            effect.Techniques[0].Passes[0].Begin();

            world.SetValue(worldMatrix);
            view.SetValue(camera.View);
            projection.SetValue(camera.Projection);
            diffuseTexture.SetValue(skyBoxTexture);

            effect.CommitChanges();

            GraphicsDevice.RenderState.CullMode = CullMode.None;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            sphereMesh.Render(GraphicsDevice, camera);

            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            effect.Techniques[0].Passes[0].End();
            effect.End();
        }
    }
}