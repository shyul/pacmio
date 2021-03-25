﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Xu;
using Xu.Chart;

namespace Pacmio.Analysis
{
    public class TrendLineAnalysis : PatternAnalysis, IChartBackground
    {
        public TrendLineAnalysis(TrailingPivotPtAnalysis tpa)
        {
            string label = "(" + tpa.MaximumInterval + ")";
            Name = GetType().Name + label;

            TrailingPivotPointAnalysis = tpa;
            TrailingPivotPointAnalysis.AddChild(this);

            Column_Result = new PatternColumn(this, typeof(TrendLineDatum), tpa.MaximumInterval);
            AreaName = MainBarChartArea.DefaultName;
        }

        public TrendLineAnalysis(int maximumInterval)
        {
            string label = "(" + maximumInterval + ")";
            Name = GetType().Name + label;

            TrailingPivotPointAnalysis = new TrailingPivotPtAnalysis(maximumInterval);
            TrailingPivotPointAnalysis.AddChild(this);

            Column_Result = new PatternColumn(this, typeof(TrendLineDatum), maximumInterval);
            AreaName = MainBarChartArea.DefaultName;
        }

        public override int GetHashCode() => GetType().GetHashCode() ^ Name.GetHashCode();

        #region Calculation

        public TrailingPivotPtAnalysis TrailingPivotPointAnalysis { get; }

        public override int MaximumInterval => TrailingPivotPointAnalysis.MaximumInterval;

        protected override void Calculate(BarAnalysisPointer bap)
        {
            BarTable bt = bap.Table;

            for (int i = bap.StartPt; i < bap.StopPt; i++)
            {
                if (bt[i] is Bar b && b[Column_Result] is null)
                {
                    double range_delta = (b.PivotRange.Max - b.PivotRange.Min) / 2;
                    double center = b.Close;

                    TrendLineDatum tld = new TrendLineDatum();
                    tld.TotalRange = new(center - range_delta, center + range_delta);
                    tld.AddLine(b.PivotPts.Select(n => n.Value));
                    b[Column_Result] = tld;
                }
            }
        }

        #endregion Calculation

        #region Chart Graphics

        public void DrawBackground(Graphics g, BarChart bc)
        {
            if (bc.LastBar_1 is Bar b && AreaName is string areaName && bc[areaName] is Area a && b[Column_Result] is TrendLineDatum tld)
            {
                int StartPt = a.StartPt;
                int StopPt = a.StopPt;

                g.SetClip(a.Bounds);
                g.SmoothingMode = SmoothingMode.HighQuality;

                double maxStrength = tld.StrengthRange.Max;
                foreach (TrendLine line in tld)
                {
                    int x1 = line.X1 - StartPt;
                    if (x1 >= 0)
                    {
                        int x3 = StopPt - StartPt - 1;
                        double y1 = line.Y1;
                        double y3 = y1 + (line.TrendRate * (x3 - x1));

                        Point pt1 = new Point(a.IndexToPixel(x1), a.AxisY(AlignType.Right).ValueToPixel(y1));
                        Point pt3 = new Point(a.IndexToPixel(x3), a.AxisY(AlignType.Right).ValueToPixel(y3));

                        int intensity = (255 * line.Strength / maxStrength).ToInt32();

                        if (intensity > 255) intensity = 255;
                        else if (intensity < 1) intensity = 1;

                        if (y3 > y1)
                            g.DrawLine(new Pen(Color.FromArgb(intensity, 26, 120, 32)), pt1, pt3);
                        else if (y3 < y1)
                            g.DrawLine(new Pen(Color.FromArgb(intensity, 120, 26, 32)), pt1, pt3);
                        else
                            g.DrawLine(new Pen(Color.FromArgb(intensity, 32, 32, 32)), pt1, pt3);
                    }
                }

                g.SmoothingMode = SmoothingMode.Default;
                g.ResetClip();
            }
        }

        #endregion Chart Graphics
    }
}