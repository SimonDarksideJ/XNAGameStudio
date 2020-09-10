#region File Description
//-----------------------------------------------------------------------------
// ParticleSequence.cs
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
#endregion

namespace RobotGameData.ParticleSystem
{
    #region ParticleSequence Information

    /// <summary>
    /// contains particle information.
    /// It can load an XML file(.Particle).
    /// </summary>
    [Serializable]
    public class ParticleSequenceInfo
    {
        public string Name = String.Empty; 
        public List<TimeSequenceInfo> TimeSequencesInfo = new List<TimeSequenceInfo>();
    }

    #endregion

    #region ParticleSequence

    /// <summary>
    /// It includes the information on the particle and a 
    /// single TimeSequence which it controls
    /// </summary>
    public class ParticleSequence : GameSceneNode
    {
        #region Fields

        ParticleSequenceInfo sequenceInfo = null;
        string resourcePath = String.Empty;

        TimeSequence timeSequence = null;

        #endregion

        #region Properties

        public ParticleSequenceInfo SequenceInfo
        {
            get { return sequenceInfo; }
        }

        public string ResourcePath
        {
            get { return resourcePath; }
        }

        public TimeSequence TimeSequence
        {
            get { return timeSequence; }
        }

        public bool IsPlaying
        {
            get { return timeSequence.IsActive; }
        }

        public bool IsInfinite
        {
            get { return timeSequence.IsInfinite; }
        }

        #endregion
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="info">particle sequence information structure</param>
        /// <param name="resourceFolder">resource path</param>
        public ParticleSequence(ref ParticleSequenceInfo info, string resourceFolder)
            : base()
        {
            Create(ref info, resourceFolder);
        }

        /// <summary>
        /// creates instance particle by using the source particle.
        /// </summary>
        /// <param name="source">source particle</param>
        /// <returns>new instance particle</returns>
        public static ParticleSequence CreateInstance(ref ParticleSequence source)
        {
            ParticleSequenceInfo info = source.SequenceInfo;

            return new ParticleSequence(ref info, source.ResourcePath);
        }

        /// <summary>
        /// Creates using particle information.
        /// </summary>
        /// <param name="info">the particle information</param>
        /// <param name="resourcePath">the particle file</param>
        public void Create(ref ParticleSequenceInfo info, string resourcePath)
        {
            this.sequenceInfo = info;
            this.resourcePath = resourcePath;

            //  Add TimeSequence to child
            timeSequence = new TimeSequence();
            AddChild(timeSequence);

            for (int i = 0; i < info.TimeSequencesInfo.Count; i++)
            {
                TimeSequenceInfo timeSequenceInfo = info.TimeSequencesInfo[i];

                //  Create a TimeSequenceData using TimeSequenceInfo
                TimeSequenceData timeSequenceData = 
                    new TimeSequenceData(timeSequenceInfo);

                //  Create a particle using ParticleInfo and TextureSequence 
                //  in the TimeSequenceInfo
                Particle particle = new Particle(resourcePath,
                                                timeSequenceInfo.ParticleInfo, 
                                                timeSequenceInfo.TextureSequence);

                //  Set owner particle to the TimeSequenceData
                timeSequenceData.Owner = particle;

                //  Add the particle to the TimeSequence's child
                timeSequence.AddChild(particle);

                //  Add TimeSequenceData to the TimeSequence
                timeSequence.AddSequence(timeSequenceData);
            }
                        
            Name = info.Name;

            //  Particle off
            Stop();
        }

        /// <summary>
        /// Updates the particle.
        /// </summary>
        protected override void OnUpdate(GameTime gameTime)
        {
            if (timeSequence.IsActive == false && timeSequence.IsInfinite == false)
                Stop();

            base.OnUpdate(gameTime);
        }

        /// <summary>
        /// Reset and play the particle.
        /// </summary>
        public void Reset()
        {
            this.Enabled = true;
            this.Visible = true;

            TimeSequence.Enabled = true;
            TimeSequence.Visible = true;

            TimeSequence.Reset();
        }

        /// <summary>
        /// stops and disables particle.
        /// </summary>
        public void Stop()
        {
            this.Enabled = false;
            this.Visible = false;

            TimeSequence.Enabled = false;
            TimeSequence.Visible = false;

            TimeSequence.Stop();
        }

        /// <summary>
        /// play the particle.
        /// </summary>
        public void Play()
        {        
            for (int i = 0; i < timeSequence.Count; i++)
            {
                TimeSequenceData sequence = timeSequence.GetSequence(i);
                
                Particle particle = sequence.Owner as Particle;
                particle.RootAxis = RootAxis;
                particle.WorldTransform = WorldTransform;                
            }

            Reset();
        }

        /// <summary>
        /// changes the transform matrix of particle.
        /// </summary>
        /// <param name="transform">transform matrix</param>
        public void SetTransform(Matrix transform)
        {
            this.WorldTransform = transform;

            for (int i = 0; i < timeSequence.Count; i++)
            {
                TimeSequenceData sequence = timeSequence.GetSequence(i);

                Particle particle = sequence.Owner as Particle;
                particle.RootAxis = this.RootAxis;
                particle.WorldTransform = this.WorldTransform;
            }
        }
    }

    #endregion
}
