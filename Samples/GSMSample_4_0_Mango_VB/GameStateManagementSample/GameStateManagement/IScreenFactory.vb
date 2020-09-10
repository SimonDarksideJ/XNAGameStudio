#Region "File Description"
'-----------------------------------------------------------------------------
' IScreenFactory.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

''' <summary>
''' Defines an object that can create a screen when given its type.
''' </summary>
Public Interface IScreenFactory
    ''' <summary>
    ''' Creates a GameScreen from the given type.
    ''' </summary>
    ''' <param name="screenType">The type of screen to create.</param>
    ''' <returns>The newly created screen.</returns>
    Function CreateScreen(ByVal screenType As Type) As GameScreen
End Interface