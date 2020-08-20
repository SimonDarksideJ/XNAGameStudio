#Region "File Description"
'-----------------------------------------------------------------------------
' BetGameComponent.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports CardsFramework
Imports Blackjack.GameStateManagement
#End Region

Public Class BetGameComponent
    Inherits DrawableGameComponent
#Region "Fields and Properties"
    Private players As List(Of Player)
    Private theme As String
    Private assetNames() As Integer = {5, 25, 100, 500}
    Private chipsAssets As Dictionary(Of Integer, Texture2D)
    Private blankChip As Texture2D
    Private positions() As Vector2
    Private cardGame As CardsFramework.CardsGame
    Private spriteBatch As SpriteBatch

    Private isKeyDown As Boolean = False

    Private bet As Button
    Private clear As Button

    Private Property ChipOffset As Vector2
    Private Shared insuranceYPosition As Single = 120 * BlackJackGame.HeightScale
    Private Shared secondHandOffset As New Vector2(25 * BlackJackGame.WidthScale, 30 * BlackJackGame.HeightScale)

    Private currentChipComponent As New List(Of AnimatedGameComponent)
    Private currentBet As Integer = 0
    Private input As InputState
    Private inputHelper As InputHelper
#End Region

#Region "Initiaizations"
    ''' <summary>
    ''' Creates a new instance of the <see cref="BetGameComponent"/> class.
    ''' </summary>
    ''' <param name="players">A list of participating players.</param>
    ''' <param name="input">An instance of 
    ''' <see cref="GameStateManagement.InputState"/> which can be used to 
    ''' check user input.</param>
    ''' <param name="theme">The name of the selcted card theme.</param>
    ''' <param name="cardGame">An instance of <see cref="CardsGame"/> which
    ''' is the current game.</param>
    Public Sub New(ByVal players As List(Of Player), ByVal input As InputState, ByVal theme As String, ByVal cardGame As CardsGame)
        MyBase.New(cardGame.Game)
        Me.players = players
        Me.theme = theme
        Me.cardGame = cardGame
        Me.input = input
        chipsAssets = New Dictionary(Of Integer, Texture2D)
    End Sub


    ''' <summary>
    ''' Initializes the component.
    ''' </summary>
    Public Overrides Sub Initialize()
#If WINDOWS_PHONE Then
        ' Enable tap gesture
        TouchPanel.EnabledGestures = GestureType.Tap
#End If
        ' Get xbox cursor
        inputHelper = Nothing
        For componentIndex = 0 To Game.Components.Count - 1
            If TypeOf Game.Components(componentIndex) Is InputHelper Then
                inputHelper = CType(Game.Components(componentIndex), InputHelper)
                Exit For
            End If
        Next componentIndex

        ' Show mouse
        Game.IsMouseVisible = True
        MyBase.Initialize()

        spriteBatch = New SpriteBatch(Game.GraphicsDevice)

        ' Calculate chips position for the chip buttons which allow placing the bet
        Dim size As Rectangle = chipsAssets(assetNames(0)).Bounds

        Dim bounds As Rectangle = spriteBatch.GraphicsDevice.Viewport.TitleSafeArea

        positions(chipsAssets.Count - 1) = New Vector2(bounds.Left + 10, bounds.Bottom - size.Height - 80)
        For chipIndex = 2 To chipsAssets.Count
            size = chipsAssets(assetNames(chipsAssets.Count - chipIndex)).Bounds
            positions(chipsAssets.Count - chipIndex) = positions(chipsAssets.Count - (chipIndex - 1)) - New Vector2(0, size.Height + 10)
        Next chipIndex

        ' Initialize bet button
        bet = New Button("ButtonRegular", "ButtonPressed", input, cardGame) With {.Bounds = New Rectangle(bounds.Left + 10, bounds.Bottom - 60, 100, 50), .Font = cardGame.Font, .Text = "Deal"}
        AddHandler bet.Click, AddressOf Bet_Click
        Game.Components.Add(bet)

        ' Initialize clear button
        clear = New Button("ButtonRegular", "ButtonPressed", input, cardGame) With {.Bounds = New Rectangle(bounds.Left + 120, bounds.Bottom - 60, 100, 50), .Font = cardGame.Font, .Text = "Clear"}
        AddHandler clear.Click, AddressOf Clear_Click
        Game.Components.Add(clear)
        ShowAndEnableButtons(False)
    End Sub
#End Region

#Region "Loading"
    ''' <summary>
    ''' Load component content.
    ''' </summary>
    Protected Overrides Sub LoadContent()
        ' Load blank chip texture
        blankChip = Game.Content.Load(Of Texture2D)(String.Format("Images\Chips\chip{0}", "White"))

        ' Load chip textures
        Dim assetNames() As Integer = {5, 25, 100, 500}
        For chipIndex = 0 To assetNames.Length - 1
            chipsAssets.Add(assetNames(chipIndex), Game.Content.Load(Of Texture2D)(String.Format("Images\Chips\chip{0}", assetNames(chipIndex))))
        Next chipIndex
        positions = New Vector2(assetNames.Length - 1) {}

        MyBase.LoadContent()
    End Sub
