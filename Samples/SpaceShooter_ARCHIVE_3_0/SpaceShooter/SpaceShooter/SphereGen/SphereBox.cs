#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SpaceShooter
{
    struct VertexPosTex
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;

        public static int SizeInBytes
        {
            get
            {
                return (sizeof(float) * 3) + (sizeof(float) * 2);
            }
        }
    }

    class SphereBox : IDisposable
    {
        VertexPosTex[] vertices;
        short[][] indices;

        int nVertices;
        int nIndices;
        int nBuffers;

        VertexDeclaration vDecl;
        VertexBuffer vBuffer;
        IndexBuffer[] iBuffer;

        public SphereBox(float size, int hFaces, int vFaces)
        {
            CreateSphere(size, hFaces, vFaces);
        }

        protected virtual void Dispose(bool all)
        {
            if(vDecl != null)
            {
                vDecl.Dispose();
                vDecl = null;
            }

            if(vBuffer != null)
            {
                vBuffer.Dispose();
                vBuffer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void TileUVs(int uScale, int vScale)
        {
            for (int i = 0; i < nVertices; i++)
            {
                vertices[i].TextureCoordinate.X *= uScale;
                vertices[i].TextureCoordinate.Y *= vScale;
            }
        }

        public void LoadContent(Game game)
        {
            // Create the resources for Graphics here.
            // Need to call this if we lose device for any reason.
            vBuffer = new VertexBuffer(game.GraphicsDevice, typeof(VertexPosTex), nVertices, BufferUsage.WriteOnly);
            iBuffer = new IndexBuffer[nBuffers];
            for (int i = 0; i < nBuffers; i++)
            {
                iBuffer[i] = new IndexBuffer(game.GraphicsDevice, typeof(short), nIndices, BufferUsage.WriteOnly);
            }

            VertexElement[] elements = new VertexElement[2];
            elements[0] = new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0);
            elements[1] = new VertexElement(0, 12, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0);

            vDecl = new VertexDeclaration(game.GraphicsDevice, elements);

            vBuffer.SetData<VertexPosTex>(vertices);

            for (int i = 0; i < nBuffers; i++)
            {
                iBuffer[i].SetData<short>(indices[i]);
            }
        }

        public void Render(GraphicsDevice device, Camera camera)
        {
            for (int i = 0; i < nBuffers; i++)
            {
                device.VertexDeclaration = vDecl;
                device.Vertices[0].SetSource(vBuffer, 0, VertexPosTex.SizeInBytes);
                device.Indices = iBuffer[i];

                device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, nVertices, 0, nIndices - 2);
            }
        }

        void CreateSphere(float radius, int hFaces, int vFaces)
        {
            nVertices = hFaces * (vFaces + 1);
            vertices = new VertexPosTex[nVertices];
            nIndices = (vFaces + 1) * 2;
            nBuffers = (hFaces - 1);
            indices = new short[(hFaces - 1)][];

            int i;
            float PI = (float)Math.PI;
            for (i = 0; i < hFaces; i++)
            {
                float phi = ((float)i / (float)(hFaces - 1) - 0.5f) * (float)PI;
                for (int j = 0; j <= vFaces; j++)
                {
                    float theta = (float)j / (float)vFaces * (float)PI * 2;
                    int n = (i * (vFaces + 1)) + j;
                    float x = (float)(Math.Cos(phi) * Math.Cos(theta));
                    float y = (float)Math.Sin(phi);
                    float z = (float)(Math.Cos(phi) * Math.Sin(theta));

                    vertices[n].Position.X = x * radius;
                    vertices[n].Position.Y = y * radius;
                    vertices[n].Position.Z = z * radius;

                    vertices[n].TextureCoordinate.X = 1.0f - (float)j / (float)vFaces;
                    vertices[n].TextureCoordinate.Y = 1.0f - (float)i / (float)(hFaces - 1);
                }
            }

            for (i = 0; i < hFaces - 1; i++)
            {
                indices[i] = new short[nIndices];
                for (int j = 0; j <= vFaces; j++)
                {
                    indices[i][j * 2 + 0] = (short)(i * (vFaces + 1) + j);
                    indices[i][j * 2 + 1] = (short)((i + 1) * (vFaces + 1) + j);
                }
            }
        }
    }
}