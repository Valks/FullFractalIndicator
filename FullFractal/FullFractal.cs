using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

using System.Collections.Generic;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class FullFractal : Indicator
    {
        [Parameter(DefaultValue = 5, MinValue = 5)]
        public int period { get; set; }

        [Parameter("Horizontal Continuation line", DefaultValue = true)]
        public bool showHorizontalContinuationLine { get; set; }

        [Parameter("Vertical Continuation line", DefaultValue = true)]
        public bool showVerticalContinuationLine { get; set; }

        [Parameter("Link highs and lows", DefaultValue = true)]
        public bool linkHighLow { get; set; }

        [Parameter("Print debug index", DefaultValue = false)]
        public bool printDebugIndex { get; set; }

        private const String circle = "◯";
        private const String arrowUp = "▲";
        private const String arrowDown = "▼";
        private const String badSignal = "⛝";
        private const Colors linkColor = Colors.Beige;
        private const Colors highHorizontalLineColor = Colors.Red;
        private const Colors lowHorizontalLineColor = Colors.DarkCyan;
        private const LineStyle linkLineStyle = LineStyle.Lines;
        private FractalService fractalService;

        protected override void Initialize()
        {
            FractalOptions options = new FractalOptions(period, showHorizontalContinuationLine, showVerticalContinuationLine, linkHighLow);
            fractalService = new FractalService(MarketSeries, options);
            fractalService.onFractal(plot);
        }

        public override void Calculate(int index)
        {
            int effectiveIndex = index - 1;
            fractalService.processIndex(effectiveIndex);

            plotHorizontalContinuationLine(index, fractalService.getLastHighFractal());
            plotHorizontalContinuationLine(index, fractalService.getLastLowFractal());
        }


        private void plot(FractalEvent fractalEvent)
        {
            Fractal fractal = fractalEvent.fractal;
            plotFractalsLink(fractal.getPrevious(), fractal.getBest());
            plotBadFractalSignals(fractal);
            plotArrow(fractal);
            plotVerticalContinuationLine(fractal.getBest());
        }

        private void plotVerticalContinuationLine(Fractal fractal)
        {
            if (!showVerticalContinuationLine)
                return;

            Fractal previousSameSideFractal = fractal.getPreviousOfSameSide();
            if (previousSameSideFractal == null)
                return;

            Colors color = fractal.high ? Colors.Brown : Colors.Blue;
            String name = previousSameSideFractal.index + "-vertical-line-" + (fractal.high ? "high" : "low");
            ChartObjects.DrawLine(name, fractal.index, previousSameSideFractal.value, fractal.index, fractal.value, color, 1, LineStyle.Dots);
        }

        private void plotHorizontalContinuationLine(int index, Fractal fractal)
        {
            if (!showHorizontalContinuationLine || fractal == null)
                return;

            int middleIndex = fractalService.getMiddleIndex(index - 1);
            bool isNewFractal = middleIndex == fractal.index;
            if (isNewFractal)
                drawHorizontalLineForPreviousFractalOfSameSide(fractal, middleIndex);

            int lastOpositeFractalIndex = getPreviousIndex(fractal);
            String name = (fractal.high ? "high" : "low") + "-horizontal-line-" + lastOpositeFractalIndex;
            drawHorizontalLine(index, fractal, name);
        }

        private void drawHorizontalLineForPreviousFractalOfSameSide(Fractal fractal, int middleIndex)
        {
            Fractal previousOfSameSide = fractal.getPreviousOfSameSide();
            if (previousOfSameSide == null)
                return;
            int previousIndex = getPreviousIndex(previousOfSameSide);
            String newLineName = (fractal.high ? "high" : "low") + "-horizontal-line-" + previousIndex;
            drawHorizontalLine(middleIndex, previousOfSameSide, newLineName);
        }

        private void drawHorizontalLine(int index, Fractal fractal, string name)
        {
            Colors color = fractal.high ? highHorizontalLineColor : lowHorizontalLineColor;
            ChartObjects.DrawLine(name, fractal.index, fractal.value, index, fractal.value, color, 1, LineStyle.Dots);
        }

        private void plotFractalsLink(Fractal fractal1, Fractal fractal2)
        {
            if (!linkHighLow || fractal1 == null || fractal2 == null)
                return;
            ChartObjects.DrawLine(fractal1.index + "-link", fractal1.index, fractal1.value, fractal2.index, fractal2.value, linkColor, 1, linkLineStyle);
        }

        private void plotBadFractalSignals(Fractal fractal)
        {
            List<Fractal> allWorse = fractal.getBadFractals();
            for (int i = 0; i < allWorse.Count; i++)
                plotBadFractalSignal(getPreviousIndex(fractal) + "-badSignal-" + i, allWorse[i]);
        }

        private void plotBadFractalSignal(String name, Fractal fractal)
        {
            ChartObjects.DrawText(name, badSignal, fractal.index, getTextPosition(fractal, 1.9), VerticalAlignment.Center, HorizontalAlignment.Center, Colors.Aqua);
        }

        private void plotArrow(Fractal fractal)
        {
            String name = fractal.index + "-arrow-" + (fractal.high ? "high" : "low");
            String arrow = fractal.isHigher() ? arrowUp : arrowDown;
            Colors color = getArrowColor(fractal);
            ChartObjects.DrawText(name, arrow, fractal.index, getTextPosition(fractal, 0.9), VerticalAlignment.Center, HorizontalAlignment.Center, color);
            if (printDebugIndex)
                ChartObjects.DrawText(fractal.index + "-index", fractal.index + "", fractal.index, getTextPosition(fractal, 1.5), VerticalAlignment.Center, HorizontalAlignment.Center, Colors.Aqua);
        }

        private double getTextPosition(Fractal fractal, double offsetMultiplier = 2)
        {
            double peakValue = fractal.high ? MarketSeries.High[fractal.index] : MarketSeries.Low[fractal.index];
            double distanceToBar = Symbol.PipSize * ScaleHelper.getScale(TimeFrame) * offsetMultiplier;
            double yPos = peakValue + distanceToBar * (fractal.high ? 1 : -1);
            return yPos;
        }

        private static int getPreviousIndex(Fractal fractal)
        {
            Fractal previous = fractal == null ? null : fractal.getPrevious();
            int previousIndex = previous == null ? 0 : previous.index;
            return previousIndex;
        }

        private static Colors getArrowColor(Fractal fractal)
        {
            switch (fractal.getFractalType())
            {
                case FractalType.HigherHigh:
                    return Colors.OrangeRed;
                case FractalType.LowerHigh:
                    return Colors.Red;
                case FractalType.HigherLow:
                    return Colors.DarkCyan;
                case FractalType.LowerLow:
                    return Colors.Blue;
            }
            return Colors.White;
        }
    }
}
