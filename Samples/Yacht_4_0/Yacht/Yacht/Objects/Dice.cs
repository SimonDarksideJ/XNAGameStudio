#region File Description
//-----------------------------------------------------------------------------
// Dice.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Threading;


#endregion

namespace Yacht
{
    #region Enums
    

    /// <summary>
    /// Possible die values.
    /// </summary>
    public enum DiceValue : byte
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6
    }


    #endregion

    /// <summary>
    /// A six-sided die which can draw itself on screen.
    /// </summary>
    public class Dice : IComparable<Dice>
    {
        #region Fields


        static Random random = new Random();
        static Texture2D diceStrip;
        static Rectangle[] dice = new Rectangle[6];

        Timer timer = null;

        #endregion

        #region Loading
        

        /// <summary>
        /// Loads assets that will be used by all instances.
        /// </summary>
        /// <param name="content">Content manager to use when loading assets.</param>
        public static void LoadAssets(ContentManager content)
        {
            diceStrip = content.Load<Texture2D>(@"Images\dice");

            // Crete rectangles which designate the position of individual die faces in the dice strip texture which
            // contains all faces
            for (int i = 0; i < dice.Length; i++)
            {
                dice[i] = new Rectangle(i * diceStrip.Width / dice.Length, 0, 
                    diceStrip.Width / dice.Length, diceStrip.Height);
            }
        }


        #endregion

        #region Events and Properties


        /// <summary>
        /// Event launched when the die is tapped.
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Die's value.
        /// </summary>
        public DiceValue Value { get; set; }

        /// <summary>
        /// Die's position on the screen.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Whether the die is currently rolling or not.
        /// </summary>
        public bool IsRolling { get; private set; }


        #endregion

        #region Initialization


        /// <summary>
        /// Create a new die instance.
        /// </summary>
        public Dice()
        {
            Value = DiceValue.One;
        }


        #endregion

        #region Update and Render


        /// <summary>
        /// Update the die
        /// </summary>
        public void Update()
        {
            // Check if need to roll and skip some calls to make a delay effect 
            // of the rolling.

            if (IsRolling && timer == null)
            {
                timer = new Timer(RandomizeDiceMotion, null, random.Next(300, 600), -1);
            }
        }

        /// <summary>
        /// Handle the users input to check if the die was tapped.
        /// </summary>
        /// <param name="sample">Input gesture.</param>
        public void HandleInput(GestureSample sample)
        {
            if (sample.GestureType == GestureType.Tap)
            {
                // Create the touch rectangle
                Rectangle touchRect = new Rectangle((int)sample.Position.X - 1, (int)sample.Position.Y - 1, 2, 2);

                // Create the die bounds rectangle
                Rectangle bounds = dice[0];
                bounds.X += (int)Position.X;
                bounds.Y += (int)Position.Y;

                // Check for intersection between the rectangles
                if (bounds.Intersects(touchRect) && Click != null)
                {
                    Click(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Draw the die on the screen
        /// </summary>
        /// <param name="spriteBatch">A <see cref="SpriteBatch"/> to draw the die with.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Create the destination rectangle according to the die's position
            Rectangle bounds = dice[0];
            bounds.X += (int)Position.X;
            bounds.Y += (int)Position.Y;

            // Draw the die on the screen
            spriteBatch.Draw(diceStrip, bounds, dice[(int)Value - 1], Color.White);
        }


        #endregion

        private void RandomizeDiceMotion(object obj)
        {
            timer = null;
            Value = (DiceValue)random.Next(1, 7);
            IsRolling = random.Next(0, 5) != 1;
        }

        #region Public Methods


        /// <summary>
        /// Start rolling the die.
        /// </summary>
        public void Roll()
        {
            IsRolling = true;
        }

        /// <summary>
        /// Compare between two dice.
        /// </summary>
        /// <param name="other">The die to compare with.</param>
        /// <returns>0 if both dice are equal, 1 if the current instance is greater and -1 otherwise.</returns>
        public int CompareTo(Dice other)
        {
            if (other != null)
            {
                if ((int)Value > (int)other.Value)
                {
                    return 1;
                }
                else if ((int)Value < (int)other.Value)
                {
                    return -1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Check for intersection between a rectangle and the die's bounds.
        /// </summary>
        /// <param name="rect">The rectangle to check for intersection.</param>
        /// <returns>True if the rectangle intersects with the die and false otherwise.</returns>
        public bool Intersects(Rectangle rect)
        {
            // Get the die's bounds
            Rectangle bounds = dice[0];
            bounds.X += (int)Position.X;
            bounds.Y += (int)Position.Y;

            // Return the intersection result
            return bounds.Intersects(rect);
        }


        #endregion
    }
}
