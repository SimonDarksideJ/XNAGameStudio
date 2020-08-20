#region File Description
//-----------------------------------------------------------------------------
// EnvmapDemo.cs
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
#endregion

namespace XnaGraphicsDemo
{
    /// <summary>
    /// Demo shows how to use EnvironmentMapEffect.
    /// </summary>
    class EnvmapDemo : MenuComponent
    {
        // Fields.
        Model model;
        Texture2D background;

        FloatMenuEntry amount;
        FloatMenuEntry fresnel;
        FloatMenuEntry specular;


        /// <summary>
        /// Constructor.
        /// </summary>
        public EnvmapDemo(DemoGame game)
            : base(game)
        {
            Entries.Add(amount = new FloatMenuEntry() { Text = "envmap" });
            Entries.Add(fresnel = new FloatMenuEntry() { Text = "fresnel" });
            Entries.Add(specular = new FloatMenuEntry() { Text = "specular" });
            Entries.Add(new MenuEntry { Text = "back", Clicked = delegate { Game.SetActiveMenu(0); } });
        }


        /// <summary>
        /// Resets the menu state.
        /// </summary>
        public override void Reset()
        {
            amount.Value = 1;
            fresnel.Value = 0.25f;
            specular.Value = 0.5f;

            base.Reset();
        }


        /// <summary>
        /// Loads content for this demo.
        /// </summary>
        protected override void LoadContent()
        {
            background = Game.Content.Load<Texture2D>("background");
            model = Game.Content.Load<Model>("saucer");
        }


        /// <summary>
        /// Draws the EnvironmentMapEffect demo.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Draw the background image.
            SpriteBatch.Begin(0, BlendState.Opaque);
            SpriteBatch.Draw(background, new Rectangle(0, 0, 480, 800), Color.White);
            SpriteBatch.End();

            DrawTitle("environment map effect", null, new Color(93, 142, 196));

            // Compute camera matrices.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            Matrix rotation = Matrix.CreateRotationX(time * 0.3f) *
                              Matrix.CreateRotationY(time);

            Matrix view = Matrix.CreateLookAt(new Vector3(4500, -400, 0),
                                              new Vector3(0, -400, 0),
                                              Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    GraphicsDevice.Viewport.AspectRatio,
                                                                    10, 10000);

            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap; 
            
            // Draw the spaceship model.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (EnvironmentMapEffect effect in mesh.Effects)
                {
                    Matrix world = transforms[mesh.ParentBone.Index] * rotation;

                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    effect.EnvironmentMapAmount = amount.Value;
                    effect.FresnelFactor = fresnel.Value * 2;
                    effect.EnvironmentMapSpecular = new Vector3(1, 1, 0.5f) * specular.Value;
                }

                mesh.Draw();
            }
            
            base.Draw(gameTime);
        }
    }
}
