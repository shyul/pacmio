﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2020 Xu Li - me@xuli.us
/// 
/// The trade rule applies to each contract
/// 
/// ***************************************************************************

using System;

namespace Pacmio
{
    public interface IAnalysisSetting : IEquatable<IAnalysisSetting>
    {
        string Name { get; }

        BarAnalysisSet BarAnalysisSet(BarFreq barFreq);
    }
}
