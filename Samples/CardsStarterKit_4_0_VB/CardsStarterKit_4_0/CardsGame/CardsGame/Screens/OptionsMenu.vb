#Region "File Description"
'-----------------------------------------------------------------------------
' OptionsMenu.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Blackjack.GameStateManagement
Imports CardsFramework
#End Region


Friend Class OptionsMenu
    Inherits MenuScreen
    Private themes As New Dictionary(Of String, Texture2D)
    Private card As AnimatedGameComponent
    Private background As Texture2D
    Private safeArea As Rectangle


#Region "Initializations"

    ''' <summary>
    ''' Initializes a new instance of the screen.
    ''' </summary>
    Public Sub New()
        MyBase.New("")

    End Sub
#End Region

    ''' <summary>
    ''' Loads content required by the screen, and initializes the displayed menu.
    ''' </summary>
    Public Overrides Sub LoadContent()
        safeArea = ScreenManager.SafeArea
        ' Create our menu entries.
        Dim themeGameMenuEntry As New MenuEntry("Deck")
        Dim returnMenuEntry As New MenuEntry("Return")

        ' Hook up menu event handlers.
        AddHandler themeGameMenuEntry.Selected, AddressOf ThemeGameMenuEntrySelected
        AddHandler returnMenuEntry.Selected, AddressOf OnCancel

        ' Add entries to the menu.
        MenuEntries.Add(themeGameMenuEntry)
        MenuEntries.Add(returnMenuEntry)

        themes.Add("Red", ScreenManager.Game.Content.Load(Of Texture2D)("Images\Cards\CardBack_Red"))
        themes.Add("Blue", ScreenManager.Game.Content.Load(Of Texture2D)("Images\Cards\CardBack_Blue"))
        background = ScreenManager.Game.Content.Load(Of Texture2D)("Images\UI\table")

        card = New AnimatedGameComponent(ScreenManager.Game, themes(MainMenuScreen.Theme)) With {.CurrentPosition = New Vector2(safeArea.Center.X, safeArea.Center.Y - 50)}

        ScreenManager.Game.Components.Add(card)

        MyBase.LoadContent()
    End Sub

#Region "Update and Render"
    ''' <summary>
    ''' Respond to "Theme" Item Selection
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ThemeGameMenuEntrySelected(ByVal sender As Object, ByVal e As EventArgs)
        If MainMenuScreen.Theme = "Red" Then
            MainMenuScreen.Theme = "Blue"
        Else
            MainMenuScreen.Theme = "Red"
        End If
        card.CurrentFrame = themes(MainMenuScreen.Theme)
    End Sub

    ''' <summary>
    ''' Respond to "Return" Item Selection
    ''' </summary>
    ''' <param name="playerIndex"></param>
    Protected Overrides Sub OnCancel(ByVal playerIndex As PlayerIndex)
        ScreenManager.Game.Components.Remove(card)
        ExitScreen()
    End Sub

    ''' <summary>
    ''' Draws the menu.
    ''' </summary>
    ''' <param name="gameTime"></param>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        ScreenManager.SpriteBatch.Begin()

        ' Draw the card back
        ScreenManager.SpriteBatch.Draw(background, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White * TransitionAlpha)

        ScreenManager.SpriteBatch.End()
        MyBase.Draw(gameTime)
    End Sub
#End Region
End Class
