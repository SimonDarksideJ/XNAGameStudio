#region File Description
//-----------------------------------------------------------------------------
// AnimatedComponent.cs
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

namespace NinjAcademy
{
    /// <summary>
    /// This abstract class represent a component which has an animation that represents it visually.
    /// </summary>
    class AnimatedComponent : TexturedDrawableGameComponent
    {
        #region Fields and Properties
        

        private Vector2 halfFrameSize;

        protected Animation animation;

        /// <summary>
        /// Bounding box containing the component (actually a plane, as the box is flat). As the components position
        /// may change, the bounding box is calculated each time it is retrieved.
        /// </summary>
        public override BoundingBox Bounds
        {
            get
            {
                return new BoundingBox(new Vector3(Position, 0),
                    new Vector3(Position + new Vector2(animation.FrameWidth, animation.FrameHeight), 0));
            }
        }

        /// <summary>
        /// The width of a single frame of the component's animation.
        /// </summary>
        public override float Width
        {
            get
            {
                return animation.FrameWidth;
            }
        }

        /// <summary>
        /// The height of a single frame of the component's animation.
        /// </summary>
        public override float Height
        {
            get
            {
                return animation.FrameHeight;
            }
        }

        /// <summary>
        /// The center of the component's bounds. As the components position may change, the value is calculated each 
        /// time it is retrieved.
        /// </summary>
        public override Vector2 Center
        {
            get
            {
                return Position + halfFrameSize;
            }
        }        


        #endregion

        #region Initialization
        

        /// <summary>
        /// Creates a new animated component instance.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Screen where the component will be presented.</param>
        /// <param name="animation">The animation which the component will display.</param>
        public AnimatedComponent(Game game, GameScreen gameScreen, Animation animation) : 
            base(game, gameScreen, animation.AnimationSheet)
        {
            this.animation = animation;
            VisualCenter = animation.VisualCenter;
            halfFrameSize = new Vector2(animation.FrameWidth, animation.FrameHeight) / 2f;
        }

        /// <summary>
        /// Creates a new animated component instance with a single-frame animation.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Screen where the component will be presented.</param>
        /// <param name="texture">The texture which will serve as a single frame animation.</param>
        /// <remarks>Calling this constructor creates a new animation object.</remarks>
        public AnimatedComponent(Game game, GameScreen gameScreen, Texture2D texture) :
            this(game, gameScreen, new Animation(texture, new Point(texture.Width, texture.Height), new Point(1, 1), 
                new Vector2(texture.Width, texture.Height) / 2f, false))
        {            
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the component by updating its embedded animation.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to Update.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            animation.Update(gameTime);
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Reset's the associated animation.
        /// </summary>
        public void ResetAnimation()
        {
            animation.PlayFromFrameIndex(0);
        }

        /// <summary>
        /// Pauses the component's animation.
        /// </summary>
        public void Pause()
        {
            animation.IsActive = false;
        }

        /// <summary>
        /// Resume's the component's animation.
        /// </summary>
        public void Resume()
        {
            animation.IsActive = true;
        }


        #endregion
    }
}
