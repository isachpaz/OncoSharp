// OncoSharp
// Copyright (c) 2014 - 2025 Dr. Ilias Sachpazidis
// Licensed for non-commercial academic and research use only.
// Commercial use requires a separate license.
// See https://github.com/isachpaz/OncoSharp for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OncoSharp.Statistics.Abstractions.MLEEstimators;
using Plotly.NET;
using Plotly.NET.TraceObjects;

namespace OncoSharp.Statistics.Abstractions.Diagnostics
{
    public static class TcpDosePlotter
    {
        /// <summary>
        /// Plots predicted TCP and observed outcomes vs a scalar dose value for all cases.
        /// </summary>
        /// <typeparam name="TData">Input data type used by the estimator.</typeparam>
        /// <typeparam name="TParameters">Parameter type used by the estimator.</typeparam>
        /// <param name="estimator">TCP estimator with ComputeTcp implementation.</param>
        /// <param name="inputData">Input data for each observation.</param>
        /// <param name="observations">Observed outcomes (true/false).</param>
        /// <param name="parameters">Model parameters used to compute TCP.</param>
        /// <param name="doseSelector">Function that maps TData to a scalar dose value.</param>
        /// <param name="outputPath">Path to save the plot HTML (optional).</param>
        /// <param name="title">Plot title.</param>
        /// <param name="doseLabel">X-axis label for the dose.</param>
        /// <param name="curveX">Optional x-values for a model response curve.</param>
        /// <param name="curveY">Optional y-values for a model response curve.</param>
        /// <param name="curveName">Legend label for the model response curve.</param>
        /// <param name="width">Plot width in pixels.</param>
        /// <param name="height">Plot height in pixels.</param>
        /// <returns>Path to the saved HTML file.</returns>
        public static string PlotTcpVsDose<TData, TParameters>(
            TcpMaximumLikelihoodEstimator<TData, TParameters> estimator,
            IList<TData> inputData,
            IList<bool> observations,
            TParameters parameters,
            Func<TData, double> doseSelector,
            string outputPath = null,
            string title = "TCP vs Dose",
            string doseLabel = "Dose",
            IReadOnlyList<double> curveX = null,
            IReadOnlyList<double> curveY = null,
            string curveName = "Model curve",
            int width = 1000,
            int height = 700)
            where TParameters : new()
        {
            if (estimator == null) throw new ArgumentNullException(nameof(estimator));
            if (inputData == null) throw new ArgumentNullException(nameof(inputData));
            if (observations == null) throw new ArgumentNullException(nameof(observations));
            if (doseSelector == null) throw new ArgumentNullException(nameof(doseSelector));
            if (inputData.Count != observations.Count)
                throw new ArgumentException("Observations and inputData must have the same number of elements.");

            var doses = new List<double>(inputData.Count);
            var predicted = new List<double>(inputData.Count);
            var observed = new List<double>(inputData.Count);

            for (int i = 0; i < inputData.Count; i++)
            {
                double dose = doseSelector(inputData[i]);
                double tcp = estimator.ComputeTcp(parameters, inputData[i]);

                if (!IsFinite(dose))
                    throw new InvalidOperationException($"Dose selector returned a non-finite value at index {i}.");
                if (!IsFinite(tcp))
                    throw new InvalidOperationException($"ComputeTcp returned a non-finite value at index {i}.");

                doses.Add(dose);
                predicted.Add(tcp);
                observed.Add(observations[i] ? 1.0 : 0.0);
            }

            if (doses.Count == 0)
                throw new InvalidOperationException("No valid points available for plotting.");

            var observedMarker = Marker.init(Color: Color.fromString("black"), Size: 8);
            var observedChart = Chart2D.Chart.Point<double, double, string>(
                x: doses.ToArray(),
                y: observed.ToArray(),
                Name: "Observed",
                Marker: observedMarker);

            var predictedMarker = Marker.init(Color: Color.fromString("#1f77b4"), Size: 8);
            var predictedChart = Chart2D.Chart.Point<double, double, string>(
                x: doses.ToArray(),
                y: predicted.ToArray(),
                Name: "Predicted",
                Marker: predictedMarker);

            var charts = new List<GenericChart>
            {
                observedChart,
                predictedChart
            };

            List<double> curveXFiltered = null;
            List<double> curveYFiltered = null;
            if (curveX != null || curveY != null)
            {
                if (curveX == null || curveY == null)
                    throw new ArgumentException("curveX and curveY must be provided together.");
                if (curveX.Count != curveY.Count)
                    throw new ArgumentException("curveX and curveY must have the same number of points.");

                curveXFiltered = new List<double>(curveX.Count);
                curveYFiltered = new List<double>(curveY.Count);
                for (int i = 0; i < curveX.Count; i++)
                {
                    double x = curveX[i];
                    double y = curveY[i];
                    if (IsFinite(x) && IsFinite(y))
                    {
                        curveXFiltered.Add(x);
                        curveYFiltered.Add(y);
                    }
                }

                if (curveXFiltered.Count > 1)
                {
                    var curveLine = Line.init(Color: Color.fromString("#ff7f0e"), Width: 2.0);
                    var curveChart = Chart2D.Chart.Line<double, double, string>(
                        x: curveXFiltered.ToArray(),
                        y: curveYFiltered.ToArray(),
                        Name: curveName,
                        Line: curveLine);
                    charts.Add(curveChart);
                }
            }

            double xMin = doses.Min();
            double xMax = doses.Max();
            if (curveXFiltered != null && curveXFiltered.Count > 0)
            {
                xMin = Math.Min(xMin, curveXFiltered.Min());
                xMax = Math.Max(xMax, curveXFiltered.Max());
            }
            var halfLine = Line.init(Color: Color.fromString("gray"), Width: 1.5, Dash: StyleParam.DrawingStyle.Dash);
            var halfChart = Chart2D.Chart.Line<double, double, string>(
                x: new[] { xMin, xMax },
                y: new[] { 0.5, 0.5 },
                Name: "TCP=0.5",
                Line: halfLine);

            charts.Add(halfChart);
            var combined = Chart.Combine(charts);
            combined = Chart.WithTitle(title, null).Invoke(combined);
            combined = Chart.WithXAxisStyle<double, double, double>(TitleText: doseLabel).Invoke(combined);
            combined = Chart.WithYAxisStyle<double, double, double>(
                TitleText: "TCP",
                MinMax: Tuple.Create(0.0, 1.0)).Invoke(combined);
            combined = Chart.WithSize((double)width, (double)height).Invoke(combined);

            string targetPath = outputPath;
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                string safeTitle = string.Join("_", title.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
                if (string.IsNullOrWhiteSpace(safeTitle))
                {
                    safeTitle = "TcpVsDose";
                }

                string stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
                targetPath = Path.Combine(Path.GetTempPath(), $"{safeTitle}_{stamp}.html");
            }
            else
            {
                string extension = Path.GetExtension(targetPath);
                if (string.IsNullOrEmpty(extension) ||
                    (!extension.Equals(".html", StringComparison.OrdinalIgnoreCase) &&
                     !extension.Equals(".htm", StringComparison.OrdinalIgnoreCase)))
                {
                    targetPath += ".html";
                }
            }

            combined.SaveHtml(targetPath);
            return targetPath;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
