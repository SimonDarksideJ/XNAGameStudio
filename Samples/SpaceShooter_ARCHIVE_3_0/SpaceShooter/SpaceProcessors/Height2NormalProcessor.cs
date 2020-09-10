using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

// TODO: replace these with the processor input and output types.
using TInput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.TextureContent;
using TOutput = Microsoft.Xna.Framework.Content.Pipeline.Graphics.Texture2DContent;

namespace FrontierProcessors
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This is part of a Content Pipeline Extension Library project.
    ///
    /// </summary>
    [ContentProcessor(DisplayName = "SpaceProcessors.Height2NormalProcessor")]
    public class Height2NormalProcessor : TextureProcessor
    {
        public override TInput Process(TInput input, ContentProcessorContext context)
        {
            PixelBitmapContent<Color> texture = ((PixelBitmapContent<Color>)input.Faces[0][0]);

            PixelBitmapContent<Color> normalMap = CreateNormalMap(texture);

            Texture2DContent result = new Texture2DContent();

            result.Faces[0] = normalMap;

            // Call the base with the new texture to gen mipmaps and new format
            base.Process(result, context);

            return result;
        }

        PixelBitmapContent<Color> CreateNormalMap(PixelBitmapContent<Color> input)
        {
            int width = input.Width;
            int height = input.Height;

            PixelBitmapContent<Color> result = new PixelBitmapContent<Color>(width, height);

            float h1, h2, h3, h4;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < width - 1)
                        h1 = ((float)input.GetPixel(x + 1, y).R / 255.0f);
                    else // wrap x to other side of texture.
                        h1 = ((float)input.GetPixel(0, y).R / 255.0f);

                    if (x > 0)
                        h3 = ((float)input.GetPixel(x - 1, y).R / 255.0f);
                    else // Wrap x to other side of texture.
                        h3 = ((float)input.GetPixel((width - 1), y).R / 255.0f);

                    if (y < height - 1)
                        h2 = ((float)input.GetPixel(x, y + 1).R / 255.0f);
                    else // Wrap y to other side of texture
                        h2 = ((float)input.GetPixel(x, 0).R / 255.0f);

                    if (y > 0)
                        h4 = (float)(input.GetPixel(x, y - 1).R / 255.0f);
                    else // Wrap y to other side of texture
                        h4 = (float)(input.GetPixel(x, (height - 1)).R / 255.0f);

                    Vector3 v1 = new Vector3(1.0f, 0.0f, (h1 - h3));
                    Vector3 v2 = new Vector3(0.0f, 1.0f, (h2 - h4));

                    Vector3 normal = Vector3.Cross(v1, v2);
                    normal.Normalize();

                    normal.X = (normal.X + 1.0f) * 0.5f;
                    normal.Y = (normal.Y + 1.0f) * 0.5f;
                    normal.Z = (normal.Z + 1.0f) * 0.5f;

                    result.SetPixel(x, y, new Color(normal));
                }
            }

            return result;
        }
    }
}