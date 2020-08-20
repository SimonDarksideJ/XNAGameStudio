#region File Description
//-----------------------------------------------------------------------------
// BingMapsViewer.cs
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
using System.Device.Location;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using System.Net;


#endregion

namespace BingMaps
{
    enum BingMapsViewType
    {
        Aerial,
        Road
    }

    class BingMapsViewer
    {
        #region Fields and Properties


        public string BingMapKey { get; private set; }
        
        public int ZoomLevel { get; private set; }        

        public GraphicsDevice GraphicsDevice { get; private set; }

        public SpriteBatch SpriteBatch { get; private set; }

        public BingMapsViewType ViewType { get; set; }

        public BingMapsTiles ActiveTiles { get; set; }
        
        // The geo position at the center of the current center tile
        GeoCoordinate centerGeoCoordinate;
        // Offset used for drawing the map tiles on the screen
        Vector2 offset = Vector2.Zero;

        Texture2D defaultImage;
        Texture2D unavailableImage;

        readonly Rectangle screenBounds;
        readonly Vector2 screenCenterVector;
        readonly Vector2 tileDimensions;
        readonly Vector2 maxOffsetAbs;


        #endregion

        #region Initializtion


        /// <summary>
        /// Initializes new Bing maps viewer.
        /// </summary>
        /// <param name="bingMapKey">The key used to access the Bing maps service.</param>
        /// <param name="defaultImage">The default image to show until actual image data is retrieved.</param>
        /// <param name="unavailableImage">The image to use for tiles where an image is not available.</param>
        /// <param name="location">The initial location to use.</param>
        /// <param name="zoomlevel">The zoom level (1 - 22).</param>
        /// <param name="gridSize">The size of the active tile grid around the desired location. For example, 
        /// specifying 3 will retrieve a 3x3 set of images around the desired location. This number must be
        /// positive and odd.</param>
        /// <param name="spriteBatch">A sprite batch that can be used to draw to the display.</param>        
        public BingMapsViewer(string bingMapKey, Texture2D defaultImage, Texture2D unavailableImage,
            GeoCoordinate location, int gridSize, int zoomlevel, SpriteBatch spriteBatch)
        {
            BingMapKey = bingMapKey;
            GraphicsDevice = spriteBatch.GraphicsDevice;
            SpriteBatch = spriteBatch;

            ZoomLevel = zoomlevel;

            screenBounds = GraphicsDevice.Viewport.Bounds;
            tileDimensions = new Vector2(screenBounds.Width, screenBounds.Height);            
            screenCenterVector = tileDimensions / 2;

            this.defaultImage = defaultImage;
            this.unavailableImage = unavailableImage;

            // Initialize the tile set object
            ActiveTiles = new BingMapsTiles(bingMapKey, unavailableImage, spriteBatch, tileDimensions, gridSize)
            {
                ZoomLevel = zoomlevel,
                ViewType = BingMapsViewType.Aerial
            };

            maxOffsetAbs = tileDimensions * ((ActiveTiles.ActiveTilePlaneSize - 1) / 2);            

            ViewType = BingMapsViewType.Aerial;            

            // Initialize the tile set with the appropriate tile images
            centerGeoCoordinate = location;
            ActiveTiles.InitializeActiveTilePlane(location);
        }


        #endregion

        #region Rendering


        /// <summary>
        /// Draw the map on the screen. Assumes that SpriteBatch.Begin has been called.
        /// </summary>
        public void Draw()
        {
            int centerIndex = (ActiveTiles.ActiveTilePlaneSize - 1) / 2;

            for (int i = 0; i < ActiveTiles.ActiveTilePlaneSize; i++)
            {
                for (int j = 0; j < ActiveTiles.ActiveTilePlaneSize; j++)
                {
                    // Add an offset to the drawn image depending on its position in the tile matrix
                    Vector2 extraOffset = new Vector2((i - centerIndex) * tileDimensions.X,
                        (j - centerIndex) * tileDimensions.Y);

                    Texture2D image = ActiveTiles[i, j].Image;
                    if (image == null)
                    {
                        image = defaultImage;
                    }

                    SpriteBatch.Draw(image, screenCenterVector + offset + extraOffset, null, Color.White, 0,
                        tileDimensions / 2f, 1, SpriteEffects.None, 0);
                }
            }
        }        


        #endregion

        #region Public Methods


        /// <summary>
        /// Refreshes the displayed images by getting them again from the REST services.
        /// </summary>
        public void RefreshImages()
        {
            ActiveTiles.ViewType = ViewType;
            ActiveTiles.InitializeActiveTilePlane(centerGeoCoordinate);
        }

        /// <summary>
        /// Centers the view on a specified geo-coordinate.
        /// </summary>
        /// <param name="location">Geo-coordinate to center the view on.</param>
        public void CenterOnLocation(GeoCoordinate location)
        {
            centerGeoCoordinate = location;
            offset = Vector2.Zero;
            ActiveTiles.InitializeActiveTilePlane(location);
        }        

        /// <summary>
        /// Move the map by a specified offset, but prevent a move that would transition to a tile that is off the
        /// active tile set.
        /// </summary>
        /// <param name="gestureOffset">The offset to move the map by.</param>
        public void MoveByOffset(Vector2 gestureOffset)
        {
            offset += gestureOffset;

            Vector2 fixOffset = Vector2.Zero;

            if (offset.X < -maxOffsetAbs.X || offset.X > maxOffsetAbs.X)                
            {
                fixOffset.X = -gestureOffset.X;
            }
            if (offset.Y < -maxOffsetAbs.Y || offset.Y > maxOffsetAbs.Y)
            {
                fixOffset.Y = -gestureOffset.Y;
            }

            offset += fixOffset;
        }          


        #endregion

 
    }
}
