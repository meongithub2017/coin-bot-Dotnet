﻿using CoinBot.Business.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoinBot.Business.Builders.Interface
{
    public interface ITradeBuilder
    {
        /// <summary>
        /// Check if config file exists
        /// </summary>
        /// <returns>Boolean of result</returns>
        bool ConfigFileExits();

        /// <summary>
        /// Check if settings file exists
        /// </summary>
        /// <returns>Boolean of result</returns>
        bool SettingsFileExists();

        /// <summary>
        /// Get BotSettings
        /// </summary>
        /// <returns>BotSettings object</returns>
        BotSettings GetBotSettings();

        /// <summary>
        /// Load bot settings from disk
        /// </summary>
        void LoadBotSettingsFile();

        /// <summary>
        /// Set BotSettings in builder
        /// </summary>
        /// <param name="settings">Updated Settings</param>
        /// <returns>Boolean when complete</returns>
        bool SetBotSettings(BotSettings settings);
        
        /// <summary>
        /// Get all open stop losses
        /// </summary>
        /// <returns>Collection of OpenStopLoss</returns>
        IEnumerable<OpenStopLoss> GetStopLosses();

        /// <summary>
        /// Get all transactions
        /// </summary>
        /// <returns>Collection of TradeInformation</returns>
        IEnumerable<TradeInformation> GetTradeHistory();

        /// <summary>
        /// Get current balance
        /// </summary>
        /// <returns>BotBalance object</returns>
        BotBalance GetBalance();

        /// <summary>
        /// Update balances from exchange
        /// </summary>
        void UpdateBalances();

        /// <summary>
        /// Get Balance history
        /// </summary>
        /// <returns>Collection of BotBalance objects</returns>
        IEnumerable<BotBalance> GetBalanceHistory();

        /// <summary>
        /// Set paper balances on load
        /// </summary>
        void SetPaperBalance();

        /// <summary>
        /// Get paper balances available
        /// </summary>
        /// <param name="btcStartingValue">Starting btc value for paper trading (default = 0)</param>
        /// <returns>Collection of balance objects</returns>
        List<Balance> GetPaperBalances(decimal btcStartingValue = 0);

        /// <summary>
        /// Log balances to file
        /// </summary>
        /// <returns>Boolean when complete</returns>
        bool LogBalances();

        /// <summary>
        /// Log Trades
        /// </summary>
        /// <param name="tradeInformation">New TradeInformation object</param>
        /// <returns></returns>
        bool LogTransaction(TradeInformation tradeInformation);

        /// <summary>
        /// Get balances available
        /// </summary>
        /// <returns>Collection of balance objects</returns>
        List<Balance> GetBalances();

        /// <summary>
        /// Get Candlesticks from exchange API
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="interval">Candlestick Interval</param>
        /// <param name="range">Number of candlesticks to get</param>
        /// <returns>Array of Candlestick objects</returns>
        Candlestick[] GetCandlesticks(string symbol, Interval interval, int range);

        /// <summary>
        /// Place a trade
        /// </summary>
        /// <param name="tradeParams">Trade parameters</param>
        /// <returns>TradeResponse object</returns>
        TradeResponse PlaceTrade(TradeParams tradeParams);

        /// <summary>
        /// Set up respository
        /// </summary>
        /// <returns>Boolean when complete</returns>
        bool SetupRepository();

        /// <summary>
        /// Check if Stop Loss Hit
        /// </summary>
        /// <param name="currentPrice">Current price of coin</param>
        /// <returns>Nullable decimal value of stop loss</returns>
        decimal? StoppedOutCheck(decimal currentPrice);

        /// <summary>
        /// Buy crypto
        /// </summary>
        /// <param name="orderPrice">Buy price</param>
        /// <param name="tradeType">Trade Type</param>
        /// <returns>Boolean when complete</returns>
        bool BuyCrypto(decimal orderPrice, TradeType tradeType);

        /// <summary>
        /// Sell crypto
        /// </summary>
        /// <param name="orderPrice">Current price</param>
        /// <param name="tradeType">Trade Type</param>
        void SellCrypto(decimal orderPrice, TradeType tradeType);

        /// <summary>
        /// Check status of placed trade
        /// </summary>
        /// <param name="orderId">OrderId of trade</param>
        /// <returns>Boolean value of filled status</returns>
        bool CheckTradeStatus(long orderId);

        /// <summary>
        /// Get quantity to trade based
        /// </summary>
        /// <param name="tradeType">TradeType object</param>
        /// <param name="orderPrice">Requested trade price</param>
        /// <returns>decimal of quantity to purchase</returns>
        decimal GetTradeQuantity(TradeType tradeType, decimal orderPrice);

        /// <summary>
        /// Cancel a stop loss
        /// </summary>
        /// <returns>Boolean value when complete</returns>
        bool CancelStopLoss();

        /// <summary>
        /// Make a trade
        /// </summary>
        /// <param name="tradeType">TradeType object</param>
        /// <param name="orderPrice">Trade price</param>
        /// <returns>TradeResponse object</returns>
        TradeResponse MakeTrade(TradeType tradeType, decimal orderPrice);

        /// <summary>
        /// Place a stop loss
        /// </summary>
        /// <param name="orderPrice">Trade price</param>
        /// <param name="quantity">Quantity to trade</param>
        /// <returns>TradeResponse object</returns>
        TradeResponse PlaceStopLoss(decimal orderPrice, decimal quantity);

        /// <summary>
        /// Cancel trade
        /// </summary>
        /// <param name="orderId">OrderId to cancel</param>
        /// <param name="origClientOrderId">ClientOrderId to cancel</param>
        void CancelTrade(long orderId, string origClientOrderId);

        /// <summary>
        /// Cancel a trade
        /// </summary>
        /// <param name="tradeParams">CancelTrade parameters</param>
        /// <returns>TradeResponse object</returns>
        TradeResponse CancelTrade(CancelTradeParams tradeParams);

        /// <summary>
        /// Cancel a paper trade for testing purposes
        /// </summary>
        /// <param name="tradeParams">Trade parameters</param>
        /// <returns>TradeResponse object</returns>
        TradeResponse CancelPaperTrade(CancelTradeParams tradeParams);

        /// <summary>
        /// Get status of a trade
        /// </summary>
        /// <param name="orderId">OrderId of trade</param>
        /// <returns>OrderResponse</returns>
        OrderResponse GetOrderStatus(long orderId);

        /// <summary>
        /// Get status of a paper trade
        /// </summary>
        /// <param name="orderId">OrderId of trade</param>
        /// <returns>OrderResponse</returns>
        OrderResponse GetPaperOrderStatus(long orderId);
    }
}
