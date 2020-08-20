#region File Information
//-----------------------------------------------------------------------------
// DynamicMenuSample.cs
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
using DynamicMenu.Controls;
#endregion

namespace DynamicMenuSample
{
    /// <summary>
    /// This class shows how to use the DynamicMenu library for a Windows Phone 7 application.
    /// </summary>
    public class DynamicMenuSample : Game
    {
        #region Fields

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private PhoneScreen phoneScreen = new PhoneScreen();

        private Container dynamicControlsContainer;
        private Container loadedControlsContainer;

        private List<Button> pageButtons = new List<Button>();

        // Page 1 controls
        private Button hueChangeButton;
        private Button textChangeButton;
        private Button bouncingButton;
        private Button getBigButton;

        // Page 1 bouncing button parameters
        private bool bouncingButtonActive = false;
        private int bouncingButtonChange = 200;
        private int bouncingButtonLeft = 0;
        private int bouncingButtonStartLeft = 0;

        private int textButtonIndex = 1;

        // Used for passing to the menu controls
        private List<GestureSample> gestureList = new List<GestureSample>();

        #endregion

        #region Definitions

        enum EDynamicControlPage
        {
            Page1,
            Page2,
            Page3
        }

        #endregion

        #region Initialization

        public DynamicMenuSample()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Lock to portrait mode
            graphics.SupportedOrientations = DisplayOrientation.Portrait |
                DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            graphics.IsFullScreen = true;

            TouchPanel.EnabledGestures = GestureType.Tap;

            // Portrait is the default given the back buffer width/height
            phoneScreen.CurrentOrientation = DisplayOrientation.Portrait;

            this.Window.OrientationChanged += new EventHandler<EventArgs>(Window_OrientationChanged);
        }

        void Window_OrientationChanged(object sender, EventArgs e)
        {
            phoneScreen.CurrentOrientation = Window.CurrentOrientation;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Build the top container
            AddPageButton(phoneScreen.Container1, 0, "Page 1", EDynamicControlPage.Page1);
            AddPageButton(phoneScreen.Container1, 135, "Page 2", EDynamicControlPage.Page2);
            AddPageButton(phoneScreen.Container1, 270, "Page 3", EDynamicControlPage.Page3);


            dynamicControlsContainer = new Container();
            // Fills the container
            dynamicControlsContainer.Left = 0;
            dynamicControlsContainer.Top = 0;
            dynamicControlsContainer.Width = 400;
            dynamicControlsContainer.Height = 400;

            // Set up Page 1's buttons
            hueChangeButton = new Button();
            hueChangeButton.Left = 100;
            hueChangeButton.Top = 10;
            hueChangeButton.Width = 200;
            hueChangeButton.Height = 80;
            hueChangeButton.Hue = Color.Green;
            hueChangeButton.Text = "Change Hue";
            hueChangeButton.TextColor = Color.White;
            hueChangeButton.FontName = @"Fonts\ControlFont";
            hueChangeButton.BackTextureName = @"Textures\button";
            hueChangeButton.PressedTextureName = @"Textures\buttonpressed";
            hueChangeButton.Tapped += new EventHandler(hueChangeButton_Tapped);

            dynamicControlsContainer.AddControl(hueChangeButton);

            textChangeButton = new Button();
            textChangeButton.Left = 100;
            textChangeButton.Top = 100;
            textChangeButton.Width = 200;
            textChangeButton.Height = 80;
            textChangeButton.Hue = Color.Red;
            textChangeButton.Text = "Index: " + textButtonIndex;
            textChangeButton.TextColor = Color.White;
            textChangeButton.FontName = @"Fonts\ControlFont";
            textChangeButton.BackTextureName = @"Textures\button";
            textChangeButton.PressedTextureName = @"Textures\buttonpressed";
            textChangeButton.Tapped += new EventHandler(textChangeButton_Tapped);

            dynamicControlsContainer.AddControl(textChangeButton);

            bouncingButton = new Button();
            bouncingButton.Left = 100;
            bouncingButton.Top = 190;
            bouncingButton.Width = 200;
            bouncingButton.Height = 80;
            bouncingButton.Hue = Color.Blue;
            bouncingButton.Text = "Bounce";
            bouncingButton.TextColor = Color.White;
            bouncingButton.FontName = @"Fonts\ControlFont";
            bouncingButton.BackTextureName = @"Textures\button";
            bouncingButton.PressedTextureName = @"Textures\buttonpressed";
            bouncingButton.Tapped += new EventHandler(bouncingButton_Tapped);

            dynamicControlsContainer.AddControl(bouncingButton);


            getBigButton = new Button();
            getBigButton.Left = 100;
            getBigButton.Top = 280;
            getBigButton.Width = 200;
            getBigButton.Height = 80;
            getBigButton.Hue = Color.Purple;
            getBigButton.Text = "Get big";
            getBigButton.TextColor = Color.White;
            getBigButton.FontName = @"Fonts\ControlFont";
            getBigButton.BackTextureName = @"Textures\button";
            getBigButton.PressedTextureName = @"Textures\buttonpressed";
            getBigButton.Tapped += new EventHandler(getBigButton_Tapped);

            dynamicControlsContainer.AddControl(getBigButton);

            phoneScreen.Container2.AddControl(dynamicControlsContainer);
            phoneScreen.Container2.BackTextureName = @"Textures\checkerboard";

            // Set up Page 2 and Page 3's container
            loadedControlsContainer = new Container();
            // Fills the container
            loadedControlsContainer.Left = 0;
            loadedControlsContainer.Top = 0;
            loadedControlsContainer.Width = 400;
            loadedControlsContainer.Height = 400;

            phoneScreen.Container2.AddControl(loadedControlsContainer);

            phoneScreen.Initialize();

            base.Initialize();

            // Start by showing page 1
            ShowPage(EDynamicControlPage.Page1);

        }

