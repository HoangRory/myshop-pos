using LuciferCore.Attributes;
using MyShop.Client.Services.Interfaces;
using System.Net.Http;

namespace MyShop.Client.Services
{
    [Plugin("Service", "Image")]
    public class ImageService : IImageService
    {
        private readonly HttpClient _http;
        private const string ApiPath = "v1/api/image";

        private string Url(string endpoint = "") => ((IAPI)this).GetFullUrl(ApiPath, endpoint);

        public ImageService(HttpClient http)
        {
            _http = http;
        }

        /// <summary>
        /// Upload image file to server
        /// </summary>
        public async Task<string> UploadImageAsync(byte[] fileData, string fileName)
        {
            try
            {
                // Create request with octet-stream content type as per API requirement
                var content = new ByteArrayContent(fileData);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                var response = await _http.PostAsync(Url("upload"), content);

                if (response.IsSuccessStatusCode)
                {
                    // Server returns the file name as plain text or JSON with the file name
                    var result = await response.Content.ReadAsStringAsync();

                    // If response is wrapped in quotes (JSON string), remove them
                    return result.Trim('"');
                }

                throw new Exception($"Upload failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading image: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Download image from server
        /// </summary>
        public async Task<byte[]> DownloadImageAsync(string fileName)
        {
            try
            {
                // Create a GET request but include the file name in the request body.
                // HttpClient doesn't provide a GetAsync overload that accepts a body,
                // so construct an HttpRequestMessage and call SendAsync.
                var request = new HttpRequestMessage(HttpMethod.Get, Url("download"));
                request.Content = new StringContent($"\"{fileName}\"", System.Text.Encoding.UTF8, "text/plain");

                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                throw new Exception($"Download failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading image '{fileName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parse Images string to list of file names
        /// Images string format: "file1.png;file2.png;file3.png"
        /// </summary>
        public List<string> ParseImageNames(string imagesString)
        {
            if (string.IsNullOrWhiteSpace(imagesString))
                return new List<string>();

            return imagesString
                .Split(';')
                .Select(name => name.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();
        }

        /// <summary>
        /// Download all images from Images string
        /// </summary>
        public async Task<Dictionary<string, byte[]>> DownloadImagesAsync(string imagesString)
        {
            var result = new Dictionary<string, byte[]>();
            var imageNames = ParseImageNames(imagesString);

            foreach (var fileName in imageNames)
            {
                try
                {
                    var imageBytes = await DownloadImageAsync(fileName);
                    result[fileName] = imageBytes;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to download image '{fileName}': {ex.Message}");
                    // Continue with other images instead of throwing
                }
            }

            return result;
        }
    }
}
