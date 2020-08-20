#Region "File Description"
'-----------------------------------------------------------------------------
' Sprite.cs
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics

''' <summary>
''' Base class for the Paddle and Ball
''' </summary>
Public MustInherit Class Sprite
    ' The texture for the sprite
    Protected texture As Texture2D

    ' Our textures contain shadows, so this specifies the actual area that is collideable
    Protected collisionBounds As Rectangle

    Public Position As Vector2

    ''' <summary>
    ''' Gets the collision bounds in texture space for the sprite.
    ''' </summary>
    Public ReadOnly Property BaseCollisionBounds() As Rectangle
        Get
            Return collisionBounds
        End Get
    End Property

    ''' <summary>
    ''' Gets the collision bounds in world space for the sprite.
    ''' </summary>
    Public ReadOnly Property Bounds() As Rectangle
        Get
            ' Start with our source collision bounds
            'INSTANT VB NOTE: The local variable bounds was renamed since Visual Basic will not allow local variables with the same name as their enclosing function or property:
            Dim bounds_Renamed As Rectangle = collisionBounds

            ' Offset the bounds by the current position
            bounds_Renamed.X += CInt(Position.X)
            bounds_Renamed.Y += CInt(Position.Y)

            Return bounds_Renamed
        End Get
    End Property

    ''' <summary>
    ''' Positions the sprite centered on the given point.
    ''' </summary>
    ''' <param name="center">The location on which to center the sprite.</param>
    Public Sub CenterAtLocation(ByVal center As Vector2)
        Position = center - New Vector2(texture.Width \ 2, texture.Height \ 2)
    End Sub

    ''' <summary>
    ''' Allows the sprite to load content.
    ''' </summary>
    ''' <param name="content">The ContentManager used to load content.</param>
    Public Overridable Sub LoadContent(ByVal content As ContentManager)
    End Sub


    ''' <summary>
    ''' Draws the sprite.
    ''' </summary>
    Public Overridable Sub Draw(ByVal spriteBatch As SpriteBatch)
        spriteBatch.Draw(texture, Position, Color.White)
    End Sub
End Class
