#region File Description
//-----------------------------------------------------------------------------
// SceneItem.cs
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
#endregion

namespace Spacewar
{
    /// <summary>
    /// SceneItem is any item that can be in a scenegraph
    /// </summary>
    public class SceneItem : List<SceneItem>
    {
        /// <summary>
        /// Collision Radius of this item
        /// </summary>
        protected float radius;

        /// <summary>
        /// Should this item be deleted?
        /// </summary>
        protected bool delete;

        /// <summary>
        /// Simulation paused for this items, nothing will update
        /// </summary>
        private bool paused;

        /// <summary>
        /// The root SceneItem
        /// </summary>
        protected SceneItem Root;

        /// <summary>
        /// The parent SceneItem
        /// </summary>
        protected SceneItem Parent;

        /// <summary>
        /// Shape is the actual renderable object
        /// </summary>
        protected Shape shape;

        /// <summary>
        /// The position of this item
        /// </summary>
        protected Vector3 position;

        /// <summary>
        /// The velocity of this item
        /// </summary>
        protected Vector3 velocity;

        /// <summary>
        /// The acceleration of the item
        /// </summary>
        protected Vector3 acceleration;

        /// <summary>
        /// The current rotation of this item
        /// </summary>
        protected Vector3 rotation;

        /// <summary>
        /// The scaling transformation for this object
        /// </summary>
        protected Vector3 scale = new Vector3(1f, 1f, 1f);

        /// <summary>
        /// The center of rotation and scaling
        /// </summary>
        protected Vector3 center;

        private Game game;

        #region Properties
        public bool Delete
        {
            get
            {
                return delete;
            }
            set
            {
                delete = value;
            }
        }

        public float Radius
        {
            get
            {
                return radius;
            }

            set
            {
                radius = value;
            }
        }

        public Shape ShapeItem
        {
            get
            {
                return shape;
            }

            set
            {
                shape = value;
            }
        }

        public bool Paused
        {
            get
            {
                return paused;
            }

            set
            {
                paused = value;
            }
        }

        public Vector3 Acceleration
        {
            get
            {
                return acceleration;
            }
        }

