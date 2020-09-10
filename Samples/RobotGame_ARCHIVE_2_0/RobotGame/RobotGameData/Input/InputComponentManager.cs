#region File Description
//-----------------------------------------------------------------------------
// InputComponentManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace RobotGameData.Input
{
    /// <summary>
    /// It allows the creation of InputComponent up to 4, which is the maximum 
    /// allowed number, and waits for the user’s inputs.
    /// </summary>
    public class InputComponentManager
    {
        #region Fields

        protected InputComponent[] inputComponents;
        
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public InputComponentManager()
        {
            //  create four input components
            inputComponents = new InputComponent[] 
            {
                new InputComponent( PlayerIndex.One),
                new InputComponent( PlayerIndex.Two),
                new InputComponent( PlayerIndex.Three),
                new InputComponent( PlayerIndex.Four),
            };
        }

        /// <summary>
        /// Initialize all input components
        /// </summary>
        public void Initialize()
        {
            for( int i = 0; i < inputComponents.Length; i++)
                inputComponents[i].Initialize();
        }

        /// <summary>
        /// Reset all input components
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < inputComponents.Length; i++)
                inputComponents[i].Reset();
        }

        /// <summary>
        /// Update all input components
        /// </summary>
        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < inputComponents.Length; i++)
                inputComponents[i].Update(gameTime);
        }
        
        public InputComponent GetInputComponent(PlayerIndex idx)
        {
            return inputComponents[(Int32)idx];
        }

        /// <summary>
        /// This is the function which does the input process before updating.
        /// </summary>
        public void PreUpdate()
        {
            for (int i = 0; i < inputComponents.Length; i++)
                inputComponents[i].PreUpdate();
        }

        /// <summary>
        /// This is the function which does the input process after updating.
        /// </summary>
        public void PostUpdate()
        {
            for (int i = 0; i < inputComponents.Length; i++)
                inputComponents[i].PostUpdate();
        }
    }
}
