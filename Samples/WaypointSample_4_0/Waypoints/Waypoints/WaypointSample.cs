#region File Description
//-----------------------------------------------------------------------------
// WaypointSample.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
#endregion

namespace Waypoint
{
  /// <summary>
  /// This is the main type for your game
  /// </summary>
  public class WaypointSample : Microsoft.Xna.Framework.Game
  {
    #region Constants
#if WINDOWS_PHONE
    /// <summary>
    /// Screen width in pixels
    /// </summary>
    const int screenWidth = 800;
    /// <summary>
    /// Screen height in pixels
    /// </summary>
    const int screenHeight = 480;
#else
        /// <summary>
    /// Screen width in pixels
    /// </summary>
    const int screenWidth = 853;
    /// <summary>
    /// Screen height in pixels
    /// </summary>
    const int screenHeight = 480;
#endif
    /// <summary>
    /// Cursor move speed in pixels per second
    /// </summary>
    const float cursorMoveSpeed = 250.0f;

    // the text we display on screen, created here to make our Draw method cleaner
    private const string helpText =
        "Use the arrow keys to move the cursor\n" +
        "Press A to place a waypoint\n" +
        "Press B to change steering behavior\n" +
        "Press X to reset the tank and waypoints\n";

    #endregion

    #region Fields
    
    // Graphics data
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;

    // Cursor data
    Texture2D cursorTexture;
    Vector2 cursorCenter;
    Vector2 cursorLocation;

    // HUD data
    SpriteFont hudFont;
    // Where the HUD draws on the screen
    Vector2 hudLocation;

    // Input data
    KeyboardState previousKeyboardState;
    GamePadState previousGamePadState;
    KeyboardState currentKeyboardState;
    GamePadState currentGamePadState;

    // The waypoint-following tank
    Tank tank;

#if WINDOWS_PHONE
    //Menu Bar data
    Texture2D blankTexture;
    int menuBarButton1_Left = 150;
    int menuBarButton2_Left = 500;
    int menuBarButtonTop = 5;
    int menuBarButtonWidth = 150;
    int menuBarButtonHeight = 30;
    int menuBar_Height = 40;

    RenderTarget2D renderTarget;
    bool isBehaviorChangeRequested = false;
    bool isClearRequested = false;
#endif
    #endregion

    #region Initialization
    /// <summary>
    /// Construct a WaypointSample object
    /// </summary>
    public WaypointSample()
    {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";

#if WINDOWS_PHONE
      // Frame rate is 30 fps by default for Windows Phone.
      TargetElapsedTime = TimeSpan.FromTicks(333333);

      // Pre-autoscale settings.
      graphics.PreferredBackBufferWidth = 480;
      graphics.PreferredBackBufferHeight = 800;

      graphics.SupportedOrientations = DisplayOrientation.Default;
      graphics.IsFullScreen = true;
#else
      // Pre-autoscale settings.
      graphics.PreferredBackBufferWidth = screenWidth;
      graphics.PreferredBackBufferHeight = screenHeight;
#endif


      tank = new Tank(this);
      Components.Add(tank);
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
      // This places the HUD near the upper left corner of the screen
      hudLocation = new Vector2(
          (float)Math.Floor(screenWidth * .1f),
          (float)Math.Floor(screenHeight * .1f));

      // places the cursor in the center of the screen
      cursorLocation =
          new Vector2((float)screenWidth / 2, (float)screenHeight / 2);

      // places the tank halfway between the center of the screen and the
      // upper left corner
      tank.Reset(
          new Vector2((float)screenWidth / 4, (float)screenHeight / 4));

      base.Initialize();

#if WINDOWS_PHONE
      renderTarget = new RenderTarget2D(graphics.GraphicsDevice, 800, 480);
#endif
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      // Create a new SpriteBatch, which can be used to draw textures.
      spriteBatch = new SpriteBatch(GraphicsDevice);

      cursorTexture = Content.Load<Texture2D>("cursor");
      cursorCenter =
          new Vector2(cursorTexture.Width / 2, cursorTexture.Height / 2);

      hudFont = Content.Load<SpriteFont>("HUDFont");

#if WINDOWS_PHONE
      blankTexture = Content.Load<Texture2D>("blank");
#endif
    }
    #endregion

