#region File Description
//-----------------------------------------------------------------------------
// Particle.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.GameObject;
using RobotGameData.Render;
using RobotGameData.GameInterface;
using RobotGameData.Resource;
using RobotGameData.Helper;
#endregion

namespace RobotGameData.ParticleSystem
{
    #region Particle Object
    
    /// <summary>
    /// this object has information of one particle.
    /// </summary>
    public class ParticleObject
    {
        [Flags]
        public enum States
        {
            None            = 0x00000000,
            Enable          = 0x00000001,
            Initialized     = 0x00000002,
            Fired           = 0x00000004,
            FirstTime       = 0x00000008,
        }

        public uint state = 0;
        public float time = 0.0f;
        public Vector3 emitDir = Vector3.Zero;
        public float posInterval = 0.0f;
        public float[] posArg = new float[2];
        public Vector3 startPos = Vector3.Zero;
        public Vector3 targetPos = Vector3.Zero;
        public Vector3[] pos = new Vector3[2];
        public float posRandomInterval = 0.0f;
        public float posRandom = 0.0f;

        public float mass = 0.0f;
        public Vector3 gravity = Vector3.Zero;

        public float scaleInterval = 0.0f;
        public float[] scaleArg = new float[2];
        public Vector3 startScale = Vector3.One;
        public Vector3 targetScale = Vector3.One;
        public Vector3 scale = Vector3.One;
        public float scaleRandom = 0.0f;

        public float rotateAccm = 0.0f;
        public float rotateArg = 0.0f;
        public Vector3 rotateDir = Vector3.Zero;
        public float rotateRandom = 0.0f;
        public float textureSequenceInterval = 0.0f;
        public Color color = Color.Black;

        public bool IsState(States state)
        {
            return ((this.state & (uint)state) > 0);
        }

        public bool IsState(uint state)
        {
            return ((this.state & state) > 0);
        }
    }

    #endregion

    #region Particle

    /// <summary>
    /// It means a single individual particle. 
    /// The number of Particle class and the number of TimeSequenceData class, 
    /// which contains the time information of the Particle class, are equal.
    /// Each particle may have only one of the following scene properties: 
    /// GameMesh, GamePointSprite, GameSprite3D, GameBillboard.
    /// </summary>
    public class Particle : GameSceneNode
    {
        #region Fields

        static Vector3 globalGravityDirection = new Vector3(0.0f, -1.0f, 0.0f);
        static float globalGravityFactor = 98.0f;

        ParticleInfo sequenceInfo = null;
        TextureSequence textureSequence = null;
        string resourcePath = String.Empty;
        
        GameSceneNode sceneRoot = null;
        GameMesh sceneMesh = null;
        GamePointSprite scenePointSprite = null;
        GameSprite3D sceneSprite3D = null;
        GameBillboard sceneBillboard = null;

        bool isStarting = false;
        bool isStopping = false;
        bool changeTime = false;
        int posArgCount = 0;
        int scaleArgCount = 0;
        float localTime = 0.0f;
        Vector3 emitRight = Vector3.Zero;
        Vector3 emitUp = Vector3.Zero;
        Vector3 upRight = Vector3.Zero;
        Vector3 upAt = Vector3.Zero;
        float emitInterval = 0.0f;
        float posRinterval = 0.0f;
        float scaleRinterval = 0.0f;

        List<ParticleObject> particleObjectList = new List<ParticleObject>();

        Texture2D texture2D = null;

        KeyFrameTable.Interpolation positionInterpolate = 
            KeyFrameTable.Interpolation.None;
        KeyFrameTable.Interpolation scaleInterpolate = 
            KeyFrameTable.Interpolation.None;
        KeyFrameTable.Interpolation colorInterpolate =
            KeyFrameTable.Interpolation.None;

        bool enableRotate = false;
        float rObjectLifeTime = 0.0f;
        Matrix? refMatrix = null;
        bool refEnable = false;

        #endregion

        #region Properties

        public ParticleInfo Info
        {
            get { return sequenceInfo; }
            set { sequenceInfo = value; }
        }

        public bool IsPlaying
        {
            get { return isStarting; }
        }

        public TextureSequence TextureSequence
        {
            get { return textureSequence; }
            set { textureSequence = value; }
        }

        public Texture2D Texture
        {
            get { return texture2D; }
            set
            {
                texture2D = value;
            }
        }

