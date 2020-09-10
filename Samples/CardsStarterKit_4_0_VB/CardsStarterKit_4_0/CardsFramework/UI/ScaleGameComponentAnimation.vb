#Region "File Description"
'-----------------------------------------------------------------------------
' ScaleGameComponentAnimation.vb
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


''' <summary>
''' An animation which scales a component.
''' </summary>
Public Class ScaleGameComponentAnimation
    Inherits AnimatedGameComponentAnimation
#Region "Fields"
    Private percent As Single = 0
    Private beginFactor As Single
    Private factorDelta As Single
#End Region

#Region "Initialzations"
    ''' <summary>
    ''' Initializes a new instance of the class.
    ''' </summary>
    ''' <param name="beginFactor">The initial scale factor.</param>
    ''' <param name="endFactor">The eventual scale factor.</param>
    Public Sub New(ByVal beginFactor As Single, ByVal endFactor As Single)
        Me.beginFactor = beginFactor
        factorDelta = endFactor - beginFactor
    End Sub
#End Region

    ''' <summary>
    ''' Runs the scaling animation.
    ''' </summary>
    ''' <param name="gameTime">Game time information.</param>
    Public Overrides Sub Run(ByVal gameTime As GameTime)
        Dim texture As Texture2D
        If IsStarted() Then
            texture = Component.CurrentFrame
            If texture IsNot Nothing Then
                ' Calculate the completion percent of animation
                percent += CSng(gameTime.ElapsedGameTime.TotalSeconds / Duration.TotalSeconds)

                ' Inflate the component with an increasing delta. The eventual
                ' delta will have the componenet scale to the specified target
                ' scaling factor.
                Dim bounds As Rectangle = texture.Bounds
                bounds.X = CInt(Fix(Component.CurrentPosition.X))
                bounds.Y = CInt(Fix(Component.CurrentPosition.Y))
                Dim currentFactor As Single = beginFactor + factorDelta * percent - 1
                bounds.Inflate(CInt(Fix(bounds.Width * currentFactor)), CInt(Fix(bounds.Height * currentFactor)))
                Component.CurrentDestination = bounds
            End If
        End If
    End Sub
End Class
