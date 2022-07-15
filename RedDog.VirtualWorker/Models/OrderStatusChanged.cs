using System;
using System.Text.Json.Serialization;

namespace RedDog.VirtualWorker.Models
{
    public enum OrderStatus
        {
            Created,
            InProgress,
            Completed
        }
        
    public class OrderStatusChanged
    {
        [JsonPropertyName("orderId")]        
        public Guid OrderId { get; set; }
        
        [JsonPropertyName("orderStatus")]
        public OrderStatus OrderStatus { get; set; }
    }
}