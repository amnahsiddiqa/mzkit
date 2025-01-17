﻿Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra.Xml
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class JaccardSet : Implements INamedValue

    Public Property libname As String Implements INamedValue.Key
    Public Property mz1 As Double
    Public Property ms2 As Double()

    ''' <summary>
    ''' rt equals to ZERO means no rt data
    ''' </summary>
    ''' <returns></returns>
    Public Property rt As Double

    Public Overrides Function ToString() As String
        Return libname
    End Function

End Class

Public Class JaccardSearch : Inherits Ms2Search

    ''' <summary>
    ''' the ms2 fragment data pack
    ''' </summary>
    ReadOnly mzSet As JaccardSet()
    ReadOnly cutoff As Double

    Sub New(ref As IEnumerable(Of JaccardSet), cutoff As Double)
        Call MyBase.New

        Me.mzSet = ref.ToArray
        Me.cutoff = cutoff
    End Sub

    Public Overrides Function Search(centroid() As ms2, mz1 As Double) As ClusterHit
        Dim query As Double() = centroid.Select(Function(i) i.mz).ToArray
        Dim subset = mzSet.Where(Function(i) da(mz1, i.mz1)).ToArray
        Dim jaccard = subset _
            .Select(Function(i)
                        Dim itr = GlobalAlignment.MzIntersect(i.ms2, query, da)
                        Dim uni = GlobalAlignment.MzUnion(i.ms2, query, da)

                        Return (i, itr, uni)
                    End Function) _
            .Where(Function(j) j.itr.Length / j.uni.Length > cutoff) _
            .ToArray

        ' get a cluster of the hit result
        If jaccard.Length > 0 Then
            ' get alignment score vector and the alignment details 
            Dim scores = jaccard.Select(Function(j) score(centroid, j.i, j.itr, j.uni)).ToArray
            ' get index of the max score
            Dim forward As New Vector(scores.Select(Function(a) a.forward))
            Dim reverse As New Vector(scores.Select(Function(a) a.reverse))
            Dim jaccard2 As New Vector(scores.Select(Function(a) a.jaccard))
            Dim i As Integer = which.Max(forward * reverse * jaccard2)
            'Dim alignments = scores _
            '    .Select(Function(a) a.alignment) _
            '    .IteratesALL _
            '    .GroupBy(Function(a) a.mz, da) _
            '    .Select(Function(a)
            '                Return New SSM2MatrixFragment With {
            '                    .mz = Val(a.name),
            '                    .da = da.DeltaTolerance,
            '                    .query = a.First.query,
            '                    .ref = a.Select(Function(ai) ai.ref).Average
            '                }
            '            End Function) _
            '    .ToArray

            ' 20221017 just returns the best one in jaccard alignment mode
            Dim best_align = scores(i)
            Dim best_hit = jaccard(i)

            Return New ClusterHit With {
                .jaccard = best_align.jaccard,
                .ClusterForward = {best_align.forward},
                .ClusterReverse = {best_align.reverse},
                .ClusterJaccard = {best_align.jaccard},
                .ClusterId = {best_hit.i.libname},
                .ClusterRt = {best_hit.i.rt},
                .Id = jaccard(i).i.libname,
                .queryMz = mz1,
                .forward = .ClusterForward.Average,
                .reverse = .ClusterReverse.Average,
                .representive = best_align.alignment
            }
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' create jaccard data alignment result
    ''' </summary>
    ''' <param name="centroid"></param>
    ''' <param name="ref"></param>
    ''' <param name="itr"></param>
    ''' <param name="uni"></param>
    ''' <returns></returns>
    Private Function score(centroid() As ms2,
                           ref As JaccardSet,
                           itr As Double(),
                           uni As Double()) As (jaccard As Double, forward As Double, reverse As Double, alignment As SSM2MatrixFragment())

        Dim jaccard As Double = itr.Length / uni.Length
        Dim hits = itr _
            .Select(Function(mzi)
                        Return centroid _
                            .Where(Function(m) da(m.mz, mzi)) _
                            .OrderByDescending(Function(m) m.intensity) _
                            .First
                    End Function) _
            .ToArray
        Dim cos = GlobalAlignment.TwoDirectionSSM(centroid, hits, da)
        Dim align = GlobalAlignment.CreateAlignment(centroid, hits, da).ToArray

        Return (jaccard, cos.forward, cos.reverse, align)
    End Function
End Class
