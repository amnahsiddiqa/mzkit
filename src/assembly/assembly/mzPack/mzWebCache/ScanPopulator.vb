﻿#Region "Microsoft.VisualBasic::02e0a495e46e32d67df35c6bbbe9c195, mzkit\src\assembly\assembly\mzPack\mzWebCache\ScanPopulator.vb"

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

    '   Total Lines: 124
    '    Code Lines: 97
    ' Comment Lines: 5
    '   Blank Lines: 22
    '     File Size: 5.16 KB


    '     Interface IScanReader
    ' 
    '         Function: CreateScan
    ' 
    '     Class ScanPopulator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateScan, CreateScanGeneral, (+2 Overloads) Load, PopulateValidScans
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.DataReader
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace mzData.mzWebCache

    Public Interface IScanReader

        Function CreateScan(scan As Object, Optional uniqueId As String = Nothing) As MSScan

    End Interface

    ''' <summary>
    ''' helper module for read a raw data file and populate 
    ''' a unify data scan model object
    ''' </summary>
    ''' <typeparam name="Scan"></typeparam>
    Public MustInherit Class ScanPopulator(Of Scan) : Implements IScanReader

        Protected ms1 As ScanMS1
        Protected products As New List(Of ScanMS2)
        Protected trim As LowAbundanceTrimming
        Protected ms1Err As Tolerance
        Protected rawName As String

        Protected ReadOnly reader As MsDataReader(Of Scan)
        Protected ReadOnly invalidScans As New List(Of Scan)

        Sub New(mzErr As String, intocutoff As Double)
            ms1Err = Tolerance.ParseScript(mzErr)
            reader = dataReader()
            trim = New RelativeIntensityCutoff(intocutoff)
        End Sub

        Protected MustOverride Function dataReader() As MsDataReader(Of Scan)
        Protected MustOverride Function loadScans(rawfile As String) As IEnumerable(Of Scan)

        Private Iterator Function PopulateValidScans(scans As IEnumerable(Of Scan)) As IEnumerable(Of Scan)
            For Each scan As Scan In scans
                If reader.IsEmpty(scan) Then
                    Call $"missing scan value of [{reader.GetScanId(scan)}]".Warning
                    Call invalidScans.Add(scan)
                Else
                    Yield scan
                End If
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function CreateScanGeneral(obj As Object, Optional uniqueId As String = Nothing) As MSScan Implements IScanReader.CreateScan
            Return CreateScan(scan:=DirectCast(obj, Scan), uniqueId)
        End Function

        Public Function CreateScan(scan As Scan, Optional uniqueId As String = Nothing) As MSScan
            Dim scan_time As Double = reader.GetScanTime(scan)
            Dim scan_name As String = reader.GetScanId(scan)

            If scan_name.StringEmpty Then
                scan_name = $"[MS1] {rawName}"
            End If

            Dim scan_id As String = If(uniqueId.StringEmpty, scan_name, $"[{uniqueId}]{scan_name}")
            Dim msms As ms2() = reader.GetMsMs(scan).Centroid(ms1Err, trim).ToArray
            Dim msLevel As Integer = reader.GetMsLevel(scan)

            If msLevel = 1 OrElse msLevel = 0 Then
                Return New ScanMS1 With {
                    .BPC = reader.GetBPC(scan),
                    .TIC = reader.GetTIC(scan),
                    .rt = scan_time,
                    .scan_id = scan_id,
                    .mz = msms.Select(Function(a) a.mz).ToArray,
                    .into = msms.Select(Function(a) a.intensity).ToArray
                }
            Else
                Return New ScanMS2 With {
                    .rt = scan_time,
                    .parentMz = reader.GetParentMz(scan),
                    .scan_id = scan_id,
                    .intensity = reader.GetBPC(scan),
                    .mz = msms.Select(Function(a) a.mz).ToArray,
                    .into = msms.Select(Function(a) a.intensity).ToArray,
                    .polarity = PrecursorType.ParseIonMode(reader.GetPolarity(scan), True),
                    .activationMethod = reader.GetActivationMethod(scan),
                    .centroided = reader.GetCentroided(scan),
                    .charge = reader.GetCharge(scan),
                    .collisionEnergy = reader.GetCollisionEnergy(scan)
                }
            End If
        End Function

        Public Iterator Function Load(scans As IEnumerable(Of Scan), Optional progress As Action(Of String) = Nothing) As IEnumerable(Of ScanMS1)
            Dim i As i32 = 1
            Dim ms1Yields As Integer = 0

            For Each scan As Scan In PopulateValidScans(scans)
                Dim scanVal As MSScan = CreateScan(scan, ++i)
                Dim isMs1 As Boolean = TypeOf scanVal Is ScanMS1

                If isMs1 Then
                    If Not ms1 Is Nothing Then
                        ms1.products = products.ToArray
                        ms1Yields += 1
                        products.Clear()

                        Yield ms1
                    End If

                    ms1 = scanVal
                Else
                    Call products.Add(scanVal)
                End If

                If Not progress Is Nothing Then
                    Call progress(scanVal.scan_id)
                End If
            Next

            If Not ms1 Is Nothing Then
                ms1.products = products.ToArray
                products.Clear()

                Yield ms1
            ElseIf ms1Yields = 0 Then
                For Each fake As ScanMS1 In yieldFakeMs1(products)
                    Yield fake
                Next
            End If
        End Function

        Private Iterator Function yieldFakeMs1(products As IEnumerable(Of ScanMS2)) As IEnumerable(Of ScanMS1)
            For Each ms2 As ScanMS2 In products
                Yield New ScanMS1 With {
                    .BPC = ms2.intensity,
                    .into = {ms2.intensity},
                    .products = {ms2},
                    .mz = {ms2.parentMz},
                    .rt = ms2.rt,
                    .scan_id = "[MS1]" & ms2.scan_id,
                    .TIC = ms2.intensity
                }
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Load(rawfile As String, Optional progress As Action(Of String) = Nothing) As IEnumerable(Of ScanMS1)
            Return Load(loadScans(rawfile), progress)
        End Function
    End Class
End Namespace
