#Region "File Description"
'-----------------------------------------------------------------------------
' GameSettings.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.IO.IsolatedStorage
Imports System.Windows

''' <summary>
''' An object to hold all game settings, expressed as a DependencyObject so we can use
''' data binding against the UI.
''' </summary>
Public Class GameSettings
    Inherits DependencyObject
    ''' <summary>
    ''' Gets or sets whether sounds should be played by the game.
    ''' </summary>
    Public Property PlaySounds() As Boolean
        Get
            Return CBool(GetValue(PlaySoundsProperty))
        End Get
        Set(ByVal value As Boolean)
            SetValue(PlaySoundsProperty, value)
        End Set
    End Property

    ' Using a DependencyProperty as the backing store for PlaySounds.  This enables animation, styling, binding, etc...
    Public Shared ReadOnly PlaySoundsProperty As DependencyProperty = DependencyProperty.Register("PlaySounds", GetType(Boolean), GetType(GameSettings), New PropertyMetadata(True))

    ''' <summary>
    ''' Saves the settings to IsolatedStorage.
    ''' </summary>
    Public Sub Save()
        ' Populate the ApplicationSettings collection with our values
        IsolatedStorageSettings.ApplicationSettings("PlaySounds") = PlaySounds

        ' Make sure to save the settings
        IsolatedStorageSettings.ApplicationSettings.Save()
    End Sub

    ''' <summary>
    ''' Loads the settings from IsolatedStorage.
    ''' </summary>
    Public Sub Load()
        ' Restore settings from the ApplicationSettings collection
        If IsolatedStorageSettings.ApplicationSettings.Contains("PlaySounds") Then
            PlaySounds = CBool(IsolatedStorageSettings.ApplicationSettings("PlaySounds"))
        End If
    End Sub
End Class