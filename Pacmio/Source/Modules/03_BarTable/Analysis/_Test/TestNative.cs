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
    public static class TestNative
    {
        public static BarAnalysisList BarAnalysisList
        {
            get
            {
                RelativeToAverage relative_body = new RelativeToAverage(Bar.Column_Body, 20);
                RelativeToAverage relative_volume = new RelativeToAverage(Bar.Column_Volume, 20);

                DebugSeries csd_relative_body = new DebugColumnSeries(relative_body);
                DebugSeries csd_relative_volume = new DebugColumnSeries(relative_volume);
                DebugSeries csd_consecutive_type = new DebugColumnSeries(Bar.Column_ConsecutiveType);

                DebugSeries csd_range = new DebugColumnSeries(Bar.Column_Range);
                DebugSeries csd_nr = new DebugColumnSeries(Bar.Column_NarrowRange);
                DebugSeries csd_gp = new DebugColumnSeriesOsc(Bar.Column_GapPercent);
                DebugSeries csd_typ = new DebugLineSeries(Bar.Column_Typical);
                DebugSeries csd_pivot = new DebugColumnSeriesOsc(Bar.Column_Pivot);
                DebugSeries csd_pivotstr = new DebugColumnSeriesOsc(Bar.Column_PivotStrength);

                DebugSeries csd_trend = new DebugColumnSeriesOsc(Bar.Column_TrendStrength);

                var stdev = new STDEV(Bar.Column_Typical, 20);
                DebugSeries csd_stdev = new DebugColumnSeries(stdev.Column_Percent);
                stdev.AddChild(csd_stdev);

                TimeFramePricePosition potf = new(BarFreq.Annually);
                DebugSeries csd_potf = new DebugLineSeries(potf);



                List<BarAnalysis> sample_list = new()
                {
                    //new NativeApexAnalysis(),

                    //new CandleStickDojiMarubozuSignal(),
                    //new CandleStickShadowStarSignal(),
                    //csd_potf,
                    //csd_range,
                    //csd_nr,
                    //csd_gp,
                    //csd_typ,
                    //csd_relative_body,
                    //csd_relative_volume,
                    //csd_consecutive_type,
                    //csd_trend,
                    //csd_pivot,
                    //csd_pivotstr,
                    //csd_trend,

                    //csd_stdev, 
                    //new CHOP(20),

                    //new CrossIndicator(),
                    //new CHOP(),

        
                };

                BarAnalysisList bat = new(sample_list);

                return bat;
            }
        }

        public static BarAnalysisList BarAnalysisListTimeFrame
        {
            get
            {
                var csd_potf = new DebugColumnSeries(new TimeFramePricePosition(BarFreq.Daily));
                var csd_tfcv = new DebugColumnSeries(new TimeFrameCumulativeVolume(BarFreq.Daily));

                var tfrv = new TimeFrameRelativeVolume(new TimePeriod(new Time(9, 30), new Time(10, 00)), 5, BarFreq.Daily);
                var csd_tfrv = new DebugColumnSeries(tfrv);
                var csd_tfrv_ema = new DebugColumnSeries(tfrv.Column_EMA);

                List<BarAnalysis> sample_list = new()
                {
                    //csd_potf,
                    csd_tfcv,
                    csd_tfrv_ema,
                    csd_tfrv,

                };

                BarAnalysisList bat = new(sample_list);

                return bat;
            }
        }
    }
}
