﻿#Region "Microsoft.VisualBasic::a273e214adaf08d4a2f3cf4302f747ae, mzkit\src\assembly\mzPack\mzPack.vb"

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

'   Total Lines: 242
'    Code Lines: 182
' Comment Lines: 28
'   Blank Lines: 32
'     File Size: 8.55 KB


' Class mzPack
' 
'     Properties: Application, Chromatogram, CountMs2, maxIntensity, MS
'                 rtmax, rtmin, Scanners, source, Thumbnail
'                 totalIons
' 
'     Function: CastToPeakMs2, GetAllParentMz, GetAllScanMs1, GetBasePeak, GetMs2Peaks
'               GetXIC, hasMs2, PopulateAllScans, Read, ReadAll
'               ToString, Write
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports BioNovoGene.Analytical.MassSpectrometry.Math
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Chromatogram
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math

''' <summary>
''' the unify in-memory data model of the mzkit MS data model.
''' (mzPack文件格式模型)
''' </summary>
Public Class mzPack

    ''' <summary>
    ''' 一般为二维散点图
    ''' </summary>
    ''' <returns></returns>
    Public Property Thumbnail As Image
    Public Property MS As ScanMS1()
    Public Property Application As FileApplicationClass

    ''' <summary>
    ''' TIC/BPC
    ''' </summary>
    ''' <returns></returns>
    Public Property Chromatogram As DataReader.Chromatogram
    ''' <summary>
    ''' the file name of the raw data source file
    ''' </summary>
    ''' <returns></returns>
    Public Property source As String
    Public Property metadata As New Dictionary(Of String, String)

    ''' <summary>
    ''' 其他的扫描器数据，例如紫外扫描
    ''' </summary>
    ''' <returns></returns>
    Public Property Scanners As Dictionary(Of String, ChromatogramOverlap)

    Public ReadOnly Property rtmin As Double
        Get
            If MS.IsNullOrEmpty Then
                Return 0
            End If
            Return Aggregate scan As ScanMS1 In MS Into Min(scan.rt)
        End Get
    End Property

    Public ReadOnly Property rtmax As Double
        Get
            If MS.IsNullOrEmpty Then
                Return 0
            End If
            Return Aggregate scan As ScanMS1 In MS Into Max(scan.rt)
        End Get
    End Property

    Public ReadOnly Property totalIons As Double
        Get
            Return Aggregate scan As ScanMS1 In MS Into Sum(scan.TIC)
        End Get
    End Property

    Public ReadOnly Property maxIntensity As Double
        Get
            If MS.IsNullOrEmpty Then
                Return 0
            End If
            Return Aggregate scan As ScanMS1 In MS Into Max(scan.BPC)
        End Get
    End Property

    ''' <summary>
    ''' get size of the ms1 scan data
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property size As Integer
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return MS.TryCount
        End Get
    End Property

    ''' <summary>
    ''' get number of total ms2 scan data
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property CountMs2 As Integer
        Get
            Return Aggregate scan As ScanMS1
                   In MS
                   Let nsize As Integer = scan.products.TryCount
                   Into Sum(nsize)
        End Get
    End Property

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetBasePeak() As ms2
        Return MS _
            .Select(Function(scan) scan.GetMs) _
            .IteratesALL _
            .OrderByDescending(Function(mzi) mzi.intensity) _
            .FirstOrDefault
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return source
    End Function

    ''' <summary>
    ''' is there any MS2 data in current raw data file?
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function hasMs2() As Boolean
        Return MS.Any(Function(ms1) Not ms1.products.IsNullOrEmpty)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetAllParentMz(tolerance As Tolerance) As Double()
        Return MS _
            .Select(Function(scan) scan.mz) _
            .IteratesALL _
            .GroupBy(tolerance) _
            .Select(Function(mz)
                        Return Double.Parse(mz.name)
                    End Function) _
            .ToArray
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetXIC(mz As Double, mzErr As Tolerance) As ChromatogramTick()
        Return MS _
            .Select(Function(i)
                        Return New ChromatogramTick With {
                            .Time = i.rt,
                            .Intensity = i.GetIntensity(mz, mzErr)
                        }
                    End Function) _
            .ToArray
    End Function

    Public Function GetAllScanMs1(Optional centroid As Tolerance = Nothing) As IEnumerable(Of ms1_scan)
        If Not centroid Is Nothing Then
            Return MS.GetAllCentroidScanMs1(centroid)
        Else
            Return MS _
                .Select(Function(scan)
                            Return scan.mz _
                                .Select(Function(mzi, i)
                                            Return New ms1_scan With {
                                                .mz = mzi,
                                                .intensity = scan.into(i),
                                                .scan_time = scan.rt
                                            }
                                        End Function)
                        End Function) _
                .IteratesALL
        End If
    End Function

    Public Iterator Function GetMs2Peaks() As IEnumerable(Of PeakMs2)
        For Each ms1 As ScanMS1 In MS
            For Each ms2 As ScanMS2 In ms1.products
                Yield CastToPeakMs2(ms2, file:=source)
            Next
        Next
    End Function

    ''' <summary>
    ''' using scan id as lib guid
    ''' </summary>
    ''' <param name="ms2"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function CastToPeakMs2(ms2 As ScanMS2, Optional file As String = "n/a") As PeakMs2
        Return New PeakMs2 With {
            .activation = ms2.activationMethod.ToString,
            .collisionEnergy = ms2.collisionEnergy,
            .file = file,
            .intensity = ms2.intensity,
            .lib_guid = ms2.ToString & $"[{ms2.parentMz.ToString("F4")},{ms2.rt.ToString("F2")}]",
            .meta = New Dictionary(Of String, String),
            .mz = ms2.parentMz,
            .precursor_type = "",
            .rt = ms2.rt,
            .scan = ms2.scan_id,
            .mzInto = ms2.GetMs.ToArray
        }
    End Function

    ''' <summary>
    ''' a wrapper of <see cref="ReadAll(Stream, Boolean, Boolean, Boolean)"/>
    ''' </summary>
    ''' <param name="filepath"></param>
    ''' <param name="ignoreThumbnail"></param>
    ''' <param name="skipMsn"></param>
    ''' <param name="verbose"></param>
    ''' <returns></returns>
    Public Shared Function Read(filepath As String,
                                Optional ignoreThumbnail As Boolean = False,
                                Optional skipMsn As Boolean = False,
                                Optional verbose As Boolean = False) As mzPack

        Using file As Stream = filepath.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
            Dim pack As mzPack = ReadAll(file, ignoreThumbnail, skipMsn:=skipMsn, verbose:=verbose)

            ' 20221109 MSI.mzPack is the internal pipeline raw data file name
            ' andalso we should treated as empty
            If pack.source.StringEmpty OrElse
                pack.source.TextEquals("MSI.mzPack") Then
                pack.source = filepath.FileName
            End If

            Return pack
        End Using
    End Function

    ''' <summary>
    ''' load all content data in mzpack object into memory at one time.
    ''' the file format version is test from the magic number.
    ''' (一次性加载所有原始数据)
    ''' </summary>
    ''' <param name="file">
    ''' the file version will be automatically detected
    ''' </param>
    ''' <returns>
    ''' a unify mzpack in-memory data model
    ''' </returns>
    Public Shared Function ReadAll(file As Stream,
                                   Optional ignoreThumbnail As Boolean = False,
                                   Optional skipMsn As Boolean = False,
                                   Optional verbose As Boolean = True) As mzPack

        Dim ver As Integer = file.GetFormatVersion
        Dim pack As mzPack

        If ver = 1 Then
            pack = v1MemoryLoader.ReadAll(file, ignoreThumbnail, skipMsn, verbose)
        ElseIf ver = 2 Then
            pack = New mzStream(file).ReadModel(ignoreThumbnail, skipMsn, verbose)
        ElseIf (TypeOf file Is MemoryStream OrElse TypeOf file Is FileStream) AndAlso file.Length = 0 Then
            ' is empty data
            Return New mzPack With {
                .MS = {}
            }
        Else
            Throw New InvalidProgramException("unknow file format!")
        End If

        If pack.source.StringEmpty Then
            If TypeOf file Is FileStream Then
                pack.source = DirectCast(file, FileStream).Name.FileName
            End If
        End If

        Return pack
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Write(file As Stream,
                          Optional version As Integer = 2,
                          Optional progress As Action(Of String) = Nothing) As Boolean

        If version = 1 Then
            Return v1MemoryLoader.Write(Me, file, progress)
        ElseIf version = 2 Then
            Return Me.WriteStream(file, meta_size:=32 * 1024 * 1024)
        Else
            Return False
        End If
    End Function
End Class
