#region File Description
//-----------------------------------------------------------------------------
// ScreenFactory.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using GameStateManagement;

namespace GameStateManagementSample
{
    /// <summary>
    /// Our game's implementation of IScreenFactory which can handle creating the screens
    /// when resuming from being tombstoned.
    /// </summary>
    public class ScreenFactory : IScreenFactory
    {
        public GameScreen CreateScreen(Type screenType)
        {
            // All of our screens have empty constructors so we can just use Activator
            return Activator.CreateInstance(screenType) as GameScreen;

            // If we had more complex screens that had constructors or needed properties set,
            // we could do that before handing the screen back to the ScreenManager. For example
            // you might have something like this:
            //
            // if (screenType == typeof(MySuperGameScreen))
            // {
            //     bool value = GetFirstParameter();
            //     float value2 = GetSecondParameter();
            //     MySuperGameScreen screen = new MySuperGameScreen(value, value2);
            //     return screen;
            // }
            //
            // This lets you still take advantage of constructor arguments yet participate in the
            // serialization process of the screen manager. Of course you need to save out those
            // values when deactivating and read them back, but that means either IsolatedStorage or
            // using the PhoneApplicationService.Current.State dictionary.
        }
    }
}
