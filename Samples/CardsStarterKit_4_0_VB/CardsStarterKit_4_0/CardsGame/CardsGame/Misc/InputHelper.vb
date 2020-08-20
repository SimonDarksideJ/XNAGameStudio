#Region "File Description"
'-----------------------------------------------------------------------------
' InputHelper.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

''' <summary>
''' Used to simulate a cursor on the Xbox.
''' </summary>
Public Class InputHelper
    Inherits DrawableGameComponent
#Region "Fields"
    'Public Shared Game As Game
    'Private Shared instance As InputHelper

    Public IsEscape As Boolean
    Public IsPressed As Boolean

    Private drawPosition As Vector2
    Private texture As Texture2D
    Private spriteBatch As SpriteBatch
    Private maxVelocity As Single
#End Region

#Region "Initialization"

    Public Sub New(ByVal game As Game)
        MyBase.New(game)
        texture = game.Content.Load(Of Texture2D)("Images\GamePadCursor")
        spriteBatch = New SpriteBatch(game.GraphicsDevice)
        maxVelocity = CSng(game.GraphicsDevice.Viewport.Width + game.GraphicsDevice.Viewport.Height) / 3000.0F

        drawPosition = New Vector2(game.GraphicsDevice.Viewport.Width \ 2, game.GraphicsDevice.Viewport.Height \ 2)
    End Sub

#End Region

#Region "Properties"

    'Public Shared ReadOnly Property Instance As InputHelper
    '       Get
    '           If instance Is Nothing Then
    '               Instance = New InputHelper
    '           End If
    '           Return instance
    '       End Get
    '   End Property

    Public ReadOnly Property PointPosition As Vector2
        Get
            Return drawPosition + New Vector2(texture.Width / 2.0F, texture.Height / 2.0F)
        End Get
    End Property

#End Region

#Region "Update and Render"

    ''' <summary>
    ''' Updates itself.
    ''' </summary>
    ''' <param name="gameTime">Provides a snapshot of timing values.</param>
    Public Overrides Sub Update(ByVal gameTime As GameTime)
        Dim gamePadState As GamePadState = GamePad.GetState(PlayerIndex.One)

        IsPressed = gamePadState.Buttons.A = ButtonState.Pressed

        IsEscape = gamePadState.Buttons.Back = ButtonState.Pressed

        drawPosition += gamePadState.ThumbSticks.Left * New Vector2(1, -1) * gameTime.ElapsedGameTime.Milliseconds * maxVelocity
        drawPosition = Vector2.Clamp(drawPosition, Vector2.Zero, New Vector2(Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height) - New Vector2(texture.Bounds.Width, texture.Bounds.Height))
    End Sub

    ''' <summary>
    ''' Draws cursor.
    ''' </summary>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        spriteBatch.Begin()
        spriteBatch.Draw(texture, drawPosition, Nothing, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0)
        spriteBatch.End()
    End Sub

#End Region
End Class
