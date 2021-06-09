using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDog.AccountingModel
{
    [Table(nameof(Customer))]
    public class Customer
    {
        [Column(TypeName = "nvarchar(36)")]
        [Key]
        public string LoyaltyId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [Required]
        public string FirstName { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [Required]
        public string LastName { get; set; }

    }
}