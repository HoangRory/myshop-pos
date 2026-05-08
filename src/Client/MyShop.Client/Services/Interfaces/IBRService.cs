using MyShop.Client.Models;

namespace MyShop.Client.Services.Interfaces
{
    public interface IBRService
    {
        Task<List<BackupRestore>> GetAllBackupsAsync();
        Task<bool> RestoreAsync(string bkName);
        Task<bool> CreateBackupAsync();
        Task<bool> SetAutoBackupAsync();
        Task<bool> DeleteBackupAsync(string bkName);
    }
}
