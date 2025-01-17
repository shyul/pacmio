﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// BarTable Data Types
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xu;
using Xu.Chart;
using Pacmio.Analysis;

namespace Pacmio
{
    public enum BarType : int
    {
        White = 2,
        Black = 1,
        None = 0,
        Red = -2,
        HollowRed = -1
    }

    /// <summary>
    /// One Single Bar is not as simple as it sounds.
    /// </summary>
    public class Bar
    {
        #region Ctor

        public Bar(BarTable bt, DateTime time)
        {
            Table = bt;
            Period = Frequency.AlignPeriod(time);
            DataSourcePeriod = new Period(time);
        }

        public Bar(BarTable bt, Bar small_b)
        {
            Table = bt;

            if (BarFreq >= small_b.BarFreq) // Merge From A Smaller Bar
            {
                Period = Frequency.AlignPeriod(small_b.Time);
                DataSourcePeriod = new Period(small_b.DataSourcePeriod);
                Source = small_b.Source;
                Open = small_b.Open;
                High = small_b.High;
                Low = small_b.Low;
                Close = small_b.Close;
                Volume = small_b.Volume;
            }
            else
                throw new Exception("Source Bar has to be at smaller time frame!");
        }

        public Bar(BarTable bt, DateTime time, double last, double volume)
        {
            Table = bt;
            Period = Frequency.AlignPeriod(time);
            DataSourcePeriod = new Period(time);
            Source = DataSourceType.Tick;
            Open = High = Low = Close = last;
            Volume = volume;
        }

        public Bar(BarTable bt, Period pd, double open, double high, double low, double close, double volume, DataSourceType source = DataSourceType.Tick)
        {
            Table = bt;
            Period = pd;
            DataSourcePeriod = new Period(pd.Start);
            Source = source;

            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }

        public Bar(BarTable bt, DateTime time, double open, double high, double low, double close, double volume, DataSourceType source = DataSourceType.Tick)
        {
            Table = bt;
            Period = Frequency.AlignPeriod(time);

            if (Period.Start != time)
            {
                Console.WriteLine("time = " + time + " | Period Start = " + Period.Start);
            }

            DataSourcePeriod = new Period(time);
            Source = source;

            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }

        public override int GetHashCode() => Table.GetHashCode() ^ Time.GetHashCode();

        /// <summary>
        /// BarTable this Bar belongs to. And unable to change through the entire life cycle of the Bar.
        /// </summary>
        public BarTable Table { get; }

        /// <summary>
        /// BarSize of the Bar
        /// </summary>
        public BarFreq BarFreq => Table.BarFreq;

        public PriceType PriceType => Table.PriceType;

        /// <summary>
        /// Attached to the Table's frequency.
        /// </summary>
        public Frequency Frequency => Table.Frequency;

        #endregion Ctor

        #region Time and Period Info

        public int Index { get; set; } = -1;

        /// <summary>
        /// Start Time of the Bar
        /// </summary>
        // Need to get Period depending on the time and BarSize
        public DateTime Time => Period.Start;

        /// <summary>
        /// The time period of this Bar (Every OHLC is from a period of time)
        /// </summary>
        public Period Period { get; }

        /// <summary>
        /// The actual period according from the datasource
        /// For identifing partial bar (If the Period "Contains" and "Wider" than DataSourcePeriod)
        /// </summary>
        public Period DataSourcePeriod { get; private set; }

        public Bar Bar_1 => Index > 1 ? Table[Index - 1] : null;

        #endregion

        #region Original

        public bool IsValid => Source != DataSourceType.Invalid;

        /// <summary>
        /// 
        /// </summary>
        public DataSourceType Source { get; set; } = DataSourceType.Invalid;

        public double Open { get; set; } = -1;

        public double High { get; set; } = -1;

        public double Low { get; set; } = -1;

        public double Close { get; set; } = -1;

        public double Volume { get; set; } = -1;

