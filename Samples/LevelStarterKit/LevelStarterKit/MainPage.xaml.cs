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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Applications.Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Applications.Level.Resources;


namespace Microsoft.Phone.Applications.Level
{
    public partial class MainPage : PhoneApplicationPage
    {

        #region Private fields

        /// <summary>
        /// Curving angle of the glass tube or face at the extremities / edges
        /// </summary>
        private const double EdgeGlassAngle = 20.0 * Math.PI / 180.0;

        /// <summary>
        /// Actual width of the tube level - limits the range of movement of the bubble
        /// </summary>
        private const double UsableLateralAmplitude = 400;

        /// <summary>
        /// Acceleration at 1g in metric
        /// </summary>
        private const double StandardGravityInMetric = 9.81; // m/s-2

        /// <summary>
        /// Accelerometer refresh rate
        /// </summary>
        private const double AccelerometerRefreshRate = 50.0; // Hz

        /// <summary>
        /// Empirical adjustment factor to establish the buoyancy in proportion to the distance to the target position of the bubble
        /// </summary>
        private const double BuoyancyCoef = 15; // m-1

        /// <summary>
        /// Empirical adjustment factor to establish the drag in proportion to the speed of the bubble
        /// </summary>
        private const double ViscosityCoef = 10.0;

        /// <summary>
        /// Empirical adjustment factor to establish the loss of movement of the bubble when bouncing on the edge
        /// </summary>
        private const double EdgeBouncingLossCoef = 0.8;

        /// <summary>
        /// Empirical adjustment factor to establish the streching of the bubble in surface mode in relation to the its speed
        /// </summary>
        private const double SpeedBubbleStrechingCoef = 0.1;

        /// <summary>
        /// Maximum streching of bubble on X-axis (while Y-axis is being squeezed by same amount - actual streching of X/Y is the square of this value)
        /// </summary>
        private const double MaximumBubbleXYStretchRatio = 1.2;

        /// <summary>
        /// Side Way bubble at edge boost on Y axis
        /// </summary>
        private const double SideWayBubbleAtEdgeBoostOnYAxis = 1.2;

        /// <summary>
        /// Empirical adjustment factor to establish the stretching in proportion to the inclination angle when bubble at the edge
        /// </summary>
        private const double AngleBasedTubeBubbleStrechingAtEdgeCoef = 2.0;

        /// <summary>
        /// Empirical adjustment factor to establish the stretch in proportion to the speed of buoyancy of the bubble
        /// </summary>
        private const double TubeBubbleStretchBuoyancyCoef = .4;

        /// <summary>
        /// Typical median vertical acceleration while in tube mode (slightly less than 1g)
        /// </summary>
        private const double MedianVerticalAcceleration = 0.9; // g - Assume some slight inclination

        /// <summary>
        /// Maximum skew angle for bubble in tube
        /// </summary>
        private const double MaximumTubeBubbleSkew = 25.0; // Deg

        /// <summary>
        /// Minimum stretch ratio
        /// </summary>
        private const double MinimumStretchRatio = 0.5;

        /// <summary>
        /// Too avoid too much text flickering, maximize text update rate
        /// </summary>
        private const int FastTextUpdateFrequency = 10; // Hz

        /// <summary>
        /// When changes are small, update text less frequently to avoid flicker between two values
        /// </summary>
        private const int MediumTextUpdateFrequency = 2; // Hz

        /// <summary>
        /// Slowest text update
        /// </summary>
        private const int SlowTextUpdatePeriod = 3; // Sec

        /// <summary>
        /// Minimum variation of angle since last refresh to do an immediate refresh (fast update)
        /// </summary>
        private const double AngleFastChangeThreshold = 0.6; // deg

        /// <summary>
        /// Minimum variation of angle since last refresh to do an update at medium frequency
        /// </summary>
        private const double AngleMediumChangeThreshold = 0.3; // deg

        /// <summary>
        /// Text to be display above bubble indicating the angle(s) in degree
        /// In surface mode we display 2 angles (projected on X and Y axis)
        /// In tube mode we display 1 angle 
        /// </summary>
        private string _angleText = "";

        /// <summary>
        /// Current instant speed of bubble
        /// Z-axis is alway 0
        /// in tube mode, only X-axis is used, Y-axis is ignored
        /// </summary>
        private Simple3DVector _bubbleSpeed = new Simple3DVector();

        /// <summary>
        /// Current position of bubble
        /// Z-axis is alway 0
        /// in tube mode, only X-axis is used, Y-axis is ignored
        /// </summary>
        private Simple3DVector _bubblePosition = new Simple3DVector();

