﻿#Region "Microsoft.VisualBasic::09e1f2c86f808b0e0155b6ec8d4f4a72, mzkit\src\visualize\MsImaging\PixelData.vb"

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

'   Total Lines: 167
'    Code Lines: 119
' Comment Lines: 28
'   Blank Lines: 20
'     File Size: 6.01 KB


' Class PixelData
' 
'     Properties: intensity, level, mz, x, y
' 
'     Constructor: (+2 Overloads) Sub New
'     Function: GetBuffer, getIntensityAuto, Parse, ScalePixels, SequenceIndex
'               ToString
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.imzML
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap
Imports Microsoft.VisualBasic.Serialization.JSON
Imports HeatMapPixel = Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap.Pixel

''' <summary>
''' a pixels point of [x,y,color]
''' </summary>
''' <remarks>
''' the property <see cref="PixelData.intensity"/> value is the
''' <see cref="HeatMapPixel.Scale"/> value that could be used for 
''' create heatmap raster data.
''' </remarks>
Public Class PixelData : Implements IMSIPixel, IPoint2D, HeatMapPixel

    Public Property x As Integer Implements IMSIPixel.x, IPoint2D.X, HeatMapPixel.X
    Public Property y As Integer Implements IMSIPixel.y, IPoint2D.Y, HeatMapPixel.Y
    Public Property intensity As Double Implements IMSIPixel.intensity, HeatMapPixel.Scale
    Public Property level As Double
    Public Property mz As Double
    Public Property sampleTag As String

    Sub New()
    End Sub

    Sub New(x As Integer, y As Integer, into As Double)
        Me.x = x
        Me.y = y
        Me.intensity = into
    End Sub

    Public Overrides Function ToString() As String
        Return $"Dim [{x},{y}] as intensity = {intensity}"
    End Function

    Public Shared Function GetBuffer(data As PixelData()) As Byte()
        Using buf As New MemoryStream, file As New BinaryDataWriter(buf)
            Call file.Write(data.Length)
            Call file.Write(data.Select(Function(i) i.x).ToArray)
            Call file.Write(data.Select(Function(i) i.y).ToArray)
            Call file.Write(data.Select(Function(i) i.intensity).ToArray)
            Call file.Write(data.Select(Function(i) i.level).ToArray)
            Call file.Write(data.Select(Function(i) i.mz).ToArray)

            Dim tags As String() = data.Select(Function(i) i.sampleTag).ToArray

            If tags.Distinct.Count = 1 Then
                Call file.Write(-1)
                Call file.Write(If(tags(Scan0), "sample"), BinaryStringFormat.ZeroTerminated)
            Else
                Dim tagIndex = tags.Distinct.Indexing

                Call file.Write(tagIndex.Count)
                Call file.Write(tagIndex.Objects.GetJson, BinaryStringFormat.ZeroTerminated)
                Call file.Write(tags.Select(Function(str) tagIndex.IndexOf(str)).ToArray)
            End If

            Call file.Flush()

            Return buf.ToArray
        End Using
    End Function

    Public Shared Function Parse(data As Byte()) As PixelData()
        Using file As New BinaryDataReader(New MemoryStream(data))
            Dim size As Integer = file.ReadInt32
            Dim x As Integer() = file.ReadInt32s(size)
            Dim y As Integer() = file.ReadInt32s(size)
            Dim intensity As Double() = file.ReadDoubles(size)
            Dim level As Double() = file.ReadDoubles(size)
            Dim mz As Double() = file.ReadDoubles(size)
            Dim flag As Integer = file.ReadInt32
            Dim tags As String()

            If flag = -1 Then
                ' one for all
                Dim tagAll As String = file.ReadString(BinaryStringFormat.ZeroTerminated)

                tags = tagAll.Replicate(size).ToArray
            Else
                Dim tagVector As String() = file _
                    .ReadString(BinaryStringFormat.ZeroTerminated) _
                    .LoadJSON(Of String())

                tags = file _
                    .ReadInt32s(size) _
                    .Select(Function(i) tagVector(i)) _
                    .ToArray
            End If

            Return x _
                .Select(Function(xi, i)
                            Return New PixelData With {
                                .x = xi,
                                .y = y(i),
                                .intensity = intensity(i),
                                .level = level(i),
                                .mz = mz(i),
                                .sampleTag = tags(i)
                            }
                        End Function) _
                .ToArray
        End Using
    End Function

    ''' <summary>
    ''' 将响应度数据统一缩放到[0,1]之间
    ''' </summary>
    ''' <param name="pixels"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' <see cref="HeatMapRaster(Of PixelData)"/>
    ''' </remarks>
    Public Shared Iterator Function ScalePixels(pixels As PixelData()) As IEnumerable(Of PixelData)
        Dim level As Double
        Dim levelRange As DoubleRange = New Double() {0, 1}
        Dim intensityRange As DoubleRange = pixels _
            .Select(Function(p)
                        Return p.intensity
                    End Function) _
            .Range()
        Dim removesFilterSmall As Boolean = intensityRange.Max > 10

        For Each point As PixelData In pixels
            If removesFilterSmall AndAlso point.intensity < 1 Then
                Continue For
            End If

            level = intensityRange.ScaleMapping(point.intensity, levelRange)

            If level > 1 Then
                point.level = 1
            ElseIf level <= 0 Then
                level = -1
            Else
                point.level = level
            End If

            Yield point
        Next
    End Function

    ''' <summary>
    ''' 将一个二维坐标信息转换为序列索引号
    ''' </summary>
    ''' <param name="size"></param>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' 这个函数一般是用于处理mzpack文件中的原始数据
    ''' 并且mzpack文件中的像素点信息应该是按行扫描得到的结果
    ''' </remarks>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function SequenceIndex(size As Size, x As Integer, y As Integer) As Integer
        Return y * size.Width + x
    End Function
End Class
