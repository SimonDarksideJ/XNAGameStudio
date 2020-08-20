#region File Description
//-----------------------------------------------------------------------------
// BasicDemo.cs
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
using GeneratedGeometry;
#endregion

namespace XnaGraphicsDemo
{
    /// <summary>
    /// Enum controls what kind of lighting to use.
    /// </summary>
    public enum LightingMode
    {
        NoLighting,
        OneVertexLight,
        ThreeVertexLights,
        ThreePixelLights,
    }

    
    /// <summary>
    /// Demo shows how to use BasicEffect.
    /// </summary>
    class BasicDemo : MenuComponent
    {
        // Fields.
        Model grid;
        Tank tank = new Tank();
        LightModeMenu lightMode;
        BoolMenuEntry textureEnable;
        float zoom = 1;


        /// <summary>
        /// Constructor.
        /// </summary>
        public BasicDemo(DemoGame game)
            : base(game)
        {
            Entries.Add(textureEnable = new BoolMenuEntry("texture"));
            Entries.Add(lightMode = new LightModeMenu());
            Entries.Add(new MenuEntry { Text = "back", Clicked = delegate { Game.SetActiveMenu(0); } });
        }


        /// <summary>
        /// Resets the menu state.
        /// </summary>
        public override void Reset()
        {
            lightMode.LightMode = LightingMode.ThreeVertexLights;
            textureEnable.Value = true;
            zoom = 1;

            base.Reset();
        }


        /// <summary>
        /// Loads content for this demo.
        /// </summary>
        protected override void LoadContent()
        {
            tank.Load(Game.Content);

            grid = Game.Content.Load<Model>("grid");
        }


        /// <summary>
        /// Updates the tank animation.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            tank.Animate(gameTime);

            base.Update(gameTime);
        }

        
        /// <summary>
        /// Draws the BasicEffect demo.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds;

            // Compute camera matrices.
            Matrix rotation = Matrix.CreateRotationY(time * 0.1f);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                    GraphicsDevice.Viewport.AspectRatio,
                                                                    10,
                                                                    20000);

            Matrix view = Matrix.CreateLookAt(new Vector3(1500, 550, 0) * zoom + new Vector3(0, 150, 0),
                                              new Vector3(0, 150, 0),
                                              Vector3.Up);

            // Draw the title.
            DrawTitle("basic effect", new Color(192, 192, 192), new Color(156, 156, 156));

            // Set render states.
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            // Draw the background grid.
            grid.Draw(Matrix.CreateScale(1.5f) * rotation, view, projection);

            // Draw the tank model.
            tank.Draw(rotation, view, projection, lightMode.LightMode, textureEnable.Value);

            base.Draw(gameTime);
        }


        /// <summary>
        /// Dragging up and down on the menu background zooms in and out.
        /// </summary>
        protected override void OnDrag(Vector2 delta)
        {
            zoom = MathHelper.Clamp(zoom * (float)Math.Exp(delta.Y / 400), 0.4f, 6);
        }


        /// <summary>
        /// Custom menu entry subclass for cycling through the different lighting options.
        /// </summary>
        class LightModeMenu : MenuEntry
        {
            public LightingMode LightMode = LightingMode.ThreeVertexLights;


            public override void OnClicked()
            {
                if (LightMode == LightingMode.ThreePixelLights)
                    LightMode = 0;
                else
                    LightMode++;

                base.OnClicked();
            }


            public override string Text
            {
                get
                {
                    switch (LightMode)
                    {
                        case LightingMode.NoLighting:        return "no lighting";
                        case LightingMode.OneVertexLight:    return "one vertex light";
                        case LightingMode.ThreeVertexLights: return "three vertex lights";
                        case LightingMode.ThreePixelLights:  return "three pixel lights";

                        default:
                            throw new NotSupportedException();
                    }
                }

                set { }
            }
        }
    }
}
