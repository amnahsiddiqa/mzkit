Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq

<HideModuleName>
Public Module Extensions

    <Extension>
    Public Iterator Function ScalePixels(region As IEnumerable(Of TissueRegion),
                                         newDims As Size,
                                         Optional currentDims As Size = Nothing) As IEnumerable(Of TissueRegion)

        Dim tissue_map As TissueRegion() = region.ToArray

        If currentDims.IsEmpty Then
            currentDims = tissue_map.GetDimension
        End If

        For Each subregion As TissueRegion In tissue_map
            Yield subregion.ScalePixels(currentDims, newDims)
        Next
    End Function

    <Extension>
    Public Function ScalePixels(region As TissueRegion, currentDims As Size, newDims As Size) As TissueRegion
        Dim scalX As Double = newDims.Width / currentDims.Width
        Dim scalY As Double = newDims.Height / currentDims.Height

        Return New TissueRegion With {
            .color = region.color,
            .label = region.label,
            .points = region.points _
                .Select(Function(pi)
                            Return New Point(pi.X * scalX, pi.Y * scalY)
                        End Function) _
                .ToArray
        }
    End Function

    ''' <summary>
    ''' make polygon shape object raster matrix
    ''' </summary>
    ''' <param name="polygons"></param>
    ''' <param name="dimension"></param>
    ''' <param name="label"></param>
    ''' <param name="color"></param>
    ''' <returns></returns>
    <Extension>
    Public Function RasterGeometry2D(polygons As IEnumerable(Of Polygon2D),
                                     dimension As Size,
                                     label As String,
                                     color As Color) As TissueRegion

        Dim x As New List(Of Integer)
        Dim y As New List(Of Integer)
        Dim regions As Polygon2D() = polygons.SafeQuery.ToArray

        If regions.Length = 1 AndAlso regions(Scan0).length > 512 Then
            ' is a pack of density pixels
            Call x.AddRange(regions(Scan0).xpoints.Select(Function(xi) CInt(xi)))
            Call y.AddRange(regions(Scan0).ypoints.Select(Function(yi) CInt(yi)))
        Else
            For i As Integer = 1 To dimension.Width
                For j As Integer = 1 To dimension.Height
#Disable Warning
                    If regions.Any(Function(r) r.inside(i, j)) Then
                        Call x.Add(i)
                        Call y.Add(j)
                    End If
#Enable Warning
                Next
            Next
        End If

        Return New TissueRegion With {
            .color = color,
            .label = label,
            .points = x _
                .Select(Function(xi, i)
                            Return New Point(xi, y(i))
                        End Function) _
                .ToArray
        }
    End Function
End Module
