//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Phone.Notification;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PushNotificationClient
{
    /// <summary>
    /// PushNotificationClient is a simple XNA Framework application that opens an HttpNotificationChannel
    /// and waits to receive notifications.  Status messages are printed to an onscreen console as well
    /// as the debug output.  In order to copy the notification channel URI to the companion sender app,
    /// this program should be run under a debugger so the output can be seen.
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont font;

        DebugConsole console;

        private HttpNotificationChannel httpChannel;
        const string channelName = "ExampleXNAPushChannel";
        const string serviceName = "ExampleXNAPushService";

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Pre-autoscale settings.
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;

            graphics.IsFullScreen = true;
        }


        /// <summary>
        /// Initialize the game and create the notification channel.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Create or open a notification channel.
            CreateNotificationChannel();
        }


        /// <summary>
        /// Load UI resources and create the output console.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");

            console = new DebugConsole(font, graphics.GraphicsDevice.Viewport.Bounds);
        }

      
        /// <summary>
        /// Check for player input (in this case, the back button is the only input that
        /// matters).
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit.
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            console.Draw(spriteBatch);

            base.Draw(gameTime);
        }

        
        /// <summary>
        /// Creates (or re-opens) the push channel, and subscribes to push notification events.
        /// </summary>
        private void CreateNotificationChannel()
        {
            //Try to find an existing channel.
            console.AddLine("Looking for notification channel");

            httpChannel = HttpNotificationChannel.Find(channelName);
            
            // If no existing channel is found, create a new one.  The device's URI will be in the 
            // event args of the HttpNotificationChannel.ChannelUriUpdated event.
            if (null == httpChannel)
            {
                console.AddLine("Creating new channel");

                httpChannel = new HttpNotificationChannel(channelName, serviceName);
                httpChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(httpChannel_ChannelUriUpdated);
                httpChannel.Open();
            }
            else
            {
                console.AddLine("Got existing channel Uri:\n" + httpChannel.ChannelUri.ToString());
            }

            console.AddLine("Subscribing to channel events.");

            // Add an event handler for raw push messages. 
            httpChannel.HttpNotificationReceived += new EventHandler<HttpNotificationEventArgs>(httpChannel_HttpNotificationReceived);

            // Add an event handler for toast push messages. 
            httpChannel.ShellToastNotificationReceived += new EventHandler<NotificationEventArgs>(httpChannel_ShellToastNotificationReceived);

            // Tile notifications cannot be handled in-game.

            // Add an event handler for channel errors.
            httpChannel.ErrorOccurred += new EventHandler<NotificationChannelErrorEventArgs>(httpChannel_ErrorOccurred);
        }
        

        /// <summary>
        /// The callback for a channel update.  The event args contain the URI that points to the device.
        /// </summary>
        private void httpChannel_ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            // This URI is used to send notifications to the device and would need to be
            // sent to a game server  or other web service any time the URI gets updated.
            console.AddLine("Channel updated. Got Uri:\n" + httpChannel.ChannelUri.ToString());

            // Bind to the shell so the phone knows the app wants to receive notifications.
            console.AddLine("Binding to shell.");
            httpChannel.BindToShellToast();
            httpChannel.BindToShellTile();
        }


        /// <summary>
        /// The callback for a channel error.
        /// </summary>
        private void httpChannel_ErrorOccurred(object sender, NotificationChannelErrorEventArgs e)
        {
            console.AddLine("Error occurred: " + e.Message);

            console.AddLine("Trying to reopen channel.");
            httpChannel.Close();
            httpChannel.Open();
        }


        /// <summary>
        /// The callback for receiving a raw notification.
        /// </summary>
        private void httpChannel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            console.AddLine("Got raw notification:");

            // The client and server must agree on the format of this notification: it's just bytes
            // as far as the phone is concerned.  If the game is not running, this notification will
            // be dropped.  In this case, the payload is a string that was serialized with BinaryWriter.

            BinaryReader reader = new BinaryReader(e.Notification.Body, System.Text.Encoding.UTF8);
            
            string notificationText = reader.ReadString();
            
            console.AddLine(notificationText);
        }


        /// <summary>
        /// The callback for receiving a toast notification.
        /// </summary>
        private void httpChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            console.AddLine("Got toast notification:");
            
            if (e.Collection != null)
            {
                Dictionary<string, string> collection = (Dictionary<string, string>)e.Collection;

                foreach (string elementName in collection.Keys)
                {
                    console.AddLine(elementName + " : " + collection[elementName]);
                }
            }            
        }
    }
}