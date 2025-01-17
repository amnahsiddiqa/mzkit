﻿Imports System.IO
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module Shimadzu

    Public Function CheckTableHeader(header As String()) As Boolean
        If header(Scan0) <> "X" Then
            Return False
        ElseIf header(1) <> "Y" Then
            Return False
        ElseIf header(2) <> "ROI" Then
            Return False
        ElseIf Not header.Skip(3).All(Function(str) str.IsSimpleNumber) Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Function GetFileTag(file As Stream) As String
        If TypeOf file Is FileStream Then
            Return DirectCast(file, FileStream).Name.FileName
        Else
            Return "Shimadzu iMScope TRIO"
        End If
    End Function

    Public Function ImportsMzPack(file As Stream,
                                  Optional sample As String = Nothing,
                                  Optional println As Action(Of String) = Nothing) As mzPack

        Using buffer As New StreamReader(file)
            Dim headers As String() = buffer.ReadLine.Split(","c)
            Dim mz As Vector = headers _
                .Skip(3) _
                .Select(AddressOf Double.Parse) _
                .AsVector
            Dim line As Value(Of String) = ""
            Dim tokens As String()
            Dim x, y As Integer
            Dim ROI As String
            Dim intensity As Vector
            Dim scans As New List(Of ScanMS1)
            Dim i As BooleanVector

            sample = If(sample.StringEmpty, GetFileTag(file), sample)

            Do While (line = buffer.ReadLine) IsNot Nothing
                tokens = line.Split(","c)
                x = Integer.Parse(tokens(0))
                y = Integer.Parse(tokens(1))
                ROI = tokens(2)
                intensity = tokens _
                    .Skip(3) _
                    .Select(AddressOf Double.Parse) _
                    .AsVector
                i = intensity > 0

                scans += New ScanMS1 With {
                    .BPC = intensity.Max,
                    .into = intensity(i),
                    .meta = New Dictionary(Of String, String) From {
                        {"sample", sample},
                        {"ROI", ROI},
                        {"x", x},
                        {"y", y}
                    },
                    .mz = mz(i),
                    .products = Nothing,
                    .rt = buffer.BaseStream.Position,
                    .TIC = intensity.Sum,
                    .scan_id = $"[MS1][{x},{y}][{sample}] Full Scan, ROI={ROI}, total_ions={ .TIC}"
                }

                If Not println Is Nothing Then
                    Call println(scans.Last.scan_id)
                End If
            Loop

            Return New mzPack With {
                .Application = FileApplicationClass.MSImaging,
                .MS = scans.ToArray,
                .source = sample
            }
        End Using
    End Function
End Module
