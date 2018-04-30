using System;
using cAlgo.API;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class FullFractal : Indicator
    {
        [Parameter(DefaultValue = 5, MinValue = 5)]
        public int period { get; set; }

        [Parameter("Highlight bad fractals", DefaultValue = true)]
        public bool highlightBadFractals { get; set; }

        [Parameter("Draw circles", DefaultValue = true)]
        public bool drawCircles { get; set; }

        [Parameter("Draw errors", DefaultValue = true)]
        public bool drawArrows { get; set; }

        [Output("High continuation line", Color = Colors.Blue, PlotType = PlotType.Line, LineStyle = LineStyle.Dots)]
        public IndicatorDataSeries horizontalContinuationLineHigh { get; set; }

        [Output("Low continuation line", Color = Colors.Orange, PlotType = PlotType.Line, LineStyle = LineStyle.Dots)]
        public IndicatorDataSeries horizontalContinuationLineLow { get; set; }

        [Output("High-Low line", Color = Colors.White, PlotType = PlotType.Line, LineStyle = LineStyle.Lines)]
        public IndicatorDataSeries highLowLink { get; set; }

//        [Parameter("Link highs and lows", DefaultValue = true)]
//        public bool linkHighLow { get; set; }

        private const String arrowUp = "▲";
        private const String arrowDown = "▼";
        private const String circle = "◯";
        private const String badSignal = "⛝";
        private const Colors linkColor = Colors.Beige;
        private const Colors highHorizontalLineColor = Colors.Red;
        private const Colors lowHorizontalLineColor = Colors.DarkCyan;
        private const LineStyle linkLineStyle = LineStyle.Lines;
        private FractalService fractalService;

        protected override void Initialize()
        {
            fractalService = new FractalService(MarketSeries, period);
            fractalService.onFractal(plot);
        }

        public override void Calculate(int index)
        {
            int effectiveIndex = index - 1;

            fractalService.processIndex(effectiveIndex);
            printHorizontal(index);
        }

        private void printHorizontal(int index)
        {
            Fractal lastfractal = fractalService.getLastFractal();
            if (lastfractal == null)
                return;
            Fractal previousFractal = fractalService.getLastFractal().getPrevious();
            if (previousFractal == null)
                return;
            Fractal highFractal = lastfractal.high ? lastfractal : previousFractal;
            Fractal lowFractal = lastfractal.low ? lastfractal : previousFractal;

            double highestHighValue = highFractal.value;
            double lowestLowValue = lowFractal.value;
            highestHighValue = adjustCurrentHigh(index, highFractal, highestHighValue, lowFractal, ref lowestLowValue);

            for (int i = highFractal.index + 1; i < index; i++)
                horizontalContinuationLineHigh[i] = highestHighValue;
            for (int i = lowFractal.index + 1; i < index; i++)
                horizontalContinuationLineLow[i] = lowestLowValue;
        }

        private double adjustCurrentHigh(int index, Fractal highFractal, double highestHighValue, Fractal lowFractal, ref double lowestLowValue)
        {
            for (int i = highFractal.index; i < index; i++)
                if (MarketSeries.High[i] > highestHighValue)
                    highestHighValue = MarketSeries.High[i];
            for (int i = lowFractal.index; i < index; i++)
                if (MarketSeries.Low[i] < lowestLowValue)
                    lowestLowValue = MarketSeries.Low[i];
            return highestHighValue;
        }


        private void plot(FractalEvent fractalEvent)
        {
            Fractal fractal = fractalEvent.fractal.getBest();

            if (drawCircles)
                drawCircle(fractal);

            linkHighs(fractalEvent);
            linkLows(fractalEvent);
            linkFractals(fractalEvent);

            if (drawArrows)
                plotArrow(fractal);
        }

        private void drawCircle(Fractal fractal)
        {
            string label = getCircleLabel(fractal);
            ChartObjects.DrawText(label, circle, fractal.index, fractal.value, VerticalAlignment.Center, HorizontalAlignment.Center, Colors.Aqua);

            Fractal previous = fractal.getPreviousOfSameSide();
            if (previous != null)
            {
                Fractal current = fractal.getPrevious(false);
                while (current.index > previous.index)
                {
                    if (current.high != fractal.high)
                    {
                        current = current.getPrevious(false);
                        continue;
                    }
                    ChartObjects.RemoveObject(getCircleLabel(current));
//                    if (!highlightBadFractals)
//                        ChartObjects.RemoveObject(getArrowLabel(current));
                    if (highlightBadFractals)
                        ChartObjects.DrawText(getCircleLabel(current), circle, current.index, current.value, VerticalAlignment.Center, HorizontalAlignment.Center, Colors.Red);

                    current = current.getPrevious(false);
                }
            }
        }

        private static string getCircleLabel(Fractal fractal)
        {
            return "circle-" + (fractal.high ? "H" : "L") + "-" + fractal.index;
        }

        private void linkHighs(FractalEvent fractalEvent)
        {
            Fractal fractal = fractalEvent.fractal.getBest();
            Fractal previous = fractal.getPreviousOfSameSide();
            if (fractal.high && previous != null)
            {
                double highest = Math.Max(fractal.value, previous.value);
                for (int i = previous.index + 1; i < fractal.index; i++)
                    horizontalContinuationLineHigh[i] = highest;

                for (int i = fractal.index; i < fractalEvent.index; i++)
                    horizontalContinuationLineHigh[i] = fractal.value;
            }
        }

        private void linkFractals(FractalEvent fractalEvent)
        {
            Fractal fractal = fractalEvent.fractal.getBest();
            Fractal previous = fractal.getPreviousOfSameSide();
            if (previous != null)
            {
                for (int i = previous.index + 1; i < fractal.index - 1; i++)
                    highLowLink[i] = Double.NaN;

                highLowLink[previous.index] = previous.value;
                highLowLink[fractal.index] = fractal.value;
            }
        }

        private void linkLows(FractalEvent fractalEvent)
        {
            Fractal fractal = fractalEvent.fractal.getBest();
            Fractal previous = fractal.getPreviousOfSameSide();
            if (fractal.low && previous != null)
            {
                double lowest = Math.Min(fractal.value, previous.value);
                for (int i = previous.index + 1; i < fractal.index; i++)
                    horizontalContinuationLineLow[i] = lowest;

                for (int i = fractal.index; i < fractalEvent.index; i++)
                    horizontalContinuationLineLow[i] = fractal.value;
            }
        }

        private void plotArrow(Fractal fractal)
        {
            String arrow = fractal.isHigher() ? arrowUp : arrowDown;
            Colors color = getArrowColor(fractal);
            ChartObjects.DrawText(getArrowLabel(fractal), arrow, fractal.index, getTextPosition(fractal, 1.5), VerticalAlignment.Center, HorizontalAlignment.Center, color);
        }

        private static string getArrowLabel(Fractal fractal)
        {
            return "arrow-" + (fractal.high ? "high" : "low") + "-" + fractal.index;
        }

        private double getTextPosition(Fractal fractal, double offsetMultiplier = 2)
        {
            double peakValue = fractal.high ? MarketSeries.High[fractal.index] : MarketSeries.Low[fractal.index];
            double distanceToBar = Symbol.PipSize * ScaleHelper.getScale(TimeFrame) * offsetMultiplier;
            double yPos = peakValue + distanceToBar * (fractal.high ? 1 : -1);
            return yPos;
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
