#region File Description
//-----------------------------------------------------------------------------
// SpriteSheetProcessor.cs
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
#endregion

namespace SpriteSheetPipeline
{
  /// <summary>
  /// Custom content processor takes an array of individual sprite filenames (which
  /// will typically be imported from an XML file), reads them all into memory,
  /// arranges them onto a single larger texture, and returns the resulting sprite
  /// sheet object.
  /// </summary>
  [ContentProcessor]
  public class SpriteSheetProcessor : ContentProcessor<string[], SpriteSheetContent>
  {
    /// <summary>
    /// Converts an array of sprite filenames into a sprite sheet object.
    /// </summary>
    public override SpriteSheetContent Process(string[] input,
                                               ContentProcessorContext context)
    {
      SpriteSheetContent spriteSheet = new SpriteSheetContent();
      List<BitmapContent> sourceSprites = new List<BitmapContent>();

      // Loop over each input sprite filename.
      foreach (string inputFilename in input)
      {
        // Store the name of this sprite.
        string spriteName = Path.GetFileNameWithoutExtension(inputFilename);

        spriteSheet.SpriteNames.Add(spriteName, sourceSprites.Count);

        // Load the sprite texture into memory.
        ExternalReference<TextureContent> textureReference =
                        new ExternalReference<TextureContent>(inputFilename);

        TextureContent texture =
            context.BuildAndLoadAsset<TextureContent,
                                      TextureContent>(textureReference, "TextureProcessor");

        sourceSprites.Add(texture.Faces[0][0]);
      }

      // Pack all the sprites into a single large texture.
      BitmapContent packedSprites = SpritePacker.PackSprites(sourceSprites,
                                          spriteSheet.SpriteRectangles, context);

      spriteSheet.Texture.Mipmaps.Add(packedSprites);

      return spriteSheet;
    }
  }
}