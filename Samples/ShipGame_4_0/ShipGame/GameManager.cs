#region File Description
//-----------------------------------------------------------------------------
// GameManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BoxCollider;

namespace ShipGame
{
    // supported rendering techniques
    public enum RenderTechnique
    {
        PlainMapping = 0,        // plain texture mapping
        NormalMapping,           // normal mapping
        ViewMapping              // view aligned mapping (used for blaster)
    }

    // game modes
    public enum GameMode
    {
        None = 0,
        SinglePlayer,            // single player mode
        MultiPlayer              // multiplayer mode
    }

    // animated sprites
    public enum AnimSpriteType
    {
        Blaster = 0,              // blaster hit
        Missile,                  // missile explode
        Ship,                     // ship explode
        Spawn,                    // ship/object spawn
        Shield                    // ship shield
    }

    // projectiles
    public enum ProjectileType
    {
        Blaster = 0,              // blaster projectile
        Missile                   // missile projectile
    }

    // particle systems
    public enum ParticleSystemType
    {
        ShipExplode = 0,           // ship explode
        ShipTrail,               // ship trail
        MissileExplode,          // missile explode
        MissileTrail,            // missile trail
        BlasterExplode           // blaster explode
    }

    // powerup types
    public enum PowerupType
    {
        Energy = 0,                // 50% energy
        Missile                    // 3 missiles
    }

    public class GameManager : IDisposable
    {
        AnimSpriteManager animatedSprite;        // animated sprite manager
        ProjectileManager projectile;        // projectile manager
        ParticleManager particle;            // particle manager
        PowerupManager powerup;              // powerup manager
        SoundBank sound;                     // sound manager

        GameMode gameMode = GameMode.SinglePlayer;    // current game mode

        // ship file names for each player (selected and set from player screen)
        String[] shipFile = new String[GameOptions.MaxPlayers];

        uint invertY = 0;        // bit array for mouse invert Y
        // (selected and set from player screen)

        String levelFile;        // current level file name
        // (selected and set from level screen)

        // the player objects
        PlayerShip[] players = new PlayerShip[GameOptions.MaxPlayers];

        // pad vibration times for each player (zero for no vibration)
        float[] vibrationTime = new float[GameOptions.MaxPlayers];

        Viewport viewportLeft;          // left split screen viewport
        Viewport viewportRight;         // right split screen viewport

        Matrix projectionFull;                // full screen projection matrix
        Matrix projectionSplit;               // split screen projection matrix

        Model levelColor;                // level model
        EntityList levelSpawns;          // level spawn points
        LightList levelLights;           // level lights
        CollisionMesh levelCollision;    // level collision model

        // particle texture files (matches ParticleSystemType)
        String[] particleFiles = new String[] { "Spark1", "Point1", "Spark2", "Point1", "Point2" };
        Texture2D[] particleTextures;

        // animated sprite texture files (matches AnimSpriteType)
        String[] animatedSpriteFiles = new String[] 
                    {    "BlasterGrid_16", "MissileGrid_16", "ShipGrid_32", 
                        "SpawnGrid_16", "ShieldGrid_32" };
        Texture2D[] animatedSpriteTextures;

        // projectile modell files (matches ProjectileType)
        String[] projectileFiles = new String[] { "blaster", "missile" };
        Model[] projectileModels;

        // powerup model files (matches PowerupType)
        String[] powerupFiles = new String[] { "energy", "missile" };
        Model[] powerupModels;

        Texture2D hudCrosshair;       // hud crosshair texture
        Texture2D hudEnergy;          // hud energy/shield/boost texture
        Texture2D hudMissile;         // hud missile texture
        Texture2D hudScore;           // hud score texture
        Texture2D hudBars;            // hud energy/shield/boost bars texture

        Texture2D damageTexture;      // damage indication texture

        // list of currently playing 3D sounds
        List<Cue> cueSounds = new List<Cue>();
        // 3D sounds finished and ready to delete
        List<Cue> cueSoundsDelete = new List<Cue>();

        // global bone array used by DrawModel method
        Matrix[] bones = new Matrix[GameOptions.MaxBonesPerModel];

        /// <summary>
        /// Create a new game manager
        /// </summary>
        public GameManager(SoundBank soundBank)
        {
            sound = soundBank;
            animatedSprite = new AnimSpriteManager();
            projectile = new ProjectileManager(this);
            particle = new ParticleManager();
            powerup = new PowerupManager(this);
        }

        /// <summary>
        /// Get the game mode
        /// </summary>
        public GameMode GameMode
        {
            get { return gameMode; }
            set { gameMode = value; }
        }

        /// <summary>
        /// Set the game level
        /// </summary>
        public void SetLevel(String levelFileName)
        {
            levelFile = levelFileName;
        }

