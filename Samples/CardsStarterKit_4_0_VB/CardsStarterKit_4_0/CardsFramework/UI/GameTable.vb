#Region "File Description"
'-----------------------------------------------------------------------------
' GameTable.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
#End Region


''' <summary>
''' The UI representation of the table where the game is played.
''' </summary>
Public Class GameTable
    Inherits DrawableGameComponent
#Region "Fields and Properties and Indexer"
    Private _theme As String
    Public Property Theme As String
        Get
            Return _theme
        End Get
        Private Set(ByVal value As String)
            _theme = value
        End Set
    End Property

    Private _tableTexture As Texture2D
    Public Property TableTexture As Texture2D
        Get
            Return _tableTexture
        End Get
        Private Set(ByVal value As Texture2D)
            _tableTexture = value
        End Set
    End Property

    Private _dealerPosition As Vector2
    Public Property DealerPosition As Vector2
        Get
            Return _dealerPosition
        End Get
        Private Set(ByVal value As Vector2)
            _dealerPosition = value
        End Set
    End Property

    Private _spriteBatch As SpriteBatch
    Public Property SpriteBatch As SpriteBatch
        Get
            Return _spriteBatch
        End Get
        Private Set(ByVal value As SpriteBatch)
            _spriteBatch = value
        End Set
    End Property

    Private _placeOrder As Func(Of Integer, Vector2)
    Public Property PlaceOrder As Func(Of Integer, Vector2)
        Get
            Return _placeOrder
        End Get
        Private Set(ByVal value As Func(Of Integer, Vector2))
            _placeOrder = value
        End Set
    End Property

    Private _tableBounds As Rectangle
    Public Property TableBounds As Rectangle
        Get
            Return _tableBounds
        End Get
        Private Set(ByVal value As Rectangle)
            _tableBounds = value
        End Set
    End Property

    Private _places As Integer
    Public Property Places As Integer
        Get
            Return _places
        End Get
        Private Set(ByVal value As Integer)
            _places = value
        End Set
    End Property

    ''' <summary>
    ''' Returns the player position on the table according to the player index.
    ''' </summary>
    ''' <param name="index">Player's index.</param>
    ''' <returns>The position of the player corrsponding to the 
    ''' supplied index.</returns>
    ''' <remarks>The location's are relative to the entire game area, even
    ''' if the table only occupies part of it.</remarks>
    Default Public ReadOnly Property Item(ByVal index As Integer) As Vector2
        Get
            Return New Vector2(TableBounds.Left, TableBounds.Top) + PlaceOrder(index)
        End Get
    End Property
#End Region

#Region "Initiaizations"
    ''' <summary>
    ''' Initializes a new instance of the class.
    ''' </summary>
    ''' <param name="tableBounds">The table bounds.</param>
    ''' <param name="dealerPosition">The dealer's position.</param>
    ''' <param name="places">Amount of places on the table</param>
    ''' <param name="placeOrder">A method to convert player indices to their
    ''' respective location on the table.</param>
    ''' <param name="theme">The theme used to display UI elements.</param>
    ''' <param name="game">The associated game object.</param>
    Public Sub New(ByVal tableBounds As Rectangle, ByVal dealerPosition As Vector2, ByVal places As Integer, ByVal placeOrder As Func(Of Integer, Vector2), ByVal theme As String, ByVal game As Game)
        MyBase.New(game)
        Me.TableBounds = tableBounds
        Me.DealerPosition = dealerPosition + New Vector2(tableBounds.Left, tableBounds.Top)
        Me.Places = places
        Me.PlaceOrder = placeOrder
        Me.Theme = theme
        SpriteBatch = New SpriteBatch(game.GraphicsDevice)
    End Sub

    ''' <summary>
    ''' Load the table texture.
    ''' </summary>
    Protected Overrides Sub LoadContent()
        Dim assetName As String = String.Format("Images\UI\table")
        TableTexture = Game.Content.Load(Of Texture2D)(assetName)

        MyBase.LoadContent()
    End Sub
#End Region

#Region "Render"
    ''' <summary>
    ''' Render the table.
    ''' </summary>
    ''' <param name="gameTime">Time passed since the last call to 
    ''' this method.</param>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        SpriteBatch.Begin()

        ' Draw the table texture
        SpriteBatch.Draw(TableTexture, TableBounds, Color.White)

        SpriteBatch.End()

        MyBase.Draw(gameTime)
    End Sub
#End Region
End Class