        /// <summary>
        /// Adds a button to change to a page of controls.
        /// </summary>
        /// <param name="container">The container to add the button to.</param>
        /// <param name="left">The left position of the button.</param>
        /// <param name="text">The text to set the button text to.</param>
        /// <param name="page">The enumeration for the page that will be shown when this button is tapped.</param>
        private void AddPageButton(Container container, int left, string text, EDynamicControlPage page)
        {
            Button button = new Button();
            button.Left = left;
            button.Top = 10;
            button.Width = 130;
            button.Text = text;
            button.Height = 80;
            button.FontName = @"Fonts\ControlFont";
            button.BackTextureName = @"Textures\button";
            button.PressedTextureName = @"Textures\buttonpressed";
            button.Tag = page;
            button.Tapped += new EventHandler(pageButton_Tapped);

            container.AddControl(button);
            pageButtons.Add(button);
        }

        /// <summary>
        /// Shows the specified page of controls.
        /// </summary>
        /// <param name="page">The page to show.</param>
        private void ShowPage(EDynamicControlPage page)
        {
            // Change the color of the buttons to represent the current choice
            foreach (Button pageButton in pageButtons)
            {
                EDynamicControlPage buttonPage = (EDynamicControlPage)pageButton.Tag;

                if (buttonPage == page)
                {
                    pageButton.TextColor = Color.Black;
                    pageButton.Hue = Color.Yellow;
                }
                else
                {
                    pageButton.TextColor = Color.LightGray;
                    pageButton.Hue = Color.DarkGray;
                }
            }

            // Hide both containers
            dynamicControlsContainer.Visible = false;
            loadedControlsContainer.Visible = false;

            switch (page)
            {
                case EDynamicControlPage.Page1:
                    dynamicControlsContainer.Visible = true;
                    break;
                case EDynamicControlPage.Page2:
                    loadedControlsContainer.Visible = true;
                    LoadControls(@"Menus\MenuPage2");
                    break;
                case EDynamicControlPage.Page3:
                    loadedControlsContainer.Visible = true;
                    Container container = LoadControls(@"Menus\MenuPage3");

                    SetupPage3(container);
                    break;

            }
        }

        /// <summary>
        /// Load the controls for one of the dynamically loaded pages
        /// </summary>
        /// <param name="sampleName">Xml asset tag to load</param>
        /// <returns>The container that was created</returns>
        private Container LoadControls(string sampleName)
        {
            loadedControlsContainer.Controls.Clear();

            Container loadedContainer = Content.Load<Container>(sampleName);
            loadedContainer.Initialize();
            loadedContainer.LoadContent(GraphicsDevice, Content);

            loadedControlsContainer.AddControl(loadedContainer);

            return loadedContainer;
        }

        /// <summary>
        /// Sets up the actions for the dynamically loaded controls for page 3
        /// </summary>
        /// <param name="container">The container that contains Page 3's controls</param>
        private void SetupPage3(Container container)
        {
            IControl control = container.FindControlByName("AdvanceButton");
            Button advanceButton = control as Button;

            if (advanceButton == null)
            {
                throw new Exception("Failed to find the control named AdvanceButton in MenuPage3.xml");
            }

            control = container.FindControlByName("ProgressBar");

            if (control == null)
            {
                throw new Exception("Failed to find the control nameed ProgressBar in MenuPage3.xml");
            }

            advanceButton.Tag = control;
            advanceButton.Tapped += new EventHandler(advanceButton_Tapped);
        }

        #endregion

        #region Load and unload

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            phoneScreen.LoadContent(GraphicsDevice, Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        #endregion

        #region Update

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

            gestureList.Clear();

            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample sample = TouchPanel.ReadGesture();
                gestureList.Add(sample);
            }

