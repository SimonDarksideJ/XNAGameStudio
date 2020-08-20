#region File Description
//-----------------------------------------------------------------------------
// GameModel.cs
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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData.Render;
using RobotGameData.Resource;
using RobotGameData.Collision;
#endregion


namespace RobotGameData.GameObject
{
    /// <summary>
    /// It contains and processes the XNA’s “Model” variable.
    /// </summary>
    public class GameModel : GameSceneNode
    {
        #region Fields

        ModelData modelData = null;
        Vector3 veclocity = Vector3.Zero;
        Matrix rotateMatrix = Matrix.Identity;
        Matrix[] boneTransforms = null;
        ModelBone rootBone = null;

        RenderLighting[] lighting = null;
        RenderMaterial material = null;
        bool activeLighting = false;
        bool activeFog = false;

        bool enableCulling = false;
        CollideElement modelCollide = null;        
        BoundingSphere cullingSphere = new BoundingSphere();
        Vector3 cullingSphereLocalCenter = Vector3.Zero;

        bool alphaTestEnable = false;
        bool alphaBlendEnable = false;
        CompareFunction alphaFunction = CompareFunction.Always;
        Blend sourceBlend = Blend.One;
        Blend destinationBlend = Blend.Zero;
        BlendFunction blendFunction = BlendFunction.Add;
        int referenceAlpha = 0;
        bool depthBufferEnable = true;
        bool depthBufferWriteEnable = true;
        CompareFunction depthBufferFunction = CompareFunction.LessEqual;
        CullMode cullMode = CullMode.CullCounterClockwiseFace;

        #endregion

        #region Events

        public class RenderingCustomEffectEventArgs : EventArgs
        {
            private RenderTracer renderTracer;
            public RenderTracer RenderTracer
            {
                get { return renderTracer; }
            }

            private ModelMesh mesh;
            public ModelMesh Mesh
            {
                get { return mesh; }
            }

            private Effect effect;
            public Effect Effect
            {
                get { return effect; }
            }

            private Matrix world;
            public Matrix World
            {
                get { return world; }
            }

            public RenderingCustomEffectEventArgs(RenderTracer renderTracer,
                ModelMesh mesh, Effect effect, Matrix world)
                : base()
            {
                this.renderTracer = renderTracer;
                this.mesh = mesh;
                this.effect = effect;
                this.world = world;
            }
        }

        public event EventHandler<RenderingCustomEffectEventArgs> RenderingCustomEffect;

        #endregion

        #region Properties

        public ModelData ModelData
        {
            get { return modelData; }
            protected set { modelData = value; }
        }

        public Matrix[] BoneTransforms
        {
            get { return boneTransforms; }
            protected set { boneTransforms = value; }
        }

        public ModelBone RootBone
        {
            get { return rootBone; }
            protected set { rootBone = value; }
        }

        public CollideElement Collide
        {
            get { return modelCollide; }
            protected set { modelCollide = value; }
        }

        public bool EnableCulling
        {
            get { return enableCulling; }
            set { enableCulling = value; }
        }

        public bool ActiveLighting
        {
            get { return activeLighting; }
            set { activeLighting = value; }
        }

        public bool ActiveFog
        {
            get { return activeFog; }
            set { activeFog = value; }
        }

        public Vector3 Velocity
        {
            get { return this.veclocity; }
            protected set { this.veclocity = value; }
        }

        public RenderLighting[] Lighting
        {
            get { return lighting; }
            protected set { lighting = value; }
        }

        public RenderMaterial Material
        {
            get { return material; }
            set { material = value; }
        }

        public bool AlphaTestEnable
        {
            get { return alphaTestEnable; }
            set { alphaTestEnable = value; }
        }

        public bool AlphaBlendEnable
        {
            get { return alphaBlendEnable; }
            set { alphaBlendEnable = value; }
        }

        public int ReferenceAlpha
        {
            get { return referenceAlpha; }
            set { referenceAlpha = value; }
        }        

        public CompareFunction AlphaFunction
        {
            get { return alphaFunction; }
            set { alphaFunction = value; }
        }

        public bool DepthBufferEnable
        {
            get { return depthBufferEnable; }
            set { depthBufferEnable = value; }
        }

        public bool DepthBufferWriteEnable
        {
            get { return depthBufferWriteEnable; }
            set { depthBufferWriteEnable = value; }
        }

        public CompareFunction DepthBufferFunction
        {
            get { return depthBufferFunction; }
            set { depthBufferFunction = value; }
        }

