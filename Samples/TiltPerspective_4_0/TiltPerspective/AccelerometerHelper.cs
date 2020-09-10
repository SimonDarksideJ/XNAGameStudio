//-----------------------------------------------------------------------------
// Accelerometer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Devices.Sensors;


namespace TiltPerspectiveSample
{
    /// <summary>
    /// This interface describes a simple API for reading the accelerometer, which is needed
    /// for the XNA Game Studio IServiceProvider API.
    /// </summary>
    public interface IAccelerometerService
    {
        Vector3 RawAcceleration { get; }
        Vector3 SmoothAcceleration { get; }
    }

    /// <remarks>
    /// Wrap up the default accelerometer device as a game component. Current raw accelerometer readings can be
    /// obtained from accelerometer.AccelVector, and smoothed readings can be obtained from
    /// accelerometer.SmoothAccelVector.
    ///
    /// If the default accelerometer is not available (for example, when running on the emulator), fake readings
    /// will be synthesized so you can still see some motion.
    ///
    /// To use this component, the game should create an Accelerometer and add it to game.Components. It will
    /// automatically add itself to game.Services as well. Other game components can then locate it using
    /// game.Services.GetService(typeof(IAccelerometerService)).
    /// </remarks>
    public class AccelerometerHelper : GameComponent, IAccelerometerService
    {
        private Accelerometer Sensor;
        private DateTimeOffset LastSensorTime;

        /// <summary>
        /// Current raw (unsmoothed) accelerometer reading
        /// </summary>
        public Vector3 RawAcceleration { get; private set; }

        /// <summary>
        /// Current smoothed accelerometer reading
        /// </summary>
        public Vector3 SmoothAcceleration { get; private set; }

        /// <summary>
        /// Smoothing rate; larger numbers cause more smoothing. The default
        /// should be fine for most applications.
        /// </summary>
        public float Smoothing = .1f;

        /// <summary>
        /// Empirically determined error in accelerometer readings. It seems
        /// to be off by a constant offset.
        /// </summary>
        Vector3 SensorError = new Vector3(-0.09f, -0.02f, 0.04f);

        // If a real sensor is not available, we can fake up some inputs.
        // We will return a reading tilted FakeRollPhi away from horizontal,
        // in a direction given by FakeRollTheta, which wobbles at
        // FakeRollSpeed radians per second.
        private float FakeRollPhi = MathHelper.Pi / 8;
        private float FakeRollTheta = 0;
        private float FakeRollSpeed = 1.0f; // 1 radian/second, or one circle every ~6 seconds

        public AccelerometerHelper(Game game)
            : base(game)
        {
            game.Services.AddService(typeof(IAccelerometerService), this);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            RawAcceleration = new Vector3(0, 0, -1);
            SmoothAcceleration = new Vector3(0, 0, -1);
            Sensor = new Accelerometer();
            Sensor.ReadingChanged += OnReadingChanged;

            // try to start the sensor only on devices, catching the exception if it fails            
            if (Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Device)
            {
                try
                {
                    Sensor.Start();
                }
                catch (AccelerometerFailedException)
                {
                    Sensor = null;
                }
            }
            else
            {
                Sensor = null;
            }

            base.Initialize();
            Enabled = (Sensor == null);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (Sensor == null)
            {
                FakeRollTheta += (float)gameTime.ElapsedGameTime.TotalSeconds * FakeRollSpeed;
                FakeRollTheta = MathHelper.WrapAngle(FakeRollTheta);
                RawAcceleration = new Vector3(
                    (float)Math.Sin(FakeRollPhi) * (float)Math.Cos(FakeRollTheta),
                    (float)Math.Sin(FakeRollPhi) * (float)Math.Sin(FakeRollTheta),
                    -(float)Math.Cos(FakeRollPhi));
                SmoothAcceleration = RawAcceleration;
            }

            base.Update(gameTime);
        }

        // Callback when new actual sensor readings are available.
        private void OnReadingChanged(object sender, AccelerometerReadingEventArgs args)
        {
            RawAcceleration = new Vector3((float)args.X, (float)args.Y, (float)args.Z);
            RawAcceleration -= SensorError;

            float dt = (float)args.Timestamp.Subtract(LastSensorTime).TotalSeconds;
            LastSensorTime = args.Timestamp;
            dt = MathHelper.Clamp(dt, 0.0f, 1.0f);

            float p = (float)Math.Exp(-dt / Smoothing);
            SmoothAcceleration = Vector3.Lerp(RawAcceleration, SmoothAcceleration, p);
        }
    }
}
