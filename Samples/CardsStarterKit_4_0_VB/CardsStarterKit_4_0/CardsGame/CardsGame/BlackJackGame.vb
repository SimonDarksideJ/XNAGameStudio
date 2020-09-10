#Region "File Description"
'-----------------------------------------------------------------------------
' PauseScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports Blackjack.GameStateManagement


''' <summary>
''' This is the main type for your game
''' </summary>
Public Class BlackJackGame
    Inherits Microsoft.Xna.Framework.Game

    Private WithEvents graphics As GraphicsDeviceManager
    Private WithEvents spriteBatch As SpriteBatch

    Private screenManager As ScreenManager

    Public Shared HeightScale As Single = 1.0F
    Public Shared WidthScale As Single = 1.0F

    Public Sub New()
        graphics = New GraphicsDeviceManager(Me)
        Content.RootDirectory = "Content"

        screenManager = New ScreenManager(Me)

        screenManager.AddScreen(New BackgroundScreen, Nothing)
        screenManager.AddScreen(New MainMenuScreen, Nothing)

        Components.Add(screenManager)

#If WINDOWS Then
			IsMouseVisible = True
#ElseIf WINDOWS_PHONE Then
        ' Frame rate is 30 fps by default for Windows Phone.
        TargetElapsedTime = TimeSpan.FromTicks(333333)
        graphics.IsFullScreen = True
#Else
			Components.Add(New GamerServicesComponent(Me))
#End If

        ' Initialize sound system
        AudioManager.Initialize(Me)
    End Sub

    ''' <summary>
    ''' Allows the game to perform any initialization it needs to before starting to run.
    ''' This is where it can query for any required services and load any non-graphic
    ''' related content.  Calling MyBase.Initialize will enumerate through any components
    ''' and initialize them as well.
    ''' </summary>
    Protected Overrides Sub Initialize()
        MyBase.Initialize()

#If XBOX Then
			graphics.PreferredBackBufferHeight = graphics.GraphicsDevice.DisplayMode.Height
			graphics.PreferredBackBufferWidth = graphics.GraphicsDevice.DisplayMode.Width
#ElseIf WINDOWS Then
			graphics.PreferredBackBufferHeight = 480
			graphics.PreferredBackBufferWidth = 800
#End If
        graphics.ApplyChanges()

        Dim bounds As Rectangle = graphics.GraphicsDevice.Viewport.TitleSafeArea
        HeightScale = bounds.Height / 480.0F
        WidthScale = bounds.Width / 800.0F
    End Sub

    ''' <summary>
    ''' LoadContent will be called once per game and is the place to load
    ''' all of your content.
    ''' </summary>
    Protected Overrides Sub LoadContent()
        AudioManager.LoadSounds()
        MyBase.LoadContent()
    End Sub
End Class
