#region File Description
//-----------------------------------------------------------------------------
// Mouse.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endregion

namespace FuzzyLogic
{
    /// <summary>
    /// The mouse is very basic, and will simply wander around unless the tank gets too
    /// close. If the tank gets too close, he will flee.
    /// </summary>
    class Mouse : Entity
    {
        #region Constants

        // Controls the distance at which the mouse will flee from tank.
        const float MouseEvadeDistance = 125.0f;

        // used to avoid hysteresis when trying to decide whether or not to flee
        const float MouseHysteresis = 45.0f;

        // how fast can the mouse move?
        public override float MaxSpeed
        {
            get { return 4.25f; }
        }

        // and how fast can it turn?
        public override float TurnSpeed
        {
            get { return .2f; }
        }

        // what texture should Entity use for the mouse?
        public override string TextureFile
        {
            get { return "Mouse"; }
        }

        #endregion

        // keep track of the tank, so we know when to run away
        private Tank tank;

        // we'll need a random number generator to randomly place new mice on the
        // screen.
        static Random random = new Random();

        // the constructor takes in the variables that the mouse needs to store, and
        // starts the mouse to wandering.
        public Mouse(Rectangle levelBoundary, Tank tank)
            : base(levelBoundary)
        {
            Position = new Vector2(
                random.Next(levelBoundary.X, levelBoundary.X + levelBoundary.Width),
                random.Next(levelBoundary.Y, levelBoundary.Y + levelBoundary.Height));

            CurrentBehavior = new WanderBehavior(this);
            this.tank = tank;
        }

        /// <summary>
        /// ChooseBehavior is overriden from Entity, and will determine what the mouse
        /// should do on this update.
        /// </summary>
        protected override void ChooseBehavior(GameTime gameTime)
        {
            // the decision for what behavior to use is simple, and is based on the 
            // tank's position. if the tank is far away, we'll idle. If it gets too
            // close we'll flee.

            float distanceFromTank = Vector2.Distance(Position, tank.Position);

            // The tank is a safe distance away, so the mouse should idle:
            if (!(CurrentBehavior is WanderBehavior) &&
                distanceFromTank > MouseEvadeDistance + MouseHysteresis)
            {
                CurrentBehavior = new WanderBehavior(this);
            }

            // The tank is too close; the mouse should run:
            else if (!(CurrentBehavior is EvadeBehavior) &&
                distanceFromTank < MouseEvadeDistance - MouseHysteresis)
            {
                CurrentBehavior = new EvadeBehavior(this, tank);
            }
        }

    }
}