        public static NumericColumn Column_Open { get; } = new NumericColumn("OPEN", "O");
        public static NumericColumn Column_High { get; } = new NumericColumn("HIGH", "H");
        public static NumericColumn Column_Low { get; } = new NumericColumn("LOW", "L");
        public static NumericColumn Column_Close { get; } = new NumericColumn("CLOSE", "CLOSE");
        public static NumericColumn Column_Volume { get; } = new NumericColumn("VOLUME", string.Empty);

        public void Copy(Bar b)
        {
            if (b.Table != Table || b.Time != Time || b.Period != Period)
                throw new("bar's table has to match with this table!");

            ClearAllCalculationData();
            Source = b.Source;
            Open = b.Open;
            High = b.High;
            Low = b.Low;
            Close = b.Close;
            Volume = b.Volume;
            DataSourcePeriod = b.DataSourcePeriod;
        }

        public bool Contains(double price) => price >= Low && price <= High;

        public void ClearAllCalculationData()
        {
            Type = BarType.None;
            Gain = double.NaN;
            GainPercent = double.NaN;
            Gap = double.NaN;
            GapPercent = double.NaN;
            Typical = double.NaN;
            Range = double.NaN;
            TrueRange = double.NaN;
            NarrowRange = 0;
            TrendStrength = 0;
            Pivot = 0;
            PivotStrength = 0;

            NumericColumnsLUT.Clear();
            DatumColumnsLUT.Clear();
        }

        #endregion Original

        #region Smaller Bars

        public bool MergeFromSmallerBar(Bar smaller_b)
        {
            bool isModified = false;

            if (smaller_b.BarFreq <= BarFreq)
            {
                if (smaller_b.High > High) // New High
                {
                    High = smaller_b.High;
                    isModified = true;
                }

                if (smaller_b.Low < Low) // New Low
                {
                    Low = smaller_b.Low;
                    isModified = true;
                }

                // Smaller Bar's Stop is even earlier than this Bar's start, smaller_b.Source <= Source && ???
                if (smaller_b.DataSourcePeriod.Stop < DataSourcePeriod.Start && smaller_b.DataSourcePeriod.Start > Period.Start)
                {
                    Open = smaller_b.Open;
                    Volume += smaller_b.Volume;
                    DataSourcePeriod.Insert(smaller_b.DataSourcePeriod.Start);
                    isModified = true;
                }

                // Smaller Bar's Start is even later than this Bar's stop
                if (smaller_b.DataSourcePeriod.Start >= DataSourcePeriod.Stop)
                {
                    Close = smaller_b.Close;
                    Volume += smaller_b.Volume;
                    DataSourcePeriod.Insert(smaller_b.DataSourcePeriod.Stop);
                    isModified = true;
                }

                if (Source < smaller_b.Source) Source = smaller_b.Source; // Worse Source
            }
            else
            {
                throw new Exception("Can't merge from larger Bar!!");
            }

            return isModified;
        }

        #endregion Smaller Bars

        #region Intrinsic Indicators | NativeGainAnalysis

        public BarType Type
        {
            get => m_Type;

            set
            {
                m_Type = value;
                if (Bar_1 is Bar b_1)
                {
                    if (b_1.Type == m_Type)
                    {
                        ConsecutiveType = b_1.ConsecutiveType + 1;
                    }
                }
            }
        }

        private BarType m_Type = BarType.None;

        public int ConsecutiveType { get; private set; } = 1;

        public double Gain { get; set; } = double.NaN;

        public double GainPercent { get; set; } = double.NaN;

        public double Gap { get; set; } = double.NaN;

        public double GapPercent { get; set; } = double.NaN;

        public double Typical { get; set; } = double.NaN;

        public double Body { get; set; } = double.NaN;

        public double BodyRatio { get; set; } = double.NaN;

        public double Range { get; set; } = double.NaN;

        public double TrueRange { get; set; } = double.NaN;

