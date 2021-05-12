using System;
using System.Text.Json.Serialization;

namespace RedDog.AccountingService.Models
{
    public class OrderMetric
    {
        [JsonPropertyName("storeId")]  
        public string StoreId { get; set; }

        [JsonPropertyName("orderDate")]  
        public DateTime OrderDate { get; set; }

        [JsonPropertyName("orderHour")]  
        public int OrderHour { get; set; }

        [JsonPropertyName("orderCount")]  
        public int OrderCount { get; set; }

        [JsonPropertyName("avgFulfillmentSec")]  
        public int AvgFulfillmentTimeSec { get; set; }

        [JsonPropertyName("orderItemCount")]  
        public int OrderItemCount { get; set; }

        [JsonPropertyName("totalCost")]  
        public decimal TotalCost { get; set; }

        [JsonPropertyName("totalPrice")]  
        public decimal TotalPrice { get; set; }
    }
}