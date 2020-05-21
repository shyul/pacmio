﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// Technical Analysis Chart UI
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Xu;
using Xu.Chart;

namespace Pacmio
{
    public sealed class SignalArea : OscillatorArea
    {
        public SignalArea(BarChart chart) : base(chart, "Signal", 10)
        {
            BarChart = chart;
            Importance = Importance.Major;
            Order = int.MinValue + 1;

            UpperLimit = 0;
            LowerLimit = 0;
            UpperColor = Color.GreenYellow;
            LowerColor = Color.Pink;
            Reference = 0;
            FixedTickStep_Right = 2;

            AddSeries(SignalSeries = new SignalSeries(chart)
            {
                Order = int.MinValue + 1,
                Side = AlignType.Right,
                Name = "Signal",
                LegendName = "SIGNAL",
                LegendLabelFormat = "0.##"
            });
        }

        public BarChart BarChart { get; }

        public BarTable BarTable => BarChart.BarTable;

        public readonly SignalSeries SignalSeries;

        public override void DrawCustomBackground(Graphics g)
        {
            PositionArea.DrawPosition(g, this, BarTable);
        }
    }
}
