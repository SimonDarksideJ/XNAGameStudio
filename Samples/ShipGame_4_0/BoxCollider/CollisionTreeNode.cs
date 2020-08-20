#region File Description
//-----------------------------------------------------------------------------
// CollisionTreeNode.cs
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
    public class CollisionTreeNode
    {
        // the bounding box for the node
        CollisionBox box;

        // the node children (if null node is a leaf)
        CollisionTreeNode[] children;

        // list with elements included in the node (only created on leaf nodes)
        List<CollisionTreeElem> elems;

        /// <summary>
        /// Create a new tree node
        /// </summary>
        public CollisionTreeNode(CollisionBox collisionBox, uint subdivLevel)
        {
            if (collisionBox == null)
            {
                throw new ArgumentNullException("collisionBox");
            }

            // save node box
            box = collisionBox;

            // if subdivision needed
            if (subdivLevel > 0)
            {
                // decrease subdivision level
                subdivLevel--;

                // create the 8 children
                children = new CollisionTreeNode[8];
                CollisionBox[] childrenBox = box.GetChildren();
                for (uint i = 0; i < 8; i++)
                    children[i] = new CollisionTreeNode(childrenBox[i], subdivLevel);
            }
        }

        /// <summary>
        /// Recursive function to add an element to the tree
        /// </summary>
        public void AddElement(CollisionTreeElem e)
        {
            // if element do not intersect node, return
            if (e.box.BoxIntersect(box) == false)
                return;

            // if leaf  node (no children)
            if (children == null)
            {
                // if no elements list, add one
                if (elems == null)
                    elems = new List<CollisionTreeElem>();

                // add element to list
                elems.Add(e);
                e.AddToNode(this);
            }
            else
            {
                // if not a leaf recurse to all its children
                foreach (CollisionTreeNode n in children)
                    n.AddElement(e);
            }
        }

        /// <summary>
        /// Remove element from this node
        /// </summary>
        public void RemoveElement(CollisionTreeElem e)
        {
            if (elems != null)
                elems.Remove(e);
        }

        /// <summary>
        /// Recursive function to get all elements intersecting a given bounding box
        /// </summary>
        public void GetElements(
            CollisionBox b,
            List<CollisionTreeElem> e,
            uint recurseId)
        {
            // if selection box does not intersect node box, return
            if (b.BoxIntersect(box) == false)
                return;

            // if any elements in this node add them to selection list
            if (elems != null)
            {
                foreach (CollisionTreeElem elem in elems)
                {
                    // elements can be repeated in many nodes
                    // only add element to selection list if not already 
                    // added by another node in this same recursion
                    if (elem.lastRecurseId < recurseId)
                    {
                        // if selection box intersect the element box
                        if (elem.box.BoxIntersect(b))
                            // add element to selection list
                            e.Add(elem);
                        // set this recuse id to prevent duplicate results
                        elem.lastRecurseId = recurseId;
                    }
                }
            }

            // if not a leaf node, recurso to all children
            if (children != null)
            {
                foreach (CollisionTreeNode n in children)
                    n.GetElements(b, e, recurseId);
            }
        }
    }
}
