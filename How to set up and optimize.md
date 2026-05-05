### Instructions for trading bot.



#### 1\. Install cTrader Desktop



&nbsp;	Download: Go to the official cTrader website or your specific broker’s download page (e.g., Topstep, FTMO, or Pepperstone).



&nbsp;	Run Installer: Open the .exe file and follow the prompts.



&nbsp;	Sign In: Log in with your cTrader ID to sync your workspaces and accounts.



#### 2\. Add the Bot and Indicator (via GitHub)



##### //Since you are using raw C# (.cs) files from your repository, follow these steps to "build" them manually within the platform:



##### &nbsp;	For the S\&R Indicator:



&nbsp;		Switch to Indicators: In the same Algo tab, click the Indicators sub-tab.



&nbsp;		Create New: Click New Indicator.



&nbsp;		Paste Code: \* Delete the default code. Copy your Dynamic S\&R code from GitHub and paste it in.



&nbsp;		Build: Click Build (F11).



##### &nbsp;	For the Range Breakout Bot:



&nbsp;		Open Algo: Click the Algo tab (the robot icon) on the left sidebar of cTrader.



&nbsp;		Create New: Click the New cBot button in the top left.



&nbsp;		Paste Code: \* Delete all the default boilerplate code in the editor window. Copy the raw code from your Range Breakout v27 file on GitHub. Paste it directly into the cTrader code editor.



&nbsp;		Build: Click the Build button (or press F11). If it says "Build Succeeded" at the bottom, the bot is ready. If it doesn't, you probably skipped the indicator that the bot relies on.



&nbsp;		



#### 3\. Optimization:

#### 

##### //This is my optimization method, you may find yours to be better.

&nbsp;	

##### &nbsp;	Settings:



&nbsp;		Click the local instance of the bot (has a computer icon on its left).



&nbsp;		Click the optimization tab.



&nbsp;		Click the settings cogwheel icon.



&nbsp;		Adjust starting account balance to your account size (if using a <$20k account, set it to $100k for initial optimization). 



&nbsp;		If you're logged in to your broker (or prop firm) account, click the checkbox by "Apply commission automatically".



&nbsp;		Data: use m1 bars from server for initial optimization.



&nbsp;		Check the box next to "Download historical data for symbols to accurately convert profit and margin to account currency". This is technically only necessary if "USD" is not part of the symbol name.



&nbsp;		If optimizing during the London or New York session (8:00 AM UTC - 5:00 PM UTC or 1:00 PM UTC to 10:00 PM UTC / 3:00 AM - 12:00 PM or 8:00 AM - 5:00 PM EST), set fixed spread at or near current spread. If optimizing at another time of day or on the weekend, search either your broker's website or Google for typical spreads for the asset through your broker and set fixed spread to what you found.





##### &nbsp;	Optimization Criteria:



&nbsp;		Click the graph icon in the upper-left area of the cTrader terminal.



&nbsp;		Click the radio button "Custom".



##### &nbsp;	Resources: 



&nbsp;		Click the speedometer icon on the far right - just before the date selection.



&nbsp;		This allows you to specify how many of your CPU's "threads" (physical cores + up to one additional logical thread per core) the optimizer will use - if not doing anything resource-intensive in the background, slide the slider all the way to 100%. This setting is good for if all you're doing in the background is browsing the web, checking email, using social media, or watching video at 1080p or lower. 



&nbsp;		If you are watching UHD or playing moderately resource-intensive games (think Dead Frontier 2 on medium-low settings, COD:BO3 on medium settings, DS1\&2 on medium-low settings) slide it to around 66-75% (exact number depends on exact number of threads - if you have a six-thread CPU it will be 66%, any multiple of 4 will be 75%). 



&nbsp;		If you are playing heavily resource-intensive games (think anything from CODHQ or Elden Ring, or anything similar, even on their lowest settings), set the slider to 50%. 



&nbsp;		If you would have to set the slider below 50%, use your best judgement - it may be better to wait to run the optimization until you're doing something else. Up to you, as you are able to increase the number of threads in use later on.



