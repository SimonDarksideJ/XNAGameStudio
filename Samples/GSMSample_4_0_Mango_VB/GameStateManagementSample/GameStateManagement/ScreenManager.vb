#Region "File Description"
'-----------------------------------------------------------------------------
' ScreenManager.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Diagnostics
Imports System.Collections.Generic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input.Touch
Imports System.IO
Imports System.IO.IsolatedStorage
Imports System.Xml.Linq
#End Region
''' <summary>
''' The screen manager is a component which manages one or more GameScreen
''' instances. It maintains a stack of screens, calls their Update and Draw
''' methods at the appropriate times, and automatically routes input to the
''' topmost active screen.
''' </summary>
Public Class ScreenManager
    Inherits DrawableGameComponent
#Region "Fields"

    Private Const StateFilename As String = "ScreenManagerState.xml"

    Private screens As New List(Of GameScreen)()
    Private tempScreensList As New List(Of GameScreen)()

    Private input As New InputState()
    Private _spriteBatch As SpriteBatch
    Private _font As SpriteFont
    Private _blankTexture As Texture2D

    Private isInitialized As Boolean

#End Region

#Region "Properties"

    ''' <summary>
    ''' A default SpriteBatch shared by all the screens. This saves
    ''' each screen having to bother creating their own local instance.
    ''' </summary>
    Public ReadOnly Property SpriteBatch() As SpriteBatch
        Get
            Return _spriteBatch
        End Get
    End Property


    ''' <summary>
    ''' A default font shared by all the screens. This saves
    ''' each screen having to bother loading their own local copy.
    ''' </summary>
    Public ReadOnly Property Font() As SpriteFont
        Get
            Return _font
        End Get
    End Property


    ''' <summary>
    ''' If true, the manager prints out a list of all the screens
    ''' each time it is updated. This can be useful for making sure
    ''' everything is being added and removed at the right times.
    ''' </summary>
    Public Property TraceEnabled() As Boolean
    ''' <summary>
    ''' Gets a blank texture that can be used by the screens.
    ''' </summary>
    Public ReadOnly Property BlankTexture() As Texture2D
        Get
            Return _blankTexture
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
        TouchPanel.EnabledGestures = GestureType.None
    End Sub


    ''' <summary>
    ''' Initializes the screen manager component.
    ''' </summary>
    Public Overrides Sub Initialize()
        MyBase.Initialize()

        isInitialized = True
    End Sub


    ''' <summary>
    ''' Load your graphics content.
    ''' </summary>
    Protected Overrides Sub LoadContent()
        ' Load content belonging to the screen manager.
        Dim content As ContentManager = Game.Content

        _spriteBatch = New SpriteBatch(GraphicsDevice)
        _font = content.Load(Of SpriteFont)("menufont")
        _blankTexture = content.Load(Of Texture2D)("blank")

        ' Tell each of the screens to load their content.
        For Each screen As GameScreen In screens
            screen.Activate(False)
        Next screen
    End Sub


    ''' <summary>
    ''' Unload your graphics content.
    ''' </summary>
    Protected Overrides Sub UnloadContent()
        ' Tell each of the screens to unload their content.
        For Each screen As GameScreen In screens
            screen.Unload()
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
        tempScreensList.Clear()

        For Each screen As GameScreen In screens
            tempScreensList.Add(screen)
        Next screen

        Dim otherScreenHasFocus As Boolean = Not Game.IsActive
        Dim coveredByOtherScreen As Boolean = False

        ' Loop as long as there are screens waiting to be updated.
        Do While tempScreensList.Count > 0
            ' Pop the topmost screen off the waiting list.
            Dim screen As GameScreen = tempScreensList(tempScreensList.Count - 1)

            tempScreensList.RemoveAt(tempScreensList.Count - 1)

            ' Update the screen.
            screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen)

            If screen.ScreenState = ScreenState.TransitionOn OrElse screen.ScreenState = ScreenState.Active Then
                ' If this is the first active screen we came across,
                ' give it a chance to handle input.
                If Not otherScreenHasFocus Then
                    screen.HandleInput(gameTime, input)

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
        Dim screenNames As New List(Of String)()

        For Each screen As GameScreen In screens
            screenNames.Add(screen.GetType().Name)
        Next screen

        Debug.WriteLine(String.Join(", ", screenNames.ToArray()))
    End Sub


    ''' <summary>
    ''' Tells each screen to draw itself.
    ''' </summary>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        For Each screen As GameScreen In screens
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
        If isInitialized Then
            screen.Activate(False)
        End If

        screens.Add(screen)

        ' update the TouchPanel to respond to gestures this screen is interested in
        TouchPanel.EnabledGestures = screen.EnabledGestures
    End Sub


    ''' <summary>
    ''' Removes a screen from the screen manager. You should normally
    ''' use GameScreen.ExitScreen instead of calling this directly, so
    ''' the screen can gradually transition off rather than just being
    ''' instantly removed.
    ''' </summary>
    Public Sub RemoveScreen(ByVal screen As GameScreen)
        ' If we have a graphics device, tell the screen to unload content.
        If isInitialized Then
            screen.Unload()
        End If

        screens.Remove(screen)
        tempScreensList.Remove(screen)

        ' if there is a screen still in the manager, update TouchPanel
        ' to respond to gestures that screen is interested in.
        If screens.Count > 0 Then
            TouchPanel.EnabledGestures = screens(screens.Count - 1).EnabledGestures
        End If
    End Sub


    ''' <summary>
    ''' Expose an array holding all the screens. We return a copy rather
    ''' than the real master list, because screens should only ever be added
    ''' or removed using the AddScreen and RemoveScreen methods.
    ''' </summary>
    Public Function GetScreens() As GameScreen()
        Return screens.ToArray()
    End Function


    ''' <summary>
    ''' Helper draws a translucent black fullscreen sprite, used for fading
    ''' screens in and out, and for darkening the background behind popups.
    ''' </summary>
    Public Sub FadeBackBufferToBlack(ByVal alpha As Single)
        _spriteBatch.Begin()
        _spriteBatch.Draw(_blankTexture, GraphicsDevice.Viewport.Bounds, Color.Black * alpha)
        _spriteBatch.End()
    End Sub

    ''' <summary>
    ''' Informs the screen manager to serialize its state to disk.
    ''' </summary>
    Public Sub Deactivate()
