#Region "File Description"
'-----------------------------------------------------------------------------
' OptionsMenuScreen.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports Microsoft.Xna.Framework
#End Region

''' <summary>
''' The options screen is brought up over the top of the main menu
''' screen, and gives the user a chance to configure the game
''' in various hopefully useful ways.
''' </summary>
Friend Class OptionsMenuScreen
    Inherits MenuScreen
#Region "Fields"

    Private ungulateMenuEntry As MenuEntry
    Private languageMenuEntry As MenuEntry
    Private frobnicateMenuEntry As MenuEntry
    Private elfMenuEntry As MenuEntry

    Private Enum Ungulate
        BactrianCamel
        Dromedary
        Llama
    End Enum

    Private Shared currentUngulate As Ungulate = Ungulate.Dromedary

    Private Shared languages() As String = {"C#", "French", "Deoxyribonucleic acid"}
    Private Shared currentLanguage As Integer = 0

    Private Shared frobnicate As Boolean = True

    Private Shared elf As Integer = 23

#End Region

#Region "Initialization"


    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New()
        MyBase.New("Options")
        ' Create our menu entries.
        ungulateMenuEntry = New MenuEntry(String.Empty)
        languageMenuEntry = New MenuEntry(String.Empty)
        frobnicateMenuEntry = New MenuEntry(String.Empty)
        elfMenuEntry = New MenuEntry(String.Empty)

        SetMenuEntryText()

        Dim back As New MenuEntry("Back")

        ' Hook up menu event handlers.
        AddHandler ungulateMenuEntry.Selected, AddressOf UngulateMenuEntrySelected
        AddHandler languageMenuEntry.Selected, AddressOf LanguageMenuEntrySelected
        AddHandler frobnicateMenuEntry.Selected, AddressOf FrobnicateMenuEntrySelected
        AddHandler elfMenuEntry.Selected, AddressOf ElfMenuEntrySelected
        AddHandler back.Selected, AddressOf OnCancel

        ' Add entries to the menu.
        MenuEntries.Add(ungulateMenuEntry)
        MenuEntries.Add(languageMenuEntry)
        MenuEntries.Add(frobnicateMenuEntry)
        MenuEntries.Add(elfMenuEntry)
        MenuEntries.Add(back)
    End Sub


    ''' <summary>
    ''' Fills in the latest values for the options screen menu text.
    ''' </summary>
    Private Sub SetMenuEntryText()
        ungulateMenuEntry.Text = "Preferred ungulate: " & currentUngulate
        languageMenuEntry.Text = "Language: " & languages(currentLanguage)
        frobnicateMenuEntry.Text = "Frobnicate: " & (If(frobnicate, "on", "off"))
        elfMenuEntry.Text = "elf: " & elf
    End Sub


#End Region

#Region "Handle Input"


    ''' <summary>
    ''' Event handler for when the Ungulate menu entry is selected.
    ''' </summary>
    Private Sub UngulateMenuEntrySelected(ByVal sender As Object, ByVal e As PlayerIndexEventArgs)
        currentUngulate = CType(currentUngulate + 1, OptionsMenuScreen.Ungulate)

        If currentUngulate > Ungulate.Llama Then
            currentUngulate = 0
        End If

        SetMenuEntryText()
    End Sub


    ''' <summary>
    ''' Event handler for when the Language menu entry is selected.
    ''' </summary>
    Private Sub LanguageMenuEntrySelected(ByVal sender As Object, ByVal e As PlayerIndexEventArgs)
        currentLanguage = (currentLanguage + 1) Mod languages.Length

        SetMenuEntryText()
    End Sub


    ''' <summary>
    ''' Event handler for when the Frobnicate menu entry is selected.
    ''' </summary>
    Private Sub FrobnicateMenuEntrySelected(ByVal sender As Object, ByVal e As PlayerIndexEventArgs)
        frobnicate = Not frobnicate

        SetMenuEntryText()
    End Sub


    ''' <summary>
    ''' Event handler for when the Elf menu entry is selected.
    ''' </summary>
    Private Sub ElfMenuEntrySelected(ByVal sender As Object, ByVal e As PlayerIndexEventArgs)
        elf += 1

        SetMenuEntryText()
    End Sub


#End Region
End Class