namespace MyShop.Client.Services.Interfaces
{
    public interface IAPI
    {
        string GetFullUrl(string apiPath, string endpoint = "")
        {
            var config = Models.AppConfig.Load();
            var baseUrl = config.GetServerUrl().TrimEnd('/');
            var path = apiPath.Trim('/');

            // Nếu không có endpoint (gọi vào gốc của Controller)
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return $"{baseUrl}/{path}";
            }

            // Nếu có endpoint thì mới ghép thêm dấu /
            var method = endpoint.Trim('/');
            return $"{baseUrl}/{path}/{method}";
        }
    }
}