        public int NarrowRange { get; set; } = 0;

        public int TrendStrength { get; set; } = 0;

        public static NumericColumn Column_ConsecutiveType { get; } = new NumericColumn("ConsecutiveType", "ConsecutiveType");
        public static NumericColumn Column_Gain { get; } = new NumericColumn("GAIN", "GAIN");
        public static NumericColumn Column_GainPercent { get; } = new NumericColumn("GAINPERCENT", "G%");
        public static NumericColumn Column_Gap { get; } = new NumericColumn("GAP", "GAP");
        public static NumericColumn Column_GapPercent { get; } = new NumericColumn("GAPPERCENT", "GAP%");
        public static NumericColumn Column_Typical { get; } = new NumericColumn("TYPICAL", "TYPICAL");
        public static NumericColumn Column_Body { get; } = new NumericColumn("BODY", "BODY");
        public static NumericColumn Column_BodyRatio { get; } = new NumericColumn("BodyRatio", "BR");
        public static NumericColumn Column_Range { get; } = new NumericColumn("RANGE", "RANGE");
        public static NumericColumn Column_TrueRange { get; } = new NumericColumn("TRUERANGE", "TR");
        public static NumericColumn Column_NarrowRange { get; } = new NumericColumn("NARROWRANGE", "NR");
        public static NumericColumn Column_TrendStrength { get; } = new NumericColumn("TrendStrength", "TS");

        #endregion Intrinsic Indicators | NativeGainAnalysis

        #region Intrinsic Indicators | NativePivotAnalysis

        public int Pivot { get; set; }

        public int PivotStrength { get; set; }

        public static NumericColumn Column_Pivot { get; } = new NumericColumn("PIVOT", "PVT");
        public static NumericColumn Column_PivotStrength { get; } = new NumericColumn("PivotStrength", "PVTS");

        #endregion Intrinsic Indicators | NativePivotAnalysis

        #region Numeric Column

        /// <summary>
        /// BarAnalysis Data Line
        /// </summary>
        // set is not allowed// One column only has one data
        private Dictionary<NumericColumn, double> NumericColumnsLUT { get; } = new Dictionary<NumericColumn, double>();

