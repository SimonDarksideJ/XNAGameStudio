#region File Description
//-----------------------------------------------------------------------------
// QuadTree.cs
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
#endregion

namespace RobotGameData.Collision
{
    /// <summary>
    /// This is a quad tree only colliding. not support render model.
    /// </summary>
    public class QuadTree
    {
        /// <summary>
        /// quad tree area.
        /// </summary>
        enum IntersectionType
        {
            UpperLeft = 0,
            UpperRight,
            LowerLeft,
            LowerRight,
        }

        #region Fields

        protected QuadNode rootNode = null;
        protected int nodeCount = 0;
        protected int depthLevel = 0;

        #endregion       

        #region Properties

        public QuadNode RootNode
        {
            get { return rootNode; }
        }

        public int NodeCount
        {
            get { return nodeCount; }
        }

        public int DepthLevel
        {
            get { return depthLevel; }
        }

        #endregion

        /// <summary>
        /// build a quad tree.
        /// </summary>
        /// <param name="vertices">vertex array</param>
        /// <param name="depthLevel">quad tree depth count</param>
        /// <returns></returns>
        public int Build(Vector3[] vertices, int depthLevel)
        {
            this.depthLevel = depthLevel;

            rootNode = BuildNode(vertices, depthLevel);
            rootNode.Name = "QUADTREE: root node";

            return nodeCount;
        }        

