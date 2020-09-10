#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Device.Location;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

#endregion


namespace BingMaps
{
    /// <summary>
    /// A component that uses Bing Maps routing information to navigate between a set of push-pins.
    /// </summary>
    class Tank : DrawableGameComponent
    {

        struct PushPinRouteMapper
        {
            public Guid PushPinID { get; set; }
            public LinkedList<GeoCoordinate> PushPinRoute { get; set; }
        }

        /// <summary>
        /// Enum that indicates in what method the tank will 
        /// request the routing from the service
        /// </summary>
        public enum BingRoutingMode
        {
            Driving,
            Walking
        }

        #region Fields

        LinkedList<PushPin> pushPins;

        GeoCoordinate currentGeoPosition;
        GeoCoordinate targetGeoPosition;
        SpriteBatch spriteBatch;
        BingMapsViewer viewer;
        RouteRender routeRender;
        Texture2D tankTexture;

        LinkedListNode<GeoCoordinate> currentRouteNode;

        LinkedListNode<PushPin> currentPushPin;

        List<PushPinRouteMapper> routes = new List<PushPinRouteMapper>();

        bool isParsingMovment = false;

        float speed = 1.5f;
        float rotation = 0f;

        BingRoutingMode routingMode;

        Vector2 tankOrigin;

        /// <summary>
        /// Routing mode used when calculating the tank's route.
        /// </summary>
        /// <remarks>When setting this property the entire route is recalculated.</remarks>
        public BingRoutingMode RoutingMode 
        {
            get
            {
                return routingMode;
            }
            set
            {
                routingMode = value;
                RecalculateRoute();
            }
        }

    

        /// <summary>
        /// Whether the route needs to be drawn or not.
        /// </summary>
        public bool ShouldDrawRoute { get; set; }

        /// <summary>
        /// Indicates whether the tank is in motion.
        /// </summary>
        public bool IsMoving 
        {
            get
            {
                return currentPushPin != null && targetGeoPosition != null;
            }
        }

        /// <summary>
        /// The tank's position in screen coordinates.
        /// </summary>
        public Vector2 ScreenPosition 
        {
            get
            {
                // Get the center of the map presented
                Vector2 center = TileSystem.LatLongToPixelXY(viewer.CenterGeoCoordinate, viewer.ZoomLevel);

                // Gets the position of the target on the map
                Vector2 positionOnTheMap = TileSystem.LatLongToPixelXY(currentGeoPosition, viewer.ZoomLevel);

                // Calculate the distance which gets the position on the screen
                Vector2 position = (positionOnTheMap - center) + BingMapsViewer.ScreenCenter +  viewer.Offset ;

                return new Vector2((int)position.X, 
                    (int)position.Y);
            }
        }

