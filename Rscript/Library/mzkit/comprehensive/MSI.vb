﻿#Region "Microsoft.VisualBasic::e190b82ad56029b2dc79cb4ac2ddf4bc, mzkit\Rscript\Library\mzkit\assembly\MSI.vb"

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

'   Total Lines: 464
'    Code Lines: 336
' Comment Lines: 73
'   Blank Lines: 55
'     File Size: 19.16 KB


' Module MSI
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: basePeakMz, Correction, GetIonsJointMatrix, getStatTable, IonStats
'               loadRowSummary, MSI_summary, MSIScanMatrix, open_imzML, PeakMatrix
'               peakSamples, pixelId, PixelMatrix, pixels, pixels2D
'               rowScans, splice
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.Comprehensive.MsImaging
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.imzML
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.Pixel
Imports BioNovoGene.Analytical.MassSpectrometry.MsImaging.Reader
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports imzML = BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.imzML.XML
Imports rDataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe

<Package("MSI")>
Module MSI

    Sub New()
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(IonStat()), AddressOf getStatTable)
    End Sub

    Private Function getStatTable(ions As IonStat(), args As list, env As Environment) As rDataframe
        Dim table As New rDataframe With {
            .columns = New Dictionary(Of String, Array),
            .rownames = ions _
                .Select(Function(i) i.mz.ToString("F4")) _
                .ToArray
        }

        Call table.add(NameOf(IonStat.mz), ions.Select(Function(i) i.mz))
        Call table.add(NameOf(IonStat.pixels), ions.Select(Function(i) i.pixels))
        Call table.add(NameOf(IonStat.density), ions.Select(Function(i) i.density))
        Call table.add("basePixel.X", ions.Select(Function(i) i.basePixelX))
        Call table.add("basePixel.Y", ions.Select(Function(i) i.basePixelY))
        Call table.add(NameOf(IonStat.maxIntensity), ions.Select(Function(i) i.maxIntensity))
        Call table.add(NameOf(IonStat.Q1Intensity), ions.Select(Function(i) i.Q1Intensity))
        Call table.add(NameOf(IonStat.Q2Intensity), ions.Select(Function(i) i.Q2Intensity))
        Call table.add(NameOf(IonStat.Q3Intensity), ions.Select(Function(i) i.Q3Intensity))

        Return table
    End Function

    ''' <summary>
    ''' split the raw 2D MSI data into multiple parts with given parts
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="partition"></param>
    ''' <returns></returns>
    <ExportAPI("splice")>
    Public Function splice(raw As mzPack, Optional partition As Integer = 5) As mzPack()
        Dim sampler As New PixelsSampler(New ReadRawPack(raw))
        Dim sampling As Size = sampler.MeasureSamplingSize(resolution:=partition)
        Dim samples As NamedCollection(Of PixelScan)() = sampler.SamplingRaw(sampling).ToArray
        Dim packList As mzPack() = samples _
            .Select(Function(blockList)
                        Return New mzPack With {
                            .MS = blockList _
                                .Select(Function(p)
                                            Return DirectCast(p, mzPackPixel).scan
                                        End Function) _
                                .ToArray,
                            .Application = FileApplicationClass.MSImaging,
                            .source = blockList.name
                        }
                    End Function) _
            .ToArray

        Return packList
    End Function

    <ExportAPI("pixelId")>
    Public Function pixelId(raw As mzPack, mz As Double,
                            Optional tolerance As Object = "da:0.1",
                            Optional env As Environment = Nothing) As Object

        Dim mzErr = Math.getTolerance(tolerance, env)

        If mzErr Like GetType(Message) Then
            Return mzErr.TryCast(Of Message)
        End If

        Dim mzdiff As Tolerance = mzErr.TryCast(Of Tolerance)
        Dim scans = From scan As ScanMS1
                    In raw.MS.AsParallel
                    Where scan.mz.Any(Function(mzi) (mzdiff(mzi, mz)))
                    Let p As Point = scan.GetMSIPixel
                    Let id As String = $"{p.X},{p.Y}"
                    Select id

        Return scans.ToArray
    End Function

    ''' <summary>
    ''' get pixels size from the raw data file
    ''' </summary>
    ''' <param name="file">
    ''' imML/mzPack
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("pixels")>
    Public Function pixels(file As String, Optional env As Environment = Nothing) As Object
        If file.ExtensionSuffix("imzml") Then
            Dim allScans = XML.LoadScans(file).ToArray
            Dim width As Integer = Aggregate p In allScans Into Max(p.x)
            Dim height As Integer = Aggregate p In allScans Into Max(p.y)

            Return New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"w", width},
                    {"h", height}
                }
            }
        ElseIf file.ExtensionSuffix("mzpack") Then
            Using reader As New BinaryStreamReader(file)
                Dim allMeta = reader.EnumerateIndex _
                    .Select(AddressOf reader.GetMetadata) _
                    .IteratesALL _
                    .ToArray
                Dim x As Integer() = allMeta _
                    .Where(Function(p) p.Key.TextEquals("x")) _
                    .Select(Function(p) p.Value) _
                    .Select(AddressOf Integer.Parse) _
                    .ToArray
                Dim y As Integer() = allMeta _
                    .Where(Function(p) p.Key.TextEquals("y")) _
                    .Select(Function(p) p.Value) _
                    .Select(AddressOf Integer.Parse) _
                    .ToArray

                Return New list With {
                    .slots = New Dictionary(Of String, Object) From {
                        {"w", x.Max},
                        {"h", y.Max}
                    }
                }
            End Using
        Else
            Return Internal.debug.stop("unsupported file!", env)
        End If
    End Function

    <ExportAPI("open.imzML")>
    Public Function open_imzML(file As String) As Object
        Dim scans As ScanData() = imzML.LoadScans(file:=file).ToArray
        Dim ibd = ibdReader.Open(file.ChangeSuffix("ibd"))

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"scans", scans},
                {"ibd", ibd}
            }
        }
    End Function

    ''' <summary>
    ''' each raw data file is a row scan data
    ''' </summary>
    ''' <param name="y">
    ''' this function will returns the pixel summary data if the ``y`` parameter greater than ZERO.
    ''' </param>
    ''' <param name="correction">
    ''' used for data summary, when the ``y`` parameter is greater than ZERO, 
    ''' this parameter will works.
    ''' </param>
    ''' <param name="raw">
    ''' a file list of mzpack data files
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("row.scans")>
    Public Function rowScans(raw As String(),
                             Optional y As Integer = 0,
                             Optional correction As Correction = Nothing,
                             Optional env As Environment = Nothing) As Object

        If raw.IsNullOrEmpty Then
            Return Internal.debug.stop("the required raw data file list is empty!", env)
        ElseIf raw.Length = 1 Then
            If y > 0 Then
                Using file As FileStream = raw(Scan0).Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                    Return file.loadRowSummary(y, correction)
                End Using
            Else
                Return Internal.debug.stop("the pixels of column must be specific!", env)
            End If
        Else
            Dim loader = Iterator Function() As IEnumerable(Of mzPack)
                             For Each path As String In raw
                                 Using file As FileStream = path.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                                     Yield mzPack.ReadAll(file, ignoreThumbnail:=True)
                                 End Using
                             Next
                         End Function

            Return pipeline.CreateFromPopulator(loader())
        End If
    End Function

    <Extension>
    Private Function loadRowSummary(file As Stream, y As Integer, correction As Correction) As iPixelIntensity()
        Dim mzpack As mzPack = mzPack.ReadAll(file, ignoreThumbnail:=True)
        Dim pixels As iPixelIntensity() = mzpack.MS _
            .Select(Function(col, i)
                        Dim basePeakMz As Double = col.mz(which.Max(col.into))

                        Return New iPixelIntensity With {
                            .average = col.into.Average,
                            .basePeakIntensity = col.into.Max,
                            .totalIon = col.into.Sum,
                            .x = If(correction Is Nothing, i + 1, correction.GetPixelRowX(col)),
                            .y = y,
                            .basePeakMz = basePeakMz
                        }
                    End Function) _
            .ToArray

        Return pixels
    End Function

    ''' <summary>
    ''' Fetch MSI summary data
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <returns></returns>
    <ExportAPI("MSI_summary")>
    Public Function MSI_summary(raw As mzPack) As MSISummary
        Return raw.MS _
            .Select(Function(p)
                        Return New iPixelIntensity With {
                            .x = Integer.Parse(p.meta("x")),
                            .y = Integer.Parse(p.meta("y")),
                            .average = p.into.Average,
                            .basePeakIntensity = p.into.Max,
                            .totalIon = p.into.Sum,
                            .basePeakMz = p.mz(which.Max(p.into))
                        }
                    End Function) _
            .DoCall(AddressOf MSISummary.FromPixels)
    End Function

    ''' <summary>
    ''' calculate the X scale
    ''' </summary>
    ''' <param name="totalTime"></param>
    ''' <param name="pixels"></param>
    ''' <param name="hasMs2"></param>
    ''' <returns></returns>
    <ExportAPI("correction")>
    Public Function Correction(totalTime As Double, pixels As Integer, Optional hasMs2 As Boolean = False) As Correction
        If hasMs2 Then
            Return New ScanMs2Correction(totalTime, pixels)
        Else
            Return New ScanTimeCorrection(totalTime, pixels)
        End If
    End Function

    <ExportAPI("basePeakMz")>
    Public Function basePeakMz(summary As MSISummary) As LibraryMatrix
        Return summary.GetBasePeakMz
    End Function

    <ExportAPI("ionStat")>
    Public Function IonStats(raw As mzPack, Optional gridSize As Integer = 5, Optional da As Double = 0.01) As IonStat()
        Return IonStat.DoStat(raw, nsize:=gridSize, da:=da).ToArray
    End Function

    <ExportAPI("ions_jointmatrix")>
    Public Function GetIonsJointMatrix(raw As list, Optional env As Environment = Nothing) As rDataframe
        Dim allStats = raw.getNames _
            .AsParallel _
            .Select(Function(name)
                        Return New NamedValue(Of IonStat())(name, IonStats(raw.getValue(Of mzPack)(name, env)))
                    End Function) _
            .ToArray
        Dim allMz As Double() = allStats _
            .Select(Function(i) i.Value) _
            .IteratesALL _
            .Select(Function(i) i.mz) _
            .GroupBy(Tolerance.DeltaMass(0.05)) _
            .Select(Function(i) Val(i.name)) _
            .ToArray
        Dim mat As New rDataframe With {
            .rownames = allMz _
                .Select(Function(mzi) mzi.ToString("F4")) _
                .ToArray,
            .columns = New Dictionary(Of String, Array)
        }
        Dim daErr As Tolerance = Tolerance.DeltaMass(0.1)

        For Each file In allStats
            Dim pixels = allMz _
                .Select(Function(mzi)
                            Return file.Value _
                                .Where(Function(i) daErr.Equals(i.mz, mzi)) _
                                .Select(Function(i) i.pixels) _
                                .OrderByDescending(Function(i) i) _
                                .FirstOrDefault
                        End Function) _
                .ToArray

            mat.add(file.Name, pixels)
        Next

        Return mat
    End Function

    ''' <summary>
    ''' combine each row scan raw data files as the pixels 2D matrix
    ''' </summary>
    ''' <param name="rowScans">
    ''' data result comes from the function ``row.scans``.
    ''' </param>
    ''' <param name="yscale">
    ''' apply for mapping smooth MS1 to ms2 scans
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("scans2D")>
    Public Function pixels2D(<RRawVectorArgument>
                             rowScans As Object,
                             Optional correction As Correction = Nothing,
                             Optional intocutoff As Double = 0.05,
                             Optional yscale As Double = 1,
                             Optional env As Environment = Nothing) As Object

        Dim pipeline As pipeline = pipeline.TryCreatePipeline(Of mzPack)(rowScans, env)

        If yscale <> 1.0 Then
            Call base.print($"yscale is {yscale}", , env)
        End If

        If pipeline.isError Then
            Return pipeline.getError
        Else
            Return pipeline _
                .populates(Of mzPack)(env) _
                .MSICombineRowScans(
                    correction:=correction,
                    intocutoff:=intocutoff,
                    yscale:=yscale,
                    progress:=Sub(msg)
                                  Call base.print(msg, , env)
                              End Sub
                )
        End If
    End Function

    ''' <summary>
    ''' combine each row scan summary vector as the pixels 2D matrix
    ''' </summary>
    ''' <param name="rowScans"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("scanMatrix")>
    <RApiReturn(GetType(MSISummary))>
    Public Function MSIScanMatrix(<RRawVectorArgument> rowScans As Object, Optional env As Environment = Nothing) As Object
        Dim data As pipeline = pipeline.TryCreatePipeline(Of iPixelIntensity)(rowScans, env)

        If data.isError Then
            Return data.getError
        End If

        Return data _
            .populates(Of iPixelIntensity)(env) _
            .DoCall(AddressOf MSISummary.FromPixels)
    End Function

    <ExportAPI("peakMatrix")>
    Public Function PeakMatrix(raw As mzPack, Optional topN As Integer = 3, Optional mzError As Object = "da:0.05", Optional env As Environment = Nothing) As Object
        Dim err = Math.getTolerance(mzError, env)

        If err Like GetType(Message) Then
            Return err.TryCast(Of Message)
        End If

        Return raw _
            .TopIonsPeakMatrix(topN, err.TryCast(Of Tolerance).GetScript) _
            .ToArray
    End Function

    ''' <summary>
    ''' split the raw MSI 2D data into multiple parts with given resolution parts
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="resolution"></param>
    ''' <param name="mzError"></param>
    ''' <param name="cutoff"></param>
    ''' <param name="env"></param>
    ''' <returns>returns the raw matrix data that contains the peak samples.</returns>
    <ExportAPI("peakSamples")>
    Public Function peakSamples(raw As mzPack,
                                Optional resolution As Integer = 100,
                                Optional mzError As Object = "da:0.05",
                                Optional cutoff As Double = 0.05,
                                Optional env As Environment = Nothing) As Object

        Dim err = Math.getTolerance(mzError, env)

        If err Like GetType(Message) Then
            Return err.TryCast(Of Message)
        End If

        Dim sampler As New PixelsSampler(New ReadRawPack(raw))
        Dim sampling As Size = sampler.MeasureSamplingSize(resolution)
        Dim samples = sampler.Sampling(sampling, err.TryCast(Of Tolerance)).ToArray
        Dim matrix As DataSet() = samples _
            .AlignMzPeaks(
                mzErr:=err.TryCast(Of Tolerance),
                cutoff:=cutoff,
                getPeaks:=Function(p) p.GetMs,
                getSampleId:=Function(p) $"{p.X},{p.Y}"
            ) _
            .ToArray

        Return matrix
    End Function

    ''' <summary>
    ''' get number of ions in each pixel scans
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <returns></returns>
    <ExportAPI("pixelIons")>
    Public Function PixelIons(raw As mzPack) As Integer()
        Return raw.MS.Select(Function(scan) scan.size).ToArray
    End Function

    ''' <summary>
    ''' dumping raw data matrix as text table file. 
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="file"></param>
    ''' <param name="tolerance"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("pixelMatrix")>
    Public Function PixelMatrix(raw As mzPack, file As Stream,
                                Optional tolerance As Object = "da:0.05",
                                Optional env As Environment = Nothing) As Message

        Dim mzErr = Math.getTolerance(tolerance, env)

        If mzErr Like GetType(Message) Then
            Return mzErr.TryCast(Of Message)
        End If

        Dim da As Tolerance = mzErr.TryCast(Of Tolerance)
        Dim allMz As ms2() = raw.MS _
            .Select(Function(i) i.GetMs) _
            .IteratesALL _
            .ToArray _
            .Centroid(da, New RelativeIntensityCutoff(0.01)) _
            .OrderBy(Function(i) i.mz) _
            .ToArray
        Dim text As New StreamWriter(file)

        Call text.WriteLine({""}.JoinIterates(allMz.Select(Function(a) a.mz.ToString("F4"))).JoinBy(","))
        Call text.Flush()

        For Each pixel As ScanMS1 In raw.MS
            Dim pid As String = $"{pixel.meta!x};{pixel.meta!y}"
            Dim msData = pixel.GetMs.ToArray
            Dim vec As String() = allMz _
                .Select(Function(mzi)
                            Dim mz As ms2 = msData _
                                .Where(Function(i) da(i.mz, mzi.mz)) _
                                .FirstOrDefault

                            If mz Is Nothing Then
                                Return "0"
                            Else
                                Return mz.intensity.ToString
                            End If
                        End Function) _
                .ToArray

            Call text.WriteLine({pid}.JoinIterates(vec).JoinBy(","))
            Call Console.WriteLine(pixel.scan_id)
        Next

        Call text.Flush()

        Return Nothing
    End Function
End Module