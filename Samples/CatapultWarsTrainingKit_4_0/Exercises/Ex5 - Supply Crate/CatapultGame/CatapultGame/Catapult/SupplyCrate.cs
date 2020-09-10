#region File Description
//-----------------------------------------------------------------------------
// SupplyCrate.cs
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
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Devices;
using System.Xml.Linq;
#endregion

namespace CatapultGame
{
    #region Catapult states definition enum
    public enum CrateState
    {
        Idle,
        Hit        
    }
    #endregion

    public class SupplyCrate : DrawableGameComponent
    {
        #region Variables/Fields and Properties
        // Hold what the game to which the catapult belongs
        CatapultGame curGame = null;

        SpriteBatch spriteBatch;

        Vector2 catapultCenter;
        bool isAI;

        public bool AnimationRunning { get; set; }

        public bool IsDestroyed { get; set; }
        
        Texture2D idleTexture;
        string idleTextureName;

        Dictionary<string, Animation> animations;

        // State of the crate during its last update
        CrateState lastUpdateState = CrateState.Idle;
        
        // Current state of the Crate
        CrateState currentState;
        public CrateState CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }        

        // Constants/properties relating to the crate's position
        //const int positionXOffset = 60;
        const int positionXOffset = -100;
        const int positionYOffset = 35;
        public Vector2 Position {get; set;}                

        public int Width
        {
            get
            {
                return idleTexture.Width;
            }
        }
        
        public int Height
        {
            get
            {
                return idleTexture.Height;
            }
        }
        #endregion

        #region Initialization
        public SupplyCrate(Game game)
            : base(game)
        {
            curGame = (CatapultGame)game;
        }

        /// <summary>
        /// Creates a new supply crate instance. The crate is positioned relative to 
        /// a catapult, using internal constants and depending on whether or not the
        /// catapult is player controlled or not.
        /// </summary>
        /// <param name="game">The game itself.</param>
        /// <param name="screenSpriteBatch">Sprite batch with which to draw on 
        /// the display.</param>
        /// <param name="IdleTextureName">Name (path) of the crate's 
        /// idle texture.</param>
        /// <param name="CatapultCenterPosition">The position of the center of the
        /// catapult to which the crate belongs.</param>
        /// <param name="isAI">Whether or not the associated catapult is 
        /// AI conteolled.</param>
        public SupplyCrate(Game game, SpriteBatch screenSpriteBatch,
          string IdleTextureName, Vector2 CatapultCenterPosition, bool isAI)
            : this(game)
        {
            idleTextureName = IdleTextureName;            
            spriteBatch = screenSpriteBatch;
            catapultCenter = CatapultCenterPosition;
            this.isAI = isAI;
            animations = new Dictionary<string, Animation>();
        }

        /// <summary>
        /// Function initializes the crate instance and loads the animations 
        /// from the XML definition sheet
        /// </summary>
        public override void Initialize()
        {
            // Define initial state of the crate
            AnimationRunning = false;
            currentState = CrateState.Idle;

            // Load multiple animations form XML definition
            XDocument doc = XDocument.Load("Content/Textures/Crate/AnimationsDef.xml");
            XName name = XName.Get("Definition");
            var definitions = doc.Document.Descendants(name);

            // Loop over all definitions in XML
            foreach (var animationDefinition in definitions)
            {
                // Get a name of the animation
                string animatonAlias = animationDefinition.Attribute("Alias").Value;
                Texture2D texture =
                    curGame.Content.Load<Texture2D>(
                    animationDefinition.Attribute("SheetName").Value);

                // Get the frame size (width & height)
                Point frameSize = new Point();
                frameSize.X = 
                    int.Parse(animationDefinition.Attribute("FrameWidth").Value);
                frameSize.Y = 
                    int.Parse(animationDefinition.Attribute("FrameHeight").Value);

                // Get the frames sheet dimensions
                Point sheetSize = new Point();
                sheetSize.X = 
                    int.Parse(animationDefinition.Attribute("SheetColumns").Value);
                sheetSize.Y = 
                    int.Parse(animationDefinition.Attribute("SheetRows").Value);

                // Defing animation speed
                TimeSpan frameInterval = TimeSpan.FromSeconds((float)1 /
                    int.Parse(animationDefinition.Attribute("Speed").Value));

                Animation animation = new Animation(texture, frameSize, sheetSize);

                // If definition has an offset defined - means that it should be 
                // rendered relatively to some element/other animation - load it
                if (null != animationDefinition.Attribute("OffsetX") &&
                    null != animationDefinition.Attribute("OffsetY"))
                {
                    animation.Offset = new Vector2(
                        int.Parse(animationDefinition.Attribute("OffsetX").Value),
                        int.Parse(animationDefinition.Attribute("OffsetY").Value));
                }

                animations.Add(animatonAlias, animation);                
            }

            // Load the idle texture
            idleTexture = curGame.Content.Load<Texture2D>(idleTextureName);

            int xOffset = isAI ? positionXOffset : -positionXOffset - idleTexture.Width;
            Position = catapultCenter + new Vector2(xOffset, positionYOffset);            

            base.Initialize();
        }
        #endregion

        #region Update and Render
        public override void Update(GameTime gameTime)
        {            
            CrateState postUpdateStateChange = 0;

            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            // The crate is destroyed, so there is nothing to update
            if (IsDestroyed)
            {
                base.Update(gameTime);
                return;
            }
                
            switch (currentState)
            {
                case CrateState.Idle:
                    // Nothing to do
                    break;                
                case CrateState.Hit:
                    // Progress hit animation
                    if (animations["explode"].IsActive == false)
                    {
                        IsDestroyed = true;
                    }

                    animations["explode"].Update();
                    break;
                default:
                    break;
            }

            lastUpdateState = currentState;
            if (postUpdateStateChange != 0)
            {
                currentState = postUpdateStateChange;
            }

            base.Update(gameTime);
        }
        
        public override void Draw(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime");

            // Crate is destroyed, there is nothing to draw
            if (IsDestroyed)
            {
                base.Draw(gameTime);
                return;
            }

            // Using the last update state makes sure we do not draw
            // before updating animations properly
            switch (lastUpdateState)
            {
                case CrateState.Idle:
                    spriteBatch.Draw(idleTexture, Position, Color.White);
                    break;
                case CrateState.Hit:
                    // Crate hit animation
                    animations["explode"].Draw(spriteBatch, Position, 
                        SpriteEffects.None);                    
                    break;
                default:
                    break;
            }

            base.Draw(gameTime);
        }
        #endregion

        #region Hit
        /// <summary>
        /// Start Hit sequence on crate
        /// </summary>
        public void Hit()
        {
            AnimationRunning = true;
            animations["explode"].PlayFromFrameIndex(0);
            currentState = CrateState.Hit;
        }
        #endregion
    }
}
