using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace WindowsPhone.Recipes.Push.Server.Behaviors
{
	/// <summary>
    /// An attached behavior that handle a visual element from the visual tree to associated view-model implementing the <see cref="IVisualHost"/> interface.
	/// </summary>
	public class VisualBinderBehavior : Behavior<FrameworkElement>
	{
        private IVisualHost _visualHost;

        protected override void OnAttached()
        {
            _visualHost = FindVisualHost();
            if (_visualHost == null)
            {
                throw new InvalidOperationException("Visual host wasn't found in the data context hierarchy.");
            }

            _visualHost.Visual = AssociatedObject;

            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            _visualHost.Visual = null;

            base.OnDetaching();
        }

        private IVisualHost FindVisualHost()
        {
            DependencyObject targetObject = AssociatedObject;
            IVisualHost visualHost = AssociatedObject.DataContext as IVisualHost;
            while (targetObject != null)
            {
                if (visualHost != null)
                {
                    return visualHost;
                }

                targetObject = VisualTreeHelper.GetParent(targetObject);
            }

            return null;
        }       
	}
}
