#region File Description
//-----------------------------------------------------------------------------
// Screen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Spacewar
{
    /// <summary>
    /// Screen represents a unit of rendering for the game, generally transitional point such 
    /// as splash screens, selection screens and the actual game levels.
    /// </summary>
    abstract public class Screen
    {
        /// <summary>
        /// The root of the scene graph for this screen
        /// </summary>
        protected SceneItem scene = null;

        /// <summary>
        /// Overlay points to a screen that will be drawn AFTER this one, more than likely overlaying it
        /// </summary>
        protected Screen overlay;

        protected SpriteBatch batch = null;

        protected Game game = null;

        public Game GameInstance
        {
            get
            {
                return game;
            }
        }

        public SpriteBatch SpriteBatch
        {
            get
            {
                return batch;
            }
        }

        public Screen(Game game)
        {
            this.game = game;
            this.scene = new SceneItem(game);

            if (game != null)
            {
                IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));
                batch = new SpriteBatch(graphicsService.GraphicsDevice);
            }
        }

        /// <summary>
        /// Update changes the layout and positions based on input or other variables
        /// Base class updates all items in the scene then calls any overlays to get them to render themselves
        /// </summary>
        /// <param name="time">Total game time since it was started</param>
        /// <param name="elapsedTime">Elapsed game time since the last call to update</param>
        /// <returns>The next gamestate to transition to. Default is the return value of an overlay or NONE. Override Update if you want to change this behaviour</returns>
        public virtual GameState Update(TimeSpan time, TimeSpan elapsedTime)
        {
            //Update the Scene
            scene.Update(time, elapsedTime);

            //Default is no state changes, override the class if you want a different state
            return (overlay == null) ? GameState.None : overlay.Update(time, elapsedTime);
        }

        /// <summary>
        /// Renders this scene. The base class renders everything in the sceen graph and then calls any overlays to 
        /// get them to render themselves
        /// </summary>
        public virtual void Render()
        {
            //Render this scene then any overlays
            scene.Render();

            if (overlay != null)
                overlay.Render();
        }

        /// <summary>
        /// Tidies up the scene. Base class does nothing but calls the overlays 
        /// </summary>
        public virtual void Shutdown()
        {
            if (overlay != null)
                overlay.Shutdown();

            if (batch != null)
            {
                batch.Dispose();
                batch = null;
            }
        }

        /// <summary>
        /// OnCreateDevice is called when the device is created
        /// </summary>
        public virtual void OnCreateDevice()
        {
            //Re-Create the Sprite Batch!
            IGraphicsDeviceService graphicsService = (IGraphicsDeviceService)game.Services.GetService(typeof(IGraphicsDeviceService));
            batch = new SpriteBatch(graphicsService.GraphicsDevice);
        }
    }
}
