using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MyShop.Client.Models
{
    public class Order : INotifyPropertyChanged
    {
        private int _orderId;
        public int OrderId
        {
            get => _orderId;
            set
            {
                if (_orderId != value)
                {
                    _orderId = value;
                    OnPropertyChanged(nameof(OrderId));
                }
            }
        }

        private int? _accountId;
        public int? AccountId
        {
            get => _accountId;
            set
            {
                if (_accountId != value)
                {
                    _accountId = value;
                    OnPropertyChanged(nameof(AccountId));
                }
            }
        }

        private DateTime? _createdAt;
        public DateTime? CreatedAt
        {
            get => _createdAt;
            set
            {
                if (_createdAt != value)
                {
                    _createdAt = value;
                    OnPropertyChanged(nameof(CreatedAt));
                }
            }
        }

        private byte? _status;
        public byte? Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(DisplayOrderId));
                }
            }
        }

        private byte? _paymentMethod;
        public byte? PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                if (_paymentMethod != value)
                {
                    _paymentMethod = value;
                    OnPropertyChanged(nameof(PaymentMethod));
                }
            }
        }

        private decimal? _subTotal;
        public decimal? SubTotal
        {
            get => _subTotal;
            set
            {
                if (_subTotal != value)
                {
                    _subTotal = value;
                    OnPropertyChanged(nameof(SubTotal));
                }
            }
        }

        private string? _voucherCode;
        public string? VoucherCode
        {
            get => _voucherCode;
            set
            {
                if (_voucherCode != value)
                {
                    _voucherCode = value;
                    OnPropertyChanged(nameof(VoucherCode));
                }
            }
        }

        private decimal? _discountAmount;
        public decimal? DiscountAmount
        {
            get => _discountAmount;
            set
            {
                if (_discountAmount != value)
                {
                    _discountAmount = value;
                    OnPropertyChanged(nameof(DiscountAmount));
                }
            }
        }

        private decimal? _finalTotal;
        public decimal? FinalTotal
        {
            get => _finalTotal;
            set
            {
                if (_finalTotal != value)
                {
                    _finalTotal = value;
                    OnPropertyChanged(nameof(FinalTotal));
                }
            }
        }

        private string? _note;
        public string? Note
        {
            get => _note;
            set
            {
                if (_note != value)
                {
                    _note = value;
                    OnPropertyChanged(nameof(Note));
                }
            }
        }

        private ObservableCollection<OrderItem> _orderItems = new();
        public ObservableCollection<OrderItem> OrderItems
        {
            get => _orderItems;
            set
            {
                if (_orderItems != value)
                {
                    _orderItems = value;
                    OnPropertyChanged(nameof(OrderItems));
                }
            }
        }

        public string StatusText
        {
            get
            {
                return Status switch
                {
                    (byte)OrderStatus.Pending => "Chờ thanh toán",
                    (byte)OrderStatus.Paid => "Đã thanh toán",
                    (byte)OrderStatus.Cancelled => "Đã hủy",
                    _ => "Không xác định"
                };
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var newStatus = value switch
                    {
                        "Chờ thanh toán" => (byte)OrderStatus.Pending,
                        "Đã thanh toán" => (byte)OrderStatus.Paid,
                        "Đã hủy" => (byte)OrderStatus.Cancelled,
                        _ => Status
                    };
                    Status = newStatus;
                }
            }
        }

        public int DisplayOrderId
        {
            get
            {
                // Return -1 for pending orders, actual ID for paid/cancelled orders
                return Status == (byte)OrderStatus.Pending ? -1 : OrderId;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
