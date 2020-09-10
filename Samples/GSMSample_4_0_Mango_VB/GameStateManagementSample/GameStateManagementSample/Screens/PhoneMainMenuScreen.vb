#Region "File Description"
'-----------------------------------------------------------------------------
' PhoneMainMenuScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region
Imports GameStateManagement
Imports Microsoft.Xna.Framework
Friend Class PhoneMainMenuScreen
    Inherits PhoneMenuScreen
    Public Sub New()
        MyBase.New("Main Menu")
        ' Create a button to start the game
        Dim playButton As New Button("Play")
        AddHandler playButton.Tapped, AddressOf playButton_Tapped
        MenuButtons.Add(playButton)

        ' Create two buttons to toggle sound effects and music. This sample just shows one way
        ' of making and using these buttons; it doesn't actually have sound effects or music
        Dim sfxButton As New BooleanButton("Sound Effects", True)
        AddHandler sfxButton.Tapped, AddressOf sfxButton_Tapped
        MenuButtons.Add(sfxButton)

        Dim musicButton As New BooleanButton("Music", True)
        AddHandler musicButton.Tapped, AddressOf musicButton_Tapped
        MenuButtons.Add(musicButton)
    End Sub

    Private Sub playButton_Tapped(ByVal sender As Object, ByVal e As EventArgs)
        ' When the "Play" button is tapped, we load the GameplayScreen
        LoadingScreen.Load(ScreenManager, True, PlayerIndex.One, New GameplayScreen())
    End Sub

    Private Sub sfxButton_Tapped(ByVal sender As Object, ByVal e As EventArgs)
        Dim button As BooleanButton = TryCast(sender, BooleanButton)

        ' In a real game, you'd want to store away the value of 
        ' the button to turn off sounds here. :)
    End Sub

    Private Sub musicButton_Tapped(ByVal sender As Object, ByVal e As EventArgs)
        Dim button As BooleanButton = TryCast(sender, BooleanButton)

        ' In a real game, you'd want to store away the value of 
        ' the button to turn off music here. :)
    End Sub

    Protected Overrides Sub OnCancel()
        ScreenManager.Game.Exit()
        MyBase.OnCancel()
    End Sub
End Class