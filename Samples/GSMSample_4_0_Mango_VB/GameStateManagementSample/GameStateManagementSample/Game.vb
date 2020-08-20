#Region "File Description"
'-----------------------------------------------------------------------------
' Game.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region
Imports GameStateManagement
Imports Microsoft.Xna.Framework

''' <summary>
''' Sample showing how to manage different game states, with transitions
''' between menu screens, a loading screen, the game itself, and a pause
''' menu. This main game class is extremely simple: all the interesting
''' stuff happens in the ScreenManager component.
''' </summary>
Public Class GameStateManagementGame
    Inherits Microsoft.Xna.Framework.Game
    Private graphics As GraphicsDeviceManager
    Private screenManager As ScreenManager
    Private screenFactory As ScreenFactory

    ''' <summary>
    ''' The main game constructor.
    ''' </summary>
    Public Sub New()
        Content.RootDirectory = "Content"

        graphics = New GraphicsDeviceManager(Me)
        TargetElapsedTime = TimeSpan.FromTicks(333333)

#If WINDOWS_PHONE Then
        graphics.IsFullScreen = True

        ' Choose whether you want a landscape or portait game by using one of the two helper functions.
        InitializeLandscapeGraphics()
        ' InitializePortraitGraphics();
#End If

        ' Create the screen factory and add it to the Services
        screenFactory = New ScreenFactory()
        Services.AddService(GetType(IScreenFactory), screenFactory)

        ' Create the screen manager component.
        screenManager = New ScreenManager(Me)
        Components.Add(screenManager)

#If WINDOWS_PHONE Then
        ' Hook events on the PhoneApplicationService so we're notified of the application's life cycle
        AddHandler Microsoft.Phone.Shell.PhoneApplicationService.Current.Launching, AddressOf GameLaunching
        AddHandler Microsoft.Phone.Shell.PhoneApplicationService.Current.Activated, AddressOf GameActivated
        AddHandler Microsoft.Phone.Shell.PhoneApplicationService.Current.Deactivated, AddressOf GameDeactivated
#Else
			' On Windows and Xbox we just add the initial screens
			AddInitialScreens()
#End If
    End Sub

    Private Sub AddInitialScreens()
        ' Activate the first screens.
        screenManager.AddScreen(New BackgroundScreen(), Nothing)

        ' We have different menus for Windows Phone to take advantage of the touch interface
#If WINDOWS_PHONE Then
        screenManager.AddScreen(New PhoneMainMenuScreen(), Nothing)
#Else
			screenManager.AddScreen(New MainMenuScreen(), Nothing)
#End If
    End Sub

    ''' <summary>
    ''' This is called when the game should draw itself.
    ''' </summary>
    Protected Overrides Sub Draw(ByVal gameTime As GameTime)
        graphics.GraphicsDevice.Clear(Color.Black)

        ' The real drawing happens inside the screen manager component.
        MyBase.Draw(gameTime)
    End Sub

#If WINDOWS_PHONE Then
    ''' <summary>
    ''' Helper method to the initialize the game to be a portrait game.
    ''' </summary>
    Private Sub InitializePortraitGraphics()
        graphics.PreferredBackBufferWidth = 480
        graphics.PreferredBackBufferHeight = 800
    End Sub

    ''' <summary>
    ''' Helper method to initialize the game to be a landscape game.
    ''' </summary>
    Private Sub InitializeLandscapeGraphics()
        graphics.PreferredBackBufferWidth = 800
        graphics.PreferredBackBufferHeight = 480
    End Sub

    Private Sub GameLaunching(ByVal sender As Object, ByVal e As Microsoft.Phone.Shell.LaunchingEventArgs)
        AddInitialScreens()
    End Sub

    Private Sub GameActivated(ByVal sender As Object, ByVal e As Microsoft.Phone.Shell.ActivatedEventArgs)
        ' Try to deserialize the screen manager
        If Not screenManager.Activate(e.IsApplicationInstancePreserved) Then
            ' If the screen manager fails to deserialize, add the initial screens
            AddInitialScreens()
        End If
    End Sub

    Private Sub GameDeactivated(ByVal sender As Object, ByVal e As Microsoft.Phone.Shell.DeactivatedEventArgs)
        ' Serialize the screen manager when the game deactivated
        screenManager.Deactivate()
    End Sub
#End If
End Class