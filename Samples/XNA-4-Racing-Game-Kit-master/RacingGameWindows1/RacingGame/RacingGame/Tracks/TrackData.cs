#region File Description
//-----------------------------------------------------------------------------
// TrackData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using RacingGame.Helpers;
#endregion

namespace RacingGame.Tracks
{
    /// <summary>
    /// Track data, imported from 3ds max data. See TrackImporter class.
    /// </summary>
    public class TrackData
    {
        #region Constants
        /// <summary>
        /// Directory where all the track data files are stored.
        /// </summary>
        public const string Directory = "Content";
        /// <summary>
        /// Extension for the track data files.
        /// </summary>
        public const string Extension = "Track";
        #endregion

        #region Variables
        /// <summary>
        /// Track points
        /// </summary>
        private List<Vector3> trackPoints = new List<Vector3>();

        #region WidthHelper class
        /// <summary>
        /// Width helper
        /// </summary>
        [Serializable]
        public class WidthHelper
        {
            /// <summary>
            /// Position
            /// </summary>
            public Vector3 pos;
            /// <summary>
            /// Scale
            /// </summary>
            public float scale;

            /// <summary>
            /// Create width helper
            /// </summary>
            public WidthHelper()
            {
            }

            /// <summary>
            /// Create width helper
            /// </summary>
            /// <param name="setPos">Set position</param>
            /// <param name="setScale">Set scale</param>
            public WidthHelper(Vector3 setPos, float setScale)
            {
                pos = setPos;
                scale = setScale;
            }
        }
        #endregion

        /// <summary>
        /// Width helper position
        /// </summary>
        private List<WidthHelper> widthHelpers = new List<WidthHelper>();

        #region RoadHelper class
        /// <summary>
        /// Road helper
        /// </summary>
        [Serializable]
        public class RoadHelper
        {
            /// <summary>
            /// Helper type
            /// </summary>
            public enum HelperType
            {
                Tunnel,
                Palms,
                Laterns,
                Reset,
            }

            /// <summary>
            /// Type
            /// </summary>
            public HelperType type;
            /// <summary>
            /// Position
            /// </summary>
            public Vector3 pos;

            /// <summary>
            /// Create road helper
            /// </summary>
            public RoadHelper()
            {
            }

            /// <summary>
            /// Create road helper
            /// </summary>
            /// <param name="setType">Set type</param>
            /// <param name="setPos">Set position</param>
            public RoadHelper(HelperType setType, Vector3 setPos)
            {
                type = setType;
                pos = setPos;
            }
        }
        #endregion

        /// <summary>
        /// Tunnel helper position
        /// </summary>
        private List<RoadHelper> roadHelpers = new List<RoadHelper>();

        #region NeutralObject class
        /// <summary>
        /// Neutral object
        /// </summary>
        [Serializable]
        public class NeutralObject
        {
            /// <summary>
            /// Model name
            /// </summary>
            public string modelName;
            /*not required, just use the matrix
            /// <summary>
            /// Position
            /// </summary>
            public Vector3 pos;
             */
            /// <summary>
            /// Matrix
            /// </summary>
            public Matrix matrix;

            /// <summary>
            /// Create neutral object
            /// </summary>
            public NeutralObject()
            {
            }

            /// <summary>
            /// Create neutral object
            /// </summary>
            /// <param name="setModelName">Set model name</param>
            /// <param name="setMatrix">Set matrix</param>
            public NeutralObject(string setModelName, Matrix setMatrix)
            {
                modelName = setModelName;
                matrix = setMatrix;
            }
        }
        #endregion

        /// <summary>
        /// List of neutral objects used in this level
        /// </summary>
        private List<NeutralObject> objects = new List<NeutralObject>();
        #endregion

        #region Properties
        /// <summary>
        /// Track points
        /// </summary>
        /// <returns>List</returns>
        public List<Vector3> TrackPoints
        {
            get
            {
                return trackPoints;
            }
        }

        /// <summary>
        /// Width helpers
        /// </summary>
        /// <returns>List</returns>
        public List<WidthHelper> WidthHelpers
        {
            get
            {
                return widthHelpers;
            }
        }

        /// <summary>
        /// Tunnel helper position
        /// </summary>
        /// <returns>List</returns>
        public List<RoadHelper> RoadHelpers
        {
            get
            {
                return roadHelpers;
            }
        }

        /// <summary>
        /// Neutrals objects
        /// </summary>
        /// <returns>List</returns>
        public List<NeutralObject> NeutralsObjects
        {
            get
            {
                return objects;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create track data, empty constructor, required for Serialization.
        /// </summary>
        public TrackData()
        {
        }

        /// <summary>
        /// Create track data (used only in TrackImporter).
        /// </summary>
        /// <param name="setFilename">Set filename</param>
        /// <param name="setTrackPoints">Set track points</param>
        /// <param name="setWidthHelpers">Set width helpers</param>
        /// <param name="setRoadHelpers">Set road helpers</param>
        /// <param name="setObjects">Set objects</param>
        public TrackData(List<Vector3> setTrackPoints,
            List<WidthHelper> setWidthHelpers,
            List<RoadHelper> setRoadHelpers,
            List<NeutralObject> setObjects)
        {
            trackPoints = setTrackPoints;
            widthHelpers = setWidthHelpers;
            roadHelpers = setRoadHelpers;
            objects = setObjects;
        }

        /// <summary>
        /// Load track data
        /// </summary>
        /// <param name="setFilename">Set filename</param>
        public static TrackData Load(string setFilename)
        {
            // Load track data
            StreamReader file = new StreamReader(TitleContainer.OpenStream(
                Directory + "\\" + setFilename + "." + Extension));

            // Load everything into this class with help of the XmlSerializer.
            TrackData loadedTrack = (TrackData)
                new XmlSerializer(typeof(TrackData)).
                Deserialize(file.BaseStream);

            // Close the file
            file.Close();

            // Return loaded file
            return loadedTrack;
        }
        #endregion

    }
}
