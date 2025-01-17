﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xu;
using Xu.GridView;

namespace Pacmio
{
    public class TradeInfoGridView : GridWidget<ExecutionInfo>
    {
        public TradeInfoGridView() : base("Order History")
        {
            SourceRows = ExecutionManager.List;
            ExecutionManager.DataProvider.AddDataConsumer(this);
            DataIsUpdated(null);
        }

        ~TradeInfoGridView()
        {
            ExecutionManager.DataProvider.RemoveDataConsumer(this);
            Dispose();
        }

        //public int MaximumRows { get; set; } = int.MaxValue;
    }
}
