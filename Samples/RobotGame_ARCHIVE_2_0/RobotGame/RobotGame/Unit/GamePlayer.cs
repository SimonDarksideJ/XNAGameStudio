#region File Description
//-----------------------------------------------------------------------------
// GamePlayer.cs
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using RobotGameData;
using RobotGameData.Input;
using RobotGameData.Collision;
using RobotGameData.ParticleSystem;
using RobotGameData.GameObject;
using RobotGameData.Camera;
using RobotGameData.Helper;
using RobotGameData.Render;
using RobotGameData.Sound;
#endregion

namespace RobotGame
{
    /// <summary>
    /// This class is one of the most important classes in RobotGame and 
    /// does all functions of the player robot.
    /// After receiving the key inputs from user, it processes the functions 
    /// that are related to the player robot’s movement, 
    /// actions, animations, and weapons.
    /// All the actions for the player robot are defined within this class.  
    /// This class also provides an interface for actions.
    /// Among its members, DebugMode can change the play mode and 
    /// it’s useful for debugging and testing. (Default mode is “None”)
    /// It contains GamePlayerSpec, which is the information class of the player unit.
    /// </summary>
    public class GamePlayer : GameUnit
    {
        #region Action Define

        /// <summary>
        /// player's lower action
        /// </summary>
        public enum LowerAction
        {
            Unknown = 0,
            Idle,
            Run,
            Damage,
            Walk,
            BackwardWalk,
            LeftTurn,
            RightTurn,

            ForwardDead,
            BackwardDead,
            LeftDead,
            RightDead,

            BoosterPrepare,
            BoosterActive,
            BoosterFinish,            
            BoosterBreak,
            BoosterLeftTurn,
            BoosterRightTurn,

            Count
        }

        /// <summary>
        /// player's upper action
        /// </summary>
        public enum UpperAction
        {
            Unknown = 0,
            Idle,
            Run,
            Damage,
            WeaponChange,

            ForwardNonFire,
            LeftNonFire,
            RightNonFire,

            ForwardMachineGunFire,
            LeftMachineGunFire,
            RightMachineGunFire,
            ReloadMachineGun,

            ForwardShotgunFire,
            LeftShotgunFire,
            RightShotgunFire,
            ReloadShotgun,

            ForwardHandgunFire,
            LeftHandgunFire,
            RightHandgunFire,
            ReloadHandgun,

            ForwardDead,
            BackwardDead,
            LeftDead,
            RightDead,

            BoosterPrepare,
            BoosterActive,
            BoosterFinish,            
            BoosterBreak,
            BoosterLeftTurn,
            BoosterRightTurn,

            Count
        }
        #endregion

        public enum DebugMode
        {
            /// <summary>
            /// normal play
            /// </summary>
            None = 0, 

            /// <summary>
            /// player never dies.
            /// </summary>
            NeverDie,   

            /// <summary>
            /// destroy an enemy with one shot.
            /// </summary>
            Superman,    

            /// <summary>
            /// player never dies and destroys an enemy with one shot.
            /// </summary>
            God,         

            Count
        }

        #region Fields

        //  The player information
        GamePlayerSpec specData = null;

        PlayerIndex playerIndex = PlayerIndex.One;

        GameInput gameInput = null;

        //  Debug play
        DebugMode debugMode = DebugMode.None;

        bool enableHandleInput = true;
        bool tryEmptyWeeapon = false;
        bool isActiveBooster = false;
        bool isDelayBooster = false;
        int killPoint = 0;

        //  The current action
        LowerAction currentLowerAction = LowerAction.Unknown;
        UpperAction currentUpperAction = UpperAction.Unknown;
        
        LowerAction prepareLowerAction = LowerAction.Unknown;
        UpperAction prepareUpperAction = UpperAction.Unknown;
        bool isOverwriteLowerAction = false;
        bool isOverwriteUpperAction = false;

        //  The animation indices
        int[] indexLowerAnimation = null;
        int[] indexUpperAnimation = null;

        //  Dummy bone indices
        int indexLeftHandWeaponDummy = -1;
        int indexRightHandWeaponDummy = -1;
        int indexLeftBoosterDummy = -1;
        int indexRightBoosterDummy = -1;
        int indexLeftFootDummy = -1;
        int indexRightFootDummy = -1;

        float defaultAnimationScaleFactor = 1.0f;
        float actionElapsedTime = 0.0f;
        float boosterElapsedTime = 0.0f;
        float boosterFinishDurationTime = 0.0f;
        float boosterBreakDurationTime = 0.0f;
        float boosterWaveEffectTime = 0.0f;
        float runDurationTime = 0.0f;
        float walkDurationTime = 0.0f;
        float moveElapsedTime = 0.0f;
        float weaponChangeDurationTime = 0.0f;
        float pickupElapsedTime = 0.0f;

        float rootRotationAngle = 0.0f;
        float rootElapsedAngle = 0.0f;

        GameWeapon possiblePickupWeapon = null;

        ModelBone boneWaist = null;
        Matrix matrixWaistBoneSource = Matrix.Identity;
        Vector3 moveDirection = Vector3.Zero;

        //  Sound cue
        Cue soundFireEmpty = null;
        Cue soundRun = null;
        Cue soundWalk = null;
        Cue soundDestroy = null;
        Cue soundBooster = null;
        Cue soundBoosterPrepare = null;

        //  The booster particles (left & right)
        ParticleSequence[] particleBoosterOn = { null, null};
        ParticleSequence[] particleBoosterPrepare = { null, null};
        ParticleSequence[] particleBoosterGround = { null, null };

        #endregion        
                
        #region Properties

        public GamePlayerSpec SpecData
        {
            get { return this.specData; }
        }

        public PlayerIndex PlayerIndex
        {
            get { return this.playerIndex; }
            set { this.playerIndex = value; }
        }

        public UnitClassId UnitClass
        {
            get { return specData.UnitClass; }
        }

        public UnitTypeId UnitType
        {
            get { return specData.UnitType; }
        }

        public DebugMode Mode
        {
            get { return this.debugMode; }
        }

        public float RunSpeed
        {
            get { return this.specData.RunSpeed; }
        }

        public float WalkSpeed
        {
            get { return this.specData.WalkSpeed; }
        }

        public float WalkBackwardSpeed
        {
            get { return this.specData.WalkBackwardSpeed; }
        }

        public float TurnAngle
        {
            get { return this.specData.TurnAngle; }
        }

        public float BoosterTurnAngle
        {
            get { return this.specData.BoosterTurnAngle; }
        }

        public LowerAction CurrentLowerAction
        {
            get { return this.currentLowerAction; }
        }

        public UpperAction CurrentUpperAction
        {
            get { return this.currentUpperAction; }
        }

        public bool EnableHandleInput
        {
            get { return this.enableHandleInput; }
            set { this.enableHandleInput = value; }
        }

        public bool IsTryEmptyWeapon
        {
            get { return this.tryEmptyWeeapon; }
        }

        public bool IsFinishedDead
        {
            get { return (IsDead && this.actionElapsedTime == 0.0f); }
        }

        public bool IsCriticalDamaged
        {
            get
            {
                return (this.CurrentUpperAction == UpperAction.Damage ||
                        this.prepareUpperAction == UpperAction.Damage);
            }
        }

        public bool IsWeaponChanging
        {
            get
            {
                return (this.CurrentUpperAction == UpperAction.WeaponChange ||
                        this.prepareUpperAction == UpperAction.WeaponChange);
            }
        }
        
        public float BoosterCoolTimeRate
        {
            get 
            {
                return (this.boosterElapsedTime / this.SpecData.BoosterActiveTime);
            }
        }

        public float BoosterWaveEffectTime
        {
            get { return this.boosterWaveEffectTime; }
        }

        public float PickupCoolTimeRate
        {
            get { return (this.pickupElapsedTime / 1.0f); }
        }

        public bool IsReadyToUseBooster
        {
            get { return (BoosterCoolTimeRate == 1.0f); }
        }

        public bool IsActiveBooster
        {
            get { return this.isActiveBooster; }
        }

        public bool IsDelayBooster
        {
            get { return this.isDelayBooster; }
        }

