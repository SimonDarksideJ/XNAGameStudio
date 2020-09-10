using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsPhone.Recipes.Push.Client
{
    public class PushContextErrorEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }

        public PushContextErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
