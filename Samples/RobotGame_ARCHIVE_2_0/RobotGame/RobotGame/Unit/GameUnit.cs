#region File Description
//-----------------------------------------------------------------------------
// GameUnit.cs
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData;
using RobotGameData.GameObject;
using RobotGameData.Collision;
using RobotGameData.ParticleSystem;
using RobotGameData.Helper;
using RobotGameData.Render;
using RobotGameData.Resource;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It contains the members that all mech units use and processes the common stuff.
    /// It also contains Weapons function and collision layer as a reference.  
    /// It updates the 3D sound emitter.
    /// It also has the members for the number of the unit’s life (HP) and weapon.
    /// It also defines the virtual functions for the unit’s action.
    /// It provides a basic hit test interface.
    /// </summary>
    public class GameUnit : GameAnimateModel
    {
        #region Fields

        int life = 0;           //  Life value
        int maxLife = 0;        //  Maximum life value

        GameWeapon refCurrentWeapon = null;
        int currentWeaponSlot = -1;

        AudioEmitter emitter = new AudioEmitter();

        GameQuad simpleShadow = null;

        protected Matrix spawnPoint = Matrix.Identity;
        protected List<GameWeapon> weaponList = new List<GameWeapon>();

        protected CollisionLayer colLayerMoveWorld = null;
        protected CollisionLayer colLayerHitWorld = null;
        protected CollisionLayer colLayerFriendlyMech = null;
        protected CollisionLayer colLayerEnemyMech = null;
        protected CollisionLayer colLayerAllMech = null;
        protected CollisionLayer colLayerItems = null;
        protected CollisionLayer[] colLayerVersusTeam = null;

        #endregion

        #region Properties

        public int Life
        {
            get { return this.life; }
            set { this.life = value; }
        }

        public int MaxLife
        {
            get { return this.maxLife; }
            set { this.maxLife = value; }
        }

        public Matrix SpawnPoint
        {
            get { return spawnPoint; }
            set { spawnPoint = value; }
        }

        public AudioEmitter Emitter
        {
            get { return emitter; }
        }

        public bool IsFullLife
        {
            get { return (this.Life == this.MaxLife); }
        }

        public bool IsDead
        {
            get { return (Life <= 0); }
        }

        public bool IsFiring
        {
            get { return (this.CurrentWeapon.State == GameWeapon.WeaponState.Firing); }
        }

        public bool IsReloading
        {
            get
            { 
                return (this.CurrentWeapon.State == GameWeapon.WeaponState.Reloading); 
            }
        }

        public bool IsPossibleWeaponChange
        {
            get
            {
                return ((weaponList.Count > 1) &&
                       (this.CurrentWeapon.State == GameWeapon.WeaponState.Ready));
            }
        }

        public int CurrentWeaponSlot
        {
            get { return currentWeaponSlot; }
        }

        public int WeaponCount
        {
            get { return weaponList.Count; }
        }

        public GameWeapon CurrentWeapon
        {
            get { return this.refCurrentWeapon; }
        }

        public GameWeapon DefaultWeapon
        {
            get { return this.weaponList[0]; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameUnit(string filename)
            : base(filename) {}

        protected override void UnloadContent()
        {
            weaponList.Clear();

            base.UnloadContent();
        }

        /// <summary>
        /// initializes with collison layer and creates a simple shadow.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();       
     
            colLayerMoveWorld =
                        RobotGameGame.CurrentGameLevel.CollisionLayerMoveWorld;

            colLayerHitWorld =
                        RobotGameGame.CurrentGameLevel.CollisionLayerHitWorld;

            colLayerFriendlyMech =
                        RobotGameGame.CurrentGameLevel.CollisionLayerFriendlyMech;

            colLayerEnemyMech =
                        RobotGameGame.CurrentGameLevel.CollisionLayerEnemyMech;

            colLayerAllMech =
                        RobotGameGame.CurrentGameLevel.CollisionLayerAllMech;

            colLayerItems =
                        RobotGameGame.CurrentGameLevel.CollisionLayerItems;

            colLayerVersusTeam = RobotGameGame.CurrentGameLevel.CollisionVersusTeam;

            //  creates a simple shadow.
            GameResourceTexture2D resource =
                    FrameworkCore.ResourceManager.LoadTexture("Textures/shadow");

            simpleShadow = new GameQuad(new Vector3(0.0f, 0.02f, 0.0f),
                        Vector3.Up, Vector3.Forward, 3.0f, 3.0f);

            simpleShadow.Name = "simple shadow";
            simpleShadow.Texture = resource.Texture2D;
            simpleShadow.LightingEnabled = false;
            simpleShadow.Alpha = 0.6f;
            simpleShadow.AlphaBlendEnable = true;
            simpleShadow.ReferenceAlpha = 0;
            simpleShadow.DepthBufferEnable = true;
            simpleShadow.DepthBufferWriteEnable = true;
            simpleShadow.DepthBufferFunction = CompareFunction.Less;
            simpleShadow.SourceBlend = Blend.SourceAlpha;
            simpleShadow.DestinationBlend = Blend.InverseSourceAlpha;
            simpleShadow.BlendFunction = BlendFunction.Add;
            simpleShadow.AlphaFunction = CompareFunction.Greater;
            simpleShadow.CullMode = CullMode.CullCounterClockwiseFace;

            this.AddChild(simpleShadow);
        }

        /// <summary>
        /// reset all members.
        /// starts at the specified spawn position.
        /// </summary>
        protected override void OnReset()
        {
            this.WorldTransform = this.SpawnPoint;

            this.SourceBlend = Blend.One;
            this.DestinationBlend = Blend.Zero;
            this.AlphaBlendEnable = false;
            this.ReferenceAlpha = 0;

            base.OnReset();
        }

        #endregion

        /// <summary>
        /// updates the emitter and the simple shadow.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void OnUpdate(GameTime gameTime)
        {
            emitter.Forward = Direction;
            emitter.Position = Position;
            emitter.Up = Up;
            emitter.Velocity = Velocity;

            //  This shadow follows to this.
            simpleShadow.Position = new Vector3(this.Position.X, 0.0f, this.Position.Z);

            base.OnUpdate(gameTime);
        }

        #region Weapon

        /// <summary>
        /// creates a weapon by spec file.
        /// </summary>
        /// <param name="specFileName">weapon information file (.spec)</param>
        public void CreateWeapon(string specFileName)
        {
            GameWeaponSpec spec = new GameWeaponSpec();
            spec = 
                (GameWeaponSpec)GameDataSpecManager.Load(specFileName, spec.GetType());

            GameWeapon createWeapon = new GameWeapon(spec);
            createWeapon.AttachOwner(this);

            weaponList.Add(createWeapon);

            SelectWeapon(0);
        }

        /// <summary>
        /// changes a weapon.
        /// </summary>
        /// <param name="slot">weapon slot number</param>
        public void SelectWeapon(int slot)
        {
            this.refCurrentWeapon = weaponList[slot];
            this.currentWeaponSlot = slot;

            for (int i = 0; i < this.weaponList.Count; i++)
            {
                if (i == slot)
                {
                    weaponList[i].Visible = true;
                    weaponList[i].Enabled = true;
                }
                else
                {
                    weaponList[i].Visible = false;
                    weaponList[i].Enabled = false;
                }
            }
        }

        public GameWeapon GetWeapon(int slot)
        {
            return weaponList[slot];
        }

        public virtual void WeaponFire() { }

        #endregion

        #region Action

        public virtual void ActionIdle() { }

        public virtual bool ActionFire() { return false; }

        public virtual bool ActionMelee() { return false; }

        public virtual void ActionHit(GameUnit attacker) { }

        public virtual bool ActionReload(GameWeapon weapon) { return false; }

        public virtual void ActionDamage(GameUnit attacker) { }

        public virtual void ActionDead(Vector3 attackerPosition) { }

        #endregion

        #region Collision detect

        /// <summary>
        /// checks the collision test, when unit is moving, 
        /// against and in order of the other units, world, item.  
        /// When there is a collision, returns a result report.
        /// </summary>
        /// <param name="vVelocityAmount"></param>
        /// <returns></returns>
        public CollisionResult MoveHitTest(Vector3 velocityAmount)
        {
            //  first, test with world
            CollisionResult result = MoveHitTestWithWorld(velocityAmount);
            if (result != null)
                return result;

            //  second, test with other units
            result = MoveHitTestWithMech(velocityAmount);
            if (result != null)
                return result;

            //  third, test with items
            result = MoveHitTestWithItem(velocityAmount);
            if (result != null)
                return result;

            return null;
        }

        /// <summary>
        /// checks for collision test again other units when the unit is moving.        
        /// </summary>
        public CollisionResult MoveHitTestWithMech(Vector3 velocityAmount)
        {
            CollideSphere playerSphere = Collide as CollideSphere;
            float radius = playerSphere.Radius;

            //  creates simulation sphere for collision.
            Matrix transformSimulate = TransformedMatrix *
                                       Matrix.CreateTranslation(velocityAmount);

            CollideSphere sphereSimulate = 
                            new CollideSphere(playerSphere.LocalCenter, radius);

            sphereSimulate.Transform(transformSimulate);
            sphereSimulate.Id = playerSphere.Id;

            //  checks for the collision with other mech.
            CollisionResult result = FrameworkCore.CollisionContext.HitTest(
                                                            sphereSimulate, 
                                                            ref colLayerAllMech, 
                                                            ResultType.NearestOne);

            if (result != null)
            {
                if (0.0f >= result.distance)
                {
                    return result;
                }
            }

            return null;
        }

        public CollisionResult MoveHitTestWithItem(Vector3 velocityAmount)
        {
            CollideSphere playerSphere = Collide as CollideSphere;
            float radius = playerSphere.Radius;

            //  creates simulation sphere for collision.
            Matrix transformSimulate = TransformedMatrix *
                                       Matrix.CreateTranslation(velocityAmount);

            CollideSphere sphereSimulate =
                            new CollideSphere(playerSphere.LocalCenter, radius);

            sphereSimulate.Transform(transformSimulate);
            sphereSimulate.Id = playerSphere.Id;

            //  checks for the collision with items.
            CollisionResult result = FrameworkCore.CollisionContext.HitTest(
                                                            sphereSimulate,
                                                            ref colLayerItems,
                                                            ResultType.NearestOne);

            if (result != null)
            {
                if (0.0f >= result.distance)
                {
                    return result;
                }
            }

            return null;
        }

        public CollisionResult MoveHitTestWithWorld(Vector3 velocityAmount)
        {
            CollideSphere playerSphere = Collide as CollideSphere;
            float radius = playerSphere.Radius;

            //  calculate simulated position for collision.
            Matrix transformSimulate = TransformedMatrix *
                                       Matrix.CreateTranslation(velocityAmount);

            // first, check using sphere.
            {
                CollideSphere simulateSphere =
                                new CollideSphere(playerSphere.LocalCenter, radius);

                simulateSphere.Transform(transformSimulate);
                simulateSphere.Id = playerSphere.Id;

                CollisionResult result = FrameworkCore.CollisionContext.HitTest(
                                                                simulateSphere,
                                                                ref colLayerMoveWorld,
                                                                ResultType.NearestOne);

                if (result != null)
                {
                    return result;
                }
            }

            //  second, check using ray.
            {
                Vector3 direction = velocityAmount;
                direction.Normalize();

                CollideRay simulateRay =
                        new CollideRay(playerSphere.BoundingSphere.Center, direction);

                simulateRay.Id = playerSphere.Id;

                CollisionResult result = FrameworkCore.CollisionContext.HitTest(
                                                                simulateRay,
                                                                ref colLayerMoveWorld,
                                                                ResultType.NearestOne);

                if (result != null)
                {
                    if (result.distance <= radius)
                        return result;
                }
            }

            return null;
        }

        /// <summary>
        /// checks the collision test, when unit is moving, 
        /// against and in order of the other units, world, item.  
        /// When there is a collision, returns a result report.
        /// calculates the movement range per single frame and tests for collision.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="vMoveVelocity"></param>
        /// <returns></returns>
        public CollisionResult MoveHitTest(GameTime gameTime, Vector3 moveVelocity)
        {
            Vector3 velocitySimulate =
                CalculateVelocityPerFrame(gameTime, moveVelocity);

            //  checks for the collision.
            CollisionResult result = MoveHitTest(velocitySimulate);
            if (result != null)
            {
                return result;
            }

            return null;
        }

        public CollisionResult HitTestWithWorld(Vector3 start, Vector3 direction)
        {
            CollideRay simulateRay = new CollideRay(start, direction);
            simulateRay.Id = Collide.Id;

            //  Test collision
            CollisionResult result =
                    FrameworkCore.CollisionContext.HitTest(simulateRay,
                                                            ref colLayerMoveWorld,
                                                            ResultType.NearestOne);
            return result;
        }

        #endregion

    }
}
