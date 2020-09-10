#region File Description
//-----------------------------------------------------------------------------
// TileInformation.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Net;
using System.IO;
using System.Device.Location;
using Microsoft.Xna.Framework;


#endregion


namespace BingMaps
{
    /// <summary>
    /// Contains information about a single map tile.
    /// </summary>
    class TileInformation : IDisposable
    {
        #region Fields Properties

        Texture2D image;
        Vector2 imageSize;
        bool isPerformingRequest = false;
        byte[] imageBuffer;       

        /// <summary>
        /// Texture which contains the image representing the tile.
        /// </summary>
        public Texture2D Image 
        { 
            get
            {
                return image;
            }
            set 
            {
                image = value;
                imageSize = value == null ? Vector2.Zero : new Vector2(value.Width, value.Height);
            }
        }

        /// <summary>
        /// A buffer used to support asynchronous image operations.
        /// </summary>
        public byte[] AsyncImageBuffer
        {
            get
            {
                return imageBuffer;
            }
        }        

        /// <summary>
        /// Web client to use for retrieving the tile's image from the Bing maps REST service.
        /// </summary>
        private WebClient RequestClient { get; set; }        

        /// <summary>
        /// Whether the currently pending web request to get the represented tile's image was cancelled.
        /// </summary>
        public bool IsRequestCancelled { get; private set; }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new tile information instance.
        /// </summary>
        /// <param name="readCompleteHandler">Handler to call when read operations initiated by 
        /// <see cref="RequestImageAync"/> are completed. Make sure that the handler calls 
        /// <see cref="MarkImageRequestCompleted"/> or <see cref="MarkImageRequestCancelled"/> once it finishes 
        /// handling the request.</param>
        public TileInformation(OpenReadCompletedEventHandler readCompleteHandler)
        {
            RequestClient = new WebClient();
            RequestClient.OpenReadCompleted += readCompleteHandler;
            IsRequestCancelled = false;
            imageBuffer = new byte[BingMapsTiles.InitialImageBufferSize];
        }


        #endregion

        #region Public Methods
        
        
        /// <summary>
        /// Initiates an asynchronous web request to get the represented tile's image.
        /// </summary>
        /// <param name="imageUri">Uri from which the image is to be retrieved.</param>
        /// <remarks>The object itself will be supplied as the user token for the asynchronous operation.</remarks>
        /// <exception cref="System.InvalidOperationException">There is already a request in progress.</exception>
        public void RequestImageAync(Uri imageUri)
        {
            if (isPerformingRequest)
            {
                throw new InvalidOperationException("There is already an ongoing web request.");
            }

            isPerformingRequest = true;
            RequestClient.OpenReadAsync(imageUri, this);
        }

        /// <summary>
        /// Cancel the currently ongoing web request for the tile's image.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">There is no request in progress.</exception>
        public void CancelImageRequest()
        {
            if (!isPerformingRequest)
            {
                throw new InvalidOperationException("There is no request to cancel.");
            }

            IsRequestCancelled = true;

            RequestClient.CancelAsync();            
        }

        /// <summary>
        /// Denotes that the current image web request has been successfully cancelled.
        /// </summary>
        public void MarkImageRequestCancelled()
        {
            isPerformingRequest = false;
            IsRequestCancelled = false;
        }

        /// <summary>
        /// Denotes that the current image web request has been successfully completed.
        /// </summary>
        public void MarkImageRequestCompleted()
        {
            isPerformingRequest = false;

            if (IsRequestCancelled)
            {                
                throw new InvalidOperationException("Cannot mark a cancelled request as completed.");                
            }            
        }        


        #endregion
        

        #region IDisposable Members and Related Mehtods


        /// <summary>
        /// Disposes the object's <see cref="Image"/> and stops any ongoing asynchronous requests.
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the object's <see cref="Image"/> and potentially stops any ongoing asynchronous requests.
        /// </summary>
        /// <param name="imageOnly">True to only clean the tile's image, false to cancel ongoing asynchronous
        /// requests as well.</param>
        public void Dispose(bool imageOnly)
        {
            if (Image != null)
            {
                Image.Dispose();
                Image = null;
            }            

            if (imageOnly == true)
            {
                return;
            }

            if (isPerformingRequest && !IsRequestCancelled)
            {
                CancelImageRequest();
            }
        }


        #endregion
    }
}
