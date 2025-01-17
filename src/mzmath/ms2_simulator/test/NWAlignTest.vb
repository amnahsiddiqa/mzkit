﻿#Region "Microsoft.VisualBasic::418124a83bc5e08b1a50c414f0c94992, mzkit\src\mzmath\ms2_simulator\test\NWAlignTest.vb"

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

    '   Total Lines: 16
    '    Code Lines: 9
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 507.00 B


    ' Module NWAlignTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra

Module NWAlignTest

    Sub Main()
        Dim a As LibraryMatrix = {1, 3, 4, 5, 6, 7, 8, 9, 10}
        Dim b As LibraryMatrix = {1, 3, 4, 5, 6, 7, 8, 9, 10, 12}
        Dim c As LibraryMatrix = {1, 2, 4, 5, 6, 7, 8, 9, 10}

        '  Dim self = GlobalAlignment.NWGlobalAlign(a, a)
        '  Dim insert = GlobalAlignment.NWGlobalAlign(a, b)
        ' Dim substitue = GlobalAlignment.NWGlobalAlign(a, c)

        Pause()
    End Sub
End Module
