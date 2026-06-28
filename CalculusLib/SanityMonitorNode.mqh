//+-----------------------------------------------------------+
//| SYSTEM: SanityMonitorNode                                 |
//| ELI5: The Immune System. It catches "germs" (bad data)    |
//|       before they make the robot sick.                    |
//+-----------------------------------------------------------+
#ifndef CALC_SANITY_MONITOR
#define CALC_SANITY_MONITOR

class CSanityMonitorNode {
public:
   // ELI5: If the price jumps too high or low too fast, 
   // this says "stop, that's impossible!"
   bool IsDataSane(double currentPrice, double previousPrice) {
      double diff = MathAbs(currentPrice - previousPrice);
      if(diff > 100.0) { // Arbitrary sanity threshold
         Print("SANITY: Garbage data detected! Blocking.");
         return false;
      }
      return true;
   }
};
#endif