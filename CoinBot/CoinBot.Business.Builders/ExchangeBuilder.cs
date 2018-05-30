﻿using CoinBot.Business.Builders.Interface;
using CoinBot.Business.Entities;
using CoinBot.Core;
using CoinBot.Data;
using CoinBot.Data.Interface;
using GDAXSharp.Services.Products.Models;
using GDAXSharp.Services.Products.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoinBot.Business.Builders
{
    public class ExchangeBuilder : IExchangeBuilder
    {
        private Exchange _thisExchange;
        private IBinanceRepository _bianceRepo;
        private IGdaxRepository _gdaxRepo;
        private Helper _helper;

        public ExchangeBuilder()
        {
            _bianceRepo = new BinanceRepository();
            _gdaxRepo = new GdaxRepository();
            _helper = new Helper();
        }

        public ExchangeBuilder(IBinanceRepository binanceRepo)
        {
            _bianceRepo = binanceRepo;
            _gdaxRepo = new GdaxRepository();
            _helper = new Helper();
        }

        public ExchangeBuilder(IGdaxRepository gdaxRepo)
        {
            _bianceRepo = new BinanceRepository();
            _gdaxRepo = gdaxRepo;
            _helper = new Helper();
        }

        public ExchangeBuilder(IBinanceRepository binanceRepo, IGdaxRepository gdaxRepo)
        {
            _bianceRepo = binanceRepo;
            _gdaxRepo = gdaxRepo;
            _helper = new Helper();
        }

        /// <summary>
        /// Set BotSettings
        /// </summary>
        /// <param name="settings">BotSettings Object</param>
        public void SetExchange(BotSettings settings)
        {
            _thisExchange = settings.exchange;
        }

        /// <summary>
        /// Validate exhange api is configured
        /// </summary>
        /// <param name="exchange">Current exchange to use</param>
        /// <returns>Boolean if configured correctly</returns>
        public bool ValidateExchangeConfigured(Exchange exchange)
        {
            _thisExchange = exchange;

            if (_thisExchange == Exchange.BINANCE)
            {
                return _bianceRepo.ValidateExchangeConfigured();
            }
            else if (_thisExchange == Exchange.GDAX)
            {
                return _gdaxRepo.ValidateExchangeConfigured();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set Exchange Api Info
        /// </summary>
        /// <param name="apiInfo">ApiInformation for exhange</param>
        /// <returns>Boolean when complete</returns>
        public bool SetExchangeApi(ApiInformation apiInfo)
        {
            if(_thisExchange == Exchange.BINANCE)
            {
                return _bianceRepo.SetExchangeApi(apiInfo);                
            }
            else if(_thisExchange == Exchange.GDAX)
            {
                return _gdaxRepo.SetExchangeApi(apiInfo);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get Candlesticks
        /// </summary>
        /// <param name="symbol">Trading Symbol</param>
        /// <param name="interval">Candlestick Interval</param>
        /// <param name="range">Number of sticks to return</param>
        /// <returns>BotStick Array</returns>
        public BotStick[] GetCandlesticks(string symbol, Interval interval, int range)
        {
            if(_thisExchange == Exchange.BINANCE)
            {
                Candlestick[] sticks = null;

                while(sticks == null)
                {
                    sticks = _bianceRepo.GetCandlestick(symbol, interval, range).Result;
                }

                return BinanceStickToBotStick(sticks);
            }
            else if(_thisExchange == Exchange.GDAX)
            {
                CandleGranularity candleGranularity;
                switch(interval)
                {
                    case Interval.OneM:
                        candleGranularity = CandleGranularity.Minutes1;
                        break;
                    case Interval.FiveM:
                        candleGranularity = CandleGranularity.Minutes5;
                        break;
                    case Interval.FifteenM:
                        candleGranularity = CandleGranularity.Minutes15;
                        break;
                    case Interval.OneH:
                        candleGranularity = CandleGranularity.Hour1;
                        break;
                    case Interval.SixH:
                        candleGranularity = CandleGranularity.Hour6;
                        break;
                    case Interval.OneD:
                        candleGranularity = CandleGranularity.Hour24;
                        break;
                    default:
                        candleGranularity = CandleGranularity.Minutes1;
                        break;
                }

                Candle[] sticks = null;
                
                while (sticks == null)
                {
                    sticks = _gdaxRepo.GetCandleSticks(symbol, range, candleGranularity).Result.Take(range).ToArray();
                }
                
                return GdaxStickToBotStick(sticks.ToArray());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get Balances for account
        /// </summary>
        /// <returns>Collection of Balance objects</returns>
        public List<Balance> GetBalance()
        {
            if (_thisExchange == Exchange.BINANCE)
            {
                var account = _bianceRepo.GetBalance().Result;

                return account.balances;
            }
            else if (_thisExchange == Exchange.GDAX)
            {
                var accountList = _gdaxRepo.GetBalance().Result.ToArray();

                var balances = new List<Balance>();

                for (int i = 0; i < accountList.Count(); i++)
                {
                    var balance = new Balance
                    {
                        asset = accountList[i].Currency.ToString(),
                        free = accountList[i].Available,
                        locked = accountList[i].Hold
                    };
                    balances.Add(balance);
                }

                return GdaxAccountCollectionToBalanceCollection(accountList).ToList();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Place a Trade
        /// </summary>
        /// <param name="tradeParams">TradeParams object</param>
        /// <returns>TradeResponse object</returns>
        public TradeResponse PlaceTrade(TradeParams tradeParams)
        {
            if(_thisExchange == Exchange.BINANCE)
            {
                var response = _bianceRepo.PostTrade(tradeParams).Result;

                return response;
            }
            else if(_thisExchange == Exchange.GDAX)
            {
                GDAXSharp.Services.Orders.Models.Responses.OrderResponse response;

                if (tradeParams.type == "STOPLOSS")
                {
                    response = _gdaxRepo.PlaceStopLimit(tradeParams).Result;
                }
                else
                {
                    response = _gdaxRepo.PlaceTrade(tradeParams).Result;
                }

                return GdaxOrderResponseToTradeResponse(response);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get Order Details
        /// </summary>
        /// <param name="trade">TradeResponse object</param>
        /// <param name="symbol">Trading symbol</param>
        /// <returns>OrderResponse object</returns>
        public OrderResponse GetOrderDetail(TradeResponse trade, string symbol = "")
        {
            if (_thisExchange == Exchange.BINANCE)
            {
                var response = _bianceRepo.GetOrder(symbol, trade.orderId).Result;

                return response;
            }
            else if (_thisExchange == Exchange.GDAX)
            {
                var response = _gdaxRepo.GetOrder(trade.clientOrderId).Result;

                return GdaxOrderResponseToOrderResponse(response);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Delete a trade
        /// </summary>
        /// <param name="tradeParams">CancelTradeParams object</param>
        /// <returns>TradeResponse object</returns>
        public TradeResponse DeleteTrade(CancelTradeParams tradeParams)
        {
            if (_thisExchange == Exchange.BINANCE)
            {
                var response = _bianceRepo.DeleteTrade(tradeParams).Result;

                return response;
            }
            else if (_thisExchange == Exchange.GDAX)
            {
                var response = _gdaxRepo.CancelTrade(tradeParams.origClientOrderId).Result;

                var tradeResponse = new TradeResponse
                {
                    clientOrderId = response.OrderIds.ToList().FirstOrDefault().ToString()
                };

                return tradeResponse;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Convert Binance Candlestick array to BotStick array
        /// </summary>
        /// <param name="binanceArray">Binance Candlestick array</param>
        /// <returns>BotStick array</returns>
        private BotStick[] BinanceStickToBotStick(Candlestick[] binanceArray)
        {
            return this._helper.MapEntity<Candlestick[], BotStick[]>(binanceArray);
        }

        /// <summary>
        /// Convert GDAX Candle array to BotStick array
        /// </summary>
        /// <param name="gdaxArray">GDAX Candle array</param>
        /// <returns>BotStick array</returns>
        private BotStick[] GdaxStickToBotStick(Candle[] gdaxArray)
        {
            return this._helper.MapEntity<Candle[], BotStick[]>(gdaxArray);
        }

        /// <summary>
        /// Convert GDAX OrderResponse to TradeResponse
        /// </summary>
        /// <param name="response">GDAX OrderReponse object</param>
        /// <returns>TradeReponse object</returns>
        private TradeResponse GdaxOrderResponseToTradeResponse(GDAXSharp.Services.Orders.Models.Responses.OrderResponse response)
        {
            return _helper.MapEntity<GDAXSharp.Services.Orders.Models.Responses.OrderResponse, TradeResponse>(response);
        }

        /// <summary>
        /// Convert GDAX OrderResponse to OrderResponse
        /// </summary>
        /// <param name="response">GDAX OrderReponse object</param>
        /// <returns>OrderReponse object</returns>
        private OrderResponse GdaxOrderResponseToOrderResponse(GDAXSharp.Services.Orders.Models.Responses.OrderResponse response)
        {
            return _helper.MapEntity<GDAXSharp.Services.Orders.Models.Responses.OrderResponse, OrderResponse>(response);
        }

        /// <summary>
        /// Convert GDAX CancelOrderResponse to TradeResponse
        /// </summary>
        /// <param name="response">GDAX CancelOrderResponse object</param>
        /// <returns>TradeReponse object</returns>
        private TradeResponse GdaxCancelOrderResponseToTradeResponse(GDAXSharp.Services.Orders.Models.Responses.CancelOrderResponse response)
        {
            return _helper.MapEntity<GDAXSharp.Services.Orders.Models.Responses.CancelOrderResponse, TradeResponse>(response);
        }

        /// <summary>
        /// Convert GDAX Account collection to Balance collection
        /// </summary>
        /// <param name="response">GDAX Account collection</param>
        /// <returns>Balance collection</returns>
        private IEnumerable<Balance> GdaxAccountCollectionToBalanceCollection(IEnumerable<GDAXSharp.Services.Accounts.Models.Account> accountList)
        {
            return _helper.MapEntity<IEnumerable<GDAXSharp.Services.Accounts.Models.Account>, IEnumerable<Balance>>(accountList);
        }
    }
}
