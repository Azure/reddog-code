using System;
using System.Text.Json.Serialization;

namespace RedDog.AccountingService.Models
{
      public class TimeSeries<T>
    {
        [JsonPropertyName("pointInTime")]  
        public DateTime PointInTime { get; set; }

        [JsonPropertyName("value")]  
        public T Value { get; set; }

    }
}