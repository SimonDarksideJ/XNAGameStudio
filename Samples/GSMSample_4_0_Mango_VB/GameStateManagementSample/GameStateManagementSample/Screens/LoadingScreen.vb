#Region "File Description"
'-----------------------------------------------------------------------------
' LoadingScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports GameStateManagement
#End Region

''' <summary>
''' The loading screen coordinates transitions between the menu system and the
''' game itself. Normally one screen will transition off at the same time as
''' the next screen is transitioning on, but for larger transitions that can
''' take a longer time to load their data, we want the menu system to be entirely
''' gone before we start loading the game. This is done as follows:
''' 
''' - Tell all the existing screens to transition off.
''' - Activate a loading screen, which will transition on at the same time.
''' - The loading screen watches the state of the previous screens.
''' - When it sees they have finished transitioning off, it activates the real
'''   next screen, which may take a long time to load its data. The loading
'''   screen will be the only thing displayed while this load is taking place.
''' </summary>
Friend Class LoadingScreen
    Inherits GameScreen
#Region "Fields"

    Private loadingIsSlow As Boolean
    Private otherScreensAreGone As Boolean

    Private screensToLoad() As GameScreen

#End Region

#Region "Initialization"


    ''' <summary>
    ''' The constructor is private: loading screens should
    ''' be activated via the static Load method instead.
    ''' </summary>
    Private Sub New(ByVal screenManager As ScreenManager, ByVal loadingIsSlow As Boolean, ByVal screensToLoad() As GameScreen)
        Me.loadingIsSlow = loadingIsSlow
        Me.screensToLoad = screensToLoad

        TransitionOnTime = TimeSpan.FromSeconds(0.5)
    End Sub


    ''' <summary>
    ''' Activates the loading screen.
    ''' </summary>
    Public Shared Sub Load(ByVal screenManager As ScreenManager, ByVal loadingIsSlow As Boolean, ByVal controllingPlayer? As PlayerIndex, ByVal ParamArray screensToLoad() As GameScreen)
        ' Tell all the current screens to transition off.
        For Each screen As GameScreen In screenManager.GetScreens()
            screen.ExitScreen()
        Next screen

        ' Create and activate the loading screen.
        Dim loadingScreen As New LoadingScreen(screenManager, loadingIsSlow, screensToLoad)

        screenManager.AddScreen(loadingScreen, controllingPlayer)
    End Sub


#End Region

#Region "Update and Draw"


    ''' <summary>
    ''' Updates the loading screen.
    ''' </summary>
    Public Overrides Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
        MyBase.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen)

        ' If all the previous screens have finished transitioning
        ' off, it is time to actually perform the load.
        If otherScreensAreGone Then
            ScreenManager.RemoveScreen(Me)

            For Each screen As GameScreen In screensToLoad
                If screen IsNot Nothing Then
                    ScreenManager.AddScreen(screen, ControllingPlayer)
                End If
            Next screen

            ' Once the load has finished, we use ResetElapsedTime to tell
            ' the  game timing mechanism that we have just finished a very
            ' long frame, and that it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime()
        End If
    End Sub


    ''' <summary>
    ''' Draws the loading screen.
    ''' </summary>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        ' If we are the only active screen, that means all the previous screens
        ' must have finished transitioning off. We check for this in the Draw
        ' method, rather than in Update, because it isn't enough just for the
        ' screens to be gone: in order for the transition to look good we must
        ' have actually drawn a frame without them before we perform the load.
        If (ScreenState = ScreenState.Active) AndAlso (ScreenManager.GetScreens().Length = 1) Then
            otherScreensAreGone = True
        End If

        ' The gameplay screen takes a while to load, so we display a loading
        ' message while that is going on, but the menus load very quickly, and
        ' it would look silly if we flashed this up for just a fraction of a
        ' second while returning from the game to the menus. This parameter
        ' tells us how long the loading is going to take, so we know whether
        ' to bother drawing the message.
        If loadingIsSlow Then
            Dim spriteBatch As SpriteBatch = ScreenManager.SpriteBatch
            Dim font As SpriteFont = ScreenManager.Font

            Const message As String = "Loading..."

            ' Center the text in the viewport.
            Dim viewport As Viewport = ScreenManager.GraphicsDevice.Viewport
            Dim viewportSize As New Vector2(viewport.Width, viewport.Height)
            Dim textSize As Vector2 = font.MeasureString(message)
            Dim textPosition As Vector2 = (viewportSize - textSize) / 2

            Dim color As Color = color.White * TransitionAlpha

            ' Draw the text.
            spriteBatch.Begin()
            spriteBatch.DrawString(font, message, textPosition, color)
            spriteBatch.End()
        End If
    End Sub


#End Region
End Class