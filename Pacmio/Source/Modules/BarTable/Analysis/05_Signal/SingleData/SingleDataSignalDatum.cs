﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// The trade rule applies to each contract
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xu;

namespace Pacmio.Analysis
{
    public class SingleDataSignalDatum : ISignalDatum
    {
        public SingleDataSignalType Type { get; set; } = SingleDataSignalType.None;

        public double[] TrailPoints { get; set; } = new double[] { };
    }
}
