#region File Description
//-----------------------------------------------------------------------------
// ChaseBehavior.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace FuzzyLogic
{
    /// <summary>
    /// ChaseBehavior is a Behavior that will make an entity chase after another. The 
    /// logic is the same as we have seen in the previous AI sample. 
    /// </summary>
    public class ChaseBehavior : Behavior
    {
        // The entity we are chasing
        private Entity chase;

        public ChaseBehavior(Entity entity, Entity chase)
            : base(entity)
        {
            this.chase = chase;
        }

        public override void Update()
        {
            // Chasing is simple: we just turn towards the entity we want to chase,
            // and go as fast as possible.
            TurnToFace(chase.Position, Entity.TurnSpeed);
            Entity.CurrentSpeed = Entity.MaxSpeed;
        }
    }
}
