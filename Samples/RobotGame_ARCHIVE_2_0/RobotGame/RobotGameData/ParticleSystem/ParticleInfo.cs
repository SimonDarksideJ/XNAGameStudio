#region File Description
//-----------------------------------------------------------------------------
// ParticleInfo.cs
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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RobotGameData.ParticleSystem
{
    #region VertexData

    /// <summary>
    /// contains the vertex information of the particle object of mesh type.
    /// </summary>
    [Serializable]
    public class VertexData
    {
        public bool HasPosition = false;
        public bool HasNormal = false;
        public bool HasColor = false;
        public bool HasTextureCoord = false;
        public bool HasIndex = false;

        public List<Vector3> Position = null;
        public List<Vector3> Normal = null;
        public List<Color> Color = null;
        public List<Vector2> TextureCoord = null;
        public List<short> Index = null;
    }

    #endregion

    /// <summary>
    /// contains every information of particle.
    /// Based on the information, particle gets updated.
    /// </summary>
    #region Particle Information
    [Serializable]
    public class ParticleInfo
    {
        #region Enum types

        [Flags]
        public enum ParamStyles
        {
            None                = 0x00000000,
            Random              = 0x00000001,
            Interpolate         = 0x00000002,
            Clamp               = 0x00000004,
            Gravity             = 0x00000010,
            RandomInterpolate   = 0x00000020,
        }

        public enum ParticleObjectType
        {
            PointSprite = 0,
            Sprite = 1,
            Billboard = 2,
            Scene = 3,
            AnchorBillboard = 4,
        }

        public enum EmitObjectType
        {
            Cone = 0,
            Sphere = 1,
            Disk = 2,
            RectPlane = 3,
            CirclePlane = 4,
        }

        public enum FuncType
        {
            None = 0,
            Constant = 1,
            Sin = 2,
            Cos = 3,
            Rnd = 4,
            Table = 5,
        }

        #endregion

        #region Persistent Member Fields

        // Persistent Members
        public string Name = String.Empty;
        public ParticleObjectType ParticleType = ParticleObjectType.PointSprite;
        public VertexData MeshData = null;
        public string TextureFileName = String.Empty;
        public bool AlphaBlendEnable = true;
        public bool DepthBufferEnable = false;
        public Blend SourceBlend = Blend.One;
        public Blend DestinationBlend = Blend.One;
        public BlendFunction BlendFunction = BlendFunction.Add;
        public float LifeTime = 0.0f;
        public float ObjectLifeTime = 0.0f;
        public float MassMin = 0.0f;
        public float MassMax = 0.0f;
        public int InitialObjectCount = 0;
        public int MaxObjectCount = 0;
        public int EmitCount = 0;
        public bool Volatile = false;

        public EmitObjectType EmitType = EmitObjectType.Cone;
        public Vector3 EmitPosition = Vector3.Zero;
        public Vector3 EmitDirection = Vector3.Forward;
        public float EmitAngle = 0.0f;
        public float EmitInterval = 0.0f;
        public Vector3 UpVector = Vector3.Up;

        public uint PositionStyle = 0;
        public float PositionUpdateInterval = 0.0f;
        public List<FuncType> PositionFunc = null;
        public List<float> PositionInit = null;
        public List<float> PositionFactor = null;
        public List<KeyFrameTable> PositionTable = null;
        public float PositionMin = 0.0f;
        public float PositionMax = 0.0f;
        public float PositionInitialRandomFactor = 1.0f;
        public Vector3 PositionRandomFactor = Vector3.One;
        public float PositionRandomInterval = 0.0f;

        public uint ScaleStyle = 0;
        public float ScaleUpdateInterval = 0.0f;
        public List<FuncType> ScaleFunc = null;
        public List<float> ScaleInit = null;
        public List<float> ScaleFactor = null;
        public List<KeyFrameTable> ScaleTable = null;
        public float ScaleInitialRandomFactor = 1.0f;
        public float ScaleMin = 0.0f;
        public float ScaleMax = 0.0f;
        public Vector3 ScaleMask = Vector3.One;
        public float ScaleBillboardFactor = 1.0f;

        public uint RotateStyle = 0;
        public float RotateUpdateInterval = 0.0f;
        public float RotateRandomFactor = 1.0f;
        public FuncType RotateFunc = FuncType.None;
        public float RotateInit = 0.0f;
        public float RotateFactor = 1.0f;
        public KeyFrameTable RotateTable = null;

        public uint ColorStyle = 0;
        public float ColorUpdateInterval = 0.0f;
        public FuncType RgbFunc = FuncType.None;
        public string RgbInit = String.Empty;
        public KeyFrameTable Rtable = null;
        public KeyFrameTable Gtable = null;
        public KeyFrameTable Btable = null;
        public KeyFrameTable Atable = null;

        public FuncType AlphaFunc = FuncType.None;
        public uint AlphaInit = 255;

        #endregion

        #region Volatile Member Fields

        // Volatile Members
        Color eRgbInit = Color.Black;

        public const int FuncCount = 2;

        #endregion

        #region Properties

        public Color RgbInitValue
        {
            get { return eRgbInit; }
        }

        public bool IsPositionStyle(uint param)
        {
            return ((PositionStyle & param) > 0);
        }

        public bool IsPositionStyle(ParamStyles param)
        {
            return ((PositionStyle & (uint)param) > 0);
        }

        public bool IsScaleStyle(uint param)
        {
            return ((ScaleStyle & param) > 0);
        }

        public bool IsScaleStyle(ParamStyles param)
        {
            return ((ScaleStyle & (uint)param) > 0);
        }

        public bool IsRotateStyle(uint param)
        {
            return ((RotateStyle & param) > 0);
        }

        public bool IsRotateStyle(ParamStyles param)
        {
            return ((RotateStyle & (uint)param) > 0);
        }

        public bool IsColorStyle(uint param)
        {
            return ((ColorStyle & param) > 0);
        }

        public bool IsColorStyle(ParamStyles param)
        {
            return ((ColorStyle & (uint)param) > 0);
        }

        #endregion

        /// <summary>
        /// Initialize members
        /// </summary>
        public void Initialize()
        {
            //  Initialize KeyFrameTable
            if (PositionTable != null)
            {
                for (int i = 0; i < PositionTable.Count; i++)
                    PositionTable[i].Initialize();
            }

            if (ScaleTable != null)
            {
                for (int i = 0; i < ScaleTable.Count; i++)
                    ScaleTable[i].Initialize();
            }

            if( RotateTable != null)
                RotateTable.Initialize();

            if (Rtable != null)
                Rtable.Initialize();

            if (Gtable != null)
                Gtable.Initialize();

            if (Btable != null)
                Btable.Initialize();

            if (Atable != null)
                Atable.Initialize();

            //  Initialize eRgbInit's Color
            string[] color = RgbInit.Split(',');
            eRgbInit = new Color(byte.Parse(color[0]),       //  R
                                 byte.Parse(color[1]),       //  G
                                 byte.Parse(color[2]),       //  B
                                 byte.Parse(color[3]));      //  A
        }
    }

    #endregion
}