        /// <summary>
        /// Storyboard for initial fade-in effect
        /// </summary>
        private Storyboard _fadeInStoryboard;

        /// <summary>
        /// Animation for initial fade-in effect
        /// </summary>
        private DoubleAnimation _fadeInAnimation;

        /// <summary>
        /// Storyboard for rotating the tube level
        /// </summary>
        private Storyboard _tubeRotationStoryboard;

        /// <summary>
        /// Animation for rotating the tube level
        /// </summary>
        private DoubleAnimation _tubeRotationAnimation;

        /// <summary>
        /// Easing function for rotating the tubelevel
        /// </summary>
        private ExponentialEase _levelEasing;

        /// <summary>
        /// Current device orientation
        /// </summary>
        private DeviceOrientation _levelOrientation;

        /// <summary>
        /// Which direction to move the tube level - based on orientation of the device
        /// </summary>
        private int _bubbleDirection;

        /// <summary>
        /// True if surface / bullseye level is shown
        /// False if tube level is shown
        /// </summary>
        private bool _surfaceShown;

        /// <summary>
        /// Application Bar
        /// </summary>
        private ApplicationBar _appBar;

        /// <summary>
        /// Application bar button for calibration
        /// </summary>
        private ApplicationBarIconButton _calibrateAppBarButton;

        /// <summary>
        /// Current numerical angle value used to measure delta since last attempt to update angle text
        /// </summary>
        private double _angle;

        /// <summary>
        /// Current numerical angle value used to measure delta since last attempt to update angle text
        /// </summary>
        private double _angleOld;

        /// <summary>
        /// Counter used to trigger angle text updates on subdivisions of faster update rate
        /// </summary>
        private int _fastTextUpdateCounter;

        /// <summary>
        /// indicates that orientation of the device has been set once before
        /// </summary>
        private bool _orientationDefined = false;

        /// <summary>
        /// Dispatch timer for the Angle text display
        /// </summary>
        private DispatcherTimer _angleTextDispatchTimer;

        #endregion


        #region Constructor and finalizer

        public MainPage()
        {
            InitializeComponent();
        }

        #endregion


        #region Private properties

        /// <summary>
        /// True if surface / bullseye level is shown
        /// False if tube level is shown
        /// </summary>
        private bool SurfaceShown
        {
            get
            {
                return _surfaceShown;
            }
            set
            {
                if (value != _surfaceShown || !_orientationDefined)
                {
                    if (!_orientationDefined)
                    {
                        _fadeInStoryboard.Begin();
                        _orientationDefined = true;
                    }
                    SurfaceLevel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                    TubeLevel.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                    _angleText = "";
                    BubbleAngle.Text = "";
                    SurfaceLevelAngle.Text = "";
                    _fastTextUpdateCounter = 0;
                    _surfaceShown = value;
                }
            }

        }

        private bool calibrateXAxis
        {
            get
            {
                return (DeviceOrientationHelper.IsFlat(_levelOrientation) || DeviceOrientationHelper.IsPortrait(_levelOrientation));
            }
        }

        private bool calibrateYAxis
        {
            get
            {
                return (DeviceOrientationHelper.IsFlat(_levelOrientation) || DeviceOrientationHelper.IsLandscape(_levelOrientation));
            }
        }

        #endregion


        #region Protected methods

        /// <summary>
        /// On navigation to this page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (AccelerometerHelper.Instance.NoAccelerometer)
            { // No accelerometer, display error message
                NoAccelPresent.Visibility = Visibility.Visible;
                CenterOfSurface.Visibility = Visibility.Collapsed;
                SurfaceBubble.Visibility = Visibility.Collapsed;
            }
            else
            {
                SetupTimers();
                SetupAppBar();
                SetupAnimations();

                AccelerometerHelper.Instance.ReadingChanged += new EventHandler<AccelerometerHelperReadingEventArgs>(accelerometerHelper_ReadingChanged);
                DeviceOrientationHelper.Instance.OrientationChanged += new EventHandler<DeviceOrientationChangedEventArgs>(orientationHelper_OrientationChanged);

                // initial state of orientation:
                if (!_orientationDefined)
                {
                    DeviceOrientationChangedEventArgs deviceOrientationEventArgs = new DeviceOrientationChangedEventArgs();
                    deviceOrientationEventArgs.CurrentOrientation = DeviceOrientationHelper.Instance.CurrentOrientation;
                    if (deviceOrientationEventArgs.CurrentOrientation != DeviceOrientation.Unknown)
                    {
                        deviceOrientationEventArgs.PreviousOrientation = DeviceOrientation.Unknown;
                        ChangeLevelOrientation(deviceOrientationEventArgs);
                    }
                }

            }
        }

