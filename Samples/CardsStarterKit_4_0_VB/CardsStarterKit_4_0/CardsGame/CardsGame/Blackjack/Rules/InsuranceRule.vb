#Region "File Description"
'-----------------------------------------------------------------------------
' InsuranceRule.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text
Imports CardsFramework


''' <summary>
''' Represents a rule which checks if the human player can use insurance
''' </summary>
Friend Class InsuranceRule
    Inherits GameRule
    Private dealerHand As Hand
    Private done As Boolean = False

    ''' <summary>
    ''' Creates a new instance of the <see cref="InsuranceRule"/> class.
    ''' </summary>
    ''' <param name="dealerHand">The dealer's hand.</param>
    Public Sub New(ByVal dealerHand As Hand)
        Me.dealerHand = dealerHand
    End Sub

    ''' <summary>
    ''' Checks whether or not the dealer's revealed card is an ace.
    ''' </summary>
    Public Overrides Sub Check()
        If Not done Then
            If dealerHand.Count > 0 Then
                If dealerHand(0).Value = CardValue.Ace Then
                    FireRuleMatch(EventArgs.Empty)
                End If
                done = True
            End If
        End If
    End Sub
End Class
