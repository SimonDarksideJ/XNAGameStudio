#region File Description
//-----------------------------------------------------------------------------
// AlignBehavior.cs
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
    /// AlignBehavior is a Behavior that makes an animal move in the same
    /// direction that the other Animal it sees is
    /// </summary>
    class AlignBehavior : Behavior
    {
        #region Initialization
        public AlignBehavior(Animal animal)
            : base(animal)
        {
        }
        #endregion

        #region Update

        /// <summary>
        /// AlignBehavior.Update infuences the owning animal to move in same the 
        /// direction as the otherAnimal that it sees.
        /// </summary>
        /// <param name="otherAnimal">the Animal to react to</param>
        /// <param name="aiParams">the Behaviors' parameters</param>
        public override void Update(Animal otherAnimal, AIParameters aiParams)
        {
            base.ResetReaction();

            if (otherAnimal != null)
            {
                reacted = true;
                reaction = otherAnimal.Direction * aiParams.PerMemberWeight;
            }
        }
        #endregion
    }
}
