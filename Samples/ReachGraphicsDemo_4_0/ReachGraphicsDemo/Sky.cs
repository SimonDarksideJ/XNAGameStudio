#region File Description
//-----------------------------------------------------------------------------
// Sky.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace GeneratedGeometry
{
    /// <summary>
    /// Runtime class for loading and rendering a textured skydome
    /// that was created during the build process by the SkyProcessor.
    /// </summary>
    public class Sky
    {
        #region Fields

        public Model Model;
        public Texture2D Texture;

        #endregion


        /// <summary>
        /// Helper for drawing the skydome mesh.
        /// </summary>
        public void Draw(Matrix view, Matrix projection)
        {
            GraphicsDevice GraphicsDevice = Texture.GraphicsDevice;

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            view.Translation = Vector3.Zero;

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.View = view;
                    effect.Projection = projection;
                    effect.Texture = Texture;
                    effect.TextureEnabled = true;
                }

                mesh.Draw();
            }
        }
    }
}
