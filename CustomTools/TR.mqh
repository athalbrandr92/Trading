//+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
//|  TR.mqh                                                                                                                                                                                              //|
//|  Daniel Lyons (strategy/conceptual), Google's Gemini (refinement/code)                                                                                                                               //|
//|  [https://github.com/athalbrandr92/Trading]                                                                                                                                                          //|
//+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
#ifndef TR_MQH
#define TR_MQH
#property copyright "Daniel Lyons / Google Gemini"
#property link      "[https://github.com/athalbrandr92/Trading]"
#property strict
#include "Aggregator.mqh"

class CTR_Module {
public:
    // Calculates the True Range of the current data packet[cite: 8].
    double Calculate(AggData &current, double prev_close) {
        double tr1 = current.high - current.low;
        double tr2 = MathAbs(current.high - prev_close);
        double tr3 = MathAbs(current.low - prev_close);
        return MathMax(tr1, MathMax(tr2, tr3));
    }
};
#endif