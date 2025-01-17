﻿#Region "Microsoft.VisualBasic::eb59792f581a5e6361be808c079706b1, mzkit\src\metadna\metaDNA\KEGGHandler.vb"

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

    '   Total Lines: 28
    '    Code Lines: 22
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.40 KB


    ' Class KEGGHandler
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) CreateIndex, Wraps
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Ms1.PrecursorType
Imports BioNovoGene.BioDeep.MSEngine
Imports SMRUCC.genomics.Assembly.KEGG.DBGET.bGetObject

<Assembly: InternalsVisibleTo("mzkit")>

''' <summary>
''' the metabolite annotation search engine for the kegg metabolites
''' </summary>
Public NotInheritable Class KEGGHandler : Inherits MSSearch(Of KEGGCompound)

    Private Sub New(compounds As IEnumerable(Of KEGGCompound), types As MzCalculator(), tolerance As Tolerance)
        Call MyBase.New(compounds, tolerance, types)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Wraps(compounds As IEnumerable(Of Compound)) As IEnumerable(Of KEGGCompound)
        Return compounds.Select(Function(c) New KEGGCompound With {.KEGG = c})
    End Function

    Public Overloads Shared Function CreateIndex(compounds As IEnumerable(Of KEGGCompound), types As MzCalculator(), tolerance As Tolerance) As MSSearch(Of KEGGCompound)
        Return New KEGGHandler(compounds, types, tolerance)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Function CreateIndex(compounds As IEnumerable(Of Compound), types As MzCalculator(), tolerance As Tolerance) As MSSearch(Of KEGGCompound)
        Return CreateIndex(Wraps(compounds), types, tolerance)
    End Function
End Class
