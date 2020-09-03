﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// Trade-Ideas API
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xu;
using TradeIdeas.TIProData;
using TradeIdeas.TIProData.Configuration;
using TradeIdeas.ServerConnection;
using System.Windows.Forms;

namespace Pacmio.TIProData
{
    public static partial class Client
    {
        public static TopListScanner tls { get; private set; }

        public static void TestTopList()
        {
            DateTime time = new DateTime(2020, 09, 03, 06, 30, 00);

            Console.WriteLine("TotalDays = " + (DateTime.Now - time).TotalDays);

            long epoch = time.ToEpoch().ToInt64();

            Console.WriteLine("Epoch = " + epoch);

            tls = new TopListScanner("Low Price Gappers")
            {
                PriceMin = 1.5,
                PriceMax = 25,
                GapPercentMin = 5,
                GapPercentMax = -5,
                VolumeMin = 50e3,
                AverageTrueRangeMin = 0.25,
                ExtraConfig = "form=1&sort=MaxGUP&omh=1&col_ver=1&show0=D_Symbol&show1=Price&show2=Float&show3=SFloat&show4=GUP&show5=TV&show6=EarningD&show7=Vol5&show8=STP&show9=RV&show10=D_Name&show11=RD&show12=FCP&show13=D_Sector&show14=",
            };

            tls.IsSnapshot = true;
            //tls.IsHistory = true;
            //tls.HistoricalTime = new DateTime(2020, 06, 10, 06, 30, 00);

            tls.Start();

            if (Connected)
            {


            }
        }
    }


    public class TopListScanner : Scanner
    {
        public TopListScanner(string name, int numberOfRows = 100)
        {
            Name = name;
            NumberOfRows = numberOfRows;
        }

        private TopList TopList { get; set; }

        public override void Start()
        {
            string configStr = ConfigString;
            Console.WriteLine(Name + " | " + configStr);

            var toplist = Client.Connection.TopListManager.GetTopList(configStr);
            TopList = toplist is TopList tl ? tl : null;
            TopList.TopListStatus += new TopListStatus(TopListStatus_Handler);
            TopList.TopListData += new TopListData(TopListData_Handler);
            TopList.Start();
        }

        public override void Stop() 
        { 
            TopList.Stop();
        }

        public DateTime LastUpdateTime { get; private set; } = DateTime.MinValue;

        public override string Name { get => GetConfigString("WN"); set => SetConfig("WN", value); }

        public override int NumberOfRows { get => GetConfigInt("count"); set => SetConfig("count", value); }

        public override bool IsSnapshot { get => m_IsSnapshot || IsHistory; set => m_IsSnapshot = value; }

        private bool m_IsSnapshot = false;

        public bool IsHistory { get => GetConfigBool("hist", "1"); set { SetConfig("hist", value, "1"); } }

        public DateTime HistoricalTime
        {
            get => m_HistoricalTime;

            set
            {
                DateTime time = value;

                if ((DateTime.Now - time).TotalDays > 90)
                    time = DateTime.Now.AddDays(-90);

                m_HistoricalTime = time;
            }
        }

        private DateTime m_HistoricalTime;

        public override double PriceMin { get => GetConfigDouble("MinPrice"); set => SetConfig("MinPrice", value); }

        public override double PriceMax { get => GetConfigDouble("MaxPrice"); set => SetConfig("MaxPrice", value); }

        public override double VolumeMin { get => GetConfigDouble("MinTV"); set => SetConfig("MinTV", value); }

        public override double VolumeMax { get => GetConfigDouble("MaxTV"); set => SetConfig("MaxTV", value); }

        public override double MarketCapMin { get => GetConfigDouble("MinMCap"); set => SetConfig("MinMCap", value); }

        public override double MarketCapMax { get => GetConfigDouble("MaxMCap"); set => SetConfig("MaxMCap", value); }

        public override double GapPercentMin { get => GetConfigDouble("MinGUP"); set => SetConfig("MinGUP", value); }

        public override double GapPercentMax { get => GetConfigDouble("MaxGUP"); set => SetConfig("MaxGUP", value); }

        public double AverageTrueRangeMin { get => GetConfigDouble("MinATR"); set => SetConfig("MinATR", value); }

        public double AverageTrueRangeMax { get => GetConfigDouble("MaxATR"); set => SetConfig("MaxATR", value); }

        public List<Exchange> Exchanges { get; } = new List<Exchange>() { Exchange.NYSE, Exchange.NASDAQ, Exchange.ARCA, Exchange.AMEX, Exchange.BATS };

        public string ExtraConfig { get; set; } = string.Empty;

