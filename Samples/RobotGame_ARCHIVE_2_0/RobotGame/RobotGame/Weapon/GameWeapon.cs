#region File Description
//-----------------------------------------------------------------------------
// GameWeapon.cs
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
using RobotGameData.GameObject;
using RobotGameData.GameInterface;
using RobotGameData.Helper;
using RobotGameData.Render;
using RobotGameData.Collision;
using RobotGameData.Sound;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It defines the common actions for weapons and process the states.
    /// The weapon’s owner is changeable and 
    /// the interfaces for picking up and dropping are provided.
    /// It contains GameWeaponSpec, which is the weapon’s information class.
    /// </summary>
    public class GameWeapon : GameSceneNode
    {
        #region Action Define

        /// <summary>
        /// state for weapon.
        /// </summary>
        public enum WeaponState
        {
            Ready = 0,
            Firing,
            Reloading,

            Count
        }
        #endregion

        #region Fields

        int remainAmmo = 0;
        int currentAmmo = 0;
        int fireCount = 0;
        float actionElapsedTime = 0.0f;
        float rotateAngleAccm = 0.0f;
        float rotateAnglePerSecond = 40.0f;
        int dummySwichingIndex = 0;

        bool isDropped = false;

        GameWeaponSpec specData = null;
        WeaponState state = WeaponState.Ready;
        CollideElement modelCollide = null;

        int[] indexWeaponFireDummy = null;
        int[] indexWeaponAttachDummy = null;

        Cue soundFire = null;
        Cue soundReload = null;

        public GameModel[] modelWeapon = null;        

        #endregion

        #region Properties

        public GameWeaponSpec SpecData
        {
            get { return this.specData; }
        }

        public WeaponState State
        {
            get { return this.state; }
        }

        public WeaponType WeaponType
        {
            get { return specData.Type; }
        }

        public bool InWorld
        {
            get { return (this.isDropped); }
        }

        public bool InPlayer
        {
            get { return (this.Owner is GamePlayer); }
        }

        public GameModel DroppedModel
        {
            get { return modelWeapon[0]; }
        }

        public NodeBase Owner
        {
            get { return this.Parent; }
        }

        public GameUnit OwnerUnit
        {
            get { return (this.Owner as GameUnit); }
        }

        public int CurrentAmmo
        {
            get { return this.currentAmmo; }
            set { this.currentAmmo = value; }
        }

        public int RemainAmmo
        {
            get { return this.remainAmmo; }
            set { this.remainAmmo = value; }
        }

        public bool NeedToReload
        {
            get { return (this.CurrentAmmo == 0); }
        }

        public CollideElement Collide
        {
            get { return modelCollide; }
            protected set { modelCollide = value; }
        }

        public Matrix LeftFireBone
        {
            get
            {
                int boneIdx = indexWeaponFireDummy[0];
                return modelWeapon[0].BoneTransforms[boneIdx];
            }
        }

        public Matrix RightFireBone
        {
            get
            {
                int boneIdx = indexWeaponFireDummy[1];
                return modelWeapon[1].BoneTransforms[boneIdx];
            }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameWeapon(GameWeaponSpec spec)
        {
            this.specData = spec;

            //  model
            if (spec.ModelAlone )
            {
                modelWeapon = new GameModel[spec.ModelCount];
                indexWeaponFireDummy = new int[spec.ModelCount];
                indexWeaponAttachDummy = new int[spec.ModelCount];

                //  Find bones
                for (int i = 0; i < spec.ModelCount; i++ )
                {
                    modelWeapon[i] = new GameModel(spec.ModelFilePath);
                    modelWeapon[i].Name = spec.ModelFilePath;

                    for (int j = 0; j < modelWeapon[i].ModelData.model.Bones.Count; j++)
                    {
                        ModelBone bone = modelWeapon[i].ModelData.model.Bones[j];

                        //  Gun muzzule bone
                        if (bone.Name == spec.MuzzleBone)
                            indexWeaponFireDummy[i] = bone.Index;

                        //  Gun attach bone to character
                        if (bone.Name == spec.AttachBone)
                            indexWeaponAttachDummy[i] = bone.Index;
                    }
                }

                //  Apply NormalMap and SpeculaMap to only player's machine gun
                if (this.WeaponType == WeaponType.PlayerMachineGun)
                {
                    for (int i = 0; i < spec.ModelCount; i++)
                    {
                        modelWeapon[i].RenderingCustomEffect +=
                            new EventHandler<GameModel.RenderingCustomEffectEventArgs>(
                            OnEffectProcess);
                    }
                }
            }

            Reset();
        }

        /// <summary>
        /// set all effect parameters.
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

        /// <summary>
        /// sticks to the owner as a child.
        /// Will depend on owner as a child.
        /// </summary>
        /// <param name="owner"></param>
        public void AttachOwner(NodeBase owner)
        {
            DetachOwner();

            if (specData.ModelAlone )
            {
                for (int i = 0; i < specData.ModelCount; i++)
                {
                    //  New parent is owner now
                    this.AddChild(modelWeapon[i]);

                    modelWeapon[i].SetRootAxis(Matrix.Identity);
                }
            }

            owner.AddChild(this);
        }

        /// <summary>
        /// detaches from owner.
        /// </summary>
        public void DetachOwner()
        {
            if (specData.ModelAlone )
            {
                for (int i = 0; i < specData.ModelCount; i++)
                    modelWeapon[i].RemoveFromParent();
            }

            this.RemoveFromParent();
        }

        /// <summary>
        /// resets the current state, remaining bullet, and other information.
        /// </summary>
        public void Reset()
        {
            this.remainAmmo = this.specData.TotalBulletCount;
            this.currentAmmo = this.specData.ReloadBulletCount;

            this.fireCount = 0;

            this.state = WeaponState.Ready;
        }
        
        /// <summary>
        /// processes the current state finfo.
        /// If dropped onto the world, only one model gets rotated.
        /// </summary>
        protected override void OnUpdate(GameTime gameTime)
        {
            //  In world ?
            if (InWorld )
            {
                //  Drawable weapon model is one
                GameModel droppedWeapon = this.modelWeapon[0];

                // will be enable the first weapon model.
                droppedWeapon.Enabled = true;
                droppedWeapon.Visible = true;

                //  moves first weapon model to new position
                droppedWeapon.Position = this.Position;

                //  disables the second weapon model.
                // 
                for (int i = 1; i < modelWeapon.Length; i++)
                {
                    this.modelWeapon[i].Enabled = false;
                    this.modelWeapon[i].Visible = false;
                } 

                rotateAngleAccm += rotateAnglePerSecond *
                               (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (rotateAngleAccm > 360.0f)
                    rotateAngleAccm = 0.0f;

                //  rotates weapon's model in the world.
                droppedWeapon.WorldTransform =
                        Matrix.CreateRotationY(MathHelper.ToRadians(rotateAngleAccm)) *
                        TransformedMatrix;

                //  updates collision.
                if (modelCollide != null)
                {
                    modelCollide.Transform(TransformedMatrix);
                }
            }

            switch (this.state)
            {
                //  ready now.
                case WeaponState.Ready:
                    {
                        this.actionElapsedTime = 0.0f;
                    }
                    break;
                //  firing now.
                case WeaponState.Firing:
                    {
                        //  Finished firing
                        if (this.specData.FireIntervalTime <= this.actionElapsedTime)
                        {
                            this.fireCount++;

                            if (this.fireCount >= this.specData.FireCount)
                            {
                                this.state = WeaponState.Ready;
                                this.fireCount = 0;
                            }
                            else
                            {
                                this.OwnerUnit.WeaponFire();
                            }

                            this.actionElapsedTime = 0.0f;
                        }
                        //  Firing now
                        else
                        {
                            this.actionElapsedTime += 
                                    (float)FrameworkCore.ElapsedDeltaTime.TotalSeconds;
                        }
                    }
                    break;
                //  reloading now.
                case WeaponState.Reloading:
                    {
                        //  Finished reloading
                        if (this.specData.ReloadIntervalTime <= this.actionElapsedTime)
                        {
                            //  Unlimited ammo
                            if (this.specData.TotalBulletCount == -1)
                            {
                                this.currentAmmo = specData.ReloadBulletCount;
                                this.remainAmmo = specData.ReloadBulletCount;
                            }
                            //  When the number of the total bullets is sufficient
                            if (this.specData.ReloadBulletCount - this.currentAmmo
                                                            <= this.remainAmmo)
                            {
                                int supplyAmmo = specData.ReloadBulletCount 
                                                    - this.currentAmmo;

                                this.remainAmmo -= supplyAmmo;
                                this.currentAmmo += supplyAmmo;
                            }
                            //  When the no. of the reload bullets is smaller than 
                            //  the no. of total bullets,
                            else
                            {
                                int supplyAmmo = this.remainAmmo;

                                this.remainAmmo -= supplyAmmo;
                                this.currentAmmo += supplyAmmo;
                            }                            
                                                        
                            this.state = WeaponState.Ready;
                            this.actionElapsedTime = 0.0f;
                        }
                        //  Reloading now
                        else
                        {
                            this.actionElapsedTime += 
                                (float)FrameworkCore.ElapsedDeltaTime.TotalSeconds;
                        }
                    }
                    break;
            }

            base.OnUpdate(gameTime);
        }

        /// <summary>
        /// checks for the collision at the aiming angle.  
        /// If it collides with an enemy mech, calls ActionHit() function.
        /// Weapon’s collision checks the world and enemy both.
        /// When playing firing particle, the number of the weapon’s model 
        /// must be considered.
        /// Since the player’s weapon is a dual weapon system, there are two models.  
        /// However, for enemies, there are enemies with single weapon system.
        /// Therefore, it plays firing particle at the gun point
        /// as many as the number of models.
        /// </summary>
        /// <param name="position">the start position of firing</param>
        /// <param name="direction">the direction of firing</param>
        /// <param name="distance">the range of firing</param>
        /// <param name="targetCollisionLayer">target collision layer</param>
        /// <param name="worldCollisionLayer">world collision layer</param>
        /// <param name="fireBone1">the fire matrix of first weapon</param>
        /// <param name="fireBone2">the fire matrix of second weapon</param>
        /// <returns></returns>
        public bool Fire(Vector3 position, Vector3 direction, float distance, 
                         ref CollisionLayer targetCollisionLayer,
                         ref CollisionLayer worldCollisionLayer,
                         Matrix? fireBone1, Matrix? fireBone2)
        {
            bool hit = false;

            Vector3 firePosition = Vector3.Zero;
            Vector3 targetPosition = position + (direction * distance);
            SoundTrack fireSound = SoundTrack.Count;
            ParticleType fireParticle = ParticleType.Count;
            ParticleType unitHitParticle = ParticleType.Count;
            ParticleType worldHitParticle = ParticleType.Count;

            Matrix fixedAxis = Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f));

            this.state = WeaponState.Firing;            

            if (this.currentAmmo <= 0)  return hit;

            //  Reduces a bullet.
            this.currentAmmo--;

            switch (this.WeaponType)
            {
                case WeaponType.PlayerMachineGun:
                    {
                        fireSound = SoundTrack.PlayerMachineGunFire;

                        fireParticle = ParticleType.PlayerMachineGunFire;
                        unitHitParticle = ParticleType.PlayerMachineGunUnitHit;
                        worldHitParticle = ParticleType.PlayerMachineGunWorldHit;
                    }
                    break;
                case WeaponType.PlayerShotgun:
                    {
                        fireSound = SoundTrack.PlayerShotgunFire;

                        fireParticle = ParticleType.PlayerShotgunFire;
                        unitHitParticle = ParticleType.PlayerShotgunUnitHit;
                        worldHitParticle = ParticleType.PlayerShotgunWorldHit;
                    }
                    break;
                case WeaponType.PlayerHandgun:
                    {
                        fireSound = SoundTrack.PlayerHandgunFire;

                        fireParticle = ParticleType.PlayerHandgunFire;
                        unitHitParticle = ParticleType.PlayerHandgunUnitHit;
                        worldHitParticle = ParticleType.PlayerHandgunWorldHit;
                    }
                    break;
                case WeaponType.CameleerGun:
                    {
                        fireSound = SoundTrack.CameleerFire;

                        fireParticle = ParticleType.EnemyGunFire;
                        unitHitParticle = ParticleType.EnemyGunUnitHit;
                        worldHitParticle = ParticleType.EnemyGunWorldHit;
                    }
                    break;
                case WeaponType.MaomingGun:
                    {
                        fireSound = SoundTrack.MaomingFire;

                        fireParticle = ParticleType.PlayerMachineGunFire;
                        unitHitParticle = ParticleType.EnemyGunUnitHit;
                        worldHitParticle = ParticleType.EnemyGunWorldHit;
                    }
                    break;
                case WeaponType.DuskmasCannon:
                    {
                        fireSound = SoundTrack.DuskmasFire;

                        fireParticle = ParticleType.EnemyCannonFire;
                        unitHitParticle = ParticleType.EnemyCannonUnitHit;
                        worldHitParticle = ParticleType.EnemyCannonWorldHit;
                    }
                    break;
                case WeaponType.TigerCannon:
                    {
                        fireSound = SoundTrack.TankFire;
                        fireParticle = ParticleType.EnemyCannonFire;
                        unitHitParticle = ParticleType.EnemyCannonUnitHit;
                        worldHitParticle = ParticleType.EnemyCannonWorldHit;
                    }
                    break;
                case WeaponType.HammerCannon:
                    {
                        fireSound = SoundTrack.HammerFire;

                        fireParticle = ParticleType.EnemyCannonFire;
                        unitHitParticle = ParticleType.EnemyCannonUnitHit;
                        worldHitParticle = ParticleType.EnemyCannonWorldHit;
                    }
                    break;
                case WeaponType.PhantomMelee:
                    {
                        fireSound = SoundTrack.BossMelee;

                        fireParticle = ParticleType.Count;
                        unitHitParticle = ParticleType.EnemyMeleeUnitHit;
                        worldHitParticle = ParticleType.EnemyCannonWorldHit;
                    }
                    break;
            }
                
            if (this.WeaponType != WeaponType.PlayerShotgun &&
                this.WeaponType != WeaponType.PlayerHandgun)
            {
                StopFireSound();
            }

            //  Play a weapon firing sound
            if (RobotGameGame.CurrentGameLevel.Info.GamePlayType ==
                GamePlayTypeId.Versus)
            {
                soundFire = GameSound.Play3D(fireSound, 
                    RobotGameGame.SinglePlayer.Emitter);
            }
            else
            {
                soundFire = GameSound.Play3D(fireSound, this.OwnerUnit.Emitter);
            }

            //  Play firing particles
            if (specData.ModelAlone )
            {
                //  Multi fire
                if (this.SpecData.FireCount == 1)
                {
                    for (int i = 0; i < SpecData.ModelCount; i++)
                    {
                        int boneIdx = indexWeaponFireDummy[i];
                        Matrix boneTransform = modelWeapon[i].BoneTransforms[boneIdx];

                        GameParticle.PlayParticle(fireParticle, boneTransform,
                            fixedAxis);
                    }

                    //  In case of two handed weapons, the index is changed 
                    //  so that the tracer bullet will fire alternatively.
                    if (dummySwichingIndex == 0)
                        dummySwichingIndex = 1;
                    else 
                        dummySwichingIndex = 0;

                    int boneIndex = indexWeaponFireDummy[dummySwichingIndex];
                    
                    firePosition =
                        modelWeapon[dummySwichingIndex].BoneTransforms[
                            boneIndex].Translation;
                }
                //  Delayed fire
                else
                {
                    if (this.fireCount == 0)
                    {
                        GameParticle.PlayParticle(fireParticle, this.RightFireBone, 
                            fixedAxis);
                        firePosition = this.RightFireBone.Translation;
                    }
                    else if (this.fireCount == 1)
                    {
                        GameParticle.PlayParticle(fireParticle, this.LeftFireBone,
                            fixedAxis);
                        firePosition = this.LeftFireBone.Translation;
                    }
                }
            }
            else
            {
                Matrix fireMatrix = Matrix.Identity;

                if (fireBone1 != null)
                    GameParticle.PlayParticle(fireParticle, (Matrix)fireBone1,
                        fixedAxis);

                if (fireBone2 != null)
                    GameParticle.PlayParticle(fireParticle, (Matrix)fireBone2, 
                        fixedAxis);

                if (fireBone1 != null && fireBone2 != null)
                {
                    //  In case of two handed weapons, the index is changed 
                    //  so that the tracer bullet will fire alternatively.
                    if (dummySwichingIndex == 0)
                    {
                        fireMatrix = (Matrix)fireBone1;
                        dummySwichingIndex = 1;
                    }
                    else
                    {
                        fireMatrix = (Matrix)fireBone2;
                        dummySwichingIndex = 0;
                    }   
                }    
                else if( fireBone1 != null)
                    fireMatrix = (Matrix)fireBone1;
                else if (fireBone2 != null)
                    fireMatrix = (Matrix)fireBone2;

                firePosition = fireMatrix.Translation;
            }

            //  Hit testing
            CollisionResult collideResult = FireHitTest(position, direction, distance,
                                                 ref targetCollisionLayer,
                                                 ref worldCollisionLayer);

            if (collideResult != null)
            {
                //  Play hitting particle
                {
                    ParticleType hitParticle = ParticleType.Count;

                    //  To player
                    if (collideResult.detectedCollide.Owner is GameUnit)
                    {
                        GameUnit detectGameUnit = 
                            collideResult.detectedCollide.Owner as GameUnit;

                        // Calculate a random intersect point for 
                        // hitting particle in unit sphere
                        CollideSphere sphere =
                            collideResult.detectedCollide as CollideSphere;

                        switch (this.WeaponType)
                        {
                            case WeaponType.PlayerMachineGun:
                            case WeaponType.PlayerShotgun:
                            case WeaponType.PlayerHandgun:
                            case WeaponType.CameleerGun:
                            case WeaponType.MaomingGun:
                                {
                                    Vector3 dir = this.OwnerUnit.Position - 
                                                  detectGameUnit.Position;

                                    dir.Normalize();
                                    dir.X += HelperMath.RandomNormal2();
                                    dir.Y += HelperMath.RandomNormal2();
                                    dir.Normalize();

                                    collideResult.normal = (Vector3)dir;
                                    collideResult.intersect = 
                                        sphere.BoundingSphere.Center +
                                        ((Vector3)dir * sphere.Radius);
                                }
                                break;
                            case WeaponType.DuskmasCannon:
                            case WeaponType.HammerCannon:
                            case WeaponType.TigerCannon:
                            case WeaponType.PhantomMelee:
                                {
                                    Vector3 dir = this.OwnerUnit.Position -
                                                  sphere.BoundingSphere.Center;

                                    dir.Normalize();

                                    collideResult.normal = (Vector3)dir;
                                    collideResult.intersect = 
                                        sphere.BoundingSphere.Center;
                                }
                                break;
                        }

                        hitParticle = unitHitParticle;
                        targetPosition = (Vector3)collideResult.intersect;
                    }
                    //  To world
                    else
                    {
                        hitParticle = worldHitParticle;
                        targetPosition = (Vector3)collideResult.intersect;
                    }

                    if (collideResult.normal != null)
                    {
                        GameParticle.PlayParticle(hitParticle,
                                 (Vector3)collideResult.intersect, 
                                 (Vector3)collideResult.normal,
                                 Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)));
                    }
                    else
                    {
                        GameParticle.PlayParticle(hitParticle,
                            Matrix.CreateTranslation((Vector3)collideResult.intersect),
                            Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)));
                    }
                }

                //  Hit to other mech
                if (collideResult.detectedCollide.Owner is GameUnit)
                {
                    GameUnit HitUnit = (GameUnit)collideResult.detectedCollide.Owner;

                    //  Call hit function to unit
                    HitUnit.ActionHit(this.OwnerUnit);

                    if (HitUnit.IsDead )
                    {
                        if (this.OwnerUnit is GamePlayer)
                        {
                            //  If the versus mode, you'll be get kill point
                            if( RobotGameGame.CurrentStage is VersusStageScreen)
                            {
                                VersusStageScreen stage = 
                                    RobotGameGame.CurrentStage as VersusStageScreen;

                                GamePlayer player = this.OwnerUnit as GamePlayer;

                                player.KillPoint++;

                                stage.DisplayKillPoint((int)player.PlayerIndex, 
                                                       player.KillPoint);
                            }
                        }
                    }

                    hit = true;
                }
            }


            //  Fire the tracer bullet particle
            if( this.specData.TracerBulletFire )
            {
                RobotGameGame.CurrentStage.TracerBulletManager.Fire(0,
                                                    firePosition,
                                                    targetPosition,
                                                    this.specData.TracerBulletSpeed, 
                                                    this.specData.TracerBulletLength, 
                                                    this.specData.TracerBulletThickness, 
                                                    true);
            }
            
            //  Cannot fire
            return hit;
        }

        /// <summary>
        /// stops firing sound.
        /// </summary>
        /// <returns></returns>
        public bool StopFireSound()
        {
            if (GameSound.IsPlaying(soundFire) )
            {
                GameSound.Stop(soundFire);
                return true;
            }

            return false;
        }

        /// <summary>
        /// stops reloading sound.
        /// </summary>
        /// <returns></returns>
        public bool StopReloadSound()
        {
            if (GameSound.IsPlaying(soundReload) )
            {
                GameSound.Stop(soundReload);
                return true;
            }

            return false;
        }

        /// <summary>
        /// whethher firing is possible.
        /// </summary>
        /// <returns>true or false</returns>
        public bool IsPossibleToFire()
        {
            if ((this.state == WeaponState.Ready) && (this.currentAmmo > 0))
                return true;

            return false;
        }

        /// <summary>
        /// whether reload is possible.
        /// </summary>
        /// <returns>true or false</returns>
        public bool IsPossibleToReload()
        {
            if ((this.state == WeaponState.Ready) &&
                (this.RemainAmmo > 0 || this.specData.TotalBulletCount == -1) &&
                (this.currentAmmo < this.specData.ReloadBulletCount))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// sets a collision elememt of weapon.
        /// </summary>
        /// <param name="collide">collision elememt</param>
        public void SetCollide(CollideElement collide)
        {
            modelCollide = collide;
            modelCollide.Name = Name + "_Collide";
            modelCollide.Owner = (object)this;
        }

        /// <summary>
        /// enables/disables the culling of weapon that has been dropped.
        /// </summary>
        public void SetDroppedModelEnableCulling(bool enable)
        {
            //  Drawable weapon model is one
            GameModel droppedWeapon = this.modelWeapon[0];

            droppedWeapon.EnableCulling = enable;
        }

        /// <summary>
        /// enables/disables the implementation of fog.
        /// </summary>
        /// <param name="enable"></param>
        public void SetDroppedModelActiveFog(bool enable)
        {
            //  Drawable weapon model is one
            GameModel droppedWeapon = this.modelWeapon[0];

            droppedWeapon.ActiveFog = enable;
        }

        /// <summary>
        /// enables/disables the implementation of light.
        /// </summary>
        /// <param name="enable"></param>
        public void SetDroppedModelActiveLighting(bool enable)
        {
            //  Drawable weapon model is one
            GameModel droppedWeapon = this.modelWeapon[0];

            droppedWeapon.ActiveLighting = enable;
        }

        /// <summary>
        /// owner acquires weapon.
        /// </summary>
        /// <param name="owner"></param>
        public void Pickup(NodeBase owner)
        {
            //  Attach node to new owner
            AttachOwner(owner);

            //  All weapon model is active
            for (int i = 0; i < modelWeapon.Length; i++)
            {
                this.modelWeapon[i].Enabled = true;
                this.modelWeapon[i].Visible = true;
                this.modelWeapon[i].WorldTransform = Matrix.Identity;
            }

            //  Collision is off
            this.Collide.RemoveInLayer();

            DroppedModel.SetRootAxis(Matrix.Identity);

            this.WorldTransform = Matrix.Identity;
            this.isDropped = false;
            this.Enabled = false;
            this.Visible = false;
        }

        /// <summary>
        /// drops weapon at specified position.
        /// </summary>
        /// <param name="position">new drop position</param>
        /// <param name="parent">new scene node</param>
        /// <param name="layer">new collision layer</param>
        public void Drop(Vector3 position, NodeBase parent, CollisionLayer layer)
        {
            //  Detached scene node from owner
            AttachOwner(parent);

            //  Add a collision
            if( layer != null)
                layer.AddCollide(this.Collide);

            DroppedModel.SetRootAxis(
                Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)));

            this.Position = position;
            this.isDropped = true;
            this.Enabled = true;
            this.Visible = true;
        }

        /// <summary>
        /// takes the weapon off from owener and disables it.
        /// </summary>
        public void Discard()
        {
            DetachOwner();

            //  Collision is off
            this.Collide.RemoveInLayer();

            DroppedModel.SetRootAxis(Matrix.Identity);

            this.WorldTransform = Matrix.Identity;
            this.isDropped = false;
            this.Enabled = false;
            this.Visible = false;
        }

        /// <summary>
        /// reloads weapon.
        /// plays reloading particle depending on unit type.
        /// </summary>
        /// <param name="type">unit type</param>
        /// <returns></returns>
        public bool Reload(UnitTypeId type)
        {
            this.state = WeaponState.Reloading;

            ParticleType reloadParticle = ParticleType.Count;
            SoundTrack reloadSound = SoundTrack.Count;

            switch (this.WeaponType)
            {
                case WeaponType.PlayerMachineGun:
                    {
                        reloadParticle = ParticleType.PlayerMachineGunReload;

                        switch (type)
                        {
                            case UnitTypeId.Grund:
                            case UnitTypeId.Kiev:
                                reloadSound = SoundTrack.PlayerMachineGunGrundReload;
                                break;
                            case UnitTypeId.Mark:
                            case UnitTypeId.Yager:
                                reloadSound = SoundTrack.PlayerMachineGunMarkReload;
                                break;
                        }
                    }
                    break;                    
                case WeaponType.PlayerShotgun:
                    {
                        //  Shotgun reload is no particle
                        reloadSound = SoundTrack.PlayerShotgunReload;
                    }
                    break;
                case WeaponType.PlayerHandgun:
                    {
                        reloadParticle = ParticleType.PlayerHandgunReload;
                        reloadSound = SoundTrack.PlayerHandgunReload;
                    }
                    break;
            }

            //  Play a reloading particle
            if (reloadParticle != ParticleType.Count)
            {
                for (int i = 0; i < SpecData.ModelCount; i++)
                {
                    int boneIdx = -1;

                    Matrix boneTransform = Matrix.Identity;

                    boneIdx = this.indexWeaponFireDummy[i];
                    boneTransform = modelWeapon[i].BoneTransforms[boneIdx];

                    if (reloadParticle != ParticleType.Count)
                    {
                        GameParticle.PlayParticle(reloadParticle, boneTransform,
                            Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)));
                    }
                }
            }

            switch (this.WeaponType)
            {
                case WeaponType.PlayerMachineGun:
                    {
                        if (GameSound.IsPlaying(soundFire))
                            GameSound.Stop(soundFire);
                    }
                    break;
            }

            //  Play a reload sound
            if (reloadSound != SoundTrack.Count)
            {
                if (RobotGameGame.CurrentGameLevel.Info.GamePlayType == 
                    GamePlayTypeId.Versus)
                {
                    soundReload = GameSound.Play3D(reloadSound, 
                        RobotGameGame.SinglePlayer.Emitter);
                }
                else
                {
                    soundReload = GameSound.Play3D(reloadSound, this.OwnerUnit.Emitter);
                }
            }

            return true;
        }

        /// <summary>
        /// checks for the weapon firing collision.
        /// When collision check succeeds, returns a result report.  
        /// If not, returns null.
        /// </summary>
        /// <param name="position">the start position of firing</param>
        /// <param name="direction">the direction of firing</param>
        /// <param name="distance">the range of firing</param>
        /// <param name="targetCollisionLayer">target collision layer</param>
        /// <param name="worldCollisionLayer">world collision layer</param>
        /// <returns>result report</returns>
        protected static CollisionResult FireHitTest(Vector3 position,
                                              Vector3 direction,
                                              float distance,
                                              ref CollisionLayer targetCollisionLayer,
                                              ref CollisionLayer worldCollisionLayer)
        {
            bool doHit = false;

            CollideRay collideRay = new CollideRay(position, direction);

            //  checks with enemies.
            CollisionResult fireResult = FrameworkCore.CollisionContext.HitTest(
                                                    collideRay,
                                                    ref targetCollisionLayer,
                                                    ResultType.NearestOne);

            if (fireResult != null)
            {
                if (fireResult.distance <= distance)
                {
                    //  Hit unit
                    doHit = true;
                }
            }

            //  checks with world.
            CollisionResult worldResult = FrameworkCore.CollisionContext.HitTest(
                                                            collideRay,
                                                            ref worldCollisionLayer,
                                                            ResultType.NearestOne);

            if (worldResult != null)
            {
                //  Hit world
                if (worldResult.distance <= distance)
                {
                    if (doHit)
                    {
                        //  closer world
                        if (worldResult.distance < fireResult.distance)
                        {
                            return worldResult;
                        }
                    }
                    else
                    {
                        return worldResult;
                    }
                }
            }

            if (doHit)
            {
                return fireResult;
            }

            return null;
        }
    }
}