        public Blend SourceBlend
        {
            get { return sourceBlend; }
            set { sourceBlend = value; }
        }

        public Blend DestinationBlend
        {
            get { return destinationBlend; }
            set { destinationBlend = value; }
        }

        public BlendFunction BlendFunction
        {
            get { return blendFunction; }
            set { blendFunction = value; }
        }

        public CullMode CullMode
        {
            get { return cullMode; }
            set { cullMode = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resource">model resource</param>
        public GameModel(GameResourceModel resource)
            : base()
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            BindModel(resource.ModelData);
        }

        public GameModel(string fileName)
            : base()
        {
            LoadModel(fileName);
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            //  model moves to the position
            if (this.veclocity != Vector3.Zero)
            {
                Vector3 velocityPerFrame = 
                            CalculateVelocityPerFrame(gameTime, this.veclocity);

                AddPosition(velocityPerFrame);                
            }

            //  Updates collision mesh
            if (modelCollide != null)
            {
                modelCollide.Transform(TransformedMatrix);
            }

            //  If Animated bones
            if (this is GameAnimateModel)
            {
                //  If this is root bone, 
                //  the world transformed matrix weight with only root bone
                this.ModelData.model.Root.Transform *= this.TransformedMatrix;
            }
            //  If no animated bones (static bones)
            else
            {   // Set the world matrix as the root transform of the model.
                this.ModelData.model.Root.Transform = this.TransformedMatrix;
            }

            // Look up combined bone matrices for the entire the model.
            this.ModelData.model.CopyAbsoluteBoneTransformsTo(this.boneTransforms);
        }

        protected override void OnDraw(RenderTracer renderTracer)
        {
            RenderState renderState = renderTracer.Device.RenderState;

            //  Transform culling sphere
            if (enableCulling )
            {
                cullingSphere.Center = Vector3.Transform(cullingSphereLocalCenter,
                                                RootAxis * Collide.TransformMatrix);

                //  Check if the the model is contained inside or intersecting
                //  with the frustum.
                ContainmentType cullingResult = 
                                    renderTracer.Frustum.Contains(cullingSphere);

                //  Draw a the model If inside in the frustum
                if (cullingResult == ContainmentType.Disjoint)
                    return;
            }

            renderState.AlphaTestEnable = alphaTestEnable;
            renderState.AlphaBlendEnable = alphaBlendEnable;
            renderState.AlphaFunction = alphaFunction;
            renderState.SourceBlend = sourceBlend;
            renderState.DestinationBlend = destinationBlend;
            renderState.BlendFunction = blendFunction;

            renderState.ReferenceAlpha = referenceAlpha;
            renderState.DepthBufferEnable = depthBufferEnable;
            renderState.DepthBufferWriteEnable = depthBufferWriteEnable;
            renderState.DepthBufferFunction = depthBufferFunction;
            renderState.CullMode = cullMode;

            // Draw the model.
            for( int i = 0; i < ModelData.model.Meshes.Count; i++)
            {
                ModelMesh mesh = ModelData.model.Meshes[i];

                for (int j = 0; j < mesh.Effects.Count; j++)
                {
                    //  call a entried custom effect processing
                    if (RenderingCustomEffect != null)
                    {
                        //  Shader custom processing
                        RenderingCustomEffect(this, 
                            new RenderingCustomEffectEventArgs(renderTracer, mesh,
                            mesh.Effects[j], BoneTransforms[mesh.ParentBone.Index]));
                    }
                    else if (mesh.Effects[j] is BasicEffect)
                    {
                        BasicEffect effect = (BasicEffect)mesh.Effects[j];

                        //  Apply fog
                        if (renderTracer.Fog != null && ActiveFog )
                        {
                            RenderFog fog = renderTracer.Fog;

                            effect.FogEnabled = fog.enabled;

                            if (effect.FogEnabled )
                            {
                                effect.FogStart = fog.start;
                                effect.FogEnd = fog.end;
                                effect.FogColor = fog.color.ToVector3();
                            }
                        }
                        else
                        {
                            effect.FogEnabled = false;
                        }

                        if (ActiveLighting )
                        {
                            //  Apply lighting
                            if (renderTracer.Lighting != null)
                            {
                                RenderLighting lighting = renderTracer.Lighting;

                                effect.LightingEnabled = lighting.enabled;

                                if (effect.LightingEnabled )
                                {
                                    effect.AmbientLightColor =
                                                lighting.ambientColor.ToVector3();

                                    effect.DirectionalLight0.Enabled = true;

                                    effect.DirectionalLight0.Direction =
                                                lighting.direction;

                                    effect.DirectionalLight0.DiffuseColor =
                                                lighting.diffuseColor.ToVector3();

                                    effect.DirectionalLight0.SpecularColor =
                                                lighting.specularColor.ToVector3();
                                }
                            }

                            if (Lighting != null)
                            {
                                effect.LightingEnabled = true;

                                for (int cnt = 0; cnt < Lighting.Length; cnt++)
                                {
                                    RenderLighting lighting = Lighting[cnt];
                                    BasicDirectionalLight basicLight = null;

                                    if (cnt == 0)
                                        basicLight = effect.DirectionalLight1;
                                    else if (cnt == 1)
                                        basicLight = effect.DirectionalLight2;
                                    else
                                        continue;

                                    if (lighting.enabled )
                                    {
                                        basicLight.Enabled = true;

                                        basicLight.Direction =
                                                        lighting.direction;

                                        basicLight.DiffuseColor =
                                                lighting.diffuseColor.ToVector3();

                                        basicLight.SpecularColor =
                                                lighting.specularColor.ToVector3();
                                    }
                                    else
                                    {
                                        basicLight.Enabled = false;
                                    }
                                }
                            }

                            if (renderTracer.Lighting == null && Lighting == null)
                            {
                                effect.LightingEnabled = false;
                            }

                            //  Apply material
                            if (Material != null)
                            {
                                effect.Alpha = Material.alpha;

                                effect.DiffuseColor =
                                        Material.diffuseColor.ToVector3();

                                effect.SpecularColor =
                                        Material.specularColor.ToVector3();

                                effect.SpecularPower =
                                        Material.specularPower;

                                effect.EmissiveColor =
                                        Material.emissiveColor.ToVector3();

                                effect.VertexColorEnabled =
                                        Material.vertexColorEnabled;

                                effect.PreferPerPixelLighting =
                                        Material.preferPerPixelLighting;
                            }
                        }
                        else
                        {
                            effect.LightingEnabled = false;
                        }

                        //  Apply transform
                        effect.World = BoneTransforms[mesh.ParentBone.Index];
                        effect.View = renderTracer.View;
                        effect.Projection = renderTracer.Projection;
                    }
                }

                mesh.Draw();
            }
        }

        protected override void OnReset()
        {
            this.veclocity = Vector3.Zero;

            //  Reset the bone's transform by source transform
            this.ModelData.model.CopyBoneTransformsFrom(this.ModelData.boneTransforms);

            // Set the world matrix as the root transform of the model.
            ModelData.model.Root.Transform = Matrix.Identity;

            // Look up combined bone matrices for the entire the model.
            ModelData.model.CopyAbsoluteBoneTransformsTo(this.boneTransforms);

            base.OnReset();
        }

        public void LoadModel(string modelFileName)
        {
            //  First, Find the model resource from ResourceManager by key
            GameResourceModel resource = 
                FrameworkCore.ResourceManager.GetModel(modelFileName);

            if (resource == null)
            {
                // Load the model.
                FrameworkCore.ResourceManager.LoadContent<Model>(modelFileName, 
                    modelFileName);

                resource = FrameworkCore.ResourceManager.GetModel(modelFileName);
            }

            //  Load and find resource failed.
            if (resource == null)
            {
                throw new ArgumentException("Cannot load the model : " +
                    modelFileName);
            }

            BindModel(resource.ModelData);
        }

        public virtual void BindModel(ModelData modelData)
        {
            this.ModelData = modelData;
            this.rootBone = modelData.model.Root;

            //  Set to bone transform matrix
            this.boneTransforms = new Matrix[this.ModelData.model.Bones.Count];
            this.ModelData.model.CopyAbsoluteBoneTransformsTo(this.boneTransforms);

            // Compute the bounding sphere of the ModelData.
            cullingSphere = new BoundingSphere();

            for (int i = 0; i < this.ModelData.model.Meshes.Count; i++)
            {
                ModelMesh mesh = this.ModelData.model.Meshes[i];

                cullingSphere = BoundingSphere.CreateMerged(cullingSphere, 
                                                            mesh.BoundingSphere);
            }

            cullingSphereLocalCenter = cullingSphere.Center;
        }

        public void Move(Vector3 velocity)
        {
            this.veclocity = velocity;
        }

        public void MoveStop()
        {
            this.veclocity = Vector3.Zero;
        }

        public Vector3 CalculateVelocity(Vector3 velocity)
        {
            Vector3 v = Vector3.Zero;

            if (velocity.Z != 0.0f)
            {
                v += Direction * velocity.Z;
            }

            if (velocity.X != 0.0f)
            {
                v += Right * velocity.X;
            }

            if (velocity.Y != 0.0f)
            {
                v += Up * velocity.Y;
            }

            return v;
        }

        public Vector3 CalculateVelocityPerFrame(GameTime gameTime, Vector3 velocity)
        {
            return CalculateVelocity(velocity) * 
                (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Rotate(Vector2 rotationAmount)
        {
            // Scale rotation amount to radians per second
            rotationAmount *= (float)FrameworkCore.ElapsedDeltaTime.TotalSeconds;

            // Create rotation matrix from rotation amount
            rotateMatrix = Matrix.CreateFromAxisAngle(
                            Right,
                            MathHelper.ToRadians(rotationAmount.Y)) * 
                            Matrix.CreateRotationY(
                                MathHelper.ToRadians(rotationAmount.X));

            // Rotate orientation vectors
            Direction = Vector3.TransformNormal(Direction, rotateMatrix);
            Up = Vector3.TransformNormal(Up, rotateMatrix);

            // Re-normalize orientation vectors
            Direction.Normalize();
            Up.Normalize();

            // Re-calculate Right
            Right = Vector3.Cross(Direction, Up);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            Up = Vector3.Cross(Right, Direction);
        }

        public void DumpTrace()
        {
            System.Diagnostics.Debug.WriteLine(WorldTransform.ToString());
        }

        public void SetRootAxis(Matrix rotation)
        {
            RootAxis = rotation;
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public void SetPosition(float x, float y, float z)
        {
            Position = new Vector3(x, y, z);
        }

        public void AddPosition(Vector3 position)
        {
            Position += position;
        }

        public void AddPosition(float x, float y, float z)
        {
            Position += new Vector3(x, y, z);
        }

        public Matrix SetRotationX(float angle)
        {
            Vector3 position = Position;
            WorldTransform = Matrix.CreateRotationX(MathHelper.ToRadians(angle));
            Position = position;

            return WorldTransform;
        }

        public Matrix SetRotationY(float angle)
        {
            Vector3 position = Position;
            WorldTransform = Matrix.CreateRotationY(MathHelper.ToRadians(angle));
            Position = position;

            return WorldTransform;
        }

        public Matrix SetRotationZ(float angle)
        {
            Vector3 position = Position;
            WorldTransform = Matrix.CreateRotationZ(MathHelper.ToRadians(angle));
            Position = position;

            return WorldTransform;
        }

        public Matrix AddRotationX(float angle)
        {
            WorldTransform += Matrix.CreateRotationX(MathHelper.ToRadians(angle));

            return WorldTransform;
        }

        public Matrix AddRotationY(float angle)
        {
            WorldTransform += Matrix.CreateRotationY(MathHelper.ToRadians(angle));

            return WorldTransform;
        }

        public Matrix AddRotationZ(float angle)
        {
            WorldTransform += Matrix.CreateRotationZ(MathHelper.ToRadians(angle));

            return WorldTransform;
        }

        public Matrix SetRotationAxis(Vector3 axis, float angle)
        {
            Vector3 position = Position;

            WorldTransform = 
                        Matrix.CreateFromAxisAngle(axis, MathHelper.ToRadians(angle));

            Position = position;

            return WorldTransform;
        }

        public Matrix AddRotationAxis(Vector3 axis, float angle)
        {
            Vector3 position = Position;

            WorldTransform += 
                        Matrix.CreateFromAxisAngle(axis, MathHelper.ToRadians(angle));

            Position = position;

            return WorldTransform;
        }

        public void SetCollide(CollideElement collide)
        {
            modelCollide = collide;
            modelCollide.Name = Name + "_Collide";
            modelCollide.Owner = (object)this;
        }

        public Vector3 GetMoveAt(Vector3 velocity)
        {
            Vector3 v = Vector3.Zero;

            if (velocity.Z != 0.0f)
            {
                v += Direction * velocity.Z;
            }

            if (velocity.X != 0.0f)
            {
                v += Right * velocity.X;
            }

            if (velocity.Y != 0.0f)
            {
                v += Up * velocity.Y;
            }

            if (v == Vector3.Zero)
                return Vector3.Zero;
            else
                return Vector3.Normalize(v); 
        }
    }
}
