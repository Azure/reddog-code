using System.Text.Json.Serialization;

namespace RedDog.OrderService.Models
{
    public class CustomerOrderItem
    {
        [JsonPropertyName("menuItemId")]
        public int MenuItemId { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        
    }
}