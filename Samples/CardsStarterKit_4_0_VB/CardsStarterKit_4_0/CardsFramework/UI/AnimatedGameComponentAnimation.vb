#Region "File Description"
'-----------------------------------------------------------------------------
' AnimatedGameComponentAnimation.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Microsoft.Xna.Framework
#End Region

''' <summary>
''' Represents an animation that can alter an animated component.
''' </summary>
Public Class AnimatedGameComponentAnimation
#Region "Fields and Properties"
    Protected Property Elapsed As TimeSpan
    Private _component As AnimatedGameComponent
    Public Property Component As AnimatedGameComponent
        Get
            Return _component
        End Get
        Friend Set(ByVal value As AnimatedGameComponent)
            _component = value
        End Set
    End Property
    ''' <summary>
    ''' An action to perform before the animation begins.
    ''' </summary>
    Public PerformBeforeStart As Action(Of Object)
    Public Property PerformBeforSartArgs As Object
    ''' <summary>
    ''' An action to perform once the animation is complete.
    ''' </summary>
    Public PerformWhenDone As Action(Of Object)
    Public Property PerformWhenDoneArgs As Object

    Private _animationCycles As UInteger = 1
    ''' <summary>
    ''' Sets the amount of cycles to perform for the animation.
    ''' </summary>
    Public Property AnimationCycles As UInteger
        Get
            Return _animationCycles
        End Get
        Set(ByVal value As UInteger)
            If value > 0 Then
                _animationCycles = value
            End If
        End Set
    End Property

    Public Property StartTime As Date
    Public Property Duration As TimeSpan

    ''' <summary>
    ''' Returns the time at which the animation is estimated to end.
    ''' </summary>
    Public ReadOnly Property EstimatedTimeForAnimationCompletion As TimeSpan
        Get
            If _isStarted Then
                Return (Duration.Subtract(Elapsed))
            Else
                Return StartTime.Subtract(Date.Now) + Duration
            End If
        End Get
    End Property

    Public Property IsLooped As Boolean

    Private _isDone As Boolean = False

    Private _isStarted As Boolean = False
#End Region

#Region "Initiaizations"
    ''' <summary>
    ''' Initializes a new instance of the class. Be default, an animation starts
    ''' immediately and has a duration of 150 milliseconds.
    ''' </summary>
    Public Sub New()
        StartTime = Date.Now
        Duration = TimeSpan.FromMilliseconds(150)
    End Sub
#End Region

    ''' <summary>
    ''' Check whether or not the animation is done playing. Looped animations
    ''' never finish playing.
    ''' </summary>
    ''' <returns>Whether or not the animation is done playing</returns>
    Public Function IsDone() As Boolean
        If Not _isDone Then
            _isDone = (Not IsLooped) AndAlso (Elapsed >= Duration)
            If _isDone AndAlso PerformWhenDone IsNot Nothing Then
                PerformWhenDone(PerformWhenDoneArgs)
                PerformWhenDone = Nothing
            End If
        End If
        Return _isDone
    End Function

    ''' <summary>
    ''' Returns whether or not the animation is started. As a side-effect, starts
    ''' the animation if it is not started and it is time for it to start.
    ''' </summary>
    ''' <returns>Whether or not the animation is started</returns>
    Public Function IsStarted() As Boolean
        If Not _isStarted Then
            If StartTime <= Date.Now Then
                If PerformBeforeStart IsNot Nothing Then
                    PerformBeforeStart(PerformBeforSartArgs)
                    PerformBeforeStart = Nothing
                End If
                StartTime = Date.Now
                _isStarted = True
            End If
        End If
        Return _isStarted
    End Function

    ''' <summary>
    ''' Increases the amount of elapsed time as seen by the animation, but only
    ''' if the animation is started.
    ''' </summary>
    ''' <param name="elapsedTime">The timespan by which to incerase the animation's
    ''' elapsed time.</param>
    Friend Sub AccumulateElapsedTime(ByVal elapsedTime As TimeSpan)
        If _isStarted Then
            Elapsed = Elapsed.Add(elapsedTime)
        End If
    End Sub

    ''' <summary>
    ''' Runs the animation.
    ''' </summary>
    ''' <param name="gameTime">Game time information.</param>
    Public Overridable Sub Run(ByVal gameTime As GameTime)
        Dim isStarted As Boolean = Me.IsStarted
    End Sub
End Class
