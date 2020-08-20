#Region "File Description"
'-----------------------------------------------------------------------------
' UIUtilty.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
#End Region


Public NotInheritable Class UIUtilty
    ''' <summary>
    ''' Gets the name of a card asset.
    ''' </summary>
    ''' <param name="card">The card type for which to get the asset name.</param>
    ''' <returns>The card's asset name.</returns>
    Public Shared Function GetCardAssetName(ByVal card As TraditionalCard) As String
        Return String.Format("{0}{1}", If((card.Value Or CardValue.FirstJoker) = CardValue.FirstJoker OrElse (card.Value Or CardValue.SecondJoker) = CardValue.SecondJoker, "", card.Type.ToString()), card.Value)
    End Function
End Class
