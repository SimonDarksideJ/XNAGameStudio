#Region "File Description"
'-----------------------------------------------------------------------------
' AudioManager.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

''' <summary>
''' Component that manages audio playback for all sounds.
''' </summary>
Public Class AudioManager
    Inherits GameComponent
#Region "Fields"


    ''' <summary>
    ''' The singleton for this type.
    ''' </summary>
    Private Shared _audioManager As AudioManager = Nothing
    Public Shared ReadOnly Property Instance As AudioManager
        Get
            Return _audioManager
        End Get
    End Property

    Private Shared ReadOnly soundAssetLocation As String = "Sounds/"

    ' Audio Data        
    Private soundBank As Dictionary(Of String, SoundEffectInstance)
    Private musicBank As Dictionary(Of String, Song)


#End Region

#Region "Initialization"


    Private Sub New(ByVal game As Game)
        MyBase.New(game)
    End Sub

    ''' <summary>
    ''' Initialize the static AudioManager functionality.
    ''' </summary>
    ''' <param name="game">The game that this component will be attached to.</param>
    Public Overloads Shared Sub Initialize(ByVal game As Game)
        _audioManager = New AudioManager(game)
        _audioManager.soundBank = New Dictionary(Of String, SoundEffectInstance)
        _audioManager.musicBank = New Dictionary(Of String, Song)

        game.Components.Add(_audioManager)
    End Sub


#End Region

#Region "Loading Methodes"


    ''' <summary>
    ''' Loads a single sound into the sound manager, giving it a specified alias.
    ''' </summary>
    ''' <param name="contentName">The content name of the sound file. Assumes all sounds are located under
    ''' the "Sounds" folder in the content project.</param>
    ''' <param name="alias">Alias to give the sound. This will be used to identify the sound uniquely.</param>
    ''' <remarks>Loading a sound with an alias that is already used will have no effect.</remarks>
    Public Shared Sub LoadSound(ByVal contentName As String, ByVal [alias] As String)
        Dim soundEffect As SoundEffect = _audioManager.Game.Content.Load(Of SoundEffect)(soundAssetLocation & contentName)
        Dim soundEffectInstance As SoundEffectInstance = soundEffect.CreateInstance

        If Not _audioManager.soundBank.ContainsKey([alias]) Then
            _audioManager.soundBank.Add([alias], soundEffectInstance)
        End If
    End Sub

    ''' <summary>
    ''' Loads a single song into the sound manager, giving it a specified alias.
    ''' </summary>
    ''' <param name="contentName">The content name of the sound file containing the song. Assumes all sounds are 
    ''' located under the "Sounds" folder in the content project.</param>
    ''' <param name="alias">Alias to give the song. This will be used to identify the song uniquely.</param>
    ''' /// <remarks>Loading a song with an alias that is already used will have no effect.</remarks>
    Public Shared Sub LoadSong(ByVal contentName As String, ByVal [alias] As String)
        Dim song As Song = _audioManager.Game.Content.Load(Of Song)(soundAssetLocation & contentName)

        If Not _audioManager.musicBank.ContainsKey([alias]) Then
            _audioManager.musicBank.Add([alias], song)
        End If
    End Sub

    ''' <summary>
    ''' Loads and organizes the sounds used by the game.
    ''' </summary>
    Public Shared Sub LoadSounds()
        LoadSound("Bet", "Bet")
        LoadSound("CardFlip", "Flip")
        LoadSound("CardsShuffle", "Shuffle")
        LoadSound("Deal", "Deal")
    End Sub

    ''' <summary>
    ''' Loads and organizes the music used by the game.
    ''' </summary>
    Public Shared Sub LoadMusic()
        LoadSong("InGameSong_Loop", "InGameSong_Loop")
        LoadSong("MenuMusic_Loop", "MenuMusic_Loop")
    End Sub

#End Region

