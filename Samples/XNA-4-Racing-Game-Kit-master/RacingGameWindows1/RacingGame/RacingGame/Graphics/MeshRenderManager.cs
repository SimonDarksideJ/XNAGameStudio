#region File Description
//-----------------------------------------------------------------------------
// MeshRenderManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using RacingGame.GameLogic;
using RacingGame.Helpers;
using RacingGame.Shaders;
using RacingGame.Tracks;
#endregion

namespace RacingGame.Graphics
{
    /// <summary>
    /// Mesh render manager, a little helper class which allows us to render
    /// all our models with much faster performance through sorting by
    /// material and shader techniques.
    /// 
    /// We keep a list of lists of lists for rendering our RenderableMeshes
    /// sorted by techniques first and materials second.
    /// The most outer list contains all techniques (see
    /// MeshesPerMaterialPerTechniques).
    /// Then the first inner list contains all materials (see MeshesPerMaterial).
    /// And finally the most inner list contains all meshes that use the
    /// technique and material we got.
    /// Additionally we could also sort by shaders, but since all models
    /// use the same shader (normalMapping), that is the only thing we support
    /// here. Improving it to support more shaders is easily possible.
    /// 
    /// All this is created in the Model constructor. At runtime we just
    /// go through these lists and render everything down as quickly as
    /// possible.
    /// </summary>
    public class MeshRenderManager
    {
        #region Remember vertex and index buffers
        /// <summary>
        /// Don't set vertex and index buffers again if they are already set this
        /// frame.
        /// </summary>
        static VertexBuffer lastVertexBufferSet = null;
        static IndexBuffer lastIndexBufferSet = null;
        #endregion

        #region RenderableMesh helper class
        /// <summary>
        /// Renderable mesh, created in the Model constructor and is rendered
        /// when we render all the models at once at the end of each frame!
        /// </summary>
        public class RenderableMesh
        {
            #region Variables
            /// <summary>
            /// Vertex buffer
            /// </summary>
            public VertexBuffer vertexBuffer;
            /// <summary>
            /// Index buffer
            /// </summary>
            public IndexBuffer indexBuffer;
            /// <summary>
            /// Material
            /// </summary>
            public Material material;
            /// <summary>
            /// Used technique
            /// </summary>
            public EffectTechnique usedTechnique;
            /// <summary>
            /// World parameter
            /// </summary>
            public EffectParameter worldParameter;

            /// <summary>
            /// Stream offset, vertex stride, etc.
            /// All parameters we need for rendering.
            /// </summary>
            public int baseVertex, numVertices, startIndex, primitiveCount;

            /// <summary>
            /// List of render matrices we use every frame. At creation time
            /// this list is unused and empty, but for each frame we use this
            /// list to remember which objects we want to render.
            /// Of course rendering happens only if this list is not empty.
            /// After each frame this list is cleared again.
            /// </summary>
            public List<Matrix> renderMatrices = new List<Matrix>();
            #endregion

            #region Constructor
            /// <summary>
            /// Create renderable mesh
            /// </summary>
            /// <param name="setVertexBuffer">Set vertex buffer</param>
            /// <param name="setIndexBuffer">Set index buffer</param>
            /// <param name="setMaterial">Set material</param>
            /// <param name="setUsedTechnique">Set used technique</param>
            /// <param name="setWorldParameter">Set world parameter</param>
            /// <param name="setBaseVertex">Set base vertex</param>
            /// <param name="setNumVertices">Set number vertices</param>
            /// <param name="setStartIndex">Set start index</param>
            /// <param name="setPrimitiveCount">Set primitive count</param>
            public RenderableMesh(VertexBuffer setVertexBuffer,
                IndexBuffer setIndexBuffer, Material setMaterial,
                EffectTechnique setUsedTechnique, EffectParameter setWorldParameter,
                int setBaseVertex,
                int setNumVertices, int setStartIndex, int setPrimitiveCount)
            {
                vertexBuffer = setVertexBuffer;
                indexBuffer = setIndexBuffer;
                material = setMaterial;
                usedTechnique = setUsedTechnique;
                worldParameter = setWorldParameter;
                baseVertex = setBaseVertex;
                numVertices = setNumVertices;
                startIndex = setStartIndex;
                primitiveCount = setPrimitiveCount;
            }
            #endregion

