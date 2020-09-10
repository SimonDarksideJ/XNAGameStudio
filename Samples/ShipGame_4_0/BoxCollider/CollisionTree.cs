#region File Description
//-----------------------------------------------------------------------------
// CollisionTree.cs
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
    public class CollisionTree
    {
        // the tree root node
        CollisionTreeNode root;

        // the last recurse id used (for selections without duplicates)
        uint recurseId;

        public CollisionTree(CollisionBox box, uint subdivLevel)
        {
            root = new CollisionTreeNode(box, subdivLevel);
            recurseId = 0;
        }

        public void AddElement(CollisionTreeElem elem)
        {
            root.AddElement(elem);
        }

        public void RemoveElement(CollisionTreeElemDynamic dynamicElem)
        {
            if (dynamicElem != null)
            {
                dynamicElem.RemoveFromNodes();
            }
        }

        public void GetElements(CollisionBox collisionBox, 
            List<CollisionTreeElem> elements)
        {
            root.GetElements(collisionBox, elements, ++recurseId);
        }

        public bool PointMove(
            Vector3 pointStart, Vector3 pointEnd, Vector3[] vertices,
            float frictionFactor, float bumpFactor, uint recurseLevel,
            out Vector3 pointResult)
        {
            pointResult = pointStart;

            Vector3 delta = pointEnd - pointStart;
            float delta_len = delta.Length();
            if (delta_len < 0.00001f)
                return false;

            float total_dist = delta_len;
            delta *= 1.0f / delta_len;

            float bias = 0.01f;

            pointEnd += delta * bias;

            bool collision_hit = false;

            while (recurseLevel > 0)
            {
                float dist;
                Vector3 pos, norm;
                if (false == PointIntersect(pointStart, pointEnd, vertices, 
                                        out dist, out pos, out norm))
                {

                    pointStart = pointEnd - delta * bias;
                    break;
                }

                collision_hit = true;

                dist -= bias / Math.Abs(Vector3.Dot(delta, norm));
                if (dist > 0)
                {
                    pointStart += delta * dist;
                    total_dist -= dist;
                }

                Vector3 reflect_dir = Vector3.Normalize(Vector3.Reflect(delta, norm));

                Vector3 n = norm * Vector3.Dot(reflect_dir, norm);
                Vector3 t = reflect_dir - n;

                reflect_dir = frictionFactor * t + bumpFactor * n;

                pointEnd = pointStart + reflect_dir * total_dist;

                delta = pointEnd - pointStart;
                delta_len = delta.Length();
                if (delta_len < 0.00001f)
                    break;
                delta *= 1.0f / delta_len;

                pointEnd += delta * bias;

                recurseLevel--;
            }

            pointResult = pointStart;
            return collision_hit;
        }

        public bool BoxMove(
            CollisionBox box, 
            Vector3 pointStart, 
            Vector3 pointEnd, 
            Vector3[] vertices,
            float frictionFactor, 
            float bumpFactor, 
            uint recurseLevel,
            out Vector3 pointResult)
        {
            pointResult = pointStart;

            Vector3 delta = pointEnd - pointStart;
            float deltaLength = delta.Length();
            if (deltaLength < 0.00001f)
                return false;

            float totalDistance = deltaLength;
            delta *= 1.0f / deltaLength;

            float bias = 0.01f;

            pointEnd += delta * bias;

            bool collisionHit = false;

            while (recurseLevel > 0)
            {
                float dist;
                Vector3 pos, norm;
                if (false == BoxIntersect(box, pointStart, pointEnd, vertices, 
                                    out dist, out pos, out norm))
                {

                    pointStart = pointEnd - delta * bias;
                    break;
                }

                collisionHit = true;

                dist -= bias / Math.Abs(Vector3.Dot(delta, norm));
                if (dist > 0)
                {
                    pointStart += delta * dist;
                    totalDistance -= dist;
                }

                Vector3 reflectDirection = 
                    Vector3.Normalize(Vector3.Reflect(delta, norm));

                Vector3 n = norm * Vector3.Dot(reflectDirection, norm);
                Vector3 t = reflectDirection - n;

                reflectDirection = frictionFactor * t + bumpFactor * n;

                pointEnd = pointStart + reflectDirection * totalDistance;

                delta = pointEnd - pointStart;
                deltaLength = delta.Length();
                if (deltaLength < 0.00001f)
                    break;
                delta *= 1.0f / deltaLength;

                pointEnd += delta * bias;

                recurseLevel--;
            }

            pointResult = pointStart;
            return collisionHit;
        }

        public bool PointIntersect(
            Vector3 rayStart, 
            Vector3 rayEnd, 
            Vector3[] vertices,
            out float intersectDistance, 
            out Vector3 intersectPosition, 
            out Vector3 intersectNormal)
        {
            intersectDistance = 0.0f;
            intersectPosition = rayStart;
            intersectNormal = Vector3.Zero;

            Vector3 rayDirection = rayEnd - rayStart;
            float rayLength = rayDirection.Length();
            if (rayLength == 0)
                return false;

            CollisionBox rayBox = new CollisionBox(float.MaxValue, -float.MaxValue);
            rayBox.AddPoint(rayStart);
            rayBox.AddPoint(rayEnd);
            Vector3 inflate = new Vector3(0.001f, 0.001f, 0.001f);
            rayBox.min -= inflate;
            rayBox.max += inflate;

            List<CollisionTreeElem> elems = new List<CollisionTreeElem>();
            root.GetElements(rayBox, elems, ++recurseId);

            rayDirection *= 1.0f / rayLength;
            intersectDistance = rayLength;

            bool intersected = false;

            foreach (CollisionTreeElem e in elems)
            {
                float distance;
                Vector3 position;
                Vector3 normal;
                if (true == e.PointIntersect(rayStart, rayDirection, vertices, 
                                    out distance, out position, out normal))
                {
                    if (distance < intersectDistance)
                    {
                        intersectDistance = distance;
                        intersectPosition = position;
                        intersectNormal = normal;
                        intersected = true;
                    }
                }
            }

            return intersected;
        }

        public bool BoxIntersect(
            CollisionBox box, 
            Vector3 rayStart, 
            Vector3 rayEnd, 
            Vector3[] vertices,
            out float intersectDistance, 
            out Vector3 intersectPosition, 
            out Vector3 intersectNormal)
        {
            intersectDistance = 0.0f;
            intersectPosition = rayStart;
            intersectNormal = Vector3.Zero;

            Vector3 rayDirection = rayEnd - rayStart;
            float rayLength = rayDirection.Length();
            if (rayLength == 0)
                return false;

            CollisionBox rayBox = new CollisionBox(box.min + rayStart, 
                                                box.max + rayStart);
            rayBox.AddPoint(rayBox.min + rayDirection);
            rayBox.AddPoint(rayBox.max + rayDirection);
            Vector3 inflate = new Vector3(0.001f, 0.001f, 0.001f);
            rayBox.min -= inflate;
            rayBox.max += inflate;

            List<CollisionTreeElem> elems = new List<CollisionTreeElem>();
            root.GetElements(rayBox, elems, ++recurseId);

            rayDirection *= 1.0f / rayLength;
            intersectDistance = rayLength;

            bool intersected = false;

            foreach (CollisionTreeElem e in elems)
            {
                float distance;
                Vector3 position;
                Vector3 normal;
                if (true == e.BoxIntersect(box, rayStart, rayDirection, vertices, 
                                    out distance, out position, out normal))
                {
                    if (distance < intersectDistance)
                    {
                        intersectDistance = distance;
                        intersectPosition = position;
                        intersectNormal = normal;
                        intersected = true;
                    }
                }
            }

            return intersected;
        }
    }
}
