﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// The trade rule applies to each contract
/// 
/// ***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xu;

namespace Pacmio
{
    public enum SingleDataSignalType : int
    {
        None = 0,

        Within = 1,

        EnterFromBelow = 2,

        EnterFromAbove = -2,

        Above = 3,

        Below = -3,

        ExitAbove = 5,

        ExitBelow = -5,

        CrossUp = 6,

        CrossDown = -6,

        BounceUp = 100,

        BounceDown = -100,
    }
}
