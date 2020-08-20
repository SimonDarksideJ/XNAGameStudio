#region File Description
//-----------------------------------------------------------------------------
// BingMapsTiles.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Net;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.IO;


#endregion

namespace BingMaps
{
    #region Supporting Types


    /// <summary>
    /// Used to denote the cache's current activity.
    /// </summary>
    enum CachePhase
    {
        Idle,
        CancellingRequests,
        InitializingTiles,        
        Disposing
    }


    #endregion

    /// <summary>
    /// Manages a map tile cache. The class does not only manage a cache of map tiles but is also responsible for
    /// acquiring them from the Bing REST service.
    /// </summary>
    class BingMapsTiles : IDisposable
    {
        #region Fileds, Properties and Indexer


        /// <summary>
        /// A plane of 5x5 (assuming <see cref="ActiveTilePlaneSize"/> is 5) tiles which will be loaded to memory 
        /// for fast interaction. The center of the screen will point to a location inside the tile at [2, 2]. 
        /// [0, 0] is the top-left tile, while [4, 4] is the bottom-right tile.
        /// </summary>        
        TileInformation[,] activeTilePlane;

        Vector2 tileDimensions;
        Rectangle screenBounds;

        public const int MinZoomLevel = 1;
        public const int MaxZoomLevel = 23;

        CachePhase state = CachePhase.Idle;

        int pendingRequestCount = 0;

        GeoCoordinate centerGeoCoordinate;

        Texture2D unavailableImage;

        /// <summary>
        /// Returns the tile information for the specified coordinates on the active tile plane.
        /// </summary>
        /// <param name="x">X-coordinate of the desired tile. Must be between 0 and 4 inclusive.</param>
        /// <param name="y">Y-coordinate of the desired tile. Must be between 0 and 4 inclusive.</param>
        /// <returns>Tile information of the specified tile.</returns>
        /// <remarks>The possible values supplied for <paramref name="x"/> and <paramref name="y"/> are under the
        /// assumption that <see cref="ActiveTilePlaneSize"/> is 5.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">Either x or y are out of bounds.</exception>
        public TileInformation this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= ActiveTilePlaneSize)
                {
                    throw new ArgumentOutOfRangeException("x", String.Format("Valid values are 0 to {0} inclusive",
                        ActiveTilePlaneSize - 1));
                }

                if (y < 0 || y >= ActiveTilePlaneSize)
                {
                    throw new ArgumentOutOfRangeException("y", String.Format("Valid values are 0 to {0} inclusive",
                        ActiveTilePlaneSize - 1));
                }

