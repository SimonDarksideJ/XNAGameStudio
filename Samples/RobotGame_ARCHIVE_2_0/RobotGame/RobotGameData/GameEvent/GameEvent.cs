#region File Description
//-----------------------------------------------------------------------------
// GameEvent.cs
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
#endregion

namespace RobotGameData.GameEvent
{
    /// <summary>
    /// This is the base class of all the events in the game.
    /// </summary>
    public abstract class GameEventBase
    {
        #region Fields

        protected GameSceneNode owner = null;

        protected float localTime = 0.0f;
        protected bool waitingAction = true;
        protected bool finishedAction = false;

        #endregion

        #region Properties

        public GameSceneNode Owner
        {
            get { return Owner; }
        }

        public float LocalTime
        {
            get { return localTime; }
        }

        public bool IsWatingAction
        {
            get { return waitingAction; }
        }

        public bool IsFinishedAction
        {
            get { return finishedAction; }
        }

        #endregion

        /// <summary>
        /// Update local time of this event
        /// </summary>
        public void Update(GameTime gameTime)
        {
            localTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public virtual void ExecuteAction() { }
    }
}
