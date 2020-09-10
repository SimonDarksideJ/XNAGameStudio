using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

using WindowsPhone.Recipes.Push.Server.Behaviors;
using WindowsPhone.Recipes.Push.Messasges;
using WindowsPhone.Recipes.Push.Server.Models;
using WindowsPhone.Recipes.Push.Server.Services;

namespace WindowsPhone.Recipes.Push.Server.ViewModels
{
    /// <summary>
    /// Represents the Custom Tile Image push notification pattern.
    /// </summary>
    /// <remarks>
    /// Send a tile update with a dynamically created image located in the remote server (in our case localhost).
    /// The image will be dynamically generated upon each request. The tile Count and Title in the
    /// payload are optional.
    /// </remarks>
    [Export(typeof(PushPatternViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
    internal sealed class CustomTileImagePushPatternViewModel : PushPatternViewModel, IVisualHost
    {
        #region Constants

        /// <value>Tile image maximum width.</value>
        public const int TileImageWidth = 173;

        /// <value>Tile image maximum height.</value>
        public const int TileImageHeight = 173;

        /// <value>Tile image dpi.</value>
        public const int TileImageDpi = 96;        

        #endregion

        #region Fields

        /// <value>Collection of brushes for choosing text message color.</value>
        private Brush[] _textColors;

        /// <value>Selected tile image background.</value>
        private ImageSource _tileBackground;

        /// <value>Text for diplaying on the tile custom image.</value>
        private string _freeText;

        /// <value>Tile custom image text size.</value>
        private double _textSize;

        #endregion

        #region Properties

        [Import]
        private ImageService ImageService { get; set; }

        /// <summary>
        /// Gets a list of brushes for choosing text message color.
        /// </summary>
        public Brush[] TextColors
        {
            get
            {
                if (_textColors == null)
                {
                    _textColors = (from property in typeof(Brushes).GetProperties(BindingFlags.Static | BindingFlags.Public)
                                   select (Brush)property.GetValue(null, null)).ToArray();
                }

                return _textColors;
            }
        }

        /// <summary>
        /// Gets or sets the visual element used for generating a custom image from.
        /// </summary>
        public Visual Visual
        {
            get;
            set;
        }

        /// <summary>
        /// Get or sets the prefered tile background image source.
        /// </summary>
        public ImageSource TileBackground
        {
            get { return _tileBackground; }

            set
            {
                if (_tileBackground != value)
                {
                    _tileBackground = value;
                    NotifyPropertyChanged("TileBackground");
                }
            }
        }

        /// <summary>
        /// Gets or sets a text to be displayed on the tile generated image.
        /// </summary>
        public string FreeText
        {
            get { return _freeText; }

            set
            {
                if (_freeText != value)
                {
                    _freeText = value;
                    NotifyPropertyChanged("FreeText");
                }
            }
        }

        /// <summary>
        /// Gets or sets the text size of the tile generated image.
        /// </summary>
        public double TextSize
        {
            get { return _textSize; }

            set
            {
                if (_textSize != value)
                {
                    _textSize = value;
                    NotifyPropertyChanged("TextSize");
                }
            }
        }               

        #endregion

        #region Commands

        /// <summary>
        /// Gets the command for picking a tile background image.
        /// </summary>
        public ICommand PickImageCommand { get; private set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize new instance of this type with defaults.
        /// </summary>
        public CustomTileImagePushPatternViewModel()
        {
            InitializeDefaults();

            PickImageCommand = new RelayCommand(
                p =>
                {
                    // On execution, open file dialog for picking tile background image.
                    var openDialog = new OpenFileDialog
                    {
                        Title = "Tile Background",
                        Filter = "Jpeg|*.jpg|Bmp|*.bmp",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                        Multiselect = false
                    };

                    if (openDialog.ShowDialog() == true)
                    {
                        TileBackground = new BitmapImage(new Uri(openDialog.FileName));
                    }
                });
        }

        #endregion

        #region Protected

        protected override void OnActivated()
        {
            base.OnActivated();

            // Register to the ImageService.ImageRequest event. This event is raised
            // whenever ImageService.GetTileImage is called.
            ImageService.ImageRequest += Service_ImageRequest;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            ImageService.ImageRequest -= Service_ImageRequest;
        }

        protected override void OnSend()
        {
            // Starts by sending a tile notification to all relvant subscribers.
            // This tile notification updates the tile with custom image.
            var tileMsg = new TilePushNotificationMessage(MessageSendPriority.High)
            {
                Count = Count,
                Title = Title
            };

            foreach (var subscriber in PushService.Subscribers)
            {
                // Set the tile background image uri with the address of the ImageService.GetTileImage,
                // REST service, using current subscriber channel uri as a parameter to bo sent to the service.
                tileMsg.BackgroundImageUri = new Uri(string.Format(ImageService.GetTileImageService, string.Empty));
                tileMsg.SendAsync(subscriber.ChannelUri, Log, Log);
            }
        }

        #endregion

        #region Privates

        private void Service_ImageRequest(object sender, Services.ImageRequestEventArgs e)
        {
            // This event is raised by our local push-service as result of
            // the tile msg we've sent to each subscriber. This is the time
            // to pick the right tile image for the subscriber.
            if (Visual != null)
            {
                RenderImage(Visual, e.ImageStream);
            }
        }

        private static void RenderImage(Visual visual, Stream stream)
        {
            // The next lines of code uses WPF to dynamically create an image.
            // In a real server we shouldn't use WPF, hence a 3rd party image generator library is required.
            var renderer = new RenderTargetBitmap(TileImageWidth, TileImageHeight, TileImageDpi, TileImageDpi, PixelFormats.Pbgra32);
            renderer.Render(visual);

            var pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderer));
            pngEncoder.Save(stream);
        }

        private void InitializeDefaults()
        {
            DisplayName = "Custom Tile";
            Description = "Send a tile update with a dynamically created image located in the remote server (in our case localhost). The image will be dynamically generated upon each request. The tile Count and Title in the payload are optional.";
            TextSize = 20;
        }

        #endregion
    }
}
