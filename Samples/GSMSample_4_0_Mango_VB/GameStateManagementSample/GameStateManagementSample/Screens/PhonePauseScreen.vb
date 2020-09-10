#Region "File Description"
'-----------------------------------------------------------------------------
' PhonePauseScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

''' <summary>
''' A basic pause screen for Windows Phone
''' </summary>
Friend Class PhonePauseScreen
    Inherits PhoneMenuScreen
    Public Sub New()
        MyBase.New("Paused")
        ' Create the "Resume" and "Exit" buttons for the screen

        Dim resumeButton As New Button("Resume")
        AddHandler resumeButton.Tapped, AddressOf resumeButton_Tapped
        MenuButtons.Add(resumeButton)

        Dim exitButton As New Button("Exit")
        AddHandler exitButton.Tapped, AddressOf exitButton_Tapped
        MenuButtons.Add(exitButton)
    End Sub

    ''' <summary>
    ''' The "Resume" button handler just calls the OnCancel method so that 
    ''' pressing the "Resume" button is the same as pressing the hardware back button.
    ''' </summary>
    Private Sub resumeButton_Tapped(ByVal sender As Object, ByVal e As EventArgs)
        OnCancel()
    End Sub

    ''' <summary>
    ''' The "Exit" button handler uses the LoadingScreen to take the user out to the main menu.
    ''' </summary>
    Private Sub exitButton_Tapped(ByVal sender As Object, ByVal e As EventArgs)
        LoadingScreen.Load(ScreenManager, False, Nothing, New BackgroundScreen(), New PhoneMainMenuScreen())
    End Sub

    Protected Overrides Sub OnCancel()
        ExitScreen()
        MyBase.OnCancel()
    End Sub
End Class