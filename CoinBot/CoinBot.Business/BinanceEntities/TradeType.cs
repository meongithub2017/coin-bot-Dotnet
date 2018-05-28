﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CoinBot.Business.Entities
{
    public enum TradeType
    {
        [Description("NONE")]
        NONE,
        [Description("BUY")]
        BUY,
        [Description("VOLUMEBUY")]
        VOLUMEBUY,
        [Description("VOLUMEBUYSELLOFF")]
        VOLUMEBUYSELLOFF,
        [Description("SELL")]
        SELL,
        [Description("VOLUMESELL")]
        VOLUMESELL,
        [Description("VOLUMESELLBUYOFF")]
        VOLUMESELLBUYOFF,
        [Description("STOPLOSS")]
        STOPLOSS
    }
}
