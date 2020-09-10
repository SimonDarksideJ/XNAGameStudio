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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Diagnostics;

#if WINDOWS_PHONE
using Microsoft.Phone.Controls;
#endif

namespace Microsoft.Phone.Applications.Common.Controls
{
  /// <summary>
  /// Provides attached properties for adding a 'tilt' effect to all controls within a container
  /// </summary>
  public class TiltEffect : DependencyObject
  {

    #region Constructor and Static Constructor
    /// <summary>
    /// This is not a constructable class, but can't be static because it derives from DependencyObject
    /// </summary>
    private TiltEffect()
    {
    }

    /// <summary>
    /// Initialize the static properties
    /// </summary>
    static TiltEffect()
    {
      // For extra fun, add this to the list: typeof(Microsoft.Phone.Controls.PhoneApplicationPage)
      TiltableItems = new List<Type>() { typeof(ButtonBase), typeof(ListBoxItem), };
      UseLogarithmicEase = false;
    }

    #endregion


    #region Fields and simple properties

    // These constants are the same as the built-in effects
    /// <summary>
    /// Maximum amount of tilt, in radians
    /// </summary>
    const double MaxAngle = 0.3;

    /// <summary>
    /// Maximum amount of depression, in pixels
    /// </summary>
    const double MaxDepression = 25;

    /// <summary>
    /// Delay between releasing an element and the tilt release animation playing
    /// </summary>
    static readonly TimeSpan TiltReturnAnimationDelay = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Duration of tilt release animation
    /// </summary>
    static readonly TimeSpan TiltReturnAnimationDuration = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// The control that is currently being tilted
    /// </summary>
    static FrameworkElement currentTiltElement;

    /// <summary>
    /// The single instance of a storyboard used for all tilts
    /// </summary>
    static Storyboard tiltReturnStoryboard;

    /// <summary>
    /// The single instance of an X rotation used for all tilts
    /// </summary>
    static DoubleAnimation tiltReturnXAnimation;

    /// <summary>
    /// The single instance of a Y rotation used for all tilts
    /// </summary>
    static DoubleAnimation tiltReturnYAnimation;

    /// <summary>
    /// The single instance of a Z depression used for all tilts
    /// </summary>
    static DoubleAnimation tiltReturnZAnimation;

    /// <summary>
    /// The center of the tilt element
    /// </summary>
    static Point currentTiltElementCenter;

    /// <summary>
    /// Whether the animation just completed was for a 'pause' or not
    /// </summary>
    static bool wasPauseAnimation = false;

    /// <summary>
    /// Whether to use a slightly more accurate (but slightly slower) tilt animation easing function
    /// </summary>
    public static bool UseLogarithmicEase { get; set; }

    /// <summary>
    /// Default list of items that are tiltable
    /// </summary>
    public static List<Type> TiltableItems { get; private set; }

    #endregion


    #region Dependency properties

    /// <summary>
    /// Whether the tilt effect is enabled on a container (and all its children)
    /// </summary>
    public static readonly DependencyProperty IsTiltEnabledProperty = DependencyProperty.RegisterAttached(
      "IsTiltEnabled",
      typeof(bool),
      typeof(TiltEffect),
      new PropertyMetadata(OnIsTiltEnabledChanged)
      );

    /// <summary>
    /// Gets the IsTiltEnabled dependency property from an object
    /// </summary>
    /// <param name="source">The object to get the property from</param>
    /// <returns>The property's value</returns>
    public static bool GetIsTiltEnabled(DependencyObject source) { return (bool)source.GetValue(IsTiltEnabledProperty); }

    /// <summary>
    /// Sets the IsTiltEnabled dependency property on an object
    /// </summary>
    /// <param name="source">The object to set the property on</param>
    /// <param name="value">The value to set</param>
    public static void SetIsTiltEnabled(DependencyObject source, bool value) { source.SetValue(IsTiltEnabledProperty, value); }

    /// <summary>
    /// Suppresses the tilt effect on a single control that would otherwise be tilted
    /// </summary>
    public static readonly DependencyProperty SuppressTiltProperty = DependencyProperty.RegisterAttached(
      "SuppressTilt",
      typeof(bool),
      typeof(TiltEffect),
      null
      );

    /// <summary>
    /// Gets the SuppressTilt dependency property from an object
    /// </summary>
    /// <param name="source">The object to get the property from</param>
    /// <returns>The property's value</returns>
    public static bool GetSuppressTilt(DependencyObject source) { return (bool)source.GetValue(SuppressTiltProperty); }

