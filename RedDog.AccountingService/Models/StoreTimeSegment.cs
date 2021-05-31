using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RedDog.AccountingService.Models
{
    public class StoreTimeSegment
    {
        [JsonPropertyName("storeId")]  
        public string StoreId { get; set; }

        [JsonPropertyName("year")]  
        public int Year {get;set;}

        [JsonPropertyName("month")]  
        public int Month {get;set;}

        [JsonPropertyName("day")]  
        public int Day {get;set;}
        
    }

    public class StoreTimeSegmentHour: StoreTimeSegment
    {
        [JsonPropertyName("hour")]  
        public int Hour {get;set;}
    }

    public class StoreTimeSegmentMinute : StoreTimeSegmentHour
    {
        [JsonPropertyName("minute")]  
        public int Minute {get;set;}
    }

}