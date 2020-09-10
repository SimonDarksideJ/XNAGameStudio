using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using WindowsPhone.Recipes.Push.Server.Services;
using WindowsPhone.Recipes.Push.Server.ViewModels;

namespace WindowsPhone.Recipes.Push.Server
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public CompositionContainer Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Initialize MEF to export parts from current assembly.
            var catalog = new AssemblyCatalog(typeof(App).Assembly);
            Container = new CompositionContainer(catalog);
            Container.ComposeParts();

            // Create and show the main window where MainViewModel is the default source for data-binding.
            new MainWindow
            {
                DataContext = Container.GetExportedValue<MainViewModel>()

            }.Show();

            base.OnStartup(e);
        }        
    }
}
