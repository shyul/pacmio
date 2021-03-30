﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// The trade rule applies to each contract
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.Serialization;
using Xu;

namespace Pacmio.Analysis
{
    /// <summary>
    /// Indication: Move into Either Enter or Exit
    /// Active Indicator, yield score, and check other time frame's scores
    /// </summary>
    public abstract class IndicatorExec : Indicator, ISingleComplex
    {
        protected IndicatorExec(BarFreq freq, DataType type = DataType.Trades)
        {
            //IndicatorSet = new(this, freq, type);

            IndicatorSet = new();

            Column_Result = new DatumColumn(Name, typeof(ExecutionDatum));


            SignalSeries = new(this);
        }

        public IndicatorSet IndicatorSet { get; }

        public double MaximumHoldingTimeInMs { get; set; }


        #region Execution

        public DatumColumn Column_Result { get; }




        public abstract ExecutionDatum GenerateExecution(BarAnalysisPointer bap);

        // Assure Tables from other time frame has the same or later ticker time...

        // Get existing Position...

        // Determine trade direction, add liquidity or remove  liquidity

        // Fetch all results from other filtes.

        // Construct the score...




        #endregion Execution



        /// <summary>
        /// Example: Only trade 9:30 AM to 10 AM
        /// </summary>
        public MultiTimePeriod TradeTimeOfDay { get; set; }

        /// <summary>
        /// Wait 1000 ms, and cancel the rest of the unfiled order if there is any.
        /// </summary>
        public double WaitMsForOutstandingOrder { get; }

        /// <summary>
        /// If the price goes 1% to the upper side of the triggering level, then cancel the rest of the order.
        /// Can use wait Ms and set limit price.
        /// </summary>
        public double MaximumPriceGoingPositionFromDecisionPointPrecent { get; } = double.NaN;

        /// <summary>
        /// If the price goes ?? % to the down side of the triggering price, then cancel the unfiled order.
        /// </summary>
        public double MaximumPriceGoinNegativeFromDecisionPointPrecent { get; }

        #region Order

        public double MinimumRiskRewardRatio { get; set; }

        public double MinimumTradeSize { get; set; }

        #endregion Order

        public OrderInfo GenerateOrder(ExecutionDatum ed)
        {


            return new OrderInfo();
        }

        // Step 1: Define WatchList (Filters) Group sort by time frame -> Filter has B.A.S 

        // Step 1a: optionally manually defined [[[[ Daily ]]]] Scanner for faster live trading

        // Step 2: Define Signal Group



        #region Training Settings

        //   public double SlippageRatio { get; set; } = 0.0001;

        /// <summary>
        /// The unit for training time frames
        /// </summary>
        public virtual BarFreq TrainingUnitFreq { get; set; } = BarFreq.Daily;

        /// <summary>
        /// The number of days for getting the bench mark: RR ratio, win rate, volatility, max win, max loss, and so on.
        /// The commission model shall be defined by Simulate Engine.
        /// </summary>
        public virtual int TrainingLength { get; set; } = 5;

        /// <summary>
        /// The number of days enters the actual trade or tradelog for simulation | final bench mark.
        /// Only when the SimulationResult is positive (or above a threshold), does the trading start log, and this time, it logs the trades.
        /// </summary>
        public virtual int TradingLength { get; set; } = 1;

        #endregion Training Settings
    }
}