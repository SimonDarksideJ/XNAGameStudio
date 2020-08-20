#Region "File Description"
'-----------------------------------------------------------------------------
' CardsCollection.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text


''' <summary>
''' Card related <see cref="EventArgs"/> holding event information of a <see cref="TraditionalCard"/> 
''' </summary>
Public Class CardEventArgs
    Inherits EventArgs
    Public Property Card As TraditionalCard
End Class

''' <summary>
''' A packet of cards
''' </summary>
''' <remarks>
''' A card packet may be initialized with a collection of cards. 
''' It may lose cards or deal them to <see cref="Hand"/>, but may
''' not receive new cards unless derived and overridden.
''' </remarks>
Public Class CardPacket
#Region "Field Property Indexer"
    Protected Property cards As List(Of TraditionalCard)

    ''' <summary>
    ''' An event which triggers when a card is removed from the collection.
    ''' </summary>
    Public Event LostCard As EventHandler(Of CardEventArgs)

    Public ReadOnly Property Count As Integer
        Get
            Return cards.Count
        End Get
    End Property

    ''' <summary>
    ''' Initializes a card collection by simply allocating a new card list.
    ''' </summary>
    Protected Sub New()
        cards = New List(Of TraditionalCard)
    End Sub

    ''' <summary>
    ''' Returns a card at a specified index in the collection.
    ''' </summary>
    ''' <param name="index">The card's index.</param>
    ''' <returns>The card at the specified index.</returns>
    Default Public ReadOnly Property Item(ByVal index As Integer) As TraditionalCard
        Get
            Return cards(index)
        End Get
    End Property
#End Region

#Region "Initializations"
    ''' <summary>
    ''' Initializes a new instance of the <see cref="CardPacket"/> class.
    ''' </summary>
    ''' <param name="numberOfDecks">The number of decks to add to 
    ''' the collection.</param>
    ''' <param name="jokersInDeck">The amount of jokers in each deck.</param>
    ''' <param name="suits">The suits to add to each decks. Suits are specified 
    ''' as flags and several can be added.</param>
    ''' <param name="cardValues">The card values which will appear in each deck.
    ''' values are specified as flags and several can be added.</param>
    Public Sub New(ByVal numberOfDecks As Integer, ByVal jokersInDeck As Integer, ByVal suits As CardSuit, ByVal cardValues As CardValue)
        cards = New List(Of TraditionalCard)

        For deckIndex = 0 To numberOfDecks - 1
            AddSuit(suits, cardValues)

            For j = 0 To jokersInDeck \ 2 - 1
                cards.Add(New TraditionalCard(CardSuit.Club, CardValue.FirstJoker, Me))
                cards.Add(New TraditionalCard(CardSuit.Club, CardValue.SecondJoker, Me))
            Next j

            If jokersInDeck Mod 2 = 1 Then
                cards.Add(New TraditionalCard(CardSuit.Club, CardValue.FirstJoker, Me))
            End If
        Next deckIndex
    End Sub
#End Region

