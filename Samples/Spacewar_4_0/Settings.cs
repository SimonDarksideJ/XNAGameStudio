#region File Description
//-----------------------------------------------------------------------------
// Settings.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Xml.Serialization;
#endregion

namespace Spacewar
{
    /// <summary>
    /// The Setting class handles loading and saving of global application settings.
    /// The normal .Net classes (System.Configuration) for doing this are not available on the CF (and therefore 360)
    /// </summary>
    public class Settings
    {
        #region General App Settings

        /// <summary>
        /// The path to look for all media in
        /// </summary>
        public string MediaPath = @"";

        /// <summary>
        /// The name of the window when running in windowed mode
        /// </summary>
        public string WindowTitle = "Spacewar";

        /// <summary>
        /// The length of each level in seconds;
        /// </summary>
        public int LevelTime = 30;

        /// <summary>
        /// How much maximum thrust to apply
        /// </summary>
        public float ThrustPower = 100f;

        /// <summary>
        /// How much friction to apply to slow down the ship
        /// </summary>
        public float FrictionFactor = .1f;

        /// <summary>
        /// Maximum speed a ship will accelerate to
        /// </summary>
        public float MaxSpeed = 200f;

        /// <summary>
        /// Time ships spend in recovery after destruction
        /// </summary>
        public float ShipRecoveryTime = 1.6f;

        #endregion

        #region Sun settings

        /// <summary>
        /// The position of the sun
        /// </summary>
        public Vector2 SunPosition = new Vector2(0f, 0f);
        /// <summary>
        /// How fast items are pulled towards the sun
        /// </summary>
        public double GravityStrength = 500000.0;
        /// <summary>
        /// Power defines the fall off of the suns gravity. 2.0 is 1/(n^2) as in normal gravity.
        /// Bigger numbers fall off faster.
        /// </summary>
        public int GravityPower = 2;

        /// <summary>
        /// Affect the color of the sun shader
        /// </summary>
        public float ColorDistribution = 3.0f;

        /// <summary>
        /// Affects the fade of the sun shader
        /// </summary>
        public float Fade = 4.0f;

        /// <summary>
        /// Affects the flame speed of the sun shader
        /// </summary>
        public float FlameSpeed = 0.22f;

        /// <summary>
        /// Affects the spread of flames of the sun shder
        /// </summary>
        public float Spread = 0.50f;

        /// <summary>
        /// Affects the flames of the sun shader
        /// </summary>
        public float Flamability = 1.74f;

        /// <summary>
        /// Size of the sun
        /// </summary>
        public float Size = 70f;

        #endregion

        #region AsteroidSettings

        /// <summary>
        /// Realtive Scale of Asteroids
        /// </summary>
        public float AsteroidScale = .02f;

        #endregion

        #region BulletSettings
        /// <summary>
        /// RelativeScale of bullets
        /// </summary>
        public float BulletScale = .02f;

        #endregion

        #region Ship settings

        /// <summary>
        /// Relative scaling of the ships
        /// </summary>
        public float ShipScale = 0.02f;

        /// <summary>
        /// Stores settings for the player ships
        /// </summary>
        public struct PlayerShipInfo
        {
            /// <summary>
            /// The start position of this ship
            /// </summary>
            public Vector2 StartPosition;
            /// <summary>
            /// The start angle of this ship
            /// </summary>
            public double StartAngle;

            /// <summary>
            /// Makes a new ShipInfo
            /// </summary>
            /// <param name="startPosition">Start position</param>
            /// <param name="startAngle">Start Angle</param>
            public PlayerShipInfo(Vector2 startPosition, double startAngle)
            {
                StartPosition = startPosition;
                StartAngle = startAngle;
            }

        }

        /// <summary>
        /// Store default information about the ships
        /// </summary>
        public PlayerShipInfo[] Ships = new PlayerShipInfo[2] {new PlayerShipInfo(new Vector2(-300, 0), 90), 
                                                   new PlayerShipInfo(new Vector2(300, 0), 90)};

        #endregion

        #region WeaponParameters
        /// <summary>
        /// Stores information about weapons
        /// </summary>
        public struct WeaponInfo
        {
            /// <summary>
            /// Cost of the weapon
            /// </summary>
            public int Cost;

            /// <summary>
            /// Nubmer of seconds the projectile lasts for
            /// </summary>
            public double Lifetime;

            /// <summary>
            /// Maximum number of ths projectile that can be shot at a time
            /// </summary>
            public int Max;

            /// <summary>
            /// How many projectile fired per trigger pull
            /// </summary>
            public int Burst;

            /// <summary>
            /// Acceleration of the projectile
            /// </summary>
            public float Acceleration;

            /// <summary>
            /// How much damage this bullet does
            /// </summary>
            public int Damage;

            /// <summary>
            /// Creates a new weapon
            /// </summary>
            /// <param name="cost">Cost of the weapon</param>
            public WeaponInfo(int cost, double lifetime, int max, int burst, float acceleration, int damage)
            {
                Cost = cost;
                Lifetime = lifetime;
                Max = max;
                Burst = burst;
                Acceleration = acceleration;
                Damage = damage;
            }
        }

        /// <summary>
        /// Stores default information about the weapons
        /// </summary>
        public WeaponInfo[] Weapons = new WeaponInfo[] 
        {
            new WeaponInfo(0, 3.0, 5, 1, 0, 1), //Pea
            new WeaponInfo(1000, 3.0, 4, 3, 0, 1), //mgun
            new WeaponInfo(2000, 3.0, 3, 3, 0, 1), //double mgun
            new WeaponInfo(3000, 2.0, 1, 1, 1.0f, 5), //Rocket
            new WeaponInfo(4000, 2.0, 3, 1, 0f, 5), //BFG
        };

