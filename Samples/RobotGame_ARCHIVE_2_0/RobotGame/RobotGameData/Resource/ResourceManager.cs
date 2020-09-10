#region File Description
//-----------------------------------------------------------------------------
// ResourceManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.GameObject;
#endregion

namespace RobotGameData.Resource
{
    /// <summary>
    /// It converts the loaded resource files (texture, model) so that 
    /// they are manageable from the inside and searchable from the outside.  
    /// The base class that has the loaded resource is GameResourceBase.
    /// </summary>
    public class ResourceManager : DrawableGameComponent
    {
        #region Fields

        protected bool debugTrace = false;
        protected ContentManager contentManager = null;
        protected long totalResourceMemory = 0;

        protected Dictionary<string, GameResourceBase> ResourceStorage = 
                                            new Dictionary<string, GameResourceBase>();
 
        #endregion

        #region Properties

        public ContentManager ContentManager
        {
            get { return contentManager; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">game</param>
        /// <param name="contentRootDirectory">root directory path of the content</param>
        public ResourceManager(Game game, string contentRootDirectory) : base(game)
        {
            contentManager = new ContentManager(game.Services, contentRootDirectory);
        }

        /// <summary>
        /// Remove all resource elements.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            //  Delete all resources in the storage
            RemoveResourceAll(disposing);

            contentManager.Unload();
            contentManager.Dispose();
            contentManager = null;

            base.Dispose(true);
        }

        /// <summary>
        /// loads a resource file in the content folder.
        /// </summary>
        /// <typeparam name="T">resource type (i.e. Model or Texture2D)</typeparam>
        /// <param name="key">resource key name</param>
        /// <param name="filePath">resource file name</param>
        /// <returns>resource element</returns>
        public GameResourceBase LoadContent<T>(string key, string filePath)
        {
            if (FrameworkCore.Game.GraphicsDevice == null)
                throw new InvalidOperationException("No graphics device.");

            GameResourceBase resource = FindResourceByKey(key);

            if (resource != null)
                return resource;

            //  loads a resource by content manager
            T obj = contentManager.Load<T>(filePath);
            if (obj == null)
                throw new ArgumentException("Fail to load content (" + key + 
                                                    " : " + filePath + ")");

            if (obj is Texture2D)
            {
                resource = new GameResourceTexture2D(key, filePath,
                                                    obj as Texture2D);
            }
            else if (obj is Model)
            {
                resource = new GameResourceModel(key, filePath, obj as Model);
            }
            else if (obj is AnimationSequence)
            {
                resource = new GameResourceAnimation(key, filePath,
                                                        obj as AnimationSequence);
            }
            else if (obj is SpriteFont)
            {
                resource = new GameResourceFont(key, filePath, obj as SpriteFont);
            }
            else if (obj is Effect)
            {
                resource = new GameResourceEffect(key, filePath, obj as Effect);
            }
            else
            {
                throw new NotSupportedException("Not supported the resource");
            }

            if( debugTrace)
            {
                System.Diagnostics.Debug.WriteLine(
                                    string.Format("Load Resource : {0} ({1})", 
                                                filePath, resource.ToString()));
            }

            if (AddResource(resource))
                return resource;

            return null;
        }

        /// <summary>
        /// adds a resource element.
        /// </summary>
        /// <param name="resource">resource element</param>
        public bool AddResource(GameResourceBase resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            if (string.IsNullOrEmpty(resource.Key))
            {
                throw new ArgumentException("The resource contains an invalid key.");
            }
            else if (ResourceStorage.ContainsKey(resource.Key))
            {
                throw new ArgumentException(
                                "The resource is already in the manager.");
            }

            ResourceStorage.Add(resource.Key, resource);

            return true;
        }

        /// <summary>
        /// removes a resource element by key name.
        /// </summary>
        /// <param name="key">resource key name</param>
        public bool RemoveResource(string key, bool disposing)
        {
            if (ResourceStorage.ContainsKey(key))
            {
                if (debugTrace)
                {
                    System.Diagnostics.Debug.WriteLine(
                        string.Format("Dispose Resource : {0} ({1})",
                                        ResourceStorage[key].AssetName, 
                                        ResourceStorage[key].ToString()));
                }

                if( disposing)
                    ResourceStorage[key].Dispose();
            }

            return ResourceStorage.Remove(key);
        }

        /// <summary>
        /// removes a resource element by object.
        /// </summary>
        /// <param name="resource">a resource element</param>
        public bool RemoveResource(GameResourceBase resource, bool disposing)
        {
            return RemoveResource(resource.Key, disposing);
        }

        /// <summary>
        /// remove all resource elements.
        /// </summary>
        public void RemoveResourceAll(bool disposing)
        {
            foreach (string key in ResourceStorage.Keys)
            {
                if (debugTrace)
                {
                    System.Diagnostics.Debug.WriteLine(
                        string.Format("Dispose Resource : {0} ({1})",
                                        ResourceStorage[key].AssetName, 
                                        ResourceStorage[key].ToString()));
                }

                if( disposing)
                    ResourceStorage[key].Dispose();
            }

            ResourceStorage.Clear();

            // Clean up some garbage
            GC.Collect();
        }

        /// <summary>
        /// removes a resource element by object.
        /// </summary>
        /// <param name="resource">resource element object</param>
        public bool RemoveResourceByObject(object resource, bool disposing)
        {
            //  Finding the resource in storage by object
            GameResourceBase res = FindResource(resource);            
            if( res != null)
            {
                return RemoveResource(res, disposing);
            }

            return false;
        }

        /// <summary>
        /// finds a resource element by key name.
        /// </summary>
        /// <param name="key">resource key name</param>
        public GameResourceBase FindResourceByKey(string key)
        {
            //  Finding the resource in storage by key
            if (ResourceStorage.ContainsKey(key))
            {
                return ResourceStorage[key];
            }

            return null;
        }

        /// <summary>
        /// finds a resource element by id.
        /// </summary>
        /// <param name="id">resource id number</param>
        public GameResourceBase FindResourceById(int id)
        {
            //  Finding the resource in storage by ID
            foreach (GameResourceBase resource in ResourceStorage.Values)
            {
                if (resource.Id == id)
                    return resource;                
            }

            return null;
        }

        /// <summary>
        /// finds a resource element by asset name.
        /// </summary>
        /// <param name="assetName">resource asset name</param>
        public GameResourceBase FindResourceByAssetName(string assetName)
        {
            //  Finding the resource in storage by name
            foreach (GameResourceBase resource in ResourceStorage.Values)
            {
                if (resource.AssetName == assetName)
                    return resource;
            }

            return null;
        }

        /// <summary>
        /// finds a resource element by object.
        /// </summary>
        /// <param name="resource">resource element object</param>
        public GameResourceBase FindResource(object resource)
        {
            //  Finding the resource in storage
            foreach (GameResourceBase res in ResourceStorage.Values)
            {
                if (res.Resource.Equals(resource))
                    return res;
            }

            return null;
        }

        /// <summary>
        /// gets a texture by key name.
        /// </summary>
        /// <param name="key">resource key name</param>
        public GameResourceTexture2D GetTexture2D(string key)
        {
            return (GameResourceTexture2D)FindResourceByKey(key);
        }

        /// <summary>
        /// gets a model resource by key name.
        /// </summary>
        /// <param name="key">resource key name</param>
        public GameResourceModel GetModel(string key)
        {
            return (GameResourceModel)FindResourceByKey(key);
        }

        /// <summary>
        /// gets a animation resource by key name.
        /// </summary>
        /// <param name="key">resource key name</param>
        public GameResourceAnimation GetAnimation(string key)
        {
            return (GameResourceAnimation)FindResourceByKey(key);
        }

        /// <summary>
        /// gets a font resource by key name.
        /// </summary>
        /// <param name="key">resource key name</param>
        public GameResourceFont GetFont(string key)
        {
            return (GameResourceFont)FindResourceByKey(key);
        }

        /// <summary>
        /// gets a effect resource by key name.
        /// </summary>
        /// <param name="key">resource key name</param>
        public GameResourceEffect GetEffect(string key)
        {
            return (GameResourceEffect)FindResourceByKey(key);
        }

        /// <summary>
        /// loads a effect file(.fx).
        /// </summary>
        /// <param name="fileName">effect file name in the content folder</param>
        /// <returns>resource element</returns>
        public GameResourceEffect LoadEffect(string fileName)
        {
            string keyName = Path.GetFileName(fileName);

            //  Find the texture resource from ResourceManager by file name
            GameResourceEffect resource = GetEffect(keyName);

            //  If can't find stored resource
            if (resource == null)
            {
                LoadContent<Effect>(keyName, fileName);

                resource = GetEffect(keyName);
            }

            //  Can't get resource If loading failed!
            if (resource == null)
            {
                throw new ArgumentException("Fail to effect : " + fileName);
            }
            else if (resource.Effect.IsDisposed)
            {
                throw new InvalidOperationException(
                    "Already disposed texture : " + fileName);
            }

            return resource;
        }

        /// <summary>
        /// loads a texture file (i.e. tga or bmp).
        /// </summary>
        /// <param name="fileName">image file name in the content folder</param>
        /// <returns>resource element</returns>
        public GameResourceTexture2D LoadTexture(string fileName)
        {
            string keyName = Path.GetFileName(fileName);

            //  Find the texture resource from ResourceManager by file name
            GameResourceTexture2D resource = GetTexture2D(keyName);

            //  If can't find stored resource
            if (resource == null)
            {
                LoadContent<Texture2D>(keyName, fileName);

                resource = GetTexture2D(keyName);
            }

            //  Can't get resource If loading failed!
            if (resource == null)
            {
                throw new ArgumentException("Fail to loaded texture : " + fileName);
            }
            else if (resource.Texture2D.IsDisposed)
            {
                throw new InvalidOperationException(
                    "Already disposed texture : " + fileName);
            }

            return resource;
        }
        
        /// <summary>
        /// loads an animation file (.Animation)
        /// </summary>
        /// <param name="fileName">animation file name in the content folder</param>
        /// <returns>resource element</returns>
        public GameResourceAnimation LoadAnimation(string fileName)
        {
            //  Find the texture resource from ResourceManager by file name
            GameResourceAnimation resource = GetAnimation(fileName);

            //  If can't find stored resource
            if (resource == null)
            {
                LoadContent<AnimationSequence>(fileName, fileName);

                resource = GetAnimation(fileName);
            }

            //  Can't get resource If loading failed!
            if (resource == null)
            {
                throw new ArgumentException("Fail to loaded animation : " +
                    fileName);
            }

            return resource;
        }
    }
}