    /// <summary>
    /// Sets the SuppressTilt dependency property from an object
    /// </summary>
    /// <param name="source">The object to get the property from</param>
    /// <param name="value">New value for suppress Tilt</param>
    /// <returns>The property's value</returns>
    public static void SetSuppressTilt(DependencyObject source, bool value) { source.SetValue(SuppressTiltProperty, value); }


    /// <summary>
    /// Property change handler for the IsTiltEnabled dependency property
    /// </summary>
    /// <param name="target">The element that the property is atteched to</param>
    /// <param name="args">Event args</param>
    /// <remarks>
    /// Adds or removes event handlers from the element that has (un)registered for tilting
    /// </remarks>
    static void OnIsTiltEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
      if (target is FrameworkElement)
      {
        // Add / remove our event handler if necessary
        if ((bool)args.NewValue == true)
        {
          (target as FrameworkElement).ManipulationStarted += TiltEffect_ManipulationStarted;
        }
        else
        {
          (target as FrameworkElement).ManipulationStarted -= TiltEffect_ManipulationStarted;
        }
      }
    }

    #endregion


    #region Top-level manipulation event handlers

    /// <summary>
    /// Event handler for ManipulationStarted
    /// </summary>
    /// <param name="sender">sender of the event - this will be the tilt container (eg, entire page)</param>
    /// <param name="e">event args</param>
    static void TiltEffect_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      Debug.WriteLine("Started: " + e.ManipulationOrigin.X + ", " + e.ManipulationOrigin.Y);
      TryStartTiltEffect(sender as FrameworkElement, e);
    }

    /// <summary>
    /// Event handler for ManipulationDelta
    /// </summary>
    /// <param name="sender">sender of the event - this will be the tilting object (eg a button)</param>
    /// <param name="e">event args</param>
    static void TiltEffect_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      Debug.WriteLine("Delta: " + e.ManipulationOrigin.X + ", " + e.ManipulationOrigin.Y);
      ContinueTiltEffect(sender as FrameworkElement, e);
    }

    /// <summary>
    /// Event handler for ManipulationCompleted
    /// </summary>
    /// <param name="sender">sender of the event - this will be the tilting object (eg a button)</param>
    /// <param name="e">event args</param>
    static void TiltEffect_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      Debug.WriteLine("Completed: " + e.ManipulationOrigin.X + ", " + e.ManipulationOrigin.Y);
      EndTiltEffect(currentTiltElement);
    }

    #endregion


    #region Core tilt logic

    /// <summary>
    /// Checks if the manipulation should cause a tilt, and if so starts the tilt effect
    /// </summary>
    /// <param name="source">The source of the manipulation (the tilt container, eg entire page)</param>
    /// <param name="e">The args from the ManipulationStarted event</param>
    static void TryStartTiltEffect(FrameworkElement source, ManipulationStartedEventArgs e)
    {
      foreach (FrameworkElement ancestor in (e.OriginalSource as FrameworkElement).GetVisualAncestors())
      {
        foreach (Type t in TiltableItems)
        {
          if (t.IsAssignableFrom(ancestor.GetType()))
          {
            if ((bool)ancestor.GetValue(SuppressTiltProperty) != true)
            {
              // Use first child of the control, so that we can add transforms and not
              // impact any transforms on the control itself
              FrameworkElement element = VisualTreeHelper.GetChild(ancestor, 0) as FrameworkElement;
              FrameworkElement container = e.ManipulationContainer as FrameworkElement;

              if (element == null || container == null)
                return;

              // touch point relative to the element being tilted
              Point tiltTouchPoint = container.TransformToVisual(element).Transform(e.ManipulationOrigin);

              // center of the element being tilted
              Point elementCenter = new Point(element.ActualWidth / 2, element.ActualHeight / 2);

              // Camera adjustment
              Point centerToCenterDelta = GetCenterToCenterDelta(element, source);

              BeginTiltEffect(element, tiltTouchPoint, elementCenter, centerToCenterDelta);
              return;
            }
          }
        }
      }
    }

    /// <summary>
    /// Computes the delta between the centre of an element and its container
    /// </summary>
    /// <param name="element">The element to compare</param>
    /// <param name="container">The element to compare against</param>
    /// <returns>A point that represents the delta between the two centers</returns>
    static Point GetCenterToCenterDelta(FrameworkElement element, FrameworkElement container)
    {
      Point elementCenter = new Point(element.ActualWidth / 2, element.ActualHeight / 2);
      Point containerCenter;

#if WINDOWS_PHONE

  // need to special-case the frame because it lies about its width / height
      if (container is PhoneApplicationFrame)
      {
        PhoneApplicationFrame frame = container as PhoneApplicationFrame;

        // Switch width and height in landscape mode
        if ((frame.Orientation & PageOrientation.Landscape) == PageOrientation.Landscape)
        {
          Debug.WriteLine("Switching container coordinates because it's the root frame");
          containerCenter = new Point(container.ActualHeight / 2, container.ActualWidth / 2);
        }
        else
          containerCenter = new Point(container.ActualWidth / 2, container.ActualHeight / 2);
      }
      else
        containerCenter = new Point(container.ActualWidth / 2, container.ActualHeight / 2);
#else

      containerCenter = new Point(container.ActualWidth / 2, container.ActualHeight / 2);

#endif

      Point transformedElementCenter = element.TransformToVisual(container).Transform(elementCenter);
      Point result = new Point(containerCenter.X - transformedElementCenter.X, containerCenter.Y - transformedElementCenter.Y);
      Debug.WriteLine("Transforming center " + transformedElementCenter + " to " + containerCenter + "; got " + result);
      return result;
    }

    /// <summary>
    /// Begins the tilt effect by preparing the control and doing the initial animation
    /// </summary>
    /// <param name="element">The element to tilt </param>
    /// <param name="touchPoint">The touch point, in element coordinates</param>
    /// <param name="centerPoint">The center point of the element in element coordinates</param>
    /// <param name="centerDelta">The delta between the <paramref name="element"/>'s center and 
    /// the container's center</param>
    static void BeginTiltEffect(FrameworkElement element, Point touchPoint, Point centerPoint, Point centerDelta)
    {
      Debug.WriteLine("BeginTilt: " + touchPoint + " / " + centerPoint + " / " + centerDelta);

      if (tiltReturnStoryboard != null)
        StopTiltReturnStoryboardAndCleanup();

      if (PrepareControlForTilt(element, centerDelta) == false)
        return;

      currentTiltElement = element;
      currentTiltElementCenter = centerPoint;
      PrepareTiltReturnStoryboard(element);

      ApplyTiltEffect(currentTiltElement, touchPoint, currentTiltElementCenter);
    }

    /// <summary>
    /// Prepares a control to be tilted by setting up a plane projection and some event handlers
    /// </summary>
    /// <param name="element">The control that is to be tilted</param>
    /// <param name="centerDelta">Delta between the element's center and the tilt container's</param>
    /// <returns>true if successful; false otherwise</returns>
    /// <remarks>
    /// This method is pretty conservative; it will fail any attempt to tilt a control that already
    /// has a projection on it
    /// </remarks>
    static bool PrepareControlForTilt(FrameworkElement element, Point centerDelta)
    {
      // Don't clobber any existing transforms
      if (element.Projection != null || (element.RenderTransform != null && element.RenderTransform.GetType() != typeof(MatrixTransform)))
        return false;

      TranslateTransform transform = new TranslateTransform();
      transform.X = centerDelta.X;
      transform.Y = centerDelta.Y;
      element.RenderTransform = transform;

      PlaneProjection projection = new PlaneProjection();
      projection.GlobalOffsetX = -1 * centerDelta.X;
      projection.GlobalOffsetY = -1 * centerDelta.Y;
      element.Projection = projection;

      element.ManipulationDelta += TiltEffect_ManipulationDelta;
      element.ManipulationCompleted += TiltEffect_ManipulationCompleted;

      return true;
    }

    /// <summary>
    /// Removes modifications made by PrepareControlForTilt
    /// </summary>
    /// <param name="element">THe control to be un-prepared</param>
    /// <remarks>
    /// This method is pretty basic; it doesn't do anything to detect if the control being un-prepared
    /// was previously prepared
    /// </remarks>
    static void RevertPrepareControlForTilt(FrameworkElement element)
    {
      element.ManipulationDelta -= TiltEffect_ManipulationDelta;
      element.ManipulationCompleted -= TiltEffect_ManipulationCompleted;
      element.Projection = null;
      element.RenderTransform = null;
    }

    /// <summary>
    /// Creates the tilt return storyboard (if not already created) and targets it to the projection
    /// </summary>
    /// <param name="element">The element to applu the storyboard to</param>
    static void PrepareTiltReturnStoryboard(FrameworkElement element)
    {

      if (tiltReturnStoryboard == null)
      {
        tiltReturnStoryboard = new Storyboard();
        tiltReturnStoryboard.Completed += TiltReturnStoryboard_Completed;

        tiltReturnXAnimation = new DoubleAnimation();
        Storyboard.SetTargetProperty(tiltReturnXAnimation, new PropertyPath(PlaneProjection.RotationXProperty));
        tiltReturnXAnimation.BeginTime = TiltReturnAnimationDelay;
        tiltReturnXAnimation.To = 0;
        tiltReturnXAnimation.Duration = TiltReturnAnimationDuration;

        tiltReturnYAnimation = new DoubleAnimation();
        Storyboard.SetTargetProperty(tiltReturnYAnimation, new PropertyPath(PlaneProjection.RotationYProperty));
        tiltReturnYAnimation.BeginTime = TiltReturnAnimationDelay;
        tiltReturnYAnimation.To = 0;
        tiltReturnYAnimation.Duration = TiltReturnAnimationDuration;

        tiltReturnZAnimation = new DoubleAnimation();
        Storyboard.SetTargetProperty(tiltReturnZAnimation, new PropertyPath(PlaneProjection.GlobalOffsetZProperty));
        tiltReturnZAnimation.BeginTime = TiltReturnAnimationDelay;
        tiltReturnZAnimation.To = 0;
        tiltReturnZAnimation.Duration = TiltReturnAnimationDuration;

        if (UseLogarithmicEase)
        {
          tiltReturnXAnimation.EasingFunction = new LogarithmicEase();
          tiltReturnYAnimation.EasingFunction = new LogarithmicEase();
          tiltReturnZAnimation.EasingFunction = new LogarithmicEase();
        }

        tiltReturnStoryboard.Children.Add(tiltReturnXAnimation);
        tiltReturnStoryboard.Children.Add(tiltReturnYAnimation);
        tiltReturnStoryboard.Children.Add(tiltReturnZAnimation);
      }

      Storyboard.SetTarget(tiltReturnXAnimation, element.Projection);
      Storyboard.SetTarget(tiltReturnYAnimation, element.Projection);
      Storyboard.SetTarget(tiltReturnZAnimation, element.Projection);
    }


    /// <summary>
    /// Continues a tilt effect that is currently applied to an element, presumably because
    /// the user moved their finger
    /// </summary>
    /// <param name="element">The element being tilted</param>
    /// <param name="e">The manipulation event args</param>
    static void ContinueTiltEffect(FrameworkElement element, ManipulationDeltaEventArgs e)
    {
      FrameworkElement container = e.ManipulationContainer as FrameworkElement;
      if (container == null || element == null)
        return;

      Point tiltTouchPoint = container.TransformToVisual(element).Transform(e.ManipulationOrigin);

      // If touch moved outside bounds of element, then pause the tilt (but don't cancel it)
      if (new Rect(0, 0, currentTiltElement.ActualWidth, currentTiltElement.ActualHeight).Contains(tiltTouchPoint) != true)
      {
        Debug.WriteLine("Pause at " + tiltTouchPoint.X + ", " + tiltTouchPoint.Y);
        PauseTiltEffect();
        return;
      }

      // Apply the updated tilt effect
      ApplyTiltEffect(currentTiltElement, e.ManipulationOrigin, currentTiltElementCenter);
    }

    /// <summary>
    /// Ends the tilt effect by playing the animation  
    /// </summary>
    /// <param name="element">The element being tilted</param>
    static void EndTiltEffect(FrameworkElement element)
    {
      if (element != null)
      {
        element.ManipulationCompleted -= TiltEffect_ManipulationCompleted;
        element.ManipulationDelta -= TiltEffect_ManipulationDelta;
      }

      if (tiltReturnStoryboard != null)
      {
        wasPauseAnimation = false;
        if (tiltReturnStoryboard.GetCurrentState() != ClockState.Active)
          tiltReturnStoryboard.Begin();
      }
      else
        StopTiltReturnStoryboardAndCleanup();
    }

    /// <summary>
    /// Handler for the storyboard complete event
    /// </summary>
    /// <param name="sender">sender of the event</param>
    /// <param name="e">event args</param>
    static void TiltReturnStoryboard_Completed(object sender, EventArgs e)
    {
      if (wasPauseAnimation)
        ResetTiltEffect(currentTiltElement);
      else
        StopTiltReturnStoryboardAndCleanup();
    }

    /// <summary>
    /// Resets the tilt effect on the control, making it appear 'normal' again 
    /// </summary>
    /// <param name="element">The element to reset the tilt on</param>
    /// <remarks>
    /// This method doesn't turn off the tilt effect or cancel any current
    /// manipulation; it just temporarily cancels the effect
    /// </remarks>
    static void ResetTiltEffect(FrameworkElement element)
    {
      PlaneProjection projection = element.Projection as PlaneProjection;
      projection.RotationY = 0;
      projection.RotationX = 0;
      projection.GlobalOffsetZ = 0;
    }

    /// <summary>
    /// Stops the tilt effect and release resources applied to the currently-tilted control
    /// </summary>
    static void StopTiltReturnStoryboardAndCleanup()
    {
      if (tiltReturnStoryboard != null)
        tiltReturnStoryboard.Stop();

      RevertPrepareControlForTilt(currentTiltElement);
    }

    /// <summary>
    /// Pauses the tilt effect so that the control returns to the 'at rest' position, but doesn't
    /// stop the tilt effect (handlers are still attached, etc.)
    /// </summary>
    static void PauseTiltEffect()
    {
      if ((tiltReturnStoryboard != null) && !wasPauseAnimation)
      {
        tiltReturnStoryboard.Stop();
        wasPauseAnimation = true;
        tiltReturnStoryboard.Begin();
      }
    }

    /// <summary>
    /// Resets the storyboard to not running
    /// </summary>
    private static void ResetTiltReturnStoryboard()
    {
      tiltReturnStoryboard.Stop();
      wasPauseAnimation = false;
    }

    /// <summary>
    /// Applies the tilt effect to the control
    /// </summary>
    /// <param name="element">the control to tilt</param>
    /// <param name="touchPoint">The touch point, in the container's coordinates</param>
    /// <param name="centerPoint">The center point of the container</param>
    static void ApplyTiltEffect(FrameworkElement element, Point touchPoint, Point centerPoint)
    {
      // Kill any active animation
      ResetTiltReturnStoryboard();

      // Get relative point of the touch in percentage of container size
      Point normalizedPoint = new Point(
          Math.Min(Math.Max(touchPoint.X / (centerPoint.X * 2), 0), 1),
          Math.Min(Math.Max(touchPoint.Y / (centerPoint.Y * 2), 0), 1));

      // Magic math from shell...
      double xMagnitude = Math.Abs(normalizedPoint.X - 0.5);
      double yMagnitude = Math.Abs(normalizedPoint.Y - 0.5);
      double xDirection = -Math.Sign(normalizedPoint.X - 0.5);
      double yDirection = Math.Sign(normalizedPoint.Y - 0.5);
      double angleMagnitude = xMagnitude + yMagnitude;
      double xAngleContribution = xMagnitude + yMagnitude > 0 ? xMagnitude / (xMagnitude + yMagnitude) : 0;

      double angle = angleMagnitude * MaxAngle * 180 / Math.PI;
      double depression = (1 - angleMagnitude) * MaxDepression;

      // RotationX and RotationY are the angles of rotations about the x- or y-*axis*;
      // to achieve a rotation in the x- or y-*direction*, we need to swap the two.
      // That is, a rotation to the left about the y-axis is a rotation to the left in the x-direction,
      // and a rotation up about the x-axis is a rotation up in the y-direction.
      PlaneProjection projection = element.Projection as PlaneProjection;
      projection.RotationY = angle * xAngleContribution * xDirection;
      projection.RotationX = angle * (1 - xAngleContribution) * yDirection;
      projection.GlobalOffsetZ = -depression;
    }

    #endregion


    #region Custom easing function

    /// <summary>
    /// Provides an easing function for the tilt return
    /// </summary>
    private class LogarithmicEase : EasingFunctionBase
    {
      /// <summary>
      /// Computes the easing function
      /// </summary>
      /// <param name="normalizedTime">The time</param>
      /// <returns>The eased value</returns>
      protected override double EaseInCore(double normalizedTime)
      {
        return Math.Log(normalizedTime + 1) / 0.693147181; // ln(t + 1) / ln(2)
      }
    }

    #endregion
  }

  /// <summary>
  /// Couple of simple helpers for walking the visual tree
  /// </summary>
  static class TreeHelpers
  {
    /// <summary>
    /// Gets the ancestors of the element, up to the root
    /// </summary>
    /// <param name="node">The element to start from</param>
    /// <returns>An enumerator of the ancestors</returns>
    public static IEnumerable<FrameworkElement> GetVisualAncestors(this FrameworkElement node)
    {
      FrameworkElement parent = node.GetVisualParent();
      while (parent != null)
      {
        yield return parent;
        parent = parent.GetVisualParent();
      }
    }

    /// <summary>
    /// Gets the visual parent of the element
    /// </summary>
    /// <param name="node">The element to check</param>
    /// <returns>The visual parent</returns>
    public static FrameworkElement GetVisualParent(this FrameworkElement node)
    {
      return VisualTreeHelper.GetParent(node) as FrameworkElement;
    }
  }
}