        /// <summary>
        /// Set the player ships and invert Y otions
        /// </summary>
        public void SetShips(String shipPlayer1, String shipPlayer2, uint invertYAxis)
        {
            shipFile[0] = shipPlayer1;
            shipFile[1] = shipPlayer2;
            invertY = invertYAxis;
        }

        /// <summary>
        /// Check if invert Y otions is enabled for a given player
        /// </summary>
        public bool GetInvertY(int player)
        {
            return (invertY & (1 << player)) != 0;
        }

        /// <summary>
        /// Get file name for the ship selected by a given player
        /// </summary>
        public String GetPlayerShip(int player)
        {
            return shipFile[player];
        }

        /// <summary>
        /// Get the winner player on a multiplayer match
        /// </summary>
        public int PlayerWinner
        {
            get
            {
                if (gameMode == GameMode.SinglePlayer)
                {
                    return 0;
                }
                else
                {
                    if (players[0].Score > players[1].Score)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
        }

        /// <summary>
        /// Load the game files (level, ships, wepons, etc...)
        /// </summary>
        public void LoadFiles(ContentManager content)
        {
            String level = levelFile + "/" + levelFile;

            // load level model
            levelColor = content.Load<Model>("levels/" + level);

            // load collision model
            Model collisionModel = content.Load<Model>(
                                        "levels/" + level + "_collision");
            levelCollision = new CollisionMesh(collisionModel,
                                        GameOptions.CollisionMeshSubdivisions);
            collisionModel = null;

            // load spawns and lights
            levelSpawns = EntityList.Load("content/levels/" + level + "_spawns.xml");
            levelLights = LightList.Load("content/levels/" + level + "_lights.xml");

            // load particle textures
            if (particleTextures == null)
            {
                int i, j = particleFiles.GetLength(0);
                particleTextures = new Texture2D[j];
                for (i = 0; i < j; i++)
                    particleTextures[i] = content.Load<Texture2D>(
                            "particles/" + particleFiles[i]);
            }

            // load animated sprite textures
            if (animatedSpriteTextures == null)
            {
                int i, j = animatedSpriteFiles.GetLength(0);
                animatedSpriteTextures = new Texture2D[j];
                for (i = 0; i < j; i++)
                    animatedSpriteTextures[i] = content.Load<Texture2D>(
                            "explosions/" + animatedSpriteFiles[i]);
            }

            // load projectile models
            if (projectileModels == null)
            {
                int i, j = projectileFiles.GetLength(0);
                projectileModels = new Model[j];
                for (i = 0; i < j; i++)
                    projectileModels[i] = content.Load<Model>(
                            "projectiles/" + projectileFiles[i]);
            }

            // load powerup models
            if (powerupModels == null)
            {
                int i, j = powerupFiles.GetLength(0);
                powerupModels = new Model[j];
                for (i = 0; i < j; i++)
                    powerupModels[i] = content.Load<Model>(
                            "powerups/" + powerupFiles[i]);
            }

            // cerate players
            for (int i = 0; i < GameOptions.MaxPlayers; i++)
                if (shipFile[i] != null)
                {
                    Model ShipModel = content.Load<Model>(
                            "ships/" + shipFile[i]);

                    EntityList ShipEnities = EntityList.Load(
                            "content/ships/" + shipFile[i] + ".xml");

                    players[i] = new PlayerShip(this, i,
                        ShipModel, ShipEnities, GameOptions.CollisionBoxRadius);
                }
                else
                    players[i] = null;

            // create powerups
            EntityList powerups = EntityList.Load(
                            "content/levels/" + level + "_powerups.xml");

            foreach (Entity entity in powerups.Entities)
            {
                switch (entity.name)
                {
                    case "energy":
                        AddPowerup(PowerupType.Energy, entity.transform);
                        break;
                    case "missile":
                        AddPowerup(PowerupType.Missile, entity.transform);
                        break;
                }
            }

            // load hud textures
            if (gameMode == GameMode.SinglePlayer)
            {
                hudCrosshair = content.Load<Texture2D>(
                                    "screens/hud_sp_crosshair");
                hudEnergy = content.Load<Texture2D>(
                                    "screens/hud_sp_energy");
                hudMissile = content.Load<Texture2D>(
                                    "screens/hud_sp_missile");
                hudScore = content.Load<Texture2D>(
                                    "screens/hud_sp_score");
                hudBars = content.Load<Texture2D>(
                                    "screens/hud_sp_bars");
            }
            else
            {
                hudCrosshair = content.Load<Texture2D>(
                                    "screens/hud_mp_crosshair");
                hudEnergy = content.Load<Texture2D>(
                                    "screens/hud_mp_energy");
                hudMissile = content.Load<Texture2D>(
                                    "screens/hud_mp_missile");
                hudScore = content.Load<Texture2D>(
                                    "screens/hud_mp_score");
                hudBars = content.Load<Texture2D>(
                                    "screens/hud_mp_bars");
            }

            // load damage indicator texture
            damageTexture = content.Load<Texture2D>("screens/damage");
        }


        /// <summary>
        /// Unload game files
        /// </summary>
        public void UnloadFiles()
        {
            // unload level
            levelColor = null;
            levelCollision = null;
            levelSpawns = null;
            levelLights = null;

            // unload poasticles
            particleTextures = null;
            // unload animated sprites
            animatedSpriteTextures = null;
            // unload projectiles
            projectileModels = null;
            // unload powerups
            powerupModels = null;

            // unload players
            for (int i = 0; i < GameOptions.MaxPlayers; i++)
            {
                // must displose player so that it releases its effects
                if (players[i] != null)
                    players[i].Dispose();
                players[i] = null;
            }

            // unload hud
            hudCrosshair = null;
            hudEnergy = null;
            hudMissile = null;
            hudScore = null;
            hudBars = null;

            // unload damage texture
            damageTexture = null;

            // unload powerups
            powerup.Clear();
        }

        /// <summary>
        /// Play a sound in 2D
        /// </summary>
        public void PlaySound(String soundName)
        {
            sound.PlayCue(soundName);
        }

        /// <summary>
        /// Play a sound in 3D at given position 
        /// (just fake 3D using distance attenuation but no stereo)
        /// </summary>
        public void PlaySound3D(String soundName, Vector3 position)
        {
            // get distance from sound to closest player
            float minimumDistance = 1e10f;
            for (int i = 0; i < GameOptions.MaxPlayers; i++)
                if (players[i] != null && players[i].IsAlive)
                {
                    float dist = (position - players[i].Position).LengthSquared();
                    if (dist < minimumDistance)
                        minimumDistance = dist;
                }

            // create a new sound instance
            Cue cue = sound.GetCue(soundName);
            cueSounds.Add(cue);

            // set volume based on distance from closest player
            cue.SetVariable("Distance", (float)Math.Sqrt(minimumDistance));

            // play sound 
            cue.Play();
        }

        /// <summary>
        /// Add vibration to the gamepad of the given player
        /// </summary>
        public void SetVibration(int player, float duration)
        {
            vibrationTime[player] = duration;
        }

        /// <summary>
        /// Add damage to all players inside a splash sphere with distance attenuation
        /// </summary>
        public void AddDamageSplash(int attacker,
            float damage, Vector3 position, float radius)
        {
            // check all players
            for (int i = 0; i < GameOptions.MaxPlayers; i++)
                // if player is alive
                if (players[i] != null && players[i].IsAlive)
                {
                    // get squared distance from player to splash center
                    Vector3 vec = players[i].Position - position;
                    float len = vec.LengthSquared();
                    // if player inside sphere
                    if (len < radius * radius)
                    {
                        // get actual length
                        len = (float)Math.Sqrt(len);

                        // compute damage intensity (squared not linear inside sphere)
                        float intensity = len / radius;
                        intensity = 1.0f - intensity * intensity;

                        // normalize vector used for pushing direction
                        vec *= 1.0f / len;

                        // apply damage and push player
                        AddDamage(attacker, i, intensity * damage, vec);
                    }
                }
        }

        /// <summary>
        /// Add damage to a player and check for player kill
        /// </summary>
        public void AddDamage(int attacker, int defender,
            float damage, Vector3 pushDirection)
        {
            // push defender for taking the damage
            players[defender].AddImpulseForce(5000 * pushDirection * damage);

            // apply damage to defender
            players[defender].AddEnergy(-damage);

            // set vibration on the defender gamepad
            SetVibration(defender, -0.3f);

            // if defender dies
            if (players[defender].IsAlive == false)
            {
                // compute explosion position
                Matrix m = players[defender].Transform;
                m.Translation += 25 * m.Forward;

                // add ship explosion animated sprite
                AddAnimSprite(AnimSpriteType.Ship,
                    m.Translation, 100, 0.0f, 20, DrawMode.AdditiveAndGlow, defender);

                // add ship explosion particle system
                AddParticleSystem(ParticleSystemType.ShipExplode, m);

                // if suicide
                if (attacker == defender)
                {
                    // attacker lose point
                    players[attacker].Score = Math.Max(0, players[attacker].Score - 1);
                }
                else
                {
                    // attacker win point
                    players[attacker].Score++;
                }

                // ship explosion adds splash damage
                AddDamageSplash(defender, 0.4f, m.Translation, 1000);

                // play explode sound
                PlaySound("ship_explode");
            }
        }

        /// <summary>
        /// Process game input
        /// </summary>
        public void ProcessInput(float elapsedTime, InputManager input)
        {
            // process input for player 1
            players[0].ProcessInput(elapsedTime, input, 0);

            // if in multiplayer mode, process input for player 2
            if (gameMode == GameMode.MultiPlayer)
                players[1].ProcessInput(elapsedTime, input, 1);
        }

        /// <summary>
        /// Update game for given elapsed time
        /// </summary>
        public void Update(float elapsedTime)
        {
            // update player 1
            players[0].Update(elapsedTime, levelCollision, levelSpawns);

            // if in multiplayer mode
            if (gameMode == GameMode.MultiPlayer)
            {
                // update player 2
                players[1].Update(elapsedTime, levelCollision, levelSpawns);

                // if both players are alive
                if (players[0].IsAlive && players[1].IsAlive)
                {
                    // test collision between players
                    Vector3 position1 = players[0].Position;
                    Vector3 position2 = players[1].Position;
                    CollisionBox player1Box = new CollisionBox(
                                                    position1 + players[0].box.min,
                                                    position1 + players[0].box.max);
                    CollisionBox player2Box = new CollisionBox(
                                                    position2 + players[1].box.min,
                                                    position2 + players[1].box.max);
                    // if player boxes intersect
                    if (player1Box.BoxIntersect(player2Box))
                    {
                        // compute push direction
                        Vector3 direction = Vector3.Normalize(position2 - position1);

                        // push players in oposide directions
                        direction *= GameOptions.ShipCollidePush;
                        players[0].AddImpulseForce(-direction);
                        players[1].AddImpulseForce(direction);

                        // play ship collide sound
                        PlaySound("ship_collide");
                    }
                }
            }

            // update animated sprites
            animatedSprite.Update(elapsedTime);

            // update animated projectiles
            projectile.Update(elapsedTime);

            // update particle systems
            particle.Update(elapsedTime);

            // update powerups
            powerup.Update(elapsedTime);

            // delete any finished 3D sounds
            cueSoundsDelete.Clear();
            foreach (Cue cue in cueSounds)
                if (cue.IsStopped)
                    cueSoundsDelete.Add(cue);
            foreach (Cue cue in cueSoundsDelete)
            {
                cueSounds.Remove(cue);
                cue.Dispose();
            }

            // if gamepad vibreate enabled
            if (GameOptions.UseGamepadVibrate)
            {
                // check vibration for each player
                for (int i = 0; i < GameOptions.MaxPlayers; i++)
                {
                    float leftMotorAmount = 0;
                    float rightMotorAmount = 0;

                    // if left vibration
                    if (vibrationTime[i] > 0)
                    {
                        leftMotorAmount = GameOptions.VibrationIntensity *
                            Math.Min(1.0f,
                            vibrationTime[i] / GameOptions.VibrationFadeout);
                        vibrationTime[i] = Math.Max(0.0f,
                            vibrationTime[i] - elapsedTime);
                    }
                    else
                        // if right vibration
                        if (vibrationTime[i] < 0)
                        {
                            rightMotorAmount = GameOptions.VibrationIntensity *
                                Math.Min(1.0f,
                                -vibrationTime[i] / GameOptions.VibrationFadeout);
                            vibrationTime[i] = Math.Min(0.0f,
                                    vibrationTime[i] + elapsedTime);
                        }

                    // set vibration values
                    GamePad.SetVibration((PlayerIndex)i, leftMotorAmount,
                        rightMotorAmount);
                }
            }
        }

        /// <summary>
        /// Draw the 3D game screen
        /// </summary>
        public void Draw3D(GraphicsDevice gd)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            // clear background
            gd.Clear(Color.Black);

            // draw scene
            DrawScene(gd, RenderTechnique.NormalMapping);
        }

        /// <summary>
        /// Draw the HUD interface
        /// </summary>
        void DrawHud(FontManager font, Rectangle rect, Vector3 bars,
            int barsLeft, int barsWidth, bool crosshair)
        {
            Rectangle r = new Rectangle(0, 0, 0, 0);

            // if crosshair enabled
            if (crosshair)
            {
                // draw crosshair hud texture
                r.X = rect.X + (rect.Width - hudCrosshair.Width) / 2;
                r.Y = rect.Y + (rect.Height - hudCrosshair.Height) / 2;
                r.Width = hudCrosshair.Width;
                r.Height = hudCrosshair.Height;
                font.DrawTexture(hudCrosshair, r,
                    Color.White, BlendState.AlphaBlend);
            }

            // draw score hud texture
            r.X = rect.X + (rect.Width - hudScore.Width) / 2;
            r.Y = rect.Y;
            r.Width = hudScore.Width;
            r.Height = hudScore.Height;
            font.DrawTexture(hudScore, r, Color.White, BlendState.AlphaBlend);

            // draw missile hud texture
            r.X = rect.X + rect.Width - hudMissile.Width;
            r.Y = rect.Y + rect.Height - hudMissile.Height;
            r.Width = hudMissile.Width;
            r.Height = hudMissile.Height;
            font.DrawTexture(hudMissile, r, Color.White, BlendState.AlphaBlend);

            // draw energy hud texture
            r.X = rect.X;
            r.Y = rect.Y + rect.Height - hudEnergy.Height;
            r.Width = hudEnergy.Width;
            r.Height = hudEnergy.Height;
            font.DrawTexture(hudEnergy, r, Color.White, BlendState.AlphaBlend);

            // get hud bars
            Rectangle s = new Rectangle(0, 0, hudBars.Width, hudBars.Height);

            // draw the energy bar
            r.Width = s.Width = barsLeft + (int)(barsWidth * bars.X);
            font.DrawTexture(hudBars, r, s, Color.Red, BlendState.Additive);

            // draw the shield bar
            r.Width = s.Width = barsLeft + (int)(barsWidth * bars.Y);
            font.DrawTexture(hudBars, r, s, Color.Green, BlendState.Additive);

            // draw the boost bar
            r.Width = s.Width = barsLeft + (int)(barsWidth * bars.Z);
            font.DrawTexture(hudBars, r, s, Color.Blue, BlendState.Additive);
        }

        /// <summary>
        /// Draw the 2D game screen
        /// </summary>
        public void Draw2D(FontManager font)
        {
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }

            Rectangle rect = font.ScreenRectangle;

            // if in single player mode
            if (gameMode == GameMode.SinglePlayer)
            {
                if (players[0].IsAlive)
                {
                    // draw hud 
                    DrawHud(font, rect, players[0].Bars, 70, 120,
                        players[0].Camera3rdPerson == false);

                    // draw missile count
                    font.DrawText(FontType.ArialMedium,
                        players[0].MissileCount.ToString(),
                        new Vector2(rect.Right - 138, rect.Bottom - 120),
                        Color.LightCyan);
                }

                // draw damage indicator
                Color DamageColor = players[0].DamageColor;
                if (DamageColor.A > 0)
                    font.DrawTexture(damageTexture, rect,
                        DamageColor, BlendState.AlphaBlend);
            }
            else
            {
                // multiplayer half horizontal screen
                rect.Width /= 2;

                // if player is alive
                if (players[0].IsAlive)
                {
                    // draw hud 
                    DrawHud(font, rect, players[0].Bars, 80, 100,
                        players[0].Camera3rdPerson == false);

                    // draw missile count
                    font.DrawText(FontType.ArialMedium,
                        players[0].MissileCount.ToString(),
                        new Vector2(rect.Right - 138, rect.Bottom - 125),
                        Color.LightCyan);
                }

                // draw damage indicator
                Color damageColor = players[0].DamageColor;
                if (damageColor.A > 0)
                    font.DrawTexture(damageTexture, rect,
                        damageColor, BlendState.AlphaBlend);

                // second player on second horizontal half
                rect.X += rect.Width;

                // if player is alive
                if (players[1].IsAlive)
                {
                    // draw hud
                    DrawHud(font, rect, players[1].Bars, 80, 100,
                        players[1].Camera3rdPerson == false);

                    // draw missile count
                    font.DrawText(FontType.ArialMedium,
                        players[1].MissileCount.ToString(),
                        new Vector2(rect.Right - 138, rect.Bottom - 125),
                        Color.LightCyan);
                }

                // draw damage indicator
                damageColor = players[1].DamageColor;
                if (damageColor.A > 0)
                    font.DrawTexture(damageTexture, rect,
                        damageColor, BlendState.AlphaBlend);

                // draw score
                font.DrawText(FontType.ArialLarge,
                    players[0].Score.ToString(),
                    new Vector2(rect.Width / 2 - 20, 20),
                    Color.LightCyan);
                font.DrawText(FontType.ArialLarge,
                    players[1].Score.ToString(),
                    new Vector2(rect.Width * 3 / 2 - 20, 20),
                    Color.LightCyan);
            }
        }

