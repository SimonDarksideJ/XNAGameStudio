Imports Microsoft.VisualBasic
Imports System
Imports System.Windows
Imports System.Windows.Navigation
Imports Microsoft.Phone.Controls
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Audio
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input.Touch

Partial Public Class GamePage
    Inherits PhoneApplicationPage
    Private content As ContentManager
    Private timer As GameTimer
    Private spriteBatch As SpriteBatch
    Private random As New Random()

    ' Blank texture used to draw the center line of the play area
    Private blank As Texture2D

    ' The settings for the game
    Private settings As GameSettings

    ' UIElementRenderer allows us to draw Silverlight UI into a Texture2D, which we can then draw
    ' using SpriteBatch to composite UI on top of our game
    Private uiRenderer As UIElementRenderer

    ' Sound effects that play when the ball bounces off something or a player scores
    Private plink As SoundEffect
    Private score As SoundEffect

    ' The two paddles and the ball
    Private playerPaddle As New Paddle(False)
    Private computerPaddle As New Paddle(True)
    Private ball As New Ball()

    ' The scores for the game
    Private playerScore As Integer = 0
    Private computerScore As Integer = 0

    Public Sub New()
        InitializeComponent()

        ' Get the content manager from the application
        content = (TryCast(Application.Current, App)).Content

        ' Get the settings from the application;
        settings = (TryCast(Application.Current, App)).Settings

        ' Create a timer for this page
        timer = New GameTimer()
        timer.UpdateInterval = TimeSpan.FromTicks(333333)
        AddHandler timer.Update, AddressOf OnUpdate
        AddHandler timer.Draw, AddressOf OnDraw

        ' We use the LayoutUpdated event in order to handle creation of the UIElementRenderer
        AddHandler LayoutUpdated, AddressOf GamePage_LayoutUpdated
    End Sub

    Protected Overrides Sub OnNavigatedTo(ByVal e As NavigationEventArgs)
        ' Set the sharing mode of the graphics device to turn on XNA rendering
        SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(True)

        ' Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = New SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice)

        ' Create the blank texture
        blank = New Texture2D(SharedGraphicsDeviceManager.Current.GraphicsDevice, 1, 1)
        blank.SetData({Color.White})

        ' Load the sound effects
        plink = content.Load(Of SoundEffect)("plink")
        score = content.Load(Of SoundEffect)("score")

        ' Load our three objects
        ball.LoadContent(content)
        playerPaddle.LoadContent(content)
        computerPaddle.LoadContent(content)

        ' Center each paddle on their respective sides
        playerPaddle.CenterAtLocation(New Vector2(100, 240))
        computerPaddle.CenterAtLocation(New Vector2(700, 240))

        ' Reset the ball
        ResetBall()

        ' Start the timer
        timer.Start()

        MyBase.OnNavigatedTo(e)
    End Sub

    Protected Overrides Sub OnNavigatedFrom(ByVal e As NavigationEventArgs)
        ' Stop the timer
        timer.Stop()

        ' Set the sharing mode of the graphics device to turn off XNA rendering
        SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(False)

        MyBase.OnNavigatedFrom(e)
    End Sub

    ''' <summary>
    ''' Helper that plays a sound, respective the game options
    ''' </summary>
    Private Sub PlaySound(ByVal sound As SoundEffect)
        If settings.PlaySounds Then
            sound.Play()
        End If
    End Sub

    ''' <summary>
    ''' Resets the ball to the center of the screen and generates a new velocity for it.
    ''' </summary>
    Private Sub ResetBall()
        ' Put the ball in the center of the screen
        ball.CenterAtLocation(New Vector2(400, 240))

        ' Generate an angle for the ball to be launched
        Dim angle As Single = MathHelper.ToRadians(random.Next(-45, 45))

        ' Figure out which player the ball is going towards first
        Dim towardsPlayer As Boolean = random.Next() Mod 2 = 0

        ' Apply the player decision to flip the angle if it's to head at the player
        If towardsPlayer Then
            angle += MathHelper.Pi
        End If

        ' Generate the velocity from this angle
        ball.Velocity = New Vector2(CSng(Math.Cos(angle)), CSng(Math.Sin(angle))) * 300.0F
    End Sub

    ''' <summary>
    ''' Allows the page to run logic such as updating the world,
    ''' checking for collisions, gathering input, and playing audio.
    ''' </summary>
    Private Sub OnUpdate(ByVal sender As Object, ByVal e As GameTimerEventArgs)
        ' Check for input which will drive the player's paddle
        Dim touches As TouchCollection = TouchPanel.GetState()
        If touches.Count > 0 Then
            ' We just use the first touch present
            Dim touch As TouchLocation = touches(0)

            ' Center the paddle vertically on the user's touch, based on the paddle's base collision bounds
            playerPaddle.Position.Y = touch.Position.Y - playerPaddle.BaseCollisionBounds.Height \ 2

            ' Make sure the player can't leave the screen
            playerPaddle.ClampToScreen()
        End If

        ' Update the ball
        ball.Update(e.ElapsedTime)

        ' And run the AI for the computer paddle
        computerPaddle.UpdateAI(e.ElapsedTime, ball)

        ' We now want to clamp the ball so it can't leave the top or bottom of the screen
        If ball.Bounds.Bottom > 480 Then
            ' Resolve the collision and reverse the ball's direction
            ball.Position.Y = 480 - ball.Bounds.Height
            ball.Velocity.Y *= -1

            ' Play our sound effect
            PlaySound(plink)
        ElseIf ball.Bounds.Top < 0 Then
            ' Resolve the collision and reverse the ball's direction
            ball.Position.Y = ball.BaseCollisionBounds.Y
            ball.Velocity.Y *= -1

            ' Play our sound effect
            PlaySound(plink)
        End If

        ' Check for collisions between the ball and the paddles based on the direction of the ball
        If ball.Velocity.X < 0 Then
            If playerPaddle.Collide(ball) Then
                PlaySound(plink)
            End If
        ElseIf ball.Velocity.X > 0 Then
            If computerPaddle.Collide(ball) Then
                PlaySound(plink)
            End If
        End If

        ' Check if the ball is off the screen
        Dim result As Integer = ball.IsOffscreen()

        ' If result is 1, the player scored a point
        If result = 1 Then
            ' Add a point to the player
            IncrementPlayerScore()

            ' Play the score sound effect
            PlaySound(score)

            ' Reset the ball for the next point
            ResetBall()

            ' Otherwise if the result is -1, the computer scored a point
        ElseIf result = -1 Then
            ' Add a point to the computer
            IncrementComputerScore()

            ' Play the score sound effect
            PlaySound(score)

            ' Reset the ball for the next point
            ResetBall()
        End If
    End Sub

    ''' <summary>
    ''' Helper that increments the player's score and updates the UI text.
    ''' </summary>
    Private Sub IncrementPlayerScore()
        playerScore += 1
        playerScoreTextBlock.Text = String.Format("Player Score: {0}", playerScore)
    End Sub

    ''' <summary>
    ''' Helper that increments the computer's score and updates the UI text.
    ''' </summary>
    Private Sub IncrementComputerScore()
        computerScore += 1
        computerScoreTextBlock.Text = String.Format("Computer Score: {0}", computerScore)
    End Sub

    ''' <summary>
    ''' Allows the page to draw itself.
    ''' </summary>
    Private Sub OnDraw(ByVal sender As Object, ByVal e As GameTimerEventArgs)
        ' Draw the Silverlight UI into the texture
        uiRenderer.Render()

        SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue)

        spriteBatch.Begin()

        ' Draw the center line of the game
        spriteBatch.Draw(blank, New Rectangle(398, 0, 4, 480), Color.White * 0.8F)

        ' Draw our three game objects
        playerPaddle.Draw(spriteBatch)
        computerPaddle.Draw(spriteBatch)
        ball.Draw(spriteBatch)

        ' Draw the UI on top of the scene
        spriteBatch.Draw(uiRenderer.Texture, Vector2.Zero, Color.White)
        spriteBatch.End()
    End Sub

    ''' <summary>
    ''' Invoked when the page's layout changes so we can make sure our UIElementRenderer is properly
    ''' constructed for the appropriate size.
    ''' </summary>
    Private Sub GamePage_LayoutUpdated(ByVal sender As Object, ByVal e As EventArgs)
        Dim width As Integer = CInt(ActualWidth)
        Dim height As Integer = CInt(ActualHeight)

        ' Ensure the page size is valid
        If width <= 0 OrElse height <= 0 Then
            Return
        End If

        ' Do we already have a UIElementRenderer of the correct size?
        If uiRenderer IsNot Nothing AndAlso uiRenderer.Texture IsNot Nothing AndAlso uiRenderer.Texture.Width = width AndAlso uiRenderer.Texture.Height = height Then
            Return
        End If

        ' Before constructing a new UIElementRenderer, be sure to Dispose the old one
        If uiRenderer IsNot Nothing Then
            uiRenderer.Dispose()
        End If

        ' Create the renderer
        uiRenderer = New UIElementRenderer(Me, width, height)
    End Sub
End Class