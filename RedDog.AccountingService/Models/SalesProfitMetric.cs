using System;
using System.Text.Json.Serialization;

namespace RedDog.AccountingService.Models
{
    public class SalesProfitMetric
    {
        [JsonPropertyName("storeId")]  
        public string StoreId { get; set; }

        [JsonPropertyName("orderYear")]  
        public int OrderYear { get; set; }

        [JsonPropertyName("orderMonth")]  
        public int OrderMonth { get; set; }

        [JsonPropertyName("orderDay")]  
        public int OrderDay { get; set; }

        [JsonPropertyName("totalOrders")]  
        public int TotalOrders { get; set; }

        [JsonPropertyName("totalOrderItems")]  
        public int TotalOrderItems { get; set; }

        [JsonPropertyName("totalSales")]  
        public decimal TotalSales { get; set; }

        [JsonPropertyName("totalProfit")]  
        public decimal TotalProfit { get; set; }
    }
}