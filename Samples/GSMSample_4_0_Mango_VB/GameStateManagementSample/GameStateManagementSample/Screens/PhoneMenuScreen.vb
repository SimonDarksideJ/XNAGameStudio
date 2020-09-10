#Region "File Description"
'-----------------------------------------------------------------------------
' PhoneMenuScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Collections.Generic
Imports GameStateManagement
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input
Imports Microsoft.Xna.Framework.Input.Touch

''' <summary>
''' Provides a basic base screen for menus on Windows Phone leveraging the Button class.
''' </summary>
Friend Class PhoneMenuScreen
    Inherits GameScreen
    Private _menuButtons As New List(Of Button)()
    Private menuTitle As String

    Private menuCancel As InputAction
    ''' <summary>
    ''' Gets the list of buttons, so derived classes can add or change the menu contents.
    ''' </summary>
    Protected ReadOnly Property MenuButtons() As IList(Of Button)
        Get
            Return _menuButtons
        End Get
    End Property

    ''' <summary>
    ''' Creates the PhoneMenuScreen with a particular title.
    ''' </summary>
    ''' <param name="title">The title of the screen</param>
    Public Sub New(ByVal title As String)
        menuTitle = title

        TransitionOnTime = TimeSpan.FromSeconds(0.5)
        TransitionOffTime = TimeSpan.FromSeconds(0.5)

        ' Create the menuCancel action
        menuCancel = New InputAction(New Buttons() {Buttons.Back}, Nothing, True)

        ' We need tap gestures to hit the buttons
        EnabledGestures = GestureType.Tap
    End Sub

    Public Overrides Sub Activate(ByVal instancePreserved As Boolean)
        ' When the screen is activated, we have a valid ScreenManager so we can arrange
        ' our buttons on the screen
        Dim y As Single = 140.0F
        Dim center As Single = ScreenManager.GraphicsDevice.Viewport.Bounds.Center.X
        For i As Integer = 0 To MenuButtons.Count - 1
            Dim b As Button = MenuButtons(i)

            b.Position = New Vector2(center - b.Size.X / 2, y)
            y += b.Size.Y * 1.5F
        Next i

        MyBase.Activate(instancePreserved)
    End Sub

    Public Overrides Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
        ' Update opacity of the buttons
        For Each b As Button In _menuButtons
            b.Alpha = TransitionAlpha
        Next b

        MyBase.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen)
    End Sub

    ''' <summary>
    ''' An overrideable method called whenever the menuCancel action is triggered
    ''' </summary>
    Protected Overridable Sub OnCancel()
    End Sub

    Public Overrides Sub HandleInput(ByVal gameTime As GameTime, ByVal input As InputState)
        ' Test for the menuCancel action
        Dim player As PlayerIndex
        If menuCancel.Evaluate(input, ControllingPlayer, player) Then
            OnCancel()
        End If

        ' Read in our gestures
        For Each gesture As GestureSample In input.Gestures
            ' If we have a tap
            If gesture.GestureType = GestureType.Tap Then
                ' Test the tap against the buttons until one of the buttons handles the tap
                For Each b As Button In _menuButtons
                    If b.HandleTap(gesture.Position) Then
                        Exit For
                    End If
                Next b
            End If
        Next gesture

        MyBase.HandleInput(gameTime, input)
    End Sub

    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        Dim graphics As GraphicsDevice = ScreenManager.GraphicsDevice
        Dim spriteBatch As SpriteBatch = ScreenManager.SpriteBatch
        Dim font As SpriteFont = ScreenManager.Font

        spriteBatch.Begin()

        ' Draw all of the buttons
        For Each b As Button In _menuButtons
            b.Draw(Me)
        Next b

        ' Make the menu slide into place during transitions, using a
        ' power curve to make things look more interesting (this makes
        ' the movement slow down as it nears the end).
        Dim transitionOffset As Single = CSng(Math.Pow(TransitionPosition, 2))

        ' Draw the menu title centered on the screen
        Dim titlePosition As New Vector2(graphics.Viewport.Width \ 2, 80)
        Dim titleOrigin As Vector2 = font.MeasureString(menuTitle) / 2
        Dim titleColor As Color = New Color(192, 192, 192) * TransitionAlpha
        Dim titleScale As Single = 1.25F

        titlePosition.Y -= transitionOffset * 100

        spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0, titleOrigin, titleScale, SpriteEffects.None, 0)

        spriteBatch.End()

        MyBase.Draw(gameTime)
    End Sub
End Class