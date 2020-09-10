#Region "File Description"
'-----------------------------------------------------------------------------
' MainMenuScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports Microsoft.Xna.Framework
#End Region

''' <summary>
''' The main menu screen is the first thing displayed when the game starts up.
''' </summary>
Friend Class MainMenuScreen
    Inherits MenuScreen
#Region "Initialization"


    ''' <summary>
    ''' Constructor fills in the menu contents.
    ''' </summary>
    Public Sub New()
        MyBase.New("Main Menu")
        ' Create our menu entries.
        Dim playGameMenuEntry As New MenuEntry("Play Game")
        Dim optionsMenuEntry As New MenuEntry("Options")
        Dim exitMenuEntry As New MenuEntry("Exit")

        ' Hook up menu event handlers.
        AddHandler playGameMenuEntry.Selected, AddressOf PlayGameMenuEntrySelected
        AddHandler optionsMenuEntry.Selected, AddressOf OptionsMenuEntrySelected
        AddHandler exitMenuEntry.Selected, AddressOf OnCancel

        ' Add entries to the menu.
        MenuEntries.Add(playGameMenuEntry)
        MenuEntries.Add(optionsMenuEntry)
        MenuEntries.Add(exitMenuEntry)
    End Sub


#End Region

#Region "Handle Input"


    ''' <summary>
    ''' Event handler for when the Play Game menu entry is selected.
    ''' </summary>
    Private Sub PlayGameMenuEntrySelected(ByVal sender As Object, ByVal e As PlayerIndexEventArgs)
        LoadingScreen.Load(ScreenManager, True, e.PlayerIndex, New GameplayScreen())
    End Sub


    ''' <summary>
    ''' Event handler for when the Options menu entry is selected.
    ''' </summary>
    Private Sub OptionsMenuEntrySelected(ByVal sender As Object, ByVal e As PlayerIndexEventArgs)
        ScreenManager.AddScreen(New OptionsMenuScreen(), e.PlayerIndex)
    End Sub


    ''' <summary>
    ''' When the user cancels the main menu, ask if they want to exit the sample.
    ''' </summary>
    Protected Overrides Sub OnCancel(ByVal playerIndex As PlayerIndex)
        Const message As String = "Are you sure you want to exit this sample?"

        Dim confirmExitMessageBox As New MessageBoxScreen(message)

        AddHandler confirmExitMessageBox.Accepted, AddressOf ConfirmExitMessageBoxAccepted

        ScreenManager.AddScreen(confirmExitMessageBox, playerIndex)
    End Sub


    ''' <summary>
    ''' Event handler for when the user selects ok on the "are you sure
    ''' you want to exit" message box.
    ''' </summary>
    Private Sub ConfirmExitMessageBoxAccepted(ByVal sender As Object, ByVal e As PlayerIndexEventArgs)
        ScreenManager.Game.Exit()
    End Sub


#End Region
End Class