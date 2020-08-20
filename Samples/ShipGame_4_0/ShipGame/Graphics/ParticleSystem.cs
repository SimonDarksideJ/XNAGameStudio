#region File Description
//-----------------------------------------------------------------------------
// ParticleSystem.cs
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
    public class Particle
    {
        public Vector3 position;        // particle position
        public Vector3 velocity;        // particle velocity
        public Vector2 random;     // two normalized random numbers

        /// <summary>
        /// Create a new particle
        /// </summary>
        public Particle(Vector3 position, Vector3 velocity, Vector2 random)
        {
            this.position = position;
            this.velocity = velocity;
            this.random = random;
        }
    }
    
    public class ParticleSystem
    {
        ParticleSystemType particleType;    // particle system type

        bool enabled;               // is enabled? 
                                      // (when disabled will not update or render)
        DrawMode drawMode;          // drawing mode (alpha or additive and glow)

        bool loop;                  // is a loop? (if disabled will do a single burst)

        Texture2D texture;          // texture map for the particles

        float elapsedTime;          // elapsed time since activated
        float totalTime;            // total time for particle system
        float particleTime;         // particle life 
                                      // (if less than total time particles will loop)

        Vector4 startColor;         // start particle color and opacity
        Vector4 endColor;           // end particle color and opacity

        Vector2 velocity;           // start velocity range 
                                      // (random pick and end velocity is always zero)
        Vector2 pointSize;          // point size range (random pick)

        float emissionAngle;        // emission cone 
                                      // (0 for omni, >0 for more and more Z direction)
        float velocityScale;        // scale factor for particle velocity

        int count;                  // total number of particle
        int renderCount;            // number of particles in render buffer

        Matrix transform;           // the particle system position and orientation

        // the list of particles
        List<Particle> particles;

        // random generator
        Random random = new Random();

        /// <summary>
        /// Create a new particle system
        /// </summary>
        public ParticleSystem(
            ParticleSystemType type, 
            int count,
            float angle,
            float particleTime, 
            float totalTime, 
            float minimumSize, 
            float maximumSize, 
            float minimumVelocity, 
            float maximumVelocity, 
            Vector4 startColor, 
            Vector4 endColor, 
            Texture2D texture,
            DrawMode mode,
            Matrix transform)
        {
            particleType = type;
            enabled = true;
            loop = false;

            this.count = count;
            particles = new List<Particle>(count);

            velocity = new Vector2(minimumVelocity, maximumVelocity);

            pointSize = new Vector2(minimumSize, maximumSize);

            emissionAngle = angle;
            velocityScale = 1.0f;

            this.totalTime = totalTime;
            this.particleTime = particleTime;

            this.texture = texture;
            drawMode = mode;

            this.transform = transform;

            this.startColor = startColor;
            this.endColor = endColor;

            // if particle life is less then system life we have a loop
            if (particleTime < totalTime)
            {
                // start at a negative time offset so particles flow 
                // into birth when life turns positive
                elapsedTime = -particleTime;
                loop = true;
            }

            // create all particles
            CreateParticles();
        }

        /// <summary>
        /// Set the particle system position and orientation
        /// </summary>
        public void SetTransform(Matrix transform)
        {
            this.transform = transform;
        }

        /// <summary>
        /// Set the total life of the particle system 
        /// (set to 0 to destroy it before end)
        /// </summary>
        public void SetTotalTime(float totalTime)
        {
            this.totalTime = totalTime;
        }

        /// <summary>
        /// Get/Set the current velocity scale factor (1.0 for no scaling)
        /// </summary>
        public float VelocityScale
        {
            get { return velocityScale; }
            set { velocityScale = value; }
        }

        /// <summary>
        /// Enable/Disable the particle system
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Create the particle systems
        /// </summary>
        void CreateParticles()
        {
            Vector3 randomVelocity;
            float lengthSquared;

            // for each particle
            for( int i=0;i<count; i++ )
            {
                // get an equally distributed random direction
                do
                {
                    randomVelocity = new Vector3(
                           (float)random.NextDouble() - 0.5f,
                           (float)random.NextDouble() - 0.5f,
                           (float)random.NextDouble() - 0.5f);
                    lengthSquared = randomVelocity.LengthSquared();
                    // if outside sphere get another sample
                } while (lengthSquared > 1.0f);

                // add to the Z direction for a cone like emmission
                randomVelocity.Z += emissionAngle;

                // normalize direction
                randomVelocity = Vector3.Normalize(randomVelocity);

                // normalized random number
                float randomNumber = (float)random.NextDouble();

                // velocity vector from range of min and max values
                randomVelocity *= velocity.X * randomNumber + 
                    velocity.Y * (1.0f - randomNumber);

                // random scale and time offset 
                // (zero time offset to emitt all particles at same time)
                Vector2 randomVector = new Vector2((float)random.NextDouble(), 
                    loop ? (float)random.NextDouble() : 0);

                // add the particle to the list
                particles.Add(new Particle(Vector3.Zero, randomVelocity, randomVector));
            }
        }

        /// <summary>
        /// Update particle systems
        /// </summary>
        public bool Update(float elapsedTime)
        {
            // if enabled
            if (enabled)
            {
                // add elapsed time for this frame
                this.elapsedTime += elapsedTime;

                // if partcile system is finished
                if (this.elapsedTime > totalTime)
                    // return false to delete object
                    return false;
            }

            // return true to keep object alive
            return true;
        }

        /// <summary>
        /// Add the particle system to the given vertex array
        /// </summary>
        public int AddToVertArray(
            VertexPositionNormalTexture[] vertexBufffer,
            int vertexBufferPosition, 
            int pointsLeft)
        {
            int count = 0;

            // for each particle
            foreach (Particle p in particles)
            {
                // if still space in vertex array
                if (count >= pointsLeft)
                    break;

                // set position
                vertexBufffer[vertexBufferPosition + count].Position = p.position;
                // set velocity in vertex normal
                vertexBufffer[vertexBufferPosition + count].Normal = p.velocity;
                // set random vaues in texture coordinates
                vertexBufffer[vertexBufferPosition + count].TextureCoordinate = p.random;

                count++;
            }

            // store number of particles in render vertex array
            renderCount = count;

            return count;
        }

        /// <summary>
        /// Gets the number of particles to render
        /// (if negative tells how many particles to skip when disabled)
        /// </summary>
        public int RenderCount
        {
            get { return (enabled ? renderCount : -renderCount); }
        }

        /// <summary>
        /// Set the effect parameters for this particle system
        /// </summary>
        public DrawMode SetEffect(
            EffectParameter effectWorldViewProjection,
            EffectParameter effectTexture,
            EffectParameter effectStartColor,
            EffectParameter effectEndColor,
            EffectParameter effectTimes,
            EffectParameter effectPointSize,
            EffectParameter effectVelocityScale,
            Matrix viewProjection)
        {
            // set world view projection matrix
            if (effectWorldViewProjection != null)
            {
                effectWorldViewProjection.SetValue(transform * viewProjection);
            }

            // set texture
            if (effectTexture != null)
            {
                effectTexture.SetValue(texture);
            }
            
            // set start color
            if (effectStartColor != null)
            {
                effectStartColor.SetValue(startColor);
            }

            // set end color
            if (effectEndColor != null)
            {
                effectEndColor.SetValue(endColor);
            }

            // set elapsed time, particle time and total time
            if (effectTimes != null)
            {
                effectTimes.SetValue(new Vector3(elapsedTime, particleTime, totalTime));
            }

            // set minimum and maximum point sizes
            if (effectPointSize != null)
            {
                effectPointSize.SetValue(pointSize);
            }
            
            // set velocity scale
            if (effectVelocityScale != null)
            {
                effectVelocityScale.SetValue(velocityScale);
            }

            // return true to enable additive blending (if false alpha blending is used)
            return drawMode;
        }
    }
}
