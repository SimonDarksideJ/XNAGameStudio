#Region "File Description"
'-----------------------------------------------------------------------------
' BlackJackAnimatedDealerHandComponent.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text
Imports CardsFramework


Public Class BlackjackAnimatedDealerHandComponent
    Inherits AnimatedHandGameComponent
    ''' <summary>
    ''' Creates a new instance of the 
    ''' <see cref="BlackjackAnimatedDealerHandComponent"/> class.
    ''' </summary>
    ''' <param name="place">A number indicating the hand's position on the 
    ''' game table.</param>
    ''' <param name="hand">The dealer's hand.</param>
    ''' <param name="cardGame">The associated game.</param>
    Public Sub New(ByVal place As Integer, ByVal hand As Hand, ByVal cardGame As CardsGame)
        MyBase.New(place, hand, cardGame)
    End Sub

    ''' <summary>
    ''' Gets the position relative to the hand position at which a specific card
    ''' contained in the hand should be rendered.
    ''' </summary>
    ''' <param name="cardLocationInHand">The card's location in the hand (0 is the
    ''' first card in the hand).</param>
    ''' <returns>An offset from the hand's location where the card should be 
    ''' rendered.</returns>
    Public Overrides Function GetCardRelativePosition(ByVal cardLocationInHand As Integer) As Vector2
        Return New Vector2(30 * cardLocationInHand, 0)
    End Function
End Class