        public GameSceneNode SceneMesh
        {
            get { return sceneMesh; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resourcePath">texture resource path</param>
        /// <param name="info">particle information structure</param>
        /// <param name="textureSequence">texture sequence class</param>
        public Particle(string resourcePath, ParticleInfo info, 
                        TextureSequence textureSequence) : base()
        {
            if (String.IsNullOrEmpty(resourcePath))
                throw new ArgumentNullException("resourcePath");
            if (info == null)
                throw new ArgumentNullException("info");

            this.sequenceInfo = info;            

            this.textureSequence = textureSequence;
            this.resourcePath = resourcePath;

            //  Create particle's scene root
            this.sceneRoot = new GameSceneNode();
            this.sceneRoot.Name = "Particle Scene Root";

            AddChild(this.sceneRoot);

            Create();
        }

        public static void SetGravityDirection(Vector3 vec)
        {
            globalGravityDirection = vec;
        }

        public static void SetGravityFactor(float val)
        {
            globalGravityFactor = val;
        }

        public void SetRefMatrixEnable(bool enabled)
        {
            refEnable = enabled;
        }

        public void SetRefMatrix(Matrix? matrix)
        {
            refMatrix = matrix;
        }

        /// <summary>
        /// enables/disables a particle’s scene object.
        /// </summary>
        /// <param name="enable"></param>
        public void SetSceneEnable(bool enable)
        {
            sceneRoot.Enabled = enable;
            sceneRoot.Visible = enable;
        }

        /// <summary>
        /// by using the particle information, a particle object is created
        /// and initialized.
        /// </summary>
        public void Create()
        {
            //  Initialize Particle Information 
            Info.Initialize();

            Name = sequenceInfo.Name;
            sceneRoot.RemoveAllChild(false);

            if (sequenceInfo.TextureFileName.Length > 0)
            {
                string resourceFullPath = 
                            Path.Combine(resourcePath, sequenceInfo.TextureFileName);

                GameResourceTexture2D resource = 
                            FrameworkCore.ResourceManager.LoadTexture(resourceFullPath);
                
                //  Set to texture
                Texture = resource.Texture2D;

                //  Set to texture in TextureSequence 
                if (TextureSequence != null)
                    TextureSequence.SetTexture(Texture);
            }

            enableRotate = false;

            switch (Info.ParticleType)
            {
                    //  PointSprite
                case ParticleInfo.ParticleObjectType.PointSprite:
                    {
                        scenePointSprite = new GamePointSprite();
                        scenePointSprite.Create(Info.MaxObjectCount, Texture, 
                                                RenderingSpace.World, false);

                        scenePointSprite.AlphaBlendEnable = 
                            sequenceInfo.AlphaBlendEnable;
                        scenePointSprite.DepthBufferEnable = 
                            sequenceInfo.DepthBufferEnable;
                        scenePointSprite.SourceBlend = 
                            sequenceInfo.SourceBlend;
                        scenePointSprite.DestinationBlend = 
                            sequenceInfo.DestinationBlend;
                        scenePointSprite.BlendFunction = 
                            sequenceInfo.BlendFunction;
                        scenePointSprite.DepthBufferEnable = true;
                        scenePointSprite.DepthBufferWriteEnable = false;
                        scenePointSprite.DepthBufferFunction = CompareFunction.Less;
                        scenePointSprite.AlphaFunction = CompareFunction.Greater;
                        scenePointSprite.CullMode = CullMode.None;

                        sceneRoot.AddChild(scenePointSprite);
                    }
                    break;
                    //  Sprite3D
                case ParticleInfo.ParticleObjectType.Sprite:
                    {
                        sceneSprite3D = new GameSprite3D();
                        sceneSprite3D.Create(Info.MaxObjectCount, Texture, 
                                             RenderingSpace.World, false);

                        sceneSprite3D.AlphaBlendEnable = 
                            sequenceInfo.AlphaBlendEnable;
                        sceneSprite3D.DepthBufferEnable = 
                            sequenceInfo.DepthBufferEnable;
                        sceneSprite3D.SourceBlend = 
                            sequenceInfo.SourceBlend;
                        sceneSprite3D.DestinationBlend = 
                            sequenceInfo.DestinationBlend;
                        sceneSprite3D.BlendFunction =
                            sequenceInfo.BlendFunction;
                        sceneSprite3D.DepthBufferEnable = true;
                        sceneSprite3D.DepthBufferWriteEnable = false;
                        sceneSprite3D.DepthBufferFunction = CompareFunction.Less;
                        sceneSprite3D.AlphaFunction = CompareFunction.Greater;
                        sceneSprite3D.CullMode = CullMode.None;

                        sceneRoot.AddChild(sceneSprite3D);

                        enableRotate = true; //  Possible to rotation
                    }
                    break;
                    //  Billboard
                case ParticleInfo.ParticleObjectType.AnchorBillboard:
                case ParticleInfo.ParticleObjectType.Billboard:
                    {
                        sceneBillboard = new GameBillboard();
                        sceneBillboard.Create(Info.MaxObjectCount, Texture, 
                                              RenderingSpace.World, false);

                        sceneBillboard.AlphaBlendEnable = 
                            sequenceInfo.AlphaBlendEnable;
                        sceneBillboard.DepthBufferEnable = 
                            sequenceInfo.DepthBufferEnable;
                        sceneBillboard.SourceBlend = 
                            sequenceInfo.SourceBlend;
                        sceneBillboard.DestinationBlend =
                            sequenceInfo.DestinationBlend;
                        sceneBillboard.BlendFunction = 
                            sequenceInfo.BlendFunction;
                        sceneBillboard.DepthBufferEnable = true;
                        sceneBillboard.DepthBufferWriteEnable = false;
                        sceneBillboard.DepthBufferFunction = CompareFunction.Less;
                        sceneBillboard.AlphaFunction = CompareFunction.Greater;
                        sceneBillboard.CullMode = CullMode.None;

                        sceneRoot.AddChild(sceneBillboard);
                    }
                    break;
                    //  Mesh
                case ParticleInfo.ParticleObjectType.Scene:
                    {
                        if( Info.MeshData != null)
                        {
                            int vertextCount = Info.MeshData.Position.Count;
                            int indexCount = 0;

                            if (Info.MeshData.Index != null)
                                indexCount = Info.MeshData.Index.Count;

                            sceneMesh = new GameMesh();
                            sceneMesh.Create(vertextCount, indexCount, Texture);

                            sceneMesh.AlphaBlendEnable = 
                                sequenceInfo.AlphaBlendEnable;
                            sceneMesh.DepthBufferEnable = 
                                sequenceInfo.DepthBufferEnable;
                            sceneMesh.SourceBlend =
                                sequenceInfo.SourceBlend;
                            sceneMesh.DestinationBlend = 
                                sequenceInfo.DestinationBlend;
                            sceneMesh.BlendFunction =
                                sequenceInfo.BlendFunction;
                            sceneMesh.DepthBufferEnable = true;
                            sceneMesh.DepthBufferWriteEnable = false;
                            sceneMesh.DepthBufferFunction = CompareFunction.Less;
                            sceneMesh.AlphaFunction = CompareFunction.Greater;
                            sceneMesh.CullMode = CullMode.None;

                            //  Has position data
                            if (Info.MeshData.HasPosition )
                            {
                                sceneMesh.SetPositionData(
                                                Info.MeshData.Position.ToArray());
                            }

                            //  Has color data
                            if (Info.MeshData.HasColor )
                            {
                                sceneMesh.SetColorData(
                                                Info.MeshData.Color.ToArray());
                            }

                            //  Has texture coordinate data
                            if (Info.MeshData.HasTextureCoord )
                            {
                                sceneMesh.SetTextureCoordData(
                                                Info.MeshData.TextureCoord.ToArray());
                            }

                            //  Has index data
                            if (Info.MeshData.HasIndex )
                            {
                                sceneMesh.SetIndexData(Info.MeshData.Index.ToArray());
                            }

                            //  Use VB and IB
                            if (sceneMesh.userPrimitive == false)
                            {
                                sceneMesh.BindVertexBuffer();
                                sceneMesh.BindIndexBuffer();
                            }

                            sceneRoot.AddChild(sceneMesh);
                        }

                        enableRotate = true; //  can rotate
                    }
                    break;
            }

            SetSceneEnable(false);

            //  Particle Object
            particleObjectList.Clear();
            for (int i = 0; i < Info.MaxObjectCount; i++)
            {
                ParticleObject obj = new ParticleObject();
                particleObjectList.Add(obj);
            }

            // Emit Right & Up Vector
            {
                Matrix mtx = Helper3D.MakeMatrixWithAt(Info.EmitDirection, 
                                                       Info.UpVector);

                emitRight = mtx.Right;
                emitUp = mtx.Up;
            }

            // Up Right & At Vector
            {
                Matrix mtx = Helper3D.MakeMatrixWithUp(Info.UpVector, 
                                                       Matrix.Identity.Forward);

                upRight = mtx.Right;
                upAt = mtx.Forward;
            }

            posArgCount = 0;
            for (int i = ParticleInfo.FuncCount - 1; i >= 0; i--)
            {
                if (Info.PositionFunc[i] != ParticleInfo.FuncType.None)
                {
                    posArgCount = i + 1;
                    break;
                }
            }

            scaleArgCount = 0;
            for (int i = ParticleInfo.FuncCount - 1; i >= 0; i--)
            {
                if (Info.ScaleFunc[i] != ParticleInfo.FuncType.None)
                {
                    scaleArgCount = i + 1;
                    break;
                }
            }

            if( Info.IsPositionStyle( ParticleInfo.ParamStyles.Interpolate))
                positionInterpolate = KeyFrameTable.Interpolation.Lerp;
            else
                positionInterpolate = KeyFrameTable.Interpolation.None;

            if (Info.IsScaleStyle(ParticleInfo.ParamStyles.Interpolate))
                scaleInterpolate = KeyFrameTable.Interpolation.Lerp;
            else
                scaleInterpolate = KeyFrameTable.Interpolation.None;

            if (Info.IsColorStyle(ParticleInfo.ParamStyles.Interpolate))
                colorInterpolate = KeyFrameTable.Interpolation.Lerp;
            else
                colorInterpolate = KeyFrameTable.Interpolation.None;

            rObjectLifeTime = 1.0f / Info.ObjectLifeTime;

            isStarting = false;
            isStopping = false;
        }      

        /// <summary>
        /// starts and enables the particle objects.
        /// </summary>
        public void Start()
        {
            SetSceneEnable(true);

            for (int i = 0; i < Info.MaxObjectCount; i++)
                particleObjectList[i].state = 0;

            for (int i = 0; i < Info.InitialObjectCount; i++)
            {
                ParticleObject obj = particleObjectList[i];
                EmitParticle(ref obj);
            }

            emitInterval = Info.EmitInterval;

            if (Info.PositionUpdateInterval > 0.0f)
                posRinterval = 1.0f / Info.PositionUpdateInterval;
            else
                posRinterval = 0.0f;

            if (Info.ScaleUpdateInterval > 0.0f)
                scaleRinterval = 1.0f / Info.ScaleUpdateInterval;
            else
                scaleRinterval = 0.0f;

            isStarting = true;
            isStopping = false;
            localTime = 0.0f;
        }

        /// <summary>
        /// disables the particle objects.
        /// </summary>
        public void Stop()
        {
            isStopping = true;
            localTime = 0.0f;
        }

        /// <summary>
        /// emits the specified particle object according to the emit type.
        /// </summary>
        /// <param name="obj">a particle object</param>
        protected void EmitParticle(ref ParticleObject obj)
        {
            float massRange = 0.0f;
            Matrix mtx1 = Matrix.Identity;
            float rnd1 = 0.0f, rnd2 = 0.0f;

            obj.state = (uint)(ParticleObject.States.Enable | 
                               ParticleObject.States.Fired | 
                               ParticleObject.States.FirstTime);

            obj.time = 0.0f;
            obj.posInterval = Info.PositionUpdateInterval;
            obj.scaleInterval = Info.ScaleUpdateInterval;
            obj.textureSequenceInterval = 0.0f;

            massRange = Info.MassMax - Info.MassMin;
            obj.mass = Info.MassMin + (HelperMath.RandomNormal() * massRange);
            obj.gravity = Vector3.Zero;
            obj.posRandomInterval = Info.PositionRandomInterval;

            switch (Info.EmitType)
            {
                //  CONE type : changes tje direction randomly 
                //  from the default EmitDirection.
                case ParticleInfo.EmitObjectType.Cone:
                    {
                        rnd1 = HelperMath.RandomNormal2() * Info.EmitAngle;
                        rnd2 = HelperMath.RandomNormal2() * Info.EmitAngle;

                        mtx1 = Matrix.CreateFromAxisAngle(emitRight, 
                                                          MathHelper.ToRadians(rnd1));

                        mtx1 = mtx1 * Matrix.CreateFromAxisAngle(emitUp, 
                                                          MathHelper.ToRadians(rnd2));

                        obj.emitDir = Vector3.TransformNormal(Info.EmitDirection, mtx1);

                        obj.startPos = Info.EmitPosition;
                    }
                    break;
                //  SPHERE type : make the emit direction to the total random direction.
                case ParticleInfo.EmitObjectType.Sphere:
                    {
                        obj.emitDir = HelperMath.RandomVector2();
                        obj.emitDir.Normalize();
                        obj.startPos = Info.EmitPosition;
                    }
                    break;
                //  Disk type: basically same as the cone,
                //  (however, except the rotation around the EmitUpVector as the axis)
                //  it makes the direction which is randomly turned 
                //  as the UpVector as the axis. 
                case ParticleInfo.EmitObjectType.Disk:
                    {
                        rnd1 = HelperMath.RandomNormal2() * Info.EmitAngle;

                        mtx1 = Matrix.CreateFromAxisAngle(emitRight, 
                                                          MathHelper.ToRadians(rnd1));

                        mtx1 = mtx1 * Matrix.CreateFromAxisAngle(Info.UpVector, 
                                                        HelperMath.RandomNormal() *
                                                        2.0f *
                                                        MathHelper.Pi);

                        obj.emitDir = Vector3.TransformNormal(Info.EmitDirection, mtx1);

                        obj.startPos = Info.EmitPosition;
                    }
                    break;
                //  RECT_PLANE type : The EmitAngle will follow the size 
                //  of a square area, not an angle.
                //  The EmitDirection is always constant.
                case ParticleInfo.EmitObjectType.RectPlane:
                    {
                        obj.emitDir = Info.EmitDirection;

                        rnd1 = (HelperMath.RandomNormal2() * 0.5f) * Info.EmitAngle;
                        rnd2 = (HelperMath.RandomNormal2() * 0.5f) * Info.EmitAngle;
                        obj.pos[0] = upRight * rnd1;
                        obj.pos[0] += upAt * rnd2;

                        obj.startPos = obj.pos[0] + Info.EmitPosition;
                    }
                    break;
                //  CIRCLE_PLANE type : The circular area whose radius is the EmitAngle.
                case ParticleInfo.EmitObjectType.CirclePlane:
                    {
                        obj.emitDir = Info.EmitDirection;

                        rnd1 = HelperMath.RandomNormal() * Info.EmitAngle;
                        rnd2 = HelperMath.RandomNormal() * MathHelper.Pi * 2.0f;

                        obj.pos[0] = upRight * rnd1;  //  Scaling

                        mtx1 = Matrix.CreateFromAxisAngle(Info.UpVector, rnd2);

                        obj.pos[0] = Helper3D.TransformCoord(obj.pos[0], mtx1);
                        obj.startPos = obj.pos[0] + Info.EmitPosition;
                    }
                    break;
            }

            if (refEnable)
            {
                obj.startPos = Helper3D.TransformCoord(obj.startPos, (Matrix)refMatrix);
                obj.emitDir = Vector3.TransformNormal(obj.emitDir, (Matrix)refMatrix);
            }

            obj.pos[0] = obj.startPos;
            obj.scale = Vector3.Zero;

            obj.posRandom = 1.0f + ((HelperMath.RandomNormal2() * 0.5f) * 
                                    Info.PositionInitialRandomFactor);

            obj.scaleRandom = 1.0f + ((HelperMath.RandomNormal2() * 0.5f) * 
                                        Info.ScaleInitialRandomFactor);

            obj.rotateRandom = HelperMath.RandomNormal2();

            switch (Info.ParticleType)
            {
                case ParticleInfo.ParticleObjectType.AnchorBillboard:
                    {
                        obj.pos[1] = obj.pos[0];
                    }
                    break;
                case ParticleInfo.ParticleObjectType.Scene:
                    {
                        obj.rotateDir = HelperMath.RandomVector2();
                    }
                    break;
            }

            {
                for (int i = 0; i < posArgCount; i++)
                    obj.posArg[i] = Info.PositionInit[i];

                for (int i = 0; i < scaleArgCount; i++)
                    obj.scaleArg[i] = Info.ScaleInit[i];

                obj.rotateAccm = Info.RotateInit + (HelperMath.RandomNormal2() * 
                                                    Info.RotateRandomFactor);
            }
        }

        /// <summary>
        /// It calculates the local time and controls the particle’s 
        /// start and stop functions.
        /// </summary>
        /// <param name="gameTime">game time</param>
        protected override void OnUpdate(GameTime gameTime)
        {
            if (localTime == 0.0f && isStarting == false)
            {
                //  Starting particle
                Start();
            }

            // If life time is 0, it's infinity time
            if (Info.LifeTime > 0.0f)
            {
                //  time goes
                localTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                changeTime = true;

                if (localTime > Info.LifeTime)
                {
                    //  Stopping particle. 
                    //  It will be not finish this particle until stop \
                    //  all active particle object
                    Stop();
                }
            }
            else
            {   
                //  forever active that particle's LifeTime is zero
                changeTime = true;
            }

            //  Update particle transform
            {
                if (scenePointSprite != null)
                {
                    scenePointSprite.RootAxis = this.RootAxis;
                    scenePointSprite.WorldTransform = this.WorldTransform;
                }

                if (sceneSprite3D != null)
                {
                    sceneSprite3D.RootAxis = this.RootAxis;
                    sceneSprite3D.WorldTransform = this.WorldTransform;
                }

                if (sceneBillboard != null)
                {
                    sceneBillboard.RootAxis = this.RootAxis;
                    sceneBillboard.WorldTransform = this.WorldTransform;
                }

                if (sceneMesh != null)
                {
                    sceneMesh.RootAxis = this.RootAxis;
                    sceneMesh.WorldTransform = this.WorldTransform;
                }
            }
        }

        /// <summary>
        /// It calls Drive() function by using the data of the ParticleInfo to update 
        /// the information of each ParticleObject’s position, rotation, and scale.
        /// </summary>
        /// <param name="renderTracer">render information in the RenderContext</param>
        protected override void OnDraw(RenderTracer renderTracer)
        {
            //  Update particle info
            Drive(renderTracer);
        }

        /// <summary>        
        /// When the information of each ParticleObject gets updated, 
        /// the vertex values of the GameMesh, GamePointSprite, GameSprite3D, and
        /// GameBillboard classes are updated.
        /// </summary>
        /// <param name="renderTracer">render information in the RenderContext</param>
        protected void Drive(RenderTracer renderTracer)
        {
            int emitCount = 0, aliveCount = 0;
            float time = 0, s = 0.0f;
            float lifeTime = 0.0f;
            Vector3 dir = Vector3.Zero;;
            bool isEnable = false;
            bool isEmit = false;
            bool isInterpolate = false;
            Matrix mtx = Matrix.Identity;
            Vector3 vGravDir = Vector3.Zero;
            bool isUv = false;
            Vector2 uv1 = Vector2.Zero;
            Vector2 uv2 = Vector2.Zero;

            // If the changeTime is false, no update
            if (changeTime == false) return;
            else if (changeTime ) changeTime = false;

            time = (float)renderTracer.GameTime.ElapsedGameTime.TotalSeconds;

            emitInterval -= time;
            if (isStopping == false && emitInterval <= 0.0f)
            {
                //  New particle object emit
                isEmit = true;
                emitInterval = Info.EmitInterval;
            }

            mtx = this.TransformedMatrix * renderTracer.View;
            mtx = Helper3D.Transpose(mtx);

            //  Calculate gravity direction 
            vGravDir = Vector3.TransformNormal(globalGravityDirection, mtx );

            for (int i = 0; i < Info.MaxObjectCount; i++)
            {
                ParticleObject obj = particleObjectList[i];

                isEnable = true;
                isUv = false;
                lifeTime = 0.0f;

                //  If the object state is deactivate
                if (!obj.IsState(ParticleObject.States.Enable))
                {                    
                    if ((isEmit ) && (emitCount < Info.EmitCount))
                    {
                        if (!((Info.Volatile ) && 
                            obj.IsState(ParticleObject.States.Fired)))
                        {
                            emitCount++;

                            //  Emit a new particle object
                            EmitParticle(ref obj);
                        }
                        else
                        {
                            isEnable = false;
                        }
                    }
                    else
                    {
                        isEnable = false;
                    }
                }
                else
                {
                    // If the object state is activate
                    obj.time += time;
                    lifeTime = obj.time * rObjectLifeTime;
                    if (lifeTime >= 1.0f)
                    {
                        //  The object is finished
                        if ((isEmit ) && (Info.Volatile == false))
                        {
                            //  Finished particle object is restarting!
                            emitCount++;

                            //  Emit a new particle object
                            EmitParticle(ref obj);

                            lifeTime = 0.0f;
                        }
                        //  Has to deactivate
                        else
                        {
                            obj.state &= ~((uint)ParticleObject.States.Enable);
                            isEnable = false;
                        }
                    }
                }

                if (isEnable == false)
                {
                    switch (Info.ParticleType)
                    {
                        case ParticleInfo.ParticleObjectType.PointSprite:
                            {
                                scenePointSprite.SetObjectEnable(i, false);
                            }
                            break;
                        case ParticleInfo.ParticleObjectType.Sprite:
                            {
                                sceneSprite3D.SetObjectEnable(i, false);
                            }
                            break;
                        case ParticleInfo.ParticleObjectType.AnchorBillboard:
                        case ParticleInfo.ParticleObjectType.Billboard:
                            {
                                sceneBillboard.SetUpdateType(i, false);
                            }
                            break;
                        case ParticleInfo.ParticleObjectType.Scene:
                            {
                                sceneMesh.Enabled = false;
                                sceneMesh.Visible = false;
                            }
                            break;
                    }
                }
                else
                {
                    float tempInterval = 0.0f;

                    isInterpolate = true;
                    aliveCount++;

                    //////////////////////////////////////////////////////////
                    //  Position
                    obj.posInterval += time;
                    tempInterval = obj.posInterval;

                    if (tempInterval >= Info.PositionUpdateInterval)
                    {
                        int last = posArgCount - 1;

                        if (Info.PositionUpdateInterval < time)
                        {
                            tempInterval = obj.posInterval = time;
                            isInterpolate = false;
                        }

                        switch (Info.PositionFunc[last])
                        {
                            case ParticleInfo.FuncType.Constant:
                                {
                                    obj.posArg[last] = Info.PositionInit[last];
                                }
                                break;
                            case ParticleInfo.FuncType.Sin:
                                {
                                    obj.posArg[last] = 
                                        (float)Math.Sin(MathHelper.PiOver2 * lifeTime) *
                                        Info.PositionFactor[last];
                                }
                                break;
                            case ParticleInfo.FuncType.Cos:
                                {
                                    obj.posArg[last] = 
                                        (float)Math.Cos(MathHelper.PiOver2 * lifeTime) *
                                        Info.PositionFactor[last];
                                }
                                break;
                            case ParticleInfo.FuncType.Rnd:
                                {
                                    obj.posArg[last] = HelperMath.RandomNormal2();
                                }
                                break;
                            case ParticleInfo.FuncType.Table:
                                {
                                    KeyFrameTable table = Info.PositionTable[last];

                                    obj.posArg[last] = table.GetKeyFrame(lifeTime,
                                        positionInterpolate);
                                }
                                break;
                        }

                        obj.posArg[last] *= obj.posRandom;

                        if (last == 1)
                        {
                            //  Accelation...v = v0 + at
                            obj.posArg[0] += obj.posArg[1] * tempInterval;
                        }

                        if (Info.IsPositionStyle(ParticleInfo.ParamStyles.Clamp))
                        {
                            obj.posArg[0] = MathHelper.Clamp(obj.posArg[0], 
                                                            Info.PositionMin, 
                                                            Info.PositionMax);
                        }

                        //  Move distance...s = vt
                        s = obj.posArg[0] * tempInterval;

                        //  Calculate move amount
                        dir = obj.emitDir * s;

                        //  Applies acceleration of gravity 
                        if (Info.IsPositionStyle(ParticleInfo.ParamStyles.Gravity))
                        {
                            float _fact = globalGravityFactor * (obj.mass * 
                                (tempInterval * tempInterval));

                            obj.gravity += vGravDir * _fact;
                            dir += obj.gravity;
                        }

                        obj.targetPos = obj.pos[0] + dir;
                        obj.startPos = obj.pos[0];

                        obj.posInterval -= Info.PositionUpdateInterval;
                    }

                    //  If random type
                    if (Info.IsPositionStyle(ParticleInfo.ParamStyles.Random))
                    {
                        obj.posRandomInterval -= time;

                        if (obj.posRandomInterval <= 0.0f)
                        {
                            Vector3 rndVec = HelperMath.RandomVector2();

                            rndVec *= Info.PositionRandomFactor;
                            obj.targetPos += rndVec;

                            obj.posRandomInterval = Info.PositionRandomInterval;
                        }
                    }

                    //  Trace to position
                    if (Info.ParticleType == ParticleInfo.ParticleObjectType.Billboard)
                    {
                        obj.pos[1] = obj.pos[0];
                    }

                    // Position interpolate
                    if (isInterpolate)
                    {
                        obj.pos[0] = Vector3.Lerp(obj.startPos, obj.targetPos,
                                      (obj.posInterval * posRinterval));
                    }
                    else
                    {
                        obj.pos[0] = obj.targetPos;
                    }

                    //////////////////////////////////////////////////////////
                    // Scale
                    isInterpolate = true;

                    obj.scaleInterval += time;
                    tempInterval = obj.scaleInterval;

                    if (tempInterval >= Info.ScaleUpdateInterval)
                    {
                        int last = scaleArgCount - 1;

                        if (Info.ScaleUpdateInterval < time)
                        {
                            tempInterval = obj.scaleInterval = time;
                            isInterpolate = false;
                        }

                        switch (Info.ScaleFunc[last])
                        {
                            case ParticleInfo.FuncType.Constant:
                                {
                                    obj.scaleArg[last] = Info.ScaleInit[last];
                                }
                                break;
                            case ParticleInfo.FuncType.Sin:
                                {
                                    obj.scaleArg[last] = 
                                        (float)Math.Sin(MathHelper.PiOver2 * lifeTime) *
                                        Info.ScaleFactor[last];
                                }
                                break;
                            case ParticleInfo.FuncType.Cos:
                                {
                                    obj.scaleArg[last] = 
                                        (float)Math.Cos(MathHelper.PiOver2 * lifeTime) *
                                        Info.ScaleFactor[last];
                                }
                                break;
                            case ParticleInfo.FuncType.Rnd:
                                {
                                    obj.scaleArg[last] = HelperMath.RandomNormal2();
                                }
                                break;
                            case ParticleInfo.FuncType.Table:
                                {
                                    KeyFrameTable table = Info.ScaleTable[last];
                                    obj.scaleArg[last] = table.GetKeyFrame(lifeTime, 
                                        scaleInterpolate);
                                }
                                break;
                        }

                        // Applies random value
                        obj.scaleArg[last] *= obj.scaleRandom;

                        if( last == 1)
                            obj.scaleArg[0] += obj.scaleArg[1] * tempInterval;

                        if (Info.IsScaleStyle(ParticleInfo.ParamStyles.Clamp))
                        {
                            obj.scaleArg[0] = MathHelper.Clamp(obj.scaleArg[0],
                                                        Info.ScaleMin, Info.ScaleMax);
                        }

                        obj.targetScale = Info.ScaleMask * obj.scaleArg[0];

                        //  back up the previous scale value
                        obj.startScale = obj.scale;       

                        obj.scaleInterval -= Info.ScaleUpdateInterval;
                    }

                    if (isInterpolate)
                    {
                        obj.scale = Vector3.Lerp(obj.startScale, obj.targetScale,
                                    (obj.scaleInterval * scaleRinterval));
                    }
                    else
                    {
                        obj.scale = obj.targetScale;
                    }

                    //////////////////////////////////////////////////////////
                    // Rotation
                    if( enableRotate)
                    {
                        switch(Info.RotateFunc)
                        {
                            case ParticleInfo.FuncType.None:
                                {
                                    obj.rotateArg = 0.0f;
                                }
                                break;
                            case ParticleInfo.FuncType.Constant:
                                {
                                    obj.rotateArg = Info.RotateInit * time;
                                }
                                break;
                            case ParticleInfo.FuncType.Sin:
                                {
                                    obj.rotateArg = 
                                        (float)Math.Sin(MathHelper.PiOver2 * lifeTime) *
                                        Info.RotateFactor;
                                }
                                break;
                            case ParticleInfo.FuncType.Cos:
                                {
                                    obj.rotateArg = 
                                        (float)Math.Sin(MathHelper.PiOver2 * lifeTime) *
                                        Info.RotateFactor;
                                }
                                break;
                            case ParticleInfo.FuncType.Rnd:
                                {
                                    obj.rotateArg = HelperMath.RandomNormal2();
                                }
                                break;
                            case ParticleInfo.FuncType.Table:
                                {
                                    KeyFrameTable table = Info.RotateTable;

                                    obj.rotateArg = table.GetKeyFrame(lifeTime,       
                                                    KeyFrameTable.Interpolation.Lerp);
                                }
                                break;
                        }

                        obj.rotateAccm += obj.rotateArg * obj.rotateRandom;
                    }

                    //////////////////////////////////////////////////////////
                    // Color
                    {
                        byte r = obj.color.R;
                        byte g = obj.color.G;
                        byte b = obj.color.B;
                        byte a = obj.color.A;

                        if (!Info.IsColorStyle(ParticleInfo.ParamStyles.Random) || 
                            obj.IsState(ParticleObject.States.FirstTime))
                        {                            
                            switch (Info.RgbFunc)
                            {
                                case ParticleInfo.FuncType.Constant:
                                    {
                                        r = Info.RgbInitValue.R;
                                        g = Info.RgbInitValue.G;
                                        b = Info.RgbInitValue.B;
                                    }
                                    break;
                                case ParticleInfo.FuncType.Table:
                                    {
                                        float rt = 0.0f;

                                        if (Info.IsColorStyle(
                                                    ParticleInfo.ParamStyles.Random))
                                        {
                                            rt = HelperMath.RandomNormal();
                                        }
                                        else
                                        {
                                            rt = lifeTime;
                                        }

                                        r = (byte)Info.Rtable.GetKeyFrame(rt, 
                                                                    colorInterpolate);

                                        g = (byte)Info.Gtable.GetKeyFrame(rt, 
                                                                    colorInterpolate);

                                        b = (byte)Info.Btable.GetKeyFrame(rt, 
                                                                    colorInterpolate);
                                    }
                                    break;
                                default:
                                    {
                                        r = 255;
                                        g = 255;
                                        b = 255;
                                    }
                                    break;
                            }
                        }
                        
                        float alpha = 255f;
                        switch (Info.AlphaFunc)
                        {
                            case ParticleInfo.FuncType.Constant:
                                alpha = (float)Info.AlphaInit;
                                break;
                            case ParticleInfo.FuncType.Sin:
                                alpha = 255f * (float)Math.Sin(
                                    MathHelper.PiOver2 * lifeTime);
                                break;
                            case ParticleInfo.FuncType.Cos:
                                alpha = 255f * (float)Math.Cos(
                                    MathHelper.PiOver2 * lifeTime);
                                break;
                            case ParticleInfo.FuncType.Table:
                                alpha = Info.Atable.GetKeyFrame(lifeTime, 
                                    colorInterpolate);
                                break;
                            default:
                                alpha = 255f;
                                break;
                        }
                        a = (byte)alpha;

                        obj.color = new Color(r, g, b, a);
                    }

                    //////////////////////////////////////////////////////////
                    // TextureSequence
                    if (textureSequence != null)
                    {
                        obj.textureSequenceInterval -= time;

                        // Calculate texture sequence
                        if( (obj.textureSequenceInterval <= 0.0f) &&
                            ((textureSequence.IsFixedFrameMode == false) || 
                            (obj.IsState( ParticleObject.States.FirstTime))))
                        {
                            obj.textureSequenceInterval = textureSequence.StaticInterval;

                            textureSequence.GetUV(obj.time, out uv1, out uv2);
                            isUv = true;
                        }
                    }

                    switch (Info.ParticleType)
                    {
                        case ParticleInfo.ParticleObjectType.PointSprite:
                            {
                                scenePointSprite.SetObjectEnable(i, true);
                                scenePointSprite.SetCenter(i, obj.pos[0]);
                                scenePointSprite.SetSize(i, obj.scale.X, obj.scale.Y);
                                scenePointSprite.SetColor(i, obj.color);

                                if (isUv)
                                    scenePointSprite.SetTextureCoord(i, uv1, uv2);
                            }
                            break;
                        case ParticleInfo.ParticleObjectType.Sprite:
                            {
                                sceneSprite3D.SetObjectEnable(i, true);
                                sceneSprite3D.SetCenter(i, obj.pos[0]);
                                sceneSprite3D.SetSize(i, obj.scale.X, obj.scale.Y);
                                sceneSprite3D.SetColor(i, obj.color);
                                sceneSprite3D.SetRotation(i, obj.rotateAccm);

                                if (isUv)
                                    sceneSprite3D.SetTextureCoord(i, uv1, uv2);
                            }
                            break;
                        case ParticleInfo.ParticleObjectType.Billboard:
                            {
                                Vector3 diff = Vector3.Zero;
                                diff = obj.pos[0] - obj.pos[1];
                                diff *= Info.ScaleBillboardFactor;
                                diff += obj.pos[0];

                                sceneBillboard.SetUpdateType(i, true);
                                sceneBillboard.SetStart(i, obj.pos[0]);
                                sceneBillboard.SetEnd(i, diff);
                                sceneBillboard.SetSize(i, obj.scale.Y);
                                sceneBillboard.SetColor(i, obj.color);

                                if(isUv)
                                    sceneBillboard.SetTextureCoord(i, uv1, uv2);
                            }
                            break;
                        case ParticleInfo.ParticleObjectType.AnchorBillboard:
                            {
                                sceneBillboard.SetUpdateType(i, true);
                                sceneBillboard.SetStart(i, obj.pos[1]);
                                sceneBillboard.SetEnd(i, obj.pos[0]);
                                sceneBillboard.SetSize(i, obj.scale.Y);
                                sceneBillboard.SetColor(i, obj.color);

                                if (isUv)
                                    sceneBillboard.SetTextureCoord(i, uv1, uv2);
                            }
                            break;
                        case ParticleInfo.ParticleObjectType.Scene:
                            {
                                sceneMesh.Enabled = true;
                                sceneMesh.Visible = true;

                                //  Rotation
                                Matrix localTransform = Matrix.CreateFromAxisAngle(
                                                        obj.rotateDir, obj.rotateAccm);

                                //  Scale
                                localTransform = Helper3D.PostScale(localTransform,
                                    obj.scale);

                                //  Position
                                localTransform.Translation = obj.pos[0];

                                //  Set transform
                                sceneMesh.WorldTransform = localTransform * 
                                                           sceneMesh.WorldTransform;
                            }
                            break;
                    }
                }

                obj.state &= ~((uint)ParticleObject.States.FirstTime);
            }

            if (isStopping && aliveCount == 0)
            {
                //  Particle disabled
                SetSceneEnable(false);
            }
        }
    }

    #endregion
}
