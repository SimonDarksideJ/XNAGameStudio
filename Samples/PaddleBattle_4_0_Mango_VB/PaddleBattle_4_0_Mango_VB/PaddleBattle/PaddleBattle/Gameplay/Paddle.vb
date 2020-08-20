#Region "File Description"
'-----------------------------------------------------------------------------
' Paddle.cs
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics

''' <summary>
''' The paddles, either controlled by the player or computer.
''' </summary>
Public Class Paddle
    Inherits Sprite
    ' Is this paddle controlled by the computer?
    Private isAiPaddle As Boolean

    ''' <summary>
    ''' Initializes a new Paddle.
    ''' </summary>
    ''' <param name="isAiPaddle">Is this the computer paddle?</param>
    Public Sub New(ByVal isAiPaddle As Boolean)
        Me.isAiPaddle = isAiPaddle
    End Sub

    ''' <summary>
    ''' Allows the paddle to load content.
    ''' </summary>
    Public Overrides Sub LoadContent(ByVal content As ContentManager)
        ' Determine the texture to load based on whether the paddle is the computer or not
        Dim asset As String = If(isAiPaddle, "paddle_red", "paddle_blue")
        Texture = content.Load(Of Texture2D)(asset)

        ' Set the collision bounds for accurate collision detection to account
        ' for the shadows and empty space in the texture
        collisionBounds = New Rectangle(6, 1, 31, 109)
    End Sub

    ''' <summary>
    ''' Tests for and reacts to a collision with the ball.
    ''' </summary>
    ''' <param name="ball">The ball to collide with the paddle</param>
    Public Function Collide(ByVal ball As Ball) As Boolean
        ' Test if the bounds of the ball and paddle are intersecting
        If Bounds.Intersects(ball.Bounds) Then
            ' Get the magnitude of the ball's velocity and in which direction on the X axis the ball is moving
            Dim ballVelocityMagnitude As Single = ball.Velocity.Length()
            Dim ballXDirection As Integer = Math.Sign(ball.Velocity.X)

            ' Compute a value in the range [-1, 1] indicating where on the paddle the collison
            ' happened, where -1 is the top and 1 is the bottom
            Dim paddleCenter As Single = Bounds.Center.Y
            Dim ballCenter As Single = ball.Bounds.Center.Y
            Dim offset As Single = (ballCenter - paddleCenter) / (Bounds.Height \ 2)

            ' Now we generate the ball's resulting angle based on the square of the offset to
            ' simulate a curved surface rather than a flat surface, thus giving the ball a different
            ' bounce based on where it hits the paddle
            Dim angle As Single = (offset * offset) * MathHelper.ToRadians(60) * Math.Sign(offset)

            ' Use this angle to generate the new direction of the ball
            ball.Velocity = New Vector2(CSng(Math.Cos(angle)) * ballXDirection, CSng(Math.Sin(angle)))

            ' Test to see if the ball hasn't passed the center of the paddle. If it hasn't, we need to flip
            ' the X direction of the velocity so it bounces back towards the other paddle.
            If (ballXDirection > 0 AndAlso ball.Bounds.Center.X < Bounds.Center.X) OrElse (ballXDirection < 0 AndAlso ball.Bounds.Center.X > Bounds.Center.X) Then
                ball.Velocity.X *= -1
            End If

            ' Scale the velocity back to the original magnitude
            ball.Velocity *= ballVelocityMagnitude

            ' Resolve the collision correctly for the paddle based on the ball's new direction
            If ball.Velocity.X < 0 Then
                ball.Position.X = Bounds.Left - ball.BaseCollisionBounds.Width
            Else
                ball.Position.X = Bounds.Right - ball.BaseCollisionBounds.X
            End If

            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Allows the AI to track the given ball
    ''' </summary>
    Public Sub UpdateAI(ByVal elapsedTime As TimeSpan, ByVal ball As Ball)
        ' We want to try and put the center of the paddle at the same place as the center of the ball
        Dim targetY As Single = ball.Position.Y + ball.Bounds.Height \ 2 - BaseCollisionBounds.Height \ 2

        ' Instead of just putting the paddle there, we want to make it slide into place
        Const speed As Single = 120
        Dim delta As Single = targetY - Position.Y

        ' If we're within one frame's speed of reaching the destination, simply clamp to that location
        If Math.Abs(delta) < speed * CSng(elapsedTime.TotalSeconds) Then
            Position.Y = targetY

            ' Otherwise compute the direction and move in the direction towards the ball
        Else
            Dim direction As Integer = Math.Sign(delta)
            Position.Y += direction * speed * CSng(elapsedTime.TotalSeconds)
        End If

        ' Ensure the paddle remains on screen
        ClampToScreen()
    End Sub

    ''' <summary>
    ''' Clamps the paddle to the screen.
    ''' </summary>
    Public Sub ClampToScreen()
        If Bounds.Top < 0 Then
            Position.Y = BaseCollisionBounds.Y
        ElseIf Bounds.Bottom > 480 Then
            Position.Y = 480 - BaseCollisionBounds.Height
        End If
    End Sub
End Class