#region File Description
//-----------------------------------------------------------------------------
// GameResourceModel.cs
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

namespace RobotGameData.Resource
{
    /// <summary>
    /// saves the transform matrix of the read-in model class and the bones of 
    /// the model’s initial state.
    /// </summary>
    public class ModelData
    {
        public Model model = null;
        public Matrix[] boneTransforms = null;
    }

    /// <summary>
    /// a resource element structure with Model class.
    /// When a model(.FBX or .X) file is loaded from the resource manager, 
    /// it gets stored here.
    /// </summary>
    public class GameResourceModel : GameResourceBase
    {
        #region Fields

        ModelData modelData = new ModelData();

        #endregion

        #region Properties

        public ModelData ModelData
        {
            get { return modelData; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">key name</param>
        /// <param name="assetName">asset name</param>
        /// <param name="resource">model resource</param>
        public GameResourceModel(string key, string assetName, Model resource)
            : base(key, assetName)
        {
            this.modelData.model = resource;

            this.modelData.boneTransforms = new Matrix[resource.Bones.Count];
            this.modelData.model.CopyBoneTransformsTo(this.modelData.boneTransforms);

            this.resource = (object)this.modelData;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (modelData != null)
                {
                    modelData.model = null;
                    modelData = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
