#region File Description
//-----------------------------------------------------------------------------
// CollisionMesh.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion



namespace BoxCollider
{

    struct CustomVertex : IVertexType
    {
        public Vector3 Position;
        public Vector4 Normal;
        public Vector4 Binormal;
        public Vector3 Tangent;


        public CustomVertex(
            Vector3 position,
            Vector4 normal,
            Vector4 binormal,
            Vector3 tangent)
        {
            Position = position;
            Normal = normal;
            Binormal = binormal;
            Tangent = tangent;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.Normal, 0),
            new VertexElement(28, VertexElementFormat.Vector4, VertexElementUsage.Binormal, 0),
            new VertexElement(44, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return CustomVertex.VertexDeclaration; }
        }

    }

    public class CollisionMesh
    {
        // mesh vertices
        Vector3[] vertices;
        // mesh faces
        CollisionFace[] faces;
        // tree with meshes faces
        CollisionTree tree;

        public CollisionMesh(Model model, uint subdivLevel)
        {
            int verticesCapacity = 0;
            int facesCapacity = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    verticesCapacity += part.VertexBuffer.VertexCount;
                    facesCapacity += part.PrimitiveCount;
                }
            }

            vertices = new Vector3[verticesCapacity];
            faces = new CollisionFace[facesCapacity];

            int verticesLength = 0;
            int facesLength = 0;

            Matrix[] modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix meshTransform = modelTransforms[mesh.ParentBone.Index];

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    int vertexCount = part.VertexBuffer.VertexCount;
                    CustomVertex[] partVertices = new CustomVertex[vertexCount];
                    part.VertexBuffer.GetData(partVertices);

                    for (int i = 0; i < vertexCount; i++)
                    {
                        vertices[verticesLength + i] =
                            Vector3.Transform(partVertices[i].Position, meshTransform);
                    }

                    int indexCount = part.IndexBuffer.IndexCount;
                    short[] partIndices = new short[indexCount];
                    part.IndexBuffer.GetData(partIndices);

                    for (int i = 0; i < part.PrimitiveCount; i++)
                    {
                        faces[facesLength + i] = new CollisionFace(
                            part.StartIndex + i * 3, partIndices,
                            verticesLength + part.VertexOffset, vertices);
                    }

                    verticesLength += vertexCount;
                    facesLength += part.PrimitiveCount;

                }

            }

            CollisionBox box = new CollisionBox(float.MaxValue, -float.MaxValue);
            for (int i = 0; i < verticesCapacity; i++)
                box.AddPoint(vertices[i]);

            if (subdivLevel > 6)
                subdivLevel = 6; // max 8^6 nodes
            tree = new CollisionTree(box, subdivLevel);
            for (int i = 0; i < facesCapacity; i++)
                tree.AddElement(faces[i]);
        }

        public bool PointIntersect(
            Vector3 rayStart,
            Vector3 rayEnd,
            out float intersectDistance,
            out Vector3 intersectPosition,
            out Vector3 intersectNormal)
        {
            return tree.PointIntersect(rayStart, rayEnd, vertices,
                out intersectDistance, out intersectPosition, out intersectNormal);
        }

        public bool BoxIntersect(
            CollisionBox box,
            Vector3 rayStart,
            Vector3 rayEnd,
            out float intersectDistance,
            out Vector3 intersectPosition,
            out Vector3 intersectNormal)
        {
            return tree.BoxIntersect(box, rayStart, rayEnd, vertices,
                out intersectDistance, out intersectPosition, out intersectNormal);
        }

        public void PointMove(
            Vector3 pointStart,
            Vector3 pointEnd,
            float frictionFactor,
            float bumpFactor,
            uint recurseLevel,
            out Vector3 pointResult)
        {
            tree.PointMove(pointStart, pointEnd, vertices,
                frictionFactor, bumpFactor, recurseLevel,
                out pointResult);
        }

        public bool BoxMove(
            CollisionBox box, Vector3 pointStart, Vector3 pointEnd,
            float frictionFactor, float bumpFactor, uint recurseLevel,
            out Vector3 pointResult)
        {
            return tree.BoxMove(box, pointStart, pointEnd,
                vertices, frictionFactor, bumpFactor, recurseLevel,
                out pointResult);
        }

        public void GetElements(CollisionBox b, List<CollisionTreeElem> e)
        {
            tree.GetElements(b, e);
        }

        public void AddElement(CollisionTreeElem e)
        {
            tree.AddElement(e);
        }

        public void RemoveElement(CollisionTreeElemDynamic e)
        {
            tree.RemoveElement(e);
        }
    }
}
