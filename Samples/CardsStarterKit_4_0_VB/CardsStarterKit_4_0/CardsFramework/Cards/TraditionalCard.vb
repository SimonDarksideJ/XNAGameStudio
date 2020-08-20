#Region "File Description"
'-----------------------------------------------------------------------------
' TraditionalCard.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Microsoft.Xna.Framework
#End Region


''' <summary>
''' Enum defining the various types of cards for a traditional-western card-set
''' </summary>
<Flags()>
Public Enum CardSuit
    Heart = &H1
    Diamond = &H2
    Club = &H4
    Spade = &H8
    ' Sets:
    AllSuits = Heart Or Diamond Or Club Or Spade
End Enum

''' <summary>
''' Enum defining the various types of card values for a traditional-western card-set
''' </summary>
<Flags()>
Public Enum CardValue
    Ace = &H1
    Two = &H2
    Three = &H4
    Four = &H8
    Five = &H10
    Six = &H20
    Seven = &H40
    Eight = &H80
    Nine = &H100
    Ten = &H200
    Jack = &H400
    Queen = &H800
    King = &H1000
    FirstJoker = &H2000
    SecondJoker = &H4000
    ' Sets:
    AllNumbers = &H3FF
    NonJokers = &H1FFF
    Jokers = FirstJoker Or SecondJoker
    AllFigures = Jack Or Queen Or King
End Enum

''' <summary>
''' Traditional-western card
''' </summary>
''' <remarks>
''' Each card has a defined <see cref="CardSuit">Type</see> and <see cref="CardValue">Value</see>
''' as well as the <see cref="CardPacket"/> in which it is being held.
''' A card may not be held in more than one <see cref="CardPacket"/>. This is achived by enforcing any card transfer
''' operation between CarkPackets and <see cref="Hand"/>s to be performed only from within the card's 
''' MoveToHand method only. This method accesses <c>internal</c> <see cref="Hand.Add"/> method and 
''' <see cref="CardPacket.Remove"/> method accordingly to complete the card transfer operation.
''' </remarks>
Public Class TraditionalCard
#Region "Properties"
    Public Property Type As CardSuit
    Public Property Value As CardValue
    Public HoldingCardCollection As CardPacket
#End Region

#Region "Initiaizations"
    ''' <summary>
    ''' Initializes a new instance of the <see cref="TraditionalCard"/> class.
    ''' </summary>
    ''' <param name="type">The card suit. Supports only a single value.</param>
    ''' <param name="value">The card's value. Only single values are 
    ''' supported.</param>
    ''' <param name="holdingCardCollection">The holding card collection.</param>
    Friend Sub New(ByVal type As CardSuit, ByVal value As CardValue, ByVal holdingCardCollection As CardPacket)
        ' Check for single type
        Select Case type
            Case CardSuit.Club, CardSuit.Diamond, CardSuit.Heart, CardSuit.Spade
            Case Else
                Throw New ArgumentException("type must be single value", "type")
        End Select

        ' Check for single value
        Select Case value
            Case CardValue.Ace, CardValue.Two, CardValue.Three, CardValue.Four, CardValue.Five, CardValue.Six, CardValue.Seven, CardValue.Eight, CardValue.Nine, CardValue.Ten, CardValue.Jack, CardValue.Queen, CardValue.King, CardValue.FirstJoker, CardValue.SecondJoker
            Case Else
                Throw New ArgumentException("value must be single value", "value")
        End Select

        Me.Type = type
        Me.Value = value
        Me.HoldingCardCollection = holdingCardCollection
    End Sub
#End Region

    ''' <summary>
    ''' Moves the card from its current <see cref="CardPacket"/> to the specified <paramref name="hand"/>. 
    ''' This method of operation prevents any one card instance from being held by more than one
    ''' <see cref="CardPacket"/> at the same time.
    ''' </summary>
    ''' <param name="hand">The receiving hand.</param>
    Public Sub MoveToHand(ByVal hand As Hand)
        HoldingCardCollection.Remove(Me)
        HoldingCardCollection = hand
        hand.Add(Me)
    End Sub
End Class
