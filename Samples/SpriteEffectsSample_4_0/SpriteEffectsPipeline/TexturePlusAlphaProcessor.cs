#region File Description
//-----------------------------------------------------------------------------
// TexturePlusAlphaProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace SpriteShaderPipeline
{
    /// <summary>
    /// Custom content processor reads two bitmaps, one containing color information,
    /// the other containing greyscale opacity data, and merges them into a single
    /// combined texture that has both RGB color data and an alpha channel.
    /// </summary>
    [ContentProcessor]
    public class TexturePlusAlphaProcessor : TextureProcessor
    {
        public override TextureContent Process(TextureContent input,
                                               ContentProcessorContext context)
        {
            TextureContent colorTexture = input;

            // The color texture was passed to us as input, but we also need to go
            // load in the texture containing our alpha information. We locate this
            // using a naming convention: it should have the same name as the main
            // color texture, but with a "_alpha" suffix, so for instance "cat.jpg"
            // goes with "cat_alpha.jpg".
            string colorFilename = colorTexture.Identity.SourceFilename;

            string alphaFilename = Path.GetDirectoryName(colorFilename) +
                                   Path.DirectorySeparatorChar +
                                   Path.GetFileNameWithoutExtension(colorFilename) +
                                   "_alpha" +
                                   Path.GetExtension(colorFilename);

            // Ask the content pipeline to load in the alpha texture.
            ExternalReference<TextureContent> alphaReference;
            alphaReference = new ExternalReference<TextureContent>(alphaFilename);

            TextureContent alphaTexture;
            alphaTexture = context.BuildAndLoadAsset<TextureContent, TextureContent>
                                                                (alphaReference, null);

            // Convert both textures to Color format, for ease of processing.
            colorTexture.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            alphaTexture.ConvertBitmapType(typeof(PixelBitmapContent<Color>));

            PixelBitmapContent<Color> colorBitmap, alphaBitmap;

            colorBitmap = (PixelBitmapContent<Color>)colorTexture.Faces[0][0];
            alphaBitmap = (PixelBitmapContent<Color>)alphaTexture.Faces[0][0];

            if ((colorBitmap.Width != alphaBitmap.Width) ||
                (colorBitmap.Height != alphaBitmap.Height))
            {
                throw new InvalidContentException(
                                    "Color and alpha bitmaps are not the same size.");
            }

            // Merge the two bitmaps.
            for (int y = 0; y < colorBitmap.Height; y++)
            {
                for (int x = 0; x < colorBitmap.Width; x++)
                {
                    Color color = colorBitmap.GetPixel(x, y);
                    Color alphaAsGreyscale = alphaBitmap.GetPixel(x, y);

                    byte alpha = (byte)((alphaAsGreyscale.R +
                                         alphaAsGreyscale.G +
                                         alphaAsGreyscale.B) / 3);

                    Color combinedColor = new Color(color.R, color.G, color.B, alpha);

                    colorBitmap.SetPixel(x, y, combinedColor);
                }
            }

            // Chain to the base SpriteTextureProcessor.
            return base.Process(colorTexture, context);
        }
    }
}