                return activeTilePlane[x, y];
            }
        }

        int zoomLevel;

        /// <summary>
        /// The zoom level used to display the map tiles
        /// </summary>
        public int ZoomLevel
        {
            get
            {
                return zoomLevel;
            }
            set
            {
                if (value < MinZoomLevel || value > MaxZoomLevel)
                {
                    throw new ArgumentException(String.Format("Valid values are {0} to {1} inclusive",
                        MinZoomLevel, MaxZoomLevel));
                }

                zoomLevel = value;
            }
        }

        public string BingMapKey { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
        public BingMapsViewType ViewType { get; set; }

        /// <summary>
        /// The X and Y index of the center tile in the active tile plane
        /// </summary>
        private int PlaneCenterIndex
        {
            get
            {
                return (ActiveTilePlaneSize - 1) / 2;
            }
        }

        /// <summary>
        /// The size of both of the active tile plane's dimensions.
        /// </summary>
        public int ActiveTilePlaneSize { get; private set; }

        /// <summary>
        /// The amount of bytes to pre-allocate for image buffers.
        /// </summary>
        public const int InitialImageBufferSize = 150000;


        #endregion

        #region Initializations


        /// <summary>
        /// Initialize new Bing Maps tiles object.
        /// </summary>
        /// <param name="bingMapKey">The key used to access the Bing maps service.</param>        
        /// <param name="unavailableImage">The image to display in tiles for which an actual image is 
        /// unavailable.</param>
        /// <param name="spriteBatch">A sprite batch that can be used to draw to the display.</param>
        /// <param name="tileDimensions">The dimensions of a map tile. These will be used as the map dimensions
        /// for REST service requests.</param>
        /// <param name="planeDimension">The size of the active tile plane. Must be an odd number and 
        /// larger than 0.</param>
        public BingMapsTiles(string bingMapKey, Texture2D unavailableImage, SpriteBatch spriteBatch,
            Vector2 tileDimensions, int planeDimension)
        {
            if (planeDimension % 2 != 1 || planeDimension < 1)
            {
                throw new ArgumentOutOfRangeException(
                    "planeDimension", "Plane dimension must be an odd positive number.");
            }

            ActiveTilePlaneSize = planeDimension;

            activeTilePlane = new TileInformation[ActiveTilePlaneSize, ActiveTilePlaneSize];

            this.tileDimensions = tileDimensions;
            BingMapKey = bingMapKey;
            SpriteBatch = spriteBatch;
            screenBounds = spriteBatch.GraphicsDevice.Viewport.Bounds;
            this.unavailableImage = unavailableImage;
            zoomLevel = (MaxZoomLevel + MinZoomLevel) / 2;
        }

        /// <summary>
        /// Initializes the active tiles by requesting the images required in order to center the display at the
        /// specified geo-coordinate. If there are ongoing image requests, they will be cancelled first.
        /// </summary>
        /// <param name="centerTileKey">Geo-coordinate which will serve as the center of the .</param>
        public void InitializeActiveTilePlane(GeoCoordinate centerGeocoordinate)
        {
            this.centerGeoCoordinate = centerGeocoordinate;

            // If we have pending requests at this point, we must wait for them to be cancelled
            if (pendingRequestCount != 0)
            {
                state = CachePhase.CancellingRequests;
                CancelActiveRequestsAndResetImages();
            }
            else
            {
                state = CachePhase.InitializingTiles;
                GetActivePlaneImages(centerGeocoordinate);
            }            
        }        


        #endregion               

        #region Private Methods


        /// <summary>
        /// Cancels all currently ongoing tile image requests, and disposes of all current tile images.
        /// </summary>
        private void CancelActiveRequestsAndResetImages()
        {
            for (int xIndex = 0; xIndex < ActiveTilePlaneSize; xIndex++)
            {
                for (int yIndex = 0; yIndex < ActiveTilePlaneSize; yIndex++)
                {
                    TileInformation cellInformation = activeTilePlane[xIndex, yIndex];

                    if (cellInformation != null)
                    {
                        cellInformation.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the active tile plane by requesting images centered at the specified geo-coordinate.
        /// </summary>        
        /// <param name="centerCoordinate">The geo-coordinate which will serve as the center of the 
        /// middle tile.</param>
        private void GetActivePlaneImages(GeoCoordinate centerCoordinate)
        {
            Vector2 centerPixelXY = TileSystem.LatLongToPixelXY(centerCoordinate, zoomLevel);

            int planeCenterIndex = PlaneCenterIndex;

            for (int xIndex = 0; xIndex < ActiveTilePlaneSize; xIndex++)
            {
                int xDelta = xIndex - planeCenterIndex;

                for (int yIndex = 0; yIndex < ActiveTilePlaneSize; yIndex++)
                {
                    int yDelta = yIndex - planeCenterIndex;

                    TileInformation cellInformation = activeTilePlane[xIndex, yIndex];

                    // Initialize or clean the active tile cube cell
                    if (cellInformation == null)
                    {

                        cellInformation = new TileInformation(TileServerRequestCompleted);
                        activeTilePlane[xIndex, yIndex] = cellInformation;
                    }
                    else
                    {
                        cellInformation.Dispose();
                    }

                    // Calculate the center geo-coordinate for the current tile
                    Vector2 tileCenterPixelXY = centerPixelXY + tileDimensions * new Vector2(xDelta, yDelta);
                    GeoCoordinate tileCenterGeoCoordinate;

                    try
                    {
                        tileCenterGeoCoordinate = TileSystem.PixelXYToLatLong(tileCenterPixelXY, zoomLevel);
                        GetImageFromServer(cellInformation, tileCenterGeoCoordinate, zoomLevel, ViewType);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        cellInformation.Image = unavailableImage;
                    }
                }
            }
        }        

        /// <summary>
        /// Moves to the next phase of action when a previous phase completes.
        /// </summary>        
        private void MoveToNextPhase()
        {
            switch (state)
            {
                case CachePhase.Idle:
                    break;
                case CachePhase.CancellingRequests:
                    state = CachePhase.InitializingTiles;
                    GetActivePlaneImages(centerGeoCoordinate);
                    break;
                case CachePhase.InitializingTiles:
                    state = CachePhase.Idle;
                    break;
                case CachePhase.Disposing:
                    // Nothing to do here
                    break;
                default:
                    break;
            }
        }       

        /// <summary>
        /// Asynchronously gets a tile image from the Bing maps REST service.
        /// </summary>
        /// <param name="requestInformation">The tile information which is to handle the request and receive the 
        /// image.</param>        
        /// <param name="centerCoordinate">The geo-coordinate which should serve as the image center.</param>
        /// <param name="zoomLevel">The desired image zoom level.</param>
        /// <param name="viewType">The desired image view type.</param>
        void GetImageFromServer(TileInformation requestInformation, GeoCoordinate centerCoordinate, int zoomLevel,
            BingMapsViewType viewType)
        {
            // Build the request URI according to the parameters
            string requestUri = string.Format(
                "http://dev.virtualearth.net/REST/V1/Imagery/Map/{4}/{0},{1}/{2}?mapSize={5},{6}&key={3}",
                centerCoordinate.Latitude, centerCoordinate.Longitude, zoomLevel, BingMapKey, viewType, 
                (int)tileDimensions.X, (int)tileDimensions.Y);

            // Launch the request
            requestInformation.RequestImageAync(new Uri(requestUri, UriKind.Absolute));

            pendingRequestCount++;
        }

        /// <summary>
        /// Place images received from the REST service in the proper place in the active tile cube, and save them to
        /// the cache.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TileServerRequestCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            pendingRequestCount--;

            TileInformation requestInformation = (TileInformation)e.UserState;

            // Do not handle the event if the request was cancelled
            if (requestInformation.IsRequestCancelled)
            {
                requestInformation.MarkImageRequestCancelled();

                if (pendingRequestCount == 0)
                {
                    MoveToNextPhase();
                }

                return;
            }

            bool imageAvailable = e.Error == null ? true : false;

            // Clean the old image, if any
            requestInformation.Dispose(true);

            try
            {
                if (imageAvailable == true)
                {
                    SetTileImage(e, requestInformation);
                }
            }
            catch (Exception)
            {
                imageAvailable = false;
            }

            if (imageAvailable == false)
            {
                requestInformation.Image = unavailableImage;                
            }

            requestInformation.MarkImageRequestCompleted();

            // If all asynchronous calls returned, we can move to the next phase
            if (pendingRequestCount == 0)
            {
                MoveToNextPhase();
            }
        }

        /// <summary>
        /// Sets the image from the image stream contained in the supplied asynchronous as the image for the tile 
        /// represented by the supplied tile information. The image stream will be closed.
        /// </summary>
        /// <param name="e">Asynchronous read result which contains the image stream and is free of errors.</param>
        /// <param name="tileInformation">Tile information where the image is to be set.</param>
        private void SetTileImage(OpenReadCompletedEventArgs e, TileInformation tileInformation)
        {
            // Read the image from the stream
            Texture2D image = Texture2D.FromStream(SpriteBatch.GraphicsDevice, e.Result);

            e.Result.Seek(0, SeekOrigin.Begin);

            int imageByteCount = (int)e.Result.Length;

            byte[] imageBuffer = tileInformation.AsyncImageBuffer;

            // Resize the image buffer if it is too small
            if (imageByteCount > imageBuffer.Length)
            {
                Array.Resize<byte>(ref imageBuffer, imageByteCount);
            }

            e.Result.Read(imageBuffer, 0, imageByteCount);

            e.Result.Close();

            tileInformation.Image = image;            
        }                


        #endregion

        /// <summary>
        /// Delete all files the cache create and clear the active tile cube.
        /// </summary>
        public void Dispose()
        {
            for (int xIndex = 0; xIndex < ActiveTilePlaneSize; xIndex++)
            {
                for (int yIndex = 0; yIndex < ActiveTilePlaneSize; yIndex++)
                {
                    activeTilePlane[xIndex, yIndex].Dispose();
                }
            }         
        }       
    }
}
