#Region "File Description"
'-----------------------------------------------------------------------------
' ScreenFactory.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region
Imports GameStateManagement
''' <summary>
''' Our game's implementation of IScreenFactory which can handle creating the screens
''' when resuming from being tombstoned.
''' </summary>
Public Class ScreenFactory
    Implements IScreenFactory

    Public Function CreateScreen(ByVal screenType As Type) As GameScreen Implements GameStateManagement.IScreenFactory.CreateScreen
        ' All of our screens have empty constructors so we can just use Activator
        Return TryCast(Activator.CreateInstance(screenType), GameScreen)

        ' If we had more complex screens that had constructors or needed properties set,
        ' we could do that before handing the screen back to the ScreenManager. For example
        ' you might have something like this:
        '
        'If screenType Is GetType(MySuperGameScreen) Then
        'Dim value As Boolean = GetFirstParameter()
        'Dim value2 As Single = GetSecondParameter()
        'Dim screen As New MySuperGameScreen(value, value2)
        'Return screen
        'End If
        '
        ' This lets you still take advantage of constructor arguments yet participate in the
        ' serialization process of the screen manager. Of course you need to save out those
        ' values when deactivating and read them back, but that means either IsolatedStorage or
        ' using the PhoneApplicationService.Current.State dictionary.
    End Function
End Class