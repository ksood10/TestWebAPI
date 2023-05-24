using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Globalization;
using System.Linq.Expressions;

namespace TestWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private DateOnly FROMDATE = new(2023, 1, 1);
        private readonly ILogger<TestController> _logger;
        private readonly int LimitMaxNumberOfDays = 100;
        private string BASEURL = "https://api.iex.cloud/v1/data/CORE/HISTORICAL_PRICES/";
        private string TOKENURL = "?token=pk_16f8d9a3765b402c9f75cf5df49bc701";


        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;

        }

        [HttpGet]
        public async Task<string> GetDailyReturns(string symbol ="MSFT", DateTime? fromDate =null,  DateTime? toDate = null)
        {
            string url = BASEURL + symbol+ TOKENURL +"&from=" +"2023-1-1"+"&to="+"2023-2-1";
            var httpClient = new HttpClient ();
            
            var response = await httpClient.GetAsync(url);
           
            if (fromDate != null && toDate != null && toDate.Value.Subtract(fromDate.Value).Days > LimitMaxNumberOfDays)
            {
                var stockdata = new StockData[1];
                stockdata[0] = new StockData() ;
                return JsonConvert.SerializeObject(stockdata);
            }

            if (fromDate == null)
                fromDate = new DateTime(DateTime.Now.Year, 1, 1);
            
            if (toDate == null)
                toDate = DateTime.Now;

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var stockdata = JsonConvert.DeserializeObject<StockData[]>(jsonResponse);
                if (stockdata == null)
                {
                    stockdata = new StockData[1];
                    stockdata[0] = new StockData();
                }

                // Calculate daily returns
                var returns = new List<DailyReturn>();
                for (int i = 1; i < stockdata.Length; i++)
                {
                    decimal dailyReturn = (stockdata[i].close - stockdata[i - 1].close) / stockdata[i - 1].close;
                    returns.Add(new DailyReturn
                    {
                        Date = stockdata[i].date,
                        DailyReturnValue = dailyReturn
                    });
                }
                return JsonConvert.SerializeObject(returns);
            }

            throw new Exception("Error getting data."+ response.StatusCode);
           
        }


    }

}
