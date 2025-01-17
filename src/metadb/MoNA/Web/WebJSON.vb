﻿Imports System.Runtime.CompilerServices
Imports BioNovoGene.Analytical.MassSpectrometry.Math.Spectra
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class WebJSON

    Friend Const urlBase As String = "https://mona.fiehnlab.ucdavis.edu/rest/spectra/%s"
    Friend Const query As String = "https://mona.fiehnlab.ucdavis.edu/rest/spectra/search?endpoint=search&query=compound.names=q=%27name=like=%22{q}%22%27%20and%20(tags.text==%22LC-MS%22)%20and%20(metaData=q=%27name==%22ms%20level%22%20and%20value==%22MS2%22%27)&size=30"

    Public Property spectrum As String
    Public Property id As String

    Public Overrides Function ToString() As String
        Return id
    End Function

    Public Function ParseMatrix() As LibraryMatrix
        Dim data As String() = spectrum.Split(" "c)
        Dim mzinto As ms2() = data _
            .Select(Function(str)
                        Dim t As String() = str.Split(":"c)
                        Dim m As New ms2 With {
                            .mz = Val(t(0)),
                            .intensity = Val(t(1))
                        }

                        Return m
                    End Function) _
            .ToArray

        Return New LibraryMatrix With {
            .name = id,
            .ms2 = mzinto
        }
    End Function

    Public Shared Function SearchLCMSByName(name As String, Optional cache As String = "./.mona/") As WebJSON()
        Static cachePool As New Dictionary(Of String, QueryWeb)

        Return cachePool _
            .ComputeIfAbsent(
                key:=cache,
                lazyValue:=Function()
                               Return New QueryWeb(cache)
                           End Function) _
            .Query(Of WebJSON())((False, name), cacheType:=".json")
    End Function

    Public Shared Function GetJson(id As String, Optional cache As String = "./.mona/") As WebJSON
        Static cachePool As New Dictionary(Of String, QueryWeb)

        Return cachePool _
            .ComputeIfAbsent(
                key:=cache,
                lazyValue:=Function()
                               Return New QueryWeb(cache)
                           End Function) _
            .Query(Of WebJSON)((False, id), cacheType:=".json")
    End Function

End Class

Friend Class QueryWeb : Inherits WebQuery(Of (name_query As Boolean, q As String))

    Public Sub New(<CallerMemberName>
                   Optional cache As String = Nothing,
                   Optional interval As Integer = -1,
                   Optional offline As Boolean = False)

        Call MyBase.New(
            url:=AddressOf getRestUrl,
            contextGuid:=Function(id) $"{id.name_query}+{id.q}".MD5,
            parser:=AddressOf parseJSON,
            prefix:=Function(id) id.Substring(0, 2),
            cache:=cache,
            interval:=interval,
            offline:=offline
        )
    End Sub

    Private Shared Function parseJSON(json As String, schema As Type) As Object
        If schema Is GetType(WebJSON) Then
            Return json.LoadJSON(Of WebJSON)
        ElseIf schema Is GetType(WebJSON()) Then
            Return json.LoadJSON(Of WebJSON())
        Else
            Throw New NotImplementedException
        End If
    End Function

    Private Shared Function getRestUrl(context As (name_query As Boolean, q As String)) As String
        If context.name_query Then
            Return WebJSON.query.Replace("{q}", context.q.UrlEncode)
        Else
            Return sprintf(WebJSON.urlBase, context.q)
        End If
    End Function
End Class