#Region "File Description"
'-----------------------------------------------------------------------------
' InputState.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Namespace GameStateManagement
	''' <summary>
	''' Helper for reading input from keyboard, gamepad, and touch input. This class 
	''' tracks both the current and previous state of the input devices, and implements 
	''' query methods for high level input actions such as "move up through the menu"
	''' or "pause the game".
	''' </summary>
	Public Class InputState
		#Region "Fields"

		Public Const MaxInputs As Integer = 4

        Public ReadOnly CurrentKeyboardStates() As KeyboardState
		Public ReadOnly CurrentGamePadStates() As GamePadState

		Public ReadOnly LastKeyboardStates() As KeyboardState
		Public ReadOnly LastGamePadStates() As GamePadState

		Public ReadOnly GamePadWasConnected() As Boolean

#If WINDOWS_PHONE Then
		Public TouchState As TouchCollection

        Public ReadOnly Gestures As New List(Of GestureSample)
#End If

		#End Region

		#Region "Initialization"


		''' <summary>
		''' Constructs a new input state.
		''' </summary>
		Public Sub New()
			CurrentKeyboardStates = New KeyboardState(MaxInputs - 1){}
			CurrentGamePadStates = New GamePadState(MaxInputs - 1){}

			LastKeyboardStates = New KeyboardState(MaxInputs - 1){}
			LastGamePadStates = New GamePadState(MaxInputs - 1){}

			GamePadWasConnected = New Boolean(MaxInputs - 1){}
		End Sub


		#End Region

		#Region "Public Methods"


		''' <summary>
		''' Reads the latest state of the keyboard and gamepad.
		''' </summary>
		Public Sub Update()
            For i = 0 To MaxInputs - 1
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
#If WINDOWS_PHONE Then
            TouchState = TouchPanel.GetState

            Gestures.Clear()
			Do While TouchPanel.IsGestureAvailable
                Gestures.Add(TouchPanel.ReadGesture)
			Loop
#End If
		End Sub


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

				Dim i As Integer = CInt(Fix(playerIndex))

				Return (CurrentKeyboardStates(i).IsKeyDown(key) AndAlso LastKeyboardStates(i).IsKeyUp(key))
			Else
				' Accept input from any player.
				Return (IsNewKeyPress(key, PlayerIndex.One, playerIndex) OrElse IsNewKeyPress(key, PlayerIndex.Two, playerIndex) OrElse IsNewKeyPress(key, PlayerIndex.Three, playerIndex) OrElse IsNewKeyPress(key, PlayerIndex.Four, playerIndex))
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

				Dim i As Integer = CInt(Fix(playerIndex))

				Return (CurrentGamePadStates(i).IsButtonDown(button) AndAlso LastGamePadStates(i).IsButtonUp(button))
			Else
				' Accept input from any player.
				Return (IsNewButtonPress(button, PlayerIndex.One, playerIndex) OrElse IsNewButtonPress(button, PlayerIndex.Two, playerIndex) OrElse IsNewButtonPress(button, PlayerIndex.Three, playerIndex) OrElse IsNewButtonPress(button, PlayerIndex.Four, playerIndex))
			End If
		End Function


		''' <summary>
		''' Checks for a "menu select" input action.
		''' The controllingPlayer parameter specifies which player to read input for.
		''' If this is null, it will accept input from any player. When the action
		''' is detected, the output playerIndex reports which player pressed it.
		''' </summary>
		Public Function IsMenuSelect(ByVal controllingPlayer? As PlayerIndex, <System.Runtime.InteropServices.Out()> ByRef playerIndex As PlayerIndex) As Boolean
			Return IsNewKeyPress(Keys.Space, controllingPlayer, playerIndex) OrElse IsNewKeyPress(Keys.Enter, controllingPlayer, playerIndex) OrElse IsNewButtonPress(Buttons.A, controllingPlayer, playerIndex) OrElse IsNewButtonPress(Buttons.Start, controllingPlayer, playerIndex)
		End Function


		''' <summary>
		''' Checks for a "menu cancel" input action.
		''' The controllingPlayer parameter specifies which player to read input for.
		''' If this is null, it will accept input from any player. When the action
		''' is detected, the output playerIndex reports which player pressed it.
		''' </summary>
		Public Function IsMenuCancel(ByVal controllingPlayer? As PlayerIndex, <System.Runtime.InteropServices.Out()> ByRef playerIndex As PlayerIndex) As Boolean
			Return IsNewKeyPress(Keys.Escape, controllingPlayer, playerIndex) OrElse IsNewButtonPress(Buttons.B, controllingPlayer, playerIndex) OrElse IsNewButtonPress(Buttons.Back, controllingPlayer, playerIndex)
		End Function


		''' <summary>
		''' Checks for a "menu up" input action.
		''' The controllingPlayer parameter specifies which player to read
		''' input for. If this is null, it will accept input from any player.
		''' </summary>
		Public Function IsMenuUp(ByVal controllingPlayer? As PlayerIndex) As Boolean
			Dim playerIndex As PlayerIndex

			Return IsNewKeyPress(Keys.Up, controllingPlayer, playerIndex) OrElse IsNewKeyPress(Keys.Left, controllingPlayer, playerIndex) OrElse IsNewButtonPress(Buttons.DPadLeft, controllingPlayer, playerIndex) OrElse IsNewButtonPress(Buttons.LeftThumbstickLeft, controllingPlayer, playerIndex)
		End Function


		''' <summary>
		''' Checks for a "menu down" input action.
		''' The controllingPlayer parameter specifies which player to read
		''' input for. If this is null, it will accept input from any player.
		''' </summary>
		Public Function IsMenuDown(ByVal controllingPlayer? As PlayerIndex) As Boolean
			Dim playerIndex As PlayerIndex

			Return IsNewKeyPress(Keys.Down, controllingPlayer, playerIndex) OrElse IsNewKeyPress(Keys.Right, controllingPlayer, playerIndex) OrElse IsNewButtonPress(Buttons.DPadRight, controllingPlayer, playerIndex) OrElse IsNewButtonPress(Buttons.LeftThumbstickRight, controllingPlayer, playerIndex)
		End Function


		''' <summary>
		''' Checks for a "pause the game" input action.
		''' The controllingPlayer parameter specifies which player to read
		''' input for. If this is null, it will accept input from any player.
		''' </summary>
		Public Function IsPauseGame(ByVal controllingPlayer? As PlayerIndex) As Boolean
			Dim playerIndex As PlayerIndex

			Return IsNewKeyPress(Keys.Escape, controllingPlayer, playerIndex) OrElse IsNewButtonPress(Buttons.Back, controllingPlayer, playerIndex) OrElse IsNewButtonPress(Buttons.Start, controllingPlayer, playerIndex)
		End Function


		#End Region
	End Class
End Namespace
