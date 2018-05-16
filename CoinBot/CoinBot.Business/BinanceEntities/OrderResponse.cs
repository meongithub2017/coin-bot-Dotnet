﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CoinBot.Business.Entities
{
    public class OrderResponse
    {
        public string symbol { get; set; }
        public long orderId { get; set; }
        public string clientOrderId { get; set; }
        public decimal price { get; set; }
        public decimal origQty { get; set; }
        public decimal executedQty { get; set; }
        public OrderStatus status { get; set; }
        public TimeInForce timeInForce { get; set; }
        public OrderType type { get; set; }
        public TradeType side { get; set; }
        public decimal stopPrice { get; set; }
        public decimal iceburgQty { get; set; }
        public long time { get; set; }
        public bool isWorking { get; set; }
    }
}
