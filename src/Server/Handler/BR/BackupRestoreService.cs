using LuciferCore.Extensions;
using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Storage;
using Microsoft.EntityFrameworkCore;
using Server.Database;
using System.Text;

namespace Server.Handler.BR;

public class BackupRestoreService
{
    public async Task<ResponseModel> Restore(BackupRestore? backup)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (backup == null)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Backup data is null."u8, StorageData.TextPlainCharset);
            return response;
        }

        // Đảm bảo đường dẫn tuyệt đối vì SQL Server cần nó
        var backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BackupRestore", backup.Name);

        if (!File.Exists(backupPath))
        {
            response.MakeCustomResponse<byte, byte, byte>(404, StorageData.Http11Protocol, "Backup file not found."u8, StorageData.TextPlainCharset);
            return response;
        }

        using var context = Lucifer.GetModelT<DbContext>();
        var dbName = DBConfig.Database;

        try
        {
            // 1. Chuyển ngữ cảnh sang master và đá văng các kết nối khác
            // 2. Thực hiện Restore
            // 3. Trả lại quyền truy cập bình thường
            string sql = $@"
            USE master;
            ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            RESTORE DATABASE [{dbName}] FROM DISK = '{backupPath}' WITH REPLACE;
            ALTER DATABASE [{dbName}] SET MULTI_USER;";

            // Vì đây là lệnh can thiệp hệ thống, ta nên tăng Timeout 
            // đề phòng file backup nặng xử lý lâu.
            context.Database.SetCommandTimeout(300); // 5 phút

            await context.Database.ExecuteSqlRawAsync(sql);

            response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "Restore successful."u8, StorageData.TextPlainCharset);
        }
        catch (Exception ex)
        {
            try
            {
                using var recoveryContext = Lucifer.GetModelT<DbContext>();
                await recoveryContext.Database.ExecuteSqlRawAsync($"USE master; ALTER DATABASE [{dbName}] SET MULTI_USER;");
            }
            catch { /* Ignore */ }

            // Nếu lỗi xảy ra trong lúc đang SINGLE_USER, phải cố gắng trả lại MULTI_USER
            // để hệ thống không bị "khóa chết"
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, Encoding.UTF8.GetBytes(ex.Message), StorageData.TextPlainCharset);
        }

        return response;
    }

    public async Task<ResponseModel> CreateBackup()
    {
        var response = Lucifer.Rent<ResponseModel>();
        var dbName = DBConfig.Database;
        var fileName = $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
        var backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BackupRestore", fileName);

        try
        {
            using var context = Lucifer.GetModelT<DbContext>();
            context.Database.SetCommandTimeout(300);

            // Query chuẩn như Agent của bạn
            string sql = $@"BACKUP DATABASE [{dbName}] TO DISK = '{backupPath}' WITH COMPRESSION, STATS = 10;";

            await context.Database.ExecuteSqlRawAsync(sql);

            response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "Create Backup Successful"u8, StorageData.TextPlainCharset);
        }
        catch (Exception ex)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Create Backup Failed"u8, StorageData.TextPlainCharset);
        }
        return response;
    }
    public async Task<ResponseModel> GetAllBackups()
    {
        var response = Lucifer.Rent<ResponseModel>();
        var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BackupRestore");

        try
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var files = new DirectoryInfo(folderPath)
                .GetFiles("*.bak")
                .Select(f => new
                {
                    Name = f.Name,
                    UpdateAt = f.LastWriteTime,
                    Size = $"{f.Length / 1024} KB"
                })
                .OrderByDescending(x => x.UpdateAt)
                .ToList();

            // Serialize dữ liệu sang JSON byte array


            response.MakeCustomResponse<byte, char, byte>(200, StorageData.Http11Protocol, files.ToJson(), StorageData.ApplicationJson);
        }
        catch (Exception ex)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Failed to retrieve backups"u8, StorageData.TextPlainCharset);
        }
        return response;
    }

    public async Task<ResponseModel> DeleteBackup(BackupRestore? backup)
    {
        var response = Lucifer.Rent<ResponseModel>();
        if (backup == null || string.IsNullOrEmpty(backup.Name))
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "File name is missing"u8, StorageData.TextPlainCharset);
            return response;
        }

        try
        {
            // Path.GetFileName để chống Path Traversal (Security)
            var safeName = Path.GetFileName(backup.Name);
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BackupRestore", safeName);

            if (File.Exists(path))
            {
                File.Delete(path);
                response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "Delete Successful"u8, StorageData.TextPlainCharset);
            }
            else
            {
                response.MakeCustomResponse<byte, byte, byte>(404, StorageData.Http11Protocol, "File not found"u8, StorageData.TextPlainCharset);
            }
        }
        catch (Exception ex)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Delete Failed"u8, StorageData.TextPlainCharset);
        }
        return response;
    }

    public async Task<ResponseModel> SetAutoBackup(HttpModel request)
    {
        var response = Lucifer.Rent<ResponseModel>();

        try
        {
            // Giả sử bạn lưu trạng thái vào DB hoặc Config file
            // Để demo: Trả về thành công và thông báo cơ chế đã được kích hoạt
            Lucifer.SetInterval(() =>
            {
                // Logic tạo backup tự động, có thể gọi CreateBackup() hoặc viết trực tiếp ở đây
                CreateBackup().Wait();
            }, TimeSpan.FromHours(24)); // Backup mỗi 24 giờ

            response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, "Auto Backup Schedule Updated"u8, StorageData.TextPlainCharset);
        }
        catch (Exception)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Failed to set auto backup"u8, StorageData.TextPlainCharset);
        }
        return response;
    }
}
