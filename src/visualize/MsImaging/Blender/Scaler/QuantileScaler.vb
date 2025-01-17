﻿Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Quantile

Namespace Blender.Scaler

    Public Class QuantileScaler : Inherits Scaler

        ReadOnly q As Double

        Sub New(Optional q As Double = 0.5)
            Me.q = q
        End Sub

        Protected Overrides Function DoIntensityScale(into() As Double) As Double()
            Dim quantile As QuantileEstimationGK = into.GKQuantile
            Dim cutoff As Double = quantile.Query(q)
            Dim v As New Vector(into)

            v(v > cutoff) = Vector.Scalar(cutoff)

            Return v
        End Function

        Public Overrides Function ToString() As String
            Return $"Q({q.ToString("F4")})"
        End Function
    End Class
End Namespace