#Region "File Description"
'-----------------------------------------------------------------------------
' MenuScreen.vb
'
' XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Namespace GameStateManagement
	''' <summary>
	''' Base class for screens that contain a menu of options. The user can
	''' move up and down to select an entry, or cancel to back out of the screen.
	''' </summary>
	Friend MustInherit Class MenuScreen
		Inherits GameScreen
		#Region "Fields"

		' the number of pixels to pad above and below menu entries for touch input
		Private Const menuEntryPadding As Integer = 35

        Private _menuEntries As New List(Of MenuEntry)
        Private selectedEntry As Integer = 0
        Private menuTitle As String
#If WINDOWS Then
		Private isMouseDown As Boolean = False
#End If
        Private bounds As Rectangle
#End Region

#Region "Properties"


        ''' <summary>
        ''' Gets the list of menu entries, so derived classes can add
        ''' or change the menu contents.
        ''' </summary>
        Protected ReadOnly Property MenuEntries As IList(Of MenuEntry)
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
#If WINDOWS_PHONE Then
            ' menus generally only need Tap for menu selection
            EnabledGestures = GestureType.Tap
#End If

            Me.menuTitle = menuTitle

            TransitionOnTime = TimeSpan.FromSeconds(0.5)
            TransitionOffTime = TimeSpan.FromSeconds(0.5)
        End Sub


#End Region

