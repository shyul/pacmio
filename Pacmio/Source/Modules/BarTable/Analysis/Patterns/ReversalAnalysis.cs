﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Xu;
using Xu.Chart;

namespace Pacmio
{
    public class ReversalAnalysis : BarAnalysis, ISingleData, IChartPattern
    {
        public ReversalAnalysis()
        {


            CalculatePivotRange.AddChild(this);
        }

        public CalculatePivotRange CalculatePivotRange { get; } = new CalculatePivotRange();

        public NumericColumn Column { get; protected set; }

        public NumericColumn Column_Result { get; protected set; }

        protected override void Calculate(BarAnalysisPointer bap)
        {
            BarTable bt = bap.Table;

            if (bap.StartPt < 0)
                bap.StartPt = 0;

            for (int i = bap.StartPt; i < bap.StopPt; i++)
            {
                if (bt[i] is Bar b && b[this] is PivotRangeDatum prd)// && b[Column] is double d)
                {
                    Bar b_1 = bt[i - 1];

                    double close_1 = b_1.Close;

                    double open = b.Open;
                    double close = b.Close;
                    double high = b.High;
                    double low = b.Low;

                    double w1 = prd.Weight(close_1, open); // gap
                    double w2 = prd.Weight(open, close); // movement
                    //double w3 = prd.Weight(low, high);




                    // Range intersection

                    // Cross up is plus, cross down is negative

                    // Abs the cross up and cross down is get its total travelling length

                    // Less to zero sum but large distance means reversal

                    // Large sum and large abs sums means penitration

                }
            }
        }

        public bool ChartEnabled { get; set; } = true;

        public string AreaName { get; }

        public void DrawBackground(Graphics g, BarChart bc)
        {
            if (ChartEnabled && AreaName is string areaName && bc[areaName] is Area a && bc.LastBar is Bar b && b[this] is PivotRangeDatum prd)
            {


            }
        }

        public void DrawOverlay(Graphics g, BarChart bc)
        {

        }


    }
}