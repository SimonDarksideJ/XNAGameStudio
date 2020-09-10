#Region "File Description"
'-----------------------------------------------------------------------------
' Button.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports Microsoft.VisualBasic
Imports GameStateManagement
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

''' <summary>
''' A special button that handles toggling between "On" and "Off"
''' </summary>
Friend Class BooleanButton
    Inherits Button
    Private [option] As String
    Private value As Boolean

    ''' <summary>
    ''' Creates a new BooleanButton.
    ''' </summary>
    ''' <param name="option">The string text to display for the option.</param>
    ''' <param name="value">The initial value of the button.</param>
    Public Sub New(ByVal [option] As String, ByVal value As Boolean)
        MyBase.New([option])
        Me.option = [option]
        Me.value = value

        GenerateText()
    End Sub

    Protected Overrides Sub OnTapped()
        ' When tapped we need to toggle the value and regenerate the text
        value = Not value
        GenerateText()

        MyBase.OnTapped()
    End Sub

    ''' <summary>
    ''' Helper that generates the actual Text value the base class uses for drawing.
    ''' </summary>
    Private Sub GenerateText()
        Text = String.Format("{0}: {1}", [option], If(value, "On", "Off"))
    End Sub
End Class

''' <summary>
''' Represents a touchable button.
''' </summary>
Friend Class Button
    ''' <summary>
    ''' The text displayed in the button.
    ''' </summary>
    Public Text As String = "Button"

    ''' <summary>
    ''' The position of the top-left corner of the button.
    ''' </summary>
    Public Position As Vector2 = Vector2.Zero

    ''' <summary>
    ''' The size of the button.
    ''' </summary>
    Public Size As New Vector2(250, 75)

    ''' <summary>
    ''' The thickness of the border drawn for the button.
    ''' </summary>
    Public BorderThickness As Integer = 4

    ''' <summary>
    ''' The color of the button border.
    ''' </summary>
    Public BorderColor As New Color(200, 200, 200)

    ''' <summary>
    ''' The color of the button background.
    ''' </summary>
    Public FillColor As Color = New Color(100, 100, 100) * 0.75F

    ''' <summary>
    ''' The color of the text.
    ''' </summary>
    Public TextColor As Color = Color.White

    ''' <summary>
    ''' The opacity of the button.
    ''' </summary>
    Public Alpha As Single = 0.0F

    ''' <summary>
    ''' Invoked when the button is tapped.
    ''' </summary>
    Public Event Tapped As EventHandler(Of EventArgs)

    ''' <summary>
    ''' Creates a new Button.
    ''' </summary>
    ''' <param name="text">The text to display in the button.</param>
    Public Sub New(ByVal text As String)
        Me.Text = text
    End Sub

    ''' <summary>
    ''' Invokes the Tapped event and allows subclasses to perform actions when tapped.
    ''' </summary>
    Protected Overridable Sub OnTapped()
        RaiseEvent Tapped(Me, EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Passes a tap location to the button for handling.
    ''' </summary>
    ''' <param name="tap">The location of the tap.</param>
    ''' <returns>True if the button was tapped, false otherwise.</returns>
    Public Function HandleTap(ByVal tap As Vector2) As Boolean
        If tap.X >= Position.X AndAlso tap.Y >= Position.Y AndAlso tap.X <= Position.X + Size.X AndAlso tap.Y <= Position.Y + Size.Y Then
            OnTapped()
            Return True
        End If

        Return False
    End Function

    ''' <summary>
    ''' Draws the button
    ''' </summary>
    ''' <param name="screen">The screen drawing the button</param>
    Public Sub Draw(ByVal screen As GameScreen)
        ' Grab some common items from the ScreenManager
        Dim spriteBatch As SpriteBatch = screen.ScreenManager.SpriteBatch
        Dim font As SpriteFont = screen.ScreenManager.Font
        Dim blank As Texture2D = screen.ScreenManager.BlankTexture

        ' Compute the button's rectangle
        Dim r As New Rectangle(CInt(Position.X), CInt(Position.Y), CInt(Size.X), CInt(Size.Y))

        ' Fill the button
        spriteBatch.Draw(blank, r, FillColor * Alpha)

        ' Draw the border
        spriteBatch.Draw(blank, New Rectangle(r.Left, r.Top, r.Width, BorderThickness), BorderColor * Alpha)
        spriteBatch.Draw(blank, New Rectangle(r.Left, r.Top, BorderThickness, r.Height), BorderColor * Alpha)
        spriteBatch.Draw(blank, New Rectangle(r.Right - BorderThickness, r.Top, BorderThickness, r.Height), BorderColor * Alpha)
        spriteBatch.Draw(blank, New Rectangle(r.Left, r.Bottom - BorderThickness, r.Width, BorderThickness), BorderColor * Alpha)

        ' Draw the text centered in the button
        Dim textSize As Vector2 = font.MeasureString(Text)
        Dim textPosition As Vector2 = New Vector2(r.Center.X, r.Center.Y) - textSize / 2.0F
        textPosition.X = CInt(textPosition.X)
        textPosition.Y = CInt(textPosition.Y)
        spriteBatch.DrawString(font, Text, textPosition, TextColor * Alpha)
    End Sub
End Class