#Region "File Description"
'-----------------------------------------------------------------------------
' GameplayScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Blackjack.GameStateManagement
Imports CardsFramework
Imports Blackjack
#End Region


Friend Class GameplayScreen
    Inherits GameScreen
#Region "Fields and Properties"
    Private _blackJackGame As BlackjackCardGame

    Private _inputHelper As InputHelper

    Private _theme As String
    Private _pauseEnabledComponents As New List(Of DrawableGameComponent)
    Private _pauseVisibleComponents As New List(Of DrawableGameComponent)
    Private _safeArea As Rectangle

    Private Shared playerCardOffset() As Vector2 = {New Vector2(100.0F * BlackJackGame.WidthScale, 190.0F * BlackJackGame.HeightScale), New Vector2(336.0F * BlackJackGame.WidthScale, 210.0F * BlackJackGame.HeightScale), New Vector2(570.0F * BlackJackGame.WidthScale, 190.0F * BlackJackGame.HeightScale)}
#End Region

#Region "Initiaizations"
    ''' <summary>
    ''' Initializes a new instance of the screen.
    ''' </summary>
    Public Sub New(ByVal theme As String)
        TransitionOnTime = TimeSpan.FromSeconds(0.0)
        TransitionOffTime = TimeSpan.FromSeconds(0.5)
#If WINDOWS_PHONE Then
        EnabledGestures = GestureType.Tap
#End If
        Me._theme = theme
    End Sub
#End Region

#Region "Loading"
    ''' <summary>
    ''' Load content and initializes the actual game.
    ''' </summary>
    Public Overrides Sub LoadContent()
        _safeArea = ScreenManager.SafeArea

        ' Initialize virtual cursor
        _inputHelper = New InputHelper(ScreenManager.Game)
        _inputHelper.DrawOrder = 1000
        ScreenManager.Game.Components.Add(_inputHelper)
        ' Ignore the curser when not run in Xbox
#If Not XBOX Then
        _inputHelper.Visible = False
        _inputHelper.Enabled = False
#End If

        _blackJackGame = New BlackjackCardGame(ScreenManager.GraphicsDevice.Viewport.Bounds, New Vector2(_safeArea.Left + _safeArea.Width \ 2 - 50, _safeArea.Top + 20), AddressOf GetPlayerCardPosition, ScreenManager, _theme)


        InitializeGame()

        MyBase.LoadContent()
    End Sub

    ''' <summary>
    ''' Unload content loaded by the screen.
    ''' </summary>
    Public Overrides Sub UnloadContent()
        ScreenManager.Game.Components.Remove(_inputHelper)

        MyBase.UnloadContent()
    End Sub
#End Region

#Region "Update and Render"
    ''' <summary>
    ''' Handle user input.
    ''' </summary>
    ''' <param name="input">User input information.</param>
    Public Overrides Sub HandleInput(ByVal input As InputState)
        If input.IsPauseGame(Nothing) Then
            PauseCurrentGame()
        End If

        MyBase.HandleInput(input)
    End Sub

    ''' <summary>
    ''' Perform the screen's update logic.
    ''' </summary>
    ''' <param name="gameTime">The time that has passed since the last call to 
    ''' this method.</param>
    ''' <param name="otherScreenHasFocus">Whether or not another screen has
    ''' the focus.</param>
    ''' <param name="coveredByOtherScreen">Whether or not another screen covers
    ''' this one.</param>
    Public Overrides Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
#If XBOX Then
			If Guide.IsVisible Then
				PauseCurrentGame()
			End If
#End If
        If _blackJackGame IsNot Nothing AndAlso (Not coveredByOtherScreen) Then
            _blackJackGame.Update(gameTime)
        End If

        MyBase.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen)
    End Sub

    ''' <summary>
    ''' Draw the screen
    ''' </summary>
    ''' <param name="gameTime"></param>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        MyBase.Draw(gameTime)

        If _blackJackGame IsNot Nothing Then
            _blackJackGame.Draw(gameTime)
        End If

    End Sub
#End Region

