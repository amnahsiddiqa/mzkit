﻿Imports System.IO
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports Microsoft.VisualBasic.Data.IO

Public Class mzPackWriter : Inherits BinaryStreamWriter

    ''' <summary>
    ''' ``[readkey => tempfile]`` 
    ''' </summary>
    ReadOnly scanners As New Dictionary(Of String, String)
    ''' <summary>
    ''' temp file path of the thumbnail image
    ''' </summary>
    Dim thumbnail As String
    Dim scannerIndex As New Dictionary(Of String, Long)

    Public Sub New(file As String)
        MyBase.New(file)
    End Sub

    Private Sub writeScanners()
        Dim indexOffset As Long = file.Position

        ' index offset
        Call file.Write(0&)
        Call file.Flush()

        For Each scanner In scanners
            Dim start As Long = file.Position
            Dim bytes As Byte() = scanner.Value.ReadBinary

            Call file.Write(bytes.Length)
            Call file.Write(bytes)
            Call scannerIndex.Add(scanner.Key, start)
            Call file.Flush()
        Next

        Using file.TemporarySeek(indexOffset, SeekOrigin.Begin)
            Call file.Write(file.Position)
            Call file.Flush()
        End Using
    End Sub

    Private Sub writeScannerIndex()
        Call file.Write(scannerIndex.Count)

        For Each item In scannerIndex
            Call file.Write(item.Value)
            Call file.Write(item.Key, BinaryStringFormat.ZeroTerminated)
        Next

        Call file.Flush()
    End Sub

    Private Sub writeThumbnail()
        Dim start As Long = file.Position

        Call file.Write(thumbnail.ReadBinary)
        Call file.Write(start)
        Call file.Flush()
    End Sub

    Protected Overrides Sub writeIndex()
        ' write MS index
        MyBase.writeIndex()

        Call writeScanners()
        Call writeScannerIndex()
        Call writeThumbnail()
    End Sub
End Class