#If Not WINDOWS_PHONE Then
			Return
#Else
        ' Open up isolated storage
        Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication()
            ' Create an XML document to hold the list of screen types currently in the stack
            Dim doc As New XDocument()
            Dim root As New XElement("ScreenManager")
            doc.Add(root)

            ' Make a copy of the master screen list, to avoid confusion if
            ' the process of deactivating one screen adds or removes others.
            tempScreensList.Clear()
            For Each screen As GameScreen In screens
                tempScreensList.Add(screen)
            Next screen

            ' Iterate the screens to store in our XML file and deactivate them
            For Each screen As GameScreen In tempScreensList
                ' Only add the screen to our XML if it is serializable
                If screen.IsSerializable Then
                    ' We store the screen's controlling player so we can rehydrate that value
                    Dim playerValue As String = If(screen.ControllingPlayer.HasValue, screen.ControllingPlayer.Value.ToString(), "")

                    root.Add(New XElement("GameScreen", New XAttribute("Type", screen.GetType().AssemblyQualifiedName), New XAttribute("ControllingPlayer", playerValue)))
                End If

                ' Deactivate the screen regardless of whether we serialized it
                screen.Deactivate()
            Next screen

            ' Save the document
            Using stream As IsolatedStorageFileStream = storage.CreateFile(StateFilename)
                doc.Save(stream)
            End Using
        End Using
#End If
    End Sub

    Public Function Activate(ByVal instancePreserved As Boolean) As Boolean
#If Not WINDOWS_PHONE Then
			Return False
#Else
        ' If the game instance was preserved, the game wasn't dehydrated so our screens still exist.
        ' We just need to activate them and we're ready to go.
        If instancePreserved Then
            ' Make a copy of the master screen list, to avoid confusion if
            ' the process of activating one screen adds or removes others.
            tempScreensList.Clear()

            For Each screen As GameScreen In screens
                tempScreensList.Add(screen)
            Next screen

            For Each screen As GameScreen In tempScreensList
                screen.Activate(True)
            Next screen

            ' Otherwise we need to refer to our saved file and reconstruct the screens that were present
            ' when the game was deactivated.
        Else
            ' Try to get the screen factory from the services, which is required to recreate the screens
            Dim screenFactory As IScreenFactory = TryCast(Game.Services.GetService(GetType(IScreenFactory)), IScreenFactory)
            If screenFactory Is Nothing Then
                Throw New InvalidOperationException("Game.Services must contain an IScreenFactory in order to activate the ScreenManager.")
            End If

            ' Open up isolated storage
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication()
                ' Check for the file; if it doesn't exist we can't restore state
                If Not storage.FileExists(StateFilename) Then
                    Return False
                End If

                ' Read the state file so we can build up our screens
                Using stream As IsolatedStorageFileStream = storage.OpenFile(StateFilename, FileMode.Open)
                    Dim doc As XDocument = XDocument.Load(stream)

                    ' Iterate the document to recreate the screen stack
                    For Each screenElem As XElement In doc.Root.Elements("GameScreen")
                        ' Use the factory to create the screen
                        Dim screenType As Type = Type.GetType(screenElem.Attribute("Type").Value)
                        Dim screen As GameScreen = screenFactory.CreateScreen(screenType)

                        ' Rehydrate the controlling player for the screen
                        Dim controllingPlayer? As PlayerIndex = If(screenElem.Attribute("ControllingPlayer").Value <> "", CType(System.Enum.Parse(GetType(PlayerIndex), screenElem.Attribute("ControllingPlayer").Value, True), PlayerIndex), CType(Nothing, PlayerIndex?))
                        screen.ControllingPlayer = controllingPlayer

                        ' Add the screen to the screens list and activate the screen
                        screen.ScreenManager = Me
                        screens.Add(screen)
                        screen.Activate(False)

                        ' update the TouchPanel to respond to gestures this screen is interested in
                        TouchPanel.EnabledGestures = screen.EnabledGestures
                    Next screenElem
                End Using
            End Using
        End If

        Return True
#End If
    End Function

#End Region
End Class