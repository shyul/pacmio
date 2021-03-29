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
using System.Threading;
using System.Threading.Tasks;
using Xu;
using Pacmio.Analysis;

namespace Pacmio
{
    /// <summary>
    /// BarTable: the ultimate data holder for technical analysis with fundamental awareness
    /// </summary>
    public sealed class BarTable :
        IDatumTable,
        IDataProvider,
        IEquatable<BarTable>,
        IEquatable<BarDataFile>,
        IEquatable<(Contract, BarFreq, DataType)>,
        IEquatable<((string name, Exchange exchange, string typeName) ContractKey, BarFreq BarFreq, DataType Type)>
    {
        #region Ctor

        public BarTable(Contract c, BarFreq barFreq, DataType type, bool adjustDividend = false)
        {
            Contract = c;
            BarFreq = barFreq;
            Frequency = BarFreq.GetAttribute<BarFreqInfo>().Frequency;
            Type = type;
            BarTableSet = new BarTableSet(c, adjustDividend);

            CalculateTickCancelTs = new CancellationTokenSource();
            CalculateTickTask = new Task(() => CalculateTickWorker(), CalculateTickCancelTs.Token);
            CalculateTickTask.Start();
        }

        public BarTable(BarTableSet bts, BarFreq barFreq, DataType type)
        {
            BarTableSet = bts;
            Contract = bts.Contract;
            BarFreq = barFreq;
            Frequency = BarFreq.GetAttribute<BarFreqInfo>().Frequency;
            Type = type;

            CalculateTickCancelTs = new CancellationTokenSource();
            CalculateTickTask = new Task(() => CalculateTickWorker(), CalculateTickCancelTs.Token);
            CalculateTickTask.Start();
        }

        ~BarTable() => Dispose();

        public void Dispose()
        {
            lock (DataConsumers)
            {
                foreach (IDataRenderer idr in DataConsumers)
                {
                    RemoveDataConsumer(idr);
                    if (idr is BarChart bc)
                        bc.Close();
                    else
                        RemoveDataConsumer(idr);
                }
            }

            CalculateTickCancelTs.Cancel();
            Clear();
            GC.Collect();
        }

        public override string ToString() => Name + " | LastTime = " + (BarFreq >= BarFreq.Daily ? LastTime.ToString("MM-dd-yyyy") : LastTimeBound.ToString("dd, HH:mm:ss")) + " | Count = " + Count + " | LastClose = " + LastClose;

        public string Name => Contract.TypeFullName + ": " + Contract.Name + " (" + Contract.ExchangeName +
                " / " + Contract.CurrencySymbol + Contract.CurrencyCode + " / " + Frequency + ")";

        //public bool IsLive => BarTableSet.IsLive;

        public bool AdjustDividend => BarTableSet.AdjustDividend;

        /// <summary>
        /// BarTableSet this BarTable belongs to: specified for multi-time frame access.
        /// </summary>
        public BarTableSet BarTableSet { get; }

        public Contract Contract { get; }

        public BarFreq BarFreq { get; }

        public Frequency Frequency { get; }

        public DataType Type { get; }

        public ((string name, Exchange exchange, string typeName) ContractKey, BarFreq barFreq, DataType type) Key => (Contract.Key, BarFreq, Type);

        #endregion Ctor

        #region Data

        /// <summary>
        /// For Multi Thread Access
        /// </summary>
        public object DataLockObject { get; } = new();

        /// <summary>
        /// The Rows Data Storage
        /// The Storage is not directly accessible outside of the class.
        /// </summary>
        private List<Bar> Rows { get; } = new();

        private Dictionary<DateTime, int> TimeToRows { get; } = new();

        /// <summary>
        /// Returns the number of the Rows in the BarTable.
        /// </summary>
        public int Count => Rows.Count;

        /// <summary>
        /// Returns if the BarTable is has no Bars.
        /// </summary>
        public bool IsEmpty => Count < 1;

        /// <summary>
        /// Returns Last Row's Index
        /// </summary>
        private int LastIndex => Count - 1;

        #region Access Bars

        /// <summary>
        /// Lookup Bar by Index. Mostly used in the Chart.
        /// </summary>
        /// <param name="i">Index of the Bar in the Rows</param>
        /// <returns>Bar according to the given index</returns>
        //public Bar this[int i] => i >= Count || i < 0 ? null : Rows[i];
        public Bar this[int i] => Rows[i];

        /// <summary>
        /// Lookup Bar by Time. Time is rounded to the closest next time in the Rows.
        /// </summary>
        /// <param name="time">time of the Bar</param>
        /// <returns>Bar closest to the given time</returns>
        public Bar this[DateTime time]
        {
            get
            {
                time = Frequency.Align(time);
                lock (DataLockObject)
                {
                    if (TimeToRows.ContainsKey(time))
                        return Rows[TimeToRows[time]];
                    else
                        return null; // Rows.Where(n => n.Period.Contains(time)).FirstOrDefault();
                }
            }
        }

        public IEnumerable<Bar> Bars => Rows;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">Start and included index</param>
        /// <param name="stop">End and not included index</param>
        /// <returns></returns>
        public List<Bar> this[int i, int count] => Rows.Last(i, count);

        public List<Bar> this[Period pd]
        {
            get
            {
                lock (DataLockObject)
                {
                    return Rows.Where(n => pd.Contains(n.Time)).OrderBy(n => n.Time).ToList();
                }
            }
        }

        public double this[int i, NumericColumn column] => i >= Count || i < 0 ? double.NaN : Rows[i][column]; //   this[i] is Bar b ? b[column] : double.NaN;

        public IDatum this[int i, DatumColumn column] => i >= Count || i < 0 ? null : Rows[i][column]; // this[i] is Bar b ? b[column] : null;

        public double LastClose
        {
            get
            {
                lock (DataLockObject)
                {
                    return Count > 0 ? Rows.Last().Close : -1;
                }
            }
        }

        #endregion Access Bars

        #region Time

        /// <summary>
        /// Returns if the Rows already has time stamp
        /// </summary>
        public bool Contains(DateTime time) //=> Rows.Where(n => n.Time == time).Count() > 0;
        {
            lock (DataLockObject)
            {
                return TimeToRows.ContainsKey(time);
            }
        }

        /// <summary>
        /// Returns the earlies Row's time
        /// </summary>
        public DateTime FirstTime
        {
            get
            {
                lock (DataLockObject)
                {
                    return Count > 0 ? this[0].Time : DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Returns Last Row's time
        /// </summary>
        public DateTime LastTime
        {
            get
            {
                lock (DataLockObject)
                {
                    return Count > 0 ? TimeToRows.Last().Key : DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Last Most time including the Bar Period
        /// </summary>
        public DateTime LastTimeBound
        {
            get
            {
                lock (DataLockObject)
                {
                    return Count > 0 ? Rows.Last().DataSourcePeriod.Stop : DateTime.MinValue;
                }
            }
        }

        public bool IsActiveToday => LastTime.Date == Contract.CurrentTime.Date || (!Contract.IsTrading && LastTime.Date == Contract.LatestClosingDateTime.Date);

        /// <summary>
        /// Returns current BarTable's maximum time span
        /// </summary>
        public Period Period => new(FirstTime, LastTimeBound);

        /// <summary>
        /// Last Time by specific data source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public DateTime LastTimeBy(DataSourceType source)
        {
            lock (DataLockObject)
            {
                var res = Rows.Where(n => n.Source <= source).OrderBy(n => n.Time);
                return (res.Count() > 0) ? res.Last().Time : DateTime.MinValue;
            }
        }

        public DateTime IndexToTime(int i)
        {
            lock (DataLockObject)
            {
                if (i < 0)
                {
                    return FirstTime - Frequency * (-i);
                }
                else if (i >= Count)
                {
                    return LastTime + Frequency * (i - Count + 1);
                }
                else
                {
                    return this[i].Time;
                }
            }
        }

        /// <summary>
        /// Get the Index from a given time
        /// </summary>
        /// <param name="time">time is rectified to the index found in "M_Rows"</param>
        /// <returns>Index of the nearest time</returns>
        public int IndexOf(ref DateTime time)
        {
            lock (DataLockObject)
            {
                if (Count > 0)
                {
                    DateTime t = time;

                    if (TimeToRows.ContainsKey(t))
                    {
                        return TimeToRows[t];
                    }
                    else if (time <= FirstTime)
                    {
                        time = FirstTime;
                        return 0;
                    }
                    else if (time >= LastTime)
                    {
                        time = LastTime;
                        return LastIndex;
                    }
                    else if (Rows.Where(n => n.Period.Contains(t)).FirstOrDefault() is Bar b)
                    {
                        time = b.Time;
                        return b.Index;
                    }
                }
            }

            return -1;
        }

        #endregion Time

        #endregion Data

        #region Load Bars

        public void Clear()
        {
            Status = TableStatus.Default;

            lock (DataLockObject)
            {
                TimeToRows.Clear();
                Rows.Clear();
                ResetCalculationPointer();
            }
        }

        private void LoadBars(List<Bar> sorted_bars)
        {
            if (sorted_bars.Count > 0)
            {
                if (this != sorted_bars.FirstOrDefault().Table)
                    throw new("bar's table has to match with this table!");

                Status = TableStatus.Loading;

                lock (DataLockObject)
                {
                    TimeToRows.Clear();
                    Rows.Clear();
                    ResetCalculationPointer();

                    for (int i = 0; i < sorted_bars.Count; i++)
                    {
                        Bar b = sorted_bars[i];
                        b.Index = i;
                        Rows.Add(b);
                        TimeToRows.Add(b.Time, i);
                    }
                }

                Status = TableStatus.DataReady;
            }
        }

        public void LoadBars(MultiPeriod mp, bool adjustDividend, CancellationTokenSource cts = null)
        {
            if (mp is not null)
            {
                BarDataFile bdf = Contract.GetBarDataFile(BarFreq, Type);
                foreach (var period in mp)
                {
                    Period pd = new Period(period);
                    DateTime newStart = pd.Start.AddSeconds(-1000 * Frequency.Span.TotalSeconds);
                    pd.Insert(newStart);
                    bdf.Fetch(pd, cts);
                }

                var sorted_list = bdf.LoadBars(this, mp, adjustDividend);
                LoadBars(sorted_list);
            }
        }

        public void LoadBars(BarDataFile bdf, bool adjustDividend)
        {
            var sorted_list = bdf.LoadBars(this, adjustDividend);
            LoadBars(sorted_list);
        }

        public void LoadFromSmallerBar(List<Bar> sorted_bars)
        {
            if (sorted_bars.Count > 0)
            {
                BarTable bt = sorted_bars.FirstOrDefault().Table;
                if (Contract != bt.Contract || Type != bt.Type)
                    throw new("bar's table has to match with this table!");

                Status = TableStatus.Loading;

                lock (DataLockObject)
                {
                    TimeToRows.Clear();
                    Rows.Clear();
                    ResetCalculationPointer();

                    int j = 0;
                    for (int i = 0; i < sorted_bars.Count; i++)
                    {
                        Bar sb = sorted_bars[i];

                        if (this[sb.Time] is Bar b)
                        {
                            b.MergeFromSmallerBar(sb);
                        }
                        else
                        {
                            Bar nb = new(this, sb);
                            nb.Index = j;
                            Rows.Add(nb);
                            TimeToRows.Add(nb.Time, nb.Index);
                            j++;
                        }
                    }

                    //Rows[Count - 1].Source = DataSourceType.Tick;
                }

                Status = TableStatus.DataReady;
            }
        }

        public void MergeFromSmallerBar(DateTime tickTime, Bar sb)
        {
            if (this[sb.Time] is Bar b)
            {
                b.MergeFromSmallerBar(sb);
            }
            else
            {
                Bar nb = new(this, sb);
                nb.Index = Count;
                Rows.Add(nb);
                TimeToRows.Add(nb.Time, nb.Index);
            }

            LastTickTime = tickTime;

            if (Status >= TableStatus.DataReady)
            {
                //Status = TableStatus.Ticking;
                //SetCalculationPointer(LastCalculateIndex - 1);
                CalculateTickRequested = true;
            }

            UpdateTime = DateTime.Now;
        }

        #endregion Load Bars

        #region Add Ticks

        public DateTime LastTickTime { get; private set; } = DateTime.MinValue;

        public void AddPriceTick(DateTime tickTime, double last, double volume, DataSourceType minimumSource = DataSourceType.IB)
        {
            DateTime time = Frequency.Align(tickTime);
            bool isUpdated = true;

            if (this[time] is Bar b)
            {
                if (b.Source >= minimumSource)
                {
                    if (last > b.High) // New High
                    {
                        b.High = last; // Also update 
                    }

                    if (last < b.Low) // New Low
                    {
                        b.Low = last;
                    }

                    if (tickTime <= b.DataSourcePeriod.Start && tickTime >= b.Period.Start) // Eariler Open
                    {
                        b.Open = last;
                        b.DataSourcePeriod.Insert(tickTime);
                    }

                    if (tickTime >= b.DataSourcePeriod.Stop && tickTime < b.Period.Stop) // Later Close
                    {
                        b.Close = last;
                        b.DataSourcePeriod.Insert(tickTime);
                        Console.WriteLine("Added inbound tick[ " + b.Source + " | " + tickTime + " | " + b.DataSourcePeriod.Start + " -> " + b.DataSourcePeriod.Stop + " | " + b.Period + "] to existing bar: " + b.DataSourcePeriod + " | " + b.Period);
                    }
                    else
                    {
                        Console.WriteLine("********** Inbound Tick Time Overflow ***********");
                    }

                    b.Volume += volume;
                    b.Source = DataSourceType.Realtime;
                }
                else
                {
                    Console.WriteLine("###### Inbound Tick Ignored, because source = " + b.Source);
                    isUpdated = false;
                }
            }
            else
            {
                if (Count > 0 && this[LastIndex] is Bar lastBar && lastBar.Source >= DataSourceType.IB)
                {
                    lastBar.Source = DataSourceType.Realtime;
                }

                Bar nb = new(this, tickTime, last, volume);
                nb.Index = Count;
                Rows.Add(nb);
                TimeToRows.Add(nb.Time, nb.Index);

            }

            LastTickTime = tickTime;

            if (isUpdated && Status >= TableStatus.DataReady)
            {
                //Status = TableStatus.Ticking;
                //SetCalculationPointer(LastCalculateIndex - 1);
                CalculateTickRequested = true;
            }

            UpdateTime = DateTime.Now;
        }

        #endregion Add Ticks

        #region Data/Bar Analysis (TA) Calculation

        private Dictionary<BarAnalysis, BarAnalysisPointer> BarAnalysisPointerLUT { get; } = new();

        private BarAnalysisPointer GetBarAnalysisPointer(BarAnalysis ba)
        {
            lock (BarAnalysisPointerLUT)
            {
                if (!BarAnalysisPointerLUT.ContainsKey(ba))
                    BarAnalysisPointerLUT.Add(ba, new BarAnalysisPointer(this, ba));

                return BarAnalysisPointerLUT[ba];
            }
        }

        public void Remove(BarAnalysis ba)
        {
            lock (BarAnalysisPointerLUT)
            {
                if (BarAnalysisPointerLUT.ContainsKey(ba))
                    BarAnalysisPointerLUT.Remove(ba);
            }
        }

        public void ResetCalculationPointer(BarAnalysis ba) => Remove(ba);

        private Dictionary<BarAnalysisSet, BarAnalysisSetPointer> BarAnalysisSetPointerLUT { get; } = new();

        private BarAnalysisSetPointer GetBarAnalysisSetPointer(BarAnalysisSet bas)
        {
            lock (BarAnalysisSetPointerLUT)
            {
                if (!BarAnalysisSetPointerLUT.ContainsKey(bas))
                    BarAnalysisSetPointerLUT.Add(bas, new BarAnalysisSetPointer(this, bas));

                return BarAnalysisSetPointerLUT[bas];
            }
        }

        public void ResetCalculationPointer(BarAnalysisSet bas)
        {
            lock (BarAnalysisPointerLUT)
                lock (BarAnalysisSetPointerLUT)
                {
                    if (BarAnalysisSetPointerLUT.ContainsKey(bas))
                    {
                        BarAnalysisSetPointerLUT.Remove(bas);
                        bas.RunEach(ba => ResetCalculationPointer(ba));
                    }
                }
        }

        public BarAnalysisSetPointer this[BarAnalysisSet bas] => GetBarAnalysisSetPointer(bas);

        private void ResetCalculationPointer()
        {
            lock (BarAnalysisSetPointerLUT)
                lock (BarAnalysisPointerLUT)
                {
                    BarAnalysisPointerLUT.Clear();
                    BarAnalysisSetPointerLUT.Clear();
                }
        }

        private void SetCalculationPointer(int pt)
        {
            if (pt < 0)
                pt = 0;
            else if (pt > LastIndex)
                pt = LastIndex;

            lock (BarAnalysisSetPointerLUT)
                lock (BarAnalysisPointerLUT)
                {
                    foreach (BarAnalysisSetPointer basp in BarAnalysisSetPointerLUT.Values)
                        basp.LastCalculateIndex = Math.Min(basp.LastCalculateIndex, pt);

                    foreach (BarAnalysisPointer bap in BarAnalysisPointerLUT.Values)
                        bap.StartPt = Math.Min(bap.StartPt, pt); //if (bap.StartPt > pt) bap.StartPt = pt;
                }
        }

        public void SetCalculationPointer(ref DateTime time)
        {
            int pt = IndexOf(ref time) - 1;
            SetCalculationPointer(pt);
        }

        private BarAnalysisPointer Calculate(BarAnalysis ba)
        {
            BarAnalysisPointer bap = GetBarAnalysisPointer(ba);
            ba.Update(bap);
            return bap;
        }

        #region Basic Analysis

        public static NativeGainAnalysis GainAnalysis { get; } = new();

        #endregion Basic Analysis

        public int LastCalculateIndex { get; private set; } = -1;

        /// <summary>
        /// Returns the Last Bar in the Table. Null is the BarTable is empty.
        /// </summary>
        public Bar LastBar => LastCalculateIndex < 0 ? null : this[LastCalculateIndex];

        public Bar LastBar_1 => LastCalculateIndex < 1 ? null : this[LastCalculateIndex - 1];

        /// <summary>
        /// The mighty calculate for all technicial analysis
        /// </summary>
        private void Calculate(IEnumerable<BarAnalysis> bas, bool debugInfo = true)
        {
            Status = TableStatus.Calculating;

            if (debugInfo)
            {
                Console.WriteLine("\n==================");
                Console.WriteLine("Table: " + Name + " | Count: " + Count);
            }

            DateTime total_start_time = DateTime.Now;

            int startPt = Count;

            if (Count > 0)
            {
                startPt = Math.Min(startPt, Calculate(GainAnalysis).StartPt);
                //startPt = Math.Min(startPt, Calculate(PivotAnalysis).StartPt);

                foreach (BarAnalysis ba in bas)
                {
                    DateTime single_start_time = DateTime.Now;

                    BarAnalysisPointer bap = GetBarAnalysisPointer(ba);
                    int original_start = bap.StartPt;
                    int original_stop = bap.StopPt;
                    ba.Update(bap);

                    startPt = Math.Min(startPt, bap.StartPt);

                    if (debugInfo)
                    {
                        Console.WriteLine(ba.Name + " | (" + original_start + "->" + bap.StartPt + ") | Time " + (DateTime.Now - single_start_time).TotalMilliseconds.ToString() + "ms");
                    }
                }
            }

            int lastIndex = Math.Min(Count - 1, startPt);

            if (bas is BarAnalysisSet bas0)
                GetBarAnalysisSetPointer(bas0).LastCalculateIndex = lastIndex;
            else
                LastCalculateIndex = lastIndex;

            if (debugInfo)
            {
                Console.WriteLine("------------------");
                Console.WriteLine(Name + " | " + (DateTime.Now - total_start_time).TotalMilliseconds.ToString() + "ms" + " | Stopped at: " + lastIndex + " | LastTime = " + (BarFreq >= BarFreq.Daily ? Rows[lastIndex].Time.ToString("MM-dd-yyyy") : Rows[lastIndex].DataSourcePeriod.Stop.ToString("HH:mm:ss")) + " | LastClose = " + LastClose);
                Console.WriteLine("==================\n");
            }
        }

        public void CalculateRefresh(BarAnalysisSet bas)
        {
            if (Count > 0 && bas is BarAnalysisSet)
                lock (DataLockObject)
                {
                    Calculate(bas);
                    Status = TableStatus.CalculateFinished;
                    Status = TableStatus.Ready;

                    lock (DataConsumers)
                    {
                        DataConsumers.Where(n => n is BarChart bc && bc.BarAnalysisSet == bas).Select(n => n as BarChart).ToList().ForEach(n => { n.PointerToEnd(); });
                    }
                }
        }

        private Task CalculateTickTask { get; }

        private CancellationTokenSource CalculateTickCancelTs { get; }

        private bool CalculateTickRequested { get; set; } = false;

        public DateTime LastCalculatedTickTime { get; private set; } = DateTime.MinValue;

        private void CalculateTickWorker()
        {
            while (CalculateTickCancelTs is CancellationTokenSource cts && !cts.IsCancellationRequested)
            {
                if (CalculateTickRequested)
                {
                    Status = TableStatus.Ticking;
                    lock (DataLockObject)
                    {
                        CalculateTickRequested = false;
                        Calculate(BarAnalysisPointerLUT.Keys);
                        LastCalculatedTickTime = LastTickTime;
                    }
                    Status = TableStatus.TickingFinished;
                    Status = TableStatus.Ready;
                }
                else
                    Thread.Sleep(1);
            }
        }

        public void ResetCalculateData()
        {
            CalculateTickRequested = false;
            Status = TableStatus.Default;
            lock (DataLockObject)
            {
                ResetCalculationPointer();
                Rows.AsParallel().ForAll(n => n.ClearAllCalculationData());
            }
            Status = TableStatus.DataReady;
        }

        #endregion Data/Bar Analysis (TA) Calculation

        #region Data Consumers

        private List<IDataConsumer> DataConsumers { get; } = new();

        private IEnumerable<IDataRenderer> DataRenderers => DataConsumers.Where(n => n is IDataRenderer).Select(n => n as IDataRenderer);

        public bool AddDataConsumer(IDataConsumer idk) => DataConsumers.CheckAdd(idk);

        public bool RemoveDataConsumer(IDataConsumer idk) => DataConsumers.CheckRemove(idk);

        public DateTime UpdateTime { get; private set; } = TimeTool.MinInvalid;

        public bool ReadyToShow => Count > 0 && Status >= TableStatus.DataReady;

        public TableStatus Status
        {
            get => m_Status;

            set
            {
                m_Status = value;
                UpdateTime = DateTime.Now;

                lock (DataConsumers)
                {
                    if (ReadyToShow)
                    {
                        if (m_Status == TableStatus.CalculateFinished)
                        {
                            foreach (var idr in DataRenderers) idr.PointerToEnd();
                        }
                        else if (m_Status == TableStatus.TickingFinished)
                        {
                            foreach (var idr in DataRenderers) idr.PointerToNextTick();
                        }
                    }

                    DataConsumers.ForEach(n => n.DataIsUpdated(this));
                }
            }
        }

        private TableStatus m_Status = TableStatus.Default;

        #endregion Data Consumers

        #region Equality

        public bool Equals(BarTable other) => Key == other.Key;
        public bool Equals(((string name, Exchange exchange, string typeName) ContractKey, BarFreq BarFreq, DataType Type) other) => Key == other;
        public bool Equals((Contract, BarFreq, DataType) other) => (Contract, BarFreq, Type) == other;
        public bool Equals(BarDataFile other) => Key == other.Key;

        public static bool operator ==(BarTable s1, BarTable s2) => s1.Equals(s2);
        public static bool operator !=(BarTable s1, BarTable s2) => !s1.Equals(s2);
        public static bool operator ==(BarTable s1, BarDataFile s2) => s1.Equals(s2);
        public static bool operator !=(BarTable s1, BarDataFile s2) => !s1.Equals(s2);
        public static bool operator ==(BarTable s1, (Contract c, BarFreq barFreq, DataType type) s2) => s1.Equals(s2);
        public static bool operator !=(BarTable s1, (Contract c, BarFreq barFreq, DataType type) s2) => !s1.Equals(s2);

        /// <summary>
        /// https://stackoverflow.com/questions/4219261/overriding-operator-how-to-compare-to-null
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (other is BarTable bt)
                return Equals(bt);
            else if (other is BarDataFile bdf)
                return Equals(bdf);
            else if (other.GetType() == typeof((Contract, BarFreq, DataType)))
                return Equals(((Contract, BarFreq, DataType))other);
            else if (other.GetType() == typeof(((string name, Exchange exchange, string typeName) ContractKey, BarFreq BarFreq, DataType Type)))
                return Equals((((string name, Exchange exchange, string typeName) ContractKey, BarFreq BarFreq, DataType Type))other);
            else
                return false;
        }

        public override int GetHashCode() => Key.GetHashCode();

        #endregion Equality

        /*
public void AppendBars(List<Bar> sorted_bars)
{
    if (sorted_bars.Count > 0)
    {
        if (this != sorted_bars.FirstOrDefault().Table)
            throw new("bar's table has to match with this table!");

        Status = TableStatus.Loading;

        lock (DataLockObject)
        {
            int earliestIndex = -1;

            for (int i = 0; i < sorted_bars.Count; i++)
            {
                Bar b = sorted_bars[i];

                if (this[b.Time] is Bar b_ori)
                {
                    if (b.Source <= b_ori.Source)
                    {
                        b_ori.Copy(b);
                        b.Index = b_ori.Index;
                    }
                }
                else
                {
                    Rows.Add(b);
                    b.Index = LastIndex;
                    TimeToRows.Add(b.Time, b.Index);
                }

                if (earliestIndex == -1)
                    earliestIndex = b.Index;
            }

            if (earliestIndex > 0) SetCalculationPointer(earliestIndex - 1);
        }

        Status = TableStatus.DataReady;
    }
}
*/
    }
}
