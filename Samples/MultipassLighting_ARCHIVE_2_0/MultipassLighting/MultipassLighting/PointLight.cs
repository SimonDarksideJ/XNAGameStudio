#region File Description
//-----------------------------------------------------------------------------
// PointLight.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace MultipassLightingSample
{
    class PointLight
    {
        private float rangeValue = 30f;
        private float falloffValue = 2f;
        private Vector4 positionValue;
        private Color colorValue = Color.White;

        public PointLight(Vector4 initialPosition)
        {
            positionValue = initialPosition;
        }

        #region Light Properties
        public Vector4 Position
        {
            set
            {
                positionValue = value;
            }
            get
            {
                return positionValue;
            }
        }


        public Color Color
        {
            set
            {
                colorValue = value;
            }
            get
            {
                return colorValue;
            }
        }

        public float Range
        {
            set
            {
                rangeValue = value;
            }
            get
            {
                return rangeValue;
            }
        }


        public float Falloff
        {
            set
            {
                falloffValue = value;
            }
            get
            {
                return falloffValue;
            }
        }

        public void UpdateLight(EffectParameter lightParameter)
        {
            lightParameter.StructureMembers["position"].SetValue(positionValue);
            lightParameter.StructureMembers["falloff"].SetValue(falloffValue);
            lightParameter.StructureMembers["range"].SetValue(rangeValue);
            lightParameter.StructureMembers["color"].SetValue(
                    colorValue.ToVector4());
        }

        #endregion
    }
}
