using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models;

[Table("order")]
public partial class Order
{
    [Key]
    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("account_id")]
    public int? AccountId { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("sub_total", TypeName = "decimal(18, 2)")]
    public decimal? SubTotal { get; set; }

    [Column("voucher_code")]
    [StringLength(20)]
    [Unicode(false)]
    public string? VoucherCode { get; set; }

    [Column("discount_amount", TypeName = "decimal(18, 2)")]
    public decimal? DiscountAmount { get; set; }

    [Column("final_total", TypeName = "decimal(18, 2)")]
    public decimal? FinalTotal { get; set; }

    [Column("note")]
    public string? Note { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("Orders")]
    public virtual Account? Account { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [ForeignKey("VoucherCode")]
    [InverseProperty("Orders")]
    public virtual DiscountVoucher? VoucherCodeNavigation { get; set; }
}
