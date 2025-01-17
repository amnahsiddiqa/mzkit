﻿Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace SCiLSLab

    Public Class SpotPack : Inherits PackFile

        Public Property index As Dictionary(Of String, SpotSite)

        Public Function X() As Double()
            Return (From p As SpotSite
                    In index.Values
                    Let xi As Double = p.x
                    Select xi).ToArray
        End Function

        Public Function Y() As Double()
            Return (From p As SpotSite
                    In index.Values
                    Let yi As Double = p.y
                    Select yi).ToArray
        End Function

        Public Shared Function ParseFile(file As Stream) As SpotPack
            Dim pull As New SpotPack
            Dim spots As SpotSite() = ParseTable(
                file:=file,
                byrefPack:=pull,
                parseLine:=AddressOf SpotSite.Parse,
                println:=Sub(text)
                             ' do nothing
                         End Sub).ToArray

            pull.index = spots.ToDictionary(Function(sp) sp.index)

            Return pull
        End Function
    End Class

    Public Class SpotSite

        Public Property index As String
        Public Property x As Double
        Public Property y As Double

        Public Overrides Function ToString() As String
            Return $"spot_{index} [{x},{y}]"
        End Function

        Friend Shared Function Parse(row As String(), headers As Index(Of String), println As Action(Of String)) As SpotSite
            ' Spot index;x;y
            Dim index As String = row(headers("Spot index"))
            Dim x As Double = Val(row(headers("x")))
            Dim y As Double = Val(row(headers("y")))

            Return New SpotSite With {
                .index = index,
                .x = x,
                .y = y
            }
        End Function

    End Class
End Namespace