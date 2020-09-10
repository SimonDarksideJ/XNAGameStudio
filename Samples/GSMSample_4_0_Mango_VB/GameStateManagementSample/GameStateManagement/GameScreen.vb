#Region "File Description"
'-----------------------------------------------------------------------------
' GameScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.IO
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Input.Touch

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
    ''' <summary>
    ''' Normally when one screen is brought up over the top of another,
    ''' the first screen will transition off to make room for the new
    ''' one. This property indicates whether the screen is only a small
    ''' popup, in which case screens underneath it do not need to bother
    ''' transitioning off.
    ''' </summary>
    Public Property IsPopup() As Boolean = False 

    ''' <summary>
    ''' Indicates how long the screen takes to
    ''' transition on when it is activated.
    ''' </summary>
    Public Property TransitionOnTime() As TimeSpan = TimeSpan.Zero


    ''' <summary>
    ''' Indicates how long the screen takes to
    ''' transition off when it is deactivated.
    ''' </summary>
    Public Property TransitionOffTime() As TimeSpan= TimeSpan.Zero


    ''' <summary>
    ''' Gets the current position of the screen transition, ranging
    ''' from zero (fully active, no transition) to one (transitioned
    ''' fully off to nothing).
    ''' </summary>
    Public Property TransitionPosition() As Single = 1


    ''' <summary>
    ''' Gets the current alpha of the screen transition, ranging
    ''' from 1 (fully active, no transition) to 0 (transitioned
    ''' fully off to nothing).
    ''' </summary>
    Public ReadOnly Property TransitionAlpha() As Single
        Get
            Return 1.0F - TransitionPosition
        End Get
    End Property


    ''' <summary>
    ''' Gets the current screen transition state.
    ''' </summary>
    Public Property ScreenState() As ScreenState

    ''' <summary>
    ''' There are two possible reasons why a screen might be transitioning
    ''' off. It could be temporarily going away to make room for another
    ''' screen that is on top of it, or it could be going away for good.
    ''' This property indicates whether the screen is exiting for real:
    ''' if set, the screen will automatically remove itself as soon as the
    ''' transition finishes.
    ''' </summary>
    Public Property IsExiting() As Boolean = False


    ''' <summary>
    ''' Checks whether this screen is active and can respond to user input.
    ''' </summary>
    Public ReadOnly Property IsActive() As Boolean
        Get
            Return (Not otherScreenHasFocus) AndAlso (ScreenState = ScreenState.TransitionOn OrElse ScreenState = ScreenState.Active)
        End Get
    End Property

    Private otherScreenHasFocus As Boolean


    ''' <summary>
    ''' Gets the manager that this screen belongs to.
    ''' </summary>
    Public Property ScreenManager() As ScreenManager

    ''' <summary>
    ''' Gets the index of the player who is currently controlling this screen,
    ''' or null if it is accepting input from any player. This is used to lock
    ''' the game to a specific player profile. The main menu responds to input
    ''' from any connected gamepad, but whichever player makes a selection from
    ''' this menu is given control over all subsequent screens, so other gamepads
    ''' are inactive until the controlling player returns to the main menu.
    ''' </summary>
    Public Property ControllingPlayer() As PlayerIndex?
 


    ''' <summary>
    ''' Gets the gestures the screen is interested in. Screens should be as specific
    ''' as possible with gestures to increase the accuracy of the gesture engine.
    ''' For example, most menus only need Tap or perhaps Tap and VerticalDrag to operate.
    ''' These gestures are handled by the ScreenManager when screens change and
    ''' all gestures are placed in the InputState passed to the HandleInput method.
    ''' </summary>
    Public Property EnabledGestures() As GestureType= GestureType.None


    ''' <summary>
    ''' Gets whether or not this screen is serializable. If this is true,
    ''' the screen will be recorded into the screen manager's state and
    ''' its Serialize and Deserialize methods will be called as appropriate.
    ''' If this is false, the screen will be ignored during serialization.
    ''' By default, all screens are assumed to be serializable.
    ''' </summary>
    Public Property IsSerializable() As Boolean = True 
 

    ''' <summary>
    ''' Activates the screen. Called when the screen is added to the screen manager or if the game resumes
    ''' from being paused or tombstoned.
    ''' </summary>
    ''' <param name="instancePreserved">
    ''' True if the game was preserved during deactivation, false if the screen is just being added or if the game was tombstoned.
    ''' On Xbox and Windows this will always be false.
    ''' </param>
    Public Overridable Sub Activate(ByVal instancePreserved As Boolean)
    End Sub


    ''' <summary>
    ''' Deactivates the screen. Called when the game is being deactivated due to pausing or tombstoning.
    ''' </summary>
    Public Overridable Sub Deactivate()
    End Sub


    ''' <summary>
    ''' Unload content for the screen. Called when the screen is removed from the screen manager.
    ''' </summary>
    Public Overridable Sub Unload()
    End Sub


    ''' <summary>
    ''' Allows the screen to run logic, such as updating the transition position.
    ''' Unlike HandleInput, this method is called regardless of whether the screen
    ''' is active, hidden, or in the middle of a transition.
    ''' </summary>
    Public Overridable Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
        Me.otherScreenHasFocus = otherScreenHasFocus

        If IsExiting Then
            ' If the screen is going away to die, it should transition off.
            ScreenState = ScreenState.TransitionOff

            If Not UpdateTransition(gameTime, TransitionOffTime, 1) Then
                ' When the transition finishes, remove the screen.
                ScreenManager.RemoveScreen(Me)
            End If
        ElseIf coveredByOtherScreen Then
            ' If the screen is covered by another, it should transition off.
            If UpdateTransition(gameTime, TransitionOffTime, 1) Then
                ' Still busy transitioning.
                ScreenState = ScreenState.TransitionOff
            Else
                ' Transition finished!
                ScreenState = ScreenState.Hidden
            End If
        Else
            ' Otherwise the screen should transition on and become active.
            If UpdateTransition(gameTime, TransitionOnTime, -1) Then
                ' Still busy transitioning.
                ScreenState = ScreenState.TransitionOn
            Else
                ' Transition finished!
                ScreenState = ScreenState.Active
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
        TransitionPosition += transitionDelta * direction

        ' Did we reach the end of the transition?
        If ((direction < 0) AndAlso (TransitionPosition <= 0)) OrElse ((direction > 0) AndAlso (TransitionPosition >= 1)) Then
            TransitionPosition = MathHelper.Clamp(TransitionPosition, 0, 1)
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
    Public Overridable Sub HandleInput(ByVal gameTime As GameTime, ByVal input As InputState)
    End Sub


    ''' <summary>
    ''' This is called when the screen should draw itself.
    ''' </summary>
    Public Overridable Sub Draw(ByVal gameTime As GameTime)
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
            IsExiting = True
        End If
    End Sub
End Class