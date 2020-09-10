#region File Description
//-----------------------------------------------------------------------------
// RenderContext.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.Camera;
#endregion

namespace RobotGameData.Render
{
    #region Render Tracer
    /// <summary>
    /// rendering information.
    /// </summary>
    public class RenderTracer
    {
        public GraphicsDevice Device = null;
        public GameTime GameTime = null;
        public BoundingFrustum Frustum = null;
        public Matrix View = Matrix.Identity;
        public Matrix ViewInvert = Matrix.Identity;
        public Matrix Projection = Matrix.Identity;
        public SpriteBatch SpriteBatch = null;
        public RenderFog Fog = null;
        public RenderLighting Lighting = null;
    }
    #endregion

    #region Render Fog
    /// <summary>
    /// fog information.
    /// </summary>
    public class RenderFog
    {
        public bool enabled = false;
        public float start = 1.0f;
        public float end = 10000.0f;
        public Color color = Color.White;
    }
    #endregion

    #region Render Lighting
    /// <summary>
    /// lighting information.
    /// </summary>
    public class RenderLighting
    {
        public bool enabled = false;
        public Vector3 direction = Vector3.Zero;
        public Color ambientColor = Color.White;
        public Color diffuseColor = Color.White;
        public Color specularColor = Color.White;
    }
    #endregion

    #region Render Material
    /// <summary>
    /// material information
    /// </summary>
    public class RenderMaterial
    {
        public float alpha = 1.0f;
        public Color diffuseColor = Color.White;
        public Color specularColor = Color.White;
        public Color emissiveColor = Color.Black;
        public float specularPower = 24;
        public bool vertexColorEnabled = false;
        public bool preferPerPixelLighting = false;
    }
    #endregion

    /// <summary>
    /// It has an important role of managing all of the scene objects 
    /// and drawing on screen.  scene3DRoot node manages and allows the drawing 
    /// of all of the object nodes on 3D screen. 
    /// There is only one scene3DRoot node in framework.
    /// For scene2DLayer, depending on the game content, there may be 
    /// several scene 2D root nodes and it allows the registered 
    /// 2D sprite nodes to be drawn. Depending on the creation, 
    /// there may be zero or n numbers.
    /// The internal draw function classifies the nature of the dimension, 3D or 2D, 
    /// and draws on screen by level.
    /// Also, it has Viewport and view frustum.
    /// </summary>
    public class RenderContext : DrawableGameComponent
    {
        #region Fields

        /// <summary>
        /// The scene renrder information
        /// </summary>
        RenderTracer renderTracer = new RenderTracer();

        SpriteBatch spriteBatch = null; 
  
        /// <summary>
        /// The root scene node
        /// </summary>
        GameSceneNode scene3DRoot = null;
        
        /// <summary>
        /// The 2D scene node layer
        /// </summary>
        List<GameSceneNode> scene2DLayer = null;

        /// <summary>
        /// The 2D overlay scene layer
        /// </summary>
        List<GameSceneNode> scene2DOverlay = null;

        /// <summary>
        /// Frustum culling for render mesh
        /// </summary>
        BoundingFrustum frustum = null;

        /// <summary>
        /// color of the back buffer.
        /// </summary>
        Color clearColor = Color.Black;

        public event EventHandler RenderingPreDrawScene;
        public event EventHandler RenderingPostDrawScene;
        public event EventHandler RenderingPreDraw3D;
        public event EventHandler RenderingPostDraw3D;
        public event EventHandler RenderingPreDraw2D;
        public event EventHandler RenderingPostDraw2D;

        #endregion

        #region Properties

        public GameSceneNode Scene3DRoot { get { return scene3DRoot; } }
        public GameSceneNode[] Scene2DLayers { get { return scene2DLayer.ToArray(); } }
        public GameSceneNode Scene2DFadeLayer { get { return scene2DOverlay[0]; } }
        public GameSceneNode Scene2DTopLayer { get { return scene2DOverlay[1]; } }
        public SpriteBatch SpriteBatch { get { return spriteBatch; } }
        public BoundingFrustum Frustum { get { return frustum; } }

