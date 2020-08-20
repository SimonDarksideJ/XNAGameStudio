#region File Description
//-----------------------------------------------------------------------------
// Viewer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using RobotGameData.Camera;
#endregion

namespace RobotGameData.Render
{
    #region Enum

    public enum ViewerWidth
    {
        Invalid = 0,
        Width480 = 853,
        Width720 = 1280,
        Width1080 = 1920,
    }

    public enum ViewerHeight
    {
        Invalid = 0,
        Height480 = 480,
        Height720 = 720,
        Height1080 = 1080,
    }

    #endregion

    #region ViewCamera

    /// <summary>
    /// a view class with a viewport and a camera.
    /// With the split-screen mode, there can be more than one viewport and one camera.
    /// </summary>
    public class ViewCamera
    {
        #region Fields

        int count = 0;

        List<Viewport> viewport = new List<Viewport>();
        List<CameraBase> camera = new List<CameraBase>();

        #endregion

        #region Properties

        public int Count
        {
            get { return count; }
        }

        public CameraBase FirstCamera
        {
            get { return camera[0]; }
        }

        #endregion

        #region Constructors

        public ViewCamera()
        {
            count = 0;
            viewport.Clear();
            camera.Clear();
        }

        public ViewCamera(CameraBase camera, Rectangle? viewportRectangle)
        {
            Add(camera, viewportRectangle);
        }

        #endregion

        /// <summary>
        /// adds a camera and viewport.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="viewportRectangle"></param>
        public void Add(CameraBase camera, Rectangle? viewportRectangle)
        {
            this.camera.Add(camera);

            Viewport newViewport = FrameworkCore.Game.GraphicsDevice.Viewport;
            if (viewportRectangle == null)
            {
                newViewport.X = 0;
                newViewport.Y = 0;
                newViewport.Width = FrameworkCore.Game.GraphicsDevice.Viewport.Width;
                newViewport.Height = FrameworkCore.Game.GraphicsDevice.Viewport.Height;
            }
            else
            {
                newViewport.X = viewportRectangle.Value.X;
                newViewport.Y = viewportRectangle.Value.Y;
                newViewport.Width = viewportRectangle.Value.Width;
                newViewport.Height = viewportRectangle.Value.Height;
            }

            this.viewport.Add(newViewport);

            count++;
        }

        /// <summary>
        /// removes the camera by index.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            viewport.RemoveAt(index);
            camera.RemoveAt(index);
        }

        /// <summary>
        /// resize the camera and viewport by index.
        /// </summary>
        /// <param name="index">an index of camera</param>
        /// <param name="x">screen x-position</param>
        /// <param name="y">screen y-position</param>
        /// <param name="width">screen width</param>
        /// <param name="height">screen height</param>
        public void Resize(int index, int x, int y, int width, int height)
        {
            CameraBase camera = GetCamera(index);
            camera.Resize(width, height);

            Viewport newViewport = GetViewport(index);

            newViewport.X = x;
            newViewport.Y = y;
            newViewport.Width = width;
            newViewport.Height = height;

            this.viewport[index] = newViewport;
        }

        public CameraBase GetCamera(int index)
        {
            return this.camera[index];
        }

