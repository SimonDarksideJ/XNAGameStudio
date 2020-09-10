#region File Description
//-----------------------------------------------------------------------------
// EntityList.cs
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
#endregion



namespace ShipGame
{
    public struct Entity
    {
        public String name;            // entity name
        public Matrix transform;       // entity transform matrix

        /// <summary>
        /// Create a new entity with given name and transform matrix
        /// </summary>
        public Entity(String entityName, Matrix entityTransform)
        {
            name = entityName;
            transform = entityTransform;
        }
    }

    public class EntityList
    {
        // entities list
        public List<Entity> entities = new List<Entity>();

        // last random number generated (to prevent repetition)
        int lastRandom = -1;

        /// <summary>
        /// Get the entity transform matrix
        /// </summary>
        public Matrix GetTransform(String name)
        {
            foreach (Entity e in entities)
            {
                if (e.name == name)
                {
                    return e.transform;
                }
            }

            return Matrix.Identity;
        }

        /// <summary>
        /// Get a random transform matrix from the list preventing repetiton
        /// </summary>
        public Matrix GetTransformRandom(Random random)
        {
            // if no itens return indentity
            if (entities.Count == 0)
                return Matrix.Identity;

            // if only one item available return it
            if (entities.Count == 1)
                return entities[0].transform;

            // pick a random item different from the last one
            int rnd;
            do 
            { 
                rnd = random.Next(entities.Count); 
            } while (rnd == lastRandom);
            
            // set new last random number
            lastRandom = rnd;

            // return transform for random pick
            return entities[rnd].transform;
        }

        /// <summary>
        /// Get the list of entities
        /// </summary>
        public List<Entity> Entities
        {
            get { return entities; }
        }

        /// <summary>
        /// Save the list to a xml file
        /// </summary>
        public bool Save(String filename)
        {
            // open stream
            Stream stream;
            stream = File.Create(filename);
            if (stream == null)
                return false;

            // serialize
            XmlSerializer serializer = new XmlSerializer(typeof(EntityList));
            serializer.Serialize(stream, this);
            serializer = null;

            // close
            stream.Close();
            stream = null;

            return true;
        }

        /// <summary>
        /// Static function to load a entity list from a xml file
        /// </summary>
        public static EntityList Load(String filename)
        {
            // open file
            Stream stream;
            try
            {
                stream = File.OpenRead(filename);
            }
            catch (FileNotFoundException e)
            {
                System.Console.WriteLine("EntityList load error:" + e.Message);
                stream = null;
            }
            if (stream == null)
                return null;

            // serialize
            XmlSerializer serializer = new XmlSerializer(typeof(EntityList));
            EntityList entityList = (EntityList)serializer.Deserialize(stream);
            serializer = null;

            // close
            stream.Close();
            stream = null;

            return entityList;
        }
    }
}
