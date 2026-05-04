using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MyShop.Client.Models
{
    public class Order : INotifyPropertyChanged
    {
        private string _orderId = string.Empty;
        public string OrderId
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

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(nameof(Date));
                }
            }
        }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (_totalAmount != value)
                {
                    _totalAmount = value;
                    OnPropertyChanged(nameof(TotalAmount));
                }
            }
        }

        private string _status = "Pending";
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
