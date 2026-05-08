using LuciferCore.Attributes;
using MyShop.Client.Helpers;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MyShop.Client.ViewModels
{
    [Plugin("ViewModel", "Backup & Restore")]
    public class BRViewModel : INotifyPropertyChanged
    {
        private readonly IBRService _brService;
        private readonly IDialogService _dialogService;
        private List<BackupRestore> _allBackups = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<BackupRestore> DisplayList { get; } = new();

        // IsLoaded = true khi App rảnh, false khi đang xử lý API
        private bool _isLoaded = true;
        public bool IsLoaded
        {
            get => _isLoaded;
            set { _isLoaded = value; OnPropertyChanged(nameof(IsLoaded)); }
        }

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { _searchKeyword = value; OnPropertyChanged(nameof(SearchKeyword)); ApplyFilters(); }
        }

        private int _pageIndex = 1;
        public int PageIndex
        {
            get => _pageIndex;
            set { _pageIndex = value; OnPropertyChanged(nameof(PageIndex)); UpdateDisplayList(); }
        }

        public int TotalPages => FilteredCount == 0 ? 1 : (int)Math.Ceiling((double)FilteredCount / 10);
        public int FilteredCount { get; private set; }

        public ICommand CreateBackupCommand { get; }
        public ICommand RestoreCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand AutoBackupCommand { get; }

        public BRViewModel(IBRService bRService, IDialogService dialogService)
        {
            _brService = bRService;
            _dialogService = dialogService;

            // Dùng RelayCommand có kiểm tra IsLoaded để tự động disable nút khi đang chạy
            CreateBackupCommand = new RelayCommand(async _ => await CreateBackup(), _ => IsLoaded);
            AutoBackupCommand = new RelayCommand(async _ => await SetAutoBackup(), _ => IsLoaded);

            RestoreCommand = new RelayCommand(async p => await Restore(p as string), _ => IsLoaded);
            DeleteCommand = new RelayCommand(async p => await Delete(p as string), _ => IsLoaded);

            PrevPageCommand = new RelayCommand(_ => { if (PageIndex > 1) PageIndex--; }, _ => IsLoaded && PageIndex > 1);
            NextPageCommand = new RelayCommand(_ => { if (PageIndex < TotalPages) PageIndex++; }, _ => IsLoaded && PageIndex < TotalPages);

            _ = LoadData();
        }

        public async Task LoadData()
        {
            IsLoaded = false;
            try
            {
                var data = await _brService.GetAllBackupsAsync();
                _allBackups = data.OrderByDescending(x => x.CreateAt).ToList();
                ApplyFilters();
            }
            finally
            {
                IsLoaded = true;
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allBackups.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
                filtered = filtered.Where(x => x.Name.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase));

            FilteredCount = filtered.Count();
            OnPropertyChanged(nameof(TotalPages));
            PageIndex = 1;
            UpdateDisplayList();
        }

        private void UpdateDisplayList()
        {
            var filtered = _allBackups.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
                filtered = filtered.Where(x => x.Name.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase));

            var paged = filtered.Skip((PageIndex - 1) * 10).Take(10).ToList();

            DisplayList.Clear();
            foreach (var item in paged) DisplayList.Add(item);
        }

        private async Task CreateBackup()
        {
            IsLoaded = false;
            try
            {
                if (await _brService.CreateBackupAsync())
                {
                    _dialogService.Success("Thành công", "Bản sao lưu mới đã được tạo.");
                    await LoadData();
                }
                else _dialogService.Error("Lỗi", "Không thể tạo bản sao lưu.");
            }
            finally { IsLoaded = true; }
        }

        private async Task SetAutoBackup()
        {
            IsLoaded = false;
            try
            {
                var success = await _brService.SetAutoBackupAsync();
                if (success)
                {
                    _dialogService.Success("Cấu hình", "Đã cập nhật chế độ tự động sao lưu định kỳ.");
                }
                else
                {
                    _dialogService.Error("Lỗi", "Không thể thiết lập tự động sao lưu.");
                }
            }
            finally { IsLoaded = true; }
        }

        private async Task Restore(string? name)
        {
            if (string.IsNullOrEmpty(name)) return;

            // Xác nhận trước khi làm hành động nguy hiểm
            if (_dialogService.Confirm("Xác nhận phục hồi", $"Hệ thống sẽ ghi đè dữ liệu bằng bản sao lưu '{name}'. Bạn có chắc chắn không?"))
            {
                IsLoaded = false;
                try
                {
                    if (await _brService.RestoreAsync(name))
                        _dialogService.Success("Thành công", "Hệ thống đã phục hồi thành công.");
                    else
                        _dialogService.Error("Lỗi", "Khôi phục thất bại.");
                }
                finally { IsLoaded = true; }
            }
        }

        private async Task Delete(string? name)
        {
            if (string.IsNullOrEmpty(name)) return;

            if (_dialogService.Confirm("Xác nhận xóa", $"Bản sao lưu '{name}' sẽ bị xóa vĩnh viễn. Tiếp tục?"))
            {
                IsLoaded = false;
                try
                {
                    if (await _brService.DeleteBackupAsync(name))
                    {
                        await LoadData();
                    }
                    else _dialogService.Error("Lỗi", "Không thể xóa bản sao lưu.");
                }
                finally { IsLoaded = true; }
            }
        }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}