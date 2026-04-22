using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Server.Models;

[Table("product")]
[Index("Sku", Name = "UQ__product__DDDF4BE713AE91B5", IsUnique = true)]
public partial class Product
{
    [Key]
    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("sku")]
    [StringLength(50)]
    [Unicode(false)]
    public string Sku { get; set; } = null!;

    [Column("name")]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Column("import_price", TypeName = "decimal(18, 2)")]
    public decimal ImportPrice { get; set; }

    [Column("sale_price", TypeName = "decimal(18, 2)")]
    public decimal SalePrice { get; set; }

    [Column("stock_count")]
    public int? StockCount { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
