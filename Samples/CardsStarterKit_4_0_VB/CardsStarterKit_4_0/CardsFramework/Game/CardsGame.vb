#Region "File Description"
'-----------------------------------------------------------------------------
' Cardsgame.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
#End Region


''' <summary>
''' A cards-game handler
''' </summary>
''' <remarks>
''' Use a singleton of a class that derives from class to empower a cards-game, while making sure
''' to call the various methods in order to allow the implementing instance to run the game.
''' </remarks>
Public MustInherit Class CardsGame
#Region "Fields and Properties"
    Protected rules As List(Of GameRule)
    Protected players As List(Of Player)
    Protected dealer As CardPacket

    Private _minimumPlayers As Integer
    Public Property MinimumPlayers As Integer
        Get
            Return _minimumPlayers
        End Get
        Protected Set(ByVal value As Integer)
            _minimumPlayers = value
        End Set
    End Property

    Private _maximumPlayers As Integer
    Public Property MaximumPlayers As Integer
        Get
            Return _maximumPlayers
        End Get
        Protected Set(ByVal value As Integer)
            _maximumPlayers = value
        End Set
    End Property

    Private _theme As String
    Public Property Theme As String
        Get
            Return _theme
        End Get
        Protected Set(ByVal value As String)
            _theme = value
        End Set
    End Property
    Protected Friend cardsAssets As Dictionary(Of String, Texture2D)

    Private _gameTable As GameTable
    Public Property GameTable As GameTable
        Get
            Return _gameTable
        End Get
        Protected Set(ByVal value As GameTable)
            _gameTable = value
        End Set
    End Property

    Public Property Font As SpriteFont
    Public Property SpriteBatch As SpriteBatch
    Public Property Game As Game
#End Region

#Region "Initializations"
    ''' <summary>
    ''' Initializes a new instance of the <see cref="CardsGame"/> class.
    ''' </summary>
    ''' <param name="decks">The amount of decks in the game.</param>
    ''' <param name="jokersInDeck">The amount of jokers in each deck.</param>
    ''' <param name="suits">The suits which will appear in each deck. Multiple 
    ''' values can be supplied using flags.</param>
    ''' <param name="cardValues">The card values which will appear in each deck. 
    ''' Multiple values can be supplied using flags.</param>
    ''' <param name="minimumPlayers">The minimal amount of players 
    ''' for the game.</param>
    ''' <param name="maximumPlayers">The maximal amount of players 
    ''' for the game.</param>
    ''' <param name="theme">The name of the theme to use for the 
    ''' game's assets.</param>
    ''' <param name="game">The associated game object.</param>
    Public Sub New(ByVal decks As Integer, ByVal jokersInDeck As Integer, ByVal suits As CardSuit, ByVal cardValues As CardValue, ByVal minimumPlayers As Integer, ByVal maximumPlayers As Integer, ByVal gameTable As GameTable, ByVal theme As String, ByVal game As Game)
        rules = New List(Of GameRule)
        players = New List(Of Player)
        dealer = New CardPacket(decks, jokersInDeck, suits, cardValues)

        Me.Game = game
        Me.MinimumPlayers = minimumPlayers
        Me.MaximumPlayers = maximumPlayers

        Me.Theme = theme
        cardsAssets = New Dictionary(Of String, Texture2D)
        Me.GameTable = gameTable
        gameTable.DrawOrder = -10000
        game.Components.Add(gameTable)
    End Sub
#End Region

#Region "Virtual Methods"
    ''' <summary>
    ''' Checks which of the game's rules need to be fired.
    ''' </summary>
    Public Overridable Sub CheckRules()
        For ruleIndex = 0 To rules.Count - 1
            rules(ruleIndex).Check()
        Next ruleIndex
    End Sub

    ''' <summary>
    ''' Returns a card's value in the scope of the game.
    ''' </summary>
    ''' <param name="card">The card for which to return the value.</param>
    ''' <returns>The card's value.</returns>        
    Public Overridable Function CardValue(ByVal card As TraditionalCard) As Integer
        Select Case card.Value
            Case CardsFramework.CardValue.Ace
                Return 1
            Case CardsFramework.CardValue.Two
                Return 2
            Case CardsFramework.CardValue.Three
                Return 3
            Case CardsFramework.CardValue.Four
                Return 4
            Case CardsFramework.CardValue.Five
                Return 5
            Case CardsFramework.CardValue.Six
                Return 6
            Case CardsFramework.CardValue.Seven
                Return 7
            Case CardsFramework.CardValue.Eight
                Return 8
            Case CardsFramework.CardValue.Nine
                Return 9
            Case CardsFramework.CardValue.Ten
                Return 10
            Case CardsFramework.CardValue.Jack
                Return 11
            Case CardsFramework.CardValue.Queen
                Return 12
            Case CardsFramework.CardValue.King
                Return 13
            Case Else
                Throw New ArgumentException("Ambigous card value")
        End Select
    End Function
#End Region

#Region "Abstract Methods"
    ''' <summary>
    ''' Adds a player to the game.
    ''' </summary>
    ''' <param name="player">The player to add to the game.</param>
    Public MustOverride Sub AddPlayer(ByVal player As Player)

    ''' <summary>
    ''' Gets the player who is currently taking his turn.
    ''' </summary>
    ''' <returns>The currently active player.</returns>
    Public MustOverride Function GetCurrentPlayer() As Player

    ''' <summary>
    ''' Deals cards to the participating players.
    ''' </summary>
    Public MustOverride Sub Deal()

    ''' <summary>
    ''' Initializes the game lets the players start playing.
    ''' </summary>
    Public MustOverride Sub StartPlaying()
#End Region

#Region "Loading"
    ''' <summary>
    ''' Load the basic contents for a card game (the card assets)
    ''' </summary>
    Public Sub LoadContent()
        SpriteBatch = New SpriteBatch(Game.GraphicsDevice)
        ' Initialize a full deck
        Dim fullDeck As New CardPacket(1, 2, CardSuit.AllSuits, CardsFramework.CardValue.NonJokers Or CardsFramework.CardValue.Jokers)
        Dim assetName As String

        ' Load all card assets
        For cardIndex = 0 To 53
            assetName = UIUtilty.GetCardAssetName(fullDeck(cardIndex))
            LoadUITexture("Cards", assetName)
        Next cardIndex
        ' Load card back picture
        LoadUITexture("Cards", "CardBack_" & Theme)

        ' Load the game's font
        Font = Game.Content.Load(Of SpriteFont)(String.Format("Fonts\Regular"))

        GameTable.Initialize()
    End Sub

    ''' <summary>
    ''' Loads the UI textures for the game, taking the theme into account.
    ''' </summary>
    ''' <param name="folder">The asset's folder under the theme folder. For example,
    ''' for an asset belonging to the "Fish" theme and which sits in 
    ''' "Images\Fish\Beverages\Soda" folder under the content project, use
    ''' "Beverages\Soda" as this argument's value.</param>
    ''' <param name="assetName">The name of the asset.</param>
    Public Sub LoadUITexture(ByVal folder As String, ByVal assetName As String)
        cardsAssets.Add(assetName, Game.Content.Load(Of Texture2D)(String.Format("Images\{0}\{1}", folder, assetName)))
    End Sub
#End Region
End Class
