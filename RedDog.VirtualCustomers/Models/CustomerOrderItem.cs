using System.Text.Json.Serialization;

namespace RedDog.VirtualCustomers.Models
{
    public class CustomerOrderItem
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        
    }
}