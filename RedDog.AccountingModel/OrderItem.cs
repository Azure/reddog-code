using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDog.AccountingModel
{
    [Table(nameof(OrderItem))]
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int ProductId { get; set; }
        
        [Column(TypeName = "nvarchar(50)")]
        [Required]
        public string ProductName { get; set; }
        
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public Guid OrderId { get; set; }

        public Order Order { get; set; }
    }
}