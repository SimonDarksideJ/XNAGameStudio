#region File Description
//-----------------------------------------------------------------------------
// ClientServerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
#endregion

namespace ClientServer
{
    /// <summary>
    /// Sample showing how to implement a simple multiplayer
    /// network session, using a client/server network topology.
    /// </summary>
    public class ClientServerGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        const int screenWidth = 1067;
        const int screenHeight = 600;

        const int maxGamers = 16;
        const int maxLocalGamers = 4;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        KeyboardState currentKeyboardState;
        GamePadState currentGamePadState;

        NetworkSession networkSession;

        PacketWriter packetWriter = new PacketWriter();
        PacketReader packetReader = new PacketReader();
        
        string errorMessage;

        #endregion

        #region Initialization


        public ClientServerGame()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;

            Content.RootDirectory = "Content";

            Components.Add(new GamerServicesComponent(this));
        }


        /// <summary>
        /// Load your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            font = Content.Load<SpriteFont>("Font");
        }


        #endregion

        #region Update


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            HandleInput();

            if (networkSession == null)
            {
                // If we are not in a network session, update the
                // menu screen that will let us create or join one.
                UpdateMenuScreen();
            }
            else
            {
                // If we are in a network session, update it.
                UpdateNetworkSession();
            }

            base.Update(gameTime);
        }


        /// <summary>
        /// Menu screen provides options to create or join network sessions.
        /// </summary>
        void UpdateMenuScreen()
        {
            if (IsActive)
            {
                if (Gamer.SignedInGamers.Count == 0)
                {
                    // If there are no profiles signed in, we cannot proceed.
                    // Show the Guide so the user can sign in.
                    Guide.ShowSignIn(maxLocalGamers, false);
                }
                else if (IsPressed(Keys.A, Buttons.A))
                {
                    // Create a new session?
                    CreateSession();
                }
                else if (IsPressed(Keys.B, Buttons.B))
                {
                    // Join an existing session?
                    JoinSession();
                }
            }
        }


        /// <summary>
        /// Starts hosting a new network session.
        /// </summary>
        void CreateSession()
        {
            DrawMessage("Creating session...");

            try
            {
                networkSession = NetworkSession.Create(NetworkSessionType.SystemLink,
                                                       maxLocalGamers, maxGamers);

                HookSessionEvents();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }


        /// <summary>
        /// Joins an existing network session.
        /// </summary>
        void JoinSession()
        {
            DrawMessage("Joining session...");

            try
            {
                // Search for sessions.
                using (AvailableNetworkSessionCollection availableSessions =
                            NetworkSession.Find(NetworkSessionType.SystemLink,
                                                maxLocalGamers, null))
                {
                    if (availableSessions.Count == 0)
                    {
                        errorMessage = "No network sessions found.";
                        return;
                    }

                    // Join the first session we found.
                    networkSession = NetworkSession.Join(availableSessions[0]);

                    HookSessionEvents();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }


        /// <summary>
        /// After creating or joining a network session, we must subscribe to
        /// some events so we will be notified when the session changes state.
        /// </summary>
        void HookSessionEvents()
        {
            networkSession.GamerJoined += GamerJoinedEventHandler;
            networkSession.SessionEnded += SessionEndedEventHandler;
        }


        /// <summary>
        /// This event handler will be called whenever a new gamer joins the session.
        /// We use it to allocate a Tank object, and associate it with the new gamer.
        /// </summary>
        void GamerJoinedEventHandler(object sender, GamerJoinedEventArgs e)
        {
            int gamerIndex = networkSession.AllGamers.IndexOf(e.Gamer);

            e.Gamer.Tag = new Tank(gamerIndex, Content, screenWidth, screenHeight);
        }


        /// <summary>
        /// Event handler notifies us when the network session has ended.
        /// </summary>
        void SessionEndedEventHandler(object sender, NetworkSessionEndedEventArgs e)
        {
            errorMessage = e.EndReason.ToString();

            networkSession.Dispose();
            networkSession = null;
        }


        /// <summary>
        /// Updates the state of the network session, moving the tanks
        /// around and synchronizing their state over the network.
        /// </summary>
        void UpdateNetworkSession()
        {
            // Read inputs for locally controlled tanks, and send them to the server.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                UpdateLocalGamer(gamer);
            }

            // If we are the server, update all the tanks and transmit
            // their latest positions back out over the network.
            if (networkSession.IsHost)
            {
                UpdateServer();
            }

            // Pump the underlying session object.
            networkSession.Update();

            // Make sure the session has not ended.
            if (networkSession == null)
                return;

            // Read any incoming network packets.
            foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
            {
                if (gamer.IsHost)
                {
                    ServerReadInputFromClients(gamer);
                }
                else
                {
                    ClientReadGameStateFromServer(gamer);
                }
            }
        }


        /// <summary>
        /// Helper for updating a locally controlled gamer.
        /// </summary>
        void UpdateLocalGamer(LocalNetworkGamer gamer)
        {
            // Look up what tank is associated with this local player,
            // and read the latest user inputs for it. The server will
            // later use these values to control the tank movement.
            Tank localTank = gamer.Tag as Tank;

            ReadTankInputs(localTank, gamer.SignedInGamer.PlayerIndex);

            // Only send if we are not the server. There is no point sending packets
            // to ourselves, because we already know what they will contain!
            if (!networkSession.IsHost)
            {
                // Write our latest input state into a network packet.
                packetWriter.Write(localTank.TankInput);
                packetWriter.Write(localTank.TurretInput);

                // Send our input data to the server.
                gamer.SendData(packetWriter,
                               SendDataOptions.InOrder, networkSession.Host);
            }
        }


        /// <summary>
        /// This method only runs on the server. It calls Update on all the
        /// tank instances, both local and remote, using inputs that have
        /// been received over the network. It then sends the resulting
        /// tank position data to everyone in the session.
        /// </summary>
        void UpdateServer()
        {
            // Loop over all the players in the session, not just the local ones!
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                // Look up what tank is associated with this player.
                Tank tank = gamer.Tag as Tank;

                // Update the tank.
                tank.Update();

                // Write the tank state into the output network packet.
                packetWriter.Write(gamer.Id);
                packetWriter.Write(tank.Position);
                packetWriter.Write(tank.TankRotation);
                packetWriter.Write(tank.TurretRotation);
            }

            // Send the combined data for all tanks to everyone in the session.
            LocalNetworkGamer server = (LocalNetworkGamer)networkSession.Host;

            server.SendData(packetWriter, SendDataOptions.InOrder);
        }


        /// <summary>
        /// This method only runs on the server. It reads tank inputs that
        /// have been sent over the network by a client machine, storing
        /// them for later use by the UpdateServer method.
        /// </summary>
        void ServerReadInputFromClients(LocalNetworkGamer gamer)
        {
            // Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                gamer.ReceiveData(packetReader, out sender);

                if (!sender.IsLocal)
                {
                    // Look up the tank associated with whoever sent this packet.
                    Tank remoteTank = sender.Tag as Tank;

                    // Read the latest inputs controlling this tank.
                    remoteTank.TankInput = packetReader.ReadVector2();
                    remoteTank.TurretInput = packetReader.ReadVector2();
                }
            }
        }


        /// <summary>
        /// This method only runs on client machines. It reads
        /// tank position data that has been computed by the server.
        /// </summary>
        void ClientReadGameStateFromServer(LocalNetworkGamer gamer)
        {
            // Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // Read a single packet from the network.
                gamer.ReceiveData(packetReader, out sender);

                // This packet contains data about all the players in the session.
                // We keep reading from it until we have processed all the data.
                while (packetReader.Position < packetReader.Length)
                {
                    // Read the state of one tank from the network packet.
                    byte gamerId = packetReader.ReadByte();
                    Vector2 position = packetReader.ReadVector2();
                    float tankRotation = packetReader.ReadSingle();
                    float turretRotation = packetReader.ReadSingle();

                    // Look up which gamer this state refers to.
                    NetworkGamer remoteGamer = networkSession.FindGamerById(gamerId);

                    // This might come back null if the gamer left the session after
                    // the host sent the packet but before we received it. If that
                    // happens, we just ignore the data for this gamer.
                    if (remoteGamer != null)
                    {
                        // Update our local state with data from the network packet.
                        Tank tank = remoteGamer.Tag as Tank;

                        tank.Position = position;
                        tank.TankRotation = tankRotation;
                        tank.TurretRotation = turretRotation;
                    }
                }
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (networkSession == null)
            {
                // If we are not in a network session, draw the
                // menu screen that will let us create or join one.
                DrawMenuScreen();
            }
            else
            {
                // If we are in a network session, draw it.
                DrawNetworkSession();
            }

            base.Draw(gameTime);
        }


        /// <summary>
        /// Draws the startup screen used to create and join network sessions.
        /// </summary>
        void DrawMenuScreen()
        {
            string message = string.Empty;

            if (!string.IsNullOrEmpty(errorMessage))
                message += "Error:\n" + errorMessage.Replace(". ", ".\n") + "\n\n";

            message += "A = create session\n" +
                       "B = join session";

            spriteBatch.Begin();

            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);
            
            spriteBatch.End();
        }


        /// <summary>
        /// Draws the state of an active network session.
        /// </summary>
        void DrawNetworkSession()
        {
            spriteBatch.Begin();

            // For each person in the session...
            foreach (NetworkGamer gamer in networkSession.AllGamers)
            {
                // Look up the tank object belonging to this network gamer.
                Tank tank = gamer.Tag as Tank;

                // Draw the tank.
                tank.Draw(spriteBatch);

                // Draw a gamertag label.
                string label = gamer.Gamertag;
                Color labelColor = Color.Black;
                Vector2 labelOffset = new Vector2(100, 150);

                if (gamer.IsHost)
                    label += " (server)";

                // Flash the gamertag to yellow when the player is talking.
                if (gamer.IsTalking)
                    labelColor = Color.Yellow;

                spriteBatch.DrawString(font, label, tank.Position, labelColor, 0,
                                       labelOffset, 0.6f, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }


        /// <summary>
        /// Helper draws notification messages before calling blocking network methods.
        /// </summary>
        void DrawMessage(string message)
        {
            if (!BeginDraw())
                return;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.DrawString(font, message, new Vector2(161, 161), Color.Black);
            spriteBatch.DrawString(font, message, new Vector2(160, 160), Color.White);

            spriteBatch.End();

            EndDraw();
        }

        
        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input.
        /// </summary>
        private void HandleInput()
        {
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (IsActive && IsPressed(Keys.Escape, Buttons.Back))
            {
                Exit();
            }
        }


        /// <summary>
        /// Checks if the specified button is pressed on either keyboard or gamepad.
        /// </summary>
        bool IsPressed(Keys key, Buttons button)
        {
            return (currentKeyboardState.IsKeyDown(key) ||
                    currentGamePadState.IsButtonDown(button));
        }


        /// <summary>
        /// Reads input data from keyboard and gamepad, and stores
        /// it into the specified tank object.
        /// </summary>
        void ReadTankInputs(Tank tank, PlayerIndex playerIndex)
        {
            // Read the gamepad.
            GamePadState gamePad = GamePad.GetState(playerIndex);

            Vector2 tankInput = gamePad.ThumbSticks.Left;
            Vector2 turretInput = gamePad.ThumbSticks.Right;

            // Read the keyboard.
            KeyboardState keyboard = Keyboard.GetState(playerIndex);

            if (keyboard.IsKeyDown(Keys.Left))
                tankInput.X = -1;
            else if (keyboard.IsKeyDown(Keys.Right))
                tankInput.X = 1;

            if (keyboard.IsKeyDown(Keys.Up))
                tankInput.Y = 1;
            else if (keyboard.IsKeyDown(Keys.Down))
                tankInput.Y = -1;

            if (keyboard.IsKeyDown(Keys.A))
                turretInput.X = -1;
            else if (keyboard.IsKeyDown(Keys.D))
                turretInput.X = 1;

            if (keyboard.IsKeyDown(Keys.W))
                turretInput.Y = 1;
            else if (keyboard.IsKeyDown(Keys.S))
                turretInput.Y = -1;

            // Normalize the input vectors.
            if (tankInput.Length() > 1)
                tankInput.Normalize();

            if (turretInput.Length() > 1)
                turretInput.Normalize();

            // Store these input values into the tank object.
            tank.TankInput = tankInput;
            tank.TurretInput = turretInput;
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (ClientServerGame game = new ClientServerGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
