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
using System.Diagnostics;
using Microsoft.Devices.Sensors;


namespace Microsoft.Phone.Applications.Common
{
    /// <summary>
    /// Arguments provided by the Accelerometer Helper data event
    /// </summary>
    public class AccelerometerHelperReadingEventArgs : EventArgs
    {
        /// <summary>
        /// Raw, unfiltered accelerometer data (acceleration vector in all 3 dimensions) coming directly from sensor.
        /// This is required for updating rapidly reacting UI.
        /// </summary>
        public Simple3DVector RawAcceleration { get; set; }

        /// <summary>
        /// Filtered accelerometer data using a combination of a low-pass and threshold triggered high-pass on each axis to 
        /// elimate the majority of the sensor low amplitude noise while trending very quickly to large offsets (not perfectly
        /// smooth signal in that case), providing a very low latency. This is ideal for quickly reacting UI updates.
        /// </summary>
        public Simple3DVector OptimallyFilteredAcceleration { get; set; }

        /// <summary>
        /// Filtered accelerometer data using a 1 Hz first-order low-pass on each axis to elimate the main sensor noise
        /// while providing a medium latency. This can be used for moderatly reacting UI updates requiring a very smooth signal.
        /// </summary>
        public Simple3DVector LowPassFilteredAcceleration { get; set; }

        /// <summary>
        /// Filtered and temporally averaged accelerometer data using an arithmetic mean of the last 25 "optimaly filtered" 
        /// samples (see above), so over 500ms at 50Hz on each axis, to virtually eliminate most sensor noise. 
        /// This provides a very stable reading but it has also a very high latency and cannot be used for rapidly reacting UI.
        /// </summary>
        public Simple3DVector AverageAcceleration { get; set; }
    }

    /// <summary>
    /// Accelerometer Helper Class, providing filtering and local calibration of accelerometer sensor data 
    /// </summary>
    public sealed class AccelerometerHelper: IDisposable
    {
        #region Private fields

        // Singleton instance for helper - prefered solution to static class to avoid static constructor (10x slower)
        private static volatile AccelerometerHelper _singletonInstance;

        private static Object _syncRoot = new Object();

        /// <summary>
        /// Accelerometer sensor
        /// </summary>
        private Accelerometer _sensor;

        /// <summary>
        /// This is the inclination angle on any axis beyond which the device cannot be calibrated on that particular axis
        /// </summary>
        private const double MaximumCalibrationTiltAngle = 20.0 * Math.PI / 180.0; // 20 deg inclination from non-calibrated axis max

        /// <summary>
        /// Corresponding lateral acceleration offset at 1g of Maximum Calibration Tilt Angle
        /// </summary>
        private static double _maximumCalibrationOffset = Math.Sin(MaximumCalibrationTiltAngle);

        /// <summary>
        /// This is the maximum inclination angle variation on any axis between the average acceleration and the filtered 
        /// acceleration beyond which the device cannot be calibrated on that particular axis.
        /// The calibration cannot be done until this condition is met on the last contiguous samples from the accelerometer
        /// </summary>
        private const double MaximumStabilityTiltDeltaAngle = 0.5 * Math.PI / 180.0; // 0.5 deg inclination delta at max

        /// <summary>
        /// Corresponding lateral acceleration offset at 1g of Maximum Stability Tilt Delta Angle
        /// </summary>
        private static double _maximumStabilityDeltaOffset = Math.Sin(MaximumStabilityTiltDeltaAngle);

        /// <summary>
        /// Number of samples for which the accelemeter is "stable" (filtered acceleration is within Maximum Stability Tilt 
        /// Delta Angle of average acceleration)
        /// </summary>
        private int _deviceStableCount = 0;

        /// <summary>
        /// Number of prior samples to keep for averaging.       
        /// The higher this number, the larger the latency will be: 
        /// At 50Hz sampling rate: Latency = 20ms * SamplesCount
        /// </summary>
        private const int SamplesCount = 25; // averaging and checking stability on 500ms

        /// <summary>
        ///  This is the smoothing factor used for the 1st order discrete Low-Pass filter
        ///  The cut-off frequency fc = fs * K/(2*PI*(1-K))
        /// </summary>
        private const double LowPassFilterCoef = 0.1; // With a 50Hz sampling rate, this is gives a 1Hz cut-off

        /// <summary>
        /// Maximum amplitude of noise from sample to sample. 
        /// This is used to remove the noise selectively while allowing fast trending for larger amplitudes
        /// </summary>
        private const double NoiseMaxAmplitude = 0.05; // up to 0.05g deviation from filtered value is considered noise

