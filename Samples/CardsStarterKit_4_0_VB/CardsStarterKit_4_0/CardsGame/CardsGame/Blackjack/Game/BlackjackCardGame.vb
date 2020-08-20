#Region "File Description"
'-----------------------------------------------------------------------------
' BlackjackGame.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports CardsFramework
Imports System.Threading
Imports Blackjack.GameStateManagement
Imports System.Reflection
#End Region


''' <summary>
''' The various possible game states.
''' </summary>
Public Enum BlackjackGameState
    Shuffling
    Betting
    Playing
    Dealing
    RoundEnd
    GameOver
End Enum

Friend Class BlackjackCardGame
    Inherits CardsGame
#Region "Fields and Properties"
    Private playerHandValueTexts As New Dictionary(Of Player, String)
    Private playerSecondHandValueTexts As New Dictionary(Of Player, String)
    ' Stores used cards
    Private deadCards As New Hand
    Private dealerPlayer As BlackjackPlayer
    Private turnFinishedByPlayer() As Boolean
    Private dealDuration As TimeSpan = TimeSpan.FromMilliseconds(500)

    Private animatedHands() As AnimatedHandGameComponent
    ' An additional list for managing hands created when performing a split.
    Private animatedSecondHands() As AnimatedHandGameComponent

    Private betGameComponent As BetGameComponent
    Private dealerHandComponent As AnimatedHandGameComponent
    Private buttons As New Dictionary(Of String, Button)
    Private newGame As Button
    Private showInsurance As Boolean

    ' An offset used for drawing the second hand which appears after a split in
    ' the correct location.
    Private secondHandOffset As New Vector2(100 * BlackJackGame.WidthScale, 25 * BlackJackGame.HeightScale)
    Private Shared ringOffset As New Vector2(0, 110)

#If WINDOWS_PHONE Then
    Private frameSize As New Vector2(162, 162)
#Else
		Private frameSize As New Vector2(180, 180)
#End If

    Public Property State As BlackjackGameState
    Private screenManager As ScreenManager

    Private Const maxPlayers As Integer = 3
    Private Const minPlayers As Integer = 1
#End Region

#Region "Initializations"
    ''' <summary>
    ''' Creates a new instance of the <see cref="BlackjackCardGame"/> class.
    ''' </summary>
    ''' <param name="tableBounds">The table bounds. These serves as the bounds for 
    ''' the game's main area.</param>
    ''' <param name="dealerPosition">Position for the dealer's deck.</param>
    ''' <param name="placeOrder">A method that translate a player index into the
    ''' position of his deck on the game table.</param>
    ''' <param name="screenManager">The games <see cref="ScreenManager"/>.</param>
    ''' <param name="theme">The game's deck theme name.</param>
    Public Sub New(ByVal tableBounds As Rectangle, ByVal dealerPosition As Vector2, ByVal placeOrder As Func(Of Integer, Vector2), ByVal screenManager As ScreenManager, ByVal theme As String)
        MyBase.New(6, 0, CardSuit.AllSuits, CardsFramework.CardValue.NonJokers, minPlayers, maxPlayers, New BlackJackTable(ringOffset, tableBounds, dealerPosition, maxPlayers, placeOrder, theme, screenManager.Game), theme, screenManager.Game)
        dealerPlayer = New BlackjackPlayer("Dealer", Me)
        turnFinishedByPlayer = New Boolean(MaximumPlayers - 1) {}
        Me.screenManager = screenManager

        If animatedHands Is Nothing Then
            animatedHands = New AnimatedHandGameComponent(maxPlayers - 1) {}
        End If
        If animatedSecondHands Is Nothing Then
            animatedSecondHands = New AnimatedHandGameComponent(maxPlayers - 1) {}
        End If
    End Sub

    ''' <summary>
    ''' Performs necessary initializations.
    ''' </summary>
    Public Sub Initialize()
        MyBase.LoadContent()
        ' Initialize a new bet component
        betGameComponent = New BetGameComponent(players, screenManager.input, Theme, Me)
        Game.Components.Add(betGameComponent)

        ' Initialize the game buttons
        Dim buttonsText() As String = {"Hit", "Stand", "Double", "Split", "Insurance"}
        For buttonIndex = 0 To buttonsText.Length - 1
            Dim button As New Button("ButtonRegular", "ButtonPressed", screenManager.input, Me) With {.Text = buttonsText(buttonIndex), .Bounds = New Rectangle(screenManager.SafeArea.Left + 10 + buttonIndex * 110, screenManager.SafeArea.Bottom - 60, 100, 50), .Font = Me.Font, .Visible = False, .Enabled = False}
            buttons.Add(buttonsText(buttonIndex), button)
            Game.Components.Add(button)
        Next buttonIndex

        newGame = New Button("ButtonRegular", "ButtonPressed", screenManager.input, Me) With {.Text = "New Hand", .Bounds = New Rectangle(screenManager.SafeArea.Left + 10, screenManager.SafeArea.Bottom - 60, 200, 50), .Font = Me.Font, .Visible = False, .Enabled = False}

        ' Alter the insurance button's bounds as it is considerably larger than
        ' the other buttons
        Dim insuranceBounds As Rectangle = buttons("Insurance").Bounds
        insuranceBounds.Width = 200
        buttons("Insurance").Bounds = insuranceBounds

        AddHandler newGame.Click, AddressOf newGame_Click
        Game.Components.Add(newGame)

        ' Register to click event
        AddHandler buttons("Hit").Click, AddressOf Hit_Click
        AddHandler buttons("Stand").Click, AddressOf Stand_Click
        AddHandler buttons("Double").Click, AddressOf Double_Click
        AddHandler buttons("Split").Click, AddressOf Split_Click
        AddHandler buttons("Insurance").Click, AddressOf Insurance_Click
    End Sub
#End Region

