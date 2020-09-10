#region File Description
//-----------------------------------------------------------------------------
// TexturedDrawableGameComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion


namespace NinjAcademy
{
    /// <summary>
    /// This abstract class represent a component which has a texture that represents it visually.
    /// </summary>
    public abstract class TexturedDrawableGameComponent : RestorableStateComponent
    {
        #region Fields/Properties

        private Vector2 halfTextureDimensions;

        protected SpriteBatch spriteBatch;
        protected Texture2D texture;        
        protected GameScreen gameScreen;

        /// <summary>
        /// The component's position.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Bounding box containing the component (actually a plane, as the box is flat). As the components position
        /// may change, the bounding box is calculated each time it is retrieved.
        /// </summary>
        public virtual BoundingBox Bounds
        {
            get
            {
                return new BoundingBox(new Vector3(Position, 0), 
                    new Vector3(Position + new Vector2(texture.Width, texture.Height), 0));
            }            
        }

        /// <summary>
        /// The center of the component's bounds. As the components position may change, the value is calculated each 
        /// time it is retrieved.
        /// </summary>
        public virtual Vector2 Center 
        {
            get 
            {
                return Position + halfTextureDimensions;
            }            
        }

        /// <summary>
        /// The width of the component's texture.
        /// </summary>
        public virtual float Width
        {
            get
            {
                return texture.Width;
            }
        }

        /// <summary>
        /// The height of the component's texture.
        /// </summary>
        public virtual float Height
        {
            get
            {
                return texture.Height;
            }
        }

        /// <summary>
        /// Returns the center of the texture which represents the component.
        /// </summary>
        protected Vector2 VisualCenter { get; set; }

        #endregion

        #region Initialization
        

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameScreen">Screen where the component will be presented.</param>
        /// <param name="texture">The asset which serves as this component's texture.</param>
        public TexturedDrawableGameComponent(Game game, GameScreen gameScreen, Texture2D texture)
            : base(game)
        {
            this.gameScreen = gameScreen;
            this.texture = texture;
            spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            halfTextureDimensions = new Vector2(texture.Width, texture.Height) / 2f;
            VisualCenter = halfTextureDimensions;
        }


        #endregion        
    }
}