        /// <summary>
        /// Indicates that the helper has not been initialized yet.
        /// This is used for filter past data initialization
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// Circular buffer of filtered samples
        /// </summary>
        private Simple3DVector[] _sampleBuffer = new Simple3DVector[SamplesCount];

        /// <summary>
        /// n-1 of low pass filter output
        /// </summary>
        private Simple3DVector _previousLowPassOutput;

        /// <summary>
        /// n-1 of optimal filter output
        /// </summary>
        private Simple3DVector _previousOptimalFilterOutput;

        /// <summary>
        /// Sum of all the filtered samples in the circular buffer file
        /// </summary>
        private Simple3DVector _sampleSum = new Simple3DVector(0.0 * SamplesCount, 0.0 * SamplesCount, -1.0 * SamplesCount); // assume start flat: -1g in z axis

        /// <summary>
        /// Index in circular buffer of samples
        /// </summary>
        private int _sampleIndex;

        /// <summary>
        /// Average acceleration
        /// This is a simple arithmetic average over the entire _sampleFile (SamplesCount elements) which contains filtered readings
        /// This is used for the calibration, to get a more steady reading of the acceleration
        /// </summary>
        private Simple3DVector _averageAcceleration;

        private const string AccelerometerCalibrationKeyName = "AccelerometerCalibration";

        /// <summary>
        /// Accelerometer is active and reading value when true
        /// </summary>
        private bool _active = false;

        #endregion


        #region Public events

        /// <summary>
        /// New raw and processed accelerometer data available event.
        /// Fires every 20ms.
        /// </summary>
        public event EventHandler<AccelerometerHelperReadingEventArgs> ReadingChanged;

        #endregion


        #region Constructor and finalizer

        /// <summary>
        /// Private constructor,
        /// Use Instance property to get singleton instance
        /// </summary>
        private AccelerometerHelper()
        {
            // Determine if accelerometer is present

            _sensor = new Accelerometer();
            if (_sensor == null)
            {
                NoAccelerometer = true;
            }
            else
            {
                NoAccelerometer = (_sensor.State == SensorState.NotSupported);
            }
            _sensor = null;

            //Set up buckets for calculating rolling average of the accelerations
            _sampleIndex = 0;
            ZeroAccelerationCalibrationOffset = AccelerometerCalibrationPersisted;
        }

        #endregion


        #region Public properties

