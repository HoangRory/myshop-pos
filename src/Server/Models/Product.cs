using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Server.Models;

[Table("product")]
[Index("Sku", Name = "UQ__product__DDDF4BE71184CD7E", IsUnique = true)]
public partial class Product
{
    [Key]
    [Column("product_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }

    [Column("sku")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Sku { get; set; } = null!;

    [Column("name")]
    [StringLength(200)]
    public string? Name { get; set; } = null!;

    [Column("import_price", TypeName = "decimal(18, 2)")]
    public decimal? ImportPrice { get; set; }

    [Column("sale_price", TypeName = "decimal(18, 2)")]
    public decimal? SalePrice { get; set; }

    [Column("stock_count")]
    public int? StockCount { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("images")]
    public string? Images { get; set; }

    [Column("category_id")]
    public int? CategoryId { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    [JsonIgnore]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
