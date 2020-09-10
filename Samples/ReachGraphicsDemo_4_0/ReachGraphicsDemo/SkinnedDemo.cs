#region File Description
//-----------------------------------------------------------------------------
// SkinnedDemo.cs
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
using GeneratedGeometry;
#endregion

namespace XnaGraphicsDemo
{
    /// <summary>
    /// Demo shows how to use SkinnedEffect.
    /// </summary>
    class SkinnedDemo : MenuComponent
    {
        // Fields.
        Sky sky;
        Model dude;
        AnimationPlayer animationPlayer;

        float cameraRotation = 0;
        float cameraArc = 0;


        /// <summary>
        /// Constructor.
        /// </summary>
        public SkinnedDemo(DemoGame game)
            : base(game)
        {
            Entries.Add(new MenuEntry { Text = "back", Clicked = delegate { Game.SetActiveMenu(0); } });
        }


        /// <summary>
        /// Resets the menu state.
        /// </summary>
        public override void Reset()
        {
            cameraRotation = 0;
            cameraArc = 0;

            base.Reset();
        }


        /// <summary>
        /// Loads content for this demo.
        /// </summary>
        protected override void LoadContent()
        {
            sky = Game.Content.Load<Sky>("sky");
            dude = Game.Content.Load<Model>("dude");

            // Look up our custom skinning information.
            SkinningData skinningData = dude.Tag as SkinningData;

            if (skinningData == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // Create an animation player, and start decoding an animation clip.
            animationPlayer = new AnimationPlayer(skinningData);

            AnimationClip clip = skinningData.AnimationClips["Take 001"];

            animationPlayer.StartClip(clip);
        }


        /// <summary>
        /// Updates the animation.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            animationPlayer.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);

            base.Update(gameTime);
        }


        /// <summary>
        /// Draws the SkinnedEffect demo.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Compute camera matrices.
            const float cameraDistance = 100;

            Matrix view = Matrix.CreateTranslation(0, -40, 0) *
                          Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                          Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                          Matrix.CreateLookAt(new Vector3(0, 0, -cameraDistance),
                                              new Vector3(0, 0, 0), Vector3.Up);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    GraphicsDevice.Viewport.AspectRatio,
                                                                    1,
                                                                    10000);

            // Draw the background.
            GraphicsDevice.Clear(Color.Black);

            sky.Draw(view, projection);

            DrawTitle("skinned effect", null, new Color(127, 112, 104));

            // Draw the animating character.
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            Matrix[] bones = animationPlayer.GetSkinTransforms();

            foreach (ModelMesh mesh in dude.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = Vector3.Zero;
                }

                mesh.Draw();
            }

            base.Draw(gameTime);
        }


        /// <summary>
        /// Dragging on the menu background rotates the camera.
        /// </summary>
        protected override void OnDrag(Vector2 delta)
        {
            cameraRotation += delta.X / 4;
            cameraArc = MathHelper.Clamp(cameraArc - delta.Y / 4, -70, 70);
        }
    }
}