#Region "Private Methods"
    ''' <summary>
    ''' Adds suits of cards to the collection.
    ''' </summary>
    ''' <param name="suits">The suits to add to each decks. Suits are specified 
    ''' as flags and several can be added.</param>
    ''' <param name="cardValues">The card values which will appear in each deck.
    ''' values are specified as flags and several can be added.</param>
    Private Sub AddSuit(ByVal suits As CardSuit, ByVal cardValues As CardValue)
        If (suits And CardSuit.Club) = CardSuit.Club Then
            AddCards(CardSuit.Club, cardValues)
        End If

        If (suits And CardSuit.Diamond) = CardSuit.Diamond Then
            AddCards(CardSuit.Diamond, cardValues)
        End If

        If (suits And CardSuit.Heart) = CardSuit.Heart Then
            AddCards(CardSuit.Heart, cardValues)
        End If

        If (suits And CardSuit.Spade) = CardSuit.Spade Then
            AddCards(CardSuit.Spade, cardValues)
        End If
    End Sub

    ''' <summary>
    ''' Adds cards to the collection.
    ''' </summary>
    ''' <param name="suit">The suit of the added cards.</param>
    ''' <param name="cardValues">The card values which will appear in each deck.
    ''' values are specified as flags and several can be added.</param>
    Private Sub AddCards(ByVal suit As CardSuit, ByVal cardValues As CardValue)
        If (cardValues And CardValue.Ace) = CardValue.Ace Then
            cards.Add(New TraditionalCard(suit, CardValue.Ace, Me))
        End If

        If (cardValues And CardValue.Two) = CardValue.Two Then
            cards.Add(New TraditionalCard(suit, CardValue.Two, Me))
        End If

        If (cardValues And CardValue.Three) = CardValue.Three Then
            cards.Add(New TraditionalCard(suit, CardValue.Three, Me))
        End If

        If (cardValues And CardValue.Four) = CardValue.Four Then
            cards.Add(New TraditionalCard(suit, CardValue.Four, Me))
        End If

        If (cardValues And CardValue.Five) = CardValue.Five Then
            cards.Add(New TraditionalCard(suit, CardValue.Five, Me))
        End If

        If (cardValues And CardValue.Six) = CardValue.Six Then
            cards.Add(New TraditionalCard(suit, CardValue.Six, Me))
        End If

        If (cardValues And CardValue.Seven) = CardValue.Seven Then
            cards.Add(New TraditionalCard(suit, CardValue.Seven, Me))
        End If

        If (cardValues And CardValue.Eight) = CardValue.Eight Then
            cards.Add(New TraditionalCard(suit, CardValue.Eight, Me))
        End If

        If (cardValues And CardValue.Nine) = CardValue.Nine Then
            cards.Add(New TraditionalCard(suit, CardValue.Nine, Me))
        End If

        If (cardValues And CardValue.Ten) = CardValue.Ten Then
            cards.Add(New TraditionalCard(suit, CardValue.Ten, Me))
        End If

        If (cardValues And CardValue.Jack) = CardValue.Jack Then
            cards.Add(New TraditionalCard(suit, CardValue.Jack, Me))
        End If

        If (cardValues And CardValue.Queen) = CardValue.Queen Then
            cards.Add(New TraditionalCard(suit, CardValue.Queen, Me))
        End If

        If (cardValues And CardValue.King) = CardValue.King Then
            cards.Add(New TraditionalCard(suit, CardValue.King, Me))
        End If
    End Sub
#End Region

#Region "Public Methods"
    ''' <summary>
    ''' Shuffles the cards in the packet by randomly changing card placement.
    ''' </summary>
    Public Sub Shuffle()
        Dim random As New Random
        Dim shuffledDeck As New List(Of TraditionalCard)

        Do While cards.Count > 0
            Dim card As TraditionalCard = cards(random.Next(0, cards.Count))
            cards.Remove(card)
            shuffledDeck.Add(card)
        Loop

        cards = shuffledDeck
    End Sub

    ''' <summary>
    ''' Removes the specified card from the packet. The first matching card
    ''' will be removed.
    ''' </summary>
    ''' <param name="card">The card to remove.</param>
    ''' <returns>The card that was removed from the collection.</returns>
    ''' <remarks>
    ''' Please note that removing a card from a packet may only be performed internally by
    ''' other card-framework classes to maintain the principle that a card may only be held
    ''' by one <see cref="CardPacket"/> only at any given time.
    ''' </remarks>
    Friend Function Remove(ByVal card As TraditionalCard) As TraditionalCard
        If cards.Contains(card) Then
            cards.Remove(card)

            RaiseEvent LostCard(Me, New CardEventArgs With {.Card = card})

            Return card
        End If
        Return Nothing
    End Function


    ''' <summary>
    ''' Removes all the cards from the collection.
    ''' </summary>
    ''' <returns>A list of all the cards that were removed.</returns>
    Friend Function Remove() As List(Of TraditionalCard)
        Dim cards As List(Of TraditionalCard) = Me.cards
        Me.cards = New List(Of TraditionalCard)
        Return cards
    End Function

    ''' <summary>
    ''' Deals the first card from the collection to a specified hand.
    ''' </summary>
    ''' <param name="destinationHand">The destination hand.</param>
    ''' <returns>The card that was moved to the hand.</returns>
    Public Function DealCardToHand(ByVal destinationHand As Hand) As TraditionalCard
        Dim firstCard As TraditionalCard = cards(0)

        firstCard.MoveToHand(destinationHand)

        Return firstCard
    End Function

    ''' <summary>
    ''' Deals several cards to a specified hand.
    ''' </summary>
    ''' <param name="destinationHand">The destination hand.</param>
    ''' <param name="count">The amount of cards to deal.</param>
    ''' <returns>A list of the cards that were moved to the hand.</returns>
    Public Function DealCardsToHand(ByVal destinationHand As Hand, ByVal count As Integer) As List(Of TraditionalCard)
        Dim dealtCards As New List(Of TraditionalCard)

        For cardIndex = 0 To count - 1
            dealtCards.Add(DealCardToHand(destinationHand))
        Next cardIndex

        Return dealtCards
    End Function
#End Region
End Class
