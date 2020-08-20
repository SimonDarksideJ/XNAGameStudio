#Region "File Description"
'-----------------------------------------------------------------------------
' BackgroundScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics
Imports GameStateManagement
#End Region
''' <summary>
''' The background screen sits behind all the other menu screens.
''' It draws a background image that remains fixed in place regardless
''' of whatever transitions the screens on top of it may be doing.
''' </summary>
Friend Class BackgroundScreen
    Inherits GameScreen
#Region "Fields"

    Private content As ContentManager
    Private backgroundTexture As Texture2D

#End Region

#Region "Initialization"


    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New()
        TransitionOnTime = TimeSpan.FromSeconds(0.5)
        TransitionOffTime = TimeSpan.FromSeconds(0.5)
    End Sub


    ''' <summary>
    ''' Loads graphics content for this screen. The background texture is quite
    ''' big, so we use our own local ContentManager to load it. This allows us
    ''' to unload before going from the menus into the game itself, wheras if we
    ''' used the shared ContentManager provided by the Game class, the content
    ''' would remain loaded forever.
    ''' </summary>
    Public Overrides Sub Activate(ByVal instancePreserved As Boolean)
        If Not instancePreserved Then
            If content Is Nothing Then
                content = New ContentManager(ScreenManager.Game.Services, "Content")
            End If

            backgroundTexture = content.Load(Of Texture2D)("background")
        End If
    End Sub


    ''' <summary>
    ''' Unloads graphics content for this screen.
    ''' </summary>
    Public Overrides Sub Unload()
        content.Unload()
    End Sub


#End Region

#Region "Update and Draw"


    ''' <summary>
    ''' Updates the background screen. Unlike most screens, this should not
    ''' transition off even if it has been covered by another screen: it is
    ''' supposed to be covered, after all! This overload forces the
    ''' coveredByOtherScreen parameter to false in order to stop the base
    ''' Update method wanting to transition off.
    ''' </summary>
    Public Overrides Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
        MyBase.Update(gameTime, otherScreenHasFocus, False)
    End Sub


    ''' <summary>
    ''' Draws the background screen.
    ''' </summary>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        Dim spriteBatch As SpriteBatch = ScreenManager.SpriteBatch
        Dim viewport As Viewport = ScreenManager.GraphicsDevice.Viewport
        Dim fullscreen As New Rectangle(0, 0, viewport.Width, viewport.Height)

        spriteBatch.Begin()

        spriteBatch.Draw(backgroundTexture, fullscreen, New Color(TransitionAlpha, TransitionAlpha, TransitionAlpha))

        spriteBatch.End()
    End Sub


#End Region
End Class