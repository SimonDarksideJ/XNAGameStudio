#region File Description
//-----------------------------------------------------------------------------
// RacingGameModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace RacingGameContentProcessors
{
    /// <summary>
    /// RacingGame model processor for x files. Loads models the same way as
    /// the ModelProcessor class, but generates tangents and some additional
    /// data too.
    /// </summary>
    [ContentProcessor(DisplayName = "RacingGame Model (Tangent support)")]
    public class RacingGameModelProcessor : ModelProcessor
    {
        #region Process
        /// <summary>
        /// Process the model
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="context">Context for logging</param>
        /// <returns>Model content</returns>
        public override ModelContent Process(
            NodeContent input, ContentProcessorContext context)
        {
            // First generate tangent data because x files don't store them
            GenerateTangents(input, context);

            // Use the name of the bone for our mesh name if it is not set
            UseParentBoneNameIfMeshNameIsNotSet(input);

            // Store the current selected technique and if the texture uses alpha
            // into the mesh name for each mesh part.
            StoreEffectTechniqueInMeshName(input, context);

            // And let the rest be processed by the default model processor
            return base.Process(input, context);
        }
        #endregion

        #region Generate tangents
        /// <summary>
        /// Generate tangents helper method, x files do not have tangents
        /// exported, we have to generate them ourselfs.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="context">Context for logging</param>
        private void GenerateTangents(
            NodeContent input, ContentProcessorContext context)
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                // Generate trangents for the mesh. We don't want binormals,
                // so null is passed in for the last parameter.
                MeshHelper.CalculateTangentFrames(mesh,
                    VertexChannelNames.TextureCoordinate(0),
                    VertexChannelNames.Tangent(0), null);
            }

            // Go through all childs
            foreach (NodeContent child in input.Children)
            {
                GenerateTangents(child, context);
            }
        }
        #endregion

        #region UseParentBoneNameIfMeshNameIsNotSet
        /// <summary>
        /// Use parent bone's name if mesh's name is not set.
        /// </summary>
        /// <param name="input"></param>
        private void UseParentBoneNameIfMeshNameIsNotSet(NodeContent input)
        {
            if (String.IsNullOrEmpty(input.Name) &&
                input.Parent != null &&
                String.IsNullOrEmpty(input.Parent.Name) == false)
                input.Name = input.Parent.Name;

            foreach (NodeContent node in input.Children)
                UseParentBoneNameIfMeshNameIsNotSet(node);
        }
        #endregion

        #region StoreEffectMaterialsAndTechniques
        /// <summary>
        /// Stores the current selected technique and if the texture uses alpha
        /// into the mesh name for each mesh part.
        /// </summary>
        private void StoreEffectTechniqueInMeshName(
            NodeContent input, ContentProcessorContext context)
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                foreach (GeometryContent geom in mesh.Geometry)
                {
                    EffectMaterialContent effectMaterial =
                        geom.Material as EffectMaterialContent;
                    if (effectMaterial != null)
                    {
                        if (effectMaterial.OpaqueData.ContainsKey("technique"))
                        {
                            // Store technique here! (OpaqueData["technique"] is an
                            // int32) If we have multiple mesh parts in our mesh object,
                            // there will be multiple techniques listed at the end of
                            // our mesh name.
                            input.Name =
                                input.Name + effectMaterial.OpaqueData["technique"];
                        }
                    }
                }
            }

            // Go through all childs
            foreach (NodeContent child in input.Children)
            {
                StoreEffectTechniqueInMeshName(child, context);
            }
        }
        #endregion
    }
}
