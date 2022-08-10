using System;
using System.Text.Json.Serialization;

namespace RedDog.MakeLineService.Models
{
    public class OrderStatusChanged
    {
        [JsonPropertyName("orderId")]        
        public Guid OrderId { get; set; }
        
        [JsonPropertyName("orderStatus")]
        public OrderStatus OrderStatus { get; set; }
    }
}