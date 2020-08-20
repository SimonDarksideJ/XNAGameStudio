#region File Description
//-----------------------------------------------------------------------------
// Map.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using PathfindingData;
#endregion

namespace Pathfinding
{
    #region Map Tile Type Enum
    public enum MapTileType
    {
        MapEmpty,
        MapBarrier,
        MapStart,
        MapExit
    }
    #endregion

    public class Map
    {
        #region Fields

        // Draw data
        private Texture2D tileTexture;
        private Vector2 tileSquareCenter;
        private Texture2D dotTexture;
        private Vector2 dotTextureCenter;
        private Texture2D barrierTexture;
        private Color tileColor1 = Color.Navy;
        private Color tileColor2 = Color.LightBlue;
        private Color startColor = Color.Green;
        private Color exitColor = Color.Red;

        // Map data
        private List<MapData> maps;
        private MapTileType[,] mapTiles;
        private int currentMap;
        private int numberColumns;
        private int numberRows;

        #endregion

        #region Properties

        /// <summary>
        /// The height/width of a tile square
        /// </summary>
        public float TileSize
        {
            get { return tileSize; }
        }
        private float tileSize;

        /// <summary>
        /// The Draw scale as a % of TileSize
        /// </summary>
        public float Scale
        {
            get { return scale; }
        }
        private float scale;

        /// <summary>
        /// Start positon on the Map
        /// </summary>
        public Point StartTile
        {
            get { return startTile; }
        }
        private Point startTile;

        /// <summary>
        /// End position in the Map
        /// </summary>
        public Point EndTile
        {
            get { return endTile; }
        }
        private Point endTile;

        /// <summary>
        /// Set: reload Map data, Get: has the Map data changed
        /// </summary>
        public bool MapReload
        {
            get { return mapReload; }
            set { mapReload = value; }
        }
        private bool mapReload;

        #endregion

        #region Initialization

        /// <summary>
        /// Load Draw textures and Map data
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            tileTexture = content.Load<Texture2D>("whiteTile");
            barrierTexture = content.Load<Texture2D>("barrier");
            dotTexture = content.Load<Texture2D>("dot");

            dotTextureCenter = new Vector2(dotTexture.Width / 2, dotTexture.Height / 2);

            maps = new List<MapData>();
            maps.Add(content.Load<MapData>("map1"));
            maps.Add(content.Load<MapData>("map2"));
            maps.Add(content.Load<MapData>("map3"));
            maps.Add(content.Load<MapData>("map4"));

            ReloadMap();

            mapReload = true;
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw the map and all it's elements
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // These two loops go through each tile in the map, starting at the upper
            // left and going to the upper right, then repeating for the next row of 
            // tiles until the end
            for (int i = 0; i < numberRows; i++)
            {
                for (int j = 0; j < numberColumns; j++)
                {
                    // Get the screen coordinates of the tile
                    Vector2 tilePosition = MapToWorld(j, i, false);

                    // Alternate between the 2 tile colors to create a checker pattern
                    Color currentColor = (i+j)%2==1?tileColor1:tileColor2;
                    
                    // Draw the tile
                    spriteBatch.Draw(tileTexture, tilePosition,null,currentColor,0f,
                        Vector2.Zero,scale,SpriteEffects.None,0f);

                    // If the current tile is a type with a special draw element, the 
                    //start location, the end location or a barrier, then draw that.
                    switch (mapTiles[j, i])
                    {
                        case MapTileType.MapBarrier:
                            spriteBatch.Draw(
                                barrierTexture, tilePosition, null, Color.White,
                                0f, Vector2.Zero, scale, SpriteEffects.None, .25f);
                            break;
                        case MapTileType.MapStart:
                            spriteBatch.Draw(
                                dotTexture, tilePosition + tileSquareCenter, null,
                                startColor, 0f, dotTextureCenter, scale,
                                SpriteEffects.None, .25f);
                            break;
                        case MapTileType.MapExit:
                            spriteBatch.Draw(
                                dotTexture, tilePosition + tileSquareCenter, null,
                                exitColor, 0f, dotTextureCenter, scale,
                                SpriteEffects.None, .25f);
                            break;
                        default:
                            break;
                    }
                }
            }

            spriteBatch.End();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Translates a map tile location into a screen position
        /// </summary>
        /// <param name="column">column position(x)</param>
        /// <param name="row">row position(y)</param>
        /// <param name="centered">true: return the location of the center of the tile
        /// false: return the position of the upper-left corner of the tile</param>
        /// <returns>screen position</returns>
        public Vector2 MapToWorld(int column, int row, bool centered)
        {
            Vector2 screenPosition = new Vector2();

            if (InMap(column, row))
            {
                screenPosition.X = column * tileSize;
                screenPosition.Y = row * tileSize;
                if (centered)
                {
                    screenPosition += tileSquareCenter;
                }
            }
            else
            {
                screenPosition = Vector2.Zero;
            }
            return screenPosition;
        }

