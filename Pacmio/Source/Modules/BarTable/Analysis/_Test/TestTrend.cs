﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using Xu;
using Xu.Chart;

namespace Pacmio.Analysis
{
    public static class TestTrend
    {
        public static BarAnalysisSet BarAnalysisSet
        {
            get
            {
                var volumeEma = new EMA(Bar.Column_Volume, 20) { Color = Color.DeepSkyBlue, LineWidth = 2 };
                volumeEma.LineSeries.Side = AlignType.Left;
                volumeEma.LineSeries.Label = volumeEma.GetType().Name + "(" + volumeEma.Interval.ToString() + ")";
                volumeEma.LineSeries.LegendName = "VOLUME";
                volumeEma.LineSeries.LegendLabelFormat = "0.##";

                SMA slow_MA = new SMMA(5) { Color = Color.Orange, LineWidth = 2 };
                SMA fast_MA = new EMA(5) { Color = Color.DodgerBlue, LineWidth = 1 };
                //var ma_cross = new CrossIndicator(fast_MA, slow_MA);

                DebugSeries csd_range = new DebugColumnSeries(Bar.Column_Range);
                DebugSeries csd_nr = new DebugColumnSeries(Bar.Column_NarrowRange);
                DebugSeries csd_gp = new DebugColumnSeriesOsc(Bar.Column_GapPercent);
                DebugSeries csd_typ = new DebugLineSeries(Bar.Column_Typical);
                DebugSeries csd_pivot = new DebugColumnSeriesOsc(Bar.Column_Pivot);
                DebugSeries csd_trend = new DebugColumnSeriesOsc(Bar.Column_TrendStrength);

                PositionOfTimeframe potf = new(BarFreq.Annually);
                DebugSeries csd_potf = new DebugLineSeries(potf);

                NativePivotAnalysis npa = new(250);
                TrailingPivotPtAnalysis tpa = new(npa);

                List<BarAnalysis> sample_list = new()
                {
                    //new NativePivotAnalysis(),
                    //csd_potf,
                    //csd_typ,
                    //csd_pivot,
                    //csd_trend,

                    new TrendLineAnalysis(tpa),
                    new HorizontalLineAnalysis(tpa),
                    //new GetReversalIndexFromRangeBoundAnalysis(),
                    new FlagAnalysis(),
                    //new VolumeByPriceAnalysis(),
                };

                BarAnalysisSet bas = new(sample_list);

                return bas;
            }
        }
    }
}