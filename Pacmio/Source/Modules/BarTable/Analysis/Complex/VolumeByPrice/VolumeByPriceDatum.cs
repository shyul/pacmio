﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// https://school.stockcharts.com/doku.php?id=technical_indicators:volume_by_price
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Xu;
using Xu.Chart;

namespace Pacmio.Analysis
{
    public class VolumeByPriceDatum : IDatum
    {
        public Range<double> IntervalRange { get; set; }

        public Dictionary<Range<double>, double> PriceRangeToVolumeLUT { get; } = new Dictionary<Range<double>, double>();
    }
}