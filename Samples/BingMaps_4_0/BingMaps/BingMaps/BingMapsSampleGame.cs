#region File Description
//-----------------------------------------------------------------------------
// BingMapsSampleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Disclaimer
/******************************************************************************
 *                                                                            *
 *               This sample is a data heavy application                      *
 *                                                                            *
 *           Make sure  you have a data plan when deploy to phone             *
 *                                                                            *
 ******************************************************************************/
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
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Device.Location;
using System.Net;
using System.Xml.Linq;
using System.Xml;
#endregion

namespace BingMaps
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BingMapsSampleGame : Game
    {
        #region Fields


        Texture2D blank;
        SpriteFont buttonFont;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GeoCoordinate startingCoordinate;

        BingMapsViewer bingMapsViewer;

        #error For the sample to work, you need to acquire a Bing Maps key. See http://www.bingmapsportal.com/
        const string BingAppKey = "<Bing Maps API Key>";
        Button switchViewButton;

        // Web client used to retrieve the coordinates of locations request by the user
        WebClient locationWebClient;
        string locationToFocusOn;


        #endregion

        #region Initialization


        public BingMapsSampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            locationWebClient = new WebClient();
            locationWebClient.OpenReadCompleted += new OpenReadCompletedEventHandler(ReceivedLocationCoordinates);

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.IsFullScreen = true;

            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag | GestureType.DragComplete;
        }


        #endregion

        #region Loading


        /// <summary>
        /// Load Bing maps assets.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures
            spriteBatch = new SpriteBatch(GraphicsDevice);

            blank = Content.Load<Texture2D>("blank");
            buttonFont = Content.Load<SpriteFont>("Font");

            switchViewButton = new Button("Switch to\nRoad view", buttonFont, Color.White,
                new Rectangle(10, 10, 100, 60), Color.Black, blank, spriteBatch);
            switchViewButton.Click += switchViewButton_Click;

            startingCoordinate = new GeoCoordinate(47.639597, -122.12845);

            Texture2D defaultImage = Content.Load<Texture2D>("noImage");
            Texture2D unavailableImage = Content.Load<Texture2D>("noImage");
            bingMapsViewer = new BingMapsViewer(BingAppKey, defaultImage, unavailableImage,
                startingCoordinate, 5, 15, spriteBatch);
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Update the Bing maps application.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // If we have a pending focus request and the web client is not busy, perform it
            if (locationToFocusOn != null && !locationWebClient.IsBusy)
            {
                FocusOnLocationAsync(locationToFocusOn);
                locationToFocusOn = null;
            }

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            HandleInput();

            base.Update(gameTime);
        }

        /// <summary>
        /// Handle the input of the sample.
        /// </summary>
        private void HandleInput()
        {
            // Read all gesture
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample sample = TouchPanel.ReadGesture();

                if (switchViewButton.HandleInput(sample))
                    return;

                if (sample.GestureType == GestureType.Tap)
                {
                    if (!Guide.IsVisible)
                        Guide.BeginShowKeyboardInput(PlayerIndex.One, "Select location", "Type in a location to focus on.",
                            String.Empty, LocationSelected, null);
                }
                else if (sample.GestureType == GestureType.FreeDrag)
                {
                    // Move the map when dragging
                    bingMapsViewer.MoveByOffset(sample.Delta);
                }
            }
        }

        /// <summary>
        /// Draw the map on the screen.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            spriteBatch.Begin();

            // Draw the map 
            bingMapsViewer.Draw();

            switchViewButton.Draw();


            spriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Non-Public Methods


        /// <summary>
        /// Sends an asynchronous web request to retrieve the coordinates of a location on which the user wishes to
        /// focus.
        /// </summary>
        /// <param name="locationToFocusOn">String describing the location to focus on.</param>
        private void FocusOnLocationAsync(string locationToFocusOn)
        {
            locationWebClient.OpenReadAsync(
                new Uri(String.Format(@"http://dev.virtualearth.net/REST/v1/Locations?o=xml&q={0}&key={1}",
                    locationToFocusOn, BingAppKey), UriKind.Absolute));
        }

        /// <summary>
        /// Handler called when the request for a location's coordinates returns.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceivedLocationCoordinates(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                return;
            }

            if (e.Error != null)
            {
                if (!Guide.IsVisible)
                    Guide.BeginShowMessageBox("Error focusing on location", e.Error.Message, new string[] { "OK" },
                        0, MessageBoxIcon.Error, null, null);
                return;
            }

            GeoCoordinate receivedCoordinate;

            try
            {
                // Parse the response XML to get the location's geo-coordinate
                XDocument locationResponseDoc = XDocument.Load(e.Result);
                XNamespace docNamespace = locationResponseDoc.Root.GetDefaultNamespace();
                var locationNodes = locationResponseDoc.Descendants(XName.Get("Location", docNamespace.NamespaceName));

                if (locationNodes.Count() == 0)
                {
                    Guide.BeginShowMessageBox("Invalid location",
                        "The requested location was not recognized by the system.", new string[] { "OK" },
                        0, MessageBoxIcon.Error, null, null);
                    return;
                }

                XElement pointNode = locationNodes.First().Descendants(
                    XName.Get("Point", docNamespace.NamespaceName)).FirstOrDefault();

                if (pointNode == null)
                {
                    Guide.BeginShowMessageBox("Invalid location result", "The location result is missing data.",
                        new string[] { "OK" }, 0, MessageBoxIcon.Error, null, null);
                    return;
                }

                XElement longitudeNode = pointNode.Element(XName.Get("Longitude", docNamespace.NamespaceName));
                XElement latitudeNode = pointNode.Element(XName.Get("Latitude", docNamespace.NamespaceName));

                if (longitudeNode == null || latitudeNode == null)
                {
                    Guide.BeginShowMessageBox("Invalid location result", "The location result is missing data.",
                        new string[] { "OK" }, 0, MessageBoxIcon.Error, null, null);
                    return;
                }

                receivedCoordinate = new GeoCoordinate(double.Parse(latitudeNode.Value),
                    double.Parse(longitudeNode.Value));
            }
            catch (Exception err)
            {
                Guide.BeginShowMessageBox("Error getting location coordinates", err.Message, new string[] { "OK" },
                    0, MessageBoxIcon.Error, null, null);
                return;
            }

            bingMapsViewer.CenterOnLocation(receivedCoordinate);
        }

        /// <summary>
        /// Handler called when the user clicks the button to change the view type.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void switchViewButton_Click(object sender, EventArgs e)
        {
            if (bingMapsViewer != null)
            {
                BingMapsViewType previousType = bingMapsViewer.ViewType;

                switch (bingMapsViewer.ViewType)
                {
                    case BingMapsViewType.Aerial:
                        bingMapsViewer.ViewType = BingMapsViewType.Road;
                        break;
                    case BingMapsViewType.Road:
                        bingMapsViewer.ViewType = BingMapsViewType.Aerial;
                        break;
                    default:
                        break;
                }

                switchViewButton.Text = String.Format("Switch to\n{0} view", previousType);
                bingMapsViewer.RefreshImages();
            }
        }

        /// <summary>
        /// Performs cleanup actions when exiting the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnExiting(object sender, EventArgs args)
        {
            bingMapsViewer.ActiveTiles.Dispose();

            base.OnExiting(sender, args);
        }

        /// <summary>
        /// Handler launched once the user selects a location to zoom on.
        /// </summary>
        /// <param name="result">Asynchronous call result containing the text typed by the user.</param>
        private void LocationSelected(IAsyncResult result)
        {
            locationToFocusOn = Guide.EndShowKeyboardInput(result);

            if (String.IsNullOrEmpty(locationToFocusOn))
            {
                return;
            }

            // Cancel any ongoing request, or do nothing if there was none
            locationWebClient.CancelAsync();
        }


        #endregion
    }
}
