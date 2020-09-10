#region File Description
//-----------------------------------------------------------------------------
// CpuSkinnedModel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CpuSkinningDataTypes
{
    public class CpuSkinnedModel
    {
        private readonly List<CpuSkinnedModelPart> modelParts = new List<CpuSkinnedModelPart>();

        /// <summary>
        /// Gets the SkinningData associated with this model.
        /// </summary>
        public SkinningData SkinningData { get; internal set; }

        /// <summary>
        /// Gets a collection of the model parts that make up this model.
        /// </summary>
        public ReadOnlyCollection<CpuSkinnedModelPart> Parts { get; internal set; }

        internal CpuSkinnedModel(List<CpuSkinnedModelPart> modelParts, SkinningData skinningData)
        {
            this.modelParts = modelParts;
            this.SkinningData = skinningData;
            this.Parts = new ReadOnlyCollection<CpuSkinnedModelPart>(this.modelParts);
        }

        /// <summary>
        /// Sets the bone matrices for all model parts.
        /// </summary>
        public void SetBones(Matrix[] bones)
        {
            foreach (var part in modelParts)
            {
                part.SetBones(bones);
            }
        }

        /// <summary>
        /// Draws the model using the specified camera matrices and the default
        /// lighting model.
        /// </summary>
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            foreach (var part in modelParts)
            {
                // use the new effect interfaces so this code will still work
                // even if someone changes the effect on the part to a new type.

                IEffectMatrices matrices = part.Effect as IEffectMatrices;
                if (matrices != null)
                {
                    matrices.World = world;
                    matrices.View = view;
                    matrices.Projection = projection;
                }

                IEffectLights lights = part.Effect as IEffectLights;
                if (lights != null)
                {
                    lights.EnableDefaultLighting();
                }

                part.Draw();
            }
        }
    }
}
