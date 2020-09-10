#region File Description
//-----------------------------------------------------------------------------
// GameSceneNode.cs
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
using RobotGameData.Render;
#endregion

namespace RobotGameData
{
    public struct SortInfo
    {
        public GameSceneNode node;
        public float depth;

        public void Clear()
        {
            this.node = null;
            this.depth = 0.0f;
        }

        public void CopyTo(ref SortInfo target)
        {
            target.node = this.node;
            target.depth = this.depth;
        }
    }

    /// <summary>
    /// GameSceneNode is node for drawing and updating.
    /// When the framework gets drawn, all of the registered nodes OnDraw function 
    /// is automatically called.  When GameSceneNode’s “Visible” is set to false, 
    /// it doesn’t get drawn in the framework.
    /// </summary>
    public class GameSceneNode : GameNode
    {
        public enum DrawOrderType
        {
            None = 0,
            Ascending,
            Descending,
        }

        #region Fields

        /// <summary>
        /// If not set to visible, the child as well as itself will not draw.
        /// </summary>
        bool visible = true;

        DrawOrderType orderWhenDraw = DrawOrderType.None;

        Matrix rootAxis = Matrix.Identity;
        Matrix worldTransform = Matrix.Identity;
        Vector3 position = Vector3.Zero;
        Vector3 direction = Vector3.Forward;
        Vector3 up = Vector3.Up;
        Vector3 right = Vector3.Right;

        static SortInfo[] sortArray = new SortInfo[1024];

        public event EventHandler VisibleChanged;

        #endregion

        #region Properties

        public bool Visible
        {
            get { return visible; }
            set
            {
                visible = value;

                if (VisibleChanged != null)
                    VisibleChanged(this, null);
            }
        }

        public DrawOrderType OrderWhenDraw
        {
            get { return orderWhenDraw; }
            set { orderWhenDraw = value; }
        }

        public Matrix RootAxis
        {
            get { return rootAxis; }
            set { rootAxis = value; }
        }

        public Matrix WorldTransform
        {
            get { return worldTransform; }
            set
            {
                worldTransform = value;

                position = worldTransform.Translation;
                direction = worldTransform.Forward;
                up = worldTransform.Up;
                right = worldTransform.Right;
            }
        }

        public Matrix TransformedMatrix
        {
            get { return rootAxis * worldTransform; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = worldTransform.Translation = value; }
        }

        public Vector3 Direction
        {
            get { return direction; }
            set { direction = worldTransform.Forward = value; }
        }

        public Vector3 Right
        {
            get { return right; }
            set { right = worldTransform.Right = value; }
        }

        public Vector3 Up
        {
            get { return up; }
            set { up = worldTransform.Up = value; }
        } 

        #endregion

        /// <summary>
        /// This is just for defining interface and does not get used.
        /// </summary>
        public virtual void Draw(GameTime gameTime) { }

        /// <summary>
        /// It draws itself and child nodes.
        /// When "Visible" is set to false, it will not get executed.
        /// </summary>
        public virtual void Draw(RenderTracer renderTracer)
        {
            //  If not set to visible, it will not draw.
            if (Visible )
            {                
                switch (orderWhenDraw)
                {
                    case DrawOrderType.None:
                        {                            
                            //  Draw itself
                            OnDraw(renderTracer);

                            //  Draw each child node
                            if (childList != null)
                            {
                                for (int i = 0; i < childList.Count; i++)
                                {
                                    GameSceneNode gameSceneNode = 
                                        childList[i] as GameSceneNode;

                                    if ((gameSceneNode != null) && 
                                        gameSceneNode.Visible)
                                    {
                                        gameSceneNode.Draw(renderTracer);
                                    }
                                }
                            }
                        }
                        break;
                    case DrawOrderType.Ascending:
                        {
                            //  draw up in order
                            DrawSort(renderTracer, true);
                        }
                        break;
                    case DrawOrderType.Descending:
                        {
                            //  draw up in order
                            DrawSort(renderTracer, false);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// When Draw() function is executed, it will be called automatically.  
        /// It gets overridden and defined externally.
        /// When the parent is called, the child also gets called.
        /// </summary>
        protected virtual void OnDraw(RenderTracer renderTracer) { }
                
        /// <summary>
        /// draw up in order every scene node when before drawing and draw scene.
        /// </summary>
        protected void DrawSort(RenderTracer renderTracer, bool ascending)
        {
            int nodeCount = 0;

            //  fill in sort array.
            for (int i = 0; i < ChildCount; i++)
            {
                GameSceneNode child = GetChild(i) as GameSceneNode;

                if (child.Visible )
                {
                    Matrix modelView = child.TransformedMatrix * renderTracer.View;

                    sortArray[nodeCount].node = child;
                    sortArray[nodeCount].depth = modelView.Translation.Length();

                    nodeCount++;
                }
            }

            //  draw every scene node using quick sort algorithm.
            QuickSort(ref sortArray, 0, nodeCount - 1, ascending);

            //  draw every sorted node in array.
            for (int i = 0; i < nodeCount; i++)
            {
                sortArray[i].node.Draw(renderTracer);
                sortArray[i].Clear();
            }
        }

        /// <summary>
        /// uses quick sort algorithm.
        /// </summary>
        /// <param name="data">data array</param>
        /// <param name="left">start index</param>
        /// <param name="right">end index</param>
        /// <param name="ascending">ascending or descending</param>
        public void QuickSort(ref SortInfo[] data, int left, int right, bool ascending)
        {
            int leftIndex = left;
            int rightIndex = right;
            float pivot = data[(left+right)/2].depth;

            SortInfo temp = new SortInfo();

            while (leftIndex <= rightIndex)
            {
                //  search
                if (ascending )
                {
                    for (; data[leftIndex].depth < pivot; leftIndex++) ;
                    for (; data[rightIndex].depth > pivot; rightIndex--) ;
                }
                else
                {
                    for (; data[leftIndex].depth > pivot; leftIndex++) ;
                    for (; data[rightIndex].depth < pivot; rightIndex--) ;
                }

                //  swap two values
                if (leftIndex <= rightIndex)
                {
                    data[leftIndex].CopyTo(ref temp);
                    data[rightIndex].CopyTo(ref data[leftIndex]);
                    temp.CopyTo(ref data[rightIndex]);

                    leftIndex++;
                    rightIndex--;
                }
            }

            //  retry sorting
            if (rightIndex > left)
                QuickSort(ref data, left, rightIndex, ascending);

            //  retry sorting
            if (leftIndex < right)
                QuickSort(ref data, leftIndex, right, ascending);
        }
    }
}


