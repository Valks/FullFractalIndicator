using System.Collections.Generic;
using cAlgo.API;

namespace cAlgo.Indicators
{
    
    public class ScaleHelper
    {
        private static Dictionary<TimeFrame, double> scale = new Dictionary<TimeFrame, double> 
        {
            {
                TimeFrame.Minute,
                1
            },
            {
                TimeFrame.Minute2,
                toScale(2) 
            },
            {
                TimeFrame.Minute3,
                toScale(3)
            },
            {
                TimeFrame.Minute4,
                toScale(4)
            },
            {
                TimeFrame.Minute5,
                toScale(5)
            },
            {
                TimeFrame.Minute6,
                toScale(6)
            },
            {
                TimeFrame.Minute7,
                toScale(7)
            },
            {
                TimeFrame.Minute8,
                toScale(8)
            },
            {
                TimeFrame.Minute9,
                toScale(9)
            },
            {
                TimeFrame.Minute10,
                toScale(10)
            },
            {
                TimeFrame.Minute15,
                toScale(15)
            },
            {
                TimeFrame.Minute20,
                toScale(20)
            },
            {
                TimeFrame.Minute30,
                toScale(16)
            },
            {
                TimeFrame.Minute45,
                toScale(18)
            },
            {
                TimeFrame.Hour,
                toScale(40)
            },
            {
                TimeFrame.Hour2,
                toScale(45)
            },
            {
                TimeFrame.Hour3,
                toScale(50)
            },
            {
                TimeFrame.Hour4,
                toScale(55)
            },
            {
                TimeFrame.Hour6,
                toScale(60)
            },
            {
                TimeFrame.Hour8,
                toScale(65)
            },
            {
                TimeFrame.Hour12,
                toScale(75)
            },
            {
                TimeFrame.Daily,
                toScale(105)
            },
            {
                TimeFrame.Day2,
                toScale(120)
            },
            {
                TimeFrame.Day3,
                toScale(140)
            },
            {
                TimeFrame.Weekly,
                toScale(400)
            },
            {
                TimeFrame.Monthly,
                toScale(1000)
            }
        };

        private static double toScale(int minutes)
        {
            return 1 + 0.2 * (minutes-1);
        }

        public static double getScale(TimeFrame timeframe)
        {
            if(scale.ContainsKey(timeframe))
                return scale[timeframe];
            return scale[TimeFrame.Minute];
        }
    }
}