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

namespace MaterialsAndLightsSample
{
    class PointLight
    {
        //////////////////////////////////////////////////////////////
        // Example 3.1                                              //
        //                                                          //
        // The only parameter that will be changing for every light //
        // each frame is the postion parameter.  For the sake of    //
        // reducing string look-ups, the position parameter is      //
        // stored as a parameter instance.  The other parameters    //
        // are updated much less frequently, so a tradeoff has been //
        // made for clarity.                                        //
        //////////////////////////////////////////////////////////////
        private EffectParameter positionParameter;
        private EffectParameter instanceParameter;
        private float rangeValue = 30f;
        private float falloffValue = 2f;
        private Vector4 positionValue;
        private Color colorValue = Color.White;

        public PointLight(Vector4 initialPosition, EffectParameter lightParameter)
        {
            //////////////////////////////////////////////////////////////
            // Example 3.2                                              //
            //                                                          //
            // An instance of a light is bound to an instance of a      //
            // Light structure defined in the effect.  The              //
            // "StructureMembers" property of a parameter is used to    //
            // access the individual fields of a structure.             //
            //////////////////////////////////////////////////////////////
            instanceParameter = lightParameter;
            positionParameter = instanceParameter.StructureMembers["position"];
            Position = initialPosition;
            instanceParameter.StructureMembers["range"].SetValue(rangeValue);
            instanceParameter.StructureMembers["falloff"].SetValue(falloffValue);
            instanceParameter.StructureMembers["color"].SetValue(
                colorValue.ToVector4());
        }

        #region Light Properties
        public Vector4 Position
        {
            set
            {
                positionValue = value;
                positionParameter.SetValue(positionValue);
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
                instanceParameter.StructureMembers["color"].SetValue(
                    colorValue.ToVector4());
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
                instanceParameter.StructureMembers["range"].SetValue(rangeValue);
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
                instanceParameter.StructureMembers["falloff"].SetValue(falloffValue);
            }
            get
            {
                return falloffValue;
            }
        }

        #endregion
    }
}
