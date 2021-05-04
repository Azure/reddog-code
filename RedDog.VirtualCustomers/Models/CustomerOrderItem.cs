using System.Text.Json.Serialization;

namespace RedDog.VirtualCustomers.Models
{
    public class CustomerOrderItem
    {
        [JsonPropertyName("menuItemId")]
        public int MenuItemId { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        
    }
}