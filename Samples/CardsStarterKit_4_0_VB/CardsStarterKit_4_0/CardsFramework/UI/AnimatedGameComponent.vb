#Region "File Description"
'-----------------------------------------------------------------------------
' AnimatedGameComponent.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports CardsFramework
#End Region

''' <summary>
''' A game component.
''' Enable variable display while managing and displaying a set of
''' <see cref="AnimatedGameComponentAnimation">Animations</see>
''' </summary>
Public Class AnimatedGameComponent
    Inherits DrawableGameComponent
#Region "Fields and Properties"

    Public Property CurrentFrame As Texture2D
    Public Property CurrentSegment As Rectangle?
    Public Property Text As String
    Public Property TextColor As Color
    Public IsFaceDown As Boolean = True
    Public Property CurrentPosition As Vector2
    Public Property CurrentDestination As Rectangle?

    Private runningAnimations As New List(Of AnimatedGameComponentAnimation)

    ''' <summary>
    ''' Whether or not an animation belonging to the component is running.
    ''' </summary>
    Public Overridable ReadOnly Property IsAnimating As Boolean
        Get
            Return runningAnimations.Count > 0
        End Get
    End Property

    Private _cardGame As CardsGame
    Public Property CardGame As CardsGame
        Get
            Return _cardGame
        End Get
        Private Set(ByVal value As CardsGame)
            _cardGame = value
        End Set
    End Property
#End Region

#Region "Initializatios"
    ''' <summary>
    ''' Initializes a new instance of the class, using black text color.
    ''' </summary>
    ''' <param name="game">The associated game class.</param>
    Public Sub New(ByVal game As Game)
        MyBase.New(game)
        TextColor = Color.Black
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the class, using black text color.
    ''' </summary>
    ''' <param name="game">The associated game class.</param>
    ''' <param name="currentFrame">The texture serving as the current frame
    ''' to display as the component.</param>
    Public Sub New(ByVal game As Game, ByVal currentFrame As Texture2D)
        Me.New(game)
        Me.CurrentFrame = currentFrame
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the class, using black text color.
    ''' </summary>
    ''' <param name="cardGame">The associated card game.</param>
    ''' <param name="currentFrame">The texture serving as the current frame
    ''' to display as the component.</param>
    Public Sub New(ByVal cardGame As CardsGame, ByVal currentFrame As Texture2D)
        Me.New(cardGame.Game)
        Me.CardGame = cardGame
        Me.CurrentFrame = currentFrame
    End Sub
#End Region

#Region "Update and Render"
    ''' <summary>
    ''' Keeps track of the component's animations.
    ''' </summary>
    ''' <param name="gameTime">The time which as elapsed since the last call
    ''' to this method.</param>
    Public Overrides Sub Update(ByVal gameTime As GameTime)
        MyBase.Update(gameTime)

        Dim animationIndex As Integer = 0
        Do While animationIndex < runningAnimations.Count
            runningAnimations(animationIndex).AccumulateElapsedTime(gameTime.ElapsedGameTime)
            runningAnimations(animationIndex).Run(gameTime)
            If runningAnimations(animationIndex).IsDone() Then
                runningAnimations.RemoveAt(animationIndex)
                animationIndex -= 1
            End If

            animationIndex += 1
        Loop
    End Sub

    ''' <summary>
    ''' Draws the animated component and its associated text, if it exists, at
    ''' the object's set destination. If a destination is not set, its initial
    ''' position is used.
    ''' </summary>
    ''' <param name="gameTime">The time which as elapsed since the last call
    ''' to this method.</param>
    Public Overrides Sub Draw(ByVal gameTime As GameTime)
        MyBase.Draw(gameTime)

        Dim spriteBatch As SpriteBatch

        If CardGame IsNot Nothing Then
            spriteBatch = CardGame.SpriteBatch
        Else
            spriteBatch = New SpriteBatch(Game.GraphicsDevice)
        End If

        spriteBatch.Begin()

        ' Draw at the destination if one is set
        If CurrentDestination.HasValue Then
            If CurrentFrame IsNot Nothing Then
                spriteBatch.Draw(CurrentFrame, CurrentDestination.Value, CurrentSegment, Color.White)
                If Text IsNot Nothing Then
                    Dim size As Vector2 = CardGame.Font.MeasureString(Text)
                    Dim textPosition As New Vector2(CurrentDestination.Value.X + CurrentDestination.Value.Width \ 2 - size.X / 2, CurrentDestination.Value.Y + CurrentDestination.Value.Height \ 2 - size.Y / 2)

                    spriteBatch.DrawString(CardGame.Font, Text, textPosition, TextColor)
                End If
            End If
            ' Draw at the component's position if there is no destination
        Else
            If CurrentFrame IsNot Nothing Then
                spriteBatch.Draw(CurrentFrame, CurrentPosition, CurrentSegment, Color.White)
                If Text IsNot Nothing Then
                    Dim size As Vector2 = CardGame.Font.MeasureString(Text)
                    Dim textPosition As New Vector2(CurrentPosition.X + CurrentFrame.Bounds.Width \ 2 - size.X / 2, CurrentPosition.Y + CurrentFrame.Bounds.Height \ 2 - size.Y / 2)

                    spriteBatch.DrawString(CardGame.Font, Text, textPosition, TextColor)
                End If
            End If
        End If

        spriteBatch.End()
    End Sub
#End Region

    ''' <summary>
    ''' Adds an animation to the animated component.
    ''' </summary>
    ''' <param name="animation">The animation to add.</param>
    Public Sub AddAnimation(ByVal animation As AnimatedGameComponentAnimation)
        animation.Component = Me
        runningAnimations.Add(animation)
    End Sub

    ''' <summary>
    ''' Calculate the estimated time at which the longest lasting animation currently managed 
    ''' will complete.
    ''' </summary>
    ''' <returns>The estimated time for animation complete </returns>
    Public Overridable Function EstimatedTimeForAnimationsCompletion() As TimeSpan
        Dim result As TimeSpan = TimeSpan.Zero

        If IsAnimating Then
            For animationIndex = 0 To runningAnimations.Count - 1
                If runningAnimations(animationIndex).EstimatedTimeForAnimationCompletion > result Then
                    result = runningAnimations(animationIndex).EstimatedTimeForAnimationCompletion
                End If
            Next animationIndex
        End If

        Return result
    End Function
End Class
