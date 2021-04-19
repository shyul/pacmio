﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;

namespace Pacmio
{
    public class EntryDecision : IDecision
    {
        public EntryDecision(Bar b)
        {
            DecisionBar = b;
        }

        public Bar DecisionBar { get; }

        public EntryType Type { get; set; }

        /// <summary>
        /// Positive means adding scale amount of position
        /// Negative means removing scale amount of position
        /// This is a ratio data: 1 means initial entry of the maximum riskable position, 2 means add double
        /// 0.5 (Remove Liq) means remove half, 1 (Remove) means empty the position.
        /// The actual "quantity" will be calculated with R/R and WinRate of the backtesting result.
        /// </summary>
        public double Scale { get; set; } = 1;

        public double EntryPrice { get; set; }

        public double ProfitTakePrice { get; set; }

        public double StopLossPrice { get; set; }

        public double Risk => EntryPrice - StopLossPrice;
    }
}