using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ComponentModel.Composition;
using System.IO;

namespace WindowsPhone.Recipes.Push.Server.Services
{
    /// <summary>
    /// Represents a tile image REST service.
    /// </summary>
    [Export, PartCreationPolicy(CreationPolicy.Shared), ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class ImageService : IImageService
    {
        #region Constants

        /// <value>Url of the GetTileImage REST service.</value>
        public const string GetTileImageService = "http://localhost:8000/ImageService/GetTileImage?parameter={0}";

        #endregion

        #region Fields        

        private ServiceHost _serviceHost;
        
        #endregion

        #region Events

        /// <summary>
        /// Raise when dynamic image creation is requested.
        /// </summary>
        public event EventHandler<ImageRequestEventArgs> ImageRequest;

        #endregion

        #region Operations

        /// <summary>
        /// Host this service using WCF.
        /// </summary>
        public void Host()
        {
            _serviceHost = new ServiceHost(this);
            _serviceHost.Open();
        }

        /// <summary>
        /// Get a generated custom tile image stream for the given uri.
        /// </summary>
        /// <param name="parameter">The tile image request parameter.</param>
        /// <returns>A stream of the custom tile image generated.</returns>
        public Stream GetTileImage(string parameter)
        {
            if (ImageRequest != null)
            {
                var args = new ImageRequestEventArgs(parameter);
                ImageRequest(this, args);

                // Seek the stream back to the begining just in case.
                args.ImageStream.Seek(0, SeekOrigin.Begin);

                return args.ImageStream;
            }

            return Stream.Null;
        } 
        #endregion

        #region Privates Logic

        private void OnImageRequest(ImageRequestEventArgs args)
        {
            if (ImageRequest != null)
            {
                ImageRequest(this, args);
            }
        }

        #endregion
    }
}
