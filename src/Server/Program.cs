using LuciferCore.Main;

Lucifer.CMD("/init di");


try
{
    using var context = new Server.Database.EFcore.Sql.EFSqlContext();
    if (context.Database.CanConnect())
    {
        Console.WriteLine("[Database] Kết nối DB thành công (test).");
    }
    else
    {
        Console.WriteLine("[Database] Không thể kết nối DB (test).");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Database] Lỗi khi test kết nối DB: {ex.Message}");
}

try
{
    Lucifer.CMD("/run"u8);
    Console.WriteLine("[Server] Ứng dụng đã khởi động xong và sẵn sàng nhận kết nối.");
}
catch (Exception ex)
{
    Console.WriteLine($"[Server] Lỗi khi chạy Lucifer.CMD(/run): {ex}");
    if (ex.InnerException != null)
        Console.WriteLine($"[Server] Inner exception: {ex.InnerException}");
    throw;
}

// Test kết nối DB khi khởi động
