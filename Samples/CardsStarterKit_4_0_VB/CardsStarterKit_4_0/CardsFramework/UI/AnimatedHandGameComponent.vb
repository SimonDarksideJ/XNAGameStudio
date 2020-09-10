#Region "File Description"
'-----------------------------------------------------------------------------
' AnimatedHandGameComponent.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports CardsFramework
Imports Microsoft.Xna.Framework
#End Region


Public Class AnimatedHandGameComponent
    Inherits AnimatedGameComponent
#Region "Fields and Properties"
    Private _place As Integer
    Public Property Place As Integer
        Get
            Return _place
        End Get
        Private Set(ByVal value As Integer)
            _place = value
        End Set
    End Property
    Public ReadOnly Hand As Hand

    Private heldAnimatedCards As New List(Of AnimatedCardsGameComponent)

    Public Overloads Overrides ReadOnly Property IsAnimating As Boolean
        Get
            For animationIndex = 0 To heldAnimatedCards.Count - 1
                If heldAnimatedCards(animationIndex).IsAnimating Then
                    Return True
                End If
            Next animationIndex
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Returns the animated cards contained in the hand.
    ''' </summary>
    Public ReadOnly Property AnimatedCards As IEnumerable(Of AnimatedCardsGameComponent)
        Get
            Return heldAnimatedCards.AsReadOnly
        End Get
    End Property
#End Region

#Region "Initiaizations"
    ''' <summary>
    ''' Initializes a new instance of the animated hand component. This means
    ''' setting the hand's position and initializing all animated cards and their
    ''' respective positions. Also, registrations are performed to the associated
    ''' <paramref name="hand"/> events to update the animated hand as cards are
    ''' added or removed.
    ''' </summary>
    ''' <param name="place">The player's place index (-1 for the dealer).</param>
    ''' <param name="hand">The hand represented by this instance.</param>
    ''' <param name="cardGame">The associated card game.</param>
    Public Sub New(ByVal place As Integer, ByVal hand As Hand, ByVal cardGame As CardsGame)
        MyBase.New(cardGame, Nothing)
        Me.Place = place
        Me.Hand = hand
        AddHandler hand.ReceivedCard, AddressOf Hand_ReceivedCard
        AddHandler hand.LostCard, AddressOf Hand_LostCard

        ' Set the component's position
        If place = -1 Then
            CurrentPosition = Me.CardGame.GameTable.DealerPosition
        Else
            CurrentPosition = Me.CardGame.GameTable(place)
        End If

        ' Create and initialize animated cards according to the cards in the 
        ' associated hand
        For cardIndex = 0 To hand.Count - 1
            Dim animatedCardGameComponent As New AnimatedCardsGameComponent(hand(cardIndex), Me.CardGame) With {.CurrentPosition = CurrentPosition + New Vector2(30 * cardIndex, 0)}

            heldAnimatedCards.Add(animatedCardGameComponent)
            Game.Components.Add(animatedCardGameComponent)
        Next cardIndex

        AddHandler Game.Components.ComponentRemoved, AddressOf Components_ComponentRemoved
    End Sub
#End Region

#Region "Update"
    ''' <summary>
    ''' Updates the component.
    ''' </summary>
    ''' <param name="gameTime">The time which elapsed since this method was last
    ''' called.</param>
    Public Overloads Overrides Sub Update(ByVal gameTime As GameTime)
        ' Arrange the hand's animated cards' positions
        For animationIndex = 0 To heldAnimatedCards.Count - 1
            If Not heldAnimatedCards(animationIndex).IsAnimating Then
                heldAnimatedCards(animationIndex).CurrentPosition = CurrentPosition + GetCardRelativePosition(animationIndex)
            End If
        Next animationIndex
        MyBase.Update(gameTime)
    End Sub
#End Region

