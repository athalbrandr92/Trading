using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class RangeBreakout : Robot
    {
        // --- 1. TIME SETTINGS ---
        [Parameter("Start Time (hour)", DefaultValue = 11, Group = "1. Time")] public int StartTime { get; set; }
        [Parameter("End Time (hour)", DefaultValue = 9, Group = "1. Time")] public int EndTime { get; set; }
        [Parameter("Open Range Minute", DefaultValue = 0, Group = "1. Time")] public int OpenRangeMin { get; set; }

        // --- 2. STRATEGY SETTINGS ---
        [Parameter("Lookback Bars", DefaultValue = 12, Group = "2. Strategy")] public int LookbackBars { get; set; }
        [Parameter("Risk (%)", DefaultValue = 0.5, Group = "2. Strategy")] public double Risk { get; set; }
        [Parameter("R:R Ratio (Long)", DefaultValue = 9, Group = "2. Strategy")] public double RRRatioLong { get; set; }
        [Parameter("R:R Ratio (Short)", DefaultValue = 9, Group = "2. Strategy")] public double RRRatioShort { get; set; }

        // --- 3. INDICATOR SETTINGS ---
        [Parameter("MA Lookback", DefaultValue = 150, Group = "3. Indicators")] public int MALookback { get; set; }
        public enum MaModeType { PriceAboveBelow, SlopeRisingFalling }
        [Parameter("MA Logic Mode", DefaultValue = MaModeType.SlopeRisingFalling, Group = "3. Indicators")] public MaModeType MaLogic { get; set; }
        [Parameter("RSI Threshold", DefaultValue = 25, Group = "3. Indicators")] public int RSIVal { get; set; }
        [Parameter("RSI High-Low", DefaultValue = true, Group = "3. Indicators")] public bool RSIHiLo { get; set; }
        [Parameter("RSI Reverse", DefaultValue = false, Group = "3. Indicators")] public bool RSIReverse { get; set; }

        // --- 4. ADX FILTER ---
        public enum AdxFilterMode { Off, Min, Max, MinMax }
        [Parameter("ADX Mode", DefaultValue = AdxFilterMode.Off, Group = "4. ADX Filter")] public AdxFilterMode AdxMode { get; set; }
        [Parameter("ADX Period", DefaultValue = 14, Group = "4. ADX Filter")] public int AdxPeriod { get; set; }
        [Parameter("ADX Min Level", DefaultValue = 20, Group = "4. ADX Filter")] public double AdxMin { get; set; }
        [Parameter("ADX Max Level", DefaultValue = 45, Group = "4. ADX Filter")] public double AdxMax { get; set; }

        // --- 5. FILTERS ---
        [Parameter("Min Body Ratio", DefaultValue = 0.4, Group = "5. Filters")] public double MinBodyRatio { get; set; }
        [Parameter("Max Rejection Wick", DefaultValue = 0.0, Group = "5. Filters")] public double MaxRejectionWickRatio { get; set; }
        [Parameter("Max Spread (Pips)", DefaultValue = 1.0, Group = "5. Filters")] public double MaxSpread { get; set; }
        [Parameter("Max Candle (ATR Mult)", DefaultValue = 2.8, Group = "5. Filters")] public double MaxCandleAtrMultiplier { get; set; }
        [Parameter("Min Volatility Ratio", DefaultValue = 0.7, Group = "5. Filters")] public double MinVolatilityRatio { get; set; }
        [Parameter("ATR Short Period", DefaultValue = 14, Group = "5. Filters")] public int AtrPeriod { get; set; }
        [Parameter("ATR Long Period", DefaultValue = 100, Group = "5. Filters")] public int AtrLongPeriod { get; set; }

        // --- 6. TRAILING STOPS ---
        public enum TrailType { None, PSAR, MTF_EMA, Extrema, Chandelier }
        [Parameter("Trailing Type", DefaultValue = TrailType.None, Group = "6. Trailing")] public TrailType SelectedTrail { get; set; }
        [Parameter("Trail TimeFrame", DefaultValue = "Hour", Group = "6. Trailing")] public TimeFrame TrailTF { get; set; }
        [Parameter("EMA Trail Period", DefaultValue = 49, Group = "6. Trailing")] public int EmaTrailPeriod { get; set; }
        [Parameter("Extrema Lookback", DefaultValue = 6, Group = "6. Trailing")] public int ExtremaBars { get; set; }
        [Parameter("PSAR Min AF", DefaultValue = 0.02, Group = "6. Trailing")] public double PsarMinAF { get; set; }
        [Parameter("PSAR Max AF", DefaultValue = 0.2, Group = "6. Trailing")] public double PsarMaxAF { get; set; }
        [Parameter("Chandelier Mult", DefaultValue = 3.0, Group = "6. Trailing")] public double ChandelierMult { get; set; }

        // --- 7. MANAGEMENT ---
        [Parameter("Max Positions", DefaultValue = 5, Group = "7. Management")] public int MaxPositions { get; set; }
        [Parameter("Order Magic", DefaultValue = 1, Group = "7. Management")] public int OrderMagic { get; set; }

        private Bars _htfBars;
        private double openHigh, openLow, secondaryHigh, secondaryLow;
        private bool justTraded = false;
        private ExponentialMovingAverage ema, trailEma;
        private RelativeStrengthIndex rsi;
        private AverageTrueRange _atrShort, _atrLong, _chandelierAtr;
        private DirectionalMovementSystem _adx;
        private ParabolicSAR _psar;
        private DateTime lastBarTime = DateTime.MinValue;
        private string Label => $"RB_{OrderMagic}";
        private double startingBalance;

        protected override void OnStart()
        {
            // --- SANITY CHECKS ---
            if (AdxMode == AdxFilterMode.Off && (AdxPeriod != 14 || AdxMin != 20 || AdxMax != 45)) { Print("Stopping: ADX Params must be default when Off."); Stop(); return; }
            
            if (SelectedTrail == TrailType.None)
            {
                if (TrailTF != TimeFrame.Hour || EmaTrailPeriod != 49 || ExtremaBars != 6 || PsarMinAF != 0.02 || PsarMaxAF != 0.2 || ChandelierMult != 3.0)
                { Print("Stopping: Trailing Params must be default when None."); Stop(); return; }
            }

            if (RSIHiLo && RSIReverse) { Print("Conflict: RSI HiLo and RSI Reverse cannot both be true."); Stop(); return; }

            // --- INITIALIZATION ---
            ema = Indicators.ExponentialMovingAverage(Bars.ClosePrices, MALookback);
            rsi = Indicators.RelativeStrengthIndex(Bars.ClosePrices, 14);
            _atrShort = Indicators.AverageTrueRange(Bars, AtrPeriod, MovingAverageType.Exponential);
            _atrLong = Indicators.AverageTrueRange(Bars, AtrLongPeriod, MovingAverageType.Exponential);
            _adx = Indicators.DirectionalMovementSystem(AdxPeriod);

            if (SelectedTrail == TrailType.MTF_EMA)
                trailEma = Indicators.ExponentialMovingAverage(MarketData.GetBars(TrailTF).ClosePrices, EmaTrailPeriod);
            if (SelectedTrail == TrailType.PSAR)
                _psar = Indicators.ParabolicSAR(MarketData.GetBars(TrailTF), PsarMinAF, PsarMaxAF);
            if (SelectedTrail == TrailType.Extrema || SelectedTrail == TrailType.Chandelier)
                _htfBars = MarketData.GetBars(TrailTF);
            if (SelectedTrail == TrailType.Chandelier)
                _chandelierAtr = Indicators.AverageTrueRange(_htfBars, AtrPeriod, MovingAverageType.Exponential);

            lastBarTime = Bars.OpenTimes.LastValue;
            startingBalance = Account.Balance;
        }

        protected override void OnTick() { if (SelectedTrail != TrailType.None) HandleTrailing(); }

        protected override void OnBar()
        {
            DefineRanges();
            DateTime current = Bars.OpenTimes.LastValue;
            if (current == lastBarTime) return;
            lastBarTime = current;

            if (EnableTrade() && Positions.Count(p => p.Label == Label) < MaxPositions)
            {
                double adxVal = _adx.ADX.Last(1);
                if (CheckAdx(adxVal))
                    TradeIfAble(ema.Result.LastValue, rsi.Result.Last(1), _atrShort.Result.Last(1), _atrLong.Result.Last(1));
            }
        }

        protected override double GetFitness(GetFitnessArgs args)
        {
            if (args.TotalTrades < 40) return 0;
            if (args.NetProfit <= 0) return args.NetProfit;
            if (args.MaxEquityDrawdownPercentages > 7) return args.MaxEquityDrawdownPercentages * -1;

            double rScore = CalculateRsquared(args);
            double recoveryFactor = args.NetProfit / Math.Max(1.0, args.MaxEquityDrawdown);
            double winRate = (double)args.WinningTrades / args.TotalTrades;
            double alphaScore = args.ProfitFactor * recoveryFactor * winRate * Math.Pow(rScore, 2);
            double tradeWeight = Math.Log10(args.TotalTrades);
            double nPP = args.NetProfit / startingBalance;

            return ((alphaScore * tradeWeight) / Math.Max(0.1, args.MaxEquityDrawdownPercentages)) * nPP;
        }

        private double CalculateRsquared(GetFitnessArgs args)
        {
            var trades = args.History.OrderBy(t => t.ClosingTime).ToList();
            int n = trades.Count;
            if (n < 5) return 0;
            double[] y = new double[n];
            double cumulative = 0;
            for (int i = 0; i < n; i++) { cumulative += trades[i].NetProfit; y[i] = cumulative; }
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;
            for (int i = 0; i < n; i++) { double x = i; sumX += x; sumY += y[i]; sumXY += x * y[i]; sumX2 += x * x; sumY2 += y[i] * y[i]; }
            double numerator = (n * sumXY) - (sumX * sumY);
            double denominator = Math.Sqrt(((n * sumX2) - (sumX * sumX)) * ((n * sumY2) - (sumY * sumY)));
            if (denominator == 0) return 0;
            double r = numerator / denominator;
            return Math.Max(0, r * r);
        }

        private bool CheckAdx(double val)
        {
            return AdxMode switch
            {
                AdxFilterMode.Min => val >= AdxMin,
                AdxFilterMode.Max => val <= AdxMax,
                AdxFilterMode.MinMax => val >= AdxMin && val <= AdxMax,
                _ => true
            };
        }

        private void DefineRanges()
        {
            var serverTime = Server.Time;
            if (serverTime.Hour == StartTime && serverTime.Minute == OpenRangeMin)
            {
                int bb = LookbackBars; if (Bars.Count < bb + 1) return;
                openHigh = Bars.HighPrices.Last(1); openLow = Bars.LowPrices.Last(1);
                for (int i = 2; i <= bb; i++) { openHigh = Math.Max(openHigh, Bars.HighPrices.Last(i)); openLow = Math.Min(openLow, Bars.LowPrices.Last(i)); }
            }
        }

        private void TradeIfAble(double maVal, double rsiVal, double atrS, double atrL)
        {
            if (Symbol.Spread / Symbol.PipSize > MaxSpread) return;
            if (atrS < (atrL * MinVolatilityRatio)) return;
            double close = Bars.ClosePrices.Last(1), open = Bars.OpenPrices.Last(1);
            double barH = Bars.HighPrices.Last(1), barL = Bars.LowPrices.Last(1);
            double totR = barH - barL, body = Math.Abs(close - open);
            if (totR > (atrS * MaxCandleAtrMultiplier)) return;
            double bodyRat = totR == 0 ? 0 : body / totR;
            double lWick = (barH - close) / (totR - (barH - close) == 0 ? 1 : totR - (barH - close));
            double sWick = (close - barL) / (totR - (close - barL) == 0 ? 1 : totR - (close - barL));
            double rsiB = 100 - RSIVal;
            bool maB = MaLogic == MaModeType.PriceAboveBelow ? close > maVal : maVal > ema.Result.Last(2);
            bool maS = MaLogic == MaModeType.PriceAboveBelow ? close < maVal : maVal < ema.Result.Last(2);

            if (!HasOpenPositionsForLabel())
            {
                if (open >= openLow && open <= openHigh)
                {
                    if (close > openHigh && maB && bodyRat >= MinBodyRatio && lWick <= MaxRejectionWickRatio && ((rsiVal > RSIVal && !RSIHiLo) || (RSIHiLo && rsiVal > RSIVal && rsiVal < rsiB) || (RSIReverse && rsiVal < RSIVal)))
                        ExecuteBuy(LotCalc(Risk, Math.Abs(Symbol.Ask - openLow)), openLow, Symbol.Ask + (Math.Abs(Symbol.Ask - openLow) * RRRatioLong));
                    if (close < openLow && maS && bodyRat >= MinBodyRatio && sWick <= MaxRejectionWickRatio && ((rsiVal < rsiB && !RSIHiLo) || (rsiVal < rsiB && rsiVal > RSIVal && RSIHiLo) || (RSIReverse && rsiVal > rsiB)))
                        ExecuteSell(LotCalc(Risk, Math.Abs(openHigh - Symbol.Bid)), openHigh, Symbol.Bid - (Math.Abs(openHigh - Symbol.Bid) * RRRatioShort));
                }
            }
        }

        private void HandleTrailing()
        {
            foreach (var pos in Positions.Where(p => p.Label == Label))
            {
                double? nSL = null;
                if (SelectedTrail == TrailType.PSAR) nSL = _psar.Result.LastValue;
                else if (SelectedTrail == TrailType.MTF_EMA) nSL = trailEma.Result.LastValue;
                else if (SelectedTrail == TrailType.Extrema) nSL = pos.TradeType == TradeType.Buy ? _htfBars.LowPrices.Minimum(ExtremaBars) : _htfBars.HighPrices.Maximum(ExtremaBars);
                else if (SelectedTrail == TrailType.Chandelier)
                {
                    double atr = _chandelierAtr.Result.LastValue;
                    int barsSinceEntry = _htfBars.OpenTimes.GetIndexByTime(pos.EntryTime);
                    int count = _htfBars.Count - 1 - barsSinceEntry;
                    if (count < 1) count = 1;
                    
                    if (pos.TradeType == TradeType.Buy)
                        nSL = _htfBars.HighPrices.Maximum(count) - (atr * ChandelierMult);
                    else
                        nSL = _htfBars.LowPrices.Minimum(count) + (atr * ChandelierMult);
                }

                if (nSL.HasValue)
                {
                    if (pos.TradeType == TradeType.Buy && nSL > pos.StopLoss && nSL < Symbol.Bid) ModifyPosition(pos, (double)nSL, pos.TakeProfit);
                    else if (pos.TradeType == TradeType.Sell && nSL < pos.StopLoss && nSL > Symbol.Ask) ModifyPosition(pos, (double)nSL, pos.TakeProfit);
                }
            }
        }

        private bool AfterStart() { var n = Server.Time; if (StartTime < EndTime) return n.Hour >= StartTime && n.Hour < EndTime; return !(n.Hour >= EndTime && n.Hour < StartTime); }
        private bool EnableTrade() => !HasOpenPositionsForLabel() && AfterStart() && openHigh != 0;
        private bool HasOpenPositionsForLabel() => Positions.Any(p => p.Label == Label && p.SymbolName == SymbolName);

        private void ExecuteBuy(double v, double sl, double? tp)
        {
            v = NormalizeVolume(v); if (v < Symbol.VolumeInUnitsStep) return;
            ExecuteMarketOrder(TradeType.Buy, SymbolName, v, Label, Math.Max(1.1, Math.Round(Math.Abs(Symbol.Ask - sl) / Symbol.PipSize, 1)), tp.HasValue ? (double?)Math.Round(Math.Abs(tp.Value - Symbol.Ask) / Symbol.PipSize, 1) : null);
            justTraded = true;
        }

        private void ExecuteSell(double v, double sl, double? tp)
        {
            v = NormalizeVolume(v); if (v < Symbol.VolumeInUnitsStep) return;
            ExecuteMarketOrder(TradeType.Sell, SymbolName, v, Label, Math.Max(1.1, Math.Round(Math.Abs(sl - Symbol.Bid) / Symbol.PipSize, 1)), tp.HasValue ? (double?)Math.Round(Math.Abs(tp.Value - Symbol.Bid) / Symbol.PipSize, 1) : null);
            justTraded = true;
        }

        private double NormalizeVolume(double v) { if (double.IsNaN(v) || v <= 0) return 0; return Math.Round(Math.Clamp(Math.Floor(v / Symbol.VolumeInUnitsStep) * Symbol.VolumeInUnitsStep, Symbol.VolumeInUnitsStep, Symbol.VolumeInUnitsStep * 1000), 6); }
        private double LotCalc(double rP, double slD) { if (Symbol.TickSize == 0 || slD <= 0) return Symbol.VolumeInUnitsMin; double rM = Account.Balance * rP / 100.0; double mPS = (slD / Symbol.TickSize) * Symbol.TickValue * Symbol.VolumeInUnitsStep; return mPS <= 0 ? Symbol.VolumeInUnitsStep : Math.Round(Math.Clamp(Math.Floor(rM / mPS) * Symbol.VolumeInUnitsStep, Symbol.VolumeInUnitsStep, Symbol.VolumeInUnitsStep * 1000), 6); }
    }
}