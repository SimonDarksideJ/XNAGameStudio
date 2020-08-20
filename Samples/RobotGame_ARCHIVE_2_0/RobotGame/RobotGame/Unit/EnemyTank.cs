#region File Description
//-----------------------------------------------------------------------------
// EnemyTank.cs
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData;
using RobotGameData.AI;
using RobotGameData.Collision;
using RobotGameData.Helper;
using RobotGameData.Render;
using RobotGameData.Sound;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It reads in the animation data for the tank type of enemy units and 
    /// processes all of the actions and A.I.
    /// The unit that is up for this processing is Tiger.
    /// </summary>
    public class EnemyTank : GameEnemy
    {
        #region Action Define

        /// <summary>
        /// defined tank's action
        /// </summary>
        public enum Action
        {
            Unknown = 0,
            Idle,
            Damage,
            Move,
            Fire,
            Reload,
            LeftTurn,
            RightTurn,
            Dead,

            Count
        }

        #endregion

        #region Fields

        protected Action currentAction = Action.Unknown;

        Action engageAction = Action.Unknown;
        bool isOverwriteAction = false;

        int indexFireWeaponBone = -1;
        int indexTurretBone = -1;
        Matrix transformTurret = Matrix.Identity;
        float turretAngleSpeed = 0.0f;

        #endregion

        #region Properties

        public Action CurrentAction
        {
            get { return currentAction; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public EnemyTank(ref GameEnemySpec spec)
            : base(ref spec) {}

        /// <summary>
        /// initializes the model and action.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            for (int i = 0; i < ModelData.model.Bones.Count; i++)
            {
                ModelBone bone = ModelData.model.Bones[i];

                if (bone.Name == "GunDummy")
                    indexFireWeaponBone = bone.Index;
            }

            indexTurretBone = ModelData.model.Bones["Top"].Index;
            transformTurret = ModelData.boneTransforms[indexTurretBone];

            PlayAction(Action.Idle);
        }

        /// <summary>
        /// reset members.
        /// restarts at the specified spawn position.
        /// </summary>
        protected override void OnReset()
        {
            base.OnReset();

            // Reset the turret's transform
            turretAngleSpeed = 0.0f;
            transformTurret = ModelData.boneTransforms[indexTurretBone];
            ModelData.model.Bones["Top"].Transform = transformTurret;
       
            //  Reset the material
            Material.alpha = 1.0f;
            Material.diffuseColor = Color.White;
            Material.specularColor = Color.Black;
            Material.emissiveColor = Color.Black;
            Material.specularPower = 12.8f;
            Material.vertexColorEnabled = false;

            // Default the Action
            PlayAction(Action.Idle);
        }

        /// <summary>
        /// processes the animation and action info.
        /// Turns the turret model at the specified turret turn angle.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void OnUpdate(GameTime gameTime)
        {
            isOverwriteAction = false;

            ProcessAction(gameTime);

            //  Apply animation
            if ((engageAction != Action.Unknown && engageAction != CurrentAction) ||
                isOverwriteAction)
            {
                PlayAction(engageAction);
            }

            base.OnUpdate(gameTime);

            if (turretAngleSpeed != 0.0f)
            {
                float rotationbyFrame = 
                        turretAngleSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                transformTurret *= Matrix.CreateFromAxisAngle(transformTurret.Right, 
                    MathHelper.ToRadians(rotationbyFrame));
            }

            ModelData.model.Bones[indexTurretBone].Transform = transformTurret;
            ModelData.model.CopyAbsoluteBoneTransformsTo(BoneTransforms);
        }

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
                        //  Return to idle state
                        ActionIdle();

                        this.actionElapsedTime = 0.0f;
                    }
                    break;
                case Action.Fire:
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
                case Action.Dead:
                    {
                        const float duration = 1.0f;
                        const float secondDestroyGap = 1.0f;

                        if (this.actionElapsedTime < duration + secondDestroyGap)
                        {
                            if (this.actionElapsedTime >= secondDestroyGap)
                            {
                                Material.alpha = 
                                        ((duration + secondDestroyGap) - 
                                        this.actionElapsedTime) / secondDestroyGap;

                                //  Second destroy particle
                                if (GameSound.IsPlaying(soundDestroy2) == false)
                                {
                                    soundDestroy2 = GameSound.Play3D(
                                                SoundTrack.DestroyHeavyMech2, this);

                                    Matrix world = Matrix.CreateTranslation(
                                                        WorldTransform.Translation);

                                    GameParticle.PlayParticle(ParticleType.DestroyTank2,
                                                              world, Matrix.Identity);
                                }
                            }
                        }

                        if (this.actionElapsedTime >= duration + secondDestroyGap)
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
        /// processes the idle info.
        /// </summary>
        public override void ActionIdle()
        {
            engageAction = Action.Idle;
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

            //  Superman mode (DEBUG)
            if (RobotGameGame.SinglePlayer.Mode == GamePlayer.DebugMode.Superman ||
                RobotGameGame.SinglePlayer.Mode == GamePlayer.DebugMode.God)
            {
                Life = 0;  // If player is superman mode, This unit must be die ^^
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
            }
        }

        /// <summary>
        /// attacked by enemy. takes the damage action.
        /// </summary>
        /// <param name="attacker"></param>
        public override void ActionDamage(GameUnit attacker)
        {
            engageAction = Action.Damage;

            this.actionElapsedTime = 0.0f;
        }

        /// <summary>
        /// takes the destruction action.
        /// </summary>
        /// <param name="attackerPosition">the position of attacker</param>
        public override void ActionDead(Vector3 attackerPosition)
        {
            engageAction = Action.Dead;

            MoveStop();

            //  Remove collision
            colLayerEnemyMech.RemoveCollide(this.Collide);
            colLayerAllMech.RemoveCollide(this.Collide);

            GameSound.Stop(soundMove);
            soundDestroy1 = GameSound.Play3D(SoundTrack.DestroyHeavyMech1, this);

            Matrix world = Matrix.CreateTranslation(WorldTransform.Translation);

            GameParticle.PlayParticle(ParticleType.DestroyTank1, world, 
                Matrix.Identity);

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

            turretAngleSpeed = 0.0f;
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
        /// plays animation according to action.
        /// </summary>
        /// <param name="action">tank's action</param>
        public void PlayAction(Action action)
        {
            currentAction = action;
        }

        #endregion

        #region Precess A.I.

        /// <summary>
        /// tank's A.I. function.
        /// checks whether an attack objective is within shooting range.
        /// if it is within shooting range, attacks.  Otherwise, moves.
        /// </summary>
        /// <param name="aiBase">current A.I.</param>
        /// <param name="gameTime"></param>
        public override void OnAISearchEvent(AIBase aiBase)
        {
            if (!aiBase.IsActive)
            {
                //  Searching Player inside fire range
                float distanceBetweenPlayer = 
                    Vector3.Distance(RobotGameGame.SinglePlayer.Position, Position);

                Vector3 targetDirection = 
                    Vector3.Normalize(RobotGameGame.SinglePlayer.Position - Position);

                Vector3 turretDirection = BoneTransforms[indexTurretBone].Forward;

                float dot = Vector3.Dot(turretDirection, targetDirection);
                float angleDiff = MathHelper.ToDegrees(dot);

                if (Math.Abs(angleDiff) < 3.0f)
                {
                    //  Firing inside fire range
                    if (distanceBetweenPlayer <= CurrentWeapon.SpecData.FireRange)
                    {
                        SetNextAI(AIType.Attack, 1.0f);
                    }
                    else
                    {
                        SetNextAI(AIType.Search, 1.0f);
                    }
                }
                else
                {
                    //  Turning right 
                    if (angleDiff > 3.0f)
                    {
                        SetNextAI(AIType.TurnRight, 
                            Math.Abs(angleDiff) / SpecData.TurnAngle);
                    }
                    //  Turning left
                    else if (angleDiff < -3.0f)
                    {
                        SetNextAI(AIType.TurnLeft, 
                            Math.Abs(angleDiff) / SpecData.TurnAngle);
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid processing");
                    }
                }

                turretAngleSpeed = 0.0f;
            }

            MoveStop();
        }

        /// <summary>
        /// tank's A.I. function.
        /// turns the body right.
        /// </summary>
        /// <param name="aiBase">current A.I.</param>
        /// <param name="gameTime"></param>
        public override void OnAITurnRightEvent(AIBase aiBase)
        {
            if (aiBase.IsActive)
            {
                turretAngleSpeed = -SpecData.TurnAngle;
            }
            else
            {
                SetNextAI(AIType.Search, HelperMath.RandomNormal());
            }
        }

        /// <summary>
        /// tank's A.I. function.
        /// turns the body left.
        /// </summary>
        /// <param name="aiBase">current A.I.</param>
        /// <param name="gameTime"></param>
        public override void OnAITurnLeftEvent(AIBase aiBase)
        {
            if (aiBase.IsActive)
            {
                turretAngleSpeed = SpecData.TurnAngle;
            }
            else
            {
                SetNextAI(AIType.Search, HelperMath.RandomNormal());
            }
        }

        /// <summary>
        /// tank's A.I. function.
        /// moves to the position and stops when collides with others.
        /// </summary>
        /// <param name="aiBase">current A.I.</param>
        /// <param name="gameTime"></param>
        public override void OnAIMoveEvent(AIBase aiBase, GameTime gameTime)
        {
            if (aiBase.IsActive)
            {
                Vector3 vMoveVelocity = new Vector3(0.0f, 0.0f, SpecData.MoveSpeed);

                CollisionResult result = MoveHitTest(gameTime, vMoveVelocity);
                if (result != null)
                {
                    if (GameSound.IsPlaying(soundMove))
                        GameSound.Stop(soundMove);

                    MoveStop();     //  Cannot move                   
                }
                else
                {
                    Move(vMoveVelocity);

                    if (!GameSound.IsPlaying(soundMove))
                        soundMove = GameSound.Play3D(SoundTrack.TankMove, this);
                }
            }
            else
            {
                if (GameSound.IsPlaying(soundMove))
                    GameSound.Stop(soundMove);

                MoveStop();

                turretAngleSpeed = 0.0f;

                SetNextAI(AIType.Search, HelperMath.RandomNormal());
            }
        }

        /// <summary>
        /// tank's A.I. function.
        /// attacks when the attack objective is within the shooting range.
        /// Otherwise, it searches for an attack object again.
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
                //  If weapon fired, just action fire
                ActionFire();

                WeaponFire();
            }

            turretAngleSpeed = 0.0f;
            SetNextAI(AIType.Search, HelperMath.RandomNormal());
        }

        #endregion

        /// <summary>
        /// fires forward.
        /// There is a divergence of a specified range around the shooting angle.
        /// </summary>
        public override void WeaponFire()
        {
            Vector3 fireStart = BoneTransforms[indexTurretBone].Translation;
            Vector3 fireDirection = BoneTransforms[indexTurretBone].Up;

            //  The weapon firing. Play a sound and particle
            CurrentWeapon.Fire(fireStart, fireDirection,
                               CurrentWeapon.SpecData.FireRange,
                               ref colLayerFriendlyMech,
                               ref colLayerHitWorld,
                               BoneTransforms[indexFireWeaponBone], null);
        }
    }
}