        public int KillPoint
        {
            get { return this.killPoint; }
            set { this.killPoint = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public GamePlayer(ref GamePlayerSpec spec, PlayerIndex index)
            : base(spec.ModelFilePath)
        {
            this.specData = spec;

            if (this.specData == null)
                throw new ArgumentException("Cannot read player spec file.");
            
            Name = this.specData.UnitType.ToString();
            Life = this.specData.Life;
            MaxLife = this.SpecData.Life;

            this.PlayerIndex = index;

            //  Set game input
            InputComponent input =
                        FrameworkCore.InputManager.GetInputComponent(this.PlayerIndex);

            this.gameInput = new GameInput(input);
            this.gameInput.SetDefaultKey(this.PlayerIndex);

            //  Load Animations
            LoadAnimationData(this.specData.AnimationFolderPath);

            //  Load a default waepon
            CreateWeapon(this.specData.DefaultWeaponFilePath);

            //  Find index of dummy bones
            for (int i = 0; i < ModelData.model.Bones.Count; i++)
            {
                ModelBone bone = ModelData.model.Bones[i];

                if (bone.Name == "HeroPointDummyLeft")
                    this.indexLeftHandWeaponDummy = bone.Index;
                else if (bone.Name == "HeroPointDummyRight")
                    this.indexRightHandWeaponDummy = bone.Index;
                else if (bone.Name == "BoosterDummyLeft")
                    this.indexLeftBoosterDummy = bone.Index;
                else if (bone.Name == "BoosterDummyRight")
                    this.indexRightBoosterDummy = bone.Index;
                else if (bone.Name == "BoosterDummy")
                    this.indexLeftBoosterDummy = bone.Index;
                else if (bone.Name == "L Foot")
                    this.indexLeftFootDummy = bone.Index;
                else if (bone.Name == "R Foot")
                    this.indexRightFootDummy = bone.Index;
            }

            //  The normal map and the specular map effects are applied to “Grund” and 
            //  “Mark” only.
            if (this.UnitType == UnitTypeId.Grund ||
                this.UnitType == UnitTypeId.Mark)
            {
                RenderingCustomEffect += 
                    new EventHandler<RenderingCustomEffectEventArgs>(OnEffectProcess);
            }
        }

        /// <summary>
        /// initializes the model and action.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            this.boneWaist = ModelData.model.Bones["Spine"];

            //  Gets a waist's matrix
            this.matrixWaistBoneSource = 
                this.ModelData.boneTransforms[this.boneWaist.Index];

            Reset(false);            
        }

        /// <summary>
        /// reset all members.
        /// restarts at the specified spawn position.
        /// </summary>
        protected override void OnReset()
        {
            base.OnReset();

            Life = this.specData.Life;
            
            this.isOverwriteLowerAction = false;
            this.isOverwriteUpperAction = false;
            this.enableHandleInput = true;
            this.tryEmptyWeeapon = false;
            this.isDelayBooster = false;
            this.isActiveBooster = false;
            this.actionElapsedTime = 0.0f;
            this.boosterWaveEffectTime = 0.0f;
            this.moveElapsedTime = 0.0f;
            this.pickupElapsedTime = 0.0f;

            boosterElapsedTime = this.SpecData.BoosterActiveTime;

            Material.alpha = 1.0f;

            //  Default Action
            PlayLowerAction(LowerAction.Idle);
            PlayUpperAction(UpperAction.Idle);

            //  Reset the weapon
            CurrentWeapon.Reset();

            //  Add collision
            if (RobotGameGame.CurrentStage is VersusStageScreen)
            {
                if (!colLayerVersusTeam[(int)this.PlayerIndex].IsContain(Collide))
                    colLayerVersusTeam[(int)this.PlayerIndex].AddCollide(Collide);
            }
            else
            {
                if (!colLayerFriendlyMech.IsContain(Collide))
                    colLayerFriendlyMech.AddCollide(Collide);
            }

            this.Enabled = true;
            this.Visible = true;

            this.CurrentWeapon.Enabled = true;
            this.CurrentWeapon.Visible = true;
        }

        #endregion

        #region Update & Draw

        /// <summary>
        /// processes the animation and action info.
        /// turns the model’s waist depending on the current action 
        /// and movement direction.
        /// </summary>
        protected override void OnUpdate(GameTime gameTime)
        {
            //  Play the low animation
            if (this.prepareLowerAction != LowerAction.Unknown ||
                this.isOverwriteLowerAction)
            {
                PlayLowerAction(this.prepareLowerAction, 0.0f);
            }

            //  Play the upper animation
            if (this.prepareUpperAction != UpperAction.Unknown ||
                this.isOverwriteUpperAction)
            {
                PlayUpperAction(this.prepareUpperAction, 0.0f);
            }

            ProcessAction(gameTime);

            //  Update animation and transform
            base.OnUpdate(gameTime);

            //  body turnning left 
            if (this.rootRotationAngle > 0.0f)
            {
                if (this.rootRotationAngle > this.rootElapsedAngle)
                {
                    this.rootElapsedAngle = MathHelper.Clamp(
                                                    this.rootElapsedAngle + 3.0f,
                                                    0.0f,
                                                    this.rootRotationAngle);
                }
            }
            //  body turnning right
            else if (this.rootRotationAngle < 0.0f)
            {
                if (this.rootRotationAngle < this.rootElapsedAngle)
                {
                    this.rootElapsedAngle = MathHelper.Clamp(
                                                    this.rootElapsedAngle - 3.0f,
                                                    this.rootRotationAngle,
                                                    0.0f);
                }
            }
            //  body returnning forward
            else
            {
                if (this.rootElapsedAngle > 0.0f)
                {
                    this.rootElapsedAngle = MathHelper.Clamp(
                                                    this.rootElapsedAngle - 4.5f,
                                                    0.0f,
                                                    this.rootElapsedAngle);
                }
                else if (this.rootElapsedAngle < 0.0f)
                {
                    this.rootElapsedAngle = MathHelper.Clamp(
                                                    this.rootElapsedAngle + 4.5f,
                                                    this.rootElapsedAngle,
                                                    0.0f);
                }
            }

            //  Rotates the waist.
            float currentWaistAngle = 0.0f;
            if (this.CurrentUpperAction == UpperAction.ForwardMachineGunFire ||
                this.CurrentUpperAction == UpperAction.ForwardShotgunFire ||
                this.CurrentUpperAction == UpperAction.ForwardHandgunFire ||
                this.CurrentUpperAction == UpperAction.ForwardNonFire ||
                this.CurrentUpperAction == UpperAction.LeftMachineGunFire ||
                this.CurrentUpperAction == UpperAction.LeftShotgunFire ||
                this.CurrentUpperAction == UpperAction.LeftHandgunFire ||
                this.CurrentUpperAction == UpperAction.LeftNonFire ||
                this.CurrentUpperAction == UpperAction.RightMachineGunFire ||
                this.CurrentUpperAction == UpperAction.RightShotgunFire ||
                this.CurrentUpperAction == UpperAction.RightHandgunFire ||
                this.CurrentUpperAction == UpperAction.RightNonFire)
            {
                currentWaistAngle = -(this.rootElapsedAngle);
            }

            if (this.CurrentLowerAction == LowerAction.Idle ||
                this.CurrentLowerAction == LowerAction.Damage ||
                this.CurrentLowerAction == LowerAction.ForwardDead ||
                this.CurrentLowerAction == LowerAction.BackwardDead ||
                this.CurrentLowerAction == LowerAction.LeftDead ||
                this.CurrentLowerAction == LowerAction.RightDead)
            {
                SetRootAxis(Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)));
                this.boneWaist.Transform = this.matrixWaistBoneSource;

                this.rootRotationAngle = 0.0f;
                this.rootElapsedAngle = 0.0f;

                ModelData.model.CopyAbsoluteBoneTransformsTo(BoneTransforms);
            }
            else
            {
                if (currentWaistAngle != 0.0f || this.rootElapsedAngle != 0.0f)
                {                    
                    SetRootAxis(Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)) *
                                Matrix.CreateRotationY(
                                MathHelper.ToRadians(this.rootElapsedAngle)));

                    this.boneWaist.Transform *= 
                        Matrix.CreateFromAxisAngle(boneWaist.Transform.Right,
                        MathHelper.ToRadians(currentWaistAngle));

