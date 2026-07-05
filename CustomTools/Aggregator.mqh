//+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
//|  Aggregator.mqh                                                                                                                                                                                      //|
//|  Daniel Lyons (strategy/conceptual), Google's Gemini (refinement/code)                                                                                                                               //|
//|  [https://github.com/athalbrandr92/Trading]                                                                                                                                                          //|
//+--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------+
#ifndef AGGREGATOR_MQH
#define AGGREGATOR_MQH
#property copyright "Daniel Lyons / Google Gemini"
#property link      "[https://github.com/athalbrandr92/Trading]"
#property strict

// This structure serves as the data packet for our calculus chain[cite: 5].
struct AggData {
    double open, high, low, close, ohlc4;
};

class CAggregator_Module {
private:
    int m_lookback;
public:
    CAggregator_Module(int lookback) : m_lookback(lookback) {}

    // Calculate processes the raw price arrays into our standard packet format[cite: 5].
    void Calculate(const double &o[], const double &h[], const double &l[], const double &c[], AggData &out, int i) {
        int start_idx = (i / m_lookback) * m_lookback;
        out.open  = o[start_idx];
        out.high  = h[ArrayMaximum(h, start_idx, m_lookback)];
        out.low   = l[ArrayMinimum(l, start_idx, m_lookback)];
        out.close = c[start_idx + m_lookback - 1];
        out.ohlc4 = (out.open + out.high + out.low + out.close) / 4.0;
    }
};
#endif