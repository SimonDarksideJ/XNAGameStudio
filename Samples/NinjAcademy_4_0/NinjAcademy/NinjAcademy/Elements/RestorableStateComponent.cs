#region File Description
//-----------------------------------------------------------------------------
// RestorableStateComponent.cs
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

namespace NinjAcademy
{
    /// <summary>
    /// A drawable game component which can remember and restore its state.
    /// </summary>
    public class RestorableStateComponent : DrawableGameComponent
    {
        #region Fields


        bool stateStored;
        bool wasVisible;
        bool wasEnabled;


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new component instance.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        public RestorableStateComponent(Game game) : base(game)
        {
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Stores the component's visibility and whether or not it is enabled.
        /// </summary>
        public void StoreState()
        {
            stateStored = true;
            wasVisible = Visible;
            wasEnabled = Enabled;
        }

        /// <summary>
        /// Restores the component's state after it has been stored.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this method is called before calling 
        /// <see cref="StoreState"/>.</exception>
        public void RestoreState()
        {
            if (stateStored == false)
            {
                throw new InvalidOperationException("Cannot restore the current state before storing it first.");
            }

            stateStored = false;

            Visible = wasVisible;
            Enabled = wasEnabled;
        }


        #endregion
    }
}
