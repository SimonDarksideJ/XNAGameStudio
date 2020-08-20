#region File Description
//-----------------------------------------------------------------------------
// TracerBullet.cs
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
using RobotGameData.GameObject;
using RobotGameData.Resource;
using RobotGameData.GameInterface;
#endregion

namespace RobotGame
{
    #region BulletObject

    /// <summary>
    /// an object with information of a trajectory of a bullet.
    /// Contains the position, length, direction, and speed of trajectory.
    /// </summary>
    public class BulletObject : IIdentity
    {
        #region Fields

        int id = -1;
        bool isActive = false;
        bool possibleToMissing = false;

        Vector3 startPosition = Vector3.Zero;
        Vector3 destinationPosition = Vector3.Zero;
        Vector3 direction = Vector3.Forward;
        Vector3 headPosition = Vector3.Zero;
        Vector3 tailPosition = Vector3.Zero;
        Vector3 velocity = Vector3.Zero;

        float speed = 0.0f;
        float length = 0.0f;
        float untilDestinationDistance = 0.0f;
        float untilCurrentDistance = 0.0f;

        #endregion

        #region Properties

        public int Id { get { return id; } }
        public Vector3 StartPosition { get { return startPosition; } } 
        public Vector3 DestinationPosition { get { return destinationPosition; } }
        public Vector3 Direction { get { return direction; } }
        public Vector3 HeadPosition { get { return headPosition; } }
        public Vector3 TailPosition { get { return tailPosition; } }
        public float Speed { get { return speed; } }
        public float Length { get { return length; } }
        public bool PossibleToMissing { get { return possibleToMissing; } }
        public bool IsActive { get { return isActive; } }
        public bool Arrived 
        { 
            get { return (untilCurrentDistance >= untilDestinationDistance); }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public BulletObject(int id)
        {
            this.id = id;

            Reset();
        }

        public void Reset()
        {
            this.isActive = false;
            this.startPosition = Vector3.Zero;
            this.destinationPosition = Vector3.Zero;
            this.direction = Vector3.Zero;
            this.headPosition = Vector3.Zero;
            this.tailPosition = Vector3.Zero;

            speed = 0.0f;
            length = 0.0f;
            untilDestinationDistance = 0.0f;
            untilCurrentDistance = 0.0f;

            this.isActive = false;
        }

        public void Fire(Vector3 startPosition, Vector3 destinationPosition,
                        float speed, float lengh, bool possibleToMissing)
        {
            this.startPosition = startPosition;
            this.destinationPosition = destinationPosition;
            this.direction = destinationPosition - startPosition;
            this.direction = Vector3.Normalize(this.direction);
            this.speed = speed;
            this.length = lengh;
            this.possibleToMissing = possibleToMissing;

            this.headPosition = startPosition;
            this.tailPosition = startPosition + (this.direction * this.length);

            this.untilCurrentDistance = 0;
            this.untilDestinationDistance = 
                Vector3.Distance(this.DestinationPosition, this.startPosition);

            this.isActive = true;
        }

        public void FrameMove(GameTime gameTime)
        {
            this.velocity = this.direction * this.speed;
            this.velocity *= (float)gameTime.ElapsedGameTime.TotalSeconds;

            this.headPosition += this.velocity;
            this.tailPosition = this.direction * this.length;
            this.tailPosition += this.headPosition;

            this.untilCurrentDistance =
                    Vector3.Distance(this.tailPosition, this.startPosition);
        }
    }

    #endregion

    /// <summary>
    /// Scene node uses GameBillboard and each bullet creates 
    /// information in BullectObject as much as the number of the instances 
    /// and the class automatically manages.  When the shot bullet reaches 
    /// the specific point, it becomes disabled and gets removed from screen 
    /// and used again at re-firing.
    /// </summary>
    public class TracerBullets : IIdentity
    {
        #region Fields

        int id = -1;
        BulletObject[] bullets = null;

        GameResourceTexture2D textureResource = null;
        GameBillboard tracerBulletBillboard = new GameBillboard();

        #endregion

        #region Properties

