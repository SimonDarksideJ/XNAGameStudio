#region File Description
//-----------------------------------------------------------------------------
// Accelerometer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Devices;
using Microsoft.Devices.Sensors;
#endregion

namespace Yacht
{
    /// <summary>
    /// A static encapsulation of accelerometer input to provide games with a polling-based
    /// accelerometer system.
    /// </summary>
    public static class Accelerometer
    {
        // The accelerometer sensor on the device
        private static Microsoft.Devices.Sensors.Accelerometer accelerometer
            = new Microsoft.Devices.Sensors.Accelerometer();

        // Raise when the user shake the phone.
        public static event EventHandler ShakeDetected;

        private static bool shaking;
        private static double shakeThreshold = 0.3;
        private static int shakeCount = 0;

        // we want to prevent the Accelerometer from being initialized twice.
        private static bool isInitialized = false;

        // we need an object for locking because the ReadingChanged event is fired
        // on a different thread than our game
        private static object threadLock = new object();

        // we use this to keep the last known value from the accelerometer callback
        private static Vector3 nextValue = new Vector3();

        // whether or not the accelerometer is active
        private static bool isActive = false;

        /// <summary>
        /// Initializes the Accelerometer for the current game. This method can only be called once per game.
        /// </summary>
        public static void Initialize()
        {
            // make sure we don't initialize the Accelerometer twice
            if (isInitialized)
            {
                return;
            }

            // try to start the sensor only on devices, catching the exception if it fails
            if (Microsoft.Devices.Environment.DeviceType == DeviceType.Device)
            {
                try
                {
                    accelerometer.ReadingChanged +=
                        new EventHandler<Microsoft.Devices.Sensors.AccelerometerReadingEventArgs>(
                            sensor_ReadingChanged);
                    accelerometer.Start();
                    isActive = true;
                }
                catch (Microsoft.Devices.Sensors.AccelerometerFailedException)
                {
                    isActive = false;
                }
            }
            else
            {
                // we always return isActive on emulator because we use the arrow
                // keys for simulation which is always available.
                isActive = true;
            }

            // remember that we are initialized
            isInitialized = true;
        }

        private static void sensor_ReadingChanged(object sender,
            Microsoft.Devices.Sensors.AccelerometerReadingEventArgs e)
        {
            // Store the accelerometer value in our variable to be used on the next Update.
            lock (threadLock)
            {
                // Do shake detection if need.
                if (ShakeDetected != null)
                {
                    // Check if the phone was shake 4 times then raise the event.
                    if (!shaking && CheckForShake(nextValue, e, shakeThreshold) && shakeCount >= 4)
                    {
                        shaking = true;
                        shakeCount = 0;
                        ShakeDetected(null, EventArgs.Empty);
                    }
                    else if (CheckForShake(nextValue, e, shakeThreshold))
                    {
                        shakeCount++;
                    }
                    else if (!CheckForShake(nextValue, e, 0.1))
                    {
                        shakeCount = 0;
                        shaking = false;
                    }
                }
                // Read the next value.
                nextValue = new Vector3((float)e.X, (float)e.Y, (float)e.Z);
            }
        }

        /// <summary>
        /// Check if the phone is shaking.
        /// </summary>
        /// <param name="lastValue">The last value that was read from the accelerometer.</param>
        /// <param name="currentValue">The current value that was read from the accelerometer.</param>
        /// <param name="threshold">The threshold value for the difference in one of the axes that constitutes as
        /// the phone being shaken.</param>
        /// <returns>True if the phone has been shaken and false otherwise.</returns>
        private static bool CheckForShake(Vector3 lastValue,
            AccelerometerReadingEventArgs currentValue, double threshold)
        {
            // Calculate the difference between the last two accelerometer readings
            double deltaX = Math.Abs((double)(lastValue.X - currentValue.X));
            double deltaY = Math.Abs((double)(lastValue.Y - currentValue.Y));
            double deltaZ = Math.Abs((double)(lastValue.Z - currentValue.Z));

            // If the difference is bigger than the threshold, the phone was shaken
            return (deltaX > threshold && deltaY > threshold) ||
                (deltaX > threshold && deltaZ > threshold) ||
                (deltaY > threshold && deltaZ > threshold);
        }

        /// <summary>
        /// Gets the current state of the accelerometer.
        /// </summary>
        /// <returns>A new AccelerometerState with the current state of the accelerometer.</returns>
        public static AccelerometerState GetState()
        {
            // make sure we've initialized the Accelerometer before we try to get the state
            if (!isInitialized)
            {
                throw new InvalidOperationException("You must Initialize before you can call GetState");
            }

            // create a new value for our state
            Vector3 stateValue = new Vector3();

            // if the accelerometer is active
            if (isActive)
            {
                if (Microsoft.Devices.Environment.DeviceType == DeviceType.Device)
                {
                    // if we're on device, we'll just grab our latest reading from the accelerometer
                    lock (threadLock)
                    {
                        stateValue = nextValue;
                    }
                }
                else
                {
                    // if we're in the emulator, we'll generate a fake acceleration value using the arrow keys
                    // press the pause/break key to toggle keyboard input for the emulator
                    KeyboardState keyboardState = Keyboard.GetState();

                    stateValue.Z = -1;

                    if (keyboardState.IsKeyDown(Keys.Left))
                        stateValue.X = -.1f;
                    if (keyboardState.IsKeyDown(Keys.Right))
                        stateValue.X = .1f;
                    if (keyboardState.IsKeyDown(Keys.Up))
                        stateValue.Y = .1f;
                    if (keyboardState.IsKeyDown(Keys.Down))
                        stateValue.Y = -.1f;

                    stateValue.Normalize();
                }
            }

            return new AccelerometerState(stateValue, isActive);
        }
    }

    /// <summary>
    /// An encapsulation of the accelerometer's current state.
    /// </summary>
    public struct AccelerometerState
    {
        /// <summary>
        /// Gets the accelerometer's current value in G-force.
        /// </summary>
        public Vector3 Acceleration { get; private set; }

        /// <summary>
        /// Gets whether or not the accelerometer is active and running.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Initializes a new AccelerometerState.
        /// </summary>
        /// <param name="acceleration">The current acceleration (in G-force) of the accelerometer.</param>
        /// <param name="isActive">Whether or not the accelerometer is active.</param>
        public AccelerometerState(Vector3 acceleration, bool isActive)
            : this()
        {
            Acceleration = acceleration;
            IsActive = isActive;
        }

        /// <summary>
        /// Returns a string containing the values of the Acceleration and IsActive properties.
        /// </summary>
        /// <returns>A new string describing the state.</returns>
        public override string ToString()
        {
            return string.Format("Acceleration: {0}, IsActive: {1}", Acceleration, IsActive);
        }
    }
}
