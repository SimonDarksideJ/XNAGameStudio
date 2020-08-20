#Region "File Description"
'-----------------------------------------------------------------------------
' Ball.cs
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics

''' <summary>
''' The ball bouncing in our game.
''' </summary>
Public Class Ball
    Inherits Sprite
    ''' <summary>
    ''' The velocity of the ball.
    ''' </summary>
    Public Velocity As Vector2

    ''' <summary>
    ''' Allows the ball to load content.
    ''' </summary>
    Public Overrides Sub LoadContent(ByVal content As ContentManager)
        ' Load the ball texture
        Texture = content.Load(Of Texture2D)("ball")

        ' Set the collision bounds for accurate collision detection to account
        ' for the shadows and empty space in the texture
        collisionBounds = New Rectangle(4, 4, 28, 28)
    End Sub

    ''' <summary>
    ''' Checks if the ball is off the screen.
    ''' </summary>
    ''' <returns>
    ''' 1 if the ball went off the right side of the screen, 
    ''' -1 if the ball went off the left side of the screen,
    ''' 0 if the ball is still on the screen.</returns>
    Public Function IsOffscreen() As Integer
        If Position.X >= 800 Then
            Return 1
        End If
        If Position.X <= -Texture.Width Then
            Return -1
        End If
        Return 0
    End Function

    ''' <summary>
    ''' Updates the ball.
    ''' </summary>
    Public Sub Update(ByVal elapsedTime As TimeSpan)
        Position += Velocity * CSng(elapsedTime.TotalSeconds)
    End Sub
End Class
