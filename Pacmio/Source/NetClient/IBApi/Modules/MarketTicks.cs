﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// Interactive Brokers API
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using Xu;

namespace Pacmio.IB
{
    public partial class Client
    {
        public int MaximumSubscription => 100;

        public int ActiveSubscriptionCount => ActiveMarketTicks.Count + ActiveRealTimeBars.Count + ScanRequestList.Count;

        public bool SubscriptionOverflow => ActiveSubscriptionCount > MaximumSubscription - 5;

        public readonly ConcurrentDictionary<int, Contract> ActiveMarketTicks = new ConcurrentDictionary<int, Contract>();

        /// <summary>
        /// enericTickList:
        ///     100 Option Volume(currently for stocks)
        ///     101 Option Open Interest(currently for stocks)
        ///     104 Historical Volatility(currently for stocks)
        ///     105 Average Option Volume(currently for stocks)
        ///     106 Option Implied Volatility(currently for stocks)
        ///     162 Index Future Premium
        ///     165 Miscellaneous Stats
        ///     221 Mark Price(used in TWS P&L computations)
        ///     225 Auction values(volume, price and imbalance)
        ///     233 RTVolume - contains the last trade price, last trade size, last trade time, total volume, VWAP, and single trade flag.
        ///     236 Shortable
        ///     256 Inventory
        ///     258 Fundamental Ratios
        ///     411 Realtime Historical Volatility
        ///     456 IBDividends
        ///     375 RT Volume filtered for BarTable
        ///     
        /// https://interactivebrokers.github.io/tws-api/tick_types.html
        /// 
        /// Regulatory Snapshots:
        /// For stocks, there are individual exchange-specific market data subscriptions necessary to receive streaming quotes.
        /// For instance, for NYSE stocks this subscription is known as "Network A", for ARCA/AMEX stocks it is called "Network B" and for NASDAQ stocks it is "Network C".
        /// Each subscription is added a la carte and has a separate market data fee.
        /// 
        /// Alternatively, there is also a "US Securities Snapshot Bundle" subscription which does not provide streaming data but which allows for real time calculated snapshots of US market NBBO prices.
        /// By setting the 5th parameter in the function IBApi::EClient::reqMktData to True, a regulatory snapshot request can be made from the API.
        /// The returned value is a calculation of the current market state based on data from all available exchanges.
        /// 
        /// Important: Each regulatory snapshot made will incur a fee of 0.01 USD to the account. This applies to both live and paper accounts..
        /// If the monthly fee for regulatory snapshots reaches the price of a particular 'Network' subscription,
        /// the user will automatically be subscribed to that Network subscription for continuous streaming quotes and charged the associated fee for that month.
        /// At the end of the month the subscription will be terminated.
        /// Each listing exchange will be capped independently and will not be combined across listing exchanges.
        /// 
        /// https://interactivebrokers.github.io/tws-api/md_request.html#regulatory_snapshot
        /// 
        /// client.reqMktData(1004, ContractSamples.USStockAtSmart(), "233,236,258", false, false, null);
        /// (0)"1"-(1)"11"-(2)"10000002"-(3)"0"-(4)"AAPL"-(5)"STK"-(6)""-(7)"0"-(8)""-(9)""-(10)"SMART"-(11)"ISLAND"-(12)"USD"-(13)""-(14)""-(15)"0"-(16)""-(17)"0"-(18)"0"
        /// (0)"1"-(1)"11"-(2)"10000001"-(3)"0"-(4)"FB"  -(5)"STK"-(6)""-(7)"0"-(8)""-(9)""-(10)"SMART"-(11)"ISLAND"-(12)"USD"-(13)""-(14)""-(15)"0"-(16)""-(17)"0"-(18)"0"
        /// </summary>
        /// <param name="c"></param>
        /// <param name="genericTickList">"233,236,258"</param>
        /// <param name="snapshot">snapshot means a single request</param>
        /// <param name="regulatorySnaphsot">Regulatory Snapshots</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool SendRequest_MarketTicks(Contract c, string genericTickList = "236,375",
            bool snapshot = false, bool regulatorySnaphsot = false,
            ICollection<(string, string)> options = null)
        {
            var (valid_exchange, exchangeCode) = ApiCode.GetIbCode(c.Exchange);

            if (Connected && valid_exchange && !ActiveMarketTicks.Values.Contains(c) && !SubscriptionOverflow)
            {
                (int requestId, string requestType) = RegisterRequest(RequestType.RequestMarketData);
                c.MarketData.NetClient = this;
                c.MarketData.TickerId = requestId;
                ActiveMarketTicks.CheckAdd(requestId, c);

                bool useSmart = c is ITradable it && it.AutoExchangeRoute;
                string lastTradeDateOrContractMonth = "";
                double strike = 0;
                string right = "";
                string multiplier = "";

                if (c is IOption opt)
                {
                    lastTradeDateOrContractMonth = opt.LastTradeDateOrContractMonth;
                    strike = opt.Strike;
                    right = opt.Right;
                    multiplier = opt.Multiplier;
                }

                List<string> paramsList = new List<string>() {
                    requestType,
                    "11",
                    requestId.Param(),
                    c.ConId.Param(), // Contract id
                    c.Name, // Contract Symbol
                    c.TypeApiCode, // Contract SecType
                    lastTradeDateOrContractMonth, // Contract Date
                    (strike == 0) ? "0" : strike.ToString("0.0###"),
                    right, // Contract Right
                    multiplier, // Contract Multiplier
                    useSmart ? "SMART" : exchangeCode,  // "ISLAND", Contract Exchange, Specify the Primary Exchange attribute to avoid contract ambiguity
                    exchangeCode, // PrimaryExch,
                    c.CurrencyCode, //  "USD", // Contract Currency
                    string.Empty, // Contract Local Symnbol
                    string.Empty, // Contract Trading Class
                    //"0",
                    //genericTickList,
                    //snapshot.Param(),
                    //regulatorySnaphsot.Param(),
                    //options.Param(),
                };

                if (c is ICombo ic)
                {
                    if (ic.ComboLegs is null)
                    {
                        paramsList.Add("0");
                    }
                    else
                    {
                        paramsList.Add(ic.ComboLegs.Count.ParamPos());
                        foreach (ComboLeg leg in ic.ComboLegs)
                        {
                            paramsList.AddRange(new string[] {
                                leg.ConId.ParamPos(),
                                leg.Ratio.Param(),
                                leg.Action,
                                leg.Exchange,
                            });
                            /*
                            paramsList.Add(leg.ConId.ParamPos());
                            paramsList.Add(leg.Ratio.Param());
                            paramsList.Add(leg.Action);
                            paramsList.Add(leg.Exchange);*/
                        }
                    }
                }

                if (c is IDeltaNeutral dnc && !(dnc.DeltaNeutralContract is null))
                {
                    DeltaNeutralContract deltaNeutralContract = dnc.DeltaNeutralContract;
                    paramsList.AddRange(new string[] {
                        "1",
                        deltaNeutralContract.ConId.ParamPos(),
                        deltaNeutralContract.Delta.Param(),
                        deltaNeutralContract.Price.Param(),
                    });
                    /*
                    paramsList.Add("1"); // true
                    paramsList.Add(deltaNeutralContract.ConId.ParamPos());
                    paramsList.Add(deltaNeutralContract.Delta.Param());
                    paramsList.Add(deltaNeutralContract.Price.Param());*/
                }
                else
                {
                    paramsList.Add("0"); // 15
                }

                paramsList.AddRange(new string[] {
                    genericTickList,
                    snapshot.Param(),
                    regulatorySnaphsot.Param(),
                    options.Param(),
                });

                SendRequest(paramsList);
                return true;
            }

            return false;
        }

