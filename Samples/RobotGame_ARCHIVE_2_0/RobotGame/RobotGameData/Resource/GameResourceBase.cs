#region File Description
//-----------------------------------------------------------------------------
// GameResourceBase.cs
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
using RobotGameData.GameInterface;
#endregion

namespace RobotGameData.Resource
{
    /// <summary>
    /// the base class from which all resource structures must inherit.
    /// </summary>
    public class GameResourceBase : IDisposable, IIdentity 
    {
        #region Fields
                
        int             id = -1;
        string          keyName = String.Empty;
        string          assetName = String.Empty;
        bool            isDisposed;

        protected object        resource = null;

        #endregion

        #region Properties

        public int Id               
        {
            get
            {
                if (id == 0)
                {
                    id = GetHashCode();
                }
                return id;
            }
        }

        public string Key          
        {
            get { return keyName; } 
        }

        public string AssetName     
        {
            get { return assetName; } 
        }

        public object Resource      
        { 
            get { return resource; }
        }

        public bool IsDisposed     
        { 
            get { return isDisposed; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="assetName">asset name</param>
        public GameResourceBase(string key, string assetName)
        {
            this.keyName = key;
            this.assetName = assetName;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    //if we're manually disposing,
                    //then managed content should be unloaded
                    resource = null;
                }
                isDisposed = true;
            }
        }
    }
}
