#region File Description
//-----------------------------------------------------------------------------
// TimeSequence.cs
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
using RobotGameData.Render;
#endregion

namespace RobotGameData.ParticleSystem
{
    #region Enum

    public enum SequenceStyle
    {
        /// <summary>
        /// N/A
        /// </summary>
        None = 0,

        /// <summary>
        /// has been disabled and does not process any info.
        /// </summary>
        Disable,
    }

    public enum SequenceState
    {
        /// <summary>
        /// N/A
        /// </summary>
        None = 0,

        /// <summary>
        /// currently in active.
        /// </summary>
        Active,
    }

    #endregion

    #region TimeSequenceInfo

    /// <summary>    
    /// time information contains particle information.
    /// Contains information on the start time and the life time.
    /// </summary>
    [Serializable]
    public class TimeSequenceInfo
    {        
        public float StartTime = 0.0f;
        public float Duration = 0.0f;
        public SequenceStyle Style = SequenceStyle.None;
        public ParticleInfo ParticleInfo = null;
        public TextureSequence TextureSequence = null;
    }

    #endregion

    #region TimeSequenceData

    /// <summary>
    /// a data class that updates the current state by using time information.
    /// </summary>
    public class TimeSequenceData
    {
        #region Fields

        TimeSequenceInfo sequenceInfo;
        GameSceneNode owner = null;

        SequenceStyle style = SequenceStyle.None;
        SequenceState state = SequenceState.None;

        #endregion

        #region Properties

        public SequenceStyle Style
        {
            get { return style; }
            set { style = value; }
        }

        public SequenceState State
        {
            get { return state; }
            set { state = value; }
        }

        public GameSceneNode Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public TimeSequenceInfo SequenceInfo
        {
            get { return sequenceInfo; }
        }

        #endregion

        #region Constructors

        public TimeSequenceData(TimeSequenceInfo info)
        {
            sequenceInfo = info;

            if (sequenceInfo.ParticleInfo != null)
                sequenceInfo.ParticleInfo.Initialize();

            Style = info.Style;
        }

        #endregion
    }

    #endregion

    #region TimeSequence

    /// <summary>
    /// It has all particles.  By using TimeSequenceData, 
    /// it calculates the lifetime of each Particle class.  
    /// It also has Reset(), Stop() function which control all particle simultaneously.
    /// </summary>
    public class TimeSequence : GameSceneNode
    {
        #region Fields

        List<TimeSequenceData> timeSequenceDataList = new List<TimeSequenceData>();

        float localTIme = 0.0f;
        float duration = 0.0f;
        bool active = false;     //  default is false
        bool infinite = false;

        #endregion

        #region Properties

        public bool IsActive
        {
            get { return active; }
        }

        public bool IsInfinite
        {
            get { return infinite; }
        }

        public float LocalTime
        {
            get { return localTIme; }
            set { localTIme = value; }
        }

        public int Count
        {
            get { return timeSequenceDataList.Count; }
        }

        #endregion

        /// <summary>
        /// enables/disables the scene owner
        /// </summary>
        /// <param name="data">time data</param>
        /// <param name="enable">enable flag</param>
        public static void SetOwnerEnable(TimeSequenceData data, bool enable)
        {
            if (data.Owner != null)
            {
                data.Owner.Enabled = enable;
                data.Owner.Visible = enable;
            }
        }

        /// <summary>
        /// adds a new time data.
        /// </summary>
        /// <param name="data">new time data</param>
        public void AddSequence(TimeSequenceData data)
        {
            timeSequenceDataList.Add(data);

            SetOwnerEnable(data, false);

            UpdateDuration();
        }

        /// <summary>
        /// removes the time data.
        /// </summary>
        /// <param name="data">the time data</param>
        public void RemoveSequence(TimeSequenceData data)
        {
            timeSequenceDataList.Remove(data);
        }

