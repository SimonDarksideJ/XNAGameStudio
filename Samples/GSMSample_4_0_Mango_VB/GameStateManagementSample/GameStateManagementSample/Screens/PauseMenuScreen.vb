#Region "File Description"
'-----------------------------------------------------------------------------
' PauseMenuScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports Microsoft.Xna.Framework
#End Region

''' <summary>
''' The pause menu comes up over the top of the game,
''' giving the player options to resume or quit.
''' </summary>
Friend Class PauseMenuScreen
    Inherits MenuScreen
#Region "Initialization"


    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New()
        MyBase.New("Paused")
        ' Create our menu entries.
        Dim resumeGameMenuEntry As New MenuEntry("Resume Game")
        Dim quitGameMenuEntry As New MenuEntry("Quit Game")

        ' Hook up menu event handlers.
        AddHandler resumeGameMenuEntry.Selected, AddressOf OnCancel
        AddHandler quitGameMenuEntry.Selected, AddressOf QuitGameMenuEntrySelected

        ' Add entries to the menu.
        MenuEntries.Add(resumeGameMenuEntry)
        MenuEntries.Add(quitGameMenuEntry)
    End Sub


#End Region

#Region "Handle Input"


    ''' <summary>
    ''' Event handler for when the Quit Game menu entry is selected.
    ''' </summary>
    Private Sub QuitGameMenuEntrySelected(ByVal sender As Object, ByVal e As PlayerIndexEventArgs)
        Const message As String = "Are you sure you want to quit this game?"

        Dim confirmQuitMessageBox As New MessageBoxScreen(message)

        AddHandler confirmQuitMessageBox.Accepted, AddressOf ConfirmQuitMessageBoxAccepted

        ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer)
    End Sub


    ''' <summary>
    ''' Event handler for when the user selects ok on the "are you sure
    ''' you want to quit" message box. This uses the loading screen to
    ''' transition from the game back to the main menu screen.
    ''' </summary>
    Private Sub ConfirmQuitMessageBoxAccepted(ByVal sender As Object, ByVal e As PlayerIndexEventArgs)
        LoadingScreen.Load(ScreenManager, False, Nothing, New BackgroundScreen(), New MainMenuScreen())
    End Sub


#End Region
End Class