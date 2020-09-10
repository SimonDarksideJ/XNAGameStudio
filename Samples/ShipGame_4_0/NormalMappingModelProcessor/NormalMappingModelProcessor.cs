#region File Description
//-----------------------------------------------------------------------------
// NormalMappingModelProcessor.cs
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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using System.Text;
#endregion



namespace NormalMappingProcessor
{
    /// <summary>
    /// The NormalMappingModelProcessor is used to change the material/effect applied
    /// to a model. After going through this processor, the output model will be set
    /// up to be rendered with NormalMapping.fx.
    /// </summary>
    [ContentProcessor(DisplayName = "Model - ShipGame Normal Mapping")]
    public class NormalMappingModelProcessor : ModelProcessor
    {
        public const string TextureMapKey = "Texture";
        public const string NormalMapKey = "Bump0";
        public const string SpecularMapKey = "Specular0";
        public const string GlowMapKey = "Emissive0";

        static string[] fileKeys = { "Bump0", "Specular0", "Emissive0" };
        static string[] fileExt = { "_n.tga", "_s.tga", "_i.tga" };

        public override ModelContent Process(NodeContent input,
            ContentProcessorContext context)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            context.Logger.LogImportantMessage("processing: " + input.Name);
            PreprocessSceneHierarchy(input, context, input.Name);
            return base.Process(input, context);
        }


        /// <summary>
        /// Recursively calls MeshHelper.CalculateTangentFrames for every MeshContent
        /// object in the NodeContent scene. This function could be changed to add 
        /// more per vertex data as needed.
        /// </summary>
        /// <param initialFileName="input">A node in the scene.  The function should be called
        /// with the root of the scene.</param>
        private void PreprocessSceneHierarchy(NodeContent input,
            ContentProcessorContext context, string inputName)
        {
            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                MeshHelper.CalculateTangentFrames(mesh,
                    VertexChannelNames.TextureCoordinate(0),
                    VertexChannelNames.Tangent(0),
                    VertexChannelNames.Binormal(0));

                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    if (false == geometry.Material.Textures.ContainsKey(TextureMapKey))
                        geometry.Material.Textures.Add(TextureMapKey,
                                new ExternalReference<TextureContent>(
                                        "null_color.tga"));
                    else
                    {
                        context.Logger.LogImportantMessage("has: " + geometry.Material.Textures[TextureMapKey].Filename);
                        string fileName = Path.GetFileName(geometry.Material.Textures[TextureMapKey].Filename);
                        if (fileName != null && fileName.StartsWith("ship") && fileName.EndsWith("_c.tga"))
                            InsertMissedMapTextures(geometry.Material.Textures,
                                fileName.Substring(0, fileName.Length - "_c.tga".Length), context);
                    }

                    if (false == geometry.Material.Textures.ContainsKey(NormalMapKey))
                        geometry.Material.Textures.Add(NormalMapKey,
                                new ExternalReference<TextureContent>(
                                        "null_normal.tga"));
                    else
                        context.Logger.LogImportantMessage("has: " + geometry.Material.Textures[NormalMapKey].Filename);

                    if (false == geometry.Material.Textures.ContainsKey(SpecularMapKey))
                        geometry.Material.Textures.Add(SpecularMapKey,
                                new ExternalReference<TextureContent>(
                                        "null_specular.tga"));
                    else
                        context.Logger.LogImportantMessage("has: " + geometry.Material.Textures[SpecularMapKey].Filename);

                    if (false == geometry.Material.Textures.ContainsKey(GlowMapKey))
                        geometry.Material.Textures.Add(GlowMapKey,
                                new ExternalReference<TextureContent>(
                                        "null_glow.tga"));
                    else
                        context.Logger.LogImportantMessage("has: " + geometry.Material.Textures[GlowMapKey].Filename);
                }
            }

            foreach (NodeContent child in input.Children)
            {
                PreprocessSceneHierarchy(child, context, inputName);
            }
        }

        /// <summary>
        /// Ship models miss map textures. We insert them were need.
        /// </summary>
        /// <param initialFileName="textureReferenceDictionary"></param>
        /// <param initialFileName="initialFileName"></param>
        private void InsertMissedMapTextures(TextureReferenceDictionary textures, string initialFileName,
                        ContentProcessorContext context)
        {
            for (int i = 0; i < fileKeys.Length; i++)
            {
                string key = fileKeys[i];
                if (textures.ContainsKey(key))
                    continue;
                string fileName = "ships/" + initialFileName + fileExt[i];
                textures.Add(key,
                    new ExternalReference<TextureContent>(fileName));
                context.Logger.LogImportantMessage("inserted: " + fileName);
            }
        }

        // acceptableVertexChannelNames are the inputs that the normal map effect
        // expects.  The NormalMappingModelProcessor overrides ProcessVertexChannel
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
        /// <param initialFileName="geometry">the geometry object which contains the 
        /// vertex channel</param>
        /// <param initialFileName="vertexChannelIndex">the index of the vertex channel
        /// to operate on</param>
        /// <param initialFileName="context">the context that the processor is operating
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
            EffectMaterialContent normalMappingMaterial = new EffectMaterialContent();
            normalMappingMaterial.Effect = new ExternalReference<EffectContent>
                ("shaders/NormalMapping.fx");

            // copy the textures in the original material to the new normal mapping
            // material. this way the diffuse texture is preserved. The
            // PreprocessSceneHierarchy function has already added the normal map
            // texture to the Textures collection, so that will be copied as well.
            foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture
                in material.Textures)
            {
                normalMappingMaterial.Textures.Add(texture.Key, texture.Value);
            }

            return context.Convert<MaterialContent, MaterialContent>
                (normalMappingMaterial, typeof(MaterialProcessor).Name);
        }
    }
}
