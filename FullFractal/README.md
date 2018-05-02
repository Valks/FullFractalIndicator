Display fractals in your chart:

Shows arrows indicating if the fractal is a higher high, higher low, lower high or lower low
Show lines connecting highs
Show lines connecting lows
Show lines connecting highs and lows
Play notification sounds
Send email notifications
 

To add notification sounds, set in the configuration the path to the sound file, i.e.:  C:\Windows\Media\notification.wav

To receive email notifications you need to configure your email notification settings in cTrader (see https://help.spotware.com/calgo/cbots/email-notifications)

 

Do you want to contribute? Send a pull request on https://gitlab.com/douglascvas/FullFractal

----------------------

v 1.4:

Added notification sounds
Added email notification
----------------------

v 1.3:

Fixed bug that would mark every new fractal as bad fractal
----------------------

v 1.2:

Added support for choosing line colors
Added circles to highlight fractals (with option to turn off)
Changed symbol used to highlight bad fractals
Due to technical limitations on the cTrader platform, it is not possible to choose which color to apply to the circles and arrows.



 ![alt text](https://ctdn.com/guides/images/0d333f81879ac21e9c275a21115116fe0316f263.png "FullFractal")



// **************************************************

Tips for developers: 

You can add FullFractal indicator as reference to your project (Robot or Indicator)  by clicking in "Manage References" in cAlgo, and use the FractalService class to have the fractals being processed in your application:

```
FractalService fractalService = new FractalService(MarketSeries, period);
fractalService.onFractal(handler);
```
where handler is a method with following signature:

```
private void handler(FractalEvent fractalEvent)
```
The handler will be called whenever you have a new fractal.

In order to process the bars, you need to call 

```
fractalService.processIndex(index)
```
at each new bar. Example:

```
public override void Calculate(int index)
{
    // index - 1 because index is not yet closed
    fractalService.processIndex(index - 1);
}
```