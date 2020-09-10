#region File Description
//-----------------------------------------------------------------------------
// PushPin.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.Device.Location;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace BingMaps
{
    /// <summary>
    /// A visual cue which may be placed on the map.
    /// </summary>
    class PushPin
    {
        #region Fields and Properties
        public GeoCoordinate Location { get; set; }
        public Guid ID { get; private set; }

        BingMapsViewer bingMapsViewer;

        Rectangle screenBounds;

        Texture2D texture;
        Vector2 position;
        #endregion

        /// <summary>
        /// Creates a new push-pin instance.
        /// </summary>
        /// <param name="bingMapsViewer">The associated <see cref="BingMapsViewer"/> that will draw the 
        /// pushpin.</param>
        /// <param name="location">The geo-coordinate where the push-pin should be placed.</param>
        /// <param name="texture">The texture used to render the push-pin.</param>
        public PushPin(BingMapsViewer bingMapsViewer, GeoCoordinate location, Texture2D texture)
        {
            this.ID = Guid.NewGuid();
            this.bingMapsViewer = bingMapsViewer;
            Location = location;
            this.texture = texture;
            screenBounds = bingMapsViewer.SpriteBatch.GraphicsDevice.Viewport.Bounds;
        }

        /// <summary>
        /// DrawTank the pushpin on the screen, if the screen contains the push pin.
        /// </summary>
        public void Draw()
        {
            // Gets the center of the map displayed
            Vector2 center = TileSystem.LatLongToPixelXY(bingMapsViewer.CenterGeoCoordinate, bingMapsViewer.ZoomLevel);

            // Gets the position of the push pin on the map
            position = TileSystem.LatLongToPixelXY(Location, bingMapsViewer.ZoomLevel);

            // Calculates the distance between them
            Vector2 placeOnScreen = (position - center) + BingMapsViewer.ScreenCenter + bingMapsViewer.Offset;

            // If the pushpin is within screen bounds, draw it
            if (screenBounds.Contains((int)placeOnScreen.X, (int)placeOnScreen.Y))
            {
                placeOnScreen.X -= texture.Width / 2;
                placeOnScreen.Y -= texture.Height;
                bingMapsViewer.SpriteBatch.Draw(texture, placeOnScreen, Color.White);
            }
        }
    }
}
