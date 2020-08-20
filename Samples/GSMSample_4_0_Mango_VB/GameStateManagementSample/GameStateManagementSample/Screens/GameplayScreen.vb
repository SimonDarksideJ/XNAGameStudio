#Region "File Description"
'-----------------------------------------------------------------------------
' GameplayScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports Microsoft.VisualBasic
Imports System.Threading
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input
Imports GameStateManagement
#End Region

''' <summary>
''' This screen implements the actual game logic. It is just a
''' placeholder to get the idea across: you'll probably want to
''' put some more interesting gameplay in here!
''' </summary>
Friend Class GameplayScreen
    Inherits GameScreen
#Region "Fields"

    Private content As ContentManager
    Private gameFont As SpriteFont

    Private playerPosition As New Vector2(100, 100)
    Private enemyPosition As New Vector2(100, 100)

    Private random As New Random()

    Private pauseAlpha As Single

    Private pauseAction As InputAction

#End Region

#Region "Initialization"


    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New()
        TransitionOnTime = TimeSpan.FromSeconds(1.5)
        TransitionOffTime = TimeSpan.FromSeconds(0.5)

        pauseAction = New InputAction(New Buttons() {Buttons.Start, Buttons.Back}, New Keys() {Keys.Escape}, True)
    End Sub


    ''' <summary>
    ''' Load graphics content for the game.
    ''' </summary>
    Public Overrides Sub Activate(ByVal instancePreserved As Boolean)
        If Not instancePreserved Then
            If content Is Nothing Then
                content = New ContentManager(ScreenManager.Game.Services, "Content")
            End If

            gameFont = content.Load(Of SpriteFont)("gamefont")

            ' A real game would probably have more content than this sample, so
            ' it would take longer to load. We simulate that by delaying for a
            ' while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000)

            ' once the load has finished, we use ResetElapsedTime to tell the game's
            ' timing mechanism that we have just finished a very long frame, and that
            ' it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime()
        End If

#If WINDOWS_PHONE Then
        If Microsoft.Phone.Shell.PhoneApplicationService.Current.State.ContainsKey("PlayerPosition") Then
            playerPosition = CType(Microsoft.Phone.Shell.PhoneApplicationService.Current.State("PlayerPosition"), Vector2)
            enemyPosition = CType(Microsoft.Phone.Shell.PhoneApplicationService.Current.State("EnemyPosition"), Vector2)
        End If
#End If
    End Sub


    Public Overrides Sub Deactivate()
#If WINDOWS_PHONE Then
        Microsoft.Phone.Shell.PhoneApplicationService.Current.State("PlayerPosition") = playerPosition
        Microsoft.Phone.Shell.PhoneApplicationService.Current.State("EnemyPosition") = enemyPosition
