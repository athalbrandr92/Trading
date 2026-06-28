//+-----------------------------------------------------------+
//|  IStrategy.mqh                                            |
//+-----------------------------------------------------------+
#property strict
#include "DataManager.mqh"

interface IStrategy {
   // Every strategy must implement its own analysis logic
   virtual string Analyze(MarketData &md) = 0;
};