        #endregion

        #region Backdrop
        /// <summary>
        /// How fast the 2 backdrops fade between each other
        /// </summary>
        public float CrossFadeSpeed = 0.2f;

        /// <summary>
        /// How much the 2 backdrops move 
        /// </summary>
        public float OffsetSpeed = 0.1f;

        #endregion

        #region Lighting
        /// <summary>
        /// Store informtion about lighting the scenes
        /// </summary>
        public struct ShipLighting
        {
            /// <summary>
            /// Ambient component color
            /// </summary>
            public Vector4 Ambient;
            /// <summary>
            /// The direction of the directional light
            /// </summary>
            public Vector4 DirectionalDirection;
            /// <summary>
            /// The color of the directional light
            /// </summary>
            public Vector4 DirectionalColor;
            /// <summary>
            /// The position of the point light
            /// </summary>
            public Vector4 PointPosition;
            /// <summary>
            /// The color of the point light
            /// </summary>
            public Vector4 PointColor;
            /// <summary>
            /// The fall off of the point light
            /// </summary>
            public float PointFactor;

            /// <summary>
            /// Creates a new lighting scheme
            /// </summary>
            /// <param name="ambient">Ambient color</param>
            /// <param name="directionalDirection">Directional light direction</param>
            /// <param name="directionalColor">Directional light color</param>
            /// <param name="pointPosition">Point light position</param>
            /// <param name="pointColor">Point light color</param>
            /// <param name="pointFactor">Point light fall off</param>
            public ShipLighting(Vector4 ambient, Vector4 directionalDirection, Vector4 directionalColor, Vector4 pointPosition, Vector4 pointColor, float pointFactor)
            {
                Ambient = ambient;
                DirectionalDirection = directionalDirection;
                DirectionalColor = directionalColor;
                PointPosition = pointPosition;
                PointColor = pointColor;
                PointFactor = pointFactor;
            }
        }

        /// <summary>
        /// Lighting parameters for in game and menu shaders
        /// </summary>
        public ShipLighting[] ShipLights = new ShipLighting[]
        {   //0 is in game
            new ShipLighting(new Vector4(1f, 1f, 1f, 1.0f),
                            new Vector4(1f, 1f, 1f, 0f),
                            new Vector4(.4f, .4f, .8f, 1.0f),
                            new Vector4(0f, 0f, 0f, 0f),
                            new Vector4(.8f, .6f, 0f, 1.0f),
                            .01f),
            //1 is menu screens
            new ShipLighting(new Vector4(.2f, .2f, .2f, 1.0f),
                            new Vector4(1f, 1f, 1f, 0f),
                            new Vector4(.4f, .4f, .8f, 1.0f),
                            new Vector4(0f, 0f, 0f, 0f),
                            new Vector4(.8f, .6f, 0f, 1.0f),
                            .008f),
        };
        #endregion

        #region Keyboard Settings
        /// <summary>
        /// Keyboard settings for two players
        /// Note: not allowing extensibility for more than 2 players
        /// </summary>

        // player 1
        public Keys Player1Start = Keys.LeftControl;
        public Keys Player1Back = Keys.LeftShift;
        public Keys Player1A = Keys.V;
        public Keys Player1B = Keys.G;
        public Keys Player1X = Keys.F;
        public Keys Player1Y = Keys.T;
        public Keys Player1ThumbstickLeftXmin = Keys.A;
        public Keys Player1ThumbstickLeftXmax = Keys.D;
        public Keys Player1ThumbstickLeftYmin = Keys.S;
        public Keys Player1ThumbstickLeftYmax = Keys.W;
        public Keys Player1Left = Keys.A;
        public Keys Player1Right = Keys.D;
        public Keys Player1Down = Keys.S;
        public Keys Player1Up = Keys.W;
        public Keys Player1LeftTrigger = Keys.Q;
        public Keys Player1RightTrigger = Keys.E;

        // player 2
        public Keys Player2Start = Keys.RightControl;
        public Keys Player2Back = Keys.RightShift;
        public Keys Player2A = Keys.Home;
        public Keys Player2B = Keys.End;
        public Keys Player2X = Keys.PageUp;
        public Keys Player2Y = Keys.PageDown;
        public Keys Player2ThumbstickLeftXmin = Keys.Left;
        public Keys Player2ThumbstickLeftXmax = Keys.Right;
        public Keys Player2ThumbstickLeftYmin = Keys.Down;
        public Keys Player2ThumbstickLeftYmax = Keys.Up;
        public Keys Player2Left = Keys.Left;
        public Keys Player2Right = Keys.Right;
        public Keys Player2Down = Keys.Down;
        public Keys Player2Up = Keys.Up;
        public Keys Player2LeftTrigger = Keys.Insert;
        public Keys Player2RightTrigger = Keys.Delete;
        #endregion

        #region Load/Save code
        /// <summary>
        /// Saves the current settings
        /// </summary>
        /// <param name="filename">The filename to save to</param>
        public void Save(string filename)
        {
            Stream stream = File.Create(filename);

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            serializer.Serialize(stream, this);
            stream.Close();
        }

        /// <summary>
        /// Loads settings from a file
        /// </summary>
        /// <param name="filename">The filename to load</param>
        public static Settings Load(string filename)
        {
            Stream stream = File.OpenRead(filename);
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            return (Settings)serializer.Deserialize(stream);
        }
        #endregion
    }
}
