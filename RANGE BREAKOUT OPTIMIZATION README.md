Optimizer parameters for Range Breakout v27

Timeframe: probably no need to let the optimizer change this - optimize the lower timeframes the bot was designed for first and then move up. If you're using tick data, feel free to drop all the way down to the m1. I started my optimizations on the m5, using m1 prices instead of ticks.

Start time (hour): within one to two hours before and within one to two hours after the start hour of the primary trading hour of the asset (i.e. Gold typically optimizes 8AM to 9AM start times)

End time (hour): At least one hour beyond the start time if you don't want it to trade 24 hours (if you do, set it the same). This one can really be any number of the daily hours (0-23) - use your judgement.

Open range minute: the minute at which to form the range, looking back over {Lookback bars} number of bars. I.E. 8AM Start Hour with 30 Open range minute with 6 lookback bars on the m5 calculates its daily range as the highest and lowest values between 8:00 to 8:30 AM.

Lookback bars: As mentioned above, how many *bars* to look back over - these should typically be stepped in increments of whatever gives you 5, 10, or 15 minute intervals between 1 and whatever the max for that timeframe is. I.E. typical m5 Lookback bars value I optimize is 3-12 by 3, so 15 minute range up to one full hour by 15-minute intervals. Note - much higher timeframes (h1+) start to lose meaning just a bit as far as a daily range is concerned - it flat won't work on the D1+.

Risk%: what amount to attempt to risk per trade - it might wind up slightly (or more than slightly in some cases) off due to rounding and account size vs minimum lot size. DO NOT OPTIMIZE THIS UNLESS YOU ARE ABSOLUTELY CERTAIN YOU KNOW WHAT YOU ARE DOING. THIS IS THE MOST LIKELY PARAMETER TO BLOW YOUR ACCOUNT.

Max Dollar Risk: Max risk in dollars (or perhaps account currency, look at the code for clarification) above which you will not take the trade no matter how good the setup is. This is related to a tiered stop loss - the Range Breakout bot first attempts to place its SL at the opposite side of the range +- an ATR-based buffer, then compares the trade risk to the Max Dollar Risk - if it exceeds it (because, say, you're trading XAUUSD on a $5k account) it then tries dropping the ATR buffer, then if it still exceeds the Max Dollar Risk, it refuses to place the trade. Don't optimize this - decide beforehand what risk you're willing to tolerate.

ATR Buffer Mult: Remember what I just said about an ATR-based buffer extending the SL from the exact opposite side of the range? This is it. I typically let it optimize from 0.5-2.5 by 0.5.

R:R Ratio (Long) and (Short): these tell the bot how far out to place the take profit in either direction if Primary TP Mode is set to fixed or HalfandHalf. Flat multiple of the final SL distance (including the buffer). For fixed, the entire position is linked to the set TP - for HalfandHalf, half of the position (rounded up to the nearest 0.01 lot) is closed on the TP and the rest is allowed to run with only a trailing stop and the rejection logic telling it when to close. Uncheck these if not using Fixed or HalfandHalf.

Primary TP Mode: how to set the TP. I mentioned Fixed and HalfandHalf above - the remaining options are Pivot Points (daily R1/R2/R3/S1/S2/S3 levels), Session (or is it Daily?) Extrema (maximum high and low of x number of prior days - currently bugged and needs fixed along with needing the name updated), and Trailing Only, which is the other half of the HalfandHalf method, just without a hard take profit for any portion of the position.

Pivot levels (1-3): which pivot level to put the TPs on. 1-3 are most common, but I removed the limiter and so it can technically go higher if you want. Best to only optimize it at reasonable levels to save on CPU resources - the highest I've seen the optimizer spit out was only 4. Also, uncheck this box if not specifically using the Pivot Points TP method.

Extrema Lookback (Days): How many days to look back over to get the "daily" high/low for TP placement. As mentioned, it's currently bugged and I keep forgetting to update the code - setting it to 1 current looks at the currently forming candle rather than the prior candle. Not recommended, though I have seen some decent results from it before realizing it was bugged. Recommend optimizing this between either 1 and 6 (if you're cool with the bug) or 2 and 5 for prior day to a week ago. Uncheck if not using the Extrema TP method.

Enable Wolfe Override: Look up Wolfe waves if you're curious - they're geometric chart formations that project a price line into the future. If this is active, the bot continuously scans for Wolfe Wave patterns and will override your current TP (or set a new one for Trailing Only) to be the current Estimated Price on Arrival (EPA) projected by the Wolfe wave. 

MA Lookback: used for the EMA filter. I use 150 by default and haven't bothered optimizing it in a long time. Use your judgement. It works with the MA Logic Mode parameter to determine how to filter trades - either the price has to be above or below the EMA or the EMA has to be climbing or dropping to allow the trade.

