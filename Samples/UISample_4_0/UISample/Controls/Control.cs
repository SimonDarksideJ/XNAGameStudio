//-----------------------------------------------------------------------------
// Control.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace UserInterfaceSample.Controls
{
    /// <summary>
    /// Control is the base class for a simple UI controls framework.
    ///
    /// Controls are grouped into a heirarchy: each control has a Parent and
    /// an optional list of Children. They draw themselves
    ///
    /// Input handling
    /// ==============
    /// Call HandleInput() once per update on the root control of the heirarchy; it will then call HandleInput() on
    /// its children. Controls can override HandleInput() to check for touch events and the like. If you override this
    /// function, you need to call base.HandleInput() to let your child controls see input.
    ///
    /// Note that Controls do not have any special system for setting TouchPanel.EnabledGestures; if you're using
    /// gesture-sensitive controls, you need to set EnabledGestures as appopriate in each GameScreen.
    ///
    /// Rendering
    /// =========
    /// Override Control.Draw() to render your control. Control.Draw() takes a 'DrawContext' struct, which contains
    /// the GraphicsDevice and other objects useful for drawing. It also contains a SpriteBatch, which will have had
    /// Begin() called *before* Control.Draw() is called. This allows batching of sprites across multiple controls
    /// to improve rendering speed.
    ///
    /// Layout
    /// ======
    /// Controls have a Position and Size, which defines a rectangle. By default, Size is computed
    /// auotmatically by an internal call to ComputeSize(), which each child control can implement
    /// as appropriate. For example, TextControl uses the size of the rendered text.
    ///
    /// If you *write* to the Size property, auto-sizing will be disabled, and the control will
    /// retain the written size unless you write to it again.
    ///
    /// There is no dynamic layout system. Instead, container controls (PanelControl in particular)
    /// contain methods for positioning their child controls into rows, columns, or other arrangements.
    /// Client code should create the controls needed for a screen, then call one or more of these
    /// layout functions to position them.
    /// </summary>
    public class Control
    {
        #region Private fields
        private Vector2 position;
        private Vector2 size;
        private bool sizeValid = false;
        private bool autoSize = true;
        private List<Control> children = null;
        #endregion

        #region Properties

        /// <summary>
        /// Draw() is not called unless Control.Visible is true (the default).
        /// </summary>
        public bool Visible = true;

        /// <summary>
        /// Position of this control within its parent control.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                if (Parent != null)
                {
                    Parent.InvalidateAutoSize();
                }
            }
        }

        /// <summary>
        /// Size if this control. See above for a discussion of the layout system.
        /// </summary>
        public Vector2 Size
        {
            // Default behavior is for ComputeSize() to determine the size, and then cache it.
            get
            {
                if (!sizeValid)
                {
                    size = ComputeSize();
                    sizeValid = true;
                }
                return size;
            }

            // Setting the size overrides whatever ComputeSize() would return, and disables autoSize
            set
            {
                size = value;
                sizeValid = true;
                autoSize = false;
                if (Parent != null)
                {
                    Parent.InvalidateAutoSize();
                }
            }
        }

        /// <summary>
        /// Call this method when a control's content changes so that its size needs to be recomputed. This has no
        /// effect if autoSize has been disabled.
        /// </summary>
        protected void InvalidateAutoSize()
        {
            if (autoSize)
            {
                sizeValid = false;
                if (Parent != null)
                {
                    Parent.InvalidateAutoSize();
                }
            }
        }

        /// <summary>
        /// The control containing this control, if any
        /// </summary>
        public Control Parent { get; private set; }

        /// <summary>
        /// Number of child controls of this control
        /// </summary>
        public int ChildCount { get { return children == null ? 0 : children.Count; } }

        /// <summary>
        /// Indexed access to the children of this control.
        /// </summary>
        public Control this[int childIndex]
        {
            get
            {
                return children[childIndex];
            }
        }
        #endregion

        #region Child control API
        public void AddChild(Control child)
        {
            if (child.Parent != null)
            {
                child.Parent.RemoveChild(child);
            }
            AddChild(child, ChildCount);
        }

        public void AddChild(Control child, int index)
        {
            if (child.Parent != null)
            {
                child.Parent.RemoveChild(child);
            }
            child.Parent = this;
            if (children == null)
            {
                children = new List<Control>();
            }
            children.Insert(index, child);
            OnChildAdded(index, child);
        }

        public void RemoveChildAt(int index)
        {
            Control child = children[index];
            child.Parent = null;
            children.RemoveAt(index);
            OnChildRemoved(index, child);
        }


        /// <summary>
        /// Remove the given control from this control's list of children.
        /// </summary>
        public void RemoveChild(Control child)
        {
            if(child.Parent != this)
                throw new InvalidOperationException();

            RemoveChildAt(children.IndexOf(child));
        }
        #endregion

        #region Virtual methods for derived classes to override

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public virtual void Draw(DrawContext context)
        {
            Vector2 origin = context.DrawOffset;
            for(int i=0; i<ChildCount; i++)
            {
                Control child = children[i];
                if (child.Visible)
                {
                    context.DrawOffset = origin + child.Position;
                    child.Draw(context);
                }
            }
        }

        /// <summary>
        /// Called once per frame to update the control; override this method if your control requires custom updates.
        /// Call base.Update() to update any child controls.
        /// </summary>
        public virtual void Update(GameTime gametime)
        {
            for (int i = 0; i < ChildCount; i++)
            {
                children[i].Update(gametime);
            }
        }

        /// <summary>
        /// Called once per frame to update the control; override this method if your control requires custom updates.
        /// Call base.Update() to update any child controls.
        /// </summary>
        public virtual void HandleInput(InputState input)
        {
            for (int i = 0; i < ChildCount; i++)
            {
                children[i].HandleInput(input);
            }
        }

        /// <summary>
        /// Called when the Size property is read and sizeValid is false. Call base.ComputeSize() to compute the
        /// size (actually the lower-right corner) of all child controls.
        /// </summary>
        public virtual Vector2 ComputeSize()
        {
            if (children == null || children.Count == 0)
            {
                return Vector2.Zero;
            }
            else
            {
                Vector2 bounds = children[0].Position + children[0].Size;
                for (int i = 1; i < children.Count; i++)
                {
                    Vector2 corner = children[i].Position + children[i].Size;
                    bounds.X = Math.Max(bounds.X, corner.X);
                    bounds.Y = Math.Max(bounds.Y, corner.Y);
                }
                return bounds;
            }
        }

        /// <summary>
        /// Called after a child control is added to this control. The default behavior is to call InvalidateAutoSize().
        /// </summary>
        protected virtual void OnChildAdded(int index, Control child)
        {
            InvalidateAutoSize();
        }

        /// <summary>
        /// Called after a child control is removed from this control. The default behavior is to call InvalidateAutoSize().
        /// </summary>
        protected virtual void OnChildRemoved(int index, Control child)
        {
            InvalidateAutoSize();
        }
        #endregion

        #region Static methods

        // Call this method once per frame on the root of your control heirarchy to draw all the controls.
        // See ControlScreen for an example.
        public static void BatchDraw(Control control, GraphicsDevice device, SpriteBatch spriteBatch, Vector2 offset, GameTime gameTime)
        {
            if (control != null && control.Visible)
            {
                spriteBatch.Begin();
                control.Draw(new DrawContext
                {
                    Device = device,
                    SpriteBatch = spriteBatch,
                    DrawOffset = offset + control.Position,
                    GameTime = gameTime
                });
                spriteBatch.End();
            }
        }
        #endregion
    }
}
