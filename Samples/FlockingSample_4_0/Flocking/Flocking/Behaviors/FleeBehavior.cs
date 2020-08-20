#region File Description
//-----------------------------------------------------------------------------
// FleeBehavior.cs
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
    /// FleeBehavior is a Behavior that makes an animal run from another
    /// </summary>
    public class FleeBehavior : Behavior
    {
        #region Initialization
        public FleeBehavior(Animal animal)
            : base(animal)
        {
        }
        #endregion

        #region Update
        public override void Update(Animal otherAnimal, AIParameters aiParams)
        {
            base.ResetReaction();

            Vector2 dangerDirection = Vector2.Zero;

            //Vector2.Dot will return a negative result in this case if the 
            //otherAnimal is behind the animal, in that case we don’t have to 
            //worry about it because we’re already moving away from it.
            if (Vector2.Dot(
                Animal.Location, Animal.ReactionLocation) >= -(Math.PI / 2))
            {
                //set the animal to fleeing so that it flashes red
                Animal.Fleeing = true;
                reacted = true;

                dangerDirection = Animal.Location - Animal.ReactionLocation;
                Vector2.Normalize(ref dangerDirection, out dangerDirection);
                
                reaction = (aiParams.PerDangerWeight * dangerDirection);
            }
        }
        #endregion
    }
}
