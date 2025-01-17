﻿Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.mzData.mzWebCache
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace MsImaging

    Public Class Metadata

        Public Property scan_x As Integer
        Public Property scan_y As Integer
        Public Property resolution As Double
        Public Property mass_range As DoubleRange

        ''' <summary>
        ''' the string name of <see cref="FileApplicationClass"/>, 
        ''' not the description id value
        ''' </summary>
        ''' <returns></returns>
        Public Property [class] As String

        Public ReadOnly Property physical_width As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return scan_x * resolution
            End Get
        End Property

        Public ReadOnly Property physical_height As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return scan_y * resolution
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetDimension() As Size
            Return New Size(scan_x, scan_y)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{mass_range.Min.ToString("F4")} - {mass_range.Max.ToString("F4")}] {scan_x}x{scan_y}@{resolution}um"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMetadata() As Dictionary(Of String, String)
            Dim datalist As New Dictionary(Of String, String) From {
                {"width", scan_x},
                {"height", scan_y},
                {"resolution", resolution}
            }

            If Not mass_range Is Nothing Then
                datalist!mzmin = mass_range.Min
                datalist!mzmax = mass_range.Max
            End If

            Return datalist
        End Function

    End Class
End Namespace