            if (bouncingButtonActive)
            {
                bouncingButtonLeft += (int)(bouncingButtonChange * gameTime.ElapsedGameTime.TotalSeconds);

                if (bouncingButtonLeft + bouncingButton.Width > 400 && bouncingButtonChange > 0)
                {
                    bouncingButtonLeft = 400 - bouncingButton.Width;
                    bouncingButtonChange *= -1;
                }
                else if (bouncingButtonChange < 0 && bouncingButtonLeft < bouncingButtonStartLeft)
                {
                    bouncingButtonLeft = bouncingButtonStartLeft;
                    bouncingButtonActive = false;
                }

                bouncingButton.Left = bouncingButtonLeft;
            }

            phoneScreen.Update(gameTime, gestureList);

            base.Update(gameTime);
        }

        #endregion

        #region Render

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            phoneScreen.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for a page button.  Changes the page of controls being shown.
        /// </summary>
        void pageButton_Tapped(object sender, EventArgs e)
        {
            Button button = sender as Button;

            EDynamicControlPage page = (EDynamicControlPage)button.Tag;

            ShowPage(page);
        }

        /// <summary>
        /// Handler for Page 1's hue change button.  This applies a random color to the background of the 
        /// button.
        /// </summary>
        void hueChangeButton_Tapped(object sender, EventArgs e)
        {
            Button button = sender as Button;

            Random rnd = new Random();
            // Choose a random color, 0.0-1.0 for each component
            float r = (float)rnd.NextDouble();
            float g = (float)rnd.NextDouble();
            float b = (float)rnd.NextDouble();
            //Make the alpha value a random value between 0.5 and 1.0
            float a = (float)(rnd.NextDouble() * .5 + .5);

            Color newColor = new Color(r, g, b) * a;
            // Font color complements the hue
            Color fontColor = new Color(1.0f - r, 1.0f - g, 1.0f - b);

            // Apply a transition which changes the hue over 2 seconds
            DynamicMenu.Transitions.Transition transition =
                new DynamicMenu.Transitions.Transition(null, null, null, null, button.Hue, newColor);
            transition.TransitionLength = 2.0f;

            button.ApplyTransition(transition);

            // Set the text color immediately
            button.TextColor = fontColor;
        }

        /// <summary>
        /// Handler for Page 1's text change button.
        /// </summary>
        void textChangeButton_Tapped(object sender, EventArgs e)
        {
            Button button = sender as Button;

            // Increment the index to display
            ++textButtonIndex;
            button.Text = "Index: " + textButtonIndex;
        }

        /// <summary>
        /// Handler for Page 1's bouncing button.  This starts the process to animate the position of the button
        /// </summary>
        void bouncingButton_Tapped(object sender, EventArgs e)
        {
            Button button = sender as Button;

            // don't respond when we're already bouncing
            if (bouncingButtonActive)
            {
                return;
            }

            // Start the button movement
            bouncingButtonActive = true;
            if (bouncingButtonChange < 0)
            {
                // make the change positive
                bouncingButtonChange *= -1;
            }
            bouncingButtonStartLeft = button.Left;
            bouncingButtonLeft = button.Left;
        }

        /// <summary>
        /// Handler for Page 1's getBig button.  This applies a transition to make the control larger
        /// </summary>
        void getBigButton_Tapped(object sender, EventArgs e)
        {
            Button button = sender as Button;

            // Apply a transition that will make the control bigger in place
            DynamicMenu.Transitions.Transition transition = new DynamicMenu.Transitions.Transition(null,
                new Point(0, 240), null, new Point(400, 160), null, null);
            // Create a handler for when the above transition is complete
            transition.TransitionComplete += new EventHandler(getBig_TransitionComplete);

            button.ApplyTransition(transition);

        }

        /// <summary>
        /// Handler for the completion of Page 1's getBig button transition.  This will return the control to its 
        /// normal size.
        /// </summary>
        void getBig_TransitionComplete(object sender, EventArgs e)
        {
            DynamicMenu.Transitions.Transition oldTransition = sender as DynamicMenu.Transitions.Transition;
            Button button = oldTransition.Control as Button;

            // Apply a transition that will return the control to it's original size
            DynamicMenu.Transitions.Transition newTransition = new DynamicMenu.Transitions.Transition(null,
                    new Point(100, 280), null, new Point(200, 80), null, null);
            button.ApplyTransition(newTransition);
        }

        /// <summary>
        /// Handler for Page 3's Advance button.  This will increment the position of the progress bar.
        /// </summary>
        void advanceButton_Tapped(object sender, EventArgs e)
        {
            Button button = sender as Button;
            ProgressBar progressBar = button.Tag as ProgressBar;

            // Advance the progress bar by 10 units
            int curProgress = progressBar.Position;
            curProgress += 10;

            // The position starts at the beginning again
            if (curProgress > progressBar.MaxValue)
            {
                curProgress = 0;
            }

            progressBar.Position = curProgress;
        }

        #endregion
    }
}
