#region File Description
//-----------------------------------------------------------------------------
// EvadeBehavior.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace FuzzyLogic
{
    /// <summary>
    /// EvadeBehavior is a Behavior that will make an entity evade another. The 
    /// logic is the same as we have seen in the previous AI sample. 
    /// </summary>
    public class EvadeBehavior : Behavior
    {
        // The entity to evade
        private Entity evade;

        public EvadeBehavior(Entity entity, Entity evade)
            : base(entity)
        {
            this.evade = evade;
        }

        public override void Update()
        {
            // The evasion behavior is accomplished by using the TurnToFace function to 
            // turn towards a point on a straight line facing away from the entity we're
            // trying to evade. In other words, if the tank is point A, and the mouse is 
            // point B, the "seek point" is C.
            //     C
            //   B
            // A
            Vector2 seekPosition = 2 * Entity.Position - evade.Position;
            TurnToFace(seekPosition, Entity.TurnSpeed);
            Entity.CurrentSpeed = Entity.MaxSpeed;
        }
    }
}