#Region "Public Methods"
    ''' <summary>
    ''' Gets the card's offset from the hand position according to its index
    ''' in the hand.
    ''' </summary>
    ''' <param name="cardLocationInHand">The card index in the hand.</param>
    ''' <returns></returns>
    Public Overridable Function GetCardRelativePosition(ByVal cardLocationInHand As Integer) As Vector2
        Return Nothing
    End Function

    ''' <summary>
    ''' Finds the index of a specified card in the hand.
    ''' </summary>
    ''' <param name="card">The card to locate.</param>
    ''' <returns>The card's index inside the hand, or -1 if it cannot be
    ''' found.</returns>
    Public Function GetCardLocationInHand(ByVal card As TraditionalCard) As Integer
        For animationIndex = 0 To heldAnimatedCards.Count - 1
            If heldAnimatedCards(animationIndex).Card Is card Then
                Return animationIndex
            End If
        Next animationIndex
        Return -1
    End Function

    ''' <summary>
    ''' Gets the animated game component associated with a specified card.
    ''' </summary>
    ''' <param name="card">The card for which to get the animation 
    ''' component.</param>
    ''' <returns>The card's animation component, or null if such a card cannot
    ''' be found in the hand.</returns>
    Public Function GetCardGameComponent(ByVal card As TraditionalCard) As AnimatedCardsGameComponent
        Dim location As Integer = GetCardLocationInHand(card)
        If location = -1 Then
            Return Nothing
        End If

        Return heldAnimatedCards(location)
    End Function

    ''' <summary>
    ''' Gets the animated game component associated with a specified card.
    ''' </summary>
    ''' <param name="location">The location where the desired card is 
    ''' in the hand.</param>
    ''' <returns>The card's animation component.</returns> 
    Public Function GetCardGameComponent(ByVal location As Integer) As AnimatedCardsGameComponent
        If location = -1 OrElse location >= heldAnimatedCards.Count Then
            Return Nothing
        End If

        Return heldAnimatedCards(location)
    End Function
#End Region

#Region "Event Handlers"
    ''' <summary>
    ''' Handles the ComponentRemoved event of the Components control.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="Microsoft.Xna.Framework.GameComponentCollectionEventArgs"/> 
    ''' instance containing the event data.</param>
    Private Sub Components_ComponentRemoved(ByVal sender As Object, ByVal e As GameComponentCollectionEventArgs)
        If e.GameComponent Is Me Then
            Dispose()
        End If
    End Sub

    ''' <summary>
    ''' Handles the hand's LostCard event be removing the corresponding animated
    ''' card.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="CardsFramework.CardEventArgs"/> 
    ''' instance containing the event data.</param>
    Private Sub Hand_LostCard(ByVal sender As Object, ByVal e As CardEventArgs)
        ' Remove the card from screen
        Dim animationIndex As Integer = 0
        Do While animationIndex < heldAnimatedCards.Count
            If heldAnimatedCards(animationIndex).Card Is e.Card Then
                Game.Components.Remove(heldAnimatedCards(animationIndex))
                heldAnimatedCards.RemoveAt(animationIndex)
                Exit Sub
            End If
            animationIndex += 1
        Loop
    End Sub

    ''' <summary>
    ''' Handles the hand's ReceivedCard event be adding a corresponding 
    ''' animated card.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="CardsFramework.CardEventArgs"/> 
    ''' instance containing the event data.</param>
    Private Sub Hand_ReceivedCard(ByVal sender As Object, ByVal e As CardEventArgs)
        ' Add the card to the screen
        Dim animatedCardGameComponent As New AnimatedCardsGameComponent(e.Card, CardGame) With {.Visible = False}

        heldAnimatedCards.Add(animatedCardGameComponent)
        Game.Components.Add(animatedCardGameComponent)
    End Sub
#End Region

    ''' <summary>
    ''' Calculate the estimated time at which the longest lasting animation currently managed 
    ''' will complete.
    ''' </summary>
    ''' <returns>The estimated time for animation complete </returns>
    Public Overloads Overrides Function EstimatedTimeForAnimationsCompletion() As TimeSpan
        Dim result As TimeSpan = TimeSpan.Zero

        If IsAnimating Then
            For animationIndex = 0 To heldAnimatedCards.Count - 1
                If heldAnimatedCards(animationIndex).EstimatedTimeForAnimationsCompletion() > result Then
                    result = heldAnimatedCards(animationIndex).EstimatedTimeForAnimationsCompletion()
                End If
            Next animationIndex
        End If

        Return result
    End Function

    ''' <summary>
    ''' Properly disposes of the component when it is removed.
    ''' </summary>
    ''' <param name="disposing"></param>
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        ' Remove the registrations to the event to make this 
        ' instance collectable by gc
        RemoveHandler Hand.ReceivedCard, AddressOf Hand_ReceivedCard
        RemoveHandler Hand.LostCard, AddressOf Hand_LostCard

        MyBase.Dispose(disposing)
    End Sub
End Class
