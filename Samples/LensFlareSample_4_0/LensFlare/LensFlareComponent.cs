#region File Description
//-----------------------------------------------------------------------------
// LensFlareComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace LensFlare
{
    /// <summary>
    /// Reusable component for drawing a lensflare effect over the top of a 3D scene.
    /// </summary>
    public class LensFlareComponent : DrawableGameComponent
    {
        #region Constants


        // How big is the circular glow effect?
        const float glowSize = 400;

        // How big a rectangle should we examine when issuing our occlusion queries?
        // Increasing this makes the flares fade out more gradually when the sun goes
        // behind scenery, while smaller query areas cause sudden on/off transitions.
        const float querySize = 100;


        #endregion

        #region Fields


        // These are set by the main game to tell us the position of the camera and sun.
        public Matrix View;
        public Matrix Projection;

        public Vector3 LightDirection = Vector3.Normalize(new Vector3(-1, -0.1f, 0.3f));


        // Computed by UpdateOcclusion, which projects LightDirection into screenspace.
        Vector2 lightPosition;
        bool lightBehindCamera;


        // Graphics objects.
        Texture2D glowSprite;
        SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        VertexPositionColor[] queryVertices;


        // Custom blend state so the occlusion query polygons do not show up on screen.
        static readonly BlendState ColorWriteDisable = new BlendState
        {
            ColorWriteChannels = ColorWriteChannels.None 
        };


        // An occlusion query is used to detect when the sun is hidden behind scenery.
        OcclusionQuery occlusionQuery;
        bool occlusionQueryActive;
        float occlusionAlpha;


        // The lensflare effect is made up from several individual flare graphics,
        // which move across the screen depending on the position of the sun. This
        // helper class keeps track of the position, size, and color for each flare.
        class Flare
        {
            public Flare(float position, float scale, Color color, string textureName)
            {
                Position = position;
                Scale = scale;
                Color = color;
                TextureName = textureName;
            }

            public float Position;
            public float Scale;
            public Color Color;
            public string TextureName;
            public Texture2D Texture;
        }


        // Array describes the position, size, color, and texture for each individual
        // flare graphic. The position value lies on a line between the sun and the
        // center of the screen. Zero places a flare directly over the top of the sun,
        // one is exactly in the middle of the screen, fractional positions lie in
        // between these two points, while negative values or positions greater than
        // one will move the flares outward toward the edge of the screen. Changing
        // the number of flares, or tweaking their positions and colors, can produce
        // a wide range of different lensflare effects without altering any other code.
        Flare[] flares =
        {
            new Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "flare1"),
            new Flare( 0.3f, 0.4f, new Color(100, 255, 200), "flare1"),
            new Flare( 1.2f, 1.0f, new Color(100,  50,  50), "flare1"),
            new Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "flare1"),

            new Flare(-0.3f, 0.7f, new Color(200,  50,  50), "flare2"),
            new Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "flare2"),
            new Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "flare2"),

            new Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "flare3"),
            new Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "flare3"),
            new Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "flare3"),
        };


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new lensflare component.
        /// </summary>
        public LensFlareComponent(Game game)
            : base(game)
        {
        }


        /// <summary>
        /// Loads the content used by the lensflare component.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a SpriteBatch for drawing the glow and flare sprites.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the glow and flare textures.
            glowSprite = Game.Content.Load<Texture2D>("glow");

            foreach (Flare flare in flares)
            {
                flare.Texture = Game.Content.Load<Texture2D>(flare.TextureName);
            }

            // Effect for drawing occlusion query polygons.
            basicEffect = new BasicEffect(GraphicsDevice);
            
            basicEffect.View = Matrix.Identity;
            basicEffect.VertexColorEnabled = true;

            // Create vertex data for the occlusion query polygons.
            queryVertices = new VertexPositionColor[4];

            queryVertices[0].Position = new Vector3(-querySize / 2, -querySize / 2, -1);
            queryVertices[1].Position = new Vector3( querySize / 2, -querySize / 2, -1);
            queryVertices[2].Position = new Vector3(-querySize / 2,  querySize / 2, -1);
            queryVertices[3].Position = new Vector3( querySize / 2,  querySize / 2, -1);

            // Create the occlusion query object.
            occlusionQuery = new OcclusionQuery(GraphicsDevice);
        }

    
        #endregion
        
        #region Draw


        /// <summary>
        /// Draws the lensflare component.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // Check whether the light is hidden behind the scenery.
            UpdateOcclusion();

            // Draw the flare effect.
            DrawGlow();
            DrawFlares();

            RestoreRenderStates();
        }


        /// <summary>
        /// Mesures how much of the sun is visible, by drawing a small rectangle,
        /// centered on the sun, but with the depth set to as far away as possible,
        /// and using an occlusion query to measure how many of these very-far-away
        /// pixels are not hidden behind the terrain.
        /// 
        /// The problem with occlusion queries is that the graphics card runs in
        /// parallel with the CPU. When you issue drawing commands, they are just
        /// stored in a buffer, and the graphics card can be as much as a frame delayed
        /// in getting around to processing the commands from that buffer. This means
        /// that even after we issue our occlusion query, the occlusion results will
        /// not be available until later, after the graphics card finishes processing
        /// these commands.
        /// 
        /// It would slow our game down too much if we waited for the graphics card,
        /// so instead we delay our occlusion processing by one frame. Each time
        /// around the game loop, we read back the occlusion results from the previous
        /// frame, then issue a new occlusion query ready for the next frame to read
        /// its result. This keeps the data flowing smoothly between the CPU and GPU,
        /// but also causes our data to be a frame out of date: we are deciding
        /// whether or not to draw our lensflare effect based on whether it was
        /// visible in the previous frame, as opposed to the current one! Fortunately,
        /// the camera tends to move slowly, and the lensflare fades in and out
        /// smoothly as it goes behind the scenery, so this out-by-one-frame error
        /// is not too noticeable in practice.
        /// </summary>
        public void UpdateOcclusion()
        {
            // The sun is infinitely distant, so it should not be affected by the
            // position of the camera. Floating point math doesn't support infinitely
            // distant vectors, but we can get the same result by making a copy of our
            // view matrix, then resetting the view translation to zero. Pretending the
            // camera has not moved position gives the same result as if the camera
            // was moving, but the light was infinitely far away. If our flares came
            // from a local object rather than the sun, we would use the original view
            // matrix here.
            Matrix infiniteView = View;

            infiniteView.Translation = Vector3.Zero;

            // Project the light position into 2D screen space.
            Viewport viewport = GraphicsDevice.Viewport;

            Vector3 projectedPosition = viewport.Project(-LightDirection, Projection,
                                                         infiniteView, Matrix.Identity);

            // Don't draw any flares if the light is behind the camera.
            if ((projectedPosition.Z < 0) || (projectedPosition.Z > 1))
            {
                lightBehindCamera = true;
                return;
            }

            lightPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
            lightBehindCamera = false;

            if (occlusionQueryActive)
            {
                // If the previous query has not yet completed, wait until it does.
                if (!occlusionQuery.IsComplete)
                    return;

                // Use the occlusion query pixel count to work
                // out what percentage of the sun is visible.
                const float queryArea = querySize * querySize;

                occlusionAlpha = Math.Min(occlusionQuery.PixelCount / queryArea, 1);
            }

            // Set renderstates for drawing the occlusion query geometry. We want depth
            // tests enabled, but depth writes disabled, and we disable color writes
            // to prevent this query polygon actually showing up on the screen.
            GraphicsDevice.BlendState = ColorWriteDisable;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            // Set up our BasicEffect to center on the current 2D light position.
            basicEffect.World = Matrix.CreateTranslation(lightPosition.X,
                                                         lightPosition.Y, 0);

            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0,
                                                                        viewport.Width,
                                                                        viewport.Height,
                                                                        0, 0, 1);

            basicEffect.CurrentTechnique.Passes[0].Apply();

            // Issue the occlusion query.
            occlusionQuery.Begin();

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, queryVertices, 0, 2);

            occlusionQuery.End();

            occlusionQueryActive = true;
        }


        /// <summary>
        /// Draws a large circular glow sprite, centered on the sun.
        /// </summary>
        public void DrawGlow()
        {
            if (lightBehindCamera || occlusionAlpha <= 0)
                return;

            Color color = Color.White * occlusionAlpha;
            Vector2 origin = new Vector2(glowSprite.Width, glowSprite.Height) / 2;
            float scale = glowSize * 2 / glowSprite.Width;

            spriteBatch.Begin();

            spriteBatch.Draw(glowSprite, lightPosition, null, color, 0,
                             origin, scale, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        /// <summary>
        /// Draws the lensflare sprites, computing the position
        /// of each one based on the current angle of the sun.
        /// </summary>
        public void DrawFlares()
        {
            if (lightBehindCamera || occlusionAlpha <= 0)
                return;

            Viewport viewport = GraphicsDevice.Viewport;

            // Lensflare sprites are positioned at intervals along a line that
            // runs from the 2D light position toward the center of the screen.
            Vector2 screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;
            
            Vector2 flareVector = screenCenter - lightPosition;

            // Draw the flare sprites using additive blending.
            spriteBatch.Begin(0, BlendState.Additive);

            foreach (Flare flare in flares)
            {
                // Compute the position of this flare sprite.
                Vector2 flarePosition = lightPosition + flareVector * flare.Position;

                // Set the flare alpha based on the previous occlusion query result.
                Vector4 flareColor = flare.Color.ToVector4();

                flareColor.W *= occlusionAlpha;

                // Center the sprite texture.
                Vector2 flareOrigin = new Vector2(flare.Texture.Width,
                                                  flare.Texture.Height) / 2;

                // Draw the flare.
                spriteBatch.Draw(flare.Texture, flarePosition, null,
                                 new Color(flareColor), 1, flareOrigin,
                                 flare.Scale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }


        /// <summary>
        /// Sets renderstates back to their default values after we finish drawing
        /// the lensflare, to avoid messing up the 3D terrain rendering.
        /// </summary>
        void RestoreRenderStates()
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
        }


        #endregion
    }
}
