#region File Description
//-----------------------------------------------------------------------------
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace CustomModelProcessor
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// </summary>
    [ContentProcessor(DisplayName = "SpaceProcessors.AlphaTextureContentProcessor")]
    public class AlphaTextureContentProcessor : TextureProcessor
    {
        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            PixelBitmapContent<Color> texture = ((PixelBitmapContent<Color>)input.Faces[0][0]);

            PixelBitmapContent<Color> alphaMap = CreateAlphaChannel(texture);

            Texture2DContent result = new Texture2DContent();

            result.Faces[0] = alphaMap;

            // The TextureProcessor class presents all of the same parameters as a the regular texture processor
            // to the developer in the content pipeline.  In order for it to act on those parameters,
            // you must call the base class!
            base.Process(result, context);

            return result;
        }

        static PixelBitmapContent<Color> CreateAlphaChannel(PixelBitmapContent<Color> input)
        {
            int width = input.Width;
            int height = input.Height;

            PixelBitmapContent<Color> result = new PixelBitmapContent<Color>(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = input.GetPixel(x, y);

                    // Adds/Replaces Alpha channel with the "Brightest color channel value
                    // A better way would be to average all three colors and use that for alpha.
                    // This will work just fine for grey scale textures and is simpler...
                    byte resultAlpha1 = (byte)(color.PackedValue & 0xff);
                    byte resultAlpha2 = (byte)(color.PackedValue & 0xff00);
                    byte resultAlpha3 = (byte)(color.PackedValue & 0xff0000);

                    byte resultAlpha = Math.Max(Math.Max(resultAlpha1, resultAlpha2), resultAlpha3);
                    color.PackedValue = (uint)(resultAlpha << 24) + (0x00ffffff);

                    result.SetPixel(x, y, color);
                }
            }

            return result;
        }
    }
}