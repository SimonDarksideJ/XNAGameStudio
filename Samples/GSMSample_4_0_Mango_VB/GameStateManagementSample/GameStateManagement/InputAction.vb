#Region "File Description"
'-----------------------------------------------------------------------------
' InputAction.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Input

''' <summary>
''' Defines an action that is designated by some set of buttons and/or keys.
''' 
''' The way actions work is that you define a set of buttons and keys that trigger the action. You can
''' then evaluate the action against an InputState which will test to see if any of the buttons or keys
''' are pressed by a player. You can also set a flag that indicates if the action only occurs once when
''' the buttons/keys are first pressed or whether the action should occur each frame.
''' 
''' Using this InputAction class means that you can configure new actions based on keys and buttons
''' without having to directly modify the InputState type. This means more customization by your games
''' without having to change the core classes of Game State Management.
''' </summary>
Public Class InputAction
    Private ReadOnly buttons() As Buttons
    Private ReadOnly keys() As Keys
    Private ReadOnly newPressOnly As Boolean

    ' These delegate types map to the methods on InputState. We use these to simplify the evalute method
    ' by allowing us to map the appropriate delegates and invoke them, rather than having two separate code paths.
    Private Delegate Function ButtonPress(ByVal button As Buttons, ByVal controllingPlayer? As PlayerIndex, <System.Runtime.InteropServices.Out()> ByRef player As PlayerIndex) As Boolean
    Private Delegate Function KeyPress(ByVal key As Keys, ByVal controllingPlayer? As PlayerIndex, <System.Runtime.InteropServices.Out()> ByRef player As PlayerIndex) As Boolean

    ''' <summary>
    ''' Initializes a new InputAction.
    ''' </summary>
    ''' <param name="buttons">An array of buttons that can trigger the action.</param>
    ''' <param name="keys">An array of keys that can trigger the action.</param>
    ''' <param name="newPressOnly">Whether the action only occurs on the first press of one of the buttons/keys, 
    ''' false if it occurs each frame one of the buttons/keys is down.</param>
    Public Sub New(ByVal buttons() As Buttons, ByVal keys() As Keys, ByVal newPressOnly As Boolean)
        ' Store the buttons and keys. If the arrays are null, we create a 0 length array so we don't
        ' have to do null checks in the Evaluate method
        Me.buttons = If(buttons IsNot Nothing, TryCast(buttons.Clone(), Buttons()), New Buttons() {})
        Me.keys = If(keys IsNot Nothing, TryCast(keys.Clone(), Keys()), New Keys() {})

        Me.newPressOnly = newPressOnly
    End Sub

    ''' <summary>
    ''' Evaluates the action against a given InputState.
    ''' </summary>
    ''' <param name="state">The InputState to test for the action.</param>
    ''' <param name="controllingPlayer">The player to test, or null to allow any player.</param>
    ''' <param name="player">If controllingPlayer is null, this is the player that performed the action.</param>
    ''' <returns>True if the action occurred, false otherwise.</returns>
    Public Function Evaluate(ByVal state As InputState, ByVal controllingPlayer? As PlayerIndex, <System.Runtime.InteropServices.Out()> ByRef player As PlayerIndex) As Boolean
        ' Figure out which delegate methods to map from the state which takes care of our "newPressOnly" logic
        Dim buttonTest As ButtonPress
        Dim keyTest As KeyPress
        If newPressOnly Then
            buttonTest = AddressOf state.IsNewButtonPress
            keyTest = AddressOf state.IsNewKeyPress
        Else
            buttonTest = AddressOf state.IsButtonPressed
            keyTest = AddressOf state.IsKeyPressed
        End If

        ' Now we simply need to invoke the appropriate methods for each button and key in our collections
        For Each button As Buttons In buttons
            If buttonTest(button, controllingPlayer, player) Then
                Return True
            End If
        Next button
        For Each key As Keys In keys
            If keyTest(key, controllingPlayer, player) Then
                Return True
            End If
        Next key

        ' If we got here, the action is not matched
        player = PlayerIndex.One
        Return False
    End Function
End Class