&nbsp;		\*\*\*IN CASE YOU SKIPPED AHEAD AFTER SEEING WHAT YOU INTEND TO DO WHILE OPTIMIZING\*\*\* You can adjust the number of threads in use later on. If your game is stuttering and freezing, you can reduce it, if you started low and closed your game out, you can bump it back to 100. Note - it may take a moment for a reduction in the number of threads in use to take effect. I am not certain, but I think that the thread has to finish its current pass before being deselected.





##### &nbsp;	Parameters:

##### 

##### &nbsp;		For your \*first\* optimization (there will be several per symbol/timeframe combination, following a specific pattern):



###### &nbsp;		Parameter Group 1. Time



&nbsp;			Uncheck Timeframe. This will be run only on the timeframe selected. I recommend starting with the m5. In case you missed them, the options for asset and timeframe are there in the cBot instance.



&nbsp;			Start Hour: pair of good possibilities. Let the optimizer go wild from 0-23 (it will figure something out) or set it to something like hour before/hour of the market open for that asset up to an hour or two after the market open.



&nbsp;			End Hour: 0-23 is best. You never know what will turn out to be the absolute best time for that particular strategy to stop trading. It will loop around if the end hour is less than or equal to the start hour - if equal, it simply won't stop trading as long as the market is open.



&nbsp;			Open Range Minute: depends heavily on preference. 0-59 by 1 is fine, 0-55 by 5 is fine, 0-50 by 10 is fine, 0-45 by 15 is fine, etc.



&nbsp;			Lookback Minutes: you'll probably want this one to be either 5-90 by 5s or 15-90 by 15s, but technically any setting might be good. Use your best judgement.



&nbsp;			Uncheck Enable Holiday Blackout. This should \*ALWAYS\* be on ("Yes" in the cTrader UI). XAUUSD (Gold/Dollar) had a 400 point spike on a single price tick (less than a second) over the holidays in 2025. 400 points is $400 gained or lost on the minimum 0.01 lot size - you could lose your entire account on a single price tick during the holidays.



&nbsp;			Blackout Start Month: December by default, current code does not allow it to be optimized. Starting in any other month doesn't make sense. If you want to change it, first off, you're making a mistake, second, change it yourself in the code.



&nbsp;			Blackout Start Day: 20th by default. This is the last day the bot will place a new trade before the holidays begin. The code is set up so this can be optimized between the 10th and the 24th of December, to see when would be best if you're letting the bot decide when to stop trading. That's italicized because it is dangerous. You DO NOT want to leave a trade open over the holidays.



&nbsp;			Blackout End Month: January by default. Current code does not allow it to be changed. Same warning as the Blackout Start Month.



&nbsp;			Blackout End Day: Defaults to the 5th. Optimizer lets you optimize for starting back up as early as the second or waiting as long as the 10th.



###### &nbsp;		Parameter Group 2. Strategy

&nbsp;			

&nbsp;			Risk Percentage: This defaults to 1 - and that is a decent starting point for many people. If you are using a prop firm, however, you'll want this number lower. I personally use 0.2. If you are going to optimize it (say you're looking for max gains while still under the hard risk threshold), keep it kinda low. 0.1-1.0 should be your maximum range - I actually doubt that anything above 0.5% risk per trade will give you a parameter set that stays below 5% max equity drawdown (soft cap, as that's actually the daily hard cap for prop trading) and anything above 1% per trade is unlikely to keep you below 10% max equity drawdown (hard cap - you'll lose your prop firm account if you lose more than 10% of the account); the custom GetFitness() function accounts for this and halves the score of all parameter sets that took a drawdown greater than 5% and anything past 10% is punished exponentially by its drawdown.