        /// <summary>
        /// Draw the 3D game scene
        /// </summary>
        void DrawScene(GraphicsDevice gd, RenderTechnique technique)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            if (gameMode == GameMode.SinglePlayer)
            {
                // camera position and view projection matrix
                Vector3 cameraPosition = players[0].CameraPosition;
                Matrix viewProjection = players[0].ViewMatrix * projectionFull;

                // draw the level geomery
                DrawModel(gd, levelColor, technique, cameraPosition,
                    Matrix.Identity, viewProjection, levelLights);

                // if in 3rd person mode draw player ship
                bool camera3rdPerson = players[0].Camera3rdPerson;
                if (camera3rdPerson)
                    players[0].Draw(gd, technique,
                        cameraPosition, viewProjection, levelLights);

                // draw projectiles
                projectile.Draw(gd, technique, cameraPosition, viewProjection,
                    levelLights);

                // draw powerups
                powerup.Draw(gd, technique, cameraPosition, viewProjection, levelLights);

                // draw animated sprites
                animatedSprite.Draw(gd, cameraPosition, players[0].ViewUp,
                    viewProjection, 0, camera3rdPerson);

                // draw particle systems
                particle.Draw(gd, viewProjection);
            }
            else
            {
                // set left viewport
                gd.Viewport = viewportLeft;

                // camera position and view projection matrix for player 1
                Vector3 cameraPosition = players[0].CameraPosition;
                Matrix viewProjection = players[0].ViewMatrix * projectionSplit;

                // draw the level geomery
                DrawModel(gd, levelColor, technique, cameraPosition,
                    Matrix.Identity, viewProjection, levelLights);

                // draw player 2 ship
                players[1].Draw(gd, technique, cameraPosition, viewProjection,
                    levelLights);

                // if in 3rd person mode draw player 1 ship
                bool camera3rdPerson = players[0].Camera3rdPerson;
                if (camera3rdPerson)
                    players[0].Draw(gd, technique,
                        cameraPosition, viewProjection, levelLights);

                // draw projectiles
                projectile.Draw(gd, technique, cameraPosition, viewProjection,
                    levelLights);

                // draw powerups
                powerup.Draw(gd, technique, cameraPosition, viewProjection, levelLights);

                // draw animated sprites
                animatedSprite.Draw(gd, cameraPosition, players[0].ViewUp,
                    viewProjection, 0, camera3rdPerson);

                // draw particle systems
                particle.Draw(gd, viewProjection);

                // setup right viewport
                gd.Viewport = viewportRight;

                // camera position and view projection matrix for player 2
                cameraPosition = players[1].CameraPosition;
                viewProjection = players[1].ViewMatrix * projectionSplit;

                // draw the level geomery
                DrawModel(gd, levelColor, technique, cameraPosition,
                    Matrix.Identity, viewProjection, levelLights);

                // draw player 1 ship
                players[0].Draw(gd, technique, cameraPosition, viewProjection,
                    levelLights);

                // if in 3rd person mode draw player 2 ship
                camera3rdPerson = players[1].Camera3rdPerson;
                if (camera3rdPerson)
                    players[1].Draw(gd, technique,
                        cameraPosition, viewProjection, levelLights);

                // draw projectiles
                projectile.Draw(gd, technique, cameraPosition, viewProjection,
                    levelLights);

                // draw powerups
                powerup.Draw(gd, technique, cameraPosition, viewProjection, levelLights);

                // draw animated sprites
                animatedSprite.Draw(gd, cameraPosition, players[1].ViewUp,
                    viewProjection, 1, camera3rdPerson);

                // draw particle systems
                particle.Draw(gd, viewProjection);
            }
        }

