#region File Description
//-----------------------------------------------------------------------------
// Board.cs
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
using Microsoft.Xna.Framework;
using System.Diagnostics;
#endregion

namespace Pickture
{
    /// <summary>
    /// A puzzle board.
    /// </summary>
    class Board
    {
        #region Fields

        // Board dimensions
        int width;
        int height;
        bool twoSided;

        // Board layout
        List<Chip> chips = new List<Chip>();
        Chip[,] layout;
        Matrix[,] boardPositionMatrices;
        const float ChipSpacing = 1.0f;
        int emptyX;
        int emptyY;        

        // Rendering properties
        Camera camera;
        Lighting lighting = new Lighting();
        LightingEffect lightingEffect = new LightingEffect();
        Model chipModel;
        Matrix[] chipTransforms;
        PictureSet currentPictureSet = null;
        PictureSet nextPictureSet = null;

        // Controls animating between textures
        float textureRotationTime = 0.0f;
        float textureRotationDuration = 2.5f;

        // Controls animated chip shifting
        List<Chip> shiftingChips = new List<Chip>();
        int shiftX;
        int shiftY;
        float currentShiftTime;
        const float ShiftDuration = 0.1f;

        #endregion

        #region Properties

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public bool TwoSided
        {
            get { return twoSided; }
        }


        public IEnumerable<Chip> Chips
        {
            get { return chips; }
        }

        public Chip GetChip(int x, int y)
        {
            return layout[x, y];
        }

        public int EmptyX
        {
            get { return emptyX; }
        }

        public int EmptyY
        {
            get { return emptyY; }
        }


        public Camera Camera
        {
            get { return this.camera; }
        }

        public LightingEffect LightingEffect
        {
            get { return lightingEffect; }
        }

        public PictureSet CurrentPictureSet
        {
            get { return currentPictureSet; }
        }

        public PictureSet NextPictureSet
        {
            get { return nextPictureSet; }
        }


        public bool IsShifting
        {
            get
            {
                return shiftingChips.Count > 0;
            }
        }       

        public int ShiftX
        {
            get { return shiftX; }
        }
        
        public int ShiftY
        {
            get { return shiftY; }
        }

        #endregion

        #region Initialization

        public Board(int sizeX, int sizeY, bool twoSided)
        {
            this.twoSided = twoSided;
            this.width = sizeX;
            this.height = sizeY;

            camera =
                new Camera((float)Math.Max(sizeX, sizeY) * 400.0f / 5.0f);

            nextPictureSet = PictureDatabase.GetNextPictureSet(twoSided ? 2 : 1);

            this.layout = new Chip[Width, Height];
            this.boardPositionMatrices = new Matrix[Width, Height];

            // Create all the chips
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Chip chip = new Chip();
                    chips.Add(chip);

                    chip.XPosition = x;
                    chip.YPosition = y;
                    layout[x, y] = chip;

                    Vector2 chipTexCoordFactor =
                        new Vector2(1.0f / Width, 1.0f / Height);

                    chip.TexCoordScale = chipTexCoordFactor;
                    chip.TexCoordTranslationFront =
                        new Vector2((x * chipTexCoordFactor.X),
                        ((Height - 1) - y) * chipTexCoordFactor.Y);
                    chip.TexCoordTranslationBack =
                        new Vector2(((Width - 1) - x) * chipTexCoordFactor.X,
                        ((Height - 1) - y) * chipTexCoordFactor.Y);
                }
            }