        /// <summary>
        /// Translates a map tile location into a screen position
        /// </summary>
        /// <param name="location">map location</param>
        /// <param name="centered">true: return the location of the center of the tile
        /// false: return the position of the upper-left corner of the tile</param>
        /// <returns>screen position</returns>
        public Vector2 MapToWorld(Point location, bool centered)
        {
            Vector2 screenPosition = new Vector2();

            if (InMap(location.X, location.Y))
            {
                screenPosition.X = location.X * tileSize;
                screenPosition.Y = location.Y * tileSize;
                if (centered)
                {
                    screenPosition += tileSquareCenter;
                }
            }
            else
            {
                screenPosition = Vector2.Zero;
            }
            return screenPosition;
        }

        /// <summary>
        /// Returns true if the given map location exists
        /// </summary>
        /// <param name="column">column position(x)</param>
        /// <param name="row">row position(y)</param>
        private bool InMap(int column, int row)
        {
            return (row >= 0 && row < numberRows &&
                column >= 0 && column < numberColumns);
        }

        /// <summary>
        /// Returns true if the given map location exists and is not 
        /// blocked by a barrier
        /// </summary>
        /// <param name="column">column position(x)</param>
        /// <param name="row">row position(y)</param>
        private bool IsOpen(int column, int row)
        {
            return InMap(column, row) && mapTiles[column, row] != MapTileType.MapBarrier;
        }

        /// <summary>
        /// Enumerate all the map locations that can be entered from the given 
        /// map location
        /// </summary>
        public IEnumerable<Point> OpenMapTiles(Point mapLoc)
        {
            if (IsOpen(mapLoc.X, mapLoc.Y + 1))
                yield return new Point(mapLoc.X, mapLoc.Y + 1);
            if (IsOpen(mapLoc.X, mapLoc.Y - 1))
                yield return new Point(mapLoc.X, mapLoc.Y - 1);
            if (IsOpen(mapLoc.X + 1, mapLoc.Y))
                yield return new Point(mapLoc.X + 1, mapLoc.Y);
            if (IsOpen(mapLoc.X - 1, mapLoc.Y))
                yield return new Point(mapLoc.X - 1, mapLoc.Y);
        }

        /// <summary>
        /// Create a viewport for the Map based on the passed in viewport and the 
        /// size of the map, scales the graphics to fit.
        /// </summary>
        /// <param name="viewport">Screen viewport</param>
        /// <returns>Map viewport</returns>
        public void UpdateMapViewport(Rectangle safeViewableArea)
        {            
            // This finds the largest sized tiles we can draw while still keeping 
            // everything in the given viewable area
            tileSize = Math.Min(safeViewableArea.Height / (float)numberRows,
                safeViewableArea.Width / (float)numberColumns);

            scale = tileSize / (float)tileTexture.Height;
            tileSquareCenter = new Vector2(tileSize / 2);           
        }

        /// <summary>
        /// Finds the minimum number of tiles it takes to move from Point A to 
        /// Point B if there are no barriers in the way
        /// </summary>
        /// <param name="pointA">Start position</param>
        /// <param name="pointB">End position</param>
        /// <returns>Distance in tiles</returns>
        public static int StepDistance(Point pointA, Point pointB)
        {
            int distanceX = Math.Abs(pointA.X - pointB.X);
            int distanceY = Math.Abs(pointA.Y - pointB.Y);

            return distanceX + distanceY;
        }

        /// <summary>
        /// Finds the minimum number of tiles it takes to move from the current 
        /// position to the end location on the Map if there are no barriers in 
        /// the way
        /// </summary>
        /// <param name="point">Current position</param>
        /// <returns>Distance to end in tiles</returns>
        public int StepDistanceToEnd(Point point)
        {
            return StepDistance(point, endTile);
        }

        /// <summary>
        /// Load the next map
        /// </summary>
        public void CycleMap()
        {
            currentMap = (currentMap + 1) % maps.Count;

            mapReload = true;
        }

        /// <summary>
        /// Reload map data
        /// </summary>
        public void ReloadMap()
        {
            // Set the map height and width
            numberColumns = maps[currentMap].NumberColumns;
            numberRows = maps[currentMap].NumberRows;
            
            // Recreate the tile array
            mapTiles = new MapTileType[maps[currentMap].NumberColumns, maps[currentMap].NumberRows];
            
            // Set the start
            startTile = maps[currentMap].Start;
            mapTiles[startTile.X, startTile.Y] = MapTileType.MapStart;
            
            // Set the end
            endTile = maps[currentMap].End;
            mapTiles[endTile.X, endTile.Y] = MapTileType.MapExit;

            int x = 0;
            int y = 0;
            // Set the barriers
            for (int i = 0; i < maps[currentMap].Barriers.Count; i++)
            {
                x = maps[currentMap].Barriers[i].X;
                y = maps[currentMap].Barriers[i].Y;
                
                mapTiles[x, y] =  MapTileType.MapBarrier;
            }

            mapReload = false;
        }

        #endregion
    }
}
