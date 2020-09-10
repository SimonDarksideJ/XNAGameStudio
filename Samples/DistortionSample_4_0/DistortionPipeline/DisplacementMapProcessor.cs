#region File Description
//-----------------------------------------------------------------------------
// DisplacementMapProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.ComponentModel;
#endregion

namespace DistortionPipeline
{
    /// <summary>
    /// Converts height maps into 2D displacement maps for use as a texture on 
    /// distorters. The height map is first converted into a normal map before becoming
    /// a displacement map. See the NormapMapProcessor class for more details.
    /// </summary>
    [ContentProcessor]
    public class DisplacementMapProcessor :
        ContentProcessor<TextureContent, TextureContent>
    {
        #region Processor Parameters


        /// <summary>
        /// Amount of distortion in the map.
        /// </summary>
        private float distortionScale = 0.5f;

        [DisplayName("Distortion Scale")]
        [DefaultValue(0.5f)]
        [Description("Amount of distortion.")]
        public float DistortionScale
        {
            get { return distortionScale; }
            set { distortionScale = value; }
        }


        #endregion



        /// <summary>
        /// Converts a greyscale height bitmap into a distplacement map.
        /// </summary>        
        public override TextureContent Process(TextureContent input,
                                               ContentProcessorContext context)
        {
            // this processor builds on the output of the NormalMapProcessor, so 
            // we start by chaining to that
            input = context.Convert<TextureContent, TextureContent>(input,
                "NormalMapProcessor");

            // Perform the conversion to a displacement map
            PixelBitmapContent<NormalizedByte4> bitmap =
                (PixelBitmapContent<NormalizedByte4>)input.Faces[0][0];
            ConvertNormalsToDisplacement(bitmap, DistortionScale);

            // Convert to NormalizedByte2 because only the X and Y channels are needed
            input.ConvertBitmapType(typeof(PixelBitmapContent<NormalizedByte2>));

            return input;
        }

        /// <summary>
        /// Flattens vectors from a normal map into a 2D displacement map
        /// and stores them in the RG portion of the bitmap.
        /// </summary>
        public static void ConvertNormalsToDisplacement(
            PixelBitmapContent<NormalizedByte4> bitmap, float distortionScale)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    // Get the normal from the normal map
                    Vector4 normal4 = bitmap.GetPixel(x, y).ToVector4();
                    Vector3 normal3 = new Vector3(normal4.X, normal4.Y, normal4.Z);

                    // Determine the magnitude of the distortion at this pixel
                    float amount = Vector3.Dot(normal3, Vector3.Backward) *
                        distortionScale;

                    // Create a displacement vector of that magnitude in the direction
                    // of the normal projected onto the plane of the texture.
                    Vector2 normal2 = new Vector2(normal3.X, normal3.Y);
                    Vector2 displacement = normal2 * amount + new Vector2(.5f, .5f);

                    // Store the result
                    bitmap.SetPixel(x, y,
                        new NormalizedByte4(displacement.X, displacement.Y, 0, 0));
                }
            }
        }
    }
}
