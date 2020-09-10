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

namespace WindowsPhone.Recipes.Push.Client.Controls
{
    public class ViewTransitions<T>
    {
        private class ViewTransition
        {
            public T To { get; set; }
            public Action Action { get; set; }
        }

        public T _viewState;
        private readonly Dictionary<T, ViewTransition> _transitions = new Dictionary<T, ViewTransition>();

        public ViewTransitions(T initialState)
        {
            _viewState = initialState;
        }

        public void AddTransition(T from, T to, Action action)
        {
            _transitions[from] = new ViewTransition { To = to, Action = action };
        }

        public void Transition()
        {
            var transition = _transitions[_viewState];
            _viewState = transition.To;
            transition.Action();
        }
    }
}
