using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RedDog.AccountingService.Models
{
    public class OrdersTimeSeries
    {
        [JsonPropertyName("storeId")]  
        public string StoreId { get; set; }

        [JsonPropertyName("values")]  
        public List<TimeSeries<int>> Values {get; set;}
        
    }

}