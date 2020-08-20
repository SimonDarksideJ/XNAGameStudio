//---------------------------------------------------------------------------------------------------------------------
// ShapeOps.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//---------------------------------------------------------------------------------------------------------------------

using System;
using CollisionSample;
using Microsoft.Xna.Framework;


namespace UnitTests
{
    /// <summary>
    /// IShapeOps provides an interface to consistently manipulate various types of bounding shapes,
    /// to avoid duplication of complex test code. Note that this is not a complete and generic
    /// set of operations; it only includes methods necessary to implement our test cases.
    /// </summary>
    interface IShapeOps<T>
    {
        // The shape being managed by this instance
        T Shape { get; set; }

        // Initialize Shape to a randomly sized, positioned, and oriented new object.
        // The "scale" parameter should provide an approximate size.
        void CreateRandomShape(Random random, float scale);

        // Move Shape by the given vector
        void Translate(Vector3 translation);

        // Scale shape uniformly around the origin by the given amount
        void Scale(float scale);

        // Return a point from inside Shape chosen uniformly at random
        Vector3 RandomInteriorPoint(Random random);

        // Returns true iff the given point is inside Shape
        bool ContainsPoint(Vector3 point);

        // Return an array of points whose bounding hull contains Shape.
        Vector3[] GetHull();

        // Return the minimum distance from Shape to the given plane
        // (which will be negative if Shape intersects the halfspace
        // defined by the plane.)
        float MinimumDistanceFromPlane(Plane plane);
    }

    // Implement IShapeOps for BoundingSphere
    class BoundingSphereOps : IShapeOps<BoundingSphere>
    {
        BoundingSphere shape;
        public BoundingSphere Shape { get { return shape; } set { shape = value; } }

        public override string ToString()
        {
            return String.Format("BoundingSphere(new Vector3({0},{1},{2}), {3})", shape.Center.X, shape.Center.Y, shape.Center.Z, shape.Radius);
        }

        public void CreateRandomShape(Random random, float scale)
        {
            shape.Radius = random.NextFloat(0, scale);
            shape.Center = random.PointInCube() * scale;
        }

        public void Translate(Vector3 translation)
        {
            shape.Center += translation;
        }

        public void Scale(float scale)
        {
            shape.Radius *= scale;
            shape.Center *= scale;
        }

        public Vector3 RandomInteriorPoint(Random random)
        {
            return random.PointInSphere() * shape.Radius + shape.Center;
        }

        public bool ContainsPoint(Vector3 point)
        {
            return shape.Contains(point) != ContainmentType.Disjoint;
        }

        public Vector3[] GetHull()
        {
            Vector3 c = shape.Center;
            float r = shape.Radius;
            return new Vector3[]
            {
                c + new Vector3(r, r, r),
                c + new Vector3(-r, r, r),
                c + new Vector3(r, -r, r),
                c + new Vector3(-r, -r, r),
                c + new Vector3(r, r, -r),
                c + new Vector3(-r, r, -r),
                c + new Vector3(r, -r, -r),
                c + new Vector3(-r, -r, -r)
            };
        }

        public float MinimumDistanceFromPlane(Plane plane)
        {
            return plane.DotCoordinate(shape.Center) - shape.Radius;
        }
    }

    // Implement IShapeOps for Triangle
    class TriangleOps : IShapeOps<Triangle>
    {
        Triangle shape;
        public Triangle Shape { get { return shape; } set { shape = value; } }

        public override string ToString()
        {
            return String.Format("Triangle(new Vector3({0},{1},{2}), new Vector3({3},{4},{5}), new Vector3({6},{7},{8}))",
                shape.V0.X, shape.V0.Y, shape.V0.Z,
                shape.V1.X, shape.V1.Y, shape.V1.Z,
                shape.V2.X, shape.V2.Y, shape.V2.Z);
        }

        public void CreateRandomShape(Random random, float scale)
        {
            shape.V0 = random.PointInCube() * scale;
            shape.V1 = random.PointInCube() * scale;
            shape.V2 = random.PointInCube() * scale;
        }

        public void Translate(Vector3 translation)
        {
            shape.V0 += translation;
            shape.V1 += translation;
            shape.V2 += translation;
        }

