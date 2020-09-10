#Region "File Description"
'-----------------------------------------------------------------------------
' MathUtility.vb
'
' Microsoft XNA Community Game Platform
' Copyright (C) Microsoft Corporation. All rights reserved.
'-----------------------------------------------------------------------------
#End Region

#Region "Using Statements"
Imports System.Text
Imports Microsoft.Xna.Framework
#End Region


Public NotInheritable Class MathUtility
    ''' <summary>
    ''' Rotates a point around another specified point.
    ''' </summary>
    ''' <param name="point">The point to rotate.</param>
    ''' <param name="origin">The rotation origin or "axis".</param>
    ''' <param name="rotation">The rotation amount in radians.</param>
    ''' <returns>The position of the point after rotating it.</returns>
    Public Shared Function RotateAboutOrigin(ByVal point As Vector2, ByVal origin As Vector2, ByVal rotation As Single) As Vector2
        ' Point relative to origin   
        Dim u As Vector2 = point - origin

        If u = Vector2.Zero Then
            Return point
        End If

        ' Angle relative to origin   
        Dim a As Single = CSng(Math.Atan2(u.Y, u.X))

        ' Rotate   
        a += rotation

        ' U is now the new point relative to origin   
        u = u.Length() * New Vector2(CSng(Math.Cos(a)), CSng(Math.Sin(a)))
        Return u + origin
    End Function
End Class
