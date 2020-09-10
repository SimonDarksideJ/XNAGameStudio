#region File Description
//-----------------------------------------------------------------------------
// CustomModelContent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace CustomModelPipeline
{
    /// <summary>
    /// Content Pipeline class provides a design time equivalent of the runtime
    /// CustomModel class. This stores the output from the CustomModelProcessor,
    /// right before it gets written into the XNB binary. This class is similar
    /// in shape to the runtime CustomModel, but stores the data as simple managed
    /// objects rather than GPU data types. This avoids us having to instantiate
    /// any actual GPU objects during the Content Pipeline build process, which
    /// is essential when building graphics for Xbox. The build always runs on
    /// Windows, and it would be problematic if we tried to instantiate Xbox
    /// types on the Windows GPU during this process!
    /// </summary>
    [ContentSerializerRuntimeType("CustomModelSample.CustomModel, CustomModelSample")]
    public class CustomModelContent
    {
        // Internally our custom model is made up from a list of model parts.
        [ContentSerializer]
        List<ModelPart> modelParts = new List<ModelPart>();


        // Each model part represents a piece of geometry that uses one single
        // effect. Multiple parts are needed to represent models that use more
        // than one effect.
        [ContentSerializerRuntimeType("CustomModelSample.CustomModel+ModelPart, CustomModelSample")]
        class ModelPart
        {
            public int TriangleCount;
            public int VertexCount;

            // These properties are not the same type as their equivalents in the
            // runtime CustomModel! Here, we are using design time managed classes,
            // while the runtime CustomModel uses actual GPU types. The Content
            // Pipeline knows about the relationship between the design time and
            // runtime types (thanks to the ContentTypeWriter.GetRuntimeType method),
            // so it can automatically translate one to the other. At design time we
            // use things like VertexBufferContent, IndexCollection and MaterialContent,
            // but when the serializer reads this data back at runtime, it will load
            // into the corresponding VertexBuffer, IndexBuffer, and Effect classes.
            public VertexBufferContent VertexBufferContent;
            public IndexCollection IndexCollection;

            // A single material instance may be shared by more than one ModelPart,
            // in which case we only want to write a single copy of the material
            // data into the XNB file. The SharedResource attribute tells the
            // serializer to take care of this merging for us.
            [ContentSerializer(SharedResource = true)]
            public MaterialContent MaterialContent;
        }


        /// <summary>
        /// Helper function used by the CustomModelProcessor
        /// to add new ModelPart information.
        /// </summary>
        public void AddModelPart(int triangleCount, int vertexCount,
                                 VertexBufferContent vertexBufferContent,
                                 IndexCollection indexCollection,
                                 MaterialContent materialContent)
        {
            ModelPart modelPart = new ModelPart();

            modelPart.TriangleCount = triangleCount;
            modelPart.VertexCount = vertexCount;
            modelPart.VertexBufferContent = vertexBufferContent;
            modelPart.IndexCollection = indexCollection;
            modelPart.MaterialContent = materialContent;

            modelParts.Add(modelPart);
        }
    }
}
