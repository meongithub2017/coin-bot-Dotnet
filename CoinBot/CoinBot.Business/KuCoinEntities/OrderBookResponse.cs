﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoinBot.Business.Entities.KuCoinEntities
{
    public class OrderBookResponse
    {
        [JsonProperty(PropertyName = "SELL")]
        public OrderBook[] sells { get; set; }
        [JsonProperty(PropertyName = "BUY")]
        public OrderBook[] buys { get; set; }
        public long timestamp { get; set; }
    }
}
