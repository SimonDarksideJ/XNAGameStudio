using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using Microsoft.Phone.Controls;
using System.Diagnostics;

namespace TombstoningSample
{
    public class StateUtils
    {
        // A List of Actions that will be executed on the first render. This is used to scroll
        // the ScrollViewer to the correct offset.
        static List<Action> workItems;


        /// <summary>
        /// Saves the contents and selection location of a TextBox to the state dictionary.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="textBox">The TextBox to be preserved.</param>
        public static void PreserveState(IDictionary<string, object> state, TextBox textBox)
        {
            state[textBox.Name + "_Text"] = textBox.Text;
            state[textBox.Name + "_SelectionStart"] = textBox.SelectionStart;
            state[textBox.Name + "_SelectionLength"] = textBox.SelectionLength;
        }
        /// <summary>
        /// Restores the contents and selection location of a TextBox from the page's state dictionary.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="textBox">The TextBox to be restored.</param>
        /// <param name="defaultValue">A default value that is used if the saved value cannot be retrieved.</param>
        public static void RestoreState(IDictionary<string, object> state, TextBox textBox, string defaultValue)
        {
            textBox.Text = TryGetValue<string>(state, textBox.Name + "_Text", defaultValue);
            textBox.SelectionStart = TryGetValue<int>(state, textBox.Name + "_SelectionStart", textBox.Text.Length);
            textBox.SelectionLength = TryGetValue<int>(state, textBox.Name + "_SelectionLength", 0);
        }

        /// <summary>
        /// Saves the checked state of a CheckBox to the state dictionary.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="checkBox">The CheckBox to be preserved.</param>
        public static void PreserveState(IDictionary<string, object> state, CheckBox checkBox)
        {
            state[checkBox.Name + "_IsChecked"] = checkBox.IsChecked;
        }
        /// <summary>
        /// Restores the checked state of a CheckBox from the page's state dictionary.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="checkBox">The CheckBox to be restored.</param>
        /// <param name="defaultValue">A default value that is used if the saved value cannot be retrieved.</param>
        public static void RestoreState(IDictionary<string, object> state, CheckBox checkBox, bool defaultValue)
        {
            checkBox.IsChecked = TryGetValue<bool>(state, checkBox.Name + "_IsChecked", defaultValue);
        }

        /// <summary>
        /// Saves the value of a Slider to the state dictionary.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="slider">The Slider to be preserved.</param>
        public static void PreserveState(IDictionary<string, object> state, Slider slider)
        {
            state[slider.Name + "_Value"] = slider.Value;
        }
        /// <summary>
        /// Restores the value of a Slider from the page's state dictionary.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="slider">The Slider to be restored.</param>
        /// <param name="defaultValue">A default value that is used if the saved value cannot be retrieved.</param>
        public static void RestoreState(IDictionary<string, object> state, Slider slider, double defaultValue)
        {
            slider.Value = TryGetValue<double>(state, slider.Name + "_Value", defaultValue);
        }

        /// <summary>
        /// Saves the checked state of a RadioButton to the state dictionary.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="radioButton">The RadioButton to be preserved.</param>
        public static void PreserveState(IDictionary<string, object> state, RadioButton radioButton)
        {
            state[radioButton.Name + "_IsChecked"] = radioButton.IsChecked;
        }
        /// <summary>
        /// Restores the checked state of a RadioButton from the page's state dictionary.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="radioButton">The RadioButton to be restored.</param>
        /// <param name="defaultValue">A default value that is used if the saved value cannot be retrieved.</param>
        public static void RestoreState(IDictionary<string, object> state, RadioButton radioButton, bool defaultValue)
        {
            radioButton.IsChecked = TryGetValue<bool>(state, radioButton.Name + "_IsChecked", defaultValue);
        }

