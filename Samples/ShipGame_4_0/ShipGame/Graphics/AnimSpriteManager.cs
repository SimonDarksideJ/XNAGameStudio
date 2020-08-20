#region File Description
//-----------------------------------------------------------------------------
// AnimSpriteManager.cs
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
    public enum DrawMode
    {
        Alpha = 0,
        Additive = 1,
        AlphaAndGlow = 2,
        AdditiveAndGlow = 3,
    }

    public class AnimSpriteManager : IDisposable
    {
        VertexBuffer[] vertexBuffer;          // vertex buffer for each player with 
        // all running animated sprites
        VertexDeclaration vertexDeclaration;  // vertex delcaration

        Effect effect;                        // the effect
        EffectTechnique effectTechnique;      // effect technique
        EffectParameter effectTexture;        // effect texture parameter
        EffectParameter effectFrameOffset;    // effect frame offset parameter
        EffectParameter effectFrameSize;      // effect frame size parameter
        EffectParameter effectFrameBlend;     // effect frame blend parameter
        EffectParameter effectViewProjection; // effect view projection parameter

        // the vertex array for all running animated sprites
        VertexPositionTexture[] vertices;

        // linked list of active animated sprites
        LinkedList<AnimSprite> animatedSprites;

        // linked list of nodes to delete from the animated sprites list
        List<LinkedListNode<AnimSprite>> deleteSprites;

        /// <summary>
        /// Create a new animated sprite manager
        /// </summary>
        public AnimSpriteManager()
        {
            vertices = new VertexPositionTexture[GameOptions.MaxSprites * 6];
            vertexBuffer = new VertexBuffer[GameOptions.MaxPlayers];
            animatedSprites = new LinkedList<AnimSprite>();
            deleteSprites = new List<LinkedListNode<AnimSprite>>();
        }

        /// <summary>
        /// Add a new animated sprite
        /// </summary>
        public bool Add(AnimSprite a)
        {
            if (animatedSprites.Count < GameOptions.MaxSprites)
            {
                animatedSprites.AddLast(a);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update all animated sprites
        /// </summary>
        public void Update(float elapsedTime)
        {
            // empty deleted sprites list
            deleteSprites.Clear();

            // for each animated sprite
            LinkedListNode<AnimSprite> Node = animatedSprites.First;
            while (Node != null)
            {
                // update animated sprite
                bool running = Node.Value.Update(elapsedTime);

                // if finished running add to delete list
                if (running == false)
                    deleteSprites.Add(Node);

                // move to next node
                Node = Node.Next;
            }

            // delete all nodes in delete list
            foreach (LinkedListNode<AnimSprite> s in deleteSprites)
                animatedSprites.Remove(s);
        }

        /// <summary>
        /// Draw all sprites aligning to given camera
        /// </summary>
        public void Draw(GraphicsDevice gd, Vector3 cameraPos, Vector3 cameraUp,
            Matrix viewProjection, int player, bool camera3rdPerson)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            // if no sprites to render, return
            if (animatedSprites.Count == 0)
                return;

            // for each animated sprite
            int vertexBufferPosition = 0;
            foreach (AnimSprite s in animatedSprites)
            {
                // add it to the vertex array
                s.AddToVertArray(vertices, vertexBufferPosition,
                    cameraPos, cameraUp, player, camera3rdPerson);

                // update vertex buffer position
                vertexBufferPosition += 6;
            }

            // set the vertex buffer data
            vertexBuffer[player].SetData<VertexPositionTexture>(vertices, 0,
                animatedSprites.Count * 6);

            // enable alpha blending and disable depth write
            gd.BlendState = BlendState.AlphaBlend;
            gd.DepthStencilState = DepthStencilState.DepthRead;

            // select technique
            effect.CurrentTechnique = effectTechnique;

            // set view projection matrix
            effectViewProjection.SetValue(viewProjection);

            // set vertex buffer and declaration
            gd.SetVertexBuffer(vertexBuffer[player]);

            // begin effect
            effect.CurrentTechnique.Passes[0].Apply();

            // for each animated sprite
            vertexBufferPosition = 0;
            foreach (AnimSprite sprite in animatedSprites)
            {
                // set effect parameters
                DrawMode mode = sprite.SetEffect(effectTexture, effectFrameOffset,
                                                effectFrameSize, effectFrameBlend);

                // if additive blend
                if (((int)mode & 1) != 0)
                    // set additive blend
                    gd.BlendState = BlendState.Additive;
                else
                    // set alpha blend
                    gd.BlendState = BlendState.AlphaBlend;

                // if glow enabled
                //if (((int)mode & 2) != 0)
                //    gd.RenderState.AlphaSourceBlend = Blend.One;
                //else
                //    gd.RenderState.AlphaSourceBlend = Blend.Zero;


                // draw the sprite quad
                gd.DrawPrimitives(PrimitiveType.TriangleList, vertexBufferPosition, 2);

                // update vertex buffer position
                vertexBufferPosition += 6;
            }


            // reset vertex buffer declaration
            gd.SetVertexBuffer(null);

            // reset blend and depth write
            gd.BlendState = BlendState.Additive;
            gd.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>
        /// Load content
        /// </summary>
        public void LoadContent(GraphicsDevice gd,
            ContentManager content)
        {
            if (gd == null)
            {
                throw new ArgumentNullException("gd");
            }

            // load effect
            effect = content.Load<Effect>("shaders/AnimSprite");

            // get techinque
            effectTechnique = effect.Techniques["AnimSprite"];

            // get parameters
            effectTexture = effect.Parameters["Texture"];
            effectFrameOffset = effect.Parameters["FrameOffset"];
            effectFrameSize = effect.Parameters["FrameSize"];
            effectFrameBlend = effect.Parameters["FrameBlend"];
            effectViewProjection = effect.Parameters["ViewProj"];

            // create the vertex declaration
            vertexDeclaration =
                new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());

            // create the vertex buffer
            vertexBuffer[0] = new VertexBuffer(gd, typeof(VertexPositionTexture),
                GameOptions.MaxSprites * 6, BufferUsage.WriteOnly);
            vertexBuffer[1] = new VertexBuffer(gd, typeof(VertexPositionTexture),
                GameOptions.MaxSprites * 6, BufferUsage.WriteOnly);
        }

        /// <summary>
        /// Unload content
        /// </summary>
        public void UnloadContent()
        {
            // unload effect and parameters
            effectTexture = null;
            effectFrameOffset = null;
            effectFrameSize = null;
            effectFrameBlend = null;
            effectViewProjection = null;
            effectTechnique = null;
            effect = null;
            // unload vertex buffer and declaration
            if (vertexBuffer[0] != null)
            {
                vertexBuffer[0].Dispose();
                vertexBuffer[0] = null;
            }
            if (vertexBuffer[1] != null)
            {
                vertexBuffer[1].Dispose();
                vertexBuffer[1] = null;
            }
            if (vertexDeclaration != null)
            {
                vertexDeclaration.Dispose();
                vertexDeclaration = null;
            }
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
                UnloadContent();
            }
        }
        #endregion
    }
}