        public Color ClearColor
        {
            get { return this.clearColor; }
            set { this.clearColor = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">game</param>
        public RenderContext(Game game)
            : base(game)
        {
            //  Create scene root nodes
            scene2DLayer = new List<GameSceneNode>();
            scene2DOverlay = new List<GameSceneNode>();

            GameSceneNode SceneFade = new GameSceneNode();
            SceneFade.Name = "Scene2DFadeRoot";

            GameSceneNode SceneTop = new GameSceneNode();
            SceneTop.Name = "Scene2DTopRoot";

            scene2DOverlay.Add(SceneFade);
            scene2DOverlay.Add(SceneTop);

            scene3DRoot = new GameSceneNode();
            scene3DRoot.Name = "Scene3DRoot";
        }

        /// <summary>
        /// Allows the game component to perform any initialization
        /// it needs to before starting to run.  This is where it can query 
        /// for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            spriteBatch = new SpriteBatch(FrameworkCore.Game.GraphicsDevice);

            DefaultRenderState();

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //  3D Scene update
            scene3DRoot.Update(gameTime);
            
            //  2D Layers update
            for (int i = 0; i < scene2DLayer.Count; i++)
                scene2DLayer[i].Update(gameTime);

            //  Overlay Layer update
            for (int i = 0; i < scene2DOverlay.Count; i++)
                scene2DOverlay[i].Update(gameTime);            

            base.Update(gameTime);
        }

        /// <summary>
        /// draws every scene node which is attached to the scene node.
        /// Draws in order of 3D scene node, 2D scene node, and overlay text.
        /// When there are more than one view cameras that have registered 
        /// to the 3D scene, Viewport is applied to the 3D scene node, 
        /// which is drawn as many as the number of the view camera.
        /// When render event handler has been registered, it is called 
        /// before or after drawing.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            //  Set to render information
            renderTracer.Device = FrameworkCore.Game.GraphicsDevice;
            renderTracer.GameTime = gameTime;
            renderTracer.SpriteBatch = SpriteBatch;
            renderTracer.Fog = FrameworkCore.Viewer.BasicFog;
            renderTracer.Lighting = FrameworkCore.Viewer.BasicLighting;

            ClearBackBuffer();

            if (RenderingPreDrawScene != null)
            {
                RenderingPreDrawScene(this, EventArgs.Empty);
            }

            //  3D Render
            {
                if (RenderingPreDraw3D != null)
                {
                    RenderingPreDraw3D(this, EventArgs.Empty);
                }

                //  process multiple camera
                for (int i = 0; i < FrameworkCore.CurrentCamera.Count; i++)
                {
                    ViewCamera viewCamera = FrameworkCore.Viewer.CurrentCamera;
                    CameraBase camera = viewCamera.GetCamera(i);

                    renderTracer.View = camera.ViewMatrix;
                    renderTracer.ViewInvert = Matrix.Invert(camera.ViewMatrix);
                    renderTracer.Projection = camera.ProjectionMatrix;

                    // Update frustum
                    UpdateFrustum(camera.ViewMatrix, camera.ProjectionMatrix);
                    renderTracer.Frustum = this.Frustum;

                    //  Set to each viewport
                    ApplyViewport(viewCamera.GetViewport(i));

                    //  Render the 3D scene
                    Draw3D(renderTracer);                    
                }

                //  restore to default viewport
                ApplyViewport(FrameworkCore.DefaultViewport);

                if (RenderingPostDraw3D != null)
                {
                    RenderingPostDraw3D(this, EventArgs.Empty);
                }
            }

            //  2D Render
            {
                if (RenderingPreDraw2D != null)
                {
                    RenderingPreDraw2D(this, EventArgs.Empty);
                }
               
                //  Render the 2D scene
                Draw2D(renderTracer);

                if (RenderingPostDraw2D != null)
                {
                    RenderingPostDraw2D(this, EventArgs.Empty);
                }
            }

            if (RenderingPostDrawScene != null)
            {
                RenderingPostDrawScene(this, EventArgs.Empty);
            }

            // Display overlaped texts or others
            DrawOverlayText(gameTime);

            // Draw other components
            base.Draw(gameTime);            
        }

        /// <summary>
        /// removes every scene node and scene layer that have been attached to 
        /// the scene root node.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (spriteBatch != null)
                {
                    spriteBatch.Dispose();
                    spriteBatch = null;
                }                
            }

            ClearScene3DRoot(disposing);
            ClearScene2DLayer(disposing);
            ClearScene2DOverlay(disposing);

            scene3DRoot = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// applies the specified Viewport to GraphicsDevice.
        /// </summary>
        /// <param name="viewport">a new Viewport</param>
        public static void ApplyViewport(Viewport viewport)
        {
            FrameworkCore.Game.GraphicsDevice.Viewport = viewport;
        }

