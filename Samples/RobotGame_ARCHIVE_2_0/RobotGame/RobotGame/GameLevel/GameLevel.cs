#region File Description
//-----------------------------------------------------------------------------
// GameLevel.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RobotGameData;
using RobotGameData.GameObject;
using RobotGameData.Render;
using RobotGameData.GameEvent;
using RobotGameData.Collision;
using RobotGameData.Helper;
#endregion

namespace RobotGame
{
    #region Enums

    public enum GamePlayTypeId
    {
        /// <summary>
        /// N/A
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// a play type that clears a stage with a certain objective.
        /// </summary>
        StageClear,

        /// <summary>
        /// a play type of one-on-one versus mode.
        /// </summary>
        Versus,
    }

    public enum SpawnTypeId
    {
        /// <summary>
        /// a type of getting spawned after a certain period of time.
        /// </summary>
        Time = 0,

        /// <summary>
        /// a type of getting spawned when moving to a certain area.
        /// </summary>
        Area,
    }

    public enum VictoryConditionTypeId
    {
        /// <summary>
        /// N/A
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// a type of winning the game by being the sole survivor.
        /// </summary>
        Survive,

        /// <summary>
        /// a type of winning the game by eliminating every enemy in stage.
        /// </summary>
        DestroyAllEnemies,

        /// <summary>
        /// a type of winning by just destroying the boss.
        /// </summary>
        DestroyOnlyBoss,
    }

    #endregion

    #region Level Data

    [Serializable]
    public class WorldInLevel
    {
        public string WorldModelFile = String.Empty;
        public string WorldCubemapFile = String.Empty;

        public Vector3 WorldModelPosition = Vector3.Zero;
        public Vector3 WorldModelAxis = Vector3.Zero;

        public bool FogEnable = false;
        public bool LightingEnable = false;
        public bool AlphaBlendEnable = false;

        public string SkyModelFile = String.Empty;
        public Vector3 SkyModelPosition = Vector3.Zero;
        public Vector3 SkyModelAxis = Vector3.Zero;
        public bool SkyFollowOwner = false;

        public string CollisionMoveModelFile = String.Empty;
        public Vector3 CollisionMoveModelPosition = Vector3.Zero;
        public Vector3 CollisionMoveModelAxis = Vector3.Zero;

        public string CollisionHitModelFile = String.Empty;
        public Vector3 CollisionHitModelPosition = Vector3.Zero;
        public Vector3 CollisionHitModelAxis = Vector3.Zero;
    }

    /// <summary>
    /// player information for stage level.
    /// it has spec file name, start position in the game, and material information.
    /// </summary>
    [Serializable]
    public class PlayerInLevel
    {
        public string SpecFilePath = String.Empty;

        public Vector3 SpawnPoint = Vector3.Zero;
        public Vector3 MaterialDiffuseColor = Vector3.One;
        public Vector3 MaterialSpecularColor = Vector3.One;
        public Vector3 MaterialEmissiveColor = Vector3.Zero;
        public float MaterialSpecularPower = 1.0f;
        public float SpawnAngle = 0.0f;
    }

    /// <summary>
    /// enemy information for stage level.
    /// it has spec file name, spawn position in the game, and A.I. setting information.
    /// </summary>
    [Serializable]
    public class EnemyInLevel
    {
        public string SpecFilePath = String.Empty;

        public SpawnTypeId SpawnType = SpawnTypeId.Time;
        public Vector3 SpawnPoint = Vector3.Zero;
        public float SpawnTime = 0.0f;        
        public float SpawnRadius = 0.0f;
        public float SpawnAngle = 0.0f;

        public GameEnemy.AIType StartAi = GameEnemy.AIType.Search;
        public float StartAiTime = 1.0f;

        public Vector3 MaterialDiffuseColor = Vector3.One;
        public Vector3 MaterialSpecularColor = Vector3.One;
        public Vector3 MaterialEmissiveColor = Vector3.Zero;
        public float MaterialSpecularPower = 1.0f;
    }

    /// <summary>
    /// item information for stage level.
    /// it has spec file name and spawn position in the game.
    /// </summary>
    [Serializable]
    public class ItemInLevel
    {
        public string SpecFilePath = String.Empty;
        public Vector3 Position = Vector3.Zero;
    }

    /// <summary>
    /// weapon information for stage level.
    /// it has spec file name and spawn position in the game.
    /// </summary>
    [Serializable]
    public class WeaponInLevel
    {
        public string SpecFilePath = String.Empty;
        public Vector3 Position = Vector3.Zero;
    }

    /// <summary>
    /// spawn position in the game for stage level.
    /// </summary>
    [Serializable]
    public class RespawnInLevel
    {
        public Vector3 SpawnPoint = Vector3.Zero;
        public float SpawnAngle = 0.0f;
    }

