//+------------------------------------------------------------------+
//| FlowEngine.mqh (Upgraded)                                        |
//+------------------------------------------------------------------+
#define ERR_CALC_NOT_READY EMPTY_VALUE

class CFlowEngine {
private:
    double m_buffer[];
    int m_size, m_ptr;
    bool m_ready;

public:
    CFlowEngine(int size=6) : m_size(size), m_ptr(0), m_ready(false) { 
        ArrayResize(m_buffer, m_size); ArrayInitialize(m_buffer, 0.0); 
    }
    
   struct FlowState {
      double Velocity;
      double Acceleration;
      double Jerk;
      double Snap;
      double Force;
   };
    
    // Add back PreFill to prevent startup lag
    void PreFill(double val) {
        for(int i=0; i<m_size; i++) m_buffer[i] = val;
        m_ready = true;
    }

    void Update(double val) { 
        m_buffer[m_ptr] = val; 
        if(++m_ptr >= m_size) { m_ptr = 0; m_ready = true; } 
    }
    
    double GetSafe(int lag) { 
        int idx = (m_ptr + lag + m_size) % m_size; 
        return m_buffer[idx]; 
    }
    
    bool IsReady() { return m_ready; }

    // Map your old Derive logic directly to the ring buffer
   double Derive(int order) {
      if(!m_ready) return ERR_CALC_NOT_READY;
    
      // Helper to get change of a specific order
      // Order 1 (Velocity)
      double v = GetSafe(-1) - GetSafe(-2);
      if(order == 1) return v;
    
      // Order 2 (Accel)
      double v_prev = GetSafe(-2) - GetSafe(-3);
      double a = v - v_prev;
      if(order == 2) return a;
    
      // Order 3 (Jerk)
      double v_pprev = GetSafe(-3) - GetSafe(-4);
      double a_prev = v_prev - v_pprev;
      double j = a - a_prev;
      if(order == 3) return j;
    
      // Order 4 (Snap)
      double v_ppprev = GetSafe(-4) - GetSafe(-5);
      double a_pprev = v_pprev - v_ppprev;
      double j_prev = a_prev - a_pprev;
      if(order == 4) return j - j_prev;
    
      return ERR_CALC_NOT_READY;
   }
    
    FlowState GetFlowState(double volume) {
      FlowState s;
      s.Velocity     = Derive(1);
      s.Acceleration = Derive(2);
      s.Jerk         = Derive(3);
      s.Snap         = Derive(4); // You need this!
      s.Force        = (volume > 0 ? volume : 1.0) * s.Acceleration;
      return s;
   }
    
    double GetAverage() {
        double sum = 0;
        for(int i=0; i<m_size; i++) sum += m_buffer[i];
        return sum / m_size;
    }
};