#Region "File Description"
'-----------------------------------------------------------------------------
' BackgroundScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text
Imports Blackjack.GameStateManagement


Friend Class BackgroundScreen
    Inherits GameScreen
    Private background As Texture2D
    Private safeArea As Rectangle

    ''' <summary>
    ''' Initializes a new instance of the screen.
    ''' </summary>
    Public Sub New()
        TransitionOnTime = TimeSpan.FromSeconds(0.0)
        TransitionOffTime = TimeSpan.FromSeconds(0.5)
    End Sub

#Region "Loading"
    ''' <summary>
    ''' Load graphics content for the screen.
    ''' </summary>
    Public Overrides Sub LoadContent()
        background = ScreenManager.Game.Content.Load(Of Texture2D)("Images\titlescreen")
        safeArea = ScreenManager.Game.GraphicsDevice.Viewport.TitleSafeArea
        MyBase.LoadContent()
    End Sub
#End Region

#Region "Update and Render"
    ''' <summary>
    ''' Allows the screen to run logic, such as updating the transition position.
    ''' Unlike HandleInput, this method is called regardless of whether the screen
    ''' is active, hidden, or in the middle of a transition.
    ''' </summary>
    ''' <param name="gameTime"></param>
    ''' <param name="otherScreenHasFocus"></param>
    ''' <param name="coveredByOtherScreen"></param>
    Public Overrides Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
        MyBase.Update(gameTime, otherScreenHasFocus, False)
    End Sub

    ''' <summary>
    ''' This is called when the screen should draw itself.
    ''' </summary>
    ''' <param name="gameTime"></param>
    Public Overrides Sub Draw(ByVal gameTime As Microsoft.Xna.Framework.GameTime)
        ScreenManager.SpriteBatch.Begin()

        ScreenManager.SpriteBatch.Draw(background, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White * TransitionAlpha)

        ScreenManager.SpriteBatch.End()

        MyBase.Draw(gameTime)
    End Sub
#End Region
End Class
