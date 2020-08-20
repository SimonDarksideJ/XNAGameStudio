#region File Description
//-----------------------------------------------------------------------------
// GameMenuScreen.cs
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
using RobotGameData.GameObject;
#endregion

namespace RobotGameData.Screen
{
    /// <summary>
    /// Base class for screens that contatain a menu. 
    /// The user can move up and down to select an entry, 
    /// or cancel to back out of the screen.
    /// </summary>
    public class GameMenuScreen : GameScreen
    {
        #region Fields

        List<Sprite2DObject> menuEntries = new List<Sprite2DObject>();
        int[] selectedVerticalEntryIndex = new int[4];
        int[] selectedHorizontalEntryIndex = new int[4];

        #endregion

        #region Properties

        /// <summary>
        /// Gets the list of menu entry strings, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        public IList<Sprite2DObject> MenuEntries
        {
            get { return menuEntries; }
        }

        public static int InputCount
        {
            get { return FrameworkCore.ScreenManager.ScreenInput.Length; }
        }

       
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameMenuScreen()
        {
            for( int i=0; i<4; i++)
            {
                selectedVerticalEntryIndex[i] = 0;
                selectedHorizontalEntryIndex[i] = 0;
            }

            menuEntries.Clear();
        }

        #region Handle Input

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(GameTime gameTime)
        {
            for (int i = 0; i < FrameworkCore.ScreenManager.InputCount; i++)
            {
                GameScreenInput input = FrameworkCore.ScreenManager.ScreenInput[i];

                // Move to the previous menu entry?
                if (input.MenuUp)
                {
                    selectedVerticalEntryIndex[i]--;

                    OnFocusEntry(i, selectedVerticalEntryIndex[i], 
                                selectedHorizontalEntryIndex[i]);
                }

                // Move to the next menu entry?
                if (input.MenuDown)
                {
                    selectedVerticalEntryIndex[i]++;

                    OnFocusEntry(i, selectedVerticalEntryIndex[i], 
                                selectedHorizontalEntryIndex[i]);
                }

                // Move to the previous menu entry?
                if (input.MenuLeft)
                {
                    selectedHorizontalEntryIndex[i]--;

                    OnFocusEntry(i, selectedVerticalEntryIndex[i], 
                                selectedHorizontalEntryIndex[i]);
                }

                // Move to the next menu entry?
                if (input.MenuRight)
                {
                    selectedHorizontalEntryIndex[i]++;

                    OnFocusEntry(i, selectedVerticalEntryIndex[i], 
                                selectedHorizontalEntryIndex[i]);
                }

                // Accept or cancel the menu?
                if (input.MenuSelect)
                {
                    OnSelectedEntry(i, selectedVerticalEntryIndex[i], 
                                selectedHorizontalEntryIndex[i]);
                }
                else
                {
                    if (input.MenuCancel)
                    {
                        OnCancel(i);
                    }

                    if (input.MenuExit)
                    {
                        OnExit(i);
                    }
                }
            }            
        }

        /// <summary>
        /// Notifies derived classes that a menu entry has been chosen.
        /// </summary>
        public virtual void OnSelectedEntry(int inputIndex,
                                            int verticalEntryIndex,
                                            int horizontalEntryIndex) { }

        /// <summary>
        /// Notifies derived classes that a menu entry has been focused.
        /// </summary>
        public virtual void OnFocusEntry( int inputIndex,
                                          int verticalEntryIndex,
                                          int horizontalEntryIndex) { }


        /// <summary>
        /// Notifies derived classes that the menu has been cancelled.
        /// </summary>
        public virtual void OnCancel(int inputIndex) { }

        /// <summary>
        /// Notifies derived classes that a menu entry has been exited.
        /// </summary>
        public virtual void OnExit(int inputIndex) { }

        /// <summary>
        /// Notifies derived classes that a menu entry has been updated.
        /// </summary>
        public virtual void OnUpdateEntry(GameTime gameTime, 
                                          int[] verticalEntryIndex,
                                          int[] horizontalEntryIndex) { }

        #endregion

        #region Update & Draw

        /// <summary>
        /// always calls the update function for the menu entry.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                      bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (MenuEntries.Count > 0)
                OnUpdateEntry(gameTime, selectedVerticalEntryIndex, 
                                        selectedHorizontalEntryIndex);
        }

        public override void Draw(GameTime gameTime)
        {
            throw new NotImplementedException(
                "The method or operation is not implemented.");
        }

        #endregion

        public void SetVerticalEntryIndex(int index, int value)
        {
            selectedVerticalEntryIndex[index] = value;
        }

        public void SetHorizontalEntryIndex(int index, int value)
        {
            selectedHorizontalEntryIndex[index] = value;
        }

        public void AddMenuEntry(Sprite2DObject item)
        {
            MenuEntries.Add(item);
        }

        public void RemoveMenuEntry(int index)
        {
            MenuEntries.RemoveAt(index);
        }

        public void RemoveMenuEntry(Sprite2DObject item)
        {
            MenuEntries.Remove(item);
        }

        public void RemoveAllMenuEntry()
        {
            MenuEntries.Clear();
        }
    }
}
