#region File Description
//-----------------------------------------------------------------------------
// ProjectileManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion



namespace ShipGame
{
    public class ProjectileManager
    {
        GameManager game;        // game manager

        // linked list of active projectiles
        LinkedList<Projectile> projectiles;

        // linked list of nodes to delete from the projectiles list
        List<LinkedListNode<Projectile>> deleteProjectiles;

        /// <summary>
        /// Create a new projetcile manager
        /// </summary>
        public ProjectileManager(GameManager game)
        {
            this.game = game;

            projectiles = new LinkedList<Projectile>();
            deleteProjectiles = new List<LinkedListNode<Projectile>>();
        }

        /// <summary>
        /// Add a new projectile
        /// </summary>
        public void Add(Projectile p)
        {
            projectiles.AddLast(p);
        }

        /// <summary>
        /// Update all projectiles
        /// </summary>
        public void Update(float elapsedTime)
        {
            // empty deleted projectiles list
            deleteProjectiles.Clear();

            // for each powerup
            LinkedListNode<Projectile> Node = projectiles.First;
            while (Node != null)
            {
                // update projectile
                bool running = Node.Value.Update(elapsedTime, game);

                // if finished running add to delete list
                if (running == false)
                    deleteProjectiles.Add(Node);

                // move to next node
                Node = Node.Next;
            }

            // delete all nodes in delete list
            foreach (LinkedListNode<Projectile> p in deleteProjectiles)
                projectiles.Remove(p);
        }

        /// <summary>
        /// Draw all projectiles
        /// </summary>
        public void Draw(GraphicsDevice gd, RenderTechnique technique, 
            Vector3 cameraPosition, Matrix viewProjection, LightList lights)
        {
            // draw all projectiles
            foreach(Projectile p in projectiles)
                p.Draw(game, gd, technique, cameraPosition, viewProjection, lights);
        }
    }
}
