#region File Description
//-----------------------------------------------------------------------------
// TracerBulletManager.cs
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
using RobotGameData;
#endregion

namespace RobotGame
{
    /// <summary>
    /// It creates TracerBullets class, which represents the trajectory of bullets, 
    /// and updates the class automatically.
    /// In order to show the bullets’ trajectories, 
    /// use the Fire() function and give the starting point and 
    /// the end point to activate.
    /// </summary>
    public class TracerBulletManager
    {
        #region Fields

        /// <summary>
        /// If set to false, all of the related functions get turned off.
        /// </summary>
        bool activeOn = true;

        List<TracerBullets> tracerBullets = new List<TracerBullets>();

        #endregion

        /// <summary>
        /// create a bullet group and create bullet instances.
        /// </summary>
        /// <param name="id">bullet group id</param>
        /// <param name="instanceCount">object count</param>
        /// <param name="textureFileName">bullet texture file</param>
        /// <param name="sceneParent">3D scene parent node</param>
        public void AddBulletInstance(int id, int instanceCount, string textureFileName, 
                        GameSceneNode sceneParent)
        {
            if (activeOn == false) return;

            TracerBullets defaultBullet = new TracerBullets(id, instanceCount, 
                                                        textureFileName, sceneParent);

            tracerBullets.Add( defaultBullet);            
        }

        /// <summary>
        /// remove all bullet groups.
        /// </summary>
        public void UnloadContent()
        {
            foreach(TracerBullets bullets in tracerBullets)
            {
                bullets.UnloadContent();
            }

            tracerBullets.Clear();
        }

        /// <summary>
        /// update all bullet groups.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (activeOn == false) return;

            foreach(TracerBullets bullets in tracerBullets)
            {
                bullets.Drive(gameTime);
            }
        }

        /// <summary>
        /// fire a bullet.
        /// </summary>
        /// <param name="id">bullet group id</param>
        /// <param name="startPosition">the start position of bullet</param>
        /// <param name="destinationPosition">the target position of bullet</param>
        /// <param name="speed">the speed of bullet</param>
        /// <param name="length">the length of bullet</param>
        /// <param name="thickness">the thickness of bullet</param>
        /// <param name="possibleToMissing"></param>
        /// <returns></returns>
        public bool Fire(int id, Vector3 startPosition, Vector3 destinationPosition,
            float speed, float length, float thickness, bool possibleToMissing)
        {
            if (activeOn == false) return false;

            TracerBullets findBullets = null;

            foreach(TracerBullets bullets in tracerBullets)
            {
                if( id == bullets.Id)
                    findBullets = bullets;
            }

            if (findBullets == null)
                throw new InvalidOperationException("Cannot find bullets ID");

            return findBullets.Fire(startPosition, destinationPosition, speed, length,
                                    thickness, possibleToMissing);
        }
    }
}
