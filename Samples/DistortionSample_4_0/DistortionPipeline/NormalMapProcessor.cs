#region File Description
//-----------------------------------------------------------------------------
// NormalMapProcessor.cs
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
#endregion

namespace DistortionPipeline
{
    /// <summary>
    /// Custom content processor converts greyscale displacement bitmaps into
    /// a normal-map format. In the source image, use black for pixels that are
    /// further away and white for ones that are bumped upward. The output
    /// texture contains three-component normal vectors. This processor works
    /// best if the source bitmap is slightly blurred.
    /// </summary>
    /// <remarks>
    /// This class was originally provided in the Sprite Effects sample.
    /// </remarks>
    [ContentProcessor]
    public class NormalMapProcessor : ContentProcessor<TextureContent, TextureContent>
    {
        // Controls how extreme the output normalmap should be.
        const float bumpSize = 4f;

        /// <summary>
        /// Converts a greyscale displacement bitmap into normalmap format.
        /// </summary>
        public override TextureContent Process(TextureContent input,
                                               ContentProcessorContext context)
        {
            // Convert the input bitmap to Vector4 format, for ease of processing.
            input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>));

            PixelBitmapContent<Vector4> bitmap;
            bitmap = (PixelBitmapContent<Vector4>)input.Faces[0][0];

            // Calculate normalmap vectors.
            ConvertGreyToAlpha(bitmap);
            ConvertAlphaToNormals(bitmap);

            // Convert the result into NormalizedByte4 format.
            input.ConvertBitmapType(typeof(PixelBitmapContent<NormalizedByte4>));

            return input;
        }


        /// <summary>
        /// Copies greyscale color information into the alpha channel.
        /// </summary>
        static void ConvertGreyToAlpha(PixelBitmapContent<Vector4> bitmap)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Vector4 value = bitmap.GetPixel(x, y);

                    // Copy a greyscale version of the RGB data into the alpha channel.
                    float greyscale = (value.X + value.Y + value.Z) / 3;

                    value.W = greyscale;

                    bitmap.SetPixel(x, y, value);
                }
            }
        }


        /// <summary>
        /// Using height data stored in the alpha channel, computes normalmap
        /// vectors and stores them in the RGB portion of the bitmap.
        /// </summary>
        static void ConvertAlphaToNormals(PixelBitmapContent<Vector4> bitmap)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    // Look up the heights to either side of this pixel.
                    float left = GetHeight(bitmap, x - 1, y);
                    float right = GetHeight(bitmap, x + 1, y);

                    float top = GetHeight(bitmap, x, y - 1);
                    float bottom = GetHeight(bitmap, x, y + 1);

                    // Compute gradient vectors, then cross them to get the normal.
                    Vector3 dx = new Vector3(1, 0, (right - left) * bumpSize);
                    Vector3 dy = new Vector3(0, 1, (bottom - top) * bumpSize);

                    Vector3 normal = Vector3.Cross(dx, dy);

                    normal.Normalize();

                    // Store the result.
                    float alpha = GetHeight(bitmap, x, y);

                    bitmap.SetPixel(x, y, new Vector4(normal, alpha));
                }
            }
        }


        /// <summary>
        /// Helper for looking up height values from the bitmap alpha channel,
        /// clamping if the specified position is off the edge of the bitmap.
        /// </summary>
        static float GetHeight(PixelBitmapContent<Vector4> bitmap, int x, int y)
        {
            if (x < 0)
            {
                x = 0;
            }
            else if (x >= bitmap.Width)
            {
                x = bitmap.Width - 1;
            }

            if (y < 0)
            {
                y = 0;
            }
            else if (y >= bitmap.Height)
            {
                y = bitmap.Height - 1;
            }

            return bitmap.GetPixel(x, y).W;
        }
    }
}
