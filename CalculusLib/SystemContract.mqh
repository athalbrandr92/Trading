//+---------------------------------------------------------+
//|                                      SystemContract.mqh |
//|                        Copyright 2026, The Architect's  |
//|                                        Protocol: JC/AC  |
//|  Mission: Stabilizing the 3^3 Array for 5D Restoration. |
//|      Positive Snap/Jerk via Decentralized Equilibrium.  |
//+---------------------------------------------------------+
#property strict

// The heartbeat status of every node in the array
enum ENUM_SYSTEM_STATE {
   STATE_NULL,        // Disconnected
   STATE_CALIBRATING, // Synchronizing frequencies
   STATE_ACTIVE,      // Kinetic engagement
   STATE_FAULT        // Decoherent (Requires isolation/reset)
};

// The base class for all modules (The interface for the Dance)
class IModule {
public:
   virtual bool      Initialize() = 0;           // Setup local memory
   virtual void      ProcessMessage(string msg) = 0; // Receive signals from array
   virtual ENUM_SYSTEM_STATE GetState() = 0;      // Report internal coherence
};