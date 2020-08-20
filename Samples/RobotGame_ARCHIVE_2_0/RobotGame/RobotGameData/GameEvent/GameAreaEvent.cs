#region File Description
//-----------------------------------------------------------------------------
// GameAreaEvent.cs
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
using RobotGameData;
#endregion

namespace RobotGameData.GameEvent
{
    /// <summary>
    /// When a particular scene node comes into a particular area,
    /// this event is executed.
    /// </summary>
    public class GameAreaEvent : GameEventBase
    {
        #region Fields

        protected Vector3 actionPosition = Vector3.Zero;
        protected float actionRadius = 0.0f;

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">event type</param>
        /// <param name="position">position</param>
        /// <param name="radius">area radius</param>
        /// <param name="owner">event owner</param>
        /// <param name="visibledOwner">owner visible flag</param>
        public GameAreaEvent(Vector3 position, float radius,
                              GameSceneNode owner, bool visibledOwner)
        {
            SetAction(position, radius, owner, visibledOwner);
        }

        /// <summary>
        /// Set area event
        /// </summary>
        /// <param name="position">The position of this event</param>
        /// <param name="radius">The radius of this event</param>
        /// <param name="owner">The scene owner of this event</param>
        /// <param name="visibledOwner">
        /// Visibility of the scene owner before start this event
        /// </param>
        public void SetAction(Vector3 position, float radius,
                              GameSceneNode owner, bool visibledOwner)
        {
            this.actionPosition = position;
            this.actionRadius = radius;

            this.owner = owner;
            this.owner.Enabled = false;
            this.owner.Visible = visibledOwner;
        }

        /// <summary>
        /// If the distance between the "targetPosition" and the selected 
        ///  action position is short, the status of the game changes so that 
        ///  an event might occur.  In this case, it returns "true".
        /// </summary>
        public bool IsExecuteAction(Vector3 targetPosition)
        {
            float distance = Vector3.Distance(targetPosition, this.actionPosition);

            if (distance <= this.actionRadius)
                return true;

            return false;
        }

        /// <summary>
        /// It activates event.
        /// scene owner gets reset and starts activating.
        /// </summary>
        public override void ExecuteAction()
        {
            this.owner.Reset(true);

            this.owner.Enabled = true;
            this.owner.Visible = true;

            waitingAction = false;
            finishedAction = true;
        }
    }

}