            // Remove one random chip
            emptyX = RandomHelper.Random.Next(0, Width);
            emptyY = RandomHelper.Random.Next(0, Height);
            Chip removed = layout[emptyX, emptyY];
            chips.Remove(removed);
            layout[emptyX, emptyY] = null;
        }

        public void LoadContent()
        {
            chipModel = Pickture.Instance.Content.Load<Model>("Models/Chip");

            // Cache the chip transforms
            chipTransforms = new Matrix[chipModel.Bones.Count];
            chipModel.CopyAbsoluteBoneTransformsTo(chipTransforms);

            // Now that we have the chip model, we can get its size
            // The size comes from mesh[0]'s bounding sphere radius which must be
            // transformed by the mesh's bone. Here, we simply scale by the X or Y
            // scale, whichever is larger. We don't care about the Z scale because
            // the chips are only placed in the XY plane.
            float sphereScale =
                Math.Max(chipTransforms[0].M11, chipTransforms[0].M22);
            float chipSize = chipModel.Meshes[0].BoundingSphere.Radius *
                sphereScale * ChipSpacing;

            // For each board location
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // We use the chip size to determine the transformation matrix
                    // for a chip at that location

                    Vector3 chipPos = new Vector3(
                        (Width - 1) * -0.5f, (Height - 1) * -0.5f, 0.0f);

                    chipPos += new Vector3(x, y, 0.0f);
                    chipPos *= chipSize;

                    boardPositionMatrices[x, y] = Matrix.CreateTranslation(chipPos);
                }
            }

            if (currentPictureSet != null)
                currentPictureSet.Load();
            if (nextPictureSet != null)
                nextPictureSet.Load();

            lightingEffect.LoadContent();
        }

        public void UnloadContent()
        {
            // Don't unload the current picture set, the completed screen may use it
            if (nextPictureSet != null)
                nextPictureSet.Unload();
        }

        #endregion

        #region Board interaction

        /// <summary>
        /// Determines if the puzzle has been completely solved.
        /// </summary>
        /// <returns>True when all of the chips are in the correct location with the
        /// correct orientation. Otherwise, returns false.</returns>
        public bool IsPuzzleComplete()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Chip chip = layout[x, y];
                    if (chip != null)
                    {
                        if (chip.XPosition != x || chip.YPosition != y)
                        {
                            return false;
                        }

                        if (!chip.HorizontalRotationCorrect ||
                            !chip.VerticalRotationCorrect)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
       
        /// <summary>
        /// Shifts a chip and all the chips inbetween towards the empty space.
        /// </summary>
        /// <param name="fromX">The X position of the chip to be shifted.</param>
        /// <param name="fromY">The Y position of the chip to be shifted.</param>
        public void Shift(int fromX, int fromY)
        {
            // If an animated shift is already in progress, do not shift again
            if (IsShifting)
                return;

            // Determine the shift direction
            shiftX = Math.Sign(emptyX - fromX);
            shiftY = Math.Sign(emptyY - fromY);
            
            // Shifts can only occur along axis aligned unit vectors
            if (Math.Abs(shiftX) + Math.Abs(shiftY) == 1)
            {
                currentShiftTime = 0.0f;
                Audio.Play("Shift Chip");

                // Loop from the empty space to the shift origin, shifting each chip
                // along the way, until the origin is now the empty space
                while (emptyX != fromX || emptyY != fromY)
                {
                    int nextEmptyX = emptyX - shiftX;
                    int nextEmptyY = emptyY - shiftY;

                    // Shift the current chip
                    Chip chip = layout[nextEmptyX, nextEmptyY];
                    layout[emptyX, emptyY] = chip;
                    // Include this chip for shifting animation
                    shiftingChips.Add(chip);

                    emptyX = nextEmptyX;
                    emptyY = nextEmptyY;
                }

                // Now this chip at the shift origin is the empty space
                layout[fromX, fromY] = null;
            }
        }

        #endregion

        #region Update and Draw

        public void Update(GameTime gameTime)
        {
            this.camera.Update(gameTime);
            this.lighting.Update(gameTime, camera);

            // When there is a "next picture set" animate a cross fade to it
            if (nextPictureSet != null)
            {
                textureRotationTime += (float)gameTime.ElapsedRealTime.TotalSeconds;
                if (textureRotationTime >= textureRotationDuration)
                {
                    // The current textures are no longer needed
                    if (currentPictureSet != null)
                        currentPictureSet.Unload();

                    // because the next texture set are now current
                    currentPictureSet = nextPictureSet;
                    nextPictureSet = null;

                    textureRotationTime = 0.0f;
                }
            }

            if (IsShifting)
            {
                // Animate shift time
                currentShiftTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (currentShiftTime > ShiftDuration)
                {
                    // Stop shifting
                    shiftingChips.Clear();
                    shiftX = 0;
                    shiftY = 0;
                }
            }

            // Update all chips
            foreach (Chip chip in chips)
                chip.Update(gameTime);
        }


        public void Draw()
        {
            lightingEffect.LightPos.SetValue(this.lighting.Position);
            lightingEffect.CameraPos.SetValue(this.camera.Position);

            DrawHelper.SetState();

            // For each board location
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Chip chip = layout[x, y];

                    // Nothing to draw for the empty space
                    if (chip == null)
                        continue;

                    // Determine the base transformation matrix for this chip. Only
                    // translation is accounted for here, orientation is added by the
                    // chip itself in its Draw method.
                    Matrix transform;
                    if (shiftingChips.Contains(chip))
                    {
                        // When a chip is shifted, it is put in its new location in the
                        // layout, but its rendered location is linearly interpolated
                        // back to its previous location.
                        Matrix.Lerp(ref boardPositionMatrices[x - shiftX, y - shiftY],
                            ref boardPositionMatrices[x, y],
                            currentShiftTime / ShiftDuration,
                            out transform);
                    }
                    else
                    {
                        transform = boardPositionMatrices[x, y];
                    }

                    chip.Draw(this, chipModel, transform, chipTransforms);
                }
            }
        }

        #endregion
    }
}
