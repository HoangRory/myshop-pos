using LuciferCore.Extensions;
using LuciferCore.Main;
using LuciferCore.Model;
using LuciferCore.Storage;

namespace Server.Handler.Image;

public class ImageService
{
    private readonly string _imageStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/image");

    public ImageService()
    {
        // Đảm bảo thư mục tồn tại khi khởi tạo service
        if (!Directory.Exists(_imageStoragePath))
        {
            Directory.CreateDirectory(_imageStoragePath);
        }
    }

    public async Task<ResponseModel> UploadImage(RequestModel request)
    {
        var response = Lucifer.Rent<ResponseModel>();
        var data = request.BodySpan;

        if (data.IsEmpty)
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        try
        {
            // 1. Tạo tên file ngẫu nhiên (GUID)
            string fileName = $"{Guid.NewGuid():N}.png";
            string filePath = Path.Combine(_imageStoragePath, fileName);

            // 2. Ghi dữ liệu binary trực tiếp từ Span ra File
            await File.WriteAllBytesAsync(filePath, data.ToArray());

            // 3. Trả về tên file cho Client (Client sẽ dùng tên này để download)
            response.MakeCustomResponse<byte, char, byte>(201, StorageData.Http11Protocol, fileName.ToJson(), StorageData.ApplicationJson);
        }
        catch (Exception)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Upload Failed"u8, StorageData.TextPlainCharset);
        }

        return response;
    }

    public async Task<ResponseModel> DownloadImage(RequestModel request)
    {
        var response = Lucifer.Rent<ResponseModel>();

        // Giả sử Client gửi body là "guid.png"
        var fileName = request.BodySpan.FromJson<string>();

        if (string.IsNullOrEmpty(fileName))
        {
            response.MakeCustomResponse<byte, byte, byte>(400, StorageData.Http11Protocol, "Bad Request"u8, StorageData.TextPlainCharset);
            return response;
        }

        try
        {
            // 1. Chống kỹ thuật Path Traversal (Bảo mật)
            // Chỉ lấy tên file, không cho phép dùng ../../ để đọc file hệ thống
            string safeFileName = Path.GetFileName(fileName);
            string filePath = Path.Combine(_imageStoragePath, safeFileName);

            if (!File.Exists(filePath))
            {
                response.MakeCustomResponse<byte, byte, byte>(404, StorageData.Http11Protocol, "Image Not Found"u8, StorageData.TextPlainCharset);
                return response;
            }

            // 2. Đọc binary file
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

            // 3. Trả về cho Client với Header Image/Png
            // Lưu ý: MyShop cần ảnh nên Content-Type phải là image/png hoặc octet-stream
            response.MakeCustomResponse<byte, byte, byte>(200, StorageData.Http11Protocol, fileBytes, "image/png"u8);
        }
        catch (Exception)
        {
            response.MakeCustomResponse<byte, byte, byte>(500, StorageData.Http11Protocol, "Download Failed"u8, StorageData.TextPlainCharset);
        }

        return response;
    }
}