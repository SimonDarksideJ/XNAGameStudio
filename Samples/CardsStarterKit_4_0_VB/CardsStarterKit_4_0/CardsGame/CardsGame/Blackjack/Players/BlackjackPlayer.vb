#Region "File Description"
'-----------------------------------------------------------------------------
' BlackjackPlayer.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text
Imports CardsFramework


''' <summary>
''' Depicts hands the player can interact with.
''' </summary>
Public Enum HandTypes
    First
    Second
End Enum

Public Class BlackjackPlayer
    Inherits Player
#Region "Fields/Properties"
    ' Various fields which depict the state of the players two hands

    Private _firstValue As Integer

    Private _firstValueConsiderAce As Boolean

    Private _secondValue As Integer

    Private _secondValueConsiderAce As Boolean

    Public Property Bust As Boolean
    Public Property SecondBust As Boolean
    Public Property BlackJack As Boolean
    Public Property SecondBlackJack As Boolean
    Public Property [Double] As Boolean
    Public Property SecondDouble As Boolean

    Public Property IsSplit As Boolean

    Private _secondHand As Hand
    Public Property SecondHand As Hand
        Get
            Return _secondHand
        End Get
        Private Set(ByVal value As Hand)
            _secondHand = value
        End Set
    End Property

    ''' <summary>
    ''' The type of hand that the player is currently interacting with.
    ''' </summary>
    Public Property CurrentHandType As HandTypes

    ''' <summary>
    ''' Returns the hand that the player is currently interacting with.
    ''' </summary>
    Public ReadOnly Property CurrentHand As Hand
        Get
            Select Case CurrentHandType
                Case HandTypes.First
                    Return Hand
                Case HandTypes.Second
                    Return SecondHand
                Case Else
                    Throw New Exception("No hand to return")
            End Select
        End Get
    End Property

    Public ReadOnly Property FirstValue As Integer
        Get
            Return _firstValue
        End Get
    End Property
    Public ReadOnly Property FirstValueConsiderAce As Boolean
        Get
            Return _firstValueConsiderAce
        End Get
    End Property

    Public ReadOnly Property SecondValue As Integer
        Get
            Return _secondValue
        End Get
    End Property
    Public ReadOnly Property SecondValueConsiderAce As Boolean
        Get
            Return _secondValueConsiderAce
        End Get
    End Property

    Public ReadOnly Property MadeBet As Boolean
        Get
            Return BetAmount > 0
        End Get
    End Property

    Public Property IsDoneBetting As Boolean

    Public Property Balance As Single

    Private _betAmount As Single
    Public Property BetAmount As Single
        Get
            Return _betAmount
        End Get
        Private Set(ByVal value As Single)
            _betAmount = value
        End Set
    End Property
    Public Property IsInsurance As Boolean
#End Region

    ''' <summary>
    ''' Creates a new blackjack player instance.
    ''' </summary>
    ''' <param name="name">The player's name.</param>
    ''' <param name="game">The game associated with the player.</param>
    Public Sub New(ByVal name As String, ByVal game As CardsFramework.CardsGame)
        MyBase.New(name, game)
        Balance = 500
        CurrentHandType = HandTypes.First
    End Sub

    ''' <summary>
    ''' Calculates the value represented by a specified hand.
    ''' </summary>
    ''' <param name="hand">The hand for which to calculate the value.</param>
    ''' <param name="game">The associated game.</param>
    ''' <param name="value">Will contain the hand's value. If the hand has two
    ''' possible values due to it containing an ace, this will be the lower
    ''' value.</param>
    ''' <param name="considerAce">Whether or not an ace can be considered to
    ''' make the hand have an alternative value.</param>
    Private Shared Sub CalulateValue(ByVal hand As Hand, ByVal game As CardsFramework.CardsGame, <System.Runtime.InteropServices.Out()> ByRef value As Integer, <System.Runtime.InteropServices.Out()> ByRef considerAce As Boolean)
        value = 0
        considerAce = False

        For cardIndex = 0 To hand.Count - 1
            value += game.CardValue(hand(cardIndex))

            If hand(cardIndex).Value = CardValue.Ace Then
                considerAce = True
            End If
        Next cardIndex

        If considerAce AndAlso value + 10 > 21 Then
            considerAce = False
        End If
    End Sub

#Region "Public Methods"
    ''' <summary>
    ''' Bets a specified amount of money, if the player's balance permits it.
    ''' </summary>
    ''' <param name="amount">The amount to bet.</param>
    ''' <returns>True if the player has enough money to perform the bet, false
    ''' otherwise.</returns>
    ''' <remarks>The player's bet amount and balance are only updated if this
    ''' method returns true.</remarks>
    Public Function Bet(ByVal amount As Single) As Boolean
        If amount > Balance Then
            Return False
        End If
        BetAmount += amount
        Balance -= amount
        Return True
    End Function

    ''' <summary>
    ''' Resets the player's bet to 0, returning the current bet amount to 
    ''' the player's balance.
    ''' </summary>
    Public Sub ClearBet()
        Balance += BetAmount
        BetAmount = 0
    End Sub

    ''' <summary>
    ''' Calculates the values of the player's two hands.
    ''' </summary>
    Public Sub CalculateValues()
        CalulateValue(Hand, Game, _firstValue, _firstValueConsiderAce)

        If SecondHand IsNot Nothing Then
            CalulateValue(SecondHand, Game, _secondValue, _secondValueConsiderAce)
        End If
    End Sub

    ''' <summary>
    ''' Reset's the player's various state fields.
    ''' </summary>
    Public Sub ResetValues()
        BlackJack = False
        SecondBlackJack = False
        Bust = False
        SecondBust = False
        [Double] = False
        SecondDouble = False
        _firstValue = 0
        _firstValueConsiderAce = False
        IsSplit = False
        _secondValue = 0
        _secondValueConsiderAce = False
        BetAmount = 0
        IsDoneBetting = False
        IsInsurance = False
        CurrentHandType = HandTypes.First
    End Sub

    ''' <summary>
    ''' Initializes the player's second hand.
    ''' </summary>
    Public Sub InitializeSecondHand()
        SecondHand = New Hand
    End Sub

    ''' <summary>
    ''' Splits the player's current hand into two hands as per the blackjack rules.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">Thrown if performing a split
    ''' is not legal for the current player status.</exception>
    Public Sub SplitHand()
        If SecondHand Is Nothing Then
            Throw New InvalidOperationException("Second hand is not initialized.")
        End If

        If IsSplit = True Then
            Throw New InvalidOperationException("A hand cannot be split more than once.")
        End If

        If Hand.Count <> 2 Then
            Throw New InvalidOperationException("You must have two cards to perform a split.")
        End If

        If Hand(0).Value <> Hand(1).Value Then
            Throw New InvalidOperationException("You can only split when both cards are of identical value.")
        End If

        IsSplit = True

        ' Move the top card in the first hand to the second hand
        Hand(1).MoveToHand(SecondHand)
    End Sub
#End Region
End Class
