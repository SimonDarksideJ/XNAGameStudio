#Region "File Description"
'-----------------------------------------------------------------------------
' ScreenManager.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.IO
Imports System.IO.IsolatedStorage

Namespace GameStateManagement
	''' <summary>
	''' The screen manager is a component which manages one or more GameScreen
	''' instances. It maintains a stack of screens, calls their Update and Draw
	''' methods at the appropriate times, and automatically routes input to the
	''' topmost active screen.
	''' </summary>
	Public Class ScreenManager
		Inherits DrawableGameComponent
		#Region "Fields"

        Private _screens As New List(Of GameScreen)
        Private _screensToUpdate As New List(Of GameScreen)

        Public input As New InputState


        Private _spriteBatch As SpriteBatch

        Private _font As SpriteFont

        Private _blankTexture As Texture2D

        Private _buttonBackground As Texture2D

        Private _isInitialized As Boolean

        Private _traceEnabled As Boolean

#End Region

#Region "Properties"


        ''' <summary>
        ''' A default SpriteBatch shared by all the screens. This saves
        ''' each screen having to bother creating their own local instance.
        ''' </summary>
        Public ReadOnly Property SpriteBatch As SpriteBatch
            Get
                Return _spriteBatch
            End Get
        End Property

        Public ReadOnly Property ButtonBackground As Texture2D
            Get
                Return _buttonBackground
            End Get
        End Property

        Public ReadOnly Property BlankTexture As Texture2D
            Get
                Return _blankTexture
            End Get
        End Property

        ''' <summary>
        ''' A default font shared by all the screens. This saves
        ''' each screen having to bother loading their own local copy.
        ''' </summary>
        Public ReadOnly Property Font As SpriteFont
            Get
                Return _font
            End Get
        End Property


        ''' <summary>
        ''' If true, the manager prints out a list of all the screens
        ''' each time it is updated. This can be useful for making sure
        ''' everything is being added and removed at the right times.
        ''' </summary>
        Public Property TraceEnabled As Boolean
            Get
                Return _traceEnabled
            End Get
            Set(ByVal value As Boolean)
                _traceEnabled = value
            End Set
        End Property

        ''' <summary>
        ''' Returns the portion of the screen where drawing is safely allowed.
        ''' </summary>
        Public ReadOnly Property SafeArea As Rectangle
            Get
                Return Game.GraphicsDevice.Viewport.TitleSafeArea
            End Get
        End Property
#End Region

#Region "Initialization"


        ''' <summary>
        ''' Constructs a new screen manager component.
        ''' </summary>
        Public Sub New(ByVal game As Game)
            MyBase.New(game)
            ' we must set EnabledGestures before we can query for them, but
            ' we don't assume the game wants to read them.
#If WINDOWS_PHONE Then
            TouchPanel.EnabledGestures = GestureType.None
#End If
        End Sub


        ''' <summary>
        ''' Initializes the screen manager component.
        ''' </summary>
        Public Overrides Sub Initialize()
            MyBase.Initialize()

            _isInitialized = True
        End Sub


        ''' <summary>
        ''' Load your graphics content.
        ''' </summary>
        Protected Overrides Sub LoadContent()
            ' Load content belonging to the screen manager.
            Dim content As ContentManager = Game.Content

            _spriteBatch = New SpriteBatch(GraphicsDevice)
            _font = content.Load(Of SpriteFont)("Fonts/MenuFont")
            _blankTexture = content.Load(Of Texture2D)("Images/blank")
            _buttonBackground = content.Load(Of Texture2D)("Images/ButtonRegular")

            ' Tell each of the screens to load their content.
            For Each screen In _screens
                screen.LoadContent()
            Next screen
        End Sub


        ''' <summary>
        ''' Unload your graphics content.
        ''' </summary>
        Protected Overrides Sub UnloadContent()
            ' Tell each of the screens to unload their content.
            For Each screen In _screens
                screen.UnloadContent()
            Next screen
        End Sub


#End Region