    /// <summary>
    /// stage information for stage level.
    /// it has victory condition type, world fog, default world lighting information,
    /// players, enemies, items, weapons and spawn positions in the game.
    /// </summary>
    [Serializable]
    public class GameLevelInfo
    {
        public GamePlayTypeId GamePlayType = GamePlayTypeId.Unknown;
        public VictoryConditionTypeId VictoryConditionType = 
                                                VictoryConditionTypeId.Unknown;
        public float FOV = 36.0f;

        public bool FogEnable = false;
        public float FogStart = 0.0f;
        public float FogEnd = 0.0f;
        public Vector3 FogColor = Vector3.Zero;

        public bool LightingEnable = false;
        public Vector3 LightingAmbientColor = Vector3.One;
        public Vector3 LightingDiffuseColor = Vector3.One;
        public Vector3 LightingSpecularColor = Vector3.One;
        public Vector3 LightingDirection = Vector3.Down;

        // Controls how bright a pixel needs to be before it will bloom.
        // Zero makes everything bloom equally, while higher values select
        // only brighter colors. Somewhere between 0.25 and 0.5 is good.
        public float ParameterBloomThreshold = 0.25f;

        // Controls how much blurring is applied to the bloom image.
        // The typical range is from 1 up to 10 or so.
        public float ParameterBlurAmount = 4.0f;

        // Controls the amount of the bloom and base images that
        // will be mixed into the final scene. Range 0 to 1.
        public float ParameterBloomIntensity = 1.25f;
        public float ParameterBaseIntensity = 1.0f;

        // Independently control the color saturation of the bloom and
        // base images. Zero is totally desaturated, 1.0 leaves saturation
        // unchanged, while higher values increase the saturation level.
        public float ParameterBloomSaturation = 1.0f;
        public float ParameterBaseSaturation = 1.0f;

        public WorldInLevel WorldInLevel = null;
        public PlayerInLevel PlayerInLevel = null;

        public string ParticleListFile = String.Empty;

        public List<EnemyInLevel> EnemyInLevelList = new List<EnemyInLevel>();
        public List<ItemInLevel> ItemInLevelList = new List<ItemInLevel>();
        public List<WeaponInLevel> WeaponInLevelList = new List<WeaponInLevel>();
        public List<RespawnInLevel> RespawnInLevelList = new List<RespawnInLevel>();
    }

    #endregion

    /// <summary>
    /// It loads all the information needed for a stage level.
    /// Into the Level data file (.level), the winning condition and 
    /// the gameplay method for the stage, the values of lighting and fog and 
    /// the information of players, enemies, weapons, and items, which are used 
    /// in the gaming world, are read at once.
    /// </summary>
    public class GameLevel
    {
        #region Fields

        /// <summary>
        /// Level information
        /// </summary>
        GameLevelInfo levelInfo = new GameLevelInfo();

        /// <summary>
        /// Players container
        /// </summary>
        List<GamePlayer> playerList = new List<GamePlayer>();

        /// <summary>
        /// enemies container
        /// </summary>
        List<GameEnemy> enemyList = new List<GameEnemy>();

        /// <summary>
        /// items container
        /// </summary>
        List<GameItemBox> itemList = new List<GameItemBox>();

        /// <summary>
        /// weapons container
        /// </summary>
        List<GameWeapon> weaponList = new List<GameWeapon>();

        GameWorld gameWorld = null;
        GameSkybox gameSky = null;
        GameModel moveCollision = null;
        GameModel hitCollision = null;

        //  Scene nodes
        GameSceneNode sceneWorldRoot = null;
        GameSceneNode sceneMechRoot = null;
        GameSceneNode sceneParticleRoot = null;
        GameSceneNode sceneCollisionRoot = null;

        //  Collide layers
        CollisionLayer collisionLayerMoveWorld = null;
        CollisionLayer collisionLayerHitWorld = null;
        CollisionLayer collisionLayerFriendlyMech = null;
        CollisionLayer collisionLayerEnemyMech = null;
        CollisionLayer collisionLayerAllMech = null;
        CollisionLayer collisionLayerItems = null;
        CollisionLayer[] collisionVersusTeam = null;

        const int quadLevel = 3;

        #endregion

        #region Properties

        public GameLevelInfo Info
        {
            get { return levelInfo; }
        }

        public GamePlayer SinglePlayer
        {
            get { return GetPlayerInLevel(0); }
        }

        public int PlayerCountInLevel
        {
            get { return playerList.Count; }
        }

        public int EnemyCountInLevel
        {
            get { return enemyList.Count; }
        }

        public int ItemCountInLevel
        {
            get { return itemList.Count; }
        }

        public int WeaponCountInLevel
        {
            get { return weaponList.Count; }
        }

        public GameWorld GameWorld
        {
            get { return this.gameWorld; }
            set { this.gameWorld = value; }
        }

        public GameSkybox GameSky
        {
            get { return this.gameSky; }
            set { this.gameSky = value; }
        }

        public GameModel MoveCollision
        {
            get { return this.moveCollision; }
            set { this.moveCollision = value; }
        }

        public GameModel HitCollision
        {
            get { return this.hitCollision; }
            set { this.hitCollision = value; }
        }

