﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// https://interactivebrokers.github.io/tws-api/basic_contracts.html#stk
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using Xu;

namespace Pacmio
{
    /// <summary>
    /// Inflated Symbol with more information
    /// This type is to be serialized into file blobs
    /// </summary>
    [Serializable, DataContract(Name = "Stock")]
    public class Stock : Contract, IBusiness
    {
        #region Ctor

        public Stock(string name, Exchange exchange)
        {
            Name = name;
            Exchange = exchange;
            //m_StockData = new StockData();
        }

        [IgnoreDataMember, Browsable(true), ReadOnly(true), DisplayName("Security Type")]
        public override string TypeName => "STOCK";

        [IgnoreDataMember, Browsable(true), ReadOnly(true), DisplayName("Security Type Full Name")]
        public override string TypeFullName => "Stock";
        /*
        [DataMember]
        public bool IsETF { get; set; } = false;
        */
        #endregion Ctor

        [IgnoreDataMember]
        public override bool NeedUpdate => (DateTime.Now - UpdateTime).Days > 2 && (base.NeedUpdate || ISIN.Length < 2);

        #region Identification

        [IgnoreDataMember, Browsable(true), DisplayName("Full Name")]
        public override string FullName
        {
            get
            {
                if (BusinessInfo is BusinessInfo bi)
                    if (bi.FullName.Length > 3)
                        m_fullName = bi.FullName;
                    else
                    {
                        bi.FullName = m_fullName;
                        bi.IsModified = true;
                    }

                return m_fullName;
            }
            set
            {

                m_fullName = value;

                if (BusinessInfo is BusinessInfo bi) 
                {
                    BusinessInfo.FullName = m_fullName;
                    BusinessInfo.IsModified = true;
                }
            }
        }

        [DataMember, Browsable(true), Category("IDs"), DisplayName("ISIN")]
        public string ISIN { get; set; } = string.Empty;

        [IgnoreDataMember]
        public BusinessInfo BusinessInfo
        {
            get
            {
                if (m_BusinessInfo is null)
                    m_BusinessInfo = BusinessInfoManager.GetOrCreateBusinessInfo(ISIN);

                return m_BusinessInfo;
            }
        }

        [IgnoreDataMember]
        private BusinessInfo m_BusinessInfo = null;

        [DataMember]
        public string Industry { get; set; }

        [DataMember]
        public string Category { get; set; }

        [DataMember]
        public string Subcategory { get; set; }

        #endregion Identification

        #region Order and Trade Data



        #endregion Order and Trade Data

        #region Equality

        public bool Equals(BusinessInfo other) => ISIN == other.ISIN;

        public static bool operator ==(Stock s1, BusinessInfo s2) => s1.Equals(s2);
        public static bool operator !=(Stock s1, BusinessInfo s2) => !s1.Equals(s2);

        public override bool Equals(object obj) => base.Equals(obj);

        public override int GetHashCode() => Key.GetHashCode();

        #endregion Equality
    }
}
