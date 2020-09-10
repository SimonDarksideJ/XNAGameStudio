#region File Description
//-----------------------------------------------------------------------------
// EnemyMech.cs
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
using RobotGameData;
using RobotGameData.AI;
using RobotGameData.Collision;
using RobotGameData.GameObject;
using RobotGameData.ParticleSystem;
using RobotGameData.Helper;
using RobotGameData.Sound;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It reads in the animation data for the robot type of enemy units and 
    /// processes all of the actions and A.I.
    /// The units that are up for the processing are 
    /// Cameleer, Maoming, Hammer, and Duskmas.
    /// </summary>
    public class EnemyMech : GameEnemy
    {
        #region Action Define

        /// <summary>
        /// defined mech's action
        /// </summary>
        public enum Action
        {
            Unknown = 0,
            Idle,
            Damage,
            Fire,
            Melee,
            Reload,
            ForwardWalk,
            BackwardWalk,
            LeftWalk,
            LeftTurn,
            RightWalk,
            RightTurn,

            ForwardDead,
            BackwardDead,
            LeftDead,
            RightDead,

            Count
        }

        #endregion

        #region Fields
                
        float defaultAnimationScaleFactor = 1.0f;
        float fireDelayElapsedTime = 0.0f;
        float moveDurationTime = 0.0f;
        float moveElapsedTime = 0.0f;

        bool isCriticalDamaged = false;
        bool isMoveBlocked = false;

        protected Action currentAction = Action.Unknown;

        Action engageAction = Action.Unknown;
        bool isOverwriteAction = false;

        int[] indexAnimation = null;

        int indexLeftFireWeaponBone = -1;
        int indexRightFireWeaponBone = -1;

        #endregion

        #region Properties

        public Action CurrentAction
        {
            get { return currentAction; }
        }

        public bool IsCriticalDamaged
        {
            get { return isCriticalDamaged; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public EnemyMech(ref GameEnemySpec spec)
            : base(ref spec)
        {
            //  Load Animations
            if (spec.AnimationFolderPath.Length > 0)
                LoadAnimationData(spec.AnimationFolderPath);
        }

        /// <summary>
        /// initializes the model and action.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            PlayAction(Action.Idle);

            //  Find the weapon fire bone
            for (int i = 0; i < ModelData.model.Bones.Count; i++)
            {
                ModelBone bone = ModelData.model.Bones[i];
             
                switch (this.UnitType)
                {
                    case UnitTypeId.Cameleer:
                    case UnitTypeId.Duskmas:
                        {
                            if (bone.Name == "GunDummy")
                                indexRightFireWeaponBone = bone.Index;
                            else if (bone.Name == "GunDummy01")
                                indexLeftFireWeaponBone = bone.Index;
                        }
                        break;
                    case UnitTypeId.Tiger:
                        {
                            if (bone.Name == "GunDummy")
                                indexRightFireWeaponBone = bone.Index;
                        }
                        break;
                    case UnitTypeId.Maoming:
                    case UnitTypeId.Hammer:
                        {
                            if (bone.Name == "GunDummyRight")
                                indexRightFireWeaponBone = bone.Index;
                            else if (bone.Name == "GunDummyLeft")
                                indexLeftFireWeaponBone = bone.Index;
                        } 
                        break;
                }                
            }
        }


        /// <summary>
        /// reset members.
        /// restarts at the specified spawn position.
        /// </summary>
        protected override void OnReset()
        {
            base.OnReset();

            moveElapsedTime = 0.0f;
            fireDelayElapsedTime = 0.0f;

            isCriticalDamaged = false;
            isMoveBlocked = false;
            isOverwriteAction = false;
            engageAction = Action.Unknown;
                        
            Material.alpha = 1.0f;
            Material.diffuseColor = Color.White;
            Material.specularColor = Color.Black;
            Material.emissiveColor = Color.Black;
            Material.specularPower = 12.8f;
            Material.vertexColorEnabled = false;
        }

        #endregion

        /// <summary>
        /// processes the animation and action info.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void OnUpdate(GameTime gameTime)
        {
            isOverwriteAction = false;

            //  Apply animation
            if ((engageAction != Action.Unknown && engageAction != CurrentAction) ||
                isOverwriteAction)
            {
                PlayAction(engageAction);
            }

            ProcessAction(gameTime);

            base.OnUpdate(gameTime);
        }

        #region Load Data

        /// <summary>
        /// load all animations for action.
        /// </summary>
        /// <param name="path">animation folder without file name</param>
        public void LoadAnimationData(string path)
        {
            int animCount = (int)Action.Count;

            indexAnimation = new int[animCount];

            //  Load all animations
            indexAnimation[(int)Action.Idle] = 
                        AddAnimation(path + "Idle");

            indexAnimation[(int)Action.Damage] = 
                        AddAnimation(path + "Damage");

            indexAnimation[(int)Action.ForwardWalk] = 
                        AddAnimation(path + "WalkFront");

            indexAnimation[(int)Action.BackwardWalk] = 
                        AddAnimation(path + "WalkBack");

            indexAnimation[(int)Action.LeftWalk] = 
                        AddAnimation(path + "WalkLeft");

            indexAnimation[(int)Action.RightWalk] = 
                        AddAnimation(path + "WalkRight");

            indexAnimation[(int)Action.LeftTurn] = 
                        AddAnimation(path + "TurnLeft");

            indexAnimation[(int)Action.RightTurn] = 
                        AddAnimation(path + "TurnRight");

            indexAnimation[(int)Action.ForwardDead] = 
                        AddAnimation(path + "DeathFront");

            indexAnimation[(int)Action.BackwardDead] = 
                        AddAnimation(path + "DeathBack");

            indexAnimation[(int)Action.LeftDead] = 
                        AddAnimation(path + "DeathLeft");

            indexAnimation[(int)Action.RightDead] = 
                        AddAnimation(path + "DeathRight");

            if (this.UnitType == UnitTypeId.Duskmas || 
                this.UnitType == UnitTypeId.Hammer)
            {
                indexAnimation[(int)Action.Fire] = 
                        AddAnimation(path + "AttackFar");
                
                indexAnimation[(int)Action.Melee] = 
                        AddAnimation(path + "AttackNear");
            }

            moveDurationTime = 
                        GetAnimation(indexAnimation[(int)Action.ForwardWalk]).Duration;
        }

        #endregion

        #region Process Action

        /// <summary>
        /// processes the current action info.
        /// </summary>
        /// <param name="gameTime"></param>
        protected void ProcessAction(GameTime gameTime)
        {
            switch (CurrentAction)
            {
                //  Playing damage animation process
                case Action.Damage:
                    {
                        AnimationSequence animation = 
                                    GetAnimation(indexAnimation[(int)CurrentAction]);

                        if (this.actionElapsedTime >= animation.Duration)
                        {
                            //  Return to idle state
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
                case Action.Fire:
                case Action.Melee:
                    {
                        if (this.actionElapsedTime >= 
                                        CurrentWeapon.SpecData.FireIntervalTime)
                        {
                            //  Return to idle state
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
                case Action.Reload:
                    {
                        if (this.actionElapsedTime >= 
                                        CurrentWeapon.SpecData.ReloadIntervalTime)
                        {
                            //  Return to idle state
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
                //  Playing dead animation process
                case Action.ForwardDead:
                case Action.BackwardDead:
                case Action.LeftDead:
                case Action.RightDead:
                    {
                        AnimationSequence animation = 
                                    GetAnimation(indexAnimation[(int)CurrentAction]);

                        const float secondDestroyGap = 0.5f;

                        if (this.actionElapsedTime < 
                                    animation.Duration + secondDestroyGap)
                        {
                            if(this.actionElapsedTime >= animation.Duration - 0.5f)
                            {
                                ParticleType type = ParticleType.Count;
                                SoundTrack sound = SoundTrack.DestroyLightMech2;

                                Material.alpha = 
                                            ((animation.Duration + secondDestroyGap) - 
                                            this.actionElapsedTime) / secondDestroyGap;

                                switch(this.UnitClass)
                                {
                                    case UnitClassId.LightMech:
                                        {
                                            sound = SoundTrack.DestroyLightMech2;
                                            type = ParticleType.DestroyLightMech2;
                                        }
                                        break;
                                    case UnitClassId.HeavyMech:
                                        {
                                            sound = SoundTrack.DestroyHeavyMech2;
                                            type = ParticleType.DestroyHeavyMech2;
                                        }
                                        break;
                                };

                                //  Second destroy particle
                                if (GameSound.IsPlaying(soundDestroy2) == false)
                                {
                                    soundDestroy2 = GameSound.Play3D(sound, this);

                                    Matrix world = Matrix.CreateTranslation(
                                                            WorldTransform.Translation);
                                                                        
                                    GameParticle.PlayParticle(type, world, 
                                                                Matrix.Identity);
                                }                                
                            }
                        }

                        if (this.actionElapsedTime >= 
                                        animation.Duration + secondDestroyGap)
                        {
                            this.Enabled = false;
                            this.Visible = false;
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
        /// determines the animation according to velocity.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="vMoveVelocity"></param>
        public void ActionMovement(GameTime gameTime, Vector3 vMoveVelocity)
        {
            bool canMove = true;

            //  Forward
            if (vMoveVelocity.Z > 0.0f)
            {
                if (CurrentAction != Action.ForwardWalk)
                    engageAction = Action.ForwardWalk;
            }
            //  Backward
            else if (vMoveVelocity.Z < 0.0f)
            {
                if (CurrentAction != Action.BackwardWalk)
                    engageAction = Action.BackwardWalk;
            }
            else if (vMoveVelocity.X > 0.0f)
            {
                if (CurrentAction != Action.RightWalk)
                    engageAction = Action.RightWalk;
            }
            //  Backward
            else if (vMoveVelocity.X < 0.0f)
            {
                if (CurrentAction != Action.LeftWalk)
                    engageAction = Action.LeftWalk;
            }
            //  Move Stop
            else
            {
                canMove = false;

                ActionIdle();
            }

            SoundTrack moveSound = SoundTrack.CameleerWalk;
            switch (this.UnitType)
            {
                case UnitTypeId.Cameleer: moveSound = SoundTrack.CameleerWalk; break;
                case UnitTypeId.Maoming: moveSound = SoundTrack.MaomingWalk; break;
                case UnitTypeId.Duskmas: moveSound = SoundTrack.DuskmasWalk; break;
                case UnitTypeId.Tiger: moveSound = SoundTrack.TankMove; break;
                case UnitTypeId.Hammer: moveSound = SoundTrack.HammerWalk; break;
            }

            if (canMove)
            {
                //  Play a moving sound
                if (engageAction == Action.ForwardWalk || 
                    engageAction == Action.BackwardWalk ||
                    engageAction == Action.RightWalk || 
                    engageAction == Action.LeftWalk)
                {
                    if ((CurrentAction != Action.ForwardWalk && 
                        CurrentAction != Action.BackwardWalk &&
                        CurrentAction != Action.RightWalk && 
                        CurrentAction != Action.LeftWalk) || moveElapsedTime == 0.0f)
                    {
                        GameSound.Stop(soundMove);
                        soundMove = GameSound.Play3D(moveSound, this);

                        moveElapsedTime = 0.0f;
                    }

                    //  Calculate run animation playing time for run sound
                    if (moveDurationTime > moveElapsedTime)
                        moveElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    else
                        moveElapsedTime = 0.0f;
                }
            }
            else
            {
                if (GameSound.IsPlaying(soundMove))
                    GameSound.Stop(soundMove);
            }
        }

        /// <summary>
        /// takes a firing action.
        /// </summary>
        /// <returns></returns>
        public override bool ActionFire()
        {
            engageAction = Action.Fire;

            return true;
        }

        /// <summary>
        /// takes a melee attack action.
        /// </summary>
        /// <returns></returns>
        public override bool ActionMelee()
        {
            engageAction = Action.Melee;

            return true;
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

                engageAction = Action.Reload;

                return true;
            }

            return false;
        }

        /// <summary>
        /// takes the idle action.
        /// </summary>
        public override void ActionIdle()
        {
            if (GameSound.IsPlaying(soundMove))
                GameSound.Stop(soundMove);

            engageAction = Action.Idle;

            this.actionElapsedTime = 0.0f;
            this.fireDelayElapsedTime = 0.0f;
        }

        /// <summary>
        /// gets attacked by enemy. When HP is 0, gets destroyed.
        /// </summary>
        /// <param name="attacker"></param>
        public override void ActionHit(GameUnit attacker)
        {
            if (IsDead) return;

            if (attacker.CurrentWeapon == null)
                throw new InvalidOperationException("The harmer no have weapon");

            GameWeaponSpec AttackerWeaponSpec = attacker.CurrentWeapon.SpecData;
                        
            //  Superman mode (DEBUG)
            if (RobotGameGame.SinglePlayer.Mode == GamePlayer.DebugMode.Superman ||
                RobotGameGame.SinglePlayer.Mode == GamePlayer.DebugMode.God)
            {
                Life = 0;  // If player is superman mode, This enemy is must die ^^
            }
            else
            {
                //  Reduce our player's life point by harmer's weapon power
                Life -= AttackerWeaponSpec.Damage;
                if (Life < 0) Life = 0;
            }

            if (Life == 0)
            {
                //  mech is dead.
                ActionDead(attacker.WorldTransform.Translation);
            }
        }

        /// <summary>
        /// gets attacked by enemy. Takes a damaged action.
        /// </summary>
        /// <param name="attacker"></param>
        public override void ActionDamage(GameUnit attacker)
        {
            engageAction = Action.Damage;

            this.actionElapsedTime = 0.0f;
            this.fireDelayElapsedTime = 0.0f;
        }

        /// <summary>
        /// takes a turning action.
        /// Does not actually turn.
        /// </summary>
        /// <param name="TurnAngle">turning angle</param>
        public void ActionTurn(float turnAngle)
        {
            if (turnAngle < 0.0f)
            {
                if (engageAction == Action.Idle || engageAction == Action.LeftTurn)
                    engageAction = Action.RightTurn;
            }
            else
            {
                if (engageAction == Action.Idle || engageAction == Action.RightTurn)
                    engageAction = Action.LeftTurn;
            }

            this.actionElapsedTime = 0.0f;
        }

        /// <summary>
        /// takes a damaged action.
        /// </summary>
        /// <param name="attackerPosition">the position of attacker</param>
        public override void ActionDead(Vector3 attackerPosition)
        {
            ParticleType type = ParticleType.Count;

            Vector3 dir = Vector3.Normalize(attackerPosition - Position);

            float FrontDot = Vector3.Dot(Direction, dir);
            float RightDot = Vector3.Dot(Right, dir);

            if (FrontDot > 0.5f)
            {
                // Hit from front
                engageAction = Action.BackwardDead;
            }
            else if (FrontDot < -0.5f)
            {
                // Hit from back
                engageAction = Action.ForwardDead;
            }
            else if (RightDot >= 0.0f)
            {
                //  Hit from right
                engageAction = Action.LeftDead;
            }
            else if (RightDot < 0.0f)
            {
                //  Hit from left
                engageAction = Action.RightDead;
            }

            if (UnitClass == UnitClassId.LightMech)
            {
                soundDestroy1 = GameSound.Play3D(SoundTrack.DestroyLightMech1, this);
                type = ParticleType.DestroyLightMech1;
            }
            else if (UnitClass == UnitClassId.HeavyMech)
            {
                soundDestroy1 = GameSound.Play3D(SoundTrack.DestroyHeavyMech1, this);
                type = ParticleType.DestroyHeavyMech1;
            }

            Matrix world = Matrix.CreateTranslation(WorldTransform.Translation);

            GameParticle.PlayParticle(type, world, Matrix.Identity);

            //  Remove collision
            colLayerEnemyMech.RemoveCollide(this.Collide);
            colLayerAllMech.RemoveCollide(this.Collide);

            MoveStop();

            GameSound.Stop(soundMove);

            Material.alpha = 1.0f;
            Material.diffuseColor = Color.Black;
            Material.specularColor = Color.Black;
            Material.emissiveColor = Color.Black;
            Material.vertexColorEnabled = true;

            this.SourceBlend = Blend.SourceAlpha;
            this.DestinationBlend = Blend.InverseSourceAlpha;
            this.AlphaBlendEnable = true;
            this.ReferenceAlpha = 0;

            //  AI process stop
            AIContext.Enabled = false;
        }

        /// <summary>
        /// plays animation according to action.
        /// </summary>
        /// <param name="action">mech's action</param>
        public void PlayAction(Action action)
        {
            PlayAction(action, 0.0f);
        }

        /// <summary>
        /// plays animation according to action.
        /// </summary>
        /// <param name="action">mech's action</param>
        /// <param name="startTime">start time of animation</param>
        public void PlayAction(Action action, float startTime)
        {
            AnimPlayMode playMode = AnimPlayMode.Repeat;
            float blendTime = 0.0f;

            this.currentAction = action;

            switch (action)
            {
                case Action.Idle:
                    {
                        blendTime = 0.5f;
                    }
                    break;
                case Action.Fire:
                    {
                        if (this.UnitType == UnitTypeId.Duskmas || 
                            this.UnitType == UnitTypeId.Hammer)
                        {
                            playMode = AnimPlayMode.Once;
                            blendTime = 0.2f;
                        }
                        else
                        {
                            return;
                        }
                    }
                    break;
                case Action.Melee:
                    {
                        if (this.UnitType == UnitTypeId.Duskmas ||
                            this.UnitType == UnitTypeId.Hammer)
                        {
                            playMode = AnimPlayMode.Once;
                            blendTime = 0.2f;
                        }
                        else
                        {
                            return;
                        }
                    }
                    break;
                case Action.Damage:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                case Action.ForwardWalk:
                    {
                        blendTime = 0.3f;
                    }
                    break;
                case Action.BackwardWalk:
                    {
                        blendTime = 0.3f;
                    }
                    break;
                case Action.LeftWalk:
                    {
                        blendTime = 0.3f;
                    }
                    break;
                case Action.RightWalk:
                    {
                        blendTime = 0.3f;
                    }
                    break;
                case Action.LeftTurn:
                    {
                        blendTime = 0.5f;
                    }
                    break;
                case Action.RightTurn:
                    {
                        blendTime = 0.5f;
                    }
                    break;
                case Action.ForwardDead:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                case Action.BackwardDead:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                case Action.LeftDead:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                case Action.RightDead:
                    {
                        playMode = AnimPlayMode.Once;
                        blendTime = 0.2f;
                    }
                    break;
                case Action.Reload:
                    {
                        return;
                    }
            }

            PlayAnimation(indexAnimation[(int)action], startTime, blendTime, 
                            defaultAnimationScaleFactor, playMode);

            this.actionElapsedTime = 0.0f;
        }

        #endregion

        #region Precess A.I.

        /// <summary>
        /// mech's A.I. function.
        ///  checks whether there is an attack objective within shooting range.
        /// If there is, attacks, otherwise, moves.
        /// </summary>
        /// <param name="aiBase">current A.I.</param>
        /// <param name="gameTime"></param>
        public override void OnAISearchEvent(AIBase aiBase)
        {
            if (!aiBase.IsActive)
            {
                CollideSphere collideSphere = this.Collide as CollideSphere;
                CollideSphere playerSphere = 
                                RobotGameGame.SinglePlayer.Collide as CollideSphere;

                //  Searching Player inside fire range
                float distanceBetweenPlayer = Vector3.Distance(
                    RobotGameGame.SinglePlayer.Position, Position) - 
                    playerSphere.Radius;

                Vector3 targetDirection = Vector3.Normalize(
                    RobotGameGame.SinglePlayer.Position - this.Position);

                float dotRight = Vector3.Dot(Right, targetDirection);
                float dotDirection = Vector3.Dot(Direction, targetDirection);
                float angle = MathHelper.ToDegrees(dotRight);

                //  Forward area
                if (dotDirection > 0.0f)
                {
                    if (Math.Abs(angle) <= 5.0f)
                    {
                        //  Firing inside fire range
                        if (distanceBetweenPlayer <= CurrentWeapon.SpecData.FireRange)
                        {
                            SetNextAI(AIType.Attack, 0.5f);
                        }
                        // Trace to player
                        else
                        {
                            float movingTime = (float)HelperMath.Randomi(2) + 
                                HelperMath.RandomNormal();

                            Vector3 simulateAmount = new Vector3(
                                                    0.0f, 0.0f, SpecData.MoveSpeed) * 
                                                    movingTime;

                            simulateAmount = CalculateVelocity(simulateAmount);

                            Vector3 start = WorldTransform.Translation + 
                                            (WorldTransform.Up * 1.0f);

                            //  Test collision
                            CollisionResult result = HitTestWithWorld(start, Direction);

                            // clear moving way ?
                            if (result != null)
                            {
                                float moveDistance = SpecData.MoveSpeed * movingTime;

                                float resultDistance = result.distance - 
                                                        collideSphere.Radius;

                                if (resultDistance > 0.0f)
                                {
                                    if (resultDistance < moveDistance)
                                    {
                                        //  Recalculate moving time for move and stop
                                        movingTime *= resultDistance / moveDistance;

                                        isMoveBlocked = true;
                                    }
                                    else
                                    {
                                        isMoveBlocked = false;
                                    }
                                }
                                //  Don't move
                                else
                                {
                                    isMoveBlocked = true;
                                }
                            }
                            else
                            {
                                isMoveBlocked = false;                                
                            }

                            if (isMoveBlocked)
                            {
                                //  turn
                                if (dotRight > 1.0f)
                                {
                                    SetNextAI(AIType.TurnRight, 
                                        90.0f / SpecData.TurnAngle);
                                }
                                else
                                {
                                    SetNextAI(AIType.TurnLeft,
                                        90.0f / SpecData.TurnAngle);
                                }
                            }
                            else
                            {
                                SetNextAI(AIType.Move, movingTime);
                            }
                        }
                    }
                    //  Turning right 
                    else if (angle >= 5.0f)
                    {
                        SetNextAI(AIType.TurnRight, 
                            Math.Abs(angle) / SpecData.TurnAngle);
                    }
                    //  Turning left
                    else if (angle < -5.0f)
                    {
                        SetNextAI(AIType.TurnLeft,
                            Math.Abs(angle) / SpecData.TurnAngle);
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid processing.");
                    }
                }
                //  Behind area
                else
                {
                    //  turn
                    if (dotRight > 1.0f)
                        SetNextAI(AIType.TurnRight, 90.0f / SpecData.TurnAngle);
                    else
                        SetNextAI(AIType.TurnLeft, 90.0f / SpecData.TurnAngle);
                }

                if (this.CurrentAction != Action.Idle)
                    ActionIdle();

                MoveStop();
            }
        }
        
        /// <summary>
        /// mech's A.I. function.
        /// turns the body right.
        /// </summary>
        /// <param name="aiBase">current A.I.</param>
        /// <param name="gameTime"></param>
        public override void OnAITurnRightEvent(AIBase aiBase)
        {
            MoveStop();

            if (aiBase.IsActive)
            {
                float fTurnAngleSpeed = -SpecData.TurnAngle;

                //  Turning right
                Rotate(new Vector2(fTurnAngleSpeed, 0.0f));
                ActionTurn(fTurnAngleSpeed);
            }
            else
            {
                if (isMoveBlocked)
                {
                    CollideSphere collideSphere = Collide as CollideSphere;

                    float movingTime = 
                            (float)HelperMath.Randomi(2) + HelperMath.RandomNormal();

                    Vector3 simulateAmount = 
                                    new Vector3(0.0f, 0.0f, SpecData.MoveSpeed) * 
                                    movingTime;

                    simulateAmount = CalculateVelocity(simulateAmount);

                    Vector3 start = WorldTransform.Translation + 
                                    (WorldTransform.Up * 1.0f);

                    CollisionResult result = HitTestWithWorld(start, Direction);

                    // clear moving way?
                    if (result != null)
                    {
                        float moveDistance = SpecData.MoveSpeed * movingTime;
                        float resultDistance = result.distance - collideSphere.Radius;

                        if (resultDistance > 0.0f)
                        {
                            if (resultDistance < moveDistance)
                            {
                                //  Recalculate moving time for move and stop
                                movingTime *= resultDistance / moveDistance;

                                isMoveBlocked = true;
                            }
                            else
                            {
                                isMoveBlocked = false;
                            }
                        }
                        //  Don't move
                        else
                        {
                            isMoveBlocked = true;
                        }
                    }
                    else
                    {
                        isMoveBlocked = false;
                    }

                    if (isMoveBlocked)
                    {
                        //  turn right
                        SetNextAI(AIType.TurnRight, 90.0f / SpecData.TurnAngle);
                    }
                    else
                    {
                        SetNextAI(AIType.Move, movingTime);
                    }
                }
                else
                    SetNextAI(AIType.Search, HelperMath.RandomNormal());
            }
        }

        /// <summary>
        /// mech's A.I. function.
        /// turns the body left.
        /// </summary>
        /// <param name="aiBase">current A.I.</param>
        /// <param name="gameTime"></param>
        public override void OnAITurnLeftEvent(AIBase aiBase)
        {
            MoveStop();

            if (aiBase.IsActive)
            {
                float fTurnAngleSpeed = SpecData.TurnAngle;

                //  Turning left
                Rotate(new Vector2(fTurnAngleSpeed, 0.0f));
                ActionTurn(fTurnAngleSpeed);
            }
            else
            {
                if (isMoveBlocked)
                {
                    CollideSphere collideSphere = Collide as CollideSphere;

                    float movingTime = 
                            (float)HelperMath.Randomi(2) + HelperMath.RandomNormal();

                    Vector3 simulateAmount = 
                        new Vector3(0.0f, 0.0f, SpecData.MoveSpeed) * movingTime;

                    simulateAmount = CalculateVelocity(simulateAmount);

                    Vector3 start = 
                            WorldTransform.Translation + (WorldTransform.Up * 1.0f);

                    //  Test collision
                    CollisionResult result = HitTestWithWorld(start, Direction);

                    // clear moving way?
                    if (result != null)
                    {
                        float moveDistance = SpecData.MoveSpeed * movingTime;
                        float resultDistance = result.distance - collideSphere.Radius;

                        if (resultDistance > 0.0f)
                        {
                            if (resultDistance < moveDistance)
                            {
                                //  Recalculate moving time for move and stop
                                movingTime *= resultDistance / moveDistance;

                                isMoveBlocked = true;
                            }
                            else
                            {
                                isMoveBlocked = false;
                            }
                        }
                        //  Don't move
                        else
                        {
                            isMoveBlocked = true;
                        }
                    }
                    else
                    {
                        isMoveBlocked = false;
                    }

                    if (isMoveBlocked)
                    {
                        //  turn right
                        SetNextAI(AIType.TurnLeft, 90.0f / SpecData.TurnAngle);
                    }
                    else
                    {
                        SetNextAI(AIType.Move, movingTime);
                    }
                }
                else
                    SetNextAI(AIType.Search, HelperMath.RandomNormal());
            }
        }

        /// <summary>
        /// mech's AI function.
        /// moves to the position and stops when collided with others.
        /// </summary>
        /// <param name="aiBase">current A.I.</param>
        /// <param name="gameTime"></param>
        public override void OnAIMoveEvent(AIBase aiBase, GameTime gameTime)
        {
            if (aiBase.IsActive)
            {
                Vector3 moveVelocity = new Vector3(0.0f, 0.0f, SpecData.MoveSpeed);

                Vector3 simulateAmount = 
                            CalculateVelocityPerFrame(gameTime, moveVelocity * 2.0f);

                //  Check moving area
                CollisionResult result = MoveHitTestWithMech(simulateAmount);

                //  If moving way is blocked
                if (result != null)
                {
                    MoveStop();

                    Vector3 moveAt = GetMoveAt(this.Velocity);
                    Vector3 oppositeDirection = Vector3.Negate(moveAt);

                    //  Move to reverse direction
                    AddPosition(oppositeDirection * Math.Abs(result.distance));

                    //  Crash with player, and attacking
                    if (result.detectedCollide.Owner is GamePlayer)
                    {
                        SetNextAI(AIType.Attack, 0.5f);                        
                    }
                    else
                    {
                        //  turn to right
                        SetNextAI(AIType.TurnRight, 90.0f / SpecData.TurnAngle);
                    }
                }
                else
                {
                    //  Don't move by enemy's weapon damage
                    if (IsCriticalDamaged)
                        MoveStop();
                    else
                        Move(moveVelocity);
                }
            }
            else
            {
                MoveStop();

                //  If moving way is blocked
                if (isMoveBlocked)
                {
                    //  turn to right
                    SetNextAI(AIType.TurnRight, 90.0f / SpecData.TurnAngle);
                }
                else
                {
                    SetNextAI(AIType.Search, HelperMath.RandomNormal());
                }
            }

            ActionMovement(gameTime, Velocity);
        }

        /// <summary>
        /// mech's A.I. function.
        /// attacks an attack objective if it is within shooting range.
        /// If not, searches for an attack objective again.
        /// </summary>
        /// <param name="aiBase">current A.I.</param>
        /// <param name="gameTime"></param>
        public override void OnAIAttackEvent(AIBase aiBase, GameTime gameTime)
        {
            MoveStop();

            //  The enmey weapon reloading
            if (CurrentWeapon.CurrentAmmo == 0 && !IsFiring && !IsReloading)
            {
                ActionReload(CurrentWeapon);
            }
            else if (CurrentWeapon.IsPossibleToFire())
            {
                //  Attack action
                if (currentAction == Action.Fire)
                {
                    //  Waiting for ready to first fire
                    if (this.fireDelayElapsedTime >=
                                    CurrentWeapon.SpecData.FireDelayTimeTillFirst)
                    {
                        this.fireDelayElapsedTime = 0.0f;

                        WeaponFire();
                    }
                    else
                    {
                        this.fireDelayElapsedTime +=
                                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                }
                else
                {
                    ActionFire();
                }
            }

            if (!aiBase.IsActive && currentAction != Action.Fire)
            {
                SetNextAI(AIType.Search, HelperMath.RandomNormal());
            }
        }

        #endregion

        /// <summary>
        /// fires forward.
        /// There is a divergence of a specified range around the shooting angle.
        /// </summary>
        public override void WeaponFire()
        {
            int weaponCount = 1;
            Vector3 start = Vector3.Zero;
            Vector3 direction = Vector3.Zero;

            //  Calculate fire and target info
            if (CurrentWeapon.WeaponType == WeaponType.CameleerGun ||
                CurrentWeapon.WeaponType == WeaponType.MaomingGun ||
                CurrentWeapon.WeaponType == WeaponType.DuskmasCannon ||
                CurrentWeapon.WeaponType == WeaponType.HammerCannon ||
                CurrentWeapon.WeaponType == WeaponType.TigerCannon)
            {
                int fireHorizontalTiltAngle =
                                CurrentWeapon.SpecData.FireHorizontalTiltAngle;

                float fireHorizontal = MathHelper.ToRadians(
                                                    (float)HelperMath.Randomi(
                                                    -fireHorizontalTiltAngle,
                                                    fireHorizontalTiltAngle));

                int fireVerticalTiltAngle =
                                    CurrentWeapon.SpecData.FireVerticalTiltAngle;

                float fireVertical = MathHelper.ToRadians(
                                                    (float)HelperMath.Randomi(
                                                     -fireVerticalTiltAngle,
                                                     fireVerticalTiltAngle));

                start = WorldTransform.Translation + (WorldTransform.Up * 2.0f);

                Matrix RndMatrix =
                            Matrix.CreateFromAxisAngle(Up, fireHorizontal) *
                            Matrix.CreateFromAxisAngle(Right, fireVertical) *
                            WorldTransform;

                direction = RndMatrix.Forward;
            }
            else if (CurrentWeapon.WeaponType == WeaponType.PhantomMelee)
            {
                start = WorldTransform.Translation + (WorldTransform.Up * 2.0f);

                Matrix rot = Matrix.CreateFromAxisAngle(
                             WorldTransform.Right,
                             MathHelper.ToRadians(-30.0f));

                direction = Vector3.Transform(start, rot);
            }

            switch (this.UnitType)
            {
                case UnitTypeId.Cameleer:
                case UnitTypeId.Hammer:
                    weaponCount = 2;
                    break;
                case UnitTypeId.Maoming:
                case UnitTypeId.Duskmas:
                case UnitTypeId.Tiger:
                    weaponCount = 1;
                    break;
            }

            Matrix? fireFxBone1 = null, fireFxBone2 = null;

            for (int i = 0; i < weaponCount; i++)
            {
                if (i == 0)
                {
                    Matrix fireMatrix = TransformedMatrix;

                    fireMatrix.Translation = 
                                BoneTransforms[indexRightFireWeaponBone].Translation;

                    fireFxBone1 = fireMatrix;
                }
                else if (i == 1)
                {
                    Matrix fireMatrix = TransformedMatrix;

                    fireMatrix.Translation =
                                BoneTransforms[indexLeftFireWeaponBone].Translation;

                    fireFxBone2 = fireMatrix;
                }                
            }

            //  The weapon firing. Play the sound and particle
            CurrentWeapon.Fire(start, direction,
                               CurrentWeapon.SpecData.FireRange,
                               ref colLayerFriendlyMech,
                               ref colLayerHitWorld,
                               fireFxBone1, fireFxBone2);
        }
    }
}
