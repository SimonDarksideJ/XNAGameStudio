#region File Description
//-----------------------------------------------------------------------------
// GameSettings.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.IO.IsolatedStorage;
using System.Windows;

namespace PaddleBattle
{
    /// <summary>
    /// An object to hold all game settings, expressed as a DependencyObject so we can use
    /// data binding against the UI.
    /// </summary>
    public class GameSettings : DependencyObject
    {
        /// <summary>
        /// Gets or sets whether sounds should be played by the game.
        /// </summary>
        public bool PlaySounds
        {
            get { return (bool)GetValue(PlaySoundsProperty); }
            set { SetValue(PlaySoundsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlaySounds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaySoundsProperty =
            DependencyProperty.Register("PlaySounds", typeof(bool), typeof(GameSettings), new PropertyMetadata(true));

        /// <summary>
        /// Saves the settings to IsolatedStorage.
        /// </summary>
        public void Save()
        {
            // Populate the ApplicationSettings collection with our values
            IsolatedStorageSettings.ApplicationSettings["PlaySounds"] = PlaySounds;

            // Make sure to save the settings
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        /// <summary>
        /// Loads the settings from IsolatedStorage.
        /// </summary>
        public void Load()
        {
            // Restore settings from the ApplicationSettings collection
            if (IsolatedStorageSettings.ApplicationSettings.Contains("PlaySounds"))
            {
                PlaySounds = (bool)IsolatedStorageSettings.ApplicationSettings["PlaySounds"];
            }
        }
    }
}