        /*
        public bool RequestMarketTicks(BagComboContract c, string genericTickList = "236",
            bool snapshot = false, bool regulatorySnaphsot = false, ICollection<(string, string)> marketQuoteOptions = null)
        {
            var (valid_type, secTypeCode) = ApiCode.GetIbCode(c.Type);
            var (valid_exchange, exchangeCode) = ApiCode.GetIbCode(c.Exchange);

            if (Connected && valid_type && valid_exchange && !ActiveTicks.Values.Contains(c))
            {
                (int tickerId, string typeStr) = RegisterRequest(RequestType.RequestMarketData);

                c.MarketData.TickerId = tickerId;
                ActiveTicks.CheckAdd(tickerId, c);

                List<string> paramsList = new List<string>() {
                    typeStr,
                    "11",
                    tickerId.ParamPos(),

                    c.ConId.Param(),
                    c.Name,
                    secTypeCode,
                    c.LastTradeDateOrContractMonth,
                    (c.Strike == 0) ? "0" : c.Strike.ToString("0.0###"),
                    c.Right,
                    c.Multiplier,
                    c.AutoExchangeRoute ? "SMART" : exchangeCode, // "ISLAND" exchange,
                    exchangeCode, // primaryExch,
                    c.CurrencyCode, // USD / currency,
                    c.LocalSymbol,
                    c.TradingClass
                };

                if (c.IsBAG)
                {
                    if (c.ComboLegs is null)
                    {
                        paramsList.Add("0");
                    }
                    else
                    {
                        paramsList.Add(c.ComboLegs.Count.ParamPos());
                        foreach (ComboLeg leg in c.ComboLegs)
                        {
                            paramsList.Add(leg.ConId.ParamPos());
                            paramsList.Add(leg.Ratio.Param());
                            paramsList.Add(leg.Action);
                            paramsList.Add(leg.Exchange);
                        }
                    }
                }

                if (c.DeltaNeutralContract is null)
                {
                    paramsList.Add("0"); // 15
                }
                else
                {
                    DeltaNeutralContract deltaNeutralContract = c.DeltaNeutralContract;
                    paramsList.Add("1"); // true
                    paramsList.Add(deltaNeutralContract.ConId.ParamPos());
                    paramsList.Add(deltaNeutralContract.Delta.Param());
                    paramsList.Add(deltaNeutralContract.Price.Param());
                }

                paramsList.Add(genericTickList); // 16
                paramsList.Add(snapshot.Param()); // 17
                paramsList.Add(regulatorySnaphsot.Param()); // 18
                paramsList.Add(marketQuoteOptions.Param()); // 19

                SendRequest(paramsList);
                return true;
            }
            return false;
        }
        */

        public bool SendCancel_MarketTicks(int requestId)
        {
            RemoveRequest(requestId, RequestType.RequestMarketData);
            lock (ActiveMarketTicks)
            {
                ActiveMarketTicks.TryRemove(requestId, out Contract c);
                c.MarketData.Status = MarketQuoteStatus.DelayedFrozen;
                c.MarketData.NetClient = null;
                c.MarketData.TickerId = -1;
            }
            // Emit update cancelled.
            return false;
        }


    }
}
