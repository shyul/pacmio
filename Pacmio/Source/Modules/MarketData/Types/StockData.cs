﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xu;

namespace Pacmio
{
    [Serializable, DataContract]
    public class StockData : BidAskData
    {
        public override void Initialize(Contract c)
        {
            base.Initialize(c);
            if (LiveBarTables is null) LiveBarTables = new List<BarTable>();
            RTLastTime = DateTime.MinValue;
            RTLastPrice = -1;
        }

        [DataMember]
        public DateTime BarTableEarliestTime { get; set; } = DateTime.MinValue;

        [DataMember]
        public Dictionary<DateTime, (DataSource DataSource, double Close, double Dividend)> DividendTable { get; private set; }
            = new Dictionary<DateTime, (DataSource DataSource, double Close, double Dividend)>();

        [DataMember]
        public Dictionary<DateTime, (DataSource DataSource, double Split)> SplitTable { get; private set; }
            = new Dictionary<DateTime, (DataSource DataSource, double Split)>();

        [DataMember]
        public Dictionary<DateTime, (DataSource DataSource, double EPS)> EPSTable { get; private set; }
            = new Dictionary<DateTime, (DataSource DataSource, double EPS)>();

        [DataMember]
        public Dictionary<DateTime, (DataSource DataSource, double Target)> TargetPriceList { get; private set; }
            = new Dictionary<DateTime, (DataSource DataSource, double Target)>();

        public MultiPeriod<(double Price, double Volume)> BarTableAdjust(bool includeDividend = false)
        {
            MultiPeriod<(double Price, double Volume)> list = new MultiPeriod<(double Price, double Volume)>();

            var split_list = SplitTable.Select(n => (n.Key, true, n.Value.Split));
            var dividend_list = DividendTable.Select(n => (n.Key, false, n.Value.Dividend / n.Value.Close));
            var split_dividend_list = split_list.Concat(dividend_list).OrderByDescending(n => n.Key);

            DateTime latestTime = DateTime.MaxValue;
            double adj_price = 1;
            double adj_vol = 1;

            foreach (var pair in split_dividend_list)
            {
                DateTime time = pair.Key;
                double value = pair.Item3;

                //Console.WriteLine("->> Loading: " + time + " / " + pair.Key.Type + " / " + pair.Value.Value);

                if (pair.Item2 && value != 1)
                {
                    list.Add(time, latestTime, (adj_price, adj_vol));
                    adj_price /= value;
                    adj_vol /= value;
                    latestTime = time;
                }

                if (!pair.Item2 && value != 0 && includeDividend)
                {
                    list.Add(time, latestTime, (adj_price, adj_vol));
                    adj_price *= 1 / (1 + value);
                    latestTime = time;
                }
            }

            list.Add(latestTime, DateTime.MinValue, (adj_price, adj_vol));

            return list;
        }

        // TODO: 
        // Queue the Tape Here
        [IgnoreDataMember]
        public DateTime RTLastTime { get; private set; } = DateTime.MinValue;

        [IgnoreDataMember]
        public double RTLastPrice { get; private set; } = -1;

        public void InboundLiveTick(DateTime time, double price, double size)
        {
            if (time > RTLastTime)
            {
                RTLastTime = time;

                if (double.IsNaN(price))
                    price = RTLastPrice;
                else
                    RTLastPrice = price;

                if (price > 0)
                {
                    // Construct the Tape Here !!!




                    lock (LiveBarTables)
                    {
                        Parallel.ForEach(LiveBarTables.Where(n => n.IsLive), bt => {
                            if (bt.BarFreq >= BarFreq.Daily && bt.LastTime < time.Date)
                            {
                                if (!double.IsNaN(Open) && !double.IsNaN(Open) && !double.IsNaN(Open) && !double.IsNaN(Volume))
                                {
                                    Bar b = new Bar(bt, time.Date, DataSource.Tick, Open, High, Low, LastPrice, Volume, Open, High, Low, LastPrice, Volume);
                                    bt.MergeFromSmallerBar(b);

                                }
                            }

                            if (bt.BarFreq < BarFreq.Daily || bt.LastTime == time.Date)
                            {
                                bt.AddPriceTick(time, price, size);
                            }
                        });
                    }
                }
            }
        }

        [IgnoreDataMember]
        public List<BarTable> LiveBarTables { get; set; } = new List<BarTable>();

        [DataMember]
        public Dictionary<int, (DateTime Time, double Price, double Size, Exchange MarketMaker)> MarketDepth { get; private set; }
            = new Dictionary<int, (DateTime Time, double Price, double Size, Exchange MarketMaker)>();


        // Tape

        // Position in Range

        // L2




        [DataMember]
        public double MarketCap { get; set; } = double.NaN;

        [DataMember]
        public double FloatShares { get; set; } = double.NaN;

        [DataMember]
        public double ShortPercent { get; set; } = double.NaN;

        [DataMember]
        public double ShortableShares { get; set; } = double.NaN;

        [DataMember]
        public double ShortStatus { get; set; } = double.NaN;




        // Add O H L and Last to daily tables 

        public void StartTicks()
        {
            List<string> param = new List<string>();

            // Also Fetch BarTable first if they are current...?
        }

        public void StopTicks()
        {

        }


        // News

        [DataMember]
        public bool EnableNews { get; set; } = false;

        /// <summary>
        /// Be aware of toggling changes
        /// </summary>
        [DataMember]
        public bool EnableShortableShares { get; set; } = false;




        // Social Media
    }
}
