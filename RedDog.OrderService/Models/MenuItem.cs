using System.Collections.Generic;
using Newtonsoft.Json;

namespace RedDog.OrderService.Models
{
    public class MenuItem
    {
        [JsonProperty("menuItemid")]
        public int MenuItemId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        public static List<MenuItem> GetAll()
        {
            var items = new List<MenuItem>();
            items.Add(new MenuItem
            {
                MenuItemId = 1,
                Name = "Americano",
                Description = "Americano",
                Price = 2.99m,
                ImageUrl = "https://daprworkshop.blob.core.windows.net/images/americano.jpg"
            });
            items.Add(new MenuItem
            {
                MenuItemId = 2,
                Name = "Caramel Macchiato",
                Description = "Caramel Macchiato",
                Price = 4.99m,
                ImageUrl = "https://daprworkshop.blob.core.windows.net/images/macchiato.jpg"
            });
            items.Add(new MenuItem
            {
                MenuItemId = 3,
                Name = "Latte",
                Description = "Latte",
                Price = 3.99m,
                ImageUrl = "https://daprworkshop.blob.core.windows.net/images/latte.jpg"
            });

            return items;
        }
    }
}