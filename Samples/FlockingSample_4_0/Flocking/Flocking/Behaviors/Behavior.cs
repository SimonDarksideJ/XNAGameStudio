#region File Description
//-----------------------------------------------------------------------------
// Behavior.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace Flocking
{
    /// <summary>
    /// Behavior is the base class for the four flock behaviors in this sample: 
    /// aligning, cohesion, separation and fleeing. It is an abstract class, 
    /// leaving the implementation of Update up to its subclasses. Animal objects 
    /// can have an arbitrary number of behaviors, after the entity calls Update 
    /// on the behavior the reaction results are stored in reaction so the owner 
    /// can query it.
    /// </summary>
    public abstract class Behavior
    {
        #region Fields
        /// <summary>
        /// Keep track of the animal that this behavior belongs to.
        /// </summary>
        public Animal Animal
        {
            get { return animal; }
            set { animal = value; }
        }
        private Animal animal;

        /// <summary>
        /// Store the behavior reaction here.
        /// </summary>
        public Vector2 Reaction
        {
            get { return reaction; }
        }
        protected Vector2 reaction;

        /// <summary>
        /// Store if the behavior has reaction results here.
        /// </summary>
        public bool Reacted
        {
            get { return reacted; }
        }
        protected bool reacted;
        #endregion

        #region Initialization
        protected Behavior(Animal animal)
        {
            this.animal = animal;
        }
        #endregion

        #region Update
        /// <summary>
        /// Abstract function that the subclass must impliment. Figure out the 
        /// Behavior reaction here.
        /// </summary>
        /// <param name="otherAnimal">the Animal to react to</param>
        /// <param name="aiParams">the Behaviors' parameters</param>
        public abstract void Update(Animal otherAnimal, AIParameters aiParams);

        /// <summary>
        /// Reset the behavior reactions from the last Update
        /// </summary>
        protected void ResetReaction()
        {
            reacted = false;
            reaction = Vector2.Zero;
        }
        #endregion
    }
}
