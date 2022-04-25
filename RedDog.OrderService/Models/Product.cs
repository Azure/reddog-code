using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedDog.OrderService.Models
{
    public class Product
    {
        // Note: Use Products with category field
        private static string _productDefinitionFilename =  Environment.GetEnvironmentVariable("PRODUCT_DEFINITION_FILENAME") ?? "DrugStoreProducts-categorized.json";
        private static List<Product> _products;

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("unitCost")]
        public decimal UnitCost { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("categoryId")]
        public string CategoryId { get; set; }

        public static async Task<List<Product>> GetAllAsync()
        {
            if(_products == null)
            {
                using FileStream jsonStream = File.OpenRead($"ProductDefinitions/{_productDefinitionFilename}");
                _products = await JsonSerializer.DeserializeAsync<List<Product>>(jsonStream);
            }

            return _products;
        }
    }
}