#Region "File Description"
'-----------------------------------------------------------------------------
' TransitionGameComponentAnimation.vb
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
''' An animation which moves a component from one point to the other.
''' </summary>
Public Class TransitionGameComponentAnimation
    Inherits AnimatedGameComponentAnimation
#Region "Fields"
    Private sourcePosition As Vector2
    Private positionDelta As Vector2
    Private percent As Single = 0
    Private destinationPosition As Vector2
#End Region

#Region "Initializations"
    ''' <summary>
    ''' Initializes a new instance of the class.
    ''' </summary>
    ''' <param name="sourcePosition">The source position.</param>
    ''' <param name="destinationPosition">The destination position.</param>
    Public Sub New(ByVal sourcePosition As Vector2, ByVal destinationPosition As Vector2)
        Me.destinationPosition = destinationPosition
        Me.sourcePosition = sourcePosition
        positionDelta = destinationPosition - sourcePosition
    End Sub
#End Region

    ''' <summary>
    ''' Runs the transition animation.
    ''' </summary>
    ''' <param name="gameTime">Game time information.</param>
    Public Overrides Sub Run(ByVal gameTime As GameTime)
        If IsStarted() Then
            ' Calculate the animation's completion percentage.
            percent += CSng(gameTime.ElapsedGameTime.TotalSeconds / Duration.TotalSeconds)

            ' Move the component towards the destination as the animation
            ' progresses
            Component.CurrentPosition = sourcePosition + positionDelta * percent

            If IsDone() Then
                Component.CurrentPosition = destinationPosition
            End If
        End If
    End Sub
End Class
