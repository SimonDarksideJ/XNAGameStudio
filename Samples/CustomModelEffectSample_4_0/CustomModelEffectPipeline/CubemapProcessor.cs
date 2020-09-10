#region File Description
//-----------------------------------------------------------------------------
// CubemapProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
#endregion

namespace CustomModelEffectPipeline
{
    /// <summary>
    /// Custom content pipeline processor converts regular
    /// 2D images into reflection cubemaps.
    /// </summary>
    [ContentProcessor]
    public class CubemapProcessor : ContentProcessor<TextureContent, TextureCubeContent>
    {
        const int cubemapSize = 256;


        /// <summary>
        /// Converts an arbitrary 2D image into a reflection cubemap.
        /// </summary>
        public override TextureCubeContent Process(TextureContent input,
                                                   ContentProcessorContext context)
        {
            // Convert the input data to Color format, for ease of processing.
            input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));

            // Mirror the source image from left to right.
            PixelBitmapContent<Color> mirrored;

            mirrored = MirrorBitmap((PixelBitmapContent<Color>)input.Faces[0][0]);

            // Create the six cubemap faces.
            TextureCubeContent cubemap = new TextureCubeContent();

            cubemap.Faces[(int)CubeMapFace.NegativeZ] = CreateSideFace(mirrored, 0);
            cubemap.Faces[(int)CubeMapFace.NegativeX] = CreateSideFace(mirrored, 1);
            cubemap.Faces[(int)CubeMapFace.PositiveZ] = CreateSideFace(mirrored, 2);
            cubemap.Faces[(int)CubeMapFace.PositiveX] = CreateSideFace(mirrored, 3);
            cubemap.Faces[(int)CubeMapFace.PositiveY] = CreateTopFace(mirrored);
            cubemap.Faces[(int)CubeMapFace.NegativeY] = CreateBottomFace(mirrored);

            // Calculate mipmap data.
            cubemap.GenerateMipmaps(true);

            // Compress the cubemap into DXT1 format.
            cubemap.ConvertBitmapType(typeof(Dxt1BitmapContent));