        /// <summary>
        /// updates the bounding frustum.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="proj"></param>
        public void UpdateFrustum(Matrix view, Matrix proj)
        {
            // Computes the bounding frustum.
            this.frustum = new BoundingFrustum(view * proj);
        }

        /// <summary>
        /// returns the 2D scene layer of the specified index.
        /// </summary>
        /// <param name="index">an index of 2D scene layer</param>
        public GameSceneNode GetScene2DLayer(int index)
        {
            if (scene2DLayer.Count - 1 < index)
            {
                throw new ArgumentException(
                    "Cannot return a 2D layer. Invalid index (" + 
                    index.ToString() + ")");
            }

            return scene2DLayer[index];
        }

        /// <summary>
        /// creates new 2D scene layers.
        /// </summary>
        /// <param name="count">count of layers</param>
        public void CreateScene2DLayer(int count)
        {
            for (int i = 0; i < count; i++)
            {
                scene2DLayer.Add(new GameSceneNode());
            }
        }

        /// <summary>
        /// clears all 2D scene layers.
        /// </summary>
        public void ClearScene2DLayer(bool disposing)
        {
            if (disposing )
            {
                for (int i = 0; i < scene2DLayer.Count; i++)
                    scene2DLayer[i].Dispose();
            }

            scene2DLayer.Clear();
        }

        /// <summary>
        /// clears 2D overlay layer.
        /// </summary>
        /// <param name="disposing"></param>
        public void ClearScene2DOverlay(bool disposing)
        {
            if (disposing )
            {
                for (int i = 0; i < scene2DOverlay.Count; i++)
                    scene2DOverlay[i].Dispose();
            }

            scene2DOverlay.Clear();
        }

        /// <summary>
        /// clears 3D scene layer.
        /// </summary>
        public void ClearScene3DRoot(bool disposing)
        {
            if (scene3DRoot != null)
            {
                if( disposing)
                    scene3DRoot.Dispose();

                scene3DRoot.RemoveAllChild(true);                
            }
        }

        /// <summary>
        /// sets render target.
        /// </summary>
        /// <param name="index">an index of render target</param>
        /// <param name="renderTarget"></param>
        public void SetRenderTarget(int index, RenderTarget2D renderTarget)
        {
            GraphicsDevice.SetRenderTarget(index, renderTarget);
        }


        /// <summary>
        /// clears the back buffer of viewport.
        /// </summary>
        public void ClearBackBuffer()
        {
            //  Screen clear
            FrameworkCore.Game.GraphicsDevice.Clear(ClearOptions.Target |
                                               ClearOptions.DepthBuffer,
                                               ClearColor, 1.0f, 0);
        }

        /// <summary>
        /// sets the default render target.
        /// </summary>
        public void RestoreDefaultRenderTarget()
        {
            GraphicsDevice.SetRenderTarget(0, null);
        }

        /// <summary>
        /// draws the 3D scene.
        /// </summary>
        protected void Draw3D(RenderTracer renderTracer)
        {
            DefaultRenderState();

            //  Draw all 3D objects
            scene3DRoot.Draw(renderTracer);            
        }

        /// <summary>
        /// draws the 2D scene.
        /// </summary>
        protected void Draw2D(RenderTracer renderTracer)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            //  Draw all 2D objects
            for (int i = 0; i < scene2DLayer.Count; i++)
                scene2DLayer[i].Draw(renderTracer);

            //  Draw all 2D overlay
            for (int i = 0; i < scene2DOverlay.Count; i++)
                scene2DOverlay[i].Draw(renderTracer);

            spriteBatch.End();  
        }

        /// <summary>
        /// draws the overlay text.
        /// </summary>
        protected static void DrawOverlayText(GameTime gameTime)
        {
            //  Overlay text on screen
            FrameworkCore.TextManager.Draw(gameTime);
        }

        /// <summary>
        /// sets default render state.
        /// </summary>
        protected static void DefaultRenderState()
        {
            GraphicsDevice device = FrameworkCore.Game.GraphicsDevice;

            device.RenderState.DepthBufferEnable = true;
            device.RenderState.DepthBufferWriteEnable = true;

            device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            // Set to alpha blending
            device.RenderState.AlphaTestEnable = true;
            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            device.RenderState.BlendFunction = BlendFunction.Add;
            device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            device.RenderState.StencilEnable = false;

            // Set 0 and greater alpha compare
            device.RenderState.ReferenceAlpha = 0;
            device.RenderState.AlphaFunction = CompareFunction.Greater;
        }
    }
}


