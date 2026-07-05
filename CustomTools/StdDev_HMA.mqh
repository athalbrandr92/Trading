//+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
//|  StdDev_HMA.mqh                                                                                                                                                                                      //|
//|  Daniel Lyons (strategy/conceptual), Google's Gemini (refinement/code)                                                                                                                               //|
//|  [https://github.com/athalbrandr92/Trading]                                                                                                                                                          //|
//+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
#ifndef STDDEV_HMA_MQH
#define STDDEV_HMA_MQH
#property copyright "Daniel Lyons / Google Gemini"
#property link      "[https://github.com/athalbrandr92/Trading]"
#property strict

class CStdDev_Module {
private:
    int m_lookback;
public:
    CStdDev_Module(int lookback) : m_lookback(lookback) {}

    // Computes the standard deviation across the lookback window[cite: 7].
    void Calculate(const double &src[], double &dst) {
        int size = ArraySize(src);
        if (size < m_lookback) { dst = 0.0; return; }
        double sum = 0, sumSq = 0;
        for(int j = 0; j < m_lookback; j++) sum += src[j];
        double mean = sum / m_lookback;
        for(int j = 0; j < m_lookback; j++) sumSq += MathPow(src[j] - mean, 2);
        dst = MathSqrt(sumSq / m_lookback);
    }
};
#endif