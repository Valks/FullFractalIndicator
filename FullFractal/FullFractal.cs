using System;
using cAlgo.API;
using cAlgo.Indicators;

namespace cAlgo
{
    /**
     * FullFractal - Version 1.4
     */
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class FullFractal : Indicator
    {
        [Parameter("Period", DefaultValue = 5, MinValue = 5)]
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

        [Parameter("Play sound on new High ...\n(i.e: C:\\Windows\\Media\\chimes.wav)", DefaultValue = "")]
        public string newHighSound { get; set; }

        [Parameter("Play sound on new Low ...\n(i.e: C:\\Windows\\Media\\chord.wav)", DefaultValue = "")]
        public string newLowSound { get; set; }

        [Parameter("Send notification to email", DefaultValue = "")]
        public string emailNewFractalNotificationTo { get; set; }


        private const String arrowUp = "▲";
        private const String arrowDown = "▼";
        private const String circle = "◯";
        private FractalService fractalService;

        protected override void Initialize()
        {
            fractalService = new FractalService(MarketSeries, period);
            fractalService.onFractal(newFractalHandler);
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


        private void newFractalHandler(FractalEvent fractalEvent)
        {
            Fractal fractal = fractalEvent.fractal.getBest();

            drawCircle(fractal);

            linkHighs(fractalEvent);
            linkLows(fractalEvent);
            linkFractals(fractalEvent);

            if (drawArrows)
                plotArrow(fractal);

            if (!IsRealTime)
                return;

            sendEmailNotification(fractal);
            playSoundNotification(fractal);
        }

        private void playSoundNotification(Fractal fractal)
        {
            if (fractal.high && newHighSound.Length > 0)
                Notifications.PlaySound(newHighSound);
            else if (fractal.low && newLowSound.Length > 0)
                Notifications.PlaySound(newLowSound);
        }

        private void sendEmailNotification(Fractal fractal)
        {
            string fractalType;
            if (fractal.getFractalType() == FractalType.HigherHigh)
                fractalType = "higher high";
            else if (fractal.getFractalType() == FractalType.HigherLow)
                fractalType = "higher low";
            else if (fractal.getFractalType() == FractalType.LowerHigh)
                fractalType = "lower high";
            else
                fractalType = "lower low";

            string notificationEmailBody = "You have a new " + fractalType + " fractal on symbol " + Symbol.Code + ".";
            if (emailNewFractalNotificationTo.Length > 0)
            {
                Print("Sending email to {0}", emailNewFractalNotificationTo);
                Notifications.SendEmail("lizalves.alves@gmail.com", emailNewFractalNotificationTo, "New " + fractalType + " fractal", notificationEmailBody);
            }
        }

        private void drawCircle(Fractal fractal)
        {
            if (drawCircles)
                ChartObjects.DrawText(getCircleLabel(fractal), circle, fractal.index, fractal.value, VerticalAlignment.Center, HorizontalAlignment.Center, Colors.Aqua);

            Fractal previous = fractal.getPreviousOfSameSide();
            if (previous != null)
            {
                foreach (Fractal badFractal in fractal.getBadFractals())
                {
                    if (drawCircles)
                        ChartObjects.RemoveObject(getCircleLabel(badFractal));
                    if (!highlightBadFractals)
                        ChartObjects.RemoveObject(getArrowLabel(badFractal));
                    if (highlightBadFractals)
                        ChartObjects.DrawText(getCircleLabel(badFractal), circle, badFractal.index, badFractal.value, VerticalAlignment.Center, HorizontalAlignment.Center, Colors.Red);

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
            Fractal previous = fractal.getPrevious();
            if (previous != null)
            {
                for (int i = previous.index + 1; i < fractal.index; i++)
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
            ChartObjects.DrawText(getArrowLabel(fractal), arrow, fractal.index, getTextPosition(fractal, 0.9), VerticalAlignment.Center, HorizontalAlignment.Center, color);
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
