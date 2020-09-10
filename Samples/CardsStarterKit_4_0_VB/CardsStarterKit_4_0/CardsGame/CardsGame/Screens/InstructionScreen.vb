#Region "File Description"
'-----------------------------------------------------------------------------
' InstructionScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports System.Threading
Imports Blackjack.GameStateManagement
#End Region


Friend Class InstructionScreen
    Inherits GameplayScreen
#Region "Fields"
    Private background As Texture2D
    Private font As SpriteFont
    Private gameplayScreen As GameplayScreen
    Private theme As String
    Private isExit As Boolean = False
    Private isExited As Boolean = False
#End Region

#Region "Initialization"
    Public Sub New(ByVal theme As String)
        MyBase.New("")
        TransitionOnTime = TimeSpan.FromSeconds(0.0)
        TransitionOffTime = TimeSpan.FromSeconds(0.5)

        Me.theme = theme
#If WINDOWS_PHONE Then
        EnabledGestures = GestureType.Tap
#End If
    End Sub
#End Region

#Region "Loading"
    ''' <summary>
    ''' Load the screen resources
    ''' </summary>
    Public Overrides Sub LoadContent()
        background = Load(Of Texture2D)("Images\instructions")
        font = Load(Of SpriteFont)("Fonts\MenuFont")

        ' Create a new instance of the gameplay screen
        gameplayScreen = New GameplayScreen(theme)
    End Sub
#End Region

#Region "Update and Render"
    ''' <summary>
    ''' Exit the screen after a tap or click
    ''' </summary>
    Private Overloads Sub HandleInput(ByVal mouseState As MouseState, ByVal padState As GamePadState)
        If Not isExit Then
#If WINDOWS_PHONE Then
            If ScreenManager.input.Gestures.Count > 0 AndAlso ScreenManager.input.Gestures(0).GestureType = GestureType.Tap Then
                isExit = True
            End If
#Else

				Dim result As PlayerIndex
				If mouseState.LeftButton = ButtonState.Pressed Then
					isExit = True
				ElseIf ScreenManager.input.IsNewButtonPress(Buttons.A, Nothing, result) OrElse ScreenManager.input.IsNewButtonPress(Buttons.Start, Nothing, result) Then
					isExit = True
				End If
#End If

        End If
    End Sub

    ''' <summary>
    ''' Screen update logic
    ''' </summary>
    ''' <param name="gameTime"></param>
    ''' <param name="otherScreenHasFocus"></param>
    ''' <param name="coveredByOtherScreen"></param>
    Public Overrides Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
        If isExit AndAlso (Not isExited) Then
            ' Move on to the gameplay screen
            For Each screen In ScreenManager.GetScreens
                screen.ExitScreen()
            Next screen

            gameplayScreen.ScreenManager = ScreenManager
            ScreenManager.AddScreen(gameplayScreen, Nothing)
            isExited = True
        End If

        HandleInput(Mouse.GetState, GamePad.GetState(PlayerIndex.One))

        MyBase.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen)
    End Sub

    ''' <summary>
    ''' Render screen 
    ''' </summary>
    ''' <param name="gameTime"></param>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        Dim spriteBatch As SpriteBatch = ScreenManager.SpriteBatch

        spriteBatch.Begin()

        ' Draw Background
        spriteBatch.Draw(background, ScreenManager.GraphicsDevice.Viewport.Bounds, Color.White * TransitionAlpha)

        If isExit Then
            Dim safeArea As Rectangle = ScreenManager.SafeArea
            Dim text As String = "Loading..."
            Dim measure As Vector2 = font.MeasureString(text)
            Dim textPosition As New Vector2(safeArea.Center.X - measure.X / 2, safeArea.Center.Y - measure.Y / 2)
            spriteBatch.DrawString(font, text, textPosition, Color.Black)
        End If

        spriteBatch.End()
        MyBase.Draw(gameTime)
    End Sub
#End Region
End Class