    #region Update and Draw
    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
      // Allows the game to exit
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
        this.Exit();

      // TODO: Add your update logic here
      float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      HandleInput(elapsedTime);

      base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
#if WINDOWS_PHONE
      GraphicsDevice.SetRenderTarget(renderTarget);
#endif

      GraphicsDevice.Clear(Color.CornflowerBlue);

      base.Draw(gameTime);

      string HudString = "Behavior Type: " + tank.BehaviorType.ToString();

      spriteBatch.Begin();

#if WINDOWS_PHONE
      DrawMenuBar();
#endif

      // Draw the cursor
      spriteBatch.Draw(cursorTexture, cursorLocation, null, Color.White, 0f,
          cursorCenter, 1f, SpriteEffects.None, 0f);

#if !WINDOWS_PHONE
      // Draw the string for current behavior
      spriteBatch.DrawString(hudFont, HudString, hudLocation, Color.White);

      // draw our helper text so users know what they're doing.
      spriteBatch.DrawString(hudFont, helpText, new Vector2(10f, 250f), Color.White);
#endif

      spriteBatch.End();

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

#if WINDOWS_PHONE
    private void DrawMenuBar()
    {
      //Draw white rectangle
      Rectangle rect = new Rectangle(0, 0, screenWidth, menuBar_Height);
      spriteBatch.Draw(blankTexture, rect, Color.White);

      //Draw first "Button"
      Rectangle buttonRect1 = new Rectangle(menuBarButton1_Left, menuBarButtonTop, menuBarButtonWidth, menuBarButtonHeight);
      spriteBatch.Draw(blankTexture, buttonRect1, Color.Orange);
      spriteBatch.DrawString(hudFont, "Clear", new Vector2(menuBarButton1_Left + /*50*/60, menuBarButtonTop * 2), Color.Black);

      //Draw second "Button"
      Rectangle buttonRect2 = new Rectangle(menuBarButton2_Left, menuBarButtonTop, menuBarButtonWidth, menuBarButtonHeight);
      spriteBatch.Draw(blankTexture, buttonRect2, Color.Orange);
      spriteBatch.DrawString(hudFont, tank.BehaviorType.ToString(), new Vector2(menuBarButton2_Left + 50, menuBarButtonTop * 2), Color.Black);
    }
#endif
    #endregion

    #region Handle Input

