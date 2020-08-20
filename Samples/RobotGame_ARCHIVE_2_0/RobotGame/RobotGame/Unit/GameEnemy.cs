#region File Description
//-----------------------------------------------------------------------------
// GameEnemy.cs
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
using RobotGameData.Collision;
using RobotGameData.AI;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It defines the common actions for the enemy units and 
    /// binds the AI function interface.
    /// It contains GameEnemySpec, which is the enemy unit’s information class.
    /// </summary>
    public class GameEnemy : GameUnit
    {
        #region Enum

        /// <summary>
        /// A.I. enums
        /// </summary>
        public enum AIType
        {
            Search = 0,
            Move,
            Attack,
            TurnLeft,
            TurnRight,
        }

        #endregion

        #region Fields

        protected UnitClassId unitClass = UnitClassId.Unknown;  
        protected GameEnemySpec specData = null;
        protected AIContext aiContext = null;

        protected float actionElapsedTime = 0.0f;

        protected Cue soundMove = null;
        protected Cue soundDestroy1 = null;
        protected Cue soundDestroy2 = null;

        protected int indexAiSearch = -1;
        protected int indexAiMove = -1;
        protected int indexAiAttack = -1;
        protected int indexAiTurnLeft = -1;
        protected int indexAiTurnRight = -1;

        AIType startAi = AIType.Search;
        float startAiTime = 0.0f;

        #endregion

        #region Properties

        public GameEnemySpec SpecData
        {
            get { return this.specData; }
        }

        public UnitClassId UnitClass
        {
            get { return specData.UnitClass; }
        }

        public UnitTypeId UnitType
        {
            get { return specData.UnitType; }
        }

        public AIContext AIContext
        {
            get { return this.aiContext; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameEnemy(ref GameEnemySpec spec)
            : base(spec.ModelFilePath)
        {
            this.specData = spec;

            Name = this.specData.UnitType.ToString();
            Life = this.specData.Life;
            MaxLife = this.SpecData.Life;

            //  Load the waepon
            CreateWeapon(spec.DefaultWeaponFilePath);

            //  Create the AI context
            this.aiContext = new AIContext();
            AddChild(this.aiContext);
        }

        /// <summary>
        /// creates and initializes all base A.I.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            //  Entry patrol update function in AI
            AIBase aiSearch = new AIBase();
            aiSearch.Updating += 
                new EventHandler<AIBase.AIUpdateEventArgs>(OnSearchUpdateEvent);
            indexAiSearch = AIContext.AddAI("Search", aiSearch);

            //  Entry move update function in AI
            AIBase aiMove = new AIBase();
            aiMove.Updating += 
                new EventHandler<AIBase.AIUpdateEventArgs>(OnMoveUpdateEvent);
            indexAiMove = AIContext.AddAI("Move", aiMove);

            //  Entry attack update function in AI
            AIBase aiAttack = new AIBase();
            aiAttack.Updating +=
                new EventHandler<AIBase.AIUpdateEventArgs>(OnAttackUpdateEvent);
            indexAiAttack = AIContext.AddAI("Attack", aiAttack);

            //  Entry left turn update function in AI
            AIBase aiTurnLeft = new AIBase();
            aiTurnLeft.Updating += 
                new EventHandler<AIBase.AIUpdateEventArgs>(OnTurnLeftUpdateEvent);
            indexAiTurnLeft = AIContext.AddAI("TurnLeft", aiTurnLeft);

            //  Entry right turn update function in AI
            AIBase aiTurnRight = new AIBase();
            aiTurnRight.Updating += 
                new EventHandler<AIBase.AIUpdateEventArgs>(OnTurnRightUpdateEvent);
            indexAiTurnRight = AIContext.AddAI("TurnRight", aiTurnRight);

            WorldTransform = SpawnPoint;

            //  Remove a collision
            if (colLayerEnemyMech != null)
            {
                if (colLayerEnemyMech.IsContain(Collide))
                    colLayerEnemyMech.RemoveCollide(Collide);
            }

            //  Remove a collision
            if (colLayerAllMech != null)
            {
                if (colLayerAllMech.IsContain(Collide))
                    colLayerAllMech.RemoveCollide(Collide);
            }
        }

        /// <summary>
        /// reset members.
        /// initializes the various gauges and states.
        /// </summary>
        protected override void OnReset()
        {
            base.OnReset();

            this.Visible = true;
            this.Enabled = true;

            Life = this.specData.Life;
            this.actionElapsedTime = 0.0f;

            //  Reset the weapon
            CurrentWeapon.Reset();

            //  Add a collision
            if (colLayerEnemyMech != null)
            {
                if (!colLayerEnemyMech.IsContain(Collide))
                    colLayerEnemyMech.AddCollide(Collide);
            }

            //  Add a collision
            if (colLayerAllMech != null)
            {
                if (!colLayerAllMech.IsContain(Collide))
                    colLayerAllMech.AddCollide(Collide);
            }

            //  Initialize start AI
            int indexAi = -1;
            switch (startAi)
            {
                case AIType.Search: indexAi = indexAiSearch; break;
                case AIType.Move: indexAi = indexAiMove; break;
                case AIType.Attack: indexAi = indexAiAttack; break;
                case AIType.TurnLeft: indexAi = indexAiTurnLeft; break;
                case AIType.TurnRight: indexAi = indexAiTurnRight; break;
            }
            
            AIContext.StartAI(indexAi, startAiTime);
        }

        protected override void OnUpdate(GameTime gameTime)
        {            
            base.OnUpdate(gameTime);
        }

        #region A.I. process

        /// <summary>
        /// set a start A.I.
        /// </summary>
        /// <param name="aiBase">A.I.</param>
        /// <param name="aiTime">A.I. duration</param>
        public void SetStartAI(AIType aiType, float aiTime)
        {
            startAi = aiType;
            startAiTime = aiTime;
        }

        /// <summary>
        /// set a next A.I.
        /// </summary>
        /// <param name="aiBase">A.I.</param>
        /// <param name="aiTime">A.I. duration</param>
        public void SetNextAI(AIType aiType, float aiTime)
        {
            int indexAi = -1;
            switch (aiType)
            {
                case AIType.Search: indexAi = indexAiSearch; break;
                case AIType.Move: indexAi = indexAiMove; break;
                case AIType.Attack: indexAi = indexAiAttack; break;
                case AIType.TurnLeft: indexAi = indexAiTurnLeft; break;
                case AIType.TurnRight: indexAi = indexAiTurnRight; break;
            }

            AIContext.Enabled = true;
            AIContext.SetNextAI(indexAi, aiTime);
        }

        public virtual void OnAISearchEvent(AIBase aiBase) {}
        public virtual void OnAIMoveEvent(AIBase aiBase, GameTime gameTime) { }
        public virtual void OnAIAttackEvent(AIBase aiBase, GameTime gameTime) { }
        public virtual void OnAITurnLeftEvent(AIBase aiBase) {}
        public virtual void OnAITurnRightEvent(AIBase aiBase) { }

        /// <summary>
        /// call search A.I.
        /// </summary>
        internal void OnSearchUpdateEvent(object sender, AIBase.AIUpdateEventArgs e)
        {
            OnAISearchEvent(sender as AIBase);
        }

        /// <summary>
        /// call movement A.I.
        /// </summary>
        internal void OnMoveUpdateEvent(object sender, AIBase.AIUpdateEventArgs e)
        {
            OnAIMoveEvent(sender as AIBase, e.GameTime);
        }

        /// <summary>
        /// call turn right A.I.
        /// </summary>
        internal void OnTurnRightUpdateEvent(object sender, AIBase.AIUpdateEventArgs e)
        {
            OnAITurnRightEvent(sender as AIBase);
        }

        /// <summary>
        /// call turn left A.I.
        /// </summary>
        internal void OnTurnLeftUpdateEvent(object sender, AIBase.AIUpdateEventArgs e)
        {
            OnAITurnLeftEvent(sender as AIBase);
        }

        /// <summary>
        /// call attack A.I.
        /// </summary>
        internal void OnAttackUpdateEvent(object sender, AIBase.AIUpdateEventArgs e)
        {
            OnAIAttackEvent(sender as AIBase, e.GameTime);
        }

        #endregion
    }
}