        /// <summary>
        /// Load content
        /// </summary>
        public void LoadContent(GraphicsDevice gd,
            ContentManager content)
        {
            // load reflection cubemap texture
            //reflectCube = content.Load<TextureCube>("Reflect");

            // load content for animated sprite manager
            animatedSprite.LoadContent(gd, content);

            // load content for particle system manager
            particle.LoadContent(gd, content);

            // set up projection matrix for full and slpit screen
            float aspect = (float)gd.Viewport.Width / (float)gd.Viewport.Height;
            projectionFull = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(60), aspect, 1.0f, 10000.0f);
            projectionSplit = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(60), aspect * 0.5f, 1.0f, 10000.0f);

            // viewport for split screen
            viewportLeft = gd.Viewport;
            viewportLeft.Width = gd.Viewport.Width / 2 - 1;
            viewportRight = viewportLeft;
            viewportRight.X = gd.Viewport.Width / 2 + 1;
        }

        /// <summary>
        /// Unload content
        /// </summary>
        public void UnloadContent()
        {
            UnloadFiles();

            // unload content for animated sprite manager
            animatedSprite.UnloadContent();

            // unload content for particle system manager
            particle.UnloadContent();
        }

        /// <summary>
        /// Get index for player at given 3D position 
        /// (returns -1 if no player at that position)
        /// </summary>
        public int GetPlayerAtPosition(Vector3 position)
        {
            for (int i = 0; i < GameOptions.MaxPlayers; i++)
                if (players[i] != null && players[i].IsAlive)
                    if (players[i].box.PointInside(position - players[i].Position))
                        return i;
            return -1;
        }

        /// <summary>
        /// Get the PlayerShip object for given player id)
        /// </summary>
        public PlayerShip GetPlayer(int playerId)
        {
            return players[playerId];
        }

        /// <summary>
        /// Create a new particle system and add it to the particle system manager
        /// </summary>
        public ParticleSystem AddParticleSystem(
            ParticleSystemType type,
            Matrix transform)
        {
            ParticleSystem ps = null;

            switch (type)
            {
                case ParticleSystemType.ShipExplode:
                    ps = new ParticleSystem(
                                ParticleSystemType.ShipExplode,
                                200,                    // num particles
                                0.0f,                   // emission angle (0 for omni)
                                0.8f, 0.8f,             // particle and total time
                                20.0f, 50.0f,           // min and max size
                                600.0f, 1000.0f,        // min and max vel
                                new Vector4(1.0f, 1.0f, 1.0f, 1.6f),    // start color
                                new Vector4(1.0f, 1.0f, 1.0f, 0.0f),    // end color
                                particleTextures[(int)type],          // texture
                                DrawMode.Additive,       // draw mode
                                transform);              // transform
                    break;
                case ParticleSystemType.ShipTrail:
                    ps = new ParticleSystem(
                                ParticleSystemType.ShipTrail,
                                100,                    // num particles
                                5.0f,                   // emission angle (0 for omni)
                                0.5f, 2.0f,             // particle time and total time
                                50.0f, 100.0f,          // min and max size
                                1000.0f, 1500.0f,       // min and max vel
                                new Vector4(0.5f, 0.2f, 0.0f, 1.0f),    // start color
                                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),    // end color
                                particleTextures[(int)type],            // texture
                                DrawMode.AdditiveAndGlow,  // draw mode
                                transform);                // transform
                    break;
                case ParticleSystemType.MissileExplode:
                    ps = new ParticleSystem(
                                ParticleSystemType.MissileExplode,
                                200,                    // num particles
                                0.0f,                   // emission angle (0 for omni)
                                0.5f, 0.5f,             // particle and total time
                                20.0f, 60.0f,           // min and max size
                                800.0f, 1200.0f,        // min and max vel
                                new Vector4(1.0f, 1.0f, 1.0f, 1.5f),    // start color
                                new Vector4(1.0f, 1.0f, 1.0f, -0.5f),   // end color
                                particleTextures[(int)type],          // texture
                                DrawMode.AdditiveAndGlow,      // draw mode
                                transform);              // transform
                    break;
                case ParticleSystemType.MissileTrail:
                    ps = new ParticleSystem(
                                ParticleSystemType.MissileTrail,
                                100,                    // num particles
                                10.0f,                  // emission angle (0 for omni)
                                0.5f, 1.0f,             // particle time and total time
                                15.0f, 30.0f,           // min and max size
                                1000.0f, 1500.0f,       // min and max vel
                                new Vector4(0.5f, 0.2f, 0.0f, 1.0f),    // start color
                                new Vector4(1.0f, 0.0f, 0.0f, 0.0f),    // end color
                                particleTextures[(int)type],          // texture
                                DrawMode.AdditiveAndGlow,      // draw mode
                                transform);              // transform
                    break;
                case ParticleSystemType.BlasterExplode:
                    ps = new ParticleSystem(
                                ParticleSystemType.BlasterExplode,
                                40,                      // num particles
                                2,                       // emission angle (0 for omni)
                                0.25f, 0.25f,            // particle time and total time
                                30.0f, 40.0f,            // min and max size
                                200.0f, 800.0f,          // min and max vel
                                new Vector4(1.0f, 1.0f, 1.0f, 1.5f),    // start color
                                new Vector4(1.0f, 1.0f, 1.0f, -0.2f),   // end color
                                particleTextures[(int)type],          // texture
                                DrawMode.AdditiveAndGlow,      // draw mode
                                transform);               // transform
                    break;
            }

            if (ps != null)
                particle.Add(ps);

            return ps;
        }

        /// <summary>
        /// Create a new animated sprite and add it to the animated sprite manager
        /// </summary>
        public AnimSprite AddAnimSprite(
            AnimSpriteType type,
            Vector3 position,
            float radius,
            float viewOffset,
            float frameRate,
            DrawMode mode,
            int player)
        {
            // create animated sprite
            AnimSprite a = new AnimSprite(type, position, radius, viewOffset,
                animatedSpriteTextures[(int)type], 256, 256,
                frameRate, mode, player);

            // add it to the animated sprite manager
            animatedSprite.Add(a);

            return a;
        }

        /// <summary>
        /// Create a new powerup and add it to the powerup manager
        /// </summary>
        public Powerup AddPowerup(
            PowerupType type,
            Matrix transform)
        {
            // create powerup
            Powerup p = new Powerup(type, transform, powerupModels[(int)type]);

            // add it to the powerup manager
            powerup.Add(p);

            return p;
        }

        /// <summary>
        /// Create a new projectile and add it to the projectile manager
        /// </summary>
        public Projectile AddProjectile(
            ProjectileType type,
            int player,
            Matrix transform,
            float velocity,
            float damage,
            RenderTechnique technique)
        {
            // get source and destination positions for projectile
            Vector3 source = transform.Translation;
            Vector3 destination = source + transform.Forward * 10000;

            // ray intersect level to find out where projetile is going to hit
            float hitDist;
            Vector3 hitPos, hitNormal;
            if (levelCollision.PointIntersect(source, destination,
                                        out hitDist, out hitPos, out hitNormal))
                destination = hitPos;
            else
                hitNormal = transform.Backward;

            // create projectile
            Projectile p = new Projectile(type, projectileModels[(int)type],
                player, velocity, damage, transform, destination, technique);

            // add it to the projectile manager
            projectile.Add(p);

            return p;
        }

        /// <summary>
        /// Draw a projectile
        /// </summary>
        public void DrawProjectile(GraphicsDevice gd, ProjectileType p,
            RenderTechnique technique, Vector3 cameraPosition,
            Matrix world, Matrix viewProjection, LightList lights)
        {
            DrawModel(gd, projectileModels[(int)p], technique,
                cameraPosition, world, viewProjection, lights);
        }

        /// <summary>
        /// Draw a model using given technique and camera settings
