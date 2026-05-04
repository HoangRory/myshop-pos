using System.ComponentModel;

namespace MyShop.Client.Models
{
    public class Category : INotifyPropertyChanged
    {
        private int _categoryId;
        public int CategoryId { get => _categoryId; set { if (_categoryId != value) { _categoryId = value; OnPropertyChanged(nameof(CategoryId)); } } }

        private string _name = string.Empty;
        public string Name { get => _name; set { if (_name != value) { _name = value; OnPropertyChanged(nameof(Name)); } } }

        private string? _description;
        public string? Description { get => _description; set { if (_description != value) { _description = value; OnPropertyChanged(nameof(Description)); } } }

        private bool? _isActive;
        public bool? IsActive { get => _isActive; set { if (_isActive != value) { _isActive = value; OnPropertyChanged(nameof(IsActive)); } } }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
