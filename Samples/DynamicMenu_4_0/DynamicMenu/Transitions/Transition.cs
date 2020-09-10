#region File Information
//-----------------------------------------------------------------------------
// Transitions.cs
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
using DynamicMenu.Controls;
#endregion

namespace DynamicMenu.Transitions
{
    /// <summary>
    /// Transitions are used to apply effects to controls.  Transitions can be used to resize
    /// controls, adjust their position, or change their coloring for an adjustable period of
    /// time.
    /// To use a transition, instantiate one directly or use one of the creation methods, then
    /// use the Control's ApplyTransition method to start it.  If you wish to be informed when
    /// the transition is complete, register a callback for the TransitionComplete event before
    /// using ApplyTransition.
    /// </summary>
    public class Transition
    {
        #region Fields

        private bool startPositionSet = false;
        private bool endPositionSet = false;
        private bool startSizeSet = false;
        private bool endSizeSet = false;
        private bool startHueSet = false;
        private bool endHueSet = false;

        private Point startPosition = new Point();
        private Point endPosition = new Point();

        private Point startSize = new Point();
        private Point endSize = new Point();

        private Color startHue = new Color();
        private Color endHue = new Color();

        private IControl control = null;
        private float transitionLength = 1.0f;

        private bool transitionActive = false;
        private double transitionStartTime = 0;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a transition is complete
        /// </summary>
        public event EventHandler TransitionComplete;

        #endregion

        #region Properties

        /// <summary>
        /// The control this transition is being applied to
        /// </summary>
        public IControl Control
        {
            get { return control; }
            set { control = value; }
        }

        /// <summary>
        /// The starting position for the top left corner of the control.  This is an optional parameter.
        /// </summary>
        public Point StartPosition
        {
            get { return startPosition; }
            set
            {
                startPositionSet = true;
                startPosition = value;
            }
        }

        /// <summary>
        /// The ending position for the top left corner of the control.  This is an optional parameter.
        /// </summary>
        public Point EndPosition
        {
            get { return endPosition; }
            set
            {
                endPositionSet = true;
                endPosition = value;
            }
        }

        /// <summary>
        /// The starting width and height of the control.  This is an optional parameter.
        /// </summary>
        public Point StartSize
        {
            get { return startSize; }
            set
            {
                startSizeSet = true;
                startSize = value;
            }
        }

        /// <summary>
        /// The ending width and height of the control.  This is an optional parameter.
        /// </summary>
        public Point EndSize
        {
            get { return endSize; }
            set
            {
                endSizeSet = true;
                endSize = value;
            }
        }

        /// <summary>
        /// The starting color of the control.  This is an optional parameter.
        /// </summary>
        public Color StartColor
        {
            get { return startHue; }
            set
            {
                startHueSet = true;
                startHue = value;
            }
        }

        /// <summary>
        /// The ending color of the control.  This is an optional parameter.
        /// </summary>
        public Color EndColor
        {
            get { return endHue; }
            set
            {
                endHueSet = true;
                endHue = value;
            }
        }

        /// <summary>
        /// The length of time in seconds to play the transition over.
        /// </summary>
        public float TransitionLength
        {
            get { return transitionLength; }
            set { transitionLength = value; }
        }

