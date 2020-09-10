#region File Description
//-----------------------------------------------------------------------------
// GameTimeEvent.cs
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
    /// When the set time is passed, this event gets executed.
    /// </summary>
    public class GameTimeEvent : GameEventBase
    {
        #region Fields

        protected float actionTime = 0.0f;   // action execute time

        #endregion

        #region Properties

        public float ActionTime
        {
            get { return actionTime; }
        }

        public bool IsExecuteAction()
        {
            return (localTime >= actionTime);
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">event type</param>
        /// <param name="time">event execute time</param>
        /// <param name="owner">event owner</param>
        /// <param name="visibledOwner">owner visible flag</param>
        public GameTimeEvent(float time, GameSceneNode owner, bool visibledOwner)
        {
            SetAction(time, owner, visibledOwner);
        }

        public void SetAction(float time, GameSceneNode owner, bool visibledOwner)
        {
            this.actionTime = time;
            this.owner = owner;

            this.owner.Enabled = false;
            this.owner.Visible = visibledOwner;
        }

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