            return cubemap;
        }


        /// <summary>
        /// Our source data is just a regular 2D image, but to make a good
        /// cubemap we need this to wrap on all sides without any visible seams.
        /// An easy way of making an image wrap from left to right is simply to
        /// put a mirrored copy of the image next to the original. The point
        /// where the image mirrors is still pretty obvious, but for a reflection
        /// map this will be good enough.
        /// </summary>
        static PixelBitmapContent<Color> MirrorBitmap(PixelBitmapContent<Color> source)
        {
            int width = source.Width * 2;

            PixelBitmapContent<Color> mirrored;
            
            mirrored = new PixelBitmapContent<Color>(width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color color = source.GetPixel(x, y);

                    mirrored.SetPixel(x, y, color);
                    mirrored.SetPixel(width - x - 1, y, color);
                }
            }

            return mirrored;
        }


        /// <summary>
        /// The four side faces of the cubemap are easy to create: we just copy
        /// out the appropriate region from the middle of the source bitmap.
        /// </summary>
        static BitmapContent CreateSideFace(PixelBitmapContent<Color> source,
            int cubeSide)
        {
            PixelBitmapContent<Color> result;
            
            result = new PixelBitmapContent<Color>(cubemapSize, cubemapSize);

            Rectangle sourceRegion = new Rectangle(source.Width * cubeSide / 4,
                                                   source.Height / 3,
                                                   source.Width / 4,
                                                   source.Height / 3);

            Rectangle destinationRegion = new Rectangle(0, 0, cubemapSize, cubemapSize);

            BitmapContent.Copy(source, sourceRegion, result, destinationRegion);

            return result;
        }


        /// <summary>
        /// We have to do a lot of stretching and warping to create the top
        /// and bottom faces of the cubemap. To keep the result nicely free
        /// of jaggies, we do this computation on a larger version of the
        /// bitmap, then scale down the final result to antialias it.
        /// </summary>
        const int multisampleScale = 4;

        
        /// <summary>
        /// Folds four flaps inward from the top of the source bitmap,
        /// to create the top face of the cubemap.
        /// </summary>
        static BitmapContent CreateTopFace(PixelBitmapContent<Color> source)
        {
            PixelBitmapContent<Color> result;
            
            result = new PixelBitmapContent<Color>(cubemapSize * multisampleScale,
                                                   cubemapSize * multisampleScale);

            int right = cubemapSize * multisampleScale - 1;

            ScaleTrapezoid(source, 0, -1, result, right, 0,    -1,  0,  0,  1);
            ScaleTrapezoid(source, 1, -1, result, 0,     0,     0,  1,  1,  0);
            ScaleTrapezoid(source, 2, -1, result, 0,     right, 1,  0,  0, -1);
            ScaleTrapezoid(source, 3, -1, result, right, right, 0, -1, -1,  0);

            return BlurCubemapFace(result);
        }


        /// <summary>
        /// Folds four flaps inward from the bottom of the source bitmap,
        /// to create the bottom face of the cubemap.
        /// </summary>
        static BitmapContent CreateBottomFace(PixelBitmapContent<Color> source)
        {
            PixelBitmapContent<Color> result;

            result = new PixelBitmapContent<Color>(cubemapSize * multisampleScale,
                                                   cubemapSize * multisampleScale);

            int right = cubemapSize * multisampleScale - 1;

            ScaleTrapezoid(source, 0, 1, result, right, right, -1,  0,  0, -1);
            ScaleTrapezoid(source, 1, 1, result, 0,     right,  0, -1,  1,  0); 
            ScaleTrapezoid(source, 2, 1, result, 0,     0,      1,  0,  0,  1);
            ScaleTrapezoid(source, 3, 1, result, right, 0,      0,  1, -1,  0);

            return BlurCubemapFace(result);
        }


        /// <summary>
        /// Worker function for folding and stretching a flap from the source
        /// image to make up one quarter of the top or bottom cubemap faces.
        /// </summary>
        static void ScaleTrapezoid(PixelBitmapContent<Color> source,
                                   int cubeSide, int cubeY,
                                   PixelBitmapContent<Color> destination,
                                   int destinationX, int destinationY,
                                   int xDirection1, int yDirection1,
                                   int xDirection2, int yDirection2)
        {
            int size = destination.Width;

            // Compute the source x location.
            int baseSourceX = cubeSide * source.Width / 4;

            // Copy the image data one row at a time.
            for (int row = 0; row < size / 2; row++)
            {
                // Compute the source y location.
                int sourceY;

                if (cubeY < 0)
                    sourceY = source.Height / 3;
                else
                    sourceY = source.Height * 2 / 3;

                sourceY += cubeY * row * source.Height / 3 / (size / 2);

                // Stretch this row from the source to destination.
                int x = destinationX;
                int y = destinationY;

                int rowLength = size - row * 2;

                for (int i = 0; i < rowLength; i++)
                {
                    int sourceX = baseSourceX + i * source.Width / 4 / rowLength;

                    Color color = source.GetPixel(sourceX, sourceY);

                    destination.SetPixel(x, y, color);

                    x += xDirection1;
                    y += yDirection1;
                }

                // Advance to the start of the next row.
                destinationX += xDirection1 + xDirection2;
                destinationY += yDirection1 + yDirection2;
            }
        }

        
        /// <summary>
        /// The top and bottom cubemap faces will have a nasty discontinuity
        /// in the middle where the four source image flaps meet. We can cover
        /// this up by applying a blur filter to the problematic area.
        /// </summary>
        static BitmapContent BlurCubemapFace(PixelBitmapContent<Color> source)
        {
            // Create two temporary bitmaps.
            PixelBitmapContent<Vector4> temp1, temp2;

            temp1 = new PixelBitmapContent<Vector4>(cubemapSize, cubemapSize);
            temp2 = new PixelBitmapContent<Vector4>(cubemapSize, cubemapSize);

            // Antialias by shrinking the larger generated image to the final size.
            BitmapContent.Copy(source, temp1);

            // Apply the blur in two passes, first horizontally, then vertically.
            ApplyBlurPass(temp1, temp2, 1, 0);
            ApplyBlurPass(temp2, temp1, 0, 1);

            // Convert the result back to Color format.
            PixelBitmapContent<Color> result;

            result = new PixelBitmapContent<Color>(cubemapSize, cubemapSize);

            BitmapContent.Copy(temp1, result);

            return result;
        }


        /// <summary>
        /// Applies a single pass of a separable box filter, blurring either
        /// along the x or y axis. This could give much higher quality results
        /// if we used a gaussian filter kernel rather than this simplistic box,
        /// but this is good enough to get the job done.
        /// </summary>
        static void ApplyBlurPass(PixelBitmapContent<Vector4> source,
                                  PixelBitmapContent<Vector4> destination,
                                  int dx, int dy)
        {
            int cubemapCenter = cubemapSize / 2;

            for (int y = 0; y < cubemapSize; y++)
            {
                for (int x = 0; x < cubemapSize; x++)
                {
                    // How far is this texel from the center of the image?
                    int xDist = cubemapCenter - x;
                    int yDist = cubemapCenter - y;

                    int distance = (int)Math.Sqrt(xDist * xDist + yDist * yDist);

                    // Blur more in the center, less near the edges.
                    int blurAmount = Math.Max(cubemapCenter - distance, 0) / 8;

                    // Accumulate source texel values.
                    Vector4 blurredValue = Vector4.Zero;

                    for (int i = -blurAmount; i <= blurAmount; i++)
                    {
                        blurredValue += source.GetPixel(x + dx * i, y + dy * i);
                    }

                    // Average them to calculate a blurred result.
                    blurredValue /= blurAmount * 2 + 1;

                    destination.SetPixel(x, y, blurredValue);
                }
            }
        }
    }
}