#End Region

#Region "Update and Render"
    ''' <summary>
    ''' Perform update logic related to the component.
    ''' </summary>
    ''' <param name="gameTime">Time elapsed since the last call to 
    ''' this method.</param>
    Public Overrides Sub Update(ByVal gameTime As GameTime)
        If players.Count > 0 Then
            ' If betting is possible
            If (CType(cardGame, BlackjackCardGame)).State = BlackjackGameState.Betting AndAlso Not (CType(players(players.Count - 1), BlackjackPlayer)).IsDoneBetting Then
                Dim playerIndex As Integer = GetCurrentPlayer()

                Dim player As BlackjackPlayer = CType(players(playerIndex), BlackjackPlayer)

                ' If the player is an AI player, have it bet
                If TypeOf player Is BlackjackAIPlayer Then
                    ShowAndEnableButtons(False)
                    Dim bet As Integer = (CType(player, BlackjackAIPlayer)).AIBet()
                    If bet = 0 Then
                        Bet_Click(Me, EventArgs.Empty)
                    Else
                        AddChip(playerIndex, bet, False)
                    End If
                Else
                    ' Reveal the input buttons for a human player and handle input
                    ' remember that buttons handle their own imput, so we only check
                    ' for input on the chip buttons
                    ShowAndEnableButtons(True)

                    HandleInput(Mouse.GetState)
                End If
            End If

            ' Once all players are done betting, advance the game to the dealing stage
            If (CType(players(players.Count - 1), BlackjackPlayer)).IsDoneBetting Then
                Dim blackjackGame As BlackjackCardGame = (CType(cardGame, BlackjackCardGame))

                If Not blackjackGame.CheckForRunningAnimations(Of AnimatedGameComponent)() Then
                    ShowAndEnableButtons(False)
                    blackjackGame.State = BlackjackGameState.Dealing

                    Enabled = False
                End If
            End If
        End If

        MyBase.Update(gameTime)
    End Sub

    ''' <summary>
    ''' Gets the player which is currently betting. This is the first player who has
    ''' yet to finish betting.
    ''' </summary>
    ''' <returns>The player which is currently betting.</returns>
    Private Function GetCurrentPlayer() As Integer
        For playerIndex = 0 To players.Count - 1
            If Not (CType(players(playerIndex), BlackjackPlayer)).IsDoneBetting Then
                Return playerIndex
            End If
        Next playerIndex
        Return -1
    End Function

    ''' <summary>
    ''' Handle the input of adding chip on all platform
    ''' </summary>
    ''' <param name="mouseState">Mouse input information.</param>
    Private Sub HandleInput(ByVal mouseState As MouseState)
        Dim isPressed As Boolean = False
        Dim position As Vector2 = Vector2.Zero

        If mouseState.LeftButton = ButtonState.Pressed Then
            isPressed = True
            position = New Vector2(mouseState.X, mouseState.Y)
        ElseIf inputHelper.IsPressed Then
            isPressed = True
            position = inputHelper.PointPosition
