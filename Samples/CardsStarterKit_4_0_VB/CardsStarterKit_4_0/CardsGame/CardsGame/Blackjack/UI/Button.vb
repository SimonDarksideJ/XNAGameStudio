#Region "File Description"
'-----------------------------------------------------------------------------
' Button.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Blackjack.GameStateManagement
Imports CardsFramework
#End Region


Public Class Button
    Inherits AnimatedGameComponent
#Region "Fields and Properties"
    Private isKeyDown As Boolean = False
    Private isPressed As Boolean = False
    Private spriteBatch As SpriteBatch

    Public Property RegularTexture As Texture2D
    Public Property PressedTexture As Texture2D
    Public Property Font As SpriteFont
    Public Property Bounds As Rectangle

    Private _regularTextureField As String
    Private _pressedTextureField As String

    Public Event Click As EventHandler
    Private input As InputState

    Private inputHelper As InputHelper

#End Region

#Region "Initiaizations"
    ''' <summary>
    ''' Creates a new instance of the <see cref="Button"/> class.
    ''' </summary>
    ''' <param name="regularTexture">The name of the button's texture.</param>
    ''' <param name="pressedTexture">The name of the texture to display when the 
    ''' button is pressed.</param>
    ''' <param name="input">A <see cref="GameStateManagement.InputState"/> object
    ''' which can be used to retrieve user input.</param>
    ''' <param name="cardGame">The associated card game.</param>
    ''' <remarks>Texture names are relative to the "Images" content 
    ''' folder.</remarks>
    Public Sub New(ByVal regularTexture As String, ByVal pressedTexture As String, ByVal input As InputState, ByVal cardGame As CardsGame)
        MyBase.New(cardGame, Nothing)
        Me.input = input
        Me._regularTextureField = regularTexture
        Me._pressedTextureField = pressedTexture
    End Sub

    ''' <summary>
    ''' Initializes the button.
    ''' </summary>
    Public Overrides Sub Initialize()
#If WINDOWS_PHONE Then
        ' Enable tab gesture
        TouchPanel.EnabledGestures = GestureType.Tap
#End If
        ' Get Xbox curser
        inputHelper = Nothing
        For componentIndex = 0 To Game.Components.Count - 1
            If TypeOf Game.Components(componentIndex) Is InputHelper Then
                inputHelper = CType(Game.Components(componentIndex), InputHelper)
                Exit For
            End If
        Next componentIndex

        spriteBatch = New SpriteBatch(Game.GraphicsDevice)

        MyBase.Initialize()
    End Sub
#End Region

#Region "Loading"
    ''' <summary>
    ''' Loads the content required bt the button.
    ''' </summary>
    Protected Overrides Sub LoadContent()
        If _regularTextureField IsNot Nothing Then
            RegularTexture = Game.Content.Load(Of Texture2D)("Images\" & _regularTextureField)
        End If
        If _pressedTextureField IsNot Nothing Then
            PressedTexture = Game.Content.Load(Of Texture2D)("Images\" & _pressedTextureField)
        End If

        MyBase.LoadContent()
    End Sub
#End Region

#Region "Update and Render"
    ''' <summary>
    ''' Performs update logic for the button.
    ''' </summary>
    ''' <param name="gameTime">The time that has passed since the last call to 
    ''' this method.</param>
    Public Overrides Sub Update(ByVal gameTime As GameTime)
        If RegularTexture IsNot Nothing Then
            HandleInput(Mouse.GetState)
        End If

        MyBase.Update(gameTime)
    End Sub

    ''' <summary>
    ''' Handle the input of adding chip on all platform
    ''' </summary>
    ''' <param name="mouseState">Mouse input information.</param>
    Private Sub HandleInput(ByVal mouseState As MouseState)
        Dim pressed As Boolean = False
        Dim position As Vector2 = Vector2.Zero

#If WINDOWS_PHONE Then
        If (input.Gestures.Count > 0) AndAlso input.Gestures(0).GestureType = GestureType.Tap Then
            pressed = True
            position = input.Gestures(0).Position
        End If
#Else
			If mouseState.LeftButton = ButtonState.Pressed Then
				pressed = True
				position = New Vector2(mouseState.X, mouseState.Y)
			ElseIf inputHelper.IsPressed Then
				pressed = True
				position = inputHelper.PointPosition
			Else
				If isPressed Then
					If IntersectWith(New Vector2(mouseState.X, mouseState.Y)) OrElse IntersectWith(inputHelper.PointPosition) Then
						FireClick()
						isPressed = False
					Else

						isPressed = False
					End If
				End If

				isKeyDown = False
			End If
#End If

        If pressed Then
            If Not isKeyDown Then
                If IntersectWith(position) Then
                    isPressed = True
#If WINDOWS_PHONE Then
                    FireClick()
                    isPressed = False
#End If
                End If
                isKeyDown = True
            End If
        Else
            isKeyDown = False
        End If
    End Sub

    ''' <summary>
    ''' Checks if the button intersects with a specified position
    ''' </summary>
    ''' <param name="position">The position to check intersection against.</param>
    ''' <returns>True if the position intersects with the button, 
    ''' false otherwise.</returns>
    Private Function IntersectWith(ByVal position As Vector2) As Boolean
        Dim touchTap As New Rectangle(CInt(Fix(position.X)) - 1, CInt(Fix(position.Y)) - 1, 2, 2)
        Return Bounds.Intersects(touchTap)
    End Function

    ''' <summary>
    ''' Fires the button's click event.
    ''' </summary>
    Public Sub FireClick()
        RaiseEvent Click(Me, EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Draws the button.
    ''' </summary>
    ''' <param name="gameTime">The time that has passed since the last call to 
    ''' this method.</param>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        spriteBatch.Begin()


        spriteBatch.Draw(If(isPressed, PressedTexture, RegularTexture), Bounds, Color.White)
        If Font IsNot Nothing Then
            Dim textPosition As Vector2 = Font.MeasureString(Text)
            textPosition = New Vector2(Bounds.Width - textPosition.X, Bounds.Height - textPosition.Y)
            textPosition /= 2
            textPosition.X += Bounds.X
            textPosition.Y += Bounds.Y
            spriteBatch.DrawString(Font, Text, textPosition, Color.White)
        End If

        spriteBatch.End()

        MyBase.Draw(gameTime)
    End Sub
#End Region

    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        ClickEvent = Nothing
        MyBase.Dispose(disposing)
    End Sub
End Class
