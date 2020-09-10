#region File Description
//-----------------------------------------------------------------------------
// GameEventManager.cs
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
    /// It supports the time and area event, which is needed by the game, 
    /// and manages the registered events.
    /// GameTimeEvent class, after a specific amount of time, 
    /// activates the registered scene object (enable and visible).
    /// GameAreaEvent class activates the registered scene object (enable and visible) 
    /// when a target object comes within a specific area.
    /// </summary>
    public class GameEventManager
    {
        #region Fields

        bool enable = true;
        GameSceneNode targetScene = null;
        List<GameEventBase> gameEventList = new List<GameEventBase>();

        #endregion

        #region Properties

        public bool Enable
        {
            get { return enable; }
            set { enable = value; }
        }

        public GameSceneNode TargetScene
        {
            get { return targetScene; }
            set { targetScene = value; }
        }

        #endregion
        
        /// <summary>
        /// Update all events in the list
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (enable == false) return;

            if (gameEventList.Count > 0)
            {
                //  Update game events
                for (int i = 0; i < gameEventList.Count; i++)
                {
                    GameEventBase gameEvent = gameEventList[i];

                    //  Every "gameEvent" keeps getting updated and gets put on a hold 
                    //  until the action is executable.
                    if (gameEvent.IsWatingAction )
                    {
                        gameEvent.Update(gameTime);

                        if (gameEvent is GameTimeEvent)
                        {
                            GameTimeEvent timeEvent = gameEvent as GameTimeEvent;

                            //  If the status is in executable position, 
                            //  each event will be executed.
                            if (timeEvent.IsExecuteAction() )
                            {
                                //  Executes the time event
                                timeEvent.ExecuteAction();
                            }
                        }
                        else if (gameEvent is GameAreaEvent)
                        {
                            GameAreaEvent areaEvent = gameEvent as GameAreaEvent;

                            //  If the status is in executable position, 
                            //  each event will be executed.
                            if (areaEvent.IsExecuteAction(targetScene.Position) )
                            {
                                areaEvent.ExecuteAction();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new game event to the event manager
        /// </summary>
        public void AddEvent(GameEventBase gameEvent)
        {
            gameEventList.Add(gameEvent);
        }

        /// <summary>
        /// Removes a game event from the event manager
        /// </summary>
        public void RemoveEvent(GameEventBase gameEvent)
        {
            gameEventList.Remove(gameEvent);
        }

        /// <summary>
        /// Removes a game event by the index number from the event manager
        /// </summary>
        public void RemoveEvent(int index)
        {
            gameEventList.RemoveAt(index);
        }

        /// <summary>
        /// Clear all game events from the event manager
        /// </summary>
        public void ClearAllEvent()
        {
            gameEventList.Clear();
        }
    }
}