        public void Scale(float scale)
        {
            shape.V0 *= scale;
            shape.V1 *= scale;
            shape.V2 *= scale;
        }

        public Vector3 RandomInteriorPoint(Random random)
        {
            float u = (float)random.NextDouble();
            float v = (float)random.NextDouble();
            if (u + v > 1.0f)
            {
                u = 1 - u;
                v = 1 - v;
            }
            return (1 - u - v) * shape.V0 + u * shape.V1 + v * shape.V2;
        }

        public bool ContainsPoint(Vector3 point)
        {
            throw new InvalidOperationException();
        }

        public Vector3[] GetHull()
        {
            return new Vector3[] { shape.V0, shape.V1, shape.V2 };
        }

        public float MinimumDistanceFromPlane(Plane plane)
        {
            float d = float.MaxValue;
            foreach (Vector3 v in GetHull())
                d = Math.Min(d, plane.DotCoordinate(v));
            return d;
        }
    }

    // Implement IShapeOps for BoundingOrientedBox
    class BoundingOrientedBoxOps : IShapeOps<BoundingOrientedBox>
    {
        BoundingOrientedBox shape;
        public BoundingOrientedBox Shape { get { return shape; } set { shape = value; } }

        public void CreateRandomShape(Random random, float scale)
        {
            shape.Center = random.PointInCube() * scale;
            shape.HalfExtent = (random.PointInCube() + Vector3.One * 1.01f) * scale * 0.25f;
            shape.Orientation = random.Orientation();
        }

        public void Translate(Vector3 translation)
        {
            shape.Center += translation;
        }

        public void Scale(float scale)
        {
            shape.HalfExtent *= scale;
            shape.Center *= scale;
        }

        public Vector3 RandomInteriorPoint(Random random)
        {
            Vector3 p = new Vector3(
                random.NextFloat(-shape.HalfExtent.X, shape.HalfExtent.X),
                random.NextFloat(-shape.HalfExtent.Y, shape.HalfExtent.Y),
                random.NextFloat(-shape.HalfExtent.Z, shape.HalfExtent.Z));
            Vector3.Transform(ref p, ref shape.Orientation, out p);
            Vector3.Add(ref p, ref shape.Center, out p);
            return p;
        }

        public bool ContainsPoint(Vector3 point)
        {
            return shape.Contains(ref point);
        }

        public Vector3[] GetHull()
        {
            return shape.GetCorners();
        }

        public float MinimumDistanceFromPlane(Plane plane)
        {
            float d = float.MaxValue;
            foreach (Vector3 v in GetHull())
                d = Math.Min(d, plane.DotCoordinate(v));
            return d;
        }
    }

    // Implement IShapeOps for BoundingBox
    class BoundingBoxOps : IShapeOps<BoundingBox>
    {
        BoundingBox shape;
        public BoundingBox Shape { get { return shape; } set { shape = value; } }

        public void CreateRandomShape(Random random, float scale)
        {
            Vector3 center = random.PointInCube() * scale;
            Vector3 size = (random.PointInCube() + Vector3.One * 1.01f) * scale * 0.25f;
            shape.Min = center - size;
            shape.Max = center + size;
        }
        public void SetRandom(Vector3 position, Vector3 scale, Quaternion orientation)
        {
            shape = new BoundingBox(position - scale, position + scale);
        }

        public void Translate(Vector3 translation)
        {
            shape.Min += translation;
            shape.Max += translation;
        }

        public void Scale(float scale)
        {
            shape.Min *= scale;
            shape.Max *= scale;
        }

        public Vector3 RandomInteriorPoint(Random random)
        {
            Vector3 p = new Vector3(
                random.NextFloat(shape.Min.X, shape.Max.X),
                random.NextFloat(shape.Min.Y, shape.Max.Y),
                random.NextFloat(shape.Min.Z, shape.Max.Z));
            return p;
        }

        public bool ContainsPoint(Vector3 point)
        {
            return shape.Contains(point) != ContainmentType.Disjoint;
        }

        public Vector3[] GetHull()
        {
            return shape.GetCorners();
        }

        public float MinimumDistanceFromPlane(Plane plane)
        {
            float d = float.MaxValue;
            foreach (Vector3 v in GetHull())
                d = Math.Min(d, plane.DotCoordinate(v));
            return d;
        }
    }
}
