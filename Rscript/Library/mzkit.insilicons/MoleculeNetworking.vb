﻿Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.MoleculeNetworking
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.DataMining.BinaryTree
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("MoleculeNetworking")>
Module MoleculeNetworking

    <ExportAPI("uniqueNames")>
    Public Function unqiueNames(ions As PeakMs2()) As PeakMs2()
        Dim id As String() = ions.SafeQuery.Select(Function(i) i.lib_guid).uniqueNames

        For i As Integer = 0 To id.Length - 1
            ions(i).lib_guid = id(i)
        Next

        Return ions
    End Function

    <ExportAPI("as.graph")>
    Public Function createGraph(tree As ClusterTree, ions As PeakMs2()) As NetworkGraph
        Dim g As New NetworkGraph
        Dim bins As list = tree.MsBin(ions)
        Dim seed As Node

        For Each bin In bins.slots
            ions = REnv.asVector(Of PeakMs2)(bin.Value)
            seed = g.CreateNode(
                label:=bin.Key,
                data:=New NodeData With {
                    .Properties = New Dictionary(Of String, String) From {
                        {"rt", ions.Where(Function(d) d.lib_guid = bin.Key).First.rt}
                    },
                    .label = bin.Key,
                    .origID = bin.Key
                }
            )

            For Each ion In ions.Where(Function(i) i.lib_guid <> bin.Key)
                If g.GetElementByID(ion.lib_guid) Is Nothing Then
                    g.CreateNode(
                        label:=ion.lib_guid,
                        data:=New NodeData With {
                            .label = ion.lib_guid,
                            .origID = ion.lib_guid,
                            .Properties = New Dictionary(Of String, String) From {
                                {"rt", ion.rt}
                            }
                        }
                    )
                End If

                Call g.CreateEdge(u:=g.GetElementByID(bin.Key), v:=g.GetElementByID(ion.lib_guid), 1)
            Next
        Next

        For Each layer In ClusterTree.GetClusters(tree)
            If Not layer.Childs.IsNullOrEmpty Then
                For Each child As Tree(Of String) In layer.Childs.Values
                    Call g.CreateEdge(
                        u:=g.GetElementByID(layer.Data),
                        v:=g.GetElementByID(child.Data),
                        weight:=0.5
                    )
                Next
            End If
        Next

        Return g
    End Function

    <ExportAPI("tree")>
    Public Function Tree(ions As PeakMs2(),
                         Optional mzdiff As Double = 0.3,
                         Optional intocutoff As Double = 0.05,
                         Optional equals As Double = 0.85) As ClusterTree

        Return ions.Tree(mzdiff, intocutoff, equals)
    End Function

    ''' <summary>
    ''' populate a list of peak ms2 cluster data
    ''' </summary>
    ''' <param name="tree"></param>
    ''' <param name="ions"></param>
    ''' <returns></returns>
    <ExportAPI("msBin")>
    <Extension>
    Public Function MsBin(tree As ClusterTree, ions As PeakMs2()) As list
        Dim clusters As IEnumerable(Of ClusterTree) = ClusterTree.GetClusters(tree)
        Dim list = ions.ToDictionary(Function(i) i.lib_guid)
        Dim payload As New list With {
            .slots = New Dictionary(Of String, Object)
        }
        Dim members As New List(Of PeakMs2)

        For Each cluster As ClusterTree In clusters
            Call members.Clear()
            Call members.Add(list(cluster.Data))
            Call members.AddRange(cluster.Members.SafeQuery.Select(Function(r) list(r)))
            Call payload.add(cluster.Data, members.ToArray)
        Next

        Return payload
    End Function

    ''' <summary>
    ''' create representative spectrum data
    ''' </summary>
    ''' <param name="tree"></param>
    ''' <param name="ions"></param>
    ''' <param name="mzdiff"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("representative")>
    <RApiReturn(GetType(PeakMs2))>
    Public Function RepresentativeSpectrum(tree As ClusterTree,
                                           ions As PeakMs2(),
                                           Optional mzdiff As Object = "da:0.3",
                                           Optional env As Environment = Nothing) As Object

        Dim pack As list = tree.MsBin(ions)
        Dim output As New List(Of PeakMs2)
        Dim zero As RelativeIntensityCutoff = 0.0
        Dim mzerr = Math.getTolerance(mzdiff, env, [default]:="da:0.3")

        If mzerr Like GetType(Message) Then
            Return mzerr.TryCast(Of Message)
        End If

        Dim tolerance As Tolerance = mzerr.TryCast(Of Tolerance)

        For Each key As String In pack.getNames
            Dim cluster As PeakMs2() = pack.getValue(Of PeakMs2())(key, env, [default]:={})
            Dim union As ms2() = cluster _
                .Select(Function(i)
                            Dim maxinto As Double = i.mzInto _
                                .Select(Function(mzi) mzi.intensity) _
                                .Max

                            Return i.mzInto _
                                .Select(Function(mzi)
                                            Return New ms2 With {
                                                .mz = mzi.mz,
                                                .intensity = mzi.intensity / maxinto
                                            }
                                        End Function)
                        End Function) _
                .IteratesALL _
                .ToArray _
                .Centroid(tolerance, cutoff:=zero) _
                .ToArray
            Dim rt As Double = cluster _
                .Select(Function(c) c.rt) _
                .TabulateBin _
                .Average

            output.Add(New PeakMs2 With {
                .rt = rt,
                .activation = "NA",
                .collisionEnergy = 0,
                .file = key,
                .intensity = cluster.Sum(Function(c) c.intensity),
                .lib_guid = key,
                .mz = 0,
                .mzInto = union,
                .precursor_type = "NA",
                .scan = "NA",
                .meta = cluster _
                    .Select(Function(c) c.meta) _
                    .IteratesALL _
                    .GroupBy(Function(t) t.Key) _
                    .ToDictionary(Function(t) t.Key,
                                  Function(t)
                                      Return t _
                                          .Select(Function(ti) ti.Value) _
                                          .Distinct _
                                          .JoinBy("; ")
                                  End Function)
            })
        Next

        Return output.ToArray
    End Function
End Module