#Region "File Description"
'-----------------------------------------------------------------------------
' BlackjackAIPlayer.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text
Imports CardsFramework


Friend Class BlackjackAIPlayer
    Inherits BlackjackPlayer
#Region "Fields"
    Private Shared random As New Random

    Public Event Hit As EventHandler
    Public Event Stand As EventHandler
#End Region

    ''' <summary>
    ''' Creates a new instance of the <see cref="BlackjackAIPlayer"/> class.
    ''' </summary>
    ''' <param name="name">The name.</param>
    ''' <param name="game">The game.</param>
    Public Sub New(ByVal name As String, ByVal game As CardsGame)
        MyBase.New(name, game)
    End Sub

#Region "Pulic Methods"
    ''' <summary>
    ''' Performs a move during a round.
    ''' </summary>
    Public Sub AIPlay()
        Dim value As Integer = FirstValue
        If FirstValueConsiderAce AndAlso value + 10 <= 21 Then
            value += 10
        End If

        If value < 17 Then
            RaiseEvent Hit(Me, EventArgs.Empty)
        ElseIf StandEvent IsNot Nothing Then
            RaiseEvent Stand(Me, EventArgs.Empty)
        End If
    End Sub

    ''' <summary>
    ''' Returns the amount which the AI player decides to bet.
    ''' </summary>
    ''' <returns>The AI player's bet.</returns>
    Public Function AIBet() As Integer
        Dim chips() As Integer = {0, 5, 25, 100, 500}
        Dim bet As Integer = chips(random.Next(0, chips.Length))

        If bet < Balance Then
            Return bet
        End If

        Return 0
    End Function
#End Region
End Class
