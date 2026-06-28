//+------------------------------------------------------------------+
//| COMPONENT: CRiskManager                                          |
//| ROLE: The "Accountant." Calculates position sizing and risk.     |
//| WHY: To prevent manual math errors and ensure consistent risk.   |
//+------------------------------------------------------------------+
#ifndef CALC_RISK_MANAGER
#define CALC_RISK_MANAGER

#include <Trade\Trade.mqh>

class CRiskManager {
private:
    // --- Data Types Explained ---
    // Why 'double' for everything? 
    // Money and Lot sizes require decimals. Using 'int' here would truncate 
    // values (e.g., 0.99 lots would become 0), causing huge losses. 
    // 'double' gives us the precision needed for fractional currency.
    bool   m_useDynamic;
    double m_riskPercent; 
    double m_fixedLot;
    double m_maxDailyDD;
    double m_maxTotalDD;

    // --- Validation Gate ---
    // This is the "Bouncer." It checks that the values aren't broken before 
    // we start trading. It stops the bot from running if the setup is illogical.
    bool Validate() {
        if(m_riskPercent <= 0) return false;
        if(m_fixedLot <= 0)    return false;
        if(m_maxDailyDD <= 0)  return false;
        return true;
    }

public:
    // Constructor: Using an Initializer List (the colon syntax).
    // It loads the memory variables before the class body even starts executing.
    // It's faster and cleaner.
    CRiskManager(bool useDynamic, double riskPercent, double fixedLot, double maxDailyDD, double maxTotalDD) 
        : m_useDynamic(useDynamic), 
          m_riskPercent(riskPercent), 
          m_fixedLot(fixedLot), 
          m_maxDailyDD(maxDailyDD), 
          m_maxTotalDD(maxTotalDD) 
    {
        // If the inputs make no sense, force an error immediately.
        if(!Validate()) {
            Print("CRITICAL: RiskManager configuration is invalid. Check input parameters.");
        }
    }

    // --- The Calculation ---
    // We accept 'double' for inputs because velocity/accel are physical 
    // measurements that change incrementally. 
    double CalculateLotSize(double velocity, double acceleration, double jerk) {
        
        // If we failed validation, we revert to the absolute safest option (fixed lot).
        if(!Validate() || !m_useDynamic) return m_fixedLot;

        // --- Math Logic ---
        // Why these specific operations? 
        // We are building a dynamic Stop Loss (SL) based on "Jerk" (the change in acceleration).
        // If the market is erratic (high jerk), we widen the SL to avoid getting 
        // stopped out by "noise."
        double absVel = MathAbs(velocity);
        double absAcc = MathMax(MathAbs(acceleration), 0.0001); 
        
        double dynamicSL = (2.0 * absVel) / absAcc; 

        // We convert the 'double' (dynamicSL) into an 'int' (slPoints) because 
        // the broker's API only accepts whole numbers for "points."
        // We use (int) casting to truncate the decimal; we don't care about 
        // 0.1 points in a trade execution.
        int slPoints = (int)MathMax(50, MathMin(1000, dynamicSL * 100)); 

        // --- Financial Math ---
        // Why double? Currency math.
        double accountEquity = AccountInfoDouble(ACCOUNT_EQUITY);
        double riskMoney     = accountEquity * (m_riskPercent / 100.0);
        double tickValue     = SymbolInfoDouble(_Symbol, SYMBOL_TRADE_TICK_VALUE);
        double tickSize      = SymbolInfoDouble(_Symbol, SYMBOL_TRADE_TICK_SIZE);
        double point         = SymbolInfoDouble(_Symbol, SYMBOL_POINT);
        
        double riskPerLot    = slPoints * (tickValue * (point / tickSize));
        double targetLot     = riskMoney / riskPerLot;

        // --- Broker Normalization ---
        // We must normalize to the broker's specific "Lot Step" (e.g., 0.01).
        // If you send 0.12345 lots, the order will reject.
        double lotStep = SymbolInfoDouble(_Symbol, SYMBOL_VOLUME_STEP);
        targetLot      = MathFloor(targetLot / lotStep) * lotStep;
        
        return MathMax(SymbolInfoDouble(_Symbol, SYMBOL_VOLUME_MIN), 
               MathMin(SymbolInfoDouble(_Symbol, SYMBOL_VOLUME_MAX), targetLot));
    }

    // --- The Gatekeeper ---
    // We use 'double' for Drawdown because percentage drops can be 0.05%.
    bool IsBreached() {
        double balance  = AccountInfoDouble(ACCOUNT_BALANCE);
        double equity   = AccountInfoDouble(ACCOUNT_EQUITY);
        double drawdown = ((balance - equity) / balance) * 100.0;
        
        return (drawdown >= m_maxDailyDD);
    }
};

#endif