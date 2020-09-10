#Region "File Description"
'-----------------------------------------------------------------------------
' AnimatedCardsGameComponent.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Microsoft.Xna.Framework
Imports CardsFramework
Imports Microsoft.Xna.Framework.Graphics
#End Region

''' <summary>
''' An <see cref="AnimatedGameComponent"/> implemented for a card game
''' </summary>
Public Class AnimatedCardsGameComponent
    Inherits AnimatedGameComponent
    Private _card As TraditionalCard
    Public Property Card As TraditionalCard
        Get
            Return _card
        End Get
        Private Set(ByVal value As TraditionalCard)
            _card = value
        End Set
    End Property

    ''' <summary>
    ''' Initializes a new instance of the class.
    ''' </summary>
    ''' <param name="card">The card associated with the animation component.</param>
    ''' <param name="cardGame">The associated game.</param>
    Public Sub New(ByVal card As TraditionalCard, ByVal cardGame As CardsGame)
        MyBase.New(cardGame, Nothing)
        Me.Card = card
    End Sub

#Region "Update and Render"
    ''' <summary>
    ''' Updates the component.
    ''' </summary>
    ''' <param name="gameTime">The game time.</param>
    Public Overloads Overrides Sub Update(ByVal gameTime As GameTime)
        MyBase.Update(gameTime)


        CurrentFrame = If(IsFaceDown, CardGame.cardsAssets("CardBack_" & CardGame.Theme), CardGame.cardsAssets(UIUtilty.GetCardAssetName(Card)))
    End Sub

    ''' <summary>
    ''' Draws the component.
    ''' </summary>
    ''' <param name="gameTime">The game time.</param>
    Public Overloads Overrides Sub Draw(ByVal gameTime As GameTime)
        MyBase.Draw(gameTime)

        CardGame.SpriteBatch.Begin()

        ' Draw the current at the designated destination, or at the initial 
        ' position if a destination has not been set
        If CurrentFrame IsNot Nothing Then
            If CurrentDestination.HasValue Then
                CardGame.SpriteBatch.Draw(CurrentFrame, CurrentDestination.Value, Color.White)
            Else
                CardGame.SpriteBatch.Draw(CurrentFrame, CurrentPosition, Color.White)
            End If
        End If

        CardGame.SpriteBatch.End()
    End Sub
#End Region
End Class
