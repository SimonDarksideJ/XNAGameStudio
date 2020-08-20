#region File Description
//-----------------------------------------------------------------------------
// LightList.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion



namespace ShipGame
{
    public struct Light 
    {
        public Vector3 position;        // position
        public float radius;       // radius
        public Vector3 color;      // color

        /// <summary>
        /// Create a new list of lights
        /// </summary>
        public Light(Vector3 lightPosition, float lightRadius, Vector3 lightColor)
        {
            position = lightPosition;
            radius = lightRadius;
            color = lightColor;
        }

        /// <summary>
        /// Set light properties to given effect
        /// </summary>
        public void SetEffect(
                EffectParameter effectLightPosition,
                EffectParameter effectLightColor,
                Matrix worldInverse)
        {
            Vector4 positionRadius = 
                new Vector4(Vector3.Transform(position,worldInverse),radius);
            if (effectLightPosition != null)
            {
                effectLightPosition.SetValue(positionRadius);
            }
            if (effectLightColor != null)
            {
                effectLightColor.SetValue(color);
            }
        }
    }

    public class LightList
    {
        // ambient light
        public Vector3 ambient = new Vector3(0.3f,0.3f,0.3f);

        // list of lights
        public List<Light> lights = new List<Light>();

        /// <summary>
        /// Saves the light list to a xml file
        /// </summary>
        public bool Save(String filename)
        {
            // create stream
            Stream stream;
            stream = File.Create(filename);
            if (stream == null)
                return false;

            // serialize
            XmlSerializer serializer = new XmlSerializer(typeof(LightList));
            serializer.Serialize(stream, this);
            serializer = null;

            // close
            stream.Close();
            stream = null;

            return true;
        }

        /// <summary>
        /// Static method to load a light list from a file
        /// </summary>
        public static LightList Load(String filename)
        {
            // open file
            Stream stream;
            try
            {
                stream = File.OpenRead(filename);
            }
            catch (FileNotFoundException e)
            {
                System.Console.WriteLine("LightList load error:" + e.Message);
                stream = null;
            }
            if (stream == null)
                return null;

            // load data
            XmlSerializer serializer = new XmlSerializer(typeof(LightList));
            LightList environmentLights = (LightList)serializer.Deserialize(stream);
            serializer = null;

            // close
            stream.Close();
            stream = null;

            return environmentLights;
        }
    }
}
