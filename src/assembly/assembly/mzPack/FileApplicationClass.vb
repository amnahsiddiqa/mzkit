﻿#Region "Microsoft.VisualBasic::4f9d118fb8db53e02745768116208f81, mzkit\src\assembly\assembly\mzPack\FileApplicationClass.vb"

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

    '   Total Lines: 30
    '    Code Lines: 10
    ' Comment Lines: 18
    '   Blank Lines: 2
    '     File Size: 952.00 B


    '     Enum FileApplicationClass
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace mzData.mzWebCache

    ''' <summary>
    ''' 当前的这个mzpack原始数据文件的应用程序类型
    ''' </summary>
    Public Enum FileApplicationClass

        ''' <summary>
        ''' 普通LC-MS非靶向
        ''' </summary>
        <Description("0212F3ED-1FA8-4CB3-B95D-33F43AD60945")> LCMS
        ''' <summary>
        ''' 普通GC-MS靶向/非靶向
        ''' </summary>
        <Description("EE446B9F-204F-4FF7-8302-1D4480FE46AB")> GCMS
        ''' <summary>
        ''' LC-MS/MS靶向
        ''' </summary>
        <Description("60866B4E-708F-4B94-9B22-E4C21D4D97DD")> LCMSMS

#Region "Comprehensive Spectrum"

        ''' <summary>
        ''' GCxGC原始数据
        ''' </summary>
        <Description("B08A37D3-44BC-4D6C-86C9-D99726368446")> GCxGC
        ''' <summary>
        ''' 质谱成像
        ''' </summary>
        <Description("616DE505-3D78-45FA-BCA7-35ECEF3FE88D")> MSImaging
        ''' <summary>
        ''' 三维空间成像
        ''' </summary>
        <Description("616DE505-3D78-45FA-BCA7-35ECEF3FE83D")> MSImaging3D

        ''' <summary>
        ''' single cell metabolomics data
        ''' </summary>
        <Description("7F1953AB-E36A-4DFA-88D0-2EDCD0EC8BFC")> SingleCellsMetabolomics
#End Region
    End Enum

End Namespace
