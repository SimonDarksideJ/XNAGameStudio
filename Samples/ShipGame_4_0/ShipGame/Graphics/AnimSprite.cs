#region File Description
//-----------------------------------------------------------------------------
// AnimSprite.cs
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
    public class AnimSprite
    {
        AnimSpriteType spriteType;        // animated sprite type

        Vector3 position;           // the sprite position
        float radius;               // the sprite radius
        float viewOffset;           // view offset moves sprite in direction of camera
        int player;                 // the player it is related to (-1 for no player)

        Texture2D texture;          // the texture grid with the animation frames
        Vector2 frameSize;          // frame size in X and Y directions
        DrawMode drawMode;          // drawing mode (alpha or additive and glow)

        float frameRate;            // framerate to play animation frames
        float elapsedTime;          // elapsed time since animation start
        float totalTime;            // total animation time

        int numberFrames;              // number of frames in texture grid
        int numberFramesX;             // number of frames per row in the texture grid
        int numberFramesY;             // number of frames per column in the texture grid

        // base quad texture coordinates
        static Vector2[] QuadTexCoords = new Vector2[4]{ new Vector2(0,0), 
                                                  new Vector2(1,0), 
                                                  new Vector2(1,1), 
                                                  new Vector2(0,1) };


        /// <summary>
        /// The animated sprite position
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// The animated sprite radius
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }


        /// <summary>
        /// Create a new animated sprite
        /// </summary>
        public AnimSprite(
                AnimSpriteType type,
                Vector3 position, float radius, float viewOffset,
                Texture2D texture, int frameSizeX, int frameSizeY,
                float frameRate, DrawMode mode, int player)
        {
            if (texture == null)
            {
                throw new ArgumentNullException("texture");
            }

            spriteType = type;
            this.position = position;
            this.radius = radius;
            this.viewOffset = viewOffset;
            this.texture = texture;
            this.player = player;
            this.frameRate = frameRate;
            this.drawMode = mode;
            
            // frame size
            float sizeX = (float)frameSizeX / (float)texture.Width;
            float sizeY = (float)frameSizeY / (float)texture.Height;
            frameSize = new Vector2(sizeX, sizeY);

            // number of frames
            numberFramesX = texture.Width / frameSizeX;
            numberFramesY = texture.Height / frameSizeY;
            numberFrames = numberFramesX * numberFramesY;

            // total animation time
            totalTime = (float)numberFrames / frameRate;
            elapsedTime = 0;
        }

        /// <summary>
        /// Updates the animated sprite for given elapsed time and 
        /// return false when animation is finished and object can be released
        /// </summary>
        public bool Update(float elapsedTime)
        {
            // add frame elapsed time
            this.elapsedTime += elapsedTime;

            // if total time reached, return false to destroy object
            if (this.elapsedTime > totalTime)
                return false;

            // return true to keep object alive
            return true;
        }

        /// <summary>
        /// Set the animation total time 
        /// (set to zero to delete object before it is finished)
        /// </summary>
        public void SetTotalTime(float totalTime)
        {
            this.totalTime = totalTime;
        }

        /// <summary>
        /// Add the animated sprite geometry to the given vertex array 
        /// aligning it to the given camera
        /// </summary>
        public void AddToVertArray(VertexPositionTexture[] vertexBuffer, 
            int vertexBufferPosition, Vector3 cameraPosition, Vector3 cameraUp, 
            int player, bool camera3rdPerson)
        {
            int maximumVertexBufferPosition = vertexBufferPosition + 5;
            if ((maximumVertexBufferPosition < vertexBufferPosition) || // overflow
                (maximumVertexBufferPosition >= vertexBuffer.Length)) // too many
            {
                throw new ArgumentOutOfRangeException("vertexBufferPosition");
            }

            // view direction
            Vector3 viewDirection = position - cameraPosition;

            // distance from camera to animated sprite
            float viewDistance = viewDirection.Length();

            // normalize view direction
            viewDirection *= 1.0f / viewDistance;

            // animated sprite X axis is the right vector 
            Vector3 right = Vector3.Normalize(Vector3.Cross(cameraUp,viewDirection));
            // animated sprite Y axis is the up vector 
            Vector3 up = Vector3.Normalize(Vector3.Cross(viewDirection, right));
            
            // view direction offset used to show the animated sprits in front
            // of other objects always moving in the view diretion
            Vector3 offset = Vector3.Zero;
            if (camera3rdPerson == true || player != this.player) 
                if (viewOffset < viewDistance*0.5f)
                    offset = -viewDirection * viewOffset;
                else
                    offset = -viewDirection * (viewDistance * 0.5f);

            // setup quad vertices
            vertexBuffer[vertexBufferPosition].Position = 
                position + offset + radius * (right + up);
            vertexBuffer[vertexBufferPosition].TextureCoordinate = QuadTexCoords[0];
            vertexBuffer[vertexBufferPosition + 1].Position = 
                position + offset + radius * (-right+up);
            vertexBuffer[vertexBufferPosition + 1].TextureCoordinate = QuadTexCoords[1];
            vertexBuffer[vertexBufferPosition + 2].Position = 
                position + offset + radius * (-right-up);
            vertexBuffer[vertexBufferPosition + 2].TextureCoordinate = QuadTexCoords[2];
            vertexBuffer[vertexBufferPosition + 3].Position = 
                position + offset + radius * (right-up);
            vertexBuffer[vertexBufferPosition + 3].TextureCoordinate = QuadTexCoords[3];
            vertexBuffer[vertexBufferPosition + 4] = vertexBuffer[vertexBufferPosition];
            vertexBuffer[vertexBufferPosition + 5] = 
                vertexBuffer[vertexBufferPosition + 2];
        }

        /// <summary>
        /// Set the effect parameters for this animated sprite
        /// </summary>
        public DrawMode SetEffect(
                EffectParameter effectTexture,
                EffectParameter effectFrameOffset,
                EffectParameter effectFrameSize,
                EffectParameter effectFrameBlend)
        {
            // set texture
            if (effectTexture != null)
            {
                effectTexture.SetValue(texture);
            }
        
            // calculate opacity based on squared normalized life time
            float opacity = Math.Min(1.0f, elapsedTime / totalTime);
            opacity = 1.0f - opacity*opacity;

            // calculate the float frame position used for frame blending
            float floatFrame = elapsedTime * frameRate;

            // get the two frames to blend
            int frame = Math.Min(numberFrames - 1, (int)floatFrame);
            int nextFrame = Math.Min(numberFrames - 1, (frame + 1));

            // set frame size
            if (effectFrameSize != null)
            {
                effectFrameSize.SetValue(frameSize);
            }

            // set frame offset
            Vector4 frameOffset = new Vector4(
                    frame % numberFramesX, frame / numberFramesX,
                    nextFrame % numberFramesX, nextFrame / numberFramesX);
            if (effectFrameOffset != null)
            {
                effectFrameOffset.SetValue(frameOffset);
            }

            // set blend factor
            float blendFactor = floatFrame - (float)frame;
            if (effectFrameBlend != null)
            {
                effectFrameBlend.SetValue(new Vector2(blendFactor, 2 * opacity));
            }

            // return true to enable additive blending (if false alpha blending is used)
            return drawMode;
        }
    }
}
