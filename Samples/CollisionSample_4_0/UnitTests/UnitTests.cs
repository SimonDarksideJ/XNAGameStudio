//---------------------------------------------------------------------------------------------------------------------
// ShapeOps.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//---------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using CollisionSample;
using Microsoft.Xna.Framework;


namespace UnitTests
{
    // Test the collision primitives.
    public class UnitTests
    {
        public int TestsFailed = 0;
        public int TestsPassed = 0;

        void Fail<T1, T2>(string msg, IShapeOps<T1> obj1, IShapeOps<T2> obj2)
        {
            TestsFailed++;
            Console.WriteLine("Failed {0}(new {1}, new {2})", msg, obj1, obj2);
            Debug.WriteLine("Failed {0}(new {1}, new {2})", msg, obj1, obj2);
        }

        void Pass()
        {
            TestsPassed++;
        }

        void Check<T1, T2>(string msg, IShapeOps<T1> obj1, IShapeOps<T2> obj2, Func<T1, T2, bool> checker)
        {
            Check(msg, obj1, obj2, checker, x => x);
        }

        void Check<T1, T2, R>(string msg, IShapeOps<T1> obj1, IShapeOps<T2> obj2, Func<T1, T2, R> checker, Func<R, bool> wanted)
        {
            if (wanted(checker(obj1.Shape, obj2.Shape)))
            {
                Pass();
            }
            else
            {
                Fail(msg, obj1, obj2);
                checker(obj1.Shape, obj2.Shape); // run test again for debugging
            }
        }

        static Vector3 RandomPointOutsideShape<T>(IShapeOps<T> h, T obj, Random random)
        {
            Vector3 p1 = h.RandomInteriorPoint(random);
            Vector3 p2 = h.RandomInteriorPoint(random);

            for (; ; )
            {
                // move p2 on a line away from p1 until it's outside
                p2 = p1 + (p2 - p1) * 1.1f;
                if (!h.ContainsPoint(p2))
                    return p2;
            }
        }

        /// <summary>
        /// Test containment and intersection between randomly created shapes of type T1 and T2.
        /// </summary>
        void TestRandomObjects<T1, T2>(Random random, IShapeOps<T1> h1, IShapeOps<T2> h2, Func<T1, T2, ContainmentType> contains, Func<T1, T2, bool> intersects)
        {
            // Create a random shape of each type
            h1.CreateRandomShape(random, (float)Math.Exp(random.NextDouble() * 4 - 2));
            h2.CreateRandomShape(random, (float)Math.Exp(random.NextDouble() * 4 - 2));

            // First ensure that the shapes are intersecting (or one is inside the other), by picking a point within each
            // and translating so they coincide.
            Vector3 p1 = h1.RandomInteriorPoint(random);
            Vector3 p2 = h2.RandomInteriorPoint(random);

            h2.Translate(p1 - p2);
            Check("Contains", h1, h2, contains, c => c != ContainmentType.Disjoint);
            Check("Intersects", h1, h2, intersects);

            // Next ensure that shape2 is intersecting (not contained or disjoint), by picking another point
            // in shape2, and scaling around p1 until the new point is outside of shape1.
            p2 = h2.RandomInteriorPoint(random);
            while (h1.ContainsPoint(p2))
            {
                p2 = p1 + (p2 - p1) * 1.1f;
                h2.Translate(-p1);
                h2.Scale(1.1f);
                h2.Translate(p1);
            }
            Check("surface Intersects", h1, h2, contains, c => c == ContainmentType.Intersects);
            Check("Intersects", h1, h2, intersects);

            // Ensure that shape2 is fully contained within shape1 by scaling it down
            // around a point in shape1, until its hull is inside shape1
            for (; ; )
            {
                Vector3[] hull = h2.GetHull();
                bool allInside = true;
                foreach (Vector3 point in hull)
                {
                    if (!h1.ContainsPoint(point))
                    {
                        allInside = false;
                        break;
                    }
                }
                if (allInside)
                    break;

                p1 = h1.RandomInteriorPoint(random);
                h2.Translate(-p1);
                h2.Scale(0.9f);
                h2.Translate(p1);
            }

            // Ensure that shape2 is fully disjoint from shape1 by picking a random plane and
            // translating the shapes to opposite sides of it
            Plane plane;
            plane.Normal = Vector3.Normalize(random.PointInSphere());
            plane.D = (float)(random.NextDouble() * 2 - 1) * 100;

            h1.Translate( (-h1.MinimumDistanceFromPlane(plane) + .001f) * plane.Normal);
            plane.D = -plane.D;
            plane.Normal = -plane.Normal;
            h2.Translate( (-h2.MinimumDistanceFromPlane(plane) + .001f) * plane.Normal);

            Check("Disjoint", h1, h2, contains, c => c == ContainmentType.Disjoint);
            Check("!Intersects", h1, h2, intersects, r => !r);
        }

        public void RunTests()
        {
            Random random = new Random(1);
            for (int i = 0; i < 10000; i++)
            {
                TestRandomObjects(
                    random, new BoundingOrientedBoxOps(), new BoundingOrientedBoxOps(),
                    (b1, b2) => b1.Contains(ref b2), (b1, b2) => b1.Intersects(ref b2));

                TestRandomObjects(
                    random, new BoundingOrientedBoxOps(), new BoundingBoxOps(),
                    (b1, b2) => b1.Contains(ref b2), (b1, b2) => b1.Intersects(ref b2));

                TestRandomObjects(
                    random, new BoundingBoxOps(), new BoundingOrientedBoxOps(),
                    (b1, b2) => BoundingOrientedBox.Contains(ref b1, ref b2), (b1, b2) => b2.Intersects(ref b1));

                TestRandomObjects(
                    random, new BoundingBoxOps(), new BoundingBoxOps(),
                    (b1, b2) => b1.Contains(b2), (b1, b2) => b1.Intersects(b2));

                TestRandomObjects(
                    random, new BoundingSphereOps(), new TriangleOps(),
                    (b, t) => TriangleTest.Contains(ref b, ref t),
                    (b, t) => TriangleTest.Intersects(ref b, ref t));

                TestRandomObjects(
                    random, new BoundingBoxOps(), new TriangleOps(),
                    (b, t) => TriangleTest.Contains(ref b, ref t),
                    (b, t) => TriangleTest.Intersects(ref b, ref t.V0, ref t.V1, ref t.V2));

                TestRandomObjects(
                    random, new BoundingOrientedBoxOps(), new TriangleOps(),
                    (b, t) => TriangleTest.Contains(ref b, ref t),
                    (b, t) => TriangleTest.Intersects(ref b, ref t.V0, ref t.V1, ref t.V2));
            }
            Console.WriteLine("Passed: {0} Failed: {1}", TestsPassed, TestsFailed);
            Debug.WriteLine("Passed: {0} Failed: {1}", TestsPassed, TestsFailed);
        }
    }
}