        /// <summary>
        /// build a quad node.
        /// </summary>
        /// <param name="vertices">vertex array</param>
        /// <param name="depthLevel">quad tree depth count</param>
        /// <returns>new quad node</returns>
        protected QuadNode BuildNode(Vector3[] vertices, int depthLevel)
        {
            QuadNode newNode = null;
            
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            float centerX = 0.0f;
            float centerZ = 0.0f;

            IntersectionType[] Intersection = new IntersectionType[3];

            //  checks min and max of vertices
            for (int i = 0; i < vertices.Length; i++)
            {
                if (min.X > vertices[i].X) min.X = vertices[i].X;
                if (min.Y > vertices[i].Y) min.Y = vertices[i].Y;
                if (min.Z > vertices[i].Z) min.Z = vertices[i].Z;

                if (max.X < vertices[i].X) max.X = vertices[i].X;
                if (max.Y < vertices[i].Y) max.Y = vertices[i].Y;
                if (max.Z < vertices[i].Z) max.Z = vertices[i].Z;
            }

            centerX  = (max.X + min.X) / 2;
            centerZ  = (max.Z + min.Z) / 2;

            //  creates node
            newNode = new QuadNode(min, max);
            newNode.Depth = this.depthLevel - depthLevel;
            
            nodeCount++;            

            if (depthLevel-- > 0)
            {
                List<Vector3> upperLeftVertexList = new List<Vector3>();
                List<Vector3> upperRightVertexList = new List<Vector3>();
                List<Vector3> lowerLeftVertexList = new List<Vector3>();
                List<Vector3> lowerRightVertexList = new List<Vector3>();
                List<Vector3> medlineVertexList = new List<Vector3>();
                                
                //  vertex count
                for (int count = 0; count < vertices.Length; count += 3)
                {
                    //  Triangle
                    for (int i = 0; i < 3; i++)
                    {
                        //  inside upper left
                        if (vertices[count + i].X < centerX && 
                            vertices[count + i].Z < centerZ)
                        {
                            Intersection[i] = IntersectionType.UpperLeft;
                        }
                        //  inside upper right
                        else if (vertices[count + i].X > centerX &&
                                vertices[count + i].Z < centerZ)
                        {
                            Intersection[i] = IntersectionType.UpperRight;
                        }
                        //  inside lower left
                        else if (vertices[count + i].X < centerX &&
                                vertices[count + i].Z > centerZ)
                        {
                            Intersection[i] = IntersectionType.LowerLeft;
                        }
                        //  inside lower right
                        else if (vertices[count + i].X > centerX &&
                                vertices[count + i].Z > centerZ)
                        {
                            Intersection[i] = IntersectionType.LowerRight;
                        }
                    }

                    //  intersecting with upper left area
                    if (Intersection[0] == IntersectionType.UpperLeft &&
                        Intersection[1] == IntersectionType.UpperLeft &&
                        Intersection[2] == IntersectionType.UpperLeft)
                    {
                        upperLeftVertexList.Add(vertices[count + 0]);
                        upperLeftVertexList.Add(vertices[count + 1]);
                        upperLeftVertexList.Add(vertices[count + 2]);
                    }
                    //  intersecting with upper right area
                    else if (Intersection[0] == IntersectionType.UpperRight &&
                            Intersection[1] == IntersectionType.UpperRight &&
                            Intersection[2] == IntersectionType.UpperRight)
                    {
                        upperRightVertexList.Add(vertices[count + 0]);
                        upperRightVertexList.Add(vertices[count + 1]);
                        upperRightVertexList.Add(vertices[count + 2]);
                    }
                    //  intersecting with lower left area
                    else if (Intersection[0] == IntersectionType.LowerLeft &&
                            Intersection[1] == IntersectionType.LowerLeft &&
                            Intersection[2] == IntersectionType.LowerLeft)
                    {
                        lowerLeftVertexList.Add(vertices[count + 0]);
                        lowerLeftVertexList.Add(vertices[count + 1]);
                        lowerLeftVertexList.Add(vertices[count + 2]);
                    }
                    //  intersecting with lower right area
                    else if (Intersection[0] == IntersectionType.LowerRight &&
                            Intersection[1] == IntersectionType.LowerRight &&
                            Intersection[2] == IntersectionType.LowerRight)
                    {
                        lowerRightVertexList.Add(vertices[count + 0]);
                        lowerRightVertexList.Add(vertices[count + 1]);
                        lowerRightVertexList.Add(vertices[count + 2]);
                    }
                    //  intersecting with medline
                    else
                    {
                        medlineVertexList.Add(vertices[count + 0]);
                        medlineVertexList.Add(vertices[count + 1]);
                        medlineVertexList.Add(vertices[count + 2]);
                    }
                }

                string prefix = String.Empty;
                for (int i = 0; i < this.depthLevel - depthLevel; i++) prefix += "- ";

                //  upper left
                if (upperLeftVertexList.Count > 0)
                {
                    newNode.UpperLeftNode =
                        BuildNode(upperLeftVertexList.ToArray(), depthLevel);

                    newNode.UpperLeftNode.Parent = newNode;                  

                    newNode.UpperLeftNode.Name = "QUADTREE: " + prefix + "UL ";

                    if (newNode.UpperLeftNode.IsLeafNode )
                        newNode.UpperLeftNode.Name += "leaf";
                    else
                        newNode.UpperLeftNode.Name += "node";
                }

                //  upper right
                if (upperRightVertexList.Count > 0)
                {
                    newNode.UpperRightNode =
                        BuildNode(upperRightVertexList.ToArray(), depthLevel);

                    newNode.UpperRightNode.Parent = newNode;
                    newNode.UpperRightNode.Name = "QUADTREE: " + prefix + "UR ";

                    if (newNode.UpperRightNode.IsLeafNode )
                        newNode.UpperRightNode.Name += "leaf";
                    else
                        newNode.UpperRightNode.Name += "node";
                }

                //  lower left
                if (lowerLeftVertexList.Count > 0)
                {
                    newNode.LowerLeftNode =
                        BuildNode(lowerLeftVertexList.ToArray(), depthLevel);

                    newNode.LowerLeftNode.Parent = newNode;
                    newNode.LowerLeftNode.Name = "QUADTREE: " + prefix + "LL ";

                    if (newNode.LowerLeftNode.IsLeafNode )
                        newNode.LowerLeftNode.Name += "leaf";
                    else
                        newNode.LowerLeftNode.Name += "node";
                }

                //  lower right
                if (lowerRightVertexList.Count > 0)
                {
                    newNode.LowerRightNode =
                        BuildNode(lowerRightVertexList.ToArray(), depthLevel);

                    newNode.LowerRightNode.Parent = newNode;
                    newNode.LowerRightNode.Name = "QUADTREE: " + prefix + "LR ";

                    if (newNode.LowerRightNode.IsLeafNode )
                        newNode.LowerRightNode.Name += "leaf";
                    else
                        newNode.LowerRightNode.Name += "node";
                }

                //  intersecting with medline
                if (medlineVertexList.Count > 0)
                {
                    newNode.CreateVertices(medlineVertexList.ToArray());
                }
            }
            else
            {
                newNode.CreateVertices(vertices);
            }

            return newNode;
        }

        /// <summary>
        /// display build information to debug output window.
        /// </summary>
        /// <returns>total vertex count</returns>
        public int Dump()
        {
            int vertexCount = 0;

            vertexCount = this.rootNode.Dump();

            System.Diagnostics.Debug.WriteLine(
                            "QUADTREE: total vertex count = " + vertexCount);

            return vertexCount;
        }

        /// <summary>
        /// Finds a contaning quad node.
        /// </summary>
        public QuadNode GetNodeContaining(ref CollideElement bounds)
        {
            if (rootNode != null)
            {
                if (rootNode.Contains(ref bounds) )
                    return rootNode.GetNodeContaining(ref bounds);
            }

            return null;
        }
    }
}