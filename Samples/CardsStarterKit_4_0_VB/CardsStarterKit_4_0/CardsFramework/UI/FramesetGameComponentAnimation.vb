#Region "File Description"
'-----------------------------------------------------------------------------
' FramesetGameComponentAnimation.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework
#End Region


''' <summary>
''' A "typical" animation that consists of alternating between a set of frames.
''' </summary>
Public Class FramesetGameComponentAnimation
    Inherits AnimatedGameComponentAnimation
#Region "Fields"
    Private framesTexture As Texture2D
    Private numberOfFrames As Integer
    Private numberOfFramePerRow As Integer
    Private frameSize As Vector2

    Private percent As Double = 0
#End Region

#Region "Initializations"
    ''' <summary>
    ''' Creates a new instance of the class.
    ''' </summary>
    ''' <param name="framesTexture">The frames texture (animation sheet).</param>
    ''' <param name="numberOfFrames">The number of frames in the sheet.</param>
    ''' <param name="numberOfFramePerRow">The number of frame per row.</param>
    ''' <param name="frameSize">Size of the frame.</param>
    Public Sub New(ByVal framesTexture As Texture2D, ByVal numberOfFrames As Integer, ByVal numberOfFramePerRow As Integer, ByVal frameSize As Vector2)
        Me.framesTexture = framesTexture
        Me.numberOfFrames = numberOfFrames
        Me.numberOfFramePerRow = numberOfFramePerRow
        Me.frameSize = frameSize
    End Sub
#End Region

    ''' <summary>
    ''' Runs the frame set animation.
    ''' </summary>
    ''' <param name="gameTime">Game time information.</param>
    Public Overrides Sub Run(ByVal gameTime As GameTime)
        If IsStarted() Then
            ' Calculate the completion percent of the animation
            percent += (((gameTime.ElapsedGameTime.TotalMilliseconds / (Duration.TotalMilliseconds / AnimationCycles)) * 100))

            If percent >= 100 Then
                percent = 0
            End If

            ' Calculate the current frame index
            Dim animationIndex As Integer = CInt(Fix(numberOfFrames * percent / 100))
            Component.CurrentSegment = New Rectangle(CInt(Fix(frameSize.X)) * (animationIndex Mod numberOfFramePerRow), CInt(Fix(frameSize.Y)) * (animationIndex \ numberOfFramePerRow), CInt(Fix(frameSize.X)), CInt(Fix(frameSize.Y)))
            Component.CurrentFrame = framesTexture

        Else
            Component.CurrentFrame = Nothing
            Component.CurrentSegment = Nothing
        End If
        MyBase.Run(gameTime)
    End Sub
End Class
