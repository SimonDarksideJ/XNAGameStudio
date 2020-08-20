#Region "File Description"
'-----------------------------------------------------------------------------
' MenuScreen.vb
'
' XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Collections.Generic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input.Touch
Imports Microsoft.Xna.Framework.Input
Imports GameStateManagement
#End Region

''' <summary>
''' Base class for screens that contain a menu of options. The user can
''' move up and down to select an entry, or cancel to back out of the screen.
''' </summary>
Friend MustInherit Class MenuScreen
    Inherits GameScreen
#Region "Fields"

    Private _menuEntries As New List(Of MenuEntry)()
    Private selectedEntry As Integer = 0
    Private menuTitle As String

    Private menuUp As InputAction
    Private menuDown As InputAction
    Private menuSelect As InputAction
    Private menuCancel As InputAction

#End Region

#Region "Properties"


    ''' <summary>
    ''' Gets the list of menu entries, so derived classes can add
    ''' or change the menu contents.
    ''' </summary>
    Protected ReadOnly Property MenuEntries() As IList(Of MenuEntry)
        Get
            Return _menuEntries
        End Get
    End Property


#End Region

#Region "Initialization"


    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New(ByVal menuTitle As String)
        Me.menuTitle = menuTitle

        TransitionOnTime = TimeSpan.FromSeconds(0.5)
        TransitionOffTime = TimeSpan.FromSeconds(0.5)

        menuUp = New InputAction(New Buttons() {Buttons.DPadUp, Buttons.LeftThumbstickUp}, New Keys() {Keys.Up}, True)
        menuDown = New InputAction(New Buttons() {Buttons.DPadDown, Buttons.LeftThumbstickDown}, New Keys() {Keys.Down}, True)
        menuSelect = New InputAction(New Buttons() {Buttons.A, Buttons.Start}, New Keys() {Keys.Enter, Keys.Space}, True)
        menuCancel = New InputAction(New Buttons() {Buttons.B, Buttons.Back}, New Keys() {Keys.Escape}, True)
    End Sub


#End Region

#Region "Handle Input"


    ''' <summary>
    ''' Responds to user input, changing the selected entry and accepting
    ''' or cancelling the menu.
    ''' </summary>
    Public Overrides Sub HandleInput(ByVal gameTime As GameTime, ByVal input As InputState)
        ' For input tests we pass in our ControllingPlayer, which may
        ' either be null (to accept input from any player) or a specific index.
        ' If we pass a null controlling player, the InputState helper returns to
        ' us which player actually provided the input. We pass that through to
        ' OnSelectEntry and OnCancel, so they can tell which player triggered them.
        Dim playerIndex As PlayerIndex

        ' Move to the previous menu entry?
        If menuUp.Evaluate(input, ControllingPlayer, playerIndex) Then
            selectedEntry -= 1

            If selectedEntry < 0 Then
                selectedEntry = _menuEntries.Count - 1
            End If
        End If

        ' Move to the next menu entry?
        If menuDown.Evaluate(input, ControllingPlayer, playerIndex) Then
            selectedEntry += 1

            If selectedEntry >= _menuEntries.Count Then
                selectedEntry = 0
            End If
        End If

        If menuSelect.Evaluate(input, ControllingPlayer, playerIndex) Then
            OnSelectEntry(selectedEntry, playerIndex)
        ElseIf menuCancel.Evaluate(input, ControllingPlayer, playerIndex) Then
            OnCancel(playerIndex)
        End If
    End Sub


    ''' <summary>
    ''' Handler for when the user has chosen a menu entry.
    ''' </summary>
    Protected Overridable Sub OnSelectEntry(ByVal entryIndex As Integer, ByVal playerIndex As PlayerIndex)
        _menuEntries(entryIndex).OnSelectEntry(playerIndex)
    End Sub


    ''' <summary>
    ''' Handler for when the user has cancelled the menu.
    ''' </summary>
    Protected Overridable Sub OnCancel(ByVal playerIndex As PlayerIndex)
        ExitScreen()
    End Sub


    ''' <summary>
    ''' Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
    ''' </summary>
    Protected Sub OnCancel(ByVal sender As Object, ByVal e As PlayerIndexEventArgs)
        OnCancel(e.PlayerIndex)
    End Sub


#End Region

#Region "Update and Draw"


    ''' <summary>
    ''' Allows the screen the chance to position the menu entries. By default
    ''' all menu entries are lined up in a vertical list, centered on the screen.
    ''' </summary>
    Protected Overridable Sub UpdateMenuEntryLocations()
        ' Make the menu slide into place during transitions, using a
        ' power curve to make things look more interesting (this makes
        ' the movement slow down as it nears the end).
        Dim transitionOffset As Single = CSng(Math.Pow(TransitionPosition, 2))

        ' start at Y = 175; each X value is generated per entry
        Dim position As New Vector2(0.0F, 175.0F)

        ' update each menu entry's location in turn
        For i As Integer = 0 To _menuEntries.Count - 1
            Dim menuEntry As MenuEntry = _menuEntries(i)

            ' each entry is to be centered horizontally
            position.X = CSng(ScreenManager.GraphicsDevice.Viewport.Width) / 2 - CSng(menuEntry.GetWidth(Me)) / 2

            If ScreenState = ScreenState.TransitionOn Then
                position.X -= transitionOffset * 256
            Else
                position.X += transitionOffset * 512
            End If

            ' set the entry's position
            menuEntry._Position = position

            ' move down for the next entry the size of this entry
            position.Y += menuEntry.GetHeight(Me)
        Next i
    End Sub


    ''' <summary>
    ''' Updates the menu.
    ''' </summary>
    Public Overrides Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
        MyBase.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen)

        ' Update each nested MenuEntry object.
        For i As Integer = 0 To _menuEntries.Count - 1
            Dim isSelected As Boolean = IsActive AndAlso (i = selectedEntry)

            _menuEntries(i).Update(Me, isSelected, gameTime)
        Next i
    End Sub


    ''' <summary>
    ''' Draws the menu.
    ''' </summary>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        ' make sure our entries are in the right place before we draw them
        UpdateMenuEntryLocations()

        Dim graphics As GraphicsDevice = ScreenManager.GraphicsDevice
        Dim spriteBatch As SpriteBatch = ScreenManager.SpriteBatch
        Dim font As SpriteFont = ScreenManager.Font

        spriteBatch.Begin()

        ' Draw each menu entry in turn.
        For i As Integer = 0 To _menuEntries.Count - 1
            Dim menuEntry As MenuEntry = _menuEntries(i)

            Dim isSelected As Boolean = IsActive AndAlso (i = selectedEntry)

            menuEntry.Draw(Me, isSelected, gameTime)
        Next i

        ' Make the menu slide into place during transitions, using a
        ' power curve to make things look more interesting (this makes
        ' the movement slow down as it nears the end).
        Dim transitionOffset As Single = CSng(Math.Pow(TransitionPosition, 2))

        ' Draw the menu title centered on the screen
        Dim titlePosition As New Vector2(graphics.Viewport.Width \ 2, 80)
        Dim titleOrigin As Vector2 = font.MeasureString(menuTitle) / 2
        Dim titleColor = New Color(192, 192, 192) * TransitionAlpha
        Dim titleScale = 1.25F

        titlePosition.Y -= transitionOffset * 100

        spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0, titleOrigin, titleScale, SpriteEffects.None, 0)

        spriteBatch.End()
    End Sub


#End Region
End Class