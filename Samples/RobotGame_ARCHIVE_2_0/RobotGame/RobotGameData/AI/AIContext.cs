#region File Description
//-----------------------------------------------------------------------------
// AIContext.cs
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
using RobotGameData;
#endregion

namespace RobotGameData.AI
{
    /// <summary>
    /// This manages A.I.
    /// If an A.I. class is added, it will get updated every time when it gets played.
    /// When an A.I. gets played, the A.I.’s former Finish event handler, 
    /// which has not been replaced yet, will be called.  
    /// And, then, when the context gets updated, the A.I.’s current 
    /// Update event handler will be called.
    /// </summary>
    public class AIContext : GameNode
    {
        #region Fields

        /// <summary>
        /// If set to false, all of the related functions get turned off.
        /// </summary>
        bool activeOn = true;

        List<AIBase> aiList = new List<AIBase>();

        AIBase activeAI = null;
        AIBase nextAI = null;
        
        #endregion

        #region Properties

        public AIBase ActiveAI
        {
            get { return activeAI; }
        }

        public AIBase NextAI
        {
            get { return nextAI; }
        }

        #endregion

        #region Update

        /// <summary>
        /// It will process the A.I. which has been registered to the list.
        /// When the current A.I. is updated and then, when ActiveTime, 
        /// which is specified in the current A.I., 
        /// becomes zero, it will be replaced with the next-to-be-executed A.I.
        /// </summary>
        protected override void OnUpdate(GameTime gameTime)
        {
            if ((ActiveAI == null && NextAI == null) || !activeOn) return;

            //  Change to next AI
            if (ActiveAI == null)
            {
                PlayAI(NextAI, NextAI.ActiveTime);
            }
            else if (NextAI != null && ActiveAI.ActiveTime == 0.0f)
            {
                PlayAI(NextAI, NextAI.ActiveTime);
                nextAI = null;
            }
            else
            {
                //  Process to current AI
                if (ActiveAI != null)
                {
                    ActiveAI.Update(gameTime);

                    if (ActiveAI.ActiveTime > 0.0f)
                        ActiveAI.ActiveTime -= 
                                    (float)gameTime.ElapsedGameTime.TotalSeconds;
                    
                    if( ActiveAI.ActiveTime < 0.0f)
                        ActiveAI.ActiveTime = 0.0f;
                }
            }
        }

        #endregion

        #region A.I. Controls

        /// <summary>
        /// By using the index, it returns one from the A.I. 
        /// classes which have been registered to the list.
        /// </summary>
        public AIBase FindAI(int index)
        {
            if (!activeOn) return null;

            return aiList[index];
        }

        /// <summary>
        /// By using the name, it returns one from the A.I. 
        /// classes which have been registered to the list.
        /// </summary>
        public AIBase FindAI(string name)
        {
            if (!activeOn) return null;

            foreach (AIBase aiBase in aiList)
            {
                if (name == aiBase.Name)
                    return aiBase;
            }

            return null;
        }

        /// <summary>
        /// it adds an A.I. with the specified name to the list.
        /// </summary>
        public int AddAI(string name, AIBase aiBase)
        {
            if (!activeOn) return -1;

            aiBase.Name = name;
            aiList.Add(aiBase);

            return aiList.IndexOf(aiBase);
        }

        /// <summary>
        /// by using the index of A.I., it removes an A.I. from the registered list.
        /// </summary>
        public void RemoveAI(int index)
        {
            aiList.RemoveAt(index);
        }

        /// <summary>
        /// it removes every registered A.I.
        /// </summary>
        public void RemoveAllAI()
        {
            aiList.Clear();
        }

        /// <summary>
        /// by using the index of the registered A.I., 
        /// it specifies the next-to-be-executed A.I.
        /// The active time of the next-to-be-executed A.I. is specified by activeTime.
        /// </summary>
        public void SetNextAI(int index, float activeTime)
        {
            if (!activeOn) return;

            nextAI = FindAI(index);

            nextAI.ActiveTime = activeTime;
        }

        /// <summary>
        /// A new A.I. will be played.
        /// The active time of the A.I. is specified by activeTime.
        /// The A.I., which has been active so far, will be ignored.
        /// </summary>
        public void StartAI(int index, float activeTime)
        {
            if (!activeOn) return;

            activeAI = null;
            nextAI = null;

            PlayAI(index, activeTime);
        }

        /// <summary>
        /// The specified A.I. will be played.
        /// The active time of the A.I. is specified by activeTime.
        /// The A.I., which has been active so far, will call the 
        /// Finish event handler and will be replaced by the newly specified A.I.
        /// When the newly specified A.I. start activating, 
        /// Start event handler is called.
        /// </summary>
        private void PlayAI(int index, float activeTime)
        {
            if (!activeOn) return;

            AIBase aiBase = FindAI(index);
            PlayAI(aiBase, activeTime);
        }

        /// <summary>
        /// The specified A.I. will be played.
        /// The active time of the A.I. is specified by activeTime.
        /// The A.I., which has been active so far, will call the 
        /// Finish event handler and will be replaced by the newly specified A.I.
        /// When the newly specified A.I. start activating, 
        /// Start event handler is called.
        /// </summary>
        private void PlayAI(AIBase aiBase, float activeTime)
        {
            if (!activeOn) return;

            //  Call the finish event of the previous A.I.
            if (activeAI != null)
                activeAI.AIFinish();

            //  Set to new active A.I.
            activeAI = aiBase;
            activeAI.ActiveTime = activeTime;

            //  Call start event of new A.I.
            activeAI.AIStart();
        }

        #endregion
    }
}
