#Region "File Description"
'-----------------------------------------------------------------------------
' GameScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.IO

Namespace GameStateManagement
	''' <summary>
	''' Enum describes the screen transition state.
	''' </summary>
	Public Enum ScreenState
		TransitionOn
		Active
		TransitionOff
		Hidden
	End Enum

	''' <summary>
	''' A screen is a single layer that has update and draw logic, and which
	''' can be combined with other layers to build up a complex menu system.
	''' For instance the main menu, the options menu, the "are you sure you
	''' want to quit" message box, and the main game itself are all implemented
	''' as screens.
	''' </summary>
	Public MustInherit Class GameScreen
		#Region "Properties"


		''' <summary>
		''' Normally when one screen is brought up over the top of another,
		''' the first screen will transition off to make room for the new
		''' one. This property indicates whether the screen is only a small
		''' popup, in which case screens underneath it do not need to bother
		''' transitioning off.
		''' </summary>
        Public Property IsPopup As Boolean
            Get
                Return _isPopup
            End Get
            Protected Set(ByVal value As Boolean)
                _isPopup = value
            End Set
        End Property

        Private _isPopup As Boolean = False

		''' <summary>
		''' Indicates how long the screen takes to
		''' transition on when it is activated.
		''' </summary>
        Public Property TransitionOnTime As TimeSpan
            Get
                Return _transitionOnTime
            End Get
            Protected Set(ByVal value As TimeSpan)
                _transitionOnTime = value
            End Set
        End Property

        Private _transitionOnTime As TimeSpan = TimeSpan.Zero


        ''' <summary>
        ''' Indicates how long the screen takes to
        ''' transition off when it is deactivated.
        ''' </summary>
        Public Property TransitionOffTime As TimeSpan
            Get
                Return _transitionOffTime
            End Get
            Protected Set(ByVal value As TimeSpan)
                _transitionOffTime = value
            End Set
        End Property

        Private _transitionOffTime As TimeSpan = TimeSpan.Zero


        ''' <summary>
        ''' Gets the current position of the screen transition, ranging
        ''' from zero (fully active, no transition) to one (transitioned
        ''' fully off to nothing).
        ''' </summary>
        Public Property TransitionPosition As Single
            Get
                Return _transitionPosition
            End Get
            Protected Set(ByVal value As Single)
                _transitionPosition = value
            End Set
        End Property

        Private _transitionPosition As Single = 1


        ''' <summary>
        ''' Gets the current alpha of the screen transition, ranging
        ''' from 1 (fully active, no transition) to 0 (transitioned
        ''' fully off to nothing).
        ''' </summary>
        Public ReadOnly Property TransitionAlpha As Single
            Get
                Return 1.0F - TransitionPosition
            End Get
        End Property


        ''' <summary>
        ''' Gets the current screen transition state.
        ''' </summary>
        Public Property ScreenState As ScreenState
            Get
                Return _screenState
            End Get
            Protected Set(ByVal value As ScreenState)
                _screenState = value
            End Set
        End Property

        Private _screenState As ScreenState = ScreenState.TransitionOn


        ''' <summary>
        ''' There are two possible reasons why a screen might be transitioning
        ''' off. It could be temporarily going away to make room for another
        ''' screen that is on top of it, or it could be going away for good.
        ''' This property indicates whether the screen is exiting for real:
        ''' if set, the screen will automatically remove itself as soon as the
        ''' transition finishes.
        ''' </summary>
        Public Property IsExiting As Boolean
            Get
                Return _isExiting
            End Get
            Protected Friend Set(ByVal value As Boolean)
                _isExiting = value
            End Set
        End Property

        Private _isExiting As Boolean = False


        ''' <summary>
        ''' Checks whether this screen is active and can respond to user input.
        ''' </summary>
        Public ReadOnly Property IsActive As Boolean
            Get
                Return (Not otherScreenHasFocus) AndAlso (_screenState = ScreenState.TransitionOn OrElse _screenState = ScreenState.Active)
            End Get
        End Property

        Private otherScreenHasFocus As Boolean


        ''' <summary>
        ''' Gets the manager that this screen belongs to.
        ''' </summary>
        Public Property ScreenManager As ScreenManager
            Get
                Return _screenManager
            End Get
            Friend Set(ByVal value As ScreenManager)
                _screenManager = value
            End Set
        End Property

        Private _screenManager As ScreenManager


        ''' <summary>
        ''' Gets the index of the player who is currently controlling this screen,
        ''' or null if it is accepting input from any player. This is used to lock
        ''' the game to a specific player profile. The main menu responds to input
        ''' from any connected gamepad, but whichever player makes a selection from
        ''' this menu is given control over all subsequent screens, so other gamepads
        ''' are inactive until the controlling player returns to the main menu.
        ''' </summary>
        Public Property ControllingPlayer As PlayerIndex?
            Get
                Return _controllingPlayer
            End Get
            Friend Set(ByVal value? As PlayerIndex)
                _controllingPlayer = value
            End Set
        End Property

        Private _controllingPlayer? As PlayerIndex

