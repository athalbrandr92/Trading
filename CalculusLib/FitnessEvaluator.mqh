//+----------------------------------------------------------+
//|  CFitnessEvaluator.mqh                                   |
//+----------------------------------------------------------+
#property strict

struct FitnessParams {
   double maxTotalDD;
   double minTradesPerWeek;
   int    minTrades;
   long   magicNumber;
   datetime globalStartTime;
};

class CFitnessEvaluator {
public:
   static double Calculate(double startingBalance, bool dailyBreachDetected, FitnessParams &p) {
      long totalTrades = TesterStatistics(STAT_TRADES); 
      double netProfit = TesterStatistics(STAT_PROFIT); 
      
      // Early exit if metrics are failing
      if(netProfit <= 0) return -TesterStatistics(STAT_EQUITY_DDREL_PERCENT); 
      if(totalTrades < p.minTrades) return 0.0;
      
      netProfit = netProfit / startingBalance; 

      datetime endTime = TimeCurrent(); 
      double totalDays = (double)(endTime - p.globalStartTime) / 86400.0; 
      if(totalDays <= 0.5) totalDays = 1.0;  
      double weeks = totalDays / 5.0;  

      if(totalTrades < (weeks * p.minTradesPerWeek)) return 0.0; 

      if(dailyBreachDetected) return -TesterStatistics(STAT_EQUITY_DDREL_PERCENT); 

      double maxEquityDDPercent = TesterStatistics(STAT_EQUITY_DDREL_PERCENT); 
      if(maxEquityDDPercent > p.maxTotalDD) return -TesterStatistics(STAT_EQUITY_DDREL_PERCENT);  

      if(maxEquityDDPercent <= 0.0) maxEquityDDPercent = 0.05; 
      double stabilizedFitness = netProfit / maxEquityDDPercent;
      
      if(stabilizedFitness <= 0) return stabilizedFitness; 

      // --- R2 / Regression Logic ---
      string symName = _Symbol;
      StringToUpper(symName);
      bool isCrypto = (StringFind(symName, "BTC") >= 0 || StringFind(symName, "ETH") >= 0 || StringFind(symName, "XBT") >= 0);
      
      double r2 = 0.0;
      int tradeIndex = 0;
      double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0, cum = 0;
      
      if(HistorySelect(0, TimeCurrent())) {
         int totalHistory = HistoryDealsTotal();
         for(int i = 0; i < totalHistory; i++) {
            ulong ticket = HistoryDealGetTicket(i);
            if(ticket > 0) {
               long magicNum = HistoryDealGetInteger(ticket, DEAL_MAGIC);
               long entryType = HistoryDealGetInteger(ticket, DEAL_ENTRY);
               
               if(magicNum == p.magicNumber && entryType == DEAL_ENTRY_OUT) {
                  double dealProfit = HistoryDealGetDouble(ticket, DEAL_PROFIT) + 
                                     HistoryDealGetDouble(ticket, DEAL_COMMISSION) + 
                                     HistoryDealGetDouble(ticket, DEAL_SWAP);
                  cum += dealProfit;
                  sumX += tradeIndex;
                  sumY += cum;
                  sumXY += (double)tradeIndex * cum;
                  sumX2 += (double)tradeIndex * tradeIndex;
                  sumY2 += cum * cum;
                  tradeIndex++;
                  
                  if(tradeIndex > 1) {
                     double denominator = MathSqrt(((tradeIndex * sumX2) - (sumX * sumX)) * ((tradeIndex * sumY2) - (sumY * sumY)));
                     r2 = (denominator == 0) ? 0 : MathPow(((tradeIndex * sumXY) - (sumX * sumY)) / denominator, 2.0);
                  }
               }
            }
         }
      }

      double daysPerWeek = isCrypto ? 7.0 : 5.0;
      double tradesPerWeek = (double)totalTrades / weeks;
      double frequencyPenalty = MathPow(MathMin(1.0, tradesPerWeek / 2.0), 4.0);

      double profitFactor = TesterStatistics(STAT_PROFIT_FACTOR);
      long winningTrades = TesterStatistics(STAT_PROFIT_TRADES);
      double ddDivider = MathMax(1.0, TesterStatistics(STAT_EQUITY_DD));
      double winRatio = (double)winningTrades / (double)totalTrades;
      double logTrades = MathLog10((double)totalTrades);

      double baseScore = (((profitFactor * (netProfit / ddDivider) * winRatio * r2) * logTrades) / maxEquityDDPercent) * r2;
      return stabilizedFitness * (baseScore * frequencyPenalty);
   }
};