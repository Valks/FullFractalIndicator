namespace cAlgo.Indicators
{
    public class FractalEvent
    {
        public FractalEvent(int index, Fractal fractal)
        {
            this.fractal = fractal;
            this.index = index;
        }

        public int index { get; set; }
        public Fractal fractal { get; set; }
    }
}