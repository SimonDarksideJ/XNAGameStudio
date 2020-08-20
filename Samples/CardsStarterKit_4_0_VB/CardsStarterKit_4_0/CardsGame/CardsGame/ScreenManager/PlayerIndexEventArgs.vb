#Region "File Description"
'-----------------------------------------------------------------------------
' PlayerIndexEventArgs.vb
'
' XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Namespace GameStateManagement
	''' <summary>
	''' Custom event argument which includes the index of the player who
	''' triggered the event. This is used by the MenuEntry.Selected event.
	''' </summary>
	Friend Class PlayerIndexEventArgs
		Inherits EventArgs
		''' <summary>
		''' Constructor.
		''' </summary>
		Public Sub New(ByVal playerIndex As PlayerIndex)
            Me._playerIndex = playerIndex
        End Sub


        ''' <summary>
        ''' Gets the index of the player who triggered this event.
        ''' </summary>
        Public ReadOnly Property PlayerIndex As PlayerIndex
            Get
                Return _playerIndex
            End Get
        End Property

        Private _playerIndex As PlayerIndex
	End Class
End Namespace
