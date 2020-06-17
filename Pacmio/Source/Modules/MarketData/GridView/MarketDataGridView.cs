﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Xu;
using Xu.GridView;

namespace Pacmio
{
    public class MarketDataGridView : GridWidget
    {
        public MarketDataGridView(string name) : base(name)
        {

        }

        public MarketDataTable MarketDataTable { get; }

        public override ITable Table => MarketDataTable;

        public override IEnumerable<GridStripe> Stripes => throw new NotImplementedException();
    }
}
