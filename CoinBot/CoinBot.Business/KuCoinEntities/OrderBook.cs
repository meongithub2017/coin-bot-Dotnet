﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoinBot.Business.Entities.KuCoinEntities
{
    [JsonConverter(typeof(Converter.ObjectToArrayConverter<OrderBook>))]
    public class OrderBook
    {
        [JsonProperty(Order = 1)]
        public decimal price { get; set; }
        [JsonProperty(Order = 2)]
        public decimal quantity { get; set; }
        [JsonProperty(Order = 3)]
        public decimal pairTotal { get; set; }
    }
}
