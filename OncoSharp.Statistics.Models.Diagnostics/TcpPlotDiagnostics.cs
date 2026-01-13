// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using OncoSharp.Core.Quantities.Helpers.Maths;
using OncoSharp.Radiobiology.GEUD;
using OncoSharp.RTDomainModel;
using OncoSharp.Statistics.Abstractions.Diagnostics;
using OncoSharp.Statistics.Models.Tcp;
using OncoSharp.Statistics.Models.Tcp.Parameters;

namespace OncoSharp.Statistics.Models.Diagnostics
{
    public static class TcpPlotDiagnostics
    {
        /// <summary>
        /// Plots TCP vs gEUD for a Probit TCP model using the estimator's ComputeTcp method.
        /// </summary>
        /// <param name="estimator">Probit TCP estimator instance.</param>
        /// <param name="inputData">Plan items for each observation.</param>
        /// <param name="observations">Observed outcomes (true/false).</param>
        /// <param name="parameters">Model parameters used to compute TCP.</param>
        /// <param name="outputPath">Path to save the plot HTML (optional).</param>
        /// <param name="title">Plot title.</param>
        /// <param name="doseLabel">X-axis label for the dose.</param>
        /// <param name="width">Plot width in pixels.</param>
        /// <param name="height">Plot height in pixels.</param>
        /// <returns>Path to the saved HTML file.</returns>
        public static string PlotProbitTcpVsGeud(
            ProbitTcpEstimator estimator,
            IList<IPlanItem> inputData,
            IList<bool> observations,
            ProbitTcpParameters parameters,
            string outputPath = null,
            string title = "TCP vs gEUD",
            string doseLabel = "gEUD (EQD2)",
            int width = 1000,
            int height = 700)
        {
            if (estimator == null) throw new ArgumentNullException(nameof(estimator));
            if (inputData == null) throw new ArgumentNullException(nameof(inputData));
            if (observations == null) throw new ArgumentNullException(nameof(observations));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            var geudModel = new Geud2GyModel(parameters.AlphaVolumeEffect);
            double[] curveX = null;
            double[] curveY = null;
            var observedLabels = new List<string>(inputData.Count);

            var doseSamples = new List<double>(inputData.Count);
            foreach (var planItem in inputData)
            {
                var structureId = estimator.StructureSelector(planItem);
                var points = planItem.CalculateEqd2DoseDistribution(structureId, estimator.AlphaOverBeta);
                double geud = geudModel.Calculate(points).Value;
                if (IsFinite(geud))
                {
                    doseSamples.Add(geud);
                }
                observedLabels.Add(FormatCaseLabel(planItem));
            }

            if (doseSamples.Count > 1 && parameters.D50 > 0.0 && parameters.Gamma50 >= 0.0)
            {
                double minDose = doseSamples.Min();
                double maxDose = doseSamples.Max();
                if (IsFinite(minDose) && IsFinite(maxDose) && maxDose > minDose)
                {
                    const int curvePoints = 200;
                    double step = (maxDose - minDose) / (curvePoints - 1);
                    curveX = new double[curvePoints];
                    curveY = new double[curvePoints];
                    for (int i = 0; i < curvePoints; i++)
                    {
                        double dose = minDose + i * step;
                        curveX[i] = dose;
                        curveY[i] = ProbitResponse(dose, parameters.D50, parameters.Gamma50);
                    }
                }
            }

            return TcpDosePlotter.PlotTcpVsDose(
                estimator,
                inputData,
                observations,
                parameters,
                doseSelector: planItem =>
                {
                    var structureId = estimator.StructureSelector(planItem);
                    var points = planItem.CalculateEqd2DoseDistribution(structureId, estimator.AlphaOverBeta);
                    return geudModel.Calculate(points).Value;
                },
                outputPath: outputPath,
                title: title,
                doseLabel: doseLabel,
                observedLabels: observedLabels,
                curveX: curveX,
                curveY: curveY,
                curveName: "Model response",
                width: width,
                height: height);
        }

        private static string FormatCaseLabel(IPlanItem planItem)
        {
            string patientId = planItem?.PatientId ?? string.Empty;
            string planId = planItem?.PlanId ?? string.Empty;
            return string.IsNullOrWhiteSpace(planId) ? patientId : $"{patientId}/{planId}";
        }

        private static double ProbitResponse(double dose, double d50, double gamma50)
        {
            if (d50 <= 0.0 || gamma50 < 0.0)
                return 0.5;

            double response = gamma50 * Math.Sqrt(Math.PI) * (1.0 - dose / d50);
            return 0.5 * (1.0 - MathUtils.Erf(response));
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
