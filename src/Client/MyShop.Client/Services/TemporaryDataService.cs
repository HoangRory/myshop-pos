using LuciferCore.Attributes;
using MyShop.Client.Services.Interfaces;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace MyShop.Client.Services
{
    [Plugin("Services", "TemporaryData")]
    public class TemporaryDataService : ITemporaryDataService
    {
        private readonly string _tempDataDirectory;
        private Timer? _saveTimer;
        private bool _isRunning;
        private readonly Dictionary<string, Func<object?>> _registeredViewModels = new();
        private readonly object _lockObject = new();

        public TemporaryDataService()
        {
            // Tạo thư mục tạm thời trong AppData
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MyShop",
                "TempData"
            );
            _tempDataDirectory = appDataPath;

            // Tạo thư mục nếu không tồn tại
            if (!Directory.Exists(_tempDataDirectory))
            {
                Directory.CreateDirectory(_tempDataDirectory);
            }
        }

        public void Start(int saveIntervalMilliseconds = 30000)
        {
            lock (_lockObject)
            {
                if (_isRunning)
                    return;

                _isRunning = true;
                _saveTimer = new Timer(
                    async state => await SaveAllRegisteredViewModels(),
                    null,
                    saveIntervalMilliseconds,
                    saveIntervalMilliseconds
                );
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                    return;

                _isRunning = false;
                _saveTimer?.Dispose();
                _saveTimer = null;
            }
        }

        public void RegisterViewModel(string viewModelName, Func<object?> dataGetter)
        {
            lock (_lockObject)
            {
                _registeredViewModels[viewModelName] = dataGetter;
            }
        }

        public void UnregisterViewModel(string viewModelName)
        {
            lock (_lockObject)
            {
                _registeredViewModels.Remove(viewModelName);
            }
        }

        public async Task SaveAsync(string viewModelName, object? data)
        {
            try
            {
                var filePath = GetFilePath(viewModelName);
                var json = JsonSerializer.Serialize(data, GetSerializerOptions());
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu dữ liệu tạm: {ex.Message}");
            }
        }

        public async Task<T?> LoadAsync<T>(string viewModelName) where T : class
        {
            try
            {
                var filePath = GetFilePath(viewModelName);
                if (!File.Exists(filePath))
                    return null;

                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json, GetSerializerOptions());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi tải dữ liệu tạm: {ex.Message}");
                return null;
            }
        }

        public bool HasTemporaryData(string viewModelName)
        {
            var filePath = GetFilePath(viewModelName);
            return File.Exists(filePath);
        }

        public void DeleteTemporaryData(string viewModelName)
        {
            try
            {
                var filePath = GetFilePath(viewModelName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xóa dữ liệu tạm: {ex.Message}");
            }
        }

        public void ClearAllTemporaryData()
        {
            try
            {
                if (Directory.Exists(_tempDataDirectory))
                {
                    Directory.Delete(_tempDataDirectory, true);
                    Directory.CreateDirectory(_tempDataDirectory);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xóa tất cả dữ liệu tạm: {ex.Message}");
            }
        }

        private async Task SaveAllRegisteredViewModels()
        {
            try
            {
                Dictionary<string, Func<object?>> viewModelsSnapshot;
                lock (_lockObject)
                {
                    viewModelsSnapshot = new Dictionary<string, Func<object?>>(_registeredViewModels);
                }

                var saveTasks = viewModelsSnapshot.Select(async kvp =>
                {
                    try
                    {
                        var data = kvp.Value?.Invoke();
                        if (data == null)
                        {
                            DeleteTemporaryData(kvp.Key);
                            return;
                        }
                        await SaveAsync(kvp.Key, data);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu {kvp.Key}: {ex.Message}");
                    }
                });

                await Task.WhenAll(saveTasks);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi trong SaveAllRegisteredViewModels: {ex.Message}");
            }
        }

        private string GetFilePath(string viewModelName)
        {
            return Path.Combine(_tempDataDirectory, $"{viewModelName}.json");
        }

        private JsonSerializerOptions GetSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
    }
}
