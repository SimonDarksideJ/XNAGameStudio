#region File Description
//-----------------------------------------------------------------------------
// ShaderTextureProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework;
#endregion

namespace ShaderEffectPipeline
{
    /// <summary>
    /// The ShaderTextureProcessor takes in an encoded normal map, and outputs
    /// a texture in the NormalizedByte4 format.  Every pixel in the source texture
    /// is remapped so that values ranging from 0 to 1 will range from -1 to 1.
    /// </summary>
    [ContentProcessor]
    class ShaderTextureProcessor : ContentProcessor<TextureContent, TextureContent>
    {
        /// <summary>
        /// Process converts the encoded normals to the NormalizedByte4 format and 
        /// generates mipmaps.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override TextureContent Process(TextureContent input,
            ContentProcessorContext context)
        {
            // convert to vector4 format, so that we know what kind of data we're 
            // working with.
            input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>));
            
            // expand the encoded normals; values ranging from 0 to 1 should be
            // expanded to range to -1 to 1.
            // NOTE: in almost all cases, the input normalmap will be a
            // Texture2DContent, and will only have one face.  just to be safe,
            // we'll do the conversion for every face in the texture.
            foreach (MipmapChain mipmapChain in input.Faces)
            {
                foreach (PixelBitmapContent<Vector4> bitmap in mipmapChain)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            Vector4 encoded = bitmap.GetPixel(x, y);
                            bitmap.SetPixel(x, y, 2 * encoded - Vector4.One);
                        }
                    }
                }
            }
            
            // now that the conversion to -1 to 1 ranges is finished, convert to the 
            // runtime-ready format NormalizedByte4.
            // EDUCATIONAL: it is possible to perform the conversion to NormalizedByte4
            // in the inner loop above by copying to a new TextureContent.  For
            // the sake of simplicity, we do it the slower way.
            input.ConvertBitmapType(typeof(PixelBitmapContent<NormalizedByte4>));

            input.GenerateMipmaps(false);
            return input;
        }
    }
}
