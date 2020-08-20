#region File Description
//-----------------------------------------------------------------------------
// CohesionBehavior.cs
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
    /// CohesionBehavior is a Behavior that  makes an animal move towards another 
    /// if it's not already too close
    /// </summary>
    class CohesionBehavior : Behavior
    {
        #region Initialization
        public CohesionBehavior(Animal animal)
            : base(animal)
        {
        }
        #endregion

        #region Update

        /// <summary>
        /// CohesionBehavior.Update infuences the owning animal to move towards the
        /// otherAnimal that it sees as long as it isn’t too close, in this case 
        /// that means inside the separationDist in the passed in AIParameters.
        /// </summary>
        /// <param name="otherAnimal">the Animal to react to</param>
        /// <param name="aiParams">the Behaviors' parameters</param>
        public override void Update(Animal otherAnimal, AIParameters aiParams)
        {
            base.ResetReaction();

            Vector2 pullDirection = Vector2.Zero;
            float weight = aiParams.PerMemberWeight;

            //if the otherAnimal is too close we dont' want to fly any
            //closer to it
            if (Animal.ReactionDistance > 0.0f
                && Animal.ReactionDistance > aiParams.SeparationDistance)
            {
                //We want to make the animal move closer the the otherAnimal so we 
                //create a pullDirection vector pointing to the otherAnimal bird and 
                //weigh it based on how close the otherAnimal is relative to the 
                //AIParameters.separationDistance.
                pullDirection = -(Animal.Location - Animal.ReactionLocation);
                Vector2.Normalize(ref pullDirection, out pullDirection);

                weight *= (float)Math.Pow((double)
                    (Animal.ReactionDistance - aiParams.SeparationDistance) /
                        (aiParams.DetectionDistance - aiParams.SeparationDistance), 2);

                pullDirection *= weight;

                reacted = true;
                reaction = pullDirection;
            }
        }
        #endregion
    }
}
