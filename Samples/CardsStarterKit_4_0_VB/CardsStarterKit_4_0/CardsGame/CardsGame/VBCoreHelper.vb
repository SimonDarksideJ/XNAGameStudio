Module VBCoreHelper

    Public Function Fix(ByVal Number As Short) As Short
        Return Number
    End Function

    Public Function Fix(ByVal Number As Integer) As Integer
        Return Number
    End Function

    Public Function Fix(ByVal Number As Long) As Integer
        Return CType(Number, Integer)
    End Function

    Public Function Fix(ByVal Number As Double) As Double
        If (Number >= 0) Then
            Return Math.Floor(Number)
        End If
        Return -Math.Floor(-Number)
    End Function

    Public Function Fix(ByVal Number As Single) As Single
        If (Number >= 0) Then
            Return CType(Math.Floor(Number), Single)
        End If
        Return CType(-Math.Floor(-Number), Single)
    End Function

    Public Function Fix(ByVal Number As Decimal) As Decimal
        If (Number < Decimal.Zero) Then
            Return Decimal.Negate(Decimal.Floor(Decimal.Negate(Number)))
        End If
        Return Decimal.Floor(Number)
    End Function


    Public Function Fix(ByVal Number As Object) As Object
        If (Number Is Nothing) Then
            Throw New ArgumentNullException("Argument_Invalid Null Value")
        End If
        Dim convertible As IConvertible = TryCast(Number, IConvertible)
        If (Not convertible Is Nothing) Then
            Select Case convertible.GetTypeCode
                Case TypeCode.Boolean
                    Return convertible.ToInt32(Nothing)
                Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64
                    Return Number
                Case TypeCode.Single
                    Return Fix(convertible.ToSingle(Nothing))
                Case TypeCode.Double
                    Return Fix(convertible.ToDouble(Nothing))
                Case TypeCode.Decimal
                    Return Fix(convertible.ToDecimal(Nothing))
                Case TypeCode.String
                    Return Fix(CType(convertible.ToString(Nothing), Double))
            End Select
        End If
        Throw New ArgumentException("Argument_NotNumeric Type")
    End Function
End Module
