﻿#Region "Microsoft.VisualBasic::bd7794616a13a9e2d0f68e823386acaa, mzkit\src\visualize\MsImaging\PixelScan\ibdPixel.vb"

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

    '   Total Lines: 88
    '    Code Lines: 68
    ' Comment Lines: 4
    '   Blank Lines: 16
    '     File Size: 2.97 KB


    '     Class ibdPixel
    ' 
    '         Properties: scanId, X, Y
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetMs, GetMsPipe, GetMzIonIntensity, HasAnyMzIon, ReadMz
    ' 
    '         Sub: release
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.imzML
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra

Namespace Pixel

    ''' <summary>
    ''' pixel model for the imzML file data
    ''' </summary>
    Public Class ibdPixel : Inherits PixelScan

        Public Overrides ReadOnly Property X As Integer
        Public Overrides ReadOnly Property Y As Integer

        ReadOnly i As ScanData
        ReadOnly raw As ibdReader

        Dim memoryCache As ms2()
        Dim enableCache As Boolean = False

        Public Overrides ReadOnly Property sampleTag As String
            Get
                Return raw.UUID
            End Get
        End Property

        Public Overrides ReadOnly Property scanId As String
            Get
                Return i.ToString
            End Get
        End Property

        <DebuggerStepThrough>
        Sub New(ibd As ibdReader, pixel As ScanData, Optional enableCache As Boolean = False)
            Me.i = pixel
            Me.raw = ibd
            Me.enableCache = enableCache
            Me.X = i.x
            Me.Y = i.y
        End Sub

        <DebuggerStepThrough>
        Sub New(x As Integer, y As Integer, cache As IEnumerable(Of ms2))
            Me.memoryCache = cache.ToArray
            Me.enableCache = True
            Me.X = x
            Me.Y = y
        End Sub

        Public Overrides Function GetMs() As ms2()
            If Not enableCache Then
                Return raw.GetMSMSPipe(i).ToArray
            Else
                ' 有些像素点是空向量
                ' 所以就只判断nothing而不判断empty了
                If memoryCache Is Nothing Then
                    memoryCache = raw.GetMSMSPipe(i).ToArray
                End If

                Return memoryCache
            End If
        End Function

        Public Overrides Function HasAnyMzIon() As Boolean
            Return Not GetMs.IsNullOrEmpty
        End Function

        Protected Friend Overrides Function GetMsPipe() As IEnumerable(Of ms2)
            Return raw.GetMSMSPipe(i)
        End Function

        Public Function ReadMz() As Double()
            If Not enableCache Then
                Return raw.ReadArray(i.MzPtr)
                ' 有些像素点是空向量
                ' 所以就只判断nothing而不判断empty了
            ElseIf memoryCache.IsNullOrEmpty Then
                Return raw.ReadArray(i.MzPtr)
            Else
                Return (From m As ms2 In memoryCache Select m.mz).ToArray
            End If
        End Function

        Public Overrides Function HasAnyMzIon(mz() As Double, tolerance As Tolerance) As Boolean
            Dim mzlist As Double() = ReadMz()

            Return mz _
                .Any(Function(mzi)
                         Return mzlist.Any(Function(zzz) tolerance(zzz, mzi))
                     End Function)
        End Function

        Protected Friend Overrides Sub release()
            Erase memoryCache
        End Sub

        Public Overrides Function GetMzIonIntensity() As Double()
            Return GetMsPipe.Select(Function(i) i.intensity).ToArray
        End Function
    End Class
End Namespace
