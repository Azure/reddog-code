using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RedDog.AccountingService.Models
{
    public class OrderSummary
    {
        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("orderDate")]
        public DateTime OrderDate { get; set; }

        [JsonPropertyName("orderCompletedDate")]
        public DateTime? OrderCompletedDate { get; set; }

        [JsonPropertyName("storeId")]
        public string StoreId { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("loyaltyId")]
        public string LoyaltyId { get; set; }

        [JsonPropertyName("orderItems")]
        public List<OrderItemSummary> OrderItems { get; set; }

        [JsonPropertyName("orderTotal")]
        public decimal OrderTotal { get; set; }
    }
}