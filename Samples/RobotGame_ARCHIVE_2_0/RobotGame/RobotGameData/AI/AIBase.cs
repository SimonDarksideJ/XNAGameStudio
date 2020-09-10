#region File Description
//-----------------------------------------------------------------------------
// AIBase.cs
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
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData.AI
{
    /// <summary>
    /// s is the base class for A.I.
    /// It contains the event hander functions regarding Start, Update, and Finish.
    /// </summary>
    public class AIBase : INamed
    {
        #region Fields

        string name = String.Empty;
        float activeTime = 0.0f;

        #endregion

        #region Events

        public class AIUpdateEventArgs : EventArgs
        {
            private GameTime gameTime;
            public GameTime GameTime
            {
                get { return gameTime; }
            }

            public AIUpdateEventArgs(GameTime gameTime)
                : base()
            {
                this.gameTime = gameTime;
            }
        }

        public event EventHandler<AIUpdateEventArgs> Updating;
        public event EventHandler Starting;
        public event EventHandler Finishing;

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public float ActiveTime
        {
            get { return activeTime; }
            set { activeTime = value; }
        }

        public bool IsActive
        {
            get { return (activeTime > 0.0f); }
        }

        #endregion

        /// <summary>
        /// Reset the A.I.
        /// </summary>
        public void Reset()
        {
            ActiveTime = 0.0f;
        }

        /// <summary>
        /// Updates the A.I.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            AIUpdate(gameTime);
        }

        /// <summary>
        /// As the A.I. gets updated, the event function always gets called.
        /// </summary>
        public void AIUpdate(GameTime gameTime)
        {
            if (Updating != null)
            {
                Updating(this, new AIUpdateEventArgs(gameTime));
            }
        }

        /// <summary>
        /// As the A.I. starts, the registered event function gets called right away.
        /// </summary>
        public void AIStart()
        {
            if (Starting != null)
            {
                Starting(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// As the A.I. ends, the registered event function gets called right away.
        /// </summary>
        public void AIFinish()
        {
            if (Finishing != null)
            {
                Finishing(this, EventArgs.Empty);
            }
        }
    }
}