#endregion


        /// </summary>
        public void DrawModel(GraphicsDevice gd, Model model,
            RenderTechnique technique, Vector3 cameraPosition,
            Matrix world, Matrix viewProjection, LightList lights)
        {


            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            // get model bones
            model.CopyAbsoluteBoneTransformsTo(bones);

            BlendState bs = gd.BlendState;
            DepthStencilState ds = gd.DepthStencilState;

            gd.DepthStencilState = DepthStencilState.DepthRead;
            gd.BlendState = BlendState.Additive;


            // for each mesh in model
            foreach (ModelMesh mesh in model.Meshes)
            {
                // get mesh world matrix
                Matrix worldBone = bones[mesh.ParentBone.Index] * world;
                Matrix worldBoneInverse = Matrix.Invert(worldBone);

                // compute camera position in object space
                Vector3 cameraObjectSpace = cameraPosition - worldBone.Translation;
                cameraObjectSpace = Vector3.TransformNormal(cameraObjectSpace,
                                                        worldBoneInverse);

                gd.SamplerStates[0] = SamplerState.LinearWrap;

                // for each mesh part
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // if primitives to render
                    if (meshPart.PrimitiveCount > 0)
                    {
                        // setup vertices and indices
                        gd.SetVertexBuffer(meshPart.VertexBuffer);
                        gd.Indices = meshPart.IndexBuffer;

                        // setup effect
                        Effect effect = meshPart.Effect;
                        effect.Parameters["WorldViewProj"].SetValue(worldBone * viewProjection);
                        effect.Parameters["CameraPosition"].SetValue(cameraObjectSpace);

                        // setup technique
                        effect.CurrentTechnique =
                            meshPart.Effect.Techniques[(int)technique];


                        // if not lights specified
                        if (lights == null)
                        {
                            // begin effect
                            effect.CurrentTechnique.Passes[0].Apply();
                            // draw with plain mapping
                            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                meshPart.VertexOffset, 0, meshPart.NumVertices,
                                meshPart.StartIndex, meshPart.PrimitiveCount);
                            gd.SetVertexBuffer(null);
                            gd.Indices = null;
                        }
                        else
                        {
                            gd.DepthStencilState = DepthStencilState.Default;
                            gd.BlendState = BlendState.Opaque;

                            // get light effect parameters
                            EffectParameter effectLightPosition =
                                                effect.Parameters[1];
                            EffectParameter effectLightColor =
                                                effect.Parameters[2];
                            EffectParameter effectLightAmbient =
                                                effect.Parameters[3];

                            // ambient light
                            Vector3 ambient = lights.ambient;

                            // for each light
                            foreach (Light light in lights.lights)
                            {
                                // setup light in effect
                                effectLightAmbient.SetValue(ambient);
                                light.SetEffect(effectLightPosition,
                                    effectLightColor, worldBoneInverse);

                                // begin effect
                                effect.CurrentTechnique.Passes[0].Apply();
                                // draw primitives
                                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                    meshPart.VertexOffset, 0, meshPart.NumVertices,
                                    meshPart.StartIndex, meshPart.PrimitiveCount);

                                // setup additive blending with no depth write
                                gd.DepthStencilState = DepthStencilState.DepthRead;
                                gd.BlendState = BlendState.Additive;

                                // clear ambinet light (applied in first pass only)
                                ambient = Vector3.Zero;
                            }

                            // clear vertices and indices
                            gd.SetVertexBuffer(null);
                            gd.Indices = null;

                        }

                    }
                }
            }
            gd.DepthStencilState = ds;
            gd.BlendState = bs;
        }

        #region IDisposable Members

        bool isDisposed = false;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                if (animatedSprite != null)
                {
                    animatedSprite.Dispose();
                    animatedSprite = null;
                }
                if (particle != null)
                {
                    particle.Dispose();
                    particle = null;
                }
            }
        }
        #endregion
    }
}
