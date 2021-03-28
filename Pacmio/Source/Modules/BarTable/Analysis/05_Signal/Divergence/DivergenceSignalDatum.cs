﻿/// ***************************************************************************
/// Shared Libraries and Utilities
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xu;
using Xu.Chart;

namespace Pacmio.Analysis
{
    public class DivergenceDatum : ISignalDatum
    {
        public DivergenceSignalType Type { get; set; }

        public double[] TrailPoints { get; set; }
    }
}