        public Vector3 Velocity
        {
            get
            {
                return velocity;
            }

            set
            {
                velocity = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public Vector3 Rotation
        {
            get
            {
                return rotation;
            }

            set
            {
                rotation = value;
            }
        }

        public Vector3 Center
        {
            get
            {
                return center;
            }

            set
            {
                center = value;
            }
        }

        public Vector3 Scale
        {
            get
            {
                return scale;
            }

            set
            {
                scale = value;
            }
        }
        protected Game GameInstance
        {
            get
            {
                return game;
            }
        }
        #endregion

        /// <summary>
        /// Default constructor, does nothing special
        /// </summary>
        public SceneItem(Game game)
        {
            this.game = game;
        }

        /// <summary>
        /// Creates a SceneItem with a shape to be rendered at an initial position
        /// </summary>
        /// <param name="shape">The shape to be rendered for this item</param>
        /// <param name="initialPosition">The initial position of the item</param>
        public SceneItem(Game game, Shape shape, Vector3 initialPosition)
        {
            this.shape = shape;
            this.position = initialPosition;
            this.game = game;
        }

        /// <summary>
        /// Creates a SceneItem with a shape to be rendered 
        /// </summary>
        /// <param name="shape">The shape to be rendered for this item</param>
        public SceneItem(Game game, Shape shape)
        {
            this.shape = shape;
            this.game = game;
        }

        /// <summary>
        /// Creates a SceneItem with no shape but a position
        /// </summary>
        /// <param name="initialPosition">The initial position of the item</param>
        public SceneItem(Game game, Vector3 initialPosition)
        {
            this.position = initialPosition;
            this.game = game;
        }

        /// <summary>
        /// Adds an item to the Scene Node
        /// </summary>
        /// <param name="childItem">The item to add</param>
        public new void Add(SceneItem childItem)
        {
            //A new custom 'add' that sets the parent and the root properties
            //on the child item
            childItem.Parent = this;
            if (Root == null)
            {
                childItem.Root = this;
            }
            else
            {
                childItem.Root = Root;
            }

            //Call the 'real' add method on the dictionary
            ((List<SceneItem>)this).Add(childItem);
        }

        /// <summary>
        /// Updates any values associated with this scene item and its children
        /// </summary>
        /// <param name="time">Game time</param>
        /// <param name="elapsedTime">Elapsed game time since last call</param>
        public virtual void Update(TimeSpan time, TimeSpan elapsedTime)
        {
            if (!paused)
            {
                //Do the basic acceleration/velocity/position updates
                velocity += Vector3.Multiply(acceleration, (float)elapsedTime.TotalSeconds);
                position += Vector3.Multiply(velocity, (float)elapsedTime.TotalSeconds);
            }

            //If this item has something to draw then update it
            if (shape != null)
            {
                shape.World = Matrix.CreateTranslation(-center) *
                              Matrix.CreateScale(scale) *
                              Matrix.CreateRotationX(rotation.X) *
                              Matrix.CreateRotationY(rotation.Y) *
                              Matrix.CreateRotationZ(rotation.Z) *
                              Matrix.CreateTranslation(position + center);
                if (!paused)
                {
                    shape.Update(time, elapsedTime);
                }
            }

            //Update each child item
            foreach (SceneItem item in this)
            {
                item.Update(time, elapsedTime);
            }

            //Remove any items that need deletion
            int i = 0;
            while (i < Count)
            {
                if (this[i].delete)
                {
                    RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            //RemoveAll(IsDeleted);
        }

        private static bool IsDeleted(SceneItem item)
        {
            return item.delete;
        }

        /// <summary>
        /// Render any items associated with this scene item and its children
        /// </summary>
        public virtual void Render()
        {
            //If this item has something to draw then draw it
            if (shape != null)
            {
                shape.Render();
            }

            //Then render all of the child nodes
            foreach (SceneItem item in this)
            {
                item.Render();
            }
        }

        /// <summary>
        /// Checks if there is a collision between the this and the passed in item
        /// </summary>
        /// <param name="item">A scene item to check</param>
        /// <returns>True if there is a collision</returns>
        public virtual bool Collide(SceneItem item)
        {
            //Until we get collision meshes sorted just do a simple sphere (well circle!) check
            if ((position - item.position).Length() < radius + item.radius)
                return true;

            //If we are a ship and we are a long, thin pencil,
            //we have additional extents, check those, too!
            Ship shipItem = item as Ship;
            if (shipItem != null && shipItem.ExtendedExtent != null)
            {
                Matrix localRotation = Matrix.CreateRotationZ(shipItem.Rotation.Z);

                Vector4 extendedPosition = Vector4.Transform(shipItem.ExtendedExtent[0], localRotation);
                Vector3 localPosition = shipItem.Position + new Vector3(extendedPosition.X, extendedPosition.Y, extendedPosition.Z);
                if ((Position - localPosition).Length() < radius + item.Radius)
                    return true;

                extendedPosition = Vector4.Transform(shipItem.ExtendedExtent[1], localRotation);
                localPosition = shipItem.Position + new Vector3(extendedPosition.X, extendedPosition.Y, extendedPosition.Z);
                if ((Position - localPosition).Length() < radius + item.Radius)
                    return true;
            }

            Ship ship = this as Ship;
            if (ship != null && ship.ExtendedExtent != null)
            {
                Matrix localRotation = Matrix.CreateRotationZ(ship.Rotation.Z);

                Vector4 extendedPosition = Vector4.Transform(ship.ExtendedExtent[0], localRotation);
                Vector3 localPosition = ship.Position + new Vector3(extendedPosition.X, extendedPosition.Y, extendedPosition.Z);
                if ((localPosition - item.Position).Length() < radius + item.Radius)
                    return true;

                extendedPosition = Vector4.Transform(ship.ExtendedExtent[1], localRotation);
                localPosition = ship.Position + new Vector3(extendedPosition.X, extendedPosition.Y, extendedPosition.Z);
                if ((localPosition - item.Position).Length() < radius + item.Radius)
                    return true;
            }

            return false;
        }

        public virtual void OnCreateDevice()
        {
        }
    }
}
