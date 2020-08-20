#region File Description
//-----------------------------------------------------------------------------
// NodeBase.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData
{
    /// <summary>
    /// It processes the node structure of the basic parent and child dependency.
    /// </summary>
    public abstract class NodeBase : INamed, IDisposable
    {
        #region Fields

        string name;                   // Node name
        NodeBase parent = null;        // Parent node

        protected Collection<NodeBase> childList = null;  // Child storage
        protected bool isDisposed = false;

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public NodeBase Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public int ChildCount
        {
            get { return (childList != null ? childList.Count : 0); }
        }

        #endregion

        /// <summary>
        /// Add a child node
        /// </summary>
        public void AddChild(NodeBase node)
        {
            if(childList == null)
                childList = new Collection<NodeBase>();

            node.Parent = this;

            childList.Add(node);
        }

        /// <summary>
        /// Insert a child node
        /// </summary>
        public void InsertChild(int index, NodeBase node)
        {
            node.Parent = this;

            childList.Insert(index, node);
        }

        /// <summary>
        /// Find a child node
        /// </summary>
        public NodeBase GetChild(int index)
        {
            return childList[index];
        }

        /// <summary>
        /// Copy all child nodes to array.
        /// </summary>
        public void GetChildArray(NodeBase[] array)
        {
            childList.CopyTo(array, 0);
        }

        /// <summary>
        /// it checks whether the node is a child node
        /// </summary>
        public bool IsChild(NodeBase node)
        {
            return childList.Contains(node);
        }

        /// <summary>
        /// Remove a child node
        /// </summary>
        /// <param name="node">target node</param>
        /// <param name="isRecursive">
        /// whether to remove the child node which has been included in the target node
        /// </param>
        public void RemoveChild(NodeBase node, bool isRecursive)
        {
            if (childList != null)
            {
                if (isRecursive )
                {
                    node.RemoveAllChild(isRecursive);
                }

                childList.Remove(node);
                node.Parent = null;
            }
        }

        /// <summary>
        /// Remove all children
        /// </summary>
        /// <param name="isRecursive"></param>
        public void RemoveAllChild(bool isRecursive)
        {
            if (childList != null)
            {
                if (isRecursive )
                {
                    while (childList.Count > 0)
                    {
                        RemoveChild(childList[0], isRecursive);
                    }
                }

                childList.Clear();
            }
        }

        /// <summary>
        /// Remove this node from parent
        /// </summary>
        public void RemoveFromParent()
        {
            if (parent != null)
            {
                parent.RemoveChild(this, false);
                parent = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (childList != null)
                {
                    for (int i = 0; i < childList.Count; i++)
                    {
                        childList[i].Dispose(disposing);
                    }
                }

                if (disposing)
                {
                    //if we're manually disposing,
                    //then managed content should be unloaded
                    UnloadContent();
                }

                isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void UnloadContent() { }
    }
}
