#Region "File Description"
'-----------------------------------------------------------------------------
' MessageBoxScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input
Imports GameStateManagement
#End Region

''' <summary>
''' A popup message box screen, used to display "are you sure?"
''' confirmation messages.
''' </summary>
Friend Class MessageBoxScreen
    Inherits GameScreen
#Region "Fields"

    Private message As String
    Private gradientTexture As Texture2D

    Private menuSelect As InputAction
    Private menuCancel As InputAction

#End Region

#Region "Events"

    Public Event Accepted As EventHandler(Of PlayerIndexEventArgs)
    Public Event Cancelled As EventHandler(Of PlayerIndexEventArgs)

#End Region

#Region "Initialization"


    ''' <summary>
    ''' Constructor automatically includes the standard "A=ok, B=cancel"
    ''' usage text prompt.
    ''' </summary>
    Public Sub New(ByVal message As String)
        Me.New(message, True)
    End Sub


    ''' <summary>
    ''' Constructor lets the caller specify whether to include the standard
    ''' "A=ok, B=cancel" usage text prompt.
    ''' </summary>
    Public Sub New(ByVal message As String, ByVal includeUsageText As Boolean)
        Const usageText As String = vbLf & "A button, Space, Enter = ok" & vbLf & "B button, Esc = cancel"

        If includeUsageText Then
            Me.message = message & usageText
        Else
            Me.message = message
        End If

        IsPopup = True

        TransitionOnTime = TimeSpan.FromSeconds(0.2)
        TransitionOffTime = TimeSpan.FromSeconds(0.2)

        menuSelect = New InputAction(New Buttons() {Buttons.A, Buttons.Start}, New Keys() {Keys.Space, Keys.Enter}, True)
        menuCancel = New InputAction(New Buttons() {Buttons.B, Buttons.Back}, New Keys() {Keys.Escape, Keys.Back}, True)
    End Sub


    ''' <summary>
    ''' Loads graphics content for this screen. This uses the shared ContentManager
    ''' provided by the Game class, so the content will remain loaded forever.
    ''' Whenever a subsequent MessageBoxScreen tries to load this same content,
    ''' it will just get back another reference to the already loaded data.
    ''' </summary>
    Public Overrides Sub Activate(ByVal instancePreserved As Boolean)
        If Not instancePreserved Then
            Dim content As ContentManager = ScreenManager.Game.Content
            gradientTexture = content.Load(Of Texture2D)("gradient")
        End If
    End Sub


#End Region

#Region "Handle Input"


    ''' <summary>
    ''' Responds to user input, accepting or cancelling the message box.
    ''' </summary>
    Public Overrides Sub HandleInput(ByVal gameTime As GameTime, ByVal input As InputState)
        Dim playerIndex As PlayerIndex

        ' We pass in our ControllingPlayer, which may either be null (to
        ' accept input from any player) or a specific index. If we pass a null
        ' controlling player, the InputState helper returns to us which player
        ' actually provided the input. We pass that through to our Accepted and
        ' Cancelled events, so they can tell which player triggered them.
        If menuSelect.Evaluate(input, ControllingPlayer, playerIndex) Then
            ' Raise the accepted event, then exit the message box.
            RaiseEvent Accepted(Me, New PlayerIndexEventArgs(playerIndex))

            ExitScreen()
        ElseIf menuCancel.Evaluate(input, ControllingPlayer, playerIndex) Then
            ' Raise the cancelled event, then exit the message box.
            RaiseEvent Cancelled(Me, New PlayerIndexEventArgs(playerIndex))

            ExitScreen()
        End If
    End Sub


#End Region

#Region "Draw"


    ''' <summary>
    ''' Draws the message box.
    ''' </summary>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        Dim spriteBatch As SpriteBatch = ScreenManager.SpriteBatch
        Dim font As SpriteFont = ScreenManager.Font

        ' Darken down any other screens that were drawn beneath the popup.
        ScreenManager.FadeBackBufferToBlack(CSng(TransitionAlpha * 2) / 3)

        ' Center the message text in the viewport.
        Dim viewport As Viewport = ScreenManager.GraphicsDevice.Viewport
        Dim viewportSize As New Vector2(viewport.Width, viewport.Height)
        Dim textSize As Vector2 = font.MeasureString(message)
        Dim textPosition As Vector2 = (viewportSize - textSize) / 2

        ' The background includes a border somewhat larger than the text itself.
        Const hPad As Integer = 32
        Const vPad As Integer = 16

        Dim backgroundRectangle As New Rectangle(CInt(textPosition.X) - hPad, CInt(textPosition.Y) - vPad, CInt(textSize.X) + hPad * 2, CInt(textSize.Y) + vPad * 2)

        ' Fade the popup alpha during transitions.
        Dim color As Color = color.White * TransitionAlpha

        spriteBatch.Begin()

        ' Draw the background rectangle.
        spriteBatch.Draw(gradientTexture, backgroundRectangle, color)

        ' Draw the message box text.
        spriteBatch.DrawString(font, message, textPosition, color)

        spriteBatch.End()
    End Sub


#End Region
End Class