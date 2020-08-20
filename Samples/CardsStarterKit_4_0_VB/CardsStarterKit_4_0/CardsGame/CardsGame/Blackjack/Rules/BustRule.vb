#Region "File Description"
'-----------------------------------------------------------------------------
' BustRule.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text
Imports CardsFramework


''' <summary>
''' Represents a rule which checks if one of the player has gone bust.
''' </summary>
Public Class BustRule
    Inherits GameRule
    Private players As List(Of BlackjackPlayer)

    ''' <summary>
    ''' Creates a new instance of the <see cref="BustRule"/> class.
    ''' </summary>
    ''' <param name="players">A list of players participating in the game.</param>
    Public Sub New(ByVal players As List(Of Player))
        Me.players = New List(Of BlackjackPlayer)
        For playerIndex = 0 To players.Count - 1
            Me.players.Add(CType(players(playerIndex), BlackjackPlayer))
        Next playerIndex
    End Sub

    ''' <summary>
    ''' Check if any of the players has exceeded 21 in any of their hands.
    ''' </summary>
    Public Overrides Sub Check()
        For playerIndex = 0 To players.Count - 1
            players(playerIndex).CalculateValues()

            If Not players(playerIndex).Bust Then
                If (Not players(playerIndex).FirstValueConsiderAce) AndAlso players(playerIndex).FirstValue > 21 Then
                    FireRuleMatch(New BlackjackGameEventArgs With {.Player = players(playerIndex), .Hand = HandTypes.First})
                End If
            End If
            If Not players(playerIndex).SecondBust Then
                If (players(playerIndex).IsSplit AndAlso (Not players(playerIndex).SecondValueConsiderAce) AndAlso players(playerIndex).SecondValue > 21) Then
                    FireRuleMatch(New BlackjackGameEventArgs With {.Player = players(playerIndex), .Hand = HandTypes.Second})
                End If
            End If
        Next playerIndex
    End Sub
End Class

