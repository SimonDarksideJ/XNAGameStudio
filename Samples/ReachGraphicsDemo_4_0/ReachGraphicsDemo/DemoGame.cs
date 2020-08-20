#region File Description
//-----------------------------------------------------------------------------
// DemoGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
#endregion

namespace XnaGraphicsDemo
{
    /// <summary>
    /// The main game class.
    /// </summary>
    public class DemoGame : Microsoft.Xna.Framework.Game
    {
        // Constants.
        const float TransitionSpeed = 1.5f;
        const float ZoomyTextLifespan = 0.75f;


        // Properties.
        public GraphicsDeviceManager Graphics { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public SpriteFont Font { get; private set; }
        public SpriteFont BigFont { get; private set; }
        public Texture2D BlankTexture { get; private set; }
        public Matrix ScaleMatrix { get; private set; }


        // Fields.
        List<MenuComponent> menuComponents = new List<MenuComponent>();

        GameTime currentGameTime;


        // Transition effects provide swooshy crossfades when moving from one screen to another.
        float transitionTimer = float.MaxValue;
        int transitionMode;
        RenderTarget2D transitionRenderTarget;


        // Zoomy text provides visual feedback when selecting menu items.
        // This is implemented by the main game, rather than any individual menu
        // screen, because the zoomy effect from selecting a menu item needs to
        // display across the transition while that menu makes way for a new one.
        class ZoomyText
        {
            public string Text;
            public Vector2 Position;
            public float Age;
        }

        static List<ZoomyText> zoomyTexts = new List<ZoomyText>();


        /// <summary>
        /// Constructor.
        /// </summary>
        public DemoGame()
        {
            Content.RootDirectory = "Content";

            Graphics = new GraphicsDeviceManager(this);

            Graphics.PreferredBackBufferWidth = 480;
            Graphics.PreferredBackBufferHeight = 800;

#if WINDOWS_PHONE
            Graphics.IsFullScreen = true;
#else
            IsMouseVisible = true;
#endif

            TargetElapsedTime = TimeSpan.FromSeconds(1 / 30.0);

            // Create all the different menu screens.
            menuComponents.Add(new TitleMenu(this));
            menuComponents.Add(new BasicDemo(this));
            menuComponents.Add(new DualDemo(this));
            menuComponents.Add(new AlphaDemo(this));
            menuComponents.Add(new SkinnedDemo(this));
            menuComponents.Add(new EnvmapDemo(this));
            menuComponents.Add(new ParticleDemo(this));

            // Set all the menu screens except the first to hidden and inactive. 
            foreach (MenuComponent component in menuComponents)
            {
                component.Enabled = component.Visible = false;

                Components.Add(component);
            }

            // Make the title menu active and visible.
            menuComponents[0].Enabled = menuComponents[0].Visible = true;
        }


        /// <summary>
        /// Changes which menu screen is currently active.
        /// </summary>
        public void SetActiveMenu(int index)
        {
            // Trigger the transition effect.
            for (int i = 0; i < menuComponents.Count; i++)
            {
                if (menuComponents[i].Visible)
                {
                    BeginTransition(i, index);
                    break;
                }
            }

            // Mark the previous menu as inactive, and the new one as active.
            for (int i = 0; i < menuComponents.Count; i++)
            {
                menuComponents[i].Enabled = menuComponents[i].Visible = (i == index);

                menuComponents[i].Reset();
            }
        }


        /// <summary>
        /// Loads content and creates graphics resources.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Font = Content.Load<SpriteFont>("font");
            BigFont = Content.Load<SpriteFont>("bigfont");

            BlankTexture = new Texture2D(GraphicsDevice, 1, 1);
            BlankTexture.SetData(new Color[] { Color.White });

            transitionRenderTarget = new RenderTarget2D(GraphicsDevice, 480, 800, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, 0);
        }


        /// <summary>
        /// Updates the transition effect and zoomy text animations.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            currentGameTime = gameTime;

            UpdateZoomyText(gameTime);

