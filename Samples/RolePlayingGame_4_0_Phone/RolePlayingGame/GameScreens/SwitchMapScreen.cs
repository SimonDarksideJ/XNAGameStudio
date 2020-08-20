#region File Description
//-----------------------------------------------------------------------------
// SwitchMapScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RolePlayingGameData;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RolePlaying
{
    class SwitchMapScreen : GameScreen
    {
        #region Fields
        bool isDoneLoading;
        bool isDrawnFirst;

        string mapToLoad;
        string portalToLoad;

        Texture2D loadingTexture;

        public event EventHandler DoneLoading;

        #endregion

        #region Initialization

        public SwitchMapScreen(string mapToLoad,string portalToLoad)
        {
            Hud.IsActive = false;
            IsPopup = true;
            this.mapToLoad = mapToLoad;
            this.portalToLoad = portalToLoad;
        }

        public override void LoadContent()
        {

            loadingTexture = ScreenManager.Game.Content.Load<Texture2D>(@"Textures\MainMenu\LoadingPause");
            base.LoadContent();
        }
        #endregion

        #region Update
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!isDoneLoading && isDrawnFirst)
            {
                Session.SaveMapState();
                ScreenManager.Game.Content.Unload();
                GC.Collect();

                if (!Session.LoadMapState(mapToLoad))
                {
                    Map map = ScreenManager.Game.Content.Load<Map>(mapToLoad);
                    AudioManager.PlayMusic(map.MusicCueName,true);
                    TileEngine.SetMap(map, map.FindPortal(portalToLoad));
                    Session.singleton.ModifyMap(TileEngine.Map);
                }


                
                isDoneLoading = true;
                if (DoneLoading != null)
                {
                    DoneLoading(this, EventArgs.Empty);
                }
                Hud.IsActive = true;
                ScreenManager.RemoveScreen(this);
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(loadingTexture, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White);
            ScreenManager.SpriteBatch.End();

            isDrawnFirst = true;
            base.Draw(gameTime);
        }
        #endregion
    }
}