        /// <summary>
        /// removes time data by the index.
        /// </summary>
        /// <param name="index">an index of time data</param>
        public void RemoveSequene(int index)
        {
            timeSequenceDataList.RemoveAt(index);

            UpdateDuration();
        }

        /// <summary>
        /// removes all time data
        /// </summary>
        public void RemoveAllSequence()
        {
            timeSequenceDataList.Clear();

            duration = 0.0f;
        }

        /// <summary>
        /// gets the time data by index.
        /// </summary>
        /// <param name="index">an index of time data</param>
        /// <returns>the time data</returns>
        public TimeSequenceData GetSequence(int index)
        {
            return timeSequenceDataList[index];
        }

        /// <summary>
        /// calcurates the duration time.
        /// </summary>
        public void UpdateDuration()
        {
            duration = 0.0f;
            infinite = false;

            for (int i = 0; i < timeSequenceDataList.Count; i++)
            {             
                TimeSequenceInfo info = timeSequenceDataList[i].SequenceInfo;

                if (info.Duration <= 0.0f)
                    infinite = true;

                if (info.StartTime + info.Duration > duration)
                    duration = info.StartTime + info.Duration;
            }
        }

        /// <summary>
        /// resets local time.
        /// stops when the owner is particle and plays.
        /// </summary>
        public void Reset()
        {
            localTIme = 0.0f;
            active = true;

            for (int i = 0; i < timeSequenceDataList.Count; i++)
            {
                TimeSequenceData data = timeSequenceDataList[i];

                data.State = SequenceState.None;
                SetOwnerEnable(data, false);

                if (data.Owner is Particle)
                {
                    Particle particle = data.Owner as Particle;

                    particle.Stop();
                    particle.Start();
                }
            }
        }

        /// <summary>
        /// disables the owner.
        /// Stops when the owner is particle.
        /// </summary>
        public void Stop()
        {
            localTIme = 0.0f;
            active = false;

            for (int i = 0; i < timeSequenceDataList.Count; i++)
            {
                TimeSequenceData data = timeSequenceDataList[i];

                data.State = SequenceState.None;
                SetOwnerEnable(data, false);

                if (data.Owner is Particle)
                {
                    Particle particle = data.Owner as Particle;

                    particle.Stop();
                }
            }
        }

        /// <summary>
        /// configures the reference transform matrix.
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="matrix"></param>
        public void SetRefMatrix(bool enabled, Matrix? matrix)
        {
            for (int i = 0; i < timeSequenceDataList.Count; i++)
            {
                TimeSequenceData data = timeSequenceDataList[i];

                if (data.Owner is Particle)
                {
                    Particle particle  = data.Owner as Particle;

                    particle.SetRefMatrixEnable(enabled);
                    particle.SetRefMatrix(matrix);
                }
            }
        }

        /// <summary>
        /// updates every time data that has been registered to the list.
        /// Time data whose start time has passed gets enabled.
        /// Time data whose duration time has passed gets disabled.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void OnUpdate(GameTime gameTime)
        {
            if (!infinite && !active)
                return;

            for (int i = 0; i < timeSequenceDataList.Count; i++)
            {
                TimeSequenceData data = timeSequenceDataList[i];
             
                if (data.Style == SequenceStyle.Disable)
                    continue;

                if (data.State == SequenceState.Active)
                {
                    //  If activate
                    if (data.SequenceInfo.Duration > 0.0f)
                    {
                        // If the Duration is 0, time is infinite
                        if (localTIme >= (data.SequenceInfo.StartTime + 
                                            data.SequenceInfo.Duration))
                        {
                            data.State = SequenceState.None;
                            SetOwnerEnable(data, false);
                        }
                    }
                }
                else
                {
                    //  Starting time..
                    if (localTIme >= data.SequenceInfo.StartTime)
                    {
                        data.State = SequenceState.Active;
                        SetOwnerEnable(data, true);
                    }
                }
            }

            localTIme += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //  Finished
            if (localTIme > duration)
            {
                active = false;
            }
        }
    }

    #endregion
}