#Region "Handle Input"

        ''' <summary>
        ''' Allows the screen to create the hit bounds for a particular menu entry.
        ''' </summary>
        Protected Overridable Function GetMenuEntryHitBounds(ByVal entry As MenuEntry) As Rectangle
            ' the hit bounds are the entire width of the screen, and the height of the entry
            ' with some additional padding above and below.
            Return New Rectangle(0, CInt(Fix(entry.Destination.Y)) - menuEntryPadding, ScreenManager.GraphicsDevice.Viewport.Width, entry.GetHeight(Me) + (menuEntryPadding * 2))
        End Function

        ''' <summary>
        ''' Responds to user input, changing the selected entry and accepting
        ''' or cancelling the menu.
        ''' </summary>
        Public Overrides Sub HandleInput(ByVal input As InputState)
            ' we cancel the current menu screen if the user presses the back button
            Dim player As PlayerIndex
            If input.IsNewButtonPress(Buttons.Back, ControllingPlayer, player) Then
                OnCancel(player)
            End If
#If WINDOWS Then
            ' Take care of Keyboard input
            If input.IsMenuUp(ControllingPlayer) Then
                selectedEntry -= 1

                If selectedEntry < 0 Then
                    selectedEntry = _menuEntries.Count - 1
                End If
            ElseIf input.IsMenuDown(ControllingPlayer) Then
                selectedEntry += 1

                If selectedEntry >= _menuEntries.Count Then
                    selectedEntry = 0
                End If
            ElseIf input.IsNewKeyPress(Keys.Enter, ControllingPlayer, player) OrElse input.IsNewKeyPress(Keys.Space, ControllingPlayer, player) Then
                OnSelectEntry(selectedEntry, player)
            End If


            Dim state As MouseState = Mouse.GetState
            If state.LeftButton = ButtonState.Released Then
                If isMouseDown Then
                    isMouseDown = False
                    ' convert the position to a Point that we can test against a Rectangle
                    Dim clickLocation As New Point(state.X, state.Y)

                    ' iterate the entries to see if any were tapped
                    For i = 0 To _menuEntries.Count - 1
                        Dim menuEntry As MenuEntry = _menuEntries(i)

                        If menuEntry.Destination.Contains(clickLocation) Then
                            ' Select the entry. since gestures are only available on Windows Phone,
                            ' we can safely pass PlayerIndex.One to all entries since there is only

                            ' one player on Windows Phone.
                            OnSelectEntry(i, PlayerIndex.One)
                        End If
                    Next i
                End If
            ElseIf state.LeftButton = ButtonState.Pressed Then
                isMouseDown = True

                ' convert the position to a Point that we can test against a Rectangle
                Dim clickLocation As New Point(state.X, state.Y)

                ' iterate the entries to see if any were tapped
                For i = 0 To _menuEntries.Count - 1
                    Dim menuEntry As MenuEntry = _menuEntries(i)

                    If menuEntry.Destination.Contains(clickLocation) Then
                        selectedEntry = i
                    End If
                Next i
            End If
#ElseIf XBOX Then
            ' Take care of Gamepad input
            If input.IsMenuUp(ControllingPlayer) Then
                selectedEntry -= 1

                If selectedEntry < 0 Then
                    selectedEntry = _menuEntries.Count - 1
                End If
            ElseIf input.IsMenuDown(ControllingPlayer) Then
                selectedEntry += 1

                If selectedEntry >= _menuEntries.Count Then
                    selectedEntry = 0
                End If
            ElseIf input.IsNewButtonPress(Buttons.A, ControllingPlayer, player) Then
                OnSelectEntry(selectedEntry, player)
            End If

#ElseIf WINDOWS_PHONE Then
            ' look for any taps that occurred and select any entries that were tapped
            For Each gesture In input.Gestures
                If gesture.GestureType = GestureType.Tap Then
                    ' convert the pohat sition to a Point twe can test against a Rectangle
                    Dim tapLocation As New Point(CInt(Fix(gesture.Position.X)), CInt(Fix(gesture.Position.Y)))

                    ' iterate the entries to see if any were tapped
                    For i = 0 To _menuEntries.Count - 1
                        Dim menuEntry As MenuEntry = _menuEntries(i)

                        If menuEntry.Destination.Contains(tapLocation) Then
                            ' Select the entry. since gestures are only available on Windows Phone,
                            ' we can safely pass PlayerIndex.One to all entries since there is only
                            ' one player on Windows Phone.
                            OnSelectEntry(i, PlayerIndex.One)
                        End If
                    Next i
                End If
            Next gesture
#End If
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

#Region "Loading"
        Public Overrides Sub LoadContent()
            bounds = ScreenManager.SafeArea

            MyBase.LoadContent()
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

            ' start at Y = 475; each X value is generated per entry
            Dim position As New Vector2(0.0F, ScreenManager.Game.Window.ClientBounds.Height \ 2 - (_menuEntries(0).GetHeight(Me) + (menuEntryPadding * 2) * _menuEntries.Count))

            ' update each menu entry's location in turn
            For i = 0 To _menuEntries.Count - 1
                Dim menuEntry As MenuEntry = _menuEntries(i)

                ' each entry is to be centered horizontally
                position.X = ScreenManager.GraphicsDevice.Viewport.Width \ 2 - menuEntry.GetWidth(Me) \ 2

                If ScreenState = ScreenState.TransitionOn Then
                    position.X -= transitionOffset * 256
                Else
                    position.X += transitionOffset * 512
                End If

                ' set the entry's position
                '_menuEntry.Position = position

                ' move down for the next entry the size of this entry plus our padding
                position.Y += menuEntry.GetHeight(Me) + (menuEntryPadding * 2)
            Next i
        End Sub

        ''' <summary>
        ''' Updates the menu.
        ''' </summary>
        Public Overrides Sub Update(ByVal gameTime As GameTime, ByVal otherScreenHasFocus As Boolean, ByVal coveredByOtherScreen As Boolean)
            MyBase.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen)



            ' Update each nested MenuEntry object.
            For i = 0 To _menuEntries.Count - 1
                Dim isSelected As Boolean = IsActive AndAlso (i = selectedEntry)
                UpdateMenuEntryDestination()
                _menuEntries(i).Update(Me, isSelected, gameTime)
            Next i
        End Sub


        ''' <summary>
        ''' Draws the menu.
        ''' </summary>
        Public Overrides Sub Draw(ByVal gameTime As GameTime)
            ' make sure our entries are in the right place before we draw them
            ' UpdateMenuEntryLocations()

            Dim graphics As GraphicsDevice = ScreenManager.GraphicsDevice
            Dim spriteBatch As SpriteBatch = ScreenManager.SpriteBatch
            Dim font As SpriteFont = ScreenManager.Font

            spriteBatch.Begin()

            ' Draw each menu entry in turn.
            For i = 0 To _menuEntries.Count - 1
                Dim menuEntry As MenuEntry = _menuEntries(i)

                Dim isSelected As Boolean = IsActive AndAlso (i = selectedEntry)

                menuEntry.Draw(Me, isSelected, gameTime)
            Next i

            ' Make the menu slide into place during transitions, using a
            ' power curve to make things look more interesting (this makes
            ' the movement slow down as it nears the end).
            Dim transitionOffset As Single = CSng(Math.Pow(TransitionPosition, 2))

            ' Draw the menu title centered on the screen
            Dim titlePosition As New Vector2(graphics.Viewport.Width \ 2, 375)
            Dim titleOrigin As Vector2 = font.MeasureString(menuTitle) / 2
            Dim titleColor As Color = New Color(192, 192, 192) * TransitionAlpha
            Dim titleScale As Single = 1.25F

            titlePosition.Y -= transitionOffset * 100

            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0, titleOrigin, titleScale, SpriteEffects.None, 0)

            spriteBatch.End()
        End Sub


#End Region

#Region "Public functions"
        Public Sub UpdateMenuEntryDestination()
            Dim bounds As Rectangle = ScreenManager.SafeArea

            Dim textureSize As Rectangle = ScreenManager.ButtonBackground.Bounds
            Dim xStep As Integer = bounds.Width \ (_menuEntries.Count + 2)
            Dim maxWidth As Integer = 0

            For i = 0 To _menuEntries.Count - 1
                Dim width As Integer = _menuEntries(i).GetWidth(Me)
                If width > maxWidth Then
                    maxWidth = width
                End If
            Next i
            maxWidth += 20

            For i = 0 To _menuEntries.Count - 1
                _menuEntries(i).Destination = New Rectangle(bounds.Left + (xStep - textureSize.Width) \ 2 + (i + 1) * xStep, bounds.Bottom - textureSize.Height * 2, maxWidth, 50)
            Next i
        End Sub
#End Region
	End Class
End Namespace