        public GameSceneNode SceneWorldRoot
        {
            get { return this.sceneWorldRoot; }
            set { this.sceneWorldRoot = value; }
        }

        public GameSceneNode SceneMechRoot
        {
            get { return this.sceneMechRoot; }
            set { this.sceneMechRoot = value; }
        }

        public GameSceneNode SceneParticleRoot
        {
            get { return this.sceneParticleRoot; }
            set { this.sceneParticleRoot = value; }
        }

        public GameSceneNode SceneCollisionRoot
        {
            get { return this.sceneCollisionRoot; }
            set { this.sceneCollisionRoot = value; }
        }

        public CollisionLayer CollisionLayerMoveWorld
        {
            get { return this.collisionLayerMoveWorld; }
        }

        public CollisionLayer CollisionLayerHitWorld
        {
            get { return this.collisionLayerHitWorld; }
        }

        public CollisionLayer CollisionLayerFriendlyMech
        {
            get { return this.collisionLayerFriendlyMech; }
        }

        public CollisionLayer CollisionLayerEnemyMech
        {
            get { return this.collisionLayerEnemyMech; }
        }

        public CollisionLayer CollisionLayerAllMech
        {
            get { return this.collisionLayerAllMech; }
        }

        public CollisionLayer CollisionLayerItems
        {
            get { return this.collisionLayerItems; }
        }

