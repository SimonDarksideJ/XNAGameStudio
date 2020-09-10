#Region "File Description"
'-----------------------------------------------------------------------------
' MainMenuScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text
Imports Blackjack.GameStateManagement


Friend Class MainMenuScreen
    Inherits MenuScreen
    Public Shared Theme As String = "Red"

#Region "Initializations"

    ''' <summary>
    ''' Initializes a new instance of the screen.
    ''' </summary>
    Public Sub New()
        MyBase.New("")
    End Sub
#End Region

    Public Overrides Sub LoadContent()
        ' Create our menu entries.
        Dim startGameMenuEntry As New MenuEntry("Play")
        Dim themeGameMenuEntry As New MenuEntry("Theme")
        Dim exitMenuEntry As New MenuEntry("Exit")

        ' Hook up menu event handlers.
        AddHandler startGameMenuEntry.Selected, AddressOf StartGameMenuEntrySelected
        AddHandler themeGameMenuEntry.Selected, AddressOf ThemeGameMenuEntrySelected
        AddHandler exitMenuEntry.Selected, AddressOf OnCancel

        ' Add entries to the menu.
        MenuEntries.Add(startGameMenuEntry)
        MenuEntries.Add(themeGameMenuEntry)
        MenuEntries.Add(exitMenuEntry)

        MyBase.LoadContent()
    End Sub

#Region "Update"
    ''' <summary>
    ''' Respond to "Play" Item Selection
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub StartGameMenuEntrySelected(ByVal sender As Object, ByVal e As EventArgs)
        For Each screen In ScreenManager.GetScreens
            screen.ExitScreen()
        Next screen

        ScreenManager.AddScreen(New GameplayScreen(Theme), Nothing)
    End Sub

    ''' <summary>
    ''' Respond to "Theme" Item Selection
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ThemeGameMenuEntrySelected(ByVal sender As Object, ByVal e As EventArgs)
        ScreenManager.AddScreen(New OptionsMenu, Nothing)
    End Sub

    ''' <summary>
    ''' Respond to "Exit" Item Selection
    ''' </summary>
    ''' <param name="playerIndex"></param>
    Protected Overrides Sub OnCancel(ByVal playerIndex As PlayerIndex)
        ScreenManager.Game.Exit()
    End Sub
#End Region
End Class
