#region File Description
//-----------------------------------------------------------------------------
// TangentVertex.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Text;
#endregion

namespace RacingGame.Graphics
{
    /// <summary>
    /// Tangent vertex format for shader vertex format used all over the place.
    /// It contains: Position, Normal vector, texture coords, tangent vector.
    /// </summary>
    public struct TangentVertex : IVertexType
    {
        #region Variables
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 pos;
        /// <summary>
        /// Texture coordinates
        /// </summary>
        public Vector2 uv;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 normal;
        /// <summary>
        /// Tangent
        /// </summary>
        public Vector3 tangent;

        /*fixed number to prevent having us to use unsafe code
        /// <summary>
        /// Stride size, in XNA called SizeInBytes. I'm just conforming with that.
        /// Btw: How is this supposed to work without using unsafe AND
        /// without using System.Runtime.InteropServices.Marshal.SizeOf?
        /// </summary>
        public static unsafe int SizeInBytes
        {
            get
            {
                return (int)sizeof(TangentVertex);
            }
        }
         */
        /// <summary>
        /// Stride size, in XNA called SizeInBytes. I'm just conforming with that.
        /// </summary>
        public static int SizeInBytes
        {
            get
            {
                // 4 bytes per float:
                // 3 floats pos, 2 floats uv, 3 floats normal and 3 float tangent.
                return 4 * (3 + 2 + 3 + 3);
            }
        }

        /// <summary>
        /// U texture coordinate
        /// </summary>
        /// <returns>Float</returns>
        public float U
        {
            get
            {
                return uv.X;
            }
        }

        /// <summary>
        /// V texture coordinate
        /// </summary>
        /// <returns>Float</returns>
        public float V
        {
            get
            {
                return uv.Y;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create tangent vertex
        /// </summary>
        /// <param name="setPos">Set position</param>
        /// <param name="setU">Set u texture coordinate</param>
        /// <param name="setV">Set v texture coordinate</param>
        /// <param name="setNormal">Set normal</param>
        /// <param name="setTangent">Set tangent</param>
        public TangentVertex(
            Vector3 setPos,
            float setU, float setV,
            Vector3 setNormal,
            Vector3 setTangent)
        {
            pos = setPos;
            uv = new Vector2(setU, setV);
            normal = setNormal;
            tangent = setTangent;
        }

        /// <summary>
        /// Create tangent vertex
        /// </summary>
        /// <param name="setPos">Set position</param>
        /// <param name="setUv">Set uv texture coordinates</param>
        /// <param name="setNormal">Set normal</param>
        /// <param name="setTangent">Set tangent</param>
        public TangentVertex(
            Vector3 setPos,
            Vector2 setUv,
            Vector3 setNormal,
            Vector3 setTangent)
        {
            pos = setPos;
            uv = setUv;
            normal = setNormal;
            tangent = setTangent;
        }
        #endregion

        #region To string
        /// <summary>
        /// To string
        /// </summary>
        public override string ToString()
        {
            return "TangentVertex(pos=" + pos + ", " +
                "u=" + uv.X + ", " +
                "v=" + uv.Y + ", " +
                "normal=" + normal + ", " +
                "tangent=" + tangent + ")";
        }
        #endregion

        #region Generate vertex declaration
        /// <summary>
        /// Vertex elements for Mesh.Clone
        /// </summary>
        private static readonly VertexElement[] VertexElements =
            GenerateVertexElements();

        /// <summary>
        /// Vertex declaration for vertex buffers.
        /// </summary>
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
            // Construct new vertex declaration with tangent info
            // First the normal stuff (we should already have that)
            new VertexElement(0, VertexElementFormat.Vector3,
                              VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2,
                              VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Vector3,
                              VertexElementUsage.Normal, 0),
            // And now the tangent
            new VertexElement(32, VertexElementFormat.Vector3,
                              VertexElementUsage.Tangent, 0)
            );

        //Implement the IVertexType interface so that we can get the vertex
        //declaration straight from our custom vertex!
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        /// <summary>
        /// Generate vertex declaration
        /// </summary>
        private static VertexElement[] GenerateVertexElements()
        {
            VertexElement[] decl = new VertexElement[]
                {
                    // Construct new vertex declaration with tangent info
                    // First the normal stuff (we should already have that)
                    new VertexElement(0, VertexElementFormat.Vector3,
                        VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector2,
                        VertexElementUsage.TextureCoordinate, 0),
                    new VertexElement(20, VertexElementFormat.Vector3,
                        VertexElementUsage.Normal, 0),
                    // And now the tangent
                    new VertexElement(32, VertexElementFormat.Vector3,
                        VertexElementUsage.Tangent, 0),
                };
            return decl;
        }
        #endregion

        #region Is declaration tangent vertex declaration
        /// <summary>
        /// Returns true if declaration is tangent vertex declaration.
        /// </summary>
        public static bool IsTangentVertexDeclaration(
            VertexElement[] declaration)
        {
            if (declaration == null)
                throw new ArgumentNullException("declaration");

            return
                declaration.Length == 4 &&
                declaration[0].VertexElementUsage == VertexElementUsage.Position &&
                declaration[1].VertexElementUsage ==
                VertexElementUsage.TextureCoordinate &&
                declaration[2].VertexElementUsage == VertexElementUsage.Normal &&
                declaration[3].VertexElementUsage == VertexElementUsage.Tangent;
        }
        #endregion
    }
}
