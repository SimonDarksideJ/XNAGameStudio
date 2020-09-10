#region File Description
//-----------------------------------------------------------------------------
// TileSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Text;
using Microsoft.Xna.Framework;
using System.Device.Location;


#endregion

namespace BingMaps
{
    static class TileSystem
    {
        #region Constants


        private const double MinLatitude = -90D;
        private const double MaxLatitude = 90D;
        private const double MinLongitude = -180D;
        private const double MaxLongitude = 180D;


        #endregion

        /// <summary>
        /// Clips a number so that it falls within specified minimum and maximum values.
        /// </summary>
        /// <param name="n">The number to clip.</param>
        /// <param name="minValue">Minimum allowed value.</param>
        /// <param name="maxValue">Maximum allowed value.</param>
        /// <returns>The clipped value.</returns>
        public static double Clip(double n, double minValue, double maxValue)
        {
            while (n < minValue)
            {
                n += maxValue - minValue;
            }
            return (n + Math.Abs(minValue)) % (Math.Abs(minValue) + Math.Abs(maxValue)) - Math.Abs(minValue);
        }

        #region Public Methods


        /// <summary>
        /// Determines the map width and height (in pixels) at a specified level of detail.
        /// </summary>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail) to 23 (highest detail).</param>
        /// <returns>The map width and height in pixels.</returns>
        public static uint MapSize(int levelOfDetail)
        {
            return (uint)256 << levelOfDetail;
        }

        /// <summary>
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees) into pixel XY coordinates at 
        /// a specified level of detail.
        /// </summary>
        /// <param name="latitude">Latitude of the point, in degrees.</param>
        /// <param name="longitude">Longitude of the point, in degrees.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail) to 23 (highest detail).</param>
        /// <returns>The pixel XY coordinates corresponding to the supplied WGS-84 coordinates.</returns>
        /// <remarks>Pixel coordinates are relative to the entire map image.</remarks>
        public static Vector2 LatLongToPixelXY(double latitude, double longitude, int levelOfDetail)
        {
            if (latitude < MinLatitude || latitude > MaxLatitude)
            {
                throw new ArgumentOutOfRangeException("latitude");
            }

            if (longitude < MinLongitude || longitude > MaxLongitude)
            {
                throw new ArgumentOutOfRangeException("longitude");
            }            

            double x = (longitude + 180) / 360;
            double sinLatitude = Math.Sin(latitude * Math.PI / 180);
            double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            uint mapSize = MapSize(levelOfDetail);
            int pixelX = (int)Clip(x * mapSize + 0.5, 0, mapSize - 1);
            int pixelY = (int)Clip(y * mapSize + 0.5, 0, mapSize - 1);

            return new Vector2(pixelX, pixelY);
        }

        /// <summary>
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees) into pixel XY coordinates at 
        /// a specified level of detail.
        /// </summary>
        /// <param name="geoCoordinate">The WGS-84 coordinate to convert.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail) to 23 (highest detail).</param>
        /// <returns>The pixel XY coordinates corresponding to the supplied WGS-84 coordinates.</returns>
        /// <remarks>Pixel coordinates are relative to the entire map image.</remarks>
        public static Vector2 LatLongToPixelXY(GeoCoordinate geoCoordinate, int levelOfDetail)
        {
            return LatLongToPixelXY(geoCoordinate.Latitude, geoCoordinate.Longitude, levelOfDetail);
        }

        /// <summary>
        /// Converts a pixel from pixel XY coordinates at a specified level of detail into latitude/longitude WGS-84 
        /// coordinates (in degrees).
        /// </summary>
        /// <param name="pixelX">X coordinate of the point, in pixels.</param>
        /// <param name="pixelY">Y coordinates of the point, in pixels.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail) to 23 (highest detail).</param>
        /// <returns>The WGS-84 coordinates corresponding to the supplied pixel XY coordinates and level 
        /// of detail.</returns>
        /// <remarks>Pixel coordinates are relative to the entire map image.</remarks>
        public static GeoCoordinate PixelXYToLatLong(double pixelX, double pixelY, int levelOfDetail)
        {
            double mapSize = MapSize(levelOfDetail);

            // Make sure the pixel coordinates are not off the map image
            if (pixelX < 0 || pixelX > mapSize - 1)
            {
                throw new ArgumentOutOfRangeException("pixelX");
            }
            if (pixelY < 0 || pixelY > mapSize - 1)
            {
                throw new ArgumentOutOfRangeException("pixelY");
            }

            double x = (pixelX / mapSize) - 0.5;
            double y = 0.5 - (pixelY / mapSize);

            double latitude = Clip((double)(90D - 360D * Math.Atan(Math.Exp(-y * 2D * Math.PI)) / Math.PI),
                MinLatitude, MaxLatitude);
            double longtitude = 360D * x;
            longtitude = Clip(longtitude, MinLongitude, MaxLongitude);
            return new GeoCoordinate(latitude, longtitude);
        }

        /// <summary>
        /// Converts a pixel from pixel XY coordinates at a specified level of detail into latitude/longitude WGS-84 
        /// coordinates (in degrees).
        /// </summary>
        /// <param name="pixelPosition">Vector containing the coordinate of the point, in pixels.</param>
        /// <param name="levelOfDetail">Level of detail, from 1 (lowest detail) to 23 (highest detail).</param>
        /// <returns>The WGS-84 coordinates corresponding to the supplied pixel XY coordinates and level 
        /// of detail.</returns>
        /// <remarks>Pixel coordinates are relative to the entire map image.</remarks>
        public static GeoCoordinate PixelXYToLatLong(Vector2 pixelPosition, int levelOfDetail)
        {
            return PixelXYToLatLong(pixelPosition.X, pixelPosition.Y, levelOfDetail);
        }        


        #endregion
    }
}