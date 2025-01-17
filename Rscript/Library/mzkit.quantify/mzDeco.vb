﻿#Region "Microsoft.VisualBasic::fd5e50c47c702d05f5ba552d93385afe, Rscript\Library\mzkit.quantify\mzDeco.vb"

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

' Module mzDeco
' 
'     Function: ms1Scans, mz_deco, mz_groups
' 
' /********************************************************************************/

#End Region


Imports BioNovoGene.Analytical.MassSpectrometry.Assembly
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("mzDeco")>
Module mzDeco

    ''' <summary>
    ''' Chromatogram data deconvolution
    ''' </summary>
    ''' <param name="ms1">
    ''' a collection of the ms1 data or the mzpack raw data object
    ''' </param>
    ''' <param name="tolerance"></param>
    ''' <returns></returns>
    <ExportAPI("mz.deco")>
    <RApiReturn(GetType(PeakFeature()))>
    Public Function mz_deco(<RRawVectorArgument>
                            ms1 As Object,
                            Optional tolerance As Object = "ppm:20",
                            Optional baseline# = 0.65,
                            <RRawVectorArgument>
                            Optional peakwidth As Object = "3,20",
                            Optional env As Environment = Nothing) As Object

        Dim ms1_scans As IEnumerable(Of IMs1Scan) = ms1Scans(ms1)
        Dim errors As [Variant](Of Tolerance, Message) = Math.getTolerance(tolerance, env)
        Dim rtRange = ApiArgumentHelpers.GetDoubleRange(peakwidth, env, [default]:="3,20")

        If errors Like GetType(Message) Then
            Return errors.TryCast(Of Message)
        ElseIf rtRange Like GetType(Message) Then
            Return rtRange.TryCast(Of Message)
        End If

        Return ms1_scans _
            .GetMzGroups(mzdiff:=errors) _
            .DecoMzGroups(rtRange.TryCast(Of DoubleRange), quantile:=baseline) _
            .ToArray
    End Function

    <ExportAPI("read.peakFeatures")>
    Public Function readPeakData(file As String) As PeakFeature()
        Return file.LoadCsv(Of PeakFeature)(mute:=True).ToArray
    End Function

    <ExportAPI("peak_alignment")>
    <RApiReturn(GetType(xcms2))>
    Public Function peakAlignment(samples As list,
                                  Optional mzdiff As Object = "da:0.001",
                                  Optional norm As Boolean = False,
                                  Optional env As Environment = Nothing) As Object

        Dim mzErr = Math.getTolerance(mzdiff, env, [default]:="da:0.001")

        If mzErr Like GetType(Message) Then
            Return mzErr.TryCast(Of Message)
        End If

        Dim samplePeaks As Dictionary(Of String, PeakFeature()) = samples.AsGeneric(Of PeakFeature())(env)
        Dim sampleData As NamedCollection(Of PeakFeature)() = samplePeaks _
            .Select(Function(i)
                        Return New NamedCollection(Of PeakFeature)(i.Key, i.Value)
                    End Function) _
            .ToArray
        Dim peaktable As xcms2() = sampleData _
            .CreateMatrix(mzErr.TryCast(Of Tolerance)) _
            .ToArray
        Dim id As String() = peaktable.Select(Function(i) i.ID).uniqueNames
        Dim sampleNames As String() = samples.getNames

        For i As Integer = 0 To id.Length - 1
            Dim peak As xcms2 = peaktable(i)

            peak.ID = id(i)

            For Each sample_id As String In sampleNames
                If Not peak.Properties.ContainsKey(sample_id) Then
                    peak(sample_id) = 0.0
                End If
            Next
        Next

        If norm Then
            For Each name As String In sampleNames
                Dim v = peaktable.Select(Function(i) i(name)).AsVector
                v = v / v.Sum * (10 ^ 8)

                For i As Integer = 0 To peaktable.Length - 1
                    peaktable(i)(name) = v(i)
                Next
            Next
        End If

        Return peaktable
    End Function

    ''' <summary>
    ''' do ``m/z`` grouping under the given tolerance
    ''' </summary>
    ''' <param name="ms1"></param>
    ''' <param name="mzdiff"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("mz.groups")>
    <RApiReturn(GetType(MzGroup()))>
    Public Function mz_groups(<RRawVectorArgument>
                              ms1 As Object,
                              Optional mzdiff As Object = "ppm:20",
                              Optional env As Environment = Nothing) As Object

        Return ms1Scans(ms1).GetMzGroups(mzdiff:=Math.getTolerance(mzdiff, env)).ToArray
    End Function

    Private Function ms1Scans(ms1 As Object) As IEnumerable(Of IMs1Scan)
        If ms1 Is Nothing Then
            Return {}
        ElseIf ms1.GetType Is GetType(ms1_scan()) Then
            Return DirectCast(ms1, ms1_scan()).Select(Function(t) DirectCast(t, IMs1Scan))
        ElseIf TypeOf ms1 Is mzPack Then
            Return DirectCast(ms1, mzPack) _
                .GetAllScanMs1 _
                .Select(Function(t) DirectCast(t, IMs1Scan))
        Else
            Throw New NotImplementedException
        End If
    End Function
End Module