&nbsp;			Max Dollar Risk: Only really needed on very small accounts, specifically on certain symbols, like XAUUSD. It isn't always possible to place a trade at the risk amount you specified above, due to minimum lot sizes vs your account size. For example, I'm starting on a $5k prop firm account. On XAUUSD, using a 0.01 lot size, the actual change in price on the screen is the actual change in my equity. If the opening range is more than $10 (very, very common), I literally cannot place a trade with my stop loss at the opposite end of the range while risking only $10. So, I have a higher maximum. If you have an account larger than $20k, this number can be any number higher than what you're actually risking per trade. If you're starting with a smaller account like I am, you should have your starting balance in the settings set to 100k, so the default value 1000 should be what you use. Don't optimize this at this point. You will optimize this later.



&nbsp;			ATR Buffer Mult: The bot will normally place its stop loss at the opposite side of the daily range, plus/minus a multiple of the ATR. If using a small account, the bot will check the initial stop loss against the Max Dollar Risk set above - if it would risk too much, it drops the ATR buffer, then if it's still too risky, it skips the trade. This setting determines the multiple of the ATR that the bot uses for its buffer. It will most likely be a number between 0 and 2.5. The optimizer allows it to go as high as 5.



&nbsp;			Primary TP Mode: Has nothing to do with toilet paper. This is how the bot sets its "Take Profit" order - the price point at which the position closes in profit. There are four methods. For your first pass, set this to Trailing Only.



&nbsp;			R:R Ratio (Long): For your first optimization, just uncheck it. Leave it at its default.



&nbsp;			R:R Ratio (Short): Same as for Long.



&nbsp;			Pivot Level (1-3): Same as the R:R ratios.



&nbsp;			Extrema Lookback (Days): Same as the others.



&nbsp;			Enable Wolfe Override: This parameter determines whether your take profit order should be changed if a "Wolfe wave" is detected. A Wolfe wave is a specific pattern that forms occasionally and indicates price is going to reach to at least the exact point the Wolfe wave predicts by drawing a line from the first through the fourth point of the formation, projected into the future. I don't fully understand Wolfe waves, so I can't explain them beyond that. Leave this box checked - if it's better to close early, the optimizer will figure that out.



&nbsp;			Rejection Override RR Base: For your initial optimization, this will be unchecked and left as default.



###### &nbsp;		Parameter Group 3. Filter Settings



&nbsp;			MA Lookback: The lookback period for the EMA filter. I've only recently begun optimizing this one - I was using 150 as the standard, now I'm running it from 12 to 480 by 12s on the m5 - from the average price over the last hour to the average price over the last two days. 



&nbsp;			MA Logic Mode: Either the price has to be above or below the EMA for a long or short position, respectively, or the EMA has to be sloping upward or sloping downward for a long or short, respectively. As of the time I am typing this up, there seems to be very little difference between the two logic modes, if any difference at all - however, I made a slight update to the code and may not remember to update this section after the current optimization finishes.



&nbsp;			RSI Mode: Defaults to Standard, because I had Gemini do the coding. RangeBound is likely the best - at least, it's what I've been using so far. I'll update this later if it turns out another setting is better. I'm currently allowing this one to run through its options - you should do the same. You can turn the RSI filter completely off.



&nbsp;			RSI Threshold: This is the number used for the RSI filter. The RSI threshold applies in both directions - the number actually set is the minimum, 100 minus the number set is the maximum.



&nbsp;			Invert RSI Logic: This will invert the logic of the RSI mode. For Standard and Swapped modes, the RSI will be required to be above when it would have been required to be below and vice versa. For RangeBound, it will require an RSI value outside the range rather than inside the range. I have no idea how this might affect the bot's performance. I'm letting the optimizer toggle this on and off.



&nbsp;			ADX Mode: You can also turn this one off. I don't recommend it. This one has less complex settings than the RSI filter has, as it only tells the bot it can trade, not which direction to trade. Either above a minimum, below a maximum, or between the MinMax. Note: if the optimizer repeatedly fails to continue optimizing beyond the first generation of passes (upper hundreds of passes), this is the first setting to lock in, as MinMax is almost always the best option.



&nbsp;			ADX Period: Optimizer allows anything from 5-50 by 1. I recommend either sticking with the default (14) or optimizing 7-49 by 7. If the optimizer is still failing to continue optimizing beyond the initial generation, this is the second parameter to lock in.



