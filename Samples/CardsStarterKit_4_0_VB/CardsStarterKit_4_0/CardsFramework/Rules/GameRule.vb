#Region "File Description"
'-----------------------------------------------------------------------------
' GameRule.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
#End Region


''' <summary>
''' Represents a rule in card game.
''' </summary>
''' <remarks>
''' Inherit from this class and write your code
''' </remarks>
Public MustInherit Class GameRule
    ''' <summary>
    ''' An event which triggers when the rule conditions are matched.
    ''' </summary>
    Public Event RuleMatch As EventHandler

    ''' <summary>
    ''' Checks whether the rule conditions are met. Should call 
    ''' <see cref="FireRuleMatch"/>.
    ''' </summary>
    Public MustOverride Sub Check()

    ''' <summary>
    ''' Fires the rule's event.
    ''' </summary>
    ''' <param name="e">Event arguments.</param>
    Protected Sub FireRuleMatch(ByVal e As EventArgs)
        RaiseEvent RuleMatch(Me, e)
    End Sub
End Class
