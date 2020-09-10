#region File Description
//-----------------------------------------------------------------------------
// EvolvedSun.cs
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
    class EvolvedSun : Shape
    {
        private const int xCount = 1;
        private const int yCount = 1;
        private Effect effect;

        private EffectParameter worldParam;
        private EffectParameter worldViewProjectionParam;
        private EffectParameter sun0TextureParam;
        private EffectParameter sun1TextureParam;
        private EffectParameter blendFactor;

        private Texture2D[] sun;

        private int currentFrame;
        private double currentTime;

        public EvolvedSun(Game game)
            : base(game)
        {
        }

        public override void Create()
        {
            buffer = Plane(xCount, yCount);

            //Load the correct shader and set up the parameters
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));

            effect = SpacewarGame.ContentManager.Load<Effect>(SpacewarGame.Settings.MediaPath + @"shaders\sun");

            worldParam = effect.Parameters["world"];
            worldViewProjectionParam = effect.Parameters["worldViewProjection"];
            sun0TextureParam = effect.Parameters["Sun_Tex0"];
            sun1TextureParam = effect.Parameters["Sun_Tex1"];
            blendFactor = effect.Parameters["blendFactor"];

            //Preload the textures into the cache
            int numFrames = 5;
            sun = new Texture2D[numFrames];

            sun[0] = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\suntest1");
            sun[1] = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\suntest2");
            sun[2] = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\suntest3");
            sun[3] = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\suntest4");
            sun[4] = SpacewarGame.ContentManager.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"textures\suntest5");
        }

        public override void Update(TimeSpan timeSpan, TimeSpan elapsedTime)
        {
            base.Update(timeSpan, elapsedTime);

            // change frames every second.
            currentTime += elapsedTime.TotalMilliseconds;
            if (currentTime > 1000.0f)
            {
                currentTime = 0.0f;
                currentFrame++;
            }

            if (currentFrame > 4)
                currentFrame = 0;
        }

        public override void Render()
        {
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)GameInstance.Services.GetService(typeof(IGraphicsDeviceService));
            GraphicsDevice device = graphicsService.GraphicsDevice;

            base.Render();

            device.SetVertexBuffer(buffer);           

            worldParam.SetValue(World);
            worldViewProjectionParam.SetValue(World * SpacewarGame.Camera.View * SpacewarGame.Camera.Projection);

            float currentFactor = (float)(currentTime / 1000.0f);
            blendFactor.SetValue(currentFactor);

            sun1TextureParam.SetValue(sun[currentFrame]);

            if (currentFrame < 4)
                sun0TextureParam.SetValue(sun[currentFrame + 1]);
            else
                sun0TextureParam.SetValue(sun[0]);

            effect.Techniques[0].Passes[0].Apply();

            device.DrawPrimitives(PrimitiveType.TriangleList, 0, xCount * yCount * 2);            
        }
    }
}
