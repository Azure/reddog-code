using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RedDog.VirtualCustomers.Models
{
    public class CustomerOrder
    {
        [JsonPropertyName("storeId")]
        public string StoreId { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("loyaltyId")]
        public string LoyaltyId { get; set; }

        [JsonPropertyName("orderItems")]
        public List<CustomerOrderItem> OrderItems { get; set; }
    }
}