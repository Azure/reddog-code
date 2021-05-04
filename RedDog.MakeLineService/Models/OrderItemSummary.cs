using System.Text.Json.Serialization;

namespace RedDog.MakeLineService.Models
{
    public class OrderItemSummary
    {
        [JsonPropertyName("menuItemId")]        
        public int MenuItemId { get; set; }
        
        [JsonPropertyName("menuItemName")]
        public string MenuItemName { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }
}