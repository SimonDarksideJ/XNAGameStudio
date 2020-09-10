#region File Description
//-----------------------------------------------------------------------------
// FrameRateCounter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace ModelViewerDemo
{
    /// <summary>
    /// Helper class that measures updates per second and draws (frames) per second.
    /// </summary>
    public class FrameRateCounter
    {
        int updateCount = 0;
        TimeSpan updateAccum = TimeSpan.Zero;
        
        int frameCount = 0;
        TimeSpan frameAccum = TimeSpan.Zero;

        /// <summary>
        /// Gets a string indicating the updates per second.
        /// </summary>
        public string UpdatesPerSecond { get; private set; }

        /// <summary>
        /// Gets a string indicating the frames per second.
        /// </summary>
        public string FramesPerSecond { get; private set; }

        public FrameRateCounter()
        {
            UpdatesPerSecond = "UPS: ";
            FramesPerSecond = "FPS: ";
        }

        /// <summary>
        /// Call this from your update to measure updates per second.
        /// </summary>
        public void OnUpdate(TimeSpan elapsed)
        {
            updateCount++;

            updateAccum += elapsed;
            if (updateAccum >= TimeSpan.FromSeconds(1))
            {
                int ups = (int)Math.Round((double)updateCount / updateAccum.TotalSeconds);
                UpdatesPerSecond = string.Format("UPS: {0}", ups);

                updateCount = 0;
                updateAccum = TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Call this from your draw to measure draws (frames) per second.
        /// </summary>
        public void OnDraw(TimeSpan elapsed)
        {
            frameCount++;

            frameAccum += elapsed;
            if (frameAccum >= TimeSpan.FromSeconds(1))
            {
                int fps = (int)Math.Round((double)frameCount / frameAccum.TotalSeconds);
                FramesPerSecond = string.Format("FPS: {0}", fps);

                frameCount = 0;
                frameAccum = TimeSpan.Zero;
            }
        }
    }
}
