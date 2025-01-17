﻿#Region "Microsoft.VisualBasic::7823f1d416a9db71ba65825cebd855e2, mzkit\src\assembly\assembly\MarkupData\imzML\XML\MSISummary.vb"

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

'   Total Lines: 96
'    Code Lines: 75
' Comment Lines: 6
'   Blank Lines: 15
'     File Size: 3.40 KB


'     Class MSISummary
' 
'         Properties: rowScans, size
' 
'         Function: FromPixels, GetBasePeakMz, GetLayer, GetRowScan, ToArray
'                   ToString
' 
'     Enum IntensitySummary
' 
'         Average, BasePeak, Total
' 
'  
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq

Namespace MarkupData.imzML

    ''' <summary>
    ''' total ions/base peak/average intensity
    ''' </summary>
    Public Class MSISummary

        Public Property rowScans As iPixelIntensity()()
        ''' <summary>
        ''' the MALDI scan dimension size
        ''' </summary>
        ''' <returns></returns>
        Public Property size As Size

        Public Shared Function FromPixels(pixels As IEnumerable(Of iPixelIntensity), Optional dims As Size? = Nothing) As MSISummary
            Dim matrix2D As iPixelIntensity()() = pixels _
                .GroupBy(Function(p) p.y) _
                .OrderBy(Function(p) p.Key) _
                .Select(Function(row)
                            Return row.OrderBy(Function(p) p.x).ToArray
                        End Function) _
                .ToArray
            Dim width As Integer = matrix2D.Select(Function(p) p.Select(Function(pi) pi.x).Max).Max
            Dim height As Integer = matrix2D.Select(Function(p) p.Select(Function(pi) pi.y).Max).Max

            If dims Is Nothing Then
                Dim points = matrix2D _
                    .Select(Function(p)
                                Return p.Select(Function(pi) pi.GetPoint)
                            End Function) _
                    .IteratesALL
                Dim poly As New Polygon2D(points)

                dims = New Size(poly.xpoints.Max, poly.ypoints.Max)
            End If

            Return New MSISummary With {
                .rowScans = matrix2D,
                .size = dims
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToArray() As iPixelIntensity()
            Return rowScans.IteratesALL.ToArray
        End Function

        Public Function GetBasePeakMz() As LibraryMatrix
            Return New LibraryMatrix With {
                .centroid = False,
                .name = "basePeak M/z",
                .ms2 = rowScans _
                    .IteratesALL _
                    .Select(Function(p)
                                Return New ms2 With {
                                    .mz = p.basePeakMz,
                                    .intensity = p.basePeakIntensity,
                                    .Annotation = $"{p.x},{p.y}"
                                }
                            End Function) _
                    .ToArray
            }
        End Function

        Private Iterator Function GetRowScan(row As iPixelIntensity(), summary As IntensitySummary) As IEnumerable(Of PixelScanIntensity)
            For Each p As iPixelIntensity In row
                Dim into As Double

                Select Case summary
                    Case IntensitySummary.Average : into = p.average
                    Case IntensitySummary.BasePeak : into = p.basePeakIntensity
                    Case IntensitySummary.Total : into = p.totalIon
                    Case Else
                        Throw New NotImplementedException
                End Select

                Yield New PixelScanIntensity With {
                    .totalIon = into,
                    .x = p.x,
                    .y = p.y
                }
            Next
        End Function

        Public Function GetLayer(summary As IntensitySummary) As IEnumerable(Of PixelScanIntensity)
            Return rowScans _
                .Select(Function(row)
                            Return GetRowScan(row, summary)
                        End Function) _
                .IteratesALL
        End Function

        Public Overrides Function ToString() As String
            Return $"[width:={size.Width}, height:={size.Height}]"
        End Function

    End Class

    ''' <summary>
    ''' TIC/BPC/Average
    ''' </summary>
    Public Enum IntensitySummary As Integer
        ''' <summary>
        ''' sum all intensity signal value in a pixel
        ''' </summary>
        Total
        ''' <summary>
        ''' get a max intensity signal value in a pixel
        ''' </summary>
        BasePeak
        ''' <summary>
        ''' get the average intensity signal value in a pixel
        ''' </summary>
        Average
    End Enum

End Namespace
