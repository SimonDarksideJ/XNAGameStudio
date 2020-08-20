#region File Description
//-----------------------------------------------------------------------------
// Distorter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace DistortionSample
{
    /// <summary>
    /// A combination of model and distortion technique.
    /// </summary>
    public class Distorter
    {
        public string ModelName = String.Empty;
        public Model Model = null;
        public float DistortionScale = 0.005f;
        public Matrix World = Matrix.Identity;
        public DistortionComponent.DistortionTechnique Technique = 
            DistortionComponent.DistortionTechnique.ZeroDisplacement;
        public bool DistortionBlur = false;

        public override string ToString()
        {
            string output = 
                DistortionComponent.GetDistortionTechniqueFriendlyName(Technique) + 
                " (" + ModelName + ")";
            if (DistortionBlur)
            {
                output += ", Blurred";
            }
            return output;
        }
    }
}
