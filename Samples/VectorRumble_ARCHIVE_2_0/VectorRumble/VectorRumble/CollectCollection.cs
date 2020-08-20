#region File Description
//-----------------------------------------------------------------------------
// CollectCollection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// A collection with an additional feature to isolate the "collection"
    /// of the objects.
    /// </summary>
    sealed class CollectCollection<T>
        : System.Collections.ObjectModel.Collection<T>
    {
        #region Fields and Properties
        private World world;
        public World World
        {
            get { return world; }
        }

        private System.Collections.ObjectModel.Collection<T> garbage;
        public System.Collections.ObjectModel.Collection<T> Garbage
        {
            get { return garbage; }
            set { garbage = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new collection.
        /// </summary>
        /// <param name="world">The world this particle system resides in.</param>
        public CollectCollection(World world)
        {
            this.world = world;
            this.garbage =
            new System.Collections.ObjectModel.Collection<T>();
        }
        #endregion

        #region Collection
        /// <summary>
        /// Remove all of the "garbage" ParticleSystem objects from this collection.
        /// </summary>
        public void Collect()
        {
            for (int i = 0; i < garbage.Count; i++)
            {
                Remove(garbage[i]);
            }
            garbage.Clear();
        }
        #endregion
    }
}
