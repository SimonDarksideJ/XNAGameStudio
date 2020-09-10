using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsPhone.Recipes.Push.Client.Views
{
    public class LoginEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public LoginEventArgs(Exception exception = null)
        {
            Exception = exception;
        }
    }
}
