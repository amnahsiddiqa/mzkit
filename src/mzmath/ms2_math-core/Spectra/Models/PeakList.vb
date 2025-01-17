﻿#Region "Microsoft.VisualBasic::f8d8955a612c949cb1025a57b788cbde, mzkit\src\assembly\sciexWiffReader\WiffFileReader\PeakList.vb"

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

'   Total Lines: 11
'    Code Lines: 8
' Comment Lines: 0
'   Blank Lines: 3
'     File Size: 243.00 B


' Class PeakList
' 
'     Constructor: (+1 Overloads) Sub New
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1

Namespace Spectra

    Public Class PeakList

        ''' <summary>
        ''' the ion fragment mass list
        ''' </summary>
        ''' <returns></returns>
        Public Property mz As Double()
        ''' <summary>
        ''' the signal intensity strength 
        ''' value of the corresponding ion 
        ''' fragment mass data.
        ''' </summary>
        ''' <returns></returns>
        Public Property into As Double()

        ''' <summary>
        ''' the number of the ion fragments 
        ''' in current peak list object 
        ''' data.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property size As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return mz.Length
            End Get
        End Property

        <DebuggerStepThrough>
        Public Sub New(masses As Double(), intensities As Double())
            Me.mz = masses
            Me.into = intensities
        End Sub

        Sub New()
        End Sub

    End Class

    Public Interface IMsScan

        Function GetMs() As IEnumerable(Of ms2)
        Function GetMzIonIntensity(mz As Double, mzdiff As Tolerance) As Double

    End Interface
End Namespace