#End If

        MyBase.Deactivate()
    End Sub


    ''' <summary>
    ''' Unload graphics content used by the game.
    ''' </summary>
    Public Overrides Sub Unload()
        content.Unload()

#If WINDOWS_PHONE Then
        Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("PlayerPosition")
        Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("EnemyPosition")
#End If
    End Sub


#End Region

#Region "Update and Draw"


    ''' <summary>
    ''' Updates the state of the game. This method checks the GameScreen.IsActive
    ''' property, so the game will stop updating when the pause menu is active,
    ''' or if you tab away to a different application.
    ''' </summary>
    Public Overrides Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
        MyBase.Update(gameTime, otherScreenHasFocus, False)

        ' Gradually fade in or out depending on whether we are covered by the pause screen.
        If coveredByOtherScreen Then
            pauseAlpha = Math.Min(pauseAlpha + 1.0F / 32, 1)
        Else
            pauseAlpha = Math.Max(pauseAlpha - 1.0F / 32, 0)
        End If

        If IsActive Then
            ' Apply some random jitter to make the enemy move around.
            Const randomization As Single = 10

            enemyPosition.X += CSng(random.NextDouble() - 0.5) * randomization
            enemyPosition.Y += CSng(random.NextDouble() - 0.5) * randomization

            ' Apply a stabilizing force to stop the enemy moving off the screen.
            Dim targetPosition As New Vector2(ScreenManager.GraphicsDevice.Viewport.Width \ 2 - gameFont.MeasureString("Insert Gameplay Here").X / 2, 200)

            enemyPosition = Vector2.Lerp(enemyPosition, targetPosition, 0.05F)

            ' TODO: this game isn't very fun! You could probably improve
            ' it by inserting something more interesting in this space :-)
        End If
    End Sub


    ''' <summary>
    ''' Lets the game respond to player input. Unlike the Update method,
    ''' this will only be called when the gameplay screen is active.
    ''' </summary>
    Public Overrides Sub HandleInput(ByVal gameTime As GameTime, ByVal input As InputState)
        If input Is Nothing Then
            Throw New ArgumentNullException("input")
        End If

        ' Look up inputs for the active player profile.
        Dim playerIndex As Integer = CInt(ControllingPlayer.Value)

        Dim keyboardState As KeyboardState = input.CurrentKeyboardStates(playerIndex)
        Dim gamePadState As GamePadState = input.CurrentGamePadStates(playerIndex)

        ' The game pauses either if the user presses the pause button, or if
        ' they unplug the active gamepad. This requires us to keep track of
        ' whether a gamepad was ever plugged in, because we don't want to pause
        ' on PC if they are playing with a keyboard and have no gamepad at all!
        Dim gamePadDisconnected As Boolean = (Not gamePadState.IsConnected) AndAlso input.GamePadWasConnected(playerIndex)

        Dim player As PlayerIndex
        If pauseAction.Evaluate(input, ControllingPlayer, player) OrElse gamePadDisconnected Then
#If WINDOWS_PHONE Then
            ScreenManager.AddScreen(New PhonePauseScreen(), ControllingPlayer)
#Else
				ScreenManager.AddScreen(New PauseMenuScreen(), ControllingPlayer)
#End If
        Else
            ' Otherwise move the player position.
            Dim movement As Vector2 = Vector2.Zero

            If keyboardState.IsKeyDown(Keys.Left) Then
                movement.X -= 1
            End If

            If keyboardState.IsKeyDown(Keys.Right) Then
                movement.X += 1
            End If

            If keyboardState.IsKeyDown(Keys.Up) Then
                movement.Y -= 1
            End If

            If keyboardState.IsKeyDown(Keys.Down) Then
                movement.Y += 1
            End If

            Dim thumbstick As Vector2 = gamePadState.ThumbSticks.Left

            movement.X += thumbstick.X
            movement.Y -= thumbstick.Y

            If input.TouchState.Count > 0 Then
                Dim touchPosition As Vector2 = input.TouchState(0).Position
                Dim direction As Vector2 = touchPosition - playerPosition
                direction.Normalize()
                movement += direction
            End If

            If movement.Length() > 1 Then
                movement.Normalize()
            End If

            playerPosition += movement * 8.0F
        End If
    End Sub


    ''' <summary>
    ''' Draws the gameplay screen.
    ''' </summary>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        ' This game has a blue background. Why? Because!
        ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0)

        ' Our player and enemy are both actually just text strings.
        Dim spriteBatch As SpriteBatch = ScreenManager.SpriteBatch

        spriteBatch.Begin()

        spriteBatch.DrawString(gameFont, "// TODO", playerPosition, Color.Green)

        spriteBatch.DrawString(gameFont, "Insert Gameplay Here", enemyPosition, Color.DarkRed)

        spriteBatch.End()

        ' If the game is transitioning on or off, fade it out to black.
        If TransitionPosition > 0 OrElse pauseAlpha > 0 Then
            Dim alpha As Single = MathHelper.Lerp(1.0F - TransitionAlpha, 1.0F, pauseAlpha / 2)

            ScreenManager.FadeBackBufferToBlack(alpha)
        End If
    End Sub


#End Region
End Class