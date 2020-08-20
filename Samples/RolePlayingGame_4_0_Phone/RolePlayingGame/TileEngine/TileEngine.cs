#region File Description
//-----------------------------------------------------------------------------
// PlayerPosition.cs
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
using RolePlayingGameData;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Static class for a tileable map
    /// </summary>
    static class TileEngine
    {
        #region Map


        /// <summary>
        /// The map being used by the tile engine.
        /// </summary>
        private static Map map = null;

        /// <summary>
        /// The map being used by the tile engine.
        /// </summary>
        public static Map Map
        {
            get { return map; }
        }
        

        /// <summary>
        /// The position of the outside 0,0 corner of the map, in pixels.
        /// </summary>
        private static Vector2 mapOriginPosition;


        /// <summary>
        /// Calculate the screen position of a given map location (in tiles).
        /// </summary>
        /// <param name="mapPosition">A map location, in tiles.</param>
        /// <returns>The current screen position of that location.</returns>
        public static Vector2 GetScreenPosition(Point mapPosition)
        {
            return new Vector2(
                mapOriginPosition.X + mapPosition.X * map.TileSize.X * ScaledVector2.DrawFactor,
                mapOriginPosition.Y + mapPosition.Y * map.TileSize.Y * ScaledVector2.DrawFactor);
        }

        /// <summary>
        /// Set the map in use by the tile engine.
        /// </summary>
        /// <param name="map">The new map for the tile engine.</param>
        /// <param name="portal">The portal the party is entering on, if any.</param>
        public static void SetMap(Map newMap, MapEntry<Portal> portalEntry)
        {
            // check the parameter
            if (newMap == null)
            {
                throw new ArgumentNullException("newMap");
            }

            // assign the new map
            map = newMap;

            // reset the map origin, which will be recalculate on the first update
            mapOriginPosition = Vector2.Zero;

            // move the party to its initial position
            if (portalEntry == null)
            {
                // no portal - use the spawn position
                partyLeaderPosition.TilePosition = map.SpawnMapPosition;
                partyLeaderPosition.TileOffset = Vector2.Zero;
                partyLeaderPosition.Direction = Direction.South;
            }
            else
            {
                // use the portal provided, which may include automatic movement
                partyLeaderPosition.TilePosition = portalEntry.MapPosition;
                partyLeaderPosition.TileOffset = Vector2.Zero;
                partyLeaderPosition.Direction = portalEntry.Direction;
                autoPartyLeaderMovement = Vector2.Multiply(
                    new Vector2(map.TileSize.X, map.TileSize.Y), new Vector2(
                    portalEntry.Content.LandingMapPosition.X - 
                        partyLeaderPosition.TilePosition.X,
                    portalEntry.Content.LandingMapPosition.Y - 
                        partyLeaderPosition.TilePosition.Y));
            }
        }


        #endregion


        #region Graphics Data


        /// <summary>
        /// The viewport that the tile engine is rendering within.
        /// </summary>
        private static Viewport viewport;

        /// <summary>
        /// The viewport that the tile engine is rendering within.
        /// </summary>
        public static Viewport Viewport
        {
            get { return viewport; }
            set 
            { 
                viewport = value;
                viewportCenter = new Vector2(
                    viewport.X + viewport.Width / 2f,
                    viewport.Y + viewport.Height / 2f);
            }
        }

        
        /// <summary>
        /// The center of the current viewport.
        /// </summary>
        private static Vector2 viewportCenter;


        #endregion


        #region Party


        /// <summary>
        /// The speed of the party leader, in units per second.
        /// </summary>
        /// <remarks>
        /// The movementCollisionTolerance constant should be a multiple of this number.
        /// </remarks>
        private const float partyLeaderMovementSpeed = 3f;


        /// <summary>
        /// The current position of the party leader.
        /// </summary>
        private static PlayerPosition partyLeaderPosition = new PlayerPosition();
        public static PlayerPosition PartyLeaderPosition
        {
            get { return partyLeaderPosition; }
            set { partyLeaderPosition = value; }
        }


        /// <summary>
        /// The automatic movement remaining for the party leader.
        /// </summary>
        /// <remarks>
        /// This is typically used for automatic movement when spawning on a map.
        /// </remarks>
        private static Vector2 autoPartyLeaderMovement = Vector2.Zero;


        /// <summary>
        /// Updates the automatic movement of the party.
        /// </summary>
        /// <returns>The automatic movement for this update.</returns>
        private static Vector2 UpdatePartyLeaderAutoMovement(GameTime gameTime)
        {
            // check for any remaining auto-movement
            if (autoPartyLeaderMovement == Vector2.Zero)
            {
                return Vector2.Zero;
            }

            // get the remaining-movement direction
            Vector2 autoMovementDirection = Vector2.Normalize(autoPartyLeaderMovement);

            // calculate the potential movement vector
            Vector2 movement = Vector2.Multiply(autoMovementDirection,
                partyLeaderMovementSpeed);

            // limit the potential movement vector by the remaining auto-movement
            movement.X = Math.Sign(movement.X) * MathHelper.Min(Math.Abs(movement.X),
                Math.Abs(autoPartyLeaderMovement.X));
            movement.Y = Math.Sign(movement.Y) * MathHelper.Min(Math.Abs(movement.Y),
                Math.Abs(autoPartyLeaderMovement.Y));

            // remove the movement from the total remaining auto-movement
            autoPartyLeaderMovement -= movement;

            return movement;
        }

        private static float CalculateMovement(float direction, float speed)
        {
            if (direction <= 25 && direction > 0)
            {
                direction = 0;
            }
            else if (direction >= -25 && direction < 0)
            {
                direction = 0;
            }

            float movement = direction / speed;
            return movement;

        }

        private static Vector2 TargetPoint;

        public static void StopMovement()
        {
            distanceToFollow = 0f;
            TargetPoint = Vector2.Zero;
        }

        /// <summary>
        /// Update the user-controlled movement of the party.
        /// </summary>
        /// <returns>The controlled movement for this update.</returns>
        private static Vector2 UpdateUserMovement(GameTime gameTime)
        {
            Vector2 desiredMovement = Vector2.Zero;

            if (InputManager.Gestures.Count > 0)
            {
                foreach (var item in InputManager.Gestures)
                {
                    if (item.GestureType == GestureType.Tap)
                    {
                        if (item.Position != TargetPoint)
                        {
                            TargetPoint = item.Position;
                            distanceToFollow = Vector2.Distance(TargetPoint, partyLeaderPosition.ScreenPosition);
                        }
                        desiredMovement = FollowMovement(TargetPoint);

                    }
                    else if (item.GestureType == GestureType.VerticalDrag ||
                        item.GestureType == GestureType.HorizontalDrag)
                    {
                        if (TargetPoint != Vector2.Zero)
                        {
                            StopMovement();
                        }
                        desiredMovement = FreeMovement(desiredMovement);
                    }
                }
            }
            else
            {
                if (TargetPoint == Vector2.Zero)
                {
                    desiredMovement = FreeMovement(desiredMovement);
                }
                else
                {
                    desiredMovement = FollowMovement(TargetPoint);
                }
            }
            if (desiredMovement != Vector2.Zero)
            {
                desiredMovement.Normalize();
                desiredMovement *= 3f;
            }

            return desiredMovement;
        }

        private static Vector2 FreeMovement(Vector2 desiredMovement)
        {
            if (InputManager.CurrentTouchState.Count > 0)
            {
                TouchLocation gesture = InputManager.CurrentTouchState[InputManager.CurrentTouchState.Count - 1];
                Vector2 dir = gesture.Position - partyLeaderPosition.ScreenPosition;

                desiredMovement.X += CalculateMovement(dir.X, partyLeaderMovementSpeed);
                desiredMovement.Y += CalculateMovement(dir.Y, partyLeaderMovementSpeed);

            }
            Vector2 vec = new Vector2(desiredMovement.X, desiredMovement.Y);
            return vec;
        }

        private static float distanceToFollow;

        public static Vector2 FollowMovement(Vector2 targetPoint)
        {
            if (distanceToFollow < 3)
            {
                StopMovement();
                return Vector2.Zero;
            }
            Vector2 dir = targetPoint - partyLeaderPosition.ScreenPosition;

            Vector2 vecToMove =new Vector2(CalculateMovement(dir.X, partyLeaderMovementSpeed),
                CalculateMovement(dir.Y, partyLeaderMovementSpeed));
            distanceToFollow -= 3;
            return vecToMove;

        }


        #endregion


        #region Collision


        /// <summary>
        /// The number of pixels that characters should be allowed to move into 
        /// blocking tiles.
        /// </summary>
        /// <remarks>
        /// The partyMovementSpeed constant should cleanly divide this number.
        /// </remarks>
        const int movementCollisionTolerance = 12;


        /// <summary>
        /// Returns true if the player can move up from their current position.
        /// </summary>
        private static bool CanPartyLeaderMoveUp()
        {
            // if they're not within the tolerance of the next tile, then this is moot
            if (partyLeaderPosition.TileOffset.Y > -movementCollisionTolerance)
            {
                return true;
            }

            // if the player is at the outside left and right edges, 
            // then check the diagonal tiles
            if (partyLeaderPosition.TileOffset.X < -movementCollisionTolerance)
            {
                if (map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X - 1,
                    partyLeaderPosition.TilePosition.Y - 1)))
                {
                    return false;
                }
            }
            else if (partyLeaderPosition.TileOffset.X > movementCollisionTolerance)
            {
                if (map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X + 1,
                    partyLeaderPosition.TilePosition.Y - 1)))
                {
                    return false;
                }
            }

            // check the tile above the current one
            return !map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X,
                    partyLeaderPosition.TilePosition.Y - 1));
        }


        /// <summary>
        /// Returns true if the player can move down from their current position.
        /// </summary>
        private static bool CanPartyLeaderMoveDown()
        {
            // if they're not within the tolerance of the next tile, then this is moot
            if (partyLeaderPosition.TileOffset.Y < movementCollisionTolerance)
            {
                return true;
            }

            // if the player is at the outside left and right edges, 
            // then check the diagonal tiles
            if (partyLeaderPosition.TileOffset.X < -movementCollisionTolerance)
            {
                if (map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X - 1,
                    partyLeaderPosition.TilePosition.Y + 1)))
                {
                    return false;
                }
            }
            else if (partyLeaderPosition.TileOffset.X > movementCollisionTolerance)
            {
                if (map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X + 1,
                    partyLeaderPosition.TilePosition.Y + 1)))
                {
                    return false;
                }
            }

            // check the tile below the current one
            return !map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X,
                    partyLeaderPosition.TilePosition.Y + 1));
        }


        /// <summary>
        /// Returns true if the player can move left from their current position.
        /// </summary>
        private static bool CanPartyLeaderMoveLeft()
        {
            // if they're not within the tolerance of the next tile, then this is moot
            if (partyLeaderPosition.TileOffset.X > -movementCollisionTolerance)
            {
                return true;
            }

            // if the player is at the outside left and right edges, 
            // then check the diagonal tiles
            if (partyLeaderPosition.TileOffset.Y < -movementCollisionTolerance)
            {
                if (map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X - 1,
                    partyLeaderPosition.TilePosition.Y - 1)))
                {
                    return false;
                }
            }
            else if (partyLeaderPosition.TileOffset.Y > movementCollisionTolerance)
            {
                if (map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X - 1,
                    partyLeaderPosition.TilePosition.Y + 1)))
                {
                    return false;
                }
            }

            // check the tile to the left of the current one
            return !map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X - 1,
                    partyLeaderPosition.TilePosition.Y));
        }


        /// <summary>
        /// Returns true if the player can move right from their current position.
        /// </summary>
        private static bool CanPartyLeaderMoveRight()
        {
            // if they're not within the tolerance of the next tile, then this is moot
            if (partyLeaderPosition.TileOffset.X < movementCollisionTolerance)
            {
                return true;
            }

            // if the player is at the outside left and right edges, 
            // then check the diagonal tiles
            if (partyLeaderPosition.TileOffset.Y < -movementCollisionTolerance)
            {
                if (map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X + 1,
                    partyLeaderPosition.TilePosition.Y - 1)))
                {
                    return false;
                }
            }
            else if (partyLeaderPosition.TileOffset.Y > movementCollisionTolerance)
            {
                if (map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X + 1,
                    partyLeaderPosition.TilePosition.Y + 1)))
                {
                    return false;
                }
            }

            // check the tile to the right of the current one
            return !map.IsBlocked(new Point(
                    partyLeaderPosition.TilePosition.X + 1,
                    partyLeaderPosition.TilePosition.Y));
        }


        #endregion


        #region Updating


        /// <summary>
        /// Update the tile engine.
        /// </summary>
        public static void Update(GameTime gameTime)
        {
            // check for auto-movement
            Vector2 autoMovement = UpdatePartyLeaderAutoMovement(gameTime);

            // if there is no auto-movement, handle user controls
            Vector2 userMovement = Vector2.Zero;
            if (autoMovement == Vector2.Zero)
            {
                userMovement = UpdateUserMovement(gameTime);
                // calculate the desired position
                if (userMovement != Vector2.Zero)
                {
                    Point desiredTilePosition = partyLeaderPosition.TilePosition;
                    Vector2 desiredTileOffset = partyLeaderPosition.TileOffset;
                    PlayerPosition.CalculateMovement(
                        Vector2.Multiply(userMovement, 15f ),
                        ref desiredTilePosition, ref desiredTileOffset);

                    // check for collisions or encounters in the new tile
                    if ((partyLeaderPosition.TilePosition != desiredTilePosition) && 
                        !MoveIntoTile(desiredTilePosition))
                    {
                        StopMovement();
                        userMovement = Vector2.Zero;

                    }
                }
            }

            // move the party
            Point oldPartyLeaderTilePosition = partyLeaderPosition.TilePosition;
            partyLeaderPosition.Move(autoMovement + userMovement);

            // if the tile position has changed, check for random combat
            if ((autoMovement == Vector2.Zero) &&
                (partyLeaderPosition.TilePosition != oldPartyLeaderTilePosition))
            {
                Session.CheckForRandomCombat(Map.RandomCombat);
            }

            // adjust the map origin so that the party is at the center of the viewport
            mapOriginPosition += viewportCenter - (partyLeaderPosition.ScreenPosition + 
                Session.Party.Players[0].MapSprite.SourceOffset);

            // make sure the boundaries of the map are never inside the viewport
            mapOriginPosition.X = MathHelper.Min(mapOriginPosition.X, viewport.X);
            mapOriginPosition.Y = MathHelper.Min(mapOriginPosition.Y, viewport.Y);
            mapOriginPosition.X += MathHelper.Max(
                (viewport.X + viewport.Width) - 
                (mapOriginPosition.X + map.MapDimensions.X * (map.TileSize.X * ScaledVector2.DrawFactor )), 0f);
            mapOriginPosition.Y += MathHelper.Max(
                (viewport.Y + viewport.Height - Hud.HudHeight) - 
                (mapOriginPosition.Y + map.MapDimensions.Y * (map.TileSize.Y * ScaledVector2.DrawFactor)), 0f);
        }


        /// <summary>
        /// Performs any actions associated with moving into a new tile.
        /// </summary>
        /// <returns>True if the character can move into the tile.</returns>
        private static bool MoveIntoTile(Point mapPosition)
        {
            // if the tile is blocked, then this is simple
            if (map.IsBlocked(mapPosition))
            {
                System.Diagnostics.Debug.WriteLine("cannot move");
                return false;
            }

            // check for anything that might be in the tile
            if (Session.EncounterTile(mapPosition))
            {
                return false;
            }

            // nothing stops the party from moving into the tile
            return true;
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw the visible tiles in the given map layers.
        /// </summary>
        public static void DrawLayers(SpriteBatch spriteBatch, bool drawBase, 
            bool drawFringe, bool drawObject)
        {
            // check the parameters
            if (spriteBatch == null)
            {
                throw new ArgumentNullException("spriteBatch");
            }
            if (!drawBase && !drawFringe && !drawObject)
            {
                return;
            }

            bool lastRowDraw = false;

            Rectangle destinationRectangle =
                new Rectangle(0, 0,
                    (int)(map.TileSize.X * ScaledVector2.DrawFactor),
                    (int)(map.TileSize.Y * ScaledVector2.DrawFactor));

            for (int y = 0; y < map.MapDimensions.Y ; y++)
            {
                for (int x = 0; x < map.MapDimensions.X ; x++)
                {
                    destinationRectangle.X =
                        (int)mapOriginPosition.X + x * (int)(map.TileSize.X * ScaledVector2.DrawFactor);
                    destinationRectangle.Y =
                        (int)mapOriginPosition.Y + y * (int)(map.TileSize.Y * ScaledVector2.DrawFactor);

                    // If the tile is inside the screen
                    if (CheckVisibility(destinationRectangle))
                    {

                        Point mapPosition = new Point(x, y);
                        if (drawBase)
                        {
                            Rectangle sourceRectangle =
                                map.GetBaseLayerSourceRectangle(mapPosition);
                            if (sourceRectangle != Rectangle.Empty)
                            {
                                if (y == map.MapDimensions.Y -1)
                                {
                                    spriteBatch.Draw(map.Texture, new Vector2(
                                         destinationRectangle.X, destinationRectangle.Y),
                                         sourceRectangle, Color.White, 0f, Vector2.Zero,
                                         new Vector2(1.333f, 2.2f), SpriteEffects.None, 0f);
                                }
                                else
                                {
                                    spriteBatch.Draw(map.Texture, new Vector2(
                                        destinationRectangle.X, destinationRectangle.Y),
                                        sourceRectangle, Color.White, 0f, Vector2.Zero,
                                        ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                                }

                            }
                        }
                        if (drawFringe)
                        {
                            Rectangle sourceRectangle =
                                map.GetFringeLayerSourceRectangle(mapPosition);
                            if (sourceRectangle != Rectangle.Empty)
                            {
                                if (y == map.MapDimensions.Y - 1)
                                {
                                    spriteBatch.Draw(map.Texture, new Vector2(
                                         destinationRectangle.X, destinationRectangle.Y),
                                         sourceRectangle, Color.White, 0f, Vector2.Zero,
                                         new Vector2(1.333f, 2.2f), SpriteEffects.None, 0f);
                                }
                                else
                                {
                                    spriteBatch.Draw(map.Texture, new Vector2(
                                        destinationRectangle.X, destinationRectangle.Y),
                                        sourceRectangle, Color.White, 0f, Vector2.Zero,
                                        ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                                }
                            }
                        }
                        if (drawObject)
                        {
                            Rectangle sourceRectangle =
                                map.GetObjectLayerSourceRectangle(mapPosition);
                            if (sourceRectangle != Rectangle.Empty)
                            {
                                if (y == map.MapDimensions.Y -1)
                                {
                                    spriteBatch.Draw(map.Texture, new Vector2(
                                         destinationRectangle.X, destinationRectangle.Y),
                                         sourceRectangle, Color.White, 0f, Vector2.Zero,
                                         new Vector2(1.333f, 2.2f), SpriteEffects.None, 0f);
                                }
                                else
                                {
                                    spriteBatch.Draw(map.Texture, new Vector2(
                                        destinationRectangle.X, destinationRectangle.Y),
                                        sourceRectangle, Color.White, 0f, Vector2.Zero,
                                        ScaledVector2.DrawFactor, SpriteEffects.None, 0f);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!lastRowDraw)
                        {
                            y = y -2;
                            lastRowDraw = true;
                            break; 
                        }
                       
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if the given rectangle is within the viewport.
        /// </summary>
        public static bool CheckVisibility(Rectangle screenRectangle)
        {
            return ((screenRectangle.X > viewport.X - screenRectangle.Width) &&
                (screenRectangle.Y > viewport.Y - screenRectangle.Height) &&
                (screenRectangle.X < viewport.X + viewport.Width) &&
                (screenRectangle.Y < viewport.Y + viewport.Height));
        }

            
        #endregion
    }
}