            if (transitionTimer < float.MaxValue)
                transitionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // This updates game components, including the currently active menu screen.
            base.Update(gameTime);
        }


        /// <summary>
        /// Draws the game.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            ScaleMatrix = Matrix.CreateScale(Graphics.PreferredBackBufferWidth / 480f, Graphics.PreferredBackBufferHeight / 800f, 1);

            // This draws game components, including the currently active menu screen.
            base.Draw(gameTime);

            DrawTransitionEffect();
            DrawZoomyText();
        }


        /// <summary>
        /// Begins a transition effect, capturing a copy of the current screen into the transitionRenderTarget.
        /// </summary>
        void BeginTransition(int oldMenuIndex, int newMenuIndex)
        {
            ScaleMatrix = Matrix.Identity;

            GraphicsDevice.SetRenderTarget(transitionRenderTarget);

            // Draw the old menu screen into the rendertarget.
            menuComponents[oldMenuIndex].Draw(currentGameTime);

            // Force the rendertarget alpha channel to fully opaque.
            SpriteBatch.Begin(0, BlendState.Additive);
            SpriteBatch.Draw(BlankTexture, new Rectangle(0, 0, 480, 800), new Color(0, 0, 0, 255));
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            // Initialize the transition state.
            transitionTimer = (float)TargetElapsedTime.TotalSeconds;
            transitionMode = newMenuIndex;
        }


        /// <summary>
        /// Draws the transition effect, displaying various animating pieces of the rendertarget
        /// which contains the previous scene image over the top of the new scene. There are
        /// various different effects which animate these pieces in different ways.
        /// </summary>
        void DrawTransitionEffect()
        {
            if (transitionTimer >= TransitionSpeed)
                return;

            SpriteBatch.Begin();

            float mu = transitionTimer / TransitionSpeed;
            float alpha = 1 - mu;

            switch (transitionMode)
            {
                case 1:
                    // BasicEffect
                    DrawOpenCurtainsTransition(alpha);
                    break;

                case 2:
                case 5:
                    // DualTexture
                    // EnvironmentMap
                    DrawSpinningSquaresTransition(mu, alpha);
                    break;

                case 3:
                case 4:
                    // AlphaTest and Skinning
                    DrawChequeredAppearTransition(mu);
                    break;

                case 6:
                    // Particles
                    DrawFallingLinesTransition(mu);
                    break;

                default:
                    // Returning to menu.
                    DrawShrinkAndSpinTransition(mu, alpha);
                    break;
            }

            SpriteBatch.End();
        }


        /// <summary>
        /// Transition effect where the screen splits in half, opening down the middle.
        /// </summary>
        void DrawOpenCurtainsTransition(float alpha)
        {
            int w = (int)(240 * alpha * alpha);

            SpriteBatch.Draw(transitionRenderTarget, new Rectangle(0, 0, w, 800), new Rectangle(0, 0, 240, 800), Color.White * alpha);
            SpriteBatch.Draw(transitionRenderTarget, new Rectangle(480 - w, 0, w, 800), new Rectangle(240, 0, 240, 800), Color.White * alpha);
        }


        /// <summary>
        /// Transition effect where the screen splits into pieces, each spinning off in a different direction.
        /// </summary>
        void DrawSpinningSquaresTransition(float mu, float alpha)
        {
            Random random = new Random(23);

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Rectangle rect = new Rectangle(480 * x / 4, 800 * y / 8, 480 / 4, 800 / 8);

                    Vector2 origin = new Vector2(rect.Width, rect.Height) / 2;

                    float rotation = (float)(random.NextDouble() - 0.5) * mu * mu * 2;
                    float scale = 1 + (float)(random.NextDouble() - 0.5f) * mu * mu;

                    Vector2 pos = new Vector2(rect.Center.X, rect.Center.Y);

                    pos.X += (float)(random.NextDouble() - 0.5) * mu * mu * 400;
                    pos.Y += (float)(random.NextDouble() - 0.5) * mu * mu * 400;

                    SpriteBatch.Draw(transitionRenderTarget, pos, rect, Color.White * alpha, rotation, origin, scale, 0, 0);
                }
            }
        }


        /// <summary>
        /// Transition effect where each square of the image appears at a different time.
        /// </summary>
        void DrawChequeredAppearTransition(float mu)
        {
            Random random = new Random(23);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    Rectangle rect = new Rectangle(480 * x / 8, 800 * y / 16, 480 / 8, 800 / 16);

                    if (random.NextDouble() > mu * mu)
                        SpriteBatch.Draw(transitionRenderTarget, rect, rect, Color.White);
                }
            }

            // The zoomy text effect doesn't look so good with this
            // particular transition effect, so we temporarily disable it.
            zoomyTexts.Clear();
        }


        /// <summary>
        /// Transition effect where the image dissolves into a sequence of vertically falling lines.
        /// </summary>
        void DrawFallingLinesTransition(float mu)
        {
            Random random = new Random(23);

            const int segments = 60;

            for (int x = 0; x < segments; x++)
            {
                Rectangle rect = new Rectangle(480 * x / segments, 0, 480 / segments, 800);

                Vector2 pos = new Vector2(rect.X, 0);

                pos.Y += 800 * (float)Math.Pow(mu, random.NextDouble() * 10);

                SpriteBatch.Draw(transitionRenderTarget, pos, rect, Color.White);
            }
        }


        /// <summary>
        /// Transition effect where the image spins off toward the bottom left of the screen.
        /// </summary>
        void DrawShrinkAndSpinTransition(float mu, float alpha)
        {
            Vector2 origin = new Vector2(240, 400);
            Vector2 translate = (new Vector2(32, 800 - 32) - origin) * mu * mu;

            float rotation = mu * mu * -4;
            float scale = alpha * alpha;

            Color tint = Color.White * (float)Math.Sqrt(alpha);

            SpriteBatch.Draw(transitionRenderTarget, origin + translate, null, tint, rotation, origin, scale, 0, 0);
        }


        /// <summary>
        /// Creates a new zoomy text menu item selection effect.
        /// </summary>
        public static void SpawnZoomyText(string text, Vector2 position)
        {
            zoomyTexts.Add(new ZoomyText { Text = text, Position = position });
        }


        /// <summary>
        /// Updates the zoomy text animations.
        /// </summary>
        static void UpdateZoomyText(GameTime gameTime)
        {
            int i = 0;

            while (i < zoomyTexts.Count)
            {
                zoomyTexts[i].Age += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (zoomyTexts[i].Age >= ZoomyTextLifespan)
                    zoomyTexts.RemoveAt(i);
                else
                    i++;
            }
        }


        /// <summary>
        /// Draws the zoomy text animations.
        /// </summary>
        void DrawZoomyText()
        {
            if (zoomyTexts.Count <= 0)
                return;

            SpriteBatch.Begin(0, null, null, null, null, null, ScaleMatrix);

            foreach (ZoomyText zoomyText in zoomyTexts)
            {
                Vector2 pos = zoomyText.Position + Font.MeasureString(zoomyText.Text) / 2;

                float age = zoomyText.Age / ZoomyTextLifespan;
                float sqrtAge = (float)Math.Sqrt(age);

                float scale = 0.333f + sqrtAge * 2f;

                float alpha = 1 - age;

                SpriteFont font = BigFont;

                // Our BigFont only contains characters a-z, so if the text
                // contains any numbers, we have to use the other font instead.
                foreach (char ch in zoomyText.Text)
                {
                    if (char.IsDigit(ch))
                    {
                        font = Font;
                        scale *= 2;
                        break;
                    }
                }

                Vector2 origin = font.MeasureString(zoomyText.Text) / 2;

                SpriteBatch.DrawString(font, zoomyText.Text, pos, Color.Lerp(new Color(64, 64, 255), Color.White, sqrtAge) * alpha, 0, origin, scale, 0, 0);
            }

            SpriteBatch.End();
        }
    }
}
