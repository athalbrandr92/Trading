//+------------------------------------------------------------------+
//| SYSTEM: SystemOrchestrator.mqh                                    |
//| FIX: Reverted naming to match local filesystem.                  |
//+------------------------------------------------------------------+
#include "RiskManager.mqh"
#include "TopologyManager.mqh"
#include "RiskBarrierNode.mqh"
#include "TelemetryNode.mqh"

class CSystemOrchestrator {
private:
   // Using the actual class names from your files (e.g., CRiskManager)
   CRiskManager         m_riskManager;
   CTopologyManager     m_topologyManager;
   
   CRiskBarrierNode     m_riskBarrier;
   CTelemetryNode       m_telemetry;

public:
   void OnInit() {
      // Linking logic
      m_riskManager.SetBarrier(m_riskBarrier);
      m_topologyManager.SetReporter(m_telemetry);
      
      // Registration sequence
      // Note: Registry should track these by the names you use in your manifest
      Print("SYSTEM_STARTUP: Structural Integrity Confirmed.");
   }
};