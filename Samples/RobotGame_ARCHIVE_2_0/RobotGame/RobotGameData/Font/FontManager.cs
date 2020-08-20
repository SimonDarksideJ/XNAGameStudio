#region File Description
//-----------------------------------------------------------------------------
// FontManager.cs
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
using RobotGameData.Resource;
#endregion

namespace RobotGameData.Font
{
    /// <summary>
    /// It loads font file (.spritefont) and manages SpriteFont class.
    /// </summary>
    public class FontManager
    {
        #region Fields

        protected Dictionary<string, SpriteFont> fontList 
                                                = new Dictionary<string, SpriteFont>();

        #endregion

        public void Dispose()
        {
            fontList.Clear();
        }

        /// <summary>
        /// Creates a sprite font
        /// </summary>
        /// <param name="key">font name</param>
        /// <param name="szFile">font file name</param>
        /// <returns>sprite font</returns>
        public SpriteFont CreateFont(string key, string szFile)
        {
            if (fontList.ContainsKey(key))
                return fontList[key];

            //  First, Find SpriteFont resource from ResourceManager by key
            GameResourceFont resource = FrameworkCore.ResourceManager.GetFont(szFile);
            if (resource == null)
            {
                FrameworkCore.ResourceManager.LoadContent<SpriteFont>(szFile, szFile);

                resource = FrameworkCore.ResourceManager.GetFont(szFile);
            }

            if (resource.SpriteFont == null)
                throw new ArgumentException("Failed to load font (" + szFile + ")");

            fontList.Add(key, resource.SpriteFont);

            return resource.SpriteFont;
        }

        public SpriteFont GetFont(string key)
        {
            if (!fontList.ContainsKey(key))
                return null;

            return fontList[key];
        }

        public void RemoveFont(SpriteFont font)
        {
            foreach (string key in fontList.Keys)
            {
                if (fontList[key].Equals(font))
                {
                    RemoveFont(key);
                }
            }
        }

        public void RemoveFont(string key)
        {
            if (!fontList.ContainsKey(key))   return;

            FrameworkCore.ResourceManager.RemoveResourceByObject(fontList[key], true);

            fontList.Remove(key);
        }
    }
}


