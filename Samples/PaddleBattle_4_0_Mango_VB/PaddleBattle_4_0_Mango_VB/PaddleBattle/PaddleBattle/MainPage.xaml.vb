#Region "File Description"
'-----------------------------------------------------------------------------
' MainPage.xaml.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Windows
Imports Microsoft.Phone.Controls

Partial Public Class MainPage
    Inherits PhoneApplicationPage
    Public Sub New()
        InitializeComponent()

        ' Set the game's settings as the DataContext for our page, to hook up the databinding
        ' for the sounds checkbox
        DataContext = (TryCast(Application.Current, App)).Settings
    End Sub

    ' Respond to pressing the "Play" button by navigating to the game
    Private Sub PlayButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        NavigationService.Navigate(New Uri("/GamePage.xaml", UriKind.Relative))
    End Sub

End Class
