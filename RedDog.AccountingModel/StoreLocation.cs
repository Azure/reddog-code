using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDog.AccountingModel
{
    [Table(nameof(StoreLocation))]
    public class StoreLocation
    {
        [Column(TypeName = "nvarchar(54)")]
        [Key]
        public string StoreId { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        public string Description { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        [Required]
        public string City { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        [Required]
        public string StateProvince { get; set; }

        [Column(TypeName = "nvarchar(30)")]
        [Required]
        public string PostalCode { get; set; }

        [Column(TypeName = "nvarchar(54)")]
        [Required]
        public string Country { get; set; }

        [Column(TypeName = "decimal(12,6)")]
        [Required]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(12,6)")]
        [Required]
        public decimal Longitude { get; set; }
    }
}