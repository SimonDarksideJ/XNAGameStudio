#region File Description
//-----------------------------------------------------------------------------
// GameItemBox.cs
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
using RobotGameData.Render;
using RobotGameData.GameObject;
using RobotGameData.GameInterface;
using RobotGameData.Sound;
#endregion

namespace RobotGame
{
    /// <summary>
    /// This class represents the items that are on the ground in the world.
    /// When a player picks one up, there’s a change in the spec data, 
    /// either HP recovery or bullet refill.
    /// Once gets picked, an item disappears from screen.
    /// It contains ItemBoxSpec, which is item’s information class.
    /// </summary>
    public class GameItemBox : GameModel
    {
        #region Fields

        ItemBoxSpec specData = null;

        float rotateAngleAccm = 0.0f;
        float rotateAnglePerSecond = 40.0f;

        #endregion

        #region Properties

        public ItemBoxSpec SpecData
        {
            get { return this.specData; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameItemBox(ItemBoxSpec spec)
            : base(spec.ModelFilePath)
        {
            this.specData = spec;

            Name = spec.Type.ToString();
        }

        public override void  Initialize()
        {
              base.Initialize();
        }

        /// <summary>
        /// updates the weapon.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void OnUpdate(GameTime gameTime)
        {
            //  Calculates rotation angle
            rotateAngleAccm += rotateAnglePerSecond * 
                               (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (rotateAngleAccm > 360.0f)
                rotateAngleAccm = 0.0f;

            Vector3 pos = WorldTransform.Translation;

            //  item is always rotate in the world
            WorldTransform = 
                    Matrix.CreateRotationY(MathHelper.ToRadians(rotateAngleAccm)) *
                    Matrix.CreateTranslation(pos);

            base.OnUpdate(gameTime);
        }

        /// <summary>
        /// when an item is picked up from the world, there will be HP recovery and 
        /// replenishment of ammunition.
        /// </summary>
        /// <param name="unit">item을 pick up한 unit</param>
        public void PickUp(GameUnit unit)
        {
            //  recovery life.
            unit.Life += 
                    (int)((float)unit.MaxLife * ((float)SpecData.RecoveryLife * 0.01f));

            if (unit.Life > unit.MaxLife)
                unit.Life = unit.MaxLife;

            //  the default weapon is must be machine gun.
            //  this is machine gun ammo.
            unit.DefaultWeapon.RemainAmmo += SpecData.RecoveryBullet;

            //  collision is off.
            this.Collide.RemoveInLayer();
            this.Enabled = false;
            this.Visible = false;

            //  must be no render and no update.
            this.RemoveFromParent();            
        }
    }
}
