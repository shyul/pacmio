﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// https://interactivebrokers.github.io/tws-api/basic_contracts.html#fut
/// 
/// ***************************************************************************

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;
using Xu;
using Pacmio.IB;

namespace Pacmio
{
    // Send 39: (0)"39"-(1)"8"-(2)"60000001"-(3)"0"-(4)"ES"-(5)"FUT"-(6)"202009"-(7)"0"-(8)""-(9)""-(10)"GLOBEX"-(11)""-(12)"USD"-(13)""-(14)""-(15)"0"
    // Received ContractData: (0)"10"-(1)"8"-(2)"60000001"-(3)"ES"-(4)"FUT"-(5)"20200918 08:30 CST"-(6)"0"-(7)""-(8)"GLOBEX"-(9)"USD"-(10)"ESU0"-(11)"ES"-(12)"ES"-(13)"371749798"-(14)"0.25"-(15)"1"-(16)"50"-(17)"ACTIVETIM,AD,ADJUST,ALERT,ALGO,ALLOC,AVGCOST,BASKET,BENCHPX,COND,CONDORDER,DAY,DEACT,DEACTDIS,DEACTEOD,GAT,GTC,GTD,GTT,HID,ICE,IOC,LIT,LMT,LTH,MIT,MKT,MTL,NGCOMB,NONALGO,OCA,PEGBENCH,SCALE,SCALERST,SNAPMID,SNAPMKT,SNAPREL,STP,STPLMT,TRAIL,TRAILLIT,TRAILLMT,TRAILMIT,WHATIF"-(18)"GLOBEX,QBALGO"-(19)"1"-(20)"11004968"-(21)"E-mini S&P 500"-(22)""-(23)"202009"-(24)""-(25)""-(26)""-(27)"CST (Central Standard Time)"-(28)"20200621:1700-20200622:1515;20200622:1530-20200622:1600;20200622:1700-20200623:1515;20200623:1530-20200623:1600;20200623:1700-20200624:1515;20200624:1530-20200624:1600;20200624:1700-20200625:1515;20200625:1530-20200625:1600;20200625:1700-20200626:1515;20200626:1530-20200626:1600;20200627:CLOSED;20200628:1700-20200629:1515;20200629:1530-20200629:1600;20200629:1700-20200630:1515;20200630:1530-20200630:1600;20200630:1700-20200701:1515;20200701:1530-20200701:1600;20200701:1700-20200702:1515;20200702:1530-20200702:1600;20200702:1700-20200703:1200;20200704:CLOSED;20200705:1700-20200706:1515;20200706:1530-20200706:1600;20200706:1700-20200707:1515;20200707:1530-20200707:1600;20200707:1700-20200708:1515;20200708:1530-20200708:1600;20200708:1700-20200709:1515;20200709:1530-20200709:1600;20200709:1700-20200710:1515;20200710:1530-20200710:1600;20200711:CLOSED;20200712:1700-20200713:1515;20200713:1530-20200713:1600;20200713:1700-20200714:1515;20200714:1530-20200714:1600;20200714:1700-20200715:1515;20200715:1530-20200715:1600;20200715:1700-20200716:1515;20200716:1530-20200716:1600;20200716:1700-20200717:1515;20200717:1530-20200717:1600;20200718:CLOSED;20200719:1700-20200720:1515;20200720:1530-20200720:1600;20200720:1700-20200721:1515;20200721:1530-20200721:1600;20200721:1700-20200722:1515;20200722:1530-20200722:1600;20200722:1700-20200723:1515;20200723:1530-20200723:1600;20200723:1700-20200724:1515;20200724:1530-20200724:1600;20200725:CLOSED;20200726:CLOSED"-(29)"20200622:0830-20200622:1515;20200622:1530-20200622:1600;20200623:0830-20200623:1515;20200623:1530-20200623:1600;20200624:0830-20200624:1515;20200624:1530-20200624:1600;20200625:0830-20200625:1515;20200625:1530-20200625:1600;20200626:0830-20200626:1515;20200626:1530-20200626:1600;20200627:CLOSED;20200628:CLOSED;20200629:0830-20200629:1515;20200629:1530-20200629:1600;20200630:0830-20200630:1515;20200630:1530-20200630:1600;20200701:0830-20200701:1515;20200701:1530-20200701:1600;20200702:0830-20200702:1515;20200702:1530-20200702:1600;20200703:0830-20200703:1515;20200703:1530-20200703:1600;20200704:CLOSED;20200705:CLOSED;20200706:0830-20200706:1515;20200706:1530-20200706:1600;20200707:0830-20200707:1515;20200707:1530-20200707:1600;20200708:0830-20200708:1515;20200708:1530-20200708:1600;20200709:0830-20200709:1515;20200709:1530-20200709:1600;20200710:0830-20200710:1515;20200710:1530-20200710:1600;20200711:CLOSED;20200712:CLOSED;20200713:0830-20200713:1515;20200713:1530-20200713:1600;20200714:0830-20200714:1515;20200714:1530-20200714:1600;20200715:0830-20200715:1515;20200715:1530-20200715:1600;20200716:0830-20200716:1515;20200716:1530-20200716:1600;20200717:0830-20200717:1515;20200717:1530-20200717:1600;20200718:CLOSED;20200719:CLOSED;20200720:0830-20200720:1515;20200720:1530-20200720:1600;20200721:0830-20200721:1515;20200721:1530-20200721:1600;20200722:0830-20200722:1515;20200722:1530-20200722:1600;20200723:0830-20200723:1515;20200723:1530-20200723:1600;20200724:0830-20200724:1515;20200724:1530-20200724:1600;20200725:CLOSED;20200726:CLOSED"-(30)""-(31)""-(32)"0"-(33)"2147483647"-(34)"ES"-(35)"IND"-(36)"67,67"-(37)"20200918"
    // Received ContractDataEnd: (0)"52"-(1)"1"-(2)"60000001"

    /*
        Contract contract = new Contract();
        contract.Symbol = "ES";
        contract.SecType = "FUT";
        contract.Exchange = "GLOBEX";
        contract.Currency = "USD";
        contract.LastTradeDateOrContractMonth = "202009";
     */

    [Serializable, DataContract(Name = "Future")]
    public class Future : Contract, ITradable
    {
        [IgnoreDataMember, Browsable(true), ReadOnly(true), DisplayName("Security Type")]
        public override string TypeName => "FUTURE";

        [IgnoreDataMember, Browsable(true), ReadOnly(true), DisplayName("Security Type Full Name")]
        public override string TypeFullName => "Future";

        [IgnoreDataMember, Browsable(false), ReadOnly(true)]
        public override string TypeApiCode => "FUT";

        public string ISIN { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AutoExchangeRoute { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public bool Equals(BusinessInfo other)
        {
            throw new NotImplementedException();
        }

        public (bool valid, BusinessInfo bi) GetBusinessInfo()
        {
            throw new NotImplementedException();
        }
    }
}