        /// <summary>
        /// Whether the transition is currently being played.
        /// </summary>
        public bool TransitionActive
        {
            get { return transitionActive; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begin applying the transition to the target control
        /// </summary>
        public void StartTranstion()
        {
            transitionActive = true;
            transitionStartTime = 0;
            if (startPositionSet)
            {
                control.Left = startPosition.X;
                control.Top = startPosition.Y;
            }
            else
            {
                startPosition.X = control.Left;
                startPosition.Y = control.Top;
            }
            if (startSizeSet)
            {
                control.Width = startSize.X;
                control.Height = startSize.Y;
            }
            else
            {
                startSize.X = control.Width;
                startSize.Y = control.Height;
            }
            if (startHueSet)
            {
                control.Hue = startHue;
            }
            else
            {
                startHue = control.Hue;
            }

            if (!endPositionSet)
            {
                endPosition.X = control.Left;
                endPosition.Y = control.Top;
            }

            if (!endSizeSet)
            {
                endSize.X = control.Width;
                endSize.Y = control.Height;
            }

            if (!endHueSet)
            {
                endHue = control.Hue;
            }
        }

        /// <summary>
        /// Create a transition with the specified starting and ending parameters.  Each of these parameters
        /// is optional, with a null indicating that that parameter will not be changed with this transition.
        /// </summary>
        /// <param name="startPosition">The starting position for the top left corner of the control</param>
        /// <param name="endPosition">The ending position for the top left corner of the control</param>
        /// <param name="startSize">The starting width and height of the control</param>
        /// <param name="endSize">The ending width and height of the control</param>
        /// <param name="startColor">The starting color of the control</param>
        /// <param name="endColor">The ending color of the control</param>
        public Transition(Point? startPosition, Point? endPosition, Point? startSize, Point? endSize, 
            Color? startColor, Color? endColor)
        {
            if (startPosition.HasValue) StartPosition = startPosition.Value;
            if (endPosition.HasValue) EndPosition = endPosition.Value;
            if (startSize.HasValue) StartSize = startSize.Value;
            if (endSize.HasValue) EndSize = endSize.Value;
            if (startColor.HasValue) StartColor = startColor.Value;
            if (endColor.HasValue) EndColor = endColor.Value;
        }

        /// <summary>
        /// Increment the trasition between the starting and ending state of the control.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (transitionActive)
            {
                // Set the start time if it hasn't been set yet
                if (transitionStartTime == 0)
                {
                    transitionStartTime = gameTime.TotalGameTime.TotalSeconds;
                }

                float timeSinceStart = (float)(gameTime.TotalGameTime.TotalSeconds - transitionStartTime);
                float percentComplete = timeSinceStart / transitionLength;

                // We've reached the end
                if (percentComplete > 1.0)
                {
                    control.Left = endPosition.X;
                    control.Top = endPosition.Y;
                    control.Width = endSize.X;
                    control.Height = endSize.Y;
                    control.Hue = endHue;
                    transitionStartTime = 0;
                    transitionActive = false;
                    if (TransitionComplete != null)
                    {
                        TransitionComplete(this, new EventArgs());
                    }
                }
                else
                {
                    control.Left = (int)(startPosition.X + (endPosition.X - startPosition.X) * percentComplete);
                    control.Top = (int)(startPosition.Y + (endPosition.Y - startPosition.Y) * percentComplete);

                    control.Width = (int)(startSize.X + (endSize.X - startSize.X) * percentComplete);
                    control.Height = (int)(startSize.Y + (endSize.Y - startSize.Y) * percentComplete);

                    Vector4 curHue = startHue.ToVector4() + (endHue.ToVector4() - startHue.ToVector4()) * percentComplete;

                    control.Hue = new Color(curHue);
                }
            }
        }

        #endregion

        #region Transition creation methods

        /// <summary>
        /// Create a transition which fades in a control over the specified period of time.
        /// </summary>
        /// <param name="control">The control to apply the transition to</param>
        /// <param name="length">Amount of time to play this transition over.  
        ///   This is an optional parameter.</param>
        /// <returns>The created transition</returns>
        public static Transition CreateFadeIn(IControl control, float? length)
        {
            Color startHue = control.Hue;
            Color endHue = control.Hue;

            startHue.A = 0;

            Transition transition = new Transition(null, null, null, null, startHue, endHue);
            transition.Control = control;
            if (length.HasValue)
            {
                transition.TransitionLength = length.Value;
            }

            return transition;
        }

        /// <summary>
        /// Create a transition which fades out a control over the specified period of time.
        /// </summary>
        /// <param name="control">The control to apply the transition to</param>
        /// <param name="length">Amount of time to play this transition over.  
        ///   This is an optional parameter.</param>
        /// <returns>The created transition</returns>
        public static Transition CreateFadeOut(IControl control, float? length)
        {
            Color startHue = control.Hue;
            Color endHue = control.Hue;

            endHue.A = 0;

            Transition transition = new Transition(null, null, null, null, startHue, endHue);
            transition.Control = control;
            if (length.HasValue)
            {
                transition.TransitionLength = length.Value;
            }

            return transition;
        }

        /// <summary>
        /// Creates a transition which causes the control to fly into view from a specified starting point.
        /// </summary>
        /// <param name="control">The control to apply the transition to</param>
        /// <param name="startPos">The position of the top left corner at the start of the transition</param>
        /// <param name="length">Amount of time to play this transition over.  
        ///   This is an optional parameter.</param>
        /// <returns>The created transition</returns>
        public static Transition CreateFlyIn(IControl control, Point startPos, float? length)
        {
            Transition transition = new Transition(startPos, null, null, null, null, null);
            transition.Control = control;
            if (length.HasValue)
            {
                transition.TransitionLength = length.Value;
            }

            return transition;
        }

        /// <summary>
        /// Creates a transition which causes the control to fly out of view to a specified ending point.
        /// </summary>
        /// <param name="control">The control to apply the transition to</param>
        /// <param name="startPos">The position of the top left corner at the end of the transition</param>
        /// <param name="length">Amount of time to play this transition over.  
        ///   This is an optional parameter.</param>
        /// <returns>The created transition</returns>
        public static Transition CreateFlyOut(IControl control, Point endPos, float? length)
        {
            Transition transition = new Transition(null, endPos, null, null, null, null);
            transition.Control = control;
            if (length.HasValue)
            {
                transition.TransitionLength = length.Value;
            }

            return transition;
        }

        #endregion

    }
}
