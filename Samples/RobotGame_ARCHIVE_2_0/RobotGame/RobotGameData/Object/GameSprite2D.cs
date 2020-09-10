#region File Description
//-----------------------------------------------------------------------------
// GameSprite2D.cs
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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.Render;
using RobotGameData.Resource;
using RobotGameData.Collision;
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData.GameObject
{
    #region Sprite2DObject

    /// <summary>
    /// this structure stores an 2D sprite object’s information.
    /// </summary>
    public class Sprite2DObject : INamed
    {
        #region Fields

        String name = String.Empty;        
        Rectangle screenRectangle = Rectangle.Empty;
        Rectangle sourceRectangle = Rectangle.Empty;
        Color color = new Color(0xFF, 0xFF, 0xFF);
        float rotation = 0.0f;
        float scale = 1.0f;
        bool visible = true;

        #endregion

        #region Properties
                
        public string Name 
        { 
            get { return name; }
            set { name = value; }
        }        
        public bool Visible 
        { 
            get { return visible; }
            set { visible = value; }
        }
        public Rectangle ScreenRectangle 
        {
            get { return screenRectangle; }
            set { screenRectangle = value; }
        }
        public Vector2 ScreenPosition 
        {
            get 
            { 
                return new Vector2(screenRectangle.X, screenRectangle.Y); 
            }
            set 
            { 
                screenRectangle.X = (int)value.X; 
                screenRectangle.Y = (int)value.Y; 
            }
        }
        public Vector2 ScreenSize 
        {
            get 
            { 
                return new Vector2(screenRectangle.Width, screenRectangle.Height); 
            }
            set 
            { 
                screenRectangle.Width = (int)value.X; 
                screenRectangle.Height = (int)value.Y; 
            }
        }
        public Rectangle SourceRectangle
        {
            get { return sourceRectangle; }
            set { sourceRectangle = value; }
        }
        public Vector2 SourcePosition
        {
            get 
            { 
                return new Vector2(sourceRectangle.X, sourceRectangle.Y); 
            }
            set 
            { 
                sourceRectangle.X = (int)value.X; 
                sourceRectangle.Y = (int)value.Y; 
            }
        }
        public Vector2 SourceSize
        {
            get 
            { 
                return new Vector2(sourceRectangle.Width, sourceRectangle.Height); 
            }
            set 
            { 
                sourceRectangle.Width = (int)value.X; 
                sourceRectangle.Height = (int)value.Y; 
            }
        }
        public Color Color
        { 
            get { return color; }
            set { color = value; }
        }
        public byte Alpha
        {
            get { return color.A; }
            set { color = new Color(color.R, color.R, color.B, value); }
        }
        public float Rotation 
        { 
            get { return rotation; }
            set { rotation = value; }
        }
        public float Scale 
        { 
            get { return scale; }
            set { scale = value; }
        }

        #endregion

        #region Constructors

        public Sprite2DObject(string name, Texture2D tex)
        {
            Name = name;
            ScreenRectangle = new Rectangle(0, 0, tex.Width, tex.Height);
            SourceRectangle = new Rectangle(0, 0, tex.Width, tex.Height);
        }

        public Sprite2DObject(string name, Texture2D tex, Vector2 screenPosition)
        {
            Name = name;
            
            ScreenRectangle = new Rectangle(
                (int)screenPosition.X, (int)screenPosition.Y, 
                tex.Width, tex.Height);

            SourceRectangle = new Rectangle(0, 0, tex.Width, tex.Height);
        }

        public Sprite2DObject(string name, Texture2D tex, 
                              Vector2 screenPosition, Vector2 screenSize)
        {
            Name = name;

            ScreenRectangle = new Rectangle(
                (int)screenPosition.X, (int)screenPosition.Y, 
                (int)screenSize.X, (int)screenSize.Y);

            SourceRectangle = new Rectangle(0, 0, tex.Width, tex.Height);
        }

        public Sprite2DObject(string name, 
                              Vector2 screenPosition, Vector2 screenSize, 
                              Vector2 sourcePosition, Vector2 sourceSize)
        {
            Name = name;
            
            ScreenRectangle = new Rectangle(
                (int)screenPosition.X, (int)screenPosition.Y, 
                (int)screenSize.X, (int)screenSize.Y);

            SourceRectangle = new Rectangle(
                (int)sourcePosition.X, (int)sourcePosition.Y, 
                (int)sourceSize.X, (int)sourceSize.Y);
        }

        public Sprite2DObject(string name, Texture2D tex, 
                              Vector2 screenPosition, Rectangle sourceRectangle)
        {
            Name = name;
            
            ScreenRectangle = new Rectangle(
                (int)screenPosition.X, (int)screenPosition.Y, 
                tex.Width, tex.Height);

            SourceRectangle = sourceRectangle;
        }

        public Sprite2DObject(string name, Rectangle screenRectangle,
            Rectangle sourceRectangle)
        {
            Name = name;
            ScreenRectangle = screenRectangle;
            SourceRectangle = sourceRectangle;
        }

        #endregion
    }

    #endregion

    /// <summary>
    /// this sprite draws an image to the 2D screen.
    /// </summary>
    public class GameSprite2D : GameSceneNode
    {
        #region Fields

        Texture2D texture2D = null;
        List<Sprite2DObject> spriteList = new List<Sprite2DObject>();

        #endregion

        #region Properties

        public Texture2D TextureResource { get { return texture2D; } }

        #endregion

        /// <summary>
        /// draws the registered sprite objects by using the sprite batch.
        /// </summary>
        /// <param name="renderTracer"></param>
        protected override void OnDraw(RenderTracer renderTracer)
        {
            if (TextureResource == null)
                throw new InvalidOperationException("The texture is empty");
            else if (TextureResource.IsDisposed )
                throw new ObjectDisposedException("TextureResource");

            for (int i = 0; i < spriteList.Count; i++)
            {
                Sprite2DObject sprite = spriteList[i];

                if (sprite != null)
                {
                    //  If active visible
                    if (sprite.Visible )
                    {
                        renderTracer.SpriteBatch.Draw(TextureResource,
                                      sprite.ScreenRectangle, sprite.SourceRectangle,
                                      sprite.Color);
                    }
                }
            }
        }

        protected override void UnloadContent()
        {
            spriteList.Clear();

            base.UnloadContent();
        }
        
        /// <summary>
        /// creates 2D sprite objects using the texture.
        /// </summary>
        /// <param name="count">sprite objects count</param>
        /// <param name="fileName">texture file name</param>
        public void Create(int count, string fileName)
        {
            for (int i = 0; i < count; i++)
                spriteList.Add(null);

            GameResourceTexture2D resource = 
                                FrameworkCore.ResourceManager.LoadTexture(fileName);

            texture2D = resource.Texture2D;
        }

        /// <summary>
        /// add a sprite object.
        /// </summary>
        /// <param name="index">an index of sprite object</param>
        /// <param name="name">sprite object name</param>
        /// <returns></returns>
        public Sprite2DObject AddSprite(int index, string name)
        {
            if (TextureResource == null)
                throw new InvalidOperationException("The texture is empty");

            Sprite2DObject newSprite = new Sprite2DObject(name, TextureResource);

            AddSprite(index, newSprite);

            return newSprite;
        }

        /// <summary>
        /// add a sprite object.
        /// </summary>
        /// <param name="index">an index of sprite object</param>
        /// <param name="name">sprite object name</param>
        /// <param name="screenPosition">
        /// 2D screen position of sprite object (pixel)
        /// </param>
        /// <returns></returns>
        public Sprite2DObject AddSprite(int index, string name, Vector2 screenPosition)
        {
            if (TextureResource == null)
                throw new InvalidOperationException("The texture is empty");

            Sprite2DObject newSprite = 
                new Sprite2DObject(name, TextureResource, screenPosition);

            AddSprite(index, newSprite);

            return newSprite;
        }

        /// <summary>
        /// add a sprite object.
        /// </summary>
        /// <param name="index">an index of sprite object</param>
        /// <param name="name">sprite object name</param>
        /// <param name="screenPosition">
        /// 2D screen position of sprite object (pixel)
        /// </param>
        /// <param name="screenSize">2D screen size of sprite object (pixel)</param>
        /// <returns></returns>
        public Sprite2DObject AddSprite(int index, string name, 
                                        Vector2 screenPosition, Vector2 screenSize)
        {
            if (TextureResource == null)
                throw new InvalidOperationException("The texture is empty");

            Sprite2DObject newSprite =
                new Sprite2DObject(name, TextureResource, screenPosition, screenSize);

            AddSprite(index, newSprite);

            return newSprite;
        }

        /// <summary>
        /// add a sprite object.
        /// </summary>
        /// <param name="index">an index of sprite object</param>
        /// <param name="name">sprite object name</param>
        /// <param name="screenPosition">
        /// 2D screen position of sprite object (pixel)
        /// </param>
        /// <param name="screenSize">2D screen size of sprite object (pixel)</param>
        /// <param name="sourcePosition">position of the source image (pixel)</param>
        /// <param name="sourceSize">size of the source image (pixel)</param>
        /// <returns></returns>
        public Sprite2DObject AddSprite(int index, string name, 
                                        Vector2 screenPosition, Vector2 screenSize, 
                                        Vector2 sourcePosition, Vector2 sourceSize)
        {
            if (TextureResource == null)
                throw new InvalidOperationException("The texture is empty");

            Sprite2DObject newSprite = new Sprite2DObject(name,
                                                          screenPosition, screenSize, 
                                                          sourcePosition, sourceSize);

            AddSprite(index, newSprite);

            return newSprite;
        }

        /// <summary>
        /// add a sprite object.
        /// </summary>
        /// <param name="index">an index of sprite object</param>
        /// <param name="name">sprite object name</param>
        /// <param name="screenPosition">
        /// 2D screen position of sprite object (pixel)
        /// </param>
        /// <param name="sourceRectangle">a rectangle of the source image (pixel)</param>
        /// <returns></returns>
        public Sprite2DObject AddSprite(int index, string name,
                                        Vector2 screenPosition, 
                                        Rectangle sourceRectangle)
        {
            if (TextureResource == null)
                throw new InvalidOperationException("The texture is empty");

            Sprite2DObject newSprite = new Sprite2DObject(name, TextureResource,
                screenPosition, sourceRectangle);

            AddSprite(index, newSprite);

            return newSprite;
        }

        /// <summary>
        /// add a sprite object.
        /// </summary>
        /// <param name="index">an index of sprite object</param>
        /// <param name="name">sprite object name</param>
        /// <param name="screenRectangle">a rectangle of sprite object (pixel)</param>
        /// <param name="sourceRectangle">a rectangle of the source image (pixel)</param>
        /// <returns></returns>
        public Sprite2DObject AddSprite(int index, string name, 
                                        Rectangle screenRectangle, 
                                        Rectangle sourceRectangle)
        {
            if (TextureResource == null)
                throw new InvalidOperationException("The texture is empty");

            Sprite2DObject newSprite = 
                new Sprite2DObject(name, screenRectangle, sourceRectangle);

            AddSprite(index, newSprite);

            return newSprite;
        }

        /// <summary>
        /// add a sprite object.
        /// </summary>
        /// <param name="index">an index of sprite object</param>
        /// <param name="spriteObject">source sprite object</param>
        public void AddSprite(int index, Sprite2DObject spriteObject)
        {
            if (TextureResource == null)
                throw new InvalidOperationException("The texture is empty");

            if (spriteList.Count <= index || index < 0)
                throw new ArgumentException(
                    "Cannot add sprite. Invalid index : " + index.ToString());
            
            if( spriteList[index] != null)
                throw new ArgumentException(
                    "Cannot add sprite. already exist other sprite : " +
                    index.ToString());

            spriteList[index] = spriteObject;
        }

        /// <summary>
        /// gets the sprite object.
        /// </summary>
        /// <param name="index">an index of sprite object</param>
        /// <returns>sprite object</returns>
        public Sprite2DObject GetSprite(int index)
        {
            if( spriteList.Count >= index || index < 0)
                throw new ArgumentException( "Invalid index : " +
                    index.ToString());

            return spriteList[index];
        }

        /// <summary>
        /// configures visibility of all sprite objects.
        /// </summary>
        /// <param name="visible">visibility flag</param>
        public void VisibleSprite(bool visible)
        {
            for (int i = 0; i < spriteList.Count; i++)
                spriteList[i].Visible = visible;
        }

        /// <summary>
        /// configures a visibility of each sprite object.
        /// </summary>
        /// <param name="index">an index of sprite object</param>
        /// <param name="visible">visibility flag</param>
        public void VisibleSprite(int index, bool visible)
        {
            GetSprite(index).Visible = visible;
        }

        /// <summary>
        /// finds an index of sprite object by object
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public int FindSpriteIndex(Sprite2DObject sprite)
        {
            return spriteList.IndexOf(sprite);
        }

        /// <summary>
        /// swaps two sprite objects and changes the priority and the 
        /// position in the list.
        /// </summary>
        /// <param name="tex1"></param>
        /// <param name="tex2"></param>
        public void SwapSprite(Sprite2DObject tex1, Sprite2DObject tex2)
        {
            int index1 = FindSpriteIndex(tex1);

            //  Cannot find sprite
            if( index1 < 0)
                throw new ArgumentException("Cannot find tex1");

            int index2 = FindSpriteIndex(tex2);

            //  Cannot find sprite
            if (index2 < 0)
                throw new ArgumentException("Cannot find tex2");

            spriteList[index1] = tex2;
            spriteList[index2] = tex1;
        }
    }
}