        public double this[NumericColumn column]
        {
            get
            {
                return column switch
                {
                    NumericColumn dc when dc == Column_Open => Open,
                    NumericColumn dc when dc == Column_High => High,
                    NumericColumn dc when dc == Column_Low => Low,
                    NumericColumn dc when dc == Column_Close => Close,
                    NumericColumn dc when dc == Column_Volume => Volume,

                    NumericColumn dc when dc == Column_Gain => Gain,
                    NumericColumn dc when dc == Column_GainPercent => GainPercent,
                    NumericColumn dc when dc == Column_Gap => Gap,
                    NumericColumn dc when dc == Column_GapPercent => GapPercent,

                    NumericColumn dc when dc == Column_Typical => Typical,
                    NumericColumn dc when dc == Column_Body => Body,
                    NumericColumn dc when dc == Column_BodyRatio => BodyRatio,
                    NumericColumn dc when dc == Column_Range => Range,
                    NumericColumn dc when dc == Column_TrueRange => TrueRange,
                    NumericColumn dc when dc == Column_NarrowRange => NarrowRange,
                    NumericColumn dc when dc == Column_TrendStrength => TrendStrength,

                    NumericColumn dc when dc == Column_ConsecutiveType => ConsecutiveType,

                    NumericColumn dc when dc == Column_Pivot => Pivot,
                    NumericColumn dc when dc == Column_PivotStrength => PivotStrength,

                    NumericColumn ic when NumericColumnsLUT.ContainsKey(ic) => NumericColumnsLUT[ic],

                    _ => double.NaN,
                };
            }
            set
            {
                if (double.IsNaN(value) && NumericColumnsLUT.ContainsKey(column))
                    NumericColumnsLUT.Remove(column);
                else
                    switch (column)
                    {
                        case NumericColumn dc when dc == Column_Open: Open = value; break;
                        case NumericColumn dc when dc == Column_High: High = value; break;
                        case NumericColumn dc when dc == Column_Low: Low = value; break;
                        case NumericColumn dc when dc == Column_Close: Close = value; break;
                        case NumericColumn dc when dc == Column_Volume: Volume = value; break;

                        case NumericColumn dc when dc == Column_Gain: Gain = value; break;
                        case NumericColumn dc when dc == Column_GainPercent: GainPercent = value; break;
                        case NumericColumn dc when dc == Column_Gap: Gap = value; break;
                        case NumericColumn dc when dc == Column_GapPercent: GapPercent = value; break;

                        case NumericColumn dc when dc == Column_Typical: Typical = value; break;
                        case NumericColumn dc when dc == Column_Body: Body = value; break;
                        case NumericColumn dc when dc == Column_BodyRatio: BodyRatio = value; break;
                        case NumericColumn dc when dc == Column_Range: Range = value; break;
                        case NumericColumn dc when dc == Column_TrueRange: TrueRange = value; break;
                        case NumericColumn dc when dc == Column_NarrowRange: NarrowRange = value.ToInt32(); break;
                        case NumericColumn dc when dc == Column_TrendStrength: TrendStrength = value.ToInt32(); break;

                        case NumericColumn dc when dc == Column_ConsecutiveType: break;

                        case NumericColumn dc when dc == Column_Pivot: Pivot = value.ToInt32(); break;
                        case NumericColumn dc when dc == Column_PivotStrength: PivotStrength = value.ToInt32(); break;

                        default:
                            if (!NumericColumnsLUT.ContainsKey(column))
                                NumericColumnsLUT.Add(column, value);
                            else
                                NumericColumnsLUT[column] = value;
                            break;
                    }
            }
        }

        public double this[ISingleData isd]
        {
            get
            {
                if (isd is BarAnalysis) // ba)
                {
                    // if (ba.BarFreq == BarFreq && ba.PriceType == PriceType)
                    return this[isd.Column_Result];
                    // else
                    //    return Table.BarTableSet[Time, ba.BarFreq, ba.PriceType][isd.Column_Result];
                }
                else
                    return double.NaN;
            }

            set
            {
                if (isd is BarAnalysis) // ba && ba.BarFreq == BarFreq && ba.PriceType == PriceType)
                    this[isd.Column_Result] = value;
            }
        }

        public (double high, double low) this[IDualData idd]
        {
            get
            {
                if (idd is BarAnalysis) // ba)
                {
                    // if (ba.BarFreq == BarFreq && ba.PriceType == PriceType)
                    return (this[idd.Column_High], this[idd.Column_Low]);
                    // else
                    //     return Table.BarTableSet[Time, ba.BarFreq, ba.PriceType][idd];
                }
                else
                    return (double.NaN, double.NaN);
            }

            set
            {
                if (idd is BarAnalysis) // && ba.BarFreq == BarFreq && ba.PriceType == PriceType)
                {
                    this[idd.Column_High] = value.high;
                    this[idd.Column_Low] = value.low;
                }
            }
        }

        #endregion Numeric Column

        #region Datum Column

        private Dictionary<DatumColumn, IDatum> DatumColumnsLUT { get; } = new Dictionary<DatumColumn, IDatum>();

        public IDatum this[DatumColumn dc]
        {
            get => DatumColumnsLUT.ContainsKey(dc) ? DatumColumnsLUT[dc] : null;

            set
            {
                if (value is IDatum dat && value.GetType() == dc.DatumType)
                {
                    DatumColumnsLUT[dc] = dat;
                }
                else if (value is null && DatumColumnsLUT.ContainsKey(dc))
                {
                    DatumColumnsLUT.Remove(dc);
                }
                else
                {
                    throw new("Invalid data type assigned");
                }
            }
        }

