#region File Description
//-----------------------------------------------------------------------------
// NormalMappingModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

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
using System.ComponentModel;

namespace NormalMappingEffectPipeline
{
    /// <summary>
    /// The NormalMappingModelProcessor is used to change the material/effect applied
    /// to a model. After going through this processor, the output model will be set
    /// up to be rendered with NormalMapping.fx.
    /// </summary>
    [ContentProcessor(DisplayName="Normal Mapping Model Processor")]
    public class NormalMappingModelProcessor : ModelProcessor
    {
        // this constant determines where we will look for the normal map in the opaque
        // data dictionary.
        public const string NormalMapKey = "NormalMap";

        /// <summary>
        /// We override this property from the base processor and force it to always
        /// return true: tangent frames are required for normal mapping, so they should
        /// not be optional.
        /// </summary>
        [Browsable(false)]
        public override bool GenerateTangentFrames
        {
            get { return true; }
            set { }
        }

        /// <summary>
        /// The user can set this value in the property grid. If it is set, the model
        /// will use this value for its normal map texture, overriding anything in the
        /// opaque data. We use the display name and description attributes to control
        /// how the property appears in the UI.
        /// </summary>
        [DisplayName("Normal Map Texture")]
        [Description("If set, this file will be used as the normal map on the model, " +
        "overriding anything found in the opaque data.")]
        [DefaultValue("")]
        public string NormalMapTexture
        {
            get { return normalMapTexture; }
            set { normalMapTexture = value; }
        }
        private string normalMapTexture;



        String directory;

        public override ModelContent Process(NodeContent input,
            ContentProcessorContext context)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            directory = Path.GetDirectoryName(input.Identity.SourceFilename);

            LookUpNormalMapAndAddToTextures(input);
            return base.Process(input, context);
        }



        /// <summary>
        /// Looks into the OpaqueData property on the "mesh" object, and looks for a
        /// a string containing the path to the normal map. The normal map is added
        /// to the Textures collection for each of the mesh's materials.
        /// </summary>
        private void LookUpNormalMapAndAddToTextures(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;
            if (mesh != null)
            {
                string pathToNormalMap;

                // if NormalMapTexture hasn't been set in the UI, we'll try to look up
                // the normal map using the opaque data.
                if (String.IsNullOrEmpty(NormalMapTexture))
                {
                    pathToNormalMap =
                        mesh.OpaqueData.GetValue<string>(NormalMapKey, null);
                }
                // However, if the NormalMapTexture is set, we'll use that value for
                // ever mesh in the scene.
                else
                {
                    pathToNormalMap = NormalMapTexture;
                }

                if (pathToNormalMap == null)
                {
                    throw new InvalidContentException("the normal map is missing!");
                }
                pathToNormalMap = Path.Combine(directory, pathToNormalMap);
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    geometry.Material.Textures.Add(NormalMapKey,
                        new ExternalReference<TextureContent>(pathToNormalMap));
                }
            }

            foreach (NodeContent child in node.Children)
            {
                LookUpNormalMapAndAddToTextures(child);
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
            EffectMaterialContent normalMappingMaterial = new EffectMaterialContent();
            normalMappingMaterial.Effect = new ExternalReference<EffectContent>
                (Path.Combine(directory, "NormalMapping.fx"));

            OpaqueDataDictionary processorParameters = new OpaqueDataDictionary();
            processorParameters["ColorKeyColor"] = this.ColorKeyColor;
            processorParameters["ColorKeyEnabled"] = this.ColorKeyEnabled;
            processorParameters["TextureFormat"] = this.TextureFormat;
            processorParameters["GenerateMipmaps"] = this.GenerateMipmaps;
            processorParameters["ResizeTexturesToPowerOfTwo"] =
                                                        this.ResizeTexturesToPowerOfTwo;

            // copy the textures in the original material to the new normal mapping
            // material. this way the diffuse texture is preserved. The
            // PreprocessSceneHierarchy function has already added the normal map
            // texture to the Textures collection, so that will be copied as well.
            foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture
                in material.Textures)
            {
                normalMappingMaterial.Textures.Add(texture.Key, texture.Value);
            }

            // and convert the material using the NormalMappingMaterialProcessor,
            // who has something special in store for the normal map.
            return context.Convert<MaterialContent, MaterialContent>
                (normalMappingMaterial, typeof(NormalMappingMaterialProcessor).Name,
                processorParameters);
        }
    }
}