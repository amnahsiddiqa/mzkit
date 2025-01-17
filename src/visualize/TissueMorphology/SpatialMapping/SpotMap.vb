﻿Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Data.GraphTheory

''' <summary>
''' A spot of spatial transcriptomics mapping to a 
''' collection of spatial metabolism pixels
''' </summary>
Public Class SpotMap : Implements IPoint2D

    ''' <summary>
    ''' the spot barcode
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute>
    Public Property barcode As String
    ''' <summary>
    ''' tissue flag
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute>
    Public Property flag As Integer
    ''' <summary>
    ''' the physical xy
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute>
    Public Property physicalXY As Integer()
    ''' <summary>
    ''' the original raw spot xy
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute>
    Public Property spotXY As Integer()

    ''' <summary>
    ''' pixel x of the spatial transcriptomics spot.
    ''' (after spatial transform)
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute> Public Property STX As Double
    ''' <summary>
    ''' pixel y of the spatial transcriptomics spot.
    ''' (after spatial transform)
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute> Public Property STY As Double

    <XmlAttribute> Public Property SMX As Integer()
    <XmlAttribute> Public Property SMY As Integer()

    ''' <summary>
    ''' tissue tag data of the SMdata
    ''' </summary>
    ''' <returns></returns>
    <XmlText> Public Property TissueMorphology As String

    Private ReadOnly Property X As Integer Implements IPoint2D.X
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return STX
        End Get
    End Property

    Private ReadOnly Property Y As Integer Implements IPoint2D.Y
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return STY
        End Get
    End Property

    Public Iterator Function GetSMDataPoints() As IEnumerable(Of Point)
        For i As Integer = 0 To SMX.Length - 1
            Yield New Point(SMX(i), SMY(i))
        Next
    End Function

End Class
