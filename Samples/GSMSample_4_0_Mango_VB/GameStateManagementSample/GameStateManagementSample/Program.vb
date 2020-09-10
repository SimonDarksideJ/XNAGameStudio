Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

#If WINDOWS OrElse XBOX Then
Friend NotInheritable Class Program
    Private Sub New()
    End Sub
    Shared Sub Main(ByVal args() As String)
        Using game As New GameStateManagementGame()
            game.Run()
        End Using
    End Sub
End Class
#End If