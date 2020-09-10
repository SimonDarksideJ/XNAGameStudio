#region File Description
//-----------------------------------------------------------------------------
// EvolvedBackdrop.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace Spacewar
{
    /// <summary>
    /// Handles the drawing of the moving nebula backdrops
    /// </summary>
    public class EvolvedBackdrop : Shape, IDisposable
    {
        private const int xCount = 1;
        private const int yCount = 1;
        private Effect effect;
        private float layerFactor;
        private float timeFactor1;
        private float timeFactor2;
        private Vector4 layer1Offset;
        private Vector4 layer2Offset;

        private EffectParameter layer1TextureParam;
        private EffectParameter layer2TextureParam;
        private EffectParameter layer3TextureParam;
        private EffectParameter layerFactorParam;
        private EffectParameter layer1OffsetParam;
        private EffectParameter layer2OffsetParam;

        private Texture2D layer1;
        private Texture2D layer2;
        private Texture2D layer3;

        public EvolvedBackdrop(Game game)
            : base(game)
        {
            layer1Offset = Vector4.Zero;
            layer2Offset = Vector4.Zero;
        }

        /// <summary>
        /// Creates the quad needed to render the textures to
        /// </summary>
        public override void Create()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
            buffer = new VertexBuffer(graphicsService.GraphicsDevice, typeof(VertexPositionColor), 6, BufferUsage.WriteOnly);

            VertexPositionColor[] data = new VertexPositionColor[6];
            data[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            data[1] = new VertexPositionColor(new Vector3(1280, 0, 0), Color.White);
            data[2] = new VertexPositionColor(new Vector3(1280, 720, 0), Color.White);
            data[3] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White); //same as 0
            data[4] = new VertexPositionColor(new Vector3(1280, 720, 0), Color.White); //same as 2
            data[5] = new VertexPositionColor(new Vector3(0, 720, 0), Color.White);
            buffer.SetData<VertexPositionColor>(data);

            effect = SpacewarGame.ContentManager.Load<Effect>(SpacewarGame.Settings.MediaPath + @"shaders\backdrop");

            layer1TextureParam = effect.Parameters["layer1"];
            layer2TextureParam = effect.Parameters["layer2"];
            layer3TextureParam = effect.Parameters["layer3"];
            layerFactorParam = effect.Parameters["layerFactor"];
            layer1OffsetParam = effect.Parameters["layer1Offset"];
            layer2OffsetParam = effect.Parameters["layer2Offset"];

            //Preload the textures into the cache
            // TODO: This doesn't support multiple "boards"
            layer1 = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\B1_nebula01");
            layer2 = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\B1_nebula02");
            layer3 = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\B1_stars");
        }

        /// <summary>
        /// Moves the layers and sets the fade values
        /// </summary>
        /// <param name="time">Current Game time</param>
        /// <param name="elapsedTime">Elapsed Game time since last update</param>
        public override void Update(TimeSpan time, TimeSpan elapsedTime)
        {
            base.Update(time, elapsedTime);

            timeFactor1 += (float)elapsedTime.TotalSeconds;
            timeFactor2 += (float)elapsedTime.TotalSeconds * 2;

            //Animate the fading and moving backdrops
            //Cross fade between 0.0 and 1.0
            layerFactor = (float)(Math.Sin(timeFactor2 * SpacewarGame.Settings.CrossFadeSpeed) * .5 + .5);

            //Move the nebula layers up to 200 pixels
            layer1Offset.X = (float)((100.0 / 1480.0) * .3 * (Math.Sin(timeFactor1 * SpacewarGame.Settings.OffsetSpeed / 2.0) + 1.0));
            layer1Offset.Y = (float)((100.0 / 920.0) * (Math.Cos(timeFactor1 * SpacewarGame.Settings.OffsetSpeed / 1.4) + 1.0));

            layer2Offset.X = (float)((100.0 / 1480.0) * (Math.Sin(timeFactor1 * SpacewarGame.Settings.OffsetSpeed) + 1.0));
            layer2Offset.Y = (float)((100.0 / 920.0) * .7 * (Math.Cos(timeFactor1 * SpacewarGame.Settings.OffsetSpeed / 1.3) + 1.0));
        }

        /// <summary>
        /// Renders the backdrop
        /// </summary>
        public override void Render()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
            GraphicsDevice device = graphicsService.GraphicsDevice;

            base.Render();

            device.SetVertexBuffer(buffer);
            
            layer1TextureParam.SetValue(layer1);
            layer2TextureParam.SetValue(layer2);
            layer3TextureParam.SetValue(layer3);
            layerFactorParam.SetValue(layerFactor);
            layer1OffsetParam.SetValue(layer1Offset);
            layer2OffsetParam.SetValue(layer2Offset);

            effect.Techniques[0].Passes[0].Apply();

            device.DrawPrimitives(PrimitiveType.TriangleList, 0, xCount * yCount * 2);
        }

        protected override void Dispose(bool all)
        {
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }

            base.Dispose(all);
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