&nbsp;			ADX Min Level: Optimizer allows this to go all the way up to 40, I think, but that's stupid. ADX is a measure of trend strength, but higher numbers aren't always better. It's how strongly price ***has been trending*,** not how strongly price ***is going to trend.*** Higher values indicate the trend has already played itself out and is due for a reversal. I personally set the minimum to 5-15 by 2.5. We want to place the trade when the trend is first forming, not when it has already exhausted itself.



&nbsp;			ADX Max Level: The exhaustion parameter. Above this, the trend is likely exhausted and it's too late to get in on the action. I optimize this from 15-30 or 35 by 2.5. 



&nbsp;			Min Body Ratio: The minimum ratio of the "body" of the "candle" to its "wicks". When looking at a trading chart, the "candles" are the individual price movements printed. The "body" is the wide part. The "wicks" are the thin part that may or may not be above or below the body of the candle. The optimizer is already set to safe values - if you want a stricter run, feel free to up the Min to 0.3 or 0.4 and drop the Max as low as 0.6.



&nbsp;			Max Rejection Wick: The maximum ratio of the wick in the direction of the breakout. If the candle pulled back too sharply, it indicates that it may be a false breakout from the range. The default is safe for this one as well. For a stricter pass, you can tighten the Max down to 0.4 or 0.3.



&nbsp;			Base Max Spread (Pips): The baseline maximum spread allowable when placing a trade. The spread is the difference between the asking price for sellers and bidding price for buyers. It can get pretty wild at highly volatile times, like when I'm typing this up in early March, 2026. Therefore, I've decided to implement a dynamic max spread based on volatility. The base spread is multiplied by the ratio of the short period ATR as compared to the long period ATR, both set later.



&nbsp;			Max Candle (ATR Mult): This is the maximum size a candle can be as compared to the average of the last {short ATR period} candles. A really big candle usually indicates that the price movement happened too quickly - the trend may have already exhausted itself. Optimizer allows it to go up to 5 - I've found that it usually produces a result around 3.5 or lower. You can leave it at default or drop it to 3.5 - up to you.



&nbsp;			Min Volatility Ratio: The minimum ratio of the short ATR period to the long ATR period. We want to make sure price is really moving and not just lazily sliding sideways before placing a trade. This is basically a backup plan in case we get a fakeout from the ADX. The optimizer allows it to go as low as 0.5 and as high as 2.0, but past experience indicates the "sweet spot" may be between 0.7-1.3. However, due to the multitude of updates, I can't say for certain that's the best.



&nbsp;			 ATR Short Period: The lookback period for the shorter ATR. This is used for the candle size comparison and for the volatility ratio. Optimizer defaults to 2 to 20 by 1, I'm currently running it as 2 to 20 by 3, and it might be best to leave it at the standard 14. I'll update this later if that is the case and I remember to update.



&nbsp;			ATR Long Period: How far back to average the volatility for the comparison to current volatility. Not sure what the default is at the moment as the optimizer is running with my parameters (25 to 250 by 25) rather than the default and I don't feel like looking at the code at the moment. If you aren't familiar with fine-tuning trading indicators, use my parameters.



###### &nbsp;		Parameter Group 4. Trailing



&nbsp;			Trailing Type: Think this one defaults to "Off". So far, "Chandelier" has proven to be the best for metals, and either "PSAR" or "Chandelier" for forex pairs. For your first optimization over each symbol/timeframe combo, set it to "Chandelier". 



