#region File Description
//-----------------------------------------------------------------------------
// Avatar.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace AvatarShadows
{  
    /// <summary>
    /// Contains all of the data needed to render a single avatar.
    /// </summary>
    public class Avatar
    {
        // We use one random number generator for all avatars
        private static readonly Random random = new Random();

        /// <summary>
        /// Gets the animation used by the avatar.
        /// </summary>
        public AvatarAnimation Animation { get; private set; }

        /// <summary>
        /// Gets the description of the avatar.
        /// </summary>
        public AvatarDescription Description { get; private set; }

        /// <summary>
        /// Gets the renderer used by the avatar.
        /// </summary>
        public AvatarRenderer Renderer { get; private set; }

        /// <summary>
        /// Gets or sets the world matrix for drawing this avatar.
        /// </summary>
        public Matrix World { get; set; }

        public Avatar()
        {
            // We pick a random animation and description for each avatar we create
            Animation = new AvatarAnimation((AvatarAnimationPreset)random.Next(30));
            Description = AvatarDescription.CreateRandom();
            Renderer = new AvatarRenderer(Description, false);
        }
    }
}
