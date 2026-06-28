//+------------------------------------------------------------------+
//| SYSTEM SPECIFICATION: CalculusStrategy                           |
//|                                                                  |
//| [SYSTEM INTENT]:                                                 |
//|    To trigger trade decisions by treating market price movements |
//|    like physical forces. We don't just look for "green candles"; |
//|    we look for actual momentum.                                  |
//|                                                                  |
//| [CONTRACT]:                                                      |
//|    INPUT:  Market Data (Value, Volume).                          |
//|    OUTPUT: Signal (BUY, SELL, or HOLD).                          |
//|                                                                  |
//| [ALGORITHM NARRATIVE]:                                           |
//|    1. Ask the indicator for its current output.                  |
//|    2. Put that value into our memory engine.                     |
//|    3. Ask the memory engine for the "Total Force".               |
//|    4. If the Force is strong enough, we signal a trade.          |
//|    5. If the Force is weak or messy, we stay safe ("HOLD")       |
//+------------------------------------------------------------------+
#ifndef CALC_CALCULUS_STRATEGY // "Have we read this file before?"
#define CALC_CALCULUS_STRATEGY // "No? Okay, let's name it."

// We are pulling in our tools:
#include "IStrategy.mqh"        // The rulebook.
#include "FlowEngine.mqh"       // Our memory bank.
#include "GenericIndicator.mqh" // Our eyes/sensors.
#include "MessageBus.mqh"       // The chat system.

// This class executes a calculus-based strategy.
// It is declared for public use as an implementation of a strategy.
class CCalculusStrategy : public IStrategy { 
private: 
   // These are our private tools. They don't like sharing, 
   // but only because it's a safety guard for the internal state.
   // These are the members of this class. They don't interact with members
   // of other classes during class because that would get too confusing.
   CGenericIndicator* m_indicator; // Sticky note pointing to the indicator.
   CFlowEngine        m_engine;    // The memory engine.
   double             m_energy_threshold; // Our "minimum force" setting.
   CMessageBus        m_bus;       // The chat tool.
   double m_velThresh, m_accThresh, m_jerkThresh, m_jerkMult, m_snapThresh, 
   m_asymmetryMult, m_wVel, m_wAcc, m_wJerk, m_wSnap, m_decisionThreshold, m_velMult, m_accMult, m_snapMult; // All thresholds and multipliers needed for the strategy

public:
   // Constructor: The Setup.
   // The colon (:) is the Initializer List. We pack the backpack 
   // before the class starts.
   CCalculusStrategy(CGenericIndicator* indicator, double energy_threshold) 
      : m_indicator(indicator), m_engine(6), m_energy_threshold(energy_threshold) {}
      
   // Update SetStrategyParams
   void SetStrategyParams(double vT, double vM, double aT, double aM, double jT, double jM, double sT, double sM, double asym,
                       double wV, double wA, double wJ, double wS, double dThresh) {
      m_velThresh = vT; m_velMult = vM;
      m_accThresh = aT; m_accMult = aM;
      m_jerkThresh = jT; m_jerkMult = jM;
      m_snapThresh = sT; m_snapMult = sM;
      m_asymmetryMult = asym;
      m_wVel = wV; m_wAcc = wA;
      m_wJerk = wJ; m_wSnap = wS;
      m_decisionThreshold = dThresh;
   }

   // "override": We are keeping the family promise to have an Analyze function,
   // but implementing it our own way.
   string Analyze(MarketData &md) override {
    double val = m_indicator.GetValue(md);
    m_engine.Update(val);
    CFlowEngine::FlowState m_lastState = m_engine.GetFlowState((double)md.volume);
    
    if(!m_engine.IsReady()) return "HOLD";

    // 1. Observe (The Physics Observer)
    // We define the "Acceptable State" corridor for both Buy and Sell.
    
    // BUY: Needs to be within the corridor (Floor > X > Ceiling)
    double v_b = (m_lastState.Velocity > m_velThresh && m_lastState.Velocity < m_velThresh * m_velMult) ? 1.0 : 0.0;
    double a_b = (m_lastState.Acceleration > m_accThresh && m_lastState.Acceleration < m_accThresh * m_accMult) ? 1.0 : 0.0;
    double j_b = (m_lastState.Jerk > -m_jerkThresh && m_lastState.Jerk < m_jerkThresh * m_jerkMult) ? 1.0 : 0.0;
    double s_b = (m_lastState.Snap > m_snapThresh && m_lastState.Snap < m_snapThresh * m_snapMult) ? 1.0 : 0.0;

    // SELL: Needs to be within the asymmetric corridor
    // We apply m_asymmetryMult to pull the thresholds into the "Harder to trade" zone
    double v_s = (m_lastState.Velocity < -(m_velThresh * m_asymmetryMult) && 
                  m_lastState.Velocity > -(m_velThresh * m_velMult * m_asymmetryMult)) ? 1.0 : 0.0;
                  
    double a_s = (m_lastState.Acceleration < -(m_accThresh * m_asymmetryMult) && 
                  m_lastState.Acceleration > -(m_accThresh * m_accMult * m_asymmetryMult)) ? 1.0 : 0.0;
                  
    double j_s = (m_lastState.Jerk < (m_jerkThresh * m_asymmetryMult) && 
                  m_lastState.Jerk > -(m_jerkThresh * m_jerkMult * m_asymmetryMult)) ? 1.0 : 0.0;
                  
    double s_s = (m_lastState.Snap < -(m_snapThresh * m_asymmetryMult) && 
                  m_lastState.Snap > -(m_snapThresh * m_snapMult * m_asymmetryMult)) ? 1.0 : 0.0;

    // 2. Adapt (The Compounding Engine)
    // This rewards confluence. If all 4 "Forces" align in the corridor, the score explodes.
    double buyScore = (1.0 + v_b*m_wVel) * (1.0 + a_b*m_wAcc) * (1.0 + j_b*m_wJerk) * (1.0 + s_b*m_wSnap);
    double sellScore = (1.0 + v_s*m_wVel) * (1.0 + a_s*m_wAcc) * (1.0 + j_s*m_wJerk) * (1.0 + s_s*m_wSnap);

    // 3. Overcome (The Decision)
    if(buyScore > m_decisionThreshold) return "BUY";
    if(sellScore > m_decisionThreshold) return "SELL";
    
    return "HOLD";
   }
   
   double GetVelocity()     { return m_engine.GetFlowState(0).Velocity; }
   double GetAcceleration() { return m_engine.GetFlowState(0).Acceleration; }
   double GetJerk()         { return m_engine.GetFlowState(0).Jerk; }
};
// End of file. If you try to call it again, the `#ifndef` at the top 
// will just skip this whole thing.
#endif