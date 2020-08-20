#Region "File Description"
'-----------------------------------------------------------------------------
' BlackjackGameEventArgs.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports CardsFramework


Public Class BlackjackGameEventArgs
    Inherits EventArgs
    Public Property Player As Player
    Public Property Hand As HandTypes
End Class