        public int Id
        {
            get { return id; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instanceCount">creating instance count</param>
        /// <param name="textureFileName">texture file name</param>
        public TracerBullets(int id, int instanceCount, string textureFileName, 
                            GameSceneNode sceneParent)
        {
            this.id = id;

            textureResource = FrameworkCore.ResourceManager.LoadTexture(textureFileName);

            bullets = new BulletObject[instanceCount];

            //  Create instance bullets
            for (int i = 0; i < bullets.Length; i++)
            {
                bullets[i] = new BulletObject(i);
            }

            tracerBulletBillboard = new GameBillboard();
            tracerBulletBillboard.Create(instanceCount, textureResource.Texture2D, 
                                        RenderingSpace.World, false);

            tracerBulletBillboard.SourceBlend = Blend.SourceAlpha;
            tracerBulletBillboard.DestinationBlend = Blend.One;
            tracerBulletBillboard.BlendFunction = BlendFunction.Add;
            tracerBulletBillboard.AlphaBlendEnable = true;
            tracerBulletBillboard.DepthBufferEnable = true;
            tracerBulletBillboard.DepthBufferWriteEnable = false;
            tracerBulletBillboard.DepthBufferFunction = CompareFunction.Less;
            tracerBulletBillboard.AlphaFunction = CompareFunction.Greater;
            tracerBulletBillboard.CullMode = CullMode.None;

            sceneParent.AddChild(tracerBulletBillboard);
        }

        public void UnloadContent()
        {
            this.tracerBulletBillboard.RemoveFromParent();

            FrameworkCore.ResourceManager.RemoveResourceByObject(textureResource, true);
        }

        /// <summary>
        /// shoots a bullet object.
        /// A bullet object uses a billboard.
        /// Moves from the start position to the target position and 
        /// gets automatically managed.
        /// Becomes disabled when the bullet object reaches the target position.
        /// </summary>
        /// <param name="startPosition">the start position of bullet</param>
        /// <param name="destinationPosition">the target position of bullet</param>
        /// <param name="speed">the speed of bullet</param>
        /// <param name="length">the length of bullet</param>
        /// <param name="thickness">the thickness of bullet</param>
        /// <param name="possibleToMissing"></param>
        /// <returns></returns>
        public bool Fire(Vector3 startPosition, Vector3 destinationPosition,
            float speed, float length, float thickness, bool possibleToMissing)
        {
            float fireDistance =
                Math.Abs(Vector3.Distance(startPosition, destinationPosition));

            //  if the shooting distance is shorter than the bullet, 
            //  the firing won't be necessary.
            if (fireDistance <= length)
                return false;

            for (int i = 0; i < bullets.Length; i++)
            {
                //  it looks for the inactive instance.
                if (bullets[i].IsActive == false)
                {
                    int index = bullets[i].Id;

                    bullets[i].Fire(startPosition, destinationPosition,
                                    speed, length, possibleToMissing);

                    tracerBulletBillboard.SetUpdateType(index, true);
                    tracerBulletBillboard.SetSize(index, thickness);

                    //  Already find a free bullet.
                    return true;
                }
            }

            //  If all the bullets are busy.
            for (int i = 0; i < bullets.Length; i++)
            {
                //  It looks for an instance, which would be OK to be lost.
                if (bullets[i].PossibleToMissing )
                {
                    bullets[i].Fire(startPosition, destinationPosition,
                                    speed, length, possibleToMissing);

                    tracerBulletBillboard.SetUpdateType(i, true);
                    tracerBulletBillboard.SetSize(i, thickness);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// reset all bullet objects.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < bullets.Length; i++)
            {
                bullets[i].Reset();
            }
        }

        /// <summary>
        /// udpate all bullet objects.
        /// A bullet object, which has been fired, flies to the target position 
        /// while getting updated.
        /// </summary>
        public void Drive(GameTime gameTime)
        {
            //  Check all instance bullets
            for (int i = 0; i < bullets.Length; i++)
            {
                int index = bullets[i].Id;

                if (bullets[i].IsActive )
                {
                    //  if the bullet has reached the target,
                    if (bullets[i].Arrived )
                    {
                        bullets[i].Reset();

                        tracerBulletBillboard.SetUpdateType(index, false);
                    }
                    //  if the bullet has not reached the target, it keeps moving
                    else
                    {
                        bullets[i].FrameMove(gameTime);

                        //  Updates a bullet billboard
                        tracerBulletBillboard.SetStart(index, bullets[i].HeadPosition);
                        tracerBulletBillboard.SetEnd(index, bullets[i].TailPosition);
                        tracerBulletBillboard.SetUpdateType(index, true);  
                    }
                }
                else
                {
                    tracerBulletBillboard.SetUpdateType(index, false);  
                }
            }
        }
    }
}
