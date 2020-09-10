#region File Description
//-----------------------------------------------------------------------------
// SpriteSheetGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using SpriteSheetRuntime;
using System;
#endregion

namespace SpriteSheetSampleWindowsPhone
{
  /// <summary>
  /// This is the main type for your game
  /// </summary>
  public class SpriteSheetGame : Microsoft.Xna.Framework.Game
  {
    #region Fields

    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    SpriteSheet spriteSheet;
    SpriteFont spriteFont;
    Texture2D checker;

#if WINDOWS_PHONE
    RenderTarget2D renderTarget;
#endif
    #endregion

    #region Initialization


    public SpriteSheetGame()
    {
      Content.RootDirectory = "Content";

      graphics = new GraphicsDeviceManager(this);

#if (!WINDOWS_PHONE)
      graphics.PreferredBackBufferWidth = 853;
      graphics.PreferredBackBufferHeight = 480;
#else
      // Frame rate is 30 fps by default for Windows Phone.
      TargetElapsedTime = TimeSpan.FromTicks(333333);
      // Pre-autoscale settings.
      graphics.PreferredBackBufferWidth = 480;
      graphics.PreferredBackBufferHeight = 800;

      graphics.SupportedOrientations = DisplayOrientation.Default;
      graphics.IsFullScreen = true;
#endif
    }


#if (WINDOWS_PHONE)
    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
      // TODO: Add your initialization logic here
#if WINDOWS_PHONE
      renderTarget = new RenderTarget2D(graphics.GraphicsDevice, 800, 480);
#endif

      base.Initialize();
    }
#endif

    /// <summary>
    /// Load your content.
    /// </summary>
    protected override void LoadContent()
    {
      spriteBatch = new SpriteBatch(GraphicsDevice);
      spriteSheet = Content.Load<SpriteSheet>("SpriteSheet");
      spriteFont = Content.Load<SpriteFont>("hudFont");
      checker = Content.Load<Texture2D>("Checker");
    }


    #endregion

    #region Update and Draw


    /// <summary>
    /// Allows the game to run logic.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
      HandleInput();

      base.Update(gameTime);
    }


    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    protected override void Draw(GameTime gameTime)
    {
#if WINDOWS_PHONE
      GraphicsDevice.SetRenderTarget(renderTarget);
#endif

      float time = (float)gameTime.TotalGameTime.TotalSeconds;

      GraphicsDevice.Clear(Color.CornflowerBlue);

      spriteBatch.Begin();

      // Draw a text label.
      spriteBatch.DrawString(spriteFont, "Here are some individual sprites,\n" +
                                         "all stored in a single sprite sheet:",
                                         new Vector2(100, 80), Color.White);

      // Draw a spinning cat sprite, looking it up from the sprite sheet by name.
      spriteBatch.Draw(spriteSheet.Texture, new Vector2(200, 250),
                       spriteSheet.SourceRectangle("cat"), Color.White,
                       time, new Vector2(50, 50), 1, SpriteEffects.None, 0);

      // Draw an animating glow effect, by rapidly cycling
      // through 7 slightly different sprite images.
      const int animationFramesPerSecond = 20;
      const int animationFrameCount = 7;

      // Look up the index of the first glow sprite.
      int glowIndex = spriteSheet.GetIndex("glow1");

      // Modify the index to select the current frame of the animation.
      glowIndex += (int)(time * animationFramesPerSecond) % animationFrameCount;

      // Draw the current glow sprite.
      spriteBatch.Draw(spriteSheet.Texture, new Rectangle(100, 150, 200, 200),
                       spriteSheet.SourceRectangle(glowIndex), Color.White);

      spriteBatch.End();

      DrawEntireSpriteSheetTexture();

      base.Draw(gameTime);

#if WINDOWS_PHONE
      GraphicsDevice.SetRenderTarget(null);

      spriteBatch.Begin();
      spriteBatch.Draw(renderTarget as Texture2D,
         new Vector2(240, 400),
         null,
         Color.White,
         MathHelper.PiOver2,
         new Vector2(400, 240),
         1f,
         SpriteEffects.None,
         0);
      spriteBatch.End();
#endif
    }


    /// <summary>
    /// A real game would never do this, but when debugging, it is
    /// useful to draw the entire sprite sheet texture, so you can
    /// see how the individual sprite images have been arranged.
    /// </summary>
    void DrawEntireSpriteSheetTexture()
    {
#if WINDOWS_PHONE
      Vector2 location = new Vector2(500, 80);
#else
        Vector2 location = new Vector2(500, 80);
#endif

      // use linear wrapping for our checkerboard, but retain defaults for other arguments by passing null
      spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null);
      
      int w = spriteSheet.Texture.Width;
      int h = spriteSheet.Texture.Height;

      Rectangle rect = new Rectangle((int)location.X, (int)(location.Y + 70), w, h);

      // Draw a tiled checkerboard pattern in the background.
      spriteBatch.Draw(checker, rect, new Rectangle(0, 0, w, h), Color.White);

      spriteBatch.End();

      // start a new batch for the text and tile sheet since we don't want to use wrapping
      spriteBatch.Begin();

      // Draw a text label.
      spriteBatch.DrawString(spriteFont, "And here is the combined\n" +
                                         "sprite sheet texture:",
                                         location, Color.White);

      // Draw the (alphablended) sprite sheet texture over the top.
      spriteBatch.Draw(spriteSheet.Texture, rect, Color.White);

      spriteBatch.End();
    }


    #endregion

    #region Handle Input


    /// <summary>
    /// Handles input for quitting the game.
    /// </summary>
    private void HandleInput()
    {        
        // Allows the game to exit
        if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            || Keyboard.GetState().IsKeyDown(Keys.Escape))
            this.Exit();
    }


    #endregion
  }
}
