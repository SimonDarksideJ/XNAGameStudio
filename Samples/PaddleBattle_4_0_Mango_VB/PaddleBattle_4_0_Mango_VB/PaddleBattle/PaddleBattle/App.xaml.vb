#Region "File Description"
'-----------------------------------------------------------------------------
' App.xaml.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region
Imports System.Windows
Imports System.Windows.Navigation
Imports Microsoft.Phone.Controls
Imports Microsoft.Phone.Shell
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Content

' The App implements IServiceProvider for ContentManagers and other types to
' be able to access the graphics device services.
Partial Public Class App
    Inherits Application
    Implements IServiceProvider
    ''' <summary>
    ''' Provides easy access to the root frame of the Phone Application.
    ''' </summary>
    ''' The root frame of the Phone Application.
    Private privateRootFrame As PhoneApplicationFrame
    Public Property RootFrame() As PhoneApplicationFrame
        Get
            Return privateRootFrame
        End Get
        Private Set(ByVal value As PhoneApplicationFrame)
            privateRootFrame = value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to a ContentManager for the application.
    ''' </summary>
    Private privateContent As ContentManager
    Public Property Content() As ContentManager
        Get
            Return privateContent
        End Get
        Private Set(ByVal value As ContentManager)
            privateContent = value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to a GameTimer that is set up to pump the FrameworkDispatcher.
    ''' </summary>
    Private privateFrameworkDispatcherTimer As GameTimer
    Public Property FrameworkDispatcherTimer() As GameTimer
        Get
            Return privateFrameworkDispatcherTimer
        End Get
        Private Set(ByVal value As GameTimer)
            privateFrameworkDispatcherTimer = value
        End Set
    End Property

    ''' <summary>
    ''' Provides the settings for the game.
    ''' </summary>
    Private privateSettings As GameSettings
    Public Property Settings() As GameSettings
        Get
            Return privateSettings
        End Get
        Private Set(ByVal value As GameSettings)
            privateSettings = value
        End Set
    End Property

    ''' <summary>
    ''' Constructor for the Application object.
    ''' </summary>
    Public Sub New()
        ' Construct the settings for the game
        Settings = New GameSettings()

        ' Global handler for uncaught exceptions. 
        AddHandler UnhandledException, AddressOf Application_UnhandledException

        ' Show graphics profiling information while debugging.
        If System.Diagnostics.Debugger.IsAttached Then
            ' Display the current frame rate counters.
            Application.Current.Host.Settings.EnableFrameRateCounter = True

            ' Show the areas of the app that are being redrawn in each frame.
            'Application.Current.Host.Settings.EnableRedrawRegions = true;

            ' Enable non-production analysis visualization mode, 
            ' which shows areas of a page that are being GPU accelerated with a colored overlay.
            'Application.Current.Host.Settings.EnableCacheVisualization = true;
        End If

        ' Standard Silverlight initialization
        InitializeComponent()

        ' Phone-specific initialization
        InitializePhoneApplication()

        ' XNA initialization
        InitializeXnaApplication()
    End Sub

    ' Code to execute when the application is launching (eg, from Start)
    ' This code will not execute when the application is reactivated
    Private Sub Application_Launching(ByVal sender As Object, ByVal e As LaunchingEventArgs)
        ' Load the settings for the game
        Settings.Load()
    End Sub

    ' Code to execute when the application is activated (brought to foreground)
    ' This code will not execute when the application is first launched
    Private Sub Application_Activated(ByVal sender As Object, ByVal e As ActivatedEventArgs)
        ' Load the settings for the game
        Settings.Load()
    End Sub

    ' Code to execute when the application is deactivated (sent to background)
    ' This code will not execute when the application is closing
    Private Sub Application_Deactivated(ByVal sender As Object, ByVal e As DeactivatedEventArgs)
        ' Save the settings for the game
        Settings.Save()
    End Sub

    ' Code to execute when the application is closing (eg, user hit Back)
    ' This code will not execute when the application is deactivated
    Private Sub Application_Closing(ByVal sender As Object, ByVal e As ClosingEventArgs)
        ' Save the settings for the game
        Settings.Save()
    End Sub

    ' Code to execute if a navigation fails
    Private Sub RootFrame_NavigationFailed(ByVal sender As Object, ByVal e As NavigationFailedEventArgs)
        If System.Diagnostics.Debugger.IsAttached Then
            ' A navigation has failed; break into the debugger
            System.Diagnostics.Debugger.Break()
        End If
    End Sub

    ' Code to execute on Unhandled Exceptions
    Private Sub Application_UnhandledException(ByVal sender As Object, ByVal e As ApplicationUnhandledExceptionEventArgs)
        If System.Diagnostics.Debugger.IsAttached Then
            ' An unhandled exception has occurred; break into the debugger
            System.Diagnostics.Debugger.Break()
        End If
    End Sub

