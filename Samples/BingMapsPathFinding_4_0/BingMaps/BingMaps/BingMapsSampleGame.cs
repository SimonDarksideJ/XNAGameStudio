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

        Button changeRotuingModeButton;
        Button switchViewButton;
        Button serachButton;
        Button drawRouteButton;
        Button clearPushPinsButton;

        Texture2D tankTexture2D;
        Texture2D pushPinTexture2D;

        // Web client used to retrieve the coordinates of locations request by the user
        WebClient locationWebClient;
        string locationToFocusOn;

        Tank tank;
        LinkedList<PushPin> pushPins = new LinkedList<PushPin>();
        bool isDragingTank = false;

        public int ZoomLevel { get; private set; }


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

            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.FreeDrag | GestureType.DragComplete
                | GestureType.Hold;

            ZoomLevel = 17;
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

            // Loads assets
            blank = Content.Load<Texture2D>("blank");
            buttonFont = Content.Load<SpriteFont>("Font");
            tankTexture2D = Content.Load<Texture2D>("tank");
            pushPinTexture2D = Content.Load<Texture2D>("pushpin");
            Texture2D defaultImage = Content.Load<Texture2D>("noImage");
            Texture2D unavailableImage = Content.Load<Texture2D>("noImage");

            // Creates the button that allows the user to switch between maps
            switchViewButton = new Button("Switch to\nRoad View", buttonFont, Color.White,
                new Rectangle(10, 10, 110, 60), Color.Black, blank, spriteBatch);
            switchViewButton.Click += switchViewButton_Click;

            // The coordinate that the app will focus
            startingCoordinate = new GeoCoordinate(47.639597, -122.12845);

            bingMapsViewer = new BingMapsViewer(BingAppKey, defaultImage, unavailableImage,
                startingCoordinate, 5, ZoomLevel, spriteBatch);

            tank = new Tank(this, spriteBatch, tankTexture2D, bingMapsViewer, new Vector2(400, 240), pushPins);
            Components.Add(tank);

            changeRotuingModeButton = new Button(string.Format("Switch to\n{0}",Tank.BingRoutingMode.Walking),
                        buttonFont, Color.White, new Rectangle(10, 80, 110, 60), Color.Black, blank, spriteBatch);
            changeRotuingModeButton.Click += new EventHandler(changeRotuingModeButton_Click);

            serachButton = new Button("Search",
                        buttonFont, Color.White, new Rectangle(680, 10, 110, 60), Color.Black, blank, spriteBatch);
            serachButton.Click += new EventHandler(serachButton_Click);

            drawRouteButton = new Button("Show\nRoute",
                        buttonFont, Color.White, new Rectangle(680, 80, 110, 60), Color.Black, blank, spriteBatch);
            drawRouteButton.Click += new EventHandler(drawRouteButton_Click);

            clearPushPinsButton = new Button("Clear Road",
                        buttonFont, Color.White, new Rectangle(680, 160, 110, 60), Color.Black, blank, spriteBatch);
            clearPushPinsButton.Click +=new EventHandler(clearPushPinsButton_Click);
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
            TouchCollection touchCollection = TouchPanel.GetState();

            if (touchCollection.Count > 0)
            {
                TouchLocation touch = touchCollection[0];

                // If the tank is moving he can not be relocated
                if (!tank.IsMoving)
                {
                    if (touch.State == TouchLocationState.Pressed && !isDragingTank)
                    {
                        Rectangle toucRect = new Rectangle((int)touch.Position.X - 5, 
                                    (int)touch.Position.Y - 5, 10, 10);

                        if (tank.Bounds.Intersects(toucRect))
                        {
                            isDragingTank = true;
                        }
                    }
                    else if (touch.State == TouchLocationState.Moved && isDragingTank)
                    {
                        tank.Move(touch.Position);
                    }
                    else if (touch.State == TouchLocationState.Released && isDragingTank)
                    {
                        isDragingTank = false;
                    }
                }
            }

            // Read all gesture
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample sample = TouchPanel.ReadGesture();

                // If the search button is clicked, the button will handle it.
                if (serachButton.HandleInput(sample))
                    return;

                // If the switch button is clicked, the button will handle it.
                if (switchViewButton.HandleInput(sample))
                    return;

                // If the change mode button is clicked, the button will handle it.
                if (changeRotuingModeButton.HandleInput(sample))
                    return;

                if (tank.IsMoving)
                {
                    // If the draw route button is clicked, the button will handle it.
                    if (drawRouteButton.HandleInput(sample))
                        return;

                    // If the clear all pushpins button is clicked, the button will handle it.
                    if (clearPushPinsButton.HandleInput(sample))
                        return;
                }

                if (sample.GestureType == GestureType.Tap)
                {
                    // Gets the position on the displayed map
                    Vector2 positionOnMap = TileSystem.LatLongToPixelXY(bingMapsViewer.CenterGeoCoordinate, ZoomLevel)
                        - bingMapsViewer.Offset;

                    // Gets the distance between the gesture and the center of the screen
                    Vector2 distance = sample.Position - BingMapsViewer.ScreenCenter;
                    // Calculate the distance between the center of the screen and the position on the map
                    Vector2 positionOfGesture = positionOnMap + distance;

                    PushPin pushPin = new PushPin(bingMapsViewer,
                        TileSystem.PixelXYToLatLong(positionOfGesture, ZoomLevel), pushPinTexture2D);
                    pushPins.AddLast(pushPin);

                    // Indicates to the Tank that new push pin is added
                    tank.AddPushPin(pushPin);
                }
                else if (sample.GestureType == GestureType.FreeDrag)
                {
                    if (!isDragingTank)
                        // Move the map when dragging
                        bingMapsViewer.MoveByOffset(sample.Delta);
                }
                // When an hold gesture is caught on push pin ask if wants to delete
                else if (sample.GestureType == GestureType.Hold)
                {
                    // Creates a rectangle around the gesture
                    Rectangle touchRectangle = new Rectangle(
                        (int)sample.Position.X - 20,
                        (int)sample.Position.Y - 40, 60, 100);


                    foreach (PushPin pushPin in pushPins)
                    {
                        // Gets the center of the map displayed
                        Vector2 center = TileSystem.LatLongToPixelXY(bingMapsViewer.CenterGeoCoordinate,
                            bingMapsViewer.ZoomLevel);

                        // Gets the position of the push pin on the map
                        Vector2 position = TileSystem.LatLongToPixelXY(pushPin.Location, bingMapsViewer.ZoomLevel);

                        // Calculate the distance between them in screen scale
                        Vector2 targetPosition = (position - center) + BingMapsViewer.ScreenCenter +
                            bingMapsViewer.Offset;

                        // Checks if the push pin is in side the gesture rectangle
                        if (touchRectangle.Contains(new Rectangle((int)targetPosition.X, (int)targetPosition.Y, 1, 1)))
                        {
                            Guide.BeginShowMessageBox("Warning!", "Are you sure you want to delete this push pin?",
                                new List<string>() { "OK", "Cancel" }, 0, MessageBoxIcon.Alert,
                                DeletePushPinCallBack, pushPin);
                            return;
                        }

                    }
                }
            }
        }



        /// <summary>
        /// DrawTank the map on the screen.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);
            
            spriteBatch.Begin();

            // DrawTank the map 
            bingMapsViewer.Draw();

            foreach (PushPin pushPin in pushPins)
            {
                pushPin.Draw();
            }

            spriteBatch.End();

            spriteBatch.Begin();
            tank.DrawTank(gameTime);
            spriteBatch.End();

            spriteBatch.Begin();

            switchViewButton.Draw();

            serachButton.Draw();

            changeRotuingModeButton.Draw();


            if (tank.IsMoving)
            {
                drawRouteButton.Draw();
                clearPushPinsButton.Draw();

            }
            spriteBatch.End();
         

            base.Draw(gameTime);
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the user input a delete.
        /// </summary>
        /// <param name="ar"></param>
        public void DeletePushPinCallBack(IAsyncResult ar)
        {
            if (Guide.EndShowMessageBox(ar) == 0)
            {
                pushPins.Remove(ar.AsyncState as PushPin);
                tank.DeletePushPin(ar.AsyncState as PushPin);
            }

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
                tank.Move(receivedCoordinate);
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

                switchViewButton.Text = String.Format("Switch to\n{0} View", previousType);
                bingMapsViewer.RefreshImages();
            }
        }

        /// <summary>
        /// Handler called when the user clicks the button to change the routing mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void changeRotuingModeButton_Click(object sender, EventArgs e)
        {
            changeRotuingModeButton.Text = string.Format("Switch to\n{0}", tank.RoutingMode);

            switch (tank.RoutingMode)
        	{
		         case Tank.BingRoutingMode.Driving:
                    tank.RoutingMode = Tank.BingRoutingMode.Walking;
                     break;
                case Tank.BingRoutingMode.Walking:
                    tank.RoutingMode = Tank.BingRoutingMode.Driving;
                     break;
            }
        }

        /// <summary>
        /// Handler called when the user clicks the button to search new location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void serachButton_Click(object sender, EventArgs e)
        {
            if (!Guide.IsVisible)
                Guide.BeginShowKeyboardInput(PlayerIndex.One, "Select location", "Type in a location to focus on.",
                    String.Empty, LocationSelected, null);
        }

        /// <summary>
        /// Handler called when the user clicks the button to change the drawing mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void drawRouteButton_Click(object sender, EventArgs e)
        {
            tank.ShouldDrawRoute = !tank.ShouldDrawRoute;

            if (tank.ShouldDrawRoute)
            {
                drawRouteButton.Text = "Hide\nRoute";
            }
            else
            {
                drawRouteButton.Text = "Show\nRoute";
            }
        }

             /// <summary>
        /// Handler called when the user clicks the button to change the drawing mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void  clearPushPinsButton_Click(object sender, EventArgs e)
        {
            pushPins.Clear();
            tank.ClearRoute();
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