#Region "Sound Methods"


    ''' <summary>
    ''' Indexer. Return a sound instance by name.
    ''' </summary>
    Default Public ReadOnly Property Item(ByVal soundName As String) As SoundEffectInstance
        Get
            If _audioManager.soundBank.ContainsKey(soundName) Then
                Return _audioManager.soundBank(soundName)
            Else
                Return Nothing
            End If
        End Get
    End Property

    ''' <summary>
    ''' Plays a sound by name.
    ''' </summary>
    ''' <param name="soundName">The name of the sound to play.</param>
    Public Shared Sub PlaySound(ByVal soundName As String)
        ' If the sound exists, start it
        If _audioManager.soundBank.ContainsKey(soundName) Then
            _audioManager.soundBank(soundName).Play()
        End If
    End Sub

    ''' <summary>
    ''' Plays a sound by name.
    ''' </summary>
    ''' <param name="soundName">The name of the sound to play.</param>
    ''' <param name="isLooped">Indicates if the sound should loop.</param>
    Public Shared Sub PlaySound(ByVal soundName As String, ByVal isLooped As Boolean)
        ' If the sound exists, start it
        If _audioManager.soundBank.ContainsKey(soundName) Then
            If _audioManager.soundBank(soundName).IsLooped <> isLooped Then
                _audioManager.soundBank(soundName).IsLooped = isLooped
            End If

            _audioManager.soundBank(soundName).Play()
        End If
    End Sub


    ''' <summary>
    ''' Plays a sound by name.
    ''' </summary>
    ''' <param name="soundName">The name of the sound to play.</param>
    ''' <param name="isLooped">Indicates if the sound should loop.</param>
    ''' <param name="volume">Indicates if the volume</param>
    Public Shared Sub PlaySound(ByVal soundName As String, ByVal isLooped As Boolean, ByVal volume As Single)
        ' If the sound exists, start it
        If _audioManager.soundBank.ContainsKey(soundName) Then
            If _audioManager.soundBank(soundName).IsLooped <> isLooped Then
                _audioManager.soundBank(soundName).IsLooped = isLooped
            End If

            _audioManager.soundBank(soundName).Volume = volume
            _audioManager.soundBank(soundName).Play()
        End If
    End Sub

    ''' <summary>
    ''' Stops a sound mid-play. If the sound is not playing, this
    ''' method does nothing.
    ''' </summary>
    ''' <param name="soundName">The name of the sound to stop.</param>
    Public Shared Sub StopSound(ByVal soundName As String)
        ' If the sound exists, stop it
        If _audioManager.soundBank.ContainsKey(soundName) Then
            _audioManager.soundBank(soundName).Stop()
        End If
    End Sub

    ''' <summary>
    ''' Stops all currently playing sounds.
    ''' </summary>
    Public Shared Sub StopSounds()
        For Each sound In _audioManager.soundBank.Values
            If sound.State <> SoundState.Stopped Then
                sound.Stop()
            End If
        Next sound
    End Sub

    ''' <summary>
    ''' Pause or resume all sounds.
    ''' </summary>
    ''' <param name="resumeSounds">True to resume all paused sounds or false
    ''' to pause all playing sounds.</param>
    Public Shared Sub PauseResumeSounds(ByVal resumeSounds As Boolean)
        Dim state As SoundState = If(resumeSounds, SoundState.Paused, SoundState.Playing)

        For Each sound In _audioManager.soundBank.Values
            If sound.State = state Then
                If resumeSounds Then
                    sound.Resume()
                Else
                    sound.Pause()
                End If
            End If
        Next sound
    End Sub
    ''' <summary>
    ''' Play music by name. This stops the currently playing music first. Music will loop until stopped.
    ''' </summary>
    ''' <param name="musicSoundName">The name of the music sound.</param>
    ''' <remarks>If the desired music is not in the music bank, nothing will happen.</remarks>
    Public Shared Sub PlayMusic(ByVal musicSoundName As String)
        ' If the music sound exists
        If _audioManager.musicBank.ContainsKey(musicSoundName) Then
            ' Stop the old music sound
            If MediaPlayer.State <> MediaState.Stopped Then
                MediaPlayer.Stop()
            End If

            MediaPlayer.IsRepeating = True

            MediaPlayer.Play(_audioManager.musicBank(musicSoundName))
        End If
    End Sub

    ''' <summary>
    ''' Stops the currently playing music.
    ''' </summary>
    Public Shared Sub StopMusic()
        If MediaPlayer.State <> MediaState.Stopped Then
            MediaPlayer.Stop()
        End If
    End Sub


#End Region

#Region "Instance Disposal Methods"


    ''' <summary>
    ''' Clean up the component when it is disposing.
    ''' </summary>
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing Then
                For Each itemVar In soundBank
                    itemVar.Value.Dispose()
                Next itemVar
                soundBank.Clear()
                soundBank = Nothing
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

#End Region
End Class