        /// <summary>
        /// The tank's on-screen bounds.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                Vector2 position = ScreenPosition;
                return new Rectangle((int)position.X - tankTexture.Width / 2, (int)position.Y - tankTexture.Height /2,
                    tankTexture.Width, tankTexture.Height);
            }
        }


        #endregion

        #region Initialization

        /// <summary>
        /// Creates new instance of the tank object.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="spriteBatch">A sprite batch that can be used to draw on the display.</param>
        /// <param name="tankTexture">The tank's texture.</param>
        /// <param name="viewer">Associated <see cref="BingMapsViewer"/> over which the tank navigates.</param>
        /// <param name="startUpPosition">The tank's initial on-screen position.</param>
        /// <param name="pushPins">List of push-pins that the tank needs to follow.</param>
        public Tank(Game game, SpriteBatch spriteBatch, Texture2D tankTexture, BingMapsViewer viewer,
            Vector2 startUpPosition, LinkedList<PushPin> pushPins)
            : base(game)
        {

            this.spriteBatch = spriteBatch;
            this.viewer = viewer;
            this.tankTexture = tankTexture;
            this.pushPins = pushPins;

            // Gets the center point of the map displayed
            Vector2 positionOnMap = TileSystem.LatLongToPixelXY(viewer.CenterGeoCoordinate, viewer.ZoomLevel)
                    - viewer.Offset;

            // Calculate the distance between the center of the screen and the tank's initial position
            Vector2 distance = startUpPosition - BingMapsViewer.ScreenCenter;
            Vector2 startUpPointOnMap = positionOnMap + distance + new Vector2(32, 32);

            // Gets the tank's initial geo-coordinate
            currentGeoPosition = TileSystem.PixelXYToLatLong(startUpPointOnMap, viewer.ZoomLevel);

            RoutingMode = BingRoutingMode.Driving;
            ShouldDrawRoute = false;

            routeRender = new RouteRender(game.GraphicsDevice);

            tankOrigin = new Vector2(tankTexture.Width / 2, tankTexture.Height / 2);
        }

        #endregion

        #region Updating

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Do nothing if there is no push-pin to reach
            if (currentPushPin == null)
            {
                return;
            }

            // Check if the current push-pin does not have a route leading to it
            if (!isParsingMovment && currentPushPin != null && pushPins.Contains(currentPushPin.Value)
                && !IsRouteContained(routes,currentPushPin.Value.ID))
            {
                // Gets the missing route from the server
                GetRouteFromServer(viewer.BingMapKey, currentGeoPosition, currentPushPin.Value.Location,
                    currentPushPin);
            }
            else
            {
                // Update the target geo-coordinate if there is none
                if (routes.Count > 0 && IsRouteContained(routes,currentPushPin.Value.ID)
                    && GetMapper(routes,currentPushPin.Value.ID).PushPinRoute.Last != currentRouteNode
                    && targetGeoPosition == null)
                {
                    LinkedList<GeoCoordinate> currentRoute = 
                        GetMapper(routes, currentPushPin.Value.ID).PushPinRoute;
                    if (currentRoute != null && currentRoute.Count > 0)
                    {
                        currentRouteNode = GetMapper(routes, currentPushPin.Value.ID).PushPinRoute.First;
                        targetGeoPosition = currentRouteNode.Value;
                    }
                }

                if (targetGeoPosition != null && currentGeoPosition != null)
                {
                    // Get the center of the map presented
                    Vector2 center = TileSystem.LatLongToPixelXY(viewer.CenterGeoCoordinate, viewer.ZoomLevel);

                    // Gets the position of the target on the map
                    Vector2 position = TileSystem.LatLongToPixelXY(targetGeoPosition, viewer.ZoomLevel);

                    // Calculate the distance which gets the position on the screen
                    Vector2 targetPosition = (position - center) + BingMapsViewer.ScreenCenter;

                    // Does the same to the current position of the tank
                    position = TileSystem.LatLongToPixelXY(currentGeoPosition, viewer.ZoomLevel);
                    Vector2 currentPosition = (position - center) + BingMapsViewer.ScreenCenter;

                    Vector2 distanceBetweenTargetAndCurrentPosition = targetPosition - currentPosition;

                    // If the tank has reached the target
                    if (((distanceBetweenTargetAndCurrentPosition.X <= 1 && 
                            distanceBetweenTargetAndCurrentPosition.X >= 0) ||
                         (distanceBetweenTargetAndCurrentPosition.X >= -1 && 
                            distanceBetweenTargetAndCurrentPosition.X <= 0)) &&
                        (distanceBetweenTargetAndCurrentPosition.Y <= 1 && 
                            distanceBetweenTargetAndCurrentPosition.Y >= 0) ||
                         (distanceBetweenTargetAndCurrentPosition.Y >= -1 && 
                            distanceBetweenTargetAndCurrentPosition.Y <= 0))
                    {
                        HandleTankReachedTargetPosition();
                        return;
                    }

                    currentPosition = HandleTankMovement(targetPosition, currentPosition);

                    // Converts the current screen position to geo coordinate
                    Vector2 dif = currentPosition - BingMapsViewer.ScreenCenter;
                    Vector2 PositionOnMap = center + dif;

                    currentGeoPosition = TileSystem.PixelXYToLatLong(PositionOnMap, viewer.ZoomLevel);
                }
            }
            base.Update(gameTime);
        }

        #endregion

        #region Drawing

        /// <summary>
        /// DrawTank the component on the screen.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public void DrawTank(GameTime gameTime)
        {
            if (ShouldDrawRoute)
            {
                DrawRoute();
            }

            spriteBatch.Draw(tankTexture, ScreenPosition, null, Color.White,rotation,tankOrigin,
                                 1.0f, SpriteEffects.None, 0f);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds new push pin to the tank's collection, and possibly retrieves a route to it from the server.
        /// </summary>
        /// <param name="pushPin">Push-pin to add.</param>
        public void AddPushPin(PushPin pushPin)
        {
            if (currentPushPin == null && pushPins.First.Value == pushPin)
            {
                currentPushPin = pushPins.First;
            }
            else
            {
                LinkedListNode<PushPin> pushPinNode = pushPins.Find(pushPin);
                GetRouteFromServer(viewer.BingMapKey, pushPinNode.Previous.Value.Location, pushPinNode.Value.Location,
                     pushPinNode);
            }

        }

        /// <summary>
        /// Deletes a push-pin from the tank's collection.
        /// </summary>
        /// <param name="pushPinToDelete">The push-pin to delete.</param>
        public void DeletePushPin(PushPin pushPinToDelete)
        {
            RecalculateRoute(); 
        }

        /// <summary>
        /// Clears the route of the tank
        /// </summary>
        public void ClearRoute()
        {
            routes.Clear();
            targetGeoPosition = null;
            currentPushPin = null;
            currentRouteNode = null;
        }

        /// <summary>
        /// Move the tank to a new location.
        /// </summary>
        /// <param name="newPosition">Screen position to place the tank at.</param>
        public void Move(Vector2 newPosition)
        {
            // Gets the center point of the map displayed
            Vector2 positionOnMap = TileSystem.LatLongToPixelXY(viewer.CenterGeoCoordinate, viewer.ZoomLevel)
                    - viewer.Offset;

            // Calculate the distance between the center of the screen and the tank's position
            Vector2 distance = newPosition - BingMapsViewer.ScreenCenter;
            Vector2 newPositionOnMap = positionOnMap + distance;

            // Gets the tank's geo-coordinate
            currentGeoPosition = TileSystem.PixelXYToLatLong(newPositionOnMap, viewer.ZoomLevel);

            routes.Clear();
            pushPins.Clear();
            targetGeoPosition = null;
            currentPushPin = null;
            currentRouteNode = null;
        }

        /// <summary>
        /// Move the tank to a new location.
        /// </summary>
        /// <param name="coord">Geo-coordinate to place the tank at.</param>
        public void Move(GeoCoordinate coord)
        {
            // Gets the tank's geo-coordinate
            currentGeoPosition = coord;

            routes.Clear();
            pushPins.Clear();
            targetGeoPosition = null;
            currentPushPin = null;
            currentRouteNode = null;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Callback for receiving routing information from the service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wcRoute_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                LinkedListNode<PushPin> node = e.UserState as LinkedListNode<PushPin>;

                // The response is formatted as XML
                XDocument doc = XDocument.Load(e.Result);

                // Using xml reader to read
                XmlReader reader = doc.CreateReader();

                if (!IsRouteContained(routes,node.Value.ID))
                {
                    routes.Add(new PushPinRouteMapper()
                    {
                        PushPinID = node.Value.ID,
                        PushPinRoute = new LinkedList<GeoCoordinate>()
                    });
                }

                if (GetMapper(routes,node.Value.ID).PushPinRoute.Count > 0)
                {
                    GetMapper(routes, node.Value.ID).PushPinRoute.Clear();
                }

                while (!reader.EOF)
                {
                    // Finds the relevant elements
                    FindElement(reader, "Point");

                    string latitude = GetValue(reader);
                    string longitude = GetValue(reader);
                    if (latitude != null && longitude != null)
                    {
                        // Add the coordinate to the route
                        GetMapper(routes, node.Value.ID).PushPinRoute.AddLast(
                            new GeoCoordinate(double.Parse(latitude), double.Parse(longitude)));
                    }
                }
                isParsingMovment = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handle the scenario where the tank has reached the target push pin.
        /// </summary>
        private void HandleTankReachedTargetPosition()
        {
            // Go the the next route
            if (IsRouteContained(routes,currentPushPin.Value.ID) && 
                GetMapper(routes, currentPushPin.Value.ID).PushPinRoute.Last != currentRouteNode)
            {
                currentRouteNode = currentRouteNode.Next;
                if (currentRouteNode != null)
                {
                    targetGeoPosition = currentRouteNode.Value;
                }
                else
                {
                    targetGeoPosition = null;
                }
            }
            // If there is no next route, try getting the next push pin
            else if (!isParsingMovment)
            {
                currentPushPin = currentPushPin.Next;
                pushPins.RemoveFirst();
                if (pushPins.Count == 0)
                {
                    routes.Clear();
                }
            }
        }

        /// <summary>
        /// Moves the tank part of the way from its current position to a specified target position.
        /// </summary>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="currentPosition">The tank's current position.</param>
        private Vector2  HandleTankMovement(Vector2 targetPosition, Vector2 currentPosition)
        {
            // Calculate the distance between the current position and the target position
            Vector2 direction = -(currentPosition - targetPosition);

            if (direction != Vector2.Zero)
            {
                // Calculate the rotation of the tank
                rotation = (float)Math.Atan2(direction.Y, direction.X);

                direction.Normalize();
            }

            currentPosition += (direction * speed);
            return currentPosition;
        }

        /// <summary>
        ///  Finds an element within an XML reader.
        /// </summary>
        /// <param name="reader">The XML reader to search for the element.</param>
        /// <param name="name">The name of the element to be found.</param>
        void FindElement(XmlReader reader, string name)
        {
            while (reader.Read())
            {
                if (reader.Name == name)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the next value from the XML reader as string
        /// </summary>
        /// <param name="reader">The XML reader</param>
        /// <returns>The Value</returns>
        string GetValue(XmlReader reader)
        {
            while (!reader.HasValue && !reader.EOF)
            {
                reader.Read();
            }

            // EOF = end of stream
            if (reader.EOF)
            {
                return null;
            }
            return reader.ReadContentAsString();
        }

        /// <summary>
        /// Send Request to the Bing service in order to get a route between two points.
        /// </summary>
        /// <param name="bingMapKey">Bing maps key.</param>
        /// <param name="sourceCoord">Source geo-coordinate.</param>
        /// <param name="TargetCoord">Target geo-coordinate.</param>
        /// <param name="node"></param>
        void GetRouteFromServer(string bingMapKey, GeoCoordinate sourceCoord, GeoCoordinate TargetCoord,
                LinkedListNode<PushPin> node)
        {
            isParsingMovment = true;
            StringBuilder wayPointParameters = new StringBuilder();

            // Creates the source position argument
            wayPointParameters.Append(string.Format("&wp.{0}={1},{2}", 0, sourceCoord.Latitude,
                    sourceCoord.Longitude));

            // Creates the target position argument
            wayPointParameters.Append(string.Format("&wp.{0}={1},{2}", 1, TargetCoord.Latitude,
                TargetCoord.Longitude));

            string requestUri = string.Format(
                 "http://dev.virtualearth.net/REST/V1/Routes/{2}?optmz=distance&output=xml&rpo=points&key={0}{1}",
                 bingMapKey, wayPointParameters,RoutingMode);

            // Send The request to the bing map routing service
            WebClient wcRoute = new WebClient();
            wcRoute.OpenReadCompleted += new OpenReadCompletedEventHandler(wcRoute_OpenReadCompleted);
            wcRoute.OpenReadAsync(new Uri(requestUri, UriKind.Absolute), node);
        }

        /// <summary>
        /// Draws the route which the tank is following.
        /// </summary>
        private void DrawRoute()
        {
            if (IsMoving)
            {
                // Gets the center of the map displayed
                Vector2 center = TileSystem.LatLongToPixelXY(viewer.CenterGeoCoordinate, viewer.ZoomLevel);

                // Gets the current position of the tank
                Vector2 position = TileSystem.LatLongToPixelXY(currentGeoPosition, viewer.ZoomLevel);

                Vector2 lastPosition = (position - center) + BingMapsViewer.ScreenCenter + viewer.Offset;
                
                foreach (PushPin pushPinNode in pushPins)
                {
                    if (IsRouteContained(routes,pushPinNode.ID))
                    {
                        LinkedList<GeoCoordinate> currentRoute = GetMapper(routes, pushPinNode.ID).PushPinRoute;

                        foreach (GeoCoordinate currentRouteSegment in GetMapper(routes,pushPinNode.ID).PushPinRoute)
                        {
                            if (pushPins.First.Value == pushPinNode)
                            {
                                if (currentRoute != null && !IsBefore(currentRoute, currentRouteNode,
                                        currentRoute.Find(currentRouteSegment)))
                                {
                                    continue;
                                }
                            }

                            // Gets the current position of the tank
                            position = TileSystem.LatLongToPixelXY(currentRouteSegment, viewer.ZoomLevel);

                            Vector2 placeOnScreen = (position - center) + BingMapsViewer.ScreenCenter + viewer.Offset;

                            if (lastPosition != Vector2.Zero)
                            {
                                routeRender.AddRouteLine(lastPosition, placeOnScreen, Color.Red);
                            }

                            lastPosition = placeOnScreen;

                        }
                    }
                }

                routeRender.EndRoute();
            }
    
        }

        /// <summary>
        /// The method checks if a certain node i before or after the source node.
        /// </summary>
        /// <param name="list">The list to go over.</param>
        /// <param name="source">The source node to check against</param>
        /// <param name="target">The target node</param>
        /// <returns></returns>
        private bool IsBefore(LinkedList<GeoCoordinate> list,LinkedListNode<GeoCoordinate> source,
            LinkedListNode<GeoCoordinate> target)
        {
            foreach (var currentNode in list)
            {
                if (currentNode == source.Value)
                {
                    return true;
                }
                if (currentNode == target.Value)
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Checks if certain ID(Push Pin id) is contained at the supplied list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool IsRouteContained(List<PushPinRouteMapper> list, Guid id)
        {
            foreach (PushPinRouteMapper currnetMapping in list)
            {
                if (currnetMapping.PushPinID == id)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets specific mapper from the list by ID(Push Pin id)
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private PushPinRouteMapper GetMapper(List<PushPinRouteMapper> list, Guid id)
        {
            foreach (PushPinRouteMapper currnetMapping in list)
            {
                if (currnetMapping.PushPinID == id)
                {
                    return currnetMapping;
                }
            }
            return new PushPinRouteMapper();
        }

        /// <summary>
        /// Deletes all the routes and recalculate them
        /// </summary>
        /// <param name="value"></param>
        private void RecalculateRoute()
        {
            targetGeoPosition = null;

            routes.Clear();
            foreach (PushPin pushPin in pushPins)
            {
                LinkedListNode<PushPin> pushPinNode = pushPins.Find(pushPin);

                // The first segment of the route begins at the tank's current position
                if (pushPinNode == pushPins.First)
                {
                    GetRouteFromServer(viewer.BingMapKey, currentGeoPosition, pushPin.Location, pushPinNode);
                    currentPushPin = pushPinNode;
                }
                else
                {
                    GetRouteFromServer(viewer.BingMapKey, pushPinNode.Previous.Value.Location,
                        pushPin.Location, pushPinNode);
                }
            }
        }

        #endregion
    }
}