#If WINDOWS_PHONE Then
        ElseIf (input.Gestures.Count > 0) AndAlso input.Gestures(0).GestureType = GestureType.Tap Then
            isPressed = True
            position = input.Gestures(0).Position
#End If
        End If

        If isPressed Then
            If Not isKeyDown Then
                Dim chipValue As Integer = GetIntersectingChipValue(position)
                If chipValue <> 0 Then
                    AddChip(GetCurrentPlayer(), chipValue, False)
                End If
                isKeyDown = True
            End If
        Else
            isKeyDown = False
        End If
    End Sub

    ''' <summary>
    ''' Get which chip intersects with a given position.
    ''' </summary>
    ''' <param name="position">The position to check for intersection.</param>
    ''' <returns>The value of the chip intersecting with the specified position, or
    ''' 0 if no chips intersect with the position.</returns>
    Private Function GetIntersectingChipValue(ByVal position As Vector2) As Integer
        Dim size As Rectangle
        ' Calculate the bounds of the position
        Dim touchTap As New Rectangle(CInt(Fix(position.X)) - 1, CInt(Fix(position.Y)) - 1, 2, 2)
        For chipIndex = 0 To chipsAssets.Count - 1
            ' Calculate the bounds of the asset
            size = chipsAssets(assetNames(chipIndex)).Bounds
            size.X = CInt(Fix(positions(chipIndex).X))
            size.Y = CInt(Fix(positions(chipIndex).Y))
            If size.Intersects(touchTap) Then
                Return assetNames(chipIndex)
            End If
        Next chipIndex

        Return 0
    End Function

    ''' <summary>
    ''' Draws the component
    ''' </summary>
    ''' <param name="gameTime">Time passed since the last call to 
    ''' this method.</param>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        spriteBatch.Begin()

        ' Draws the chips
        For chipIndex = 0 To chipsAssets.Count - 1
            spriteBatch.Draw(chipsAssets(assetNames(chipIndex)), positions(chipIndex), Color.White)
        Next chipIndex

        Dim player As BlackjackPlayer

        ' Draws the player balance and bet amount
        For playerIndex = 0 To players.Count - 1
            Dim table As BlackJackTable = CType(cardGame.GameTable, BlackJackTable)
            Dim position As Vector2 = table(playerIndex) + table.RingOffset + New Vector2(table.RingTexture.Bounds.Width, 0)
            player = CType(players(playerIndex), BlackjackPlayer)
            spriteBatch.DrawString(cardGame.Font, "$" & player.BetAmount.ToString, position, Color.White)
            spriteBatch.DrawString(cardGame.Font, "$" & player.Balance.ToString, position + New Vector2(0, 30), Color.White)
        Next playerIndex

        spriteBatch.End()

        MyBase.Draw(gameTime)
    End Sub
#End Region

#Region "Public Methods"
    ''' <summary>
    ''' Adds the chip to one of the player betting zones.
    ''' </summary>
    ''' <param name="playerIndex">Index of the player for whom to add 
    ''' a chip.</param>
    ''' <param name="chipValue">The value on the chip to add.</param>
    ''' <param name="secondHand">True if this chip is added to the chip pile
    ''' belonging to the player's second hand.</param>
    Public Sub AddChip(ByVal playerIndex As Integer, ByVal chipValue As Integer, ByVal secondHand As Boolean)
        ' Only add the chip if the bet is successfully performed
        If (CType(players(playerIndex), BlackjackPlayer)).Bet(chipValue) Then
            currentBet += chipValue
            ' Add chip component
            Dim chipComponent As New AnimatedGameComponent(cardGame, chipsAssets(chipValue)) With {.Visible = False}

            Game.Components.Add(chipComponent)

            ' Calculate the position for the new chip
            Dim position As Vector2
            ' Get the proper offset according to the platform (pc, phone, xbox)
            Dim offset As Vector2 = GetChipOffset(playerIndex, secondHand)

            position = cardGame.GameTable(playerIndex) + offset + New Vector2(-currentChipComponent.Count * 2, currentChipComponent.Count * 1)


            ' Find the index of the chip
            Dim currentChipIndex As Integer = 0
            For chipIndex = 0 To chipsAssets.Count - 1
                If assetNames(chipIndex) = chipValue Then
                    currentChipIndex = chipIndex
                    Exit For
                End If
            Next chipIndex

            ' Add transition animation
            chipComponent.AddAnimation(New TransitionGameComponentAnimation(positions(currentChipIndex), position) With {.Duration = TimeSpan.FromSeconds(1.0F), .PerformBeforeStart = AddressOf ShowComponent, .PerformBeforSartArgs = chipComponent, .PerformWhenDone = AddressOf PlayBetSound})

            ' Add flip animation
            chipComponent.AddAnimation(New FlipGameComponentAnimation With {.Duration = TimeSpan.FromSeconds(1.0F), .AnimationCycles = 3})

            currentChipComponent.Add(chipComponent)
        End If
    End Sub

    ''' <summary>
    ''' Helper method to show component
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub ShowComponent(ByVal obj As Object)
        CType(obj, AnimatedGameComponent).Visible = True
    End Sub

    ''' <summary>
    ''' Helper method to play bet sound
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub PlayBetSound(ByVal obj As Object)
        AudioManager.PlaySound("Bet")
    End Sub

    ''' <summary>
    ''' Adds chips to a specified player.
    ''' </summary>
    ''' <param name="playerIndex">Index of the player.</param>
    ''' <param name="amount">The total amount to add.</param>
    ''' <param name="insurance">If true, an insurance chip is added instead of
    ''' regular chips.</param>
    ''' <param name="secondHand">True if chips are to be added to the player's
    ''' second hand.</param>
    Public Sub AddChips(ByVal playerIndex As Integer, ByVal amount As Single, ByVal insurance As Boolean, ByVal secondHand As Boolean)
        If insurance Then
            AddInsuranceChipAnimation(amount)
        Else
            AddChips(playerIndex, amount, secondHand)
        End If
    End Sub

    ''' <summary>
    ''' Resets this instance.
    ''' </summary>
    Public Sub Reset()
        ShowAndEnableButtons(True)
        currentChipComponent.Clear()
    End Sub

    ''' <summary>
    ''' Updates the balance of all players in light of their bets and the dealer's
    ''' hand.
    ''' </summary>
    ''' <param name="dealerPlayer">Player object representing the dealer.</param>
    Public Sub CalculateBalance(ByVal dealerPlayer As BlackjackPlayer)
        For playerIndex = 0 To players.Count - 1
            Dim player As BlackjackPlayer = CType(players(playerIndex), BlackjackPlayer)

            ' Calculate first factor, which represents the amount of the first
            ' hand bet which returns to the player
            Dim factor As Single = CalculateFactorForHand(dealerPlayer, player, HandTypes.First)


            If player.IsSplit Then
                ' Calculate the return factor for the second hand
                Dim factor2 As Single = CalculateFactorForHand(dealerPlayer, player, HandTypes.Second)
                ' Calculate the initial bet performed by the player
                Dim initialBet As Single = player.BetAmount / ((If(player.Double, 2.0F, 1.0F)) + (If(player.SecondDouble, 2.0F, 1.0F)))

                Dim bet1 As Single = initialBet * (If(player.Double, 2.0F, 1.0F))
                Dim bet2 As Single = initialBet * (If(player.SecondDouble, 2.0F, 1.0F))

                ' Update the balance in light of the bets and results
                player.Balance += bet1 * factor + bet2 * factor2

                If player.IsInsurance AndAlso dealerPlayer.BlackJack Then
                    player.Balance += initialBet
                End If
            Else
                If player.IsInsurance AndAlso dealerPlayer.BlackJack Then
                    player.Balance += player.BetAmount
                End If

                ' Update the balance in light of the bets and results
                player.Balance += player.BetAmount * factor
            End If

            player.ClearBet()
        Next playerIndex
    End Sub
