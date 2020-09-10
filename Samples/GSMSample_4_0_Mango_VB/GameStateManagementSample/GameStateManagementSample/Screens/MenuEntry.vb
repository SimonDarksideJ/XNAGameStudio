#Region "File Description"
'-----------------------------------------------------------------------------
' MenuEntry.vb
'
' XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports GameStateManagement
#End Region

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

    ''' <summary>
    ''' The position at which the entry is drawn. This is set by the MenuScreen
    ''' each frame in Update.
    ''' </summary>
    Private position As Vector2

#End Region

#Region "Properties"


    ''' <summary>
    ''' Gets or sets the text of this menu entry.
    ''' </summary>
    Public Property Text() As String
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
    Public Property _Position() As Vector2

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
        ' there is no such thing as a selected item on Windows Phone, so we always
        ' force isSelected to be false
#If WINDOWS_PHONE Then
			isSelected = False
#End If

        ' Draw the selected entry in yellow, otherwise white.
        Dim color As Color = If(isSelected, color.Yellow, color.White)

        ' Pulsate the size of the selected menu entry.
        Dim time As Double = gameTime.TotalGameTime.TotalSeconds

        Dim pulsate As Single = CSng(Math.Sin(time * 6)) + 1

        Dim scale As Single = 1 + pulsate * 0.05F * selectionFade

        ' Modify the alpha to fade text out during transitions.
        color *= screen.TransitionAlpha

        ' Draw text, centered on the middle of each line.
        Dim screenManager As ScreenManager = screen.ScreenManager
        Dim spriteBatch As SpriteBatch = screenManager.SpriteBatch
        Dim font As SpriteFont = screenManager.Font

        Dim origin As New Vector2(0, CSng(font.LineSpacing) / 2)

        spriteBatch.DrawString(font, _text, _Position, color, 0, origin, scale, SpriteEffects.None, 0)
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
        Return CInt(screen.ScreenManager.Font.MeasureString(Text).X)
    End Function


#End Region
End Class