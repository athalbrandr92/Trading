//+------------------------------------------------------------------+
//| SYSTEM: Calculus_MA_Test_Orchestrator                            |
//| [INTENT]: Central command center.                                |
//+------------------------------------------------------------------+
#property strict

#include <Trade\Trade.mqh>
#include <CalculusLib\CalculusStrategy.mqh>
#include <CalculusLib\RiskManager.mqh>
#include <CalculusLib\ExecutionEngine.mqh>
#include <CalculusLib\FitnessEvaluator.mqh>

// [ELI5: These are the knobs on the dashboard.]
input group "--- Money Management ---"
input bool      InpUseDynamicRisk    = true;    
input double    InpRiskPercent       = 1.0;     
input double    InpFixedLotSize      = 0.1;     
input ulong     InpMagicNumber       = 20260610; 

// [ELI5: The "Sensitivity" dials - Granular control restored.]
input group "--- Weighted Decision Thresholds ---"
input double InpDecisionThreshold = 1.5; // Trigger trade if weighted score > 1.5
input double InpVelThreshold      = 1.5;
input double InpVelMult           = 1.5;
input double InpAccThreshold      = 1.5;
input double InpAccMult           = 1.5;
input double InpJerkThreshold     = 1.5;
input double InpJerkMult          = 1.5;
input double InpSnapThreshold     = 1.5;
input double InpSnapMult          = 1.5;
input double InpAssymetryMult     = 1.15;

input group "--- Importance Weights (0.0 to 1.0) ---"
input double W_Vel  = 1.0;
input double W_Acc  = 0.5;
input double W_Jerk = 0.8;
input double W_Snap = 0.2;    

// [ELI5: The "Guardrails."]
input group "--- Prop Firm Guardrails ---"
input double    InpMaxDailyDDPercent = 5.0;
input double    InpMaxOverallDDPercent = 10.0;

// [ELI5: We need these settings to make the fitness results really mean something.]
input group "--- Fitness Settings ---"
input double    InpMinTradesPerWeek = 2.0;
input int       InpMinTotalTrades   = 75;

// [ELI5: Our specialists.]
CRiskManager* g_risk;
CExecutionEngine* g_execution;
CCalculusStrategy* g_strategy;
CFitnessEvaluator* g_evaluator;

double g_startingBalance;
datetime g_globalStartTime;
bool g_breachDetected = false; // Add this global flag
FitnessParams GetFitnessSettings() {
    FitnessParams p;
    p.maxTotalDD = InpMaxOverallDDPercent / 100;
    p.minTradesPerWeek = InpMinTradesPerWeek;
    p.minTrades = InpMinTotalTrades;
    p.magicNumber = InpMagicNumber;
    p.globalStartTime = g_globalStartTime; // Track from OnInit
    return p;
}

int OnInit()
{   
    g_risk      = new CRiskManager(InpUseDynamicRisk, InpRiskPercent, InpFixedLotSize, InpMaxDailyDDPercent, InpMaxOverallDDPercent);
    g_execution = new CExecutionEngine(InpMagicNumber);
    
    // 1. Initialize with 0.0 as the dummy placeholder
    g_strategy  = new CCalculusStrategy(NULL, 0.0); 
    
    // 2. Load the settings into the memory bank
    g_strategy.SetStrategyParams(
    InpVelThreshold, InpVelMult, 
    InpAccThreshold, InpAccMult, 
    InpJerkThreshold, InpJerkMult, 
    InpSnapThreshold, InpSnapMult, 
    InpAssymetryMult, 
    W_Vel, W_Acc, W_Jerk, W_Snap, 
    InpDecisionThreshold
);
    
    g_evaluator = new CFitnessEvaluator();
    g_startingBalance = AccountInfoDouble(ACCOUNT_BALANCE);
    g_globalStartTime = TimeCurrent();
    return(INIT_SUCCEEDED);
}

void OnTick() 
{
    // [ELI5: Only work on a fresh tick. No point in making a decision 
    // on old news.]
    if(!IsNewBar()) return;
    
    // [ELI5: Check with the risk specialist. Are we still allowed to trade?]
    if(!g_execution.CheckRiskConstraints(0.0)) {
        Print("CRITICAL: Breach detected. System Halted.");
        return;
    }
    
    // Inside OnTick()
    if(g_risk.IsBreached()) {
      g_breachDetected = true; // Set the flag and keep it set
      Print("CRITICAL: Breach detected. System Halted.");
      return;
      }

    // [ELI5: Ask the strategy for a decision.]
    string signal = g_strategy.Analyze(MarketData());

    // [ELI5: Instead of doing the work ourselves, we hand the specific 
    // instructions to the Dispatcher (SendOrder). We provide the symbol,
    // the direction, and the volume we calculated.]
    // 2. Execution Logic
    if(signal == "BUY" || signal == "SELL") {
        
        // BRIDGE: Pulling physics data through the Strategy's interface
        double v = g_strategy.GetVelocity();
        double a = g_strategy.GetAcceleration();
        double j = g_strategy.GetJerk();
        
        // HANDOFF: Calling the CORRECT function name defined in RiskManager
        // We pass the metrics into the Accountant to get the lot size.
        double lotSize = g_risk.CalculateLotSize(v, a, j);

        // EXECUTION: Dispatching the order
        ENUM_ORDER_TYPE type = (signal == "BUY") ? ORDER_TYPE_BUY : ORDER_TYPE_SELL;
        g_execution.SendOrder(_Symbol, type, lotSize);
    }
}

bool IsNewBar()
{
    static datetime lastBarTime = 0;
    datetime currentBarTime = (datetime)SeriesInfoInteger(_Symbol, _Period, SERIES_LASTBAR_DATE);
    if(currentBarTime != lastBarTime)
    {
        lastBarTime = currentBarTime;
        return true;
    }
    return false;
}

double OnTester()
{
   FitnessParams p = GetFitnessSettings();
   return g_evaluator.Calculate(g_startingBalance, g_breachDetected, p);
}

void OnDeinit(const int reason)
{
    delete g_risk;
    delete g_execution;
    delete g_strategy;
    delete g_evaluator;
}