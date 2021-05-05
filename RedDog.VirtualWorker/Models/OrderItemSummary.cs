using System.Text.Json.Serialization;

namespace RedDog.VirtualWorker.Models
{
    public class OrderItemSummary
    {
        [JsonPropertyName("productId")]        
        public int ProductId { get; set; }
        
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unitCost")]
        public decimal UnitCost { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }
    }
}