#Region "File Description"
'-----------------------------------------------------------------------------
' MenuEntry.vb
'
' XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Namespace GameStateManagement
	''' <summary>
	''' Helper class represents a single entry in a MenuScreen. By default this
	''' just draws the entry text string, but it can be customized to display menu
	''' entries in different ways. This also provides an event that will be raised
	''' when the menu entry is selected.
	''' </summary>
	Friend Class MenuEntry
		#Region "Fields"

		''' <summary>
		''' The text rendered for this entry.
		''' </summary>
        Private _text As String

        ''' <summary>
        ''' Tracks a fading selection effect on the entry.
        ''' </summary>
        ''' <remarks>
        ''' The entries transition out of the selection effect when they are deselected.
        ''' </remarks>
        Private selectionFade As Single

        Private _destination As Rectangle
#End Region

#Region "Properties"
        ''' <summary>
        ''' Gets or sets the text of this menu entry.
        ''' </summary>
        Public Property Text As String
            Get
                Return _text
            End Get
            Set(ByVal value As String)
                _text = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the position at which to draw this menu entry.
        ''' </summary>
        Public Property Destination As Rectangle
            Get
                Return _destination
            End Get
            Set(ByVal value As Rectangle)
                _destination = value
            End Set
        End Property

        Public Property Scale As Single

        Public Property Rotation As Single
#End Region

#Region "Events"
        ''' <summary>
        ''' Event raised when the menu entry is selected.
        ''' </summary>
        Public Event Selected As EventHandler(Of PlayerIndexEventArgs)


        ''' <summary>
        ''' Method for raising the Selected event.
        ''' </summary>
        Protected Friend Overridable Sub OnSelectEntry(ByVal playerIndex As PlayerIndex)
            RaiseEvent Selected(Me, New PlayerIndexEventArgs(playerIndex))
        End Sub
#End Region

#Region "Initialization"
        ''' <summary>
        ''' Constructs a new menu entry with the specified text.
        ''' </summary>
        Public Sub New(ByVal text As String)
            Me._text = text

            Scale = 1.0F
            Rotation = 0.0F
        End Sub
#End Region

#Region "Update and Draw"
        ''' <summary>
        ''' Updates the menu entry.
        ''' </summary>
        Public Overridable Sub Update(ByVal screen As MenuScreen, ByVal isSelected As Boolean, ByVal gameTime As GameTime)
            ' there is no such thing as a selected item on Windows Phone, so we always
            ' force isSelected to be false
#If WINDOWS_PHONE Then
            isSelected = False
#End If

            ' When the menu selection changes, entries gradually fade between
            ' their selected and deselected appearance, rather than instantly
            ' popping to the new state.
            Dim fadeSpeed As Single = CSng(gameTime.ElapsedGameTime.TotalSeconds) * 4

            If isSelected Then
                selectionFade = Math.Min(selectionFade + fadeSpeed, 1)
            Else
                selectionFade = Math.Max(selectionFade - fadeSpeed, 0)
            End If
        End Sub


        ''' <summary>
        ''' Draws the menu entry. This can be overridden to customize the appearance.
        ''' </summary>
        Public Overridable Sub Draw(ByVal screen As MenuScreen, ByVal isSelected As Boolean, ByVal gameTime As GameTime)
            Dim textColor As Color = If(isSelected, Color.White, Color.Black)
            Dim tintColor As Color = If(isSelected, Color.White, Color.Gray)

#If WINDOWS_PHONE Then
            ' there is no such thing as a selected item on Windows Phone, so we always
            ' force isSelected to be false

            isSelected = False
            tintColor = Color.White
            textColor = Color.Black
#End If

            ' Draw text, centered on the middle of each line.
            Dim screenManager As ScreenManager = screen.ScreenManager
            Dim spriteBatch As SpriteBatch = screenManager.SpriteBatch
            Dim font As SpriteFont = screenManager.Font

            spriteBatch.Draw(screenManager.ButtonBackground, _destination, tintColor)

            spriteBatch.DrawString(screenManager.Font, _text, getTextPosition(screen), textColor, Rotation, Vector2.Zero, Scale, SpriteEffects.None, 0)
        End Sub


        ''' <summary>
        ''' Queries how much space this menu entry requires.
        ''' </summary>
        Public Overridable Function GetHeight(ByVal screen As MenuScreen) As Integer
            Return screen.ScreenManager.Font.LineSpacing
        End Function

        ''' <summary>
        ''' Queries how wide the entry is, used for centering on the screen.
        ''' </summary>
        Public Overridable Function GetWidth(ByVal screen As MenuScreen) As Integer
            Return CInt(Fix(screen.ScreenManager.Font.MeasureString(Text).X))
        End Function

        Private Function getTextPosition(ByVal screen As MenuScreen) As Vector2
            Dim textPosition As Vector2 = Vector2.Zero
            If Scale = 1.0F Then
                textPosition = New Vector2(CInt(Fix(_destination.X)) + _destination.Width \ 2 - GetWidth(screen) \ 2, CInt(Fix(_destination.Y)))
            Else
                textPosition = New Vector2(CInt(Fix(_destination.X)) + (_destination.Width \ 2 - ((GetWidth(screen) \ 2) * Scale)), CInt(Fix(_destination.Y)) + (GetHeight(screen) - GetHeight(screen) * Scale) / 2)
            End If

            Return textPosition
        End Function
#End Region
	End Class
End Namespace
