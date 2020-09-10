#region File Description
//-----------------------------------------------------------------------------
// PictureSet.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Pickture
{
    /// <summary>
    /// One or more pictures which are loaded and unloaded together.
    /// </summary>
    class PictureSet
    {
        /// <summary>
        /// Names of each picture. On Xbox, these are content names passed to the 
        /// content manager's Load method. On Windows, these are file paths.
        /// </summary>
        string[] names;

        /// <summary>
        /// Pictures loaded as textures. This array is parallel to the names array.
        /// </summary>
        Texture2D[] textures;

        /// <summary>
        /// On Xbox, this content manager is responsible for loading and unloading the
        /// textures. This is not needed on Windows, because Texture2D.FromFile is used.
        /// </summary>
        ContentManager content;

        /// <summary>
        /// Constructs an instance of PictureSet with the specified pictures.
        /// </summary>
        /// <param name="names">On Xbox, these are content paths.
        /// On Windows, they are file paths.</param>
        public PictureSet(string[] names)
        {
            this.names = names;
            textures = new Texture2D[names.Length];

            content = new ContentManager(Pickture.Instance.Services);
        }

        /// <summary>
        /// Load all of the pictures into textures.
        /// </summary>
        public void Load()
        {
            for (int i = 0; i < names.Length; i++)
                textures[i] = Load(names[i]);
        }

        /// <summary>
        /// Loads a single picture into a texture.
        /// </summary>
        static Texture2D Load(string name)
        {
            return Pickture.Instance.Content.Load<Texture2D>(name);
        }

        /// <summary>
        /// Unloads all of the textures.
        /// </summary>
        public void Unload()
        {
            content.Unload();
        }

        /// <summary>
        /// Gets a picture's texture.
        /// </summary>
        public Texture2D GetTexture(int index)
        {
            return textures[index];
        }

        /// <summary>
        /// The number of pictures stored in this set.
        /// </summary>
        public int Count
        {
            get { return names.Length; }
        }
    }
}