using System;
using System.Linq;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators; // Required to talk to DynamicSnRBoxes

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class WaveRider : Robot
    {
        // --- 1. CORE SETTINGS ---
        [Parameter("Risk (%)", DefaultValue = 0.5, Group = "1. Core")] 
        public double Risk { get; set; }

        [Parameter("Max Dollar Risk", DefaultValue = 25.0, Group = "1. Core")] 
        public double MaxDollarRisk { get; set; }

        [Parameter("Max Spread (Pips)", DefaultValue = 1.0, Group = "1. Core")] 
        public double MaxSpread { get; set; }

        [Parameter("Macro ATR Mult (Sync with Indicator)", DefaultValue = 0.5, Group = "1. Core")] 
        public double MacroAtrMult { get; set; }

        [Parameter("Micro ATR Mult (Sync with Indicator)", DefaultValue = 0.2, Group = "1. Core")] 
        public double MicroAtrMult { get; set; }

        // --- 2. MANAGEMENT SETTINGS ---
        public enum TpStrategy { PureReversal, NextZoneQuarter }
        [Parameter("TP Strategy", DefaultValue = TpStrategy.NextZoneQuarter, Group = "2. Management")] 
        public TpStrategy SelectedTpStrategy { get; set; }

        [Parameter("Enable Trailing Stops", DefaultValue = true, Group = "2. Management")] 
        public bool EnableTrailing { get; set; }

        [Parameter("Rejection Logic", DefaultValue = RejectionMode.WickInsideCloseOutside, Group = "2. Management")] 
        public RejectionMode SnRRejectionMode { get; set; }

        // --- 3. FITNESS ---
        [Parameter("Min Trades", DefaultValue = 75, Group = "3. Fitness")] public int MinTrades { get; set; }
        [Parameter("Linear Bonus?", DefaultValue = false, Group = "3. Fitness")] public bool LinBon { get; set; }
        [Parameter("Linear Divisor", DefaultValue = 3, Group = "3. Fitness")] public int LinDiv { get; set; }
        [Parameter("Hyperbolic Exponent", DefaultValue = 0.60, Group = "3. Fitness")] public double HypExp { get; set; }

        // --- 4. TREND & MOMENTUM FILTERS ---
        [Parameter("Enable EMA Filter", DefaultValue = true, Group = "4. Filters")] 
        public bool EnableEma { get; set; }
        [Parameter("EMA Period", DefaultValue = 50, Group = "4. Filters")] 
        public int EmaPeriod { get; set; }

        public enum AdxFilterMode { Off, Min, Max, MinMax }
        [Parameter("ADX Mode", DefaultValue = AdxFilterMode.MinMax, Group = "4. Filters")] 
        public AdxFilterMode AdxMode { get; set; }
        [Parameter("ADX Period", DefaultValue = 14, Group = "4. Filters")] 
        public int AdxPeriod { get; set; }
        [Parameter("ADX Min Level", DefaultValue = 20, Group = "4. Filters")] 
        public double AdxMin { get; set; }
        [Parameter("ADX Max Level", DefaultValue = 45, Group = "4. Filters")] 
        public double AdxMax { get; set; }

        [Parameter("Enable MACD Filter", DefaultValue = true, Group = "4. Filters")] 
        public bool EnableMacd { get; set; }
        [Parameter("MACD Long Cycle", DefaultValue = 26, Group = "4. Filters")] 
        public int MacdLongCycle { get; set; }
        [Parameter("MACD Short Cycle", DefaultValue = 12, Group = "4. Filters")] 
        public int MacdShortCycle { get; set; }
        [Parameter("MACD Signal Periods", DefaultValue = 9, Group = "4. Filters")] 
        public int MacdSignalPeriods { get; set; }

        [Parameter("Enable Holiday Blackout", DefaultValue = true, Group = "4. Filters")] 
        public bool EnableHolidayBlackout { get; set; }

        [Parameter("Blackout Start Month", DefaultValue = 12, MinValue = 1, MaxValue = 12, Group = "4. Filters")] 
        public int BlackoutStartMonth { get; set; }
        [Parameter("Blackout Start Day", DefaultValue = 20, MinValue = 1, MaxValue = 31, Group = "4. Filters")] 
        public int BlackoutStartDay { get; set; }

        [Parameter("Blackout End Month", DefaultValue = 1, MinValue = 1, MaxValue = 12, Group = "4. Filters")] 
        public int BlackoutEndMonth { get; set; }
        [Parameter("Blackout End Day", DefaultValue = 5, MinValue = 1, MaxValue = 31, Group = "4. Filters")] 
        public int BlackoutEndDay { get; set; }

        // --- 5. INDICATOR S&R PARAMETERS (Passed to dynamic initialization) ---
        [Parameter("Show Multiday", DefaultValue = true, Group = "5. Indicator Vis")] public bool IndShowMultiday { get; set; }
        [Parameter("Show Prev Day", DefaultValue = true, Group = "5. Indicator Vis")] public bool IndShowPreviousDay { get; set; }
        [Parameter("Show Asian", DefaultValue = true, Group = "5. Indicator Vis")] public bool IndShowAsianSession { get; set; }
        [Parameter("Show London", DefaultValue = true, Group = "5. Indicator Vis")] public bool IndShowLondonSession { get; set; }
        [Parameter("Show NY", DefaultValue = true, Group = "5. Indicator Vis")] public bool IndShowNySession { get; set; }
        [Parameter("Show Psych", DefaultValue = true, Group = "5. Indicator Vis")] public bool IndShowPsychLevels { get; set; }
        [Parameter("Show OBs", DefaultValue = true, Group = "5. Indicator Vis")] public bool IndShowOrderBlocks { get; set; }
        [Parameter("Show Doubles", DefaultValue = true, Group = "5. Indicator Vis")] public bool IndShowDoubleTopsBottoms { get; set; }
        [Parameter("Show Consolidation", DefaultValue = true, Group = "5. Indicator Vis")] public bool IndShowConsolidation { get; set; }

        [Parameter("Psych Step", DefaultValue = 25, Group = "5. Indicator Core")] public double IndPsychLevelStep { get; set; }
        [Parameter("UTC Offset", DefaultValue = -5, Group = "5. Indicator Core")] public int IndUtcOffset { get; set; }
        [Parameter("Multiday TF", DefaultValue = "Weekly", Group = "5. Indicator Core")] public TimeFrame IndMultidayTimeFrame { get; set; }
        [Parameter("Multiday Lookback", DefaultValue = 1, Group = "5. Indicator Core")] public int IndMultidayLookback { get; set; }

        [Parameter("Asian Start", DefaultValue = 18, Group = "5. Indicator Sessions")] public int IndAsianStartHour { get; set; }
        [Parameter("Asian End", DefaultValue = 3, Group = "5. Indicator Sessions")] public int IndAsianEndHour { get; set; }
        [Parameter("London Start", DefaultValue = 3, Group = "5. Indicator Sessions")] public int IndLondonStartHour { get; set; }
        [Parameter("London End", DefaultValue = 11, Group = "5. Indicator Sessions")] public int IndLondonEndHour { get; set; }
        [Parameter("NY Start", DefaultValue = 8, Group = "5. Indicator Sessions")] public int IndNyStartHour { get; set; }
        [Parameter("NY End", DefaultValue = 17, Group = "5. Indicator Sessions")] public int IndNyEndHour { get; set; }

        [Parameter("Min Formation", DefaultValue = 10, Group = "5. Indicator Patterns")] public int IndMinFormationCandles { get; set; }
        [Parameter("Max Lookback", DefaultValue = 50, Group = "5. Indicator Patterns")] public int IndMaxLookbackCandles { get; set; }
        [Parameter("Max Consolidation ATR", DefaultValue = 1.5, Group = "5. Indicator Patterns")] public double IndMaxConsolidationAtrWidth { get; set; }

        // Variables
        private DynamicSnRBoxes _snrIndicator;
        private AverageTrueRange _atr;
        private ExponentialMovingAverage _ema;
        private DirectionalMovementSystem _adx;
        private MacdCrossOver _macd;
        private Bars _m1Bars;
        
        private DateTime _lastBarTime = DateTime.MinValue;
        private double startingBalance;

        // Labels to track the three parallel trades
        private const string LBL_MINOR = "WaveRider_Minor";
        private const string LBL_MID = "WaveRider_Mid";
        private const string LBL_MAJOR = "WaveRider_Major";

        protected override void OnStart()
        {
            _atr = Indicators.AverageTrueRange(14, MovingAverageType.Simple);
            
            // Pass all 24 parameters in EXACT order to the indicator so the optimizer can control them
            _snrIndicator = Indicators.GetIndicator<DynamicSnRBoxes>(
                IndShowMultiday, IndShowPreviousDay, IndShowAsianSession, IndShowLondonSession, IndShowNySession, 
                IndShowPsychLevels, IndShowOrderBlocks, IndShowDoubleTopsBottoms, IndShowConsolidation, 
                MacroAtrMult, MicroAtrMult, IndPsychLevelStep, IndUtcOffset, IndMultidayTimeFrame, IndMultidayLookback, 
                IndAsianStartHour, IndAsianEndHour, IndLondonStartHour, IndLondonEndHour, IndNyStartHour, IndNyEndHour, 
                IndMinFormationCandles, IndMaxLookbackCandles, IndMaxConsolidationAtrWidth
            );
            
            _ema = Indicators.ExponentialMovingAverage(Bars.ClosePrices, EmaPeriod);
            _adx = Indicators.DirectionalMovementSystem(AdxPeriod);
            _macd = Indicators.MacdCrossOver(Bars.ClosePrices, MacdLongCycle, MacdShortCycle, MacdSignalPeriods);
            
            startingBalance = Account.Balance;

            _m1Bars = MarketData.GetBars(TimeFrame.Minute);
            _m1Bars.BarOpened += OnM1BarOpened;
        }

        private void OnM1BarOpened(BarOpenedEventArgs args)
        {
            if (EnableTrailing)
                HandleTrailingStops();
        }

        protected override void OnBar()
        {
            if (_snrIndicator != null) 
            {
                double wakeUpCall = _snrIndicator.DummySignal.LastValue;
            }

            if (Bars.OpenTimes.LastValue == _lastBarTime) return;
            _lastBarTime = Bars.OpenTimes.LastValue;

            if (_snrIndicator == null || Bars.Count < Math.Max(3, EmaPeriod)) return;

            var currBar = Bars.Last(1);
            var prevBar = Bars.Last(2);

            _snrIndicator.CheckRejection(TradeType.Buy, currBar.High, currBar.Low, currBar.Close, prevBar.Close, SnRRejectionMode, out bool rejMinorBuy, out bool rejMidBuy, out bool rejMajorBuy);
            _snrIndicator.CheckRejection(TradeType.Sell, currBar.High, currBar.Low, currBar.Close, prevBar.Close, SnRRejectionMode, out bool rejMinorSell, out bool rejMidSell, out bool rejMajorSell);

            CheckForBreakouts(currBar, prevBar, out bool brkMinorBuy, out bool brkMidBuy, out bool brkMajorBuy, out bool brkMinorSell, out bool brkMidSell, out bool brkMajorSell);

            if (rejMajorBuy || brkMajorBuy) 
                ExecuteWave(ZoneTier.Major, TradeType.Buy);
            else if (rejMajorSell || brkMajorSell) 
                ExecuteWave(ZoneTier.Major, TradeType.Sell);

            if (rejMidBuy || brkMidBuy) 
                ExecuteWave(ZoneTier.Mid, TradeType.Buy);
            else if (rejMidSell || brkMidSell) 
                ExecuteWave(ZoneTier.Mid, TradeType.Sell);

            if (rejMinorBuy || brkMinorBuy) 
                ExecuteWave(ZoneTier.Minor, TradeType.Buy);
            else if (rejMinorSell || brkMinorSell) 
                ExecuteWave(ZoneTier.Minor, TradeType.Sell);
        }

        private void ExecuteWave(ZoneTier triggerTier, TradeType direction)
        {
            if (triggerTier == ZoneTier.Major)
            {
                CloseOppositePositions(LBL_MAJOR, direction);
                CloseOppositePositions(LBL_MID, direction);
                CloseOppositePositions(LBL_MINOR, direction);

                OpenPositionIfNone(LBL_MAJOR, direction, ZoneTier.Major);
                OpenPositionIfNone(LBL_MID, direction, ZoneTier.Mid);
                OpenPositionIfNone(LBL_MINOR, direction, ZoneTier.Minor);
            }
            else if (triggerTier == ZoneTier.Mid)
            {
                CloseOppositePositions(LBL_MID, direction);
                CloseOppositePositions(LBL_MINOR, direction);

                OpenPositionIfNone(LBL_MID, direction, ZoneTier.Mid);
                OpenPositionIfNone(LBL_MINOR, direction, ZoneTier.Minor);
            }
            else if (triggerTier == ZoneTier.Minor)
            {
                CloseOppositePositions(LBL_MINOR, direction);
                OpenPositionIfNone(LBL_MINOR, direction, ZoneTier.Minor);
            }
        }

        private void OpenPositionIfNone(string label, TradeType direction, ZoneTier tier)
        {
            // --- 1. SPREAD & FILTER CHECKS ---
            if (EnableHolidayBlackout && IsHoliday(Server.Time)) return;
            if (Symbol.Spread / Symbol.PipSize > MaxSpread) return;

            if (AdxMode != AdxFilterMode.Off)
            {
                double adxVal = _adx.ADX.Last(1);
                if (AdxMode == AdxFilterMode.Min && adxVal < AdxMin) return;
                if (AdxMode == AdxFilterMode.Max && adxVal > AdxMax) return;
                if (AdxMode == AdxFilterMode.MinMax && (adxVal < AdxMin || adxVal > AdxMax)) return;
            }

            if (EnableEma)
            {
                if (direction == TradeType.Buy && Bars.ClosePrices.Last(1) < _ema.Result.Last(1)) return;
                if (direction == TradeType.Sell && Bars.ClosePrices.Last(1) > _ema.Result.Last(1)) return;
            }

            if (EnableMacd)
            {
                if (direction == TradeType.Buy && _macd.Histogram.Last(1) <= 0) return;
                if (direction == TradeType.Sell && _macd.Histogram.Last(1) >= 0) return;
            }

            // --- 2. EXECUTION ---
            var existing = Positions.FirstOrDefault(p => p.Label == label && p.SymbolName == SymbolName);
            if (existing != null) return; 

            double entryPrice = direction == TradeType.Buy ? Symbol.Ask : Symbol.Bid;
            
            double? sl = CalculateInitialStopLoss(direction, entryPrice, tier);
            if (!sl.HasValue) return; 

            double slDistance = Math.Abs(entryPrice - sl.Value);
            
            double dollarRisk = Account.Balance * (Risk / 100.0);
            if (dollarRisk > MaxDollarRisk) dollarRisk = MaxDollarRisk;
            
            double volume = LotCalcDollar(dollarRisk, slDistance);

            double? tp = null;
            if (SelectedTpStrategy == TpStrategy.NextZoneQuarter)
            {
                tp = CalculateQuarterZoneTakeProfit(direction, entryPrice, tier);
            }

            ExecuteMarketOrder(direction, SymbolName, volume, label, sl, tp);
        }

        private void CloseOppositePositions(string label, TradeType newDirection)
        {
            var positions = Positions.Where(p => p.Label == label && p.SymbolName == SymbolName && p.TradeType != newDirection).ToList();
            foreach (var pos in positions)
            {
                ClosePosition(pos);
            }
        }

        private bool IsHoliday(DateTime currentTime)
        {
            if (!EnableHolidayBlackout) return false;

            int m = currentTime.Month;
            int d = currentTime.Day;

            if (BlackoutStartMonth <= BlackoutEndMonth)
            {
                // Handles blackouts within the same year (e.g., July 1 to July 10)
                DateTime start = new DateTime(currentTime.Year, BlackoutStartMonth, BlackoutStartDay);
                DateTime end = new DateTime(currentTime.Year, BlackoutEndMonth, BlackoutEndDay);
                return currentTime.Date >= start.Date && currentTime.Date <= end.Date;
            }
            else
            {
                // Handles cross-year blackouts (e.g., Dec 20 to Jan 5)
                bool isAfterStart = (m > BlackoutStartMonth) || (m == BlackoutStartMonth && d >= BlackoutStartDay);
                bool isBeforeEnd = (m < BlackoutEndMonth) || (m == BlackoutEndMonth && d <= BlackoutEndDay);
                return isAfterStart || isBeforeEnd;
            }
        }

        // --- CALCULATION LOGIC ---

        private double LotCalcDollar(double riskAmount, double slDistance)
        {
            double mPS = (slDistance / Symbol.TickSize) * Symbol.TickValue * Symbol.VolumeInUnitsStep; 
            return Math.Round(Math.Clamp(Math.Floor(riskAmount / mPS) * Symbol.VolumeInUnitsStep, Symbol.VolumeInUnitsMin, Symbol.VolumeInUnitsStep * 1000), 6);
        }

        private double? CalculateInitialStopLoss(TradeType direction, double entryPrice, ZoneTier tier)
        {
            var activeZones = _snrIndicator.ActiveZonesDict.Values.Where(z => z.Tier == tier).ToList();
            if (!activeZones.Any()) return null;

            var currentZone = activeZones.OrderBy(z => Math.Abs(((z.Top + z.Bottom) / 2) - entryPrice)).First();
            double atrBuffer = _atr.Result.Last(1) * (tier == ZoneTier.Minor ? MicroAtrMult : MacroAtrMult);

            if (direction == TradeType.Buy)
                return currentZone.Bottom - (atrBuffer / 2); 
            else
                return currentZone.Top + (atrBuffer / 2); 
        }

        private double? CalculateQuarterZoneTakeProfit(TradeType direction, double entryPrice, ZoneTier tier)
        {
            var activeZones = _snrIndicator.ActiveZonesDict.Values.Where(z => z.Tier == tier).ToList();
            if (!activeZones.Any()) return null;

            Zone targetZone = null;

            if (direction == TradeType.Buy)
            {
                targetZone = activeZones.Where(z => z.Bottom > entryPrice).OrderBy(z => z.Bottom).FirstOrDefault();
                if (targetZone != null)
                {
                    double zoneHeight = targetZone.Top - targetZone.Bottom;
                    return targetZone.Bottom + (zoneHeight * 0.25);
                }
            }
            else
            {
                targetZone = activeZones.Where(z => z.Top < entryPrice).OrderByDescending(z => z.Top).FirstOrDefault();
                if (targetZone != null)
                {
                    double zoneHeight = targetZone.Top - targetZone.Bottom;
                    return targetZone.Top - (zoneHeight * 0.25);
                }
            }
            return null;
        }

        private void CheckForBreakouts(Bar currBar, Bar prevBar, out bool brkMinorBuy, out bool brkMidBuy, out bool brkMajorBuy, out bool brkMinorSell, out bool brkMidSell, out bool brkMajorSell)
        {
            brkMinorBuy = brkMidBuy = brkMajorBuy = brkMinorSell = brkMidSell = brkMajorSell = false;

            foreach (var zone in _snrIndicator.ActiveZonesDict.Values)
            {
                if (prevBar.Close <= zone.Top && currBar.Close > zone.Top)
                {
                    if (zone.Tier == ZoneTier.Minor) brkMinorBuy = true;
                    if (zone.Tier == ZoneTier.Mid) brkMidBuy = true;
                    if (zone.Tier == ZoneTier.Major) brkMajorBuy = true;
                }
                else if (prevBar.Close >= zone.Bottom && currBar.Close < zone.Bottom)
                {
                    if (zone.Tier == ZoneTier.Minor) brkMinorSell = true;
                    if (zone.Tier == ZoneTier.Mid) brkMidSell = true;
                    if (zone.Tier == ZoneTier.Major) brkMajorSell = true;
                }
            }
        }

        private void HandleTrailingStops()
        {
            foreach (var pos in Positions.Where(p => p.SymbolName == SymbolName))
            {
                double? newSl = null;
                var currentPrice = pos.TradeType == TradeType.Buy ? Symbol.Bid : Symbol.Ask;

                if (pos.Label == LBL_MINOR)
                    newSl = FindTrailingZoneSl(pos.TradeType, currentPrice, new[] { ZoneTier.Minor, ZoneTier.Mid, ZoneTier.Major });
                else if (pos.Label == LBL_MID)
                    newSl = FindTrailingZoneSl(pos.TradeType, currentPrice, new[] { ZoneTier.Mid, ZoneTier.Major });
                else if (pos.Label == LBL_MAJOR)
                    newSl = FindTrailingZoneSl(pos.TradeType, currentPrice, new[] { ZoneTier.Major });

                if (newSl.HasValue)
                {
                    if (pos.TradeType == TradeType.Buy && newSl > pos.StopLoss && newSl < currentPrice)
                        ModifyPosition(pos, newSl, pos.TakeProfit);
                    else if (pos.TradeType == TradeType.Sell && newSl < pos.StopLoss && newSl > currentPrice)
                        ModifyPosition(pos, newSl, pos.TakeProfit);
                }
            }
        }

        private double? FindTrailingZoneSl(TradeType direction, double currentPrice, ZoneTier[] allowedTiers)
        {
            var validZones = _snrIndicator.ActiveZonesDict.Values.Where(z => allowedTiers.Contains(z.Tier)).ToList();
            if (!validZones.Any()) return null;

            if (direction == TradeType.Buy)
            {
                var trailingZone = validZones.Where(z => z.Top < currentPrice).OrderByDescending(z => z.Top).FirstOrDefault();
                if (trailingZone != null)
                {
                    double atrBuffer = _atr.Result.LastValue * (trailingZone.Tier == ZoneTier.Minor ? MicroAtrMult : MacroAtrMult);
                    return trailingZone.Bottom - atrBuffer; 
                }
            }
            else
            {
                var trailingZone = validZones.Where(z => z.Bottom > currentPrice).OrderBy(z => z.Bottom).FirstOrDefault();
                if (trailingZone != null)
                {
                    double atrBuffer = _atr.Result.LastValue * (trailingZone.Tier == ZoneTier.Minor ? MicroAtrMult : MacroAtrMult);
                    return trailingZone.Top + atrBuffer; 
                }
            }
            return null;
        }

        protected override double GetFitness(GetFitnessArgs args) 
        { 
            if (args.NetProfit <= 0) return args.NetProfit; 
            var trades = args.History.OrderBy(t => t.ClosingTime).ToList(); 
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0, cum = 0; 
            for (int i = 0; i < trades.Count; i++) 
            { 
                cum += trades[i].NetProfit; 
                sumX += i; 
                sumY += cum; 
                sumXY += i * cum; 
                sumX2 += i * i; 
                sumY2 += cum * cum; 
            } 
            
            double r2 = Math.Pow(((trades.Count * sumXY) - (sumX * sumY)) / Math.Sqrt(((trades.Count * sumX2) - (sumX * sumX)) * ((trades.Count * sumY2) - (sumY * sumY))), 2); 
            if(args.MaxEquityDrawdownPercentages >= 5 && args.MaxEquityDrawdownPercentages < 10) 
                return (((args.ProfitFactor * (args.NetProfit / Math.Max(1, args.MaxEquityDrawdown)) * ((double)args.WinningTrades / args.TotalTrades) * r2) * Math.Log10(args.TotalTrades)) / Math.Max(0.1, args.MaxEquityDrawdownPercentages)) * (args.NetProfit / startingBalance) * r2 * 0.5; 
            else if(args.MaxEquityDrawdownPercentages >= 10)
                return ((((args.ProfitFactor * (args.NetProfit / Math.Max(1, args.MaxEquityDrawdown)) * ((double)args.WinningTrades / args.TotalTrades) * r2) * Math.Log10(args.TotalTrades)) / Math.Max(0.1, args.MaxEquityDrawdownPercentages)) * (args.NetProfit / startingBalance) * r2) / args.MaxEquityDrawdownPercentages;
            else if(args.TotalTrades <= MinTrades)
                return ((((args.ProfitFactor * (args.NetProfit / Math.Max(1, args.MaxEquityDrawdown)) * ((double)args.WinningTrades / args.TotalTrades) * r2) * Math.Log10(args.TotalTrades)) / Math.Max(0.1, args.MaxEquityDrawdownPercentages)) * (args.NetProfit / startingBalance) * r2) * args.TotalTrades/MinTrades;
            else if(args.TotalTrades > MinTrades && LinBon == true)
                return (((args.ProfitFactor * (args.NetProfit / Math.Max(1, args.MaxEquityDrawdown)) * ((double)args.WinningTrades / args.TotalTrades) * r2) * Math.Log10(args.TotalTrades)) / Math.Max(0.1, args.MaxEquityDrawdownPercentages)) * (args.NetProfit / startingBalance) * r2 * (1 + (args.TotalTrades - MinTrades)/MinTrades/LinDiv); 
            else
                return (((args.ProfitFactor * (args.NetProfit / Math.Max(1, args.MaxEquityDrawdown)) * ((double)args.WinningTrades / args.TotalTrades) * r2) * Math.Log10(args.TotalTrades)) / Math.Max(0.1, args.MaxEquityDrawdownPercentages)) * (args.NetProfit / startingBalance) * r2 * (1 + (Math.Pow(args.TotalTrades - MinTrades, HypExp)/MinTrades));
        }
    }
}