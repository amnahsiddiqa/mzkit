﻿#Region "Microsoft.VisualBasic::c9a69eb2341683ea8ddfc09834fbe47f, mzkit\src\assembly\assembly\MarkupData\mzML\XML\Components.vb"

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

    '   Total Lines: 36
    '    Code Lines: 27
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.17 KB


    '     Class fileDescription
    ' 
    '         Properties: fileContent
    ' 
    '         Function: ToString
    ' 
    '     Class List
    ' 
    '         Properties: count
    ' 
    '     Class DataList
    ' 
    '         Properties: defaultDataProcessingRef
    ' 
    '     Class run
    ' 
    '         Properties: chromatogramList, defaultInstrumentConfigurationRef, defaultSourceFileRef, id, spectrumList
    '                     startTimeStamp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports BioNovoGene.Analytical.MassSpectrometry.Assembly.MarkupData.mzML.ControlVocabulary

Namespace MarkupData.mzML

    Public Class fileDescription

        Public Property fileContent As Params
        Public Property sourceFileList As SourceFileList
        Public Property contact As Params

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return fileContent.cvParams.Select(Function(a) a.name).JoinBy("; ")
        End Function

    End Class

    Public Class SourceFileList : Inherits List

        <XmlElement("sourceFile")>
        Public Property sourceFile As SourceFile()

    End Class

    Public Class SourceFile : Inherits Params

        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property location As String
        <XmlAttribute> Public Property name As String


    End Class

    Public Class List

        <XmlAttribute> Public Property count As Integer

    End Class

    Public Class DataList : Inherits List
        <XmlAttribute>
        Public Property defaultDataProcessingRef As String
    End Class

    Public Class run
        <XmlAttribute> Public Property id As String
        <XmlAttribute> Public Property defaultInstrumentConfigurationRef As String
        <XmlAttribute> Public Property startTimeStamp As String
        <XmlAttribute> Public Property defaultSourceFileRef As String

        Public Property chromatogramList As chromatogramList
        Public Property spectrumList As spectrumList
    End Class
End Namespace
