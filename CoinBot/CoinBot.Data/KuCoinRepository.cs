﻿using CoinBot.Business.Entities;
using CoinBot.Business.Entities.KuCoinEntities;
using CoinBot.Core;
using CoinBot.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CoinBot.Data
{
    public class KuCoinRepository : IKuCoinRepository
    {
        private Security security;
        private IRESTRepository _restRepo;
        private string baseUrl;
        private ApiInformation _apiInfo;
        private DateTimeHelper _dtHelper;
        private Helper _helper;
        private IFileRepository _fileRepo;

        /// <summary>
        /// Constructor
        /// </summary>
        public KuCoinRepository()
        {
            security = new Security();
            _restRepo = new RESTRepository();
            baseUrl = "https://api.kucoin.com";
            _apiInfo = new ApiInformation();
            _dtHelper = new DateTimeHelper();
            _helper = new Helper();
            _fileRepo = new FileRepository();
        }

        /// <summary>
        /// Check if the Exchange Repository is ready for trading
        /// </summary>
        /// <returns>Boolean of validation</returns>
        public bool ValidateExchangeConfigured()
        {
            var ready = string.IsNullOrEmpty(_apiInfo.apiKey) ? false : true;
            if (!ready)
                return false;

            return string.IsNullOrEmpty(_apiInfo.apiSecret) ? false : true;
        }

        /// <summary>
        /// Set ApiInformation for repository
        /// </summary>
        /// <param name="apiInfo">ApiInformation object</param>
        /// <returns>Boolean when complete</returns>
        public bool SetExchangeApi(ApiInformation apiInfo)
        {
            _apiInfo = apiInfo;
            return true;
        }

        /// <summary>
        /// Get candlesticks
        /// </summary>
        /// <param name="symbol">String of symbol</param>
        /// <param name="size">stick size</param>
        /// <param name="limit">number of sticks</param>
        /// <returns>ChartValue object</returns>
        public async Task<ChartValue> GetCandlesticks(string symbol, Interval size, int limit)
        {
            var kuPair = _helper.CreateDashedPair(symbol);
            var to = _dtHelper.UTCEndOfMinuteToUnixTime();// _dtHelper.UTCtoUnixTime();
            var from = _helper.GetFromUnixTime(to, size, (limit + 2));
            var kuSize = _helper.IntervalToKuCoinStringInterval(size);
            var endpoint = $"/v1/open/chart/history?symbol={kuPair}&resolution={kuSize}&from={from}&to={to}";
            var url = baseUrl + endpoint;

            try
            {
                var response = await _restRepo.GetApiStream<ChartValue>(url);

                return response;
            }
            catch (Exception ex)
            {
                _fileRepo.LogError($"KuCoinRepository.GetCandlesticks() error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get account balance
        /// </summary>
        /// <returns>Balance array</returns>
        public async Task<Business.Entities.KuCoinEntities.Balance[]> GetBalance()
        {
            var endpoint = "/v1/account/balance";
            var url = baseUrl + endpoint;

            var headers = GetRequestHeaders(endpoint, null);

            try
            {
                var response = await _restRepo.GetApiStream<ApiResponse<Business.Entities.KuCoinEntities.Balance[]>>(url, headers);

                return response.data;
            }
            catch (Exception ex)
            {
                _fileRepo.LogError($"KuCoinRepository.GetBalance() error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get order information
        /// </summary>
        /// <param name="symbol">string of symbol</param>
        /// <param name="tradeType">Trade type</param>
        /// <param name="orderId">long of orderId</param>
        /// <param name="page">Page number, default 1</param>
        /// <param name="limit">Number of fills to return, default 20</param>
        /// <returns>OrderResponse object</returns>
        public async Task<OrderListDetail> GetOrder(string symbol, TradeType tradeType, long orderId, int page = 1, int limit = 20)
        {
            var endpoint = "/v1/order/detail";
            var url = baseUrl + endpoint;
            var kuPair = _helper.CreateDashedPair(symbol);

            var queryString = new List<string>
            {
                $"symbol={kuPair}",
                $"type={tradeType.ToString()}",
                $"limit={limit}",
                $"page={page}",
                $"orderOid={orderId}"
            };

            var headers = GetRequestHeaders(endpoint, queryString.ToArray());

            try
            {
                var response = await _restRepo.GetApiStream<ApiResponse<DealOrder<OrderListDetail>>>(url, headers);

                return response.data.datas;
            }
            catch (Exception ex)
            {
                _fileRepo.LogError($"KuCoinRepository.GetOrder() error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all current user order information
        /// </summary>
        /// <param name="symbol">string of symbol</param>
        /// <param name="limit">Int of orders count to return, default 20</param>
        /// <param name="page">Int of page number</param>
        /// <returns>OpenOrderResponse object</returns>
        public async Task<OrderListDetail[]> GetOrders(string symbol, int limit = 20, int page = 1)
        {
            var endpoint = "/v1/deal-orders";
            var kuPair = _helper.CreateDashedPair(symbol);

            var queryString = new List<string>
            {
                $"limit={limit}",
                $"page={page}",
                $"symbol={kuPair}"
            };

            var headers = GetRequestHeaders(endpoint, queryString.ToArray());

            var url = baseUrl + endpoint + $"?{queryString[0]}&{queryString[1]}&{queryString[2]}";

            try
            {
                var response = await _restRepo.GetApiStream<ApiResponse<DealOrder<OrderListDetail[]>>>(url, headers);

                return response.data.datas;
            }
            catch (Exception ex)
            {
                _fileRepo.LogError($"KuCoinRepository.GetOrders() error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all open orders
        /// </summary>
        /// <param name="symbol">string of symbol</param>
        /// <returns>KuCoinOpenOrders object</returns>
        public async Task<OpenOrderResponse> GetOpenOrders(string symbol)
        {
            var endpoint = "/v1/order/active";
            var kuPair = _helper.CreateDashedPair(symbol);

            var queryString = new List<string>
            {
                $"symbol={kuPair}"
            };

            var headers = GetRequestHeaders(endpoint, queryString.ToArray());

            var url = baseUrl + endpoint + $"?{queryString[0]}";

            try
            {
                var response = await _restRepo.GetApiStream<ApiResponse<OpenOrderResponse>>(url, headers);
            
                return response.data;            
            }
            catch (Exception ex)
            {
                _fileRepo.LogError($"KuCoinRepository.GetOpenOrders() error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Order Book for a pair
        /// </summary>
        /// <param name="symbol">string of trading pair</param>
        /// <param name="limit">number of orders to return per side, default 100</param>
        /// <returns>OrderBook object</returns>
        public async Task<OrderBookResponse> GetOrderBook(string symbol, int limit = 100)
        {
            var kuPair = _helper.CreateDashedPair(symbol);
            var endpoint = $"/v1/open/orders?symbol={kuPair}&limit={limit}";
            var url = baseUrl + endpoint;
            
            try
            {
                var response = await _restRepo.GetApiStream<ApiResponse<OrderBookResponse>>(url);

                return response.data;
            }
            catch (Exception ex)
            {
                _fileRepo.LogError($"KuCoinRepository.GetOrderBook() error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Post/Place a trade
        /// </summary>
        /// <param name="tradeParams">Trade to place</param>
        /// <returns>KuCoinResponse object</returns>
        public async Task<ApiResponse<Dictionary<string, string>>> PostTrade(TradeParams tradeParams)
        {
            var endpoint = $"/v1/order";
            var kuPair = _helper.CreateDashedPair(tradeParams.symbol);

            var queryString = new List<string>
            {
                $"symbol={kuPair}",
                $"amount={tradeParams.quantity}",
                $"price={tradeParams.price}",
                $"type={tradeParams.side}"
            };

            var headers = GetRequestHeaders(endpoint, queryString.ToArray());

            var url = baseUrl + endpoint + $"?{_helper.ArrayToString(queryString.ToArray())}";

            try
            {
                var response = await _restRepo.PostApi<ApiResponse<Dictionary<string, string>>>(url, headers);

                return response;
            }
            catch (Exception ex)
            {
                _fileRepo.LogError($"KuCoinRepository.PostTrade() error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Delete/Cancel a trade
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="orderOid">Order id to cancel</param>
        /// <param name="tradeType">Trade type to cancel</param>
        /// <returns>TradeResponse object</returns>
        public async Task<DeleteResponse> DeleteTrade(string symbol, string orderOid, string tradeType)
        {
            var endpoint = "/v1/cancel-order";
            var kuPair = _helper.CreateDashedPair(symbol);

            var queryString = new List<string>
            {
                $"symbol={kuPair}",
                $"orderOid={orderOid}",
                $"type={tradeType}"
            };

            var headers = GetRequestHeaders(endpoint, queryString.ToArray());

            var url = baseUrl + endpoint + $"?{_helper.ArrayToString(queryString.ToArray())}";

            try
            {
                var response = await _restRepo.PostApi<DeleteResponse>(url, headers);

                return response;
            }
            catch (Exception ex)
            {
                _fileRepo.LogError($"KuCoinRepository.DeleteTrade() error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Ticker for all pairs
        /// </summary>
        /// <returns>Array of KuCoinTick objects</returns>
        public async Task<Tick[]> GetTicks()
        {
            var endpoint = "/v1/open/tick";
            var url = baseUrl + endpoint;

            try
            {
                var response = await _restRepo.GetApiStream<ApiResponse<Tick[]>>(url);

                return response.data;
            }
            catch (Exception ex)
            {
                _fileRepo.LogError($"KuCoinRepository.GetCrytpos() error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Tick for a symbol
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <returns>KuCoinTick object</returns>
        public async Task<Tick> GetTick(string symbol)
        {
            var kuPair = _helper.CreateDashedPair(symbol);
            var endpoint = $"/v1/open/tick?symbol={kuPair}";
            var url = baseUrl + endpoint;

            try
            {
                var response = await _restRepo.GetApiStream<ApiResponse<Tick>>(url);

                return response.data;
            }
            catch(Exception ex)
            {
                _fileRepo.LogError($"KuCoinRepository.GetTick() error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get KuCoinTime
        /// </summary>
        /// <returns>long of timestamp</returns>
        public long GetKuCoinTime()
        {
            var endpoint = "/v1/time";
            var url = baseUrl + endpoint;

            var response = _restRepo.GetApi<ServerTime>(url);

            response.Wait();

            return response.Result.serverTime;
        }

        private Dictionary<string, string> GetRequestHeaders(string endpoint, string[] queryString = null)
        {
            var nonce = _dtHelper.UTCtoUnixTimeMilliseconds().ToString();
            var headers = new Dictionary<string, string>();

            headers.Add("KC-API-KEY", _apiInfo.apiKey);
            headers.Add("KC-API-NONCE", nonce);
            headers.Add("KC-API-SIGNATURE", GetSignature(endpoint, nonce, queryString, 0));
            headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");

            return headers;
        }

        private Dictionary<string, string> PostRequestHeaders<T>(string endpoint, string[] queryString, T postData)
        {
            var nonce = _dtHelper.UTCtoUnixTimeMilliseconds().ToString();
            var headers = new Dictionary<string, string>();

            headers.Add("KC-API-KEY", _apiInfo.apiKey);
            headers.Add("KC-API-NONCE", nonce);
            headers.Add("KC-API-SIGNATURE", GetSignature(endpoint, nonce, queryString, postData));
            headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");

            return headers;
        }
        
        private string GetSignature<T>(string endpoint, string nonce, string[] queryString = null, T value = default(T))
        {
            queryString = queryString ?? new string[0];

            Array.Sort(queryString, StringComparer.InvariantCulture);

            var qsValues = string.Empty;

            if(queryString.Length> 0)
            {
                qsValues = _helper.ArrayToString(queryString);
            }
            else if(typeof(T) != typeof(int))
            {
                qsValues += _helper.ObjectToString<T>(value);
            }

            var sigString = $"{endpoint}/{nonce}/{qsValues}";

            var signature = security.GetKuCoinHMCACSignature(_apiInfo.apiSecret, sigString);

            return signature;
        }
    }
}