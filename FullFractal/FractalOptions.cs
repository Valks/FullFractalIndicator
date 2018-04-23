namespace cAlgo.Indicators
{
    public class FractalOptions
    {
        public FractalOptions(int period, bool showHorizontalContinuationLine, bool showVerticalContinuationLine, bool linkHighLow)
        {
            this.period = period;
            this.showHorizontalContinuationLine = showHorizontalContinuationLine;
            this.showVerticalContinuationLine = showVerticalContinuationLine;
            this.linkHighLow = linkHighLow;
        }
        
        public FractalOptions()
        {
            this.period = 5;
            this.showHorizontalContinuationLine = true;
            this.showVerticalContinuationLine = true;
            this.linkHighLow = true;
        }

        public int period { get; set; }
        public bool showHorizontalContinuationLine { get; set; }
        public bool showVerticalContinuationLine { get; set; }
        public bool linkHighLow { get; set; }
    }
}