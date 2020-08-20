#region File Description
//-----------------------------------------------------------------------------
// DualDemo.cs
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
    /// Demo shows how to use DualTextureEffect.
    /// </summary>
    class DualDemo : MenuComponent
    {
        // Fields.
        Model model;

        BoolMenuEntry showTexture;
        BoolMenuEntry showLightmap;

        Texture2D grey;

        float cameraRotation = 0;
        float cameraArc = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public DualDemo(DemoGame game)
            : base(game)
        {
            Entries.Add(showTexture = new BoolMenuEntry("texture"));
            Entries.Add(showLightmap = new BoolMenuEntry("light map"));
            Entries.Add(new MenuEntry { Text = "back", Clicked = delegate { Game.SetActiveMenu(0); } });
        }


        /// <summary>
        /// Resets the menu state.
        /// </summary>
        public override void Reset()
        {
            showTexture.Value = true;
            showLightmap.Value = true;

            cameraRotation = 124;
            cameraArc = -12;

            base.Reset();
        }


        /// <summary>
        /// Loads content for this demo.
        /// </summary>
        protected override void LoadContent()
        {
            model = Game.Content.Load<Model>("model");

            grey = new Texture2D(GraphicsDevice, 1, 1);
            grey.SetData(new Color[] { new Color(128, 128, 128, 255) });
        }


        /// <summary>
        /// Draws the DualTextureEffect demo.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            DrawTitle("dual texture effect", new Color(128, 160, 128), new Color(96, 128, 96));

            // Compute camera matrices.
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            Matrix rotation = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                              Matrix.CreateRotationZ(MathHelper.ToRadians(cameraArc));

            Matrix view = Matrix.CreateLookAt(new Vector3(35, 13, 0),
                                              new Vector3(0, 3, 0),
                                              Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    GraphicsDevice.Viewport.AspectRatio,
                                                                    2, 100);

            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            foreach (ModelMesh mesh in model.Meshes)
            {
                List<Texture2D> textures = new List<Texture2D>();

                foreach (DualTextureEffect effect in mesh.Effects)
                {
                    Matrix world = transforms[mesh.ParentBone.Index] * rotation;

                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.DiffuseColor = new Vector3(0.75f);

                    // Store the previous textures.
                    textures.Add(effect.Texture);
                    textures.Add(effect.Texture2);

                    // Optionally disable one or both textures.
                    if (!showTexture.Value)
                        effect.Texture = grey;

                    if (!showLightmap.Value)
                        effect.Texture2 = grey;
                }

                // Draw the mesh.
                mesh.Draw();

                // Restore the original textures.
                int i = 0;

                foreach (DualTextureEffect effect in mesh.Effects)
                {
                    effect.Texture = textures[i++];
                    effect.Texture2 = textures[i++];
                }
            }

            base.Draw(gameTime);
        }


        /// <summary>
        /// Dragging on the menu background rotates the camera.
        /// </summary>
        protected override void OnDrag(Vector2 delta)
        {
            cameraRotation = MathHelper.Clamp(cameraRotation + delta.X / 8, 0, 180);
            cameraArc = MathHelper.Clamp(cameraArc - delta.Y / 8, -50, 15);
        }
    }
}
