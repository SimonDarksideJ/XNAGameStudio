#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace FuzzyLogic
{
    /// <summary>
    /// The Tank class, unsurprisingly enough, represents the tank, which chases after
    /// the mice. This class contains most of the interesting logic in this sample, 
    /// including the fuzzy decision making process.
    /// </summary>
    class Tank : Entity
    {
        #region Constants

        // How fast can the tank move?
        public override float MaxSpeed
        {
            get { return 2f; }
        }

        // How fast can he turn?
        public override float TurnSpeed
        {
            get { return .075f; }
        }

        // What texture should Entity use for the tank?
        public override string TextureFile
        {
            get { return "Tank"; }
        }

        // At what distance will the tank have "caught" the mouse?
        public const float CaughtDistance = 30.0f;


        // As discussed in the accompanying doc, these next three pairs of constants
        // are the minimum and maximum values for the fuzzy logic calculations.

        const float MinDistance = 0.0f;
        const float MaxDistance = 175.0f;

        const float MinAngle = 0.0f;
        const float MaxAngle = MathHelper.PiOver2;

        static readonly TimeSpan MinTime = TimeSpan.Zero;
        static readonly TimeSpan MaxTime = TimeSpan.FromSeconds(4.0f);

        #endregion

        #region Properties

        // These next three properties get and set different weights, which control how
        // the tank will make its decision when performing fuzzy logic. There is a
        // weight for each factor the tank has to consider when selecting a mouse: 
        // the distance to the mouse, the angle to the mouse, and how long it has been
        // chasing this mouse. By adjusting these weights, you can change what kind of 
        // mice the tank will prefer to chase. This process is discussed in more detail
        // in the accompanying doc.

        // This value will make the tank prefer to chase after mice which are nearby.
        // If this value is 0, the distance will not be a factor when.
        public float FuzzyDistanceWeight
        {
            get { return fuzzyDistanceWeight; }
            set
            {
                fuzzyDistanceWeight = MathHelper.Clamp(value, 0, 1);
            }
        }
        private float fuzzyDistanceWeight = .5f;

        // This value will make the tank prefer to chase after the mice that it is 
        // already chasing after. If this value is 1, the tank never "change its mind"
        // about which mouse to chase: once it picks a target it will stay with it until
        // it either catches it, or it gets away. If this value is 0, the tank will 
        // appear to be very indecisive, and will constantly change targets.
        public float FuzzyTimeWeight
        {
            get { return fuzzyTimeWeight; }
            set
            {
                fuzzyTimeWeight = MathHelper.Clamp(value, 0, 1);
            }
        }
        private float fuzzyTimeWeight = .5f;

        // This value will make the tank prefer to chase after the mice that are 
        // directly in front of it. If this value is 1, the tank will select its target
        // based only on how much it will have to turn to catch it. If this value is 0,
        // the angle to the target will not be a factor.
        public float FuzzyAngleWeight
        {
            get { return fuzzyAngleWeight; }
            set
            {
                fuzzyAngleWeight = MathHelper.Clamp(value, 0, 1);
            }
        }
        private float fuzzyAngleWeight = .5f;


        #endregion

        #region Private Fields

        // We'll need to keep track of the list of mice that we can chase,...
        private List<Mouse> mice;

        // ... who we're currently chasing after...
        private Mouse currentlyChasingMouse;

        // ... and how long we've been chasing him.
        private TimeSpan timeChasingThisMouse;

        #endregion

        #region Initialization

        public Tank(Rectangle levelBoundary, List<Mouse> mice)
            : base(levelBoundary)
        {
            this.mice = mice;
            this.Position = new Vector2(levelBoundary.Width / 2, levelBoundary.Height / 2);
        }

        #endregion

        #region Update

        /// <summary>
        /// ChooseBehavior is overriden from entity and will use fuzzy logic to
        /// determine how the tank should act. Which mouse should it chase? or should
        /// it wander around?
        /// </summary>
        protected override void ChooseBehavior(GameTime gameTime)
        {
            // Use fuzzy logic to choose which mouse to chase.            
            Mouse nextMouse = ChooseMouse();

            // If ChooseMouse returned null, that means that there werent any suitable
            // mice in range. we should wander around.
            if (nextMouse == null)
            {
                // if we're wandering, the tank shouldn't be highlighted. 
                IsHighlighted = false;

                if (currentlyChasingMouse != null)
                {
                    currentlyChasingMouse.IsHighlighted = false;
                    currentlyChasingMouse = null;
                }

                if (!(CurrentBehavior is WanderBehavior))
                {
                    CurrentBehavior = new WanderBehavior(this);
                }
            }
            // Ok, nextMouse isn't null. Is it a new mouse?
            else if (nextMouse != currentlyChasingMouse)
            {
                // We're going to start chasing after someone, highlight the tank and
                // reset the chasing timer.
                IsHighlighted = true;
                timeChasingThisMouse = TimeSpan.Zero;

                // Unhighlight the old prey...
                if (currentlyChasingMouse != null)
                {
                    currentlyChasingMouse.IsHighlighted = false;
                }

                // ...change to the new one ....
                currentlyChasingMouse = nextMouse;

                // .... and highlight him.
                nextMouse.IsHighlighted = true;

                // Finally, set our new behavior to chase after our new prey.
                CurrentBehavior = new ChaseBehavior(this, currentlyChasingMouse);
            }
            // If we hit this, we're still chasing the same mouse. All we have to do is
            // update the "timeChasingThisMouse" timer.
            else
            {
                timeChasingThisMouse += gameTime.ElapsedGameTime;
            }
        }

        /// <summary>
        /// ChooseMouse contains the logic at the heart of this sample: the fuzzy logic
        /// calcuations that will make the tank choose its next target mouse. for an 
        /// in depth explanation, see the accompanying doc.
        /// </summary>
        private Mouse ChooseMouse()
        {
            // In order to decide which mouse to chase, we'll loop over all of them and
            // see which is the best choice.
            Mouse bestMouse = null;
            float bestFuzzyValue = 0.0f;

            for (int i = 0; i < mice.Count; i++)
            {
                // Calculate the distance to the mouse. if it's greater than the max
                // distance, the tank can't "see" this mouse and we should move on to
                // the next.
                float distance = Vector2.Distance(Position, mice[i].Position);
                if (distance > MaxDistance)
                {
                    continue;
                }

                // This variable will store the fuzzy value for this mouse: in other
                // words, "how good of a choice" this mouse is. the tank will prefer
                // mice with a fuzzy value that is close to 1.0. The fuzzy value is
                // based on three factors: distance, angle, and time.
                float fuzzy = 0.0f;

                // First, we'll use the distance to the mouse that we computed earlier
                // to add in the fuzzy distance value. This is a value that ranges from 
                // 0 to 1, and increases as the tank gets closer to the mouse.
                fuzzy += CalculateFuzzyDistance(distance) * FuzzyDistanceWeight;


                // Next, we'll calculate the angle to the mouse, and use that to get
                // a value that starts at 0 and increases towards 1 as the angle to the
                // mouse diminishes. This will make the tank prefer mice that are
                // already in front of him.
                fuzzy += CalculateFuzzyAngle(i) * FuzzyAngleWeight;


                // The final value that we'll include is time. We want the tank to 
                // prefer the mouse he is already changing. At a minimum, this can be 
                // used to prevent hysterisis, but it can also be used to make the tank
                // behave more tenaciously; focusing on one mouse even if a "better" 
                // choice appears.
                fuzzy += CalculateFuzzyTime(i) * FuzzyTimeWeight;


                // Now that we know how good of a choice this mouse is, is it better
                // than the best one we've found so far?
                if (fuzzy > bestFuzzyValue)
                {
                    bestMouse = mice[i];
                    bestFuzzyValue = fuzzy;
                }
            }
            return bestMouse;
        }

        /// <summary>
        /// Calculates a value from 0 to 1 that represents how "close" a mouse is.
        /// </summary>
        private float CalculateFuzzyDistance(float distance)
        {
            return (1 - ((distance - MinDistance) / (MaxDistance - MinDistance)));
        }

        /// <summary>
        /// Returns a value from 0 to 1 that is based on the tank's orientation, and
        /// the angle to the mouse. If the tank is facing towards the mouse, the value 
        /// will be closer to 1. If the tank is facing away from the mouse, the value
        /// will be closer to 0.
        /// </summary>
        private float CalculateFuzzyAngle(int i)
        {
            // to calculate this value, first we need to find the angle to the
            // mouse...
            Vector2 toMouse = mice[i].Position - Position;
            float angleToMouse = (float)Math.Atan2(toMouse.Y, toMouse.X);

            // and then find the difference between that angle and the tank's
            // current orientation.
            float angleDifference =
                Math.Abs(Behavior.WrapAngle(Orientation - angleToMouse));

            // Calculate fuzzyAngle, which should range from 0 to 1 based on 
            // angleDifference. We'll need to clamp it explicitly to that range.
            float fuzzyAngle =
                (1 - ((angleDifference - MinAngle) / (MaxAngle - MinAngle)));
            return MathHelper.Clamp(fuzzyAngle, 0, 1);
        }

        /// <summary>
        /// Returns a value from 0 to 1 that represents how long the tank has been
        /// chasing a mouse. If the value is 0, the tank isn't currently chasing the
        /// mouse, or just started to. If the value is 1, the tank has been chasing this
        /// mouse for a long time.
        /// </summary>
        private float CalculateFuzzyTime(int i)
        {
            // To calcuate the fuzzy time value, first we figure out how long we've
            // been chasing this mouse. For most mice, this will of course be no
            // time at all.
            TimeSpan time = TimeSpan.Zero;
            if (mice[i] == currentlyChasingMouse)
            {
                time = timeChasingThisMouse;
            }

            // Next we calculate fuzzyTime, which is a value ranging from 0 to 1. It
            // will increase as the amount of time we have spent chasing this mouse
            // increases. The value must be clamped to enforce the 0 to 1 rule.
            float fuzzyTime = (float)((time - MinTime).TotalSeconds
                / (MaxTime - MinTime).TotalSeconds);
            return MathHelper.Clamp(fuzzyTime, 0, 1.0f);
        }

        #endregion

    }
}
