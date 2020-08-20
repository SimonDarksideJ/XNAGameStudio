#region File Description
//-----------------------------------------------------------------------------
// Sphere.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PerformanceMeasuring
{
    public class Sphere
    {
        private SpherePrimitive primitive;

        public Vector3 Position;
        public Vector3 Velocity;
        public Color Color = Color.White;

        public float Radius { get; private set; }

        public BoundingSphere Bounds
        {
            get { return new BoundingSphere(Position, Radius); }
        }

        public Sphere(GraphicsDevice graphics, float radius)
        {
            primitive = new SpherePrimitive(graphics, radius * 2f, 10);
            Radius = radius;
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            primitive.Draw(Matrix.CreateTranslation(Position), view, projection, Color);
        }
    }
}