#Region "Update and Render"
    ''' <summary>
    ''' Perform the game's update logic.
    ''' </summary>
    ''' <param name="gameTime">Time elapsed since the last call to 
    ''' this method.</param>
    Public Sub Update(ByVal gameTime As GameTime)
        Select Case State
            Case BlackjackGameState.Shuffling
                ShowShuffleAnimation()
            Case BlackjackGameState.Betting
                EnableButtons(False)
            Case BlackjackGameState.Dealing
                ' Deal 2 cards and start playing
                State = BlackjackGameState.Playing
                Deal()
                StartPlaying()
            Case BlackjackGameState.Playing
                ' Calculate players' current hand values
                For playerIndex = 0 To players.Count - 1
                    CType(players(playerIndex), BlackjackPlayer).CalculateValues()
                Next playerIndex
                dealerPlayer.CalculateValues()

                ' Make sure no animations are running
                If Not CheckForRunningAnimations(Of AnimatedCardsGameComponent)() Then
                    Dim player As BlackjackPlayer = CType(GetCurrentPlayer(), BlackjackPlayer)
                    ' If the current player is an AI player, make it play
                    If TypeOf player Is BlackjackAIPlayer Then
                        CType(player, BlackjackAIPlayer).AIPlay()
                    End If

                    CheckRules()

                    ' If all players have finished playing, the 
                    ' current round ends
                    If State = BlackjackGameState.Playing AndAlso GetCurrentPlayer() Is Nothing Then
                        EndRound()
                    End If

                    ' Update button availability according to player options
                    SetButtonAvailability()
                Else
                    EnableButtons(False)
                End If
            Case BlackjackGameState.RoundEnd
                If dealerHandComponent.EstimatedTimeForAnimationsCompletion = TimeSpan.Zero Then
                    betGameComponent.CalculateBalance(dealerPlayer)
                    ' Check if there is enough money to play
                    ' then show new game option or tell the player he has lost
                    If (CType(players(0), BlackjackPlayer)).Balance < 5 Then
                        EndGame()
                    Else
                        newGame.Enabled = True
                        newGame.Visible = True
                    End If
                End If
            Case BlackjackGameState.GameOver

            Case Else
        End Select
    End Sub

    ''' <summary>
    ''' Shows the card shuffling animation.
    ''' </summary>
    Private Sub ShowShuffleAnimation()
        ' Add shuffling animation
        Dim animationComponent As New AnimatedGameComponent(Me.Game) With {.CurrentPosition = GameTable.DealerPosition, .Visible = False}
        Game.Components.Add(animationComponent)

        animationComponent.AddAnimation(New FramesetGameComponentAnimation(cardsAssets("shuffle_" & Theme), 32, 11, frameSize) With {.Duration = TimeSpan.FromSeconds(1.5F), .PerformBeforeStart = AddressOf ShowComponent, .PerformBeforSartArgs = animationComponent, .PerformWhenDone = AddressOf PlayShuffleAndRemoveComponent, .PerformWhenDoneArgs = animationComponent})
        State = BlackjackGameState.Betting
    End Sub

    ''' <summary>
    ''' Helper method to show component
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub ShowComponent(ByVal obj As Object)
        CType(obj, AnimatedGameComponent).Visible = True
    End Sub

    ''' <summary>
    ''' Helper method to play shuffle sound and remove component
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub PlayShuffleAndRemoveComponent(ByVal obj As Object)
        AudioManager.PlaySound("Shuffle")
        Game.Components.Remove(CType(obj, AnimatedGameComponent))
    End Sub

    ''' <summary>
    ''' Renders the visual elements for which the game itself is responsible.
    ''' </summary>
    ''' <param name="gameTime">Time passed since the last call to 
    ''' this method.</param>
    Public Sub Draw(ByVal gameTime As GameTime)
        SpriteBatch.Begin()

        Select Case State
            Case BlackjackGameState.Playing
                ShowPlayerValues()
            Case BlackjackGameState.GameOver
            Case BlackjackGameState.RoundEnd
                If dealerHandComponent.EstimatedTimeForAnimationsCompletion = TimeSpan.Zero Then
                    ShowDealerValue()
                End If
                ShowPlayerValues()
            Case Else
        End Select

        SpriteBatch.End()
    End Sub

    ''' <summary>
    ''' Draws the dealer's hand value on the screen.
    ''' </summary>
    Private Sub ShowDealerValue()
        ' Calculate the value to display
        Dim dealerValue As String = dealerPlayer.FirstValue.ToString
        If dealerPlayer.FirstValueConsiderAce Then
            If dealerPlayer.FirstValue + 10 = 21 Then
                dealerValue = "21"
            Else
                dealerValue &= "\" & (dealerPlayer.FirstValue + 10).ToString
            End If
        End If

        ' Draw the value
        Dim measure As Vector2 = Font.MeasureString(dealerValue)
        Dim position As Vector2 = GameTable.DealerPosition - New Vector2(measure.X + 20, 0)

        SpriteBatch.Draw(screenManager.BlankTexture, New Rectangle(CInt(Fix(position.X)) - 4, CInt(Fix(position.Y)), CInt(Fix(measure.X)) + 8, CInt(Fix(measure.Y))), Color.Black)

        SpriteBatch.DrawString(Font, dealerValue, position, Color.White)
    End Sub

    ''' <summary>
    ''' Draws the players' hand value on the screen.
    ''' </summary>
    Private Sub ShowPlayerValues()
        Dim color As Color = color.Black
        Dim currentPlayer As Player = GetCurrentPlayer()

        For playerIndex = 0 To players.Count - 1
            Dim player As BlackjackPlayer = CType(players(playerIndex), BlackjackPlayer)
            ' The current player's hand value will be red to serve as a visual
            ' prompt for who the active player is
            If player Is currentPlayer Then
                color = color.Red
            Else
                color = color.White
            End If

            ' Calculate the values to draw
            Dim playerHandValueText As String = String.Empty
            Dim playerSecondHandValueText As String = String.Empty
            If Not animatedHands(playerIndex).IsAnimating Then
                If player.FirstValue > 0 Then
                    playerHandValueText = player.FirstValue.ToString
                    ' Take the fact that an ace is wither 1 or 11 into 
                    ' consideration when calculating the value to display
                    ' Since the ace already counts as 1, we add 10 to get
                    ' the alternate value
                    If player.FirstValueConsiderAce Then
                        If player.FirstValue + 10 = 21 Then
                            playerHandValueText = "21"
                        Else
                            playerHandValueText &= "\" & (player.FirstValue + 10).ToString()
                        End If
                    End If
                    playerHandValueTexts(player) = playerHandValueText
                Else
                    playerHandValueText = Nothing
                End If

                If player.IsSplit Then
                    ' If the player has performed a split, he has an additional
                    ' hand with its own value
                    If player.SecondValue > 0 Then
                        playerSecondHandValueText = player.SecondValue.ToString
                        If player.SecondValueConsiderAce Then
                            If player.SecondValue + 10 = 21 Then
                                playerSecondHandValueText = "21"
                            Else
                                playerSecondHandValueText &= "\" & (player.SecondValue + 10).ToString()
                            End If
                        End If
                        playerSecondHandValueTexts(player) = playerSecondHandValueText
                    Else
                        playerSecondHandValueText = Nothing
                    End If
                End If
            Else
                playerHandValueTexts.TryGetValue(player, playerHandValueText)
                playerSecondHandValueTexts.TryGetValue(player, playerSecondHandValueText)
            End If

            If player.IsSplit Then
                ' If the player has performed a split, mark the active hand alone
                ' with a red value
                color = If(player.CurrentHandType = HandTypes.First AndAlso player Is currentPlayer, color.Red, color.White)

                If playerHandValueText IsNot Nothing Then
                    DrawValue(animatedHands(playerIndex), playerIndex, playerHandValueText, color)
                End If

                color = If(player.CurrentHandType = HandTypes.Second AndAlso player Is currentPlayer, color.Red, color.White)

                If playerSecondHandValueText IsNot Nothing Then
                    DrawValue(animatedSecondHands(playerIndex), playerIndex, playerSecondHandValueText, color)
                End If
            Else
                ' If there is a value to draw, draw it
                If playerHandValueText IsNot Nothing Then
                    DrawValue(animatedHands(playerIndex), playerIndex, playerHandValueText, color)
                End If
            End If
        Next playerIndex
    End Sub

    ''' <summary>
    ''' Draws the value of a player's hand above his top card.
    ''' The value will be drawn over a black background.
    ''' </summary>
    ''' <param name="animatedHand">The player's hand.</param>
    ''' <param name="place">A number representing the player's position on the
    ''' game table.</param>
    ''' <param name="value">The value to draw.</param>
    ''' <param name="valueColor">The color in which to draw the value.</param>
    Private Sub DrawValue(ByVal animatedHand As AnimatedHandGameComponent, ByVal place As Integer, ByVal value As String, ByVal valueColor As Color)
        Dim hand As Hand = animatedHand.Hand

        Dim position As Vector2 = GameTable.PlaceOrder(place) + animatedHand.GetCardRelativePosition(hand.Count - 1)
        Dim measure As Vector2 = Font.MeasureString(value)

        position.X += (cardsAssets("CardBack_" & Theme).Bounds.Width - measure.X) / 2
        position.Y -= measure.Y + 5

        SpriteBatch.Draw(screenManager.BlankTexture, New Rectangle(CInt(Fix(position.X)) - 4, CInt(Fix(position.Y)), CInt(Fix(measure.X)) + 8, CInt(Fix(measure.Y))), Color.Black)
        SpriteBatch.DrawString(Font, value, position, valueColor)

    End Sub