#If WINDOWS_PHONE Then
        ''' <summary>
        ''' Gets the gestures the screen is interested in. Screens should be as specific
        ''' as possible with gestures to increase the accuracy of the gesture engine.
        ''' For example, most menus only need Tap or perhaps Tap and VerticalDrag to operate.
        ''' These gestures are handled by the ScreenManager when screens change and
        ''' all gestures are placed in the InputState passed to the HandleInput method.
        ''' </summary>
        Public Property EnabledGestures As GestureType
            Get
                Return _enabledGestures
            End Get
            Protected Set(ByVal value As GestureType)
                _enabledGestures = value

                ' the screen manager handles this during screen changes, but
                ' if this screen is active and the gesture types are changing,
                ' we have to update the TouchPanel ourself.
                If ScreenState = ScreenState.Active Then
                    TouchPanel.EnabledGestures = value
                End If
            End Set
        End Property

        Private _enabledGestures As GestureType = GestureType.None
#End If
        ''' <summary>
        ''' Gets whether or not this screen is serializable. If this is true,
        ''' the screen will be recorded into the screen manager's state and
        ''' its Serialize and Deserialize methods will be called as appropriate.
        ''' If this is false, the screen will be ignored during serialization.
        ''' By default, all screens are assumed to be serializable.
        ''' </summary>
        Public Property IsSerializable As Boolean
            Get
                Return _isSerializable
            End Get
            Protected Set(ByVal value As Boolean)
                _isSerializable = value
            End Set
        End Property

        Private _isSerializable As Boolean = True


#End Region

#Region "Initialization"


        ''' <summary>
        ''' Load graphics content for the screen.
        ''' </summary>
        Public Overridable Sub LoadContent()
        End Sub


        ''' <summary>
        ''' Unload content for the screen.
        ''' </summary>
        Public Overridable Sub UnloadContent()
        End Sub


#End Region

#Region "Update and Draw"


        ''' <summary>
        ''' Allows the screen to run logic, such as updating the transition position.
        ''' Unlike HandleInput, this method is called regardless of whether the screen
        ''' is active, hidden, or in the middle of a transition.
        ''' </summary>
        Public Overridable Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
            Me.otherScreenHasFocus = otherScreenHasFocus

            If _isExiting Then
                ' If the screen is going away to die, it should transition off.
                _screenState = ScreenState.TransitionOff

                If Not UpdateTransition(gameTime, _transitionOffTime, 1) Then
                    ' When the transition finishes, remove the screen.
                    ScreenManager.RemoveScreen(Me)
                End If
            ElseIf coveredByOtherScreen Then
                ' If the screen is covered by another, it should transition off.
                If UpdateTransition(gameTime, _transitionOffTime, 1) Then
                    ' Still busy transitioning.
                    _screenState = ScreenState.TransitionOff
                Else
                    ' Transition finished!
                    _screenState = ScreenState.Hidden
                End If
            Else
                ' Otherwise the screen should transition on and become active.
                If UpdateTransition(gameTime, _transitionOnTime, -1) Then
                    ' Still busy transitioning.
                    _screenState = ScreenState.TransitionOn
                Else
                    ' Transition finished!
                    _screenState = ScreenState.Active
                End If
            End If
        End Sub


        ''' <summary>
        ''' Helper for updating the screen transition position.
        ''' </summary>
        Private Function UpdateTransition(ByVal gameTime As GameTime, ByVal time As TimeSpan, ByVal direction As Integer) As Boolean
            ' How much should we move by?
            Dim transitionDelta As Single

            If time = TimeSpan.Zero Then
                transitionDelta = 1
            Else
                transitionDelta = CSng(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds)
            End If

            ' Update the transition position.
            _transitionPosition += transitionDelta * direction

            ' Did we reach the end of the transition?
            If ((direction < 0) AndAlso (_transitionPosition <= 0)) OrElse ((direction > 0) AndAlso (_transitionPosition >= 1)) Then
                _transitionPosition = MathHelper.Clamp(_transitionPosition, 0, 1)
                Return False
            End If

            ' Otherwise we are still busy transitioning.
            Return True
        End Function


        ''' <summary>
        ''' Allows the screen to handle user input. Unlike Update, this method
        ''' is only called when the screen is active, and not when some other
        ''' screen has taken the focus.
        ''' </summary>
        Public Overridable Sub HandleInput(ByVal input As InputState)
        End Sub


        ''' <summary>
        ''' This is called when the screen should draw itself.
        ''' </summary>
        Public Overridable Sub Draw(ByVal gameTime As GameTime)
        End Sub


#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Tells the screen to serialize its state into the given stream.
        ''' </summary>
        Public Overridable Sub Serialize(ByVal stream As Stream)
        End Sub

        ''' <summary>
        ''' Tells the screen to deserialize its state from the given stream.
        ''' </summary>
        Public Overridable Sub Deserialize(ByVal stream As Stream)
        End Sub

        ''' <summary>
        ''' Tells the screen to go away. Unlike ScreenManager.RemoveScreen, which
        ''' instantly kills the screen, this method respects the transition timings
        ''' and will give the screen a chance to gradually transition off.
        ''' </summary>
        Public Sub ExitScreen()
            If TransitionOffTime = TimeSpan.Zero Then
                ' If the screen has a zero transition time, remove it immediately.
                ScreenManager.RemoveScreen(Me)
            Else
                ' Otherwise flag that it should transition off and then exit.
                _isExiting = True
            End If
        End Sub


#End Region

		#Region "Helper Methods"
		''' <summary>
		''' A helper method which loads assets using the screen manager's
		''' associated game content loader.
		''' </summary>
		''' <typeparam name="T">Type of asset.</typeparam>
		''' <param name="assetName">Asset name, relative to the loader root
		''' directory, and not including the .xnb extension.</param>
		''' <returns></returns>
		Public Function Load(Of T)(ByVal assetName As String) As T
			Return ScreenManager.Game.Content.Load(Of T)(assetName)
		End Function
		#End Region
	End Class
End Namespace
