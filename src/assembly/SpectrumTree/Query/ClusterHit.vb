﻿Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra.Xml

Public Class ClusterHit

    ''' <summary>
    ''' the reference id in library
    ''' </summary>
    ''' <returns></returns>
    Public Property Id As String
    Public Property representive As SSM2MatrixFragment()
    Public Property forward As Double
    Public Property reverse As Double
    Public Property jaccard As Double
    Public Property queryId As String
    Public Property queryMz As Double
    Public Property queryRt As Double

    Public Property ClusterRt As Double()
    Public Property ClusterForward As Double()
    Public Property ClusterReverse As Double()
    Public Property ClusterJaccard As Double()
    Public Property ClusterId As String()

    ''' <summary>
    ''' the basepeak m/z of the sample spectrum
    ''' </summary>
    ''' <returns></returns>
    Public Property basePeak As Double

    Public Overrides Function ToString() As String
        Return Id
    End Function

End Class
