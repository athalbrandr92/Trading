//+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
//|  HMA_Module.mqh                                                                                                                                                                                      //|
//|  Daniel Lyons (strategy/conceptual), Google's Gemini (refinement/code)                                                                                                                               //|
//|  [https://github.com/athalbrandr92/Trading]                                                                                                                                                          //|
//+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
#ifndef HMA_MODULE_MQH
#define HMA_MODULE_MQH
#property copyright "Daniel Lyons / Google Gemini"
#property link      "[https://github.com/athalbrandr92/Trading]"
#property strict

class CHMA_Module {
private:
    int m_period;
    double m_wma1[], m_wma2[], m_diff[];
public:
    CHMA_Module(int period) : m_period(period) {}

    // Main calculation for the Hull Moving Average[cite: 6].
    void Calculate(const double &src[], double &dst[]) {
        int size = ArraySize(src);
        if (size < m_period) return;
        ArrayResize(m_wma1, size); ArrayResize(m_wma2, size); ArrayResize(m_diff, size);
        ArraySetAsSeries(m_wma1, true); ArraySetAsSeries(m_wma2, true); ArraySetAsSeries(m_diff, true);
        GetWMA(src, m_wma1, (int)(m_period / 2));
        GetWMA(src, m_wma2, m_period);
        for(int i = 0; i < size - m_period; i++) m_diff[i] = 2 * m_wma1[i] - m_wma2[i];
        GetWMA(m_diff, dst, (int)MathSqrt(m_period));
    }
private:
    void GetWMA(const double &src[], double &dst[], int period) {
        int size = ArraySize(src);
        for(int i = 0; i < size - period; i++) {
            double sum = 0, weight = 0;
            for(int j = 0; j < period; j++) {
                sum += src[i + j] * (period - j);
                weight += (period - j);
            }
            dst[i] = sum / weight;
        }
    }
};
#endif