        public CollisionLayer[] CollisionVersusTeam
        {
            get { return this.collisionVersusTeam; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameLevel()
        {
            this.SceneWorldRoot = new GameSceneNode();
            this.SceneWorldRoot.Name = "Scene World Root";

            this.SceneMechRoot = new GameSceneNode();
            this.SceneMechRoot.Name = "Scene Mech Root";

            this.sceneParticleRoot = new GameSceneNode();
            this.sceneParticleRoot.Name = "Scene Particles Root";
            this.sceneParticleRoot.OrderWhenDraw = 
                GameSceneNode.DrawOrderType.Descending;

            this.sceneCollisionRoot = new GameSceneNode();
            this.sceneCollisionRoot.Name = "Scene Collisions Root";
            this.sceneCollisionRoot.Visible = false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize members.
        /// </summary>
        public void Initialize()
        {
            Clear();

            //  Add collision layers
            collisionLayerMoveWorld =
                FrameworkCore.CollisionContext.AddLayer("Collision MoveWorld");

            //  Add collision layers
            collisionLayerHitWorld =
                FrameworkCore.CollisionContext.AddLayer("Collision HitWorld");

            //  Add collision layers
            collisionLayerAllMech =
                FrameworkCore.CollisionContext.AddLayer("Collision AllMech");

            //  Add collision layers
            collisionLayerItems =
                FrameworkCore.CollisionContext.AddLayer("Collision Items");

            if (Info.GamePlayType == GamePlayTypeId.Versus)
            {
                collisionVersusTeam = new CollisionLayer[2];

                //  Add two collision layers
                for (int i = 0; i < 2; i++)
                {
                    collisionVersusTeam[i] =
                        FrameworkCore.CollisionContext.AddLayer(
                        "Collision Versus Team");
                }
            }
            else
            {
                //  Add collision layers
                collisionLayerFriendlyMech =
                    FrameworkCore.CollisionContext.AddLayer("Collision FriendlyMech");

                //  Add collision layers
                collisionLayerEnemyMech =
                    FrameworkCore.CollisionContext.AddLayer("Collision EnemyMech");
            }
        }

        /// <summary>
        /// clear members.
        /// </summary>
        public void Clear()
        {
            this.SceneWorldRoot.RemoveAllChild(true);
            this.SceneMechRoot.RemoveAllChild(true);
            this.sceneParticleRoot.RemoveAllChild(true);
            this.sceneCollisionRoot.RemoveAllChild(true);

            FrameworkCore.CollisionContext.ClearAllLayer();
        }

        #endregion

        #region Level

        /// <summary>
        /// load all of the data for stage level, player, enemies, items, weapons, etc.
        /// </summary>
        /// <returns></returns>
        public void LoadLevel(string levelFile)
        {
            this.levelInfo = (GameLevelInfo)HelperFile.LoadData(levelFile, 
                                                              levelInfo.GetType());

            RobotGameGame.CurrentGameLevel = this;

            //  Initialize level
            Initialize();

            //  Global fog
            if (levelInfo.FogEnable)
            {
                FrameworkCore.Viewer.BasicFog = new RenderFog();
                FrameworkCore.Viewer.BasicFog.enabled = true;
                FrameworkCore.Viewer.BasicFog.start = levelInfo.FogStart;
                FrameworkCore.Viewer.BasicFog.end = levelInfo.FogEnd;

                FrameworkCore.Viewer.BasicFog.color = new Color(
                                                        (byte)levelInfo.FogColor.X,
                                                        (byte)levelInfo.FogColor.Y,
                                                        (byte)levelInfo.FogColor.Z);
            }
            else
            {
                FrameworkCore.Viewer.BasicFog = new RenderFog();
                FrameworkCore.Viewer.BasicFog.enabled = false;
            }

            //  Global lighting
            if (levelInfo.LightingEnable)
            {
                FrameworkCore.Viewer.BasicLighting = new RenderLighting();
                FrameworkCore.Viewer.BasicLighting.enabled = true;

                FrameworkCore.Viewer.BasicLighting.ambientColor = new Color(
                                            (byte)levelInfo.LightingAmbientColor.X,
                                            (byte)levelInfo.LightingAmbientColor.Y,
                                            (byte)levelInfo.LightingAmbientColor.Z);

                FrameworkCore.Viewer.BasicLighting.diffuseColor = new Color(
                                            (byte)levelInfo.LightingDiffuseColor.X,
                                            (byte)levelInfo.LightingDiffuseColor.Y,
                                            (byte)levelInfo.LightingDiffuseColor.Z);

                FrameworkCore.Viewer.BasicLighting.specularColor = new Color(
                                            (byte)levelInfo.LightingSpecularColor.X,
                                            (byte)levelInfo.LightingSpecularColor.Y,
                                            (byte)levelInfo.LightingSpecularColor.Z);

                FrameworkCore.Viewer.BasicLighting.direction =
                                                        levelInfo.LightingDirection;
            }
            else
            {
                FrameworkCore.Viewer.BasicLighting = new RenderLighting();
                FrameworkCore.Viewer.BasicLighting.enabled = false;
            }

            //  Create world
            if (this.levelInfo.WorldInLevel != null)
            {
               CreateWorld(ref this.levelInfo.WorldInLevel);
            }

            //  Create particles
            CreateParticle(this.levelInfo.ParticleListFile);

            //  Create player
            if (this.levelInfo.PlayerInLevel != null)
            {
                GamePlayer player = CreatePlayer(ref this.levelInfo.PlayerInLevel, 
                                                this.SceneMechRoot);

                RobotGameGame.SinglePlayer = player;
                FrameworkCore.GameEventManager.TargetScene = player;
            }

            //  Create all enemies in the level
            if (levelInfo.EnemyInLevelList != null)
            {
                for (int i = 0; i < levelInfo.EnemyInLevelList.Count; i++)
                {
                    EnemyInLevel enemy = levelInfo.EnemyInLevelList[i];

                    CreateSpawnEnemy(ref enemy, this.SceneMechRoot);
                }
            }

            //  Create all items in the level
            if (levelInfo.ItemInLevelList != null)
            {
                for (int i = 0; i < levelInfo.ItemInLevelList.Count; i++)
                {
                    ItemInLevel item = levelInfo.ItemInLevelList[i];

                    CreateItemBox(ref item, this.SceneWorldRoot);
                }
            }

            //  Create all weapons in the level
            if (levelInfo.WeaponInLevelList != null)
            {
                for (int i = 0; i < levelInfo.WeaponInLevelList.Count; i++)
                {
                    CreateWeapon(levelInfo.WeaponInLevelList[i], SceneWorldRoot);
                }
            }

            //  Entry collision of the units to layer in the level
            switch (Info.GamePlayType)
            {
                case GamePlayTypeId.StageClear:
                    {
                        GamePlayer player = SinglePlayer;

                        //  Entry collision of the player to layer in the level
                        CollisionLayerFriendlyMech.AddCollide(player.Collide);
                        CollisionLayerAllMech.AddCollide(player.Collide);

                        //  Entry collsion of each enemy to layer in the level
                        for (int i = 0; i < EnemyCountInLevel; i++)
                        {
                            GameEnemy enemy = GetEnemyInLevel(i);

                            CollisionLayerEnemyMech.AddCollide(enemy.Collide);
                            CollisionLayerAllMech.AddCollide(enemy.Collide);
                        }
                    }
                    break;
                case GamePlayTypeId.Versus:
                    {
                    }
                    break;
            }

            //  Entry collsion of items to layer in the level
            for (int i = 0; i < ItemCountInLevel; i++)
            {
                GameItemBox item = GetItemInLevel(i);

                CollisionLayerItems.AddCollide(item.Collide);
            }

            //  Entry collsion of weapons to layer in the level
            for (int i = 0; i < WeaponCountInLevel; i++)
            {
                GameWeapon weapon = GetWeaponInLevel(i);

                CollisionLayerItems.AddCollide(weapon.Collide);
            }            
        }

        /// <summary>
        /// Check victory in the game.
        /// </summary>
        /// <returns></returns>
        public bool IsMissionClear()
        {
            switch (levelInfo.VictoryConditionType)
            {
                case VictoryConditionTypeId.DestroyAllEnemies:
                    {
                        //  Destroyed all enemies ?
                        for (int i = 0; i < enemyList.Count; i++)
                        {
                            GameEnemy enemy = enemyList[i];

                            if (!enemy.IsDead)
                            {
                                return false;
                            }
                        }

                        //  Enemies are down!!
                        return true;
                    }
                case VictoryConditionTypeId.DestroyOnlyBoss:
                    {
                        for (int i = 0; i < enemyList.Count; i++)
                        {
                            if (enemyList[i] is EnemyBoss)
                            {
                                //  Boss is down!
                                if (enemyList[i].IsDead)
                                    return true;
                            }
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Check fail in the game.
        /// </summary>
        /// <returns></returns>
        public static bool IsMissionFailed()
        {
            //  Are you game over?
            return RobotGameGame.SinglePlayer.IsDead;
        }

        #endregion  

        #region World

        /// <summary>
        /// creates a world model and a world collison.
        /// </summary>
        public void CreateWorld(ref WorldInLevel info)
        {
            //  Loads a sky model
            {
                GameSky = new GameSkybox(info.SkyModelFile);

                if (GameSky == null)
                    throw new ArgumentException("Failed to create a sky model.");

                GameSky.Name = "Skybox model";
                GameSky.EnableCulling = false;
                GameSky.ActiveFog = false;
                GameSky.ActiveLighting = false;
                GameSky.FollowOwner = info.SkyFollowOwner;

                //  Set to rotation axis
                Matrix rot = 
                    Matrix.CreateRotationX(MathHelper.ToRadians(info.SkyModelAxis.X)) *
                    Matrix.CreateRotationY(MathHelper.ToRadians(info.SkyModelAxis.Y)) *
                    Matrix.CreateRotationZ(MathHelper.ToRadians(info.SkyModelAxis.Z));
                            
                GameSky.SetRootAxis(rot);
                GameSky.SetPosition(info.SkyModelPosition);

                SceneWorldRoot.AddChild(GameSky);
            }

            //  Loads a world collison model for this stage
            {
                MoveCollision = new GameModel(info.CollisionMoveModelFile);

                if (MoveCollision == null)
                    throw new ArgumentException(
                                    "Failed to create a move collision model.");

                MoveCollision.Name = "move collison";
                MoveCollision.EnableCulling = false;
                MoveCollision.ActiveFog = false;
                MoveCollision.ActiveLighting = false;
                MoveCollision.Visible = false;

                //  Set to rotation axis
                Matrix rot =
                    Matrix.CreateRotationX(
                        MathHelper.ToRadians(info.CollisionMoveModelAxis.X)) *
                    Matrix.CreateRotationY(
                        MathHelper.ToRadians(info.CollisionMoveModelAxis.Y)) *
                    Matrix.CreateRotationZ(
                        MathHelper.ToRadians(info.CollisionMoveModelAxis.Z));

                MoveCollision.SetRootAxis(rot);
                MoveCollision.SetPosition(info.CollisionMoveModelPosition);

                SceneCollisionRoot.AddChild(MoveCollision);

                //  Creates a collision data
                Dictionary<string, object> tagData =
                        (Dictionary<string, object>)MoveCollision.ModelData.model.Tag;

                CollideModel collide = 
                            new CollideModel((Vector3[])tagData["Vertices"], quadLevel);

                MoveCollision.SetCollide(collide);

                CollisionLayerMoveWorld.AddCollide(MoveCollision.Collide);
            }

            //  Loads a Hit collison model for this stage
            {
                HitCollision = new GameModel(info.CollisionHitModelFile);

                if (HitCollision == null)
                    throw new ArgumentException(
                                    "Failed to create a hit collision model.");

                HitCollision.Name = "hit collison";
                HitCollision.EnableCulling = false;
                HitCollision.ActiveFog = false;
                HitCollision.ActiveLighting = false;
                HitCollision.Visible = false;

                //  Set to rotation axis
                Matrix rot =
                    Matrix.CreateRotationX(
                        MathHelper.ToRadians(info.CollisionHitModelAxis.X)) *
                    Matrix.CreateRotationY(
                        MathHelper.ToRadians(info.CollisionHitModelAxis.Y)) *
                    Matrix.CreateRotationZ(
                        MathHelper.ToRadians(info.CollisionHitModelAxis.Z));

                HitCollision.SetRootAxis(rot);
                HitCollision.SetPosition(info.CollisionHitModelPosition);

                SceneCollisionRoot.AddChild(HitCollision);

                //  Creates a collision data
                Dictionary<string, object> tagData = HitCollision.ModelData.model.Tag
                    as Dictionary<string, object>;

                CollideModel collide =
                            new CollideModel((Vector3[])tagData["Vertices"], quadLevel);

                HitCollision.SetCollide(collide);

                CollisionLayerHitWorld.AddCollide(HitCollision.Collide);
            }

            //  Loads a world model for this stage
            {
                GameWorld = new GameWorld(info.WorldModelFile);

                if (GameWorld == null)
                    throw new ArgumentException("Failed to create a world model.");

                //  Load a CubeMap texture
                GameWorld.LoadTextureCubeMap(info.WorldCubemapFile);

                GameWorld.Name = "world model";
                GameWorld.EnableCulling = false;
                GameWorld.ActiveFog = info.FogEnable;
                GameWorld.ActiveLighting = info.LightingEnable;

                if (info.AlphaBlendEnable)
                {
                    GameWorld.AlphaBlendEnable = true;
                    GameWorld.ReferenceAlpha = 0;
                    GameWorld.DepthBufferEnable = true;
                    GameWorld.DepthBufferWriteEnable = true;
                    GameWorld.DepthBufferFunction = CompareFunction.Less;
                    GameWorld.SourceBlend = Blend.SourceAlpha;
                    GameWorld.DestinationBlend = Blend.InverseSourceAlpha;
                    GameWorld.BlendFunction = BlendFunction.Add;
                    GameWorld.AlphaFunction = CompareFunction.Greater;
                }

                //  Set to rotation axis
                Matrix rot =
                    Matrix.CreateRotationX(
                        MathHelper.ToRadians(info.WorldModelAxis.X)) *
                    Matrix.CreateRotationY(
                        MathHelper.ToRadians(info.WorldModelAxis.Y)) *
                    Matrix.CreateRotationZ(
                        MathHelper.ToRadians(info.WorldModelAxis.Z));

                GameWorld.SetRootAxis(rot);
                GameWorld.SetPosition(info.WorldModelPosition);

                SceneWorldRoot.AddChild(GameWorld);
            }
        }

        #endregion

        #region Particle

        /// <summary>
        /// creates every particle to be used in the level.
        /// </summary>
        public void CreateParticle(string particleListFile)
        {
            //  Load all particles from the file
            GameParticle.LoadParticleList(particleListFile, SceneParticleRoot);
        }

        #endregion

        #region Player

        /// <summary>
        /// loads an player's information using spec file (.spec).
        /// </summary>
        /// <param name="info">player information for level</param>
        /// <returns>player's spec information</returns>
        public static GamePlayerSpec LoadPlayerSpec(ref PlayerInLevel info)
        {
            //  loads a information of the spawn enemy.
            GamePlayerSpec spec = new GamePlayerSpec();
            spec = (GamePlayerSpec)GameDataSpecManager.Load(info.SpecFilePath,
                                                            spec.GetType());

            return spec;
        }

        /// <summary>
        /// creates a player for level.
        /// reads an player information file(.spec) and configures the player class.
        /// The read player class is stored in the list.
        /// </summary>
        /// <param name="info">player information for level</param>
        /// <param name="sceneParent">3D scene parent node</param>
        /// <returns>player class for the game</returns>
        protected GamePlayer CreatePlayer(ref PlayerInLevel info,
                                          NodeBase sceneParent)
        {
            GamePlayer player = null;
            GamePlayerSpec spec = LoadPlayerSpec(ref info);

            switch (PlayerCountInLevel)
            {
                case 0:
                    player = new GamePlayer(ref spec, PlayerIndex.One);
                    break;
                case 1:
                    player = new GamePlayer(ref spec, PlayerIndex.Two);
                    break;
                default:
                    throw new InvalidOperationException(
                        "Added player count is overflow");
            }

            //  adds a player to list.
            AddPlayer(player);

            //  entries a player in parent scene node.
            sceneParent.AddChild(player);

            //  sets to rotation axis.
            Matrix rot = Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f));
            player.SetRootAxis(rot);

            //  sets the material.
            RenderMaterial material = new RenderMaterial();
            material.alpha = 1.0f;
            material.diffuseColor = new Color((byte)info.MaterialDiffuseColor.X,
                                              (byte)info.MaterialDiffuseColor.Y,
                                              (byte)info.MaterialDiffuseColor.Z);

            material.specularColor = new Color((byte)info.MaterialSpecularColor.X,
                                               (byte)info.MaterialSpecularColor.Y,
                                               (byte)info.MaterialSpecularColor.Z);

            material.emissiveColor = new Color((byte)info.MaterialEmissiveColor.X,
                                               (byte)info.MaterialEmissiveColor.Y,
                                               (byte)info.MaterialEmissiveColor.Z);

            material.specularPower = info.MaterialSpecularPower;

            material.vertexColorEnabled = false;
            material.preferPerPixelLighting = false;

            player.Material = material;
            player.ActiveFog = true;
            player.ActiveLighting = true;

            //  creates a collision data.
            Vector3 centerPos = Vector3.Transform(
                            new Vector3(0.0f, spec.MechRadius, 0.0f),
                            Matrix.Invert(rot));

            CollideSphere collide = new CollideSphere(centerPos,
                                                spec.MechRadius);

            player.EnableCulling = true;
            player.SetCollide(collide);
            player.ActionIdle();

            player.SpawnPoint =
                        Matrix.CreateRotationY(MathHelper.ToRadians(info.SpawnAngle)) *
                        Matrix.CreateTranslation(info.SpawnPoint);

            return player;
        }

        /// <summary>
        /// gets a player from the list.
        /// </summary>
        /// <param name="index">an index of the list</param>
        /// <returns>player class for the game</returns>
        public GamePlayer GetPlayerInLevel(int index)
        {
            if (playerList.Count > 0)
                return playerList[index];

            return null;
        }

        /// <summary>
        /// adds a player to the list.
        /// </summary>
        /// <param name="newPlayer">a new player</param>
        public void AddPlayer(GamePlayer newPlayer)
        {
            this.playerList.Add(newPlayer);
        }

        /// <summary>
        /// clear all players from the list.
        /// </summary>
        public void ClearAllPlayers()
        {
            this.playerList.Clear();
        }

        #endregion

        #region Enemy

        /// <summary>
        /// loads an enemy's information using spec file (.spec).
        /// </summary>
        /// <param name="info">enemy information for level</param>
        /// <returns>enemy's spec information</returns>
        public static GameEnemySpec LoadEnemySpec(ref EnemyInLevel info)
        {
            //  Load the spawn enemy information
            GameEnemySpec spec = new GameEnemySpec();
            spec = (GameEnemySpec)GameDataSpecManager.Load(info.SpecFilePath, 
                spec.GetType());
            
            return spec;
        }

        /// <summary>
        /// creates an enemy for level.
        /// reads an enemy information file(.spec) and configures the enemy class.
        /// The read enemy class is stored in the list.
        /// </summary>
        /// <param name="info">enemy information for level</param>
        /// <param name="sceneParent">3D scene parent node</param>
        protected void CreateSpawnEnemy(ref EnemyInLevel info,
                                        NodeBase sceneParent)
        {
            GameEnemy enemy = null;
            GameEnemySpec spec = LoadEnemySpec(ref info);

            //  creates an enemy by unit type
            switch (spec.UnitClass)
            {
                case UnitClassId.Tank:
                    enemy = new EnemyTank(ref spec);
                    break;
                case UnitClassId.LightMech:
                case UnitClassId.HeavyMech:
                    enemy = new EnemyMech(ref spec);
                    break;
                case UnitClassId.Boss:
                    enemy = new EnemyBoss(ref spec);
                    break;
                default: 
                    throw new NotSupportedException(
                                    "Not supported unit type : " + spec.UnitType);
            }
            
            //  sets the material
            RenderMaterial material = new RenderMaterial();
            material.alpha = 1.0f;
            material.diffuseColor = new Color((byte)info.MaterialDiffuseColor.X,
                                              (byte)info.MaterialDiffuseColor.Y,
                                              (byte)info.MaterialDiffuseColor.Z);

            material.specularColor = new Color((byte)info.MaterialSpecularColor.X,
                                               (byte)info.MaterialSpecularColor.Y,
                                               (byte)info.MaterialSpecularColor.Z);

            material.emissiveColor = new Color((byte)info.MaterialEmissiveColor.X,
                                               (byte)info.MaterialEmissiveColor.Y,
                                               (byte)info.MaterialEmissiveColor.Z);

            material.specularPower = info.MaterialSpecularPower;

            material.vertexColorEnabled = false;
            material.preferPerPixelLighting = false;

            enemy.Material = material;
            enemy.ActiveFog = true;
            enemy.ActiveLighting = true;

            //  adds this to the list.
            enemyList.Add(enemy);

            //  entries this in parent scene node.
            sceneParent.AddChild(enemy);

            //  sets to rotate axis.
            if (spec.UnitType == UnitTypeId.Tiger)
            {
                enemy.SetRootAxis(
                        Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(90.0f)));
            }
            else
            {
                enemy.SetRootAxis(Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)));
            }

            //  sets to stage spawn position.
            enemy.SpawnPoint = 
                        Matrix.CreateRotationY(MathHelper.ToRadians(info.SpawnAngle)) * 
                        Matrix.CreateTranslation(info.SpawnPoint);

            //  activate draw culling.
            enemy.EnableCulling = true;

            //  creates a collision data.
            {
                Vector3 centerPos = Vector3.Transform(
                                        new Vector3(0.0f, spec.MechRadius, 0.0f),
                                        Matrix.Invert(enemy.RootAxis));

                CollideSphere collide = new CollideSphere(centerPos, spec.MechRadius);
                enemy.SetCollide(collide);
            }

            //  creates a game event.
            switch (info.SpawnType)
            {
                case SpawnTypeId.Time:
                    {
                        FrameworkCore.GameEventManager.AddEvent(
                            new GameTimeEvent(info.SpawnTime, enemy, false));
                    }
                    break;
                case SpawnTypeId.Area:
                    {
                        FrameworkCore.GameEventManager.AddEvent(
                            new GameAreaEvent(info.SpawnPoint, info.SpawnRadius, 
                                                enemy, false));
                    }
                    break;
            }

            //  sets start A.I.
            enemy.SetStartAI(info.StartAi, info.StartAiTime);
        }
        
        /// <summary>
        /// gets an enemy from the list.
        /// </summary>
        /// <param name="index">an index of enemy in the list</param>
        /// <returns>enemy class</returns>
        public GameEnemy GetEnemyInLevel(int index)
        {
            return enemyList[index];
        }

        public void ClearAllEnemies()
        {
            this.enemyList.Clear();
        }

        #endregion

        #region Item

        /// <summary>
        /// loads an item's information using spec file (.spec).
        /// </summary>
        /// <param name="info">item information for level</param>
        /// <returns>item's spec information</returns>
        public static ItemBoxSpec LoadItemSpec(ref ItemInLevel info)
        {
            //  loads a spawn enemy information.
            ItemBoxSpec spec = new ItemBoxSpec();
            spec = (ItemBoxSpec)GameDataSpecManager.Load(info.SpecFilePath, 
                                                         spec.GetType());

            return spec;
        }

        /// <summary>
        /// creates an item for level.
        /// reads an item information file(.spec) and configures the item class.
        /// The read item class is stored in the list.
        /// </summary>
        /// <param name="info">item information for level</param>
        /// <param name="sceneParent">3D scene parent node</param>
        /// <returns>item class for the game</returns>
        protected GameItemBox CreateItemBox(ref ItemInLevel info,
                                            NodeBase sceneParent)
        {
            ItemBoxSpec spec = LoadItemSpec(ref info);

            GameItemBox item = new GameItemBox(spec);
            item.WorldTransform = Matrix.CreateTranslation(info.Position);
            item.SetRootAxis(Matrix.CreateRotationX(MathHelper.ToRadians(-90.0f)));

            //  creates a collision data.
            Vector3 centerPos = Vector3.Transform(
                                            new Vector3(0.0f, spec.ModelRadius, 0.0f),
                                            Matrix.Invert(item.RootAxis));

            CollideSphere collide = new CollideSphere(centerPos, spec.ModelRadius);
            item.SetCollide(collide);                

            item.EnableCulling = true;
            item.ActiveFog = true;
            item.ActiveLighting = false;

            //  adds item to the list.
            itemList.Add(item);

            //  adds item to parent scene node.
            sceneParent.AddChild(item);

            return item;
        }

        /// <summary>
        /// gets an item class from the list.
        /// </summary>
        /// <param name="index">an index of the list</param>
        /// <returns>item class for the game</returns>
        public GameItemBox GetItemInLevel(int index)
        {
            return itemList[index];
        }

        #endregion

        #region Weapon

        /// <summary>
        /// creates a weapon for level.
        /// reads an weapon information file(.spec) and configures the weapon class.
        /// The read weapon class is stored in the list.
        /// </summary>
        /// <param name="info">weapon information for level</param>
        /// <param name="sceneParent">3D scene parent node</param>
        /// <returns>weapon class for the game</returns>
        protected GameWeapon CreateWeapon(WeaponInLevel info, NodeBase sceneParent)
        {
            GameWeaponSpec spec = new GameWeaponSpec();
            spec = (GameWeaponSpec)GameDataSpecManager.Load(info.SpecFilePath, 
                                                            spec.GetType());

            GameWeapon weapon = new GameWeapon(spec);            

            //  creates a collision data.
            Vector3 centerPos = Vector3.Transform(new Vector3(0f, spec.ModelRadius, 0f),
                Matrix.Invert(weapon.RootAxis));

            CollideSphere collide = new CollideSphere(centerPos, spec.ModelRadius);
            weapon.SetCollide(collide);
            
            weapon.SetDroppedModelActiveFog(true);
            weapon.SetDroppedModelActiveLighting(false);

            //  drop to world.
            weapon.Drop(info.Position, sceneParent, null);

            //  adds a weapon to the list.
            weaponList.Add(weapon);

            return weapon;
        }

        /// <summary>
        /// gets a weapon class in the list.
        /// </summary>
        /// <param name="index">an index of the list</param>
        /// <returns>weapon class for the game</returns>
        public GameWeapon GetWeaponInLevel(int index)
        {
            return weaponList[index];
        }

        #endregion

        #region Respawn

        /// <summary>
        /// searches for the respawn position, which is the farthest away 
        /// from the currently specified respawn position, from the list.
        /// </summary>
        /// <param name="targetPosition">the position of target</param>
        /// <returns>respawn information</returns>
        public RespawnInLevel FindRespawnMostFar(Vector3 targetPosition)
        {
            int findIndex = -1;
            float mostDistance = 0;

            for (int i = 0; i < this.Info.RespawnInLevelList.Count; i++)
            {
                RespawnInLevel element = this.Info.RespawnInLevelList[i];

                float distance = Vector3.Distance(targetPosition, element.SpawnPoint);

                if (mostDistance < distance)
                {
                    mostDistance = distance;
                    findIndex = i;
                }
            }

            return this.Info.RespawnInLevelList[findIndex];
        }

        /// <summary>
        /// searches for the respawn position, which is the farthest away 
        /// from the currently specified respawn position, from the list.
        /// </summary>
        /// <param name="targetIndex">
        /// an index of target respawn position in the list
        /// </param>
        /// <returns>respawn information</returns>
        public RespawnInLevel FindRespawnMostFar(int targetIndex)
        {
            RespawnInLevel target = this.Info.RespawnInLevelList[targetIndex];

            return FindRespawnMostFar(target.SpawnPoint);
        }

        #endregion
    }
}
