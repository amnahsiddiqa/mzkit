﻿#Region "Microsoft.VisualBasic::6faa14831f7bab9d50867e4a40bae482, mzkit\src\visualize\MsImaging\PixelScan\mzPackPixel.vb"

' Author:
' 
'       xieguigang (gg.xie@bionovogene.com, BioNovoGene Co., LTD.)
' 
' Copyright (c) 2018 gg.xie@bionovogene.com, BioNovoGene Co., LTD.
' 
' 
' MIT License
' 
' 
' Permission is hereby granted, free of charge, to any person obtaining a copy
' of this software and associated documentation files (the "Software"), to deal
' in the Software without restriction, including without limitation the rights
' to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
' copies of the Software, and to permit persons to whom the Software is
' furnished to do so, subject to the following conditions:
' 
' The above copyright notice and this permission notice shall be included in all
' copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
' OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
' SOFTWARE.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 95
'    Code Lines: 79
' Comment Lines: 0
'   Blank Lines: 16
'     File Size: 3.11 KB


'     Class mzPackPixel
' 
'         Properties: mz, scan, scanId, X, Y
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: GetMsPipe, GetMzIonIntensity, GetPixelPoint, HasAnyMzIon
' 
'         Sub: release
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.Linq

Namespace Pixel

    Public Class mzPackPixel : Inherits PixelScan

        Public Overrides ReadOnly Property X As Integer
            Get
                Return pixel.X
            End Get
        End Property

        Public Overrides ReadOnly Property Y As Integer
            Get
                Return pixel.Y
            End Get
        End Property

        Public ReadOnly Property scan As ScanMS1

        ReadOnly pixel As Point

        Public Overrides ReadOnly Property scanId As String
            Get
                Return scan.scan_id
            End Get
        End Property

        Public ReadOnly Property mz As Double()
            Get
                Return scan.mz
            End Get
        End Property

        Public Overrides ReadOnly Property sampleTag As String
            Get
                If scan.hasMetaKeys(mzStreamWriter.SampleMetaName) Then
                    Return scan.meta(mzStreamWriter.SampleMetaName)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Sub New(scan As ScanMS1, Optional x As Integer = Integer.MinValue, Optional y As Integer = Integer.MinValue)
            'Dim ms1 As ms2() = scan _
            '    .GetMs _
            '    .OrderBy(Function(mzi) mzi.mz) _
            '    .ToArray

            Me.scan = scan
            'Me.scan.mz = ms1.Select(Function(a) a.mz).ToArray
            'Me.scan.into = ms1.Select(Function(a) a.intensity).ToArray

            If x = Integer.MinValue OrElse y = Integer.MinValue Then
                Me.pixel = scan.GetMSIPixel
            Else
                Me.pixel = New Point(x, y)
            End If
        End Sub

        Public Overrides Function HasAnyMzIon() As Boolean
            Return scan.size > 0
        End Function

        Protected Friend Overrides Function GetMsPipe() As IEnumerable(Of ms2)
            Return scan.GetMs
        End Function

        Public Overrides Function HasAnyMzIon(mz() As Double, tolerance As Tolerance) As Boolean
            Return mz _
                .Any(Function(mzi)
                         Return scan.mz.Any(Function(zzz) tolerance(zzz, mzi))
                     End Function)
        End Function

        Protected Friend Overrides Sub release()
            If Not scan Is Nothing Then
                Erase scan.into
                Erase scan.mz
                Erase scan.products
            End If
        End Sub

        Public Overrides Function GetMzIonIntensity(mz As Double, mzdiff As Tolerance) As Double
            Dim allMatched As New List(Of Double)
            Dim mzRaw As Double() = scan.mz

            For i As Integer = 0 To scan.mz.Length - 1
                If mzdiff(mzRaw(i), mz) Then
                    Call allMatched.Add(scan.into(i))
                End If
            Next

            If allMatched.GetLength = 0 Then
                Return 0
            Else
                Return allMatched.Max
            End If
        End Function

        Public Overrides Function GetMzIonIntensity() As Double()
            Return scan.into
        End Function
    End Class
End Namespace
