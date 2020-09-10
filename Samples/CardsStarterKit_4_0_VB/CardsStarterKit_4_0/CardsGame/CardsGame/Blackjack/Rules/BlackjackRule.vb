#Region "File Description"
'-----------------------------------------------------------------------------
' BlackjackRule.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text
Imports CardsFramework


''' <summary>
''' Represents a rule which checks if one of the player has achieved "blackjack".
''' </summary>
Public Class BlackJackRule
    Inherits GameRule
    Private players As List(Of BlackjackPlayer)

    ''' <summary>
    ''' Creates a new instance of the <see cref="BlackJackRule"/> class.
    ''' </summary>
    ''' <param name="players">A list of players participating in the game.</param>
    Public Sub New(ByVal players As List(Of Player))
        Me.players = New List(Of BlackjackPlayer)
        For playerIndex = 0 To players.Count - 1
            Me.players.Add(CType(players(playerIndex), BlackjackPlayer))
        Next playerIndex
    End Sub

    ''' <summary>
    ''' Check if any of the players has a hand value of 21 in any of their hands.
    ''' </summary>
    Public Overrides Sub Check()
        For playerIndex = 0 To players.Count - 1
            players(playerIndex).CalculateValues()

            If Not players(playerIndex).BlackJack Then
                ' Check to see if the hand is eligible for a Black Jack
                If ((players(playerIndex).FirstValue = 21) OrElse (players(playerIndex).FirstValueConsiderAce AndAlso players(playerIndex).FirstValue + 10 = 21)) AndAlso players(playerIndex).Hand.Count = 2 Then
                    FireRuleMatch(New BlackjackGameEventArgs With {.Player = players(playerIndex), .Hand = HandTypes.First})
                End If
            End If
            If Not players(playerIndex).SecondBlackJack Then
                ' Check to see if the hand is eligible for a Black Jack
                ' A Black Jack is only eligible with 2 cards in a hand                   
                If (players(playerIndex).IsSplit) AndAlso ((players(playerIndex).SecondValue = 21) OrElse (players(playerIndex).SecondValueConsiderAce AndAlso players(playerIndex).SecondValue + 10 = 21)) AndAlso players(playerIndex).SecondHand.Count = 2 Then
                    FireRuleMatch(New BlackjackGameEventArgs With {.Player = players(playerIndex), .Hand = HandTypes.Second})
                End If
            End If
        Next playerIndex
    End Sub
End Class