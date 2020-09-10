using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Xml.Linq;

namespace WindowsPhone.Recipes.Push.Client.Controls
{
    public partial class PushSettingsControl : UserControl
    {        
        public PushSettingsControl()
        {
            DataContext = PushContext.Current;

            InitializeComponent();
        }
    }    
}