                    ModelData.model.CopyAbsoluteBoneTransformsTo(BoneTransforms);
                }
            }

            // Charge the booster power
            if( this.isActiveBooster == false)
            {
                if (boosterElapsedTime < this.SpecData.BoosterActiveTime)
                {
                    float rate = this.specData.BoosterActiveTime / 
                                 this.specData.BoosterCoolTime;

                    boosterElapsedTime += 
                                (float)gameTime.ElapsedGameTime.TotalSeconds * rate;
                }
                else
                {
                    boosterElapsedTime = this.SpecData.BoosterActiveTime;
                }
            }

            //  Transform the booster particles
            if (isActiveBooster)
            {
                int index = -1;
                Matrix fixedAxis = Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f));

                for( int i = 0; i<2; i++)
                {
                    if (particleBoosterOn[i] != null)
                    {
                        if( i == 0)
                            index = this.indexLeftBoosterDummy;
                        else
                            index = this.indexRightBoosterDummy;

                        particleBoosterOn[i].SetTransform(
                                            fixedAxis * BoneTransforms[index]);
                    }

                    if (particleBoosterPrepare[i] != null)
                    {
                        if( i == 0)
                            index = this.indexLeftBoosterDummy;
                        else
                            index = this.indexRightBoosterDummy;

                        particleBoosterPrepare[i].SetTransform(
                                            fixedAxis * BoneTransforms[index]);
                    }

                    if (particleBoosterGround[i] != null)
                    {
                        if (i == 0)
                            index = this.indexLeftFootDummy;
                        else
                            index = this.indexRightFootDummy;

                        particleBoosterGround[i].SetTransform(
                                            fixedAxis * BoneTransforms[index]);
                    }
                }

                //  Booster wave effect
                if (!isDelayBooster)
                {
                    if (this.boosterWaveEffectTime > 1.0f)
                    {
                        this.boosterWaveEffectTime = 0.0f;
                    }
                    else
                    {
                        this.boosterWaveEffectTime += 
                                    ((float)gameTime.ElapsedGameTime.TotalSeconds);
                    }
                }
                else
                {
                    this.boosterWaveEffectTime = 0.0f;
                }
            }

            //  Transform the current attached weapons
            if (CurrentWeapon != null)
            {
                CurrentWeapon.modelWeapon[0].WorldTransform = 
                                        BoneTransforms[indexLeftHandWeaponDummy];

                CurrentWeapon.modelWeapon[1].WorldTransform = 
                                        BoneTransforms[indexRightHandWeaponDummy];
            }
        }

        /// <summary>
        /// set all effect parameters and process the model's effect.
        /// </summary>
        void OnEffectProcess(object sender, GameModel.RenderingCustomEffectEventArgs e) 
        {
            RenderLighting lighting = e.RenderTracer.Lighting;

            e.Effect.Parameters["LightColor"].SetValue(
                                            lighting.diffuseColor.ToVector4());

            e.Effect.Parameters["AmbientLightColor"].SetValue(
                                            lighting.ambientColor.ToVector4());

            e.Effect.Parameters["Shininess"].SetValue(1.0f);

            e.Effect.Parameters["SpecularPower"].SetValue(12.0f);

            e.Effect.Parameters["EnvironmentMap"].SetValue(
                RobotGameGame.CurrentGameLevel.GameWorld.TextureCubeMap);

            e.Effect.Parameters["World"].SetValue(e.World);

            e.Effect.Parameters["View"].SetValue(e.RenderTracer.View);

            e.Effect.Parameters["Projection"].SetValue(e.RenderTracer.Projection);

            e.Effect.Parameters["LightPosition"].SetValue(
                                    Vector3.Negate(lighting.direction) * 1000);
        }

        #endregion

        #region Load Data

        /// <summary>
        /// load all animations for action.
        /// </summary>
        /// <param name="path">animation folder without file name</param>
        public void LoadAnimationData(string path)
        {
            int lowerAnimCount = (int)LowerAction.Count;
            int upperAnimCount = (int)UpperAction.Count;

            indexLowerAnimation = new int[lowerAnimCount];
            indexUpperAnimation = new int[upperAnimCount];

            //  Load the lower animation
            {
                indexLowerAnimation[(int)LowerAction.Idle] =
                                    AddAnimation(path + "Low_Idle");

                indexLowerAnimation[(int)LowerAction.Run] =
                                    AddAnimation(path + "Low_Run");

                indexLowerAnimation[(int)LowerAction.Damage] =
                                    AddAnimation(path + "Low_Damage");

                indexLowerAnimation[(int)LowerAction.Walk] =
                                    AddAnimation(path + "Low_Walk");

                indexLowerAnimation[(int)LowerAction.LeftTurn] =    
                                    AddAnimation(path + "Low_TurnLeft");

                indexLowerAnimation[(int)LowerAction.RightTurn] =
                                    AddAnimation(path + "Low_TurnRight");

                indexLowerAnimation[(int)LowerAction.BackwardWalk] =
                                    AddAnimation(path + "Low_WalkBack");

                //  Dead lower animation
                indexLowerAnimation[(int)LowerAction.ForwardDead] =
                                    AddAnimation(path + "Low_DeathFront");

                indexLowerAnimation[(int)LowerAction.BackwardDead] =
                                    AddAnimation(path + "Low_DeathBack");

                indexLowerAnimation[(int)LowerAction.LeftDead] =
                                    AddAnimation(path + "Low_DeathLeft");

                indexLowerAnimation[(int)LowerAction.RightDead] =
                                    AddAnimation(path + "Low_DeathRight");

                //  Booster lower animation
                indexLowerAnimation[(int)LowerAction.BoosterPrepare] =
                                    AddAnimation(path + "Low_BoosterStart");

                indexLowerAnimation[(int)LowerAction.BoosterActive] =
                                    AddAnimation(path + "Low_BoosterMove");

                indexLowerAnimation[(int)LowerAction.BoosterFinish] =
                                    AddAnimation(path + "Low_BoosterEnd");

                indexLowerAnimation[(int)LowerAction.BoosterBreak] =
                                    AddAnimation(path + "Low_BoosterMiss");

                indexLowerAnimation[(int)LowerAction.BoosterLeftTurn] =
                                    AddAnimation(path + "Low_BoosterTurnLeft");

                indexLowerAnimation[(int)LowerAction.BoosterRightTurn] =
                                    AddAnimation(path + "Low_BoosterTurnRight");
            }

            //  Load the upper animation
            {
                indexUpperAnimation[(int)UpperAction.Idle] =
                                    AddAnimation(path + "Up_Idle");

                indexUpperAnimation[(int)UpperAction.Run] =
                                    AddAnimation(path + "Up_Run");

                indexUpperAnimation[(int)UpperAction.Damage] =
                                    AddAnimation(path + "Up_Damage");

                indexUpperAnimation[(int)UpperAction.WeaponChange] =
                                    AddAnimation(path + "Up_Change");


                //  Load empty weapon fire upper animation
                indexUpperAnimation[(int)UpperAction.ForwardNonFire] =
                                    AddAnimation(path + "Up_NonAttackFront");

                indexUpperAnimation[(int)UpperAction.LeftNonFire] =
                                    AddAnimation(path + "Up_NonAttackLeft");

                indexUpperAnimation[(int)UpperAction.RightNonFire] =
                                    AddAnimation(path + "Up_NonAttackRight");


                //  MachineGun fire upper animation
                indexUpperAnimation[(int)UpperAction.ForwardMachineGunFire] =
                                    AddAnimation(path + "Up_AttackFrontBase");

                indexUpperAnimation[(int)UpperAction.LeftMachineGunFire] =
                                    AddAnimation(path + "Up_AttackLeftBase");

                indexUpperAnimation[(int)UpperAction.RightMachineGunFire] =
                                    AddAnimation(path + "Up_AttackRightBase");

                indexUpperAnimation[(int)UpperAction.ReloadMachineGun] =
                                    AddAnimation(path + "Up_ReloadBase");

                //  Shotgun fire upper animation
                indexUpperAnimation[(int)UpperAction.ForwardShotgunFire] =
                                    AddAnimation(path + "Up_AttackFrontShotgun");

                indexUpperAnimation[(int)UpperAction.LeftShotgunFire] =
                                    AddAnimation(path + "Up_AttackLeftShotgun");

                indexUpperAnimation[(int)UpperAction.RightShotgunFire] =
                                    AddAnimation(path + "Up_AttackRightShotgun");

                indexUpperAnimation[(int)UpperAction.ReloadShotgun] =
                                    AddAnimation(path + "Up_ReloadShotgun");

                //  Handgun fire upper animation
                indexUpperAnimation[(int)UpperAction.ForwardHandgunFire] =
                                    AddAnimation(path + "Up_AttackFrontHandgun");

                indexUpperAnimation[(int)UpperAction.LeftHandgunFire] =
                                    AddAnimation(path + "Up_AttackLeftHandgun");

                indexUpperAnimation[(int)UpperAction.RightHandgunFire] =
                                    AddAnimation(path + "Up_AttackRightHandgun");

                indexUpperAnimation[(int)UpperAction.ReloadHandgun] =
                                    AddAnimation(path + "Up_ReloadHandgun");

                //  Dead upper animation
                indexUpperAnimation[(int)UpperAction.ForwardDead] =
                                    AddAnimation(path + "Up_DeathFront");

                indexUpperAnimation[(int)UpperAction.BackwardDead] =
                                    AddAnimation(path + "Up_DeathBack");

                indexUpperAnimation[(int)UpperAction.LeftDead] =
                                    AddAnimation(path + "Up_DeathLeft");

                indexUpperAnimation[(int)UpperAction.RightDead] =
                                    AddAnimation(path + "Up_DeathRight");

                //  Booster upper animation
                indexUpperAnimation[(int)UpperAction.BoosterPrepare] =
                                    AddAnimation(path + "Up_BoosterStart");

                indexUpperAnimation[(int)UpperAction.BoosterActive] =
                                    AddAnimation(path + "Up_BoosterMove");

                indexUpperAnimation[(int)UpperAction.BoosterFinish] =
                                    AddAnimation(path + "Up_BoosterEnd");

                indexUpperAnimation[(int)UpperAction.BoosterBreak] =
                                    AddAnimation(path + "Up_BoosterMiss");

                indexUpperAnimation[(int)UpperAction.BoosterLeftTurn] =
                                    AddAnimation(path + "Up_BoosterTurnLeft");

                indexUpperAnimation[(int)UpperAction.BoosterRightTurn] =
                                    AddAnimation(path + "Up_BoosterTurnRight");
            }

            //  Animation duration
            this.runDurationTime =
                GetAnimation(indexLowerAnimation[(int)LowerAction.Run]).Duration;

            this.walkDurationTime =
                GetAnimation(indexLowerAnimation[(int)LowerAction.Walk]).Duration;

            this.boosterFinishDurationTime =
                GetAnimation(indexLowerAnimation[
                    (int)LowerAction.BoosterFinish]).Duration;

            this.boosterBreakDurationTime =
                GetAnimation(indexLowerAnimation[
                    (int)LowerAction.BoosterBreak]).Duration;

            this.weaponChangeDurationTime =
                GetAnimation(indexUpperAnimation[
                    (int)UpperAction.WeaponChange]).Duration;
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// processes the user’s input.
        /// According to the input key, a weapon gets fired or 
        /// the player robot get moved.
        /// </summary>
        public void HandleInput(GameTime gameTime)
        {
            if( this.prepareLowerAction == LowerAction.Unknown)
                this.isOverwriteLowerAction = false;

            if (this.prepareUpperAction == UpperAction.Unknown)
                this.isOverwriteUpperAction = false;

            bool enableControl = (EnableHandleInput &&
                (FrameworkCore.CurrentCamera.FirstCamera is FollowCamera));

            //////////////////////////  Use booster
            if (this.gameInput.IsStrokeKey(GameKey.Booster) &&
                !IsDead && !IsCriticalDamaged && !isDelayBooster && 
                !IsReloading && !IsWeaponChanging && enableControl)
            {
                if (isActiveBooster == false && IsReadyToUseBooster)
                    ActionBooster();            //  Booster on
                else if (isActiveBooster)
                    ActionBoosterFinish();      //  Booster cancel
            }

            ////////////////////////    Use weapon
            {
                //  Reload weapon
                if (this.gameInput.IsStrokeKey(GameKey.WeaponReload) &&
                    (!IsDead && !IsCriticalDamaged && !IsFiring && !isActiveBooster &&
                     !isDelayBooster && !IsWeaponChanging && enableControl))
                {
                    ActionReload(CurrentWeapon);
                }
                //  Change weapon
                else if (this.gameInput.IsStrokeKey(GameKey.WeaponChange) &&
                    (!IsDead && !IsCriticalDamaged && !IsFiring && !isActiveBooster &&
                    !isDelayBooster && !IsWeaponChanging && enableControl))
                {
                    if (possiblePickupWeapon == null)
                        ActionSwapWeapon();
                    else
                        pickupElapsedTime = 0.0f;
                }
                //  Pickup weapon using change key
                else if (this.gameInput.IsPressKey(GameKey.WeaponChange) &&
                    (!IsDead && !IsCriticalDamaged && !IsFiring && !isActiveBooster &&
                    !isDelayBooster && !IsWeaponChanging && enableControl))
                {
                    if (possiblePickupWeapon != null)
                    {
                        RobotGameGame.CurrentStage.DisplayPickupCoolTime(
                                                        (int)this.PlayerIndex, true);

                        pickupElapsedTime +=
                            (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (pickupElapsedTime > 1.0f)
                        {
                            //  Pick up a weapon in the world
                            ActionPickupWeapon(possiblePickupWeapon);

                            //  Swaps the new weapon
                            ActionSwapSubWeapon();

                            pickupElapsedTime = 0.0f;

                            RobotGameGame.CurrentStage.DisplayPickupCoolTime(
                                                        (int)this.PlayerIndex, false);
                        }
                    }
                    else
                    {
                        pickupElapsedTime = 0.0f;

                        RobotGameGame.CurrentStage.DisplayPickupCoolTime(
                                                        (int)this.PlayerIndex, false);
                    }
                }
                //  Change weapon
                else if (this.gameInput.IsReleaseKey(GameKey.WeaponChange) &&
                (!IsDead && !IsCriticalDamaged && !IsFiring && !isActiveBooster &&
                 !isDelayBooster && !IsWeaponChanging && enableControl))
                {
                    if (possiblePickupWeapon != null)
                    {
                        if (pickupElapsedTime < 0.35f && pickupElapsedTime > 0.0f)
                        {
                            ActionSwapWeapon();
                        }
                    }

                    pickupElapsedTime = 0.0f;
                }

                //  Fire weapon
                if (this.gameInput.IsPressKey(GameKey.WeaponFire) &&
                     (!IsDead && !IsCriticalDamaged && !isDelayBooster &&
                      !IsReloading && !IsWeaponChanging && enableControl))
                {
                    if (CurrentWeapon.IsPossibleToFire())
                    {
                        //  Fire action
                        ActionFire();

                        WeaponFire();
                    }

                    //  Empty the weapon?
                    if (CurrentWeapon.NeedToReload && !IsReloading && !IsWeaponChanging)
                    {
                        ActionNonFire();

                        this.tryEmptyWeeapon = true;

                        if (this.CurrentWeapon.WeaponType == WeaponType.PlayerMachineGun)
                            CurrentWeapon.StopFireSound();

                        //  Play a weapon empty sound
                        if (!GameSound.IsPlaying(soundFireEmpty))
                        {
                            soundFireEmpty = GameSound.Play3D(
                                SoundTrack.PlayerEmptyBullet,
                                RobotGameGame.SinglePlayer);

                            RobotGameGame.CurrentStage.DisplayControlHelper(
                                            (int)this.PlayerIndex, 0, "(RB) RELOAD");
                        }
                    }
                }
                else
                {
                    if (this.CurrentWeapon.WeaponType == WeaponType.PlayerMachineGun)
                        CurrentWeapon.StopFireSound();

                    this.tryEmptyWeeapon = false;
                }
            }

            ////////////////////////    Rotate the player
            {
                if (this.gameInput.IsPressTurn() &&
                     (!IsDead && !IsCriticalDamaged && !isDelayBooster && enableControl))
                {
                    float angle = 0.0f;

                    //  Turn angle
                    if (isActiveBooster)
                        angle = BoosterTurnAngle;
                    else
                        angle = TurnAngle;

                    //  Look at left
                    if (this.gameInput.IsPressKey(GameKey.TurnLeft))
                    {
                        Rotate(new Vector2(angle, 0.0f));

                        if (this.CurrentLowerAction == LowerAction.Idle ||
                            this.CurrentLowerAction == LowerAction.RightTurn)
                        {
                            this.prepareLowerAction = LowerAction.LeftTurn;
                        }
                    }
                    //  Look at right
                    else if (this.gameInput.IsPressKey(GameKey.TurnRight))
                    {
                        Rotate(new Vector2(-angle, 0.0f));

                        if (this.CurrentLowerAction == LowerAction.Idle ||
                            this.CurrentLowerAction == LowerAction.LeftTurn)
                        {
                            this.prepareLowerAction = LowerAction.RightTurn;
                        }
                    }
                }
                else if (!IsDead && !IsCriticalDamaged && 
                         !isActiveBooster && enableControl)
                {
                    if (CurrentLowerAction == LowerAction.LeftTurn ||
                        CurrentLowerAction == LowerAction.RightTurn)
                    {
                        this.prepareLowerAction = LowerAction.Idle;
                    }
                }
            }

            ////////////////////////    Movement the player
            {
                bool goingOn = false;
                Vector3 moveVelocity = Vector3.Zero;

                //  Player booster control
                if (this.isActiveBooster)
                {
                    if (this.isDelayBooster == false)
                    {
                        this.moveDirection = this.Direction;

                        //  Left moving
                        if (this.gameInput.IsPressKey(GameKey.MoveLeft))
                        {
                            moveVelocity.X = -this.specData.RunSpeed;
                            moveVelocity.Z = this.SpecData.BoosterSpeed;
                        }
                        //  Right moving
                        else if (this.gameInput.IsPressKey(GameKey.MoveRight))
                        {
                            moveVelocity.X = this.specData.RunSpeed;
                            moveVelocity.Z = this.SpecData.BoosterSpeed;
                        }
                        //  Forward booster
                        else
                        {
                            moveVelocity.Z = this.SpecData.BoosterSpeed;
                        }

                        goingOn = true;
                    }
                    else
                    {
                        goingOn = false;
                    }
                }
                //  Player movement control
                else
                {
                    if (this.gameInput.IsPressMovement() &&
                        (!IsDead && !IsCriticalDamaged && 
                        !isDelayBooster && enableControl))
                    {
                        float moveSpeed = 0.0f;

                        if (IsFiring || IsReloading || 
                            IsWeaponChanging || IsTryEmptyWeapon)
                            moveSpeed = WalkSpeed;
                        else
                            moveSpeed = RunSpeed;

                        //  Move forward
                        if (this.gameInput.IsPressKey(GameKey.MoveForward))
                        {
                            //  Move Left forward
                            if (this.gameInput.IsPressKey(GameKey.MoveLeft))
                            {
                                this.moveDirection = new Vector3(-1.0f, 0, 1.0f);

                                moveVelocity = this.moveDirection *
                                        (moveSpeed * (this.moveDirection.Length() / 2));
                            }
                            //  Move Right forward
                            else if (this.gameInput.IsPressKey(GameKey.MoveRight))
                            {
                                this.moveDirection = new Vector3(1.0f, 0, 1.0f);

                                moveVelocity = this.moveDirection *
                                        (moveSpeed * (this.moveDirection.Length() / 2));
                            }
                            //  Move forward
                            else
                            {
                                this.moveDirection = new Vector3(0, 0, 1.0f);

                                moveVelocity = this.moveDirection * moveSpeed;
                            }
                        }
                        //  Move Backward
                        else if (this.gameInput.IsPressKey(GameKey.MoveBackward))
                        {
                            //  Move Left backward
                            if (this.gameInput.IsPressKey(GameKey.MoveLeft))
                            {
                                this.moveDirection = new Vector3(-1.0f, 0, -1.0f);

                                moveVelocity = this.moveDirection *
                                    (WalkBackwardSpeed * 
                                    (this.moveDirection.Length() / 2));
                            }
                            //  Move Right backward
                            else if (this.gameInput.IsPressKey(GameKey.MoveRight))
                            {
                                this.moveDirection = new Vector3(1.0f, 0, -1.0f);

                                moveVelocity = this.moveDirection *
                                    (WalkBackwardSpeed * 
                                    (this.moveDirection.Length() / 2));
                            }
                            //  Move backward
                            else
                            {
                                this.moveDirection = new Vector3(0, 0, -1.0f);

                                moveVelocity = this.moveDirection * WalkBackwardSpeed;
                            }
                        }
                        else
                        {
                            //  Move to Left
                            if (this.gameInput.IsPressKey(GameKey.MoveLeft))
                            {
                                this.moveDirection = new Vector3(-1.0f, 0, 0);
                                moveVelocity = this.moveDirection * moveSpeed;
                            }
                            //  Move to Right
                            else if (this.gameInput.IsPressKey(GameKey.MoveRight))
                            {
                                this.moveDirection = new Vector3(1.0f, 0, 0);
                                moveVelocity = this.moveDirection * moveSpeed;
                            }
                            //  Move stop
                            else
                            {
                                moveVelocity = this.moveDirection = Vector3.Zero;
                            }
                        }

                        goingOn = true;

                        pickupElapsedTime = 0.0f;
                    }
                    else
                    {
                        this.moveDirection = Vector3.Zero;

                        goingOn = false;
                    }

                    if (!IsCriticalDamaged && !IsDead)
                        ActionMovement(gameTime, this.moveDirection);
                }

                if (goingOn)
                {
                    //  Display the pick up message
                    if (possiblePickupWeapon != null)
                    {
                        RobotGameGame.CurrentStage.DisplayControlHelper(
                                            (int)this.PlayerIndex, 1, "(LB) PICK UP");
                    }
                    else
                    {
                        RobotGameGame.CurrentStage.DisableControlHelper(
                                            (int)this.PlayerIndex, 1);
                    }

                    possiblePickupWeapon = null;

                    //  Collision detecting with all enemies and
                    //  world before our player moves 
                    CollisionResult result = MoveHitTest(gameTime, moveVelocity);
                    if (result != null)
                    {
                        if (result.detectedCollide.Owner is GameItemBox)
                        {
                            GameItemBox item =
                                        result.detectedCollide.Owner as GameItemBox;

                            ActionPickUpItem(item);

                            goingOn = true;
                        }
                        else if (result.detectedCollide.Owner is GameWeapon)
                        {
                            GameWeapon weapon =
                                        result.detectedCollide.Owner as GameWeapon;

                            goingOn = true;

                            possiblePickupWeapon = weapon;
                        }
                        else
                        {                            
                            MoveStop();

                            //  Blocked booster moving
                            if (this.isActiveBooster)
                                ActionBoosterBreak();

                            goingOn = false;
                        }
                    }
                    else
                    {
                        if (IsDead || IsCriticalDamaged)
                        {
                            goingOn = false;
                        }
                        else
                        {
                            goingOn = true;
                        }
                    }
                }

                if (goingOn)
                {
                    Move(moveVelocity);
                }
                else
                {
                    MoveStop();
                }
            }

            //  Calculate camera distance with player
            CheckCollisionCamera();
        }

        #endregion            
                
        #region Process Action
        
        /// <summary>
        /// processes the current action info.
        /// </summary>
        protected void ProcessAction(GameTime gameTime)
        {
            switch (CurrentUpperAction)
            {
                case UpperAction.WeaponChange:
                    {
                        if (this.actionElapsedTime >= this.weaponChangeDurationTime)
                        {
                            ActionIdle();

                            this.actionElapsedTime = 0.0f;
                        }
                        else
                        {
                            this.actionElapsedTime +=
                                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                    }
                    break;
            }

            switch (CurrentLowerAction)
            {                
                case LowerAction.Damage:
                    {
                        if (this.actionElapsedTime >= this.specData.CriticalDamagedTime)
                        {
                            //  Return to idle state
                            ActionIdle();

                            this.actionElapsedTime = 0.0f;
                        }
                        else
                        {
                            this.actionElapsedTime += 
                                        (float)gameTime.ElapsedGameTime.TotalSeconds;

                            MoveStop();
                        }
                    }
                    break;                
                case LowerAction.BoosterActive:
                case LowerAction.BoosterLeftTurn:
                case LowerAction.BoosterRightTurn:
                    {
                        if( this.boosterElapsedTime <= 0.0f)
                        {
                            this.boosterElapsedTime = 0.0f;

                            ActionBoosterFinish();
                        }
                        else
                        {
                            this.isActiveBooster = true;
                            this.isDelayBooster = false;

                            this.boosterElapsedTime -=
                                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                    }
                    break;
                case LowerAction.BoosterPrepare:
                    {
                        MoveStop();

                        //  Ready to booster action
                        if (this.actionElapsedTime >= this.specData.BoosterPrepareTime)
                        {
                            ActionBoosterActive();

                            this.actionElapsedTime = 0.0f;
                        }
                        else
                        {
                            this.isActiveBooster = true;
                            this.isDelayBooster = true;

                            this.actionElapsedTime +=
                                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                    }
                    break;
                case LowerAction.BoosterFinish:
                    {
                        MoveStop();

                        if (this.actionElapsedTime >= this.boosterFinishDurationTime)
                        {
                            this.isDelayBooster = false;
                            this.isActiveBooster = false;

                            ActionIdle();

                            this.actionElapsedTime = 0.0f;
                        }
                        else
                        {
                            this.isActiveBooster = true;
                            this.isDelayBooster = true;                            

                            this.actionElapsedTime +=
                                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        
                    }
                    break;
                case LowerAction.BoosterBreak:
                    {
                        MoveStop();

                        if (this.actionElapsedTime >= this.boosterBreakDurationTime)
                        {
                            this.isDelayBooster = false;
                            this.isActiveBooster = false;

                            ActionIdle();

                            this.actionElapsedTime = 0.0f;
                        }
                        else
                        {
                            this.isActiveBooster = true;
                            this.isDelayBooster = true;                            

                            this.actionElapsedTime +=
                                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }

                    }
                    break;
                case LowerAction.ForwardDead:
                case LowerAction.BackwardDead:
                case LowerAction.LeftDead:
                case LowerAction.RightDead:
                    {
                        AnimationSequence animation = 
                              GetAnimation(indexLowerAnimation[(int)CurrentLowerAction]);

                        const float secondDestroyGap = 0.5f;

                        this.isActiveBooster = false;
                        this.isDelayBooster = false;

                        if (this.actionElapsedTime < 
                                        animation.Duration + secondDestroyGap)
                        {
                            if (this.actionElapsedTime >= animation.Duration - 0.5f)
                            {
                                Material.alpha = 
                                        ((animation.Duration + secondDestroyGap) - 
                                        this.actionElapsedTime) / secondDestroyGap;

                                //  Play the destroy Second particle
                                if (!GameSound.IsPlaying(soundDestroy))
                                {
                                    soundDestroy = GameSound.Play3D(
                                                        SoundTrack.DestroyLightMech2,
                                                        RobotGameGame.SinglePlayer);

                                    Matrix world = Matrix.CreateTranslation(
                                                    WorldTransform.Translation);

                                    GameParticle.PlayParticle(
                                                    ParticleType.DestroyHeavyMech2,
                                                    world, Matrix.Identity);
                                }
                            }
                        }

                        if (this.actionElapsedTime >= 
                                        animation.Duration + secondDestroyGap)
                        {
                            this.Enabled = false;
                            this.Visible = true;

                            this.actionElapsedTime = 0.0f;
                        }
                        else
                        {
                            this.actionElapsedTime += 
                                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// takes the shooting action.
        /// </summary>
        public override bool ActionFire()
        {
            UpperAction forwardFire = UpperAction.Count;
            UpperAction leftFire = UpperAction.Count;
            UpperAction rightFire = UpperAction.Count;

            //  Firing animation
            switch (this.CurrentWeapon.WeaponType)
            {
                case WeaponType.PlayerMachineGun:
                    {
                        forwardFire = UpperAction.ForwardMachineGunFire;
                        leftFire = UpperAction.LeftMachineGunFire;
                        rightFire = UpperAction.RightMachineGunFire;
                    }
                    break;
                case WeaponType.PlayerShotgun:
                    {
                        forwardFire = UpperAction.ForwardShotgunFire;
                        leftFire = UpperAction.LeftShotgunFire;
                        rightFire = UpperAction.RightShotgunFire;

                    }
                    break;
                case WeaponType.PlayerHandgun:
                    {
                        forwardFire = UpperAction.ForwardHandgunFire;
                        leftFire = UpperAction.LeftHandgunFire;
                        rightFire = UpperAction.RightHandgunFire;
                    }
                    break;
            }

            //  active booster ?
            if (isActiveBooster)
            {
                this.prepareUpperAction = forwardFire;
                this.isOverwriteUpperAction = true; //  Retry playing action
            }
            else
            {
                //  Left moving fire
                if (this.moveDirection.X < 0.0f)
                {
                    //  Forward left moving fire
                    if (this.moveDirection.Z > 0.0f)              
                    {
                        this.prepareUpperAction = leftFire;
                    }
                    //  Backward left moving fire
                    else if (this.moveDirection.Z < 0.0f)         
                    {
                        this.prepareUpperAction = rightFire;
                    }
                    //  Left moving fire
                    else                               
                    {
                        this.prepareUpperAction = leftFire;
                    }

                    this.isOverwriteUpperAction = true; //  Retry playing action
                }
                //  Right moving Fire
                else if (this.moveDirection.X > 0.0f)
                {
                    //  Forward right moving fire
                    if (this.moveDirection.Z > 0.0f)              
                    {
                        this.prepareUpperAction = rightFire;
                    }
                    //  Backward right moving fire
                    else if (this.moveDirection.Z < 0.0f)         
                    {
                        this.prepareUpperAction = leftFire;
                    }
                    //  Right moving fire
                    else                                
                    {
                        this.prepareUpperAction = rightFire;
                    }

                    this.isOverwriteUpperAction = true; //  Retry playing action
                }
                //  Forward or Backward moving Fire
                else
                {
                    this.prepareUpperAction = forwardFire;
                    this.isOverwriteUpperAction = true; //  Retry playing action
                }
            }

            return false;
        }

        /// <summary>
        /// takes the reload action.
        /// </summary>
        /// <param name="weapon">current weapon</param>
        /// <returns></returns>
        public override bool ActionReload(GameWeapon weapon)
        {
            //  If weapon reloaded, action reload
            if (CurrentWeapon.IsPossibleToReload())
            {
                CurrentWeapon.Reload(this.UnitType);

                switch (this.CurrentWeapon.WeaponType)
                {
                    case WeaponType.PlayerMachineGun:
                        {
                            this.prepareUpperAction = UpperAction.ReloadMachineGun;
                        }
                        break;
                    case WeaponType.PlayerShotgun:
                        {
                            this.prepareUpperAction = UpperAction.ReloadShotgun;
                        }
                        break;
                    case WeaponType.PlayerHandgun:
                        {
                            this.prepareUpperAction = UpperAction.ReloadHandgun;
                        }
                        break;
                }

                RobotGameGame.CurrentStage.DisableControlHelper(
                    (int)this.PlayerIndex, 0);
 
                return true;
            }

            return false;
        }

        /// <summary>
        /// changes to the next weapon.
        /// </summary>
        public void ActionSwapWeapon()
        {
            if (IsPossibleWeaponChange)
            {
                CurrentWeapon.StopFireSound();

                // Swap the next weapon
                SwapNextWeapon();

                //  Play the sound
                GameSound.Play3D(SoundTrack.PlayerSwapWeapon,
                    RobotGameGame.SinglePlayer);

                this.prepareUpperAction = UpperAction.WeaponChange;

                if (CurrentWeapon.NeedToReload)
                {
                    RobotGameGame.CurrentStage.DisplayControlHelper(
                                            (int)this.PlayerIndex, 0, "(RB) RELOAD");
                }
                else
                {
                    RobotGameGame.CurrentStage.DisableControlHelper(
                                            (int)this.PlayerIndex, 0);
                }
            }
        }

        /// <summary>
        /// changes to the sub weapon.
        /// </summary>
        public void ActionSwapSubWeapon()
        {
            if (IsPossibleWeaponChange)
            {
                CurrentWeapon.StopFireSound();

                // Swap the sub weapon
                ChangeWeapon(1);

                //  Play the sound
                GameSound.Play3D(SoundTrack.PlayerSwapWeapon, 
                    RobotGameGame.SinglePlayer);

                this.prepareUpperAction = UpperAction.WeaponChange;

                if (CurrentWeapon.NeedToReload)
                {
                    RobotGameGame.CurrentStage.DisplayControlHelper(
                                            (int)this.PlayerIndex, 0, "(RB) RELOAD");
                }
                else
                {
                    RobotGameGame.CurrentStage.DisableControlHelper(
                                            (int)this.PlayerIndex, 0);
                }
            }
        }
        
        /// <summary>
        /// acquires a sub weapon, which has been dropped on the world.
        /// If there is a sub weapon already, besides the default weapon, 
        /// the current sub weapon is dropped onto the world and acquires 
        /// the specified pickup weapon.
        /// </summary>
        /// <param name="pickupWeapon">pickup weapon</param>
        public void ActionPickupWeapon(GameWeapon pickupWeapon)
        {
            bool isSame = false;
            string message = "GOT";

            //  You've sub weapon
            if (WeaponCount > 1)
            {
                GameWeapon youveWeapon = GetWeapon(1);

                //  You've a same sub-weapon
                if (youveWeapon.WeaponType == pickupWeapon.WeaponType)
                {
                    youveWeapon.RemainAmmo += pickupWeapon.RemainAmmo;
                    pickupWeapon.Discard();
                    isSame = true;
                    possiblePickupWeapon = null;
                    RobotGameGame.CurrentStage.DisableControlHelper(
                        (int)this.PlayerIndex, 1);
                }
                //  If difference, your sub weapon must be drop to world
                else
                {
                    weaponList[1].Drop(new Vector3(Position.X, 0.5f, Position.Z), 
                        RobotGameGame.CurrentGameLevel.SceneWorldRoot,
                        RobotGameGame.CurrentGameLevel.CollisionLayerItems);
                    possiblePickupWeapon = weaponList[1];
                    weaponList.RemoveAt(1);
                }
            }
            else
            {
                RobotGameGame.CurrentStage.DisableControlHelper(
                    (int)this.PlayerIndex, 1);

                possiblePickupWeapon = null;
            }

            //  Add new weapon
            if (isSame == false)
            {
                weaponList.Add(pickupWeapon);
                pickupWeapon.Pickup(this);

                //  Play swap action
                if (this.CurrentWeaponSlot != 0)
                {
                    // Swap the new weapon
                    SelectWeapon(1);

                    //  Update selected weapon image in the Hud
                    RobotGameGame.CurrentStage.SetCurrentWeaponHud(
                        (int)this.PlayerIndex, CurrentWeapon.WeaponType);

                    this.prepareUpperAction = UpperAction.WeaponChange;

                    if (CurrentWeapon.NeedToReload)
                    {
                        RobotGameGame.CurrentStage.DisplayControlHelper(
                                        (int)this.PlayerIndex, 0, "(RB) RELOAD");
                    }
                    else
                    {
                        RobotGameGame.CurrentStage.DisableControlHelper(
                                        (int)this.PlayerIndex, 0);
                    }
                }
            }

            //  Play the pick up sound
            GameSound.Play3D(SoundTrack.PickupWeapon, RobotGameGame.SinglePlayer);
            
            switch(pickupWeapon.WeaponType)
            {
                case WeaponType.PlayerShotgun:
                    {
                        message += " SHOTGUN";
                    }
                    break;
                case WeaponType.PlayerHandgun:
                    {
                        message += " HANDGUN";
                    }
                    break;
            }

            //  Display the pick up message to screen
            RobotGameGame.CurrentStage.DisplayPickup((int)this.PlayerIndex, message,
                3.0f);
        }

        /// <summary>
        /// acquires the item on the world.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool ActionPickUpItem(GameItemBox item)
        {
            string message = "Pickup item";
            SoundTrack sound = SoundTrack.Count;

            switch (item.SpecData.Type)
            {
                case ItemType.RemedyBox:
                    {
                        if (this.IsFullLife)
                            return false;

                        message = "+ " + item.SpecData.RecoveryLife + "% ARMOR";

                        sound = SoundTrack.PickupRemedyBox;
                    }
                    break;
                case ItemType.MagazineBox:
                    {
                        //  The default weapon is must be machine gun
                        if (DefaultWeapon.WeaponType != WeaponType.PlayerMachineGun)
                            return false;

                        message = "+ " + item.SpecData.RecoveryBullet + " BULLETS";

                        sound = SoundTrack.PickupMagazine;
                    }
                    break;
            }

            //  Applies this item
            item.PickUp(this);

            //  Play the sound
            GameSound.Play3D(sound, RobotGameGame.SinglePlayer);

            //  Display item message to screen
            RobotGameGame.CurrentStage.DisplayPickup((int)this.PlayerIndex, message,
                3.0f);

            return true;
        }

        /// <summary>
        /// takes an idle action.
        /// </summary>
        public override void ActionIdle()
        {
            this.prepareLowerAction = LowerAction.Idle;
            this.prepareUpperAction = UpperAction.Idle;

            CurrentWeapon.StopFireSound();

            if (GameSound.IsPlaying(soundRun))
                GameSound.Stop(soundRun);

            if (GameSound.IsPlaying(soundWalk))
                GameSound.Stop(soundWalk);

            MoveStop();
        }

        /// <summary>
        /// attacked by enemy. when HP is 0, gets destoryed.
        /// </summary>
        /// <param name="attacker"></param>
        public override void ActionHit(GameUnit attacker)
        {
            if (IsDead) return;

            if (attacker.CurrentWeapon == null)
                throw new InvalidOperationException("The harmer no have weapon");

            GameWeaponSpec AttackerWeaponSpec = attacker.CurrentWeapon.SpecData;

            //  NeverDie mode (DEBUG)
            if (this.debugMode == DebugMode.NeverDie || this.debugMode == DebugMode.God)
            {
                Life = SpecData.Life;
            }            
            else
            {
                //  Reduce our player's life point by harmer's weapon power
                Life -= AttackerWeaponSpec.Damage;
                if (Life < 0) Life = 0;
            }

            if (Life == 0)
            {
                //  Our player is dead
                ActionDead(attacker.WorldTransform.Translation);

                BoosterParticleStop();
            }
            else
            {
                ActionDamage(attacker);
            }
        }

        /// <summary>
        /// attacked by enemy. takes the damage action.
        /// </summary>
        /// <param name="attacker"></param>
        public override void ActionDamage(GameUnit attacker)
        {
            switch (attacker.CurrentWeapon.WeaponType)
            {
                case WeaponType.PlayerHandgun:
                case WeaponType.PlayerMachineGun:
                case WeaponType.PlayerShotgun:
                case WeaponType.CameleerGun:
                case WeaponType.MaomingGun:
                    {
                        GameSound.Play3D(SoundTrack.HitGun, RobotGameGame.SinglePlayer);
                    }
                    break;
                case WeaponType.DuskmasCannon:
                case WeaponType.HammerCannon:
                case WeaponType.TigerCannon:
                    {
                        GameSound.Play3D(SoundTrack.HitCannon, 
                            RobotGameGame.SinglePlayer);
                    }
                    break;           
                case WeaponType.PhantomMelee:
                    {
                        GameSound.Play3D(SoundTrack.HitBossMelee, 
                            RobotGameGame.SinglePlayer);
                    }
                    break;
            }

            if (attacker.CurrentWeapon.SpecData.CriticalDamagedFire)
            {
                if (!isActiveBooster && !IsReloading && !IsWeaponChanging)
                {
                    this.prepareLowerAction = LowerAction.Damage;
                    this.prepareUpperAction = UpperAction.Damage;

                    this.isOverwriteLowerAction = true;
                    this.isOverwriteUpperAction = true;

                    if (GameSound.IsPlaying(soundRun))
                        GameSound.Stop(soundRun);

                    if (GameSound.IsPlaying(soundWalk))
                        GameSound.Stop(soundWalk);

                    CurrentWeapon.StopFireSound();
                }

                //  Trembling the camera
                if (FrameworkCore.CurrentCamera.FirstCamera is FollowCamera)
                {
                    InputComponent input = 
                        FrameworkCore.InputManager.GetInputComponent(this.PlayerIndex);

                    input.SetGamePadVibration(this.specData.CriticalDamagedTime / 2, 
                                              0.4f, 0.4f);
                }
           }
        }

        /// <summary>
        /// takes the destruction action.
        /// Plays different destroy animation according to the attack direction.
        /// </summary>
        /// <param name="attackerPosition"></param>
        public override void ActionDead(Vector3 attackerPosition)
        {
            Vector3 dir = Vector3.Normalize(attackerPosition - Position);

            float FrontDot = Vector3.Dot(Direction, dir);
            float RightDot = Vector3.Dot(Right, dir);

            if (FrontDot > 0.5f)
            {
                // Hit from front
                this.prepareLowerAction = LowerAction.BackwardDead;
                this.prepareUpperAction = UpperAction.BackwardDead;
            }
            else if (FrontDot < -0.5f)
            {
                // Hit from back
                this.prepareLowerAction = LowerAction.ForwardDead;
                this.prepareUpperAction = UpperAction.ForwardDead;
            }
            else if (RightDot >= 0.0f)
            {
                //  Hit from right
                this.prepareLowerAction = LowerAction.LeftDead;
                this.prepareUpperAction = UpperAction.LeftDead;
            }
            else if (RightDot < 0.0f)
            {
                //  Hit from left
                this.prepareLowerAction = LowerAction.RightDead;
                this.prepareUpperAction = UpperAction.RightDead;
            }

            //  Stop the sound
            if (GameSound.IsPlaying(soundRun))
                GameSound.Stop(soundRun);

            if (GameSound.IsPlaying(soundWalk))
                GameSound.Stop(soundWalk);

            CurrentWeapon.StopFireSound();

            //  Play a explosion particle
            GameSound.Play3D(SoundTrack.PlayerDestroy1, RobotGameGame.SinglePlayer);

            Matrix world = Matrix.CreateTranslation(Position);

            GameParticle.PlayParticle(ParticleType.DestroyHeavyMech1, 
                                      world, Matrix.Identity);

            //  vibrate the camera and the controller
            if (FrameworkCore.CurrentCamera.FirstCamera is FollowCamera)
            {
                ViewCamera viewCamera = FrameworkCore.CurrentCamera;
                FollowCamera camera = null;

                switch (this.PlayerIndex)
                {
                    case PlayerIndex.One:
                        camera = viewCamera.GetCamera(0) as FollowCamera;
                        break;
                    case PlayerIndex.Two:
                        camera = viewCamera.GetCamera(1) as FollowCamera;
                        break;
                    case PlayerIndex.Three:
                        camera = viewCamera.GetCamera(2) as FollowCamera;
                        break;
                    case PlayerIndex.Four:
                        camera = viewCamera.GetCamera(3) as FollowCamera;
                        break;
                }

                camera.SetTremble(1.0f, 0.2f);

                InputComponent input = 
                        FrameworkCore.InputManager.GetInputComponent(this.PlayerIndex);

                input.SetGamePadVibration(1.0f, 0.6f, 0.6f);
            }

            //  Remove the collision
            if (RobotGameGame.CurrentStage is VersusStageScreen)
                colLayerVersusTeam[(int)this.PlayerIndex].RemoveCollide(Collide);
            else
                colLayerFriendlyMech.RemoveCollide(Collide);

            //  Stop the booster particle and sound
            if (this.isActiveBooster)
            {
                if (GameSound.IsPlaying(soundBoosterPrepare))
                    GameSound.Stop(soundBoosterPrepare);

                if (GameSound.IsPlaying(soundBooster))
                    GameSound.Stop(soundBooster);

                BoosterParticleStop();
            }

            this.SourceBlend = Blend.SourceAlpha;
            this.DestinationBlend = Blend.InverseSourceAlpha;
            this.AlphaBlendEnable = true;
            this.ReferenceAlpha = 0;

            this.isActiveBooster = false;
            this.isDelayBooster = false;

            this.actionElapsedTime = 0.0f;

            RobotGameGame.CurrentStage.DisableAllControlHelper();
        }

        /// <summary>
        /// an animation before booster.
        /// </summary>
        public void ActionBooster()
        {
            ActionBoosterPrepare();

            if (GameSound.IsPlaying(soundWalk))
                GameSound.Stop(soundWalk);

            if (GameSound.IsPlaying(soundRun))
                GameSound.Stop(soundRun);

            this.rootRotationAngle = 0.0f;
        }

        /// <summary>
        /// an animation at the end of booster.
        /// </summary>
        public void ActionBoosterFinish()
        {
            this.isActiveBooster = true;
            this.isDelayBooster = true;
            this.actionElapsedTime = 0.0f;
            this.boosterWaveEffectTime = 0.0f;

            this.prepareLowerAction = LowerAction.BoosterFinish;
            this.prepareUpperAction = UpperAction.BoosterFinish;

            //  Stop the booster sound
            if (GameSound.IsPlaying(soundBoosterPrepare))
                GameSound.Stop(soundBoosterPrepare);

            if (GameSound.IsPlaying(soundBooster))
                GameSound.Stop(soundBooster);

            //  Stop the booster particle
            BoosterParticleStop();

            //  Vibrate the camera and controller
            if (FrameworkCore.CurrentCamera.FirstCamera is FollowCamera)
            {
                InputComponent input =
                        FrameworkCore.InputManager.GetInputComponent(this.PlayerIndex);

                input.SetGamePadVibration(boosterFinishDurationTime / 2, 0.15f, 0.15f);
            }
        }

        /// <summary>
        /// an animation of booster getting interrupted.
        /// </summary>
        protected void ActionBoosterBreak()
        {
            this.isActiveBooster = true;
            this.isDelayBooster = true;
            this.actionElapsedTime = 0.0f;
            this.boosterWaveEffectTime = 0.0f;

            this.prepareLowerAction = LowerAction.BoosterBreak;
            this.prepareUpperAction = UpperAction.BoosterBreak;

            if (GameSound.IsPlaying(soundBoosterPrepare))
                GameSound.Stop(soundBoosterPrepare);

            if (GameSound.IsPlaying(soundBooster))
                GameSound.Stop(soundBooster);

            //  Stop the particle
            BoosterParticleStop();

            //  Vibrate a camera and controller
            if (FrameworkCore.CurrentCamera.FirstCamera is FollowCamera)
            {
                ViewCamera viewCamera = FrameworkCore.CurrentCamera;
                FollowCamera camera = null;

                switch (this.PlayerIndex)
                {
                    case PlayerIndex.One:
                        camera = viewCamera.GetCamera(0) as FollowCamera;
                        break;
                    case PlayerIndex.Two:
                        camera = viewCamera.GetCamera(1) as FollowCamera;
                        break;
                    case PlayerIndex.Three:
                        camera = viewCamera.GetCamera(2) as FollowCamera;
                        break;
                    case PlayerIndex.Four:
                        camera = viewCamera.GetCamera(3) as FollowCamera;
                        break;
                }

                camera.SetTremble(0.6f, 0.1f);

                InputComponent input =
                        FrameworkCore.InputManager.GetInputComponent(this.PlayerIndex);

                input.SetGamePadVibration(boosterBreakDurationTime / 2, 0.3f, 0.3f);
            }
        }

        /// <summary>
        /// an animation of getting ready before booster.
        /// </summary>
        protected void ActionBoosterPrepare()
        {
            this.isActiveBooster = true;
            this.isDelayBooster = true;
            this.actionElapsedTime = 0.0f;
            this.boosterWaveEffectTime = 0.0f;

            this.prepareLowerAction = LowerAction.BoosterPrepare;
            this.prepareUpperAction = UpperAction.BoosterPrepare;

            if (!GameSound.IsPlaying(soundBoosterPrepare))
            {
                soundBoosterPrepare = GameSound.Play3D(SoundTrack.PlayerPrepareBooster, 
                                                       RobotGameGame.SinglePlayer);
            }

            int count = 1;
            if (this.UnitType == UnitTypeId.Mark || this.UnitType == UnitTypeId.Yager)
                count = 2;

            //  Play the booster prepare particle on the backpack
            for (int i = 0; i < count; i++)
            {
                int index = -1;

                if( i == 0)
                    index = this.indexLeftBoosterDummy;
                else
                    index = this.indexRightBoosterDummy;

                particleBoosterPrepare[i] = GameParticle.PlayParticle(
                    ParticleType.BoosterPrepare, BoneTransforms[index],
                    Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)));
            }
        }

        /// <summary>
        /// an animation of booster in progress.
        /// </summary>
        protected void ActionBoosterActive()
        {
            this.isActiveBooster = true;
            this.isDelayBooster = false;

            this.prepareLowerAction = LowerAction.BoosterActive;
            this.prepareUpperAction = UpperAction.BoosterActive;

            if (!GameSound.IsPlaying(soundBooster))
                soundBooster = GameSound.Play3D(SoundTrack.PlayerBooster, 
                                                RobotGameGame.SinglePlayer);

            Matrix fixedAxis = Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f));

            int count = 1;
            if (this.UnitType == UnitTypeId.Mark || this.UnitType == UnitTypeId.Yager)
                count = 2;

            //  Play the booster flash particle on the backpack
            for (int i = 0; i < count; i++)
            {
                int index = -1;

                if (i == 0)
                    index = this.indexLeftBoosterDummy;
                else
                    index = this.indexRightBoosterDummy;

                particleBoosterOn[i] = GameParticle.PlayParticle(
                                          ParticleType.BoosterOn,
                                          BoneTransforms[index],
                                          fixedAxis);
            }

            //  Booster slide particle on the ground
            particleBoosterGround[0] = GameParticle.PlayParticle(
                                                ParticleType.BoosterGround,
                                                BoneTransforms[indexLeftFootDummy],
                                                fixedAxis);

            particleBoosterGround[1] = GameParticle.PlayParticle(
                                                ParticleType.BoosterGround,
                                                BoneTransforms[indexRightFootDummy],
                                                fixedAxis);

            //  vibrate a camera and controller
            if (FrameworkCore.CurrentCamera.FirstCamera is FollowCamera)
            {
                InputComponent input =
                        FrameworkCore.InputManager.GetInputComponent(this.PlayerIndex);

                input.SetGamePadVibration(0.3f, 0.5f, 0.5f);
            }
        }

        /// <summary>
        /// stop all booster's particles.
        /// </summary>
        protected void BoosterParticleStop()
        {
            //  Stop the particles
            for (int i = 0; i < 2; i++)
            {
                if (particleBoosterOn[i] != null)
                {
                    particleBoosterOn[i].Stop();
                    particleBoosterOn[i] = null;
                }

                if (particleBoosterPrepare[i] != null)
                {
                    particleBoosterPrepare[i].Stop();
                    particleBoosterPrepare[i] = null;
                }

                if (particleBoosterGround[i] != null)
                {
                    particleBoosterGround[i].Stop();
                    particleBoosterGround[i] = null;
                }
            }
        }

        /// <summary>
        /// determines an moving animation according to velocity.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="moveDirection"></param>
        public void ActionMovement(GameTime gameTime, Vector3 moveDirection)
        {
            bool doMove = true;
                        
            //  Forward
            if (moveDirection.Z > 0.0f)
            {
                //  Move Left forward
                if (moveDirection.X < 0.0f)
                {
                    //  Player change root axis
                    this.rootRotationAngle = 35.0f;
                }
                //  Move Right forward
                else if (moveDirection.X > 0.0f)
                {
                    //  Player change root axis
                    this.rootRotationAngle = -35.0f;
                }
                //  Forward
                else
                {
                    this.rootRotationAngle = 0.0f;
                }

                if (IsFiring || IsReloading || IsWeaponChanging || IsTryEmptyWeapon)
                {
                    if (this.CurrentLowerAction != LowerAction.Walk)
                        this.prepareLowerAction = LowerAction.Walk;
                }
                else
                {
                    if (this.CurrentLowerAction != LowerAction.Run)
                        this.prepareLowerAction = LowerAction.Run;
                }

                if (this.CurrentUpperAction != UpperAction.Run &&
                    !IsFiring && !IsReloading && !IsWeaponChanging && 
                    !IsTryEmptyWeapon && !IsDead)
                {
                    this.prepareUpperAction = UpperAction.Run;
                }
            }
            //  Backward
            else if (moveDirection.Z < 0.0f)
            {
                //  Move Left backward
                if (moveDirection.X < 0.0f)
                {
                    //  Player change root axis
                    this.rootRotationAngle = -30.0f;
                }
                //  Move Right backward
                else if (moveDirection.X > 0.0f)
                {
                    //  Player change root axis
                    this.rootRotationAngle = 30.0f;
                }
                //  Backward
                else
                {
                    //  Player change root axis
                    this.rootRotationAngle = 0.0f;
                }

                if (this.CurrentLowerAction != LowerAction.BackwardWalk)
                    this.prepareLowerAction = LowerAction.BackwardWalk;

                if (this.CurrentUpperAction != UpperAction.Idle &&
                    !IsFiring && !IsReloading && !IsTryEmptyWeapon &&
                    !IsWeaponChanging && !IsDead)
                {
                    this.prepareUpperAction = UpperAction.Idle;
                }
            }
            else
            {
                //  Move to Left
                if (moveDirection.X < 0.0f)
                {
                    //  Player change root axis
                    this.rootRotationAngle = 65.0f;

                    if (IsFiring || IsReloading || IsWeaponChanging || IsTryEmptyWeapon)
                    {
                        if (this.CurrentLowerAction != LowerAction.Walk)
                            this.prepareLowerAction = LowerAction.Walk;
                    }
                    else
                    {
                        if (this.CurrentLowerAction != LowerAction.Run)
                            this.prepareLowerAction = LowerAction.Run;
                    }

                    if (this.CurrentUpperAction != UpperAction.Run &&
                        !IsFiring && !IsReloading && !IsTryEmptyWeapon && 
                        !IsWeaponChanging && !IsDead)
                    {
                        this.prepareUpperAction = UpperAction.Run;
                    }
                }
                //  Move to Right
                else if (moveDirection.X > 0.0f)
                {
                    //  Player change root axis
                    this.rootRotationAngle = -65.0f;

                    if (IsFiring || IsReloading || IsWeaponChanging || IsTryEmptyWeapon)
                    {
                        if (this.CurrentLowerAction != LowerAction.Walk)
                            this.prepareLowerAction = LowerAction.Walk;
                    }
                    else
                    {
                        if (this.CurrentLowerAction != LowerAction.Run)
                            this.prepareLowerAction = LowerAction.Run;
                    }

                    if (this.CurrentUpperAction != UpperAction.Run &&
                        !IsFiring && !IsReloading && !IsTryEmptyWeapon &&
                        !IsWeaponChanging && !IsDead)
                    {
                        this.prepareUpperAction = UpperAction.Run;
                    }
                }
                //  Move Stop
                else
                {
                    doMove = false;

                    if (this.CurrentLowerAction == LowerAction.Run ||
                        this.CurrentLowerAction == LowerAction.Walk ||
                        this.CurrentLowerAction == LowerAction.BackwardWalk)
                    {
                        this.prepareLowerAction = LowerAction.Idle;
                    }

                    if (this.CurrentUpperAction != UpperAction.Idle && 
                        !IsFiring && !IsReloading && !IsTryEmptyWeapon &&
                        !IsWeaponChanging && !IsDead)
                    {
                        this.prepareUpperAction = UpperAction.Idle;
                    }
                                        
                    //  Player change root axis
                    this.rootRotationAngle = 0.0f;

                    this.moveElapsedTime = 0.0f;
                }
            }

            if (doMove)
            {
                //  Play the moving sound
                if (this.CurrentLowerAction == LowerAction.Run)
                {
                    if (this.moveElapsedTime == 0.0f)
                    {
                        if (GameSound.IsPlaying(soundWalk))
                            GameSound.Stop(soundWalk);

                        if (GameSound.IsPlaying(soundRun))
                            GameSound.Stop(soundRun);

                        soundRun = GameSound.Play3D(SoundTrack.PlayerRun, 
                                                    RobotGameGame.SinglePlayer);
                    }

                    //  Calculate run animation playing time for run sound
                    if (this.runDurationTime > this.moveElapsedTime)
                    {
                        this.moveElapsedTime += 
                            (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        this.moveElapsedTime = 0.0f;
                    }
                }
                else if (this.CurrentLowerAction == LowerAction.Walk)
                {
                    if (this.moveElapsedTime == 0.0f)
                    {
                        if (GameSound.IsPlaying(soundWalk))
                            GameSound.Stop(soundWalk);

                        if (GameSound.IsPlaying(soundRun))
                            GameSound.Stop(soundRun);

                        soundWalk = GameSound.Play3D(SoundTrack.PlayerWalk,
                                                    RobotGameGame.SinglePlayer);
                    }

                    //  Calculate walk animation playing time for run sound
                    if (this.walkDurationTime > this.moveElapsedTime)
                    {
                        this.moveElapsedTime +=
                            (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        this.moveElapsedTime = 0.0f;
                    }
                }
                else if (this.CurrentLowerAction == LowerAction.BackwardWalk)
                {
                    if (this.moveElapsedTime == 0.0f)
                    {
                        if (GameSound.IsPlaying(soundWalk))
                            GameSound.Stop(soundWalk);

                        if (GameSound.IsPlaying(soundRun))
                            GameSound.Stop(soundRun);

                        soundWalk = GameSound.Play3D(SoundTrack.PlayerWalk, 
                                                    RobotGameGame.SinglePlayer);

                        this.moveElapsedTime = 0.0f;
                    }

                    //  Calculate walk animation playing time for run sound
                    if (this.walkDurationTime > this.moveElapsedTime)
                    {
                        this.moveElapsedTime +=
                            (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        this.moveElapsedTime = 0.0f;
                    }
                }
            }
            else
            {
                // Stop the move sound
                if (GameSound.IsPlaying(soundRun))
                    GameSound.Stop(soundRun);

                if (GameSound.IsPlaying(soundWalk))
                    GameSound.Stop(soundWalk);

                this.moveElapsedTime = 0.0f;
            }
        }

        /// <summary>
        /// plays an animation of not firing due to having no ammunition.
        /// </summary>
        public void ActionNonFire()
        {
            //  Left moving fire
            if (this.moveDirection.X < 0.0f)
            {
                //  Forward left moving fire
                if (this.moveDirection.Z > 0.0f)              
                {
                    if( this.prepareUpperAction != UpperAction.LeftNonFire)
                        this.prepareUpperAction = UpperAction.LeftNonFire;
                }
                //  Backward left moving fire
                else if (this.moveDirection.Z < 0.0f)         
                {
                    if (this.prepareUpperAction != UpperAction.RightNonFire)
                        this.prepareUpperAction = UpperAction.RightNonFire;
                }
                //  Left moving fire
                else                               
                {
                    if (this.prepareUpperAction != UpperAction.LeftNonFire)
                        this.prepareUpperAction = UpperAction.LeftNonFire;
                }
            }
            //  Right moving Fire
            else if (this.moveDirection.X > 0.0f)
            {
                //  Forward right moving fire
                if (this.moveDirection.Z > 0.0f)              
                {
                    if (this.prepareUpperAction != UpperAction.RightNonFire)
                        this.prepareUpperAction = UpperAction.RightNonFire;
                }
                //  Backward right moving fire
                else if (this.moveDirection.Z < 0.0f)         
                {
                    if (this.prepareUpperAction != UpperAction.LeftNonFire)
                        this.prepareUpperAction = UpperAction.LeftNonFire;
                }
                //  Right moving fire
                else                                
                {
                    if (this.prepareUpperAction != UpperAction.RightNonFire)
                        this.prepareUpperAction = UpperAction.RightNonFire;
                }
            }
            //  Forward or Backward moving Fire
            else
            {
                if (this.prepareUpperAction != UpperAction.ForwardNonFire)
                    this.prepareUpperAction = UpperAction.ForwardNonFire;
            }
        }

        /// <summary>
        /// plays a lower body animation according to action.
        /// </summary>
        /// <param name="action">lower action</param>
        public void PlayLowerAction(LowerAction action)
        {
            PlayLowerAction(action, 0.0f);
        }

        /// <summary>
        /// plays a lower body animation according to action.
        /// </summary>
        /// <param name="action">lower action</param>
        /// <param name="startTime">start time of animation</param>
        public void PlayLowerAction(LowerAction action, float startTime)
        {
            AnimPlayMode playMode = AnimPlayMode.Repeat;
            float blendTime = 0.0f;

            switch (action)
            {
                case LowerAction.Idle:
                    {
                        blendTime = 0.5f;
                    }
                    break;
                case LowerAction.Run:
                    {
                        blendTime = 0.3f;
                    }
                    break;
                case LowerAction.Damage:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                case LowerAction.Walk:
                    {
                        blendTime = 0.3f;
                    }
                    break;
                case LowerAction.BackwardWalk:
                    {
                        blendTime = 0.3f;
                    }
                    break;
                case LowerAction.LeftTurn:
                case LowerAction.RightTurn:
                    {
                        blendTime = 0.5f;
                    }
                    break;
                case LowerAction.ForwardDead:
                case LowerAction.BackwardDead:
                case LowerAction.LeftDead:
                case LowerAction.RightDead:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;                
                case LowerAction.BoosterPrepare:
                case LowerAction.BoosterFinish:
                case LowerAction.BoosterActive:
                case LowerAction.BoosterBreak:
                case LowerAction.BoosterLeftTurn:
                case LowerAction.BoosterRightTurn:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                default:
                    throw new NotSupportedException("Not supported an animation");
            }

            //  Play the lower animation
            PlayAnimation(indexLowerAnimation[(int)action],
                          startTime, blendTime, 
                          this.defaultAnimationScaleFactor, 
                          playMode);

            this.currentLowerAction = action;
            this.prepareLowerAction = LowerAction.Unknown;
            this.isOverwriteLowerAction = false;
        }

        /// <summary>
        /// plays an upper animation according to action.
        /// </summary>
        /// <param name="action">upper action</param>
        public void PlayUpperAction(UpperAction action)
        {
            PlayUpperAction(action, 0.0f);
        }

        /// <summary>
        /// plays an upper animation according to action.
        /// </summary>
        /// <param name="action">upper action</param>
        /// <param name="startTime">start time of animation</param>
        public void PlayUpperAction(UpperAction action, float startTime)
        {
            AnimPlayMode playMode = AnimPlayMode.Repeat;
            float blendTime = 0.0f;

            switch (action)
            {
                case UpperAction.Idle:
                    {
                        blendTime = 0.5f;
                    }
                    break;
                case UpperAction.Run:
                    {
                        blendTime = 0.3f;
                    }
                    break;
                case UpperAction.Damage:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                case UpperAction.WeaponChange:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                case UpperAction.ForwardNonFire:
                case UpperAction.LeftNonFire:
                case UpperAction.RightNonFire:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.5f;
                    }
                    break;
                case UpperAction.ForwardMachineGunFire:
                case UpperAction.ForwardShotgunFire:
                case UpperAction.ForwardHandgunFire:
                case UpperAction.LeftMachineGunFire:
                case UpperAction.LeftShotgunFire:
                case UpperAction.LeftHandgunFire:
                case UpperAction.RightMachineGunFire:
                case UpperAction.RightShotgunFire:
                case UpperAction.RightHandgunFire:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.5f;
                    }
                    break;
                case UpperAction.ReloadMachineGun:
                case UpperAction.ReloadShotgun:
                case UpperAction.ReloadHandgun:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.5f;
                    }
                    break;
                case UpperAction.ForwardDead:
                case UpperAction.BackwardDead:
                case UpperAction.LeftDead:
                case UpperAction.RightDead:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                case UpperAction.BoosterPrepare:
                case UpperAction.BoosterFinish:
                case UpperAction.BoosterActive:
                case UpperAction.BoosterBreak:
                case UpperAction.BoosterLeftTurn:
                case UpperAction.BoosterRightTurn:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                default:
                    throw new NotSupportedException("Not supported an animation");
            }

            //  Play the upper animation
            PlayAnimation(indexUpperAnimation[(int)action],
                          startTime, 
                          blendTime, 
                          this.defaultAnimationScaleFactor, 
                          playMode);

            this.currentUpperAction = action;
            this.prepareUpperAction = UpperAction.Unknown;
            this.isOverwriteUpperAction = false;
        }

        #endregion

        /// <summary>
        /// checks for collision between the 3rd view camera, 
        /// which follows the player moving, and the world.
        /// </summary>
        public void CheckCollisionCamera()
        {
            CollideSphere playerSphere = Collide as CollideSphere;

            if ((FrameworkCore.CurrentCamera.FirstCamera is FollowCamera) == false)
                return;

            // Check collison of the follow camera
            ViewCamera viewCamera = FrameworkCore.CurrentCamera;
            FollowCamera camera = null;

            switch (this.PlayerIndex)
            {
                case PlayerIndex.One:
                    camera = viewCamera.GetCamera(0) as FollowCamera;
                    break;
                case PlayerIndex.Two:
                    camera = viewCamera.GetCamera(1) as FollowCamera;
                    break;
                case PlayerIndex.Three:
                    camera = viewCamera.GetCamera(2) as FollowCamera;
                    break;
                case PlayerIndex.Four:
                    camera = viewCamera.GetCamera(3) as FollowCamera;
                    break;
            }

            //  Default setting offset
            Vector3 followCameraOffset = SpecData.CameraPositionOffset;

            float distanceBetweenCamera =
                        Math.Abs(Vector3.Distance(Vector3.Zero, followCameraOffset));

            CollisionResult finalResult, leftResult, rightResult;
            finalResult = leftResult = rightResult = null;

            //  Booster camera
            if (this.isActiveBooster && this.isDelayBooster == false)
            {
                followCameraOffset.Z *= 1.5f;
            }

            Vector3 start = camera.Target;
            Vector3 dir = camera.Position - start;
            dir.Normalize();

            //  check to left side of camera
            {

                CollideRay simulateRay =
                    new CollideRay(start - camera.Right, dir);

                //  check collison with world
                leftResult = FrameworkCore.CollisionContext.HitTest(
                                                                simulateRay,
                                                                ref colLayerMoveWorld,
                                                                ResultType.NearestOne);
            }

            //  check to right side of camera
            {
                CollideRay simulateRay =
                    new CollideRay(start + camera.Right, dir);

                //  check collison with world
                rightResult = FrameworkCore.CollisionContext.HitTest(
                                                                simulateRay,
                                                                ref colLayerMoveWorld,
                                                                ResultType.NearestOne);
            }

            if (leftResult != null && rightResult != null)
            {
                if( leftResult.distance < rightResult.distance)
                    finalResult = leftResult;
                else
                    finalResult = rightResult;
            }
            else if (leftResult != null)
            {
                finalResult = leftResult;
            }
            else if (rightResult != null)
            {
                finalResult = rightResult;
            }

            if (finalResult != null)
            {
                //  The camera collided behind player with world
                if (finalResult.distance < distanceBetweenCamera)
                {
                    float distance = finalResult.distance;

                    if (finalResult.distance < playerSphere.Radius)
                        distance = playerSphere.Radius;

                    // Change camera position
                    camera.PositionOffset = new Vector3(
                                0.0f,
                                followCameraOffset.Y * 
                                    (distance / distanceBetweenCamera),
                                -distance);
                }
                else
                {
                    camera.PositionOffset = followCameraOffset;
                }
            }
            else
            {
                camera.PositionOffset = followCameraOffset;
            }
        }

        /// <summary>
        /// the mission is complete.
        /// All of the player’s actions stop and no more of keyboard input is received.
        /// </summary>
        public void MissionEnd()
        {
            this.EnableHandleInput = false;

            if (isActiveBooster)
            {
                ActionBoosterFinish();
            }

            if (!IsDead)
            {
                this.isActiveBooster = false;
                this.isDelayBooster = false;

                this.actionElapsedTime = 0.0f;

                ActionIdle();
            }

            MoveStop();

            //  Remove the collision
            if (RobotGameGame.CurrentStage is VersusStageScreen)
                colLayerVersusTeam[(int)this.PlayerIndex].RemoveCollide(Collide);
            else
                colLayerFriendlyMech.RemoveCollide(Collide);

            RobotGameGame.CurrentStage.DisableAllControlHelper();
        }

        /// <summary>
        /// fires forward with the current weapon.
        /// There is a divergence of a specified range at the shooting angle.
        /// </summary>
        public override void WeaponFire()
        {
            ViewCamera viewCamera = FrameworkCore.Viewer.CurrentCamera;
            CameraBase camera = null;

            if ((viewCamera.FirstCamera is FollowCamera) == false)
            {
                throw new InvalidOperationException("Invalid camera");
            }

            switch (this.PlayerIndex)
            {
                case PlayerIndex.One: camera = viewCamera.GetCamera(0); break;
                case PlayerIndex.Two: camera = viewCamera.GetCamera(1); break;
                case PlayerIndex.Three: camera = viewCamera.GetCamera(2); break;
                case PlayerIndex.Four: camera = viewCamera.GetCamera(3); break;
            }

            //  calculates an offset by resolution rate
            int fireVerticalCenterOffset =
                (int)(-40.0f * ((float)FrameworkCore.ViewHeight / 480.0f));

            int fireRadiusOffset =
                (int)(25.0f * ((float)FrameworkCore.ViewHeight / 480.0f));

            Matrix proj = camera.ProjectionMatrix;
            Matrix view = camera.ViewMatrix;

            //  calculates a randomed target position
            Vector2 rndPoint = new Vector2(
                    (FrameworkCore.ViewWidth / 2) +
                    (float)HelperMath.Randomi(-fireRadiusOffset, fireRadiusOffset),
                    (FrameworkCore.ViewHeight / 2) + fireVerticalCenterOffset +
                    (float)HelperMath.Randomi(-fireRadiusOffset, fireRadiusOffset));

            //  make a firing line for hit test
            Ray fireRay = Helper3D.ScreenPositionToRay(
                        rndPoint, proj, view, FrameworkCore.Game.GraphicsDevice);

            Vector3 start =
                    WorldTransform.Translation + (WorldTransform.Up * 3.0f);

            CollisionLayer targetColLayer = null;

            //  decide to target mech
            if (RobotGameGame.CurrentStage is VersusStageScreen)
            {
                if (this.PlayerIndex == PlayerIndex.One)
                    targetColLayer = colLayerVersusTeam[1];
                else if (this.PlayerIndex == PlayerIndex.Two)
                    targetColLayer = colLayerVersusTeam[0];
            }
            else
            {
                targetColLayer = colLayerEnemyMech;
            }

            //  Fire the weapon. Play a sound and particle
            CurrentWeapon.Fire(start,
                               fireRay.Direction,
                               CurrentWeapon.SpecData.FireRange,
                               ref targetColLayer,
                               ref colLayerHitWorld,
                               null, null);
        }

        /// <summary>
        /// changes to the next weapon.
        /// When there is no weapon to change, no action is taken.
        /// Player can own a default weapon and a sub weapon.
        /// </summary>
        /// <returns></returns>
        public bool SwapNextWeapon()
        {
            int nextIndex = -1;

            //  You've only one weapon
            if (weaponList.Count <= 1)
                return false;

            if (CurrentWeaponSlot + 1 < weaponList.Count)
                nextIndex = CurrentWeaponSlot + 1;
            else
                nextIndex = 0;

            return ChangeWeapon(nextIndex);
        }

        /// <summary>
        /// changes to the weapon.
        /// </summary>
        /// <returns></returns>
        public bool ChangeWeapon(int slotIndex)
        {
            //  Can not change to the weapon.
            if (weaponList.Count <= 1 || weaponList.Count <= slotIndex)
                return false;

            //  Changes to the weapon
            SelectWeapon(slotIndex);

            //  Update selected weapon image in the Hud
            RobotGameGame.CurrentStage.SetCurrentWeaponHud((int)this.PlayerIndex,
                                                        CurrentWeapon.WeaponType);

            return true;
        }
    }
}
