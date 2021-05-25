using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RedDog.AccountingService.Models
{
    public class StoreTimeSegment
    {
        public StoreTimeSegment() { }

        public StoreTimeSegment(string storeId, int year, int month, int day)
        {
            StoreId = storeId;
            Year = year;
            Month = month;
            Day = day;
        }

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
        public StoreTimeSegmentHour() { }

        public StoreTimeSegmentHour(string storeId, int year, int month, int day, int hour)
        {
            StoreId = storeId;
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
        }
        [JsonPropertyName("hour")]  
        public int Hour {get;set;}
    }

    public class StoreTimeSegmentMinute : StoreTimeSegmentHour
    {
        public StoreTimeSegmentMinute() { }

        public StoreTimeSegmentMinute(string storeId, int year, int month, int day, int hour, int minute)
        {
            StoreId = storeId;
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
        }
        [JsonPropertyName("minute")]  
        public int Minute {get;set;}
    }

}