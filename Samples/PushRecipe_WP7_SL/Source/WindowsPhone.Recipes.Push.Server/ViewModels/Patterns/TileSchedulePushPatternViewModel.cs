using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;

using WindowsPhone.Recipes.Push.Messasges;
using WindowsPhone.Recipes.Push.Server.Models;
using WindowsPhone.Recipes.Push.Server.Services;

namespace WindowsPhone.Recipes.Push.Server.ViewModels
{
    /// <summary>
    /// Represents the Tile Schedule push notification pattern.
    /// </summary>
    /// <remarks>
    /// This push pattern is passive. The user schedule a tile image update request,
    /// by using the Windows Phone API, and at the time, MPNS fetches the image using
    /// the uri provided with the request.
    /// </remarks>
    [Export(typeof(PushPatternViewModel)), PartCreationPolicy(CreationPolicy.Shared)]
    internal sealed class TileSchedulePushPatternViewModel : PushPatternViewModel
    {
        #region Fields

        private string _imageFileName;
        
        #endregion

        #region Properties

        [Import]
        private ImageService ImageService { get; set; }

        /// <summary>
        /// Gets or sets a image file name sent by the user.
        /// </summary>
        public string ImageFileName
        {
            get { return _imageFileName; }

            set
            {
                if (_imageFileName != value)
                {
                    _imageFileName = value;
                    NotifyPropertyChanged("ImageFileName");
                }
            }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize new instance of this type with defaults.
        /// </summary>
        public TileSchedulePushPatternViewModel()
        {
            InitializeDefaults();
        }

        #endregion

        #region Overrides

        protected override void OnActivated()
        {
            base.OnActivated();

            // Register to the PushService.TileUpdateRequest event. This event is raised
            // whenever a user asks to update its tile.
            PushService.TileUpdateRequest += PushService_TileUpdateRequest;

            // Register to the ImageService.ImageRequest event. This event is raised
            // whenever ImageService.GetTileImage is called.
            ImageService.ImageRequest += Service_ImageRequest;
        }        

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            PushService.TileUpdateRequest -= PushService_TileUpdateRequest;
            ImageService.ImageRequest -= Service_ImageRequest;
        }

        protected override void OnSend()
        {
            // Nothing to do here.
        }

        #endregion

        #region Privates

        private void PushService_TileUpdateRequest(object sender, TileUpdateRequestEventArgs e)
        {
            // Send a tile notification message to the relevant device.
            var tileMsg = new TilePushNotificationMessage(MessageSendPriority.High)
            {
                BackgroundImageUri = new Uri(string.Format(ImageService.GetTileImageService, e.Parameter))
            };

            tileMsg.SendAsync(e.ChannelUri, Log, Log);
        }

        private void Service_ImageRequest(object sender, Services.ImageRequestEventArgs e)
        {
            ImageFileName = e.Parameter;

            // This event is raised by our local push-service as result of
            // the tile msg we've sent to a subscriber. This is the time
            // to pick the right tile image for the subscriber.
            string imageFile = Path.Combine("Resources/TileImages/Numbers", e.Parameter);
            if (File.Exists(imageFile))
            {
                using (var reader = File.OpenRead(imageFile))
                {
                    byte[] imageBuffer = new byte[reader.Length];
                    int bytesRead = reader.Read(imageBuffer, 0, imageBuffer.Length);
                    e.ImageStream.Write(imageBuffer, 0, bytesRead);
                }
            }
        }

        private void InitializeDefaults()
        {
            DisplayName = "Tile Schedule";
            Description = "This push pattern is passive. The user schedule a tile image update request, by using the Windows Phone API, and at the time, MPNS fetches the image using the uri provided with the request.";
        }

        #endregion
    }
}
