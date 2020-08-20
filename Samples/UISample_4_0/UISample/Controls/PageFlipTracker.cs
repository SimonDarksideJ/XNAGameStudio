//-----------------------------------------------------------------------------
// PageFlipTracker.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;
using System.Diagnostics;


namespace UserInterfaceSample.Controls
{
    /// <remarks>
    /// PageFlipTracker watches the touchpanel for drag and flick gestures, and computes the appropriate
    /// offsets for flipping horizontally through a multi-page display. It is used by PageFlipControl
    /// to handle the scroll logic. PageFlipTracker is broken out into a separate class so that XNA apps
    /// with their own scheme for UI controls can still use it to handle the scroll logic.
    ///
    /// Handling TouchPanel.EnabledGestures
    /// --------------------------
    /// This class watches for HorizontalDrag, DragComplete, and Flick gestures. However, it cannot just
    /// set TouchPanel.EnabledGestures, because that would most likely interfere with gestures needed
    /// elsewhere in the application. So it just exposes a const 'HandledGestures' field and relies on
    /// the client code to set TouchPanel.EnabledGestures appropriately.
    ///
    /// Handling screen rotation
    /// ------------------------
    /// This class uses TouchPanel.DisplayWidth to determine the width of the screen. DisplayWidth
    /// is automatically updated by the system when the orientation changes.
    /// </remarks>
    public class PageFlipTracker
    {
        public const GestureType GesturesNeeded = GestureType.Flick | GestureType.HorizontalDrag | GestureType.DragComplete;

        #region Tuning options

        public static TimeSpan FlipDuration = TimeSpan.FromSeconds(0.3);

        /// <summary>
        /// Exponent on curve to make page flips and springbacks start quickly and slow to a stop.
        ///
        /// Interpolation formula is (1-TransitionAlpha)^TransitionExponent, where
        /// TransitionAlpha animates uniformly from 0 to 1 over timespan FlipDuration.
        /// </summary>
        public static double FlipExponent = 3.0;

        /// <summary>
        /// By default, this many pixels of the next page will be visible
        /// on the right-hand edge of the screen, unless the current page's
        /// contentWidth is too large.
        /// </summary>
        public static int PreviewMargin = 20;

        /// <summary>
        /// How far (as a fraction of the total screen width) you have
        /// to drag a screen past its edge to trigger a flip by dragging.
        /// </summary>
        public static float DragToFlipTheshold = 1.0f / 3.0f;

        #endregion

        #region Private fields

        // Time stamp when transition started
        private DateTime flipStartTime;

        // Horizontal offset at start of current transition. Target offset is always 0.
        private float flipStartOffset;

        #endregion

        #region Properties

        // Current active page. If we're in a transition, this is the page we're transitioning TO.
        public int CurrentPage { get; private set; }

        // Offset in pixels to render currentPage at. If this is positive, other
        // pages may be visible to the left.
        //
        // This is always relative to the current page.
        public float CurrentPageOffset { get; private set; }

        public bool IsLeftPageVisible
        {
            get
            {
                return PageWidthList.Count >= 2 && CurrentPageOffset > 0;
            }
        }

        public bool IsRightPageVisible
        {
            get
            {
                return PageWidthList.Count >= 2 && CurrentPageOffset + EffectivePageWidth(CurrentPage) <= TouchPanel.DisplayWidth;
            }
        }

        // True if we're currently in a transition
        public bool InFlip { get; private set; }

        // Alpha value that animates from 0 to 1 during a spring. Will be 1 when not springing.
        public float FlipAlpha { get; private set; }

        // PageWidthList contains the width in pixels of each page. Pages can be added or removed at any time by
        // changing this list.
        public List<int> PageWidthList = new List<int>();

        #endregion

        public PageFlipTracker()
        {
        }

        public int EffectivePageWidth(int page)
        {
            int displayWidth = TouchPanel.DisplayWidth - PreviewMargin;
            return Math.Max(displayWidth, PageWidthList[page]);
        }

        // Update is called once per frame. 
        public void Update()
        {
            if (InFlip)
            {
                TimeSpan transitionClock = DateTime.Now - flipStartTime;
                if (transitionClock >= FlipDuration)
                {
                    EndFlip();
                }
                else
                {
                    double f = transitionClock.TotalSeconds / FlipDuration.TotalSeconds;
                    f = Math.Max(f, 0.0); // this shouldn't happen, but just in case time goes crazy
                    FlipAlpha = (float)(1 - Math.Pow(1 - f, FlipExponent));
                    CurrentPageOffset = flipStartOffset * (1 - FlipAlpha);
                }
            }
        }

        public void HandleInput(InputState input)
        {
            foreach (GestureSample sample in input.Gestures)
            {
                switch (sample.GestureType)
                {
                    case GestureType.HorizontalDrag:
                        CurrentPageOffset += sample.Delta.X;
                        flipStartOffset = CurrentPageOffset;
                        break;

                    case GestureType.DragComplete:
                        if (!InFlip)
                        {
                            if (CurrentPageOffset < -TouchPanel.DisplayWidth * DragToFlipTheshold)
                            {
                                // flip to next page
                                BeginFlip(1);
                            }
                            else if (CurrentPageOffset + TouchPanel.DisplayWidth * (1 - DragToFlipTheshold) > EffectivePageWidth(CurrentPage))
                            {
                                // flip to previous page
                                BeginFlip(-1);
                            }
                            else
                            {
                                // "snap back" effect when you drag a little and let go
                                BeginFlip(0);
                            }
                        }
                        break;

                    case GestureType.Flick:
                        // Only respond to mostly-horizontal flicks
                        if (Math.Abs(sample.Delta.X) > Math.Abs(sample.Delta.Y))
                        {
                            if (sample.Delta.X > 0)
                            {
                                BeginFlip(-1);
                            }
                            else
                            {
                                BeginFlip(1);
                            }
                        }
                        break;
                }
            }
        }

        void BeginFlip(int pageDelta)
        {
            if(PageWidthList.Count == 0)
                return;

            int pageFrom = CurrentPage;
            CurrentPage = (CurrentPage + pageDelta + PageWidthList.Count) % PageWidthList.Count;

            if (pageDelta > 0)
            {
                // going to next page; offset starts out large
                CurrentPageOffset += EffectivePageWidth(pageFrom);
            }
            else if(pageDelta < 0)
            {
                // going to previous page; offset starts out negative
                CurrentPageOffset -= EffectivePageWidth(CurrentPage);
            }
            InFlip = true;
            FlipAlpha = 0;
            flipStartOffset = CurrentPageOffset;
            flipStartTime = DateTime.Now;
        }

        // FIXME: private
        void EndFlip()
        {
            InFlip = false;
            FlipAlpha = 1;
            CurrentPageOffset = 0;
        }
    }
}

