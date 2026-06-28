//+------------------------------------------------------------------+
//| SYSTEM: TelemetryNode.mqh                                        |
//| ELI5: The "Black Box" node. It records the system pulse.         |
//|       Every action must be stamped here for auditing.            |
//+------------------------------------------------------------------+
#ifndef CALC_TELEMETRY_NODE
#define CALC_TELEMETRY_NODE

class CTelemetryNode {
public:
   // ELI5: When an action happens, we push it to the logs.
   // This creates the permanent paper trail for the Audit Control.
   void RecordEvent(string module, string action) {
      Print("AUDIT_TRAIL: [", module, "] performed: ", action);
   }
};
#endif