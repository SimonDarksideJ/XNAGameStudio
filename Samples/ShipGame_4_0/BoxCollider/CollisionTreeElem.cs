#region File Description
//-----------------------------------------------------------------------------
// CollisionTreeElem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion



namespace BoxCollider
{
    public class CollisionTreeElem
    {
        // bounding box for tree element
        public CollisionBox box;

        // recurse id used to compute selections without element duplicates
        public uint lastRecurseId = 0;

        /// <summary>
        /// Create a new tree element
        /// </summary>
        public CollisionTreeElem()
        {
        }

        /// <summary>
        /// Virtual function to intersect a point with the element
        /// </summary>
        public virtual bool PointIntersect(
            Vector3 rayOrigin, 
            Vector3 rayDirection, 
            Vector3[] vertices,
            out float intersectDistance, 
            out Vector3 intersectPosition, 
            out Vector3 intersectNormal)
        {
            intersectDistance = 0;
            intersectPosition = Vector3.Zero;
            intersectNormal = Vector3.Zero;
            return false;
        }

        /// <summary>
        /// Virtual function to intersect a box with the element
        /// </summary>
        public virtual bool BoxIntersect(
            CollisionBox rayBox, 
            Vector3 rayOrigin, 
            Vector3 rayDirection, 
            Vector3[] vertices,
            out float intersectDistance, 
            out Vector3 intersectPosition, 
            out Vector3 intersectNormal)
        {
            intersectDistance = 0;
            intersectPosition = Vector3.Zero;
            intersectNormal = Vector3.Zero;
            return false;
        }

        /// <summary>
        /// Virtual function when adding the element to a node
        /// </summary>
        public virtual void AddToNode(CollisionTreeNode n)
        {
        }
    }

    public class CollisionTreeElemDynamic : CollisionTreeElem
    {
        // all tree nodes the dynamic element is included
        List<CollisionTreeNode> nodes = new List<CollisionTreeNode>();

        /// <summary>
        /// Create a new dynamic tree element 
        /// (can change position moving around the tree at any time)
        /// </summary>
        public CollisionTreeElemDynamic()
            : base()
        {
        }

        /// <summary>
        /// Add the dynamic element to the node
        /// </summary>
        public override void AddToNode(CollisionTreeNode n)
        {
            nodes.Add(n);
        }

        /// <summary>
        /// Remove dynamic element from node
        /// </summary>
        public void RemoveFromNodes()
        {
            // remove element from all nodes it is included in
            foreach (CollisionTreeNode n in nodes)
            {
                n.RemoveElement(this);
            }
            nodes.Clear();
        }
    }
}