            #region Render
            /// <summary>
            /// Render this renderable mesh, MUST be called inside of the
            /// render method of ShaderEffect.normalMapping!
            /// </summary>
            /// <param name="worldMatrix">World matrix</param>
            public void RenderMesh(Matrix worldMatrix)
            {
                // Update world matrix
                ShaderEffect.normalMapping.WorldMatrix = worldMatrix;
                ShaderEffect.normalMapping.Effect.CurrentTechnique.Passes[0].Apply();//.Update();

                // Set vertex buffer and index buffer
                if (lastVertexBufferSet != vertexBuffer ||
                    lastIndexBufferSet != indexBuffer)
                {
                    lastVertexBufferSet = vertexBuffer;
                    lastIndexBufferSet = indexBuffer;
                    BaseGame.Device.SetVertexBuffer( vertexBuffer);
                    BaseGame.Device.Indices = indexBuffer;
                }

                // And render (this call takes the longest, we can't optimize
                // it any further because the vertexBuffer and indexBuffer are
                // WriteOnly, we can't combine it or optimize it any more).
                BaseGame.Device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    baseVertex, 0, numVertices, startIndex, primitiveCount);
            }

            /// <summary>
            /// Render
            /// </summary>
            public void Render()
            {
                // Render all meshes we have requested this frame.
                for (int matrixNum = 0; matrixNum < renderMatrices.Count; matrixNum++)
                    RenderMesh(renderMatrices[matrixNum]);

                // Clear all meshes, don't render them again.
                // Next frame everything will be created again.
                renderMatrices.Clear();
            }
            #endregion
        }
        #endregion

        #region MeshesPerMaterial helper class
        /// <summary>
        /// Meshes per material
        /// </summary>
        public class MeshesPerMaterial
        {
            #region Variables
            /// <summary>
            /// Material
            /// </summary>
            public Material material;
            /// <summary>
            /// Meshes
            /// </summary>
            public List<RenderableMesh> meshes = new List<RenderableMesh>();
            #endregion

            #region Properties
            /// <summary>
            /// Number of render matrices this material uses this frame.
            /// </summary>
            /// <returns>Int</returns>
            public int NumberOfRenderMatrices
            {
                get
                {
                    int ret = 0;
                    for (int meshNum = 0; meshNum < meshes.Count; meshNum++)
                        ret += meshes[meshNum].renderMatrices.Count;
                    return ret;
                }
            }
            #endregion

            #region Constructor
            /// <summary>
            /// Create meshes per material for the setMaterial.
            /// </summary>
            /// <param name="setMaterial">Set material</param>
            public MeshesPerMaterial(Material setMaterial)
            {
                material = setMaterial;
            }
            #endregion

            #region Add
            /// <summary>
            /// Adds a renderable mesh using this material.
            /// </summary>
            /// <param name="addMesh">Add mesh</param>
            public void Add(RenderableMesh addMesh)
            {
                // Make sure this mesh uses the correct material
                if (addMesh.material != material)
                    throw new ArgumentException("Invalid material, to add a mesh to " +
                        "MeshesPerMaterial it must use the specified material=" +
                        material);

                meshes.Add(addMesh);
            }
            #endregion

            #region Render
            /// <summary>
            /// Render all meshes that use this material.
            /// This method is only called if we got any meshes to render,
            /// which is determinated if NumberOfRenderMeshes is greater 0.
            /// </summary>
            public void Render()
            {
                // Set material settings. We don't have to update the shader here,
                // it will be done in RenderableMesh.Render anyway because of
                // updating the world matrix!
                ShaderEffect.normalMapping.SetParametersOptimized(material);
                // Set vertex declaration
                //always true: if (meshes.Count > 0)

                // Enable alpha if this material uses alpha
                if (material.HasAlpha)
                {
                    //TODO: AlphaTestEffect
                    //BaseGame.Device.RenderState.AlphaTestEnable = true;
                    //BaseGame.Device.RenderState.ReferenceAlpha = 128;

                    // Make 2sided, we use alpha mainly for our palms.
                    BaseGame.Device.RasterizerState = RasterizerState.CullNone;
                }
                // Render all meshes that use this material.
                for (int meshNum = 0; meshNum < meshes.Count; meshNum++)
                {
                    RenderableMesh mesh = meshes[meshNum];
                    if (mesh.renderMatrices.Count > 0)
                        mesh.Render();
                }

                // Disable alpha testing again and restore culling
                if (material.HasAlpha)
                {
                    //TODO: AlphaTestEffect
                    //BaseGame.Device.RenderState.AlphaTestEnable = false;
                    BaseGame.Device.RasterizerState = RasterizerState.CullCounterClockwise;
                }
            }
            #endregion
        }
        #endregion

