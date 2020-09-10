#region File Information
//-----------------------------------------------------------------------------
// Animation.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


#endregion

namespace NinjAcademy
{
    /// <summary>
    /// Supports animation playback. This is not an independent game component and calls to <see cref="Draw"/> and
    /// <see cref="Update"/> must be made when necessary.
    /// </summary>
    public class Animation
    {
        #region Fields and Properties

        [ContentSerializer]
        string animationSheetPath;
        [ContentSerializer]
        Point rowAndColumnAmount;
        Point currentFrame;
        [ContentSerializer]
        Point frameSize;

        TimeSpan frameChangeTimer = TimeSpan.Zero;
        TimeSpan frameChangeInterval = TimeSpan.Zero;

        [ContentSerializer]
        bool isCyclic;

        /// <summary>
        /// The texture which serves as the animation sheet.
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D AnimationSheet { get; private set; }

        /// <summary>
        /// The amount of frames contained in the animation.
        /// </summary>
        [ContentSerializer]
        public int FrameCount { get; private set; }

        /// <summary>
        /// The width of a single frame.
        /// </summary>
        [ContentSerializerIgnore]
        public int FrameWidth
        {
            get
            {
                return frameSize.X;
            }
        }

        /// <summary>
        /// The height of a single frame.
        /// </summary>
        [ContentSerializerIgnore]
        public int FrameHeight
        {
            get
            {
                return frameSize.Y;
            }
        }

        /// <summary>
        /// The index of the current animation frame.
        /// </summary>
        /// <remarks>The index value is calculated each time it is retrieved.</remarks>
        [ContentSerializerIgnore]
        public int FrameIndex
        {
            get
            {
                return rowAndColumnAmount.X * currentFrame.Y + currentFrame.X;
            }
            set
            {
                if (value >= rowAndColumnAmount.X * rowAndColumnAmount.Y + 1)
                {
                    throw new InvalidOperationException("Specified frame index exceeds available frames");
                }

                currentFrame.Y = value / rowAndColumnAmount.X;
                currentFrame.X = value % rowAndColumnAmount.X;
            }
        }

        /// <summary>
        /// The point inside a single frame which serves as its center.
        /// </summary>
        [ContentSerializer]
        public Vector2 VisualCenter { get; private set; }

        /// <summary>
        /// Controls whether the animation is updated when calling <see cref="Update"/>.
        /// </summary>
        public bool IsActive { get; set; }


        #endregion

        #region Initialization


        /// <summary>
        /// Private constructor responsible for marking the animation as active and allowing serialization.
        /// </summary>
        private Animation()
        {
            IsActive = true;
        }

        /// <summary>
        /// Creates a new instance of the animation class, without setting its animation sheet or its path.
        /// </summary>
        /// <param name="frameDimensions">The size of a single frame, in pixels.</param>
        /// <param name="rowAndColumnAmount">The amount of rows and columns in the animation sheet.</param>
        /// <param name="visualCenter">The coordinates inside a single frame which serve as its 
        /// visual center.</param>
        /// <param name="isCyclic">Whether or not the animation should loop once the final frame is reached.</param>
        public Animation(Point frameDimensions, Point rowAndColumnAmount, Vector2 visualCenter, bool isCyclic)
            : this()
        {
            frameSize = frameDimensions;
            this.VisualCenter = visualCenter;
            this.rowAndColumnAmount = rowAndColumnAmount;
            FrameCount = rowAndColumnAmount.X * rowAndColumnAmount.Y;
            this.isCyclic = isCyclic;
        }

        /// <summary>
        /// Creates a new instance of the animation class.
        /// </summary>
        /// <param name="animationSheetPath">Path to the texture resource containing the animation sheet.</param>
        /// <param name="frameDimensions">The size of a single frame, in pixels.</param>
        /// <param name="rowAndColumnAmount">The amount of rows and columns in the animation sheet.</param>
        /// <param name="visualCenter">The coordinates inside a single frame which serve as its 
        /// visual center.</param>
        /// <param name="isCyclic">Whether or not the animation should loop once the final frame is reached.</param>
        public Animation(string animationSheetPath, Point frameDimensions, Point rowAndColumnAmount,
            Vector2 visualCenter, bool isCyclic)
            : this(frameDimensions, rowAndColumnAmount, visualCenter, isCyclic)
        {
            this.animationSheetPath = animationSheetPath;
        }

