#region File Description
//-----------------------------------------------------------------------------
// RendererState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xna.Framework;
using Point = System.Windows.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ModelViewerDemo
{
    /// <summary>
    /// Implement our renderer state on top of a DependencyObject which enables us to use databinding for
    /// UI interaction or animation through a Storyboard.
    /// </summary>
	public class RendererState : DependencyObject
	{
		#region Dependency Property Fields

		// Define the viewport where XNA should render the tank
		public static readonly DependencyProperty ViewportXProperty =
            DependencyProperty.Register("ViewportX", typeof(double), typeof(RendererState), new PropertyMetadata(0.0));
		public static readonly DependencyProperty ViewportYProperty =
            DependencyProperty.Register("ViewportY", typeof(double), typeof(RendererState), new PropertyMetadata(0.0));
		public static readonly DependencyProperty ViewportWidthProperty =
            DependencyProperty.Register("ViewportWidth", typeof(double), typeof(RendererState), new PropertyMetadata(480.0));
		public static readonly DependencyProperty ViewportHeightProperty =
            DependencyProperty.Register("ViewportHeight", typeof(double), typeof(RendererState), new PropertyMetadata(800.0));

		// The position of the camera
		public static readonly DependencyProperty CameraXProperty =
            DependencyProperty.Register("CameraX", typeof(double), typeof(RendererState), new PropertyMetadata(1.0));
		public static readonly DependencyProperty CameraYProperty =
            DependencyProperty.Register("CameraY", typeof(double), typeof(RendererState), new PropertyMetadata(2.0));
		public static readonly DependencyProperty CameraZProperty =
            DependencyProperty.Register("CameraZ", typeof(double), typeof(RendererState), new PropertyMetadata(2.5));

		// The target of the camera
		public static readonly DependencyProperty CameraTargetXProperty =
            DependencyProperty.Register("CameraTargetX", typeof(double), typeof(RendererState), new PropertyMetadata(0.0));
		public static readonly DependencyProperty CameraTargetYProperty =
            DependencyProperty.Register("CameraTargetY", typeof(double), typeof(RendererState), new PropertyMetadata(0.0));
		public static readonly DependencyProperty CameraTargetZProperty =
            DependencyProperty.Register("CameraTargetZ", typeof(double), typeof(RendererState), new PropertyMetadata(0.0));

        // Tank settings
		public static readonly DependencyProperty TankRotationYProperty =
            DependencyProperty.Register("TankRotationY", typeof(double), typeof(RendererState), new PropertyMetadata(0.0));
        public static readonly DependencyProperty TankWheelAnimationEnabledProperty =
            DependencyProperty.Register("TankWheelAnimationEnabled", typeof(bool), typeof(RendererState), new PropertyMetadata(false));
        public static readonly DependencyProperty TankSteeringAnimationEnabledProperty =
            DependencyProperty.Register("TankSteeringAnimationEnabled", typeof(bool), typeof(RendererState), new PropertyMetadata(false));
        public static readonly DependencyProperty TankTurretAnimationEnabledProperty =
            DependencyProperty.Register("TankTurretAnimationEnabled", typeof(bool), typeof(RendererState), new PropertyMetadata(true));
        public static readonly DependencyProperty TankCannonAnimationEnabledProperty =
            DependencyProperty.Register("TankCannonAnimationEnabled", typeof(bool), typeof(RendererState), new PropertyMetadata(true));
        public static readonly DependencyProperty TankHatchAnimationEnabledProperty =
            DependencyProperty.Register("TankHatchAnimationEnabled", typeof(bool), typeof(RendererState), new PropertyMetadata(true));

        // Other settings
        public static readonly DependencyProperty EnableLightingProperty =
            DependencyProperty.Register("EnableLighting", typeof(bool), typeof(RendererState), new PropertyMetadata(false));
		public static readonly DependencyProperty EnableTextureProperty =
            DependencyProperty.Register("EnableTexture", typeof(bool), typeof(RendererState), new PropertyMetadata(true));
		public static readonly DependencyProperty DrawWireframeProperty =
            DependencyProperty.Register("DrawWireframe", typeof(bool), typeof(RendererState), new PropertyMetadata(false));
        public static readonly DependencyProperty ShowFrameRateProperty =
            DependencyProperty.Register("ShowFrameRate", typeof(bool), typeof(RendererState), new PropertyMetadata(false));

		#endregion

		#region Dependency Properties

		public double ViewportX
		{
			get { return (double)GetValue(ViewportXProperty); }
			set { SetValue(ViewportXProperty, value); }
		}

		public double ViewportY
		{
			get { return (double)GetValue(ViewportYProperty); }
			set { SetValue(ViewportYProperty, value); }
		}

		public double ViewportWidth
		{
			get { return (double)GetValue(ViewportWidthProperty); }
			set { SetValue(ViewportWidthProperty, value); }
		}

		public double ViewportHeight
		{
			get { return (double)GetValue(ViewportHeightProperty); }
			set { SetValue(ViewportHeightProperty, value); }
		}

		public double CameraX
		{
			get { return (double)GetValue(CameraXProperty); }
			set { SetValue(CameraXProperty, value); }
		}

		public double CameraY
		{
			get { return (double)GetValue(CameraYProperty); }
			set { SetValue(CameraYProperty, value); }
		}

		public double CameraZ
		{
			get { return (double)GetValue(CameraZProperty); }
			set { SetValue(CameraZProperty, value); }
		}

		public double CameraTargetX
		{
			get { return (double)GetValue(CameraTargetXProperty); }
			set { SetValue(CameraTargetXProperty, value); }
		}

		public double CameraTargetY
		{
			get { return (double)GetValue(CameraTargetYProperty); }
			set { SetValue(CameraTargetYProperty, value); }
		}

		public double CameraTargetZ
		{
			get { return (double)GetValue(CameraTargetZProperty); }
			set { SetValue(CameraTargetZProperty, value); }
		}

		public double TankRotationY
		{
			get { return (double)GetValue(TankRotationYProperty); }
			set { SetValue(TankRotationYProperty, value); }
		}

        public bool TankWheelAnimationEnabled
        {
            get { return (bool)GetValue(TankWheelAnimationEnabledProperty); }
            set { SetValue(TankWheelAnimationEnabledProperty, value); }
        }

        public bool TankSteeringAnimationEnabled
        {
            get { return (bool)GetValue(TankSteeringAnimationEnabledProperty); }
            set { SetValue(TankSteeringAnimationEnabledProperty, value); }
        }

        public bool TankTurretAnimationEnabled
        {
            get { return (bool)GetValue(TankTurretAnimationEnabledProperty); }
            set { SetValue(TankTurretAnimationEnabledProperty, value); }
        }

        public bool TankCannonAnimationEnabled
        {
            get { return (bool)GetValue(TankCannonAnimationEnabledProperty); }
            set { SetValue(TankCannonAnimationEnabledProperty, value); }
        }

        public bool TankHatchAnimationEnabled
        {
            get { return (bool)GetValue(TankHatchAnimationEnabledProperty); }
            set { SetValue(TankHatchAnimationEnabledProperty, value); }
        }

        public bool EnableLighting
        {
            get { return (bool)GetValue(EnableLightingProperty); }
            set { SetValue(EnableLightingProperty, value); }
        }

		public bool EnableTexture
		{
			get { return (bool)GetValue(EnableTextureProperty); }
			set { SetValue(EnableTextureProperty, value); }
		}

		public bool DrawWireframe
		{
			get { return (bool)GetValue(DrawWireframeProperty); }
			set { SetValue(DrawWireframeProperty, value); }
		}

        public bool ShowFrameRate
        {
            get { return (bool)GetValue(ShowFrameRateProperty); }
            set { SetValue(ShowFrameRateProperty, value); }
        }

		#endregion

		public Vector3 CameraPosition
		{
			get { return new Vector3((float)CameraX, (float)CameraY, (float)CameraZ); }
		}

		public Vector3 CameraTarget
		{
			get { return new Vector3((float)CameraTargetX, (float)CameraTargetY, (float)CameraTargetZ); }
		}

		public Rectangle ViewportRect
		{
			get
			{
				return new Rectangle(
					(int)ViewportX,
					(int)ViewportY,
					(int)ViewportWidth,
					(int)ViewportHeight);
			}
		}

        public LightState Light1 { get; private set; }
        public LightState Light2 { get; private set; }
        public LightState Light3 { get; private set; }

        public RendererState()
        {
            Light1 = new LightState();
            Light2 = new LightState();
            Light3 = new LightState();
        }

		#region Camera Animation Methods

		private readonly TimeSpan cameraAnimationDuration = TimeSpan.FromSeconds(0.5);
		private Storyboard cameraPositionStoryboard;
		private Storyboard cameraTargetStoryboard;

		public void AnimateCameraPosition(Vector3 newPosition)
		{
			if (cameraPositionStoryboard != null)
				cameraPositionStoryboard.Stop();

			// Create the storyboard and set us as the target
			cameraPositionStoryboard = new Storyboard();
			Storyboard.SetTarget(cameraPositionStoryboard, this);
			cameraPositionStoryboard.Completed += (s, e) => cameraPositionStoryboard = null;

			// Create the easing function we'll apply to the animations
			IEasingFunction easingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

			// Create animations for each of the axis
			DoubleAnimation xAnimation = new DoubleAnimation
			{
				From = CameraX,
				To = newPosition.X,
				Duration = cameraAnimationDuration,
				EasingFunction = easingFunction
			};
			DoubleAnimation yAnimation = new DoubleAnimation
			{
				From = CameraY,
				To = newPosition.Y,
				Duration = cameraAnimationDuration,
				EasingFunction = easingFunction
			};
			DoubleAnimation zAnimation = new DoubleAnimation
			{
				From = CameraZ,
				To = newPosition.Z,
				Duration = cameraAnimationDuration,
				EasingFunction = easingFunction
			};

			// Set the target properties of the animations
			Storyboard.SetTargetProperty(xAnimation, new PropertyPath(CameraXProperty));
			Storyboard.SetTargetProperty(yAnimation, new PropertyPath(CameraYProperty));
			Storyboard.SetTargetProperty(zAnimation, new PropertyPath(CameraZProperty));

			// Add the animations to the storyboard
			cameraPositionStoryboard.Children.Add(xAnimation);
			cameraPositionStoryboard.Children.Add(yAnimation);
			cameraPositionStoryboard.Children.Add(zAnimation);

			cameraPositionStoryboard.Begin();
		}

		public void AnimateCameraTarget(Vector3 newTarget)
		{
			if (cameraTargetStoryboard != null)
				cameraTargetStoryboard.Stop();

			// Create the storyboard and set us as the target
			cameraTargetStoryboard = new Storyboard();
			Storyboard.SetTarget(cameraTargetStoryboard, this);
			cameraTargetStoryboard.Completed += (s, e) => cameraTargetStoryboard = null;

			// Create the easing function we'll apply to the animations
			IEasingFunction easingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

			// Create animations for each of the axis
			DoubleAnimation xAnimation = new DoubleAnimation
			{
				From = CameraTargetX,
				To = newTarget.X,
				Duration = cameraAnimationDuration,
				EasingFunction = easingFunction
			};
			DoubleAnimation yAnimation = new DoubleAnimation
			{
				From = CameraTargetY,
				To = newTarget.Y,
				Duration = cameraAnimationDuration,
				EasingFunction = easingFunction
			};
			DoubleAnimation zAnimation = new DoubleAnimation
			{
				From = CameraTargetZ,
				To = newTarget.Z,
				Duration = cameraAnimationDuration,
				EasingFunction = easingFunction
			};

			// Set the target properties of the animations
			Storyboard.SetTargetProperty(xAnimation, new PropertyPath(CameraTargetXProperty));
			Storyboard.SetTargetProperty(yAnimation, new PropertyPath(CameraTargetYProperty));
			Storyboard.SetTargetProperty(zAnimation, new PropertyPath(CameraTargetZProperty));

			// Add the animations to the storyboard
			cameraTargetStoryboard.Children.Add(xAnimation);
			cameraTargetStoryboard.Children.Add(yAnimation);
			cameraTargetStoryboard.Children.Add(zAnimation);

			cameraTargetStoryboard.Begin();
		}

		#endregion
	}
}