#Region "Phone application initialization"

    ' Avoid double-initialization
    Private phoneApplicationInitialized As Boolean = False

    ' Do not add any additional code to this method
    Private Sub InitializePhoneApplication()
        If phoneApplicationInitialized Then
            Return
        End If

        ' Create the frame but don't set it as RootVisual yet; this allows the splash
        ' screen to remain active until the application is ready to render.
        RootFrame = New PhoneApplicationFrame()
        AddHandler RootFrame.Navigated, AddressOf CompleteInitializePhoneApplication

        ' Handle navigation failures
        AddHandler RootFrame.NavigationFailed, AddressOf RootFrame_NavigationFailed

        ' Ensure we don't initialize again
        phoneApplicationInitialized = True
    End Sub

    ' Do not add any additional code to this method
    Private Sub CompleteInitializePhoneApplication(ByVal sender As Object, ByVal e As NavigationEventArgs)
        ' Set the root visual to allow the application to render
        If RootVisual IsNot RootFrame Then
            RootVisual = RootFrame
        End If

        ' Remove this handler since it is no longer needed
        RemoveHandler RootFrame.Navigated, AddressOf CompleteInitializePhoneApplication
    End Sub

#End Region

#Region "XNA application initialization"

    ' Performs initialization of the XNA types required for the application.
    Private Sub InitializeXnaApplication()
        ' Create the ContentManager so the application can load precompiled assets
        Content = New ContentManager(Me, "Content")

        ' Create a GameTimer to pump the XNA FrameworkDispatcher
        FrameworkDispatcherTimer = New GameTimer()
        AddHandler FrameworkDispatcherTimer.FrameAction, AddressOf FrameworkDispatcherFrameAction
        FrameworkDispatcherTimer.Start()
    End Sub

    ' An event handler that pumps the FrameworkDispatcher each frame.
    ' FrameworkDispatcher is required for a lot of the XNA events and
    ' for certain functionality such as SoundEffect playback.
    Private Sub FrameworkDispatcherFrameAction(ByVal sender As Object, ByVal e As EventArgs)
        FrameworkDispatcher.Update()
    End Sub

#End Region

#Region "IServiceProvider Members"

    ''' <summary>
    ''' Gets a service from the ApplicationLifetimeObjects collection.
    ''' </summary>
    ''' <param name="serviceType">The type of service to retrieve.</param>
    ''' <returns>The first item in the ApplicationLifetimeObjects collection of the requested type.</returns>
    Public Function GetService(ByVal serviceType As Type) As Object Implements IServiceProvider.GetService
        ' Find the first item that matches the requested type
        For Each item As Object In ApplicationLifetimeObjects
            If serviceType.IsAssignableFrom(item.GetType()) Then
                Return item
            End If
        Next item

        ' Throw an exception if there was no matching item
        Throw New InvalidOperationException("No object in the ApplicationLifetimeObjects is assignable to " & serviceType.ToString())
    End Function

#End Region
End Class
