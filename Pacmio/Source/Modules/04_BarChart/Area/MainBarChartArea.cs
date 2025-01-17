﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// Technical Analysis Chart UI
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xu;
using Xu.Chart;

namespace Pacmio
{
    public sealed class MainBarChartArea : BarChartArea
    {
        public const string DefaultName = "Main";

        public MainBarChartArea(BarChart chart, int height, float leftAxisRatio = 0.3f) : base(chart, DefaultName, height, leftAxisRatio)
        {
            Importance = Importance.Huge;

            // Configure Price series by assigning the chart type
            // ===================================================
            AddSeries(PriceSeries = new CandleStickSeries());

            //PriceSeries.TagColumns.Add(Bar.Column_PeakTags);
            //PriceSeries.TagColumns.Add(BarTable.PivotAnalysis.Column_PeakTags);

            // Configure volume series
            // ===================================================
            AddSeries(VolumeSeries = new AdColumnSeries(Bar.Column_Volume, Bar.Column_GainPercent, 50)
            {
                Order = int.MinValue,
                Side = AlignType.Left,
                Name = Bar.Column_Volume.Name,
                LegendName = "VOLUME",
                LegendLabelFormat = "0.##"
            });
        }

        public CandleStickSeries PriceSeries { get; }

        public AdColumnSeries VolumeSeries { get; }

        public override void RemoveSeries()
        {
            Series.Where(ser => ser != PriceSeries && ser != VolumeSeries).RunEach(ser => RemoveSeries(ser));
        }

        public override void DrawCursor(Graphics g, ITable table)
        {
            if (Chart.SelectedDataPointUnregulated >= 0)
            {
                //int pt = SelectedDataPoint;

            }
            //base.DrawCursor(g);
        }
    }
}
