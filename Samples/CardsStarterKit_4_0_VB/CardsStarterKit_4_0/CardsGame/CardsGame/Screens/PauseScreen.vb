#Region "File Description"
'-----------------------------------------------------------------------------
' PauseScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text
Imports Blackjack.GameStateManagement


Friend Class PauseScreen
    Inherits MenuScreen
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
        Dim returnGameMenuEntry As New MenuEntry("Back")
        Dim exitMenuEntry As New MenuEntry("Quit")

        ' Hook up menu event handlers.
        AddHandler returnGameMenuEntry.Selected, AddressOf ReturnGameMenuEntrySelected
        AddHandler exitMenuEntry.Selected, AddressOf OnCancel

        ' Add entries to the menu.
        MenuEntries.Add(returnGameMenuEntry)
        MenuEntries.Add(exitMenuEntry)

        MyBase.LoadContent()
    End Sub

#Region "Update"
    ''' <summary>
    ''' Respond to "Return" Item Selection
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ReturnGameMenuEntrySelected(ByVal sender As Object, ByVal e As EventArgs)
        Dim screens() As GameScreen = ScreenManager.GetScreens
        Dim gameplayScreen As GameplayScreen = Nothing
        Dim res As New List(Of GameScreen)

        For screenIndex = 0 To screens.Length - 1
            If TypeOf screens(screenIndex) Is GameplayScreen Then
                gameplayScreen = CType(screens(screenIndex), GameplayScreen)
            Else
                res.Add(screens(screenIndex))
            End If
        Next screenIndex

        For Each screen In res
            screen.ExitScreen()
        Next screen

        gameplayScreen.ReturnFromPause()
    End Sub

    ''' <summary>
    ''' Respond to "Quit Game" Item Selection
    ''' </summary>
    ''' <param name="playerIndex"></param>
    Protected Overrides Sub OnCancel(ByVal playerIndex As PlayerIndex)
        Dim componentIndex As Integer = 0
        Do While componentIndex < ScreenManager.Game.Components.Count
            If Not (TypeOf ScreenManager.Game.Components(componentIndex) Is ScreenManager) Then
                If TypeOf ScreenManager.Game.Components(componentIndex) Is DrawableGameComponent Then
                    TryCast(ScreenManager.Game.Components(componentIndex), IDisposable).Dispose()
                    componentIndex -= 1
                Else
                    ScreenManager.Game.Components.RemoveAt(componentIndex)
                    componentIndex -= 1
                End If
            End If
            componentIndex += 1
        Loop

        For Each screen In ScreenManager.GetScreens
            screen.ExitScreen()
        Next screen

        ScreenManager.AddScreen(New BackgroundScreen, Nothing)
        ScreenManager.AddScreen(New MainMenuScreen, Nothing)
    End Sub
#End Region
End Class