MA Logic Mode: PriceAboveBelow and SlopeRisingFalling, described under "MA Lookback". I have found zero significant difference between the two methods and no longer bother optimizing it - I just set it to PriceAboveBelow to save a few CPU cycles.

RSI Threshold: RSI Threshold beyond which trades will not be placed. Bullish RSI value is the set value, Bearish is 100 - the set value. Works with the next two parameters. I usually have this one optimize between 25-35 by 5.

RSI High-Low: If active (best results I've seen so far say to keep it active) then no matter whether looking at a bullish trade setup or bearish setup, the price must be between the two calculated values.

RSI Reverse: If active (best results so far say no) then the logic is reversed - bullish setups need the RSI below the set value and bearish setups need it to be above the bearish calculated value. This cannot be activated with RSI High-Low (assuming Gemini didn't strip out the sanity checks) or the bot will instantly stop itself and print an error.

ADXMode: Off, Min, Max, MinMax. Off is no ADX filter, Min is ADX must be above the minimum value, Max is ADX must be below the maximum value, MinMax is range-bound between the two. I've gotten the best results so far with MinMax.

ADX Period: Default 14. I don't optimize this, but you do you.

ADX Min Level: the minimum ADX for Min and MinMax modes. I typically optimize this between 10-15 or 10-20 by 1.

ADX Max Level: the maximum ADX for Max and MinMax modes. I typically optimize this between 20-40 or 25-35 by 1.

Min Body Ratio: The minimum ratio the body of the candle must be of the total size of the candle (to rule out spinning tops and dojis). This has *very* little effect on the overall strategy now that the other filters are in place. Reasonable values should start at 0.2 or 0.3 and go up to anywhere between 0.5 and 0.8, step 0.1.

Max Rejection Wick: The maximum ratio of the opposite wick of the signal candle to overall candle size. This also has *very little* effect with the other filters activated. I set it to optimize between 0.0 and either 0.3 or 0.5 by 0.1.

Max Spread (Pips): Highly asset-dependant. Recommend only testing this while using real ticks. I typically optimize everything else first on m1 data using typical max liquidity spread (so like 25 for XAUUSD or 0.0 for EURUSD) and then run it over real ticks with this being optimized at reasonable levels compared to the typical spread (i.e. 25-50 by 5 for XAUUSD, 0.0-0.5 by 0.1 for EURUSD, etc.). If you're running an m1 optimization over real ticks, you may need to optimize for this from the start, or just nuke it by setting the max allowable really high.

Max Candle (ATR Mult): Filters out exhaustion candles. If you don't care if you take a trade off a massive news candle or not, nuke it by setting a high value. If you do, set it to something reasonable. I typically use 1.5-3.5 by either 0.25 or 0.5. 

Min Volatility Ratio: Ratio of short ATR to long ATR, to ensure the market is moving at a bare minimum speed. Set this to something reasonable - I started with 0.5-1.5 by 0.1 at first and have since tightened it to 0.7-1.3 by 0.1 for my most recent optimizations.

ATR Short period: the lookback period for the short ATR (which I believe is also the default ATR for use with other ATR calculations aside from the Min Vol Rat). I leave mine at 14.

ATR Long period: the lookback period for the long ATR. I leave this at 100 these days, though it also shows decent results at 150 and 200. If you're going to optimize it, I recommend 50-250 by 50.

Enable Holiday Blackout: You *REALLY* want this turned on. Over the low-liquidity-high-volatility holiday weeks of late December 2025, the price on XAUUSD jumped damn near 400 points on a single price tick - even a 0.01 lot size would have cost you ~$400 in less than a second if you'd had a short trade open at the time. PLEASE do not turn this off unless you are absolutely certain you're willing to risk that kind of movement or are absolutely certain you'll have your trade placed in the right direction if something like that happens again.

Blackout Start and End months and days: Numerical months and days to begin and end the blackout on. Defaults to 12/20 - 1/5. Feel free to push the start up as late as Christmas Eve if you know you'll be closing your positions manually on Dec 23 and drop the end to as early as January 2. The important thing is to be out of the market from Christmas to New Year's.

Trailing type: Five options. None (no trail), PSAR (trailed on the PSAR indicator), EMA (trailed on the EMA), Extrema (trailed at the high/low of the last x candles) and Chandelier (ATR-based, "hangs the chandelier" from the highest point back to x ATR multiple away from the current price). I don't usually optimize this anymore - damn near every asset does kinda meh with no trail, Metals don't like the PSAR (at least on timeframes near the current one), everything seems to do okay with EMA, Extrema works on a handful of assets, and Chandelier has a pretty good performance across most assets. Feel free to experiment - I still intend to toy around with this parameter especially once I have all my assets and timeframes set up how I want and have a better computer to run the optimizations faster.

Trail Timeframe: Any timeframe can be set as the source for the trail. It will load up the indicator (or Bars object) for that timeframe and trail it there. This one tends to produce the best results between the h1 and the h4, at least when trading on the m5. Not too tight as this strategy is meant to find runners (day to swing trading rather than scalping) but no so loose as to be useless.

EMA Trail Period: Lookback for the EMA trail. I highly recommend doing something that seems kinda retarded at first glance - 49 to 147 by 7 gives decent results. If you're a stickler for 10s, 50-150 by 10 is also decent.

Extrema Lookback: how many bars to look back for extrema if using that stop loss method. I'll be quite honest - I have no idea exactly what this should be set to. I haven't really gotten good results consistently enough to know. I'd recommend if you're going to try it to start with a broad pass of something like 5 or 10 to 100 by either 5 or 10 and narrow it after getting a consistent range.

PSAR Min AF and Max AF: Settings for the PSAR acceleration factor (how quickly it tightens in to the price as the trend continues). I usually set these to 0.01-0.1 by 0.01 and 0.1-1 by 0.1 if I'm using the PSAR trail.

Chandelier Mult: Multiplier for the Chandelier ATR. I optimize this as either 0.5-4 by 0.5 by 0.25 - I'm currently running some optimizations with the range tightened to 1.75-3.5 by 0.25 because most of my best results have come from that range.

Max Positions: if you want the bot to be allowed to have multiple positions open at once. I haven't played with this much as I'm starting on a $5k account and risking multiple max losses in a single day just sounds terrible. 1 position at a time with risk set to 0.2% per trade can quite easily return 25% or more per year with Max Drawdowns below 5%, typically below 3%. I may toy with this one later on when I've got more equity for cushion.

Order Magic: no effect whatsoever on individual bot instances. Just to tell the bot "these trades were placed by this instance, those were not" so that it doesn't try to update your BTC stop loss based on your XAU rules.

Min Trades: Target minimum trades for the custom GetFitness function. Refer to the project README for how the fitness function works.

Linear Bonus?: Boolean. If yes, then the bonus for exceeding the minimum trades is calculated as the excess trades divided by the linear divisor. Yes gives a better bonus for exceeding the trade target but may reward systems that overtrade. No is my default.

Linear Divisor: its use was just described. Don't let the optimizer change this - set it yourself before starting the run.

Hyperbolic Exponent: If Linear Bonus is no, then the number of trades is raised to this exponent instead of being divided linearly. To prevent overtrading, have this number be below 1. I know the default is 0.6, but I usually use 0.75 instead.

Rejection Logic: Wick in close out or close in close out. This relates to the DynamicSnRBoxes tie in - should the position close if price wicked into a zone only, or should it only be considered a valid rejection if a candle actually closed inside the zone and the next closed outside? No idea which is better yet, just leave it clicked.

Show {whatthefuckever}: Use these zones for the rejection logic? Turn them all off to disable the DynamicSnRBoxes tie-in entirely, or turn off just specific ones that interfere with your best trades. Up to you.

Ind Macro ATR: ATR multiple for the buffer for time-based and psychological level (exact prices) so the zone forms. 0-1.5 by 0.1 works great.

Ind Micro ATR: ATR multiple for the buffer added to the top and bottom of chart formation zones for safety reasons. 0-1 by 0.1 works great.

Psych Step: the psychological quartile size for the given asset. On Gold, for instance, this is $25 - for BTC, it may be something like 125 or 250 (not sure), for forex pairs it's typically around 0.0025. Don't optimize this - set it before starting.

UTC Offset: The offset from UTC for your particular time zone, so that you don't have to convert your time to UTC later on when you set the session start and end times. EST is -5.

Multiday TF: Timeframe to be used for the multiday highs and lows for the Major tier time-based zones.

Multiday Lookback: How many bars to look back over for the aforementioned TF. 1-5 by 1 works well here.

Asian/London/NY Start/End: Start and end time for the various major trading sessions. If you're in EST, leave these to their defaults.

Min Formation: Minimum number of candles for a Double Top/Bottom or Consolidation Zone formation to be considered valid. 5-25 by 5 works well.

Max Lookback: Like the previous, but the maximum instead. 30-90 by 15 works well.

Max Consolidation ATR: How wide the zone for a consolidation zone should be, at maximum. This is useful for looking specifically for zones that are "coiled" and about to break out. 0.5 to 5 by 0.5 seems to work well for this one.
