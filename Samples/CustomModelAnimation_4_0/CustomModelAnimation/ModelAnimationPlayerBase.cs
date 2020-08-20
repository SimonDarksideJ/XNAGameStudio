#region File Description
//-----------------------------------------------------------------------------
// ModelAnimationPlayerBase.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CustomModelAnimation
{
    /// <summary>
    /// This class serves as a base class for various animation players.  It contains
    /// common functionality to deal with a clip, playing it back at a speed, 
    /// notifying clients of completion, etc.
    /// </summary>
    public abstract class ModelAnimationPlayerBase
    {
        // Clip currently being played
        ModelAnimationClip currentClipValue;

        // Current timeindex and keyframe in the clip
        TimeSpan currentTimeValue;
        int currentKeyframe;

        // Speed of playback
        float playbackRate = 1.0f;

        // The amount of time for which the animation will play.
        // TimeSpan.MaxValue will loop forever. TimeSpan.Zero will play once. 
        TimeSpan duration = TimeSpan.MaxValue;

        // Amount of time elapsed while playing
        TimeSpan elapsedPlaybackTime = TimeSpan.Zero;

        // Whether or not playback is paused
        bool paused;
        
        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public ModelAnimationClip CurrentClip
        {
            get { return currentClipValue; }
        }

        /// <summary>
        /// Get/Set the current key frame index
        /// </summary>
        public int CurrentKeyFrame
        {
            get { return currentKeyframe; }
            set
            {
                IList<ModelKeyframe> keyframes = currentClipValue.Keyframes;
                TimeSpan time = keyframes[value].Time;
                CurrentTimeValue = time;
            }
        }

        /// <summary>
        /// Gets/set the current play position.
        /// </summary>
        public TimeSpan CurrentTimeValue
        {
            get { return currentTimeValue; }
            set
            {
                TimeSpan time = value;

                // If the position moved backwards, reset the keyframe index.
                if (time < currentTimeValue)
                {
                    currentKeyframe = 0;
                    InitClip();
                }

                currentTimeValue = time;

                // Read keyframe matrices.
                IList<ModelKeyframe> keyframes = currentClipValue.Keyframes;

                while (currentKeyframe < keyframes.Count)
                {
                    ModelKeyframe keyframe = keyframes[currentKeyframe];

                    // Stop when we've read up to the current time position.
                    if (keyframe.Time > currentTimeValue)
                        break;

                    // Use this keyframe
                    SetKeyframe(keyframe);

                    currentKeyframe++;
                }
            }
        }

        /// <summary>
        /// Invoked when playback has completed.
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>        
        public void StartClip(ModelAnimationClip clip)
        {
            StartClip(clip, 1.0f, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Starts playing a clip
        /// </summary>
        /// <param name="clip">Animation clip to play</param>
        /// <param name="playbackRate">Speed to playback</param>
        /// <param name="duration">Length of time to play (max is looping, 0 is once)</param>
        public void StartClip(ModelAnimationClip clip, float playbackRate, TimeSpan duration)
        {
            if (clip == null)
                throw new ArgumentNullException("Clip required");

            // Store the clip and reset playing data            
            currentClipValue = clip;
            currentKeyframe = 0;
            CurrentTimeValue = TimeSpan.Zero;
            elapsedPlaybackTime = TimeSpan.Zero;
            paused = false;

            // Store the data about how we want to playback
            this.playbackRate = playbackRate;
            this.duration = duration;

            // Call the virtual to allow initialization of the clip
            InitClip();
        }

        /// <summary>
        /// Will pause the playback of the current clip
        /// </summary>
        public void PauseClip()
        {
            paused = true;
        }

        /// <summary>
        /// Will resume playback of the current clip
        /// </summary>
        public void ResumeClip()
        {
            paused = false;
        }

        /// <summary>
        /// Virtual method allowing subclasses to do any initialization of data when the clip is initialized.
        /// </summary>
        protected virtual void InitClip()
        {
        }

        /// <summary>
        /// Virtual method allowing subclasses to set any data associated with a particular keyframe.
        /// </summary>
        /// <param name="keyframe">Keyframe being set</param>
        protected virtual void SetKeyframe(ModelKeyframe keyframe)
        {
        }

        /// <summary>
        /// Virtual method allowing subclasses to perform data needed after the animation 
        /// has been updated for a new time index.
        /// </summary>
        protected virtual void OnUpdate()
        {
        }

        /// <summary>
        /// Called during the update loop to move the animation forward
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            if (currentClipValue == null)
                return;

            if (paused)
                return;

            TimeSpan time = gameTime.ElapsedGameTime;

            // Adjust for the rate
            if (playbackRate != 1.0f)
                time = TimeSpan.FromMilliseconds(time.TotalMilliseconds * playbackRate);

            elapsedPlaybackTime += time;

            // See if we should terminate
            if (elapsedPlaybackTime > duration && duration != TimeSpan.Zero ||
                elapsedPlaybackTime > currentClipValue.Duration && duration == TimeSpan.Zero)
            {
                if (Completed != null)
                    Completed(this, EventArgs.Empty);

                currentClipValue = null;

                return;
            }

            // Update the animation position.        
            time += currentTimeValue;

            // If we reached the end, loop back to the start.
            while (time >= currentClipValue.Duration)
                time -= currentClipValue.Duration;

            CurrentTimeValue = time;

            OnUpdate();
        }
    }
}
