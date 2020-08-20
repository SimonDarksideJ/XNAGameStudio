#Region "File Description"
'-----------------------------------------------------------------------------
' InputState.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Input
Imports Microsoft.Xna.Framework.Input.Touch

''' <summary>
''' Helper for reading input from keyboard, gamepad, and touch input. This class 
''' tracks both the current and previous state of the input devices, and implements 
''' query methods for high level input actions such as "move up through the menu"
''' or "pause the game".
''' </summary>
Public Class InputState
    Public Const MaxInputs As Integer = 4

    Public ReadOnly CurrentKeyboardStates() As KeyboardState
    Public ReadOnly CurrentGamePadStates() As GamePadState

    Public ReadOnly LastKeyboardStates() As KeyboardState
    Public ReadOnly LastGamePadStates() As GamePadState

    Public ReadOnly GamePadWasConnected() As Boolean

    Public TouchState As TouchCollection

    Public ReadOnly Gestures As New List(Of GestureSample)()


    ''' <summary>
    ''' Constructs a new input state.
    ''' </summary>
    Public Sub New()
        CurrentKeyboardStates = New KeyboardState(MaxInputs - 1) {}
        CurrentGamePadStates = New GamePadState(MaxInputs - 1) {}

        LastKeyboardStates = New KeyboardState(MaxInputs - 1) {}
        LastGamePadStates = New GamePadState(MaxInputs - 1) {}

        GamePadWasConnected = New Boolean(MaxInputs - 1) {}
    End Sub

    ''' <summary>
    ''' Reads the latest state of the keyboard and gamepad.
    ''' </summary>
    Public Sub Update()
        For i As Integer = 0 To MaxInputs - 1
            LastKeyboardStates(i) = CurrentKeyboardStates(i)
            LastGamePadStates(i) = CurrentGamePadStates(i)

            CurrentKeyboardStates(i) = Keyboard.GetState(CType(i, PlayerIndex))
            CurrentGamePadStates(i) = GamePad.GetState(CType(i, PlayerIndex))

            ' Keep track of whether a gamepad has ever been
            ' connected, so we can detect if it is unplugged.
            If CurrentGamePadStates(i).IsConnected Then
                GamePadWasConnected(i) = True
            End If
        Next i

        TouchState = TouchPanel.GetState()

        Gestures.Clear()
        Do While TouchPanel.IsGestureAvailable
            Gestures.Add(TouchPanel.ReadGesture())
        Loop
    End Sub


    ''' <summary>
    ''' Helper for checking if a key was pressed during this update. The
    ''' controllingPlayer parameter specifies which player to read input for.
    ''' If this is null, it will accept input from any player. When a keypress
    ''' is detected, the output playerIndex reports which player pressed it.
    ''' </summary>
    Public Function IsKeyPressed(ByVal key As Keys, ByVal controllingPlayer? As PlayerIndex, <System.Runtime.InteropServices.Out()> ByRef playerIndex As PlayerIndex) As Boolean
        If controllingPlayer.HasValue Then
            ' Read input from the specified player.
            playerIndex = controllingPlayer.Value

            Dim i As Integer = CInt(playerIndex)

            Return CurrentKeyboardStates(i).IsKeyDown(key)
        Else
            ' Accept input from any player.
            Return (IsKeyPressed(key, playerIndex.One, playerIndex) OrElse IsKeyPressed(key, playerIndex.Two, playerIndex) OrElse IsKeyPressed(key, playerIndex.Three, playerIndex) OrElse IsKeyPressed(key, playerIndex.Four, playerIndex))
        End If
    End Function


    ''' <summary>
    ''' Helper for checking if a button was pressed during this update.
    ''' The controllingPlayer parameter specifies which player to read input for.
    ''' If this is null, it will accept input from any player. When a button press
    ''' is detected, the output playerIndex reports which player pressed it.
    ''' </summary>
    Public Function IsButtonPressed(ByVal button As Buttons, ByVal controllingPlayer? As PlayerIndex, <System.Runtime.InteropServices.Out()> ByRef playerIndex As PlayerIndex) As Boolean
        If controllingPlayer.HasValue Then
            ' Read input from the specified player.
            playerIndex = controllingPlayer.Value

            Dim i As Integer = CInt(playerIndex)

            Return CurrentGamePadStates(i).IsButtonDown(button)
        Else
            ' Accept input from any player.
            Return (IsButtonPressed(button, playerIndex.One, playerIndex) OrElse IsButtonPressed(button, playerIndex.Two, playerIndex) OrElse IsButtonPressed(button, playerIndex.Three, playerIndex) OrElse IsButtonPressed(button, playerIndex.Four, playerIndex))
        End If
    End Function


    ''' <summary>
    ''' Helper for checking if a key was newly pressed during this update. The
    ''' controllingPlayer parameter specifies which player to read input for.
    ''' If this is null, it will accept input from any player. When a keypress
    ''' is detected, the output playerIndex reports which player pressed it.
    ''' </summary>
    Public Function IsNewKeyPress(ByVal key As Keys, ByVal controllingPlayer? As PlayerIndex, <System.Runtime.InteropServices.Out()> ByRef playerIndex As PlayerIndex) As Boolean
        If controllingPlayer.HasValue Then
            ' Read input from the specified player.
            playerIndex = controllingPlayer.Value

            Dim i As Integer = CInt(playerIndex)

            Return (CurrentKeyboardStates(i).IsKeyDown(key) AndAlso LastKeyboardStates(i).IsKeyUp(key))
        Else
            ' Accept input from any player.
            Return (IsNewKeyPress(key, playerIndex.One, playerIndex) OrElse IsNewKeyPress(key, playerIndex.Two, playerIndex) OrElse IsNewKeyPress(key, playerIndex.Three, playerIndex) OrElse IsNewKeyPress(key, playerIndex.Four, playerIndex))
        End If
    End Function


    ''' <summary>
    ''' Helper for checking if a button was newly pressed during this update.
    ''' The controllingPlayer parameter specifies which player to read input for.
    ''' If this is null, it will accept input from any player. When a button press
    ''' is detected, the output playerIndex reports which player pressed it.
    ''' </summary>
    Public Function IsNewButtonPress(ByVal button As Buttons, ByVal controllingPlayer? As PlayerIndex, <System.Runtime.InteropServices.Out()> ByRef playerIndex As PlayerIndex) As Boolean
        If controllingPlayer.HasValue Then
            ' Read input from the specified player.
            playerIndex = controllingPlayer.Value

            Dim i As Integer = CInt(playerIndex)

            Return (CurrentGamePadStates(i).IsButtonDown(button) AndAlso LastGamePadStates(i).IsButtonUp(button))
        Else
            ' Accept input from any player.
            Return (IsNewButtonPress(button, playerIndex.One, playerIndex) OrElse IsNewButtonPress(button, playerIndex.Two, playerIndex) OrElse IsNewButtonPress(button, playerIndex.Three, playerIndex) OrElse IsNewButtonPress(button, playerIndex.Four, playerIndex))
        End If
    End Function
End Class