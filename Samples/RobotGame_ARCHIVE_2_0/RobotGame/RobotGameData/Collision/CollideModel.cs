#region File Description
//-----------------------------------------------------------------------------
// CollideModel.cs
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
#endregion

namespace RobotGameData.Collision
{
    /// <summary>
    /// It's a collision model.
    /// </summary>
    public class CollideModel : CollideElement
    {
        #region Fields

        protected Vector3[] vertices = null;
        protected Vector3[] normal = null;
        protected QuadTree quadTree = null;

        #endregion

        #region Properties

        public Vector3[] Vertices
        {
            get { return vertices; }
        }

        public Vector3[] Normal
        {
            get { return normal; }
        }

        public QuadTree QuadTree
        {
            get { return quadTree; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// no quad tree.
        /// </summary>
        /// <param name="vertices">model's vertices</param>
        public CollideModel(Vector3[] vertices)
            : base()
        {
            this.vertices = vertices;

            //  Creates vector array by triangle's vertices count
            this.normal = new Vector3[this.vertices.Length / 3];

            //  Creates normal vector by each triangle
            for (int i = 0; i < this.normal.Length; i++)
            {
                Vector3 v1 = this.vertices[i * 3];
                Vector3 v2 = this.vertices[i * 3 + 1];
                Vector3 v3 = this.vertices[i * 3 + 2];

                this.normal[i] = Vector3.Normalize(Vector3.Cross(v3 - v1, v2 - v1));
            }
        }

        /// <summary>
        /// Constructor.
        /// use quad tree.
        /// </summary>
        /// <param name="vertices">model's vertices</param>
        /// <param name="buildQuadTreeDepth">quad tree depth count</param>
        public CollideModel(Vector3[] vertices, int buildQuadTreeDepth) : base()
        {
            this.vertices = vertices;

            //  Creates vector array by triangle's vertices count
            this.normal = new Vector3[this.vertices.Length / 3];

            //  Creates normal vector by each triangle
            for (int i = 0; i < this.normal.Length; i++)
            {
                Vector3 v1 = this.vertices[i * 3];
                Vector3 v2 = this.vertices[i * 3 + 1];
                Vector3 v3 = this.vertices[i * 3 + 2];

                this.normal[i] = Vector3.Normalize(Vector3.Cross(v3 - v1, v2 - v1));
            }

            //  builds quad tree.
            if (buildQuadTreeDepth > 0)
            {
                this.quadTree = new QuadTree();
                this.quadTree.Build(this.Vertices, buildQuadTreeDepth);
            }
        }

        /// <summary>
        /// Set to new transform matrix.
        /// </summary>
        public override void Transform(Matrix matrix)
        {
            if (this.QuadTree != null)
            {
                int nodeDepth = this.QuadTree.DepthLevel;

                Matrix newTransform = matrix;

                //  if changed matrix, re-build quad tree and collison vertices
                if (this.TransformMatrix != newTransform)
                {
                    for (int i=0; i < this.vertices.Length; i++)
                    {
                        this.vertices[i] = 
                                    Vector3.Transform(this.vertices[i], newTransform);
                    }

                    for (int i = 0; i < this.normal.Length; i++)
                    {
                        this.normal[i] = 
                                    Vector3.Transform(this.normal[i], newTransform);
                    }

                    //  re-build quad tree.
                    this.quadTree = null;
                    this.quadTree = new QuadTree();
                    this.quadTree.Build(this.Vertices, nodeDepth);
                }
            }

            base.Transform(matrix);
        }

        /// <summary>
        /// Return a transformed vertex.
        /// </summary>
        /// <param name="index">vertex index</param>
        /// <returns>transformed vertex vector</returns>
        public Vector3 GetTransformedVertex(int index)
        {
            if (index > this.vertices.Length)
                throw new MemberAccessException("Overflow index (" + index + ")");

            return Vector3.Transform(this.vertices[index], TransformMatrix);
        }
        /// <summary>
        /// Return a transformed normal vector.
        /// </summary>
        /// <param name="index">vertex index</param>
        /// <returns>transformed normal vector</returns>
        public Vector3 GetTransformedNormal(int index)
        {
            if (index > this.normal.Length)
                throw new MemberAccessException("Overflow index (" + index + ")");

            return Vector3.Transform(this.normal[index], TransformMatrix);
        }
    }
}