&nbsp;			Trail TimeFrame: Think this one defaults to hour (h1). Let the optimizer use reasonable settings for this. "Reasonable settings" for you may include the current timeframe or may start a step above, and may extend out to the daily timeframe (or further if you're looking for really long term trades) or may stop at the four-hour chart (h4). I usually set this to the m15 through the h4 for the m5 chart, but I started my current optimization run with it going from the m5 through the h4, so we'll see how that turns out. If in doubt, think of how you intend to use the bot - do you intend to try scalping low risk to reward trades? If so, keep it close to your current timeframe. Do you intend to "day trade" (enter and exit on the same day)? If so, you'll probably want to trail it around the m30 - h2. Do you intend to "swing trade"? Maybe trail it out to as far as the h4 or h8. If you're more of an investor just using the bot for the best entry point, you might go all the way out to the weekly chart. It's up to you.

&nbsp;		

&nbsp;			EMA Trail Period: The lookback period for the EMA if using the EMA trailing method. This one has a \*lot\* of reasonable settings. It's also a pain in the ass to optimize well. Again, it depends heavily on your trading style. It also depends heavily on which timeframe the optimizer selects. Scalpers should keep this number low, day traders mid, swing traders long, investors perhaps even longer. I personally intend to run scalping, day trading, and swing versions of the bot on my trading account - for scalping, 7 to 49 by 7 is a nice range, for day trading, 50 to 150 by tens is a nice range, and for swing trading, I expect something more like 120 to 480 by 12 or 24 will produce good results. If you're using my optimization method, you'll uncheck this one for now.



&nbsp;			Extrema Lookback (Bars): How many days to look back over to determine the high and low from that period if using this trailing method. Turns out that 1 is the currently forming daily bar - a feature rather than a bug, as it's handy for scalping. This goes all the way out to 26, to account for five-week months. If you're looking back past the most recent month for your extrema, your trailing stop is probably useless (if you're even going out as far as a month, your trailing stop might be useless). Again, this will depend on your trading style. lower numbers = closer stops. If you're using my optimization method, you'll uncheck this one for now.



&nbsp;			PSAR Min AF: The minimum acceleration factor for the parabolic support and resistance indicator. This is a trend-following indicator that tightens up more as the trend continues. I don't know exactly how it works, but I know it's useful. The default settings are the only logical settings. If you're using my optimization method, you'll uncheck this one for now.



&nbsp;			PSAR Max AF: The maximum acceleration factor for the PSAR trailing stop. The default settings are once again the only logical settings. If you're using my optimization method, you'll uncheck this one for now.



&nbsp;			Chandelier Mult: This is the ATR multiplier for the Chandelier trailing stop method. It trails the stop above/below the current price at a multiple of the current ATR. Price moves in a profitable direction? So does your stop. ATR tightens up? So does your stop. The default settings for this one are great, but tightening the range based on your preferences is okay, too.



###### &nbsp;		Parameter Group 5. Management

&nbsp;			

&nbsp;			Max Positions: Only optimize this if you know what you're doing. This setting will allow you to enter multiple trades at once, potentially risking your entire account. That doesn't mean the bot will say "Hey, there's a signal! Let's open up x trades!" Rather, it will allow more positions to be opened if the bot detects a signal later on. 



&nbsp;			Order Magic: This is only an identifier for the bot to be able to tell its trades from those placed by another instance of the bot, another bot entirely, or manual trades. I set mine to 1 for my first parameter set, 2 for my second, and so on. Don't optimize this - it does nothing.



###### &nbsp;		Parameter Group 6. Fitness



&nbsp;			Min Trades: The target number of trades for the bot to place for the timeframe you're optimizing over. Thanks to the newest update to the bot, this doesn't actually do much anymore. Still does have an effect, though, so I haven't removed it. Don't let the optimizer optimize this.



&nbsp;			Linear Bonus?: Defaults to "No" (hyperbolic rather than linear). If Yes, trades over the Min Trades target multiply the score by {number of trades over target / Linear Divisor). Don't let the optimizer optimize this.



&nbsp;			Linear Divisor: Defaults to 3. Go smaller for a bigger bonus based on trade count, go larger for a smaller bonus. Don't let the optimizer optimize this.



&nbsp;			Hyperbolic Exponent: If Linear Bonus? is set to "No", the number of trades is raised to this exponent rather than being divided. Smaller numbers for smaller bonuses, bigger numbers for bigger bonuses.

***NOTE - THE INDICATOR TIE IN STOPPED WORKING AND SO IS CURRENTLY COMMENTED OUT IN THE CODE. I HOPE TO FIX IT AT SOME POINT, BUT I NEED TO GET MY FINANCES IN ORDER FIRST. AS SUCH, THE REMAINING PARAMETER GROUP INSTRUCTIONS ARE NOT CURRENTLY NECESSARY. THERE IS MORE BELOW THEM THOUGH.***

###### &nbsp;		Parameter Group 7. SnR Management



&nbsp;			Rejection Logic: Either wick in close out or close in close out. Determines whether you close a position based off of a wick rejection of a support and resistance zone or if you require a candle to close inside and the next to close back outside. It's best to let the optimizer handle this one.



###### &nbsp;		Parameter Group 8. Indicator Vis

&nbsp;		//Note - these will all be set to "No" for your first pass if you are following my optimization method.



&nbsp;			Show Multiday: Enable/disable multiday high/low zones.



&nbsp;			Show Prev Day: Enable/disable previous day high/low zones.



&nbsp;			Show Asian: Enable/disable prior Asian session high/low zones.



&nbsp;			Show London: Enable/disable prior London session high/low zones.



&nbsp;			Show NY: Enable/disable prior New York session high/low zones.



&nbsp;			Show Psych Centuries: Enable/disable rounded psychological number (I.E. multiples of 100 on XAUUSD) zones.



&nbsp;			Show Psych Halves: Enable/disable psychological half century zones.



&nbsp;			Show Psych Quartiles: Enable/disable psychological quarter century zones.



&nbsp;			Show OBs: Enable/disable institutional order block zones.



&nbsp;			Show Doubles: Enable/disable double top/bottom zones.



&nbsp;			Show Consolidation: Enable/disable consolidation zones.



&nbsp;			Show Rejection: Enable/disable rejection formation (engulfing, shooting star, etc) zones.



###### &nbsp;		Parameter Group 9. Indicator Core

&nbsp;		//Note - these will all be unchecked for your first pass if you are following my optimization method.



&nbsp;			Ind Macro ATR Mult: ATR multiple (short ATR) to be added to / subtracted from either side of whole number levels (highs, lows, and psychological levels). The default setting range works well, but you might consider cutting it down to a maximum of 1 instead of 2.



&nbsp;			Ind Micro ATR Mult: ATR multiple (short ATR) to be added to / subtracted from the top and bottom values of chart formation zones to form a buffer around the formation and complete the zone. The default values are okay for this one as well, but you might want to bump the step up from 0.05 to 0.1.



&nbsp;			Psych Step: Defaults to 25 (metals, indices). If you don't know what the quartiles are for the symbol you're trading, ask me, use a search engine, or as an AI LLM such as ChatGPT or Gemini.



&nbsp;			UTC Offset: Your time zone's offset from Universal Coordinated Time. -5 for anyone in US Eastern time, which is why that's the default.



&nbsp;			Multiday TF: The timeframe the indicator will use to pull multiday support and resistance zones from. When you optimize for this, it should be set to h12 through either Weekly or Monthly. Up to you. I haven't really had time to dial this in yet.



&nbsp;			Multiday Lookback: How many candles to look back over to pull the multiday high and low from. The default settings for this are okay, but can be reined in if you have a specific parameter set in mind.



###### &nbsp;		Parameter Group 10. Indicator Sessions

&nbsp;		//Note - these are all best left as their defaults. Only adjust them if you know what you are doing and know you need to do so.



&nbsp;			Asian Start: Starting hour of Asian session.



&nbsp;			Asian End: Ending hour of Asian session.



&nbsp;			London Start: Starting hour of London session.



&nbsp;			London End: Ending hour of London session.



&nbsp;			NY Start: Starting hour of New York session.



&nbsp;			NY End:: Ending hour of New York session.



###### &nbsp;		Parameter Group 11. Indicator Patterns:

&nbsp;		//Note - if following my optimization method, these will all be unchecked for your first optimization run.



&nbsp;			Min Formation: The minimum number of candles for the chart formation to be a valid formation. Optimizer defaults to 3 to 30 by 1, I think, with default setting 10. I prefer 3 - 30 by 3, personally.



&nbsp;			Max Lookback: The maximum number of candles to look back over for the formation to be valid. Optimizer defaults to 15 to 150 by 5. I prefer to step by 15.



&nbsp;			Max Consolidation ATR: The maximum ATR multiple a consolidation range can be for it to be a "squeeze" formation (i.e. valid to use as support/resistance). The default optimizer settings are great for this.





##### &nbsp;	\*My Optimization Method\*

###### &nbsp;		

###### &nbsp;			First Optimization: 

###### &nbsp;	

&nbsp;				Should already be set up if you didn't skip anything. Let it run. If you get less than 1,000 passes, let it run again, up to two more times. If it fails again, set ADX Mode to MinMax and try again. If three more runs fail, set the ADX period to the standard of 14. If that also fails, you done goofed at some point. I got the genetic optimizer to give me a real run with all the boxes checked. Reread the instructions carefully.



###### &nbsp;			Second Optimization:



&nbsp;				All those things you optimized for in the first run should be deselected for this run. Set their settings to the best result from the first optimization. All those things I specifically said to deselect for the first run (Rejection Override RR Base in Parameter Group 2, all of Parameter Group 8, those of Parameter Group 9 that make sense to optimize for, and all of Parameter Group 11) should now be reselected. Let it run.

###### &nbsp;			Third Optimization:



&nbsp;				Lock in the best settings from your second optimization. Reselect everything you optimized for in the first optimization. Let it run.



###### &nbsp;			Fourth Optimization:



&nbsp;				Click the Settings cogwheel icon.



&nbsp;				Click the "Data" dropdown menu.



&nbsp;				Select "Tick data from server (accurate)"



&nbsp;				Deselect all currently optimized parameters and lock their values to your best result from the third optimization.



&nbsp;				Select "Base Max Spread (Pips)" in Parameter Group 3. Filter Settings.

&nbsp;	

&nbsp;				Set it to run in multiples of half pips (5 points for XAUUSD and any other symbol that uses integer pips rather than decimal pips, 0.5 for the rest) from the current value or average value you found online. 



&nbsp;				Let it run - this one will take a while, as it has to download a \*lot\* of data and the CPU has to process more data during the run.



###### &nbsp;			Fifth Optimization:



&nbsp;				Yeah, I know, this is getting ridiculous. Last one for this trailing stop type. If you're using an account size smaller than 20k and so set your starting balance to 100k, read on - if not, perform the above steps for the other trailing stop types (if you want to - the Chandelier very well may be the best) and then lock in your trailing stop parameters and repeat the optimization process for each individual take profit method (or one of your choosing, if you don't want to diversify), and if you really want to diversify, individual levels within the TP methods (think 0.5-3, 3.5-6, etc. for Fixed; 0.5-2.5, 3.0-5.0, 5.5-7.5 for Pivot Points; 1-3, 4-6, 7-9, etc. for Extrema) should also be optimized for. Back to the small accounts. For your final optimization:



&nbsp;				Click the Settings cogwheel icon.



&nbsp;				Adjust your starting balance to your actual account size.



&nbsp;				Click the parameters "levels" icon. 



&nbsp;				Deselect "Base Max Spread (Pips)".

&nbsp;			

&nbsp;				Select "Max Dollar Risk".



&nbsp;				Allow this to increment from about double your intended risk to about five times your intended risk. If you're managing your account exactly the same way I manage mine (and it also happens to be a 5k account), you should set this to 20 to 50 by 5.



&nbsp;				Let it run.



&nbsp;				Go back and reread the paragraph at the start of this section about repeating the process for different trailing stop methods and take profit methods - this README has gotten really long and I don't feel like typing anymore.



&nbsp;				Once you've completed all those optimizations (or if you were okay with just the Trailing Only, Chandelier trail exit method), move on to the next symbol/timeframe combo.