    /// <summary>
    /// Read keyboard and gamepad input
    /// </summary>
    private void HandleInput(float elapsedTime)
    {
      previousGamePadState = currentGamePadState;
      previousKeyboardState = currentKeyboardState;
      currentGamePadState = GamePad.GetState(PlayerIndex.One);
      currentKeyboardState = Keyboard.GetState();

      // Allows the game to exit
      if (currentGamePadState.Buttons.Back == ButtonState.Pressed ||
          currentKeyboardState.IsKeyDown(Keys.Escape))
        this.Exit();

      // Update the cursor location by listening for left thumbstick input on
      // the GamePad and direction key input on the Keyboard, making sure to
      // keep the cursor inside the screen boundary
      cursorLocation.X +=
          currentGamePadState.ThumbSticks.Left.X * cursorMoveSpeed * elapsedTime;
      cursorLocation.Y -=
          currentGamePadState.ThumbSticks.Left.Y * cursorMoveSpeed * elapsedTime;

      if (currentKeyboardState.IsKeyDown(Keys.Up))
      {
        cursorLocation.Y -= elapsedTime * cursorMoveSpeed;
      }
      if (currentKeyboardState.IsKeyDown(Keys.Down))
      {
        cursorLocation.Y += elapsedTime * cursorMoveSpeed;
      }
      if (currentKeyboardState.IsKeyDown(Keys.Left))
      {
        cursorLocation.X -= elapsedTime * cursorMoveSpeed;
      }
      if (currentKeyboardState.IsKeyDown(Keys.Right))
      {
        cursorLocation.X += elapsedTime * cursorMoveSpeed;
      }

#if WINDOWS_PHONE
      bool isTouchDetected = false;
      bool isMenuBarUsed = false;
      
      TouchCollection touches = TouchPanel.GetState();

      if (touches.Count == 1)
      {
        //Use only the single (first) touch point to make experience close to Windows/XBOX - Simulates move by mouse/gamepad & "A" button press
        TouchLocation touch = touches[0];
        if (touch.State != TouchLocationState.Invalid)
        {

          //TODO: Check on device and swap X/Y + remove Y tweaking. Workaround for EMU:
          double halfHeight = screenHeight / 2;
          double delta = halfHeight - touch.Position.X;

          //Check "button click" on menuBar
          if (halfHeight + delta < menuBar_Height) 
          {
            Rectangle touchRect = new Rectangle((int)touch.Position.Y, (int)(halfHeight + delta - 5) - 5, 10, 10);
            Rectangle button1Rect = new Rectangle(menuBarButton1_Left, menuBarButtonTop, menuBarButtonWidth, menuBarButtonHeight);
            Rectangle button2Rect = new Rectangle(menuBarButton2_Left, menuBarButtonTop, menuBarButtonWidth, menuBarButtonHeight);

            bool button1Press, button2Press;
            button1Rect.Intersects(ref touchRect, out button1Press);
            button2Rect.Intersects(ref touchRect, out button2Press);

            if (button1Press && touch.State == TouchLocationState.Released)
              isClearRequested = true;
            else if (button2Press && touch.State == TouchLocationState.Released)
              isBehaviorChangeRequested = true;

            isMenuBarUsed = true;
          }
          else
          {
            cursorLocation.X = touch.Position.Y;
            cursorLocation.Y = (int)(halfHeight + delta);
          }              

          //Don't let the cursor move under the MenuBar
          if (cursorLocation.Y < menuBar_Height + (cursorTexture.Height / 2)) 
            cursorLocation.Y = menuBar_Height + (cursorTexture.Height / 2);
        }

        if (touch.State == TouchLocationState.Released && !isMenuBarUsed)
          isTouchDetected = true;
      }
      else if (touches.Count > 1 && touches[0].State == TouchLocationState.Released) //Multiple touch simulates pressing "B" button press to change the behavior
        isBehaviorChangeRequested = true;
#endif

      cursorLocation.X = MathHelper.Clamp(cursorLocation.X, 0f, screenWidth);
      cursorLocation.Y = MathHelper.Clamp(cursorLocation.Y, 0f, screenHeight);

      // Change the tank move behavior if the user pressed B on
      // the GamePad or on the Keyboard.
      if ((previousGamePadState.Buttons.B == ButtonState.Released &&
          currentGamePadState.Buttons.B == ButtonState.Pressed) ||
          (previousKeyboardState.IsKeyUp(Keys.B) &&
          currentKeyboardState.IsKeyDown(Keys.B))
#if WINDOWS_PHONE
        || isBehaviorChangeRequested
#endif
        )
      {
        tank.CycleBehaviorType();

#if WINDOWS_PHONE
        isBehaviorChangeRequested = false;
#endif
      }

      // Add the cursor's location to the WaypointList if the user pressed A on
      // the GamePad or on the Keyboard.
      if ((previousGamePadState.Buttons.A == ButtonState.Released &&
          currentGamePadState.Buttons.A == ButtonState.Pressed) ||
          (previousKeyboardState.IsKeyUp(Keys.A) &&
          currentKeyboardState.IsKeyDown(Keys.A)) 
#if WINDOWS_PHONE
        || isTouchDetected
#endif
        )
      {
        tank.Waypoints.Enqueue(cursorLocation);
      }

      // Delete all the current waypoints and reset the tanks’ location if 
      // the user pressed X on the GamePad or on the Keyboard.
      if ((previousGamePadState.Buttons.X == ButtonState.Released &&
          currentGamePadState.Buttons.X == ButtonState.Pressed) ||
          (previousKeyboardState.IsKeyUp(Keys.X) &&
          currentKeyboardState.IsKeyDown(Keys.X))
#if WINDOWS_PHONE
          || isClearRequested
#endif
        )
      {
        tank.Reset(
            new Vector2((float)screenWidth / 4, (float)screenHeight / 4));

#if WINDOWS_PHONE
        isClearRequested = false;
#endif
      }
    }

    #endregion
  }
}
