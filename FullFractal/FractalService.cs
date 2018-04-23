using System;
using System.Collections.Generic;
using cAlgo.API.Internals;

namespace cAlgo.Indicators
{
    public class FractalService
    {
        private MarketSeries marketSeries;
        public FractalOptions options;
        public Fractal lastFractal { get; set; }

        private List<Action<FractalEvent>> onFractalListeners;

        public FractalService(MarketSeries marketSeries, FractalOptions options)
        {
            this.options = options;
            this.marketSeries = marketSeries;
            onFractalListeners = new List<Action<FractalEvent>>();
        }

        public void addFractal(Fractal fractal)
        {
            if (lastFractal != null)
                lastFractal.addFractal(fractal);

            lastFractal = fractal;
        }

        public void processIndex(int index)
        {
            if (index < options.period)
                return;

            detectLowFractal(index);
            detectHighFractal(index);
        }

        public Fractal getLastHighFractal(bool best = true)
        {
            if (lastFractal == null)
                return null;
            if (lastFractal.high)
                return best ? lastFractal.getBest() : lastFractal;
            return lastFractal.getPrevious(best);
        }

        public Fractal getLastLowFractal(bool best = true)
        {
            if (lastFractal == null)
                return null;
            if (!lastFractal.high)
                return best ? lastFractal.getBest() : lastFractal;
            return lastFractal.getPrevious(best);
        }

        public Fractal getLastFractal(bool best = true)
        {
            if (lastFractal == null)
                return null;
            return best ? lastFractal.getBest() : lastFractal;
        }

        public Action onFractal(Action<FractalEvent> listener)
        {
            onFractalListeners.Add(listener);
            return delegate () { onFractalListeners.Remove(listener); };
        }

        private bool isHighFractal(int middleIndex)
        {
            int halfPeriod = options.period / 2;
            double middleValue = marketSeries.High[middleIndex];
            for (int i = (middleIndex - halfPeriod); i <= (middleIndex + halfPeriod); i++)
            {
                if (middleValue < marketSeries.High[i])
                    return false;
            }
            return true;
        }

        private bool isLowFractal(int middleIndex)
        {
            int halfPeriod = getHalfPeriod();
            double middleValue = marketSeries.Low[middleIndex];
            for (int i = (middleIndex - halfPeriod); i <= (middleIndex + halfPeriod); i++)
            {
                if (middleValue > marketSeries.Low[i])
                    return false;
            }
            return true;
        }

        private void detectHighFractal(int index)
        {
            int middleIndex = getMiddleIndex(index);
            bool highFractal = isHighFractal(middleIndex);

            if (highFractal)
            {
                Fractal fractal = new Fractal(middleIndex, marketSeries.High[middleIndex], true);
                processFractal(index, fractal);
            }
        }

        private void detectLowFractal(int index)
        {
            int middleIndex = getMiddleIndex(index);
            bool lowFractal = isLowFractal(middleIndex);

            if (lowFractal)
            {
                Fractal fractal = new Fractal(middleIndex, marketSeries.Low[middleIndex], false);
                processFractal(index, fractal);
            }
        }

        public int getMiddleIndex(int index)
        {
            return index - getHalfPeriod();
        }

        private int getHalfPeriod()
        {
            return (options.period - (options.period % 2)) / 2;
        }

        private void processFractal(int index, Fractal fractal)
        {
            addFractal(fractal);
            FractalEvent fractalEvent = new FractalEvent(index, fractal);
            triggerOnFractal(fractalEvent);
        }

        private void triggerOnFractal(FractalEvent fractalEvent)
        {
            foreach (Action<FractalEvent> listener in onFractalListeners)
            {
                listener(fractalEvent);
            }
        }

    }
}