#region File Description
//-----------------------------------------------------------------------------
// GameNode.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData
{
    /// <summary>
    /// GameNode is node for update itself.
    /// When the framework gets update, all registered nodes’ OnUpdate function
    /// is automatically called.  When GameNode’s “Enable” is set to false, 
    /// it doesn’t get updated at the framework.
    /// </summary>
    public class GameNode : NodeBase, IGameComponent
    {
        #region Fields

        /// <summary>
        /// If not enabled, the child as well as itself will not get updated.
        /// </summary>
        bool enabled = true;

        public event EventHandler EnabledChanged;

        #endregion

        #region Properties

        public bool Enabled 
        {            
            get { return enabled; }
            set 
            { 
                enabled = value;

                if (EnabledChanged != null)
                    EnabledChanged(this, null);
            } 
        }

        #endregion
        
        /// <summary>
        /// It initializes itself and the child nodes.
        /// </summary>
        public virtual void Initialize()
        {
            if (childList != null)
            {
                for (int i = 0; i < childList.Count; i++)
                {
                    GameNode gameNode = childList[i] as GameNode;
                    if (gameNode != null)
                    {
                        gameNode.Initialize();
                    }
                }
            }
        }

        /// <summary>
        /// It updates itself and the child nodes.
        /// When "Enabled" is set to false, it will not execute.
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            //  If not activated, it won’t get updated.
            if (Enabled )
            {
                //  Update itself
                OnUpdate(gameTime);

                //  Update each child node
                if (childList != null)
                {
                    for (int i = 0; i < childList.Count; i++)
                    {
                        GameNode gameNode = childList[i] as GameNode;
                        if ((gameNode != null) && gameNode.Enabled)
                        {
                            gameNode.Update(gameTime);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reset the game node
        /// when parameter is set to true, it will also call child nodes.
        /// </summary>
        public void Reset(bool isRecursive)
        {
            //  first self
            OnReset();

            if (isRecursive )
            {
                //  child
                if (childList != null)
                {
                    for (int i = 0; i < childList.Count; i++)
                    {
                        GameNode gameNode = childList[i] as GameNode;
                        if (gameNode != null)
                        {
                            gameNode.Reset(isRecursive);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// When Reset() function is executed, it will be called automatically.  
        /// It gets overridden and defined externally.
        /// When the parent is called, the child also gets called.
        /// </summary>
        protected virtual void OnReset() { }

        /// <summary>
        /// When Update() function is executed, it will be called automatically.  
        /// It gets overridden and defined externally.
        /// When the parent is called, the child also gets called.
        /// </summary>
        protected virtual void OnUpdate(GameTime gameTime) { }        
    }
}


