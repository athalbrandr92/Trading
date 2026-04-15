using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class PureWickPressureBot : Robot
    {
        // --- Inputs ---
        [Parameter("Wick Average Lookback", Group = "Wick Logic", DefaultValue = 6, MinValue = 1)]
        public int Lookback { get; set; }

        [Parameter("ADX Smoothing", Group = "ADX Filter", DefaultValue = 13)]
        public int AdxSmoothing { get; set; }

        [Parameter("Min ADX", Group = "ADX Filter", DefaultValue = 27)]
        public int AdxMin { get; set; }

        [Parameter("Max ADX", Group = "ADX Filter", DefaultValue = 29)]
        public int AdxMax { get; set; }

        [Parameter("Volume (Lots)", Group = "Trade Settings", DefaultValue = 1.0)]
        public double Quantity { get; set; }

        [Parameter("Enable Session Filter", Group = "Time Filter", DefaultValue = false)]
        public bool UseSession { get; set; }

        [Parameter("Session Start (HH:MM)", Group = "Time Filter", DefaultValue = "09:30")]
        public string SessionStart { get; set; }

        [Parameter("Session End (HH:MM)", Group = "Time Filter", DefaultValue = "16:00")]
        public string SessionEnd { get; set; }

        [Parameter("Enable Trailing Stop", Group = "Risk Management", DefaultValue = false)]
        public bool UseTrailingStop { get; set; }

        [Parameter("Trailing Stop (Pips)", Group = "Risk Management", DefaultValue = 10)]
        public double TrailingStopPips { get; set; }

        // --- Global Variables ---
        private AverageDirectionalMovementIndexRating _adx;
        private IndicatorDataSeries _upperWicks;
        private IndicatorDataSeries _lowerWicks;
        private SimpleMovingAverage _smaUpper;
        private SimpleMovingAverage _smaLower;
        private double _startingBalance;

        protected override void OnStart()
        {
            _startingBalance = Account.Balance;
            _adx = Indicators.AverageDirectionalMovementIndexRating(AdxSmoothing);
            
            _upperWicks = CreateDataSeries();
            _lowerWicks = CreateDataSeries();
            
            _smaUpper = Indicators.SimpleMovingAverage(_upperWicks, Lookback);
            _smaLower = Indicators.SimpleMovingAverage(_lowerWicks, Lookback);
        }

        protected override void OnBar()
        {
            // 1. Calculate Pure Wicks from the candle that JUST closed (Last 1)
            double bodyMax = Math.Max(Bars.OpenPrices.Last(1), Bars.ClosePrices.Last(1));
            double bodyMin = Math.Min(Bars.OpenPrices.Last(1), Bars.ClosePrices.Last(1));

            // 2. Assign values to our series at the CURRENT index
            _upperWicks[Bars.Count - 1] = Bars.HighPrices.Last(1) - bodyMax;
            _lowerWicks[Bars.Count - 1] = bodyMin - Bars.LowPrices.Last(1);

            // 3. Logic check
            double result = _smaLower.Result.Last(0) - _smaUpper.Result.Last(0);
            double currentAdx = _adx.ADXR.Last(0);
            bool adxInRange = currentAdx >= AdxMin && currentAdx <= AdxMax;
            bool inSession = !UseSession || IsInSession();

            if (adxInRange && inSession)
            {
                if (result > 0) OpenPosition(TradeType.Buy);
                else if (result < 0) OpenPosition(TradeType.Sell);
            }
        }

        private void OpenPosition(TradeType tradeType)
        {
            var label = "WickBot";
            var currentPosition = Positions.Find(label, SymbolName);

            // 1. Close the opposite position if it exists
            if (currentPosition != null && currentPosition.TradeType != tradeType)
            {
                ClosePosition(currentPosition);
            }

            // 2. Open the new position
            if (Positions.Find(label, SymbolName) == null)
            {
                var volume = Symbol.QuantityToVolumeInUnits(Quantity);
        
                // This is the most stable signature: TradeType, Symbol, Volume, Label
                // We handle the trailing stop separately to ensure it compiles
                var result = ExecuteMarketOrder(tradeType, SymbolName, volume, label);

                if (result.IsSuccessful && UseTrailingStop)
                {
                    // If the trade opens and you want a trailing stop, we apply it here
                    var position = result.Position;
                    position.ModifyTrailingStop(true);
                }
            }
        }

        private bool IsInSession()
        {
            var now = Server.Time;
            TimeSpan start = TimeSpan.Parse(SessionStart);
            TimeSpan end = TimeSpan.Parse(SessionEnd);
            return now.TimeOfDay >= start && now.TimeOfDay <= end;
        }

        protected override double GetFitness(GetFitnessArgs args)
        {
            // 1. Initial Safety Checks
            if (args.NetProfit <= 0) return args.NetProfit;
            if (args.TotalTrades == 1) return 0;

            // 2. Detect Asset Class for Time Calculation
            bool isCrypto = SymbolName.ToUpper().Contains("BTC") ||
                            SymbolName.ToUpper().Contains("ETH") ||
                            SymbolName.ToUpper().Contains("XBT");

            TimeSpan duration = args.History.Last().ClosingTime - args.History.First().EntryTime;

            // Adjust the week divisor based on the asset class
            double daysPerWeek = isCrypto ? 7.0 : 5.0;
            double weeks = Math.Max(0.1, duration.TotalDays / daysPerWeek);
            double tradesPerWeek = args.TotalTrades / weeks;

            // Minimum Floor: 2 trades per week.
            double frequencyPenalty = Math.Min(1.0, tradesPerWeek / 2.0);
            frequencyPenalty = Math.Pow(frequencyPenalty, 4);

            // 3. Core Math (R2 / Regression)
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

            double denominator = Math.Sqrt(((trades.Count * sumX2) - (sumX * sumX)) * ((trades.Count * sumY2) - (sumY * sumY)));
            double r2 = (denominator == 0) ? 0 : Math.Pow(((trades.Count * sumXY) - (sumX * sumY)) / denominator, 2);

            // 4. Base Score Calculation
            double baseScore;
            if (args.MaxEquityDrawdownPercentages >= 10)
            {
                baseScore = ((((args.ProfitFactor * (args.NetProfit / Math.Max(1, args.MaxEquityDrawdown)) * ((double)args.WinningTrades / args.TotalTrades) * r2) * Math.Log10(args.TotalTrades)) / Math.Max(0.1, args.MaxEquityDrawdownPercentages)) * (args.NetProfit / _startingBalance) * r2) / args.MaxEquityDrawdownPercentages;
            }
            else
            {
                baseScore = (((args.ProfitFactor * (args.NetProfit / Math.Max(1, args.MaxEquityDrawdown)) * ((double)args.WinningTrades / args.TotalTrades) * r2) * Math.Log10(args.TotalTrades)) / Math.Max(0.1, args.MaxEquityDrawdownPercentages)) * (args.NetProfit / _startingBalance) * r2;
            }

            // 5. Apply Penalty
            return baseScore * frequencyPenalty;
        }
    }
}