        public override string ConfigString
        {
            get
            {
                if (Name.Length > 1) ConfigList["WN"] = Name;

                SetConfig("X_NYSE", Exchanges.Contains(Exchange.NYSE), "on");
                SetConfig("XN", Exchanges.Contains(Exchange.NASDAQ), "on");
                SetConfig("X_ARCA", Exchanges.Contains(Exchange.ARCA), "on");
                SetConfig("X_AMEX", Exchanges.Contains(Exchange.AMEX), "on");
                SetConfig("X_BATS", Exchanges.Contains(Exchange.BATS), "on");
                SetConfig("X_PINK", Exchanges.Contains(Exchange.OTCMKT), "on");

                if (IsHistory)
                {
                    long epoch = HistoricalTime.ToEpoch().ToInt64();
                    ConfigList["exact_time"] = epoch.ToString();
                }
                else if (ConfigList.ContainsKey("exact_time"))
                    ConfigList.Remove("exact_time");

                if (ExtraConfig.EndsWith("&")) ExtraConfig += "&";

                return ExtraConfig + string.Join("&", ConfigList.OrderBy(n => n.Key).Select(n => n.Key + "=" + n.Value).ToArray());
            }
        }

        public Dictionary<string, ColumnInfo> Columns { get; } = new Dictionary<string, ColumnInfo>();

        public HashSet<Stock> UnknownStock { get; } = new HashSet<Stock>();

        void TopListStatus_Handler(TopList sender)
        {
            if (sender == TopList)
            {
                Console.WriteLine("WindowName: " + sender.TopListInfo.WindowName);
                lock (Columns)
                {
                    Columns.Clear();
                    foreach (ColumnInfo c in sender.TopListInfo.Columns)
                    {
                        Columns[c.WireName] = c;
                        Console.WriteLine(c.WireName + " | " + c.Description + " | " + c.Format + " | " + c.Units + " | " + c.InternalCode + " | " + c.Graphics + " | " + c.TextHeader + " | " + c.PreferredWidth);
                    }
                }
                //Console.WriteLine(sender.TopListInfo.ToString());
            }
        }

        void TopListData_Handler(List<RowData> rows, DateTime? start, DateTime? end, TopList sender)
        {
            LastUpdateTime = DateTime.Now;
            if (IsSnapshot) Stop();

            lock (Columns)
            {
                lock (List)
                {
                    List.Clear();

                    foreach (RowData row in rows)
                    {
                        if (row.GetAsString("symbol") is string symbol && symbol.Length > 0 && Regex.IsMatch(symbol, @"^[a-zA-Z]+$"))
                        {
                            var list = ContractList.GetList(symbol, Exchanges);

                            if (list.Where(n => n is Stock).Count() > 0)
                            {
                                Stock stk = list.First() as Stock;

                                List.Add(stk);

                                if (list.Count() == 1 && row.GetAsString("c_D_Name") is string fullname && fullname.Length > 0)
                                    stk.FullName = fullname;

                                if (stk.ISIN.Length < 8 && !UnknownStock.Contains(stk))
                                {
                                    UnknownStock.Add(stk);
                                    ContractList.Fetch(stk);
                                }

                                string rowString = stk.ToString() + " >> " + stk.ISIN + " >> " + stk.FullName + " >> ";
                                foreach (string key in Columns.Keys)
                                {
                                    ColumnInfo ci = Columns[key];

                                    string dataString = row.GetAsString(key);
                                    string format = ci.Format.Trim();

                                    if (format == string.Empty)
                                    {
                                        rowString += ci.Description + "=" + dataString + "; ";
                                    }
                                    else if (format == "p")
                                    {
                                        if (row.GetAsString("four_digits") == "1")
                                        {
                                            rowString += ci.Description + "=" + dataString.ToDouble().ToString("N4") + "; ";
                                        }
                                        else
                                        {
                                            rowString += ci.Description + "=" + dataString.ToDouble().ToString("N2") + "; ";
                                        }
                                    }
                                    else
                                    {
                                        int digits = ci.Format.ToInt32();

                                        if (digits > 7)
                                            digits = 7;
                                        else if (digits < 0)
                                            digits = 0;

                                        rowString += ci.Description + "=" + dataString.ToDouble().ToString("N" + digits) + "; ";
                                    }
                                }
                                Console.WriteLine(rowString);
                                //Console.WriteLine(row.ToString());
                            }
                            else if (IB.Client.Connected && !UnknownSymbols.Contains(symbol))
                            {
                                UnknownSymbols.Add(symbol);
                                ContractList.Fetch(symbol);
                            }
                        }
                    }
                }

                // Trigger the list is updated event!!
            }
        }
    }
}
