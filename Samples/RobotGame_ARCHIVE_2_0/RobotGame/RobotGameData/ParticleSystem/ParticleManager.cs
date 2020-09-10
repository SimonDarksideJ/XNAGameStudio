#region File Description
//-----------------------------------------------------------------------------
// ParticleManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using RobotGameData.Helper;
#endregion

namespace RobotGameData.ParticleSystem
{
    #region Particle Reader

    /// <summary>
    /// this particle information gets ready by XML file.
    /// It contains the particle name, particle file path, 
    /// and the maximum instance number that are to be used in the game.
    /// </summary>
    [Serializable]
    public class ParticleReaderElement
    {
        public string EntryName = String.Empty;
        public string RelativeFilePath = String.Empty;        
        public int UseMaxCount = 0;
    }

    #endregion

    #region Particle Reader List

    /// <summary>
    /// a set of ParticleReaderElement’s to be read from an XML file.
    /// </summary>
    [Serializable]
    public class ParticleReaderList
    {
        public List<ParticleReaderElement> ParticleReaderElements = null;
    }

    #endregion

    #region Particle Storage

    /// <summary>
    /// a storage in which particle’s instance will be contained.
    /// </summary>
    public class ParticleStorage
    {
        public string entryName = String.Empty;
        public List<ParticleSequence> list = new List<ParticleSequence>();
    }

    #endregion

    /// <summary>
    /// It can load and add each particle and provides an interface 
    /// that plays each particle.
    /// Each particle is in ParticleSequence class.
    /// </summary>
    public class ParticleManager
    {
        #region Fields

        bool particleOn = true;      //  Activate all particles

        List<ParticleStorage> particleSequencesList = new List<ParticleStorage>();
        List<ParticleSequenceInfo> particleInfoList = new List<ParticleSequenceInfo>();

        #endregion

        /// <summary>
        /// Remove all particles
        /// </summary>
        public void Clear()
        {
            ClearAllParticles();

            particleInfoList.Clear();
        }

        /// <summary>
        /// Adds a new particle to the storage
        /// </summary>
        /// <param name="entryName">particle name in the game</param>
        /// <param name="particleFile">particle file (.Particle)</param>
        /// <param name="maxCount">instance count</param>
        /// <param name="sceneParent">parent scene node</param>
        /// <returns>an index of new particle</returns>
        public int AddParticle( string entryName, string particleFile, int maxCount, 
                                NodeBase sceneParent)
        {
            if (particleOn == false)
                return -1;

            ParticleStorage storage = null;
            ParticleSequenceInfo particleInfo = null;

            // checks a registerd particle.
            int index = FindParticleIndexByName(entryName);
            if (index != -1)
            {
                storage = particleSequencesList[index];
            }
            else
            {
                storage = new ParticleStorage();
                storage.entryName = entryName;
                particleSequencesList.Add(storage);
            }

            //  makes a resource path.
            string resourcePath = Path.GetDirectoryName(particleFile);

            particleInfo = FindParticleInfo(particleFile);
            if (particleInfo == null)
                particleInfo = LoadParticleSequenceInfo(particleFile);

            if (particleInfo == null)
            {
                throw new ArgumentException(
                                    "failed to load particle : " + particleFile);
            }

            ParticleSequence source = 
                            new ParticleSequence(ref particleInfo, resourcePath);

            //  makes and stores a particle instance.
            for (int i = 0; i < maxCount; i++)
            {
                ParticleSequence instance =
                                ParticleSequence.CreateInstance(ref source);

                instance.Name = entryName;

                storage.list.Add(instance);

                //  adds particle to render scene.
                sceneParent.AddChild(instance);
            }

            return particleSequencesList.IndexOf(storage);
        }

        /// <summary>
        /// reads a particle list file.
        /// Loads every particle, specified in the list file, 
        /// and registers to the manager.
        /// </summary>
        /// <param name="fileName">particle list file (.ParticleList)</param>
        /// <param name="sceneParent">3D scene parent node</param>
        public void LoadParticleList(string fileName, NodeBase sceneParent)
        {
            if (particleOn == false)
                return;

            Stream stream = File.OpenRead(Path.Combine("Content", fileName));

            XmlTextReader reader = new XmlTextReader(stream);
            XmlSerializer serializer = new XmlSerializer(typeof(ParticleReaderList));
            ParticleReaderList list = (ParticleReaderList)serializer.Deserialize(reader);

            for (int i = 0; i < list.ParticleReaderElements.Count; i++)
            {
                ParticleReaderElement element = list.ParticleReaderElements[i];

                AddParticle(element.EntryName,
                            element.RelativeFilePath,
                            element.UseMaxCount, sceneParent);
            }
        }

