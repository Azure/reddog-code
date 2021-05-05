using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedDog.AccountingModel
{
    [Table(nameof(Order))]
    public class Order
    {
        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        [Key]
        public Guid OrderId { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        [Required]
        public string StoreId { get; set; }

        [Required]
        public DateTime PlacedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [Required]
        public Customer Customer { get; set; }

        [Required]
        public List<OrderItem> OrderItems { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderTotal { get; set; }
    }
}