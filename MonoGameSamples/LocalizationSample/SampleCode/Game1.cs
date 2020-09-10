using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Globalization;

namespace LocalizationSample
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class LocalizationGame : Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont MyFont;
        SpriteFont LocalizedFont;
        SpriteFont WPFFont;
        Texture2D currentFlag;

        #endregion

        #region Initialization


        public LocalizationGame()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this);

            // Tell the resource manager what language to use when loading strings.
            Strings.Culture = CultureInfo.CurrentCulture;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            MyFont = Content.Load<SpriteFont>("BasicSpriteFont");

            LocalizedFont = Content.Load<SpriteFont>("LocalizedFont");
            WPFFont = Content.Load<SpriteFont>("WPFFont");

            currentFlag = LoadLocalizedAsset<Texture2D>("Flag");
        }


        /// <summary>
        /// Helper for loading a .xnb asset which can have multiple localized
        /// versions for different countries. This allows you localize data such
        /// as textures, models, and sound effects.
        /// 
        /// This uses a simple naming convention. If you have a default asset named
        /// "Foo", you can provide a specialized French version by calling it
        /// "Foo.fr", and a Japanese version called "Foo.ja". You can specialize even
        /// further by country as well as language, so if you wanted different assets
        /// for the United States vs. United Kingdom, you would add "Foo.en-US" and
        /// "Foo.en-GB".
        /// 
        /// This function looks first for the most specialized version of the asset,
        /// which includes both language and country. If that does not exist, it looks
        /// for a version that only specifies the language. If that still does not
        /// exist, it falls back to the original non-localized asset name.
        /// </summary>
        T LoadLocalizedAsset<T>(string assetName)
        {
            string[] cultureNames =
            {
                CultureInfo.CurrentCulture.Name,                        // eg. "en-US"
                CultureInfo.CurrentCulture.TwoLetterISOLanguageName     // eg. "en"
            };

            // Look first for a specialized language-country version of the asset,
            // then if that fails, loop back around to see if we can find one that
            // specifies just the language without the country part.
            foreach (string cultureName in cultureNames)
            {
                string localizedAssetName = assetName + '.' + cultureName;

                try
                {
                    return Content.Load<T>(localizedAssetName);
                }
                catch (ContentLoadException) { }
            }

            // If we didn't find any localized asset, fall back to the default name.
            return Content.Load<T>(assetName);
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
            string string1 = Strings.Welcome;

            string string2 = string.Format(Strings.CurrentLocale,
                                           CultureInfo.CurrentCulture.EnglishName,
                                           CultureInfo.CurrentCulture);

            string string3 = Strings.HowToChange;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.DrawString(MyFont, "Welcome to the localization sample!",Vector2.One,Color.White);

            spriteBatch.DrawString(LocalizedFont, "Default font Drawing", new Vector2(100, 70), Color.White);
            spriteBatch.DrawString(LocalizedFont, string1, new Vector2(100, 100), Color.White);
            spriteBatch.DrawString(LocalizedFont, string2, new Vector2(100, 130), Color.White);
            spriteBatch.DrawString(LocalizedFont, string3, new Vector2(100, 160), Color.White);

            spriteBatch.Draw(currentFlag, new Vector2(100, 210), Color.White);

            spriteBatch.DrawString(WPFFont, "WPF font Drawing", new Vector2(100, 330), Color.White);
            spriteBatch.DrawString(WPFFont, string1, new Vector2(100, 360), Color.White);
            spriteBatch.DrawString(WPFFont, string2, new Vector2(100, 390), Color.White);
            spriteBatch.DrawString(WPFFont, string3, new Vector2(100, 420), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.IsButtonDown(Buttons.Back))
            {
                Exit();
            }
        }


        #endregion
    }
}
