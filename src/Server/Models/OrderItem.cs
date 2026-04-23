using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Server.Models;

[Table("order_item")]
public partial class OrderItem
{
    [Key]
    [Column("order_item_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderItemId { get; set; }

    [Column("order_id")]
    public int? OrderId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    [Column("unit_price", TypeName = "decimal(18, 2)")]
    public decimal? UnitPrice { get; set; }

    [Column("total_item_price", TypeName = "decimal(29, 2)")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)] // Thêm dòng này
    public decimal? TotalItemPrice { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderItems")]
    [JsonIgnore]
    public virtual Order? Order { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("OrderItems")]
    [JsonIgnore]
    public virtual Product? Product { get; set; }
}