        /// <summary>
        /// On navigation away from this page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (!AccelerometerHelper.Instance.NoAccelerometer)
            {
                AccelerometerHelper.Instance.ReadingChanged -= new EventHandler<AccelerometerHelperReadingEventArgs>(accelerometerHelper_ReadingChanged);
                DeviceOrientationHelper.Instance.OrientationChanged -= new EventHandler<DeviceOrientationChangedEventArgs>(orientationHelper_OrientationChanged);
            }
        }

        #endregion


        #region Private methods

        /// <summary>
        /// Timer based update of the angle text
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="ea">Event arguments</param>
        private void UpdateAngleText(object sender, EventArgs ea)
        {
            double delta = (Math.Abs(_angleOld - _angle));
            if ((delta >= AngleFastChangeThreshold) || // update every time if delta is large enough
                ((delta >= AngleMediumChangeThreshold) && (_fastTextUpdateCounter % (FastTextUpdateFrequency / MediumTextUpdateFrequency) == 0)) ||
                (_fastTextUpdateCounter % (SlowTextUpdatePeriod * FastTextUpdateFrequency) == 0))
            {
                _angleOld = _angle;
                SurfaceLevelAngle.Text = _angleText;
                BubbleAngle.Text = _angleText;
            }
            _fastTextUpdateCounter++;
            bool canCalibrate = AccelerometerHelper.Instance.CanCalibrate(calibrateXAxis, calibrateYAxis);
            _calibrateAppBarButton.IsEnabled = canCalibrate;
        }

        /// <summary>
        /// Called on orientation change from orientation helper
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void orientationHelper_OrientationChanged(object sender, DeviceOrientationChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => ChangeLevelOrientation(e));
        }

        /// <summary>
        /// Change orientation of bubble surface or tube
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void ChangeLevelOrientation(DeviceOrientationChangedEventArgs e)
        {
            _bubbleSpeed = new Simple3DVector();
            if (DeviceOrientationHelper.IsFlat(e.CurrentOrientation))
            {
                SurfaceShown = true;
                DeviceOrientationInfo priorDoi = DeviceOrientationHelper.GetDeviceOrientationInfo(e.PreviousOrientation);
                _bubblePosition = new Simple3DVector(priorDoi.NormalGravityVector.X, priorDoi.NormalGravityVector.Y, 0);
            }
            else
            {
                DeviceOrientationInfo doi = DeviceOrientationHelper.GetDeviceOrientationInfo(e.CurrentOrientation);
                if (DeviceOrientationHelper.IsFlat(e.PreviousOrientation) || (e.PreviousOrientation == DeviceOrientation.Unknown))
                {
                    MoveLevel.Angle = doi.AngleOnXYPlan;
                    _bubblePosition = new Simple3DVector();
                    SurfaceShown = false;
                }
                double accumulatedLoops = MoveLevel.Angle / 360;
                accumulatedLoops = Math.Floor(accumulatedLoops + 0.5);
                _tubeRotationAnimation.To = 360 * accumulatedLoops + doi.AngleOnXYPlan;
                if ((_tubeRotationAnimation.To - MoveLevel.Angle) > 180)
                {
                    _tubeRotationAnimation.To -= 360;
                }
                if ((_tubeRotationAnimation.To - MoveLevel.Angle) < -180)
                {
                    _tubeRotationAnimation.To += 360;
                }
                _tubeRotationStoryboard.Begin();
                _bubbleDirection = doi.HorizontalAxisPolarity;
            }
            _levelOrientation = e.CurrentOrientation;
        }

        /// <summary>
        /// On Reading change / new sample from accelerometer
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void accelerometerHelper_ReadingChanged(object sender, AccelerometerHelperReadingEventArgs e)
        {
            Dispatcher.BeginInvoke(() => UpdateUI(e));
        }

        /// <summary>
        /// Creates the timers
        /// </summary>
        private void SetupTimers()
        {
            if (_angleTextDispatchTimer == null)
            {
                // create timer for text display (angles)
                _angleTextDispatchTimer = new DispatcherTimer();
                _angleTextDispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / FastTextUpdateFrequency);
                _angleTextDispatchTimer.Tick += new EventHandler(UpdateAngleText);
                _angleTextDispatchTimer.Start();
            }
        }

        /// <summary>
        /// Creates the AppBar and buttons
        /// </summary>
        private void SetupAppBar()
        {
            if (ApplicationBar == null)
            {
                _appBar = new ApplicationBar();
                ApplicationBar = _appBar;

                _calibrateAppBarButton = new ApplicationBarIconButton(new Uri("/Images/appbar.calibrate.rest.png", UriKind.Relative));
                _calibrateAppBarButton.Click += new EventHandler(Calibrate_Click);
                _calibrateAppBarButton.Text = Strings.CalibrateAppBarText;

                _appBar.Buttons.Add(_calibrateAppBarButton);
                _appBar.IsMenuEnabled = true;
                _appBar.IsVisible = true;
                _appBar.Opacity = 1;
            }
        }

        /// <summary>
        /// Calibrate button clicked
        /// </summary>
        private void Calibrate_Click(object sender, EventArgs e)
        {
            AccelerometerHelper.Instance.Calibrate(calibrateXAxis, calibrateYAxis);
            _fastTextUpdateCounter = 0;
        }

        /// <summary>
        /// Set up the animation for rotating the tube level
        /// </summary>
        private void SetupAnimations()
        {
            if (_tubeRotationStoryboard == null)
            {
                _tubeRotationStoryboard = new Storyboard();
                _tubeRotationAnimation = new DoubleAnimation();
                _tubeRotationAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
                _tubeRotationStoryboard.Children.Add(_tubeRotationAnimation);
                Storyboard.SetTarget(_tubeRotationAnimation, MoveLevel);
                Storyboard.SetTargetProperty(_tubeRotationAnimation, new PropertyPath("Angle"));
                _levelEasing = new ExponentialEase();
                _levelEasing.EasingMode = EasingMode.EaseOut;
                _tubeRotationAnimation.EasingFunction = _levelEasing;
            }

            if (_fadeInStoryboard == null)
            {
                _fadeInStoryboard = new Storyboard();
                _fadeInAnimation = new DoubleAnimation();
                _fadeInAnimation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500));
                _fadeInStoryboard.Children.Add(_fadeInAnimation);
                Storyboard.SetTarget(_fadeInAnimation, BubbleModes);
                Storyboard.SetTargetProperty(_fadeInAnimation, new PropertyPath("Opacity"));
                _fadeInAnimation.From = 0.0;
                _fadeInAnimation.To = 1.0;
            }
        }


        /// <summary>
        /// Update the UI - sensor visuals and check for device orientation
        /// </summary>
        private void UpdateUI(AccelerometerHelperReadingEventArgs e)
        {
            if (SurfaceShown)
            {
                UpdateSurfaceBubble(e);
            }
            else
            {
                UpdateTubeBubble(e);
            }
        }

        /// <summary>
        /// Update the surface level visuals
        /// </summary>
        private void UpdateSurfaceBubble(AccelerometerHelperReadingEventArgs e)
        {
            // ANGLE TEXT
            // ----------

            // Use filtered accelemeter data (steady)
            double x = -e.OptimallyFilteredAcceleration.X; // Orientation compensation
            double y = e.OptimallyFilteredAcceleration.Y;

            // Convert acceleration vector coordinates to Angles and Magnitude
            // Update reading on screen of instant inclination assuming steady device (gravity = measured acceleration)
            double magnitudeXYZ = e.OptimallyFilteredAcceleration.Magnitude;
            double xAngle = 0.0;
            double yAngle = 0.0;
            if (magnitudeXYZ != 0.0)
            {
                xAngle = Math.Asin(x / magnitudeXYZ) * 180.0 / Math.PI;
                yAngle = Math.Asin(y / magnitudeXYZ) * 180.0 / Math.PI;
            }

            _angle = Math.Abs (xAngle) + Math.Abs (yAngle);
            // Display angles as if they were buoyancy force instead of gravity (opposite) since it is
            // more natural to match targeted bubble location
            // Also orientation of Y-axis is opposite of screen for natural upward orientation
            _angleText = String.Format("{0:0.0}°  {1:0.0}°", xAngle, -yAngle);

            // BUBBLE POSITION
            // ---------------

            // ----------------------------------------------------------
            // For simplicity we are approximating that the bubble experiences a lateral attraction force
            // proportional to the distance to its target location (top of the glass based on inclination).
            // We will neglect the vertical speed of the bubble since the radius of the glass curve is much greater 
            // than the radius of radius of usable glass surface

            // Assume sphere has a 1m radius
            // Destination position is x and y
            // Current position is _bubblePosition

            // Update Buoyancy
            Simple3DVector lateralAcceleration = (new Simple3DVector(x, y, 0) - _bubblePosition) * BuoyancyCoef * StandardGravityInMetric;

            // Update drag:
            Simple3DVector drag = _bubbleSpeed * (-ViscosityCoef);

            // Update speed:
            lateralAcceleration += drag;
            lateralAcceleration /= AccelerometerRefreshRate; // impulse
            _bubbleSpeed += lateralAcceleration;
            
            // Update position
            _bubblePosition += _bubbleSpeed / AccelerometerRefreshRate;

            double edgeRadius = Math.Sin(EdgeGlassAngle);

            x = _bubblePosition.X;
            y = _bubblePosition.Y;

            // Get resulting angle and magnitude of bubble position given X and Y
            double angleFlat = Math.Atan2(y, x);
            double magnitudeFlat = Math.Sqrt(x * x + y * y);

            bool atEdge = false;
            if (magnitudeFlat > edgeRadius)
            { // Bubble reaches the edge
                magnitudeFlat = edgeRadius;
                // lossy bouncing when reaching edges
                _bubbleSpeed *= EdgeBouncingLossCoef;
                // Mirror movement along center to edge line
                _bubbleSpeed = new Simple3DVector(_bubbleSpeed.X * Math.Cos(2 * angleFlat) + _bubbleSpeed.Y * Math.Sin(2 * angleFlat),
                                                  _bubbleSpeed.X * Math.Sin(2 * angleFlat) - _bubbleSpeed.Y * Math.Cos(2 * angleFlat),
                                                  0);
                // change direction on x and y
                _bubbleSpeed *= new Simple3DVector(-1, -1, 1);
                // limit bubble position to edge
                _bubblePosition = new Simple3DVector(magnitudeFlat * Math.Cos(angleFlat), magnitudeFlat * Math.Sin(angleFlat), 0);
                atEdge = true;
            }

            // Set x and y location of the surface level bubble based on angle and magnitude
            double xPixelLocation = Math.Cos(angleFlat) * (magnitudeFlat / edgeRadius) * (UsableLateralAmplitude - SurfaceBubble.Width) / 2;
            double yPixelLocation = Math.Sin(angleFlat) * (magnitudeFlat / edgeRadius) * (UsableLateralAmplitude - SurfaceBubble.Width) / 2;
            SurfaceBubble.SetValue(Canvas.LeftProperty, xPixelLocation + (SurfaceLevelOuter.Width - SurfaceBubble.Width) / 2);
            SurfaceBubble.SetValue(Canvas.TopProperty, yPixelLocation + (SurfaceLevelOuter.Height - SurfaceBubble.Width) / 2);

            // Change bubble shape
            double stretchRatio;
            double horizontalDirection;
            if (atEdge)
            {
                stretchRatio = MaximumBubbleXYStretchRatio;
                horizontalDirection = angleFlat - Math.PI / 2; 
            }
            else
            {
                x = _bubbleSpeed.X;
                y = _bubbleSpeed.Y;
                double horizontalSpeed = Math.Sqrt(x * x + y * y);
                horizontalDirection = Math.Atan2(y, x) - Math.PI / 2;
                stretchRatio = Math.Min(horizontalSpeed * SpeedBubbleStrechingCoef, MaximumBubbleXYStretchRatio - 1.0) + 1.0;
            }
            SurfaceBubbleDirection.Angle = horizontalDirection * 180.0 / Math.PI;
            SurfaceBubbleScale.ScaleX = stretchRatio;
            SurfaceBubbleScale.ScaleY = 1.0 / stretchRatio;
        }


        /// <summary>
        /// Update the tube level visuals
        /// </summary>
        private void UpdateTubeBubble(AccelerometerHelperReadingEventArgs e)
        {
            // ANGLE TEXT
            // ----------

            // Use filtered accelemeter data (st5ady)
            double x = e.OptimallyFilteredAcceleration.X;
            double y = e.OptimallyFilteredAcceleration.Y;
            double horizontalAcceleration;
            double verticalAcceleration;

            // Choose appropriate axis based on orientation of level
            horizontalAcceleration = _bubbleDirection * (DeviceOrientationHelper.IsPortrait(_levelOrientation) ? x : y);
            verticalAcceleration = _bubbleDirection * (DeviceOrientationHelper.IsPortrait(_levelOrientation) ? y : -x); // rotate XY plan by -90

            // Convert acceleration vector coordinates to Angles and Magnitude
            // Update reading on screen of instant inclination assuming steady device (gravity = measured acceleration)
            double magnitudeXYZ = e.OptimallyFilteredAcceleration.Magnitude;
            double angle = 0.0;
            if (magnitudeXYZ != 0.0)
            {
                angle = Math.Asin(horizontalAcceleration / magnitudeXYZ) * 180.0 / Math.PI;
            }
            _angle = angle;
            // Display angles as if they were buoyancy force instead of gravity (opposite) since it is
            // more natural to match targeted bubble location
            _angleText = String.Format("{0:0.0}°", _angle);

            // BUBBLE POSITION
            // ---------------

            // ----------------------------------------------------------
            // For simplicity we are approximating that the bubble experiences a lateral attraction force
            // proportional to the distance to its target location (top of the glass based on inclination).
            // We will neglect the vertical speed of the bubble since the radius of the glass curve is much greater 
            // than the radius of radius of usable glass surface

            // Assume tube curve has a 1m radius
            // Destination position is x and y
            // Current position is _bubblePosition.X (use only one dimension)

            // Update Buoyancy
            double lateralAcceleration = (horizontalAcceleration - _bubblePosition.X) * BuoyancyCoef * StandardGravityInMetric;

            // Update drag:
            double drag = _bubbleSpeed.X * (-ViscosityCoef);

            // Update speed:
            lateralAcceleration += drag;
            lateralAcceleration /= AccelerometerRefreshRate; // impulse
            _bubbleSpeed += new Simple3DVector(lateralAcceleration, 0, 0);

            // Update position
            _bubblePosition += (_bubbleSpeed / AccelerometerRefreshRate);

            double edgeRadius = Math.Sin(EdgeGlassAngle);

            // Get resulting direction and magnitude of bubble position given X
            double magnitudeFlat = Math.Abs(_bubblePosition.X);
            double direction = Math.Sign(_bubblePosition.X);

            bool atEdge = false;
            if (magnitudeFlat > edgeRadius)
            { // Bubble reaches the edge
                magnitudeFlat = edgeRadius;
                // lossy bouncing when reaching edges
                // change direction
                _bubbleSpeed *= -EdgeBouncingLossCoef;
                // limit bubble position to edge
                _bubblePosition = new Simple3DVector(magnitudeFlat * direction, 0, 0);
                atEdge = true;
            }

            // Calculate position of bubble
            MoveBubble.X = (_bubblePosition.X / edgeRadius) * ((UsableLateralAmplitude / 2) - (BubbleCanvas.Width / 4));

            // Change bubble shape
            double stretchRatio;
            if (atEdge)
            {
                TubeBubbleSkew.AngleX = 0;
                stretchRatio = 1.0 + (Math.Abs(Math.Sin(angle / 180.0 * Math.PI)) - Math.Sin(EdgeGlassAngle)) * AngleBasedTubeBubbleStrechingAtEdgeCoef;
                TubeBubbleScale.ScaleX = 1.0 / stretchRatio;
                TubeBubbleScale.ScaleY = SideWayBubbleAtEdgeBoostOnYAxis * stretchRatio;
            }
            else
            {
                double horizontalSpeed = Math.Abs (_bubbleSpeed.X);
                double horizontalDirection = Math.Sign(_bubbleSpeed.X);
                // Stretch is proportional to horizontal speed
                stretchRatio = Math.Min(horizontalSpeed * SpeedBubbleStrechingCoef, MaximumBubbleXYStretchRatio - 1.0) + 1.0;
                if (stretchRatio < MinimumStretchRatio)
                {
                    stretchRatio = MinimumStretchRatio;
                }
                TubeBubbleSkew.AngleX = MaximumTubeBubbleSkew * (stretchRatio - 1.0) / (MaximumBubbleXYStretchRatio - 1.0) * horizontalDirection;
                // Stretch is also proportional to buoyancy
                stretchRatio -= TubeBubbleStretchBuoyancyCoef * ((verticalAcceleration / magnitudeXYZ) - MedianVerticalAcceleration);
                if (stretchRatio < MinimumStretchRatio)
                {
                    stretchRatio = MinimumStretchRatio;
                }
                TubeBubbleScale.ScaleX = 1.0 / stretchRatio;
                TubeBubbleScale.ScaleY = MaximumBubbleXYStretchRatio * stretchRatio;
            }
        }

        #endregion

    }
}