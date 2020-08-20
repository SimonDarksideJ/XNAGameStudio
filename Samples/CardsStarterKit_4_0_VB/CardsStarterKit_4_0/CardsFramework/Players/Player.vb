#Region "File Description"
'-----------------------------------------------------------------------------
' Player.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
#End Region


''' <summary>
''' Represents base player to be extended by inheritance for each
''' card game.
''' </summary>
Public Class Player
#Region "Property"
    Public Property Name As String
    Public Property Game As CardsGame
    Public Property Hand As Hand
#End Region

    Public Sub New(ByVal name As String, ByVal game As CardsGame)
        Me.Name = name
        Me.Game = game
        Hand = New Hand
    End Sub
End Class
