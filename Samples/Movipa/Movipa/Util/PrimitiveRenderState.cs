#region File Description
//-----------------------------------------------------------------------------
// PrimitiveRenderState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Movipa.Util
{
    /// <summary>
    /// Class that sets the render state.
    /// Although the render state can be declared and used separately, 
    /// it is basically designed to be used through inheritance of this class. 
    /// 
    /// レンダーステートを設定するクラスです。
    /// 単体で宣言しても使用できますが、基本的にはこのクラスを継承して
    /// 使用することを目的とします。
    /// </summary>
    public class PrimitiveRenderState
    {
        #region Helper Methods
        /// <summary>
        /// Sets the render state.
        /// 
        /// レンダーステートを設定します。
        /// </summary>
        public virtual void SetRenderState(GraphicsDevice graphics, SpriteBlendMode mode)
        {
            RenderState state = graphics.RenderState;
            if (mode == SpriteBlendMode.AlphaBlend)
            {
                // Enables AlphaBlend.
                // 
                // アルファブレンド有りに設定します。
                state.AlphaBlendEnable = true;
                state.AlphaBlendOperation = BlendFunction.Add;
                state.SourceBlend = Blend.SourceAlpha;
                state.DestinationBlend = Blend.InverseSourceAlpha;
                state.SeparateAlphaBlendEnabled = false;

                state.AlphaTestEnable = true;
                state.AlphaFunction = CompareFunction.Greater;
                state.ReferenceAlpha = 0;
            }
            else if (mode == SpriteBlendMode.Additive)
            {
                // Sets to Addition. 
                // 
                // 加算に設定します。
                state.AlphaBlendEnable = true;
                state.AlphaBlendOperation = BlendFunction.Add;
                state.SourceBlend = Blend.SourceAlpha;
                state.DestinationBlend = Blend.SourceAlpha | Blend.InverseSourceAlpha;
                state.SeparateAlphaBlendEnabled = false;

                state.AlphaTestEnable = true;
                state.AlphaFunction = CompareFunction.Greater;
                state.ReferenceAlpha = 0;
            }
            else if (mode == SpriteBlendMode.None)
            {
                // Disables AlphaBlend.
                // 
                // アルファブレンド無しに設定します。
                state.AlphaBlendEnable = false;
                state.AlphaTestEnable = false;
            }
        }
        #endregion
    }
}