        #region MeshesPerMaterialsPerTechniques helper class
        /// <summary>
        /// Meshes per material per techniques
        /// </summary>
        public class MeshesPerMaterialPerTechniques
        {
            #region Variables
            /// <summary>
            /// Technique
            /// </summary>
            public EffectTechnique technique;
            /// <summary>
            /// Meshes per materials
            /// </summary>
            public List<MeshesPerMaterial> meshesPerMaterials =
                new List<MeshesPerMaterial>();
            #endregion

            #region Properties
            /// <summary>
            /// Number of render matrices this technique uses this frame.
            /// </summary>
            /// <returns>Int</returns>
            public int NumberOfRenderMatrices
            {
                get
                {
                    int ret = 0;
                    for (int listNum = 0; listNum < meshesPerMaterials.Count; listNum++)
                        ret += meshesPerMaterials[listNum].NumberOfRenderMatrices;
                    return ret;
                }
            }
            #endregion

            #region Constructor
            /// <summary>
            /// Create meshes per material per techniques
            /// </summary>
            /// <param name="setTechnique">Set technique</param>
            public MeshesPerMaterialPerTechniques(EffectTechnique setTechnique)
            {
                technique = setTechnique;
            }
            #endregion

            #region Add
            /// <summary>
            /// Adds a renderable mesh using this technique.
            /// </summary>
            /// <param name="addMesh">Add mesh</param>
            public void Add(RenderableMesh addMesh)
            {
                // Make sure this mesh uses the correct material
                if (addMesh.usedTechnique != technique)
                    throw new ArgumentException("Invalid technique, to add a mesh to " +
                        "MeshesPerMaterialPerTechniques it must use the specified " +
                        "technique=" + technique.Name);

                // Search for the used material, maybe we have it already in list.
                for (int listNum = 0; listNum < meshesPerMaterials.Count; listNum++)
                {
                    MeshesPerMaterial existingList = meshesPerMaterials[listNum];
                    if (existingList.material == addMesh.material)
                    {
                        // Just add
                        existingList.Add(addMesh);
                        return;
                    }
                }

                // Not found, create new list and add mesh there.
                MeshesPerMaterial newList = new MeshesPerMaterial(addMesh.material);
                newList.Add(addMesh);
                meshesPerMaterials.Add(newList);
            }
            #endregion

            #region Render
            /// <summary>
            /// Render all meshes that use this technique sorted by the materials.
            /// This method is only called if we got any meshes to render,
            /// which is determinated if NumberOfRenderMeshes is greater 0.
            /// </summary>
            /// <param name="effect">Effect</param>
            public void Render(Effect effect)
            {
                // Start effect for this technique
                effect.CurrentTechnique = technique;


                // Render all pass (we always just have one)
                EffectPass pass = effect.CurrentTechnique.Passes[0];

                pass.Apply();
                // Render all meshes sorted by all materials.
                for (int listNum = 0; listNum < meshesPerMaterials.Count; listNum++)
                {
                    MeshesPerMaterial list = meshesPerMaterials[listNum];
                    if (list.NumberOfRenderMatrices > 0)
                        list.Render();
                }
            }
            #endregion
        }
        #endregion

        #region Variables
        /// <summary>
        /// Sorted meshes we got. Everything is sorted by techniques and then
        /// sorted by materials. This all happens at construction time.
        /// For rendering use renderMatrices list, which is directly in the
        /// most inner list of sortedMeshes (the RenderableMesh objects).
        /// </summary>
        List<MeshesPerMaterialPerTechniques> sortedMeshes =
            new List<MeshesPerMaterialPerTechniques>();
        #endregion

