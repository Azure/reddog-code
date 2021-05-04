using System.Text.Json.Serialization;

namespace RedDog.LoyaltyService.Models
{
    public class LoyaltySummary
    {
        [JsonPropertyName("loyaltyId")]
        public string LoyaltyId { get; set; }
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
        [JsonPropertyName("pointsEarned")]
        public int PointsEarned { get; set; }
        [JsonPropertyName("pointTotal")]
        public int PointTotal { get; set; }
    }
}