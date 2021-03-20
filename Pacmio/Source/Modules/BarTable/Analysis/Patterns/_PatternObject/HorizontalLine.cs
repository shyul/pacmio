﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;

namespace Pacmio.Analysis
{
    public class HorizontalLine : IPatternObject
    {
        public HorizontalLine(IChartPattern source, PivotPt pt1, double tolerance)
        {
            Source = source;
            P1 = pt1;
            Level = P1.Level;
            Tolerance = tolerance;
        }

        public PivotPt P1 { get; }

        public int X1 => P1.Index;

        public double Y1 => P1.Level;

        public IChartPattern Source { get; }

        public double Weight { get; set; }

        public double Level { get; protected set; }

        public double Tolerance { get; }
    }
}