        public Viewport GetViewport(int index)
        {
            return this.viewport[index];
        }
    }

    #endregion

    /// <summary>
    /// This contains and manages RenderContext class and a 3D camera.
    /// Viewer may have several cameras.
    /// </summary>
    public class Viewer
    {
        #region Fields

        RenderContext renderContext = null;
        Dictionary<string, ViewCamera> viewCameraList =
            new Dictionary<string, ViewCamera>();
        ViewCamera currentViewCamera = null;
        ViewCamera defaultViewCamera = null;

        Viewport defaultViewport;

        RenderFog basicFog = null;
        RenderLighting basicLighting = null;

        #endregion

        #region Properties

        public static Viewport CurrentViewport
        {
            get { return FrameworkCore.Game.GraphicsDevice.Viewport; }
        }

        public Viewport DefaultViewport
        {
            get { return defaultViewport; }
            set { defaultViewport = value; }
        }

        public int ViewWidth
        {
            get { return this.defaultViewport.Width; }
        }

        public int ViewHeight
        {
            get { return this.defaultViewport.Height; }
        }

        public int ViewPosX
        {
            get { return this.defaultViewport.X; }
        }

        public int ViewPosY
        {
            get { return this.defaultViewport.Y; }
        }

        public RenderContext RenderContext
        {
            get { return renderContext; }
        }

        public GameSceneNode Scene3DRoot
        {
            get { return RenderContext.Scene3DRoot; }
        }

        public Color ClearColor
        {
            get { return this.RenderContext.ClearColor; }
            set { this.RenderContext.ClearColor = value; }
        }

        public RenderFog BasicFog
        {
            get { return this.basicFog; }
            set { this.basicFog = value; }
        }

        public RenderLighting BasicLighting
        {
            get { return this.basicLighting; }
            set { this.basicLighting = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">game</param>
        public Viewer(Game game)
        {
            renderContext = new RenderContext(game);
        }

        /// <summary>
        /// initialize members and creates default camera.
        /// </summary>
        public void Initialize()
        {
            //  initialize the RenderContext
            renderContext.Initialize();

            this.defaultViewport = FrameworkCore.Game.GraphicsDevice.Viewport;

            // creates default camera
            {
                defaultViewCamera = new ViewCamera(new CameraBase(), null);

                //  default camera setting
                defaultViewCamera.FirstCamera.SetView(Vector3.Zero, Vector3.Forward, 
                    Vector3.Up);

                defaultViewCamera.FirstCamera.SetPespective(MathHelper.PiOver4,
                                            (float)ViewWidth,
                                            (float)ViewHeight, 1.0f, 1000.0f);

                currentViewCamera = defaultViewCamera;
            }
        }

        /// <summary>
        /// resize the default camera.
        /// </summary>
        /// <param name="width">screen width</param>
        /// <param name="height">screen height</param>
        public void Resize(int width, int height)
        {
            this.defaultViewport.Width = width;
            this.defaultViewport.Height = height;
        }

        /// <summary>
        /// update cameras.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Update the current camera 
            ViewCamera viewCamera = CurrentCamera;
            for (int i = 0; i < viewCamera.Count; i++)
            {
                CameraBase camera = viewCamera.GetCamera(i);
                camera.Update(gameTime);
            }

            // Update scene models in the render context
            renderContext.Update(gameTime);
        }

        public void Dispose()
        {
            if (renderContext != null)
            {
                renderContext.Dispose();
                renderContext = null;
            }
        }

        /// <summary>
        /// draws the render context.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime)
        {
            renderContext.Draw(gameTime);
        }

        /// <summary>
        /// adds a camera.
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="viewCamera">view camera</param>
        public void AddCamera(string key, ViewCamera viewCamera)
        {
            //  New camera add
            viewCamera.FirstCamera.Name = key;
            viewCameraList.Add(key, viewCamera);
        }

        /// <summary>
        /// removes the camera by index.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveCamera(string key)
        {
            viewCameraList.Remove(key);
        }

        public void RemoveAllCamera()
        {
            viewCameraList.Clear();
        }

        public ViewCamera GetViewCamera(string key)
        {
            //  Compare valid camera
            if (viewCameraList.ContainsKey(key) == false)
                return null;

            return viewCameraList[key];
        }

        /// <summary>
        /// sets to current camera.
        /// </summary>
        /// <param name="key">key name</param>
        public bool SetCurrentCamera(string key)
        {
            //  Set a current camera
            currentViewCamera = GetViewCamera(key);

            if (currentViewCamera == null)
                return false;

            return true;
        }

        public ViewCamera CurrentCamera
        {
            get { return currentViewCamera; }
        }

        /// <summary>
        /// changes to current camera by key name.
        /// </summary>
        /// <param name="key"></param>
        public void ChangeCamera(string key)
        {
            SetCurrentCamera(key);
        }
    }
}
