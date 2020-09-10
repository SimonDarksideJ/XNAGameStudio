//-----------------------------------------------------------------------------
// Game1.cs
//
// Microsoft Advanced Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Geolocation
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SampleGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        // Use this class to access Windows Phone location service
        GeoCoordinateWatcher geoWatcher;
        
        // Current status of the location service
        GeoPositionStatus currentState = GeoPositionStatus.Initializing;

        // Text boxes for displaying current and saved geo-location information
        TextBox textCurrentGeoLocation;
        TextBox textSavedGeoLocation;

        // Buttons at the bottom of the screen
        Button buttonSaveLocation;
        Button buttonToggleNorthUp;

        // List of all UI Elements, which facilitates calling .HandleTouch() and .Draw() on the UI elements
        List<UIElement> uiElementList = new List<UIElement>();

        // Textures for displaying the direction compass on screen
        Texture2D texCircle;
        Texture2D texArrow;
        Texture2D texSpot;        

        // Isostorage is used to maintain the saved geo-location across app sessions
        IsolatedStorageFile isoStore;
        const string strDataFile = "data.bin";

        // dDistance is the distance between your current location and saved location        
        double distance = 0;

        // dBearing is in what direction the saved location is related to your current location
        double bearing = 0;

        // Current geo-location
        private GeoPosition<GeoCoordinate> currentGeoCoord = null;
        private int timesRecieved = 0;        
        GeoPosition<GeoCoordinate> CurrentGeoCoord 
        {                         
            get 
            { 
                return currentGeoCoord; 
            }
            set 
            {
                currentGeoCoord = value;
                if (value != null)
                {
                    ++timesRecieved;
                    textCurrentGeoLocation.Text = "Current location status:\n" + GeoPosToText(currentGeoCoord);
                    textCurrentGeoLocation.WriteLn(string.Format("Times recieved: {0}", timesRecieved));                    
                }
            } 
        }

        // Saved geo-location (which is the current location when you touch textMenuSaveLocation)
        private GeoPosition<GeoCoordinate> savedGeoCoord = null;
        GeoPosition<GeoCoordinate> SavedGeoCoord
        {
            get
            {
                return savedGeoCoord;
            }
            set
            {
                savedGeoCoord = value;
                if (value != null)
                    textSavedGeoLocation.Text = "Saved location status:\n" + GeoPosToText(savedGeoCoord);
            }
        }        

        /// <summary>
        /// Converts geo-location to display friendly string
        /// </summary>        
        static string GeoPosToText( GeoPosition<GeoCoordinate> geoPos )
        {
            return string.Format("Time:        {0}\n" +
                                 "Pos (Latitude, Longitude, Altitude):\n" +
                                 "             {1:0.0000}, {2:0.0000}, {3:0.00}\n" +
                                 "Speed:       {4:0.000}m/s\n" +
                                 "Course:      {5:0.0}, {8}\n" +
                                 "HorizontalAccuracy: {6}m\n" + 
                                 "VerticalAccuracy:   {7}m\n",                                    
                                 geoPos.Timestamp.ToString(),
                                 geoPos.Location.Latitude, geoPos.Location.Longitude, geoPos.Location.Altitude,
                                 geoPos.Location.Speed,
                                 geoPos.Location.Course,
                                 geoPos.Location.HorizontalAccuracy,
                                 geoPos.Location.VerticalAccuracy,
                                 BearingToStr(geoPos.Location.Course)
                                 );
        }

        /// <summary>
        /// Converts degree angle to radian
        /// </summary>        
        static double DegreeToRadian(double angle)
        {
            return (angle / 180.0) * Math.PI;
        }

        /// <summary>
        /// Converts radian angle to degree
        /// </summary>        
        static double RadianToDegree(double radian)
        {
            return (radian / Math.PI) * 180.0;
        }
                        
        /// <summary>
        /// Caculates in what direction the dst location is related to src location, which is called Initial Bearing
        /// </summary>
        /// <param name="source">Source geo-location</param>
        /// <param name="destination">Destination geo-location</param>
        /// <returns>Initial Bearing in degrees</returns>
        static double InitialBearing(GeoCoordinate source, GeoCoordinate destination)
        {
            var lat1 = DegreeToRadian(source.Latitude);
            var lat2 = DegreeToRadian(destination.Latitude);
            var dLon = DegreeToRadian(destination.Longitude - source.Longitude);

            var y = Math.Sin(dLon) * Math.Cos(lat2);
            var x = Math.Cos(lat1) * Math.Sin(lat2) -
                    Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
            var brng = Math.Atan2(y, x);

            return (RadianToDegree(brng) + 360) % 360;
        }

        /// <summary>
        /// Converts bearing (or course) to display friendly directions N, E, S or W
        /// </summary>        
        static string BearingToStr(double course)
        {
            if (double.IsNaN(course))
                return string.Empty;
            
            if ((course >= 0 && course < 45) || (course >= 315 && course < 360))
                return "N";
            else
            if (course >= 45 && course < 135)
                return "E";
            else
            if (course >= 135 && course < 225)
                return "S";
            else
            //if (course >= 225 && course < 315)
                return "W";
        }

        public SampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Pre-autoscale settings.
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;                      

            isoStore = IsolatedStorageFile.GetUserStoreForApplication();

            // Request high precision location service and start the service
            geoWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);            
            geoWatcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(geoWatcher_StatusChanged);
            geoWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(geoWatcher_PositionChanged);
            geoWatcher.MovementThreshold = 0.5;
            geoWatcher.Start();            
        }

        void  buttonToggleNorthUp_OnClick(object sender)
        {
 	        if (buttonToggleNorthUp.Text == "Toggle: North up")
                buttonToggleNorthUp.Text = "Toggle: Your dir up";
            else
                buttonToggleNorthUp.Text = "Toggle: North up";
        }

        void  buttonSaveLocation_OnClick(object sender)
        {
 	        SavedGeoCoord = geoWatcher.Position;
        }        

        void ExitGame(IAsyncResult result)
        {
            Exit();
        }

        void geoWatcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            currentState = e.Status;

            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    if (geoWatcher.Permission == GeoPositionPermission.Denied)
                    {
                        string[] strings = { "ok" };
                        Guide.BeginShowMessageBox("Info", "Please turn on geo-location service in the settings tab.", strings, 0, 0, ExitGame, null);
                    }
                    else
                    if (geoWatcher.Permission == GeoPositionPermission.Granted)
                    {
                        string[] strings = { "ok" };
                        Guide.BeginShowMessageBox("Error", "Your device doesn't support geo-location service.", strings, 0, 0, ExitGame, null);
                    }
                    break;

                case GeoPositionStatus.Ready:
                    CurrentGeoCoord = geoWatcher.Position;
                    break;
            }                       
        }

        void geoWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (currentState == GeoPositionStatus.Ready)
                CurrentGeoCoord = e.Position;
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            // When our app gains focus, load the saved geo location

            GeoPosition<GeoCoordinate> temp;
            LoadFromIsoStore(strDataFile, out temp);
            SavedGeoCoord = temp;
            
            base.OnActivated(sender, args);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            // When our app loses focus, save our "saved geo location"
            
            SaveToIsoStore(strDataFile, SavedGeoCoord);
            base.OnDeactivated(sender, args);
        }

        /// <summary>
        /// Write geo location to isolated storage in binary format using the specified filename 
        /// </summary>        
        private void SaveToIsoStore(string filename, GeoPosition<GeoCoordinate> posToSave )
        {
            if (posToSave == null)
                return;

            string strBaseDir = string.Empty;
            string delimStr = "/";
            char[] delimiter = delimStr.ToCharArray();
            string[] dirsPath = filename.Split(delimiter);

            // Recreate the directory structure
            for (int i = 0; i < dirsPath.Length - 1; i++)
            {
                strBaseDir = System.IO.Path.Combine(strBaseDir, dirsPath[i]);
                isoStore.CreateDirectory(strBaseDir);
            }

            // Remove existing file
            if (isoStore.FileExists(filename))
            {
                isoStore.DeleteFile(filename);
            }

            // Write the file
            using (BinaryWriter bw = new BinaryWriter(isoStore.CreateFile(filename)))
            {
                bw.Write(posToSave.Timestamp.Ticks);
                bw.Write(posToSave.Timestamp.Offset.Ticks);
                bw.Write(posToSave.Location.Latitude);
                bw.Write(posToSave.Location.Longitude);
                bw.Write(posToSave.Location.Altitude);
                bw.Write(posToSave.Location.HorizontalAccuracy);
                bw.Write(posToSave.Location.VerticalAccuracy);
                bw.Write(posToSave.Location.Speed);
                bw.Write(posToSave.Location.Course);
                bw.Close();
            }
        }        

        /// <summary>
        /// Load geo location from isolated storage
        /// </summary>        
        private void LoadFromIsoStore(string filename, out GeoPosition<GeoCoordinate> posToLoad)
        {
            posToLoad = null;

            if (isoStore.FileExists(filename))
            {
                using (BinaryReader br = new BinaryReader(isoStore.OpenFile(filename, FileMode.Open)))
                {
                    // Reconstruct the geo-location class using data read from the data file in isostore
                    
                    if (br.BaseStream.Length == 0)
                        return;

                    long timestampTicks = br.ReadInt64();
                    long timestampOffsetTicks = br.ReadInt64();

                    TimeSpan ts = new TimeSpan(timestampOffsetTicks);
                    DateTimeOffset dto = new DateTimeOffset(timestampTicks, ts);

                    double latitude = br.ReadDouble();
                    double longitude = br.ReadDouble();
                    double altitude = br.ReadDouble();
                    double horizontalAccuracy = br.ReadDouble();
                    double verticalAccuracy = br.ReadDouble();
                    double speed = br.ReadDouble();
                    double course = br.ReadDouble();
                    GeoCoordinate gc = new GeoCoordinate(latitude, longitude, altitude, horizontalAccuracy, verticalAccuracy, speed, course);

                    posToLoad = new GeoPosition<GeoCoordinate>(dto, gc);
                }
            }
        }        

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Font");            

            // TODO: use this.Content to load your game content here            
            texCircle = Content.Load<Texture2D>("circle");
            texArrow = Content.Load<Texture2D>("arrow");
            texSpot = Content.Load<Texture2D>("spot");

            textCurrentGeoLocation = new TextBox(spriteFont)
            {                
                Position = new Vector2(0, 40),
                IsVisible = true
            };
            uiElementList.Add(textCurrentGeoLocation);

            textSavedGeoLocation = new TextBox(spriteFont)
            {                
                Position = new Vector2(0, 270),
                IsVisible = true
            };
            uiElementList.Add(textSavedGeoLocation);

            buttonSaveLocation = new Button(GraphicsDevice, spriteFont, "Save location")
            {
                Position = new Vector2(0, graphics.PreferredBackBufferHeight - 31),
                Size = new Vector2(154, 30),
                IsVisible = true
            };
            buttonSaveLocation.OnClick += new Button.ClickEventHandler(buttonSaveLocation_OnClick);

            uiElementList.Add(buttonSaveLocation);

            buttonToggleNorthUp = new Button(GraphicsDevice, spriteFont, "Toggle: North up")
            {
                Position = new Vector2(graphics.PreferredBackBufferWidth - 175, graphics.PreferredBackBufferHeight - 31),
                Size = new Vector2(174, 30),
                IsVisible = true
            };
            buttonToggleNorthUp.OnClick += new Button.ClickEventHandler(buttonToggleNorthUp_OnClick);

            uiElementList.Add(buttonToggleNorthUp);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

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
            
            TouchCollection touchCollection = TouchPanel.GetState();
            for (int t = 0; t < touchCollection.Count; t++)
            {
                for (int b = 0; b < uiElementList.Count; b++)
                {
                    uiElementList[b].HandleTouch(touchCollection[t]);
                }
            }

            if (SavedGeoCoord != null && CurrentGeoCoord != null)
            {
                distance = CurrentGeoCoord.Location.GetDistanceTo(SavedGeoCoord.Location);
                bearing = InitialBearing(CurrentGeoCoord.Location, SavedGeoCoord.Location);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
                        
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            spriteBatch.DrawString(spriteFont, currentState.ToString(), new Vector2(0, 0), Color.White);
            if (SavedGeoCoord != null && CurrentGeoCoord != null)
            {
                spriteBatch.DrawString(spriteFont, 
                                       string.Format("Distance: {0:0.00}meters, {1:0.00}feet\n" +
                                                     "Speed:   {2:0.00}meters/s, {3:0.00}km/h, {4:0.00}mph", 
                                                     distance, 
                                                     distance*3.2808399, 
                                                     CurrentGeoCoord.Location.Speed, 
                                                     CurrentGeoCoord.Location.Speed * 3.6, 
                                                     CurrentGeoCoord.Location.Speed * 3.6 * 0.621371192), 
                                       new Vector2(0, 480), Color.White);

                if (buttonToggleNorthUp.Text == "Toggle: North up")
                {
                    // North up mode
                    
                    spriteBatch.Draw(texCircle,
                                     new Vector2((graphics.PreferredBackBufferWidth - texCircle.Width) / 2, graphics.PreferredBackBufferHeight - texCircle.Height - 10),
                                     Color.White);
                    spriteBatch.Draw(texArrow, new Rectangle((graphics.PreferredBackBufferWidth) / 2, graphics.PreferredBackBufferHeight - texCircle.Height / 2 - 10, texCircle.Width, texCircle.Height),
                                     null, Color.White, 
                                     (float)DegreeToRadian(CurrentGeoCoord.Location.Course), // Your current movement direction
                                     new Vector2(128, 128), SpriteEffects.None, 0);
                    spriteBatch.Draw(texSpot, new Rectangle((graphics.PreferredBackBufferWidth) / 2, graphics.PreferredBackBufferHeight - texCircle.Height / 2 - 10, texCircle.Width, texCircle.Height),
                                     null, Color.White, 
                                     (float)DegreeToRadian(bearing), // In what direction your saved location is related to your current location
                                     new Vector2(128, 128), SpriteEffects.None, 0);
                }
                else
                {
                    // Your direction up mode

                    // Every thing is the same as above except the all angles are being shifted by (-curGeoCoord.Location.Course),
                    // so that your movement direction is always pointing up
                    spriteBatch.Draw(texCircle, new Rectangle((graphics.PreferredBackBufferWidth) / 2, graphics.PreferredBackBufferHeight - texCircle.Height / 2 - 10, texCircle.Width, texCircle.Height),
                                     null, Color.White, 
                                     -(float)DegreeToRadian(CurrentGeoCoord.Location.Course), new Vector2(128, 128), SpriteEffects.None, 0);
                    spriteBatch.Draw(texArrow, new Rectangle((graphics.PreferredBackBufferWidth) / 2, graphics.PreferredBackBufferHeight - texCircle.Height / 2 - 10, texCircle.Width, texCircle.Height),
                                     null, Color.White, 
                                     0, 
                                     new Vector2(128, 128), SpriteEffects.None, 0);
                    spriteBatch.Draw(texSpot, new Rectangle((graphics.PreferredBackBufferWidth) / 2, graphics.PreferredBackBufferHeight - texCircle.Height / 2 - 10, texCircle.Width, texCircle.Height),
                                     null, Color.White, 
                                     (float)DegreeToRadian(bearing)-(float)DegreeToRadian(CurrentGeoCoord.Location.Course), 
                                     new Vector2(128, 128), SpriteEffects.None, 0);
                }
            }
            spriteBatch.End();

            // Some ui elements combine 3D geometry with text so they can't render within a SpriteBatch.Begin/End section.
            for (int b = 0; b < uiElementList.Count; b++)
            {
                uiElementList[b].Draw(spriteBatch);
            }

            base.Draw(gameTime);
        }
    }
}
