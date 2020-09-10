#region File Description
//-----------------------------------------------------------------------------
// ShaderModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
#endregion

namespace ShaderEffectPipeline
{
    /// <summary>
    /// The ShaderModelProcessor is used to change the material/effect applied
    /// to a model.
    /// </summary>
    [ContentProcessor]
    public class ShaderModelProcessor : ModelProcessor
    {
        #region Fields

        public const string NormalMapKey = "NormalMap";
        public const string SpecularMapKey = "SpecularMap";

        String directory;

        #endregion

        public override ModelContent Process(NodeContent input,
            ContentProcessorContext context)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            directory = Path.GetDirectoryName(input.Identity.SourceFilename);

            PreprocessSceneHierarchy(input);            
            return base.Process(input, context);
        }


        /// <summary>
        /// Recursively calls MeshHelper.CalculateTangentFrames for every MeshContent
        /// object in the NodeContent scene. This function could be changed to add 
        /// more per vertex data as needed.
        /// </summary>
        /// <param name="input">A node in the scene.  The function should be called
        /// with the root of the scene.</param>
        private void PreprocessSceneHierarchy(NodeContent input)
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                MeshHelper.CalculateTangentFrames(mesh,
                    VertexChannelNames.TextureCoordinate(0),
                    VertexChannelNames.Tangent(0),
                    VertexChannelNames.Binormal(0));

                LookUpShaderAndAddToTextures(mesh);

            }

            foreach (NodeContent child in input.Children)
            {
                PreprocessSceneHierarchy(child);
            }
        }

        /// <summary>
        /// Looks into the OpaqueData property on the "mesh" object, and looks for a
        /// a string containing the path to the normal map. The normal map is added
        /// to the Textures collection for each of the mesh's materials.
        /// </summary>
        private void LookUpShaderAndAddToTextures(MeshContent mesh)
        {
            //  NormalMap
            {
                string pathToNormalMap =
                    mesh.OpaqueData.GetValue<string>(NormalMapKey, null);

                if (pathToNormalMap == null)
                {
                    throw new InvalidContentException("the normal map is missing!");
                }

                pathToNormalMap = Path.Combine(directory, pathToNormalMap);

                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    if (geometry.Material.Textures.ContainsKey(NormalMapKey) == false)
                    {
                        geometry.Material.Textures.Add(NormalMapKey,
                            new ExternalReference<TextureContent>(pathToNormalMap));
                    }
                }
            }

            //  SpecularMap
            {
                string pathToSpecularMap =
                    mesh.OpaqueData.GetValue<string>(SpecularMapKey, null);

                if (pathToSpecularMap == null)
                {
                    throw new InvalidContentException("the specular map is missing!");
                }

                pathToSpecularMap = Path.Combine(directory, pathToSpecularMap);

                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    if (geometry.Material.Textures.ContainsKey(SpecularMapKey) == false)
                    {
                        geometry.Material.Textures.Add(SpecularMapKey,
                            new ExternalReference<TextureContent>(pathToSpecularMap));
                    }
                }
            }
        }


        // acceptableVertexChannelNames are the inputs that the normal map effect
        // expects.  The ShaderModelProcessor overrides ProcessVertexChannel
        // to remove all vertex channels which don't have one of these four
        // names.
        static IList<string> acceptableVertexChannelNames =
            new string[]
            {
                VertexChannelNames.TextureCoordinate(0),
                VertexChannelNames.Normal(0),
                VertexChannelNames.Binormal(0),
                VertexChannelNames.Tangent(0)
            };


        /// <summary>
        /// As an optimization, ProcessVertexChannel is overriden to remove data which
        /// is not used by the vertex shader.
        /// </summary>
        /// <param name="geometry">the geometry object which contains the 
        /// vertex channel</param>
        /// <param name="vertexChannelIndex">the index of the vertex channel
        /// to operate on</param>
        /// <param name="context">the context that the processor is operating
        /// under.  in most cases, this parameter isn't necessary; but could
        /// be used to log a warning that a channel had been removed.</param>
        protected override void ProcessVertexChannel(GeometryContent geometry,
            int vertexChannelIndex, ContentProcessorContext context)
        {
            String vertexChannelName =
                geometry.Vertices.Channels[vertexChannelIndex].Name;
            
            // if this vertex channel has an acceptable names, process it as normal.
            if (acceptableVertexChannelNames.Contains(vertexChannelName))
            {
                base.ProcessVertexChannel(geometry, vertexChannelIndex, context);
            }
            // otherwise, remove it from the vertex channels; it's just extra data
            // we don't need.
            else
            {
                geometry.Vertices.Channels.Remove(vertexChannelName);
            }
        }


        protected override MaterialContent ConvertMaterial(MaterialContent material,
            ContentProcessorContext context)
        {
            EffectMaterialContent shaderMaterial = new EffectMaterialContent();
            shaderMaterial.Effect = new ExternalReference<EffectContent>
                ("Effects/ShaderModelEffect.fx");

            // copy the textures in the original material to the new normal mapping
            // material. this way the diffuse texture is preserved. The
            // PreprocessSceneHierarchy function has already added the normal map
            // texture to the Textures collection, so that will be copied as well.
            foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture
                in material.Textures)
            {
                shaderMaterial.Textures.Add(texture.Key, texture.Value);
            }

            // and convert the material using the ShaderMaterialProcessor,
            // who has something special in store for the normal map.
            return context.Convert<MaterialContent, MaterialContent>
                (shaderMaterial, typeof(ShaderMaterialProcessor).Name);
        }
    }
}