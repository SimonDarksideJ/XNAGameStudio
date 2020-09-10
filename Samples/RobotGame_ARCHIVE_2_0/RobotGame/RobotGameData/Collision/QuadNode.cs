#region File Description
//-----------------------------------------------------------------------------
// QuadNode.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.GameObject;
using RobotGameData.Helper;
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData.Collision
{
    /// <summary>
    /// This is a quad tree's node only colliding. not support render model.
    /// </summary>
    public class QuadNode : INamed
    {
        #region Fields

        string name = String.Empty;
        int depth = 0;

        protected BoundingBox containBox;
        
        protected Vector3[] vertices = null;
        protected int vertextCount = 0;

        protected QuadNode parentNode = null;
        protected QuadNode upperLeftNode = null;
        protected QuadNode upperRightNode = null;
        protected QuadNode lowerLeftNode = null;
        protected QuadNode lowerRightNode = null;

        protected bool visit = false;

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Depth
        {
            get { return depth; }
            set { depth = value; }
        }

        public int VertextCount
        {
            get { return vertextCount; }
        }

        public Vector3[] Vertices
        {
            get { return vertices; }
        }

        public QuadNode Parent
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        public QuadNode UpperLeftNode
        {
            get { return upperLeftNode; }
            set { upperLeftNode = value; }
        }

        public QuadNode UpperRightNode
        {
            get { return upperRightNode; }
            set { upperRightNode = value; }
        }

        public QuadNode LowerLeftNode
        {
            get { return lowerLeftNode; }
            set { lowerLeftNode = value; }
        }

        public QuadNode LowerRightNode
        {
            get { return lowerRightNode; }
            set { lowerRightNode = value; }
        }

        public bool IsLeafNode
        {
            get
            {
                return (upperLeftNode == null && upperRightNode == null &&
                       lowerLeftNode == null && lowerRightNode == null);
            }
        }

        #endregion

        /// <summary>
        /// constructor.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public QuadNode(Vector3 min, Vector3 max)
        {
            containBox.Min = min;
            containBox.Max = max;

            this.vertextCount = 0;
        }

        /// <summary>
        /// create vertices.
        /// </summary>
        public void CreateVertices(Vector3[] values)
        {
            int count = values.Length;

            this.vertices = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                this.vertices[i] = values[i];
                this.vertextCount++;
            }
        }

        /// <summary>
        /// add vertices.
        /// </summary>
        public void AddVertex(Vector3[] values)
        {
            ResizeVertexArray(values.Length);

            int lastIndex = this.vertextCount;

            for (int i = 0; i < values.Length; i++)
            {
                this.vertices[i + lastIndex] = values[i];
                this.vertextCount++;
            }
        }

        /// <summary>
        /// resize vertex array.
        /// </summary>
        /// <param name="resizingCount"></param>
        public void ResizeVertexArray(int resizingCount)
        {
            if (this.vertices.Length < this.vertextCount + resizingCount)
            {
                Vector3[] newVertices = 
                    new Vector3[this.vertices.Length + resizingCount];

                //  move to new array
                for (int i = 0; i < this.vertextCount; i++)
                    newVertices[i] = this.vertices[i];

                this.vertices = null;
                this.vertices = newVertices;
            }
        }

        /// <summary>
        /// Finds a contaning quad node.
        /// </summary>
        public QuadNode GetNodeContaining(ref CollideElement bounds)
        {
            if (this.IsLeafNode == false)
            {
                //  checks with upper left node.
                if (UpperLeftNode != null)
                {
                    if (UpperLeftNode.Contains(ref bounds) )
                        return UpperLeftNode.GetNodeContaining(ref bounds);
                }

                //  checks with upper right node.
                if (UpperRightNode != null)
                {
                    if (UpperRightNode.Contains(ref bounds) )
                        return UpperRightNode.GetNodeContaining(ref bounds);
                }

                //  checks with lower left node.
                if (LowerLeftNode != null)
                {
                    if (LowerLeftNode.Contains(ref bounds) )
                        return LowerLeftNode.GetNodeContaining(ref bounds);
                }

                //  checks with lower right node.
                if (LowerLeftNode != null)
                {
                    if (LowerLeftNode.Contains(ref bounds) )
                        return LowerLeftNode.GetNodeContaining(ref bounds);
                }
            }

            //  checks with this node.
            if( this.Contains(ref bounds) )
                return this;

            return null;
        }

        /// <summary>
        /// checks contaning.
        /// </summary>
        public bool Contains(ref CollideElement bounds)
        {
            if (bounds is CollideBox)
            {
                CollideBox target = bounds as CollideBox;

                return containBox.Intersects(target.BoundingBox);
            }
            else if (bounds is CollideSphere)
            {
                CollideSphere target = bounds as CollideSphere;

                return containBox.Intersects(target.BoundingSphere);
            }
            else if (bounds is CollideRay)
            {
                CollideRay target = bounds as CollideRay;

                return (containBox.Intersects(target.Ray) != null);
            }

            return false;
        }

        /// <summary>
        /// display build information to debug output window.
        /// </summary>
        /// <returns>total vertex count</returns>
        public int Dump()
        {
            int vertexCount = this.VertextCount;

            System.Diagnostics.Debug.WriteLine(
                                    this.Name + "[" + this.Depth + "] vertex = " +
                                    this.VertextCount.ToString() + ")");

            if (UpperLeftNode != null)
            {
                vertexCount += UpperLeftNode.Dump();
            }

            if (UpperRightNode != null)
            {
                vertexCount += UpperRightNode.Dump();
            }

            if (LowerLeftNode != null)
            {
                vertexCount += LowerLeftNode.Dump();
            }

            if (LowerRightNode != null)
            {
                vertexCount += LowerRightNode.Dump();
            }

            return vertexCount;
        }
    }
}
