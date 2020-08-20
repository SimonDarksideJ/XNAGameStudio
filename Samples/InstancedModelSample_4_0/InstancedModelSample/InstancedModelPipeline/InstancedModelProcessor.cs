#region File Description
//-----------------------------------------------------------------------------
// InstancedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace InstancedModelPipeline
{
    /// <summary>
    /// Content Pipeline processor applies the InstancedModel.fx shader
    /// onto models, so they can be drawn using hardware instancing.
    /// </summary>
    [ContentProcessor(DisplayName = "Instanced Model")]
    public class InstancedModelProcessor : ModelProcessor
    {
        ContentIdentity rootIdentity;


        /// <summary>
        /// Override the Process method to store the ContentIdentity of the model root node.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            rootIdentity = input.Identity;

            return base.Process(input, context);
        }


        /// <summary>
        /// Override the ConvertMaterial method to apply our custom InstancedModel.fx shader.
        /// </summary>
        protected override MaterialContent ConvertMaterial(MaterialContent material,
                                                           ContentProcessorContext context)
        {
            // Create a new material.
            EffectMaterialContent newMaterial = new EffectMaterialContent();

            // Tell it to use our custom InstancedModel.fx shader.
            newMaterial.Effect = new ExternalReference<EffectContent>("InstancedModel.fx",
                                                                      rootIdentity);

            // Copy the texture setting across from the original material.
            BasicMaterialContent basicMaterial = material as BasicMaterialContent;

            if ((basicMaterial != null) && (basicMaterial.Texture != null))
            {
                newMaterial.Textures.Add("Texture", basicMaterial.Texture);
            }

            // Chain to the base ModelProcessor, so it can build our new material.
            return base.ConvertMaterial(newMaterial, context);
        }
    }
}
