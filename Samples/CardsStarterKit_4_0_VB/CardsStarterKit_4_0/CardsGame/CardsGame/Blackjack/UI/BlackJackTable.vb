#Region "File Description"
'-----------------------------------------------------------------------------
' BlackJackTable.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

Imports System.Text
Imports CardsFramework


Friend Class BlackJackTable
    Inherits GameTable

    Private _ringTexture As Texture2D
    Public Property RingTexture As Texture2D
        Get
            Return _ringTexture
        End Get
        Private Set(ByVal value As Texture2D)
            _ringTexture = value
        End Set
    End Property

    Private _ringOffset As Vector2
    Public Property RingOffset As Vector2
        Get
            Return _ringOffset
        End Get
        Private Set(ByVal value As Vector2)
            _ringOffset = value
        End Set
    End Property


    Public Sub New(ByVal ringOffset As Vector2, ByVal tableBounds As Rectangle, ByVal dealerPosition As Vector2, ByVal places As Integer, ByVal placeOrder As Func(Of Integer, Vector2), ByVal theme As String, ByVal game As Game)
        MyBase.New(tableBounds, dealerPosition, places, placeOrder, theme, game)
        Me.RingOffset = ringOffset
    End Sub

    ''' <summary>
    ''' Load the component assets
    ''' </summary>
    Protected Overrides Sub LoadContent()
        Dim assetName As String = String.Format("Images\UI\ring")
        RingTexture = Game.Content.Load(Of Texture2D)(assetName)

        MyBase.LoadContent()
    End Sub

    ''' <summary>
    ''' Draw the rings of the chip on the table
    ''' </summary>
    ''' <param name="gameTime"></param>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        MyBase.Draw(gameTime)

        SpriteBatch.Begin()

        For placeIndex = 0 To Places - 1
            SpriteBatch.Draw(RingTexture, PlaceOrder(placeIndex) + RingOffset, Color.White)
        Next placeIndex

        SpriteBatch.End()
    End Sub
End Class