        #region Add
        /// <summary>
        /// Add model mesh part with the used effect to our sortedMeshes list.
        /// Neither the model mesh part nor the effect is directly used,
        /// we will extract all data from the model and only render the
        /// index and vertex buffers later.
        /// The model mesh part must use the TangentVertex format.
        /// </summary>
        /// <param name="vertexBuffer">Vertex buffer</param>
        /// <param name="indexBuffer">Index buffer</param>
        /// <param name="part">Part</param>
        /// <param name="effect">Effect</param>
        /// <returns>Renderable mesh</returns>
        public RenderableMesh Add(VertexBuffer vertexBuffer,
            IndexBuffer indexBuffer, ModelMeshPart part, Effect effect)
        {
            string techniqueName = effect.CurrentTechnique.Name;

            // Does this technique already exists?
            MeshesPerMaterialPerTechniques foundList = null;
            for (int listNum = 0; listNum < sortedMeshes.Count; listNum++)
            {
                MeshesPerMaterialPerTechniques list = sortedMeshes[listNum];

                if (list.technique.Name == techniqueName)
                {
                    foundList = list;
                    break;
                }
            }

            // Did not found list? Create new one
            if (foundList == null)
            {
                foundList = new MeshesPerMaterialPerTechniques(
                    ShaderEffect.normalMapping.GetTechnique(techniqueName));
                sortedMeshes.Add(foundList);
            }

            // Create new material from the current effect parameters.
            // This will create duplicate materials if the same material is used
            // multiple times, we check this later.
            Material material = new Material(effect);

            // Search for material inside foundList.
            for (int innerListNum = 0; innerListNum <
                foundList.meshesPerMaterials.Count; innerListNum++)
            {
                MeshesPerMaterial innerList =
                    foundList.meshesPerMaterials[innerListNum];

                // Check if this is the same material and we can use it instead.
                // For our purposes it is sufficiant if we check textures and colors.
                if (innerList.material.diffuseTexture == material.diffuseTexture &&
                    innerList.material.normalTexture == material.normalTexture &&
                    innerList.material.ambientColor == material.ambientColor &&
                    innerList.material.diffuseColor == material.diffuseColor &&
                    innerList.material.specularColor == material.specularColor &&
                    innerList.material.specularPower == material.specularPower)
                {
                    // Reuse this material and quit this search
                    material = innerList.material;
                    break;
                }
            }

            // Build new RenderableMesh object
            RenderableMesh mesh = new RenderableMesh(
                vertexBuffer, indexBuffer, material, foundList.technique,
                ShaderEffect.normalMapping.WorldParameter, part.VertexOffset,
                part.NumVertices, part.StartIndex, part.PrimitiveCount);
            foundList.Add(mesh);
            return mesh;
        }
        #endregion

        #region Render
        /// <summary>
        /// Render all meshes we collected this frame sorted by techniques
        /// and materials. This method is about 3-5 times faster than just
        /// using Model's Mesh.Draw method (see commented out code there).
        /// The reason for that is that we require only very few state changes
        /// and render everthing down as fast as we can. The only optimization
        /// left would be to put vertices of several meshes together if they
        /// are static and use the same technique and material. But since
        /// meshes have WriteOnly vertex and index buffers, we can't do that
        /// without using a custom model format.
        /// </summary>
        public void Render()
        {
            // Make sure z buffer is on
            BaseGame.Device.DepthStencilState = DepthStencilState.Default;

            // We always use the normalMapping shader here.
            Effect effect = ShaderEffect.normalMapping.Effect;
            // Set general parameters for the shader
            ShaderEffect.normalMapping.SetParametersOptimizedGeneral();
            // Don't set vertex buffer again if it does not change this frame.
            // Clear these remember settings.
            lastVertexBufferSet = null;
            lastIndexBufferSet = null;

            for (int listNum = 0; listNum < sortedMeshes.Count; listNum++)
            {
                MeshesPerMaterialPerTechniques list = sortedMeshes[listNum];

                if (list.NumberOfRenderMatrices > 0)
                    list.Render(effect);
            }
        }
        #endregion
    }
}
