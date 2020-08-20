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

struct VertexPosNormTexBump
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector3 Tangent;
    public Vector3 Binormal;
    public Vector2 TextureCoordinate;

    public static int SizeInBytes
    {
        get
        {
            return ((sizeof(float) * 3) * 4) + (sizeof(float) * 2);
        }
    }
}

class SphereMesh : IDisposable
{
    VertexPosNormTexBump[] vertices;
    short[][] indices;

    int nVertices;
    int nIndices;
    int nBuffers;

    VertexDeclaration vDecl;
    VertexBuffer vBuffer;
    IndexBuffer[] iBuffer;

    public SphereMesh(Vector3 size, int hFaces, int vFaces)
    {
        CreateSphere(1.0f, hFaces, vFaces);
        Scale(size);
    }

    protected virtual void Dispose(bool all)
    {
        if (vDecl != null)
        {
            vDecl.Dispose();
            vDecl = null;
        }

        if (vBuffer != null)
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

    public void LoadContent(Game game)
    {
        // Create the resources for Graphics here.
        // Need to call this if we lose device for any reason.
        vBuffer = new VertexBuffer(game.GraphicsDevice, typeof(VertexPosNormTexBump), nVertices, BufferUsage.WriteOnly);
        iBuffer = new IndexBuffer[nBuffers];
        for (int i = 0; i < nBuffers; i++)
        {
            iBuffer[i] = new IndexBuffer(game.GraphicsDevice, typeof(short), nIndices, BufferUsage.WriteOnly);
        }

        VertexElement[] elements = new VertexElement[5];
        elements[0] = new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0);
        elements[1] = new VertexElement(0, 12, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0);
        elements[2] = new VertexElement(0, 24, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Tangent, 0);
        elements[3] = new VertexElement(0, 36, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Binormal, 0);
        elements[4] = new VertexElement(0, 48, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0);

        vDecl = new VertexDeclaration(game.GraphicsDevice, elements);

        vBuffer.SetData<VertexPosNormTexBump>(vertices);

        for (int i = 0; i < nBuffers; i++)
        {
            iBuffer[i].SetData<short>(indices[i]);
        }
    }

    public void Render(GraphicsDevice device)
    {
        for (int i = 0; i < nBuffers; i++)
        {
            device.VertexDeclaration = vDecl;
            device.Vertices[0].SetSource(vBuffer, 0, VertexPosNormTexBump.SizeInBytes);
            device.Indices = iBuffer[i];

            device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, nVertices, 0, nIndices - 2);
        }
    }

    void CreateSphere(float radius, int hFaces, int vFaces)
    {
        nVertices = hFaces * (vFaces + 1);
        vertices = new VertexPosNormTexBump[nVertices];
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

                vertices[n].Normal.X = x;
                vertices[n].Normal.Y = y;
                vertices[n].Normal.Z = z;

                vertices[n].TextureCoordinate.X = 1.0f - (float)j / (float)vFaces;
                vertices[n].TextureCoordinate.Y = 1.0f - (float)i / (float)(hFaces - 1);

                // Compute the tangent--required for bump mapping
                float tx = (float)(Math.Sin(phi) * Math.Sin(theta));
                float ty = (float)-Math.Cos(phi);
                float tz = (float)(Math.Sin(phi) * Math.Cos(theta));

                vertices[n].Tangent.X = tx;
                vertices[n].Tangent.Y = ty;
                vertices[n].Tangent.Z = tz;

                vertices[n].Binormal = Vector3.Cross(vertices[n].Normal, vertices[n].Tangent);
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

    void Scale(Vector3 s)
    {
        int i;
        for (i = 0; i < nVertices; i++)
        {
            vertices[i].Position.X *= s.X;
            vertices[i].Position.Y *= s.Y;
            vertices[i].Position.Z *= s.Z;
        }

        // Modify the normals
        Vector3 normalScalar = new Vector3(1.0f / s.X, 1.0f / s.Y, 1.0f / s.Z);
        for (i = 0; i < nVertices; i++)
        {
            Vector3 normal = new Vector3(vertices[i].Normal.X * normalScalar.X,
                                         vertices[i].Normal.Y * normalScalar.Y,
                                         vertices[i].Normal.Z * normalScalar.Z);

            normal.Normalize();
            vertices[i].Normal = normal;
        }
    }
}