#End Region

#Region "Private Methods"
    ''' <summary>
    ''' Adds chips to a specified player in order to reach a specified bet amount.
    ''' </summary>
    ''' <param name="playerIndex">Index of the player to whom the chips are to
    ''' be added.</param>
    ''' <param name="amount">The bet amount to add to the player.</param>
    ''' <param name="secondHand">True to add the chips to the player's second
    ''' hand, false to add them to the first hand.</param>
    Private Sub AddChips(ByVal playerIndex As Integer, ByVal amount As Single, ByVal secondHand As Boolean)
        Dim assetNames() As Integer = {5, 25, 100, 500}

        Do While amount > 0
            If amount >= 5 Then
                ' Add the chip with the highest possible value
                For chipIndex = assetNames.Length To 1 Step -1
                    Do While assetNames(chipIndex - 1) <= amount
                        AddChip(playerIndex, assetNames(chipIndex - 1), secondHand)
                        amount -= assetNames(chipIndex - 1)
                    Loop
                Next chipIndex
            Else
                amount = 0
            End If
        Loop
    End Sub

    ''' <summary>
    ''' Animates the placement of an insurance chip on the table.
    ''' </summary>
    ''' <param name="amount">The amount which should appear on the chip.</param>
    Private Sub AddInsuranceChipAnimation(ByVal amount As Single)
        ' Add chip component
        Dim chipComponent As New AnimatedGameComponent(cardGame, blankChip) With {.TextColor = Color.Black, .Enabled = True, .Visible = False}

        Game.Components.Add(chipComponent)

        ' Add transition animation
        chipComponent.AddAnimation(New TransitionGameComponentAnimation(positions(0), New Vector2(GraphicsDevice.Viewport.Width \ 2, insuranceYPosition)) With {.PerformBeforeStart = AddressOf ShowComponent, .PerformBeforSartArgs = chipComponent, .PerformWhenDone = AddressOf ShowChipAmountAndPlayBetSound, .PerformWhenDoneArgs = New Object() {chipComponent, amount}, .Duration = TimeSpan.FromSeconds(1), .StartTime = Date.Now})

        ' Add flip animation
        chipComponent.AddAnimation(New FlipGameComponentAnimation With {.Duration = TimeSpan.FromSeconds(1.0F), .AnimationCycles = 3})
    End Sub

    ''' <summary>
    ''' Helper method to show the amount on the chip and play bet sound
    ''' </summary>
    ''' <param name="obj"></param>
    Private Sub ShowChipAmountAndPlayBetSound(ByVal obj As Object)
        Dim arr() As Object = CType(obj, Object())
        CType(arr(0), AnimatedGameComponent).Text = arr(1).ToString()
        AudioManager.PlaySound("Bet")
    End Sub

    ''' <summary>
    ''' Gets the offset at which newly added chips should be placed.
    ''' </summary>
    ''' <param name="playerIndex">Index of the player to whom the chip 
    ''' is added.</param>
    ''' <param name="secondHand">True if the chip is added to the player's second
    ''' hand, false otherwise.</param>
    ''' <returns>The offset from the player's position where chips should be
    ''' placed.</returns>
    Private Function GetChipOffset(ByVal playerIndex As Integer, ByVal secondHand As Boolean) As Vector2
        Dim offset As Vector2 = Vector2.Zero

        Dim table As BlackJackTable = (CType(cardGame.GameTable, BlackJackTable))
        offset = table.RingOffset + New Vector2(table.RingTexture.Bounds.Width - blankChip.Bounds.Width, table.RingTexture.Bounds.Height - blankChip.Bounds.Height) / 2.0F

        If secondHand = True Then
            offset += secondHandOffset
        End If

        Return offset
    End Function

    ''' <summary>
    ''' Show and enable, or hide and disable, the bet related buttons.
    ''' </summary>
    ''' <param name="visibleEnabled">True to show and enable the buttons, false
    ''' to hide and disable them.</param>
    Private Sub ShowAndEnableButtons(ByVal visibleEnabled As Boolean)
        bet.Visible = visibleEnabled
        bet.Enabled = visibleEnabled
        clear.Visible = visibleEnabled
        clear.Enabled = visibleEnabled
    End Sub

    ''' <summary>
    ''' Returns a factor which determines how much of a bet a player should get 
    ''' back, according to the outcome of the round.
    ''' </summary>
    ''' <param name="dealerPlayer">The player representing the dealer.</param>
    ''' <param name="player">The player for whom we calculate the factor.</param>
    ''' <param name="currentHand">The hand to calculate the factor for.</param>
    ''' <returns></returns>
    Private Function CalculateFactorForHand(ByVal dealerPlayer As BlackjackPlayer, ByVal player As BlackjackPlayer, ByVal currentHand As HandTypes) As Single
        Dim factor As Single

        Dim blackjack, bust, considerAce As Boolean
        Dim playerValue As Integer
        player.CalculateValues()

        ' Get some player status information according to the desired hand
        Select Case currentHand
            Case HandTypes.First
                blackjack = player.BlackJack
                bust = player.Bust
                playerValue = player.FirstValue
                considerAce = player.FirstValueConsiderAce
            Case HandTypes.Second
                blackjack = player.SecondBlackJack
                bust = player.SecondBust
                playerValue = player.SecondValue
                considerAce = player.SecondValueConsiderAce
            Case Else
                Throw New Exception("Player has an unsupported hand type.")
        End Select

        If considerAce Then
            playerValue += 10
        End If


        If bust Then
            factor = -1 ' Bust
        ElseIf dealerPlayer.Bust Then
            If blackjack Then
                factor = 1.5F ' Win BlackJack
            Else
                factor = 1 ' Win
            End If
        ElseIf dealerPlayer.BlackJack Then
            If blackjack Then
                factor = 0 ' Push BlackJack
            Else
                factor = -1 ' Lose BlackJack
            End If
        ElseIf blackjack Then
            factor = 1.5F
        Else
            Dim dealerValue As Integer = dealerPlayer.FirstValue

            If dealerPlayer.FirstValueConsiderAce Then
                dealerValue += 10
            End If

            If playerValue > dealerValue Then
                factor = 1 ' Win
            ElseIf playerValue < dealerValue Then
                factor = -1 ' Lose
            Else
                factor = 0 ' Push
            End If
        End If
        Return factor
    End Function
#End Region

#Region "Event Hanlders"
    ''' <summary>
    ''' Handles the Click event of the Clear button.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub Clear_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' Clear current player chips from screen and resets his bet
        currentBet = 0
        CType(players(GetCurrentPlayer()), BlackjackPlayer).ClearBet()
        For chipComponentIndex = 0 To currentChipComponent.Count - 1
            Game.Components.Remove(currentChipComponent(chipComponentIndex))
        Next chipComponentIndex
        currentChipComponent.Clear()
    End Sub

    ''' <summary>
    ''' Handles the Click event of the Bet button.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The 
    ''' <see cref="System.EventArgs"/> instance containing the event data.</param>
    Private Sub Bet_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' Finish the bet
        Dim playerIndex As Integer = GetCurrentPlayer()
        ' If the player did not bet, show that he has passed on this round
        If currentBet = 0 Then
            CType(cardGame, BlackjackCardGame).ShowPlayerPass(playerIndex)
        End If
        CType(players(playerIndex), BlackjackPlayer).IsDoneBetting = True
        currentChipComponent.Clear()
        currentBet = 0
    End Sub
#End Region
End Class