#Region "Update and Draw"


        ''' <summary>
        ''' Allows each screen to run logic.
        ''' </summary>
        Public Overrides Sub Update(ByVal gameTime As GameTime)
            ' Read the keyboard and gamepad.
            input.Update()

            ' Make a copy of the master screen list, to avoid confusion if
            ' the process of updating one screen adds or removes others.
            _screensToUpdate.Clear()

            For Each screen In _screens
                _screensToUpdate.Add(screen)
            Next screen

            Dim otherScreenHasFocus As Boolean = Not Game.IsActive
            Dim coveredByOtherScreen As Boolean = False

            ' Loop as long as there are screens waiting to be updated.
            Do While _screensToUpdate.Count > 0
                ' Pop the topmost screen off the waiting list.
                Dim screen As GameScreen = _screensToUpdate(_screensToUpdate.Count - 1)

                _screensToUpdate.RemoveAt(_screensToUpdate.Count - 1)

                ' Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen)

                If screen.ScreenState = ScreenState.TransitionOn OrElse screen.ScreenState = ScreenState.Active Then
                    ' If this is the first active screen we came across,
                    ' give it a chance to handle input.
                    If Not otherScreenHasFocus Then
                        screen.HandleInput(input)

                        otherScreenHasFocus = True
                    End If

                    ' If this is an active non-popup, inform any subsequent
                    ' screens that they are covered by it.
                    If Not screen.IsPopup Then
                        coveredByOtherScreen = True
                    End If
                End If
            Loop

            ' Print debug trace?
            If _traceEnabled Then
                TraceScreens()
            End If
        End Sub


        ''' <summary>
        ''' Prints a list of all the screens, for debugging.
        ''' </summary>
        Private Sub TraceScreens()
            Dim screenNames As New List(Of String)

            For Each screen In _screens
                screenNames.Add(screen.GetType.Name)
            Next screen

            Debug.WriteLine(String.Join(", ", screenNames.ToArray))
        End Sub


        ''' <summary>
        ''' Tells each screen to draw itself.
        ''' </summary>
        Public Overrides Sub Draw(ByVal gameTime As GameTime)
            For Each screen In _screens
                If screen.ScreenState = ScreenState.Hidden Then
                    Continue For
                End If

                screen.Draw(gameTime)
            Next screen
        End Sub


#End Region

#Region "Public Methods"


        ''' <summary>
        ''' Adds a new screen to the screen manager.
        ''' </summary>
        Public Sub AddScreen(ByVal screen As GameScreen, ByVal controllingPlayer? As PlayerIndex)
            screen.ControllingPlayer = controllingPlayer
            screen.ScreenManager = Me
            screen.IsExiting = False

            ' If we have a graphics device, tell the screen to load content.
            If _isInitialized Then
                screen.LoadContent()
            End If

            _screens.Add(screen)

            ' update the TouchPanel to respond to gestures this screen is interested in
#If WINDOWS_PHONE Then
            TouchPanel.EnabledGestures = screen.EnabledGestures
#End If
        End Sub


        ''' <summary>
        ''' Removes a screen from the screen manager. You should normally
        ''' use GameScreen.ExitScreen instead of calling this directly, so
        ''' the screen can gradually transition off rather than just being
        ''' instantly removed.
        ''' </summary>
        Public Sub RemoveScreen(ByVal screen As GameScreen)
            ' If we have a graphics device, tell the screen to unload content.
            If _isInitialized Then
                screen.UnloadContent()
            End If

            _screens.Remove(screen)
            _screensToUpdate.Remove(screen)

#If WINDOWS_PHONE Then
            ' if there is a screen still in the manager, update TouchPanel
            ' to respond to gestures that screen is interested in.
            If _screens.Count > 0 Then
                TouchPanel.EnabledGestures = _screens(_screens.Count - 1).EnabledGestures
            End If