        /// <summary>
        /// Creates a new instance of the animation class.
        /// </summary>
        /// <param name="animationSheet">The texture resource containing the animation sheet.</param>
        /// <param name="frameDimensions">The size of a single frame, in pixels.</param>
        /// <param name="rowAndColumnAmount">The amount of rows and columns in the animation sheet.</param>
        /// <param name="visualCenter">The coordinates inside a single frame which serve as its 
        /// visual center.</param>
        /// <param name="isCyclic">Whether or not the animation should loop once the final frame is reached.</param>
        public Animation(Texture2D animationSheet, Point frameDimensions, Point rowAndColumnAmount,
            Vector2 visualCenter, bool isCyclic)
            : this(String.Empty, frameDimensions, rowAndColumnAmount, visualCenter, isCyclic)
        {
            AnimationSheet = animationSheet;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sourceAnimation">The animation a copy of which should be created.</param>
        public Animation(Animation sourceAnimation)
            : this(sourceAnimation.frameSize, sourceAnimation.rowAndColumnAmount, sourceAnimation.VisualCenter,
                sourceAnimation.isCyclic)
        {
            if (sourceAnimation.AnimationSheet == null)
            {
                animationSheetPath = sourceAnimation.animationSheetPath;
            }
            else
            {
                AnimationSheet = sourceAnimation.AnimationSheet;
                animationSheetPath = String.Empty;
            }
        }

        /// <summary>
        /// Loads the animation's animation sheet using the specified content manager. Call this method before
        /// attempting to render the animation, unless you specified the animation sheet directly while creating the
        /// instance.
        /// </summary>
        /// <param name="contentManager">The content manager to use for loading the animation's sheet.</param>
        public void LoadSheet(ContentManager contentManager)
        {
            if (String.IsNullOrEmpty(animationSheetPath))
            {
                throw new ArgumentException("Cannot load the animation sheet from a null or empty path. " +
                    "Did you supply the animation sheet directly and call this method unintentionally?");
            }
            AnimationSheet = contentManager.Load<Texture2D>(animationSheetPath);
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Updates the animation's progress.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="isInMotion">Whether or not the animation element itself is
        /// currently in motion.</param>
        public void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                // See if it is time to advance to the next frame
                if (frameChangeTimer >= frameChangeInterval)
                {
                    frameChangeTimer = TimeSpan.Zero;

                    currentFrame.X++;
                    if (currentFrame.X >= rowAndColumnAmount.X)
                    {
                        currentFrame.X = 0;
                        currentFrame.Y++;
                    }

                    if (FrameIndex >= FrameCount)
                    {
                        if (isCyclic)
                        {
                            FrameIndex = 0; // Reset the animation
                        }
                        else
                        {
                            IsActive = false;
                            FrameIndex = FrameCount - 1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the current animation frame.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch with which the current frame will be rendered.</param>
        /// <param name="position">The position to draw the current frame.</param>
        /// <exception cref="System.NullReferenceException">Drawing was attempted before calling 
        /// <see cref="LoadSheet"/>.</exception>
        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            Draw(spriteBatch, position, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draws the current animation frame.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch with which the current frame will be rendered.</param>
        /// <param name="position">The position to draw the current frame.</param>
        /// <param name="rotation">Degree in radians by which to rotate the current frame. The default is 0.</param>
        /// <param name="origin">The drawing origin relative to a single frame. The default is the frame's upper
        /// left corner.</param>
        /// <param name="scale">Scale factor to apply to the current frame. The default is 1.</param>
        /// <param name="spriteEffect">SpriteEffect to apply to the current frame. By default, no effects are 
        /// applied.</param>
        /// <param name="layerDepth">The depth of a layer. By default, 0 represents the front layer and 1 represents
        /// a back layer. Use SpriteSortMode if you want sprites to be sorted during drawing. 0 is the default.</param>
        /// <exception cref="System.NullReferenceException">Drawing was attempted before calling 
        /// <see cref="LoadSheet"/>.</exception>
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 origin, float scale,
            SpriteEffects spriteEffect, float layerDepth)
        {
            spriteBatch.Draw(AnimationSheet, position,
                new Rectangle(frameSize.X * currentFrame.X, frameSize.Y * currentFrame.Y, frameSize.X, frameSize.Y),
                Color.White, rotation, origin, scale, spriteEffect, layerDepth);
        }

        /// <summary>
        /// Causes the animation to start playing from a specified frame index.
        /// </summary>
        /// <param name="frameIndex">Frame index to play the animation from.</param>
        public void PlayFromFrameIndex(int frameIndex)
        {
            FrameIndex = frameIndex;
            IsActive = true;
        }

        /// <summary>
        /// Used to set the interval between frames.
        /// </summary>
        /// <param name="interval">The interval between frames.</param>
        public void SetFrameInterval(TimeSpan interval)
        {
            frameChangeInterval = interval;
        }


        #endregion
    }
}