        public IDatum this[ISingleDatum isd]
        {
            get
            {
                if (isd is BarAnalysis)// ba)
                {
                    // if (ba.BarFreq == BarFreq && ba.PriceType == PriceType)
                    return this[isd.Column_Result];
                    // else
                    //     return Table.BarTableSet[Time, ba.BarFreq, ba.PriceType][isd.Column_Result];
                }
                else
                    return null;
            }

            set
            {
                if (isd is BarAnalysis)// ba && ba.BarFreq == BarFreq && ba.PriceType == PriceType)
                    this[isd.Column_Result] = value;
            }
        }

        public PatternDatum this[PatternAnalysis pa]
        {
            get
            {
                //if (pa.BarFreq == BarFreq && pa.PriceType == PriceType)
                return this[pa.Column_Result] as PatternDatum;
                //else
                //   return Table.BarTableSet[Time, pa.BarFreq, pa.PriceType][pa.Column_Result] as PatternDatum;
            }

            set
            {
                //if (pa.BarFreq == BarFreq && pa.PriceType == PriceType)
                this[pa.Column_Result] = value;
            }
        }

        #endregion Datum Column

        #region Signal Information Tools

        public IEnumerable<CandleStickType> CandleStickList => DatumColumnsLUT.Values.Where(n => n is CandleStickSignalDatum).Select(n => n as CandleStickSignalDatum).SelectMany(n => n.List);

        private SignalDatum this[SignalColumn dc]
        {
            get => DatumColumnsLUT.ContainsKey(dc) ? DatumColumnsLUT[dc] as SignalDatum : null;

            set
            {
                if (value is SignalDatum dat && value.GetType() == dc.DatumType)
                {
                    if (DatumColumnsLUT[dc] is not null)
                    {
                        // We may need to dispose the old the signalDatum
                    }

                    DatumColumnsLUT[dc] = dat;
                }
                else if (value is null && DatumColumnsLUT.ContainsKey(dc))
                {
                    DatumColumnsLUT.Remove(dc);
                }
                else
                {
                    throw new("Invalid data type assigned");
                }
            }
        }

        public SignalDatum this[SignalAnalysis sa]
        {
            get
            {
                if (sa.BarFreq == BarFreq && sa.PriceType == PriceType)
                    return this[sa.Column_Result];
                else
                    return Table.BarTableSet[Time, sa];
            }

            set
            {
                if (sa.BarFreq == BarFreq && sa.PriceType == PriceType)
                    this[sa.Column_Result] = value;
            }
        }

        public (double Bullish, double Bearish) GetSignalScore(IEnumerable<SignalAnalysis> sas)
        {
            BarTableSet bts = Table.BarTableSet;
            double bullish = 0, bearish = 0;
            foreach (SignalAnalysis sa in sas.Where(n => n.BarFreq >= BarFreq))
            {
                if (this[sa] is SignalDatum sd)
                {
                    double points = sd.Points;
                    if (points > 0) bullish += points;
                    else if (points < 0) bearish += points;
                }
            }
            return (bullish, bearish);
        }

        #endregion Signal Information Tools

        public double this[FilterAnalysis filter]
        {
            get
            {
                if (filter.BarFreq == BarFreq && filter.PriceType == PriceType)
                    return this[filter.Column_Result];
                else
                    return Table.BarTableSet[Time, filter.BarFreq, filter.PriceType][filter];
            }
        }

        public StrategyDatum this[Strategy s]
        {
            get
            {
                if (DatumColumnsLUT.ContainsKey(s.Column_Result) &&
                    DatumColumnsLUT[s.Column_Result] is StrategyDatum sd)
                {
                    return sd;
                }
                else
                {
                    StrategyDatum new_sd = new(this, s);
                    DatumColumnsLUT[s.Column_Result] = new_sd;
                    return new_sd;
                }
            }
        }
    }
}