#Region "Private Methods"
    ''' <summary>
    ''' Initializes the game component.
    ''' </summary>
    Private Sub InitializeGame()
        _blackJackGame.Initialize()
        ' Add human player
        _blackJackGame.AddPlayer(New BlackjackPlayer("Abe", _blackJackGame))

        ' Add AI players
        Dim player As New BlackjackAIPlayer("Benny", _blackJackGame)
        _blackJackGame.AddPlayer(player)
        AddHandler player.Hit, AddressOf player_Hit
        AddHandler player.Stand, AddressOf player_Stand

        player = New BlackjackAIPlayer("Chuck", _blackJackGame)
        _blackJackGame.AddPlayer(player)
        AddHandler player.Hit, AddressOf player_Hit
        AddHandler player.Stand, AddressOf player_Stand

        ' Load UI assets
        Dim assets() As String = {"blackjack", "bust", "lose", "push", "win", "pass", "shuffle_" & _theme}

        For chipIndex = 0 To assets.Length - 1
            _blackJackGame.LoadUITexture("UI", assets(chipIndex))
        Next chipIndex

        _blackJackGame.StartRound()
    End Sub

    ''' <summary>
    ''' Gets the player hand positions according to the player index.
    ''' </summary>
    ''' <param name="player">The player's index.</param>
    ''' <returns>The position for the player's hand on the game table.</returns>
    Private Function GetPlayerCardPosition(ByVal player As Integer) As Vector2
        Select Case player
            Case 0, 1, 2
                Return New Vector2(ScreenManager.SafeArea.Left, ScreenManager.SafeArea.Top + 200 * (blackJackGame.HeightScale - 1)) + playerCardOffset(player)
            Case Else
                Throw New ArgumentException("Player index should be between 0 and 2", "player")
        End Select
    End Function

    ''' <summary>
    ''' Pause the game.
    ''' </summary>
    Private Sub PauseCurrentGame()
        ' Move to the pause screen
        ScreenManager.AddScreen(New BackgroundScreen, Nothing)
        ScreenManager.AddScreen(New PauseScreen, Nothing)

        ' Hide and disable all components which are related to the gameplay screen
        _pauseEnabledComponents.Clear()
        _pauseVisibleComponents.Clear()
        For Each component In ScreenManager.Game.Components
            If TypeOf component Is BetGameComponent OrElse TypeOf component Is AnimatedGameComponent OrElse TypeOf component Is GameTable OrElse TypeOf component Is InputHelper Then
                Dim pauseComponent As DrawableGameComponent = CType(component, DrawableGameComponent)
                If pauseComponent.Enabled Then
                    _pauseEnabledComponents.Add(pauseComponent)
                    pauseComponent.Enabled = False
                End If
                If pauseComponent.Visible Then
                    _pauseVisibleComponents.Add(pauseComponent)
                    pauseComponent.Visible = False
                End If
            End If
        Next component
    End Sub

    ''' <summary>
    ''' Returns from pause.
    ''' </summary>
    Public Sub ReturnFromPause()
        ' Reveal and enable all previously hidden components
        For Each component In _pauseEnabledComponents
            component.Enabled = True
        Next component
        For Each component In _pauseVisibleComponents
            component.Visible = True
        Next component
    End Sub
#End Region

#Region "Event Handler"
    ''' <summary>
    ''' Responds to the event sent when AI player's choose to "Stand".
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub player_Stand(ByVal sender As Object, ByVal e As EventArgs)
        _blackJackGame.Stand()
    End Sub

    ''' <summary>
    ''' Responds to the event sent when AI player's choose to "Split".
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub player_Split(ByVal sender As Object, ByVal e As EventArgs)
        _blackJackGame.Split()
    End Sub

    ''' <summary>
    ''' Responds to the event sent when AI player's choose to "Hit".
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub player_Hit(ByVal sender As Object, ByVal e As EventArgs)
        _blackJackGame.Hit()
    End Sub

    ''' <summary>
    ''' Responds to the event sent when AI player's choose to "Double".
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub player_Double(ByVal sender As Object, ByVal e As EventArgs)
        _blackJackGame.Double()
    End Sub
#End Region
End Class
