using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Server.Models;

[Table("discount_voucher")]
public partial class DiscountVoucher
{
    [Key]
    [Column("voucher_code")]
    [StringLength(20)]
    [Unicode(false)]
    public string VoucherCode { get; set; } = null!;

    [Column("discount_type")]
    public byte DiscountType { get; set; }

    [Column("discount_value", TypeName = "decimal(18, 2)")]
    public decimal DiscountValue { get; set; }

    [Column("min_order_value", TypeName = "decimal(18, 2)")]
    public decimal? MinOrderValue { get; set; }

    [Column("max_discount_amount", TypeName = "decimal(18, 2)")]
    public decimal? MaxDiscountAmount { get; set; }

    [Column("expiry_date", TypeName = "datetime")]
    public DateTime ExpiryDate { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [InverseProperty("VoucherCodeNavigation")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
