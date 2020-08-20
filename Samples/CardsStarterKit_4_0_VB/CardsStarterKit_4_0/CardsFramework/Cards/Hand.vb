#Region "File Description"
'-----------------------------------------------------------------------------
' Hand.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
#End Region


''' <summary>
''' Represents a hand of cards held by a player, dealer or the game table
''' </summary>
''' <remarks>
''' A <see cref="Hand"/> is a type of <see cref="CardPacket"/> that may also
''' receive Card items, as well as loose them.
''' Therefore, it may receive Card items from any 
''' <see cref="CardPacket"/> or from another <see cref="Hand"/>. 
''' </remarks>
Public Class Hand
    Inherits CardPacket
    ''' <summary>
    ''' An event which triggers when a card is added to the hand.
    ''' </summary>
    Public Event ReceivedCard As EventHandler(Of CardEventArgs)

#Region "Internal Methods"
    ''' <summary>
    ''' Adds the specified card to the hand
    ''' </summary>
    ''' <param name="card">The card to add to the hand. The card will be added
    ''' as the last card of the hand.</param>
    Friend Sub Add(ByVal card As TraditionalCard)
        cards.Add(card)
        RaiseEvent ReceivedCard(Me, New CardEventArgs With {.Card = card})
    End Sub

    ''' <summary>
    ''' Adds the specified cards to the hand
    ''' </summary>
    ''' <param name="cards">The cards to add to the hand. The cards are added
    ''' as the last cards of the hand.</param>
    Friend Sub Add(ByVal cards As IEnumerable(Of TraditionalCard))
        Me.cards.AddRange(cards)
    End Sub
#End Region
End Class
