using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowsPhone.Recipes.Push.Server.Behaviors
{
    public interface IVisualHost
    {
        Visual Visual { get; set; }
    }
}