        /// <summary>
        /// searches for the particle information which has been 
        /// registered with particle file name.
        /// </summary>
        /// <param name="fileName">particle file (.Particle)</param>
        /// <returns>particle information</returns>
        public ParticleSequenceInfo FindParticleInfo(string fileName)
        {
            for (int i = 0; i < particleInfoList.Count; i++)
            {
                if (particleInfoList[i].Name == fileName)
                {
                    return particleInfoList[i];
                }
            }

            return null;
        }

        /// <summary>
        /// reads information from a particle file and registers to the list.
        /// </summary>
        /// <param name="fileName">particle file (.Particle)</param>
        /// <returns>particle information</returns>
        public ParticleSequenceInfo LoadParticleSequenceInfo(string fileName)
        {
            ParticleSequenceInfo newData = (ParticleSequenceInfo)
                                FrameworkCore.ContentManager.Load<ParticleSequenceInfo>(
                                        fileName);

            particleInfoList.Add(newData);

            return newData;
        }

        /// <summary>
        /// deletes every particle from the list.
        /// </summary>
        public void ClearAllParticles()
        {
            for(int i = 0; i < particleSequencesList.Count; i++)
            {
                particleSequencesList[i].list.Clear();
            }

            particleSequencesList.Clear();
        }

        /// <summary>
        /// searches the particle index with the registered particle name.
        /// </summary>
        /// <param name="entryName">particle name</param>
        /// <returns>an index of particle in the list</returns>
        public int FindParticleIndexByName(string entryName)
        {
            for (int i = 0; i < particleSequencesList.Count; i++)
            {
                ParticleStorage storage = particleSequencesList[i];

                if (storage.entryName == entryName)
                {
                    return particleSequencesList.IndexOf(storage);
                }
            }

            return -1;
        }

        /// <summary>
        /// plays the specified particle.
        /// </summary>
        /// <param name="id">an index of particle</param>
        /// <param name="position">The position of a particle</param>
        /// <param name="normal">The normal vector of a particle</param>
        /// <param name="axis">The axis vector of a particle</param>
        /// <returns>the plaied particle</returns>
        public ParticleSequence PlayParticle(int id, Vector3 position, 
                                             Vector3 normal, Matrix axis)
        {
            if (particleOn == false)
                return null;

            if (id == -1)
                throw new ArgumentException("Cannot find particle : " + id);

            ParticleSequence particle = FindFreeParticle(particleSequencesList[id]);
            if (particle != null)
            {
                Matrix transform;
                 
                if (Math.Abs(1.0f - Vector3.Dot(normal, Vector3.Up)) < 0.0001f)
                {
                    transform = Helper3D.MakeMatrixWithAt(normal, Vector3.Forward);
                }
                else
                {
                    transform = Helper3D.MakeMatrixWithAt(normal, Vector3.Up);
                }

                transform.Translation = position;

                particle.WorldTransform = axis * transform;
            
                PlayParticle(particle);

                return particle;
            }

            return null;
        }

        /// <summary>
        /// plays the specified particle.
        /// </summary>
        /// <param name="id">an index of particle</param>
        /// <param name="world">matrix in the world</param>
        /// <param name="axis">The axis vector of a particle</param>
        /// <returns>the plaied particle</returns>
        public ParticleSequence PlayParticle(int id, Matrix world, Matrix axis)
        {
            if (particleOn == false)
                return null;

            if (id == -1)
                throw new ArgumentException("Cannot find particle : " + id);

            ParticleSequence particle = FindFreeParticle(particleSequencesList[id]);
            if( particle != null)
            {
                particle.WorldTransform = axis * world;

                PlayParticle(particle);

                return particle;
            }

            return null;
        }

        /// <summary>
        /// plays the specified particle.
        /// </summary>
        /// <param name="particle">particle</param>
        /// <returns></returns>
        public bool PlayParticle(ParticleSequence particle)
        {
            if (particleOn == false)
                return true;

            if (particle != null)
            {
                particle.Play();
                return true;
            }

            return false;
        }

        /// <summary>
        /// searches for the particle (instance) whose activity has ended 
        /// or is yet to start.
        /// </summary>
        /// <param name="storage">particle storage</param>
        /// <returns></returns>
        protected ParticleSequence FindFreeParticle(ParticleStorage storage)
        {
            if (particleOn == false)
                return null;

            for (int i = 0; i < storage.list.Count; i++)
            {
                if (storage.list[i].IsPlaying == false)
                    return storage.list[i];
            }

            return null;
        }
    }
}