        /// <summary>
        /// Singleton instance of the Accelerometer Helper class
        /// </summary>
        public static AccelerometerHelper Instance
        {
            get
            {
                if (_singletonInstance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_singletonInstance == null)
                        {
                            _singletonInstance = new AccelerometerHelper();
                        }
                    }
                }
                return _singletonInstance;
            }
        }

        /// <summary>
        /// True when the device is "stable" (no movement for about 0.5 sec)
        /// </summary>
        public bool IsDeviceStable
        {
            get
            {
                return (_deviceStableCount >= SamplesCount);
            }
        }

        /// <summary>
        /// Property to get and set Calibration Setting Key
        /// </summary>
        private static Simple3DVector AccelerometerCalibrationPersisted
        {
            get
            {
                double x = ApplicationSettingHelper.TryGetValueWithDefault<double>(AccelerometerCalibrationKeyName + "X", 0);
                double y = ApplicationSettingHelper.TryGetValueWithDefault<double>(AccelerometerCalibrationKeyName + "Y", 0);
                return new Simple3DVector(x, y, 0);
            }

            set
            {
                bool updated = ApplicationSettingHelper.AddOrUpdateValue(AccelerometerCalibrationKeyName + "X", value.X);
                updated |= ApplicationSettingHelper.AddOrUpdateValue(AccelerometerCalibrationKeyName + "Y", value.Y);
                if (updated)
                {
                    ApplicationSettingHelper.Save();
                }
            }
        }

        /// <summary>
        /// Persistant data (calibration of accelerometer)
        /// </summary>
        public Simple3DVector ZeroAccelerationCalibrationOffset { get; private set; }

        /// <summary>
        /// Accelerometer is not present on device 
        /// </summary>
        public bool NoAccelerometer { get; private set; }

        /// <summary>
        /// Accelerometer is active and reading value when true
        /// </summary>
        public bool IsActive
        {
            get { return _active; }
            set
            {
                if (!NoAccelerometer)
                {
                    if (value)
                    {
                        if (!_active)
                        {
                            StartAccelerometer();
                        }
                    }
                    else
                    {
                        if (_active)
                        {
                            StopAccelerometer();
                        }
                    }
                }
            }
        }

        #endregion


        #region Public methods

        /// <summary>
        /// Release sensor resource if not already done
        /// </summary>
        public void Dispose()
        {
            if (_sensor != null)
            {
                _sensor.Dispose();
            }
        }

        /// <summary>
        /// Indicate that the calibration of the sensor would work along a particular set of axis
        /// because the device is "stable enough" or not inclined beyond reasonable
        /// </summary>
        /// <param name="xAxis">check stability on X axis if true</param>
        /// <param name="yAxis">check stability on X axis if true</param>
        /// <returns>true if all of the axis checked were "stable enough" or not too inclined</returns>
        public bool CanCalibrate(bool xAxis, bool yAxis)
        {
            bool retval = false;
            lock (_sampleBuffer)
            {
                if (IsDeviceStable)
                {
                    double accelerationMagnitude = 0;
                    if (xAxis)
                    {
                        accelerationMagnitude += _averageAcceleration.X * _averageAcceleration.X;
                    }
                    if (yAxis)
                    {
                        accelerationMagnitude += _averageAcceleration.Y * _averageAcceleration.Y;
                    }
                    accelerationMagnitude = Math.Sqrt(accelerationMagnitude);
                    if (accelerationMagnitude <= _maximumCalibrationOffset)
                    { // inclination is not out of bounds to consider it a calibration offset
                        retval = true;
                    }
                }
            }
            return retval;
        }

        /// <summary>
        /// Calibrates the accelerometer on X and / or Y axis and save data to isolated storage.
        /// </summary>
        /// <param name="xAxis">calibrates X axis if true</param>
        /// <param name="yAxis">calibrates Y axis if true</param>
        /// <returns>true if succeeds</returns>
        public bool Calibrate(bool xAxis, bool yAxis)
        {
            bool retval = false;
            lock (_sampleBuffer)
            {
                if (CanCalibrate(xAxis, yAxis))
                {
                    ZeroAccelerationCalibrationOffset = 
                        new Simple3DVector(
                            xAxis ? -_averageAcceleration.X : ZeroAccelerationCalibrationOffset.X,
                            yAxis ? -_averageAcceleration.Y : ZeroAccelerationCalibrationOffset.Y,
                            0);
                    // Persist data
                    AccelerometerCalibrationPersisted = ZeroAccelerationCalibrationOffset;
                    retval = true;
                }
            }
            return retval;
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Initialize Accelerometer sensor and start sampling
        /// </summary>
        private void StartAccelerometer()
        {
            try
            {
                _sensor = new Accelerometer();
                if (_sensor != null)
                {
                    _sensor.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(sensor_ReadingChanged);
                    _sensor.Start();
                    _active = true;
                    NoAccelerometer = false;
                }
                else
                {
                    _active = false;
                    NoAccelerometer = true;
                }
            }
            catch (Exception e)
            {
                _active = false;
                NoAccelerometer = true;
                Debug.WriteLine("Exception creating Accelerometer: " + e.Message);
            }
        }

        /// <summary>
        /// Stop sampling and release accelerometer sensor
        /// </summary>
        private void StopAccelerometer()
        {
            try
            {
                if (_sensor != null)
                {
                    _sensor.ReadingChanged -= new EventHandler<AccelerometerReadingEventArgs>(sensor_ReadingChanged);
                    _sensor.Stop();
                    _sensor = null;
                    _active = false;
                    _initialized = false;
                }
            }
            catch (Exception e)
            {
                _active = false;
                NoAccelerometer = true;
                Debug.WriteLine("Exception deleting Accelerometer: " + e.Message);
            }
        }

        /// <summary>
        /// 1st order discrete low-pass filter used to remove noise from raw accelerometer.
        /// </summary>
        /// <param name="newInputValue">New input value (latest sample)</param>
        /// <param name="priorOutputValue">The previous output value (filtered, one sampling period ago)</param>
        /// <returns>The new output value</returns>
        private static double LowPassFilter(double newInputValue, double priorOutputValue)
        {
            double newOutputValue = priorOutputValue + LowPassFilterCoef * (newInputValue - priorOutputValue);
            return newOutputValue;
        }

        /// <summary>
        /// discrete low-magnitude fast low-pass filter used to remove noise from raw accelerometer while allowing fast trending on high amplitude changes
        /// </summary>
        /// <param name="newInputValue">New input value (latest sample)</param>
        /// <param name="priorOutputValue">The previous (n-1) output value (filtered, one sampling period ago)</param>
        /// <returns>The new output value</returns>
        private static double FastLowAmplitudeNoiseFilter(double newInputValue, double priorOutputValue)
        {
            double newOutputValue = newInputValue;
            if (Math.Abs(newInputValue - priorOutputValue) <= NoiseMaxAmplitude)
            { // Simple low-pass filter
                newOutputValue = priorOutputValue + LowPassFilterCoef * (newInputValue - priorOutputValue);
            }
            return newOutputValue;
        }

        /// <summary>
        /// Called on accelerometer sensor sample available.
        /// Main accelerometer data filtering routine
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">AccelerometerReadingAsyncEventArgs</param>
        private void sensor_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            Simple3DVector lowPassFilteredAcceleration;
            Simple3DVector optimalFilteredAcceleration;
            Simple3DVector averagedAcceleration;
            Simple3DVector rawAcceleration = new Simple3DVector(e.X, e.Y, e.Z);

            lock (_sampleBuffer)
            {
                if (!_initialized)
                { // Initialize file with 1st value
                    _sampleSum = rawAcceleration * SamplesCount;
                    _averageAcceleration = rawAcceleration;

                    // Initialize file with 1st value
                    for (int i = 0; i < SamplesCount; i++)
                    {
                        _sampleBuffer[i] = _averageAcceleration;
                    }

                    _previousLowPassOutput = _averageAcceleration;
                    _previousOptimalFilterOutput = _averageAcceleration;

                    _initialized = true;
                }

                // low-pass filter
                lowPassFilteredAcceleration = new Simple3DVector(
                    LowPassFilter(rawAcceleration.X, _previousLowPassOutput.X),
                    LowPassFilter(rawAcceleration.Y, _previousLowPassOutput.Y),
                    LowPassFilter(rawAcceleration.Z, _previousLowPassOutput.Z));
                _previousLowPassOutput = lowPassFilteredAcceleration;

                // optimal filter
                optimalFilteredAcceleration = new Simple3DVector(
                    FastLowAmplitudeNoiseFilter(rawAcceleration.X, _previousOptimalFilterOutput.X),
                    FastLowAmplitudeNoiseFilter(rawAcceleration.Y, _previousOptimalFilterOutput.Y),
                    FastLowAmplitudeNoiseFilter(rawAcceleration.Z, _previousOptimalFilterOutput.Z));
                _previousOptimalFilterOutput = optimalFilteredAcceleration;

                // Increment circular buffer insertion index
                _sampleIndex++;
                if (_sampleIndex >= SamplesCount) _sampleIndex = 0; // if at max SampleCount then wrap samples back to the beginning position in the list

                // Add new and remove old at _sampleIndex
                Simple3DVector newVect = optimalFilteredAcceleration;
                _sampleSum += newVect;
                _sampleSum -= _sampleBuffer[_sampleIndex];
                _sampleBuffer[_sampleIndex] = newVect;

                averagedAcceleration = _sampleSum / SamplesCount;
                _averageAcceleration = averagedAcceleration;

                // Stablity check
                // If current low-pass filtered sample is deviating for more than 1/100 g from average (max of 0.5 deg inclination noise if device steady)
                // then reset the stability counter.
                // The calibration will be prevented until the counter is reaching the sample count size (calibration enabled only if entire 
                // sampling buffer is "stable"
                Simple3DVector deltaAcceleration = averagedAcceleration - optimalFilteredAcceleration;
                if ((Math.Abs(deltaAcceleration.X) > _maximumStabilityDeltaOffset) ||
                    (Math.Abs(deltaAcceleration.Y) > _maximumStabilityDeltaOffset) ||
                    (Math.Abs(deltaAcceleration.Z) > _maximumStabilityDeltaOffset))
                { // Unstable
                    _deviceStableCount = 0;
                }
                else
                {
                    if (_deviceStableCount < SamplesCount) ++_deviceStableCount;
                }

                // Add calibrations
                rawAcceleration += ZeroAccelerationCalibrationOffset;
                lowPassFilteredAcceleration += ZeroAccelerationCalibrationOffset;
                optimalFilteredAcceleration += ZeroAccelerationCalibrationOffset;
                averagedAcceleration += ZeroAccelerationCalibrationOffset;
            }

            if (ReadingChanged != null)
            {
                AccelerometerHelperReadingEventArgs readingEventArgs = new AccelerometerHelperReadingEventArgs();

                readingEventArgs.RawAcceleration = rawAcceleration;
                readingEventArgs.LowPassFilteredAcceleration = lowPassFilteredAcceleration;
                readingEventArgs.OptimallyFilteredAcceleration = optimalFilteredAcceleration;
                readingEventArgs.AverageAcceleration = averagedAcceleration;

                ReadingChanged(this, readingEventArgs);
            }

        }

        #endregion

    }
}
