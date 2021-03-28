﻿/// ***************************************************************************
/// Shared Libraries and Utilities
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xu;

namespace Pacmio.Analysis
{
    public class OscillatorSignal : SignalAnalysis
    {
        public OscillatorSignal(IOscillator iosc)
        {
            OscillatorAnalysis = iosc;

            string label = "(" + iosc.Name + ")";
            GroupName = Name = GetType().Name + label;
            Column_Result = new(Name, typeof(OscillatorSignalDatum));

            iosc.AddChild(this);
        }

        public IOscillator OscillatorAnalysis { get; }

        public double UpperLimit => OscillatorAnalysis.UpperLimit;

        public double LowerLimit => OscillatorAnalysis.LowerLimit;

        public override DatumColumn Column_Result { get; }

        public Dictionary<Range<double>, double[]> LevelToTrailPoints = new()
        {
            { new Range<double>(-1, 5), new double[] { -7, -5 } },
            { new Range<double>(5, 10), new double[] { -3 } },
            { new Range<double>(10, 20), new double[] { -1 } },
            { new Range<double>(20, 80), new double[] { 0 } },
            { new Range<double>(80, 90), new double[] { 1 } },
            { new Range<double>(90, 95), new double[] { 3 } },
            { new Range<double>(95, 101), new double[] { 7, 5 } }
        };

        protected override void Calculate(BarAnalysisPointer bap)
        {
            BarTable bt = bap.Table;

            for (int i = bap.StartPt; i < bap.StopPt; i++)
            {
                Bar b = bt[i];

                OscillatorSignalDatum d = new();

                double rsi = b[OscillatorAnalysis];

                if (rsi >= UpperLimit)
                    d.Type = OscillatorSignalType.OverBought;
                else if (rsi <= LowerLimit)
                    d.Type = OscillatorSignalType.OverSold;

                d.TrailPoints = LevelToTrailPoints.Where(n => n.Key.Contains(rsi)).Select(n => n.Value).FirstOrDefault();

                b[Column_Result] = d;
            }
        }
    }
}
