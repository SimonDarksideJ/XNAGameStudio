#region File Description
//-----------------------------------------------------------------------------
// CollisionContext.cs
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
    #region CollisionResult

    public enum ResultType
    {
        /// <summary>
        /// To find the nearest collision from itself.
        /// </summary>
        NearestOne = 0,
    }

    /// <summary>
    /// result report of collision
    /// </summary>
    public class CollisionResult
    {
        /// <summary>
        /// Distance between detected collision
        /// </summary>
        public float distance = 0.0f;

        /// <summary>
        /// Detection object count
        /// </summary>
        public int collideCount = 0;

        /// <summary>
        /// Detected object element
        /// </summary>
        public CollideElement detectedCollide = null;

        /// <summary>
        /// intersect point
        /// </summary>
        public Vector3? intersect = null;

        /// <summary>
        /// intersect normal
        /// </summary>
        public Vector3? normal = null;
    
        public void CopyTo(ref CollisionResult target)
        {
            target.distance = this.distance;
            target.collideCount = this.collideCount;
            target.detectedCollide = this.detectedCollide;
            target.intersect = this.intersect;
            target.normal = this.normal;
        }

        public void Clear()
        {
            this.distance = 0.0f;
            this.collideCount = 0;
            this.detectedCollide = null;
            this.intersect = null;
            this.normal = null;
        }
    }

    #endregion

    #region CollisionLayer

    /// <summary>
    /// This layer groups collision elements that need to be processed 
    /// for the collision collectively.
    /// </summary>
    public class CollisionLayer : INamed, IIdentity
    {
        #region Field

        string name = String.Empty;
        int id = -1;

        List<CollideElement> collideContainer = new List<CollideElement>();

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        
        public int CollideCount
        {
            get { return collideContainer.Count; }
        }

        #endregion

        /// <summary>
        /// Add a collsion element.
        /// </summary>
        public void AddCollide(CollideElement collide)
        {
            if (collideContainer.Contains(collide))
                throw new InvalidOperationException("Already entry the collide");

            collideContainer.Add(collide);

            collide.ParentLayer = this;
        }

        /// <summary>
        /// Get collision element using the index.
        /// </summary>
        public CollideElement GetCollide(int index)
        {
            return collideContainer[index];
        }

        /// <summary>
        /// Find a collision element using the name.
        /// </summary>
        public CollideElement FindCollide(string name)
        {
            // Finding collision by name
            for (int i = 0; i < collideContainer.Count; i++)
            {
                CollideElement collide = collideContainer[i];

                if (collide.Name == name)
                    return collide;
            }

            return null;
        }

        /// <summary>
        /// it checks whether the collision element which has been included. 
        /// </summary>
        public bool IsContain(CollideElement collide)
        {
            return (collideContainer.IndexOf(collide) == -1 ? false : true);
        }

        /// <summary>
        /// Remove the collision element.
        /// </summary>
        public bool RemoveCollide(CollideElement collide)
        {
            return collideContainer.Remove(collide);
        }

        /// <summary>
        /// Remove a collision element using the index.
        /// </summary>
        public void RemoveCollide(int index)
        {
            collideContainer.RemoveAt(index);
        }

        /// <summary>
        /// Remove all collision elements.
        /// </summary>
        public void RemoveAll()
        {
            collideContainer.Clear();
        }

        /// <summary>
        /// Make an Identity number.
        /// </summary>
        public int MakeId()
        {
            this.id = GetHashCode();

            return this.id;
        }
    }

    #endregion

    /// <summary>
    /// It tests for collision again the registered collision elements.
    /// When you request CollisionContext a collision test with the source 
    /// CollideElement, a result from a collision test would be returned with 
    /// all CollideElements that have been registered to the specific collision
    /// layer as the target.
    /// It supports the following collision types:  ray, model, box, and sphere.
    /// </summary>
    public class CollisionContext
    {
        #region Fields

        /// <summary>
        /// If set to false, all of the related functions get turned off.
        /// </summary>
        bool activeOn = true;

        List<CollisionLayer> collideLayerContainer
                                            = new List<CollisionLayer>();
                
        CollisionResult tempResult = new CollisionResult();

        int totalCollidingCount = 0;
        
        #endregion

        #region Properties

        public int LayerCount
        {
            get { return collideLayerContainer.Count; }
        }

        public int TotalCollidingCount
        {
            get { return totalCollidingCount; }
        }

        #endregion

        /// <summary>
        /// Creates a new collision layer using the name.
        /// </summary>
        /// <param name="name">The layer name</param>
        public CollisionLayer AddLayer(string name)
        {
            CollisionLayer newLayer = new CollisionLayer();
            newLayer.Name = name;
            newLayer.MakeId();

            collideLayerContainer.Add(newLayer);

            return newLayer;
        }

        /// <summary>
        /// Get a collison layer using the ID number.
        /// </summary>
        /// <param name="id">ID number</param>
        public CollisionLayer GetLayer(int id)
        {
            for (int i = 0; i < collideLayerContainer.Count; i++)
            {
                if( id == collideLayerContainer[i].Id)
                    return collideLayerContainer[i];
            }

            return null;
        }

        /// <summary>
        /// Get a collison layer using the name.
        /// </summary>
        /// <param name="layerName">The layer name</param>
        public CollisionLayer GetLayer(string layerName)
        {
            for (int i = 0; i < collideLayerContainer.Count; i++)
            {
                if (layerName == collideLayerContainer[i].Name)
                    return collideLayerContainer[i];
            }

            return null;
        }

        /// <summary>
        /// Remove all collsion layers.
        /// </summary>
        public void ClearAllLayer()
        {
            collideLayerContainer.Clear();
        }

        /// <summary>
        /// It tests for collision among the collision elements which 
        /// have been registered to the collision layer and returns the result.
        /// </summary>
        /// <param name="collide">Source collsion element</param>
        /// <param name="idLayer">Destination collison layer ID number</param>
        /// <param name="resultType">type of result</param>
        /// <returns>A result report</returns>
        public CollisionResult HitTest(CollideElement collide, int idLayer,
                                       ResultType resultType)
        {
            //  Get the collide layer
            CollisionLayer layer = GetLayer(idLayer);

            return HitTest(collide, ref layer, resultType);
        }

        /// <summary>
        /// It tests for collision among the collision elements which 
        /// have been registered to the collision layer and returns the result.
        /// </summary>
        /// <param name="collide">Source collsion element</param>
        /// <param name="targetLayer">Target collison layer</param>
        /// <param name="resultType">type of result</param>
        /// <returns>A result report</returns>
        public CollisionResult HitTest(CollideElement collide, 
                           ref CollisionLayer targetLayer,
                           ResultType resultType)
        {
            CollisionResult result = null; 
            tempResult.Clear();
            totalCollidingCount = 0;

            if (activeOn == false)
                return null;

            if (collide == null)
            {
                throw new ArgumentNullException("collide");
            }

            if (targetLayer == null)
            {
                throw new ArgumentNullException("targetLayer");
            }

            //  checking all collisions in current collision layer
            for (int i = 0; i < targetLayer.CollideCount; i++)
            {
                CollideElement targetCollide = targetLayer.GetCollide(i);

                //  Skip ifself
                if (collide.Equals(targetCollide))
                {
                    continue;
                }
                else if (collide.Id != 0 && targetCollide.Id != 0)
                {
                    if (collide.Id == targetCollide.Id)
                        continue;
                }

                //   If source collision is BoundingSphere
                if (collide is CollideSphere)
                {
                    CollideSphere sourceCollideSphere = collide as CollideSphere;

                    //  Test with target sphere
                    if (targetCollide is CollideSphere)         
                    {
                        CollideSphere targetCollideSphere = 
                                                targetCollide as CollideSphere;

                        TestSphereintersectSphere(sourceCollideSphere,
                                                targetCollideSphere, ref tempResult);
                    }
                    //  Test with target model
                    else if (targetCollide is CollideModel)     
                    {
                        CollideModel targetCollideModel = 
                                                targetCollide as CollideModel;

                        TestSphereintersectModel(sourceCollideSphere,
                                                targetCollideModel, ref tempResult);
                    }
                    //  Test with target box
                    else if (targetCollide is CollideBox)
                    {
                        CollideBox targetCollideBox = targetCollide as CollideBox;

                        TestSphereintersectBox(sourceCollideSphere,
                                            targetCollideBox, ref tempResult);
                    }
                    //  Test with target ray
                    if (targetCollide is CollideRay)                  
                    {
                        CollideRay targetCollideRay = 
                                            targetCollide as CollideRay;

                        TestRayintersectSphere(targetCollideRay,
                                            sourceCollideSphere, ref tempResult);
                    }
                }
                //   If source collision is Ray
                else if (collide is CollideRay)
                {
                    CollideRay sourceCollideRay = collide as CollideRay;

                    //  Test with target model
                    if (targetCollide is CollideModel)                  
                    {
                        CollideModel targetCollideModel = 
                                            targetCollide as CollideModel;

                        TestRayintersectModel(sourceCollideRay,
                                            targetCollideModel, ref tempResult);
                    }
                    //  Test with target sphere
                    else if (targetCollide is CollideSphere)            
                    {
                        CollideSphere targetCollideSphere = 
                                            targetCollide as CollideSphere;

                        TestRayintersectSphere(sourceCollideRay,
                                            targetCollideSphere, ref tempResult);
                    }
                    //  Test with target box
                    else if (targetCollide is CollideBox)               
                    {
                        CollideBox targetCollideBox = targetCollide as CollideBox;

                        TestRayintersectBox(sourceCollideRay,
                                            targetCollideBox, ref tempResult);
                    }
                }
                //   If source collision is Ray
                else if (collide is CollideBox)
                {
                    CollideBox sourceCollideBox = collide as CollideBox;

                    //  Test with target sphere
                    if (targetCollide is CollideSphere)
                    {
                        CollideSphere targetCollideSphere =
                                                targetCollide as CollideSphere;

                        TestSphereintersectBox(targetCollideSphere,
                                                sourceCollideBox, ref tempResult);
                    }
                    //  Test with target box
                    else if (targetCollide is CollideBox)
                    {
                        CollideBox targetCollideBox = targetCollide as CollideBox;

                        TestBoxintersectBox(sourceCollideBox,
                                            targetCollideBox, ref tempResult);
                    }
                    //  Test with target ray
                    else if (targetCollide is CollideRay)
                    {
                        CollideRay targetCollideRay = targetCollide as CollideRay;

                        TestRayintersectBox(targetCollideRay,
                                            sourceCollideBox, ref tempResult);
                    }
                }

                //  To find the nearest detected collision.
                if (resultType == ResultType.NearestOne)
                {                 
                    if (tempResult.collideCount > 0)
                    {
                        if(result == null)
                        {
                            result = new CollisionResult();
                            result.distance = float.MaxValue;
                        }

                        if (result.distance > tempResult.distance)
                        {
                            tempResult.CopyTo(ref result);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// It checks for the collision between two collision spheres.
        /// </summary>
        /// <param name="sourceCollide">Source collision sphere</param>
        /// <param name="targetCollide">Target collision sphere</param>
        /// <param name="result">A result report</param>
        /// <returns>True if there is a collision</returns>
        public bool TestSphereintersectSphere(CollideSphere sourceCollide, 
                                CollideSphere targetCollide, ref CollisionResult result)
        {
            totalCollidingCount++;

            // Test sphere with the other sphere
            if (sourceCollide.BoundingSphere.Intersects(targetCollide.BoundingSphere))
            {
                if (result != null)
                {
                    float twoSphereDistance = Vector3.Distance(
                                                targetCollide.BoundingSphere.Center, 
                                                sourceCollide.BoundingSphere.Center);

                    Vector3 twoSphereDirection = Vector3.Normalize(
                                                targetCollide.BoundingSphere.Center -
                                                sourceCollide.BoundingSphere.Center);

                    result.distance = Math.Abs(twoSphereDistance) -
                                        (sourceCollide.Radius + targetCollide.Radius);

                    result.detectedCollide = targetCollide;
                    result.intersect = twoSphereDirection * result.distance;
                    result.collideCount++;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// It checks for the collision between a collision sphere and a collision model.
        /// </summary>
        /// <param name="sourceCollide">Source collision sphere</param>
        /// <param name="targetCollide">Target collision model</param>
        /// <param name="result">A result report</param>
        /// <returns>True if there is a collision</returns>
        public bool TestSphereintersectModel(CollideSphere sourceCollide, 
                                CollideModel targetCollide, ref CollisionResult result)
        {
            Vector3 intersect;
            Vector3 normal;
            float distance;

            
            BoundingSphere sphere = sourceCollide.BoundingSphere;

            //  use quad tree.
            if (targetCollide.QuadTree != null)
            {
                if( TestUsingQuadTree((CollideElement)sourceCollide, 
                                        targetCollide.QuadTree.RootNode,
                                        out intersect,
                                        out normal,
                                        out distance))
                {
                    result.detectedCollide = targetCollide;
                    result.intersect = intersect;
                    result.normal = normal;
                    result.distance = distance;
                    result.collideCount++;

                    return true;
                }
            }
            // Hit test sphere with the model
            else
            {                
                if( TestSphereintersectModel(sphere, targetCollide.Vertices, 
                                            targetCollide.TransformMatrix,
                                            out intersect, out normal,
                                            out distance))
                {
                    result.detectedCollide = targetCollide;
                    result.intersect = intersect;
                    result.normal = normal;
                    result.distance = distance;
                    result.collideCount++;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// It checks for the collision between a collision ray and a collision model.
        /// </summary>
        /// <param name="sourceCollide">Source collision ray</param>
        /// <param name="targetCollide">Target collision model</param>
        /// <param name="result">A result report</param>
        /// <returns>True if there is a collision</returns>
        public bool TestRayintersectModel(CollideRay sourceCollide, 
                                                CollideModel targetCollide, 
                                                ref CollisionResult result)
        {
            Vector3 intersect;
            Vector3 normal;
            float distance;

            //  use quad tree.
            if (targetCollide.QuadTree != null)
            {
                if( TestUsingQuadTree((CollideElement)sourceCollide, 
                                        targetCollide.QuadTree.RootNode,
                                        out intersect,
                                        out normal,
                                        out distance))
                {
                    result.detectedCollide = targetCollide;
                    result.intersect = intersect;
                    result.normal = normal;
                    result.distance = distance;
                    result.collideCount++;

                    return true;
                }
            }
            // Test ray with the model
            else
            {
                if( TestRayintersectModel(sourceCollide.Ray, targetCollide.Vertices,
                                            targetCollide.TransformMatrix,
                                            out intersect,
                                            out normal,
                                            out distance))
                {
                    result.distance = distance;
                    result.detectedCollide = targetCollide;
                    result.intersect = intersect;
                    result.normal = normal;
                    result.collideCount++;
                }
                                        
            }

            return false;
        }

        /// <summary>
        /// It checks for the collision between a collision ray and a collision box.
        /// </summary>
        /// <param name="sourceCollide">Source collision ray</param>
        /// <param name="targetCollide">Target collision box</param>
        /// <param name="result">A result report</param>
        /// <returns>True if there is a collision</returns>
        public  bool TestRayintersectBox(CollideRay sourceCollide, 
                                                CollideBox targetCollide, 
                                                ref CollisionResult result)
        {
            totalCollidingCount++;

            // Test ray with the box
            float? distance = sourceCollide.Ray.Intersects(targetCollide.BoundingBox);
            if (distance != null)
            {
                if (result != null)
                {
                    result.distance = (float)distance;
                    result.detectedCollide = targetCollide;
                    result.intersect = null;
                    result.normal = null;
                    result.collideCount++;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// It checks for the collision between a collision sphere and a collision box.
        /// </summary>
        /// <param name="sourceCollide">Source collision ray</param>
        /// <param name="targetCollide">Target collision box</param>
        /// <param name="result">A result report</param>
        /// <returns>True if there is a collision</returns>
        public bool TestSphereintersectBox(CollideSphere sourceCollide,
                                                CollideBox targetCollide,
                                                ref CollisionResult result)
        {
            totalCollidingCount++;

            // Test sphere with the box
            if (sourceCollide.BoundingSphere.Intersects(targetCollide.BoundingBox))
            {
                if (result != null)
                {
                    Vector3 centerBox = 0.5f * 
                        (targetCollide.BoundingBox.Max + targetCollide.BoundingBox.Min);

                    result.distance = (float)Vector3.Distance(
                        sourceCollide.BoundingSphere.Center, centerBox)
                        - sourceCollide.BoundingSphere.Radius;

                    result.detectedCollide = targetCollide;
                    result.intersect = null;
                    result.normal = null;
                    result.collideCount++;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// It checks for two collision boxes.
        /// </summary>
        /// <param name="sourceCollide">Source collision box</param>
        /// <param name="targetCollide">Target collision box</param>
        /// <param name="result">A result report</param>
        /// <returns>True if there is a collision</returns>
        public bool TestBoxintersectBox(CollideBox sourceCollide,
                                                CollideBox targetCollide,
                                                ref CollisionResult result)
        {
            totalCollidingCount++;

            // // Test two boxes
            if (sourceCollide.BoundingBox.Intersects(targetCollide.BoundingBox))
            {
                if (result != null)
                {
                    Vector3 centerSourceBox = 0.5f *
                        (sourceCollide.BoundingBox.Max + sourceCollide.BoundingBox.Min);
                    
                    Vector3 centerTargetBox = 0.5f *
                        (targetCollide.BoundingBox.Max + targetCollide.BoundingBox.Min);

                    result.distance = 
                            (float)Vector3.Distance(centerSourceBox, centerTargetBox);
                        
                    result.detectedCollide = targetCollide;
                    result.intersect = null;
                    result.normal = null;
                    result.collideCount++;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// It checks for the collision between a collision ray and a collision sphere.
        /// </summary>
        /// <param name="sourceCollide">Source collision ray</param>
        /// <param name="targetCollide">Target collision sphere</param>
        /// <param name="result">A result report</param>
        /// <returns>True if there is a collision</returns>
        public bool TestRayintersectSphere(CollideRay sourceCollide, 
                                            CollideSphere targetCollide, 
                                            ref CollisionResult result)
        {
            totalCollidingCount++;

            // Test ray with the sphere
            float? distance = sourceCollide.Ray.Intersects(targetCollide.BoundingSphere);
            if (distance != null)
            {
                if (result != null)
                {
                    Vector3 dir = Vector3.Normalize(sourceCollide.Ray.Position - 
                                                    targetCollide.BoundingSphere.Center);

                    Vector3 length = dir * targetCollide.Radius;

                    result.distance = (float)distance;
                    result.detectedCollide = targetCollide;                    
                    result.intersect = targetCollide.BoundingSphere.Center + length;
                    result.normal = null;
                    result.collideCount++;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// It checks for the collision between a collision sphere and a collision model.
        /// </summary>
        protected bool TestSphereintersectModel(BoundingSphere sphere, 
                                                Vector3[] vertices,
                                                Matrix transform,
                                                out Vector3 intersect,
                                                out Vector3 normal,
                                                out float distance)
        {
            distance = 0.0f;
            intersect = Vector3.Zero;
            normal = Vector3.Zero;

            for (int i = 0; i < vertices.Length; i += 3)
            {
                // Transform the three vertex positions into world space
                Vector3 v1 = Vector3.Transform(vertices[i], transform);

                Vector3 v2 = Vector3.Transform(vertices[i + 1], transform);

                Vector3 v3 = Vector3.Transform(vertices[i + 2], transform);

                totalCollidingCount++;

                // Check collision
                if (Helper3D.SphereIntersectTriangle(sphere.Center, sphere.Radius,
                                                    v1, v2, v3,
                                                    out intersect, out distance))
                {
                    normal = Vector3.Normalize(Vector3.Cross(v3 - v1, v2 - v1));

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// It checks for the collision between a collision ray and a collision model.
        /// </summary>
        protected bool TestRayintersectModel(Ray ray, Vector3[] vertices,
                                            Matrix transform,
                                            out Vector3 intersect,
                                            out Vector3 normal,
                                            out float distance)
        {
            Triangle outTriangle = new Triangle(Vector3.Zero, Vector3.Zero, 
                Vector3.Zero);
            distance = 0.0f;
            intersect = Vector3.Zero;
            normal = Vector3.Zero;

            totalCollidingCount += vertices.Length;

            // Test ray with the model
            float? checkDistance = Helper3D.RayIntersectTriangle(ray, vertices, 
                transform, out outTriangle);

            if (checkDistance != null)
            {
                // Retry test for intersect point and normal
                return Helper3D.PointIntersect(ray.Position, ray.Direction, outTriangle,
                                    out distance, out intersect, out normal);
            }

            return false;
        }


        protected bool TestUsingQuadTree(CollideElement sourceCollide,
                                        QuadNode quadNode,
                                        out Vector3 intersect,
                                        out Vector3 normal,
                                        out float distance)
        {
            bool result = false;

            float tempDistance = 0.0f;
            Vector3 tempIntersect = Vector3.Zero;
            Vector3 tempNormal = Vector3.Zero;

            float closestDistance = float.MaxValue;
            Vector3 closestIntersection = Vector3.Zero;
            Vector3 closestNormal = Vector3.Zero;

            distance = 0.0f;
            intersect = Vector3.Zero;
            normal = Vector3.Zero;

            //  checks upper left node.
            if (quadNode.UpperLeftNode != null)
            {
                if (TestUsingQuadTree(sourceCollide, quadNode.UpperLeftNode,
                                        out tempIntersect, out tempNormal, 
                                        out tempDistance))
                {
                    result = true;

                    //  checks closest
                    if (closestDistance > tempDistance)
                    {
                        closestDistance = tempDistance;
                        closestIntersection = tempIntersect;
                        closestNormal = tempNormal;
                    }
                }
            }

            //  checks upper right node.
            if (quadNode.UpperRightNode != null)
            {
                if (TestUsingQuadTree(sourceCollide, quadNode.UpperRightNode,
                                        out tempIntersect, out tempNormal, 
                                        out tempDistance))
                {
                    result = true;

                    //  checks closest
                    if (closestDistance > tempDistance)
                    {
                        closestDistance = tempDistance;
                        closestIntersection = tempIntersect;
                        closestNormal = tempNormal;
                    }
                }
            }

            //  checks lower left node.
            if (quadNode.LowerLeftNode != null)
            {
                if (TestUsingQuadTree(sourceCollide, quadNode.LowerLeftNode,
                                        out tempIntersect, out tempNormal, 
                                        out tempDistance))
                {
                    result = true;

                    //  checks closest
                    if (closestDistance > tempDistance)
                    {
                        closestDistance = tempDistance;
                        closestIntersection = tempIntersect;
                        closestNormal = tempNormal;
                    }
                }
            }

            //  checks lower right node.
            if (quadNode.LowerRightNode != null)
            {
                if (TestUsingQuadTree(sourceCollide, quadNode.LowerRightNode,
                                        out tempIntersect, out tempNormal, 
                                        out tempDistance))
                {
                    result = true;

                    //  checks closest
                    if (closestDistance > tempDistance)
                    {
                        closestDistance = tempDistance;
                        closestIntersection = tempIntersect;
                        closestNormal = tempNormal;
                    }
                }
            }
            //  checks vertices in quad node.
            if (quadNode.Contains(ref sourceCollide))
            {
                //  checks vertices with bounding sphere.
                if (sourceCollide is CollideSphere)
                {
                    CollideSphere collide = sourceCollide as CollideSphere;

                    // Hit test sphere with the model
                    BoundingSphere sphere = collide.BoundingSphere;

                    if (quadNode.Vertices != null)
                    {
                        if (TestSphereintersectModel(sphere, quadNode.Vertices,
                                            Matrix.Identity,
                                            out tempIntersect, out tempNormal,
                                            out tempDistance))
                        {
                            result = true;

                            //  checks closest
                            if (closestDistance > tempDistance)
                            {
                                closestDistance = tempDistance;
                                closestIntersection = tempIntersect;
                                closestNormal = tempNormal;
                            }
                        }
                    }
                }
                //  checks vertices with ray.
                else if (sourceCollide is CollideRay)
                {
                    CollideRay collide = sourceCollide as CollideRay;

                    if (quadNode.Vertices != null)
                    {
                        if (TestRayintersectModel(collide.Ray, quadNode.Vertices,
                                            Matrix.Identity,
                                            out tempIntersect, out tempNormal,
                                            out tempDistance))
                        {
                            result = true;

                            //  checks closest
                            if (closestDistance > tempDistance)
                            {
                                closestDistance = tempDistance;
                                closestIntersection = tempIntersect;
                                closestNormal = tempNormal;
                            }
                        }
                    }
                }
            }

            //  resolve final result.
            if (result)
            {
                distance = closestDistance;
                intersect = closestIntersection;
                normal = closestNormal;
            }

            return result;
        }
    }
}
