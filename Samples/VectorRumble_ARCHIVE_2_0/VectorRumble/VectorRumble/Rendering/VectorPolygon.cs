#region File Description
//-----------------------------------------------------------------------------
// VectorPolygon.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// A series of points that may be drawn together to form a line.
    /// </summary>
    class VectorPolygon
    {
        #region Fields and Properties
        /// <summary>
        /// The raw set of points, in "model space".
        /// </summary>
        private Vector2[] points;
        public Vector2[] Points
        {
            get { return points; }
        }

        /// <summary>
        /// The transformed points, typically in "world" space
        /// </summary>
        private Vector2[] transformedPoints;
        public Vector2[] TransformedPoints
        {
            get { return transformedPoints; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new VectorPolygon object from the given points.
        /// </summary>
        /// <param name="points">The raw set of points.</param>
        public VectorPolygon(Vector2[] points)
        {
            this.points = points;
            this.transformedPoints = (Vector2[])points.Clone();
        }
        #endregion

        #region Transformation
        /// <summary>
        /// Transform the raw points by the matrix given.
        /// </summary>
        /// <param name="matrix">The transformation matrix.</param>
        public void Transform(Matrix matrix)
        {
            Vector2.Transform(points, 0, ref matrix, transformedPoints, 0,
                points.Length);
        }
        #endregion

        #region Static Constructors
        private static Random random = new Random();
        
        
        /// <summary>
        /// Creates a polygon in the shape of a circle.
        /// </summary>
        /// <param name="center">The offset of the center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="segments">The number of segments used in the circle.</param>
        /// <returns>A new VectorPolygon object in the shape of a circle.</returns>
        public static VectorPolygon CreateCircle(Vector2 center, float radius, 
            int segments)
        {
            Vector2[] points = new Vector2[segments];
            float angle = MathHelper.TwoPi / points.Length;

            for (int i = 0; i <= points.Length - 1; i++)
            {
                points[i] = new Vector2(
                    center.X + radius * (float)Math.Round(Math.Sin(angle * i), 4), 
                    center.Y + radius * (float)Math.Round(Math.Cos(angle * i), 4));
            }

            return new VectorPolygon(points);
        }


        /// <summary>
        /// Create a polygon shaped like a player.
        /// </summary>
        /// <returns>A new VectorPolygon object in the shape of a player.</returns>
        public static VectorPolygon CreatePlayer()
        {
            Vector2[] points = new Vector2[10];

            points[0] = new Vector2(-15, -2);
            points[1] = new Vector2(-11, -6);
            points[2] = new Vector2(-7, -2);
            points[3] = new Vector2(0, -9);
            points[4] = new Vector2(7, -2);
            points[5] = new Vector2(11, -6);
            points[6] = new Vector2(15, -2);
            points[7] = new Vector2(4, 9);
            points[8] = new Vector2(0, 5);
            points[9] = new Vector2(-4, 9);

            return new VectorPolygon(points);
        }
        
        
        /// <summary>
        /// Create a polygon shaped like a rocket.
        /// </summary>
        /// <returns>A new VectorPolygon object in the shape of a rocket.</returns>
        public static VectorPolygon CreateRocket()
        {
            Vector2[] points = new Vector2[4];
            points[0] = new Vector2(0, -6);
            points[1] = new Vector2(6, 0);
            points[2] = new Vector2(0, 10);
            points[3] = new Vector2(-6, 0);

            return new VectorPolygon(points);
        }


        /// <summary>
        /// Create a polygon shaped like a mine.
        /// </summary>
        /// <returns>A new VectorPolygon object in the shape of a mine.</returns>
        public static VectorPolygon CreateMine()
        {
            Vector2[] points = new Vector2[12];

            points[0] = new Vector2(0, -32);
            points[1] = new Vector2(16, -16);
            points[2] = new Vector2(32, 0);
            points[3] = new Vector2(16, 16);
            points[4] = new Vector2(0, 32);
            points[5] = new Vector2(-16, 16);
            points[6] = new Vector2(-32, 0);
            points[7] = new Vector2(-16, -16);
            points[8] = new Vector2(16, -16);
            points[9] = new Vector2(16, 16);
            points[10] = new Vector2(-16, 16);
            points[11] = new Vector2(-16, -16);

            Matrix scale = Matrix.CreateScale(0.33f);

            Vector2.Transform(points, ref scale, points);

            return new VectorPolygon(points);
        }


        /// <summary>
        /// Create a polygon shaped like an asteroid.
        /// </summary>
        /// <param name="radius">The radius of the asteroid.</param>
        /// <returns>A new VectorPolygon object in the shape of an asteroid.</returns>
        public static VectorPolygon CreateAsteroid(float radius)
        {
            VectorPolygon polygon = CreateCircle(Vector2.Zero, radius, 12);
            for (int i = 0; i < polygon.Points.Length; ++i)
            {
                Vector2 normal = Vector2.Normalize(polygon.Points[i]);
                polygon.Points[i] += normal * ((radius * 0.2f) * 
                    (float)random.NextDouble() - (radius * 0.1f));
            }

            return polygon;
        }
        #endregion
    }
}
