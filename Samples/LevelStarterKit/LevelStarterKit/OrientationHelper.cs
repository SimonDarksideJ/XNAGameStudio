// Copyright (c) 2010 Microsoft Corporation.  All rights reserved.
//
//
// Use of this source code is subject to the terms of the Microsoft
// license agreement under which you licensed this source code.
// If you did not accept the terms of the license agreement,
// you are not authorized to use this source code.
// For the terms of the license, please see the license agreement
// signed by you and Microsoft.
// THE SOURCE CODE IS PROVIDED "AS IS", WITH NO WARRANTIES OR INDEMNITIES.
//


using System;
using System.Collections.Generic;


namespace Microsoft.Phone.Applications.Common
{
    /// <summary>
    /// Possible orientations for the device
    /// </summary>
    public enum DeviceOrientation 
    {
        Unknown, 
        ScreenSideUp, 
        ScreenSideDown, 
        PortraitRightSideUp, 
        LandscapeRight, 
        LandscapeLeft, 
        PortraitUpSideDown
    }


    /// <summary>
    /// Arguments provided on device orientation change events
    /// </summary>
    public class DeviceOrientationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Current (new) orientation of the device
        /// </summary>
        public DeviceOrientation CurrentOrientation { get; set; }

        /// <summary>
        /// Previous (before this current update) orientation of the device
        /// </summary>
        public DeviceOrientation PreviousOrientation { get; set; }
    }


    /// <summary>
    /// Device Orientation Helper Class, providing 3D orientation (both landscape, both portrait and both flat modes),
    /// using the accelerometer sensor
    /// </summary>
    public sealed class DeviceOrientationHelper
    {
        #region Private fields

        // Singleton instance for helper - prefered solution to static class to avoid static constructor (10x slower)
        private static volatile DeviceOrientationHelper _singletonInstance;

        private static Object _syncRoot = new Object();

        private static Dictionary<DeviceOrientation, DeviceOrientationInfo> _deviceOrientationInfoList;

        /// <summary>
        /// Threshold horizontal acceleration to trigger an orientation change
        /// This needs to be above cos(PI/4)=0.707 to provide a proper hysteresis
        /// </summary>
        private const double tiltAccelerationThreshold = .8; // the level changes orientation when normalized acceleration exceeds .8g on non-vertical axis (about 53 deg inclination)

        /// <summary>
        /// Current device orientation
        /// </summary>
        private DeviceOrientation _currentOrientation = DeviceOrientation.Unknown;

        /// <summary>
        /// Previous device orientation
        /// </summary>
        private DeviceOrientation _previousOrientation = DeviceOrientation.Unknown;

        #endregion


        #region Public events

        /// <summary>
        /// Device Orientation changed event
        /// </summary>
        public event EventHandler<DeviceOrientationChangedEventArgs> OrientationChanged;

        #endregion


        #region Constructor and finalizer

        /// <summary>
        /// Private constructor.
        /// Use Instance property to get singleton instance
        /// </summary>
        private DeviceOrientationHelper()
        {
            if (_deviceOrientationInfoList == null)
            {
                _deviceOrientationInfoList = new Dictionary<DeviceOrientation, DeviceOrientationInfo>();
                _deviceOrientationInfoList.Add(DeviceOrientation.Unknown, new DeviceOrientationInfo(0, 0, new Simple3DVector(0, 0, 0)));
                _deviceOrientationInfoList.Add(DeviceOrientation.ScreenSideUp, new DeviceOrientationInfo(0, 1, new Simple3DVector(0, 0, -1)));
                _deviceOrientationInfoList.Add(DeviceOrientation.ScreenSideDown, new DeviceOrientationInfo(0, 1, new Simple3DVector(0, 0, 1)));
                _deviceOrientationInfoList.Add(DeviceOrientation.LandscapeRight, new DeviceOrientationInfo(-90, -1, new Simple3DVector(-1, 0, 0)));
                _deviceOrientationInfoList.Add(DeviceOrientation.LandscapeLeft, new DeviceOrientationInfo(90, 1, new Simple3DVector(1, 0, 0)));
                _deviceOrientationInfoList.Add(DeviceOrientation.PortraitRightSideUp, new DeviceOrientationInfo(0, -1, new Simple3DVector(0, -1, 0)));
                _deviceOrientationInfoList.Add(DeviceOrientation.PortraitUpSideDown, new DeviceOrientationInfo(-180, 1, new Simple3DVector(0, 1, 0)));
            }
            AccelerometerHelper.Instance.ReadingChanged += new EventHandler<AccelerometerHelperReadingEventArgs>(accelerometerHelper_ReadingChanged);
        }

        #endregion


        #region Public properties

        /// <summary>
        /// Singleton instance of the Accelerometer Helper class
        /// </summary>
        public static DeviceOrientationHelper Instance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_singletonInstance == null)
                        {
                            _singletonInstance = new DeviceOrientationHelper();
                        }
                    }
                }
                return _singletonInstance;
            }
        }

        /// <summary>
        /// Get current device orientation
        /// </summary>
        public DeviceOrientation CurrentOrientation
        {
            get { return _currentOrientation; }
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Provides information (orientation angle, axis polarity,...) base on particular device orientation
        /// </summary>
        /// <param name="deviceOrientation">device orientation</param>
        /// <returns>detailed information</returns>
        public static DeviceOrientationInfo GetDeviceOrientationInfo(DeviceOrientation deviceOrientation)
        {
            return _deviceOrientationInfoList[deviceOrientation];
        }

        /// <summary>
        /// Indicates that the device orientation provided is one of the two landscape modes
        /// </summary>
        /// <param name="deviceOrientation">device orientation</param>
        /// <returns>true if landscape</returns>
        public static bool IsLandscape(DeviceOrientation deviceOrientation)
        {
            DeviceOrientationInfo deviceOrientationInfo = GetDeviceOrientationInfo(deviceOrientation);
            return (deviceOrientationInfo.NormalGravityVector.X != 0);
        }

        /// <summary>
        /// Indicates that the device orientation provided is one of the two portrait modes
        /// </summary>
        /// <param name="deviceOrientation">device orientation</param>
        /// <returns>true if portrait</returns>
        public static bool IsPortrait(DeviceOrientation deviceOrientation)
        {
            DeviceOrientationInfo deviceOrientationInfo = GetDeviceOrientationInfo(deviceOrientation);
            return (deviceOrientationInfo.NormalGravityVector.Y != 0);
        }

        /// <summary>
        /// Indicates that the device orientation provided is one of the two flat modes
        /// </summary>
        /// <param name="deviceOrientation">device orientation</param>
        /// <returns>true if flat</returns>
        public static bool IsFlat(DeviceOrientation deviceOrientation)
        {
            DeviceOrientationInfo deviceOrientationInfo = GetDeviceOrientationInfo(deviceOrientation);
            return (deviceOrientationInfo.NormalGravityVector.Z != 0);
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Called on accelerometer sensor sample available.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">AccelerometerHelperReadingEventArgs</param>
        private void accelerometerHelper_ReadingChanged(object sender, AccelerometerHelperReadingEventArgs e)
        {
            CheckOrientation(e.LowPassFilteredAcceleration);
        }

        /// <summary>
        /// Main orientation change detection logic
        /// </summary>
        /// <param name="filteredAcceleration">current filtered acceleration</param>
        private void CheckOrientation(Simple3DVector filteredAcceleration)
        {
            DeviceOrientation currentOrientation = DeviceOrientation.Unknown;

            double xAcceleration = filteredAcceleration.X;
            double yAcceleration = filteredAcceleration.Y;
            double zAcceleration = filteredAcceleration.Z;

            // Normalize acceleration to 1g
            double magnitudeXYZ = Math.Sqrt(xAcceleration * xAcceleration + yAcceleration * yAcceleration + zAcceleration * zAcceleration);
            xAcceleration = xAcceleration / magnitudeXYZ;
            yAcceleration = yAcceleration / magnitudeXYZ;
            zAcceleration = zAcceleration / magnitudeXYZ;

            if (_currentOrientation == DeviceOrientation.Unknown)
            { // No pre-existing orientation: default is flat 
                if (zAcceleration < 0)
                {
                    currentOrientation = DeviceOrientation.ScreenSideUp;
                }
                else
                {
                    currentOrientation = DeviceOrientation.ScreenSideDown;
                }
            }

            if (yAcceleration < -tiltAccelerationThreshold)
            {
                currentOrientation = DeviceOrientation.PortraitRightSideUp;
            }
            else if (yAcceleration > tiltAccelerationThreshold)
            {
                currentOrientation = DeviceOrientation.PortraitUpSideDown;
            }
            else if (xAcceleration < -tiltAccelerationThreshold)
            {
                currentOrientation = DeviceOrientation.LandscapeLeft;
            }
            else if (xAcceleration > tiltAccelerationThreshold)
            {
                currentOrientation = DeviceOrientation.LandscapeRight;
            }
            else if (zAcceleration < -tiltAccelerationThreshold)
            {
                currentOrientation = DeviceOrientation.ScreenSideUp;
            }
            else if (zAcceleration > tiltAccelerationThreshold)
            {
                currentOrientation = DeviceOrientation.ScreenSideDown;
            }

            DeviceOrientation previousOrientation = DeviceOrientation.Unknown;
            bool fireEvent = false;

            if (currentOrientation != DeviceOrientation.Unknown)
            {
                lock (this) // Keep the lock as brief as posible
                {
                    _currentOrientation = currentOrientation;
                    if (_previousOrientation != _currentOrientation)
                    {
                        previousOrientation = _previousOrientation;
                        _previousOrientation = _currentOrientation;
                        fireEvent = true;
                    }
                }
            }

            if (fireEvent)
            {
                DeviceOrientationChangedEventArgs orientationEventArgs = new DeviceOrientationChangedEventArgs();
                orientationEventArgs.CurrentOrientation = currentOrientation;
                orientationEventArgs.PreviousOrientation = previousOrientation;
                if (OrientationChanged != null)
                {
                    OrientationChanged(this, orientationEventArgs);
                }
            }
        }
        
        #endregion

    }

}