using MyShop.Client.Services.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MyShop.Client.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;
            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Đăng ký ViewModel để tự động lưu dữ liệu định kỳ
        /// </summary>
        protected void RegisterAutoSave(ITemporaryDataService tempDataService, string viewModelName)
        {
            tempDataService.RegisterViewModel(viewModelName, () => GetAutoSaveData());
        }

        /// <summary>
        /// Hủy đăng ký ViewModel khỏi dịch vụ auto-save
        /// </summary>
        protected void UnregisterAutoSave(ITemporaryDataService tempDataService, string viewModelName)
        {
            tempDataService.UnregisterViewModel(viewModelName);
        }

        /// <summary>
        /// Cố gắng khôi phục dữ liệu tạm thời cho ViewModel này
        /// </summary>
        protected async Task<T?> TryRecoverDataAsync<T>(ITemporaryDataService tempDataService, string viewModelName) where T : class
        {
            if (!AppState.ViewModelsToRecover.Contains(viewModelName))
                return null;

            var data = await tempDataService.LoadAsync<T>(viewModelName);
            if (data != null)
            {
                AppState.ViewModelsToRecover.Remove(viewModelName);
            }
            return data;
        }

        /// <summary>
        /// Phương thức override để cung cấp dữ liệu cần lưu tạm thời
        /// Ghi đè phương thức này trong các ViewModel cần auto-save
        /// </summary>
        protected virtual object? GetAutoSaveData()
        {
            return null;
        }


        /// <summary>
        /// Xóa dữ liệu tạm thời sau khi đã phục hồi thành công hoặc khi không cần phục hồi nữa
        /// </summary>
        protected void CommitRecovery(ITemporaryDataService tempDataService, string viewModelName)
        {
            tempDataService.DeleteTemporaryData(viewModelName);
        }
    }
}

