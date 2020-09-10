#Region "File Description"
'-----------------------------------------------------------------------------
' FlipGameComponentAnimation.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
#End Region


Public Class FlipGameComponentAnimation
    Inherits AnimatedGameComponentAnimation
#Region "Fields"
    Protected percent As Integer = 0
    Public IsFromFaceDownToFaceUp As Boolean = True
#End Region

    ''' <summary>
    ''' Runs the flip animation, which makes the component appear as if it has
    ''' been flipped.
    ''' </summary>
    ''' <param name="gameTime">Game time information.</param>
    Public Overrides Sub Run(ByVal gameTime As GameTime)
        Dim texture As Texture2D
        If IsStarted() Then
            If IsDone() Then
                ' Finish tha animation
                Component.IsFaceDown = Not IsFromFaceDownToFaceUp
                Component.CurrentDestination = Nothing
            Else
                texture = Component.CurrentFrame
                If texture IsNot Nothing Then
                    ' Calculate the completion percent of the animation
                    percent += CInt(Fix(((gameTime.ElapsedGameTime.TotalMilliseconds / (Duration.TotalMilliseconds / AnimationCycles)) * 100)))

                    If percent >= 100 Then
                        percent = 0
                    End If

                    Dim currentPercent As Integer
                    If percent < 50 Then
                        ' On the first half of the animation the component is
                        ' on its initial size
                        currentPercent = percent
                        Component.IsFaceDown = IsFromFaceDownToFaceUp
                    Else
                        ' On the second half of the animation the component
                        ' is flipped
                        currentPercent = 100 - percent
                        Component.IsFaceDown = Not IsFromFaceDownToFaceUp
                    End If
                    ' Shrink and widen the component to look like it is flipping
                    Component.CurrentDestination = New Rectangle(CInt(Fix(Component.CurrentPosition.X + texture.Width * currentPercent \ 100)), CInt(Fix(Component.CurrentPosition.Y)), CInt(Fix(texture.Width - (texture.Width * currentPercent \ 100) * 2)), texture.Height)
                End If
            End If
        End If
    End Sub
End Class