        /// <summary>
        /// Saves the scroll offset of a ScrollViewer to the state dictionary.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="scrollViewer">The ScrollViewer to be preserved.</param>
        public static void PreserveState(IDictionary<string, object> state, ScrollViewer scrollViewer)
        {
            state[scrollViewer.Name + "_HorizontalOffset"] = scrollViewer.VerticalOffset;
            state[scrollViewer.Name + "_VerticalOffset"] = scrollViewer.VerticalOffset;
        }
        /// <summary>
        /// Retrieves the saved scroll offset from the state dictionary and creates a delegate to
        /// restore the scroll position on the page's first render.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="scrollViewer"></param>
        public static void RestoreState(IDictionary<string, object> state, ScrollViewer scrollViewer)
        {
            double offset = TryGetValue<double>(state, scrollViewer.Name + "_HorizontalOffset", 0);
            if (offset > 0)
            {
                ScheduleOnNextRender(delegate { scrollViewer.ScrollToHorizontalOffset(offset); });
            }
            offset = TryGetValue<double>(state, scrollViewer.Name + "_VerticalOffset", 0);
            if (offset > 0)
            {
                ScheduleOnNextRender(delegate { scrollViewer.ScrollToVerticalOffset(offset); });
            }
        }

        /// <summary>
        /// Saves the name of the control that has focus to the state dictionary.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="parent">The parent element for which focus is being saved.</param>
        public static void PreserveFocusState(IDictionary<string, object> state, FrameworkElement parent)
        {
            // Determine which control currently has focus.
            Control focusedControl = FocusManager.GetFocusedElement() as Control;

            // If no control has focus, store null in the State dictionary.
            if (focusedControl == null)
            {
                state["FocusedControlName"] = null;
            }
            else
            {
                // Find the control within the parent
                Control foundFE = parent.FindName(focusedControl.Name) as Control;

                // If the control isn't found within the parent, store null in the State dictionary.
                if (foundFE == null)
                {
                    state["FocusedElementName"] = null;
                }
                else
                {
                    // otherwise store the name of the control with focus.
                    state["FocusedElementName"] = focusedControl.Name;
                }
            }
        }

        /// <summary>
        /// Retrieves the name of the control that should have focus and creates a delegate to
        /// restore the scroll position on the page's first render.
        /// </summary>
        /// <param name="state">The calling page's state dictionary.</param>
        /// <param name="parent">The parent element for which focus is being restored.</param>
        public static void RestoreFocusState(IDictionary<string, object> state, FrameworkElement parent)
        {
            // Get the name of the control that should have focus.
            string focusedName = TryGetValue<string>(state, "FocusedElementName", null);

            // Check to see if the name is null or empty
            if (!String.IsNullOrEmpty(focusedName))
            {

                // Find the control name in the parent.
                Control focusedControl = parent.FindName(focusedName) as Control;
                if (focusedControl != null)
                {
                    // If the control is found, schedule a call to its Focus method for the next render.
                    ScheduleOnNextRender(delegate { focusedControl.Focus(); });
                }
            }
        }

        /// <summary>
        /// Helper method to retrieve a value from the state dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the value being retrieved.</typeparam>
        /// <param name="state">The state dictionary.</param>
        /// <param name="name">The key name for the value to be retrieved.</param>
        /// <param name="defaultValue">The default value returned if the requested value is not found.</param>
        /// <returns></returns>
        private static T TryGetValue<T>(IDictionary<string, object> state, string name, T defaultValue)
        {
            if (state.ContainsKey(name))
            {
                if (state[name] != null)
                {
                    return (T)state[name];
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Adds the supplied action to a list of actions that will be performed on the next render. This is
        /// used to schedule actions that cannot be completed before the page is rendered, such as setting 
        /// the offset of a ScrollViewer.
        /// </summary>
        /// <param name="action">The action to be added the list.</param>
        public static void ScheduleOnNextRender(Action action)
        {
            // If the list of work items is null, create a new one and register DoWorkOnRender as a 
            // handler for the CompositionTarget.Rendering event.
            if (workItems == null)
            {
                workItems = new List<Action>();
                CompositionTarget.Rendering += DoWorkOnRender;
            }

            // Add the supplied action to the list.
            workItems.Add(action);
        }

        /// <summary>
        /// The event handler for the CompositionTarget.Rendering event. This handler invokes the actions
        /// added with ScheduleOnNextRender. It deregisters itself from the Rendering event so that it is
        /// only called once.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void DoWorkOnRender(object sender, EventArgs args)
        {
            // Remove ourselves from the event and clear the list
            CompositionTarget.Rendering -= DoWorkOnRender;
            List<Action> work = workItems;
            workItems = null;

            foreach (Action action in work)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    
                    if (Debugger.IsAttached)
                        Debugger.Break();

                    Debug.WriteLine("Exception while doing work for " + action.Method.Name + ". " + ex.Message);
                }
            }
        }


    }
}