#End If
        End Sub


        ''' <summary>
        ''' Expose an array holding all the screens. We return a copy rather
        ''' than the real master list, because screens should only ever be added
        ''' or removed using the AddScreen and RemoveScreen methods.
        ''' </summary>
        Public Function GetScreens() As GameScreen()
            Return _screens.ToArray
        End Function


        ''' <summary>
        ''' Helper draws a translucent black fullscreen sprite, used for fading
        ''' screens in and out, and for darkening the background behind popups.
        ''' </summary>
        Public Sub FadeBackBufferToBlack(ByVal alpha As Single)
            Dim viewport As Viewport = GraphicsDevice.Viewport

            _spriteBatch.Begin()

            _spriteBatch.Draw(_blankTexture, New Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * alpha)

            _spriteBatch.End()
        End Sub

        ''' <summary>
        ''' Informs the screen manager to serialize its state to disk.
        ''' </summary>
        Public Sub SerializeState()
            ' open up isolated storage
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication
                ' if our screen manager directory already exists, delete the contents
                If storage.DirectoryExists("ScreenManager") Then
                    DeleteState(storage)

                    ' otherwise just create the directory
                Else
                    storage.CreateDirectory("ScreenManager")
                End If

                ' create a file we'll use to store the list of screens in the stack
                Using stream As IsolatedStorageFileStream = storage.CreateFile("ScreenManager\ScreenList.dat")
                    Using writer As New BinaryWriter(stream)
                        ' write out the full name of all the types in our stack so we can
                        ' recreate them if needed.
                        For Each screen In _screens
                            If screen.IsSerializable Then
                                writer.Write(screen.GetType.AssemblyQualifiedName)
                            End If
                        Next screen
                    End Using
                End Using

                ' now we create a new file stream for each screen so it can save its state
                ' if it needs to. we name each file "ScreenX.dat" where X is the index of
                ' the screen in the stack, to ensure the files are uniquely named
                Dim screenIndex = 0
                For Each screen In _screens
                    If screen.IsSerializable Then
                        Dim fileName As String = String.Format("ScreenManager\Screen{0}.dat", screenIndex)

                        ' open up the stream and let the screen serialize whatever state it wants
                        Using stream As IsolatedStorageFileStream = storage.CreateFile(fileName)
                            screen.Serialize(stream)
                        End Using

                        screenIndex += 1
                    End If
                Next screen
            End Using
        End Sub

        Public Function DeserializeState() As Boolean
            ' open up isolated storage
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication
                ' see if our saved state directory exists
                If storage.DirectoryExists("ScreenManager") Then
                    Try
                        ' see if we have a screen list
                        If storage.FileExists("ScreenManager\ScreenList.dat") Then
                            ' load the list of screen types
                            Using stream As IsolatedStorageFileStream = storage.OpenFile("ScreenManager\ScreenList.dat", FileMode.Open, FileAccess.Read)
                                Using reader As New BinaryReader(stream)
                                    Do While reader.BaseStream.Position < reader.BaseStream.Length
                                        ' read a line from our file
                                        Dim line As String = reader.ReadString

                                        ' if it isn't blank, we can create a screen from it
                                        If Not String.IsNullOrEmpty(line) Then
                                            Dim screenType As Type = Type.GetType(line)
                                            Dim screen As GameScreen = TryCast(Activator.CreateInstance(screenType), GameScreen)
                                            AddScreen(screen, PlayerIndex.One)
                                        End If
                                    Loop
                                End Using
                            End Using
                        End If

                        ' next we give each screen a chance to deserialize from the disk
                        For i = 0 To _screens.Count - 1
                            Dim filename As String = String.Format("ScreenManager\Screen{0}.dat", i)
                            Using stream As IsolatedStorageFileStream = storage.OpenFile(filename, FileMode.Open, FileAccess.Read)
                                _screens(i).Deserialize(stream)
                            End Using
                        Next i

                        Return True
                    Catch e1 As Exception
                        ' if an exception was thrown while reading, odds are we cannot recover
                        ' from the saved state, so we will delete it so the game can correctly
                        ' launch.
                        DeleteState(storage)
                    End Try
                End If
            End Using

            Return False
        End Function

        ''' <summary>
        ''' Deletes the saved state files from isolated storage.
        ''' </summary>
        Private Sub DeleteState(ByVal storage As IsolatedStorageFile)
            ' get all of the files in the directory and delete them
            Dim files() As String = storage.GetFileNames("ScreenManager\*")
            For Each file In files
                storage.DeleteFile(Path.Combine("ScreenManager", file))
            Next file
        End Sub

#End Region
	End Class
End Namespace