#End Region

#Region "Override Methods"
    ''' <summary>
    ''' Adds a player to the game.
    ''' </summary>
    ''' <param name="player">The player to add.</param>
    Public Overrides Sub AddPlayer(ByVal player As Player)
        If TypeOf player Is BlackjackPlayer AndAlso players.Count < MaximumPlayers Then
            players.Add(player)
        End If
    End Sub

    ''' <summary>
    ''' Gets the active player.
    ''' </summary>
    ''' <returns>The first payer who has placed a bet and has not 
    ''' finish playing.</returns>
    Public Overrides Function GetCurrentPlayer() As Player
        For playerIndex = 0 To players.Count - 1
            If (CType(players(playerIndex), BlackjackPlayer)).MadeBet AndAlso turnFinishedByPlayer(playerIndex) = False Then
                Return players(playerIndex)
            End If
        Next playerIndex
        Return Nothing
    End Function

    ''' <summary>
    ''' Calculate the value of a blackjack card.
    ''' </summary>
    ''' <param name="card">The card to calculate the value for.</param>
    ''' <returns>The card's value. All card values are equal to their face number,
    ''' except for jack/queen/king which value at 10.</returns>
    ''' <remarks>An ace's value will be 1. Game logic will treat it as 11 where
    ''' appropriate.</remarks>
    Public Overrides Function CardValue(ByVal card As TraditionalCard) As Integer
        Return Math.Min(MyBase.CardValue(card), 10)
    End Function

    ''' <summary>
    ''' Deals 2 cards to each player including the dealer and adds the appropriate 
    ''' animations.
    ''' </summary>
    Public Overrides Sub Deal()
        If State = BlackjackGameState.Playing Then
            Dim card As TraditionalCard
            For dealIndex = 0 To 1
                For playerIndex = 0 To players.Count - 1
                    If (CType(players(playerIndex), BlackjackPlayer)).MadeBet Then
                        ' Deal a card to one of the players
                        card = dealer.DealCardToHand(players(playerIndex).Hand)

                        AddDealAnimation(card, animatedHands(playerIndex), True, dealDuration, Date.Now + TimeSpan.FromSeconds(dealDuration.TotalSeconds * (dealIndex * players.Count + playerIndex)))
                    End If
                Next playerIndex
                ' Deal a card to the dealer
                card = dealer.DealCardToHand(dealerPlayer.Hand)
                AddDealAnimation(card, dealerHandComponent, dealIndex = 0, dealDuration, Date.Now)
            Next dealIndex
        End If
    End Sub

    ''' <summary>
    ''' Performs necessary initializations needed after dealing the cards in order
    ''' to start playing.
    ''' </summary>
    Public Overrides Sub StartPlaying()
        ' Check that there are enough players to start playing
        If (MinimumPlayers <= players.Count AndAlso players.Count <= MaximumPlayers) Then
            ' Set up and register to gameplay events

            Dim gameRule As GameRule = New BustRule(players)
            rules.Add(gameRule)
            AddHandler gameRule.RuleMatch, AddressOf BustGameRule

            gameRule = New BlackJackRule(players)
            rules.Add(gameRule)
            AddHandler gameRule.RuleMatch, AddressOf BlackJackGameRule

            gameRule = New InsuranceRule(dealerPlayer.Hand)
            rules.Add(gameRule)
            AddHandler gameRule.RuleMatch, AddressOf InsuranceGameRule

            ' Display the hands participating in the game
            For playerIndex = 0 To players.Count - 1
                If (CType(players(playerIndex), BlackjackPlayer)).MadeBet Then
                    animatedHands(playerIndex).Visible = False
                Else
                    animatedHands(playerIndex).Visible = True
                End If
            Next playerIndex
        End If
    End Sub
#End Region

#Region "Helper Methods"
    ''' <summary>
    ''' Display an animation when a card is dealt.
    ''' </summary>
    ''' <param name="card">The card being dealt.</param>
    ''' <param name="animatedHand">The animated hand into which the card 
    ''' is dealt.</param>
    ''' <param name="flipCard">Should the card be flipped after dealing it.</param>
    ''' <param name="duration">The animations desired duration.</param>
    ''' <param name="startTime">The time at which the animation should 
    ''' start.</param>
    Public Sub AddDealAnimation(ByVal card As TraditionalCard, ByVal animatedHand As AnimatedHandGameComponent, ByVal flipCard As Boolean, ByVal duration As TimeSpan, ByVal startTime As Date)
        ' Get the card location and card component
        Dim cardLocationInHand As Integer = animatedHand.GetCardLocationInHand(card)
        Dim cardComponent As AnimatedCardsGameComponent = animatedHand.GetCardGameComponent(cardLocationInHand)

        ' Add the transition animation
        cardComponent.AddAnimation(New TransitionGameComponentAnimation(GameTable.DealerPosition, animatedHand.CurrentPosition + animatedHand.GetCardRelativePosition(cardLocationInHand)) With {.StartTime = startTime, .PerformBeforeStart = AddressOf ShowComponent, .PerformBeforSartArgs = cardComponent, .PerformWhenDone = AddressOf PlayDealSound})

        If flipCard Then
            ' Add the flip animation
            cardComponent.AddAnimation(New FlipGameComponentAnimation With {.IsFromFaceDownToFaceUp = True, .Duration = duration, .StartTime = startTime.Add(duration), .PerformWhenDone = AddressOf PlayFlipSound})
        End If
    End Sub

    ''' <summary>
    ''' Helper method to play deal sound
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub PlayDealSound(ByVal obj As Object)
        AudioManager.PlaySound("Deal")
    End Sub

    ''' <summary>
    ''' Helper method to play flip sound
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub PlayFlipSound(ByVal obj As Object)
        AudioManager.PlaySound("Flip")
    End Sub

    ''' <summary>
    ''' Adds an animation which displays an asset over a player's hand. The asset
    ''' will appear above the hand and appear to "fall" on top of it.
    ''' </summary>
    ''' <param name="player">The player over the hand of which to place the
    ''' animation.</param>
    ''' <param name="assetName">Name of the asset to display above the hand.</param>
    ''' <param name="animationHand">Which hand to put cue over.</param>
    ''' <param name="waitForHand">Start the cue animation when the animation
    ''' of this hand over null of the animation of the currentHand</param>
    Private Sub CueOverPlayerHand(ByVal player As BlackjackPlayer, ByVal assetName As String, ByVal animationHand As HandTypes, ByVal waitForHand As AnimatedHandGameComponent)
        ' Get the position of the relevant hand
        Dim playerIndex As Integer = players.IndexOf(player)
        Dim currentAnimatedHand As AnimatedHandGameComponent
        Dim currentPosition As Vector2
        If playerIndex >= 0 Then
            Select Case animationHand
                Case HandTypes.First
                    currentAnimatedHand = animatedHands(playerIndex)
                    currentPosition = currentAnimatedHand.CurrentPosition
                Case HandTypes.Second
                    currentAnimatedHand = animatedSecondHands(playerIndex)
                    currentPosition = currentAnimatedHand.CurrentPosition + secondHandOffset
                Case Else
                    Throw New Exception("Player has an unsupported hand type.")
            End Select
        Else
            currentAnimatedHand = dealerHandComponent
            currentPosition = currentAnimatedHand.CurrentPosition
        End If

        ' Add the animation component 
        Dim animationComponent As New AnimatedGameComponent(Me, cardsAssets(assetName)) With {.CurrentPosition = currentPosition, .Visible = False}
        Game.Components.Add(animationComponent)

        ' Calculate when to start the animation. The animation will only begin
        ' after all hand cards finish animating
        Dim estimatedTimeToCompleteAnimations As TimeSpan
        If waitForHand IsNot Nothing Then
            estimatedTimeToCompleteAnimations = waitForHand.EstimatedTimeForAnimationsCompletion
        Else
            estimatedTimeToCompleteAnimations = currentAnimatedHand.EstimatedTimeForAnimationsCompletion
        End If

        ' Add a scale effect animation
        animationComponent.AddAnimation(New ScaleGameComponentAnimation(2.0F, 1.0F) With {.StartTime = Date.Now.Add(estimatedTimeToCompleteAnimations), .Duration = TimeSpan.FromSeconds(1.0F), .PerformBeforeStart = AddressOf ShowComponent, .PerformBeforSartArgs = animationComponent})
    End Sub

    ''' <summary>
    ''' Ends the current round.
    ''' </summary>
    Private Sub EndRound()
        RevealDealerFirstCard()
        DealerAI()
        ShowResults()
        State = BlackjackGameState.RoundEnd
    End Sub

    ''' <summary>
    ''' Causes the dealer's hand to be displayed.
    ''' </summary>
    Private Sub ShowDealerHand()
        dealerHandComponent = New BlackjackAnimatedDealerHandComponent(-1, dealerPlayer.Hand, Me)
        Game.Components.Add(dealerHandComponent)
    End Sub

    ''' <summary>
    ''' Reveal's the dealer's hidden card.
    ''' </summary>
    Private Sub RevealDealerFirstCard()
        ' Iterate over all dealer cards expect for the last
        Dim cardComponent As AnimatedCardsGameComponent = dealerHandComponent.GetCardGameComponent(1)
        cardComponent.AddAnimation(New FlipGameComponentAnimation With {.Duration = TimeSpan.FromSeconds(0.5), .StartTime = Date.Now})
    End Sub

    ''' <summary>
    ''' Present visual indication as to how the players fared in the current round.
    ''' </summary>
    Private Sub ShowResults()
        ' Calculate the dealer's hand value
        Dim dealerValue As Integer = dealerPlayer.FirstValue

        If dealerPlayer.FirstValueConsiderAce Then
            dealerValue += 10
        End If

        ' Show each player's result
        For playerIndex = 0 To players.Count - 1
            ShowResultForPlayer(CType(players(playerIndex), BlackjackPlayer), dealerValue, HandTypes.First)
            If (CType(players(playerIndex), BlackjackPlayer)).IsSplit Then
                ShowResultForPlayer(CType(players(playerIndex), BlackjackPlayer), dealerValue, HandTypes.Second)
            End If
        Next playerIndex
    End Sub

    ''' <summary>
    ''' Display's a player's status after the turn has ended.
    ''' </summary>
    ''' <param name="player">The player for which to display the status.</param>
    ''' <param name="dealerValue">The dealer's hand value.</param>
    ''' <param name="currentHandType">The player's hand to take into 
    ''' account.</param>
    Private Sub ShowResultForPlayer(ByVal player As BlackjackPlayer, ByVal dealerValue As Integer, ByVal currentHandType As HandTypes)
        ' Calculate the player's hand value and check his state (blackjack/bust)
        Dim blackjack, bust As Boolean
        Dim playerValue As Integer
        Select Case currentHandType
            Case HandTypes.First
                blackjack = player.BlackJack
                bust = player.Bust

                playerValue = player.FirstValue

                If player.FirstValueConsiderAce Then
                    playerValue += 10
                End If
            Case HandTypes.Second
                blackjack = player.SecondBlackJack
                bust = player.SecondBust

                playerValue = player.SecondValue

                If player.SecondValueConsiderAce Then
                    playerValue += 10
                End If
            Case Else
                Throw New Exception("Player has an unsupported hand type.")
        End Select
        ' The bust or blackjack state are animated independently of this method,
        ' so only trigger different outcome indications
        If player.MadeBet AndAlso ((Not blackjack) OrElse (dealerPlayer.BlackJack AndAlso blackjack)) AndAlso (Not bust) Then
            Dim assetName As String = GetResultAsset(player, dealerValue, playerValue)

            CueOverPlayerHand(player, assetName, currentHandType, dealerHandComponent)
        End If
    End Sub

    ''' <summary>
    ''' Return the asset name according to the result.
    ''' </summary>
    ''' <param name="player">The player for which to return the asset name.</param>
    ''' <param name="dealerValue">The dealer's hand value.</param>
    ''' <param name="playerValue">The player's hand value.</param>
    ''' <returns>The asset name</returns>
    Private Function GetResultAsset(ByVal player As BlackjackPlayer, ByVal dealerValue As Integer, ByVal playerValue As Integer) As String
        Dim assetName As String
        If dealerPlayer.Bust Then
            assetName = "win"
        ElseIf dealerPlayer.BlackJack Then
            If player.BlackJack Then
                assetName = "push"
            Else
                assetName = "lose"
            End If
        ElseIf playerValue < dealerValue Then
            assetName = "lose"
        ElseIf playerValue > dealerValue Then
            assetName = "win"
        Else
            assetName = "push"
        End If
        Return assetName
    End Function

    ''' <summary>
    ''' Have the dealer play. The dealer hits until reaching 17+ and then 
    ''' stands.
    ''' </summary>
    Private Sub DealerAI()
        ' The dealer may have not need to draw additional cards after his first
        ' two. Check if this is the case and if so end the dealer's play.
        dealerPlayer.CalculateValues()
        Dim dealerValue As Integer = dealerPlayer.FirstValue

        If dealerPlayer.FirstValueConsiderAce Then
            dealerValue += 10
        End If

        If dealerValue > 21 Then
            dealerPlayer.Bust = True
            CueOverPlayerHand(dealerPlayer, "bust", HandTypes.First, dealerHandComponent)
        ElseIf dealerValue = 21 Then
            dealerPlayer.BlackJack = True
            CueOverPlayerHand(dealerPlayer, "blackjack", HandTypes.First, dealerHandComponent)
        End If

        If dealerPlayer.BlackJack OrElse dealerPlayer.Bust Then
            Exit Sub
        End If

        ' Draw cards until 17 is reached, or the dealer gets a blackjack or busts
        Dim cardsDealed As Integer = 0
        Do While dealerValue <= 17
            Dim card As TraditionalCard = dealer.DealCardToHand(dealerPlayer.Hand)
            AddDealAnimation(card, dealerHandComponent, True, dealDuration, Date.Now.AddMilliseconds(1000 * (cardsDealed + 1)))
            cardsDealed += 1
            dealerPlayer.CalculateValues()
            dealerValue = dealerPlayer.FirstValue

            If dealerPlayer.FirstValueConsiderAce Then
                dealerValue += 10
            End If

            If dealerValue > 21 Then
                dealerPlayer.Bust = True
                CueOverPlayerHand(dealerPlayer, "bust", HandTypes.First, dealerHandComponent)
            End If
        Loop
    End Sub

    ''' <summary>
    ''' Displays the hands currently in play.
    ''' </summary>
    Private Sub DisplayPlayingHands()
        For playerIndex = 0 To players.Count - 1
            Dim animatedHandGameComponent As AnimatedHandGameComponent = New BlackjackAnimatedPlayerHandComponent(playerIndex, players(playerIndex).Hand, Me)
            Game.Components.Add(animatedHandGameComponent)
            animatedHands(playerIndex) = animatedHandGameComponent
        Next playerIndex

        ShowDealerHand()
    End Sub

    ''' <summary>
    ''' Starts a new game round.
    ''' </summary>
    Public Sub StartRound()
        playerHandValueTexts.Clear()
        AudioManager.PlaySound("Shuffle")
        dealer.Shuffle()
        DisplayPlayingHands()
        State = BlackjackGameState.Shuffling
    End Sub

    ''' <summary>
    ''' Sets the button availability according to the options available to the 
    ''' current player.
    ''' </summary>
    Private Sub SetButtonAvailability()
        Dim player As BlackjackPlayer = CType(GetCurrentPlayer(), BlackjackPlayer)
        ' Hide all buttons if no player is in play or the player is an AI player
        If player Is Nothing OrElse TypeOf player Is BlackjackAIPlayer Then
            EnableButtons(False)
            ChangeButtonsVisiblility(False)
            Exit Sub
        End If

        ' Show all buttons
        EnableButtons(True)
        ChangeButtonsVisiblility(True)

        ' Set insurance button availability
        buttons("Insurance").Visible = showInsurance
        buttons("Insurance").Enabled = showInsurance

        If player.IsSplit = False Then
            ' Remember that the bet amount was already reduced from the balance,
            ' so we only need to check if the player has more money than the
            ' current bet when trying to double/split

            ' Set double button availability
            If player.BetAmount > player.Balance OrElse player.Hand.Count <> 2 Then
                buttons("Double").Visible = False
                buttons("Double").Enabled = False
            End If

            If player.Hand.Count <> 2 OrElse player.Hand(0).Value <> player.Hand(1).Value OrElse player.BetAmount > player.Balance Then
                buttons("Split").Visible = False
                buttons("Split").Enabled = False
            End If
        Else
            ' We've performed a split. Get the initial bet amount to check whether
            ' or not we can double the current bet.
            Dim initialBet As Single = player.BetAmount / ((If(player.Double, 2.0F, 1.0F)) + (If(player.SecondDouble, 2.0F, 1.0F)))

            ' Set double button availability.
            If initialBet > player.Balance OrElse player.CurrentHand.Count <> 2 Then
                buttons("Double").Visible = False
                buttons("Double").Enabled = False
            End If

            ' Once you've split, you can't split again
            buttons("Split").Visible = False
            buttons("Split").Enabled = False
        End If
    End Sub

    ''' <summary>
    ''' Checks for running animations.
    ''' </summary>
    ''' <typeparam name="T">The type of animation to look for.</typeparam>
    ''' <returns>True if a running animation of the desired type is found and
    ''' false otherwise.</returns>
    Friend Function CheckForRunningAnimations(Of T As AnimatedGameComponent)() As Boolean
        Dim animationComponent As T
        For componentIndex = 0 To Game.Components.Count - 1
            animationComponent = TryCast(Game.Components(componentIndex), T)
            If animationComponent IsNot Nothing Then
                If animationComponent.IsAnimating Then
                    Return True
                End If
            End If
        Next componentIndex
        Return False
    End Function

    ''' <summary>
    ''' Ends the game.
    ''' </summary>
    Private Sub EndGame()
        ' Calculate the estimated time for all playing animations to end
        Dim estimatedTime As Long = 0
        Dim animationComponent As AnimatedGameComponent
        For componentIndex = 0 To Game.Components.Count - 1
            animationComponent = TryCast(Game.Components(componentIndex), AnimatedGameComponent)
            If animationComponent IsNot Nothing Then
                estimatedTime = Math.Max(estimatedTime, animationComponent.EstimatedTimeForAnimationsCompletion.Ticks)
            End If
        Next componentIndex

        ' Add a component for an empty stalling animation. This actually acts
        ' as a timer.
        Dim texture As Texture2D = Me.Game.Content.Load(Of Texture2D)("Images\youlose")
        animationComponent = New AnimatedGameComponent(Me, texture) With {.CurrentPosition = New Vector2(Me.Game.GraphicsDevice.Viewport.Bounds.Center.X - texture.Width \ 2, Me.Game.GraphicsDevice.Viewport.Bounds.Center.Y - texture.Height \ 2), .Visible = False}
        Me.Game.Components.Add(animationComponent)

        ' Add a button to return to the main menu
        Dim bounds As Rectangle = Me.Game.GraphicsDevice.Viewport.Bounds
        Dim center As New Vector2(bounds.Center.X, bounds.Center.Y)
        Dim backButton As New Button("ButtonRegular", "ButtonPressed", screenManager.input, Me) With {.Bounds = New Rectangle(CInt(Fix(center.X)) - 100, CInt(Fix(center.Y)) + 80, 200, 50), .Font = Me.Font, .Text = "Main Menu", .Visible = False, .Enabled = True}

        AddHandler backButton.Click, AddressOf backButton_Click

        ' Add stalling animation
        animationComponent.AddAnimation(New AnimatedGameComponentAnimation With {.Duration = TimeSpan.FromTicks(estimatedTime) + TimeSpan.FromSeconds(1), .PerformWhenDone = AddressOf ResetGame, .PerformWhenDoneArgs = New Object() {animationComponent, backButton}})
        Game.Components.Add(backButton)
    End Sub

    ''' <summary>
    ''' Helper method to reset the game
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub ResetGame(ByVal obj As Object)
        Dim arr() As Object = CType(obj, Object())
        State = BlackjackGameState.GameOver
        CType(arr(0), AnimatedGameComponent).Visible = True
        CType(arr(1), Button).Visible = True

        ' Remove all unnecessary game components
        Dim compontneIndex As Integer = 0
        Do While compontneIndex < Game.Components.Count
            If (Game.Components(compontneIndex) IsNot (CType(arr(0), AnimatedGameComponent)) AndAlso Game.Components(compontneIndex) IsNot (CType(arr(1), Button))) AndAlso (TypeOf Game.Components(compontneIndex) Is BetGameComponent OrElse TypeOf Game.Components(compontneIndex) Is AnimatedGameComponent OrElse TypeOf Game.Components(compontneIndex) Is Button) Then
                Game.Components.RemoveAt(compontneIndex)
            Else
                compontneIndex += 1
            End If
        Loop
    End Sub

    ''' <summary>
    ''' Finishes the current turn.
    ''' </summary>
    Private Sub FinishTurn()
        ' Remove all unnecessary components
        Dim componentIndex As Integer = 0
        Do While componentIndex < Game.Components.Count
            If Not (TypeOf Game.Components(componentIndex) Is GameTable OrElse TypeOf Game.Components(componentIndex) Is BlackjackCardGame OrElse TypeOf Game.Components(componentIndex) Is BetGameComponent OrElse TypeOf Game.Components(componentIndex) Is Button OrElse TypeOf Game.Components(componentIndex) Is ScreenManager OrElse TypeOf Game.Components(componentIndex) Is InputHelper) Then
                If TypeOf Game.Components(componentIndex) Is AnimatedCardsGameComponent Then
                    Dim animatedCard As AnimatedCardsGameComponent = (TryCast(Game.Components(componentIndex), AnimatedCardsGameComponent))
                    animatedCard.AddAnimation(New TransitionGameComponentAnimation(animatedCard.CurrentPosition, New Vector2(animatedCard.CurrentPosition.X, Me.Game.GraphicsDevice.Viewport.Height)) With {.Duration = TimeSpan.FromSeconds(0.4), .PerformWhenDone = AddressOf RemoveComponent, .PerformWhenDoneArgs = animatedCard})
                Else
                    Game.Components.RemoveAt(componentIndex)
                    componentIndex -= 1
                End If
            End If
            componentIndex += 1
        Loop

        ' Reset player values
        For playerIndex = 0 To players.Count - 1
            TryCast(players(playerIndex), BlackjackPlayer).ResetValues()
            players(playerIndex).Hand.DealCardsToHand(deadCards, players(playerIndex).Hand.Count)
            turnFinishedByPlayer(playerIndex) = False
            animatedHands(playerIndex) = Nothing
            animatedSecondHands(playerIndex) = Nothing
        Next playerIndex

        ' Reset the bet component
        betGameComponent.Reset()
        betGameComponent.Enabled = True

        ' Reset dealer
        dealerPlayer.Hand.DealCardsToHand(deadCards, dealerPlayer.Hand.Count)
        dealerPlayer.ResetValues()

        ' Reset rules
        rules.Clear()
    End Sub

    ''' <summary>
    ''' Helper method to remove component
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub RemoveComponent(ByVal obj As Object)
        Game.Components.Remove(CType(obj, AnimatedGameComponent))
    End Sub

    ''' <summary>
    ''' Performs the "Stand" move for the current player.
    ''' </summary>
    Public Sub Stand()
        Dim player As BlackjackPlayer = CType(GetCurrentPlayer(), BlackjackPlayer)
        If player Is Nothing Then
            Exit Sub
        End If

        ' If the player only has one hand, his turn ends. Otherwise, he now plays
        ' using his next hand
        If player.IsSplit = False Then
            turnFinishedByPlayer(players.IndexOf(player)) = True
        Else
            Select Case player.CurrentHandType
                Case HandTypes.First
                    If player.SecondBlackJack Then
                        turnFinishedByPlayer(players.IndexOf(player)) = True
                    Else
                        player.CurrentHandType = HandTypes.Second
                    End If
                Case HandTypes.Second
                    turnFinishedByPlayer(players.IndexOf(player)) = True
                Case Else
                    Throw New Exception("Player has an unsupported hand type.")
            End Select
        End If
    End Sub

    ''' <summary>
    ''' Performs the "Split" move for the current player.
    ''' This includes adding the animations which shows the first hand splitting
    ''' into two.
    ''' </summary>
    Public Sub Split()
        Dim player As BlackjackPlayer = CType(GetCurrentPlayer(), BlackjackPlayer)

        Dim playerIndex As Integer = players.IndexOf(player)

        player.InitializeSecondHand()

        Dim sourcePosition As Vector2 = animatedHands(playerIndex).GetCardGameComponent(1).CurrentPosition
        Dim targetPosition As Vector2 = animatedHands(playerIndex).GetCardGameComponent(0).CurrentPosition + secondHandOffset
        ' Create an animation moving the top card to the second hand location
        Dim animation As AnimatedGameComponentAnimation = New TransitionGameComponentAnimation(sourcePosition, targetPosition) With {.StartTime = Date.Now, .Duration = TimeSpan.FromSeconds(0.5F)}

        ' Actually perform the split
        player.SplitHand()

        ' Add additional chip stack for the second hand
        betGameComponent.AddChips(playerIndex, player.BetAmount, False, True)

        ' Initialize visual representation of the second hand
        animatedSecondHands(playerIndex) = New BlackjackAnimatedPlayerHandComponent(playerIndex, secondHandOffset, player.SecondHand, Me)
        Game.Components.Add(animatedSecondHands(playerIndex))

        Dim animatedGameComponet As AnimatedCardsGameComponent = animatedSecondHands(playerIndex).GetCardGameComponent(0)
        animatedGameComponet.IsFaceDown = False
        animatedGameComponet.AddAnimation(animation)

        ' Deal an additional cards to each of the new hands
        Dim card As TraditionalCard = dealer.DealCardToHand(player.Hand)
        AddDealAnimation(card, animatedHands(playerIndex), True, dealDuration, Date.Now + animation.EstimatedTimeForAnimationCompletion)
        card = dealer.DealCardToHand(player.SecondHand)
        AddDealAnimation(card, animatedSecondHands(playerIndex), True, dealDuration, Date.Now + animation.EstimatedTimeForAnimationCompletion + dealDuration)
    End Sub

    ''' <summary>
    ''' Performs the "Double" move for the current player.
    ''' </summary>
    Public Sub [Double]()
        Dim player As BlackjackPlayer = CType(GetCurrentPlayer(), BlackjackPlayer)

        Dim playerIndex As Integer = players.IndexOf(player)

        Select Case player.CurrentHandType
            Case HandTypes.First
                player.Double = True
                Dim betAmount As Single = player.BetAmount

                If player.IsSplit Then
                    betAmount /= 2.0F
                End If

                betGameComponent.AddChips(playerIndex, betAmount, False, False)
            Case HandTypes.Second
                player.SecondDouble = True
                If player.Double = False Then
                    ' The bet is evenly spread between both hands, add one half
                    betGameComponent.AddChips(playerIndex, player.BetAmount / 2.0F, False, True)
                Else
                    ' The first hand's bet is double, add one third of the total
                    betGameComponent.AddChips(playerIndex, player.BetAmount / 3.0F, False, True)
                End If
            Case Else
                Throw New Exception("Player has an unsupported hand type.")
        End Select
        Hit()
        Stand()
    End Sub

    ''' <summary>
    ''' Performs the "Hit" move for the current player.
    ''' </summary>
    Public Sub Hit()
        Dim player As BlackjackPlayer = CType(GetCurrentPlayer(), BlackjackPlayer)
        If player Is Nothing Then
            Exit Sub
        End If

        Dim playerIndex As Integer = players.IndexOf(player)

        ' Draw a card to the appropriate hand
        Select Case player.CurrentHandType
            Case HandTypes.First
                Dim card As TraditionalCard = dealer.DealCardToHand(player.Hand)
                AddDealAnimation(card, animatedHands(playerIndex), True, dealDuration, Date.Now)
            Case HandTypes.Second
                Dim card As TraditionalCard = dealer.DealCardToHand(player.SecondHand)
                AddDealAnimation(card, animatedSecondHands(playerIndex), True, dealDuration, Date.Now)
            Case Else
                Throw New Exception("Player has an unsupported hand type.")
        End Select
    End Sub

    ''' <summary>
    ''' Changes the visiblility of most game buttons.
    ''' </summary>
    ''' <param name="visible">True to make the buttons visible, false to make
    ''' them invisible.</param>
    Private Sub ChangeButtonsVisiblility(ByVal visible As Boolean)
        buttons("Hit").Visible = visible
        buttons("Stand").Visible = visible
        buttons("Double").Visible = visible
        buttons("Split").Visible = visible
        buttons("Insurance").Visible = visible
    End Sub

    ''' <summary>
    ''' Enables or disable most game buttons.
    ''' </summary>
    ''' <param name="enabled">True to enable the buttons , false to 
    ''' disable them.</param>
    Private Sub EnableButtons(ByVal enabled As Boolean)
        buttons("Hit").Enabled = enabled
        buttons("Stand").Enabled = enabled
        buttons("Double").Enabled = enabled
        buttons("Split").Enabled = enabled
        buttons("Insurance").Enabled = enabled
    End Sub

    ''' <summary>
    ''' Add an indication that the player has passed on the current round.
    ''' </summary>
    ''' <param name="indexPlayer">The player's index.</param>
    Public Sub ShowPlayerPass(ByVal indexPlayer As Integer)
        ' Add animation component
        Dim passComponent As New AnimatedGameComponent(Me, cardsAssets("pass")) With {.CurrentPosition = GameTable.PlaceOrder(indexPlayer), .Visible = False}
        Game.Components.Add(passComponent)

        ' Hide insurance button only when the first payer passes
        Dim performWhenDone As Action(Of Object) = Nothing
        If indexPlayer = 0 Then
            performWhenDone = AddressOf HideInshurance
        End If
        ' Add scale animation for the pass "card"
        passComponent.AddAnimation(New ScaleGameComponentAnimation(2.0F, 1.0F) With {.AnimationCycles = 1, .PerformBeforeStart = AddressOf ShowComponent, .PerformBeforSartArgs = passComponent, .StartTime = Date.Now, .Duration = TimeSpan.FromSeconds(1), .PerformWhenDone = performWhenDone})
    End Sub

    ''' <summary>
    ''' Helper method to hide insurance
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub HideInshurance(ByVal obj As Object)
        showInsurance = False
    End Sub
#End Region

#Region "Event Handlers"
    ''' <summary>
    ''' Shows the insurance button if the first player can afford insurance.
    ''' </summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="e">The <see cref="System.EventArgs"/> instance containing 
    ''' the event data.</param>
    Private Sub InsuranceGameRule(ByVal sender As Object, ByVal e As EventArgs)
        Dim player As BlackjackPlayer = CType(players(0), BlackjackPlayer)
        If player.Balance >= player.BetAmount / 2 Then
            showInsurance = True
        End If
    End Sub

    ''' <summary>
    ''' Shows the bust visual cue after the bust rule has been matched.
    ''' </summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="e">The <see cref="System.EventArgs"/> instance containing 
    ''' the event data.</param>
    Private Sub BustGameRule(ByVal sender As Object, ByVal e As EventArgs)
        showInsurance = False
        Dim args As BlackjackGameEventArgs = (TryCast(e, BlackjackGameEventArgs))
        Dim player As BlackjackPlayer = CType(args.Player, BlackjackPlayer)

        CueOverPlayerHand(player, "bust", args.Hand, Nothing)

        Select Case args.Hand
            Case HandTypes.First
                player.Bust = True

                If player.IsSplit AndAlso (Not player.SecondBlackJack) Then
                    player.CurrentHandType = HandTypes.Second
                Else
                    turnFinishedByPlayer(players.IndexOf(player)) = True
                End If
            Case HandTypes.Second
                player.SecondBust = True
                turnFinishedByPlayer(players.IndexOf(player)) = True
            Case Else
                Throw New Exception("Player has an unsupported hand type.")
        End Select
    End Sub

    ''' <summary>
    ''' Shows the blackjack visual cue after the blackjack rule has been matched.
    ''' </summary>
    ''' <param name="sender">The sender.</param>
    ''' <param name="e">The <see cref="System.EventArgs"/> instance containing 
    ''' the event data.</param>
    Private Sub BlackJackGameRule(ByVal sender As Object, ByVal e As EventArgs)
        showInsurance = False
        Dim args As BlackjackGameEventArgs = (TryCast(e, BlackjackGameEventArgs))
        Dim player As BlackjackPlayer = CType(args.Player, BlackjackPlayer)

        CueOverPlayerHand(player, "blackjack", args.Hand, Nothing)

        Select Case args.Hand
            Case HandTypes.First
                player.BlackJack = True

                If player.IsSplit Then
                    player.CurrentHandType = HandTypes.Second
                Else
                    turnFinishedByPlayer(players.IndexOf(player)) = True
                End If
            Case HandTypes.Second
                player.SecondBlackJack = True
                If player.CurrentHandType = HandTypes.Second Then
                    turnFinishedByPlayer(players.IndexOf(player)) = True
                End If
            Case Else
                Throw New Exception("Player has an unsupported hand type.")
        End Select
    End Sub

    ''' <summary>
    ''' Handles the Click event of the insurance button.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub Insurance_Click(ByVal sender As Object, ByVal e As EventArgs)
        Dim player As BlackjackPlayer = CType(GetCurrentPlayer(), BlackjackPlayer)
        If player Is Nothing Then
            Exit Sub
        End If
        player.IsInsurance = True
        player.Balance -= player.BetAmount / 2.0F
        betGameComponent.AddChips(players.IndexOf(player), player.BetAmount / 2, True, False)
        showInsurance = False
    End Sub

    ''' <summary>
    ''' Handles the Click event of the new game button.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub newGame_Click(ByVal sender As Object, ByVal e As EventArgs)
        FinishTurn()
        StartRound()
        newGame.Enabled = False
        newGame.Visible = False
    End Sub

    ''' <summary>
    ''' Handles the Click event of the hit button.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub Hit_Click(ByVal sender As Object, ByVal e As EventArgs)
        Hit()
        showInsurance = False
    End Sub

    ''' <summary>
    ''' Handles the Click event of the stand button.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub Stand_Click(ByVal sender As Object, ByVal e As EventArgs)
        Stand()
        showInsurance = False
    End Sub

    ''' <summary>
    ''' Handles the Click event of the double button.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub Double_Click(ByVal sender As Object, ByVal e As EventArgs)
        [Double]()
        showInsurance = False
    End Sub

    ''' <summary>
    ''' Handles the Click event of the split button.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub Split_Click(ByVal sender As Object, ByVal e As EventArgs)
        Split()
        showInsurance = False
    End Sub

    ''' <summary>
    ''' Handles the Click event of the back button.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">>The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub backButton_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' Remove all unnecessary components
        Dim componentIndex As Integer = 0
        Do While componentIndex < Game.Components.Count
            If Not (TypeOf Game.Components(componentIndex) Is ScreenManager) Then
                Game.Components.RemoveAt(componentIndex)
                componentIndex -= 1
            End If
            componentIndex += 1
        Loop

        For Each screen In screenManager.GetScreens
            screen.ExitScreen()
        Next screen

        screenManager.AddScreen(New BackgroundScreen, Nothing)
        screenManager.AddScreen(New MainMenuScreen, Nothing)
    End Sub
#End Region
End Class
